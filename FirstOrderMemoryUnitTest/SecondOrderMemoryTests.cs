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

            var temporalSdrBbm1 = TestUtils.GenerateSpecificSDRForTemporalWiring(iType.TEMPORAL, bbManager.Layer);
            var apicalSdrBbm1 = TestUtils.GenerateSpecificSDRForTemporalWiring(iType.APICAL, bbManager.Layer);
            var spatialSdrBbm1 = TestUtils.GetSpatialAndTemporalOverlapSDR(apicalSdrBbm1, temporalSdrBbm1);


            bbManager.Fire(temporalSdrBbm1);      //Deplarize temporal

            bbManager.Fire(apicalSdrBbm1);        //Depolarize apical

            bbManager.Fire(spatialSdrBbm1);       //Fire spatial

            var firinglist1 = bbManager.GetAllFiringNeuronsThisCycle();

            var spikinglist1 = bbManager.GetAnySpikeTrainNeuronsThisCycle();
            
            var apicalSdrBbm2 = TestUtils.AddOffsetToSDR(apicalSdrBbm1, 7);
			var spatialSdrBbm2 = TestUtils.AddOffsetToSDR(spatialSdrBbm1, 7);

            bbManager.Fire(temporalSdrBbm1);      //Deplarize temporal

            var spikinglist3 = bbManager.GetAnySpikeTrainNeuronsThisCycle();

            bbManager.Fire(apicalSdrBbm2);        //Depolarize apical

            var spikinglist5 = bbManager.GetAnySpikeTrainNeuronsThisCycle();

            bbManager.Fire(spatialSdrBbm2);       //Fire spatial

            var firinglist2 = bbManager.GetAllFiringNeuronsThisCycle();

            var spikinglist2 = bbManager.GetAnySpikeTrainNeuronsThisCycle();

            var nullSDR = TestUtils.GetEmptySpatialPattern(firinglist1.ActiveBits, 1000, 10, iType.SPATIAL, bbManager.Layer);

            for(int i=0; i< 5; i++)
            {
                bbManager.Fire(nullSDR);
            }

            var spikingList3 = bbManager.GetAnySpikeTrainNeuronsThisCycle();

			bool breakpoint = true;

            //repeat pattern without any input for 5 more cycles , ensure none of the die out without incremental cycleSteal voltage, and have established a connection forminga network of spiking neurons
        }
    }
}
