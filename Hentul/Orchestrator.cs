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
    using Hentul.Hippocampal_Entorinal_complex;
    using System.Drawing.Imaging;    
    using Hentul;
    using static FirstOrderMemory.BehaviourManagers.BlockBehaviourManager;
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
        #endregion


        #region CONSTRUCTOR 

        #region Used Variables

        public int Range { get; private set; }

        public int numPixelsProcessedPerBBM;

        public int NumBBMNeeded { get; private set; }

        private bool LogMode { get; set; }

        public bool IsMock { get; private set; }

        int NumColumns, Z;

        public BlockBehaviourManager[] fomBBM { get; private set; }

        public BlockBehaviourManager somBBM_L3B { get; private set; }

        public BlockBehaviourManager somBBM_L3A { get; private set; }

        public HippocampalComplex HCAccessor { get; private set; }

        public int[] MockBlockNumFires { get; private set; }

        private bool devbox = false;

        public int BlockSize;

        public int ImageIndex { get; private set; }

        public ulong CycleNum;

        public POINT point;

        public List<string> ImageList { get; private set; }

        public Bitmap bmp;

        public string filename;

        public NetworkMode NMode { get; set; }

        List<Position_SOM> ONbits1;
        List<Position_SOM> ONbits2;
        List<Position_SOM> ONbits3;
        List<Position_SOM> ONbits4;

        private readonly int FOMLENGTH = Convert.ToInt32(ConfigurationManager.AppSettings["FOMLENGTH"]);
        private readonly int FOMWIDTH = Convert.ToInt32(ConfigurationManager.AppSettings["FOMWIDTH"]);
        private readonly int SOM_NUM_COLUMNS = Convert.ToInt32(ConfigurationManager.AppSettings["SOMNUMCOLUMNS"]);
        private readonly int SOM_COLUMN_SIZE = Convert.ToInt32(ConfigurationManager.AppSettings["SOMCOLUMNSIZE"]);


        #endregion

        public Orchestrator(int range, bool isMock = false, bool ShouldInit = true, int mockImageIndex = 7)
        {
            //Todo : Project shape data of the input image to one region and project colour data of the image to another region.                        
            if (range != 10)
            {
                throw new InvalidOperationException("Invalid Operation !");
            }

            Range = range;  //10

            bmp = new Bitmap(range + range, range + range);

            numPixelsProcessedPerBBM = 4;

            LogMode = false;

            BlockSize = (2 * range) * (2 * range); //400

            NumBBMNeeded = (BlockSize / numPixelsProcessedPerBBM);   //100

            bool b = NumBBMNeeded % 1 != 0;

            if (NumBBMNeeded != 100)
            {
                throw new InvalidDataException("Number Of FOMM BBMs needed should always be 100, it throws off SOM Schema of 1250" + range.ToString());
            }

            fomBBM = new BlockBehaviourManager[NumBBMNeeded];

            NumColumns = 10;

            IsMock = isMock;

            Z = 4;

            CycleNum = 0;

            NMode = NetworkMode.TRAINING;

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

            HCAccessor = new HippocampalComplex();

            MockBlockNumFires = new int[NumBBMNeeded];

            ONbits1 = new List<Position_SOM>()
            {
                new Position_SOM(0,1),
                new Position_SOM(2,2)
            };

            ONbits2 = new List<Position_SOM>()
            {
                new Position_SOM(6, 0),
                new Position_SOM(8, 4)
            };

            ONbits3 = new List<Position_SOM>()
            {
                new Position_SOM(5,6),
                new Position_SOM(8,9)
            };

            ONbits4 = new List<Position_SOM>()
            {
                new Position_SOM(7,1),
                new Position_SOM(9,2)
            };

            LoadFOMnSOM();

            filename = "C:\\Users\\depint\\source\\repos\\Som Schema Docs\\sample.jpeg";

        }

        public void LoadFOMnSOM()
        {

            Console.WriteLine("Starting Initialization  of FOM objects : \n");


            for (int i = 0; i < NumBBMNeeded; i++)
            {
                fomBBM[i].Init(i);
            }


            Console.WriteLine("Finished Init for this Instance \n");
            Console.WriteLine("Range : " + Range.ToString() + "\n");
            Console.WriteLine("Total Number of Pixels :" + (Range * Range * 4).ToString() + "\n");
            Console.WriteLine("Total First Order BBMs Created : " + NumBBMNeeded.ToString() + "\n");


            Console.WriteLine("Initing SOM Instance now ... \n");

            somBBM_L3A.Init(1);

            somBBM_L3B.Init(1);

            Console.WriteLine("Finished Init for SOM Instance , Total Time ELapsed : \n");

            Console.WriteLine("Finished Initting of all Instances, System Ready!" + "\n");
        }

        #endregion


        public void ProcesStep1()
        {

            SDR_SOM fomSdr = new SDR_SOM(10, 10, new List<Position_SOM>(), iType.SPATIAL);
            SDR_SOM Sdr_Som3A = new SDR_SOM(10, 10, new List<Position_SOM>() { }, iType.SPATIAL);

            #region STEP 1
            // STEP 1 : Fire SDR's for L4 and L3A

            // STEP 1A : Fire all FOM's first!

            BlockBehaviourManager fom;

            Mapper mapper = new Mapper(NumBBMNeeded, BlockSize);

            mapper.ParseBitmap(bmp);

            List<Position_SOM> somPosition = new List<Position_SOM>();

            int blackCount = 0;

            for (int i = 0; i < bmp.Width; i++)
            {
                for (int j = 0; j < bmp.Height; j++)
                {
                    if (mapper.flagCheckArr[i, j] == false)
                    {
                        blackCount++;
                    }
                }
            }

            foreach (var kvp in mapper.FOMBBMIDS)
            {
                switch (kvp.Key)
                {
                    case MAPPERCASE.ALL:
                        {
                            foreach (var bbmID in kvp.Value)
                            {
                                fomBBM[bbmID].Fire(mapper.GetSDR_SOMForMapperCase(MAPPERCASE.ALL, LayerType.Layer_4, bbmID));
                            }
                        }
                        break;
                    case MAPPERCASE.ONETWOTHREEE:
                        {
                            foreach (var bbmID in kvp.Value)
                            {
                                fomBBM[bbmID].Fire(mapper.GetSDR_SOMForMapperCase(MAPPERCASE.ONETWOTHREEE, LayerType.Layer_4, bbmID));
                            }
                        }
                        break;
                    case MAPPERCASE.TWOTHREEFOUR:
                        {

                            foreach (var bbmID in kvp.Value)
                            {
                                fomBBM[bbmID].Fire(mapper.GetSDR_SOMForMapperCase(MAPPERCASE.TWOTHREEFOUR, LayerType.Layer_4, bbmID));
                            }
                        }
                        break;
                    case MAPPERCASE.ONETWOFOUR:
                        {
                            foreach (var bbmID in kvp.Value)
                            {
                                fomBBM[bbmID].Fire(mapper.GetSDR_SOMForMapperCase(MAPPERCASE.ONETWOFOUR, LayerType.Layer_4, bbmID));
                            }
                        }
                        break;
                    case MAPPERCASE.ONETHREEFOUR:
                        {
                            foreach (var bbmID in kvp.Value)
                            {
                                fomBBM[bbmID].Fire(mapper.GetSDR_SOMForMapperCase(MAPPERCASE.ONETHREEFOUR, LayerType.Layer_4, bbmID));
                            }
                        }
                        break;
                    case MAPPERCASE.ONETWO:
                        {
                            foreach (var bbmID in kvp.Value)
                            {
                                fomBBM[bbmID].Fire(mapper.GetSDR_SOMForMapperCase(MAPPERCASE.ONETWO, LayerType.Layer_4, bbmID));
                            }
                        }
                        break;
                    case MAPPERCASE.ONETHREE:
                        {
                            foreach (var bbmID in kvp.Value)
                            {
                                fomBBM[bbmID].Fire(mapper.GetSDR_SOMForMapperCase(MAPPERCASE.ONETHREE, LayerType.Layer_4, bbmID));
                            }
                        }
                        break;
                    case MAPPERCASE.ONEFOUR:
                        {
                            foreach (var bbmID in kvp.Value)
                            {
                                fomBBM[bbmID].Fire(mapper.GetSDR_SOMForMapperCase(MAPPERCASE.ONEFOUR, LayerType.Layer_4, bbmID));
                            }
                        }
                        break;
                    case MAPPERCASE.TWOTHREE:
                        {
                            foreach (var bbmID in kvp.Value)
                            {
                                fomBBM[bbmID].Fire(mapper.GetSDR_SOMForMapperCase(MAPPERCASE.TWOTHREE, LayerType.Layer_4, bbmID));
                            }
                        }
                        break;
                    case MAPPERCASE.TWOFOUR:
                        {
                            foreach (var bbmID in kvp.Value)
                            {
                                fomBBM[bbmID].Fire(mapper.GetSDR_SOMForMapperCase(MAPPERCASE.TWOFOUR, LayerType.Layer_4, bbmID));
                            }
                        }
                        break;
                    case MAPPERCASE.THREEFOUR:
                        {
                            foreach (var bbmID in kvp.Value)
                            {
                                fomBBM[bbmID].Fire(mapper.GetSDR_SOMForMapperCase(MAPPERCASE.THREEFOUR, LayerType.Layer_4, bbmID));
                            }
                        }
                        break;
                    case MAPPERCASE.ONE:
                        {
                            foreach (var bbmID in kvp.Value)
                            {
                                fomBBM[bbmID].Fire(mapper.GetSDR_SOMForMapperCase(MAPPERCASE.ONE, LayerType.Layer_4, bbmID));
                            }
                        }
                        break;
                    case MAPPERCASE.TWO:
                        {
                            foreach (var bbmID in kvp.Value)
                            {
                                fomBBM[bbmID].Fire(mapper.GetSDR_SOMForMapperCase(MAPPERCASE.TWO, LayerType.Layer_4, bbmID));
                            }
                        }
                        break;
                    case MAPPERCASE.THREE:
                        {
                            foreach (var bbmID in kvp.Value)
                            {
                                fomBBM[bbmID].Fire(mapper.GetSDR_SOMForMapperCase(MAPPERCASE.THREE, LayerType.Layer_4, bbmID));
                            }
                        }
                        break;
                    case MAPPERCASE.FOUR:
                        {
                            foreach (var bbmID in kvp.Value)
                            {
                                fomBBM[bbmID].Fire(mapper.GetSDR_SOMForMapperCase(MAPPERCASE.FOUR, LayerType.Layer_4, bbmID));
                            }
                        }
                        break;
                    default:
                        {
                            throw new NotImplementedException();
                        }
                }
            }

            //STEP 1B : Fire all L3B SOM's

            if (mapper.somPositions.Count != 0)
                somBBM_L3B.Fire(new SDR_SOM(1250, 10, mapper.somPositions, iType.SPATIAL));


            #endregion                       

        }

        public void ProcessStep2()
        {

            #region STEP 2

            // STEP 2 :
            // If Prediction Mode
            // Collect Predictions from L4 & L3A [ Locations of the firing neurons connected to HC should automatically get interpreted as location that needs to be looked at ]
            // Project  L3B -> HC if any for next motor output
            // Push SDRs from L4 -> L3A for better Spatial Pooling
            // Else If Training Mode
            // Push L4 -> L3A for spatial pooling
            // 
            // Let orchestrator take over with next location



            // Project  L3B -> HC for  populating object sensei into HCE

            if (NMode.Equals(NetworkMode.TRAINING))
            {
                //Create Sennsei <Location, ActivePosition> , feed it HC Accessor.
                
                //Sensation_Location sensei = new Sensation_Location()

                //HCAccessor.ProcessCurrentPatternForObject(sensei, "Apple");



            }
            else if (NMode.Equals(NetworkMode.PREDICTION))
            {
                
                // If any output from HC execute the location output if NOT then take the standar default output.
            }

            #endregion
        }        

        public string ProcessStep3()
        {
            #region STEP 3
            string obj = string.Empty;

            // STEP 3 :
            // Check if there is any desired output from HEC , Use it to depolarize L4 and Perfrom Motor Output.
            // if no motor output exists , most likely very early in training phase , let orchestrator run on its own.


            return obj;
            #endregion
        }

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
            //    somBBM_L3A.Fire(Sdr_Som3A);

            //    // init L3B to Apple
            //    SDR_SOM Sdr_SomL3B = GetSdrSomFromFOMs();
            //    somBBM_L3B.Fire(Sdr_SomL3B);

            //    // Push new object representation to 3A
            //    // Push Wiring Burst Avoiding LTP to 4 from 3A.
            //}


            return obj;
        }

        #endregion

        private SDR_SOM GetSdrSomFromFOMs()
        {
            throw new NotImplementedException();
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


        private void PrintMoreBlockVitals()
        {
            Console.WriteLine("Enter '1' to see a list of all the Block Usage List :");

            int w = Console.Read();

            if (w == 49)
            {
                ulong totalIncludedCycle = 0;

                for (int i = 0; i < fomBBM.Count(); i++)
                {
                    if (fomBBM[i].BBMID != 0)
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


        public void Restore()
        {

        }

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


        #region TRASH


        //public void GrabNProcess(ref bool[,] booleans)          //We process one image at once.
        //{
        //    //Todo : Pixel combination should not be serial , it should be randomly distributed through out the unit

        //    Stopwatch stopWatch = new Stopwatch();

        //    stopWatch.Start();

        //    int TotalReps = 2;

        //    int TotalNumberOfPixelsToProcess_X = GetRoundedTotalNumberOfPixelsToProcess(bmp.Width);
        //    int TotalNumberOfPixelsToProcess_Y = GetRoundedTotalNumberOfPixelsToProcess(bmp.Height);

        //    int TotalPixelsCoveredPerIteration = BlockOffset * BlockOffset; //2500            

        //    int num_blocks_per_bmp_x = (int)(TotalNumberOfPixelsToProcess_X / BlockOffset);
        //    int num_blocks_per_bmp_y = (int)(TotalNumberOfPixelsToProcess_Y / BlockOffset);

        //    int num_unit_per_block_x = 5;
        //    int num_unit_per_block_y = 5;

        //    int num_pixels_per_Unit_x = 10;
        //    int num_pixels_per_Unit_y = 10;

        //    for (int reps = 0; reps < TotalReps; reps++)
        //    {
        //        for (int blockid_y = 0; blockid_y < num_blocks_per_bmp_y; blockid_y++)
        //        {
        //            for (int blockid_x = 0; blockid_x < num_blocks_per_bmp_x; blockid_x++)
        //            {
        //                int bbmId = 0;

        //                for (int unitId_y = 0; unitId_y < num_unit_per_block_y; unitId_y++)
        //                {
        //                    for (int unitId_x = 0; unitId_x < num_unit_per_block_x; unitId_x++)
        //                    {
        //                        BoolEncoder boolEncoder = new BoolEncoder(100, 20);

        //                        for (int j = 0; j < num_pixels_per_Unit_x; j++)
        //                        {
        //                            for (int i = 0; i < num_pixels_per_Unit_y; i++)
        //                            {
        //                                int pixel_x = blockid_x * BlockOffset + unitId_x * UnitOffset + i;
        //                                int pixel_y = blockid_y * BlockOffset + unitId_y * UnitOffset + j;

        //                                //if the pixel is Black then tag the pixel location

        //                                if (blockid_x == 6 && blockid_y == 0 && unitId_x == 4 && unitId_y == 2 && j == 2)
        //                                {
        //                                    int bp = 1;
        //                                }

        //                                if (CheckifPixelisBlack(pixel_x, pixel_y))
        //                                {

        //                                    var dataToEncode = (j % 2).ToString() + "-" + i.ToString();
        //                                    boolEncoder.SetEncoderValues(dataToEncode);

        //                                }
        //                            }

        //                            if (j % 2 == 1)     //Bcoz one BBM covers 2 lines of pixel per unit
        //                            {
        //                                if (fomBBM[bbmId].TemporalLineArray[0, 0] == null)
        //                                {
        //                                    fomBBM[bbmId].Init(blockid_x, blockid_y, unitId_x, unitId_y, bbmId);
        //                                }

        //                                if (boolEncoder.HasValues())
        //                                {
        //                                    CycleNum++;

        //                                    var imageSDR = boolEncoder.Encode(iType.SPATIAL);

        //                                    fomBBM[bbmId++].Fire(imageSDR);

        //                                    SDR_SOM fomSDR = fomBBM[bbmId].GetPredictedSDR();

        //                                    if (fomSDR != null && fomSDR.ActiveBits.Count != 0)
        //                                    {
        //                                        fomSDR = AddSOMOverheadtoFOMSDR(fomSDR, blockid_x, blockid_y);

        //                                        somBBM_L3B.Fire(fomSDR);
        //                                    }

        //                                }

        //                                boolEncoder.ClearEncoderValues();
        //                            }
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //    }


        //    PrintMoreBlockVitals();

        //    BackUp();

        //    Console.WriteLine("Finished Processing Pixel Values : Total Time Elapsed in seconds : " + (stopWatch.ElapsedMilliseconds / 1000).ToString());

        //    Console.WriteLine("Black Pixel Count :: " + blackPixelCount.ToString());

        //    Console.WriteLine("Done Processing Image");

        //    Console.Read();
        //}

        //public int GetRoundedTotalNumberOfPixelsToProcess(int numberOfPixels_Index)
        //{
        //    if (numberOfPixels_Index % BlockOffset == 0)
        //    {
        //        return numberOfPixels_Index;
        //    }

        //    int nextMinNumberOfPixels = numberOfPixels_Index;

        //    int halfOfnextMinNumberOfPixels = numberOfPixels_Index / 2;

        //    while (nextMinNumberOfPixels % 50 != 0)
        //    {
        //        nextMinNumberOfPixels--;

        //        if (nextMinNumberOfPixels < halfOfnextMinNumberOfPixels)
        //        {
        //            Console.WriteLine(" GetRoundedTotalNumberOfPixelsToProcess() :: Unable to find the proper lower Bound");
        //        }

        //    }

        //    if (nextMinNumberOfPixels % 50 != 0)
        //        throw new InvalidDataException("Grab :: blockLength should always be factor of NumPixelToProcess");

        //    return nextMinNumberOfPixels;
        //}

        public void Grab()
        {

            Console.WriteLine("Grabbing cursor Position");

            POINT Point = this.GetCurrentPointerPosition();

            Console.WriteLine("Grabbing Screen Pixels...");

            int Range2 = Range + Range;

            int x1 = Point.X - Range < 0 ? 0 : Point.X - Range;
            int y1 = Point.Y - Range < 0 ? 0 : Point.Y - Range;
            int x2 = Math.Abs(Point.X + Range2);
            int y2 = Math.Abs(Point.Y + Range);

            //this.GetColorByRange(x1, y1, x2, y2);

            Rectangle rect = new Rectangle(x1, y1, x2, y2);

            bmp = new Bitmap(Range2 + Range2, Range2, PixelFormat.Format32bppArgb);

            Graphics g = Graphics.FromImage(bmp);

            g.CopyFromScreen(x1, y1, 0, 0, bmp.Size, CopyPixelOperation.SourceCopy);

            bmp.Save(filename, ImageFormat.Jpeg);

        }


        // Already grey scalled.
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

        public void ChangeNetworkModeTo(NetworkMode mode)
        {
            throw new NotImplementedException();
        }

        #endregion

        #endregion

        #endregion
    }


    #region Enums

    public enum VisionScope
    {
        BroadScope,
        ObjectScope,
        NarrowScope,
        UNKNOWN
    }

    #endregion
}
