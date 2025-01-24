namespace Hentul.UT
{
    using Hentul.Hippocampal_Entorinal_complex;
    using FirstOrderMemory.Models;
    using Common;

    public class HC_ECTests
    {
        Orchestrator orchestrator;
        HippocampalComplex2 hc;
        Random rand = new Random();
        List<string> objectlabellist = new List<string>();
        int objectLabelIndex;

        [SetUp]
        public void Setup()
        {
            orchestrator = Orchestrator.GetInstance(true, false, NetworkMode.PREDICTION);
            hc = orchestrator.HCAccessor;           
        }

        [Test]
        public void TestPredictObject1PositiveTest()
        {
            List<RecognisedEntity> entities = GenerateRecognisedEntity();

            List<Position2D> activeBits1 = new List<Position2D>()
                    {
                        new Position2D(777, 10),
                        new Position2D(222, 10),
                        new Position2D(333, 10),
                        new Position2D(444, 10)
                    };
           
            List<Position2D> activeBits3 = new List<Position2D>()
                    {
                        new Position2D(111, 10),
                        new Position2D(555, 10)
                    };
          
            List<Position2D> activeBits5 = new List<Position2D>()
                    {
                        new Position2D(243, 10),
                        new Position2D(234, 4),
                        new Position2D(464, 5),
                        new Position2D(33, 66),
                        new Position2D(22, 10)
                    };

            KeyValuePair<int, List<Position2D>> kvp1 = new KeyValuePair<int, List<Position2D>>(77, activeBits1);
            KeyValuePair<int, List<Position2D>> kvp2 = new KeyValuePair<int, List<Position2D>>(11, activeBits3);
            KeyValuePair<int, List<Position2D>> kvp3 = new KeyValuePair<int, List<Position2D>>(24, activeBits5);

            SortedDictionary<string, KeyValuePair<int, List<Position2D>>> dict1 = new SortedDictionary<string, KeyValuePair<int, List<Position2D>>>();

            dict1.Add("111-888-0", kvp1);
            dict1.Add("234-456-0", kvp2);
            dict1.Add("567-343-0", kvp3);

            Position2D posexpected = new Position2D(4432, 2163);

            Sensation_Location source = new Sensation_Location(dict1, posexpected);            

            hc.LoadMockObject(entities, true);

            Position2D pos = hc.PredictObject(source, null, true);

            Assert.AreEqual(posexpected.X, pos.X);
            Assert.AreEqual(posexpected.Y, pos.Y);

        }

        [Test]
        public void TestCode()
        {
            int x = 99;

            var result = x % 10;


            int bp = 1;

        }

        [Test]
        public void TestVerifyObjectSensei()
        {
            List<RecognisedEntity> entities = GenerateRecognisedEntity();

            List<Position2D> activeBits1 = new List<Position2D>()
                    {
                        new Position2D(777, 10),
                        new Position2D(222, 10),
                        new Position2D(333, 10),
                        new Position2D(444, 10)
                    };

            List<Position2D> activeBits3 = new List<Position2D>()
                    {
                        new Position2D(111, 10),
                        new Position2D(555, 10)
                    };

            List<Position2D> activeBits5 = new List<Position2D>()
                    {
                        new Position2D(243, 10),
                        new Position2D(234, 4),
                        new Position2D(464, 5),
                        new Position2D(33, 66),
                        new Position2D(22, 10)
                    };

            KeyValuePair<int, List<Position2D>> kvp1 = new KeyValuePair<int, List<Position2D>>(77, activeBits1);
            KeyValuePair<int, List<Position2D>> kvp2 = new KeyValuePair<int, List<Position2D>>(11, activeBits3);
            KeyValuePair<int, List<Position2D>> kvp3 = new KeyValuePair<int, List<Position2D>>(24, activeBits5);

            SortedDictionary<string, KeyValuePair<int, List<Position2D>>> dict1 = new SortedDictionary<string, KeyValuePair<int, List<Position2D>>>();

            dict1.Add("111-888-0", kvp1);
            dict1.Add("234-456-0", kvp2);
            dict1.Add("567-343-0", kvp3);

            Position2D posexpected = new Position2D(4432, 2163);

            Sensation_Location source = new Sensation_Location(dict1, posexpected);

            hc.LoadMockObject(entities, true);


            Position2D pos = hc.PredictObject(source, null, true);

        }



        [Test]
        public void TestVerify()
        {
            List<RecognisedEntity> entities = TestUtils.GenerateRandomEntities(4);

            hc.LoadMockObject(entities, true);

            RecognisedEntity entity = entities[0];            

            entity.Verify(null, true, 6);            
        }


        [Test, Ignore("Needs a lot more Work")]
        public void TestPreditObject2PositiveTest()
        {
            List<RecognisedEntity> entities = TestUtils.GenerateRandomEntities(4);

            hc.LoadMockObject(entities, true);


            Sensation_Location sensei = entities[1].ObjectSnapshot.ElementAt(2);
            Sensation_Location prediction = entities[1].ObjectSnapshot.ElementAt(3);

            List<string> labelList = new List<string>();

            labelList.Add(entities[1].Label);
            labelList.Add(entities[0].Label);

            List<Position2D> positions = hc.PredictObject2(sensei, prediction, labelList);

            Assert.IsTrue(positions.Count != 0);
        }

        private Sensation_Location GrabRandomSenseiFromEntity(RecognisedEntity entity)
        {
            return entity.ObjectSnapshot[rand.Next(0, entity.ObjectSnapshot.Count)];
        }
        
        
        private List<RecognisedEntity> GenerateRecognisedEntity()
        {
            List<Position2D> activeBits1 = new List<Position2D>()
                    {
                        new Position2D(777, 10),
                        new Position2D(222, 10),
                        new Position2D(333, 10),
                        new Position2D(444, 10)
                    };
            List<Position2D> activeBits2 = new List<Position2D>()
                    {
                        new Position2D(555, 3333),
                        new Position2D(546, 3234),
                        new Position2D(898, 532)
                    };
            List<Position2D> activeBits3 = new List<Position2D>()
                    {
                        new Position2D(111, 10),
                        new Position2D(555, 10)
                    };
            List<Position2D> activeBits4 = new List<Position2D>()
                    {
                        new Position2D(243, 10),
                        new Position2D(234, 4),
                        new Position2D(464, 5),
                        new Position2D(33, 66),
                        new Position2D(22, 10)
                    };
            List<Position2D> activeBits5 = new List<Position2D>()
                    {
                        new Position2D(243, 10),
                        new Position2D(234, 4),
                        new Position2D(464, 5),
                        new Position2D(33, 66),
                        new Position2D(22, 10)
                    };

            KeyValuePair<int, List<Position2D>> kvp1 = new KeyValuePair<int, List<Position2D>>(77, activeBits1);
            KeyValuePair<int, List<Position2D>> kvp2 = new KeyValuePair<int, List<Position2D>>(55, activeBits2);
            KeyValuePair<int, List<Position2D>> kvp3 = new KeyValuePair<int, List<Position2D>>(11, activeBits3);
            KeyValuePair<int, List<Position2D>> kvp4 = new KeyValuePair<int, List<Position2D>>(54, activeBits4);
            KeyValuePair<int, List<Position2D>> kvp5 = new KeyValuePair<int, List<Position2D>>(24, activeBits5);

            SortedDictionary<string, KeyValuePair<int, List<Position2D>>> dict1 = new SortedDictionary<string, KeyValuePair<int, List<Position2D>>>();

            dict1.Add("4432-2163-0", kvp1);
            dict1.Add("2332-4463-0", kvp2);
            dict1.Add("3432-8963-0", kvp3);
            dict1.Add("5632-7663-0", kvp4);

            

            SortedDictionary<string, KeyValuePair<int, List<Position2D>>> dict2 = new SortedDictionary<string, KeyValuePair<int, List<Position2D>>>();

            dict2.Add("111-888-0", kvp1);      
            dict2.Add("234-456-0", kvp3);
            dict2.Add("567-343-0", kvp5);

            SortedDictionary<string, KeyValuePair<int, List<Position2D>>> dict3 = new SortedDictionary<string, KeyValuePair<int, List<Position2D>>>();

            dict3.Add("345-219-0", kvp2);
            dict3.Add("567-8963-0", kvp4);
            dict3.Add("345-4567-0", kvp1);

            List<RecognisedEntity> recgs = new List<RecognisedEntity>()
             {
                 new RecognisedEntity("Banana")
                 {
                     ObjectSnapshot = new List<Sensation_Location>
                     {
                        new Sensation_Location(dict1, new Position2D(4432, 2163)),
                        new Sensation_Location(dict2, new Position2D(111, 888))
                     }
                 },
                 new RecognisedEntity("Ananas")
                 {
                     ObjectSnapshot = new List<Sensation_Location>
                     {
                        new Sensation_Location(dict2, new Position2D(111, 888))
                     }
                 },
                 new RecognisedEntity("Watermelon")
                 {
                     ObjectSnapshot = new List<Sensation_Location>
                     {
                        new Sensation_Location(dict1, new Position2D(345, 219)),
                        new Sensation_Location(dict3, new Position2D(345, 219))
                     }
                 }
             };           


            foreach(var recobj in recgs)
            {
                foreach(var senseloc in recobj.ObjectSnapshot)
                {
                    senseloc.ComputeStringID();
                }
            }


            return recgs;
        }
    }
}
