/// Author : Deric Pinto

namespace Hentul
{
    using Common;
    using Hentul.Enums;
    using System.Runtime.InteropServices;
    using FirstOrderMemory.BehaviourManagers;
    using System.Diagnostics;
    using System.Drawing;
    using Image = SixLabors.ImageSharp.Image;
    using FirstOrderMemory.Models.Encoders;
    using Hentul.Hippocampal_Entorinal_complex;

    internal class Orchestrator2
    {
        #region VARIABLES & CONTRUCTORS


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

        public int NumPixelsToProcessPerBlock { get; private set; }

        public int numPixelsProcessedPerBBM;

        public POINT Point { get; set; }

        public Position[] BoundaryCells { get; set; }

        public Position PlaceCell { get; private set; }

        Dictionary<string, RecognisedEntity> Objects { get; set; }

        public HCCState State { get; set; }

        private ulong CycleNum { get; set; }

        public int BlockOffset;

        public List<string> ImageList { get; private set; }

        public int ImageIndex { get; private set; }

        private Bitmap bmp;

        public BlockBehaviourManager[] fomBBM { get; private set; }

        public BlockBehaviourManager somL3a { get; private set; }

        public BlockBehaviourManager somL3b { get; private set; }

        public Orchestrator2(Position position)
        {
            Objects = new Dictionary<string, RecognisedEntity>();
            PlaceCell = position;
            BoundaryCells = new Position[4];

            CycleNum = 0;

            BlockOffset = 50;
            int NumBBMNeededForSOM = 125;
            int x1 = 10 * NumBBMNeededForSOM;
            int numBBMNeededForFOM = 10;

            for (int i = 0; i < numBBMNeededForFOM; i++)
            {
                fomBBM[i] = new BlockBehaviourManager(10, 10, 10, LayerType.Layer_4, LogMode.BurstOnly);
            }

            somL3a = new BlockBehaviourManager(x1, 10, 4);
            somL3b = new BlockBehaviourManager(x1, 10, 4);

            ImageList = AddAllTheFruits();

            Init();

            Console.WriteLine("Loading Image : " + ImageList[ImageIndex].ToString().Split(new char[1] { '\\' })[7]);

            bmp = new Bitmap(ImageList[ImageIndex]);

            if (bmp == null)
            {
                throw new InvalidCastException("Couldn't find image");
            }

        }

        public void Init()
        {
            Stopwatch stopWatch = new Stopwatch();

            Console.WriteLine("Range : " + NumPixelsToProcessPerBlock.ToString() + "\n");
            Console.WriteLine("Total Number of Pixels :" + (NumPixelsToProcessPerBlock * NumPixelsToProcessPerBlock * 4).ToString() + "\n");

            stopWatch.Start();

            Console.WriteLine("Initing SOM Instance now ... \n");

            somL3a.Init(1);

            //somL3b.Init(0, 0, 0, 0, 1);

            stopWatch.Stop();

            Console.WriteLine("Finished Init for SOM Instance , Total Time ELapsed : " + stopWatch.ElapsedMilliseconds.ToString() + "\n");

            Console.WriteLine("Finished Initting of all Instances, System Ready!" + "\n");
        }

        private List<string> AddAllTheFruits()
        {
            var dict = new List<string>();

            dict.Add(@"C:\Users\depint\source\repos\Hentul\Images\Apple.png");
            dict.Add(@"C:\Users\depint\source\repos\Hentul\Images\Ananas.png");
            dict.Add(@"C:\Users\depint\source\repos\Hentul\Images\orange.png");
            dict.Add(@"C:\Users\depint\source\repos\Hentul\Images\bannana.jpg");
            dict.Add(@"C:\Users\depint\source\repos\Hentul\Images\grapes.png");
            dict.Add(@"C:\Users\depint\source\repos\Hentul\Images\jackfruit.png");
            dict.Add(@"C:\Users\depint\source\repos\Hentul\Images\watermelon.png");
            dict.Add(@"C:\Users\depint\source\repos\Hentul\Images\white.png");

            return dict;
        }

        #endregion

        public struct POINT
        {
            public int X;
            public int Y;
        }



        /// <summary>
        /// Orchestration Engine
        /// </summary>
        public void StartCycle()
        {
            while (true)
            {
                //Check how big the entire creen / image is and kee papring through all the objects.

                DetectObject();
                StoreObject();
                MoveToNextObject();

            }
        }

        /// <summary>
        /// PROBLEMS:
        /// 
        /// ALGORITHM:
        /// 
        /// </summary>
        public void DetectObject()
        {
            if (Objects == null || Objects.Count == 0)
            {
                if (LearnFirstObject())
                {
                    // Push new object representation to 3A
                    // Push Wiring Burst Avoiding LTP to 4 from 3A.
                }
            }

            //Traverse through Object Maps and what sensory inputs are telling you.
        }

        private void MoveToNextObject()
        {
            throw new NotImplementedException();
        }

        private void StoreObject()
        {
            throw new NotImplementedException();
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
        private bool LearnFirstObject()
        {
            //No Object Frame , Have to create a sense @ Location object map from scratch for this particular object.

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

            return true;
        }

        private VisionScope GetScope()
        {
            VisionScope scopeToReturn = VisionScope.UNKNOWN;

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

        //private void CaptureScreen()
        //{
        //    System.Drawing.Rectangle bounds = Screen.PrimaryScreen.Bounds;

        //    Bitmap bitmap = new Bitmap(bounds.Width, bounds.Height);

        //    using (Graphics graphics = Graphics.FromImage(bitmap))
        //    {
        //        graphics.CopyFromScreen(Point.Empty, Point.Empty, bounds.Size);
        //    }

        //    return bitmap;
        //}

        private void AdjustScopetoNearestObject()
        {
            POINT p = GetCurrentPointerPosition();





        }

        private void ProcessBlock()
        {
            int TotalNumberOfPixelsToProcess_X = GetRoundedTotalNumberOfPixelsToProcess(bmp.Width);
            int TotalNumberOfPixelsToProcess_Y = GetRoundedTotalNumberOfPixelsToProcess(bmp.Height);

            int TotalPixelsCoveredPerIteration = BlockOffset * BlockOffset; //2500           

            int num_unit_per_block_x = 5;
            int num_unit_per_block_y = 5;

            int num_pixels_per_Unit_x = 10;
            int num_pixels_per_Unit_y = 10;

            int num_blocks_per_bmp_x = (int)(TotalNumberOfPixelsToProcess_X / BlockOffset);
            int num_blocks_per_bmp_y = (int)(TotalNumberOfPixelsToProcess_Y / BlockOffset);

            int UnitOffset = 10;

            bool IsMock = false;
            bool[,] booleans = null;
            int[] MockBlockNumFires;



            using (Image image = Image.Load(ImageList[ImageIndex]))
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
                                    //Prepare Data
                                    for (int i = 0; i < num_pixels_per_Unit_y; i++)
                                    {
                                        int pixel_x = blockid_x * BlockOffset + unitId_x * UnitOffset + i;
                                        int pixel_y = blockid_y * BlockOffset + unitId_y * UnitOffset + j;

                                        if (CheckifPixelisBlack(pixel_x, pixel_y))
                                        {

                                            var dataToEncode = (j % 2).ToString() + "-" + i.ToString();
                                            boolEncoder.SetEncoderValues(dataToEncode);

                                        }
                                    }


                                    //Process Pixel Data                                    
                                    if (j % 2 == 1)
                                    {

                                        //Need Algo for properly processing IMAGE PIXEL VALUES

                                        /*
                                        1. Get current Mouse Pointer Location 
                                        2. Depolarize the location coordinates onto the layer 4 temporally
                                        3. Fire sensation laterally to layer 4 and layer 3b
                                        4. Firings of layer 3b should be projected to layer 3a
                                        5. All of these will be assoicated to each other with the current image being recognised.
                                        6. HCE Complex will store this object in memory on these locations.
                                        7. Orchestrator will train the entire network to this object.
                                                                                                                              
                                        */

                                        if (fomBBM[bbmId].TemporalLineArray[0, 0] == null)
                                        {
                                            Console.WriteLine("Starting Initialization  of FOM objects : \n");

                                            fomBBM[bbmId].Init(bbmId);

                                            Console.WriteLine("Finished Init for this Instance" + " Block ID X : " + blockid_x.ToString() + "  Block ID Y :" + blockid_y.ToString() + " UNIT ID X : " + unitId_x.ToString() + " UNIT ID Y :" + unitId_y.ToString());
                                        }

                                        if (boolEncoder.HasValues())
                                        {
                                            CycleNum++;

                                            var imageSDR = boolEncoder.Encode(iType.SPATIAL);

                                            //FireAsPerSchema(imageSDR, bbmId, blockid_x, blockid_y);

                                        }

                                        boolEncoder.ClearEncoderValues();
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private void ComputeBoundaries()
        {

        }

        public void Update()
        {
            //Update current Position
            //Update Grid Cell Map
        }


        private static System.Drawing.Color GetColorAt(int x, int y)
        {
            IntPtr desk = GetDesktopWindow();
            IntPtr dc = GetWindowDC(desk);
            int a = (int)GetPixel(dc, x, y);
            ReleaseDC(desk, dc);
            return System.Drawing.Color.FromArgb(255, (a >> 0) & 0xff, (a >> 8) & 0xff, (a >> 16) & 0xff);
        }


        public void UpdateGridCellMap()
        {

        }

        private void AddKnownObject()
        {

        }

        public bool CheckifPixelisBlack(int x, int y)
        {
            if (x >= bmp.Width || y >= bmp.Height)
                return false;

            var color = bmp.GetPixel(x, y);

            return (color.R < 200 && color.G < 200 && color.B < 200);
        }

        public void UpdateCurrentPosition(Position position)
        {
            PlaceCell = position;
            State = HCCState.None;
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

        private POINT GetCurrentPointerPosition()
        {
            POINT point;

            point = new POINT();
            point.X = 0;
            point.Y = 0;

            if (GetCursorPos(out point))
            {
                Console.Clear();
                Console.WriteLine(point.X.ToString() + " " + point.Y.ToString());
                return point;
            }

            return point;
        }

        private int GetRoundedTotalNumberOfPixelsToProcess(int numberOfPixels_Index)
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
    }
}