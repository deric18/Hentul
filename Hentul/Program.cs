using Hentul;

Console.WriteLine("Initializing ...");

ScreenGrabber screenGrabber = new ScreenGrabber(30);

Console.WriteLine("System Initialized Finally! Press any Key to start Neural Engine :");

Console.ReadKey();

while (true)
{

    screenGrabber.Grab();

    Console.WriteLine("Processing Pixel Data ...");
    
    screenGrabber.ProcessPixelData();

    Console.WriteLine("Done Processing Pixel Data ...");

    Console.WriteLine("Movign Cursor");

    screenGrabber.MoveCursor(2);

    Thread.Sleep(1000);
}
