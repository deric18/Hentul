namespace SecondOrderMemoryUnitTest
{
    using Common;
    using SecondOrderMemory.BehaviourManagers;
    using SecondOrderMemory.Models;

    [TestClass]
    public class PredictorTests
    {
        BlockBehaviourManager bbManager;

        [TestInitialize]
        public void SetUp()
        {
            bbManager = new BlockBehaviourManager(10);

            bbManager.Init();

        }        

        [TestMethod]
        public void TestSequenceMemory()
        {
            // Project ABC Pattern 60 times and test C is predicted after B 31st time.

            SDR_SOM patternA = new SDR_SOM(10, 10, new List<Position_SOM> { new Position_SOM(0, 1, 1) }, iType.SPATIAL); //TestUtils.GetSDRFromPattern('A');
            SDR_SOM patternB = new SDR_SOM(10, 10, new List<Position_SOM> { new Position_SOM(3, 1, 1) }, iType.SPATIAL); //TestUtils.GetSDRFromPattern('B');
            SDR_SOM patternC = new SDR_SOM(10, 10, new List<Position_SOM> { new Position_SOM(5, 5, 1) }, iType.SPATIAL); //TestUtils.GetSDRFromPattern('C');
            SDR_SOM predictedSDR;

            int repCount = 0;

            while (repCount != 60)
            {                              
                
                bbManager.Fire(patternA);       //Fire A , Predict B NOT C

                if(repCount > 2)
                {
                    predictedSDR = bbManager.GetPredictedSDR();
                    
                    Assert.IsTrue(predictedSDR.IsUnionTo(patternB));
                    Assert.IsFalse(predictedSDR.IsUnionTo(patternC));
                }
                if(repCount == 8)
                {
                    int bp2 = 1;
                }

                
                if (repCount == 3)
                {
                    bbManager.Fire(patternB);       //Fire B , Predict C NOT A
                }
                else
                {
                    bbManager.Fire(patternB);       //Fire B , Predict C NOT A
                }

                if (repCount > 2)
                {
                    predictedSDR = bbManager.GetPredictedSDR();

                    Assert.IsTrue(predictedSDR.IsUnionTo(patternC));

                    Console.WriteLine("REPCOUNT : " + repCount.ToString());

                    bool b = predictedSDR.IsUnionTo(patternA, new List<Position_SOM>() { new Position_SOM(0,1,4)});

                    if (b)
                    {
                        predictedSDR.IsUnionTo(patternA, new List<Position_SOM>() { new Position_SOM(0, 1, 4) });
                    }

                    Assert.IsFalse(b);
                }

                if (repCount == 2)
                {
                    bbManager.Fire(patternC);      
                }
                else
                {
                    bbManager.Fire(patternC);       //Fire C , Predict A NOT B
                }

                predictedSDR = bbManager.GetPredictedSDR();

                if (repCount > 2)
                {
                    if(repCount >= 3)
                    {
                        bool b = predictedSDR.IsUnionTo(patternA);
                        bool c = predictedSDR.IsUnionTo(patternB);

                        if(b == false || c == true)
                        {
                            int bp = 1;
                        }
                    }

                    Assert.IsFalse(predictedSDR.IsUnionTo(patternB));
                    Assert.IsTrue(predictedSDR.IsUnionTo(patternA));
                }

                repCount++;
            }            
        }

        [TestMethod]
        public void DetectorTest1()
        {
            //Create a a specific pattern and check how long it takes for the network to detect it.


        }

        
        [TestMethod]
        public void HighVoltagePredictedNeuronGetsPickedForFiring()
        {
            // HighVoltagePredictedNeuronGetsPickedForFiring from a Column

            Position_SOM position1 = new Position_SOM(3, 2, 5);

            Position_SOM position2 = new Position_SOM(3, 2, 6);

            Position_SOM position3 = new Position_SOM(3, 2, 7);

            Position_SOM apicalSOM = new Position_SOM(3, 2, 7);

            Neuron neuron1 = bbManager.Columns[position1.X, position1.Y].Neurons[position1.Z];

            Neuron neuron2 = bbManager.Columns[position2.X, position2.Y].Neurons[position2.Z];

            Neuron neuron3 = bbManager.Columns[position3.X, position3.Y].Neurons[position3.Z];

            Assert.AreEqual(neuron1.CurrentState, NeuronState.RESTING);

            Assert.AreEqual(neuron2.CurrentState, NeuronState.RESTING);

            Assert.AreEqual(neuron3.CurrentState, NeuronState.RESTING);

            SDR_SOM apicalSdr = new SDR_SOM(10, 10, new List<Position_SOM>() { apicalSOM }, iType.APICAL);

            SDR_SOM sdr_SOM = new SDR_SOM(10, 10, new List<Position_SOM>() { position1 }, iType.SPATIAL);

            bbManager.Fire(apicalSdr, false, true);

            neuron3.ProcessVoltage(10);

            bbManager.Fire(sdr_SOM, true, false);

            //Assert.AreEqual(NeuronState.RESTING, neuron1.CurrentState);

            //Assert.AreEqual(NeuronState.PREDICTED, neuron2.CurrentState);

            Assert.AreEqual(NeuronState.FIRING, neuron3.CurrentState);
        }

        public void DetectorTest()
        {

        }

        public void MemoryTest()
        {

        }

        public void FirAndWireWorksTest()
        {
            // Todo: Check if neurons that all fired together are connected to each other or not and connect them!  
        }        
    }
}
