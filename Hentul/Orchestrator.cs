namespace Hentul
{
    using Common;
    using System.Runtime.InteropServices;
    using System;
    using Hentul.Hippocampal_Entorinal_complex;
    using System.Drawing.Imaging;
    using System.Drawing;
    using Hentul.Encoders;
    using OpenCvSharp.Text;

    public class Orchestrator
    {

        public struct POINT
        {
            public int X;
            public int Y;
        }

        #region DLLImport
        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool GetCursorPos(out POINT lpPoint);
        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr GetDesktopWindow();
        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr GetWindowDC(IntPtr window);
        [DllImport("gdi32.dll", SetLastError = true)]
        public static extern uint GetPixel(IntPtr dc, int x, int y);
        [DllImport("user32.dll", SetLastError = true)]
        public static extern int ReleaseDC(IntPtr window, IntPtr dc);
        [DllImport("User32.Dll")]
        public static extern long SetCursorPos(int x, int y);
        [DllImport("User32.Dll")]
        public static extern bool ClientToScreen(IntPtr hWnd, ref POINT point);
        #endregion

        #region CONSTRUCTOR

        #region Used Variables

        private static Orchestrator _orchestrator;

        public int Range { get; private set; }

        private bool LogMode { get; set; }

        public bool IsMock { get; private set; }

        public HippocampalComplex HCAccessor { get; private set; }

        public int[] MockBlockNumFires { get; private set; }

        private bool devbox = false;

        public int ImageIndex { get; private set; }

        public POINT point;

        public List<string> ImageList { get; private set; }

        public Bitmap bmp, bmpV2, bmpV3;

        public Bitmap bmp_g, bmpV2_g, bmpV3_g;

        public static string fileName;

        public string logfilename;

        public LogMode logMode;

        private List<string> objectlabellist { get; set; }

        private int imageIndex { get; set; }

        public NetworkMode NMode { get; set; }

        public VisionStreamProcessor VisionProcessor { get; set; }

        public TextStreamProcessor TextProcessor { get; private set; }

        public ulong CycleNum { get; private set; }

        private int NumColumns, X, Z;        

        #endregion

        private static readonly string baseDir = AppContext.BaseDirectory;
        
        private Orchestrator(int visionrange, bool isMock = false, bool ShouldInit = true,
                    NetworkMode nMode = NetworkMode.TRAINING, int mockImageIndex = 7)
        {
            X = 1250;
            NumColumns = 10;
            Z = 4;
            LogMode = false;
            Range = 10;
            NMode = nMode;
            logMode = Common.LogMode.BurstOnly;

            VisionProcessor = new VisionStreamProcessor(Range, NumColumns, X, logMode, isMock, ShouldInit);
            TextProcessor = new TextStreamProcessor(10, 5, logMode);

            if (isMock)
                ImageIndex = mockImageIndex;
            else
                ImageIndex = 0;

            Init();

            HCAccessor = new HippocampalComplex("Apple", isMock, nMode);

            objectlabellist = new List<string>
            {
                "Apple", "Ananas", "Watermelon", "JackFruit", "Grapes"
            };

            imageIndex = 1;
            fileName = Path.GetFullPath(Path.Combine(baseDir, @"..\..\..\..\..\Hentul\Hentul\Images\savedImage.png"));
            logfilename = Path.GetFullPath(Path.Combine(baseDir, @"..\..\..\..\..\Hentul\Logs\Hentul-Orchestrator.log"));
        }

        public void InitializeV2V3IfNeeded()
        {
            VisionProcessor.InitializeV2V3IfNeeded();
        }

        public static Orchestrator GetInstance(bool isMock = false, bool shouldInit = true, NetworkMode nMode = NetworkMode.TRAINING)
        {
            if (_orchestrator == null)
            {
                _orchestrator = new Orchestrator(10, isMock, shouldInit, nMode);
            }
            return _orchestrator;
        }

        private void Init()
        {
            Console.WriteLine("Finished Init for this Instance \n");
            Console.WriteLine("Range : " + Range.ToString() + "\n");
            Console.WriteLine("Initing SOM Instance now ... \n");
            Console.WriteLine("Finished Init for SOM Instance , Total Time ELapsed : \n");
            Console.WriteLine("Finished Initting of all Instances, System Ready!" + "\n");
        }

        public void BeginTraining(string objectLabel)
        {
            VisionProcessor.BeginTraining(objectLabel);
        }

        #endregion

        public void RecordAllRegions()
        {
            RecordPixels(LearningUnitType.V1);
            RecordPixels(LearningUnitType.V2);
            RecordPixels(LearningUnitType.V3);
        }

        #region Public API

        public void RecordPixels(LearningUnitType regionType = LearningUnitType.V1)
        {

            Console.WriteLine("Grabbing cursor Position");

            point = this.GetCurrentPointerPosition();

            Console.WriteLine("Grabbing Screen Pixels...");

            int currentRange = regionType switch
            {
                LearningUnitType.V1 => Range,      // 10  -> 20x20
                LearningUnitType.V2 => Range * 5,  // 50  -> 100x100
                LearningUnitType.V3 => Range * 10, // 100 -> 200x200
                _ => Range
            };


            int Range2 = Range + Range;     // We take in 20 rows and 40 columns , Mapper has similar mappings as well.

            int x1 = point.X - Range < 0 ? 0 : point.X - Range;
            int y1 = point.Y - Range < 0 ? 0 : point.Y - Range;
            int x2 = Math.Abs(point.X + Range2);
            int y2 = Math.Abs(point.Y + Range);

            //this.GetColorByRange(x1, y1, x2, y2);

            Rectangle rect = new Rectangle(x1, y1, x2, y2);

            bmp = new Bitmap(Range2 + Range2, Range2, PixelFormat.Format32bppArgb);                          

            switch (regionType)
            {
                case LearningUnitType.V1:
                    bmp = CaptureScreenRegion(rect);
                    bmp_g = ConverToEdgedBitmap(bmp);
                    break;
                case LearningUnitType.V2:
                    bmpV2 = CaptureScreenRegion(rect);
                    bmpV2 = ApplySubsampling(bmpV2, 20, 20, 3);
                    bmpV2_g = ConverToEdgedBitmap(bmpV2);
                    break;
                case LearningUnitType.V3:
                    bmpV3 = CaptureScreenRegion(rect);
                    bmpV3 = ApplySubsampling(bmpV3, 20, 20, 6);
                    bmpV3_g = ConverToEdgedBitmap(bmpV3);
                    break;
            }
        }

        private Bitmap CaptureScreenRegion(Rectangle rect)
        {
            var bmp = new Bitmap(rect.Width, rect.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

            using var g = Graphics.FromImage(bmp);

            g.CopyFromScreen(rect.Location, Point.Empty, rect.Size);

            return bmp;
        }

        private Bitmap ApplySubsampling(Bitmap source, int targetWidth, int targetHeight, int step)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (step <= 0) throw new ArgumentOutOfRangeException(nameof(step), "step must be >= 1");

            int width = source.Width;
            int height = source.Height;

            // If step == 1 treat as a full copy or optional resize to target dims.
            if (step == 1)
            {
                if (targetWidth > 0 && targetHeight > 0 && (targetWidth != width || targetHeight != height))
                {
                    var scaled = new Bitmap(targetWidth, targetHeight, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                    using var g = Graphics.FromImage(scaled);
                    g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                    g.DrawImage(source, 0, 0, targetWidth, targetHeight);
                    return scaled;
                }

                return new Bitmap(source);
            }

            // Create a result the same size as source and clear to a background color (black).
            var result = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            using (var g = Graphics.FromImage(result))
            {
                g.Clear(Color.Black);
            }

            // Copy only every 'step' pixel in both X and Y (keeps sampled pixels at original coordinates).
            // Note: uses GetPixel/SetPixel for simplicity. For large images or tight loops consider LockBits for performance.
            for (int y = 0; y < height; y += step)
            {
                for (int x = 0; x < width; x += step)
                {
                    // bounds guaranteed by loops
                    var color = source.GetPixel(x, y);
                    result.SetPixel(x, y, color);
                }
            }

            // If caller requested a downsampled target size, scale the sparse-sampled result to the target.
            if (targetWidth > 0 && targetHeight > 0 && (targetWidth != width || targetHeight != height))
            {
                var scaled = new Bitmap(targetWidth, targetHeight, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                using (var g2 = Graphics.FromImage(scaled))
                {
                    g2.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
                    g2.DrawImage(result, 0, 0, targetWidth, targetHeight);
                }
                result.Dispose();
                return scaled;
            }

            return result;
        }        

        public void ProcessVisual(ulong cycle)
        {
            if(cycle != 0 && cycle <= CycleNum)
            {
                throw new InvalidOperationException("Orchestrator :: incoming Cycle number should always be greater than internal cycle number!");
            }

            CycleNum = cycle;

            // Process all three regions
            VisionProcessor.Process(bmp, bmpV2, bmpV3, cycle);
        }

        public void AddNewCharacterSensationToHC(char ch)
        {
            if (!NMode.Equals(NetworkMode.TRAINING))
            {
                throw new InvalidOperationException("AddNewCharacterSensationToHC_T :: Network Should be in Training Mode before Predicting!");
            }

            TextProcessor.ProcessInput(ch, CycleNum);

            var som_SDR = TextProcessor.GetL3BSensation(CycleNum);

            if (som_SDR == null || som_SDR.ActiveBits.Count == 0)
            {
                throw new InvalidOperationException("som_SDR should not be Empty!");
            }

            
            Sensation firingSensei = TextProcessor.ConvertSDR_To_Sensation(som_SDR);

            if (HCAccessor.AddNewSensationToObject(firingSensei) == false)
            {
                throw new InvalidOperationException("Could Not Add Object to HC! Sensation already exist in the current Object");
            }
        }

        public List<string> GetPredictionsVisual()
        {
            if (NMode != NetworkMode.PREDICTION)
                throw new InvalidOperationException("Network Must be in PRediction Mode!");

            var predictions = VisionProcessor.GetCurrentPredictions();

            return predictions;
        }

        public void DoneWithTraining(string label = "")
        {
            HCAccessor.DoneWithTraining(label);
        }

        public void ChangeNetworkToPredictionMode()
        {
            NMode = NetworkMode.PREDICTION;
            VisionProcessor.SetNetworkModeToPrediction();
            //HCAccessor.DoneWithTraining();
            //HCAccessor.SetNetworkModeToPrediction();
        }

        //Gets the current SDR and next cycle predited SDR from classifier layer
        internal Tuple<Sensation_Location, Sensation_Location> GetSDRFromL3B()
        {

            Sensation_Location sensei = null, predictedSensei = null;

            var som_SDR = VisionProcessor.GetSL3BLatestFiringCells(LearningUnitType.V1, CycleNum);
            var predictedSDR = VisionProcessor.GetSL3BLatestFiringCells(LearningUnitType.V1, CycleNum + 1);

            if (som_SDR != null)
            {
                sensei = VisionProcessor.pEncoder.GetSenseiFromSDR_V(som_SDR, point);
            }

            if (predictedSDR != null)
            {
                predictedSensei = VisionProcessor.pEncoder.GetSenseiFromSDR_V(predictedSDR, point);
            }

            return new Tuple<Sensation_Location, Sensation_Location>(sensei, predictedSensei);
        }        

        #endregion

        #region Private Methods

        /// <summary>
        /// Takens in a bmp and preps and fires all FOM & SOM's.
        /// </summary>     
        private void ParseNFireBitmap(Bitmap greyScalebmp)
        {

            VisionProcessor.pEncoder.ParseBitmap(greyScalebmp);

            foreach (var kvp in VisionProcessor.pEncoder.FOMBBMIDS)
            {
                switch (kvp.Key)
                {
                    case MAPPERCASE.ALL:
                        {
                            foreach (var bbmID in kvp.Value)
                            {
                                VisionProcessor.pEncoder.GetSDR_SOMForMapperCase(MAPPERCASE.ALL, bbmID);
                            }
                        }
                        break;
                    case MAPPERCASE.ONETWOTHREEE:
                        {
                            foreach (var bbmID in kvp.Value)
                            {
                                VisionProcessor.pEncoder.GetSDR_SOMForMapperCase(MAPPERCASE.ONETWOTHREEE, bbmID);
                            }
                        }
                        break;
                    case MAPPERCASE.TWOTHREEFOUR:
                        {

                            foreach (var bbmID in kvp.Value)
                            {
                                VisionProcessor.pEncoder.GetSDR_SOMForMapperCase(MAPPERCASE.TWOTHREEFOUR, bbmID);
                            }
                        }
                        break;
                    case MAPPERCASE.ONETWOFOUR:
                        {
                            foreach (var bbmID in kvp.Value)
                            {
                                VisionProcessor.pEncoder.GetSDR_SOMForMapperCase(MAPPERCASE.ONETWOFOUR, bbmID);
                            }
                        }
                        break;
                    case MAPPERCASE.ONETHREEFOUR:
                        {
                            foreach (var bbmID in kvp.Value)
                            {
                                VisionProcessor.pEncoder.GetSDR_SOMForMapperCase(MAPPERCASE.ONETHREEFOUR, bbmID);
                            }
                        }
                        break;
                    case MAPPERCASE.ONETWO:
                        {
                            foreach (var bbmID in kvp.Value)
                            {
                                VisionProcessor.pEncoder.GetSDR_SOMForMapperCase(MAPPERCASE.ONETWO, bbmID);
                            }
                        }
                        break;
                    case MAPPERCASE.ONETHREE:
                        {
                            foreach (var bbmID in kvp.Value)
                            {
                                VisionProcessor.pEncoder.GetSDR_SOMForMapperCase(MAPPERCASE.ONETHREE, bbmID);
                            }
                        }
                        break;
                    case MAPPERCASE.ONEFOUR:
                        {
                            foreach (var bbmID in kvp.Value)
                            {
                                VisionProcessor.pEncoder.GetSDR_SOMForMapperCase(MAPPERCASE.ONEFOUR, bbmID);
                            }
                        }
                        break;
                    case MAPPERCASE.TWOTHREE:
                        {
                            foreach (var bbmID in kvp.Value)
                            {
                                VisionProcessor.pEncoder.GetSDR_SOMForMapperCase(MAPPERCASE.TWOTHREE, bbmID);
                            }
                        }
                        break;
                    case MAPPERCASE.TWOFOUR:
                        {
                            foreach (var bbmID in kvp.Value)
                            {
                                VisionProcessor.pEncoder.GetSDR_SOMForMapperCase(MAPPERCASE.TWOFOUR, bbmID);
                            }
                        }
                        break;
                    case MAPPERCASE.THREEFOUR:
                        {
                            foreach (var bbmID in kvp.Value)
                            {
                                VisionProcessor.pEncoder.GetSDR_SOMForMapperCase(MAPPERCASE.THREEFOUR, bbmID);
                            }
                        }
                        break;
                    case MAPPERCASE.ONE:
                        {
                            foreach (var bbmID in kvp.Value)
                            {
                                VisionProcessor.pEncoder.GetSDR_SOMForMapperCase(MAPPERCASE.ONE, bbmID);
                            }
                        }
                        break;
                    case MAPPERCASE.TWO:
                        {
                            foreach (var bbmID in kvp.Value)
                            {
                                VisionProcessor.pEncoder.GetSDR_SOMForMapperCase(MAPPERCASE.TWO, bbmID);
                            }
                        }
                        break;
                    case MAPPERCASE.THREE:
                        {
                            foreach (var bbmID in kvp.Value)
                            {
                                VisionProcessor.pEncoder.GetSDR_SOMForMapperCase(MAPPERCASE.THREE, bbmID);
                            }
                        }
                        break;
                    case MAPPERCASE.FOUR:
                        {
                            foreach (var bbmID in kvp.Value)
                            {
                                VisionProcessor.pEncoder.GetSDR_SOMForMapperCase(MAPPERCASE.FOUR, bbmID);
                            }
                        }
                        break;
                    default:
                        {
                            throw new NotImplementedException();
                        }
                }
            }
        }

        private List<Position_SOM> Conver2DtoSOMList(List<Position2D> somList)
        {
            List<Position_SOM> toReturn = new List<Position_SOM>();

            foreach (var item in somList)
            {
                toReturn.Add(new Position_SOM(item.X, item.Y));
            }

            return toReturn;
        }

        private uint GetTotalBurstCountInFOMLayerInLastCycle(ulong cycleNum)
        {
            uint totalBurstCount = 0;

            foreach (var fom in VisionProcessor.GetFOMBBMVFromLearningUnit(LearningUnitType.V1))
            {
                totalBurstCount += fom.GetTotalBurstCountInLastCycle(cycleNum);
            }

            return totalBurstCount;
        }


        // Call only after Post Classification : Decision has been made on the object and now we are actively what the object might look like at different locations.
        public bool BiasFOM(SDR_SOM hcSignal)
        {
            var spatialSignal = PixelEncoder.GetFOMEquivalentPositionsofSOM(hcSignal.ActiveBits);

            if (spatialSignal == null)
            {
                WriteLogsToFile("Empty Polarizing Signal for FOM During Waaandering!!!");
                return false;
            }

            bool flag = false;

            foreach (var kvp in spatialSignal)
            {
                if (kvp.Value.Count > 0)
                {
                    SDR_SOM fomSDR = null;

                    if (hcSignal.InputPatternType.Equals(iType.TEMPORAL))
                    {
                        fomSDR = new SDR_SOM(NumColumns, Z, kvp.Value, iType.TEMPORAL);
                    }
                    else if (hcSignal.InputPatternType == iType.APICAL)
                    {
                        fomSDR = new SDR_SOM(NumColumns, NumColumns, kvp.Value, iType.APICAL);
                    }

                    if (fomSDR != null && kvp.Key < 101)
                    {
                        RemoveDuplicateEntries(ref fomSDR);

                        if (kvp.Key == 30)
                        {
                            bool bp = true;
                        }

                        VisionProcessor.GetFOMBBMVFromLearningUnit(LearningUnitType.V1)[kvp.Key].Fire(fomSDR, CycleNum);

                        flag = true;
                    }
                }
            }

            return flag;
        }

        #endregion

        #region Helper Methods

        public void RemoveDuplicateEntries(ref SDR_SOM sdr_SOM)
        {
            int i = 0, j = 0;
            List<int> indexsToRemove = new List<int>();
            List<Position_SOM> newActiveBitsList = new List<Position_SOM>();

            foreach (var pos1 in sdr_SOM.ActiveBits)
            {
                if (newActiveBitsList.Where(item => item.X == pos1.X && item.Y == pos1.Y).Count() == 0)
                {
                    newActiveBitsList.Add(pos1);
                }
                else
                {
                    int breakpoint = 1;
                }
            }

            sdr_SOM.ActiveBits = newActiveBitsList;
        }

        public Bitmap ConverToEdgedBitmap(Bitmap bitmap)
        {
            Bitmap newBitmap = new Bitmap(bitmap.Width, bitmap.Height);

            //get a graphics object from the new image
            using (Graphics g = Graphics.FromImage(newBitmap))
            {

                //create the grayscale ColorMatrix
                ColorMatrix colorMatrix = new ColorMatrix(
                   new float[][]
                   {
                     new float[] {.3f, .3f, .3f, 0, 0},
                     new float[] {.59f, .59f, .59f, 0, 0},
                     new float[] {.11f, .11f, .11f, 0, 0},
                     new float[] {0, 0, 0, 1, 0},
                     new float[] {0, 0, 0, 0, 1}
                   });

                //create some image attributes
                using (ImageAttributes attributes = new ImageAttributes())
                {

                    //set the color matrix attribute
                    attributes.SetColorMatrix(colorMatrix);

                    //draw the original image on the new image
                    //using the grayscale color matrix
                    g.DrawImage(bitmap, new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                                0, 0, bmp.Width, bmp.Height, GraphicsUnit.Pixel, attributes);
                }
            }

            return newBitmap;
        }

        public RecognisedVisualEntity GetPredictedObject() => HCAccessor.GetCurrentPredictedObject();

        public RecognitionState CheckIfObjectIsRecognised() => HCAccessor.ObjectState;


        private bool CompareTwoPositionLists(List<Position_SOM> pattern1, List<Position_SOM> pattern2)
        {
            int breakpint = 1;

            pattern1.Sort();

            pattern2.Sort();

            List<int> unmatchedIndex = new List<int>();
            int bp2 = 1;

            int matchCount = 0;
            int index = 0;

            foreach (var item in pattern2)
            {
                if (pattern1.Where(x => x.X == item.X && x.Y == item.Y && x.Z == item.Z).Count() != 0)
                    matchCount++;
                else
                {
                    unmatchedIndex.Add(index);
                }

                index++;
            }

            return matchCount == pattern2.Count;
        }

        #endregion

        #region BIG MAN WORK

        //public string StartCycle()
        //{

        //    //Check how big the entire screen / image is and kee papring through all the objects.

        //    var str = DetectObject();

        //    StoreObject();

        //    return str;



        //}

        /// <summary>
        /// PROBLEMS:
        /// 
        /// ALGORITHM:
        /// 
        /// </summary>
        //public string DetectObject()
        //{
        //    var obj = string.Empty;

        //    if (Objects == null || Objects.Count == 0)
        //    {
        //        obj = LearnFirstObject();

        //    }

        //    //Traverse through Object Maps and what sensory inputs are telling you.

        //    return obj;
        //}



        /// <summary>
        /// 
        /// PROBLEMS:
        /// 1. How to figure out on the current mouse position whether if its on an object and how to traverse through the object of the object itself is bigger than the current visual radius.
        /// 2. How to handle 2 different types of feed processing ? Narrow vs Broad fields of vision.
        /// 
        /// ALGORITHM : 
        /// 1. Get Current Position of the Mouse Pointer and get current Screen Measurements.
        /// 2. Run around the object trying to figure out the object, go to as many unexplored locations on the object as possible.
        /// 3. After you feel you have explored all the points on the object , concurrently keep storing all the locations onto the new unrecognised Object.                
        /// </summary>
        private string LearnFirstObject()
        {
            //No Object Frame , Have to create a sense @ Location object map from scratch for this particular object.            

            // -> L4, -> L3b, L3b -> HP -> output , DoesItMatch Expectation ? y ? Get 5 more confirmations and store model : work on the second prediction if there is 

            string obj = string.Empty;

            bool stillRecognising = true;

            //while (stillRecognising)
            //{
            //    // feed L4
            //    SDR_SOM fomSdr;
            //    for (int i = 0; i < fomBBM.Length; i++)
            //    {
            //        fomSdr = GetFomSdrForIteration(i, mapper);
            //        fomBBM[i].Fire(fomSdr);
            //    }

            //    //feed same pattern SOM BBM L3A
            //    SDR_SOM Sdr_Som3A = new SDR_SOM(10, 10, new List<Position_SOM>() { }, iType.SPATIAL);
            //    somBBM_L3Af.Fire(Sdr_Som3A);

            //    // init L3B to Apple
            //    SDR_SOM Sdr_SomL3B = GetSdrSomFromFOMs();
            //    somBBM_L3BV.Fire(Sdr_SomL3B);

            //    // Push new object representation to 3A
            //    // Push Wiring Burst Avoiding LTP to 4 from 3A.
            //}


            return obj;
        }

        #endregion

        #region Public Helper Methods

        public static void MoveCursorToSpecificPosition(int x, int y)
        {
            POINT p;
            IntPtr desk = GetDesktopWindow();
            IntPtr dc = GetWindowDC(desk);

            p.X = x;
            p.Y = y;

            ClientToScreen(dc, ref p);
            SetCursorPos(p.X, p.Y);

            ReleaseDC(desk, dc);
        }

        public void MoveCursor(POINT p)
        {
            IntPtr desk = GetDesktopWindow();
            IntPtr dc = GetWindowDC(desk);

            ClientToScreen(dc, ref p);
            SetCursorPos(p.X, p.Y);

            ReleaseDC(desk, dc);
        }

        public POINT GetCurrentPointerPosition()
        {
            POINT point;

            point = new POINT();
            point.X = 0;
            point.Y = 0;

            if (GetCursorPos(out point))
            {
                //Console.Clear();
                //Console.WriteLine(point.X.ToString() + " " + point.Y.ToString());
                return point;
            }

            return point;
        }

        public static Position GetCurrentPointerPosition1()
        {
            Position position = null;
            POINT point;

            point = new POINT();
            point.X = 0;
            point.Y = 0;

            if (GetCursorPos(out point))
            {
                //Console.Clear();
                //Console.WriteLine(point.X.ToString() + " " + point.Y.ToString());
                position = new Position(point.X, point.Y);
            }

            return position;
        }

        private SDR_SOM AddSOMOverheadtoFOMSDR(SDR_SOM fomSDR, int blockidX, int blockIdY)
        {
            SDR_SOM toRet;
            int block_offset = 10;
            List<Position_SOM> newPosList = new List<Position_SOM>();

            foreach (var pos in fomSDR.ActiveBits)
            {
                newPosList.Add(new Position_SOM(blockidX * block_offset + pos.X, pos.Y));
            }

            toRet = new SDR_SOM(1250, 10, newPosList, iType.SPATIAL);

            return toRet;
        }

        public bool CheckifPixelisBlack(int x, int y)
        {
            if (x >= bmp.Width || y >= bmp.Height)
                return false;

            var color = bmp.GetPixel(x, y);

            return (color.R < 200 && color.G < 200 && color.B < 200);
        }

        public void ChangeNetworkModeToPrediction()
        {
            NMode = NetworkMode.PREDICTION;            
            VisionProcessor.SetNetworkModeToPrediction();
            //HCAccessor.SetNetworkModeToPrediction();
        }

        public void ChangeNetworkModeToTraining()
        {
            NMode = NetworkMode.TRAINING;
            HCAccessor.SetNetworkModeToTraining();
        }

        private void PrintMoreBlockVitals()
        {
            VisionProcessor.PrintBlockVitalVision(LearningUnitType.V1);
        }

        public void BackUp()
        {

            VisionProcessor.Backup();
            HCAccessor.Backup();

        }


        public static void WriteLogsToFile(string logMsg)
        {
            File.WriteAllText(fileName, logMsg);
        }

        public void Restore()
        {
            VisionProcessor.Restore();
            _orchestrator.HCAccessor = HippocampalComplex.Restore();
        }

        #endregion

        #region Future Work

        private void MoveToNextObject()
        {
            throw new NotImplementedException();
        }

        private void StoreObject()
        {
            throw new NotImplementedException();
        }

        #region PRIVATE HELPER METHODS

        private void GetPixelsAsPerScope()
        {

            POINT currentMousePosition = GetCurrentPointerPosition();


            switch (GetScope())
            {
                case VisionScope.NarrowScope:       // High Detail In-Object Scope
                    {
                        break;
                    }
                case VisionScope.ObjectScope:       // Fully View of Object
                    {
                        break;
                    }
                case VisionScope.BroadScope:        // complete visual view at capacity.
                    {
                        break;
                    }
                default:
                    {
                        break;
                    }
            }
        }

        private VisionScope GetScope()
        {
            VisionScope scopeToReturn = VisionScope.NarrowScope;

            bool makesSense = false;

            while (makesSense != false)
            {

                // Get Screen Pixel Values 

                // Analyse

                // Increase or decrease Scope && try again                
            }

            return scopeToReturn;
        }
        

        private Bitmap GrabWholeScreen()
        {

            // Bitmap bmpScreenCapture = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
            throw new NotImplementedException();



        }

                
        private void GetColorByRange(int x1, int y1, int x2, int y2)
        {
            IntPtr desk = GetDesktopWindow();

            IntPtr dc = GetWindowDC(desk);


            for (int i = x1, k = 0; i < x2 && k < Range + Range; i++, k++)
            {
                for (int j = y1, l = 0; j < y2 && l < Range + Range; j++, l++)
                {
                    int a = (int)GetPixel(dc, i, j);

                    Color color = System.Drawing.Color.FromArgb(255,
                                                 (a >> 0) & 0xff,
                                                 (a >> 8) & 0xff,
                                                 (a >> 16) & 0xff);

                    bmp.SetPixel(k, l, color);

                }

            }

            ReleaseDC(desk, dc);
        }

        #endregion

        #endregion
        

        #region LEGACY CODE

        public Position2D Verify_Predict_HC(bool isMock = false, uint iterationsToConfirmation = 10, bool legacyPipeline = false)
        {
            Position2D motorOutput = null;
            List<Position2D> positionToConfirm = new List<Position2D>();

            if (!NMode.Equals(NetworkMode.PREDICTION))
            {
                throw new InvalidOperationException("Invalid State Managemnt!");
            }

            // If any output from HC execute the location output if NOT then take the standard default output.                
            var som_SDR = VisionProcessor.GetSL3BLatestFiringCells(LearningUnitType.V1, CycleNum);
            var predictedSDR = VisionProcessor.GetSL3BLatestFiringCells(LearningUnitType.V1, CycleNum + 1);


            if (som_SDR != null)
            {
                var firingSensei = VisionProcessor.pEncoder.GetSenseiFromSDR_V(som_SDR, point);
                var predictedSensei = VisionProcessor.pEncoder.GetSenseiFromSDR_V(predictedSDR, point);

                List<string> predictedLabels = VisionProcessor.GetSupportedLabels(LearningUnitType.V1);

                if (legacyPipeline)
                {
                    motorOutput = HCAccessor.VerifyObject(firingSensei, null, isMock, iterationsToConfirmation);
                }
                else    // brand New Pipeline : Classification done Primarily through V1.
                {
                    if (VisionProcessor.v1.somBBM_L3B_V.NetWorkMode == NetworkMode.DONE)
                    {
                        VisionProcessor.v1.somBBM_L3B_V.GetCurrentPredictions();
                    }
                }
            }

            return motorOutput;
        }

        //Stores the new object on to HC
        public void AddNewVisualSensationToHc()
        {
            if (!NMode.Equals(NetworkMode.TRAINING))
            {
                throw new InvalidOperationException("INVALID State Management!");
            }

            var som_SDR = VisionProcessor.GetSL3BLatestFiringCells(LearningUnitType.V1, CycleNum);

            if (som_SDR != null)
            {
                //Wrong : location should be the location of the mouse pointer relative to the image and not just BBMID.
                var firingSensei = VisionProcessor.pEncoder.GetSenseiFromSDR_V(som_SDR, point);

                if (HCAccessor.AddNewSensationLocationToObject(firingSensei) == false)
                {
                    throw new InvalidOperationException("Could Not Add Object to HC ! Either it was NOT in TRAINING MODE or sensation already exist in the current Object");
                }
            }
            else
            {
                throw new InvalidOperationException(" som_SDR should not be null!");
            }
        }        

        public void LearnNewObject(string v)
        {
            VisionProcessor.LearnNewObject(v);
        }

        #endregion
    }


    #region Enums

    public enum LearningUnitType
    {
        V1,
        V2,
        V3
    }

    public enum VisionScope
    {
        BroadScope,
        ObjectScope,
        NarrowScope,
        UNKNOWN
    }

    #endregion
}
