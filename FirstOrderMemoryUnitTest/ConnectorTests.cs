namespace FirstOrderMemoryUnitTest
{

    using FirstOrderMemory.BehaviourManagers;

    public class ConnectorTests
    {
        Connector connector;
        BlockBehaviourManager bbManager;

       [SetUp]
        public void Setup()
        {
            bbManager = BlockBehaviourManager.GetBlockBehaviourManager(10);
            connector = new Connector();
        }

        [Test]
        public void TestReadFromSchemaFile()
        {
            connector.ReadFromSchemaFile();


            Assert.AreEqual(4000, bbManager.totalProximalConnections);
            Assert.AreEqual(4, bbManager.Columns[2, 3].Neurons[3].ConnectedNeuronsStrength.Count);
            Assert.AreEqual(4, bbManager.Columns[9, 9].Neurons[9].ConnectedNeuronsStrength.Count);
            Assert.AreEqual(4, bbManager.Columns[0, 0].Neurons[0].ConnectedNeuronsStrength.Count);
            Assert.AreEqual(4, bbManager.Columns[5, 9].Neurons[9].ConnectedNeuronsStrength.Count);
            

        }

        [Test]
        public void TestConvertStringtoNeuron()
        {
            string posString = "9-7-3";

            var parts = posString.Split('-');

            Assert.That(3, Is.EqualTo(parts.Length));
            Assert.AreEqual("9", parts[0]);
            Assert.AreEqual("7", parts[1]);
            Assert.AreEqual("3", parts[2]);
        }

       
        public void TestFire()
        {

        }


        
        public void TestWire()
        {

        }
    }
}