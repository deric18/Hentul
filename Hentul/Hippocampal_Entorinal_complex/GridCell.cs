/// Author : Deric Pinto

namespace Hentul
{
    using Common;
    using System.Diagnostics.Eventing.Reader;
    using System.Numerics;
    using System.Drawing.Imaging;
    using System.Reflection.Metadata.Ecma335;
    using System.Runtime.InteropServices;
    
    public class GridCell
    {

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


        public POINT Point { get; set; }

        public Position[] BoundaryCells { get; set; }

        public Position PlaceCell { get; private set; }

        Dictionary<string, BaseObject> Objects { get; set; }

        public GridCell(Position position) 
        { 
            Objects = new Dictionary<string, BaseObject>();
            PlaceCell = position;
            BoundaryCells = new Position[4];
        }

        public void UpdateCurrentPosition(Position position)
        {
            PlaceCell = position;
        }

        public void UpdateGridCellMap()
        {

        }

        internal void AddKnownObject()
        {

        }

        public void AddUnknownObject()
        {

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

    }
}