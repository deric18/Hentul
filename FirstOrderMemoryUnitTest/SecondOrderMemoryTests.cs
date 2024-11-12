namespace FirstOrderMemoryUnitTest
{
    using Common;
    using FirstOrderMemory.BehaviourManagers;
    using FirstOrderMemory.Models;

    [TestClass]
    public class SecondOrderMemoryTests
    {

        BlockBehaviourManager? bbManager;
        const int X = 1250;
        const int Y = 10;
        int Z = 4;
        Random rand1;

        [TestInitialize]
        public void Setup()
        {
            bbManager = new BlockBehaviourManager(X, Y, Z, BlockBehaviourManager.LayerType.Layer_3B);

            bbManager.Init(1);

            rand1 = new Random();
        }

        [TestMethod]
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

            for (int i = 0; i < BlockBehaviourManager.DISTALNEUROPLASTICITY + 10; i++)
            {
                if (i == 2)
                {
                    bool breakpoint1 = false;
                }

                bbManager.Fire(temporalSdrBbm1);      //Deplarize temporal                                

                bbManager.Fire(apicalSdrBbm1);        //Depolarize apical                                

                bbManager.Fire(spatialSdrBbm1);       //Fire spatial
                
                if (i == 0)
                {
                    spikingList1 = bbManager.GetAnySpikeTrainNeuronsThisCycle();
                    Assert.AreEqual(4, spikingList1.Count);
                }

                bbManager.Fire(temporalSdrBbm1);      //Deplarize temporal            

                bbManager.Fire(apicalSdrBbm2);        //Depolarize apical            

                bbManager.Fire(spatialSdrBbm2);       //Fire spatial                

                var spikinglist2 = bbManager.GetAnySpikeTrainNeuronsThisCycle();

                Assert.AreEqual(8, spikinglist2.Count);

                foreach (var spiker1 in spikingList1)
                {
                    var neuron1 = bbManager.GetNeuronFromPosition(spiker1);

                    foreach (var spiker2 in BBMUtils.GetNonOverlappingNeuronsFromSecondList(spikinglist2, spikingList1))
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

            var firingList = bbManager.GetAllFiringNeuronsThisCycle();

            var spikingList = bbManager.GetAnySpikeTrainNeuronsThisCycle();

            var nullSDR = TestUtils.GetEmptySpatialPattern(firingList.ActiveBits, 1000, 10, iType.SPATIAL, bbManager.Layer);

            for (int i = 0; i < 2; i++)
            {
                bbManager.Fire(nullSDR);

                var firingList2 = bbManager.GetAllFiringNeuronsThisCycle();

                var spikinglist3 = bbManager.GetAnySpikeTrainNeuronsThisCycle();
            }

            var spikingList3 = bbManager.GetAnySpikeTrainNeuronsThisCycle();

            Assert.AreEqual(0, spikingList3.Count);

            bool breakpoint = true;
        }
    }
}
