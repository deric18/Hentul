using Hentul;

Console.WriteLine("Initializing ...");

ScreenGrabber screenGrabber = new ScreenGrabber(25);

Console.WriteLine("System Initialized Finally! Press any Key to start Neural Engine :");

int x1 = 0, y1 = 0, x2 = 0, y2 = 0;

Tuple<int, int, int, int> tuple;

//Console.ReadKey();

screenGrabber.SetMousetotartingPoint();

while (true)
{

    screenGrabber.GrabNProcess();                

    Console.WriteLine("Switching to Next Image");
        
    if(screenGrabber.SwitchImage() == false)
    {
        Console.WriteLine("Done Processing all the Images!!!! Take a fucking bow Man!!! Proud of you, You deserve a break!!!!!!!!");

        Console.Read();
    }         

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
