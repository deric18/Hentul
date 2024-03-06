namespace SecondOrderMemoryUnitTest
{
    using Common;
    using SecondOrderMemory.BehaviourManagers;
    using SecondOrderMemory.Models;

    [TestClass]
    public class DetectorCircuitTest        
    {
        BlockBehaviourManager bbManager;

        [TestInitialize]
        public void SetUp()
        {
            bbManager = new BlockBehaviourManager(10);

            bbManager.Init();

        }

        [TestMethod]
        public void DetectorTest1()
        {
            //Create a a specific pattern and check how long it takes for the network to detect it.
            //Create a a specific pattern and check how long it takes for the network to detect it.
            List<SDR_SOM> sDR_SOMs = new List<SDR_SOM>();
            SDR_SOM predictedSDR;
            int patternSize = 2;
            int noiseSize = 4;
            int cycleSize = patternSize + noiseSize;
            int numCount = 0;
            int learningCurveCount = 3;
            
            sDR_SOMs.AddRange(TestUtils.GetSpecificPatternAmoungNoise(patternSize, noiseSize));

            foreach (var item in sDR_SOMs)
            {
                if (numCount % noiseSize == 1 && (numCount / noiseSize) > 0 && (numCount / cycleSize) >= learningCurveCount)
                {
                    predictedSDR = bbManager.GetPredictedSDR();
                    Assert.IsTrue(predictedSDR.IsUnionTo(sDR_SOMs[cycleSize - 1]));
                }

                bbManager.Fire(item);

                numCount++;


            }
        }

        public void DetectorTest2()
        {

        }


        public void FirAndWireWorksTest()
        {
            // Todo: Check if neurons that all fired together are connected to each other or not and connect them!  
        }        
    }
}
