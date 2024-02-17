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
        Random rand1;

        [TestInitialize]
        public void Setup()
        {
            bbManager = new BlockBehaviourManager(0, 0, 0, sizeOfColumns);

            bbManager.Init(0, 0);

            rand1 = new Random();
        }


        [TestMethod]
        public void TestMultipleInstanceOfSOMBBM()
        {
            BlockBehaviourManager clonedBBM = bbManager.CloneBBM(1,3,10);
            BlockBehaviourManager bbm3 = new BlockBehaviourManager(1,3,10);           
           
            bbm3.Init(0, 0);

            SDR_SOM randSDR = GenerateRandomSDR(iType.SPATIAL);
            
            clonedBBM.Fire(randSDR);

            bbManager.Fire(randSDR);

            bbm3.Fire(randSDR);

            for (int i = 0; i < clonedBBM.NumColumns; i++)
            {
                Assert.IsNotNull(clonedBBM.TemporalLineArray[i, clonedBBM.NumColumns - 1]);
            }

            for (int i = 0; i < clonedBBM.NumColumns; i++)
            {
                Assert.IsNotNull(clonedBBM.ApicalLineArray[i, clonedBBM.NumColumns - 1]);
            }

            for (int i = 0; i < bbm3.NumColumns; i++)
            {
                Assert.IsNotNull(bbm3.TemporalLineArray[i, bbm3.NumColumns - 1]);
            }

            for (int i = 0; i < bbm3.NumColumns; i++)
            {
                Assert.IsNotNull(bbm3.ApicalLineArray[i, bbm3.NumColumns - 1]);
            }


            Neuron newron = clonedBBM.Columns[2, 4].Neurons[5];

            Neuron temporalNeuron1 = clonedBBM.ConvertStringPosToNeuron(newron.GetMyTemporalPartner());
            Neuron temporalNeuron2 = clonedBBM.Columns[5, 3].Neurons[9];

            Assert.AreEqual("0-4-5-T", temporalNeuron1.NeuronID.ToString());
            Assert.AreEqual("0-3-9-T", clonedBBM.ConvertStringPosToNeuron(temporalNeuron2.GetMyTemporalPartner()).NeuronID.ToString());

            Neuron apicalNeuron1 = clonedBBM.ConvertStringPosToNeuron(clonedBBM.Columns[2, 4].Neurons[5].GetMyApicalPartner());
            Neuron apicalNeuron2 = clonedBBM.ConvertStringPosToNeuron(clonedBBM.Columns[5, 3].Neurons[9].GetMyApicalPartner());


            Assert.AreEqual("2-4-0-A", apicalNeuron1.NeuronID.ToString());
            Assert.AreEqual("5-3-0-A", apicalNeuron2.NeuronID.ToString());

            //Dendrtonal & Axonal  Connections for Cloned Instance
            for(int i=0; i<clonedBBM.NumColumns; i++)
            {
                for(int j=0; j<clonedBBM.NumColumns; j++) 
                {
                    for(int k=0; k<clonedBBM.NumColumns; k++)
                    {
                        Assert.That(clonedBBM.ApicalLineArray.Length, Is.EqualTo(100));

                        if(clonedBBM.Columns[i, j].Neurons[k].dendriticList.Count == 19)
                        {
                            int bp = 1;
                        }
                        Assert.AreEqual(6, clonedBBM.Columns[i, j].Neurons[k].dendriticList.Count);

                        Assert.AreEqual(4, clonedBBM.Columns[i, j].Neurons[k].AxonalList.Count);

                        Assert.IsNotNull(clonedBBM.Columns[i, j].Neurons[k].dendriticList.ElementAt(rand1.Next(0,5)));

                        Assert.IsNotNull(clonedBBM.Columns[i, j].Neurons[k].AxonalList.ElementAt(rand1.Next(0, 3)));
                    }
                }
            }
        }

        [TestMethod]
        public void TestAxonalAndDendronalConnectionsOnNeurons()
        {
            for (int i = 0; i < bbManager.NumColumns; i++)
            {
                for (int j = 0; j < bbManager.NumColumns; j++)
                {
                    for (int k = 0; k < bbManager.NumColumns; k++)
                    {
                        Assert.That(bbManager.ApicalLineArray.Length, Is.EqualTo(100));

                        if (bbManager.Columns[i, j].Neurons[k].AxonalList.Count != 4)
                        {
                            int bp = 1;
                        }

                        if (bbManager.Columns[i, j].Neurons[k].dendriticList.Count != 6)
                        {
                            int bp = 1;
                        }

                        //Assert.AreEqual(6, bbManager.Columns[i, j].Neurons[k].dendriticList.Count);

                        //Assert.AreEqual(4, bbManager.Columns[i, j].Neurons[k].AxonalList.Count);

                        Assert.IsNotNull(bbManager.Columns[i, j].Neurons[k].dendriticList.ElementAt(rand1.Next(0, 5)));

                        Assert.IsNotNull(bbManager.Columns[i, j].Neurons[k].AxonalList.ElementAt(rand1.Next(0, 3)));
                    }
                }
            }
        }

        
        public void TestFire()
        {

            //Need to do this hack : as in the first cycle there are no predicted neurons
            //bbManager.AddtoPredictedNeuronFromLastCycleMock(neuron2, neuron1);
        }

        [TestMethod]
        public void TestFireNWire()
        {
            //fire  neuron1 which has an already established connection to a known other neuron2
            //Fire a pattern that fires the other known neuron2
            //check if the connection b/w both is strengthened.

            var neuron1 = bbManager.Columns[0, 2].Neurons[0];
            var neuron2 = bbManager.Columns[5, 3].Neurons[0];


            if(!bbManager.ConnectTwoNeurons(neuron1, neuron2, ConnectionType.AXONTONEURON))
            {
                throw new InvalidProgramException("Could Not Connect 2 Neurons");                
            }

            SDR_SOM sdr1 = GenerateRandomSDRFromPosition(new List<Position_SOM>() { new Position_SOM(0, 2) }, iType.SPATIAL);

            SDR_SOM sdr2 = GenerateRandomSDRFromPosition(new List<Position_SOM>() { new Position_SOM(5, 3) }, iType.SPATIAL);

            if (!neuron1.AxonalList.TryGetValue(neuron2.NeuronID.ToString(), out Synapse neuron1Synapse) || (!neuron2.dendriticList.TryGetValue(neuron1.NeuronID.ToString(), out Synapse neuron2Synapse)))
            {
                throw new InvalidOperationException("Could not get relavent Synapses!");
            }

            uint neuron1StrengthPreFire = neuron1Synapse.GetStrength();

            uint neuron2StrengthPreFire = neuron2Synapse.GetStrength();                        

            bbManager.Fire(sdr1, true);

            bbManager.Fire(sdr2, true);

            _ = neuron1.AxonalList.TryGetValue(neuron2.NeuronID.ToString(), out Synapse value1);

            _ = neuron2.dendriticList.TryGetValue(neuron1.NeuronID.ToString(), out Synapse value2);

            uint neuron1StrengthPostFire = value1.GetStrength();

            uint neruon2PostFireStrentgh = value2.GetStrength();

            Assert.That(neruon2PostFireStrentgh - neuron2StrengthPreFire, Is.EqualTo(1));

            Assert.AreEqual(neuron1StrengthPostFire, neuron1StrengthPreFire);

        }

        [TestMethod]
        public void TestTemporalLines()
        {
            Neuron  newron = bbManager.Columns[2, 4].Neurons[5];

            Neuron temporalNeuron1 = bbManager.ConvertStringPosToNeuron(newron.GetMyTemporalPartner());
            Neuron temporalNeuron2 = bbManager.Columns[5, 3].Neurons[9];


            Assert.AreEqual("0-4-5-T", temporalNeuron1.NeuronID.ToString());
            Assert.AreEqual("0-3-9-T", bbManager.ConvertStringPosToNeuron(temporalNeuron2.GetMyTemporalPartner()).NeuronID.ToString());

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

            var temporalNeuron = bbManager.ConvertStringPosToNeuron(normalNeuron.GetMyTemporalPartner());

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
            
            Neuron normalNeuron = bbManager.ConvertStringPosToNeuron(position.ToString());

            Position_SOM overlapPos = GetSpatialAndTemporalOverlap(spatialInputPattern.ActiveBits[0], temporalInputPattern.ActiveBits[0]);

            var overlapNeuron = bbManager.GetNeuronFromPosition('N', overlapPos.X, overlapPos.Y, overlapPos.Z);

            var temporalNeuron = bbManager.ConvertStringPosToNeuron(overlapNeuron.GetMyTemporalPartner());

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

            Neuron apicalNeuron1 = bbManager.ConvertStringPosToNeuron(bbManager.Columns[2, 4].Neurons[5].GetMyApicalPartner());
            Neuron apicalNeuron2 = bbManager.ConvertStringPosToNeuron(bbManager.Columns[5, 3].Neurons[9].GetMyApicalPartner());



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

            Neuron normalNeuron = bbManager.ConvertStringPosToNeuron(position.ToString());
           
            var apicalNeuron = bbManager.ConvertStringPosToNeuron(normalNeuron.GetMyApicalPartner());

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

            Assert.IsTrue(bbm2.ConvertStringPosToNeuron(bbm2.Columns[3, 2].Neurons[5].GetMyTemporalPartner()).NeuronID.Equals(bbManager.ConvertStringPosToNeuron(bbManager.Columns[3, 2].Neurons[5].GetMyTemporalPartner()).NeuronID));
 
            Assert.AreEqual(8, bbm2.Columns[3, 3].Neurons[5].flag);
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