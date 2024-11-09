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

        [TestMethod, Ignore("")]
        public void TestHowManyPatternsCanOneFOMBlockRemember()
        {
            throw new NotImplementedException();
        }

        [TestMethod, Ignore("")]
        public void TestHowManyNeuronsCanWeDeleteBEforeNetworkForgets()
        {
            throw new NotImplementedException();
        }

        [TestMethod, Ignore("")]
        public void TestHowMuchNoiseCanBeIntroduced()
        {
            throw new NotImplementedException();
        }





    }
}
