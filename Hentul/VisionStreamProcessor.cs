﻿using System.Runtime.InteropServices;

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

        // Enhanced region support
        public enum RegionVersion { V1_20, V2_100, V3_200 }

        public sealed class SamplingSettings
        {
            public RegionVersion Version { get; set; } = RegionVersion.V1_20;
            public bool TrackCursor { get; set; } = true;
            /// <summary>
            /// 1 = no subsampling; 3 = "every 3rd pixel" requirement
            /// </summary>
            public int SubsampleStep { get; set; } = 1;
            /// <summary>
            /// Whitescale threshold (used if you convert to binary).
            /// </summary>
            public byte WhiteThreshold { get; set; } = 200;
            /// <summary>
            /// If true, invert whitescale (optional).
            /// </summary>
            public bool InvertWhite { get; set; } = false;
            /// <summary>
            /// Frame skipping for real-time performance
            /// </summary>
            public int FrameSkipCount { get; set; } = 0;
        }

        private readonly SamplingSettings _settings = new SamplingSettings();

        // Configuration methods
        public void SetRegionVersion(RegionVersion version) => _settings.Version = version;
        public void SetCursorTracking(bool enabled) => _settings.TrackCursor = enabled;
        public void SetSubsampleStep(int step) => _settings.SubsampleStep = Math.Max(1, step);
        public void SetWhitescale(byte threshold, bool invert = false)
        {
            _settings.WhiteThreshold = threshold;
            _settings.InvertWhite = invert;
        }
        public void SetFrameSkipping(int skipCount) => _settings.FrameSkipCount = Math.Max(0, skipCount);

        public LearningUnit v1 { get; private set; }    // 20x20 (original)
        public LearningUnit v2 { get; private set; }    // 100x100
        public LearningUnit v3 { get; private set; }    // 200x200

        public int Range { get; private set; }

        int X, NumColumns, Z;

        bool IsMock;

        public ulong CycleNum;

        public LogMode LogMode { get; private set; }

        // Multiple encoders for different scales
        public PixelEncoder pEncoderV1 { get; private set; }
        public PixelEncoder pEncoderV2 { get; private set; }
        public PixelEncoder pEncoderV3 { get; private set; }
        public PixelEncoder pEncoder => pEncoderV1;
        private bool _v2v3Initialized = false;


        public Bitmap bmpV1 { get; private set; }
        public Bitmap bmpV2 { get; private set; }
        public Bitmap bmpV3 { get; private set; }

        public string logfilename { get; private set; }

        public int numberOFLearningUnitsNeeded => 3; // Support all 3 regions

        private int _frameSkipCounter = 0;

        public VisionStreamProcessor(int range, int numColumns, int x, LogMode logMode, bool isMock, bool shouldInit)
        {
            this.X = x;
            this.NumColumns = numColumns;
            IsMock = isMock;
            Z = 5;
            CycleNum = 0;

            if (range != 10)
            {
                throw new InvalidOperationException("Invalid Operation !");
            }

            Range = range;

           
            BlockSize = (2 * range) * (2 * range);
            NumBBMNeededV = 100;
            pEncoderV1 = new PixelEncoder(100, 400);
            bmpV1 = new Bitmap(range + range, range + range);

            
            pEncoderV2 = new PixelEncoder(100, 400);
            bmpV2 = new Bitmap(100, 100);

            pEncoderV3 = new PixelEncoder(100, 400);
            bmpV3 = new Bitmap(200, 200);

            numPixelsProcessedPerBBM = 4;
            logfilename = Path.GetFullPath(Path.Combine(baseDir, @"..\..\..\..\..\Hentul\Logs\Hentul-Orchestrator.log"));

            if (shouldInit)
            {
                v1 = new LearningUnit(NumBBMNeededV, NumColumns, Z, X, shouldInit, logfilename, LearningUnitType.V1);
                v1.Init();

                Console.WriteLine("V1 - Total Number of Pixels: " + (Range * Range * 4).ToString());
                Console.WriteLine("V1 - Total First Order BBMs Created: " + NumBBMNeededV.ToString());
                Console.WriteLine("V2/V3 - Available on demand (not yet initialized)");
            }

            LogMode = logMode;
        }
        public void InitializeV2V3IfNeeded()
        {
            if (_v2v3Initialized) return;

            try
            {
                Console.WriteLine("Initializing V2/V3 regions...");

                int numBBMV2 = 100;   
                int numBBMV3 = 100;    

                v2 = new LearningUnit(numBBMV2, NumColumns, Z, X * 2, true, logfilename, LearningUnitType.V2);
                v2.Init();

                v3 = new LearningUnit(numBBMV3, NumColumns, Z, X * 3, true, logfilename, LearningUnitType.V3);
                v3.Init();

                _v2v3Initialized = true;
                Console.WriteLine("V2/V3 regions initialized successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"V2/V3 initialization failed: {ex.Message}");
            }
        }
        public async Task InitializeV2V3Async()
        {
            await Task.Run(() => InitializeV2V3IfNeeded());
        }
        #endregion

        public void Process(Bitmap greyScalebmp, ulong cycle)
        {
            CycleNum = cycle;

            // Frame skipping optimization
            if (_settings.FrameSkipCount > 0)
            {
                _frameSkipCounter++;
                if (_frameSkipCounter <= _settings.FrameSkipCount)
                    return;
                _frameSkipCounter = 0;
            }

            // Always process V1
            ProcessV1(greyScalebmp, cycle);

            
            if (_v2v3Initialized && v2 != null && v3 != null)
            {
                ProcessV2(greyScalebmp, cycle);
                ProcessV3(greyScalebmp, cycle);
            }

            Clean();
        }

        private void ProcessV1(Bitmap greyScalebmp, ulong cycle)
        {
            // Apply subsampling if needed
            var processedBmp = ApplySubsampling(greyScalebmp, 20, 20);
            var grayscaleBmp = ConvertToGrayscale(processedBmp);
            var whitescaleBmp = ConvertToWhitescale(grayscaleBmp);

            pEncoderV1.ParseBitmap(whitescaleBmp);
            v1.Process(pEncoderV1, cycle);
        }
       
        private void ProcessV2(Bitmap greyScalebmp, ulong cycle)
        {
            // Scale to 100x100 with subsampling
            var processedBmp = ApplySubsampling(greyScalebmp, 100, 100);
            var grayscaleBmp = ConvertToGrayscale(processedBmp);
            var whitescaleBmp = ConvertToWhitescale(grayscaleBmp);

            pEncoderV2.ParseBitmap(whitescaleBmp);
            v2.Process(pEncoderV2, cycle);
        }

        private void ProcessV3(Bitmap greyScalebmp, ulong cycle)
        {
            // Scale to 200x200 with subsampling
            var processedBmp = ApplySubsampling(greyScalebmp, 200, 200);
            var grayscaleBmp = ConvertToGrayscale(processedBmp);
            var whitescaleBmp = ConvertToWhitescale(grayscaleBmp);

            pEncoderV3.ParseBitmap(whitescaleBmp);
            v3.Process(pEncoderV3, cycle);
        }

      
        private Bitmap ApplySubsampling(Bitmap source, int targetWidth, int targetHeight)
        {
            var result = new Bitmap(targetWidth, targetHeight);
            var stepX = (double)source.Width / targetWidth;
            var stepY = (double)source.Height / targetHeight;

            using (var g = Graphics.FromImage(result))
            {
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
                g.DrawImage(source, 0, 0, targetWidth, targetHeight);
            }

            // Apply "every 3rd pixel" sampling if configured
            if (_settings.SubsampleStep > 1)
            {
                return ApplyPixelSubsampling(result, _settings.SubsampleStep);
            }

            return result;
        }

        private Bitmap ApplyPixelSubsampling(Bitmap source, int step)
        {
            var result = new Bitmap(source.Width, source.Height);

            for (int y = 0; y < source.Height; y += step)
            {
                for (int x = 0; x < source.Width; x += step)
                {
                    if (x < source.Width && y < source.Height)
                    {
                        var color = source.GetPixel(x, y);
                        // Fill the step area with the same color
                        for (int dy = 0; dy < step && y + dy < source.Height; dy++)
                        {
                            for (int dx = 0; dx < step && x + dx < source.Width; dx++)
                            {
                                result.SetPixel(x + dx, y + dy, color);
                            }
                        }
                    }
                }
            }

            return result;
        }

        private Bitmap ConvertToGrayscale(Bitmap source)
        {
            var result = new Bitmap(source.Width, source.Height);

            for (int y = 0; y < source.Height; y++)
            {
                for (int x = 0; x < source.Width; x++)
                {
                    var color = source.GetPixel(x, y);
                    var gray = (int)(color.R * 0.3 + color.G * 0.59 + color.B * 0.11);
                    var grayColor = Color.FromArgb(gray, gray, gray);
                    result.SetPixel(x, y, grayColor);
                }
            }

            return result;
        }

        private Bitmap ConvertToWhitescale(Bitmap source)
        {
            var result = new Bitmap(source.Width, source.Height);

            for (int y = 0; y < source.Height; y++)
            {
                for (int x = 0; x < source.Width; x++)
                {
                    var color = source.GetPixel(x, y);
                    var gray = (int)(color.R * 0.3 + color.G * 0.59 + color.B * 0.11);

                    bool isWhite = gray >= _settings.WhiteThreshold;
                    if (_settings.InvertWhite) isWhite = !isWhite;

                    var whiteColor = isWhite ? Color.White : Color.Black;
                    result.SetPixel(x, y, whiteColor);
                }
            }

            return result;
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
            return lType switch
            {
                LearningUnitType.V1 => v1 ?? throw new InvalidOperationException("V1 not initialized"),
                LearningUnitType.V2 => v2 ?? throw new InvalidOperationException("V2 not initialized"),
                LearningUnitType.V3 => v3 ?? throw new InvalidOperationException("V3 not initialized"),
                _ => throw new InvalidOperationException("No Matching input Learning Unit Type!")
            };
        }

        public Bitmap GetProcessedBitmap(LearningUnitType luType)
        {
            return luType switch
            {
                LearningUnitType.V1 => bmpV1,
                LearningUnitType.V2 => bmpV2,
                LearningUnitType.V3 => bmpV3,
                _ => throw new InvalidOperationException("Invalid learning unit type")
            };
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
            Console.WriteLine($"Enter '1' to see a list of all the Block Usage List for {luType}:");

            int w = Console.Read();

            if (w == 49)
            {
                ulong totalIncludedCycle = 0;
                var fomBBMV = GetFOMBBMVFromLearningUnit(luType);

                for (int i = 0; i < fomBBMV.Length; i++)
                {
                    if (fomBBMV[i].BBMID != 0)
                        Console.WriteLine($"{luType} - {i} :: Block ID : {fomBBMV[i].PrintBlockDetailsSingleLine()} | Included Cycle: {fomBBMV[i].CycleNum}");

                    totalIncludedCycle += fomBBMV[i].CycleNum;
                }

                Console.WriteLine($"{luType} - Total Participated Cycles: {totalIncludedCycle}");
                Console.WriteLine($"Orchestrator CycleNum: {CycleNum}");

                if (totalIncludedCycle != CycleNum)
                {
                    Console.WriteLine($"ERROR: Incorrect Cycle Distribution among {luType} blocks");
                    Thread.Sleep(5000);
                }
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
            v2?.LearnNewObject(objectName);
            v3?.LearnNewObject(objectName);
        }

        internal List<string> GetSupportedLabels(LearningUnitType luType) => GetLearningUnit(luType).somBBM_L3B_V.GetCurrentPredictions();

        internal void Clean()
        {
            pEncoderV1?.Clean();
            pEncoderV2?.Clean();
            pEncoderV3?.Clean();
            v1?.Clear();
            v2?.Clear();
            v3?.Clear();
        }

        // Combined predictions from all regions
        internal List<string> GetCurrentPredictions()
        {
            var predictions = new List<string>();

            if (v1?.somBBM_L3B_V != null)
                predictions.AddRange(v1.somBBM_L3B_V.GetCurrentPredictions());
            if (v2?.somBBM_L3B_V != null)
                predictions.AddRange(v2.somBBM_L3B_V.GetCurrentPredictions());
            if (v3?.somBBM_L3B_V != null)
                predictions.AddRange(v3.somBBM_L3B_V.GetCurrentPredictions());

            return predictions.Distinct().ToList();
        }

        internal void BeginTraining(string objectLabel)
        {
            v1?.BeginTraining(objectLabel);
            v2?.BeginTraining(objectLabel);
            v3?.BeginTraining(objectLabel);
        }
    }
}