namespace SecondOrderMemoryUnitTest
{    
    using SecondOrderMemory.BehaviourManagers;
    using SecondOrderMemory.Models;
    using Common;
    using NUnit.Framework;

    [TestClass]
    public class SecondOrderMemoryTest
    {
        BlockBehaviourManager? bbManager;
        const int sizeOfColumns = 10;

        [TestInitialize]
        public void Setup()
        {
            bbManager = BlockBehaviourManager.GetBlockBehaviourManager(sizeOfColumns);

            bbManager.Init(0, 0);
        }


        [TestMethod]
        public void TestMultipleInstanceOfSOMBBM()
        {
            BlockBehaviourManager bbm2 = bbManager.CloneBBM(1,3,10);
            BlockBehaviourManager bbm3 = new BlockBehaviourManager(1,3,10);           
           
            bbm3.Init(0, 0);

            SDR_SOM randSDR = GenerateRandomSDR(iType.SPATIAL);
            
            bbm2.Fire(randSDR);

            bbManager.Fire(randSDR);

            bbm3.Fire(randSDR);

            for (int i = 0; i < bbm2.NumColumns; i++)
            {
                Assert.IsNotNull(bbm2.TemporalLineArray[i, bbm2.NumColumns - 1]);
            }

            for (int i = 0; i < bbm2.NumColumns; i++)
            {
                Assert.IsNotNull(bbm2.ApicalLineArray[i, bbm2.NumColumns - 1]);
            }

            for (int i = 0; i < bbm3.NumColumns; i++)
            {
                Assert.IsNotNull(bbm3.TemporalLineArray[i, bbm3.NumColumns - 1]);
            }

            for (int i = 0; i < bbm3.NumColumns; i++)
            {
                Assert.IsNotNull(bbm3.ApicalLineArray[i, bbm3.NumColumns - 1]);
            }


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
            SDR_SOM temporalInputPattern = GenerateRandomSDR(iType.TEMPORAL);
            
            Position_SOM position = temporalInputPattern.ActiveBits[0];

            uint previousStrength = 0, currentStrength = 0;

            Neuron normalNeuron = GetSpatialNeuronFromTemporalCoordinate(position);

            Position_SOM temporalNeuronPosition = new Position_SOM(0, position.Y, position.X, 'T');

            if (normalNeuron.dendriticList.TryGetValue(temporalNeuronPosition.ToString(), out Synapse preSynapse))
            {
                previousStrength = preSynapse.GetStrength();
            }

            bbManager.Fire(temporalInputPattern, true);

            if (normalNeuron.dendriticList.TryGetValue(temporalNeuronPosition.ToString(), value: out Synapse postSynapse))
            {
                currentStrength = postSynapse.GetStrength();
            }

            var temporalNeuron = normalNeuron.GetMyTemporalPartner();

            Assert.AreEqual(temporalNeuron.NeuronID.ToString(),temporalNeuronPosition.ToString());

            Assert.AreEqual((uint)100, previousStrength);

            Assert.AreEqual(currentStrength, previousStrength);

            Assert.AreEqual(NeuronState.FIRING, temporalNeuron.CurrentState);            
        }

        [TestMethod]
        public void TestTemporalWiring()
        {
            SDR_SOM temporalInputPattern = GenerateSpecificSDRForTemporalWiring(iType.TEMPORAL);
            SDR_SOM spatialInputPattern = GenerateSpecificSDRForTemporalWiring(iType.SPATIAL);

            Position_SOM position = spatialInputPattern.ActiveBits[0];

            uint previousStrength = 0, currentStrength = 0;
            
            Neuron normalNeuron = BlockBehaviourManager.GetBlockBehaviourManager().ConvertStringPosToNeuron(position.ToString());

            Position_SOM overlapPos = GetSpatialAndTemporalOverlap(spatialInputPattern.ActiveBits[0], temporalInputPattern.ActiveBits[0]);

            var overlapNeuron = BlockBehaviourManager.GetBlockBehaviourManager().GetNeuronFromPosition('N', overlapPos.X, overlapPos.Y, overlapPos.Z);

            var temporalNeuron = overlapNeuron.GetMyTemporalPartner();

            if (overlapNeuron.dendriticList.TryGetValue(temporalNeuron.NeuronID.ToString(), out Synapse preSynapse))
            {
                previousStrength = preSynapse.GetStrength();
            }

            bbManager.Fire(temporalInputPattern, true);
            bbManager.Fire(spatialInputPattern, true);


            if (overlapNeuron.dendriticList.TryGetValue(temporalNeuron.NeuronID.ToString(), out Synapse postSynapse))
            {
                currentStrength = postSynapse.GetStrength();
            }
            
            Assert.AreEqual(temporalNeuron.NeuronID.ToString(), temporalNeuron.NeuronID.ToString());

            Assert.IsTrue(currentStrength > previousStrength);            

            Assert.AreEqual(NeuronState.FIRING, temporalNeuron.CurrentState);
        }

        [TestMethod]
        public void TestApicalLine()
        {

            Neuron apicalNeuron1 = bbManager.Columns[2, 4].Neurons[5].GetMyApicalPartner();
            Neuron apicalNeuron2 = bbManager.Columns[5, 3].Neurons[9].GetMyApicalPartner();



            Assert.AreEqual("2-4-0-A", apicalNeuron1.NeuronID.ToString());
            Assert.AreEqual("5-3-0-A", apicalNeuron2.NeuronID.ToString());
        }

        [TestMethod]
        public void TestApicalFiring()
        {
            SDR_SOM apicalInputPattern = GenerateRandomSDR(iType.APICAL);            

            Position_SOM apicalPos = apicalInputPattern.ActiveBits[0];

            Neuron apicalFiredNormalNeuron = bbManager.Columns[apicalPos.X, apicalPos.Y].Neurons[apicalPos.Z];

            int voltageBeforeFire = apicalFiredNormalNeuron.Voltage;

            bbManager.Fire(apicalInputPattern, true);

            int voltagAfterFire = apicalFiredNormalNeuron.Voltage;

            Assert.IsTrue(voltagAfterFire > voltageBeforeFire);
        }

        [TestMethod]
        public void TestApicalWiring()
        {
            SDR_SOM apicalInputPattern = GenerateSpecificSDRForTemporalWiring(iType.APICAL);
            SDR_SOM spatialInputPattern = GenerateSpecificSDRForTemporalWiring(iType.SPATIAL);

            Position_SOM position = spatialInputPattern.ActiveBits[0];

            uint previousStrength = 0, currentStrength = 0;

            Neuron normalNeuron = BlockBehaviourManager.GetBlockBehaviourManager().ConvertStringPosToNeuron(position.ToString());
           
            var apicalNeuron = normalNeuron.GetMyApicalPartner();

            if (normalNeuron.dendriticList.TryGetValue(apicalNeuron.NeuronID.ToString(), out Synapse preSynapse))
            {
                previousStrength = preSynapse.GetStrength();
            }

            bbManager.Fire(apicalInputPattern, true);
            bbManager.Fire(spatialInputPattern, true);


            if (normalNeuron.dendriticList.TryGetValue(apicalNeuron.NeuronID.ToString(), out Synapse postSynapse))
            {
                currentStrength = postSynapse.GetStrength();
            }

            Assert.AreEqual(apicalNeuron.NeuronID.ToString(), apicalNeuron.NeuronID.ToString());

            Assert.IsTrue(currentStrength > previousStrength);

            Assert.AreEqual(NeuronState.FIRING, apicalNeuron.CurrentState);
        }

        [TestMethod]
        public void TestSOMBBMClone()
        {
            BlockBehaviourManager bbm2 = bbManager.CloneBBM(0,0,0);

            bbManager.Columns[3, 3].Neurons[5].flag = 1;


            Assert.AreNotEqual(bbManager.Columns[3, 3].Neurons[5].flag, bbm2.Columns[3, 3].Neurons[5].flag);

            Assert.AreEqual(bbm2.Columns[0, 1].Neurons.Count, bbManager.Columns[0, 1].Neurons.Count);

            Assert.IsTrue(bbm2.Columns[3, 2].Neurons[5].GetMyTemporalPartner().NeuronID.Equals(bbManager.Columns[3, 2].Neurons[5].GetMyTemporalPartner().NeuronID));


            Assert.AreEqual(0, bbm2.Columns[3, 3].Neurons[5].flag);
        }

        public void TestTemporalAndApicalFiringAndWiring()
        {

        }

        private Position_SOM GetSpatialAndTemporalOverlap(Position_SOM spatial, Position_SOM temporal)
        {
            return new Position_SOM(spatial.X, spatial.Y, temporal.X);
        }

        private Neuron GetSpatialNeuronFromTemporalCoordinate(Position pos)
        {
            return bbManager.Columns[pos.Z, pos.Y].Neurons[pos.X];
        }

        private SDR_SOM GenerateRandomSDRFromPosition(List<Position_SOM> posList, iType inputPatternType)
        {
            return new SDR_SOM(10, 10, posList, inputPatternType);
        }

        private SDR_SOM GenerateRandomSDR(iType inputPatternType)
        {
            Random rand = new Random();

            int numPos = rand.Next(1, 10);

            List<Position_SOM> posList = new List<Position_SOM>();

            for(int i=0; i < numPos; i++)
            {
                posList.Add(new Position_SOM(rand.Next(0, 9), rand.Next(0, 9), rand.Next(0, 9)));
            }

            return new SDR_SOM(10, 10, posList, inputPatternType);
        }

        private SDR_SOM GenerateSpecificSDRForTemporalWiring(iType inputPatternType)
        {
            Random rand = new Random();
            int numPos = rand.Next(0, 10);

            List<Position_SOM> spatialPosList = new List<Position_SOM>()
            {
                new Position_SOM(2,4),
                new Position_SOM(8,3),
                new Position_SOM(7,2),
                new Position_SOM(0,0)
            };

            List<Position_SOM> temporalPosList = new List<Position_SOM>()
            {
                new Position_SOM(0,4),
                new Position_SOM(8,3),
                new Position_SOM(7,2),
                new Position_SOM(0,0)
            };


            if(inputPatternType == iType.TEMPORAL)
            {
                return new SDR_SOM(10, 10, temporalPosList, inputPatternType);
            }
            else
            {
                 return new SDR_SOM(10, 10, spatialPosList, inputPatternType);
            }
        }
    }
}