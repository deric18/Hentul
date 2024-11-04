namespace FirstOrderMemoryUnitTest
{
    using Common;
    using FirstOrderMemory.BehaviourManagers;
    using FirstOrderMemory.Models;

    [TestClass]
    public class SecondOrderMemoryTests
    {

        BlockBehaviourManager? bbManager;
        const int X = 10;
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

            var temporalSdr = TestUtils.GenerateSpecificSDRForTemporalWiring(iType.TEMPORAL);
            var apicalSdr = TestUtils.GenerateSpecificSDRForTemporalWiring(iType.APICAL);
            var spatialSdr = TestUtils.GetSpatialAndTemporalOverlapSDR(apicalSdr, temporalSdr);


            bbManager.Fire(temporalSdr, false, true);      //Deplarize temporal

            bbManager.Fire(apicalSdr, true, true);        //Depolarize apical

            bbManager.Fire(spatialSdr, true, true);       //Fire spatial

            var firingSdr = bbManager.GetAllFiringNeuronsThisCycle();

            var spikingNeurons = bbManager.GetAnySpikeTrainNeuronsThisCycle();

                         
            


        }    

    }
}
