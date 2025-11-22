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

            numPixelsProcessedPerBBM = 0;// needs to be computed!

            logfilename = Path.Combine(baseDir, @"..\..\..\..\..\Hentul\Logs\Hentul-Orchestrator.log");

            if (shouldInit)
            {                
                
                Console.WriteLine("V1 - Total First Order BBMs Created: " + NumBBMNeededV.ToString());
                Console.WriteLine("V2/V3 - Available on demand (not yet initialized)");

                SBBM.Initialize(X, NumColumns, Z, LayerType.Layer_3B, logMode);
                SomBBM = SBBM.Instance;
            }

            LogMode = logMode;
        }
       

        #endregion


        public void Train(Bitmap greyScalebmp, ulong cycle, string objectLabel)
        {
            CycleNum = cycle;

            var sdr = pEncoder.EncodeBitmap(greyScalebmp);

            SomBBM.Fire(sdr,cycle);
                       

            Clean();
        }
                                                              

        internal void SetNetworkModeToPrediction()
        {
            
        }

          

        internal void Clean()
        {
           
        }

        internal void SetUpObjectLabelOnce(string objectLabel)
        {
            if (!SomBBM.SetUpNewObjectLabel(objectLabel))
                throw new InvalidOperationException("Object Label Could not be set up!");
        }
    }
}