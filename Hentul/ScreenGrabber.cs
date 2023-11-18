using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;

public struct POINT
{
    public int X;
    public int Y;
}
public class ScreenGrabber
{
    public Color[,] ColorMap { get; private set; }
    public int range;

    public ScreenGrabber(int range)
    {
        this.range = range;
        this.ColorMap = new Color[range,range];
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


    public static Color GetColorAt(int x, int y)
    {
        IntPtr desk = GetDesktopWindow();
        IntPtr dc = GetWindowDC(desk);
        int a = (int)GetPixel(dc, x, y);
        ReleaseDC(desk, dc);
        return Color.FromArgb(255, (a >> 0) & 0xff, (a >> 8) & 0xff, (a >> 16) & 0xff);
    }   

    private void PrintColorMap(int x1, int y1, int x2, int y2)
    {
        for (int i = x1, k=0; i < x2 && k < 10; i++, k++)
        {
            Console.WriteLine("Row " + i);

            for (int j = y1, l=0; j < y2 && l < 10; j++,l++)
            {                
                Color color = GetColorAt(i, j);
                this.ColorMap[k,l] = color;
                Console.Write(color.A + "-" + color.R + "-" + color.G + "-" + color.B + "  ");                
            }
            
            Console.WriteLine();
        }
    }

    private POINT  GetCurrentPointerPosition()
    {
        POINT point;
        if(GetCursorPos(out point)) 
        {
            Console.Clear();
            Console.WriteLine(point.X.ToString() + " " + point.Y.ToString());            
        }
        return point;
    }

    public void Grab()
    {       
        Console.ReadKey();

        Console.WriteLine("Grabbing cursor Position");

        Console.CursorVisible = false;       
        
        POINT point =  this.GetCurrentPointerPosition();
        
        Console.CursorVisible = true;

        Console.ReadKey();

        Console.WriteLine("Grabbing Screen Pixels...");


        Console.WriteLine("Bit Map Values :");
        
        this.PrintColorMap(point.X - range, point.Y - range, point.X + range, point.Y + range);

        //send Image for Processing



        //loop again
       
    }
}