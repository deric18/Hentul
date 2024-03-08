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
        public void DetectorTestCT()
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
            int iterations = 3;
            int minValue = 0, maxValue = 9;
            
            sDR_SOMs.AddRange(TestUtils.GetSpecificPatternAmoungNoise(iterations, patternSize, noiseSize, minValue, maxValue));

            foreach (var item in sDR_SOMs)
            {
                if (numCount % noiseSize == 1 )                 // Only check Assert for pattern2
                    if((numCount / noiseSize) > 0 )             // Should avoid first iteration
                            if((numCount / cycleSize) >= learningCurveCount)        //ShouldCheckAfterLearningCurveCount 
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
