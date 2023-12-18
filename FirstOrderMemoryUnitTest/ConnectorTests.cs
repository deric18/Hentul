namespace FirstOrderMemoryUnitTest
{

    using FirstOrderMemory.BehaviourManagers;
    using FirstOrderMemory.Models;
    using System;

    public class ConnectorTests
    {
        Connector connector;
        BlockBehaviourManager bbManager;

       [SetUp]
        public void Setup()
        {
            bbManager = BlockBehaviourManager.GetBlockBehaviourManager(10);    
            
            bbManager.Init();
        }

        [Test]
        public void TestDendriticSchema()
        {            

            for (int i = 0; i < 10; i++)
            {
                for(int j = 0; j < 10; j++)
                {
                    Assert.AreEqual(1, bbManager.Columns[i, j].Init);
                }
            }

            Assert.AreEqual(4000, bbManager.totalProximalConnections);
            Assert.AreEqual(4, bbManager.Columns[2, 3].Neurons[3].dendriticList.Count);
            Assert.AreEqual(4, bbManager.Columns[9, 9].Neurons[9].dendriticList.Count);
            Assert.AreEqual(4, bbManager.Columns[0, 0].Neurons[0].dendriticList.Count);
            Assert.AreEqual(4, bbManager.Columns[5, 9].Neurons[9].dendriticList.Count);
            
        }


        [Test]
        public void TestAxonalSchema()
        {

            for (int i = 0; i < 10; i++)
            {
                for (int j = 0; j < 10; j++)
                {
                    Assert.AreEqual(1, bbManager.Columns[i, j].Init);
                }
            }


            for (int i = 0; i < 10; i++)
            {
                for (int j = 0; j < 10; j++)
                {
                    for (int k = 0; j < 10; k++)
                    {
                       if(bbManager.Columns[i, j].Neurons[k].flag != 8)
                       {
                            int bp = 1;
                       }                        
                    }
                }
            }

            Assert.AreEqual(4000, bbManager.totalAxonalConnections);
            Assert.AreEqual(4, bbManager.Columns[2, 3].Neurons[3].AxonalList.Count);
            Assert.AreEqual(4, bbManager.Columns[9, 9].Neurons[9].AxonalList.Count);
            Assert.AreEqual(4, bbManager.Columns[0, 0].Neurons[0].AxonalList.Count);
            Assert.AreEqual(4, bbManager.Columns[5, 9].Neurons[9].AxonalList.Count);

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

        [Test]
        public void TestFire()
        {
            List<Position> posList = new List<Position>()
            {
                new Position(0, 0),
                new Position(5, 5),
                new Position(2, 7),
                new Position(9, 9)
            };

            SDR sdr = GenerateNewRandomSDR(posList);

            bbManager.Fire(sdr);

            var firingNeurons = bbManager.NeuronsFiringLastCycle;

            Assert.IsTrue(firingNeurons.Count > 0);
            Assert.IsTrue(firingNeurons[0].NeuronID.Equals(posList[0]));
            Assert.IsTrue(firingNeurons[0].NeuronID.Equals(posList[0]));
            Assert.IsTrue(firingNeurons[0].NeuronID.Equals(posList[0]));
            Assert.IsTrue(firingNeurons[0].NeuronID.Equals(posList[0]));

        }

        private SDR GenerateNewRandomSDR(List<Position> posList)
        {
           return new SDR(10,10, posList);
        }

        public void TestWire()
        {

        }
    }
}