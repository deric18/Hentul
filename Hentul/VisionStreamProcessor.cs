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
        public PixelEncoder pEncoder { get; private set; }                

        public SBBM SomBBM { get; private set; }        

        public string logfilename { get; private set; }                

        public VisionStreamProcessor(LogMode logMode, bool isMock, bool shouldInit)
        {
            this.X = 3_000_000;

            NumColumns = 10;            

            IsMock = isMock;

            Z = 5;

            CycleNum = 0;            
                       
            NumBBMNeededV = 300_000;

            pEncoder = new PixelEncoder(X, NumColumns);            

            SBBM.Initialize(X, NumColumns, Z, LayerType.Layer_3B, logMode);

            SomBBM = SBBM.Instance;

            numPixelsProcessedPerBBM =  // needs to be computed!

            logfilename = Path.Combine(baseDir, @"..\..\..\..\..\Hentul\Logs\Hentul-Orchestrator.log");

            if (shouldInit)
            {                

                Console.WriteLine("V1 - Total Number of Pixels: " + (Range * Range * 4).ToString());
                Console.WriteLine("V1 - Total First Order BBMs Created: " + NumBBMNeededV.ToString());
                Console.WriteLine("V2/V3 - Available on demand (not yet initialized)");
            }

            LogMode = logMode;
        }
       

        #endregion


        public void Train(Bitmap greyScalebmp, ulong cycle)
        {
            CycleNum = cycle;

            var sdr = pEncoder.EncodeBitmap(greyScalebmp);

            
                       

            Clean();
        }
                                                              

        internal void SetNetworkModeToPrediction()
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