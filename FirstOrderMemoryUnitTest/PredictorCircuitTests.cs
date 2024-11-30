namespace FirstOrderMemoryUnitTest
{
    using Common;
    using FirstOrderMemory.BehaviourManagers;
    using FirstOrderMemory.Models;

    [TestClass]
    public class PredictorTests
    {
        BlockBehaviourManager bbManager;
        int Numcolmns = 10;
        int Z = 10;

        [TestInitialize]
        public void SetUp()
        {
            bbManager = new BlockBehaviourManager(Numcolmns, Z);

            bbManager.Init(11);

        }        

        [TestMethod]
        public void TestSequenceMemoryCT()
        {
            // Project ABC Pattern 60 times and test C is predicted after B 31st time.

            SDR_SOM patternA = new SDR_SOM(10, 10, new List<Position_SOM> { new Position_SOM(0, 1, 1) }, iType.SPATIAL); //TestUtils.GetSDRFromPattern('A');
            SDR_SOM patternB = new SDR_SOM(10, 10, new List<Position_SOM> { new Position_SOM(3, 1, 1) }, iType.SPATIAL); //TestUtils.GetSDRFromPattern('B');
            SDR_SOM patternC = new SDR_SOM(10, 10, new List<Position_SOM> { new Position_SOM(5, 5, 1) }, iType.SPATIAL); //TestUtils.GetSDRFromPattern('C');
            SDR_SOM predictedSDR;

            int repCount = 0;
            int wirecount = 10;

            while (repCount != 60)
            {
                Console.WriteLine("REPCOUNT : " + repCount.ToString());

                bbManager.Fire(patternA);       //Fire A , Predict B NOT C

                if(repCount > wirecount)
                {
                    predictedSDR = bbManager.GetPredictedSDR();
                    
                    Assert.IsTrue(predictedSDR.IsUnionTo(patternB, true, false));
                    Assert.IsFalse(predictedSDR.IsUnionTo(patternC, true, false));
                }
               
                
                if (repCount == 1)
                {
                    bbManager.Fire(patternB);       //Fire B , Predict C NOT A
                }
                else
                {
                    bbManager.Fire(patternB);
                }

                if (repCount > wirecount)
                {
                    predictedSDR = bbManager.GetPredictedSDR();

                    Assert.IsTrue(predictedSDR.IsUnionTo(patternC, true));                    

                    bool b = predictedSDR.IsUnionTo(patternA, new List<Position_SOM>() { new Position_SOM(0,1,3)});

                    if (b)
                    {
                        predictedSDR.IsUnionTo(patternA, new List<Position_SOM>() { new Position_SOM(0, 1, 3) });
                    }

                    Assert.IsFalse(b);
                }

                if (repCount < wirecount)
                {
                    bbManager.Fire(patternC);      
                }
                else
                {
                    bbManager.Fire(patternC);       //Fire C , Predict A NOT B
                }

                predictedSDR = bbManager.GetPredictedSDR();

                if (repCount > wirecount)
                {
                    if(repCount >= 3)
                    {
                        bool b = predictedSDR.IsUnionTo(patternA, true);
                        bool c = predictedSDR.IsUnionTo(patternB, true);

                        if(b == false || c == true)
                        {
                            int bp = 1;
                        }
                    }

                    Assert.IsFalse(predictedSDR.IsUnionTo(patternB, true));
                    Assert.IsTrue(predictedSDR.IsUnionTo(patternA, true));
                }

                repCount++;
            }            
        }       
        
        [TestMethod]
        public void HighVoltagePredictedNeuronGetsPickedForFiringCT()
        {
            // HighVoltagePredictedNeuronGetsPickedForFiring from a Column

            Position_SOM position1 = new Position_SOM(3, 2, 1);

            Position_SOM position2 = new Position_SOM(3, 2, 2);

            Position_SOM position3 = new Position_SOM(3, 2, 3);

            Position_SOM higherVoltagePos= new Position_SOM(3, 2, 0);

            Neuron neuron1 = bbManager.Columns[position1.X, position1.Y].Neurons[position1.Z];

            Neuron neuron2 = bbManager.Columns[position2.X, position2.Y].Neurons[position2.Z];

            Neuron neuron3 = bbManager.Columns[position3.X, position3.Y].Neurons[position3.Z];

            Assert.AreEqual(neuron1.CurrentState, NeuronState.RESTING);

            Assert.AreEqual(neuron2.CurrentState, NeuronState.RESTING);

            Assert.AreEqual(neuron3.CurrentState, NeuronState.RESTING);

            SDR_SOM apicalSdr = new SDR_SOM(10, 10, new List<Position_SOM>() { higherVoltagePos }, iType.APICAL);

            SDR_SOM sdr_SOM = new SDR_SOM(10, 10, new List<Position_SOM>() { position1 }, iType.SPATIAL);

            bbManager.Fire(apicalSdr);

            neuron3.ProcessVoltage(10);

            Assert.AreEqual(neuron3.Voltage, 50);

            bbManager.Fire(sdr_SOM);

            var firingNeuron = bbManager.NeuronsFiringLastCycle[0];

            //Assert.AreEqual(NeuronState.RESTING, neuron1.CurrentState);

            //Assert.AreEqual(NeuronState.PREDICTED, neuron2.CurrentState);

            Assert.AreEqual(neuron3.NeuronID, firingNeuron.NeuronID);
        }      

        public void MemoryTest()
        {

        }

        public void FirAndWireWorksTest()
        {
            // Todo: Check if neurons that all fired together are connected to each other or not
        }        
    }
}
