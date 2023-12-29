namespace FirstOrderMemoryUnitTest
{
    using FirstOrderMemory.BehaviourManagers;
    using FirstOrderMemory.Models;   

    public class ConnectorTests
    {        
        BlockBehaviourManager bbManager;
        const int sizeOfColumns = 10;

       [SetUp]
        public void Setup()
        {
            bbManager = BlockBehaviourManager.GetBlockBehaviourManager(sizeOfColumns);    
            
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
                    for (int k = 0; k < 10; k++)
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
        public void TestBurstFireWithoutContext()
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

            Assert.AreEqual(4, bbManager.ColumnsThatBurst.Count);            

            var firingNeurons = bbManager.NeuronsFiringLastCycle;

            Assert.AreEqual(firingNeurons.Count, posList.Count * sizeOfColumns);

            for(int i=0; i < posList.Count; i++)
            {
                Assert.IsTrue(firingNeurons.Where(x => x.NeuronID.Equals(posList[i])).Count() > 0);
            }            
        }


        [Test(Author = "Deric")]
        public void TestWire()
        {
            //fire a neuron which has an already established connection to a known other neuron
            //Fire a pattern that fires the other known neuron
            //check if the connection b/w both is strengthened.

            var prefiringNeuron = bbManager.Columns[0, 2].Neurons[0];
            var postFiringNeuron = bbManager.Columns[5, 3].Neurons[9];

            bbManager.ConnectTwoNeurons(prefiringNeuron, postFiringNeuron);

            prefiringNeuron.Fire();

            SDR incomingpattern = GenerateSDRfromPosition(new Position(5, 3));

            postFiringNeuron.dendriticList.TryGetValue(prefiringNeuron.NeuronID.ToString(), out Synapse preFireSynapse);

            uint strenghtBeforeFire = preFireSynapse.GetStrength();

            bbManager.AddtoPredictedNeuronFromLastCycleMock(postFiringNeuron, prefiringNeuron);

            bbManager.Fire(incomingpattern, true);

            postFiringNeuron.dendriticList.TryGetValue(prefiringNeuron.NeuronID.ToString(), out Synapse value2);

            uint strengthAfterFire = value2.GetStrength();

            Assert.IsTrue(strenghtBeforeFire < strengthAfterFire);

        }

        private SDR GenerateNewRandomSDR(List<Position> posList)
        {
           return new SDR(10,10, posList);
        }

        private SDR GenerateSDRfromPosition(Position pos)
        {
            return new SDR(10, 10, new List<Position>() { pos });
        }
    }
}