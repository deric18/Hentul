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
        public Color[,] ColorMap { get; private set; }
        public POINT Point { get; set; }

        private const int PixelConst = 3;

        public int range;

        public FirstOrderMemory.BehaviourManagers.BlockBehaviourManager[] somBBM { get; private set; }

        private readonly int FOMLENGTH = Convert.ToInt32(ConfigurationManager.AppSettings["FOMLENGTH"]);
        private readonly int FOMWIDTH = Convert.ToInt32(ConfigurationManager.AppSettings["FOMWIDTH"]);
        private readonly int SOM_NUM_COLUMNS = Convert.ToInt32(ConfigurationManager.AppSettings["SOMNUMCOLUMNS"]);
        private readonly int SOM_COLUMN_SIZE = Convert.ToInt32(ConfigurationManager.AppSettings["SOMCOLUMNSIZE"]);

        public ScreenGrabber(int range)
        {
            this.range = range;
            this.ColorMap = new Color[range, range];

            somBBM = new FirstOrderMemory.BehaviourManagers.BlockBehaviourManager[range];

            somBBM[0] = new FirstOrderMemory.BehaviourManagers.BlockBehaviourManager(10, 1, 0, 0);
            somBBM[1] = new FirstOrderMemory.BehaviourManagers.BlockBehaviourManager(10, 2, 0, 0);
            somBBM[2] = new FirstOrderMemory.BehaviourManagers.BlockBehaviourManager(10, 3, 0, 0);

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

            Console.WriteLine("Starting Initialization  of 3 FOM and SOM objects for 1 Pixel R G B Values : ");

            stopWatch.Start();

            somBBM[0].Init(0, 0);
            somBBM[1].Init(1, 0);
            somBBM[2].Init(2, 0);

            stopWatch.Stop();

            Console.WriteLine("Finished Init for this Instance , Total Time ELapsed : " + stopWatch.ElapsedMilliseconds.ToString());

            //Console.ReadKey();           

            Console.WriteLine("Finished Initting of all Instances, System Ready!");
        }

        public void Grab()
        {
            //send Image for Processing               

            Console.WriteLine("Grabbing cursor Position");

            Console.CursorVisible = false;

            Point = this.GetCurrentPointerPosition();

            Console.CursorVisible = true;

            Console.WriteLine("Grabbing Screen Pixels...");

            int x1 = Point.X - range < 0 ? 0 : Point.X - range;
            int y1 = Point.Y - range < 0 ? 0 : Point.Y - range;
            int x2 = Math.Abs(Point.X + range);
            int y2 = Math.Abs(Point.Y + range);

            this.PrintColorMap(x1, y1, x2, y2);
        }

        public void ProcessPixelData()
        {
            for (int i = 0; i < range; i++)
            {
                for (int j = 0; j < range; j++)
                {
                    if (ColorMap[0, 0].IsEmpty)
                    {
                        throw new InvalidOperationException();
                    }

                    byte R = ColorMap[i, j].R;
                    byte G = ColorMap[i, j].G;
                    byte B = ColorMap[i, j].B;

                    ByteEncoder[] encoder = new ByteEncoder[3];

                    encoder[0] = new ByteEncoder(100, 8);
                    encoder[1] = new ByteEncoder(100, 8);
                    encoder[2] = new ByteEncoder(100, 8);

                    Console.WriteLine("Begining Encoding");

                    encoder[0].Encode(R);
                    encoder[1].Encode(G);
                    encoder[2].Encode(B);

                    Console.WriteLine("Finsihed Encoding!!!");

                    SDR_SOM sdr1 = encoder[0].GetDenseSDR();
                    SDR_SOM sdr2 = encoder[1].GetDenseSDR();
                    SDR_SOM sdr3 = encoder[2].GetDenseSDR();

                    Console.WriteLine("Begining First Order Memory Firings");

                    somBBM[0].Fire(sdr1);
                    somBBM[1].Fire(sdr2);
                    somBBM[2].Fire(sdr3);

                    Console.WriteLine("Finsihed First Order Memory Firings");

                    SDR_SOM[] somSdrArr = new SDR_SOM[3];

                    somSdrArr[0] = somBBM[0].GetPredictedSDR();
                    somSdrArr[1] = somBBM[1].GetPredictedSDR();
                    somSdrArr[2] = somBBM[2].GetPredictedSDR();

                    SDR_SOM somSdr1 = new SDR_SOM(SOM_NUM_COLUMNS, SOM_COLUMN_SIZE, sdr1.ActiveBits, iType.SPATIAL);
                    SDR_SOM somSdr2 = new SDR_SOM(SOM_NUM_COLUMNS, SOM_COLUMN_SIZE, sdr2.ActiveBits, iType.SPATIAL);
                    SDR_SOM somSdr3 = new SDR_SOM(SOM_NUM_COLUMNS, SOM_COLUMN_SIZE, sdr3.ActiveBits, iType.SPATIAL);

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

        private void PrintColorMap(int x1, int y1, int x2, int y2)
        {
            for (int i = x1, k = 0; i < x2 && k < range; i++, k++)
            {
                //Console.WriteLine("Row " + i);

                for (int j = y1, l = 0; j < y2 && l < range; j++, l++)
                {
                    Color color = GetColorAt(i, j);
                    this.ColorMap[k, l] = color;

                    //Console.Write(color.ToString());
                }

                //Console.WriteLine();
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