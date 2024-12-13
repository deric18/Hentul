namespace Hentul.UT
{
    using Hentul.Hippocampal_Entorinal_complex;
    using FirstOrderMemory.Models;

    public class HC_ECTests
    {

        HippocampalComplex hc;

        [SetUp]
        public void Setup()
        {
            hc = new HippocampalComplex("Apple", true, Common.NetworkMode.PREDICTION);
        }


        [Test]
        public void TestProcessCurrentPatternForObjectPositiveTest()
        {
            List<RecognisedEntity> entities = GenerateRecognisedEntity();            

            KeyValuePair<int, List<Position_SOM>> kvp1 = new KeyValuePair<int, List<Position_SOM>>(77, new List<Position_SOM>());
            KeyValuePair<int, List<Position_SOM>> kvp2 = new KeyValuePair<int, List<Position_SOM>>(55, new List<Position_SOM>());
            KeyValuePair<int, List<Position_SOM>> kvp3 = new KeyValuePair<int, List<Position_SOM>>(11, new List<Position_SOM>());

            Dictionary<string, KeyValuePair<int, List<Position_SOM>>> dict1 = new Dictionary<string, KeyValuePair<int, List<Position_SOM>>>();

            dict1.Add("111-888", kvp1);
            dict1.Add("234-456", kvp2);
            dict1.Add("567-343", kvp3);

            Sensation_Location source = new Sensation_Location(dict1);

            hc.LoadMockObject(entities);

            hc.ProcessCurrentPatternForObject(1, source, null);

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
            KeyValuePair<int, List<Position_SOM>> kvp4 = new KeyValuePair<int, List<Position_SOM>>(24, activeBits4);
            KeyValuePair<int, List<Position_SOM>> kvp5 = new KeyValuePair<int, List<Position_SOM>>(24, activeBits4);

            Dictionary<string, KeyValuePair<int, List<Position_SOM>>> dict1 = new Dictionary<string, KeyValuePair<int, List<Position_SOM>>>();

            dict1.Add("4432-2163", kvp1);
            dict1.Add("2332-4463", kvp2);
            dict1.Add("3432-8963", kvp3);
            dict1.Add("5632-7663", kvp4);


            Dictionary<string, KeyValuePair<int, List<Position_SOM>>> dict2 = new Dictionary<string, KeyValuePair<int, List<Position_SOM>>>();

            dict1.Add("111-888", kvp1);      
            dict1.Add("234-456", kvp3);
            dict1.Add("567-343", kvp5);

            Dictionary<string, KeyValuePair<int, List<Position_SOM>>> dict3 = new Dictionary<string, KeyValuePair<int, List<Position_SOM>>>();

            dict1.Add("345-219", kvp2);
            dict1.Add("567-8963", kvp4);
            dict1.Add("345-4567", kvp1);

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
                        new Sensation_Location(dict2),
                        new Sensation_Location(dict3)
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



            return recgs;
        }
    }
}
