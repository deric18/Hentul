using System.Runtime.InteropServices;

namespace Hentul
{
    using System.Drawing;
    using Common;
    using Encoders;
    using FBBM = FirstOrderMemory.BehaviourManagers.BlockBehaviourManagerFOM;
    using SBBM = SecondOrderMemory.Models.BlockBehaviourManagerSOM;

    public class VisionStreamProcessor
    {

        #region Constructor & Variables

        public int numPixelsProcessedPerBBM;

        private static readonly string baseDir = AppContext.BaseDirectory;
        public int NumBBMNeededV { get; private set; }

        public int BlockSize;                        
        

        public int Range { get; private set; }

        int X, NumColumns, Z;

        bool IsMock;

        public ulong CycleNum;

        public LogMode LogMode { get; private set; }

        // Multiple encoders for different scales
        public PixelEncoder pEncoderV1 { get; private set; }                


        public Bitmap bmpV1 { get; private set; }        

        public string logfilename { get; private set; }        

        public VisionStreamProcessor(int range, int numColumns, int x, LogMode logMode, bool isMock, bool shouldInit)
        {
            this.X = x;

            this.NumColumns = numColumns;

            IsMock = isMock;

            Z = 5;

            CycleNum = 0;

            if (range != 10)
            {
                throw new InvalidOperationException("Invalid Operation !");
            }

            Range = range;
           
            BlockSize = (2 * range) * (2 * range);
            NumBBMNeededV = 100;
            pEncoderV1 = new PixelEncoder(100, 400);
            bmpV1 = new Bitmap(range + range, range + range);            

            numPixelsProcessedPerBBM = 4;
            logfilename = Path.GetFullPath(Path.Combine(baseDir, @"..\..\..\..\..\Hentul\Logs\Hentul-Orchestrator.log"));

            if (shouldInit)
            {                

                Console.WriteLine("V1 - Total Number of Pixels: " + (Range * Range * 4).ToString());
                Console.WriteLine("V1 - Total First Order BBMs Created: " + NumBBMNeededV.ToString());
                Console.WriteLine("V2/V3 - Available on demand (not yet initialized)");
            }

            LogMode = logMode;
        }
       

        #endregion


        public void ProcessInput(Bitmap greyScalebmp, ulong cycle)
        {
            CycleNum = cycle;
                       

            // Always process V1
            ProcessV1(greyScalebmp, cycle);
                       

            Clean();
        }

        private void ProcessV1(Bitmap greyScalebmp, ulong cycle)
        {            
            pEncoderV1.ParseBitmap(greyScalebmp);            
        }               
        
        private Bitmap ApplySubsampling(Bitmap source, int targetWidth, int targetHeight)
        {
            var result = new Bitmap(targetWidth, targetHeight);
            var stepX = (double)source.Width / targetWidth;
            var stepY = (double)source.Height / targetHeight;

            using (var g = Graphics.FromImage(result))
            {
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
                g.DrawImage(source, 0, 0, targetWidth, targetHeight);
            }

            return result;
        }

        private Bitmap ApplyPixelSubsampling(Bitmap source, int step)
        {
            var result = new Bitmap(source.Width, source.Height);

            for (int y = 0; y < source.Height; y += step)
            {
                for (int x = 0; x < source.Width; x += step)
                {
                    if (x < source.Width && y < source.Height)
                    {
                        var color = source.GetPixel(x, y);
                        // Fill the step area with the same color
                        for (int dy = 0; dy < step && y + dy < source.Height; dy++)
                        {
                            for (int dx = 0; dx < step && x + dx < source.Width; dx++)
                            {
                                result.SetPixel(x + dx, y + dy, color);
                            }
                        }
                    }
                }
            }

            return result;
        }
                                          

        internal void SetNetworkModeToPrediction()
        {
            
        }

        public void LearnNewObject(string objectName)
        {
            
        }        

        internal void Clean()
        {
           
        }


        internal void BeginTraining(string objectLabel)
        {
            
        }
    }
}