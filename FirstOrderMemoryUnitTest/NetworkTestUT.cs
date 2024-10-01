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
        int Z = 10;        

        [TestInitialize]
        public void Setup()
        {
            bbManager = new BlockBehaviourManager(X, Y, Z);

            bbManager.Init(1);            
        }

        [TestMethod]
        public void TestHowManyPatternsCanOneFOMBlockRemember()
        {
            Assert.Fail();
        }

        [TestMethod]
        public void TestHowManyNeuronsCanWeDeleteBEforeNetworkForgets()
        {
            Assert.Fail();
        }

        [TestMethod]
        public void TestHowMuchNoiseCanBeIntroduced()
        {
            Assert.Fail();
        }





    }
}
