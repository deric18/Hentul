namespace FirstOrderMemoryUnitTest
{
    using FirstOrderMemory.BehaviourManagers;
    using FirstOrderMemory.Models;

    public class ConnectorTests
    {
        BlockBehaviourManager bbManager;
        const int numColumns = 100;
        int numFiles = (int) Math.Sqrt(numColumns);
        const int numRows = 1;

        [OneTimeSetUp]
        public void Setup()
        {
            bbManager = BlockBehaviourManager.GetBlockBehaviourManager(numColumns, numRows);

            bbManager.Init();
        }

        [Test]
        public void TestDendriticSchema()
        {
            Random rand = new Random();
            if (numRows == 10)
            {
                for (int i = 0; i < 10; i++)
                {
                    for (int j = 0; j < 10; j++)
                    {
                        if (bbManager.Columns[i, j].Init == 4)
                        {
                            int breakpoint = 0;
                        }

                        Assert.AreEqual(1, bbManager.Columns[i, j].Init);
                    }
                }
                Assert.AreEqual(4000, bbManager.totalProximalConnections);
                Assert.AreEqual(4, bbManager.Columns[2, 3].Neurons[3].dendriticList.Count);
                Assert.AreEqual(4, bbManager.Columns[9, 9].Neurons[9].dendriticList.Count);
                Assert.AreEqual(4, bbManager.Columns[0, 0].Neurons[0].dendriticList.Count);
                Assert.AreEqual(4, bbManager.Columns[5, 9].Neurons[9].dendriticList.Count);
            }
            else
            {
                Assert.AreEqual(4, bbManager.Columns[rand.Next(0, numFiles), rand.Next(0, numFiles)].Neurons[rand.Next(0, numRows)].dendriticList.Count);
                Assert.AreEqual(4, bbManager.Columns[rand.Next(0, numFiles), rand.Next(0, numFiles)].Neurons[rand.Next(0, numRows)].dendriticList.Count);
                Assert.AreEqual(4, bbManager.Columns[rand.Next(0, numFiles), rand.Next(0, numFiles)].Neurons[rand.Next(0, numRows)].dendriticList.Count);
                Assert.AreEqual(4, bbManager.Columns[rand.Next(0, numFiles), rand.Next(0, numFiles)].Neurons[rand.Next(0, numRows)].dendriticList.Count);

            }
            
            

        }


        [Test]
        public void TestAxonalSchema()
        {
            Random rand = new Random();
            if (numRows == 10)
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
                            if (bbManager.Columns[i, j].Neurons[k].flag != 8)
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
            else
            {
                Assert.AreEqual(2, bbManager.Columns[rand.Next(0, numFiles), rand.Next(0, numFiles)].Neurons[rand.Next(0, numRows)].AxonalList.Count);
                Assert.AreEqual(2, bbManager.Columns[rand.Next(0, numFiles), rand.Next(0, numFiles)].Neurons[rand.Next(0, numRows)].AxonalList.Count);
                Assert.AreEqual(2, bbManager.Columns[rand.Next(0, numFiles), rand.Next(0, numFiles)].Neurons[rand.Next(0, numRows)].AxonalList.Count);
                Assert.AreEqual(2, bbManager.Columns[rand.Next(0, numFiles), rand.Next(0, numFiles)].Neurons[rand.Next(0, numRows)].AxonalList.Count);
            }
            
            

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

            bbManager.Fire(sdr, false, true);

            if (numRows == 10)
            {
                Assert.AreEqual(4, bbManager.ColumnsThatBurst.Count);

                var firingNeurons = bbManager.NeuronsFiringLastCycle;

                Assert.AreEqual(firingNeurons.Count, posList.Count * numColumns);

                for (int i = 0; i < posList.Count; i++)
                {
                    Assert.IsTrue(firingNeurons.Where(x => x.NeuronID.Equals(posList[i])).Count() > 0);
                }
            }
            else
            {
                Assert.AreEqual(4, bbManager.ColumnsThatBurst.Count);                

                Assert.AreEqual(posList.Count * numRows, bbManager.NeuronsFiringThisCycle.Count);

                Assert.AreEqual(NeuronState.FIRING, bbManager.Columns[posList[0].X, posList[0].Y].Neurons[posList[0].Z].CurrentState);

                for (int i = 0; i < posList.Count; i++)
                {
                    Assert.IsTrue(bbManager.NeuronsFiringThisCycle.Where(x => x.NeuronID.Equals(posList[i])).Count() > 0);
                }

            }
        }


        [Test(Author = "Deric")]
        public void TestWire1()
        {
            //fire a neuron which has an already established connection to a known other neuron
            //Fire a pattern that fires the other known neuron
            //check if the connection b/w both is strengthened.

            var prefiringNeuron = bbManager.Columns[0, 2].Neurons[0];
            var postFiringNeuron = bbManager.Columns[5, 3].Neurons[0];

            bbManager.ConnectTwoNeurons(prefiringNeuron, postFiringNeuron);

            prefiringNeuron.Fire();

            SDR incomingpattern = GenerateSDRfromPosition(new Position(5, 3));

            postFiringNeuron.dendriticList.TryGetValue(prefiringNeuron.NeuronID.ToString(), out Synapse preFireSynapse);

            uint strenghtBeforeFire = preFireSynapse.GetStrength();

            bbManager.AddtoPredictedNeuronFromLastCycleMock(postFiringNeuron, prefiringNeuron);

            bbManager.Fire(incomingpattern, true);

            postFiringNeuron.dendriticList.TryGetValue(prefiringNeuron.NeuronID.ToString(), out Synapse value2);

            uint strengthAfterFire = value2.GetStrength();

            Assert.AreEqual(1, strengthAfterFire - strenghtBeforeFire);

        }

        [Test, Ignore("In Progress")]
        public void TestWire2()
        {

        }

        private SDR GenerateNewRandomSDR(List<Position> posList)
        {
            return new SDR(10, 10, posList);
        }

        private SDR GenerateSDRfromPosition(Position pos)
        {
            return new SDR(10, 10, new List<Position>() { pos });
        }
    }
}