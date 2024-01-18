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

            Neuron temporalNeuron1 =  newron.GetMyTemporalPartner();
            Neuron temporalNeuron2 = bbManager.Columns[5, 3].Neurons[9];


            Assert.AreEqual("5-4-0-T", temporalNeuron1.NeuronID.ToString());
            Assert.AreEqual("9-3-0-T", temporalNeuron2.GetMyTemporalPartner().NeuronID.ToString());

        }

        public void TestTemporalFiring()
        {
            SDR temporalinputPattern = GenerateRandomSDRfromPosition(iType.TEMPORAL);

            bbManager.Fire(temporalinputPattern, true);



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


        private SDR GenerateNewRandomSDR(List<Position> posList, iType inputPatternType)
        {
            return new SDR(10, 10, posList, inputPatternType);
        }

        private SDR GenerateRandomSDRfromPosition(iType inputPatternType)
        {
            Random rand = new Random();
            int numPos = rand.Next(0, 10);
            List<Position> posList = new List<Position>();

            for(int i=0; i < numPos; i++)
            {
                posList.Add(new Position(rand.Next(0, 10), rand.Next(0, 10), rand.Next(0, 10)));
            }

            return new SDR(10, 10, posList, inputPatternType);
        }
    }
}