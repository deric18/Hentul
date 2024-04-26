namespace Hentul
{
    using System.Drawing;
    using System.Runtime.InteropServices;
    using System.Configuration;
    using Common;
    using FirstOrderMemory.Models;
    using System.Diagnostics;
    using SecondOrderMemory.BehaviourManagers;
    using System.Collections.Generic;
    using Hentul.UT;
    using System.Diagnostics.Eventing.Reader;

    public struct POINT
    {
        public int X;
        public int Y;
    }

    public class ScreenGrabber
    {
        private const int TOTALPARSEITERATIONS = 5;

        public bool[,] bitMap { get; private set; }

        public POINT Point { get; set; }

        private const int ByteSize = 8;

        public int NumPixelsToProcess { get; private set; }


        /// <summary>
        /// NumBuckets is the total number of FOM Blocks that will be created and it will include 
        /// </summary>
        public int NumBuckets { get; private set; }

        public int BucketRowLength { get; private set; }

        public int BucketColLength { get; private set; }

        private int Iterations;

        public Tuple<int, int> LeftUpper { get; private set; }
        public Tuple<int, int> RightUpper { get; private set; }
        public Tuple<int, int> LeftBottom { get; private set; }
        public Tuple<int, int> RightBottom { get; private set; }
        public Tuple<int, int> CenterCenter { get; private set; }

        private int RangeIterator;
        private int Offset;
        public string CurrentDirection = string.Empty;

        public const int Sparsity = 10;

        public Dictionary<int, Position_SOM> TemporalPositionsForBuckets { get; private set; }

        public Dictionary<int, LocationNPositions> BucketToData { get; private set; }

        public bool IsMock { get; private set; }

        int NumColumns, Z;

        public FirstOrderMemory.BehaviourManagers.BlockBehaviourManager[] fomBBM { get; private set; }        

        public SBBManager somBlock { get; private set; }

        private readonly int FOMLENGTH = Convert.ToInt32(ConfigurationManager.AppSettings["FOMLENGTH"]);
        private readonly int FOMWIDTH = Convert.ToInt32(ConfigurationManager.AppSettings["FOMWIDTH"]);
        private readonly int SOM_NUM_COLUMNS = Convert.ToInt32(ConfigurationManager.AppSettings["SOMNUMCOLUMNS"]);
        private readonly int SOM_COLUMN_SIZE = Convert.ToInt32(ConfigurationManager.AppSettings["SOMCOLUMNSIZE"]);

        public ScreenGrabber(int range, bool isMock = false)
        {
            //Todo : Project shape data of the input image to one region and project colour data of the image to another region.            

            NumPixelsToProcess = range;

            BucketRowLength = 5;

            BucketColLength = 2;

            Iterations = 0;

            double numerator = (2 * NumPixelsToProcess) * (2 * NumPixelsToProcess);

            double denominator = (BucketRowLength * BucketColLength);

            double value = ( numerator / denominator );

            bool b = value % 1 != 0;

            if ( value % 1 != 0)
            {
                throw new InvalidDataException("Number Of Pixels should always be a factor of BucketColLength : NumPixels : " + NumPixelsToProcess.ToString() + "  NumPixelsPerBucket" + (BucketRowLength * BucketColLength).ToString());
            }

            LeftUpper = new Tuple<int, int>(1007, 412);
            RightUpper = new Tuple<int, int>(1550, 412);
            LeftBottom = new Tuple<int, int>(1007, 972);
            RightBottom = new Tuple<int, int>(1550, 972);

            CenterCenter = new Tuple<int, int>((LeftUpper.Item1 + RightUpper.Item1) / 2, ((RightUpper.Item2 + LeftBottom.Item2) / 2));

            CurrentDirection = "RIGHT";

            Offset = range;

            RangeIterator = 0;
            
            NumBuckets = ((2 * NumPixelsToProcess) * (2 * NumPixelsToProcess) / (BucketRowLength * BucketColLength));

            BucketToData = new Dictionary<int, LocationNPositions>();

            fomBBM = new FirstOrderMemory.BehaviourManagers.BlockBehaviourManager[NumBuckets];

            TemporalPositionsForBuckets = new Dictionary<int, Position_SOM>();

            NumColumns = 10;

            IsMock = isMock;

            Z = 10;            

            for (int i = 0; i < NumBuckets; i++)
            {
                fomBBM[i] = new FirstOrderMemory.BehaviourManagers.BlockBehaviourManager(NumColumns, Z, i, 0, 0);
            }

            Init();

            //somBlock = new SOMBlockManager(NumBuckets, NumColumns, Z);
        }

        private void Init()
        {
            Stopwatch stopWatch = new Stopwatch();

            Console.WriteLine("Starting Initialization  of FOM objects : ");

            stopWatch.Start();

            for (int i = 0; i < NumBuckets; i++)
            {
                fomBBM[i].Init(i);
            }

            stopWatch.Stop();

            Console.WriteLine("Finished Init for this Instance , Total Time ELapsed : " + stopWatch.ElapsedMilliseconds.ToString());

            Console.WriteLine("Finished Initting of all Instances, System Ready!");

            Console.WriteLine("Total Pixels being collected for a range of " + NumPixelsToProcess.ToString() + " \nTotal Number of Pixels :" + (NumPixelsToProcess * NumPixelsToProcess * 4).ToString() + "\nTotal First Order BBMs Created :" + NumBuckets.ToString());

        }

        public void Grab()
        {            

            Stopwatch stopWatch = new Stopwatch();

            stopWatch.Start();

            Console.CursorVisible = false;

            Point = this.GetCurrentPointerPosition();

            Console.CursorVisible = true;

            Console.WriteLine("Grabbing Screen Pixels...");

            int x1 = Point.X - NumPixelsToProcess < 0 ? 0 : Point.X - NumPixelsToProcess;
            int y1 = Point.Y - NumPixelsToProcess < 0 ? 0 : Point.Y - NumPixelsToProcess;
            int x2 = Math.Abs(Point.X + NumPixelsToProcess);
            int y2 = Math.Abs(Point.Y + NumPixelsToProcess);

            this.ProcessColorMap(x1, y1, x2, y2);

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

            int x1 = Point.X - NumPixelsToProcess < 0 ? 0 : Point.X - NumPixelsToProcess;
            int y1 = Point.Y - NumPixelsToProcess < 0 ? 0 : Point.Y - NumPixelsToProcess;
            int x2 = Math.Abs(Point.X + NumPixelsToProcess);
            int y2 = Math.Abs(Point.Y + NumPixelsToProcess);

            return new Tuple<int, int, int, int>(x1, y1, x2, y2);
            //this.ProcessColorMap(x1, y1, x2, y2);
        }


        /// <summary>
        /// We do not take in integers as input to Block Managers , we take in bool's , if its positive we take in the value ,if its negative we move on.
        /// we divide up the entire one screenshot 25 * 25  = 625 pixels 
        /// </summary>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="x2"></param>
        /// <param name="y2"></param>
        /// <exception cref="InvalidDataException"></exception>
        public void ProcessColorMap(int x1, int y1, int x2, int y2)
        {
            //Total Screen Size = 50 * 50 = 2500 pixels.
            //Num Pixels / BBM = 10 pixels / BBM.
            //Num BBMs Required = 250.
            //Num Buckets = 250.
            //1 BBM takes in BucketRowwLength * BucketColLEngth i.e 5 * 2 = 10 pixels each.

            int bucket = 0;

            int doubleRange = 2 * NumPixelsToProcess;

            Console.WriteLine("Getting Screen Pixels : ");

            for (int j = y1, l = 0; j < y2 && l < doubleRange; j++, l++)
            {
                //Console.WriteLine("Row " + j.ToString());

                for (int i = x1, k = 0; i < x2 && k < doubleRange; i++, k++)
                {

                    Color color = IsMock ? Color.Black : GetColorAt(i, j);

                    if (color.R <= 0.2 && color.G <= 0.2 && color.B <= 0.2)
                    {
                        bucket = l / BucketRowLength + (k / BucketColLength) * BucketColLength * 10;            // This automatically sorts out the pixel locations that are active into there respective buckets. [Check out the Unit Test for mroe info]

                        Position_SOM newPosition = new Position_SOM((k % BucketColLength), (l % BucketRowLength));

                        if ( ( k % BucketRowLength > 9 ) || ( l  % BucketRowLength > 9) )
                        {
                            throw new InvalidDataException("ProcessColorMap :: Completely B.S Logic!  Fix your fucking EQUATIONSSSSSS !!!! ");
                        }                        

                        if (BucketToData.TryGetValue(bucket, out var data) )
                        {
                            data.AddNewPostion(newPosition);
                        }
                        else
                        {
                            BucketToData.Add(bucket,  new  LocationNPositions(new List<Position_SOM> { newPosition }, i , j));
                        }
                    }

                    //Console.Write("R: " + color.R.ToString() + "G: " + color.G + "B: " + color.B + " A: " + color.A + " || ");
                }

                //Console.WriteLine("");
            }

            Console.WriteLine("Done Collecting Screen Pixels");
        }

        public void ProcessPixelData()
        {
            //Input : range = 25 , Total Number Of Pixels = 2500 , Input SDR size = 100

            SDR_SOM spatialPattern, temporalPattern;

            int index = 0;

            Console.WriteLine("Processing Pixel Data :");

            foreach (var bucket in BucketToData)
            {

                spatialPattern = GetSpatialPatternForBucket(bucket.Value.Positions);

                temporalPattern = GenerateTemporalSDR(bucket.Value.X, bucket.Value.Y);

                fomBBM[index].Fire(temporalPattern);

                fomBBM[index].Fire(spatialPattern);

                ++index;
            }

            Console.WriteLine("Done Processing Pixel Data");

        }

        public void PrintBlockVital()
        {
            foreach( var fom in fomBBM)
            {
                fom.PrintBlockStats();
            }
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

            if(isMock)
            {
                toReturn.X = x; toReturn.Y = y; 
            }
            else
            {
                toReturn = GetCurrentPointerPosition();
            }

            if (toReturn.X < LeftUpper.Item1 - Offset - 1 || toReturn.Y < LeftUpper.Item2 - Offset - 1 || toReturn.X > RightBottom.Item1 + Offset + 1|| toReturn.Y > RightBottom.Item2 + Offset + 1)
            {
                int breakpoint = 1;
            }

            if ( ( toReturn.X <= CenterCenter.Item1 - 25 && toReturn.X >= CenterCenter.Item1 + 25 ) && ( toReturn.Y >= CenterCenter.Item2 - 25  && toReturn.Y <= CenterCenter.Item2 + 25))
            {
                Console.WriteLine("Reached the Center of Image ! ReStarting the system to the begining of the image");

                if(Iterations == TOTALPARSEITERATIONS)
                {
                    // Start BackUp

                    foreach( var bbm in fomBBM)
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

            switch(CurrentDirection.ToUpper())
            {
                case "RIGHT":
                    {
                        if( toReturn.X >= ( RightUpper.Item1 + RangeIterator * NumPixelsToProcess) )
                        {
                            CurrentDirection = "DOWN";             
                            toReturn.X = RightUpper.Item1;
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
                        if ( toReturn.Y >= ( RightBottom.Item2 + RangeIterator * NumPixelsToProcess) )
                        {
                            CurrentDirection = "LEFT";
                            toReturn.X = RightBottom.Item1;
                            toReturn.Y = RightBottom.Item2;
                        }
                        else
                        {
                            toReturn.Y =  toReturn.Y + Offset;
                        }
                        break;
                    }
                case "LEFT":
                    {
                        if ( toReturn.X <= ( LeftBottom.Item1 + RangeIterator * NumPixelsToProcess) )
                        {
                            CurrentDirection = "UP";
                            toReturn.X = LeftBottom.Item1;
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
                        if (toReturn.Y <= ( LeftUpper.Item2 + RangeIterator * NumPixelsToProcess) )
                        {
                            CurrentDirection = "RIGHT";
                            toReturn.X = LeftUpper.Item1 + RangeIterator * NumPixelsToProcess;
                            toReturn.Y = LeftUpper.Item2 + RangeIterator * NumPixelsToProcess;
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

            return toReturn;
        }

        public static Color GetColorAt(int x, int y)
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
        }

        #endregion
    }
}