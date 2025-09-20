using System.Runtime.InteropServices;

namespace Hentul
{
    using System.Drawing;
    using Common;
    using Encoders;
    using FBBM = FirstOrderMemory.BehaviourManagers.BlockBehaviourManagerFOM;
    using SBBM = SecondOrderMemory.Models.BlockBehaviourManagerSOM;

    public class VisionStreamProcessor
    {

        #region Constructor & Variables

        public int numPixelsProcessedPerBBM;

        private static readonly string baseDir = AppContext.BaseDirectory;
        public int NumBBMNeededV { get; private set; }

        public int BlockSize;

        public LearningUnit v1 { get; private set; }    //20 * 20

        public LearningUnit v2 { get; private set; }    // 100 * 100

        public LearningUnit v3 { get; private set; }    // 200 X 200

        public int Range { get; private set; }

        int X, NumColumns, Z;

        bool IsMock;

        public ulong CycleNum;

        public LogMode LogMode { get; private set; }

        public PixelEncoder pEncoder { get; private set; } 

        public Bitmap bmp { get; private set; }

        public string logfilename { get; private set; }
        
        public int numberOFLearningUnitsNeeded => 1; // This is a constant for now, can be changed later if needed.


        public VisionStreamProcessor(int range, int numColumns, int x, LogMode logMode, bool isMock, bool shouldInit)
        {
            this.X = x;

            this.NumColumns = numColumns;

            IsMock = isMock;

            Z = 5;

            CycleNum = 0;

            BlockSize = (2 * range) * (2 * range); //400            

            //Todo : Project shape data of the input image to one region and project colour data of the image to another region.                        
            if (range != 10)
            {
                throw new InvalidOperationException("Invalid Operation !");
            }

            Range = range;  //10

            NumBBMNeededV = 100;

            pEncoder = new PixelEncoder(NumBBMNeededV, BlockSize);

            bmp = new Bitmap(range + range, range + range);

            

            if (NumBBMNeededV != 100)
            {
                throw new InvalidDataException("Number Of FOMM BBMs needed should always be 100, it throws off SOM Schema of 1250" + range.ToString());
            }

            numPixelsProcessedPerBBM = 4;

            NumBBMNeededV = (BlockSize / numPixelsProcessedPerBBM);   //100
            
            logfilename = Path.GetFullPath(Path.Combine(baseDir, @"..\..\..\..\..\Hentul\Logs\Hentul-Orchestrator.log"));

            if (shouldInit)
            {
                v1 = new LearningUnit(NumBBMNeededV, NumColumns, Z, X, shouldInit, logfilename, LearningUnitType.V1);
                //v2 = new LearningUnit(NumBBMNeededV, NumColumns, Z, X, shouldInit, logfilename, LearningUnitType.V2);
                //v3 = new LearningUnit(NumBBMNeededV, NumColumns, Z, X, shouldInit, logfilename, LearningUnitType.V3);
            }
            
            Console.WriteLine("Total Number of Pixels :" + (Range * Range * 4).ToString() + "\n");
            Console.WriteLine("Total First Order BBMs Created : " + NumBBMNeededV.ToString() + "\n");

            if (shouldInit)
            {
                v1.Init();
                //v2.Init();
                //v3.Init();
            }

            LogMode = logMode;
        }

        #endregion
       

        public void Process(Bitmap greyScalebmp, ulong cycle)
        {
            CycleNum = cycle;

            pEncoder.ParseBitmap(greyScalebmp);

            v1.Process(pEncoder, cycle);
            //v2.Process(pEncoder, CycleNum);
            //v3.Process(pEncoder, CycleNum);

            Clean();
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
            if (v1.LType == lType)
                return v1;
            else if (v2.LType == lType)
                return v2;
            else if (v3.LType == lType)
                return v3;

            throw new InvalidOleVariantTypeException("No Matching input Learning Unit Type!");
        }
                
        public void Restore()
        {
            v1.Restore();
            v2.Restore();
            v3.Restore();
        }

        public void Backup()
        {
            v1.Backup();
            v2.Backup();
            v3.Backup();
        }


        public void PrintBlockVitalVision(LearningUnitType luType)
        {
            Console.WriteLine("Enter '1' to see a list of all the Block Usage List :");

            int w = Console.Read();

            if (w == 49)
            {
                ulong totalIncludedCycle = 0;

                var fomBBMV = GetFOMBBMVFromLearningUnit(luType);

                for (int i = 0; i < fomBBMV.Length; i++)
                {
                    if (fomBBMV[i].BBMID != 0)
                        Console.WriteLine(i.ToString() + " :: Block ID : " + fomBBMV[i].PrintBlockDetailsSingleLine() + " | " + "Included Cycle: " + fomBBMV[i].CycleNum.ToString());

                    totalIncludedCycle += fomBBMV[i].CycleNum;

                }

                Console.WriteLine("Total Participated Cycles : " + totalIncludedCycle);
                Console.WriteLine("Orchestrator CycleNum : " + CycleNum.ToString());

                if (totalIncludedCycle != CycleNum)
                {
                    Console.WriteLine("ERROR : Incorrect Cycle Distribution amoung blocks");
                    Thread.Sleep(5000);
                }
            }
        }


        internal void SetNetworkModeToPrediction()
        {
            v1.ChangeNetworkModeToPrediction();
        }

        public void LearnNewObject(string objectName)
        {
            v1.LearnNewObject(objectName);
        }

        internal List<string> GetSupportedLabels(LearningUnitType luType) => GetLearningUnit(luType).somBBM_L3B_V.GetCurrentPredictions();

        internal void Clean()
        {
            pEncoder.Clean();
            v1.Clear();
            //v2.Clear();
            //v3.Clear();
        }

        internal List<string> GetCurrentPredictions() => v1.somBBM_L3B_V.GetCurrentPredictions();

        internal void BeginTraining(string objectLabel)
        {
            v1.BeginTraining(objectLabel);
        }
    }
}
