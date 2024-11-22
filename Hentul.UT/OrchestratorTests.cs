namespace Hentul.UT
{
    using Hentul;
    using Common;
    using Hentul.Hippocampal_Entorinal_complex;    
    using FirstOrderMemory.Models;
    using static Hentul.Orchestrator;

    public  class OrchestratorTests
    {
        Orchestrator orchetrator;
        POINT point = new POINT();        

        [SetUp]
        public void Setup()
        {
            orchetrator = new Orchestrator(10, true, true, 1);
            point.X = 10;
            point.Y = 10;
        }

        [Test, Description("Test BBM ID uniqueness and correct allocation of kvps!")]
        public void TestMapperGetSenseLocFromSDR_SOM()
        {

            List<Position_SOM> posList = new List<Position_SOM>()
                {
                    new Position_SOM(777, 10),
                    new Position_SOM(555, 10),
                    new Position_SOM(111, 10),
                    new Position_SOM(243, 10)
                };

            SDR_SOM sdr = new SDR_SOM(
                1000,
                10,
                posList
                );            

            var sensloc = orchetrator.Mapper.GetSensationLocationFromSDR( sdr, point);

            int index = 0;

            foreach (var kvp in sensloc.sensLoc.Values)
            {
                Assert.AreEqual(posList[index].X / 10, kvp.Key);
                Assert.AreEqual(posList[index], kvp.Value[0]);
                index++;
            }
        }


        [Test, Description("Test all the Active bits under one BBM getss added to the ame BBM KVP.")]
        public void TestMapperGetSenseLocFromSDR_SOM1()
        {            
            SDR_SOM sdr = new SDR_SOM(
                1000,
                10,
                new List<Position_SOM>()
                {
                    new Position_SOM(0, 9),
                    new Position_SOM(1, 1),
                    new Position_SOM(2, 1),
                    new Position_SOM(3, 2)
                }
                );

            SDR_SOM sdr1 = new SDR_SOM(
                1000,
                10,
                new List<Position_SOM>()
                {
                    new Position_SOM(999, 9),
                    new Position_SOM(0, 1),
                    new Position_SOM(555, 1),
                    new Position_SOM(777, 2)
                }
                );

            var sensloc = orchetrator.Mapper.GetSensationLocationFromSDR(sdr, point);

            Assert.AreEqual(1, sensloc.sensLoc.Count);
            Assert.AreEqual(4, sensloc.sensLoc.ElementAt(0).Value.Value.Count);


            var sensloc1 = orchetrator.Mapper.GetSensationLocationFromSDR(sdr1, point);

            Assert.AreEqual(4, sensloc1.sensLoc.Count);
            Assert.AreEqual(1, sensloc1.sensLoc.ElementAt(0).Value.Value.Count);

            //Check for the correct bbmID is firing and the correct position key 
            //no need to validate positions since it will be the same.

        }

        [Test, Description("Tests GetLocationFromPosition method in the Code!")]
        public void TestMapperGetSenseLocFromSDR_SOM3()
        {
            
            List<Position_SOM> posList = new List<Position_SOM>()
                {
                    new Position_SOM(777, 10),
                    new Position_SOM(555, 10),
                    new Position_SOM(111, 10),
                    new Position_SOM(243, 10)
                };

            SDR_SOM sdr = new SDR_SOM(
                1000,
                10,
                posList
                );

            point.X = 1340;
            point.Y = 899;

            var sensloc = orchetrator.Mapper.GetSensationLocationFromSDR(sdr, point);
            

            foreach (var kvp in sensloc.sensLoc)
            {
                Position p = Position.ConvertStringToPosition(kvp.Key);
                Assert.AreEqual(point.X + GetXFromBBM_ID(kvp.Value.Key), p.X);
                Assert.AreEqual(point.Y + GetYFromBBM_ID(kvp.Value.Key), p.Y);                                
            }

        }


        [Test, Description("Tests CompareSenseiMatchPercentage for 2 Sensei's based purely on Location for Positive Outcome!")]
        public void TestCompareSenseiMatchPercentagePositiveTestForLocationIDOnly()
        {
            string positionID1 = "40-24-0";
            string positionID2 = "32-20-0";
            string positionID3 = "14-12-0";
            string positionID4 = "18-18-0";

            List<Position_SOM> activeBits1 = new List<Position_SOM>()
                    {
                        new Position_SOM(777, 10)
                    };
            List<Position_SOM> activeBits2 = new List<Position_SOM>()
                    {
                        new Position_SOM(555, 10)
                    };
            List<Position_SOM> activeBits3 = new List<Position_SOM>()
                    {
                        new Position_SOM(111, 10)
                    };
            List<Position_SOM> activeBits4 = new List<Position_SOM>()
                    {
                        new Position_SOM(243, 10)
                    };

            KeyValuePair<int, List<Position_SOM>> kvp1 = new KeyValuePair<int, List<Position_SOM>>(7, activeBits1);
            KeyValuePair<int, List<Position_SOM>> kvp2 = new KeyValuePair<int, List<Position_SOM>>(5, activeBits2);
            KeyValuePair<int, List<Position_SOM>> kvp3 = new KeyValuePair<int, List<Position_SOM>>(1, activeBits3);
            KeyValuePair<int, List<Position_SOM>> kvp4 = new KeyValuePair<int, List<Position_SOM>>(4, activeBits4);

            SDR_SOM activeSDR1 = new SDR_SOM(1000, 10, activeBits1, iType.SPATIAL);
            SDR_SOM activeSDR2 = new SDR_SOM(1000, 10, activeBits1, iType.SPATIAL);
            SDR_SOM activeSDR3 = new SDR_SOM(1000, 10, activeBits1, iType.SPATIAL);
            SDR_SOM activeSDR4 = new SDR_SOM(1000, 10, activeBits1, iType.SPATIAL);

            Sensation_Location sensei1 = new Sensation_Location();

            sensei1.AddNewSensationAtThisLocation(positionID1, kvp1);
            sensei1.AddNewSensationAtThisLocation(positionID2, kvp2);
            sensei1.AddNewSensationAtThisLocation(positionID3, kvp3);
            sensei1.AddNewSensationAtThisLocation(positionID4, kvp4);

            List<Position_SOM> activeBits = new List<Position_SOM>();

            activeBits.AddRange(activeBits1);
            activeBits.AddRange(activeBits2);
            activeBits.AddRange(activeBits3);
            activeBits.AddRange(activeBits4);

            SDR_SOM activeSDR = new SDR_SOM(1000, 10, activeBits, iType.SPATIAL);

            Sensation_Location sensei2 = orchetrator.Mapper.GetSensationLocationFromSDR(activeSDR, point);

            Match match = Sensation_Location.CompareSenseiPercentage(sensei1, sensei2, false, true);

            Assert.AreEqual(4, match.NumberOfLocationIDMatches);
            Assert.AreEqual(0, match.NumberOfBBMIDMatches);
            Assert.AreEqual(0, match.GetTotalMatchPercentage());

        }

        [Test]
        public void TestCompareSenseiMatchPercentageNegativeTestForLocationIDOnly()
        {
            string positionID1 = "40-24-0-N";
            string positionID2 = "32-44-2-N";
            string positionID3 = "33-44-2-N";
            string positionID4 = "34-44-2-N";

            List<Position_SOM> activeBits1 = new List<Position_SOM>()
                    {
                        new Position_SOM(777, 10)
                    };
            List<Position_SOM> activeBits2 = new List<Position_SOM>()
                    {
                        new Position_SOM(555, 10)
                    };
            List<Position_SOM> activeBits3 = new List<Position_SOM>()
                    {
                        new Position_SOM(111, 10)
                    };
            List<Position_SOM> activeBits4 = new List<Position_SOM>()
                    {
                        new Position_SOM(243, 10)
                    };

            KeyValuePair<int, List<Position_SOM>> kvp1 = new KeyValuePair<int, List<Position_SOM>>(77, activeBits1);
            KeyValuePair<int, List<Position_SOM>> kvp2 = new KeyValuePair<int, List<Position_SOM>>(55, activeBits2);
            KeyValuePair<int, List<Position_SOM>> kvp3 = new KeyValuePair<int, List<Position_SOM>>(11, activeBits3);
            KeyValuePair<int, List<Position_SOM>> kvp4 = new KeyValuePair<int, List<Position_SOM>>(24, activeBits4);

            SDR_SOM activeSDR1 = new SDR_SOM(1000, 10, activeBits1, iType.SPATIAL);
            SDR_SOM activeSDR2 = new SDR_SOM(1000, 10, activeBits1, iType.SPATIAL);
            SDR_SOM activeSDR3 = new SDR_SOM(1000, 10, activeBits1, iType.SPATIAL);
            SDR_SOM activeSDR4 = new SDR_SOM(1000, 10, activeBits1, iType.SPATIAL);

            Sensation_Location sensei1 = new Sensation_Location();

            sensei1.AddNewSensationAtThisLocation(positionID1, kvp1);
            sensei1.AddNewSensationAtThisLocation(positionID2, kvp2);
            sensei1.AddNewSensationAtThisLocation(positionID3, kvp3);
            sensei1.AddNewSensationAtThisLocation(positionID4, kvp4);

            List<Position_SOM> activeBits = new List<Position_SOM>();

            activeBits.AddRange(activeBits1);
            activeBits.AddRange(activeBits2);
            activeBits.AddRange(activeBits3);
            activeBits.AddRange(activeBits4);

            SDR_SOM activeSDR = new SDR_SOM(1000, 10, activeBits, iType.SPATIAL);

            Sensation_Location sensei2 = orchetrator.Mapper.GetSensationLocationFromSDR(activeSDR, point);

            Match match = Sensation_Location.CompareSenseiPercentage(sensei1, sensei2, false, true);

            Assert.AreEqual(4, match.NumberOfLocationIDMisses);
            Assert.AreEqual(0, match.NumberOfBBMIDMises);
            Assert.AreEqual(100, match.GetTotalMatchPercentage());

        }

        [Test, Description("Tests CompareSenseiMatchPercentage for 2 Sensei's based purely on BBM ID for Positive Outcome!")]
        public void TestCompareSenseiMatchPercentagePositiveTestForBBMIDOnly()
        {
            string positionID1 = "12-23-0-N";
            string positionID2 = "13-33-2-N";
            string positionID3 = "14-43-2-N";
            string positionID4 = "15-77-2-N";

            List<Position_SOM> activeBits1 = new List<Position_SOM>()
                    {
                        new Position_SOM(777, 10)                     
                    };
            List<Position_SOM> activeBits2 = new List<Position_SOM>()
                    {
                        new Position_SOM(555, 10)                        
                    };
            List<Position_SOM> activeBits3 = new List<Position_SOM>()
                    {                        
                        new Position_SOM(111, 10)
                    };
            List<Position_SOM> activeBits4 = new List<Position_SOM>()
                    {                        
                        new Position_SOM(243, 10)
                    };

            KeyValuePair<int, List<Position_SOM>> kvp1 = new KeyValuePair<int, List<Position_SOM>>(77, activeBits1);
            KeyValuePair<int, List<Position_SOM>> kvp2 = new KeyValuePair<int, List<Position_SOM>>(55, activeBits2);
            KeyValuePair<int, List<Position_SOM>> kvp3 = new KeyValuePair<int, List<Position_SOM>>(11, activeBits3);
            KeyValuePair<int, List<Position_SOM>> kvp4 = new KeyValuePair<int, List<Position_SOM>>(24, activeBits4);

            SDR_SOM activeSDR1 = new SDR_SOM(1000, 10, activeBits1, iType.SPATIAL);
            SDR_SOM activeSDR2 = new SDR_SOM(1000, 10, activeBits1, iType.SPATIAL);
            SDR_SOM activeSDR3 = new SDR_SOM(1000, 10, activeBits1, iType.SPATIAL);
            SDR_SOM activeSDR4 = new SDR_SOM(1000, 10, activeBits1, iType.SPATIAL);

            Sensation_Location sensei1 = new Sensation_Location();

            sensei1.AddNewSensationAtThisLocation(positionID1, kvp1);
            sensei1.AddNewSensationAtThisLocation(positionID2, kvp2);
            sensei1.AddNewSensationAtThisLocation(positionID3, kvp3);
            sensei1.AddNewSensationAtThisLocation(positionID4, kvp4);

            List<Position_SOM> activeBits = new List<Position_SOM>();

            activeBits.AddRange(activeBits1);
            activeBits.AddRange(activeBits2);
            activeBits.AddRange(activeBits3);
            activeBits.AddRange(activeBits4);

            SDR_SOM activeSDR = new SDR_SOM(1000, 10, activeBits, iType.SPATIAL);

            Sensation_Location sensei2 = orchetrator.Mapper.GetSensationLocationFromSDR(activeSDR, point);

            Assert.AreEqual(100, Sensation_Location.CompareSenseiPercentage(sensei1, sensei2, true, false).GetTotalMatchPercentage());

        }


        [Test, Description("Tests CompareSenseiMatchPercentage for 2 Sensei's based purely on BBM ID for Negative Outcome!")]
        public void TestCompareSenseiMatchPercentageNegativeTestForBBMIDOnly()
        {
            string positionID1 = "40-24-0-N";
            string positionID2 = "32-44-2-N";
            string positionID3 = "33-44-2-N";
            string positionID4 = "34-44-2-N";

            List<Position_SOM> activeBits1 = new List<Position_SOM>()
                    {
                        new Position_SOM(777, 10)
                    };
            List<Position_SOM> activeBits2 = new List<Position_SOM>()
                    {
                        new Position_SOM(555, 10)
                    };
            List<Position_SOM> activeBits3 = new List<Position_SOM>()
                    {
                        new Position_SOM(111, 10)
                    };
            List<Position_SOM> activeBits4 = new List<Position_SOM>()
                    {
                        new Position_SOM(243, 10)
                    };

            KeyValuePair<int, List<Position_SOM>> kvp1 = new KeyValuePair<int, List<Position_SOM>>(1, activeBits1);
            KeyValuePair<int, List<Position_SOM>> kvp2 = new KeyValuePair<int, List<Position_SOM>>(2, activeBits2);
            KeyValuePair<int, List<Position_SOM>> kvp3 = new KeyValuePair<int, List<Position_SOM>>(3, activeBits3);
            KeyValuePair<int, List<Position_SOM>> kvp4 = new KeyValuePair<int, List<Position_SOM>>(4, activeBits4);

            SDR_SOM activeSDR1 = new SDR_SOM(1000, 10, activeBits1, iType.SPATIAL);
            SDR_SOM activeSDR2 = new SDR_SOM(1000, 10, activeBits1, iType.SPATIAL);
            SDR_SOM activeSDR3 = new SDR_SOM(1000, 10, activeBits1, iType.SPATIAL);
            SDR_SOM activeSDR4 = new SDR_SOM(1000, 10, activeBits1, iType.SPATIAL);

            Sensation_Location sensei1 = new Sensation_Location();

            sensei1.AddNewSensationAtThisLocation(positionID1, kvp1);
            sensei1.AddNewSensationAtThisLocation(positionID2, kvp2);
            sensei1.AddNewSensationAtThisLocation(positionID3, kvp3);
            sensei1.AddNewSensationAtThisLocation(positionID4, kvp4);

            List<Position_SOM> activeBits = new List<Position_SOM>();

            activeBits.AddRange(activeBits1);
            activeBits.AddRange(activeBits2);
            activeBits.AddRange(activeBits3);
            activeBits.AddRange(activeBits4);

            SDR_SOM activeSDR = new SDR_SOM(1000, 10, activeBits, iType.SPATIAL);

            Sensation_Location sensei2 = orchetrator.Mapper.GetSensationLocationFromSDR(activeSDR, point);

            Assert.AreEqual(0, Sensation_Location.CompareSenseiPercentage(sensei1, sensei2, true, false).GetTotalMatchPercentage());

        }

        [Test, Description("Tests CompareSenseiMatchPercentage for 2 Sensei's based on Location && BBM ID for Positive Outcome!")]
        public void TestCompareSenseiMatchPercentagePositiveTestForBBMID_LocationIDOnly()
        {
            string positionID1 = "40-24-0-N";
            string positionID2 = "32-44-2-N";
            string positionID3 = "33-44-2-N";
            string positionID4 = "34-44-2-N";

            List<Position_SOM> activeBits1 = new List<Position_SOM>()
                    {
                        new Position_SOM(777, 10)
                    };
            List<Position_SOM> activeBits2 = new List<Position_SOM>()
                    {
                        new Position_SOM(555, 10)
                    };
            List<Position_SOM> activeBits3 = new List<Position_SOM>()
                    {
                        new Position_SOM(111, 10)
                    };
            List<Position_SOM> activeBits4 = new List<Position_SOM>()
                    {
                        new Position_SOM(243, 10)
                    };

            KeyValuePair<int, List<Position_SOM>> kvp1 = new KeyValuePair<int, List<Position_SOM>>(77, activeBits1);
            KeyValuePair<int, List<Position_SOM>> kvp2 = new KeyValuePair<int, List<Position_SOM>>(55, activeBits2);
            KeyValuePair<int, List<Position_SOM>> kvp3 = new KeyValuePair<int, List<Position_SOM>>(11, activeBits3);
            KeyValuePair<int, List<Position_SOM>> kvp4 = new KeyValuePair<int, List<Position_SOM>>(24, activeBits4);

            SDR_SOM activeSDR1 = new SDR_SOM(1000, 10, activeBits1, iType.SPATIAL);
            SDR_SOM activeSDR2 = new SDR_SOM(1000, 10, activeBits1, iType.SPATIAL);
            SDR_SOM activeSDR3 = new SDR_SOM(1000, 10, activeBits1, iType.SPATIAL);
            SDR_SOM activeSDR4 = new SDR_SOM(1000, 10, activeBits1, iType.SPATIAL);

            Sensation_Location sensei1 = new Sensation_Location();

            sensei1.AddNewSensationAtThisLocation(positionID1, kvp1);
            sensei1.AddNewSensationAtThisLocation(positionID2, kvp2);
            sensei1.AddNewSensationAtThisLocation(positionID3, kvp3);
            sensei1.AddNewSensationAtThisLocation(positionID4, kvp4);

            List<Position_SOM> activeBits = new List<Position_SOM>();

            activeBits.AddRange(activeBits1);
            activeBits.AddRange(activeBits2);
            activeBits.AddRange(activeBits3);
            activeBits.AddRange(activeBits4);

            SDR_SOM activeSDR = new SDR_SOM(1000, 10, activeBits, iType.SPATIAL);

            Sensation_Location sensei2 = orchetrator.Mapper.GetSensationLocationFromSDR(activeSDR, point);

            Assert.AreEqual(100, Sensation_Location.CompareSenseiPercentage(sensei1, sensei2, true, false).GetTotalMatchPercentage());

        }

        [Test, Description("Tests CompareSenseiMatchPercentage for 2 Sensei's based on Location && BBM ID for Negative Outcome!")]
        public void TestCompareSenseiMatchPercentageNegativeTestForBBMID_LocationIDOnly()
        {
            string positionID1 = "12-23-0-N";
            string positionID2 = "13-33-2-N";
            string positionID3 = "14-43-2-N";
            string positionID4 = "15-77-2-N";

            List<Position_SOM> activeBits1 = new List<Position_SOM>()
                    {
                        new Position_SOM(777, 10)
                    };
            List<Position_SOM> activeBits2 = new List<Position_SOM>()
                    {
                        new Position_SOM(555, 10)
                    };
            List<Position_SOM> activeBits3 = new List<Position_SOM>()
                    {
                        new Position_SOM(111, 10)
                    };
            List<Position_SOM> activeBits4 = new List<Position_SOM>()
                    {
                        new Position_SOM(243, 10)
                    };

            KeyValuePair<int, List<Position_SOM>> kvp1 = new KeyValuePair<int, List<Position_SOM>>(1, activeBits1);
            KeyValuePair<int, List<Position_SOM>> kvp2 = new KeyValuePair<int, List<Position_SOM>>(2, activeBits2);
            KeyValuePair<int, List<Position_SOM>> kvp3 = new KeyValuePair<int, List<Position_SOM>>(3, activeBits3);
            KeyValuePair<int, List<Position_SOM>> kvp4 = new KeyValuePair<int, List<Position_SOM>>(4, activeBits4);

            SDR_SOM activeSDR1 = new SDR_SOM(1000, 10, activeBits1, iType.SPATIAL);
            SDR_SOM activeSDR2 = new SDR_SOM(1000, 10, activeBits1, iType.SPATIAL);
            SDR_SOM activeSDR3 = new SDR_SOM(1000, 10, activeBits1, iType.SPATIAL);
            SDR_SOM activeSDR4 = new SDR_SOM(1000, 10, activeBits1, iType.SPATIAL);

            Sensation_Location sensei1 = new Sensation_Location();

            sensei1.AddNewSensationAtThisLocation(positionID1, kvp1);
            sensei1.AddNewSensationAtThisLocation(positionID2, kvp2);
            sensei1.AddNewSensationAtThisLocation(positionID3, kvp3);
            sensei1.AddNewSensationAtThisLocation(positionID4, kvp4);

            List<Position_SOM> activeBits = new List<Position_SOM>();

            activeBits.AddRange(activeBits1);
            activeBits.AddRange(activeBits2);
            activeBits.AddRange(activeBits3);
            activeBits.AddRange(activeBits4);

            SDR_SOM activeSDR = new SDR_SOM(1000, 10, activeBits, iType.SPATIAL);

            Sensation_Location sensei2 = orchetrator.Mapper.GetSensationLocationFromSDR(activeSDR, point);

            Match match = Sensation_Location.CompareSenseiPercentage(sensei1, sensei2, true, true);

            Assert.AreEqual(0, match.NumberOfLocationIDMatches);
            Assert.AreEqual(0, match.GetTotalMatchPercentage());

        }

        [Test, Description("Tests CompareSenseiMatchPercentage for 2 Sensei's based on Location && BBM ID && Position List for Positive Outcome!")]
        public void TestCompareSenseiMatchPercentagePositiveTestForBBMID_LocationID_PositionList()
        {
            string positionID1 = "40-24-0";
            string positionID2 = "32-20-0";
            string positionID3 = "14-12-0";
            string positionID4 = "18-18-0";

            List<Position_SOM> activeBits1 = new List<Position_SOM>()
                    {
                        new Position_SOM(777, 10)
                    };
            List<Position_SOM> activeBits2 = new List<Position_SOM>()
                    {
                        new Position_SOM(555, 10)
                    };
            List<Position_SOM> activeBits3 = new List<Position_SOM>()
                    {
                        new Position_SOM(111, 10)
                    };
            List<Position_SOM> activeBits4 = new List<Position_SOM>()
                    {
                        new Position_SOM(243, 10)
                    };

            KeyValuePair<int, List<Position_SOM>> kvp1 = new KeyValuePair<int, List<Position_SOM>>(77, activeBits1);
            KeyValuePair<int, List<Position_SOM>> kvp2 = new KeyValuePair<int, List<Position_SOM>>(55, activeBits2);
            KeyValuePair<int, List<Position_SOM>> kvp3 = new KeyValuePair<int, List<Position_SOM>>(11, activeBits3);
            KeyValuePair<int, List<Position_SOM>> kvp4 = new KeyValuePair<int, List<Position_SOM>>(24, activeBits4);

            SDR_SOM activeSDR1 = new SDR_SOM(1000, 10, activeBits1, iType.SPATIAL);
            SDR_SOM activeSDR2 = new SDR_SOM(1000, 10, activeBits1, iType.SPATIAL);
            SDR_SOM activeSDR3 = new SDR_SOM(1000, 10, activeBits1, iType.SPATIAL);
            SDR_SOM activeSDR4 = new SDR_SOM(1000, 10, activeBits1, iType.SPATIAL);

            Sensation_Location sensei1 = new Sensation_Location();

            sensei1.AddNewSensationAtThisLocation(positionID1, kvp1);
            sensei1.AddNewSensationAtThisLocation(positionID2, kvp2);
            sensei1.AddNewSensationAtThisLocation(positionID3, kvp3);
            sensei1.AddNewSensationAtThisLocation(positionID4, kvp4);

            List<Position_SOM> activeBits = new List<Position_SOM>();

            activeBits.AddRange(activeBits1);
            activeBits.AddRange(activeBits2);
            activeBits.AddRange(activeBits3);
            activeBits.AddRange(activeBits4);

            SDR_SOM activeSDR = new SDR_SOM(1000, 10, activeBits, iType.SPATIAL);

            Sensation_Location sensei2 = orchetrator.Mapper.GetSensationLocationFromSDR(activeSDR, point);

            Match match = Sensation_Location.CompareSenseiPercentage(sensei1, sensei2, true, true);

            Assert.AreEqual(4, match.NumberOfLocationIDMatches);
            Assert.AreEqual(4, match.NumberOfBBMIDMatches);
            Assert.AreEqual(100, match.GetTotalMatchPercentage());

        }

        [Test, Description("Tests CompareSenseiMatchPercentage for 2 Sensei's based on Location && BBM ID && Position Lit for Negative Outcome!")]
        public void TestCompareSenseiMatchPercentageNegativeTestForBBMID_LocationID_PositionList()
        {
            string positionID1 = "40-24-0-N";
            string positionID2 = "32-44-2-N";
            string positionID3 = "33-44-2-N";
            string positionID4 = "34-44-2-N";

            List<Position_SOM> activeBits1 = new List<Position_SOM>()
                    {
                        new Position_SOM(777, 10)
                    };
            List<Position_SOM> activeBits2 = new List<Position_SOM>()
                    {
                        new Position_SOM(555, 10)
                    };
            List<Position_SOM> activeBits3 = new List<Position_SOM>()
                    {
                        new Position_SOM(111, 10)
                    };
            List<Position_SOM> activeBits4 = new List<Position_SOM>()
                    {
                        new Position_SOM(243, 10)
                    };

            KeyValuePair<int, List<Position_SOM>> kvp1 = new KeyValuePair<int, List<Position_SOM>>(1, activeBits1);
            KeyValuePair<int, List<Position_SOM>> kvp2 = new KeyValuePair<int, List<Position_SOM>>(2, activeBits2);
            KeyValuePair<int, List<Position_SOM>> kvp3 = new KeyValuePair<int, List<Position_SOM>>(3, activeBits3);
            KeyValuePair<int, List<Position_SOM>> kvp4 = new KeyValuePair<int, List<Position_SOM>>(4, activeBits4);

            SDR_SOM activeSDR1 = new SDR_SOM(1000, 10, activeBits1, iType.SPATIAL);
            SDR_SOM activeSDR2 = new SDR_SOM(1000, 10, activeBits1, iType.SPATIAL);
            SDR_SOM activeSDR3 = new SDR_SOM(1000, 10, activeBits1, iType.SPATIAL);
            SDR_SOM activeSDR4 = new SDR_SOM(1000, 10, activeBits1, iType.SPATIAL);

            Sensation_Location sensei1 = new Sensation_Location();

            sensei1.AddNewSensationAtThisLocation(positionID1, kvp1);
            sensei1.AddNewSensationAtThisLocation(positionID2, kvp2);
            sensei1.AddNewSensationAtThisLocation(positionID3, kvp3);
            sensei1.AddNewSensationAtThisLocation(positionID4, kvp4);

            List<Position_SOM> activeBits = new List<Position_SOM>();

            activeBits.AddRange(activeBits1);
            activeBits.AddRange(activeBits2);
            activeBits.AddRange(activeBits3);
            activeBits.AddRange(activeBits4);

            SDR_SOM activeSDR = new SDR_SOM(1000, 10, activeBits, iType.SPATIAL);

            Sensation_Location sensei2 = orchetrator.Mapper.GetSensationLocationFromSDR(activeSDR, point);

            Match match = Sensation_Location.CompareSenseiPercentage(sensei1, sensei2, true, true);

            Assert.AreEqual(4, match.NumberOfLocationIDMisses);
            Assert.AreEqual(4, match.NumberOfBBMIDMises);
            Assert.AreEqual(0, match.GetTotalMatchPercentage());

        }

        private int GetXFromBBM_ID(int bbmId)
        {
            orchetrator.Mapper.Mappings.TryGetValue(bbmId, out var x);

            return x[0].X;
        }

        private int GetYFromBBM_ID(int bbmId)
        {
            orchetrator.Mapper.Mappings.TryGetValue(bbmId, out var x);

            return x[0].Y;
        }
    }
}
