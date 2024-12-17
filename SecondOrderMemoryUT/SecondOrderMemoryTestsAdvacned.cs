namespace SecondOrderMemoryUnitTest
{
    using Common;
    using NUnit.Framework;
    using SecondOrderMemory.Models;
    
    public class SecondOrderMemoryTestsAdvanced
    {

        BlockBehaviourManager? bbManager;
        const int X = 1250;
        const int Y = 10;
        int Z = 4;
        Random rand1;

        [SetUp]
        public void Setup()
        {
            bbManager = new BlockBehaviourManager(X, Y, Z, BlockBehaviourManager.LayerType.Layer_3B, BlockBehaviourManager.LogMode.Trace, true);

            bbManager.Init(1);

            rand1 = new Random();
        }

        [Test]
        public void TestNeuronsFiringLastcycle()
        {
            var temporalSdrBbm1 = TestUtils.GenerateSpecificSDRForTemporalWiring(iType.TEMPORAL, bbManager.Layer);
            var apicalSdrBbm1 = TestUtils.GenerateSpecificSDRForTemporalWiring(iType.APICAL, bbManager.Layer);
            var spatialSdrBbm1 = TestUtils.GetSpatialAndTemporalOverlapSDR(apicalSdrBbm1, temporalSdrBbm1);

            ulong counter = 1;

            for (int i = 0; i < BlockBehaviourManager.DISTALNEUROPLASTICITY + 10; i++)
            {

                bbManager.Fire(temporalSdrBbm1);      //Deplarize temporal

                bbManager.Fire(apicalSdrBbm1);        //Depolarize apical

                bbManager.Fire(spatialSdrBbm1, counter++);       //Fire spatial

                var firingList = bbManager.GetAllNeuronsFiringLatestCycle(bbManager.CycleNum + 1);

                Assert.IsTrue(firingList.ActiveBits.Count != 0);
            }
        }

        [Test]
        public void TestNeuronsFiringThiscycle()
        {
            var temporalSdrBbm1 = TestUtils.GenerateSpecificSDRForTemporalWiring(iType.TEMPORAL, bbManager.Layer);
            var apicalSdrBbm1 = TestUtils.GenerateSpecificSDRForTemporalWiring(iType.APICAL, bbManager.Layer);
            var spatialSdrBbm1 = TestUtils.GetSpatialAndTemporalOverlapSDR(apicalSdrBbm1, temporalSdrBbm1);

            ulong counter = 1;

            for (int i = 0; i < BlockBehaviourManager.DISTALNEUROPLASTICITY + 10; i++)
            {

                bbManager.Fire(temporalSdrBbm1);      //Deplarize temporal

                bbManager.Fire(apicalSdrBbm1);        //Depolarize apical

                bbManager.Fire(spatialSdrBbm1, counter++);       //Fire spatial

                var firingList = bbManager.GetAllNeuronsFiringLatestCycle(bbManager.CycleNum + 1);

                Assert.IsTrue(firingList.ActiveBits.Count != 0);
            }
        }

        [Test]
        public void TestSOMIgnorePostCycleCleanupForSpikingNeuron()
        {
            //Get 2 Spiking Neurons in a SOM Layer in 2 completely distant BBM's and then fire the same pattern over and over till they both connect and have an active Synapse , if they dont after 5 firings then Algo fails            
            SDR_SOM firinglist2 = null;

            var temporalSdrBbm1 = TestUtils.GenerateSpecificSDRForTemporalWiring(iType.TEMPORAL, bbManager.Layer);
            var apicalSdrBbm1 = TestUtils.GenerateSpecificSDRForTemporalWiring(iType.APICAL, bbManager.Layer);
            var spatialSdrBbm1 = TestUtils.GetSpatialAndTemporalOverlapSDR(apicalSdrBbm1, temporalSdrBbm1);

            var apicalSdrBbm2 = TestUtils.AddOffsetToSDR(apicalSdrBbm1, 7);
            var spatialSdrBbm2 = TestUtils.AddOffsetToSDR(spatialSdrBbm1, 7);

            List<Position_SOM> spikingList1 = new List<Position_SOM>();

            ulong counter = 1;

            for (int i = 0; i < BlockBehaviourManager.DISTALNEUROPLASTICITY + 10; i++)
            {
                if (i == 2)
                {
                    bool breakpoint1 = false;
                }

                bbManager.Fire(temporalSdrBbm1);      //Deplarize temporal                                

                bbManager.Fire(apicalSdrBbm1);        //Depolarize apical                                

                bbManager.Fire(spatialSdrBbm1, counter++);       //Fire spatial                                

                bbManager.Fire(temporalSdrBbm1);      //Deplarize temporal            

                bbManager.Fire(apicalSdrBbm2);        //Depolarize apical            

                bbManager.Fire(spatialSdrBbm2, counter++);       //Fire spatial                

                var spikinglist2 = bbManager.GetAnySpikeTrainNeuronsThisCycle();

                //Assert.AreEqual(8, spikinglist2.Count);

                foreach (var spiker1 in spikingList1)
                {
                    var neuron1 = bbManager.GetNeuronFromPosition(spiker1);

                    foreach (var spiker2 in BBMUtils.GetNonOverlappingPositionsFromSecondList(spikinglist2, spikingList1))
                    {
                        var neuron2 = bbManager.GetNeuronFromPosition(spiker2);

                        bool b = BBMUtils.CheckIfTwoNeuronsAreConnected(neuron1, neuron2);

                        if (b == false)
                        {
                            bool breakpoint1 = false;
                        }

                        Assert.IsTrue(b);
                    }
                }
            }

            var firingList = bbManager.GetAllNeuronsFiringLatestCycle(bbManager.CycleNum + 1);

            var spikingList = bbManager.GetAnySpikeTrainNeuronsThisCycle();

            var nullSDR = TestUtils.GetEmptySpatialPattern(firingList.ActiveBits, 1000, 10, iType.SPATIAL, bbManager.Layer);

            for (int i = 0; i < 2; i++)
            {
                bbManager.Fire(nullSDR, counter++);

                var firingList2 = bbManager.GetAllNeuronsFiringLatestCycle(bbManager.CycleNum + 1);

                var spikinglist3 = bbManager.GetAnySpikeTrainNeuronsThisCycle();
            }

            var spikingList3 = bbManager.GetAnySpikeTrainNeuronsThisCycle();

            Assert.AreEqual(0, spikingList3.Count);
        }


        [Test]
        public void TestSequenceMemory1()
        {
            // Project ABC Pattern 60 times and test C is predicted after B 31st time.

            SDR_SOM patternA = new SDR_SOM(10, 10, new List<Position_SOM> { new Position_SOM(0, 1, 1) }, iType.SPATIAL); //TestUtils.GetSDRFromPattern('A');
            SDR_SOM patternB = new SDR_SOM(10, 10, new List<Position_SOM> { new Position_SOM(884, 8, 3) }, iType.SPATIAL); //TestUtils.GetSDRFromPattern('B');
            SDR_SOM patternC = new SDR_SOM(10, 10, new List<Position_SOM> { new Position_SOM(429, 4, 1) }, iType.SPATIAL); //TestUtils.GetSDRFromPattern('C');
            SDR_SOM predictedSDR;

            int repCount = 0;
            int wirecount = 10;
            ulong counter = 1;

            while (repCount != 60)
            {
                Console.WriteLine("REPCOUNT : " + repCount.ToString());

                bbManager.Fire(patternA, counter++);       //Fire A , Predict B NOT C

                if (repCount > wirecount)
                {
                    predictedSDR = bbManager.GetPredictedSDRForNextCycle(counter);

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

                    bool b = predictedSDR.IsUnionTo(patternA, new List<Position_SOM>() { new Position_SOM(0, 1, 3) });

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

                predictedSDR = bbManager.GetPredictedSDRForNextCycle(counter);

                if (repCount > wirecount)
                {
                    if (repCount >= 3)
                    {
                        bool b = predictedSDR.IsUnionTo(patternA, true);
                        bool c = predictedSDR.IsUnionTo(patternB, true);

                        if (b == false || c == true)
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
        public void TestSequenceMemory2()
        {
            Neuron targetNeuron = bbManager.GetNeuronFromString("607-3-3-N");

            SDR_SOM sDR_SOM = new SDR_SOM(1250, 10, new List<Position_SOM>() { new Position_SOM(607, 3, 3) }, iType.SPATIAL);

            var connectedPos = TestUtils.FindNeuronalPositionThatAreConnectedToTargetNeuron(targetNeuron, bbManager);

            SDR_SOM neighbhourSOM = new SDR_SOM(1250, 10, connectedPos, iType.SPATIAL);

            List<Neuron> neigbhourNeurons = TestUtils.ConvertPosListotNeuronalList(connectedPos, bbManager);

            ulong counter = 1;

            foreach (var neuron in neigbhourNeurons)
            {
                neuron.ProcessVoltage(10);
            }

            int repCount = 1;

            while (repCount > 0)
            {

                bbManager.Fire(neighbhourSOM, counter++);
                repCount--;
            }

            bbManager.Fire(sDR_SOM, counter++, false, true);

            Assert.AreEqual(targetNeuron.CurrentState, NeuronState.SPIKING);
        }


        [Test]
        public void TestTargetNeuronSpikesWithNeibhouringNeuronalFiring()
        {
            Neuron targetNeuron = bbManager.GetNeuronFromString("607-3-3-N");

            SDR_SOM sDR_SOM = new SDR_SOM(1250, 10, new List<Position_SOM>() { new Position_SOM(607, 3, 3) }, iType.SPATIAL);

            var connectedPos = TestUtils.FindNeuronalPositionThatAreConnectedToTargetNeuron(targetNeuron, bbManager);

            SDR_SOM neighbhourSOM = new SDR_SOM(1250, 10, connectedPos, iType.SPATIAL);

            List<Neuron> neigbhourNeurons = TestUtils.ConvertPosListotNeuronalList(connectedPos, bbManager);

            ulong counter = 1;

            foreach (var neuron in neigbhourNeurons)
            {
                neuron.ProcessVoltage(10);
            }

            bbManager.Fire(neighbhourSOM, counter++);

            bbManager.Fire(sDR_SOM, counter++);

            Assert.AreEqual(targetNeuron.CurrentState, NeuronState.SPIKING);
        }

        [Test]
        public void TestHowManyNeighbhoursNeedToFireBeforTargetNeuronFires()
        {
            // Fire a bunch of surrounding neurons near a target neuron and make sure it fire and check how many neurons are needed before it fires.
        }

    }
}
