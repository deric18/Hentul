// See https://aka.ms/new-console-template for more information
using ExperimentalConsole;


Grabber gb = new Grabber();

while (true)
{
    gb.GetCurrentPointerPosition();
    Thread.Sleep(1000);
    
}
