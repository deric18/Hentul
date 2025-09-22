namespace Hentul
{
    using System;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.Runtime.InteropServices;
    using Common;
    using Hentul.Encoders;
    using Hentul.Hippocampal_Entorinal_complex;
    using Hentul.Interfaces;
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

        public int ImageIndex { get; private set; }

        public POINT point;

        public List<string> ImageList { get; private set; }

        public Bitmap bmp;

        public static string ImagefileName;

        public string logfilename;

        public LogMode logMode;

        private List<string> objectlabellist { get; set; }

        private int imageIndex { get; set; }

        public NetworkMode NMode { get; set; }

        public VisionStreamProcessor VisionProcessor { get; set; }

        public TextStreamProcessor TextProcessor { get; private set; }

        public ulong CycleNum { get; private set; }

        private int NumColumns, X, Z;

        public Logger logger;

        #endregion

        private static readonly string baseDir = AppContext.BaseDirectory;

        private Orchestrator(int visionrange, bool isMock = false, bool ShouldInit = true, NetworkMode nMode = NetworkMode.TRAINING, int mockImageIndex = 7)
        {

            X = 1250;

            NumColumns = 10;

            Z = 4;

            LogMode = false;

            Range = visionrange;

            NMode = nMode;

            logMode = Common.LogMode.BurstOnly;

            ImagefileName = Path.GetFullPath(Path.Combine(baseDir, @"..\..\..\..\..\Hentul\Hentul\Images\savedImage.png"));            

            SetUpLogger();

            VisionProcessor = new VisionStreamProcessor(Range, NumColumns, X, logMode, isMock, ShouldInit, logger);

            logger = new Logger(ImagefileName);

            TextProcessor = new TextStreamProcessor(10, 5, logMode, logger);

            if(ShouldInit) 
            Init();

            HCAccessor = new HippocampalComplex("Apple", isMock, nMode);                        
        }

        private void SetUpLogger()
        {            
            logger = new Logger(Path.GetFullPath(Path.Combine(baseDir, @"..\..\..\..\..\Hentul\Logs\Hentul-Orchestrator.log")));
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

        #region Public API

        /// Grabs Cursors Current Position and records pixels.        
        public void RecordPixels(bool isMock = false)
        {
            CycleNum++;

            Console.WriteLine("Grabbing cursor Position");

            point = this.GetCurrentPointerPosition();

            Console.WriteLine("Grabbing Screen Pixels...");

            int Range2 = Range + Range;     // We take in 20 rows and 40 columns , Mapper has similar mappings as well.

            int x1 = point.X - Range < 0 ? 0 : point.X - Range;
            int y1 = point.Y - Range < 0 ? 0 : point.Y - Range;
            int x2 = Math.Abs(point.X + Range2);
            int y2 = Math.Abs(point.Y + Range);

            //this.GetColorByRange(x1, y1, x2, y2);

            Rectangle rect = new Rectangle(x1, y1, x2, y2);

            bmp = new Bitmap(Range2 + Range2, Range2, PixelFormat.Format32bppArgb);

            Graphics g = Graphics.FromImage(bmp);

            g.CopyFromScreen(x1, y1, 0, 0, bmp.Size, CopyPixelOperation.SourceCopy);

            if (isMock == false)
                bmp.Save(ImagefileName, ImageFormat.Jpeg);
        }

        /// Fires L4 and L3B with the same input and output of L4 -> L3A
        public void ProcessVisual(Bitmap greyScalebmp, ulong cycle)
        {
            //ParseNFireBitmap(greyScalebmp);
            VisionProcessor.Process(greyScalebmp, cycle);
        }

        //Fire L4 & L3B for given character , Fires L3A from L4 input, Stores L3A -> HC.
        public void AddNewCharacterSensationToHC(char ch, ulong cycleNum)
        {
            if (!NMode.Equals(NetworkMode.TRAINING))
            {
                throw new InvalidOperationException("AddNewCharacterSensationToHC_T :: Network Should be in Training Mode before Predicting!");
            }

            TextProcessor.ProcessCharacter(ch, cycleNum);

            var som_SDR = TextProcessor.GetL3BSensation(cycleNum);

            if (som_SDR == null || som_SDR.ActiveBits.Count == 0)
            {
                throw new InvalidOperationException("som_SDR should not be Empty!");
            }

            //Wrong : location should be the location of the mouse pointer relative to the image and not just BBMID.
            Sensation firingSensei = TextProcessor.ConvertSDR_To_Sensation(som_SDR);

            if (HCAccessor.AddNewSensationToObject(firingSensei) == false)
            {
                throw new InvalidOperationException("Could Not Add Object to HC! Sensation already exist in the current Object");
            }
        }

        public string GetPredictionTextual()
        {
            if (NMode != NetworkMode.PREDICTION)
                throw new InvalidOperationException("Network Must be in PRediction Mode!");

            string prediction = TextProcessor.GetPrediction();

            return prediction;
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
                logger.WriteLogsToFile("Empty Polarizing Signal for FOM During Waaandering!!!");
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

        public Bitmap ConverToEdgedBitmap()
        {
            Bitmap newBitmap = new Bitmap(bmp.Width, bmp.Height);

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
                    g.DrawImage(bmp, new Rectangle(0, 0, bmp.Width, bmp.Height),
                                0, 0, bmp.Width, bmp.Height, GraphicsUnit.Pixel, attributes);
                }
            }

            return newBitmap;
        }

        public RecognisedVisualEntity GetPredictedObject() => HCAccessor.GetCurrentPredictedObject();        

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

        public void Restore()
        {
            VisionProcessor.Restore();
            _orchestrator.HCAccessor = HippocampalComplex.Restore();
        }

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

        public List<uint> StartBurstAvoidanceWandering(int totalWanders = 5)
        {
            //var temporalSignalForPosition = new SDR_SOM(NumColumns, Z, GetLocationSDR(nextDesiredPosition), iType.TEMPORAL);
            //somBBM_L3A.Fire(temporalSignalForPosition);  

            // Object recognised! 
            int counter = totalWanders > 0 ? HCAccessor.GetObjectTotalSensationCount() : totalWanders;
            int breakpoint = 1;

            List<uint> burstCache = new List<uint>();

            while (counter != 0)
            {
                if (counter == 2)
                    breakpoint = 3;

                Position2D nextDesiredPosition = HCAccessor.GetNextLocationForWandering();

                var apicalSignalSOM = new SDR_SOM(X, NumColumns, Conver2DtoSOMList(HCAccessor.GetNextSensationForWanderingPosition()), iType.APICAL);

                MoveCursorToSpecificPosition(nextDesiredPosition.X, nextDesiredPosition.Y);

                RecordPixels();

                var edgedbmp = ConverToEdgedBitmap();

                var apicalSignal = apicalSignalSOM.ActiveBits;

                var apicalSignalFOM = new SDR_SOM(X, NumColumns, apicalSignal, iType.APICAL);               //Fire FOMS with APICAL input

                BiasFOM(apicalSignalFOM);

                ParseNFireBitmap(edgedbmp);

                VisionProcessor.Clean();

                uint postBiasBurstCount = GetTotalBurstCountInFOMLayerInLastCycle(CycleNum);

                if (postBiasBurstCount > 0)
                {
                    Dictionary<int, List<Position_SOM>> bbmToFiringPositions = new Dictionary<int, List<Position_SOM>>();

                    foreach (var fom in VisionProcessor.GetFOMBBMVFromLearningUnit(LearningUnitType.V1))
                    {
                        Tuple<int, List<Position_SOM>> tuple = fom.GetBurstingColumnsInLastCycle(CycleNum);

                        if (tuple != null)
                            bbmToFiringPositions.Add(tuple.Item1, tuple.Item2);
                    }

                    breakpoint = 2;
                }

                burstCache.Add(postBiasBurstCount);

                CycleNum++;
                counter--;
            }

            return burstCache;
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
