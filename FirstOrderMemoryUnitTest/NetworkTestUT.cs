namespace FirstOrderMemoryUnitTest
{
    using FirstOrderMemory.BehaviourManagers;
    using FirstOrderMemory.Models;
    using Common;
    using NUnit.Framework;

    [TestClass]
    public class NetworkTestUT
    {

        BlockBehaviourManager? bbManager;
        const int X = 10;
        const int Y = 10;
        int Z = 4;        

        [TestInitialize]
        public void Setup()
        {
            bbManager = new BlockBehaviourManager(X, Y, Z, BlockBehaviourManager.LayerType.Layer_4);

            bbManager.Init(1);            
        }

        [Test]
        public void TestHowManyPatternsCanOneFOMBlockRemember()
        {
            throw new NotImplementedException();
        }

        [Test]
        public void TestHowManyNeuronsCanWeDeleteBEforeNetworkForgets()
        {
            throw new NotImplementedException();
        }

        [Test]
        public void TestHowMuchNoiseCanBeIntroduced()
        {
            throw new NotImplementedException();
        }





    }
}
