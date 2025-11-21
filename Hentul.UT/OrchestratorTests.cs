namespace Hentul.UT
{
    using Hentul;
    using Common;
    using Hentul.Hippocampal_Entorinal_complex;
    using static Hentul.Orchestrator;
    using System.Drawing;
    using System.IO;
    using Hentul.Encoders;
    using OpenCvSharp.Text;

    public class OrchestratorTests
    {
        Orchestrator orchestrator;
        POINT point = new POINT();
        string gPos1 = "-4--4-0";
        string gPos2 = "0-0-0";
        string gPos3 = "12-12-0";
        string gPos4 = "18-18-0";

        string bPos1 = "12-23-0-N";
        string bPos2 = "13-33-2-N";
        string bPos3 = "14-43-2-N";
        string bPos4 = "15-77-2-N";
        private static readonly string baseDir = AppContext.BaseDirectory;
        [SetUp]
        public void Setup()
        {
            orchestrator = GetInstance(true, true);
            point.X = 10;
            point.Y = 10;
        }

        [Test]
        public void TestTextInput()
        {
            List<string> wordsToTrain = new List<string>()
            {
                "MYCOMPUTER",
                "RECYCLEBIN",
                "GITBASH",
                "QBSSTUDIO",
                "HENTUL"                
            };

            int count = 0;
            int index = 0;

           for(int i=0; i<wordsToTrain.Count; i++) 
           {
                orchestrator.ChangeNetworkModeToTraining();
                
                string word = wordsToTrain[i];

                foreach (var ch in word)
                {                   
                    orchestrator.AddNewCharacterSensationToHC(ch);
                }

                orchestrator.DoneWithTraining(word);
            }

            Assert.AreEqual(5, orchestrator.HCAccessor.Objects.Count);
        }

        [Test]
        public void TestRemoveDuplicateEntries()
        {
            List<Position_SOM> list = new List<Position_SOM>()
            {
                new Position_SOM(44,33,0),
                new Position_SOM(44,33,0),
                new Position_SOM(22,33,0),
                new Position_SOM(22,33,0),
                new Position_SOM(10,33,0),
                new Position_SOM(10,33,0)
            };

            SDR_SOM sdr = new SDR_SOM(10, 10, list, iType.SPATIAL);

            orchestrator.RemoveDuplicateEntries(ref sdr);

            Assert.AreEqual(3, sdr.ActiveBits.Count);
        }

        [Test]
        public void TestStep1Positive()
        {
            // Provide Positive Greyscaled Image and verify SOM and FOM are Firing.
        }

        [Test]
        public void TestStep1Negative()
        {
            //Provide null input and verify none of the FOMs and SOM are firing.
        }

        [Test]
        public void TestStep2()
        {
            //Verify HC has created an unrecognised Entity with one sensei in it 
        }

        [Test]
        public void TestConvertUnrecognisedObjectToRecognisedObject()
        {
            string obj = "Apple";
            KeyValuePair<int, List<Position2D>> kvp = new KeyValuePair<int, List<Position2D>>(1, new List<Position2D>()
                {
                    new Position2D(1,2),
                    new Position2D(2,3),
                    new Position2D(3,4)
                });

            SortedDictionary<string, KeyValuePair<int, List<Position2D>>> dict = new SortedDictionary<string, KeyValuePair<int, List<Position2D>>>();           

            dict.Add(obj, kvp);

            Sensation_Location sensei = new Sensation_Location(dict, new Position2D(1, 2));            

            orchestrator.HCAccessor.AddNewSensationLocationToObject(sensei);

            orchestrator.DoneWithTraining();

            Assert.AreEqual(1, orchestrator.HCAccessor.Objects.Count);

        }
  
        [Test]
        public void TestMapperFOMBBMPositiveTest()
        {
            //Bitmap bmp = new Bitmap()
            //var senseLoc = orchestrator.Mapper.ParseBitmap()
        }            


        private List<Position_SOM> Conver2DtoSOMList(List<Position2D> somList)
        {
            List<Position_SOM> toReturn = new List<Position_SOM>();

            foreach (var item in somList)
            {
                toReturn.Add(new Position_SOM(item.X, item.Y));
            }

            return toReturn;
        }

        [Test, Ignore("Not Yet Implemented")]
        public void TestGreyScallinOnPixelValue()
        {
            throw new NotImplementedException();
        }
    }
}
