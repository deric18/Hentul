namespace Hentul
{
    using System.Drawing;
    using System.Runtime.InteropServices;
    using System.Configuration;
    using Common;
    using FirstOrderMemory.Models;
    using System.Diagnostics;

    public struct POINT
    {
        public int X;
        public int Y;
    }

    public class ScreenGrabber
    {
        public bool[,] bitMap { get; private set; }
        
        public POINT Point { get; set; }

        private const int ByteSize = 8;

        public int NumPixels { get; private set; }

        public int NumBuckets { get; private set; }

        public int BuketColRowLength { get; private set; }

        public Dictionary<int, List<Position_SOM>>  BucketToData { get; private set; }

        public FirstOrderMemory.BehaviourManagers.BlockBehaviourManager[] somBBM { get; private set; }

        private readonly int FOMLENGTH = Convert.ToInt32(ConfigurationManager.AppSettings["FOMLENGTH"]);
        private readonly int FOMWIDTH = Convert.ToInt32(ConfigurationManager.AppSettings["FOMWIDTH"]);
        private readonly int SOM_NUM_COLUMNS = Convert.ToInt32(ConfigurationManager.AppSettings["SOMNUMCOLUMNS"]);
        private readonly int SOM_COLUMN_SIZE = Convert.ToInt32(ConfigurationManager.AppSettings["SOMCOLUMNSIZE"]);

        public ScreenGrabber(int numPixels)
        {
            //Todo : Project shape data of the input image to one region and project colour data of the image to another region.            

            NumPixels = numPixels;

            BuketColRowLength = 5;

            if (NumPixels % BuketColRowLength == 0)
            {
                throw new InvalidDataException("Number Of Pixels should always be a factor of BucketColLength : NumPixels : "+ NumPixels.ToString() + "  NumPixelsPerBucket" +  BuketColRowLength.ToString());
            }

            NumBuckets = ((NumPixels * NumPixels) /  ( BuketColRowLength * BuketColRowLength ));           

            BucketToData = new Dictionary<int, List<Position_SOM>>();

            somBBM = new FirstOrderMemory.BehaviourManagers.BlockBehaviourManager[NumPixels];

            for(int i = 0; i < NumPixels * NumPixels; i++)
            {
                somBBM[i] = new FirstOrderMemory.BehaviourManagers.BlockBehaviourManager(10, i, 0, 0);
            }

            Init();
        }

        //public ScreenGrabber(int range, bool DontUse)
        //{
        //    this.range = range;
        //    this.ColorMap = new Color[range, range];

        //    fomBBM = new ZeroOrderMemory.BehaviourManagers.BlockBehaviourManager[PixelConst];
        //    somBBM = new FirstOrderMemory.BehaviourManagers.BlockBehaviourManager[PixelConst];


        //    fomBBM[0] = ZeroOrderMemory.BehaviourManagers.BlockBehaviourManager.GetBlockBehaviourManager(100, 1);
        //    fomBBM[1] = ZeroOrderMemory.BehaviourManagers.BlockBehaviourManager.GetBlockBehaviourManager(100, 1);
        //    fomBBM[2] = ZeroOrderMemory.BehaviourManagers.BlockBehaviourManager.GetBlockBehaviourManager(100, 1);


        //    somBBM[0, 0, 0] = new FirstOrderMemory.BehaviourManagers.BlockBehaviourManager(0, 0, 0, 10);
        //    somBBM[1, 0, 0] = new FirstOrderMemory.BehaviourManagers.BlockBehaviourManager(1, 0, 0, 10);
        //    somBBM[2, 0, 0] = new FirstOrderMemory.BehaviourManagers.BlockBehaviourManager(2, 0, 0, 10);

        //    Init();            
        //}

        private void Init()
        {
            Stopwatch stopWatch = new Stopwatch();

            Console.WriteLine("Starting Initialization  of SOM objects : ");

            stopWatch.Start();

            for (int i = 0; i < NumPixels * NumPixels; i++)
            {
                somBBM[i].Init(i);                
            }

            stopWatch.Stop();

            Console.WriteLine("Finished Init for this Instance , Total Time ELapsed : " + stopWatch.ElapsedMilliseconds.ToString());

            Console.WriteLine("Finished Initting of all Instances, System Ready!");

            //Console.ReadKey();                       
        }

        public void Grab()
        {
            //send Image for Processing               

            Console.WriteLine("Grabbing cursor Position");

            Console.CursorVisible = false;

            Point = this.GetCurrentPointerPosition();

            Console.CursorVisible = true;

            Console.WriteLine("Grabbing Screen Pixels...");

            int x1 = Point.X - NumPixels < 0 ? 0 : Point.X - NumPixels;
            int y1 = Point.Y - NumPixels < 0 ? 0 : Point.Y - NumPixels;
            int x2 = Math.Abs(Point.X + NumPixels);
            int y2 = Math.Abs(Point.Y + NumPixels);

            this.ProcessColorMap(x1, y1, x2, y2);
        }

        public void ProcessPixelData()
        {
            //Need to bucketize the screen space to buckets as per bucket size.
            //Lock Buckets to there own Location Coordinates
            //Triage and trigger each bucket to its own Block with its Location Coordinates. 

            for (int i = 0; i < NumPixels; i++)
            {
                for (int j = 0; j < NumPixels; j++)
                {                                                            

                    ByteEncoder encoder = new ByteEncoder(100, 8);                                        

                    Console.WriteLine("Begining Encoding");
                                       
                    Console.WriteLine("Finsihed Encoding!!!");

                    SDR_SOM sdr1 = encoder.GetSparseSDR();

                    Console.WriteLine("Begining First Order Memory Firings");

                    somBBM[0].Fire(sdr1);                    

                    Console.WriteLine("Finsihed First Order Memory Firings");

                    SDR_SOM somSdrArr = new SDR_SOM(10, 10, new List<Position_SOM>() { });

                    somSdrArr = somBBM[ 0].GetPredictedSDR();                    

                    SDR_SOM somSdr1 = new SDR_SOM(SOM_NUM_COLUMNS, SOM_COLUMN_SIZE, sdr1.ActiveBits, iType.SPATIAL);
                    
                    Console.WriteLine("Prepping Temporal Location SDRs for SOM");

                    var temporalSDR = GenerateTemporalSDR();

                    Console.WriteLine("Begining SOM Firings :");
                    
                    Console.WriteLine("Finished Second Order Memory Firings");
                }
            }
        }


        public Tuple<SDR, SDR> GenerateTemporalSDR()
        {
            ByteEncoder encoder = new ByteEncoder(100, 8);

            encoder.Encode((byte)Point.X);
            SDR Xsdr = encoder.GetDenseSDR(iType.TEMPORAL);
            encoder.Encode((byte)Point.Y);
            SDR Ysdr = encoder.GetDenseSDR(iType.TEMPORAL);

            return new Tuple<SDR, SDR>(Xsdr, Ysdr);

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

        public void MoveCursor(int offset)
        {
            POINT p;

            p.X = this.Point.X;
            p.Y = this.Point.Y;

            IntPtr desk = GetDesktopWindow();
            IntPtr dc = GetWindowDC(desk);

            p.X = p.X + offset;
            p.Y = p.Y + offset;

            ClientToScreen(dc, ref p);
            SetCursorPos(p.X, p.Y);

            ReleaseDC(desk, dc);

        }

        #region PRIVATE METHODS        

        private static Color GetColorAt(int x, int y)
        {
            IntPtr desk = GetDesktopWindow();
            IntPtr dc = GetWindowDC(desk);
            int a = (int)GetPixel(dc, x, y);
            ReleaseDC(desk, dc);
            return Color.FromArgb(255, (a >> 0) & 0xff, (a >> 8) & 0xff, (a >> 16) & 0xff);
        }

        private void ProcessColorMap(int x1, int y1, int x2, int y2)
        {

            int bucket = 0;            

            for (int i = x1, k = 0; i < x2 && k < NumPixels; i++, k++)
            {
                //Console.WriteLine("Row " + i);

                for (int j = y1, l = 0; j < y2 && l < NumPixels; j++, l++)
                {
                    Color color = GetColorAt(i, j);

                    if(color.A > 0.5)
                    {                         
                        bucket = ( ( j ) / BuketColRowLength);

                        Position_SOM newPosition = new Position_SOM(i, j);

                        if(BucketToData.TryGetValue(bucket, out var data))
                        {
                            data.Add(newPosition);
                        }
                        else
                        {
                            BucketToData.Add(bucket, new List<Position_SOM> () { newPosition });
                        }
                    }
                    //Console.Write(color.ToString());
                }
                //Console.WriteLine();
            }
        }

        private void PopulateDataBytes(bool[,] barr)
        {
            string toReturn = String.Empty;

            if (barr.GetUpperBound(2) != 8)
            {
                throw new InvalidOperationException("Bool Array Value should always be equal to byte Size for conversion, Check you SHitty Code! DumbFuck!!!!");
            }

            for (int i = 0; i < barr.GetUpperBound(1); i++)
            {
                for (int j = 0; j < barr.GetUpperBound(2); j++)
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