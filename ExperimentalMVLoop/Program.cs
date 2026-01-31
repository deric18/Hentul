using Common;
using Hentul;
using Hentul.Encoders;



Console.WriteLine("Enter direction");
var dir = Console.ReadKey();

SDR_SOM sdr = null;
MotorEncoder mEncoder = new MotorEncoder();
MotorStreamProcessor MPStream = new();
ulong cycleNum = 0;

switch (dir.KeyChar)
{    
    case 'W':   //Up
    case 'w':
        {
            sdr = mEncoder.ToSDR(MouseMove.Up);
            break;
        }
    case 'S':   //Down
    case 's':
        {
            sdr = mEncoder.ToSDR(MouseMove.Down);
            break;
        }
    case 'A':   //Left
    case 'a':
        {
            sdr = mEncoder.ToSDR(MouseMove.Left);
            break;
        }
    case 'D':   //Right
    case 'd':
        {
            sdr = mEncoder.ToSDR(MouseMove.Down);
            break;
        }
    default:
        {
            // Handle other input
            break;
        }
}


MPStream.ProcessSDR(sdr, cycleNum);


