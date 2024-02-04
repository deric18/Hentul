using System;
using Hentul;

Console.WriteLine("Initializing ...");

ScreenGrabber screenGrabber = new ScreenGrabber(2);

Console.WriteLine("System Initialized Finally! Press any Key to Grab Cursor Pixels 10 X 10");

Console.ReadKey();

screenGrabber.Grab();

Console.WriteLine("Processing Pixel Data ...");

screenGrabber.ProcessPixelData();

