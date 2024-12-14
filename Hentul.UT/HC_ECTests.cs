namespace Hentul.UT
{
    using Hentul.Hippocampal_Entorinal_complex;
    using FirstOrderMemory.Models;
    using Common;

    public class HC_ECTests
    {
        Orchestrator orchestrator;
        HippocampalComplex hc;

        [SetUp]
        public void Setup()
        {
            orchestrator = Orchestrator.GetInstance(true, false, NetworkMode.PREDICTION);
            hc = orchestrator.HCAccessor;
        }


        [Test]
        public void TestProcessCurrentPatternForObjectPositiveTest()
        {
            List<RecognisedEntity> entities = GenerateRecognisedEntity();

            List<Position_SOM> activeBits1 = new List<Position_SOM>()
                    {
                        new Position_SOM(777, 10),
                        new Position_SOM(222, 10),
                        new Position_SOM(333, 10),
                        new Position_SOM(444, 10)
                    };
           
            List<Position_SOM> activeBits3 = new List<Position_SOM>()
                    {
                        new Position_SOM(111, 10),
                        new Position_SOM(555, 10)
                    };
          
            List<Position_SOM> activeBits5 = new List<Position_SOM>()
                    {
                        new Position_SOM(243, 10),
                        new Position_SOM(234, 4),
                        new Position_SOM(464, 5),
                        new Position_SOM(33, 66),
                        new Position_SOM(22, 10)
                    };

            KeyValuePair<int, List<Position_SOM>> kvp1 = new KeyValuePair<int, List<Position_SOM>>(77, activeBits1);
            KeyValuePair<int, List<Position_SOM>> kvp2 = new KeyValuePair<int, List<Position_SOM>>(11, activeBits3);
            KeyValuePair<int, List<Position_SOM>> kvp3 = new KeyValuePair<int, List<Position_SOM>>(24, activeBits5);

            SortedDictionary<string, KeyValuePair<int, List<Position_SOM>>> dict1 = new SortedDictionary<string, KeyValuePair<int, List<Position_SOM>>>();

            dict1.Add("111-888-0", kvp1);
            dict1.Add("234-456-0", kvp2);
            dict1.Add("567-343-0", kvp3);

            Sensation_Location source = new Sensation_Location(dict1);

            hc.LoadMockObject(entities);

            Position pos = hc.ProcessCurrentPatternForObject(1, source, null);

            Assert.AreEqual(int.MaxValue, pos.X);
            Assert.AreEqual(int.MaxValue, pos.Y);

        }


        private List<RecognisedEntity> GenerateRecognisedEntity()
        {
            List<Position_SOM> activeBits1 = new List<Position_SOM>()
                    {
                        new Position_SOM(777, 10),
                        new Position_SOM(222, 10),
                        new Position_SOM(333, 10),
                        new Position_SOM(444, 10)
                    };
            List<Position_SOM> activeBits2 = new List<Position_SOM>()
                    {
                        new Position_SOM(555, 3333),
                        new Position_SOM(546, 3234),
                        new Position_SOM(898, 532)
                    };
            List<Position_SOM> activeBits3 = new List<Position_SOM>()
                    {
                        new Position_SOM(111, 10),
                        new Position_SOM(555, 10)
                    };
            List<Position_SOM> activeBits4 = new List<Position_SOM>()
                    {
                        new Position_SOM(243, 10),
                        new Position_SOM(234, 4),
                        new Position_SOM(464, 5),
                        new Position_SOM(33, 66),
                        new Position_SOM(22, 10)
                    };
            List<Position_SOM> activeBits5 = new List<Position_SOM>()
                    {
                        new Position_SOM(243, 10),
                        new Position_SOM(234, 4),
                        new Position_SOM(464, 5),
                        new Position_SOM(33, 66),
                        new Position_SOM(22, 10)
                    };

            KeyValuePair<int, List<Position_SOM>> kvp1 = new KeyValuePair<int, List<Position_SOM>>(77, activeBits1);
            KeyValuePair<int, List<Position_SOM>> kvp2 = new KeyValuePair<int, List<Position_SOM>>(55, activeBits2);
            KeyValuePair<int, List<Position_SOM>> kvp3 = new KeyValuePair<int, List<Position_SOM>>(11, activeBits3);
            KeyValuePair<int, List<Position_SOM>> kvp4 = new KeyValuePair<int, List<Position_SOM>>(54, activeBits4);
            KeyValuePair<int, List<Position_SOM>> kvp5 = new KeyValuePair<int, List<Position_SOM>>(24, activeBits5);

            SortedDictionary<string, KeyValuePair<int, List<Position_SOM>>> dict1 = new SortedDictionary<string, KeyValuePair<int, List<Position_SOM>>>();

            dict1.Add("4432-2163-0", kvp1);
            dict1.Add("2332-4463-0", kvp2);
            dict1.Add("3432-8963-0", kvp3);
            dict1.Add("5632-7663-0", kvp4);

            

            SortedDictionary<string, KeyValuePair<int, List<Position_SOM>>> dict2 = new SortedDictionary<string, KeyValuePair<int, List<Position_SOM>>>();

            dict2.Add("111-888-0", kvp1);      
            dict2.Add("234-456-0", kvp3);
            dict2.Add("567-343-0", kvp5);

            SortedDictionary<string, KeyValuePair<int, List<Position_SOM>>> dict3 = new SortedDictionary<string, KeyValuePair<int, List<Position_SOM>>>();

            dict3.Add("345-219-0", kvp2);
            dict3.Add("567-8963-0", kvp4);
            dict3.Add("345-4567-0", kvp1);

            List<RecognisedEntity> recgs = new List<RecognisedEntity>()
             {
                 new RecognisedEntity("Apple")
                 {
                     ObjectSnapshot = new List<Sensation_Location>
                     {
                        new Sensation_Location(dict1),
                        new Sensation_Location(dict2)
                     }
                 },
                 new RecognisedEntity("Ananas")
                 {
                     ObjectSnapshot = new List<Sensation_Location>
                     {
                        new Sensation_Location(dict2)
                     }
                 },
                 new RecognisedEntity("Watermelon")
                 {
                     ObjectSnapshot = new List<Sensation_Location>
                     {
                        new Sensation_Location(dict1),
                        new Sensation_Location(dict3)
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
