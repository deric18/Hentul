using Hentul;

Console.WriteLine("Initializing ...");

ScreenGrabber screenGrabber = new ScreenGrabber(10);

Console.WriteLine("System Initialized Finally! Press any Key to start Neural Engine :");

Console.ReadKey();

while (true)
{

    screenGrabber.Grab();

    Console.WriteLine("Processing Pixel Data ...");

    screenGrabber.ProcessPixelData();

    Console.WriteLine("Done Processing Pixel Data ...");

    Console.WriteLine("Pres any key for moving cursor and processing next set of pixels");

    //Console.ReadKey();

    Console.WriteLine("Movign Cursor by 2 pixels");

    screenGrabber.MoveCursor(2);

    Thread.Sleep(1000);
}
