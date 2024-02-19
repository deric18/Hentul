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

        [TestMethod, Ignore]
        public void StrongerCircuitWins()
        {
            //When there is prediction from neuron1 and at the same time there is a prediction from neuron2 as well and then neuron 3 fires , both connections from neuron 1 and neuron 2 should be stregthened!


        }


        public void FirAndWireWorksTest()
        {
            // Todo: Check if neurons that all fired together are connected to each other or not and connect them!  
        }        
    }
}
