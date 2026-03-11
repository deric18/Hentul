namespace Hentul
{
    using Common;
    using System.Runtime.InteropServices;
    using System;
    using System.Linq;
    using Hentul.Hippocampal_Entorinal_complex;
    using System.Drawing.Imaging;
    using System.Drawing;    

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
        [DllImport("user32.dll")]
        public static extern int GetSystemMetrics(int nIndex);
        // SM_CXSCREEN = 0 (primary screen width), SM_CYSCREEN = 1 (primary screen height)
        private const int SM_CXSCREEN = 0;
        private const int SM_CYSCREEN = 1;
        #endregion

        #region CONSTRUCTOR

        #region Used Variables                        

        public bool IsMock { get; private set; }

        public HippocampalComplex HCAccessor { get; private set; }                

        public int ImageIndex { get; private set; }

        public POINT point;        

        public Bitmap bmp, bmp_g;        

        public string logFileName;

        public LogMode logMode;

        private const string objectString = "Objejct";

        private int objectIndex { get; set; }

        private string fileName;

        private static Orchestrator _orchestrator;

        public NetworkMode NMode { get; set; }

        public VisionStreamProcessor VisionProcessor { get; set; }

        public TextStreamProcessor TextProcessor { get; private set; }

        public ulong CycleNum { get; private set; }                        

        public VisionScope visionScope { get; set; }

        #endregion

        private static readonly string baseDir = AppContext.BaseDirectory;
        
        private Orchestrator(bool isMock = false, bool ShouldInit = true, NetworkMode nMode = NetworkMode.TRAINING, int mockImageIndex = 7)
        {                        
            NMode = nMode;
            logMode = LogMode.BurstOnly;
            
            VisionProcessor = new VisionStreamProcessor(logMode, isMock, ShouldInit);
            TextProcessor = new TextStreamProcessor(10, 5, logMode);

            HCAccessor = new HippocampalComplex("Object0", isMock, nMode);            

            Init();                        

            objectIndex = 1;
            fileName = Path.GetFullPath(Path.Combine(baseDir, @"..\..\..\..\..\Hentul\Hentul\Images\savedImage.png"));
            logFileName = Path.GetFullPath(Path.Combine(baseDir, @"..\..\..\..\..\Hentul\Logs\Hentul-Orchestrator.log"));
            visionScope = VisionScope.NARROW;
        }


        public static Orchestrator GetInstance(bool isMock = false, bool shouldInit = true, NetworkMode nMode = NetworkMode.TRAINING)
        {
            if (_orchestrator == null)
            {
                _orchestrator = new Orchestrator(isMock, shouldInit, nMode);
            }
            return _orchestrator;
        }


        private void Init()
        {
            Console.WriteLine("Finished Init for this Instance \n");            
            Console.WriteLine("Initing SOM Instance now ... \n");
            Console.WriteLine("Finished Init for SOM Instance , Total Time ELapsed : \n");

            InitHC();


            Console.WriteLine("Finished Initting of all Instances, System Ready!" + "\n");

        }

        #endregion

        #region Public API


        public void InitHC()
        {
            // Read primary screen dimensions via Win32 and initialise the Graph's environment bounds.
            int screenWidth  = GetSystemMetrics(SM_CXSCREEN);
            int screenHeight = GetSystemMetrics(SM_CYSCREEN);

            HCAccessor.InitialiseEnvironment(screenWidth, screenHeight);

            Console.WriteLine($"[HC] Environment initialised: {screenWidth}x{screenHeight}");
        }


        public bool SetupLabel(Bitmap bmp, string objectLabel = null)
        {   
            var currentMousePosition = GetCurrentPointerPosition1();

            if (NMode == NetworkMode.EXPLORE && string.IsNullOrEmpty(objectLabel))
            {
                objectLabel = GetNextObjectLabel();
            }

            return VisionProcessor.SetupLabel(bmp, objectLabel, visionScope, currentMousePosition);            
        }

       

        public void TrainImage(int offsetX, int offsetY)
        {

            var locationSDR = VisionProcessor.pEncoder.GetCahced();

            // Count Total Number Of Objects in the processed chunk

            // Transform each object into Entities

            //Store them in HC-EC Complex.

            HCAccessor.StoreNewObjectLocationInGraph(locationSDR, offsetX, offsetY);
            
            VisionProcessor.SendAPicalFeedback();            
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


        public void DoneWithTraining(string label = "")
        {
            HCAccessor.DoneWithTraining(label);
        }


        //public void ChangeNetworkToPredictionMode()
        //{
        //    NMode = NetworkMode.PREDICTION;
        //    VisionProcessor.SetNetworkModeToPrediction();
        //    //HCAccessor.DoneWithTraining();
        //    //HCAccessor.SetNetworkModeToPrediction()
        //}

        #endregion

        #region PRIVATE HELPER METHODS

        private string GetNextObjectLabel()
        {
            if (logMode >= LogMode.Trace)
            {
                WriteLogsToFile("Object Label i being changed to next label");
            }

            objectIndex++;
            return objectString + objectIndex.ToString();
        }

        private void GetPixelsAsPerScope()
        {

            POINT currentMousePosition = GetCurrentPointerPosition();

            switch (GetScope())
            {
                case VisionScope.NARROW:       // High Detail In-Object Scope
                    {
                        break;
                    }
                case VisionScope.OBJECT:       // Fully View of Object
                    {
                        break;
                    }
                case VisionScope.BROAD:        // complete visual view at capacity.
                    {
                        break;
                    }
                case VisionScope.INVALID:
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
            VisionScope scopeToReturn = VisionScope.NARROW;

            bool makesSense = false;

            while (makesSense != false)
            {

                // Get Screen Pixel Values 

                // Analyse

                // Increase or decrease Scope && try again

                makesSense = DoesItMakeSense();
            }

            return scopeToReturn;
        }



        private bool DoesItMakeSense()
        {
            // Check if cursor is currently on top of an object or just screen saver ?
            //If object , check what is the size of the object and return the dimension needed to cover the whole object.
            AdjustScopetoNearestObject();


            //if Scope is not on Object then return broad scope to grab the whole screen view with lower pixel rate.
            GrabWholeScreen();

            return true;

        }

        private Bitmap GrabWholeScreen()
        {

            // Bitmap bmpScreenCapture = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
            throw new NotImplementedException();
        }

        private void AdjustScopetoNearestObject()
        {
            POINT p = GetCurrentPointerPosition();
        }


        #endregion        

        #region Private Methods

        /// <summary>
        /// Takens in a bmp and preps and fires all FOM & SOM's.
        /// </summary>     
        private void ParseNFireBitmap(Bitmap greyScalebmp)
        {
                                                
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

        public static Position2D GetCurrentPointerPosition1()
        {
            Position2D position = null;
            POINT point;

            point = new POINT();
            point.X = 0;
            point.Y = 0;

            if (GetCursorPos(out point))
            {
                //Console.Clear();
                //Console.WriteLine(point.X.ToString() + " " + point.Y.ToString());
                position = new Position2D(point.X, point.Y);
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


        public void RecordPixels(VisionScope scope)
        {
            int w = 0;
            int h = 0;

            switch (scope)
            {
                case VisionScope.NARROW:
                    w = 600;
                    h = 600;
                    break;
                case VisionScope.BROAD:
                    w = 3600;
                    h = 1800;
                    break;
            };

            var cur = GetCurrentPointerPosition();

            // Center the capture rectangle on the cursor position
            int x = Math.Max(0, cur.X - w / 2);
            int y = Math.Max(0, cur.Y - h / 2);

            bmp = CaptureScreenRegion(new Rectangle(x, y, w, h));
        }

        public void RecordPixels(int width, int height)
        {
            var cur = GetCurrentPointerPosition();

            int x = Math.Max(0, cur.X - width / 2);
            int y = Math.Max(0, cur.Y - height / 2);

            bmp = CaptureScreenRegion(new Rectangle(x, y, width, height));
        }

        private Bitmap CaptureScreenRegion(Rectangle rect)
        {
            var bmp = new Bitmap(rect.Width, rect.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            using var g = Graphics.FromImage(bmp);
            g.CopyFromScreen(rect.Location, Point.Empty, rect.Size);
            return bmp;
        }

        public bool CheckifPixelisBlack(int x, int y)
        {
            if (x >= bmp.Width || y >= bmp.Height)
                return false;

            var color = bmp.GetPixel(x, y);

            return (color.R < 200 && color.G < 200 && color.B < 200);
        }

        public void ChangeNetworkModeToPrediction(bool isMock = false)
        {
            NMode = NetworkMode.PREDICTION;
            VisionProcessor.SetNetworkModeToPrediction(isMock);
            HCAccessor.SetNetworkModeToPrediction();
        }

        public void ChangeNetworkModeToTraining()
        {
            NMode = NetworkMode.TRAINING;
            HCAccessor.SetNetworkModeToTraining();
        }

        public void SetVisionScope(VisionScope scope, bool isMock = false)
        {
            if (!isMock)
                throw new InvalidOperationException("Cannot Set Vision Scope from Outside the class!");

            visionScope = scope;
        }
       
        public void BackUp()
        {
            
            HCAccessor.Backup();

        }


        public static void WriteLogsToFile(string logMsg)
        {
            File.WriteAllText(_orchestrator.logFileName, logMsg);
        }

        public void Restore()
        {            
            _orchestrator.HCAccessor = HippocampalComplex.Restore();
        }

        #endregion      

        #region LEGACY CODE

        //public Position2D Verify_Predict_HC(bool isMock = false, uint iterationsToConfirmation = 10, bool legacyPipeline = false)
        //{
        //    Position2D motorOutput = null;
        //    List<Position2D> positionToConfirm = new List<Position2D>();
        
        //    if (!NMode.Equals(NetworkMode.PREDICTION))
        //    {
        //        throw new InvalidOperationException("Invalid State Managemnt!");
        //    }

        //    // If any output from HC execute the location output if NOT then take the standard default output.                
        //    var som_SDR = VisionProcessor.GetSL3BLatestFiringCells(LearningUnitType.V1, CycleNum);
        //    var predictedSDR = VisionProcessor.GetSL3BLatestFiringCells(LearningUnitType.V1, CycleNum + 1);


        //    if (som_SDR != null)
        //    {
        //        var firingSensei = VisionProcessor.pEncoder.GetSenseiFromSDR_V(som_SDR, point);
        //        var predictedSensei = VisionProcessor.pEncoder.GetSenseiFromSDR_V(predictedSDR, point);

        //        List<string> predictedLabels = VisionProcessor.GetSupportedLabels(LearningUnitType.V1);

        //        if (legacyPipeline)
        //        {
        //            motorOutput = HCAccessor.VerifyObject(firingSensei, null, isMock, iterationsToConfirmation);
        //        }
        //        else    // brand New Pipeline : Classification done Primarily through V1.
        //        {
        //            if (VisionProcessor.v1.somBBM_L3B_V.NetWorkMode == NetworkMode.DONE)
        //            {
        //                VisionProcessor.v1.somBBM_L3B_V.GetCurrentPredictions();
        //            }
        //        }
        //    }

        //    return motorOutput;
        //}

        ////Stores the new object on to HC
        //public void AddNewVisualSensationToHc()
        //{
        //    if (!NMode.Equals(NetworkMode.TRAINING))
        //    {
        //        throw new InvalidOperationException("INVALID State Management!");
        //    }

        //    var som_SDR = VisionProcessor.GetSL3BLatestFiringCells(LearningUnitType.V1, CycleNum);

        //    if (som_SDR != null)
        //    {
        //        //Wrong : location should be the location of the mouse pointer relative to the image and not just BBMID.
        //        var firingSensei = VisionProcessor.pEncoder.GetSenseiFromSDR_V(som_SDR, point);

        //        if (HCAccessor.AddNewSensationLocationToObject(firingSensei) == false)
        //        {
        //            throw new InvalidOperationException("Could Not Add Object to HC ! Either it was NOT in TRAINING MODE or sensation already exist in the current Object");
        //        }
        //    }
        //    else
        //    {
        //        throw new InvalidOperationException(" som_SDR should not be null!");
        //    }
        //}

        //public List<uint> StartBurstAvoidanceWandering(int totalWanders = 5)
        //{
        //    //var temporalSignalForPosition = new SDR_SOM(NumColumns, Z, GetLocationSDR(nextDesiredPosition), iType.TEMPORAL);
        //    //somBBM_L3A.Fire(temporalSignalForPosition);  

        //    // Object recognised! 
        //    int counter = totalWanders > 0 ? HCAccessor.GetObjectTotalSensationCount() : totalWanders;
        //    int breakpoint = 1;

        //    List<uint> burstCache = new List<uint>();

        //    while (counter != 0)
        //    {
        //        if (counter == 2)
        //            breakpoint = 3;

        //        Position2D nextDesiredPosition = HCAccessor.GetNextLocationForWandering();

        //        var apicalSignalSOM = new SDR_SOM(X, NumColumns, Conver2DtoSOMList(HCAccessor.GetNextSensationForWanderingPosition()), iType.APICAL);

        //        MoveCursorToSpecificPosition(nextDesiredPosition.X, nextDesiredPosition.Y);

        //        RecordPixels();

        //        var edgedbmp = ConverToEdgedBitmap();

        //        var apicalSignal = apicalSignalSOM.ActiveBits;

        //        var apicalSignalFOM = new SDR_SOM(X, NumColumns, apicalSignal, iType.APICAL);               //Fire FOMS with APICAL input

        //        BiasFOM(apicalSignalFOM);

        //        ParseNFireBitmap(edgedbmp);

        //        VisionProcessor.Clean();

        //        uint postBiasBurstCount = GetTotalBurstCountInFOMLayerInLastCycle(CycleNum);

        //        if (postBiasBurstCount > 0)
        //        {
        //            Dictionary<int, List<Position_SOM>> bbmToFiringPositions = new Dictionary<int, List<Position_SOM>>();

        //            foreach (var fom in VisionProcessor.GetFOMBBMVFromLearningUnit(LearningUnitType.V1))
        //            {
        //                Tuple<int, List<Position_SOM>> tuple = fom.GetBurstingColumnsInLastCycle(CycleNum);

        //                if (tuple != null)
        //                    bbmToFiringPositions.Add(tuple.Item1, tuple.Item2);
        //            }

        //            breakpoint = 2;
        //        }

        //        burstCache.Add(postBiasBurstCount);

        //        CycleNum++;
        //        counter--;
        //    }

        //    return burstCache;
        //}

        //public void LearnNewObject(string v)
        //{
        //    VisionProcessor.LearnNewObject(v);
        //}

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
        NARROW,        
        OBJECT,
        BROAD,
        INVALID
    }

    /// <summary>
    /// A screen-space region discovered during a BROAD scan.
    /// Produced by Phase 1 (contour detection) and consumed by Phase 2 (NARROW deep learning).
    /// </summary>
    public record DetectedRegion(int ScreenX, int ScreenY, int Width, int Height)
    {
        public int CenterX => ScreenX + Width / 2;
        public int CenterY => ScreenY + Height / 2;
    }

    #endregion
}
