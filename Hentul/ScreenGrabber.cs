namespace Hentul
{
    using System.Drawing;
    using System.Runtime.InteropServices;
    using System.Configuration;
    using Common;
    using FirstOrderMemory.Models;
    using System.Diagnostics;
    using System.Collections.Generic;
    using Hentul.UT;
    using System.Diagnostics.Eventing.Reader;
    using System.Numerics;
    using System.Drawing.Imaging;
    using FirstOrderMemory.Models.Encoders;

    public struct POINT
    {
        public int X;
        public int Y;
    }

    public class ScreenGrabber
    {

        #region Used Variables

        public int NumPixelsToProcessPerBlock { get; private set; }

        public int numPixelsProcessedPerBBM;

        public int NumBBMNeeded { get; private set; }

        private bool LogMode { get; set; }

        public bool IsMock { get; private set; }

        int NumColumns, Z;

        public Dictionary<int, List<Position_SOM>> Buckets;

        public FirstOrderMemory.BehaviourManagers.BlockBehaviourManager[] fomBBM { get; private set; }

        public int[] MockBlockNumFires { get; private set; } 

        private bool devbox = false;

        public int BlockOffset;

        public int UnitOffset;

        public int ImageIndex { get; private set; }

        public ulong CycleNum;

        public List<string> ImageList { get; private set; }

        public int blackPixelCount = 0;

        public Bitmap bmp;

        private readonly int FOMLENGTH = Convert.ToInt32(ConfigurationManager.AppSettings["FOMLENGTH"]);
        private readonly int FOMWIDTH = Convert.ToInt32(ConfigurationManager.AppSettings["FOMWIDTH"]);
        private readonly int SOM_NUM_COLUMNS = Convert.ToInt32(ConfigurationManager.AppSettings["SOMNUMCOLUMNS"]);
        private readonly int SOM_COLUMN_SIZE = Convert.ToInt32(ConfigurationManager.AppSettings["SOMCOLUMNSIZE"]);


        #endregion

        #region Unused Variables

        public POINT Point { get; set; }
        public int BucketRowLength { get; private set; }
        public int BucketColLength { get; private set; }
        public int Iterations;
        public Tuple<int, int> LeftUpper { get; private set; }
        public Tuple<int, int> RightUpper { get; private set; }
        public Tuple<int, int> LeftBottom { get; private set; }
        public Tuple<int, int> RightBottom { get; private set; }
        public Tuple<int, int> CenterCenter { get; private set; }
        private int RangeIterator;
        public const int MaxSparsity = 10;
        public Dictionary<int, LocationNPositions> BucketToData { get; private set; }
        public string CurrentDirection = string.Empty;
        public int leftBound;
        public int upperBound;
        public int rightBound;
        public int lowerBound;
        public int RounRobinIteration;


        #endregion

        public ScreenGrabber(int range, bool isMock = false, bool ShouldInit = true, int mockImageIndex = 7)
        {
            //Todo : Project shape data of the input image to one region and project colour data of the image to another region.                        

            NumPixelsToProcessPerBlock = range;

            numPixelsProcessedPerBBM = 20;

            LogMode = false;

            double numerator = (2 * NumPixelsToProcessPerBlock) * (2 * NumPixelsToProcessPerBlock);

            double denominator = numPixelsProcessedPerBBM;

            double value = (numerator / denominator);

            bool b = value % 1 != 0;

            if (value % 1 != 0)
            {
                throw new InvalidDataException("Number Of Pixels should always be a factor of BucketColLength : NumPixels : " + NumPixelsToProcessPerBlock.ToString() + "  NumPixelsPerBucket" + (BucketRowLength * BucketColLength).ToString());
            }

            BlockOffset = range * 2;

            UnitOffset = 10;

            NumBBMNeeded = (int)value;

            fomBBM = new FirstOrderMemory.BehaviourManagers.BlockBehaviourManager[NumBBMNeeded];

            NumColumns = 10;

            IsMock = isMock;

            Z = 10;

            CycleNum = 0;

            for (int i = 0; i < NumBBMNeeded; i++)
            {
                fomBBM[i] = new FirstOrderMemory.BehaviourManagers.BlockBehaviourManager(NumColumns, Z);
            } 
            
            if (isMock)
                ImageIndex = mockImageIndex;
            else
                ImageIndex = 0;

            MockBlockNumFires = new int[NumBBMNeeded];

            ImageList = AddAllTheFruits();

            LoadImage();

            #region Unused Variables
            //CenterCenter = new Tuple<int, int>((LeftUpper.Item1 + RightUpper.Item1) / 2, ((RightUpper.Item2 + LeftBottom.Item2) / 2));
            //CurrentDirection = "RIGHT";
            //Iterations = 0;
            //LeftUpper = new Tuple<int, int>(1007, 412);
            //RightUpper = new Tuple<int, int>(1550, 412);
            //LeftBottom = new Tuple<int, int>(1007, 972);
            //RightBottom = new Tuple<int, int>(1550, 972);
            //BucketToData = new Dictionary<int, LocationNPositions>();
            //RangeIterator = 0;
            //leftBound = 51;
            //rightBound = 551;
            //upperBound = 551;
            //lowerBound = 51;
            //RounRobinIteration = 0;
            #endregion
        }

        public void Init()
        {
            Stopwatch stopWatch = new Stopwatch();

            Console.WriteLine("Starting Initialization  of FOM objects : \n");

            stopWatch.Start();

            for (int i = 0; i < NumBBMNeeded; i++)
            {
                fomBBM[i].Init(0, 0, 1, 1, 10);
            }        

            stopWatch.Stop();

            Console.WriteLine("Finished Init for this Instance , Total Time ELapsed : " + stopWatch.ElapsedMilliseconds.ToString() + "\n");
            Console.WriteLine("Finished Initting of all Instances, System Ready!" + "\n");
            Console.WriteLine("Range" + NumPixelsToProcessPerBlock.ToString() + "\n");
            Console.WriteLine("Total Number of Pixels :" + (NumPixelsToProcessPerBlock * NumPixelsToProcessPerBlock * 4).ToString() + "\n");
            Console.WriteLine("Total First Order BBMs Created :" + NumBBMNeeded.ToString() + "\n");

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

        public bool SwitchImage()
        {
            ImageIndex++;

            if (ImageIndex == ImageList.Count)
                return false;

            return true;
        }

        public void LoadImage()
        {
            Console.WriteLine("Loading Image : "  + ImageList[ImageIndex].ToString().Split(new char[1] { '\\' })[7]);

            bmp = new Bitmap(ImageList[ImageIndex]);

            if (bmp == null)
            {
                throw new InvalidCastException("Couldn't find image");
            }
        }

        public void GrabNProcess(ref bool[,] booleans)          //We process one image at once.
        {
            //Todo : Pixel combination should not be serial , it should be randomly distributed through out the unit

            Stopwatch stopWatch = new Stopwatch();

            stopWatch.Start();

            int TotalNumberOfPixelsToProcess_X = GetRoundedTotalNumberOfPixelsToProcess(bmp.Width);
            int TotalNumberOfPixelsToProcess_Y = GetRoundedTotalNumberOfPixelsToProcess(bmp.Height);

            int TotalPixelsCoveredPerIteration = BlockOffset * BlockOffset; //2500            

            int num_blocks_per_bmp_x = (int)(TotalNumberOfPixelsToProcess_X / BlockOffset);
            int num_blocks_per_bmp_y = (int)(TotalNumberOfPixelsToProcess_Y / BlockOffset);

            int num_unit_per_block_x = 5;
            int num_unit_per_block_y = 5;            
            int num_pixels_per_bbm_x = 10;
            int num_pixels_per_bbm_y = 10;

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

                            for (int j = 0; j < num_pixels_per_bbm_y; j++)
                            {
                                for (int i = 0; i < num_pixels_per_bbm_x; i++)
                                {
                                    int pixel_x = blockid_x * BlockOffset + unitId_x * UnitOffset + i;
                                    int pixel_y = blockid_y * BlockOffset + unitId_y * UnitOffset + j;

                                    //if the pixel is Black then tag the pixel location

                                    if(blockid_x == 6 && blockid_y == 0 && unitId_x == 4 && unitId_y == 2 && j == 2)
                                    {
                                        int bp = 1;
                                    }

                                    if (IsMock && booleans != null)
                                    {                                        
                                        booleans[pixel_x, pixel_y] = true;
                                    }
                                    else if (CheckifPixelisBlack(pixel_x, pixel_y))
                                    {                                                                                
                                        boolEncoder.SetEncoderValues((j % 2).ToString() + "-" + i.ToString());
                                    }                                                             
                                }

                                if (IsMock == false)
                                {
                                    if (j % 2 == 1)     //Bcoz one BBM covers 2 lines of pixel per unit
                                    {
                                        CycleNum++;

                                        if (fomBBM[bbmId].TemporalLineArray[0,0] == null)
                                        {
                                            fomBBM[bbmId].Init(blockid_x, blockid_y, unitId_x, unitId_y, bbmId);
                                        }

                                        if (boolEncoder.HasValues())
                                            fomBBM[bbmId++].Fire(boolEncoder.Encode(iType.SPATIAL));

                                        boolEncoder.ClearEncoderValues();
                                    }
                                }
                                else
                                {
                                    if (j % 2 == 1)     //Bcoz one BBM covers 2 lines of pixel per unit
                                    {
                                        MockBlockNumFires[bbmId]++;
                                        bbmId++;
                                    }
                                }
                            }                    
                        }
                    }
                }
            }

            PrintBlockVital();

            Console.WriteLine("Finished Processing Pixel Values : Total Time Elapsed in seconds : " + (stopWatch.ElapsedMilliseconds / 1000).ToString());

            Console.WriteLine("Black Pixel Count :: " + blackPixelCount.ToString());

            Thread.Sleep(5000);
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

        public void PrintBlockVital()
        {
            //Console.WriteLine(@"----Block ID ------------Total # Bursts---------------------Total # Correct Predictions------------------- ");

            foreach (var fom in fomBBM)
            {
                fom.PrintBlockStats();
            }

            Console.WriteLine(" Orchestrator CycleNum : " + CycleNum.ToString());
        }

        public void BackUp()
        {
            for (int i = 0; i < fomBBM.Length; i++)
            {
                fomBBM[i].BackUp(i.ToString());
            }
        }

        #region UnUsed Code
        /*

        public Color GetColorAt(int x, int y)
        {
            if (x > 578 || y >= 600 || x < 0 || y < 0)
                return Color.White;

            return bmp.GetPixel(x, y);
        }

        public Tuple<int, int, int, int> GetNextCoordinates(int x1, int y1, int x2, int y2)
        {
            Tuple<int, int, int, int> retVal = null;

            if (RounRobinIteration >= 6)
            {
                // Todo: Implement BackUp();

                Console.WriteLine("Completed 6 Iterations of the Image !!! Swithcing to next image in queue!");

                return new Tuple<int, int, int, int>(-1, -1, -1, -1);
            }

            switch (CurrentDirection.ToUpper())
            {
                case "RIGHT":
                    {
                        if (x2 < rightBound)
                        {
                            retVal = new Tuple<int, int, int, int>(x1 + Offset, y1, x2 + Offset, y2);
                        }
                        else
                        {
                            CurrentDirection = "UP";
                            Console.WriteLine("Changing Parse Direction on the Image to " + CurrentDirection.ToString());
                            Console.WriteLine("Cuurent Image Being Analysed : " + ImageList[ImageIndex].Split(new char[] { '\\' })[7]);
                            //retVal = new Tuple<int, int, int, int>(rightBound, RounRobinIteration * Offset, rightBound - 1 + Offset, 2);
                            retVal = new Tuple<int, int, int, int>(x1, y1 + Offset, x2, y2 + Offset);
                            //retVal.Item2 = RounRobinIteration * Offset;
                            rightBound -= Offset;

                        }
                        break;
                    }
                case "DOWN":
                    {
                        if (y1 < lowerBound)
                        {
                            retVal = new Tuple<int, int, int, int>(x1, y1 - Offset, x2, y2 - Offset);
                        }
                        else
                        {
                            CurrentDirection = "RIGHT";
                            Console.WriteLine("Changing Parse Direction on the Image to " + CurrentDirection.ToString());
                            Console.WriteLine("Cuurent Image Being Analysed : " + ImageList[ImageIndex].Split(new char[] { '\\' })[7]);
                            lowerBound += Offset;
                            retVal = new Tuple<int, int, int, int>(x1 + Offset, y1, x2 + Offset, y2);
                            RounRobinIteration++;
                        }
                        break;
                    }
                case "LEFT":
                    {
                        if (x1 > leftBound)
                        {
                            retVal = new Tuple<int, int, int, int>(x1 - Offset, y1, x2 - Offset, y2);
                        }
                        else
                        {
                            CurrentDirection = "DOWN";
                            Console.WriteLine("Changing Parse Direction on the Image to " + CurrentDirection.ToString());
                            Console.WriteLine("Cuurent Image Being Analysed : " + ImageList[ImageIndex].Split(new char[] { '\\' })[7]);
                            leftBound += Offset;
                            retVal = new Tuple<int, int, int, int>(x1, y1 - Offset, x2, y2 - Offset);
                        }
                        break;
                    }
                case "UP":
                    {
                        if (y2 < upperBound)
                        {
                            retVal = new Tuple<int, int, int, int>(x1, y1 + Offset, x2, y2 + Offset);
                        }
                        else
                        {
                            CurrentDirection = "LEFT";
                            Console.WriteLine("Changing Parse Direction on the Image to " + CurrentDirection.ToString());
                            Console.WriteLine("Cuurent Image Being Analysed : " + ImageList[ImageIndex].Split(new char[] { '\\' })[7]);
                            upperBound -= Offset;
                            retVal = new Tuple<int, int, int, int>(x1 - Offset, y1, x2 - Offset, y2);
                        }
                        break;
                    }
                default:
                    {
                        throw new InvalidOperationException("GetNextCooridnate :: Direction shouldnt be anything other than above 4 values");
                    }
            }

            return retVal;
        }

        private void ResetOffsets()
        {
            leftBound = 51;
            rightBound = 551;
            upperBound = 551;
            lowerBound = 51;
            RounRobinIteration = 0;
        }

        public void PreparePixelData(int x1, int y1, int x2, int y2)
        {
            //Total Screen Size = 50 * 50 = 2500 pixels.
            //Num Pixels / BBM = 10 pixels / BBM.
            //Num BBMs Required = 250.
            //Num Buckets = 250.
            //1 BBM takes in BucketRowwLength * BucketColLEngth i.e 5 * 2 = 10 pixels each.

            int bucket = 0;
            int notBlackCount = 0;
            int blackCount = 0;

            int doubleRange = 2 * NumPixelsToProcessPerBlock;

            if (LogMode)
                Console.WriteLine("Getting Screen Pixels : ");

            for (int j = y1, l = 0; j < y2 && l < doubleRange; j++, l++)
            {
                //Console.WriteLine("Row " + j.ToString());

                for (int i = x1, k = 0; i < x2 && k < doubleRange; i++, k++)
                {

                    Color color = GetColorAt(i, j);

                    if (color.R < 200 || color.G < 200 || color.B < 200)
                    {
                        blackCount++;

                        bucket = l / BucketRowLength + (k / BucketColLength) * BucketColLength * 10;  // This automatically sorts out the pixel locations that are active into there respective buckets. [Check out the Unit Test for more info]

                        Position_SOM newPosition = new Position_SOM((k % BucketColLength), (l % BucketRowLength));

                        if ((k % BucketRowLength > 9) || (l % BucketRowLength > 9))
                        {
                            throw new InvalidDataException("ProcessColorMap :: Completely B.S Logic! Fix your fucking EQUATIONSSSSSS !!!! ");
                        }

                        if (BucketToData.TryGetValue(bucket, out var data))
                        {
                            data.AddNewPostion(newPosition);
                        }
                        else
                        {
                            BucketToData.Add(bucket, new LocationNPositions(new List<Position_SOM> { newPosition }, i, j));
                        }
                    }
                    else
                    {
                        notBlackCount++;
                    }
                }
            }

            if (blackCount == 1)
            {
                //Happens only for Mock Test Image
                blackPixelCount++;
            }
            if (LogMode)
                Console.WriteLine("Done Collecting Screen Pixels");

        }

        public void ProcessPixelData()
        {
            //Input : range = 25 , Total Number Of Pixels = 2500 , Input SDR size = 100

            SDR_SOM spatialPattern, temporalPattern;

            int index = 0;

            if (LogMode)
                Console.WriteLine("Processing Pixel Data :");

            foreach (var bucket in BucketToData)
            {

                spatialPattern = GetSpatialPatternForBucket(bucket.Value.Positions);

                temporalPattern = GenerateTemporalSDR(bucket.Value.X, bucket.Value.Y);

                fomBBM[index].Fire(temporalPattern);

                fomBBM[index].Fire(spatialPattern);

                ++index;
            }

            if (LogMode)
                Console.WriteLine("Done Processing Pixel Data");
        }

        public void Grab2()
        {

            Stopwatch stopWatch = new Stopwatch();

            stopWatch.Start();

            Console.CursorVisible = false;

            Point = this.GetCurrentPointerPosition();

            Console.CursorVisible = true;

            Console.WriteLine("Grabbing Screen Pixels...");

            int x1 = Point.X - NumPixelsToProcessPerBlock < 0 ? 0 : Point.X - NumPixelsToProcessPerBlock;
            int y1 = Point.Y - NumPixelsToProcessPerBlock < 0 ? 0 : Point.Y - NumPixelsToProcessPerBlock;
            int x2 = Math.Abs(Point.X + NumPixelsToProcessPerBlock);
            int y2 = Math.Abs(Point.Y + NumPixelsToProcessPerBlock);

            this.PreparePixelData(x1, y1, x2, y2);

            stopWatch.Stop();

            Console.WriteLine("Finished Getting Pixels Values : Total Time Elapsed in seconds : " + (stopWatch.ElapsedMilliseconds / 1000).ToString());
        }

        public Tuple<int, int, int, int> Grab1()
        {
            //send Image for Processing                           

            Console.CursorVisible = false;

            Point = this.GetCurrentPointerPosition();

            Console.CursorVisible = true;

            Console.WriteLine("Grabbing Screen Pixels...");

            int x1 = Point.X - NumPixelsToProcessPerBlock < 0 ? 0 : Point.X - NumPixelsToProcessPerBlock;
            int y1 = Point.Y - NumPixelsToProcessPerBlock < 0 ? 0 : Point.Y - NumPixelsToProcessPerBlock;
            int x2 = Math.Abs(Point.X + NumPixelsToProcessPerBlock);
            int y2 = Math.Abs(Point.Y + NumPixelsToProcessPerBlock);

            return new Tuple<int, int, int, int>(x1, y1, x2, y2);
            //this.ProcessColorMap(x1, y1, x2, y2);
        }

        private SDR_SOM GetSpatialPatternForBucket(List<Position_SOM> positions) =>
                new SDR_SOM(NumColumns, Z, positions, iType.SPATIAL);

        public SDR_SOM GenerateTemporalSDR(int x, int y)
        {

            LocationScalarEncoder encoder = new LocationScalarEncoder(100, 32);

            //Todo : buckett is not the correct parameter for encoding here.
            return encoder.Encode(x, y);

        }

        [DllImport("user32.dll")]
        static extern bool GetCursorPos(out POINT lpPoint);
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

        public void MoveCursor()
        {
            POINT p;

            IntPtr desk = GetDesktopWindow();
            IntPtr dc = GetWindowDC(desk);

            p = GetNextCursorPosition();

            Point = p;

            ClientToScreen(dc, ref p);
            SetCursorPos(p.X, p.Y);

            ReleaseDC(desk, dc);

        }

        public void SetMousetotartingPoint()
        {
            MoveCursorToSpecificPosition(LeftUpper.Item1, LeftUpper.Item2);
        }

        #region PRIVATE METHODS        

        public POINT GetNextCursorPosition(bool isMock = false, int x = 0, int y = 0)
        {
            POINT toReturn;

            if (isMock)
            {
                toReturn.X = x; toReturn.Y = y;
            }
            else
            {
                toReturn = Point;
            }

            if (toReturn.X < LeftUpper.Item1 - Offset - 1 || toReturn.Y < LeftUpper.Item2 - Offset - 1 || toReturn.X > RightBottom.Item1 + Offset + 1 || toReturn.Y > RightBottom.Item2 + Offset + 1)
            {
                int breakpoint = 1;
            }

            if ((toReturn.X <= CenterCenter.Item1 - 25 && toReturn.X >= CenterCenter.Item1 + 25) && (toReturn.Y >= CenterCenter.Item2 - 25 && toReturn.Y <= CenterCenter.Item2 + 25))
            {
                Console.WriteLine("Reached the Center of Image ! ReStarting the system to the begining of the image");

                if (Iterations == TOTALPARSEITERATIONS)
                {
                    // Start BackUp

                    foreach (var bbm in fomBBM)
                    {
                        bbm.BackUp(bbm.BlockID.X.ToString() + ".xml");
                    }

                    Console.WriteLine("System has Finished Parsing the Image & Backed Up ! Take a Bow!!! You achieved something great today!!!! ");

                    Console.Read();
                }
                else
                {
                    Iterations++;
                }

                toReturn.X = LeftUpper.Item1;
                toReturn.Y = LeftUpper.Item2;

                return toReturn;
            }

            switch (CurrentDirection.ToUpper())
            {
                case "RIGHT":
                    {
                        if (toReturn.X >= (RightUpper.Item1 + RangeIterator * NumPixelsToProcessPerBlock))
                        {
                            CurrentDirection = "DOWN";
                            toReturn.X = RightUpper.Item1 - RangeIterator * NumPixelsToProcessPerBlock;
                            toReturn.Y = RightUpper.Item2;
                        }
                        else
                        {
                            toReturn.X = toReturn.X + Offset;
                        }
                        break;
                    }
                case "DOWN":
                    {
                        if (toReturn.Y >= (RightBottom.Item2 + RangeIterator * NumPixelsToProcessPerBlock))
                        {
                            CurrentDirection = "LEFT";
                            toReturn.X = RightBottom.Item1;
                            toReturn.Y = RightBottom.Item2 - RangeIterator * NumPixelsToProcessPerBlock;
                        }
                        else
                        {
                            toReturn.Y = toReturn.Y + Offset;
                        }
                        break;
                    }
                case "LEFT":
                    {
                        if (toReturn.X <= (LeftBottom.Item1 + RangeIterator * NumPixelsToProcessPerBlock))
                        {
                            CurrentDirection = "UP";
                            toReturn.X = LeftBottom.Item1 + RangeIterator * NumPixelsToProcessPerBlock; ;
                            toReturn.Y = LeftBottom.Item2;
                            RangeIterator++;
                        }
                        else
                        {
                            toReturn.X = toReturn.X - Offset;
                        }
                        break;
                    }
                case "UP":
                    {
                        if (toReturn.Y <= (LeftUpper.Item2 + RangeIterator * NumPixelsToProcessPerBlock))
                        {
                            CurrentDirection = "RIGHT";
                            toReturn.X = LeftUpper.Item1 + RangeIterator * NumPixelsToProcessPerBlock;
                            toReturn.Y = LeftUpper.Item2 + RangeIterator * NumPixelsToProcessPerBlock;
                        }
                        else
                        {
                            toReturn.Y = toReturn.Y - Offset;
                        }
                        break;
                    }
                default:
                    {
                        throw new InvalidOperationException("Should Not Happen!");
                    }
            }

            Point = toReturn;

            return toReturn;
        }

        public static Color GetColorAt1(int x, int y)
        {
            IntPtr desk = GetDesktopWindow();
            IntPtr dc = GetWindowDC(desk);
            int a = (int)GetPixel(dc, x, y);
            ReleaseDC(desk, dc);
            return Color.FromArgb(255, (a >> 0) & 0xff, (a >> 8) & 0xff, (a >> 16) & 0xff);
        }

        private void PopulateDataBytes(bool[,] barr)
        {
            string toReturn = String.Empty;

            if (barr.GetUpperBound(2) != 8)
            {
                throw new InvalidOperationException("Bool Array Value should always be equal to byte Size for conversion, Check you SHitty Code! DumbFuck!!!!");
            }

            for (int i = 0; i < barr.GetUpperBound(0); i++)
            {
                for (int j = 0; j < barr.GetUpperBound(1); j++)
                {
                    if (barr[i, j])
                    {
                        toReturn.Append('1');
                    }
                    else
                    {
                        toReturn.Append('0');
                    }

                }

                //Data[i] = Byte.Parse(toReturn);
                toReturn = string.Empty;
            }
        }

        private POINT GetCurrentPointerPosition()
        {
            POINT point;

            if (GetCursorPos(out point))
            {
                Console.Clear();
                Console.WriteLine(point.X.ToString() + " " + point.Y.ToString());
            }
            return point;
        }


        private List<Position_SOM> ConvertFomToSomPositions(List<Position> position)
        {
            List<Position_SOM> toReturn = new List<Position_SOM>();

            foreach (var positionItem in position)
            {
                toReturn.Add(new Position_SOM(positionItem.X, positionItem.Y));
            }

            return toReturn;
        }*/

        #endregion

    }
}