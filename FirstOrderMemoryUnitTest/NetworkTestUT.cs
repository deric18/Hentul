namespace FirstOrderMemoryUnitTest
{
    using FirstOrderMemory.BehaviourManagers;
    using NUnit.Framework;
    
    public class NetworkTestUT
    {

        BlockBehaviourManager? bbManager;
        const int X = 10;
        const int Y = 10;
        int Z = 4;        

        [SetUp]
        public void Setup()
        {
            bbManager = new BlockBehaviourManager(X, Y, Z, BlockBehaviourManager.LayerType.Layer_4);

            bbManager.Init(1);            
        }

        [Test, Ignore("Needs work!")]
        public void TestHowManyPatternsCanOneFOMBlockRemember()
        {
            throw new NotImplementedException();
        }

        [Test, Ignore("Needs work!")]
        public void TestHowManyNeuronsCanWeDeleteBEforeNetworkForgets()
        {
            throw new NotImplementedException();
        }

        [Test, Ignore("Needs work!")]
        public void TestHowMuchNoiseCanBeIntroduced()
        {
            throw new NotImplementedException();
        } 
    }
}
