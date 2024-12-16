namespace Hentul.UT
{
    using Hentul;
    using Common;
    using Hentul.Hippocampal_Entorinal_complex;    
    using FirstOrderMemory.Models;
    using static Hentul.Orchestrator;
    using System.Drawing;

    public  class OrchestratorTests
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

        [SetUp]
        public void Setup()
        {
            orchestrator = GetInstance(true, true);
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

            var sensloc = orchestrator.Mapper.GetSensationLocationFromSDR( sdr, point);

            int index = 0;

            foreach (var kvp in sensloc.sensLoc.Values)
            {
                Assert.AreEqual(posList[index].X / 10, kvp.Key);
                Assert.AreEqual(posList[index], kvp.Value[0]);
                index++;
            }
        }

        [Test]
        public void TestMapperParseBitMap()
        {
            //Bitmap bp = new Bitmap("C:\\Users\\depint\\source\\repos\\Hentul\\Images\\testbmp.png");            

            Bitmap bp = new Bitmap(40, 20);

            bp.SetPixel(34, 18, Color.White);
            bp.SetPixel(34, 19, Color.White);
            bp.SetPixel(35, 18, Color.White);
            bp.SetPixel(35, 9, Color.White);


            orchestrator.Mapper.ParseBitmap(bp);

            int breapoint = 1;

            Assert.AreEqual(MAPPERCASE.ALL, orchestrator.Mapper.FOMBBMIDS.Keys.ElementAt(0));
            Assert.AreEqual(89, orchestrator.Mapper.FOMBBMIDS.Values.ElementAt(0).ElementAt(0));

            int trues = 0;
            int falses = 0;

            for (int i = 0; i < orchestrator.Mapper.testBmpCoverage.GetUpperBound(0); i++)
            {
                for (int j = 0; j < orchestrator.Mapper.testBmpCoverage.GetUpperBound(1); j++)
                {
                    //Assert.AreEqual(true, orchestrator.Mapper.flagCheckArr[i, j]);
                    if (orchestrator.Mapper.testBmpCoverage[i, j])
                    {
                        trues++;
                    }
                    else
                    {
                        falses++;
                        //Assert.False();
                    }

                }
            }

            int bp1 = 1;
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
            KeyValuePair<int, List<Position_SOM>> kvp = new KeyValuePair<int, List<Position_SOM>>(1, new List<Position_SOM>()
                {
                    new Position_SOM(1,2),
                    new Position_SOM(2,3),
                    new Position_SOM(3,4)
                });

            SortedDictionary<string, KeyValuePair<int, List<Position_SOM>>> dict = new SortedDictionary<string, KeyValuePair<int, List<Position_SOM>>>();

            dict.Add(obj, kvp);

            Sensation_Location sensei = new Sensation_Location(dict);


            orchestrator.HCAccessor.ProcessCurrentPatternForObject(1, sensei);


            orchestrator.DoneWithTraining();

            Assert.AreEqual(1, orchestrator.HCAccessor.Objects.Count);

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

            var sensloc = orchestrator.Mapper.GetSensationLocationFromSDR(sdr, point);

            Assert.AreEqual(1, sensloc.sensLoc.Count);
            Assert.AreEqual(4, sensloc.sensLoc.ElementAt(0).Value.Value.Count);


            var sensloc1 = orchestrator.Mapper.GetSensationLocationFromSDR(sdr1, point);

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

            var sensloc = orchestrator.Mapper.GetSensationLocationFromSDR(sdr, point);
            

            foreach (var kvp in sensloc.sensLoc)
            {
                Position p = Position.ConvertStringToPosition(kvp.Key);


                int x = GetXFromBBM_ID(kvp.Value.Key) > 20 ? point.X - GetYFromBBM_ID(kvp.Value.Key) : point.X + GetYFromBBM_ID(kvp.Value.Key);
                int y = GetXFromBBM_ID(kvp.Value.Key) > 20 ? point.Y - GetYFromBBM_ID(kvp.Value.Key) : point.Y + GetYFromBBM_ID(kvp.Value.Key);


                Assert.AreEqual(x, p.X);
                Assert.AreEqual(y, p.Y);
            }

        }

        [Test]
        public void TestMapperFOMBBMPositiveTest()
        {
            //Bitmap bmp = new Bitmap()
            //var senseLoc = orchestrator.Mapper.ParseBitmap()
        }

        [Test, Description("Tests CompareSenseiMatchPercentage for 2 Sensei's based purely on Location for Positive Outcome!")]
        public void TestCompareSenseiMatchPercentagePositiveTestForLocationIDOnly()
        {
            
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

            sensei1.AddNewSensationAtThisLocation(gPos1, kvp1);
            sensei1.AddNewSensationAtThisLocation(gPos2, kvp2);
            sensei1.AddNewSensationAtThisLocation(gPos3, kvp3);
            sensei1.AddNewSensationAtThisLocation(gPos4, kvp4);

            List<Position_SOM> activeBits = new List<Position_SOM>();

            activeBits.AddRange(activeBits1);
            activeBits.AddRange(activeBits2);
            activeBits.AddRange(activeBits3);
            activeBits.AddRange(activeBits4);

            SDR_SOM activeSDR = new SDR_SOM(1000, 10, activeBits, iType.SPATIAL);

            Sensation_Location sensei2 = orchestrator.Mapper.GetSensationLocationFromSDR(activeSDR, point);

            Match match = Sensation_Location.CompareSenseiPercentage(sensei1, sensei2, false, true);

            Assert.AreEqual(4, match.NumberOfLocationIDMatches);
            Assert.AreEqual(0, match.NumberOfBBMIDMatches);
            Assert.AreEqual(0, match.GetTotalMatchPercentage());

        }

        [Test]
        public void TestCompareSenseiMatchPercentageNegativeTestForLocationIDOnly()
        {            
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

            sensei1.AddNewSensationAtThisLocation(bPos1, kvp1);
            sensei1.AddNewSensationAtThisLocation(bPos2, kvp2);
            sensei1.AddNewSensationAtThisLocation(bPos3, kvp3);
            sensei1.AddNewSensationAtThisLocation(bPos4, kvp4);

            List<Position_SOM> activeBits = new List<Position_SOM>();

            activeBits.AddRange(activeBits1);
            activeBits.AddRange(activeBits2);
            activeBits.AddRange(activeBits3);
            activeBits.AddRange(activeBits4);

            SDR_SOM activeSDR = new SDR_SOM(1000, 10, activeBits, iType.SPATIAL);

            Sensation_Location sensei2 = orchestrator.Mapper.GetSensationLocationFromSDR(activeSDR, point);

            Match match = Sensation_Location.CompareSenseiPercentage(sensei1, sensei2, false, true);

            Assert.AreEqual(4, match.NumberOfLocationIDMisses);
            Assert.AreEqual(0, match.NumberOfBBMIDMises);
            Assert.AreEqual(100, match.GetTotalMatchPercentage());

        }

        [Test, Description("Tests CompareSenseiMatchPercentage for 2 Sensei's based purely on BBM ID for Positive Outcome!")]
        public void TestCompareSenseiMatchPercentagePositiveTestForBBMIDOnly()
        {
            
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

            sensei1.AddNewSensationAtThisLocation(gPos1, kvp1);
            sensei1.AddNewSensationAtThisLocation(gPos2, kvp2);
            sensei1.AddNewSensationAtThisLocation(gPos3, kvp3);
            sensei1.AddNewSensationAtThisLocation(gPos4, kvp4);

            List<Position_SOM> activeBits = new List<Position_SOM>();

            activeBits.AddRange(activeBits1);
            activeBits.AddRange(activeBits2);
            activeBits.AddRange(activeBits3);
            activeBits.AddRange(activeBits4);

            SDR_SOM activeSDR = new SDR_SOM(1000, 10, activeBits, iType.SPATIAL);

            Sensation_Location sensei2 = orchestrator.Mapper.GetSensationLocationFromSDR(activeSDR, point);

            Assert.AreEqual(100, Sensation_Location.CompareSenseiPercentage(sensei1, sensei2, true, false).GetTotalMatchPercentage());

        }


        [Test, Description("Tests CompareSenseiMatchPercentage for 2 Sensei's based purely on BBM ID for Negative Outcome!")]
        public void TestCompareSenseiMatchPercentageNegativeTestForBBMIDOnly()
        {
            
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

            sensei1.AddNewSensationAtThisLocation(bPos1, kvp1);
            sensei1.AddNewSensationAtThisLocation(bPos2, kvp2);
            sensei1.AddNewSensationAtThisLocation(bPos3, kvp3);
            sensei1.AddNewSensationAtThisLocation(bPos4, kvp4);

            List<Position_SOM> activeBits = new List<Position_SOM>();

            activeBits.AddRange(activeBits1);
            activeBits.AddRange(activeBits2);
            activeBits.AddRange(activeBits3);
            activeBits.AddRange(activeBits4);

            SDR_SOM activeSDR = new SDR_SOM(1000, 10, activeBits, iType.SPATIAL);

            Sensation_Location sensei2 = orchestrator.Mapper.GetSensationLocationFromSDR(activeSDR, point);

            Assert.AreEqual(0, Sensation_Location.CompareSenseiPercentage(sensei1, sensei2, true, false).GetTotalMatchPercentage());

        }

        [Test, Description("Tests CompareSenseiMatchPercentage for 2 Sensei's based on Location && BBM ID for Positive Outcome!")]
        public void TestCompareSenseiMatchPercentagePositiveTestForBBMID_LocationIDOnly()
        {            
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

            sensei1.AddNewSensationAtThisLocation(gPos1, kvp1);
            sensei1.AddNewSensationAtThisLocation(gPos2, kvp2);
            sensei1.AddNewSensationAtThisLocation(gPos3, kvp3);
            sensei1.AddNewSensationAtThisLocation(gPos4, kvp4);

            List<Position_SOM> activeBits = new List<Position_SOM>();

            activeBits.AddRange(activeBits1);
            activeBits.AddRange(activeBits2);
            activeBits.AddRange(activeBits3);
            activeBits.AddRange(activeBits4);

            SDR_SOM activeSDR = new SDR_SOM(1000, 10, activeBits, iType.SPATIAL);

            Sensation_Location sensei2 = orchestrator.Mapper.GetSensationLocationFromSDR(activeSDR, point);

            Assert.AreEqual(100, Sensation_Location.CompareSenseiPercentage(sensei1, sensei2, true, false).GetTotalMatchPercentage());

        }

        [Test, Description("Tests CompareSenseiMatchPercentage for 2 Sensei's based on Location && BBM ID for Negative Outcome!")]
        public void TestCompareSenseiMatchPercentageNegativeTestForBBMID_LocationIDOnly()
        {            

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

            sensei1.AddNewSensationAtThisLocation(bPos1, kvp1);
            sensei1.AddNewSensationAtThisLocation(bPos2, kvp2);
            sensei1.AddNewSensationAtThisLocation(bPos3, kvp3);
            sensei1.AddNewSensationAtThisLocation(bPos4, kvp4);

            List<Position_SOM> activeBits = new List<Position_SOM>();

            activeBits.AddRange(activeBits1);
            activeBits.AddRange(activeBits2);
            activeBits.AddRange(activeBits3);
            activeBits.AddRange(activeBits4);

            SDR_SOM activeSDR = new SDR_SOM(1000, 10, activeBits, iType.SPATIAL);

            Sensation_Location sensei2 = orchestrator.Mapper.GetSensationLocationFromSDR(activeSDR, point);

            Match match = Sensation_Location.CompareSenseiPercentage(sensei1, sensei2, true, true);

            Assert.AreEqual(0, match.NumberOfLocationIDMatches);
            Assert.AreEqual(0, match.GetTotalMatchPercentage());

        }

        [Test, Description("Tests CompareSenseiMatchPercentage for 2 Sensei's based on Location && BBM ID && Position List for Positive Outcome!")]
        public void TestCompareSenseiMatchPercentagePositiveTestForBBMID_LocationID_PositionList()
        {
            
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

            sensei1.AddNewSensationAtThisLocation(gPos1, kvp1);
            sensei1.AddNewSensationAtThisLocation(gPos2, kvp2);
            sensei1.AddNewSensationAtThisLocation(gPos3, kvp3);
            sensei1.AddNewSensationAtThisLocation(gPos4, kvp4);

            List<Position_SOM> activeBits = new List<Position_SOM>();

            activeBits.AddRange(activeBits1);
            activeBits.AddRange(activeBits2);
            activeBits.AddRange(activeBits3);
            activeBits.AddRange(activeBits4);

            SDR_SOM activeSDR = new SDR_SOM(1000, 10, activeBits, iType.SPATIAL);

            Sensation_Location sensei2 = orchestrator.Mapper.GetSensationLocationFromSDR(activeSDR, point);

            Match match = Sensation_Location.CompareSenseiPercentage(sensei1, sensei2, true, true);

            Assert.AreEqual(4, match.NumberOfLocationIDMatches);
            Assert.AreEqual(4, match.NumberOfBBMIDMatches);
            Assert.AreEqual(100, match.GetTotalMatchPercentage());

        }

        [Test, Description("Tests CompareSenseiMatchPercentage for 2 Sensei's based on Location && BBM ID && Position Lit for Negative Outcome!")]
        public void TestCompareSenseiMatchPercentageNegativeTestForBBMID_LocationID_PositionList()
        {
            
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

            sensei1.AddNewSensationAtThisLocation(bPos1, kvp1);
            sensei1.AddNewSensationAtThisLocation(bPos2, kvp2);
            sensei1.AddNewSensationAtThisLocation(bPos3, kvp3);
            sensei1.AddNewSensationAtThisLocation(bPos4, kvp4);

            List<Position_SOM> activeBits = new List<Position_SOM>();

            activeBits.AddRange(activeBits1);
            activeBits.AddRange(activeBits2);
            activeBits.AddRange(activeBits3);
            activeBits.AddRange(activeBits4);

            SDR_SOM activeSDR = new SDR_SOM(1000, 10, activeBits, iType.SPATIAL);

            Sensation_Location sensei2 = orchestrator.Mapper.GetSensationLocationFromSDR(activeSDR, point);

            Match match = Sensation_Location.CompareSenseiPercentage(sensei1, sensei2, true, true);

            Assert.AreEqual(4, match.NumberOfLocationIDMisses);
            Assert.AreEqual(4, match.NumberOfBBMIDMises);
            Assert.AreEqual(0, match.GetTotalMatchPercentage());

        }


        [Test, Description("To Test if we ever send the same locations from Mapper while predicting as opposed to Training.")]
        public void TestPixelToSensationAtLocationConversion()
        {
            int loc1X = 1254;
            int loc1Y = 6578;

            int loc2X = 7896;
            int loc2Y = 1023;

            Bitmap bp1 = new Bitmap(40, 20);

            for (int i = 0; i < 40; i++) {
                for (int j = 0; j < 20; j++) {
                    bp1.SetPixel(i, j, Color.Black);
                }
            }

            bp1.SetPixel(34, 18, Color.White);
            bp1.SetPixel(34, 19, Color.White);
            bp1.SetPixel(35, 18, Color.White);
            bp1.SetPixel(35, 9, Color.White);


            Bitmap bp2 = new Bitmap(40, 20);

            for (int i = 0; i < 40; i++)
            {
                for (int j = 0; j < 20; j++)
                {
                    bp2.SetPixel(i, j, Color.Black);
                }
            }

            bp2.SetPixel(12, 14, Color.White);
            bp2.SetPixel(13, 14, Color.White);
            bp2.SetPixel(14, 15, Color.White);
            bp2.SetPixel(15, 16, Color.White);            

            orchestrator.point.X = loc1X;
            orchestrator.point.Y = loc1Y;
            orchestrator.ProcesStep1(bp1);
            orchestrator.ProcessStep2();        


            orchestrator.point.X = loc2X;
            orchestrator.point.Y = loc2Y;
            orchestrator.ProcesStep1(bp2);
            orchestrator.ProcessStep2();


            orchestrator.DoneWithTraining();
            orchestrator.ChangeNetworkModeToPrediction();


            orchestrator.point.X = loc1X;
            orchestrator.point.Y = loc1Y;
            orchestrator.ProcesStep1(bp1);
            var pos = orchestrator.ProcessStep2(true);

            Assert.AreEqual(loc2X, pos.X);
            Assert.AreEqual(loc2Y, pos.Y); 

        }

        [Test, Ignore("Not Yet Implemented")]
        public void TestGreyScallinOnPixelValue()
        {
            throw new NotImplementedException();
        }

        private int GetXFromBBM_ID(int bbmId)
        {
            orchestrator.Mapper.Mappings.TryGetValue(bbmId, out var x);

            return x[0].X;
        }

        private int GetYFromBBM_ID(int bbmId)
        {
            orchestrator.Mapper.Mappings.TryGetValue(bbmId, out var x);

            return x[0].Y;
        }
    }
}
