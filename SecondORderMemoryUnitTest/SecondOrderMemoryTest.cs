namespace SecondORderMemoryUnitTest
{    
    using SecondOrderMemory.BehaviourManagers;
    using SecondOrderMemory.Models;

    [TestClass]
    public class SecondOrderMemoryTest
    {
        BlockBehaviourManager? bbManager;
        const int sizeOfColumns = 10;

        [TestInitialize]
        public void Setup()
        {
            bbManager = BlockBehaviourManager.GetBlockBehaviourManager(sizeOfColumns);

            bbManager.Init();
        }

        [TestMethod]
        public void TestTemporalLines()
        {
            Neuron  newron = bbManager.Columns[2, 4].Neurons[5];

            Neuron temporalNeuron =  newron.GetMyTemporalPartner();

            Assert.AreEqual("5-4-2", temporalNeuron.NeuronID.ToString());
        }

        public void TestTemporalFiring()
        {

        }

        public void TestTemporalWiring()
        {

        }


        public void TestApicalLine()
        {

        }

        public void TestAPicalFiring()
        {

        }

        public void TestApicalWiring()
        {

        }
    }
}