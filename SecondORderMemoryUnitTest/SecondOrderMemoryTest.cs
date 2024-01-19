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


            Assert.AreEqual("0-4-5-T", temporalNeuron1.NeuronID.ToString());
            Assert.AreEqual("0-3-9-T", temporalNeuron2.GetMyTemporalPartner().NeuronID.ToString());

        }

        [TestMethod]
        public void TestTemporalFiring()
        {
            SDR temporalInputPattern = GenerateRandomSDRfromPosition(iType.TEMPORAL);
            
            Position position = temporalInputPattern.ActiveBits[0];

            uint previousStrength = 0, currentStrength = 0;

            Neuron normalNeuron = GetSpatialNeuronFromTemporalCoordinate(position);

            Position temporalNeuronPosition = new Position(0, position.Y, position.X, 'T');

            if (normalNeuron.dendriticList.TryGetValue(temporalNeuronPosition.ToString(), out Synapse preSynapse))
            {
                previousStrength = preSynapse.GetStrength();
            }

            bbManager.Fire(temporalInputPattern, true);

            if (normalNeuron.dendriticList.TryGetValue(temporalNeuronPosition.ToString(), out Synapse postSynapse))
            {
                currentStrength = postSynapse.GetStrength();
            }

            var temporalNeuron = normalNeuron.GetMyTemporalPartner();

            Assert.AreEqual(temporalNeuron.NeuronID.ToString(),temporalNeuronPosition.ToString());

            Assert.AreEqual((uint)100, previousStrength);

            Assert.AreEqual(currentStrength, previousStrength);

            Assert.AreEqual(NeuronState.FIRING, temporalNeuron.CurrentState);
            
        }

        public void TestTemporalWiring()
        {

        }


        public void TestApicalLine()
        {

        }

        public void TestApicalFiring()
        {

        }

        public void TestApicalWiring()
        {

        }


        private Neuron GetSpatialNeuronFromTemporalCoordinate(Position pos)
        {
            return bbManager.Columns[pos.Z, pos.Y].Neurons[pos.X];
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
                posList.Add(new Position(rand.Next(0, 9), rand.Next(0, 9), rand.Next(0, 9)));
            }

            return new SDR(10, 10, posList, inputPatternType);
        }
    }
}