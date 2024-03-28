using Hentul;

Console.WriteLine("Initializing ...");

ScreenGrabber screenGrabber = new ScreenGrabber(25);

Console.WriteLine("System Initialized Finally! Press any Key to start Neural Engine :");

//Console.ReadKey();

screenGrabber.SetMousetotartingPoint();

while (true)
{

    screenGrabber.Grab();

    Console.WriteLine("Processing Pixel Data ...");

    screenGrabber.ProcessPixelData();

    Console.WriteLine("Done Processing Pixel Data ...");

    //Console.WriteLine("Pres any key for moving cursor and processing next set of pixels");

    //Console.ReadKey();

    Console.WriteLine("Movign Cursor by 25 pixels");
        
    screenGrabber.MoveCursor();

    Thread.Sleep(5000);

    #region Experimental Code
    //screenGrabber.MoveCursor(x1, y1);

    //Console.WriteLine("X1 :" +  x1.ToString() + " Y1: " + y1.ToString());

    //Thread.Sleep(1000);

    //screenGrabber.MoveCursor(x1 + 50 , y1);

    //Console.WriteLine("X1 :" + ( x1 + 50).ToString() + " Y1 " + ( y1).ToString());

    //Thread.Sleep(1000);

    //screenGrabber.MoveCursor(x2, y2);

    //Console.WriteLine("X2 :" + x2.ToString() + " Y2: " + y2.ToString());

    //Thread.Sleep(1000);

    //screenGrabber.MoveCursor(x2 - 50, y2 );

    //Console.WriteLine("X2 - 50 :" + (x2 - 50).ToString() + " Y2: " + y2.ToString());

    //Thread.Sleep(50000);

    #endregion
}
