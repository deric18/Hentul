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

        [SetUp]
        public void Setup()
        {
            orchestrator = Orchestrator.GetInstance(true, false, NetworkMode.PREDICTION);
            hc = orchestrator.HCAccessor;
            objectlabellist = new List<string>
            {
                "Apple",
                "Ananas",
                "Watermelon",
                "JackFruit",
                "Grapes"
            };
        }


        [Test]
        public void TestPredictObjectPositiveTest()
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

            hc.LoadMockObject(entities, false);

            Position2D pos = hc.PredictObject(1, source, null, true);

            Assert.AreEqual(posexpected.X, pos.X);
            Assert.AreEqual(posexpected.Y, pos.Y);

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


            Position2D pos = hc.PredictObject(1, source, null, true);




        }



        [Test]
        public void TestVerify()
        {
            List<RecognisedEntity> entities = GenerateRandomEntities(4);

            hc.LoadMockObject(entities, true);

            RecognisedEntity entity = entities[0];            

            entity.Verify();
            

        }

        private Sensation_Location GrabRandomSenseiFromEntity(RecognisedEntity entity)
        {
            return entity.ObjectSnapshot[rand.Next(0, entity.ObjectSnapshot.Count)];
        }

        private string GenerateRandomObjectLabels()
        {

            return objectlabellist[rand.Next(0, 6)];
        }

        private List<RecognisedEntity> GenerateRandomEntities(int num)
        {
            List<RecognisedEntity> recEnList = new List<RecognisedEntity>();

            RecognisedEntity entity;

            for (int i = 0; i < num; i++)
            {
                entity = new RecognisedEntity(GenerateRandomObjectLabels());

                for(int j = 0; j< 10; j++)
                    entity.ObjectSnapshot.Add(GenerateRandomSenation());                
            }

            return recEnList;
        }

        private Sensation_Location GenerateRandomSenation()
        {            

            Position2D cursorPos = new Position2D(rand.Next(0, 2000), rand.Next(0, 2000));

            KeyValuePair<int, List<Position2D>> kvp = GenerateNewKeyValuePair();

            SortedDictionary<string, KeyValuePair<int, List<Position2D>>> sdict = GenerateRandomDictionary(cursorPos);

            return  new Sensation_Location(sdict, cursorPos);
        }

        private SortedDictionary<string, KeyValuePair<int, List<Position2D>>> GenerateRandomDictionary(Position2D pos2d, int numEntries = 15)
        {            
            
            SortedDictionary<string, KeyValuePair<int, List<Position2D>>> sDict = new SortedDictionary<string, KeyValuePair<int, List<Position2D>>>();

            for (int i = 0; i < numEntries; i++)
            {
                sDict.Add(GetNewRandPos2d().ToString(), GenerateNewKeyValuePair());
            }

            return sDict;
        }

        private KeyValuePair<int, List<Position2D>> GenerateNewKeyValuePair(int numKvps = 1)
        {            
            List<Position2D> list = GenerateRandomPositionList();
            KeyValuePair<int, List<Position2D>> kvp = new KeyValuePair<int, List<Position2D>>(rand.Next(0,99), GenerateRandomPositionList());            
            
            return kvp;
        }

        private List<Position2D> GenerateRandomPositionList(int numPos = 3)
        {            
            List<Position2D> randPosList = new List<Position2D>();
            
            for(int i=0; i < numPos; i++)
            {
                randPosList.Add(new Position2D(rand.Next(0, 2000), rand.Next(0, 2000)));
            }

            return randPosList;
        }

        private Position2D GetNewRandPos2d() => new Position2D(rand.Next(0, 2000), rand.Next(0, 2000));

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
