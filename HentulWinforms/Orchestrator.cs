namespace HentulWinforms
{
    using FirstOrderMemory.Models;
    using FirstOrderMemory.BehaviourManagers;
    using System.Configuration;
    using System.Diagnostics;
    using Common;
    using FirstOrderMemory.Models.Encoders;
    using System.Runtime.InteropServices;
    using System;
    using HentulWinforms.Hippocampal_Entorinal_complex;

    internal class Orchestrator
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

        public int NumPixelsToProcessPerBlock { get; private set; }

        public int numPixelsProcessedPerBBM;

        public int NumBBMNeeded { get; private set; }

        private bool LogMode { get; set; }

        Dictionary<string, BaseObject> Objects { get; set; }

        public bool IsMock { get; private set; }

        int NumColumns, Z;

        public Dictionary<int, List<Position_SOM>> Buckets;

        public BlockBehaviourManager[] fomBBM { get; private set; }

        public BlockBehaviourManager somBBM_L3B { get; private set; }

        public BlockBehaviourManager somBBM_L3A { get; private set; }

        public int[] MockBlockNumFires { get; private set; }

        private bool devbox = false;

        public int BlockOffset;

        public int UnitOffset;

        public int ImageIndex { get; private set; }

        public ulong CycleNum;

        public POINT point;

        public List<string> ImageList { get; private set; }

        public int blackPixelCount = 0;

        public Bitmap bmp;

        public int range;

        private readonly int FOMLENGTH = Convert.ToInt32(ConfigurationManager.AppSettings["FOMLENGTH"]);
        private readonly int FOMWIDTH = Convert.ToInt32(ConfigurationManager.AppSettings["FOMWIDTH"]);
        private readonly int SOM_NUM_COLUMNS = Convert.ToInt32(ConfigurationManager.AppSettings["SOMNUMCOLUMNS"]);
        private readonly int SOM_COLUMN_SIZE = Convert.ToInt32(ConfigurationManager.AppSettings["SOMCOLUMNSIZE"]);


        #endregion

        public Orchestrator(int range, bool isMock = false, bool ShouldInit = true, int mockImageIndex = 7)
        {
            //Todo : Project shape data of the input image to one region and project colour data of the image to another region.                        

            this.range = range;

            NumPixelsToProcessPerBlock = range;

            bmp = new Bitmap(range + range, range + range);

            numPixelsProcessedPerBBM = 10;

            LogMode = false;

            double numerator = (2 * NumPixelsToProcessPerBlock) * (2 * NumPixelsToProcessPerBlock);

            double denominator = numPixelsProcessedPerBBM;

            double value = (numerator / denominator);

            bool b = value % 1 != 0;

            if (value % 1 != 0)
            {
                throw new InvalidDataException("Number Of Pixels should always be a factor of BucketColLength : NumPixels : " + NumPixelsToProcessPerBlock.ToString());
            }

            BlockOffset = range * 2;

            UnitOffset = 10;

            NumBBMNeeded = (int)value;

            fomBBM = new BlockBehaviourManager[NumBBMNeeded];

            int x1 = numPixelsProcessedPerBBM * NumBBMNeeded;

            somBBM_L3B = new BlockBehaviourManager(x1, 10, 4);

            NumColumns = 10;

            IsMock = isMock;

            Z = 4;

            CycleNum = 0;

            for (int i = 0; i < NumBBMNeeded; i++)
            {
                fomBBM[i] = new BlockBehaviourManager(NumColumns, NumColumns, Z, BlockBehaviourManager.LayerType.Layer_4, BlockBehaviourManager.LogMode.BurstOnly);
            }

            if (isMock)
                ImageIndex = mockImageIndex;
            else
                ImageIndex = 0;


            somBBM_L3A = new BlockBehaviourManager(1250, 10, 4, BlockBehaviourManager.LayerType.Layer_3A, BlockBehaviourManager.LogMode.BurstOnly);

            somBBM_L3B = new BlockBehaviourManager(1250, 10, 4, BlockBehaviourManager.LayerType.Layer_3B, BlockBehaviourManager.LogMode.BurstOnly);

            MockBlockNumFires = new int[NumBBMNeeded];

            LoadFOMnSOM();

        }


        public void LoadFOMnSOM()
        {

            Console.WriteLine("Starting Initialization  of FOM objects : \n");


            for (int i = 0; i < NumBBMNeeded; i++)
            {
                fomBBM[i].Init(0, 0, 1, 1, 10);
            }


            Console.WriteLine("Finished Init for this Instance \n");
            Console.WriteLine("Range : " + NumPixelsToProcessPerBlock.ToString() + "\n");
            Console.WriteLine("Total Number of Pixels :" + (NumPixelsToProcessPerBlock * NumPixelsToProcessPerBlock * 4).ToString() + "\n");
            Console.WriteLine("Total First Order BBMs Created : " + NumBBMNeeded.ToString() + "\n");


            Console.WriteLine("Initing SOM Instance now ... \n");

            somBBM_L3A.Init(0, 0, 0, 0, 1);

            somBBM_L3B.Init(0, 0, 0, 0, 1);

            Console.WriteLine("Finished Init for SOM Instance , Total Time ELapsed : \n");

            Console.WriteLine("Finished Initting of all Instances, System Ready!" + "\n");
        }


        #endregion

        public string StartCycle()
        {

            //Check how big the entire creen / image is and kee papring through all the objects.

            var str = DetectObject();
            StoreObject();

            return str;



        }

        /// <summary>
        /// PROBLEMS:
        /// 
        /// ALGORITHM:
        /// 
        /// </summary>
        public string DetectObject()
        {
            var obj = string.Empty;

            if (Objects == null || Objects.Count == 0)
            {
                obj = LearnFirstObject();
                
            }

            //Traverse through Object Maps and what sensory inputs are telling you.

            return obj;
        }



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

            while (stillRecognising)
            {
                // feed L4
                SDR_SOM fomSdr;
                for (int i = 0; i < fomBBM.Length; i++)
                {
                    fomSdr = GetFomSdrForIteration(i);
                    fomBBM[i].Fire(fomSdr);
                }

                //feed same pattern SOM BBM L3A
                SDR_SOM Sdr_Som3A = new SDR_SOM(10, 10, new List<Position_SOM>() { }, iType.SPATIAL);
                somBBM_L3A.Fire(Sdr_Som3A);

                // init L3B to Apple
                SDR_SOM Sdr_SomL3B = GetSdrSomFromFOMs();
                somBBM_L3B.Fire(Sdr_SomL3B);

                // Push new object representation to 3A
                // Push Wiring Burst Avoiding LTP to 4 from 3A.
            }


            return obj;
        }


        private SDR_SOM GetFomSdrForIteration(int i)
        {
            throw new NotImplementedException();
        }

        private SDR_SOM GetSdrSomFromFOMs()
        {
            throw new NotImplementedException();
        }










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

        private void GetNarrowcopePixelValues()
        {

        }

        private void GetObjectScopePixelValues()
        {

        }

        private void GetBroaderScopeValues()
        {

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


        public void GrabNProcess(ref bool[,] booleans)          //We process one image at once.
        {
            //Todo : Pixel combination should not be serial , it should be randomly distributed through out the unit

            Stopwatch stopWatch = new Stopwatch();

            stopWatch.Start();

            int TotalReps = 2;

            int TotalNumberOfPixelsToProcess_X = GetRoundedTotalNumberOfPixelsToProcess(bmp.Width);
            int TotalNumberOfPixelsToProcess_Y = GetRoundedTotalNumberOfPixelsToProcess(bmp.Height);

            int TotalPixelsCoveredPerIteration = BlockOffset * BlockOffset; //2500            

            int num_blocks_per_bmp_x = (int)(TotalNumberOfPixelsToProcess_X / BlockOffset);
            int num_blocks_per_bmp_y = (int)(TotalNumberOfPixelsToProcess_Y / BlockOffset);

            int num_unit_per_block_x = 5;
            int num_unit_per_block_y = 5;

            int num_pixels_per_Unit_x = 10;
            int num_pixels_per_Unit_y = 10;

            for (int reps = 0; reps < TotalReps; reps++)
            {
                for (int blockid_y = 0; blockid_y < num_blocks_per_bmp_y; blockid_y++)
                {
                    for (int blockid_x = 0; blockid_x < num_blocks_per_bmp_x; blockid_x++)
                    {
                        int bbmId = 0;

                        for (int unitId_y = 0; unitId_y < num_unit_per_block_y; unitId_y++)
                        {
                            for (int unitId_x = 0; unitId_x < num_unit_per_block_x; unitId_x++)
                            {
                                BoolEncoder boolEncoder = new BoolEncoder(100, 20);

                                for (int j = 0; j < num_pixels_per_Unit_x; j++)
                                {
                                    for (int i = 0; i < num_pixels_per_Unit_y; i++)
                                    {
                                        int pixel_x = blockid_x * BlockOffset + unitId_x * UnitOffset + i;
                                        int pixel_y = blockid_y * BlockOffset + unitId_y * UnitOffset + j;

                                        //if the pixel is Black then tag the pixel location

                                        if (blockid_x == 6 && blockid_y == 0 && unitId_x == 4 && unitId_y == 2 && j == 2)
                                        {
                                            int bp = 1;
                                        }

                                        if (CheckifPixelisBlack(pixel_x, pixel_y))
                                        {

                                            var dataToEncode = (j % 2).ToString() + "-" + i.ToString();
                                            boolEncoder.SetEncoderValues(dataToEncode);

                                        }
                                    }

                                    if (j % 2 == 1)     //Bcoz one BBM covers 2 lines of pixel per unit
                                    {
                                        if (fomBBM[bbmId].TemporalLineArray[0, 0] == null)
                                        {
                                            fomBBM[bbmId].Init(blockid_x, blockid_y, unitId_x, unitId_y, bbmId);
                                        }

                                        if (boolEncoder.HasValues())
                                        {
                                            CycleNum++;

                                            var imageSDR = boolEncoder.Encode(iType.SPATIAL);

                                            fomBBM[bbmId++].Fire(imageSDR);

                                            SDR_SOM fomSDR = fomBBM[bbmId].GetPredictedSDR();

                                            if (fomSDR != null && fomSDR.ActiveBits.Count != 0)
                                            {
                                                fomSDR = AddSOMOverheadtoFOMSDR(fomSDR, blockid_x, blockid_y);

                                                somBBM_L3B.Fire(fomSDR);
                                            }

                                        }

                                        boolEncoder.ClearEncoderValues();
                                    }
                                }
                            }
                        }
                    }
                }
            }


            PrintMoreBlockVitals();

            BackUp();

            Console.WriteLine("Finished Processing Pixel Values : Total Time Elapsed in seconds : " + (stopWatch.ElapsedMilliseconds / 1000).ToString());

            Console.WriteLine("Black Pixel Count :: " + blackPixelCount.ToString());

            Console.WriteLine("Done Processing Image");

            Console.Read();
        }

        public void Grab()
        {

            Console.WriteLine("Grabbing cursor Position");

            //Console.CursorVisible = false;

            POINT Point = this.GetCurrentPointerPosition();


            //Console.CursorVisible = true;

            Console.WriteLine("Grabbing Screen Pixels...");

            int x1 = Point.X - range < 0 ? 0 : Point.X - range;
            int y1 = Point.Y - range < 0 ? 0 : Point.Y - range;
            int x2 = Math.Abs(Point.X + range);
            int y2 = Math.Abs(Point.Y + range);

            this.GetColorByRange(x1, y1, x2, y2);

        }


        // Already grey scalled.
        private void GetColorByRange(int x1, int y1, int x2, int y2)
        {
            IntPtr desk = GetDesktopWindow();

            IntPtr dc = GetWindowDC(desk);


            for (int i = x1, k = 0; i < x2 && k < range + range; i++, k++)
            {
                for (int j = y1, l = 0; j < y2 && l < range + range; j++, l++)
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


        // Already grey scalled.
        private static Color GetColorAt(int x, int y)
        {
            IntPtr desk = GetDesktopWindow();

            IntPtr dc = GetWindowDC(desk);

            int a = (int)GetPixel(dc, x, y);

            ReleaseDC(desk, dc);

            return System.Drawing.Color.FromArgb(255,
                                                 (a >> 0) & 0xff,
                                                 (a >> 8) & 0xff,
                                                 (a >> 16) & 0xff);
        }

        public void MoveCursorToSpecificPosition(int x, int y)
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

        public int GetRoundedTotalNumberOfPixelsToProcess(int numberOfPixels_Index)
        {
            if (numberOfPixels_Index % BlockOffset == 0)
            {
                return numberOfPixels_Index;
            }

            int nextMinNumberOfPixels = numberOfPixels_Index;

            int halfOfnextMinNumberOfPixels = numberOfPixels_Index / 2;

            while (nextMinNumberOfPixels % 50 != 0)
            {
                nextMinNumberOfPixels--;

                if (nextMinNumberOfPixels < halfOfnextMinNumberOfPixels)
                {
                    Console.WriteLine(" GetRoundedTotalNumberOfPixelsToProcess() :: Unable to find the proper lower Bound");
                }

            }

            if (nextMinNumberOfPixels % 50 != 0)
                throw new InvalidDataException("Grab :: blockLength should always be factor of NumPixelToProcess");

            return nextMinNumberOfPixels;
        }

        private void PrintMoreBlockVitals()
        {
            Console.WriteLine("Enter '1' to see a list of all the Block Usage List :");

            int w = Console.Read();

            if (w == 49)
            {
                ulong totalIncludedCycle = 0;

                for (int i = 0; i < fomBBM.Count(); i++)
                {
                    if (fomBBM[i].BlockId != null)
                        Console.WriteLine(i.ToString() + " :: Block ID : " + fomBBM[i].PrintBlockDetailsSingleLine() + " | " + "Inclusded Cycle: " + fomBBM[i].CycleNum.ToString());

                    totalIncludedCycle += fomBBM[i].CycleNum;

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

        public void BackUp()
        {
            for (int i = 0; i < fomBBM.Length; i++)
            {
                fomBBM[i].BackUp(i.ToString());
            }

            somBBM_L3B.BackUp("SOM-1");
        }

        #endregion
    }

    public enum VisionScope
    {
        BroadScope,
        ObjectScope,
        NarrowScope,
        UNKNOWN
    }
}
