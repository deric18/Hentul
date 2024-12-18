namespace FirstOrderMemoryUnitTest
{
    using Common;
    using FirstOrderMemory.BehaviourManagers;
    using FirstOrderMemory.Models;
    using NUnit.Framework;
    
    public class PredictorTests
    {
        BlockBehaviourManager bbManager;
        int Numcolmns = 10;
        int Z = 5;

        [SetUp]
        public void SetUp()
        {
            bbManager = new BlockBehaviourManager(Numcolmns, Numcolmns, Z, BlockBehaviourManager.LayerType.Layer_4, BlockBehaviourManager.LogMode.Trace, false);

            bbManager.Init(11);

        }        

       [Test]
        public void TestSequenceMemoryCT()
        {
            // Project ABC Pattern 60 times and test C is predicted after B 31st time.

            SDR_SOM patternA = new SDR_SOM(10, 10, new List<Position_SOM> { new Position_SOM(0, 1, 1) }, iType.SPATIAL); //TestUtils.GetSDRFromPattern('A');
            SDR_SOM patternB = new SDR_SOM(10, 10, new List<Position_SOM> { new Position_SOM(3, 1, 1) }, iType.SPATIAL); //TestUtils.GetSDRFromPattern('B');
            SDR_SOM patternC = new SDR_SOM(10, 10, new List<Position_SOM> { new Position_SOM(5, 5, 1) }, iType.SPATIAL); //TestUtils.GetSDRFromPattern('C');
            SDR_SOM predictedSDR;

            int repCount = 0;
            int wirecount = 2;
            int total = 60;
            ulong counter = 1;

            while (repCount != total)
            {
                Console.WriteLine("REPCOUNT : " + repCount.ToString());

                bbManager.Fire(patternA, counter++);       //Fire A , Predict B NOT C

                if(repCount > wirecount)
                {
                    predictedSDR = bbManager.GetPredictedSDRForNextCycle(counter++);
                    
                    Assert.IsTrue(predictedSDR.IsUnionTo(patternB, true, false));
                    Assert.IsFalse(predictedSDR.IsUnionTo(patternC, true, false));
                }
               
                
                if (repCount == 1)
                {
                    bbManager.Fire(patternB, counter++);       //Fire B , Predict C NOT A
                }
                else
                {
                    bbManager.Fire(patternB, counter++);
                }

                if (repCount > wirecount)
                {
                    predictedSDR = bbManager.GetPredictedSDRForNextCycle(counter);

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
                    bbManager.Fire(patternC, counter++);      
                }
                else
                {
                    bbManager.Fire(patternC, counter++);       //Fire C , Predict A NOT B
                }

                predictedSDR = bbManager.GetPredictedSDRForNextCycle(counter++);

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
        
        [Test]
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

            ulong counter = 1;

            bbManager.Fire(apicalSdr);

            neuron3.ProcessVoltage(10);

            Assert.AreEqual(neuron3.Voltage, 50);

            bbManager.Fire(sdr_SOM, counter++);

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
