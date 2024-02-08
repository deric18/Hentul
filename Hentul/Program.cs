using System;
using Hentul;

Console.WriteLine("Initializing ...");

ScreenGrabber screenGrabber = new ScreenGrabber(4);

Console.WriteLine("System Initialized Finally! Press any Key to Grab Cursor Pixels 10 X 10");

Console.ReadKey();

while (true)
{

    screenGrabber.Grab();

    Console.WriteLine("Processing Pixel Data ...");

    //screenGrabber.ProcessPixelData();

    Console.WriteLine("Done Processing Pixel Data ...");

    Console.WriteLine("Movign Cursor");

    screenGrabber.MoveCursor(100);


}
