namespace FirstOrderMemoryUnitTest
{
    using FirstOrderMemory.BehaviourManagers;
    using NUnit.Framework;
    using FirstOrderMemory.Models;
    using Common;
    
    public class FirstOrderMemoryTest
    {
        BlockBehaviourManager? bbManager;
        const int X = 10;
        const int Y = 10;
        int Z = 4;
        Random rand1;

        [SetUp]
        public void Setup()
        {
            bbManager = new BlockBehaviourManager(X, Y, Z, BlockBehaviourManager.LayerType.Layer_4, BlockBehaviourManager.LogMode.None, true);

            bbManager.Init(1);

            rand1 = new Random();
        }

        [Test]
        public void TestMultipleInstanceOfSOM()
        {
            BlockBehaviourManager clonedBBM = bbManager.CloneBBM(1);
            BlockBehaviourManager bbm3 = new BlockBehaviourManager(10, 10);

            bbm3.Init(1);

            SDR_SOM randSDR = TestUtils.GenerateRandomSDR(iType.SPATIAL);
            
            ulong counter = 1;

            clonedBBM.Fire(randSDR, counter++);

            bbManager.Fire(randSDR, counter++);

            bbm3.Fire(randSDR, counter++);

            for (int i = 0; i < clonedBBM.Y; i++)
            {
                Assert.IsNotNull(clonedBBM.TemporalLineArray[i, clonedBBM.Z - 1]);
            }

            for (int i = 0; i < clonedBBM.X; i++)
            {
                Assert.IsNotNull(clonedBBM.ApicalLineArray[i, clonedBBM.Y - 1]);
            }

            for (int i = 0; i < bbm3.Y; i++)
            {
                Assert.IsNotNull(bbm3.TemporalLineArray[i, bbm3.Z - 1]);
            }

            for (int i = 0; i < bbm3.X; i++)
            {
                Assert.IsNotNull(bbm3.ApicalLineArray[i, bbm3.Y - 1]);
            }

            Neuron newron = clonedBBM.Columns[2, 4].Neurons[3];

            Neuron temporalNeuron1 = clonedBBM.GetNeuronFromString(newron.GetMyTemporalPartner1());
            Neuron temporalNeuron2 = clonedBBM.Columns[5, 3].Neurons[0];

            Assert.AreEqual("0-4-3-T", temporalNeuron1.NeuronID.ToString());
            Assert.AreEqual("0-3-0-T", clonedBBM.GetNeuronFromString(temporalNeuron2.GetMyTemporalPartner1()).NeuronID.ToString());

            Neuron apicalNeuron1 = clonedBBM.GetNeuronFromString(clonedBBM.Columns[2, 4].Neurons[3].GetMyApicalPartner1());
            Neuron apicalNeuron2 = clonedBBM.GetNeuronFromString(clonedBBM.Columns[5, 3].Neurons[0].GetMyApicalPartner1());

            Assert.AreEqual("2-4-0-A", apicalNeuron1.NeuronID.ToString());
            Assert.AreEqual("5-3-0-A", apicalNeuron2.NeuronID.ToString());

            //Dendrtonal & Axonal  Connections for Cloned Instance
            for (int i = 0; i < clonedBBM.X; i++)
            {
                for (int j = 0; j < clonedBBM.Y; j++)
                {
                    for (int k = 0; k < clonedBBM.Z; k++)
                    {
                        Assert.AreEqual(clonedBBM.ApicalLineArray.Length, 100);

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
            for (int i = 0; i < bbManager?.X; i++)
            {
                for (int j = 0; j < bbManager.Y; j++)
                {
                    for (int k = 0; k < bbManager.Z; k++)
                    {
                        Assert.AreEqual(bbManager.ApicalLineArray.Length, 100);

                        if (bbManager.Columns[i, j].Neurons[k].AxonalList.Count != 2)
                        {
                            int bp = 1;
                        }

                        Assert.AreEqual(4, bbManager.Columns[i, j].Neurons[k].ProximoDistalDendriticList.Count);

                        Assert.AreEqual(2, bbManager.Columns[i, j].Neurons[k].AxonalList.Count);

                        Assert.IsNotNull(bbManager.Columns[i, j].Neurons[k].ProximoDistalDendriticList.ElementAt(rand1.Next(0, 2)));

                        Assert.IsNotNull(bbManager.Columns[i, j].Neurons[k].AxonalList.ElementAt(rand1.Next(0, 2)));
                    }
                }
            }
        }

        [Test]
        public void TestMaxVoltageDeplorizedNeuronAlwaysGetPicked()
        {
            var apicalSdr = TestUtils.GenerateApicalOrSpatialSDRForDepolarization();

            var pos = apicalSdr.ActiveBits[0];

            var spatialSdr = new SDR_SOM(10, 10, new List<Position_SOM> { pos }, iType.SPATIAL);

            bbManager.Fire(apicalSdr);

            bbManager.Columns[pos.X, pos.Y].Neurons[Z - 1].ProcessVoltage(7);

            bbManager.Fire(spatialSdr, 1, false, true);

            var firingNeuronList = bbManager.GetAllNeuronsFiringLatestCycle(bbManager.CycleNum + 1);

            Assert.AreEqual(1, firingNeuronList.ActiveBits.Count);

            Assert.IsTrue(firingNeuronList.ActiveBits[0].X == pos.X && firingNeuronList.ActiveBits[0].Y == pos.Y && firingNeuronList.ActiveBits[0].Z == (Z - 1));
        }

        [Test]        
        public void TestConnectTwoNeuronsOrIncrementStrengthAfterInitialzationShouldFail()
        {
            //Connect 2 random neuron distally and check if the connection exists!

            var axonalTemporalNeuron = bbManager.Columns[0, 2].Neurons[0].GetMyTemporalPartner1();
            var dendriticNeuron = bbManager.Columns[5, 3].Neurons[0];

            SDR_SOM sdr1 = TestUtils.ConvertPositionToSDR(new List<Position_SOM>() { new Position_SOM(0, 2) }, iType.SPATIAL);

            bbManager.Fire(sdr1, 1);

            

        }

        public void TestFire()
        {

            //Need to do this hack : as in the first cycle there are no predicted neurons
            //bbManager.AddtoPredictedNeuronFromLastCycleMock(neuron2, neuron1);
        }

      [Test]
        public void TestAddNeuronListToNeuronsFiringThisCycleList()
        {
            var list1 = bbManager.Columns[0, 2].Neurons;

            var list2 = new List<Neuron>()
            {
                bbManager.Columns[0,2].Neurons[3],
                bbManager.Columns[0,2].Neurons[2],
                bbManager.Columns[0,2].Neurons[1],
                bbManager.Columns[0,2].Neurons[0]
            };

            bbManager.AddNeuronListToNeuronsFiringThisCycleList(list1);

            bbManager.AddNeuronListToNeuronsFiringThisCycleList(list2);

            int counter = 0;

            foreach (var neuron in bbManager.NeuronsFiringThisCycle)
            {
                if (neuron.NeuronID.Equals(bbManager.Columns[0, 2].Neurons[3].NeuronID))
                {
                    counter++;
                }
            }

            Assert.AreEqual(1, counter);

            TestInternalStructureAfterOperation();
        }

      [Test]
        public void TestFireNWireUT()
        {
            //fire  neuron1 which has an already established connection to a known other neuron2
            //Fire a pattern that fires the other known neuron2
            //check if the connection b/w both is strengthened.

            var neuron1 = bbManager.Columns[0, 2].Neurons[0];
            var neuron2 = bbManager.Columns[5, 3].Neurons[0];

            if (!bbManager.ConnectTwoNeurons(neuron1, neuron2, ConnectionType.AXONTONEURON))     //intentionally creating an inactive snapse to test out DISTALNEUROPLATICITY cycle.
            {
                throw new InvalidProgramException("Could Not Connect 2 Neurons");
            }

            SDR_SOM sdr1 = TestUtils.ConvertPositionToSDR(new List<Position_SOM>() { new Position_SOM(0, 2) }, iType.SPATIAL);

            SDR_SOM sdr2 = TestUtils.ConvertPositionToSDR(new List<Position_SOM>() { new Position_SOM(5, 3) }, iType.SPATIAL);

            if (!neuron1.AxonalList.TryGetValue(neuron2.NeuronID.ToString(), out Synapse neuron1Synapse) || (!neuron2.ProximoDistalDendriticList.TryGetValue(neuron1.NeuronID.ToString(), out Synapse neuron2Synapse)))
            {
                throw new InvalidOperationException("Could not get relavent Synapses!");
            }

            uint neuron1StrengthPreFire = neuron1Synapse.GetStrength();

            uint neuron2StrengthPreFire = neuron2Synapse.GetStrength();

            ulong counter = 1;

            for (int i = 0; i < BlockBehaviourManager.DISTALNEUROPLASTICITY; i++)
            {
                bbManager.Fire(sdr1, counter++);

                bbManager.Fire(sdr2, counter++);
            }

            _ = neuron1.AxonalList.TryGetValue(neuron2.NeuronID.ToString(), out Synapse value1);

            _ = neuron2.ProximoDistalDendriticList.TryGetValue(neuron1.NeuronID.ToString(), out Synapse value2);

            uint neuron1StrengthPostFire = value1.GetStrength();

            uint neruon2PostFireStrength = value2.GetStrength();

            Assert.AreEqual(1, (int) (neruon2PostFireStrength - neuron2StrengthPreFire));

            Assert.AreEqual(neuron1StrengthPostFire, neuron1StrengthPreFire);
        }

      [Test]
        public void TestSequenceMemoryinWireCase1Test1()
        {
            //Test Squence Memory is still performed on Temporally Depolarized neurons.

            var neuron1 = bbManager.Columns[0, 2].Neurons[0];
            var neuron2 = bbManager.Columns[5, 3].Neurons[0];
            
            SDR_SOM sdr1 = TestUtils.ConvertPositionToSDR(new List<Position_SOM>() { new Position_SOM(0, 2) }, iType.SPATIAL);
            SDR_SOM temporalSDR1 = TestUtils.ConvertSpatialToTemporal(new List<Position_SOM>() { neuron1.GetMyTemporalPartner2() }, bbManager.Layer);
            ulong counter = 1;

            bbManager.Fire(temporalSDR1);
            bbManager.Fire(sdr1, counter++);

            SDR_SOM sdr2 = TestUtils.ConvertPositionToSDR(new List<Position_SOM>() { new Position_SOM(5, 3) }, iType.SPATIAL);
            SDR_SOM temporalSDR2 = TestUtils.ConvertSpatialToTemporal(new List<Position_SOM>() { neuron2.GetMyTemporalPartner2() }, bbManager.Layer);

            bbManager.Fire(temporalSDR2);
            bbManager.Fire(sdr2, counter++);


            if (neuron1.AxonalList.TryGetValue(neuron2.NeuronID.ToString(), out Synapse neuron1Synapse) && neuron2.ProximoDistalDendriticList.TryGetValue(neuron1.NeuronID.ToString(), out Synapse neuron2Synapse))
            {
                Assert.IsTrue(true);
            }
            else
            {
                Assert.Fail();
            }
        }


      [Test]
        public void TestSequenceMemoryinWireCase1Test2()
        {
            //Test Squence Memory is still performed on Temporally Depolarized neurons.

            var neuron1 = bbManager.Columns[0, 2].Neurons[0];
            var neuron2 = bbManager.Columns[5, 3].Neurons[0];
            
            SDR_SOM temporalSDR1 = TestUtils.ConvertSpatialToTemporal(new List<Position_SOM>() { neuron1.GetMyTemporalPartner2() }, bbManager.Layer);
            SDR_SOM apicalSDR1 = TestUtils.ConvertPositionToSDR(new List<Position_SOM>() { neuron1.GetMyApicalPartner() }, iType.APICAL);
            SDR_SOM sdr1 = TestUtils.ConvertPositionToSDR(new List<Position_SOM>() { new Position_SOM(0, 2) }, iType.SPATIAL);

            ulong counter = 1;

            bbManager.Fire(temporalSDR1);
            bbManager.Fire(apicalSDR1);
            bbManager.Fire(sdr1, counter++);

            
            SDR_SOM temporalSDR2 = TestUtils.ConvertSpatialToTemporal(new List<Position_SOM>() { neuron2.GetMyTemporalPartner2() }, bbManager.Layer);
            SDR_SOM apicalSDR2 = TestUtils.ConvertPositionToSDR(new List<Position_SOM>() { neuron2.GetMyApicalPartner() }, iType.APICAL);
            SDR_SOM sdr2 = TestUtils.ConvertPositionToSDR(new List<Position_SOM>() { new Position_SOM(5, 3) }, iType.SPATIAL);

            bbManager.Fire(temporalSDR2);
            bbManager.Fire(apicalSDR2);
            bbManager.Fire(sdr2, counter++);


            if (neuron1.AxonalList.TryGetValue(neuron2.NeuronID.ToString(), out Synapse neuron1Synapse) || neuron2.ProximoDistalDendriticList.TryGetValue(neuron1.NeuronID.ToString(), out Synapse neuron2Synapse))
            {
                Assert.IsTrue(true);
            }
            else
            {
                Assert.Fail();
            }
        }


      [Test]
        public void TestWireCase1()
        {
            //Case 1 : All columns Bursted:
            //Every Neuron that fired in the previous Cycle should now have one new connection with every burst cell


            Position_SOM axonalPos = new Position_SOM(0, 2, 0, 'N');
            Position_SOM dendronalPos = new Position_SOM(5, 3, 3, 'N');

            SDR_SOM axonalSdr = TestUtils.ConvertPositionToSDR(new List<Position_SOM>() { axonalPos }, iType.SPATIAL);
            SDR_SOM dendronalSdr = TestUtils.ConvertPositionToSDR(new List<Position_SOM>() { dendronalPos }, iType.SPATIAL);

            Neuron axonalNeuron = bbManager.Columns[axonalPos.X, axonalPos.Y].Neurons[axonalPos.Z];
            Neuron dendronalNeuron = bbManager.Columns[dendronalPos.X, dendronalPos.Y].Neurons[dendronalPos.Z];

            ulong Counter = 1;

            if (!bbManager.ConnectTwoNeurons(axonalNeuron, dendronalNeuron, ConnectionType.DISTALDENDRITICNEURON))
            {
                throw new Exception("Could Not connect two neurons!");
            }

            var prefireSynapseStrength = bbManager.GetNeuronFromPosition(axonalPos).AxonalList[dendronalPos.ToString()].GetStrength();

            bbManager.Fire(axonalSdr, Counter++);

            dendronalNeuron.ProcessVoltage(10);

            bbManager.Fire(dendronalSdr, Counter++);

            //Make the synapsse Active           
            RepeatCycle(axonalSdr, dendronalSdr, BlockBehaviourManager.DISTALNEUROPLASTICITY - 1, Counter, true, dendronalNeuron);

            var postFiringSynapeStrength = bbManager.GetNeuronFromPosition(dendronalPos).ProximoDistalDendriticList[axonalPos.ToString()].GetStrength();

            Assert.IsTrue(BBMUtils.CheckIfTwoNeuronsHaveAnActiveSynapse(axonalNeuron, dendronalNeuron));

            Assert.IsTrue(prefireSynapseStrength < postFiringSynapeStrength);
        }


      [Test]
        public void TestWireCase2()
        {
            //Case 2 :  Few Correctly Fired, Few Bursted  : Strengthen the Correctly Fired Neurons
            //All Burswting neurons should create one new dendronal connection with previously fired neurons and the correctly fired neuron should be strengthed decently.

            Position_SOM axonalPos = new Position_SOM(0, 2, 0, 'N');
            Position_SOM dendronalPos1 = new Position_SOM(5, 3, 3, 'N');
            Position_SOM dendronalPos2 = new Position_SOM(2, 3, 3, 'N');

            SDR_SOM axonalSdr = TestUtils.ConvertPositionToSDR(new List<Position_SOM>() { axonalPos }, iType.SPATIAL);
            SDR_SOM dendronalSdr = TestUtils.ConvertPositionToSDR(new List<Position_SOM>() { dendronalPos1, dendronalPos2 }, iType.SPATIAL);

            Neuron axonalNeuron = bbManager.Columns[axonalPos.X, axonalPos.Y].Neurons[axonalPos.Z];
            Neuron dendronalNeuron1 = bbManager.Columns[dendronalPos1.X, dendronalPos1.Y].Neurons[dendronalPos1.Z];
            Neuron dendronalNeuron2 = bbManager.Columns[dendronalPos2.X, dendronalPos2.Y].Neurons[dendronalPos2.Z];

            ulong Counter = 1;

            if (!bbManager.ConnectTwoNeurons(axonalNeuron, dendronalNeuron1, ConnectionType.DISTALDENDRITICNEURON))
            {
                throw new Exception("Could Not connect two neurons!");
            }

            var prefireSynapseStrength1 = bbManager.GetNeuronFromPosition(axonalPos).AxonalList[dendronalPos1.ToString()].GetStrength();
            var prefireSynapseStrength2 = 1;

            RepeatCycle(axonalNeuron, dendronalNeuron1, (int)BlockBehaviourManager.DISTALNEUROPLASTICITY - 1);
                        
            bbManager.Fire(axonalSdr, Counter++);


            //Make the synapsse Active                       
            //RepeatCycle(axonalSdr, dendronalSdr, )

            dendronalNeuron1.ProcessVoltage(10);

            bbManager.Fire(dendronalSdr, Counter++);

            var postFiringSynapeStrength1 = bbManager.GetNeuronFromPosition(dendronalPos1).ProximoDistalDendriticList[axonalPos.ToString()].GetStrength();
            var postFiringSynapeStrength2 = bbManager.GetNeuronFromPosition(dendronalPos2).ProximoDistalDendriticList[axonalPos.ToString()].GetStrength();

            Assert.IsTrue(BBMUtils.CheckIfTwoNeuronsHaveAnActiveSynapse(axonalNeuron, dendronalNeuron1));

            Assert.IsTrue(BBMUtils.CheckIfTwoNeuronsAreConnected(axonalNeuron, dendronalNeuron2));

            Assert.AreEqual(prefireSynapseStrength2, (int)postFiringSynapeStrength2);

            Assert.IsTrue(prefireSynapseStrength1 < postFiringSynapeStrength1);
        }

        [Test, Ignore("Needs Work")]
        public void TestWireCase3()
        {
            // Case 3 : None Bursted , Some fired which were predicted, Some Did Not Burst But Fired which were NOT predicted 

            //Difficult to repro , will repro later. bigger fish to fry!

            Assert.Fail();
        }

        [Test]
        public void TestWireCase4()
        {
            //Case 1: All Predicted Neurons Fired without anyone Bursting.
            //When there is prediction from neuron1 and at the same time there is a prediction from neuron2 as well and then neuron 3 fires , both connections from neuron 1 and neuron 2 should be stregthened!

            SDR_SOM sdr1 = TestUtils.ConvertPositionToSDR(new List<Position_SOM>() { new Position_SOM(0, 2, 0) }, iType.SPATIAL);

            ulong counter = 1;

            bbManager.Fire(sdr1, counter++);

            SDR_SOM sdr2 = TestUtils.ConvertPositionToSDR(new List<Position_SOM>() { new Position_SOM(5, 3, 3) }, iType.SPATIAL);

            bbManager.Fire(sdr2, counter++);

            //Verify both the both the columns have atleast 1 of each other columns axons and dendrites.

            Assert.IsTrue(BBMUtils.CheckIfTwoNeuronsAreConnected(bbManager.GetNeuronFromPosition(sdr1.ActiveBits[0]), bbManager.GetNeuronFromPosition(sdr2.ActiveBits[0])));
        }


        [Test, Ignore("Needs Work")]
        public void TestWireCase5()
        {
            //Case 5 : Some Columns Bursted and Some of the Columns Fired Incorrectly.
            // Every cell bursted should have new dendronal connection with neurons firing last cycle and same with the ones that fired , if it doesnt already have one.



            Assert.Fail();

        }

        [Test, Ignore(" Prune gets called 50 cycles")]
        public void TestPrune()
        {
            //Create a dummy 2 sided Inactive connection , Increment CycleNum , Check if Prune is called and check if the new synapse is removed.            
        }

      [Test]
        public void TestTemporalLines()
        {
            Neuron neuron = bbManager.Columns[2, 4].Neurons[3];

            Neuron temporalNeuron1 = bbManager.GetNeuronFromString(neuron.GetMyTemporalPartner1());
            Neuron temporalNeuron2 = bbManager.Columns[5, 3].Neurons[2];


            Assert.AreEqual("0-4-3-T", temporalNeuron1.NeuronID.ToString());
            Assert.AreEqual("0-3-2-T", bbManager.GetNeuronFromString(temporalNeuron2.GetMyTemporalPartner1()).NeuronID.ToString());

        }

       [Test, Ignore("Needs Work!")]
        public void TestDistalDendronalConnectionsShouldNotBeLimitedUT()
        {
            // Todo: BUG : Why do DistalDendriticCount never exceed more than 400
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

      [Test]
        public void TestDistalDendriticConnectionBecomesActiveAfter5FiringsUT()
        {

            Position_SOM pos1 = new Position_SOM(0, 2, 0, 'N');
            Position_SOM pos2 = new Position_SOM(5, 3, 3, 'N');

            SDR_SOM sdr1 = TestUtils.ConvertPositionToSDR(new List<Position_SOM>() { pos1 }, iType.SPATIAL);
            SDR_SOM sdr2 = TestUtils.ConvertPositionToSDR(new List<Position_SOM>() { pos2 }, iType.SPATIAL);

            Neuron neuron1 = bbManager.Columns[pos1.X, pos1.Y].Neurons[pos1.Z];
            Neuron neuron2 = bbManager.Columns[pos2.X, pos2.Y].Neurons[pos2.Z];

            ulong counter = 1;

            if (!bbManager.ConnectTwoNeurons(neuron1, neuron2, ConnectionType.DISTALDENDRITICNEURON))
            {
                throw new Exception("Could Not connect two neurons!");
            }

            for (int i = 0; i < BlockBehaviourManager.DISTALNEUROPLASTICITY; i++)
            {
                Console.WriteLine("repcount : " + i.ToString());

                bbManager.Fire(sdr1, counter++);

                Assert.AreNotEqual(NeuronState.PREDICTED, neuron2.CurrentState);
            }

            for (int i = 0; i <= BlockBehaviourManager.DISTALNEUROPLASTICITY; i++)
            {
                Console.WriteLine("repcount : " + i.ToString());

                bbManager.Fire(sdr1, counter++);

                bbManager.Columns[pos2.X, pos2.Y].Neurons[pos2.Z].ProcessVoltage(30);

                bbManager.Fire(sdr2, counter++);

                Assert.AreNotEqual(NeuronState.PREDICTED, neuron2.CurrentState);
            }

            bbManager.Fire(sdr1, counter++);

            bbManager.Fire(sdr2, counter++);

            Assert.IsTrue(BBMUtils.CheckNeuronListHasThisNeuron(bbManager.NeuronsFiringLastCycle, neuron2));
        }


        public void TestNoCapOnTotalNumberOfDendriticConnections()
        {

        }


        [Test, Ignore("Not Yet Implemented!!!")]
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

                if (bbManager.CycleNum > 74 && bbManager.CycleNum % 25 == 0)
                {
                    dendronalconnectionsBeforePruning = BlockBehaviourManager.totalDendronalConnections;
                    postcheckCycle = bbManager.CycleNum;
                }

                if (bbManager.CycleNum > 74 && bbManager.CycleNum == postcheckCycle + 1)
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


        /// <summary>
        /// Lite up a Temporal Line Ensure the associated Normal Neurons deploarize and ensure there is no strength changes to there temporal partner.
        /// </summary>
      [Test]
        public void TestTemporalFiringUT()
        {
            SDR_SOM temporalInputPattern = TestUtils.GenerateRandomSDR(iType.TEMPORAL);

            Position_SOM temporalposition = temporalInputPattern.ActiveBits[0];

            Neuron TemporalNeuron = bbManager.GetNeuronFromString(TestUtils.GetSpatialNeuronFromTemporalCoordinate(bbManager, temporalposition).GetMyTemporalPartner1());

            int previousStrength = 0, currentStrength = 0;

            var normalNeuron = TestUtils.GetSpatialNeuronFromTemporalCoordinate(bbManager, temporalposition);

            if (normalNeuron.ProximoDistalDendriticList.TryGetValue(temporalposition.ToString(), out Synapse preSynapse))
            {
                previousStrength = (int)preSynapse.GetStrength();
            }

            bbManager.Fire(temporalInputPattern, 1, true, true);

            var temporalNeuron = bbManager.GetNeuronFromString(normalNeuron.GetMyTemporalPartner1());

            Assert.AreEqual(NeuronState.FIRING, temporalNeuron.CurrentState);

            Assert.AreEqual(NeuronState.PREDICTED, normalNeuron.CurrentState);

            if (normalNeuron.ProximoDistalDendriticList.TryGetValue(temporalposition.ToString(), value: out Synapse postSynapse))
            {
                currentStrength = (int) postSynapse.GetStrength();
            }

            Assert.AreEqual(0, previousStrength);

            Assert.AreEqual(currentStrength, previousStrength);

            Assert.AreEqual(NeuronState.FIRING, temporalNeuron.CurrentState);
        }

      [Test]
        public void TestTemporalWiringUT()
        {
            // Fire Temporal Pattern then Fire Spatial Pattern and see if the temporal wiring took place.

            SDR_SOM temporalInputPattern = TestUtils.GenerateSpecificSDRForTemporalWiring(iType.TEMPORAL, bbManager.Layer);
            SDR_SOM spatialInputPattern = TestUtils.GenerateSpecificSDRForTemporalWiring(iType.SPATIAL, bbManager.Layer);

            Position_SOM position = spatialInputPattern.ActiveBits[0];

            uint previousStrength = 0, currentStrength = 0;

            Neuron normalNeuron = bbManager.GetNeuronFromPosition(position);

            Position_SOM overlapPos = TestUtils.GetSpatialAndTemporalOverlap(spatialInputPattern.ActiveBits[0], temporalInputPattern.ActiveBits[0]);

            var overlapNeuron = bbManager.GetNeuronFromPosition('N', overlapPos.X, overlapPos.Y, overlapPos.Z);

            var temporalNeuron = bbManager.GetNeuronFromString(overlapNeuron.GetMyTemporalPartner1());

            if (overlapNeuron.ProximoDistalDendriticList.TryGetValue(temporalNeuron.NeuronID.ToString(), out Synapse preSynapse))
            {
                previousStrength = preSynapse.GetStrength();
            }

            bbManager.Fire(temporalInputPattern, 1, true, true);

            Assert.AreEqual(NeuronState.FIRING, temporalNeuron.CurrentState);

            bbManager.Fire(spatialInputPattern, 1, true, true);

            if (overlapNeuron.ProximoDistalDendriticList.TryGetValue(temporalNeuron.NeuronID.ToString(), out Synapse postSynapse))
            {
                currentStrength = postSynapse.GetStrength();
            }

            overlapNeuron.ProcessVoltage(120);

            Assert.AreEqual(temporalNeuron.NeuronID.ToString(), temporalNeuron.NeuronID.ToString());

            Assert.IsTrue(currentStrength > previousStrength);

            Assert.IsTrue(Neuron.COMMON_NEURONAL_FIRE_VOLTAGE < overlapNeuron.Voltage);
        }

      [Test]
        public void TestApicalLineUT()
        {

            Neuron apicalNeuron1 = bbManager.GetNeuronFromString(bbManager.Columns[2, 4].Neurons[3].GetMyApicalPartner1());
            Neuron apicalNeuron2 = bbManager.GetNeuronFromString(bbManager.Columns[5, 3].Neurons[2].GetMyApicalPartner1());



            Assert.AreEqual("2-4-0-A", apicalNeuron1.NeuronID.ToString());
            Assert.AreEqual("5-3-0-A", apicalNeuron2.NeuronID.ToString());
        }

      [Test]
        public void TestApicalFiringUT()
        {
            SDR_SOM apicalInputPattern = TestUtils.GenerateRandomSDR(iType.APICAL);

            Position_SOM apicalPos = apicalInputPattern.ActiveBits[0];

            Neuron apicalFiredNormalNeuron = bbManager.Columns[apicalPos.X, apicalPos.Y].Neurons[apicalPos.Z];

            int voltageBeforeFire = apicalFiredNormalNeuron.Voltage;

            bbManager.Fire(apicalInputPattern, 1, true, true);

            int voltagAfterFire = apicalFiredNormalNeuron.Voltage;

            Assert.IsTrue(voltagAfterFire > voltageBeforeFire);
        }

      [Test]
        public void TestApicalWiringUT()
        {
            //Fire an apical Neurons , Deplorize specific positions and fire those neurons via spatial firing

            List<Position_SOM> apicalPosList = new List<Position_SOM>()
            {
                new Position_SOM(2, 3, 3, 'N')
            };

            SDR_SOM apicalInputPattern = new SDR_SOM(10, 10, apicalPosList, iType.APICAL);
            SDR_SOM spatialInputPattern = new SDR_SOM(10, 10, apicalPosList, iType.SPATIAL);

            uint previousStrength = 0, currentStrength = 0;

            Neuron normalNeuron = bbManager.GetNeuronFromString(apicalPosList[0].ToString());

            var apicalNeuron = bbManager.GetNeuronFromString(normalNeuron.GetMyApicalPartner1());

            if (normalNeuron.ProximoDistalDendriticList.TryGetValue(apicalNeuron.NeuronID.ToString(), out Synapse preSynapse))
            {
                previousStrength = preSynapse.GetStrength();
            }

            bbManager.Fire(apicalInputPattern, 1, true, true);

            normalNeuron.ProcessVoltage(1);

            bbManager.Fire(spatialInputPattern, 1, true, true);

            if (normalNeuron.ProximoDistalDendriticList.TryGetValue(apicalNeuron.NeuronID.ToString(), value: out Synapse postSynapse))
            {
                currentStrength = postSynapse.GetStrength();
            }

            Assert.IsTrue(currentStrength > previousStrength);

            Assert.IsTrue(Neuron.COMMON_NEURONAL_FIRE_VOLTAGE < apicalNeuron.Voltage);
        }

        [Test]
        public void TestTemporalnApicalnSpatialFire()
        {
            var temporalSdr = TestUtils.GenerateSpecificSDRForTemporalWiring(iType.TEMPORAL, bbManager.Layer);
            var apicalSdr = TestUtils.GenerateSpecificSDRForTemporalWiring(iType.APICAL, bbManager.Layer);
            var spatialSdr = TestUtils.GetSpatialAndTemporalOverlapSDR(apicalSdr, temporalSdr);

            bbManager.Fire(temporalSdr, 1, false, true);      //Deplarize temporal

            bbManager.Fire(apicalSdr, 1, true, true);        //Depolarize apical

            bbManager.Fire(spatialSdr, 1, true, true);       //Fire spatial

            var firingSdr = bbManager.GetAllNeuronsFiringLatestCycle(bbManager.CycleNum + 1);

            Assert.IsTrue(firingSdr.IsUnionTo(spatialSdr, true));
        }


        /// <summary>
        /// After Temporal Fire , and Apical Fire and Spatail Fire, deploraized neuron should be cleaned up after Spatial Fire.
        /// </summary>
      [Test]
        public void TestStateManagementPositiveTest()
        {
            List<Position_SOM> posList = new List<Position_SOM>()
            {
                new Position_SOM(5,2,2)
            };

            SDR_SOM TemporalSdr = TestUtils.ConvertPositionToSDR(posList, iType.TEMPORAL);
            SDR_SOM ApicalSdr = TestUtils.ConvertPositionToSDR(posList, iType.APICAL);
            SDR_SOM SpatialSdr = TestUtils.ConvertPositionToSDR(posList, iType.SPATIAL);

            ulong Counter = 1;

            bbManager.Fire(TemporalSdr);

            Assert.IsTrue(bbManager.GetNeuronFromPosition('N', 7, 5, 2).Voltage > 0);

            bbManager.Fire(ApicalSdr);

            Assert.IsTrue(bbManager.GetNeuronFromPosition('n', 5, 2, 2).Voltage > 0);

            bbManager.Fire(SpatialSdr, Counter);

            var state = bbManager.GetNeuronFromPosition('n', 5, 5, 0).CurrentState;

            Assert.IsTrue(state.Equals(NeuronState.RESTING));

            Assert.AreEqual(0, bbManager.GetNeuronFromPosition('N', 7, 5, 3).Voltage);

            Assert.AreEqual(0, bbManager.GetNeuronFromPosition('n', 5, 1, 2).Voltage);
        }


      [Test]
        public void TestWiringNegativeTest()
        {
            //After spatial Fire and then an apical fire , Wire() method should not be called as there is nothing to wire. Spatial should have been cleanedup
        }        

      [Test]
        public void TestPostCycleCleanUpOnlyTemporal()
        {
            //After Temporal , Make sure Spatial Fire cleans up all the temporal and Apical Deploarizations that did not contribute to the fire.

            var temporalSdr = TestUtils.GenerateSpecificSDRForTemporalWiring(iType.TEMPORAL, bbManager.Layer);
            var spatialSdr = TestUtils.GenerateSpecificSDRForTemporalWiring(iType.SPATIAL, bbManager.Layer);
            ulong counter = 1;

            bbManager.Fire(temporalSdr, counter++);

            var predictedNeurons = bbManager.PredictedNeuronsforThisCycle.Keys.ToList();

            Assert.AreEqual(temporalSdr.ActiveBits.Count * bbManager.X, predictedNeurons.Count);

            bbManager.Fire(spatialSdr, counter++);

            Assert.AreEqual(0, bbManager.NeuronsFiringThisCycle.Count);

            foreach (var pos in temporalSdr.ActiveBits)
            {
                for( int i=0; i < Y; i ++)
                {
                    Neuron neuron = bbManager.Columns[i, pos.X].Neurons[pos.Y];

                    if (neuron.Voltage != 0)
                    {
                        bool bp = true;
                    }

                    Assert.AreEqual(0, neuron.Voltage);
                }
            }

            foreach (var pos in spatialSdr.ActiveBits)
            {
                foreach (var neuron in bbManager.Columns[pos.X, pos.Y].Neurons)
                {
                    Assert.AreEqual(0, neuron.Voltage);
                }
            }
        }

      [Test]
        public void TestPostCycleCleanUpOnlyApical()
        {
            //After Apical , Make sure Spatial Fire cleans up all the temporal and Apical Deploarizations that did not contribute to the fire.

            var apicalSdr = TestUtils.GenerateApicalOrSpatialSDRForDepolarization(iType.APICAL);
            var spatialSdr = TestUtils.GenerateApicalOrSpatialSDRForDepolarization(iType.SPATIAL);
            ulong counter = 1;

            bbManager.Fire(apicalSdr, counter++);           

            var predictedNeurons = bbManager.PredictedNeuronsforThisCycle.Keys.ToList();

            Assert.AreEqual(apicalSdr.ActiveBits.Count * bbManager.Z, predictedNeurons.Count);

            bbManager.Fire(spatialSdr, counter++);

            Assert.AreEqual(0, bbManager.NeuronsFiringThisCycle.Count);


            foreach (var pos in apicalSdr.ActiveBits)
            {
                foreach (var neuron in bbManager.Columns[pos.Y, pos.X].Neurons)
                {
                    Assert.AreEqual(NeuronState.RESTING, neuron.CurrentState);
                    Assert.AreEqual(0, neuron.Voltage);
                }
            }

            foreach (var pos in spatialSdr.ActiveBits)
            {
                foreach (var neuron in bbManager.Columns[pos.X, pos.Y].Neurons)
                {
                    Assert.AreEqual(neuron.CurrentState, NeuronState.RESTING);
                    Assert.AreEqual(0, neuron.Voltage);
                }
            }
        }

      [Test]
        public void TestPostCycleCleanupTemporalandApical1()
        {
            //After Temporal && Apical , Make sure Spatial Fire cleans up all the temporal and Apical Deploarizations that did not contribute to the fire.

            var temporalSdr = TestUtils.GenerateSpecificSDRForTemporalWiring(iType.TEMPORAL, bbManager.Layer);
            var apicalSdr = TestUtils.GenerateApicalOrSpatialSDRForDepolarization(iType.APICAL);
            var spatialSdr = TestUtils.GenerateSpecificSDRForTemporalWiring(iType.SPATIAL, bbManager.Layer);
            ulong counter = 1;

            bbManager.Fire(temporalSdr, counter++);

            var predictedNeurons = bbManager.PredictedNeuronsforThisCycle.Keys.ToList();

            Assert.AreEqual(temporalSdr.ActiveBits.Count * bbManager.X, predictedNeurons.Count);

            bbManager.Fire(apicalSdr, counter++);

            predictedNeurons = bbManager.PredictedNeuronsforThisCycle.Keys.ToList();

            Assert.AreEqual(apicalSdr.ActiveBits.Count * bbManager.Z, predictedNeurons.Count);

            bbManager.Fire(spatialSdr, counter++);

            Assert.AreEqual(0, bbManager.NeuronsFiringThisCycle.Count);

            foreach (var pos in temporalSdr.ActiveBits)
            {
                for (int i = 0; i < Y; i++)
                {
                    Neuron neuron = bbManager.Columns[i, pos.X].Neurons[pos.Y];

                    if (neuron.Voltage != 0)
                    {
                        bool bp = true;
                    }

                    Assert.AreEqual(0, neuron.Voltage);
                }
            }

            foreach (var pos in apicalSdr.ActiveBits)
            {
                foreach (var neuron in bbManager.Columns[pos.X, pos.Y].Neurons)
                {
                    Assert.AreEqual(neuron.CurrentState, NeuronState.RESTING);
                    Assert.AreEqual(0, neuron.Voltage);
                }
            }

            foreach (var pos in spatialSdr.ActiveBits)
            {
                foreach (var neuron in bbManager.Columns[pos.X, pos.Y].Neurons)
                {
                    Assert.AreEqual(neuron.CurrentState, NeuronState.RESTING);
                    Assert.AreEqual(0, neuron.Voltage);
                }
            }
        }

      [Test]
        public void TestPostCycleCleanupTemporalandApical2()
        {
            //After Temporal , Apical ,& Spatial Fire , Check for some Depolarized neuron if it gets cleaned up after one cycle

            var temporalSdr = TestUtils.GenerateSpecificSDRForTemporalWiring(iType.TEMPORAL, bbManager.Layer);
            var apicalSdr = TestUtils.GenerateApicalOrSpatialSDRForDepolarization(iType.APICAL);
            var spatialSdr = TestUtils.GenerateSpecificSDRForTemporalWiring(iType.SPATIAL, bbManager.Layer);
            var depolarizedNeuronList1 = FindDepolarizedNeuronList();
            ulong counter = 1;

            bbManager.Fire(temporalSdr, counter++);

            depolarizedNeuronList1 = FindDepolarizedNeuronList();

            var predictedNeurons = bbManager.PredictedNeuronsforThisCycle.Keys.ToList();

            Assert.AreEqual(temporalSdr.ActiveBits.Count * bbManager.X, predictedNeurons.Count);

            bbManager.Fire(apicalSdr, counter++);

            depolarizedNeuronList1 = FindDepolarizedNeuronList();

            predictedNeurons = bbManager.PredictedNeuronsforThisCycle.Keys.ToList();

            Assert.AreEqual(apicalSdr.ActiveBits.Count * bbManager.Z, predictedNeurons.Count);

            bbManager.Fire(spatialSdr, counter++);

            depolarizedNeuronList1 = FindDepolarizedNeuronList();

            Assert.AreEqual(0, bbManager.NeuronsFiringThisCycle.Count);

            foreach (var pos in temporalSdr.ActiveBits)
            {
                for (int i = 0; i < Y; i++)
                {
                    Neuron neuron = bbManager.Columns[i, pos.X].Neurons[pos.Y];

                    if (neuron.Voltage != 0)
                    {
                        bool bp = true;
                    }

                    Assert.AreEqual(0, neuron.Voltage);
                }
            }

            foreach (var pos in apicalSdr.ActiveBits)
            {
                foreach (var neuron in bbManager.Columns[pos.X, pos.Y].Neurons)
                {
                    Assert.AreEqual(neuron.CurrentState, NeuronState.RESTING);
                    Assert.AreEqual(0, neuron.Voltage);
                    Assert.AreEqual(NeuronState.RESTING, neuron.CurrentState);
                }
            }

            foreach (var pos in spatialSdr.ActiveBits)
            {
                foreach (var neuron in bbManager.Columns[pos.X, pos.Y].Neurons)
                {
                    Assert.AreEqual(neuron.CurrentState, NeuronState.RESTING);
                    Assert.AreEqual(0, neuron.Voltage);
                    Assert.AreEqual(NeuronState.RESTING, neuron.CurrentState);
                }
            }

            depolarizedNeuronList1 = FindDepolarizedNeuronList();

            if (depolarizedNeuronList1.Count != 0)
            {
                var sdr = GetSDRExcludingThisList(depolarizedNeuronList1);

                bbManager.Fire(sdr, counter++);

                var depolarizedNeuronList2 = FindDepolarizedNeuronList();

                foreach (var neuronFromList1 in depolarizedNeuronList1)
                {
                    bool value = BBMUtils.CheckNeuronListHasThisNeuron(depolarizedNeuronList2, neuronFromList1);
                    if (value)
                    {
                        int bp = 1;
                    }
                    Assert.IsFalse(value);
                }
            }
        }

        private void RepeatCycle(Neuron axonalNeuron, Neuron dendronalNeuron, int repeat, ulong counter = 1)
        {
            for(int i = 0; i < repeat; i++)
                bbManager.PramoteCorrectlyPredictedDendronal(axonalNeuron, dendronalNeuron);
        }


        //Hoping that (9,9,4) never gets added as a predicted Neuron
        private SDR_SOM GetSDRExcludingThisList(List<Neuron> depolarizedNeuronList)
        {
            return new SDR_SOM(X, Y, new List<Position_SOM>() { new Position_SOM(X - 1, Y - 1, Z - 1, 'N') }, iType.SPATIAL);
        }

      [Test]
        public void TestPostCycleCleanUpBurst()
        {
            //After Burst , Make sure all the bursted neurons and there connected neurons are cleaned up

            var spatialSdr = TestUtils.GenerateApicalOrSpatialSDRForDepolarization(iType.SPATIAL);

            bbManager.Fire(spatialSdr, 1);     //Burst

            Assert.AreEqual(0, bbManager.NeuronsFiringThisCycle.Count);

            foreach (var pos in spatialSdr.ActiveBits)
            {
                foreach (var neuron in bbManager.Columns[pos.X, pos.Y].Neurons)
                {
                    Assert.AreEqual(NeuronState.RESTING, neuron.CurrentState);
                    Assert.AreEqual(0, neuron.Voltage);
                }
            }

        }

        [Test, Ignore("Not Yet Completely Implemented")]
        public void TestBackUpAndRestore()
        {
            Neuron axonalNeuronID = bbManager.Columns[0, 7].Neurons[3];
            Neuron dendronalNeuronID = bbManager.Columns[1, 5].Neurons[2];

            bbManager.ConnectTwoNeurons(axonalNeuronID, dendronalNeuronID, ConnectionType.DISTALDENDRITICNEURON);

            bbManager.BackUp("1.xml");

            bbManager.RestoreFromBackUp("1.xml");

            //Assert.DoesNotThrow(() => new Exception());
        }

      [Test]
        public void TestSOMBBMCloneUT()
        {
            BlockBehaviourManager bbm2 = bbManager.CloneBBM(0);

            bbManager.Columns[3, 3].Neurons[3].flag = 1;

            Assert.AreNotEqual(bbManager.Columns[3, 3].Neurons[3].flag, bbm2.Columns[3, 3].Neurons[3].flag);

            Assert.AreEqual(bbm2.Columns[0, 1].Neurons.Count, bbManager.Columns[0, 1].Neurons.Count);

            Assert.IsTrue(bbm2.GetNeuronFromString(bbm2.Columns[3, 2].Neurons[2].GetMyTemporalPartner1()).NeuronID.Equals(bbManager.GetNeuronFromString(bbManager.Columns[3, 2].Neurons[2].GetMyTemporalPartner1()).NeuronID));

            Assert.AreEqual(4, bbm2.Columns[3, 3].Neurons[3].flag);
        }

      [Test]
        public void TestSOMColumnStructure()
        {
            BlockBehaviourManager somBBM = new BlockBehaviourManager(1250, 10, 4);

            somBBM.Init(1);

            for (int i = 0; i < bbManager?.X; i++)
            {
                for (int j = 0; j < bbManager.Y; j++)
                {
                    for (int k = 0; k < bbManager.Z; k++)
                    {
                        Assert.AreEqual(bbManager.ApicalLineArray.Length, 100);

                        Assert.AreEqual(bbManager.Columns[i, j].Neurons[k].ProximoDistalDendriticList.Count, 4);

                        Assert.AreEqual(2, bbManager.Columns[i, j].Neurons[k].AxonalList.Count);

                        Assert.IsNotNull(bbManager.Columns[i, j].Neurons[k].ProximoDistalDendriticList.ElementAt(rand1.Next(0, 2)));

                        Assert.IsNotNull(bbManager.Columns[i, j].Neurons[k].AxonalList.ElementAt(rand1.Next(0, 2)));
                    }
                }
            }
        }

        public void RepeatCycle(SDR_SOM axonalNeurondr, SDR_SOM dendronalNeuronSdr, int repCount, ulong counter = 1, bool ShouldDepolarize = false, Neuron neuronToDeplarize = null)
        {

            if (ShouldDepolarize == false)
            {
                for (int i = 0; i < repCount; i++)
                {

                    bbManager.Fire(axonalNeurondr, counter++);

                    bbManager.Fire(dendronalNeuronSdr, counter++);
                }
            }
            else
            {
                for (int i = 0; i < repCount; i++)
                {
                    bbManager.Fire(axonalNeurondr, counter++);

                    neuronToDeplarize.ProcessVoltage(10);

                    bbManager.Fire(dendronalNeuronSdr, counter++);
                }
            }
        }

        public void TestInternalStructureAfterOperation()
        {
            for (int i = 0; i < X; i++)
            {
                for (int j = 0; j < Z; j++)
                {
                    Assert.AreEqual(Z, bbManager.Columns[i, j].Neurons.Count);

                    Assert.AreEqual(X, bbManager.TemporalLineArray[i, j].AxonalList.Count);

                    Assert.AreEqual(Z, bbManager.ApicalLineArray[i, j].AxonalList.Count);
                }
            }


        }

        public void TestMemoryProblemsInThisTestUT()
        {
            Position_SOM psom = new Position_SOM(5, 5, 5);
            Neuron neuron = new Neuron(psom, 1, NeuronType.NORMAL);

            //Now check memory usage.

            int breakpoin = 1;

            //Assert.Pass();

        }

        public void TestTemporalAndApicalFiringAndWiringUT()
        {

        }


        private List<Neuron> FindDepolarizedNeuronList()
        {
            List<Neuron> toRetList = new List<Neuron>();

            for (int i = 0; i < X; i++)
            {
                for (int j = 0; j < Y; j++)
                {
                    for (int k = 0; k < Z; k++)
                    {
                        Neuron neuron = bbManager.Columns[i, j].Neurons[k];

                        if (!neuron.CurrentState.Equals(NeuronState.RESTING))
                        {
                            toRetList.Add(neuron);
                        }
                    }
                }
            }

            return toRetList;
        }
    }
}