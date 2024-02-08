namespace Hentul
{
    using System.Drawing;
    using System.Runtime.InteropServices;
    using System.Configuration;
    using Common;
    using SecondOrderMemory.Models;
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

        public int range;

        private FirstOrderMemory.BehaviourManagers.BlockBehaviourManager[,] fomBBM;
        private SecondOrderMemory.BehaviourManagers.BlockBehaviourManager[,] somBBM;

        private readonly int FOMLENGTH = Convert.ToInt32(ConfigurationManager.AppSettings["FOMLENGTH"]);
        private readonly int FOMWIDTH = Convert.ToInt32(ConfigurationManager.AppSettings["FOMWIDTH"]);
        private readonly int SOM_NUM_COLUMNS = Convert.ToInt32(ConfigurationManager.AppSettings["SOMNUMCOLUMNS"]);
        private readonly int SOM_COLUMN_SIZE = Convert.ToInt32(ConfigurationManager.AppSettings["SOMCOLUMNSIZE"]);

        public ScreenGrabber(int range)
        {
            this.range = range;
            this.ColorMap = new Color[range, range];

            fomBBM = new FirstOrderMemory.BehaviourManagers.BlockBehaviourManager[range, range];
            somBBM = new SecondOrderMemory.BehaviourManagers.BlockBehaviourManager[range, range];

            for ( int i=0; i< range; i++)
            {
                for(int j=0; j< range; j++)
                {                    
                    fomBBM[i, j] = FirstOrderMemory.BehaviourManagers.BlockBehaviourManager.GetBlockBehaviourManager(100, 1);
                    somBBM[i, j] = SecondOrderMemory.BehaviourManagers.BlockBehaviourManager.GetBlockBehaviourManager(10);
                }
            }
            
            //Init();            

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




        public void Grab()
        {
            //send Image for Processing               

            Console.WriteLine("Grabbing cursor Position");

            Console.CursorVisible = false;

            Point = this.GetCurrentPointerPosition();

            Console.CursorVisible = true;

            Console.WriteLine("Cursor Pos X : " + Point.X + " Y : " + Point.Y);

            Console.WriteLine("Grabbing Screen Pixels...");

            Console.WriteLine("Bit Map Values :");

            int x1 = Point.X - range < 0 ? 0 : Point.X - range;
            int y1 = Point.Y - range < 0 ? 0 : Point.Y - range;
            int x2 = Math.Abs(Point.X + range);
            int y2 = Math.Abs(Point.Y + range);

            this.PrintColorMap(x1, y1, Point.X + range, Point.Y + range);

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

                    byte A = ColorMap[i, j].A;      //Ignoring 'A' as we dont need addressable format.

                    byte R = ColorMap[i, j].R;
                    byte G = ColorMap[i, j].G;
                    byte B = ColorMap[i, j].B;

                    ByteEncoder encoder = new ByteEncoder(100, 24);

                    encoder.Encode(R, G, B);

                    SDR sdr = encoder.GetDenseSDR();

                    fomBBM[i, j].Fire(sdr);

                    SDR fomSdr = fomBBM[i, j].GetSDR();

                    SDR_SOM somSdr = new SDR_SOM(SOM_NUM_COLUMNS, SOM_COLUMN_SIZE, ConvertFomToSomPositions(fomSdr.ActiveBits), iType.SPATIAL);

                    somBBM[i, j].Fire(somSdr);
                }
            }
        }


        public void MoveCursor(int loop)
        {
            POINT p;

            p.X = this.Point.X;
            p.Y = this.Point.Y;

            IntPtr desk = GetDesktopWindow();
            IntPtr dc = GetWindowDC(desk);

            //while (loop > 0)
            //{
            p.X = p.X + 100;
            p.Y = p.Y + 100;

            ClientToScreen(dc, ref p);
            SetCursorPos(p.X, p.Y);

            loop--;
            //}
            ReleaseDC(desk, dc);

            Console.ReadLine();
        }

        #region PRIVATE METHODS

        private void Init()
        {
            for (int i = 0; i < range; i++)
            {
                for (int j = 0; j < range; j++)
                {

                    if (i == 0 && j == 0)
                    {
                        Stopwatch stopWatch = new Stopwatch();

                        Console.WriteLine("Starting Initialization  I : " + i.ToString() + "  J :" + j.ToString());

                        stopWatch.Start();

                        fomBBM[i, j].Init();
                        somBBM[i, j].Init(i, j);

                        stopWatch.Stop();

                        Console.WriteLine("Finished Init for this Instance , Total Time ELapsed : " + stopWatch.ElapsedMilliseconds.ToString());
                    }
                    else
                    {
                        fomBBM[i, j].Init();
                        //somBBM[i, j] = somBBM[0, 0].CloneBBM();
                    }
                }
            }
        }

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
                Console.WriteLine("Row " + i);

                for (int j = y1, l = 0; j < y2 && l < range; j++, l++)
                {
                    Color color = GetColorAt(i, j);
                    this.ColorMap[k, l] = color;

                    Console.Write(color.ToString());
                }

                Console.WriteLine();
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