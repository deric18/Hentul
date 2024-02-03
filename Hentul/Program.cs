using System;
using Hentul;

Console.WriteLine("Program Started");

ScreenGrabber screenGrabber = new ScreenGrabber(10);

screenGrabber.Grab();

Console.WriteLine("Processing Pixel Data ...");

screenGrabber.ProcessPixelData();

