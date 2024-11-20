namespace Hentul.UT
{
    using Hentul;
    using Common;
    using Hentul.Hippocampal_Entorinal_complex;
    using System.Text;
    using FirstOrderMemory.Models;

    public  class OrchestratorTests
    {
        Orchestrator orchetrator;

        [SetUp]
        public void Setup()
        {
            orchetrator = new Orchestrator(20, true, true, 1);
        }

        [Test]
        public void TestMapperGetSenseLocFromSDR_SOM()
        {

            SDR_SOM sdr = new SDR_SOM(
                1000,
                10,
                new List<Position_SOM>()
                {
                    new Position_SOM(777, 10),
                    new Position_SOM(555, 10),
                    new Position_SOM(111, 10),
                    new Position_SOM(243, 10)
                }
                );

            var sensloc = orchetrator.Mapper.GetSensationLocationFromSDR( sdr );

            //Check for the correct bbmID is firing and the correct position key 
            //no need to validate positions since it will be the same.

        }


        [Test]
        public void TestSenseiMatchPercentagePositiveTest()
        {
            string positionID = "33-44-2-N";
            List<Position_SOM> activeBits = new List<Position_SOM>()
                    {
                        new Position_SOM(777, 10),
                        new Position_SOM(555, 10),
                        new Position_SOM(111, 10),
                        new Position_SOM(243, 10)
                    };

            KeyValuePair<int, List<Position_SOM>> kvp = new KeyValuePair<int, List<Position_SOM>>(55, activeBits);

            SDR_SOM activeSDR = new SDR_SOM(1000, 10, activeBits, iType.SPATIAL);

            Sensation_Location sensei1 = new Sensation_Location();

            sensei1.AddNewSensationAtThisLocation(positionID, kvp);

            Sensation_Location sensei2 = orchetrator.Mapper.GetSensationLocationFromSDR(activeSDR);
            

            Assert.AreEqual(100, Sensation_Location.CompareSenseiPercentage(sensei1, sensei2));

        }

        [Test]
        public void TestSenseiMatchPercentageNegativeTest()
        {
            Sensation_Location sensei1 = new Sensation_Location();
            Sensation_Location sensei2 = new Sensation_Location();

            //sensei1.AddNewSensationAtThisLocation(new Position)

            Assert.AreEqual(100, Sensation_Location.CompareSenseiPercentage(sensei1, sensei2));

        }
    }
}
