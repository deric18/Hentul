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

        public int Range { get; private set; }

        public int NumBuckets { get; private set; }

        public int BuketColRowLength { get; private set; }
        
        public Dictionary<int, Position_SOM> TemporalPositionsForBuckets { get; private set; }

        public Dictionary<int, List<Position_SOM>>  BucketToData { get; private set; }

        public FirstOrderMemory.BehaviourManagers.BlockBehaviourManager[] fomBBM { get; private set; }

        private readonly int FOMLENGTH = Convert.ToInt32(ConfigurationManager.AppSettings["FOMLENGTH"]);
        private readonly int FOMWIDTH = Convert.ToInt32(ConfigurationManager.AppSettings["FOMWIDTH"]);
        private readonly int SOM_NUM_COLUMNS = Convert.ToInt32(ConfigurationManager.AppSettings["SOMNUMCOLUMNS"]);
        private readonly int SOM_COLUMN_SIZE = Convert.ToInt32(ConfigurationManager.AppSettings["SOMCOLUMNSIZE"]);

        public ScreenGrabber(int range )
        {
            //Todo : Project shape data of the input image to one region and project colour data of the image to another region.            

            Range = range;

            BuketColRowLength = 5;

            if ( ((float)( Range * Range ) / ( BuketColRowLength * BuketColRowLength ) % 1 )!= 0)
            {
                throw new InvalidDataException("Number Of Pixels should always be a factor of BucketColLength : NumPixels : "+ Range.ToString() + "  NumPixelsPerBucket" +  BuketColRowLength.ToString());
            }

            NumBuckets = ( Range * Range ) /  ( BuketColRowLength * BuketColRowLength );

            BucketToData = new Dictionary<int, List<Position_SOM>>();

            fomBBM = new FirstOrderMemory.BehaviourManagers.BlockBehaviourManager[NumBuckets];

            TemporalPositionsForBuckets = new Dictionary<int, Position_SOM>();

            for (int i = 0; i < NumBuckets; i++)
            {
                fomBBM[i] = new FirstOrderMemory.BehaviourManagers.BlockBehaviourManager(10, i, 0, 0);
            }

            Init();
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

            Console.WriteLine("Total Pixels being collected for a range of " + Range.ToString() + " \nTotal Number of Pixels :" + (Range * Range * 4).ToString() + "\nTotal BBMs Created :" + NumBuckets.ToString());
            
        }

        public void Grab()
        {
            //send Image for Processing                           

            Console.CursorVisible = false;

            Point = this.GetCurrentPointerPosition();

            Console.CursorVisible = true;

            Console.WriteLine("Grabbing Screen Pixels...");

            int x1 = Point.X - Range < 0 ? 0 : Point.X - Range;
            int y1 = Point.Y - Range < 0 ? 0 : Point.Y - Range;
            int x2 = Math.Abs(Point.X + Range);
            int y2 = Math.Abs(Point.Y + Range);

            this.ProcessColorMap(x1, y1, x2, y2);
        }

        public void ProcessColorMap(int x1, int y1, int x2, int y2)
        {

            int bucket = 0;

            //ProcessTemporalCoordinates(x1, y1, x2, y2);

            for (int i = x1, k = 0; i < x2 && k < 2 * Range; i++, k++)
            {
                Console.WriteLine("Row " + i);

                for (int j = y1, l = 0; j < y2 && l < 2 * Range; j++, l++)
                {
                    Color color = GetColorAt(i, j);

                    if (color.R == 0 || color.G == 0 || color.B == 0)
                    {
                        bucket = ((j - y1) / BuketColRowLength);

                        Position_SOM newPosition = new Position_SOM(k, l);

                        if (BucketToData.TryGetValue(bucket, out var data))
                        {
                            data.Add(newPosition);
                        }
                        else
                        {
                            BucketToData.Add(bucket, new List<Position_SOM>() { newPosition });
                        }
                    }

                    Console.Write("R: " + color.R.ToString() + "G: " + color.G + "B: " + color.B + " A: " + color.A + " || ");
                }
                Console.WriteLine();
            }
        }

        public void ProcessPixelData()
        {
            //Need to bucketize the screen space to buckets as per bucket size.
            //Lock Buckets to there own Location Coordinates
            //Triage and trigger each bucket to its own Block with its Location Coordinates. 

            SDR_SOM spatialPattern, temporalPattern;            

            foreach(var kvp in BucketToData)
            {
                spatialPattern = new SDR_SOM(10, 10, kvp.Value, iType.SPATIAL);


                //Todo: Fix Temporal Input Mapping , Temporal Line Arrays do not work the same as mosue corodinates on the screen
                temporalPattern = new SDR_SOM(10, 10, new List<Position_SOM>() { kvp.Value[0] }, iType.TEMPORAL);

                //Todo: Generate Location Coordinates through grid cell based on Pixel location
                //temporalPattern

                fomBBM[kvp.Key].Fire(temporalPattern);

                fomBBM[kvp.Key].Fire(spatialPattern);
                
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