namespace SecondOrderMemoryUnitTest
{
    using Common;
    using SecondOrderMemory.BehaviourManagers;
    using SecondOrderMemory.Models;

    [TestClass]
    public class FunctionalityTest
    {
        BlockBehaviourManager bbManager;

        [TestInitialize]
        public void SetUp()
        {
            bbManager = new BlockBehaviourManager(10);

            bbManager.Init();

        }

        [TestMethod]
        public void ColumnPredictedNeuronFiresTest()
        {
            Position_SOM position = new Position_SOM(3, 2, 5);

            Neuron neuron = bbManager.Columns[3, 2].Neurons[5];

            Assert.AreEqual(neuron.CurrentState, NeuronState.RESTING);

            neuron.ProcessVoltage(5);

            SDR_SOM sdr_SOM = new SDR_SOM(10, 10, new List<Position_SOM>() { position }, iType.SPATIAL);

            bbManager.Fire(sdr_SOM, false, true );

            Assert.AreEqual(NeuronState.FIRING, neuron.CurrentState);
            
        }

        [TestMethod]
        public void TestSequenceMemory()
        {
            // Project ABC Pattern 30 times and test C is predicted after B 31st time.

            SDR_SOM patternA = TestUtils.GetSDRFromPattern('A');
            SDR_SOM patternB = TestUtils.GetSDRFromPattern('B');
            SDR_SOM patternC = TestUtils.GetSDRFromPattern('C');

            int repCount = 30;

            while (repCount > 0)
            {
                bbManager.Fire(patternA);
                bbManager.Fire(patternB);
                bbManager.Fire(patternC);

                repCount--;
            }

            bbManager.Fire(patternA);

            //Assert B is Present;
            //Assert C is not Present

            bbManager.Fire(patternB);

            //Assert C is Present;
            //Assert A is not Present;
        }

        
        [TestMethod]
        public void StrongerCircuitWins()
        {

            Position_SOM position1 = new Position_SOM(3, 2, 5);

            Position_SOM position2 = new Position_SOM(3, 2, 6);

            Position_SOM position3 = new Position_SOM(3, 2, 7);

            Neuron neuron1 = bbManager.Columns[position1.X, position1.Y].Neurons[position1.Z];

            Neuron neuron2 = bbManager.Columns[position2.X, position2.Y].Neurons[position2.Z];

            Neuron neuron3 = bbManager.Columns[position3.X, position3.Y].Neurons[position3.Z];


            Assert.AreEqual(neuron1.CurrentState, NeuronState.RESTING);

            Assert.AreEqual(neuron2.CurrentState, NeuronState.RESTING);

            Assert.AreEqual(neuron3.CurrentState, NeuronState.RESTING);

            neuron1.ProcessVoltage(5);

            neuron2.ProcessVoltage(10);

            neuron3.ProcessVoltage(100);

            SDR_SOM sdr_SOM = new SDR_SOM(10, 10, new List<Position_SOM>() { position1 }, iType.SPATIAL);
            

            bbManager.Fire(sdr_SOM, false, true);

            Assert.AreEqual(NeuronState.PREDICTED, neuron1.CurrentState);

            Assert.AreEqual(NeuronState.PREDICTED, neuron2.CurrentState);

            Assert.AreEqual(NeuronState.FIRING, neuron3.CurrentState);

        }

        public void FirAndWireWorksTest()
        {
            // Todo: Check if neurons that all fired together are connected to each other or not and connect them!  
        }        
    }
}
