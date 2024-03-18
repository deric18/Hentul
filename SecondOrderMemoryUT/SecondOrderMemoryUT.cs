using Common;
using SecondOrderMemory.BehaviourManagers;
using SecondOrderMemory.Models;

namespace SecondOrderMemoryUT
{    
    public class SecondOrderMemoryTests
    {
        int sizeOfColumns = 10;
        BlockBehaviourManager bbManager;
        Random rand1;

        [SetUp]
        public void Setup()
        {
            bbManager = new BlockBehaviourManager(sizeOfColumns);

            bbManager.Init(0, 0);

            rand1 = new Random();
        }

                


        [Test]
        public void TestMultipleInstanceOfSOM()
        {
            BlockBehaviourManager clonedBBM = bbManager.CloneBBM(1, 3, 10);
            BlockBehaviourManager bbm3 = new BlockBehaviourManager(10, 1, 3, 10);

            bbm3.Init(0, 0);

            SDR_SOM randSDR = TestUtils.GenerateRandomSDR(iType.SPATIAL);

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
            for (int i = 0; i < clonedBBM.NumColumns; i++)
            {
                for (int j = 0; j < clonedBBM.NumColumns; j++)
                {
                    for (int k = 0; k < clonedBBM.NumColumns; k++)
                    {
                        Assert.That(clonedBBM.ApicalLineArray.Length, Is.EqualTo(100));

                        if (clonedBBM.Columns[i, j].Neurons[k].AxonalList.Count == 1)
                        {
                            int bp = 1;
                        }

                        Assert.AreEqual(4, clonedBBM.Columns[i, j].Neurons[k].ProximoDistalDendriticList.Count);
                        Assert.AreEqual(2, clonedBBM.Columns[i, j].Neurons[k].AxonalList.Count);
                        Assert.IsNotNull(clonedBBM.Columns[i, j].Neurons[k].ProximoDistalDendriticList.ElementAt(rand1.Next(0, 4)));
                        Assert.IsNotNull(clonedBBM.Columns[i, j].Neurons[k].AxonalList.ElementAt(rand1.Next(0, 2)));
                    }
                }
            }
        }

        [Test]
        public void TestAxonalAndDendronalConnectionsOnNeuronsUT()
        {
            for (int i = 0; i < bbManager?.NumColumns; i++)
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

                        if (bbManager.Columns[i, j].Neurons[k].ProximoDistalDendriticList.Count != 6)
                        {
                            int bp = 1;
                        }

                        //Assert.AreEqual(6, bbManager.Columns[i, j].Neurons[k].dendriticList.Count);

                        //Assert.AreEqual(4, bbManager.Columns[i, j].Neurons[k].AxonalList.Count);

                        Assert.IsNotNull(bbManager.Columns[i, j].Neurons[k].ProximoDistalDendriticList.ElementAt(rand1.Next(0, 2)));

                        Assert.IsNotNull(bbManager.Columns[i, j].Neurons[k].AxonalList.ElementAt(rand1.Next(0, 2)));
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
        public void TestFireNWireUT()
        {
            //fire  neuron1 which has an already established connection to a known other neuron2
            //Fire a pattern that fires the other known neuron2
            //check if the connection b/w both is strengthened.

            var neuron1 = bbManager.Columns[0, 2].Neurons[0];
            var neuron2 = bbManager.Columns[5, 3].Neurons[0];

            UInt16 plasticityCount = BlockBehaviourManager.DISTALNEUROPLASTICITY;

            if (!bbManager.ConnectTwoNeuronsOrIncrementStrength(neuron1, neuron2, ConnectionType.AXONTONEURON))
            {
                throw new InvalidProgramException("Could Not Connect 2 Neurons");
            }

            SDR_SOM sdr1 = TestUtils.GenerateRandomSDRFromPosition(new List<Position_SOM>() { new Position_SOM(0, 2) }, iType.SPATIAL);

            SDR_SOM sdr2 = TestUtils.GenerateRandomSDRFromPosition(new List<Position_SOM>() { new Position_SOM(5, 3) }, iType.SPATIAL);

            if (!neuron1.AxonalList.TryGetValue(neuron2.NeuronID.ToString(), out Synapse neuron1Synapse) || (!neuron2.ProximoDistalDendriticList.TryGetValue(neuron1.NeuronID.ToString(), out Synapse neuron2Synapse)))
            {
                throw new InvalidOperationException("Could not get relavent Synapses!");
            }

            uint neuron1StrengthPreFire = neuron1Synapse.GetStrength();

            uint neuron2StrengthPreFire = neuron2Synapse.GetStrength();

            for (int i = 0; i < 5; i++)
            {
                bbManager.Fire(sdr1);

                bbManager.Fire(sdr2);
            }

            _ = neuron1.AxonalList.TryGetValue(neuron2.NeuronID.ToString(), out Synapse value1);

            _ = neuron2.ProximoDistalDendriticList.TryGetValue(neuron1.NeuronID.ToString(), out Synapse value2);

            uint neuron1StrengthPostFire = value1.GetStrength();

            uint neruon2PostFireStrength = value2.GetStrength();

            Assert.That(neruon2PostFireStrength - neuron2StrengthPreFire, Is.EqualTo(1));

            Assert.AreEqual(neuron1StrengthPostFire, neuron1StrengthPreFire);

        }

        public void TestWire1UT()
        {
            //When there is prediction from neuron1 and at the same time there is a prediction from neuron2 as well and then neuron 3 fires , both connections from neuron 1 and neuron 2 should be stregthened!
        }

        [TestMethod]
        public void TestTemporalLines()
        {
            Neuron newron = bbManager.Columns[2, 4].Neurons[5];

            Neuron temporalNeuron1 = bbManager.ConvertStringPosToNeuron(newron.GetMyTemporalPartner());
            Neuron temporalNeuron2 = bbManager.Columns[5, 3].Neurons[9];


            Assert.AreEqual("0-4-5-T", temporalNeuron1.NeuronID.ToString());
            Assert.AreEqual("0-3-9-T", bbManager.ConvertStringPosToNeuron(temporalNeuron2.GetMyTemporalPartner()).NeuronID.ToString());

        }

        [Test]
        public void TestDistalDendronalConnectionsShouldNotBeLimitedUT()
        {
            //BUG : Why doe DitalDendriticCount never exceed more than 400
            // Create a new Dendronal Connection make sure it is a new Dendronal Connection , 

            List<SDR_SOM> sDR_SOMs = TestUtils.GenerateFixedRandomSDR_SOMs(8, 0, 9);
            uint dendronalconnectionsBeforePruning = 0, dendronalconnectionsAfterPruning = 0;

            for (int i = 0; i < sDR_SOMs.Count; i++)
            {

                dendronalconnectionsBeforePruning = BlockBehaviourManager.totalDendronalConnections;

                bbManager.Fire(sDR_SOMs[i]);

                dendronalconnectionsAfterPruning = BlockBehaviourManager.totalDendronalConnections;

                //Assert.IsTrue(dendronalconnectionsBeforePruning > dendronalconnectionsAfterPruning);
            }

            Assert.Fail();
        }

        public void TestDistalDendriticConnectionBecomesActiveAfter4FiringsUT()
        {
            UInt16 numRepeats = BlockBehaviourManager.DISTALNEUROPLASTICITY;

            Position_SOM pos1 = TestUtils.GenerateRandomPosition(1);
            Position_SOM pos2 = TestUtils.GenerateRandomPosition(1);

            SDR_SOM sdr1 = TestUtils.GenerateRandomSDRFromPosition(new List<Position_SOM>() { pos1 }, iType.SPATIAL);

            Neuron neuron1 = bbManager.Columns[pos1.X, pos1.Y].Neurons[pos1.Z];
            Neuron neuron2 = bbManager.Columns[pos2.X, pos2.Y].Neurons[pos2.Z];

            bbManager.ConnectTwoNeuronsOrIncrementStrength(neuron1, neuron2, ConnectionType.DISTALDENDRITICNEURON);


            for (int i = 0; i < numRepeats; i++)
            {
                bbManager.Fire(sdr1);

                Assert.AreNotEqual(neuron2.CurrentState, NeuronState.FIRING);
            }

            bbManager.Fire(sdr1);

            Assert.AreEqual(neuron2.CurrentState, NeuronState.FIRING);

        }

        public void TestNoCapOnTotalNumberOfDendriticConnections()
        {

        }

        [Test]
        public void TestPruneCycleRefresh()
        {
            //Run cycle for 26 cycles , record distal synapse count at 25 and check if the count reduced at 26th cycle.

            List<SDR_SOM> sDR_SOMs = TestUtils.GenerateFixedRandomSDR_SOMs(1002, 0, 9);
            uint dendronalconnectionsBeforePruning = 0, dendronalconnectionsAfterPruning = 0;
            ulong postcheckCycle = 0;

            for (int i = 0; i < sDR_SOMs.Count; i++)
            {

                bbManager.Fire(sDR_SOMs[i]);

                dendronalconnectionsBeforePruning = BlockBehaviourManager.totalDendronalConnections;

                if (BlockBehaviourManager.CycleNum > 74 && BlockBehaviourManager.CycleNum % 25 == 0)
                {
                    dendronalconnectionsBeforePruning = BlockBehaviourManager.totalDendronalConnections;
                    postcheckCycle = BlockBehaviourManager.CycleNum;
                }

                if (BlockBehaviourManager.CycleNum > 74 && BlockBehaviourManager.CycleNum == postcheckCycle + 1)
                {
                    dendronalconnectionsAfterPruning = BlockBehaviourManager.totalDendronalConnections;

                    if (dendronalconnectionsBeforePruning >= dendronalconnectionsAfterPruning)
                    {
                        int breakpoint = 0;
                    }

                    Assert.IsTrue(dendronalconnectionsBeforePruning > dendronalconnectionsAfterPruning);
                }
            }
        }

        [TestMethod]
        public void TestTemporalFiringUT()
        {
            SDR_SOM temporalInputPattern = TestUtils.GenerateRandomSDR(iType.TEMPORAL);

            Position_SOM position = temporalInputPattern.ActiveBits[0];

            uint previousStrength = 0, currentStrength = 0;

            Neuron normalNeuron = TestUtils.GetSpatialNeuronFromTemporalCoordinate(bbManager, position);

            Position_SOM temporalNeuronPosition = new Position_SOM(0, position.Y, position.X, 'T');

            if (normalNeuron.ProximoDistalDendriticList.TryGetValue(temporalNeuronPosition.ToString(), out Synapse preSynapse))
            {
                previousStrength = preSynapse.GetStrength();
            }

            bbManager.Fire(temporalInputPattern, true);

            if (normalNeuron.ProximoDistalDendriticList.TryGetValue(temporalNeuronPosition.ToString(), value: out Synapse postSynapse))
            {
                currentStrength = postSynapse.GetStrength();
            }

            var temporalNeuron = bbManager.ConvertStringPosToNeuron(normalNeuron.GetMyTemporalPartner());

            Assert.AreEqual(temporalNeuron.NeuronID.ToString(), temporalNeuronPosition.ToString());

            Assert.AreEqual(1, previousStrength);

            Assert.AreEqual(currentStrength, previousStrength);

            Assert.AreEqual(NeuronState.FIRING, temporalNeuron.CurrentState);
        }

        [TestMethod]
        public void TestTemporalWiringUT()
        {
            SDR_SOM temporalInputPattern = TestUtils.GenerateSpecificSDRForTemporalWiring(iType.TEMPORAL);
            SDR_SOM spatialInputPattern = TestUtils.GenerateSpecificSDRForTemporalWiring(iType.SPATIAL);

            Position_SOM position = spatialInputPattern.ActiveBits[0];

            uint previousStrength = 0, currentStrength = 0;

            Neuron normalNeuron = bbManager.ConvertStringPosToNeuron(position.ToString());

            Position_SOM overlapPos = TestUtils.GetSpatialAndTemporalOverlap(spatialInputPattern.ActiveBits[0], temporalInputPattern.ActiveBits[0]);

            var overlapNeuron = bbManager.GetNeuronFromPosition('N', overlapPos.X, overlapPos.Y, overlapPos.Z);

            var temporalNeuron = bbManager.ConvertStringPosToNeuron(overlapNeuron.GetMyTemporalPartner());

            if (overlapNeuron.ProximoDistalDendriticList.TryGetValue(temporalNeuron.NeuronID.ToString(), out Synapse preSynapse))
            {
                previousStrength = preSynapse.GetStrength();
            }

            bbManager.Fire(temporalInputPattern);
            bbManager.Fire(spatialInputPattern);


            if (overlapNeuron.ProximoDistalDendriticList.TryGetValue(temporalNeuron.NeuronID.ToString(), out Synapse postSynapse))
            {
                currentStrength = postSynapse.GetStrength();
            }

            Assert.AreEqual(temporalNeuron.NeuronID.ToString(), temporalNeuron.NeuronID.ToString());

            Assert.IsTrue(currentStrength > previousStrength);

            Assert.AreEqual(NeuronState.FIRING, temporalNeuron.CurrentState);
        }

        [Test]
        public void TestApicalLineUT()
        {

            Neuron apicalNeuron1 = bbManager.ConvertStringPosToNeuron(bbManager.Columns[2, 4].Neurons[5].GetMyApicalPartner());
            Neuron apicalNeuron2 = bbManager.ConvertStringPosToNeuron(bbManager.Columns[5, 3].Neurons[9].GetMyApicalPartner());



            Assert.AreEqual("2-4-0-A", apicalNeuron1.NeuronID.ToString());
            Assert.AreEqual("5-3-0-A", apicalNeuron2.NeuronID.ToString());
        }

        [TestMethod]
        public void TestApicalFiringUT()
        {
            SDR_SOM apicalInputPattern = TestUtils.GenerateRandomSDR(iType.APICAL);

            Position_SOM apicalPos = apicalInputPattern.ActiveBits[0];

            Neuron apicalFiredNormalNeuron = bbManager.Columns[apicalPos.X, apicalPos.Y].Neurons[apicalPos.Z];

            int voltageBeforeFire = apicalFiredNormalNeuron.Voltage;

            bbManager.Fire(apicalInputPattern, true, true);

            int voltagAfterFire = apicalFiredNormalNeuron.Voltage;

            Assert.IsTrue(voltagAfterFire > voltageBeforeFire);
        }
    }
}