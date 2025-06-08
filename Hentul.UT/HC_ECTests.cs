namespace Hentul.UT
{
    using Hentul.Hippocampal_Entorinal_complex;    
    using Common;

    public class HC_ECTests
    {
        Orchestrator orchestrator;
        HippocampalComplex hc;
        Random rand = new Random();
        List<string> objectlabellist = new List<string>();
        int objectLabelIndex;

        [SetUp]
        public void Setup()
        {
            orchestrator = Orchestrator.GetInstance(true, false, NetworkMode.PREDICTION);
            
            hc = orchestrator.HCAccessor;           
        }

        [Test, Ignore("currently ignore for now")]
        public void TestPredictObject1PositiveTest()
        {
            List<RecognisedVisualEntity> entities = GenerateRecognisedEntity();

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

            hc.SetNetworkModeToPrediction();

            hc.LoadMockObject(entities, true);

            Position2D pos = hc.VerifyObject(source, null, true);

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
            List<RecognisedVisualEntity> entities = GenerateRecognisedEntity();

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


            Position2D pos = hc.VerifyObject(source, null, true);

        }



        [Test]
        public void TestVerify()
        {
            List<RecognisedVisualEntity> entities = TestUtils.GenerateRandomEntities(4);

            hc.LoadMockObject(entities, true);

            RecognisedVisualEntity entity = entities[0];            

            entity.Verify(null, true, 6);            
        }


        [Test, Ignore("Needs a lot more Work")]
        public void TestPreditObject2PositiveTest()
        {
            List<RecognisedVisualEntity> entities = TestUtils.GenerateRandomEntities(4);

            hc.LoadMockObject(entities, true);


            Sensation_Location sensei = entities[1].ObjectSnapshot.ElementAt(2);
            Sensation_Location prediction = entities[1].ObjectSnapshot.ElementAt(3);

            List<string> labelList = new List<string>();

            labelList.Add(entities[1].Label);
            labelList.Add(entities[0].Label);

            List<Position2D> positions = hc.StoreObjectInGraph(sensei, prediction);

            Assert.IsTrue(positions.Count != 0);
        }

        [Test]
        public void TestAddingDuplicateSensationOnlyFails()
        {
            var newSensation = GenerateNewSensationforTextualObject(2);

            orchestrator.ChangeNetworkModeToTraining();



            Assert.IsTrue(hc.AddNewSensationToObject(newSensation));

            Assert.IsFalse(hc.AddNewSensationToObject(newSensation));
        }

        private Sensation GenerateNewSensationforTextualObject(int maxBBM, int numPositions = 4)
        {
            Random rand = new Random();            
            List<Position_SOM> posList = new();


            for (int i = 0; i < numPositions; i++)
            {
                posList.Add(new Position_SOM(rand.Next(10), rand.Next(10)));
            }

            Sensation newSensation = new Sensation(maxBBM, posList);

            return newSensation;
        }
        
        private List<RecognisedVisualEntity> GenerateRecognisedEntity()
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

            List<RecognisedVisualEntity> recgs = new List<RecognisedVisualEntity>()
             {
                 new RecognisedVisualEntity("Banana")
                 {
                     ObjectSnapshot = new List<Sensation_Location>
                     {
                        new Sensation_Location(dict1, new Position2D(4432, 2163)),
                        new Sensation_Location(dict2, new Position2D(111, 888))
                     }
                 },
                 new RecognisedVisualEntity("Ananas")
                 {
                     ObjectSnapshot = new List<Sensation_Location>
                     {
                        new Sensation_Location(dict2, new Position2D(111, 888))
                     }
                 },
                 new RecognisedVisualEntity("Watermelon")
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
