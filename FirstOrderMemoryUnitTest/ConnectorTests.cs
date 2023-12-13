namespace FirstOrderMemoryUnitTest
{

    using FirstOrderMemory.BehaviourManagers;

    public class ConnectorTests
    {
        Connector connector;

        [SetUp]
        public void Setup()
        {
            connector = new Connector();
        }

        [Test]
        public void TestReadFromSchemaFile()
        {
           connector.ReadFromSchemaFile();

            Assert.Pass();
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
    }
}