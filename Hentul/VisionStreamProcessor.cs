using System.Runtime.InteropServices;

namespace Hentul
{
    using System;
    using System.Drawing;
    using System.IO;
    using System.Threading;
    using Common;
    using Encoders;
    using OpenCvSharp; // ok to keep; remove if unused
    using FBBM = FirstOrderMemory.BehaviourManagers.BlockBehaviourManagerFOM;
    using SBBM = SecondOrderMemory.Models.BlockBehaviourManagerSOM;

    public class VisionStreamProcessor
    {
        #region Constructor & Variables

        public int numPixelsProcessedPerBBM;

        public int NumBBMNeededV { get; private set; }
        public int BlockSize { get; private set; }

        public LearningUnit v1 { get; private set; } // 20×20 (normalized to 40×20 for encoder)
        public LearningUnit v2 { get; private set; } // 100×100 (normalized to 40×20)
        public LearningUnit v3 { get; private set; } // 200×200 (normalized to 40×20)

        public int Range { get; private set; }

        private int X, NumColumns, Z;

        private bool IsMock;

        public ulong CycleNum;

        public LogMode LogMode { get; private set; }

        public PixelEncoder pEncoder { get; private set; }

        public Bitmap bmp { get; private set; } // legacy 20×20 scratch

        public string logfilename { get; private set; }

        // This is a constant for now, can be changed later if needed.
        public int numberOFLearningUnitsNeeded => 1;

        private readonly string baseDir = AppContext.BaseDirectory;

        public VisionStreamProcessor(int range, int numColumns, int x, LogMode logMode, bool isMock, bool shouldInit)
        {
            X = x;
            NumColumns = numColumns;
            IsMock = isMock;
            Z = 5;
            CycleNum = 0;

            // Each LU consumes a normalized 40×20 bitmap (800 px) → 100 BBMs (4 px / BBM)
            if (range != 10)
                throw new InvalidOperationException("Invalid Operation! 'range' must be 10 for a 20×20 capture window.");

            Range = range; // 10 → window 20×20
            BlockSize = (2 * range) * (2 * range); // 400 for V1 raw window (legacy)

            // 4 pixels per BBM → 100 BBMs at 40×20 (800 px). For V1 legacy (20×20) this was 400/4 = 100 too.
            numPixelsProcessedPerBBM = 4;
            NumBBMNeededV = (BlockSize / numPixelsProcessedPerBBM); // historically 100

            // Keep encoder sized for 100 BBMs (maps to 40×20 normalized inputs)
            pEncoder = new PixelEncoder(NumBBMNeededV, BlockSize);

            // Legacy scratch (20×20)
            bmp = new Bitmap(range + range, range + range);

            if (NumBBMNeededV != 100)
                throw new InvalidDataException("Number of FOM BBMs must be 100, else it throws off SOM schema of 1250.");

            LogMode = logMode;

            // Repo-friendly log path (no user-specific absolute paths)
            logfilename = Path.GetFullPath(Path.Combine(baseDir, @"..\..\..\..\..\Hentul\Logs\Hentul-Orchestrator.log"));

            if (shouldInit)
            {
                v1 = new LearningUnit(NumBBMNeededV, NumColumns, Z, X, shouldInit, logfilename, LearningUnitType.V1);
                v2 = new LearningUnit(NumBBMNeededV, NumColumns, Z, X, shouldInit, logfilename, LearningUnitType.V2);
                v3 = new LearningUnit(NumBBMNeededV, NumColumns, Z, X, shouldInit, logfilename, LearningUnitType.V3);

                v1.Init();
                v2.Init();
                v3.Init();
            }

            Console.WriteLine("Total Number of Pixels : " + (Range * Range * 4)); // (20×20) legacy note
            Console.WriteLine("Total First Order BBMs Created : " + NumBBMNeededV);
        }

        #endregion

        /// <summary>
        /// Legacy single-bitmap processing path (kept for compatibility).
        /// Your orchestrator now uses ProcessFor() per LU with normalized 40×20 inputs.
        /// </summary>
        public void Process(Bitmap greyScalebmp, ulong cycle)
        {
            CycleNum = cycle;

            pEncoder.ParseBitmap(greyScalebmp);

            v1?.Process(pEncoder, cycle);
            // v2/v3 intentionally not used in legacy path

            Clean();
        }

        /// <summary>
        /// New path: process a normalized (40×20) greyscale bitmap for the specific LearningUnitType.
        /// </summary>
        public void ProcessFor(LearningUnitType luType, Bitmap greyScaleBmp)
        {
            pEncoder.ParseBitmap(greyScaleBmp);
            var lu = GetLearningUnit(luType);
            lu.Process(pEncoder, CycleNum);
            pEncoder.Clean();
            lu.Clear();
        }

        public SDR_SOM GetSL3BLatestFiringCells(LearningUnitType luType, ulong cyclenum) =>
            GetLearningUnit(luType).somBBM_L3B_V.GetAllNeuronsFiringLatestCycle(cyclenum);

        public FBBM[] GetFOMBBMVFromLearningUnit(LearningUnitType lType)
        {
            var lu = GetLearningUnit(lType);
            return lu.fomBBMV;
        }

        internal LearningUnit GetLearningUnit(LearningUnitType lType)
        {
            if (v1 != null && v1.LType == lType) return v1;
            if (v2 != null && v2.LType == lType) return v2;
            if (v3 != null && v3.LType == lType) return v3;

            throw new InvalidOleVariantTypeException($"No LearningUnit initialized for {lType}.");
        }

        private void WriteLogsToFile(string msg)
        {
            File.WriteAllText(logfilename, msg);
        }

        public void Restore()
        {
            v1?.Restore();
            v2?.Restore();
            v3?.Restore();
        }

        public void Backup()
        {
            v1?.Backup();
            v2?.Backup();
            v3?.Backup();
        }

        public void PrintBlockVitalVision(LearningUnitType luType)
        {
            Console.WriteLine("Enter '1' to see a list of all the Block Usage List :");

            int w = Console.Read();
            if (w != 49) return; // '1'

            ulong totalIncludedCycle = 0;

            var fomBBMV = GetFOMBBMVFromLearningUnit(luType);

            for (int i = 0; i < fomBBMV.Length; i++)
            {
                if (fomBBMV[i].BBMID != 0)
                    Console.WriteLine($"{i} :: Block ID : {fomBBMV[i].PrintBlockDetailsSingleLine()} | Included Cycle: {fomBBMV[i].CycleNum}");

                totalIncludedCycle += fomBBMV[i].CycleNum;
            }

            Console.WriteLine("Total Participated Cycles : " + totalIncludedCycle);
            Console.WriteLine("Orchestrator CycleNum : " + CycleNum);

            if (totalIncludedCycle != CycleNum)
            {
                Console.WriteLine("ERROR : Incorrect Cycle distribution among blocks");
                Thread.Sleep(5000);
            }
        }

        internal void SetNetworkModeToPrediction()
        {
            v1?.ChangeNetworkModeToPrediction();
            v2?.ChangeNetworkModeToPrediction();
            v3?.ChangeNetworkModeToPrediction();
        }

        public void LearnNewObject(string objectName)
        {
            v1?.LearnNewObject(objectName);
        }

        internal List<string> GetSupportedLabels(LearningUnitType luType) =>
            GetLearningUnit(luType).somBBM_L3B_V.GetCurrentPredictions();

        internal void Clean()
        {
            pEncoder.Clean();
            v1?.Clear();
            v2?.Clear();
            v3?.Clear();
        }

        internal List<string> GetCurrentPredictions() =>
            v1?.somBBM_L3B_V.GetCurrentPredictions() ?? new List<string>();

        internal void BeginTraining(string objectLabel)
        {
            v1?.BeginTraining(objectLabel);
        }
    }
}
