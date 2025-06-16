namespace Hentul.UT
{
    using Hentul;
    using Common;
    using Hentul.Hippocampal_Entorinal_complex;
    using static Hentul.Orchestrator;
    using System.Drawing;
    using System.IO;
    using Hentul.Encoders;

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

            Assert.AreEqual(orchestrator.HCAccessor.Objects.Count, 5);
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

        [Test, Ignore("Need access to cursor and custom Apple Image to be hosted!")]
        public void TestWanderingCursor()
        {
            List<Position2D> cursorPositions = new List<Position2D>()
            {
                new Position2D( 1346, 456),
                new Position2D( 1043, 629),
                new Position2D( 1279, 620),
                new Position2D( 1498, 612),
                new Position2D( 1346, 656)
            };

            foreach (var position in cursorPositions)
            {
                if (position.X == 1498)
                {
                    bool bp1 = true;
                }

                Orchestrator.SetCursorPos(position.X, position.Y);                

                orchestrator.RecordPixels(true);
                var edgedbmp1 = orchestrator.ConverToEdgedBitmap();
                orchestrator.ProcessVisual(edgedbmp1);
                orchestrator.AddNewVisualSensationToHC();

            }

            orchestrator.DoneWithTraining();
            orchestrator.ChangeNetworkModeToPrediction();

            Orchestrator.SetCursorPos(cursorPositions[0].X, cursorPositions[0].Y);

            orchestrator.RecordPixels();

            var edgedbmp2 = orchestrator.ConverToEdgedBitmap();
            orchestrator.ProcessVisual(edgedbmp2);
            var result = orchestrator.Verify_Predict_HC(true, 4);

            Assert.AreEqual(result.X, int.MaxValue);
            Assert.AreEqual(result.Y, int.MaxValue);

            var arr = orchestrator.StartBurstAvoidanceWandering(5);

            int bp = 1;
            foreach (var i in arr)
            {
                Assert.AreEqual(i, 0);
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


            orchestrator.pEncoder.ParseBitmap(bp);

            int breapoint = 1;

            Assert.AreEqual(MAPPERCASE.ALL, orchestrator.pEncoder.FOMBBMIDS.Keys.ElementAt(0));
            Assert.AreEqual(89, orchestrator.pEncoder.FOMBBMIDS.Values.ElementAt(0).ElementAt(0));

            int trues = 0;
            int falses = 0;

            for (int i = 0; i < orchestrator.pEncoder.testBmpCoverage.GetUpperBound(0); i++)
            {
                for (int j = 0; j < orchestrator.pEncoder.testBmpCoverage.GetUpperBound(1); j++)
                {
                    //Assert.AreEqual(true, orchestrator.Mapper.flagCheckArr[i, j]);
                    if (orchestrator.pEncoder.testBmpCoverage[i, j])
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

        [Test, Ignore("Takes too long")]
        public void TestBackUp()
        {
            string backupDirHC = "C:\\Users\\depint\\source\\repos\\Hentul\\Hentul\\BackUp\\HC-EC\\";
            string backupDirFOM = "C:\\Users\\depint\\source\\repos\\Hentul\\Hentul\\BackUp\\FOM\\";
            string backupDirSOM = "C:\\Users\\depint\\source\\repos\\Hentul\\Hentul\\BackUp\\SOM\\";
            string obj = "Apple";

            KeyValuePair<int, List<Position2D>> kvp = new KeyValuePair<int, List<Position2D>>(1, new List<Position2D>()
                {
                    new Position2D(1,2),
                    new Position2D(2,3),
                    new Position2D(3,4)
                });
            SortedDictionary<string, KeyValuePair<int, List<Position2D>>> dict = new SortedDictionary<string, KeyValuePair<int, List<Position2D>>>();
            dict.Add(obj, kvp);
            Sensation_Location sensei = new Sensation_Location(dict);
            orchestrator.HCAccessor.VerifyObject(sensei);
            orchestrator.DoneWithTraining();

            orchestrator.BackUp();

            if (Directory.GetFiles(backupDirHC).Length == 0 || Directory.GetFiles(backupDirFOM).Length == 0 || Directory.GetFiles(backupDirSOM).Length == 0)
            {
                Assert.Fail();
            }
            else
            {
                Assert.Pass();
            }
        }

        [Test, Ignore("Takes too long")]
        public void TestRestore()
        {
            string backupDirHC = "C:\\Users\\depint\\source\\repos\\Hentul\\Hentul\\BackUp\\HC-EC\\";
            string backupDirFOM = "C:\\Users\\depint\\source\\repos\\Hentul\\Hentul\\BackUp\\FOM\\";
            string backupDirSOM = "C:\\Users\\depint\\source\\repos\\Hentul\\Hentul\\BackUp\\SOM\\";

            if (Directory.GetFiles(backupDirHC).Length == 0 || Directory.GetFiles(backupDirFOM).Length == 0 || Directory.GetFiles(backupDirSOM).Length == 0)
            {
                Assert.Fail();
            }

            orchestrator.Restore();

            foreach (var fom in orchestrator.VisionProcessor.fomBBMV)
            {
                for (int i = 0; i < 100; i++)
                {
                    {
                        for (int j = 0; j < 10; j++)
                        {
                            if (fom.Columns[i, j] == null)
                            {
                                Assert.Fail();
                            }
                        }
                    }
                }
            }

            for (int i = 0; i < 1000; i++)
            {
                for (int j = 0; j < 10; j++)
                {
                    if (orchestrator.VisionProcessor.somBBM_L3B_V.Columns[i, j] == null)
                    {
                        Assert.Fail();
                    }
                }
            }

            for (int i = 0; i < 1000; i++)
            {
                for (int j = 0; j < 10; j++)
                {
                    if (orchestrator.VisionProcessor.somBBM_L3A_V.Columns[i, j] == null)
                    {
                        Assert.Fail();
                    }
                }
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

            var sensloc = orchestrator.pEncoder.GetSenseiFromSDR_V(sdr, point);

            Assert.AreEqual(1, sensloc.sensLoc.Count);
            Assert.AreEqual(4, sensloc.sensLoc.ElementAt(0).Value.Value.Count);


            var sensloc1 = orchestrator.pEncoder.GetSenseiFromSDR_V(sdr1, point);

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

            var sensloc = orchestrator.pEncoder.GetSenseiFromSDR_V(sdr, point);


            foreach (var kvp in sensloc.sensLoc)
            {
                Position2D p = Position.ConvertStringToPosition2D(kvp.Key);


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
       

        [Test]
        public void TestCompareSenseiMatchPercentageNegativeTestForLocationIDOnly()
        {
            List<Position2D> activeBits1 = new List<Position2D>()
                    {
                        new Position2D(777, 10)
                    };
            List<Position2D> activeBits2 = new List<Position2D>()
                    {
                        new Position2D(555, 10)
                    };
            List<Position2D> activeBits3 = new List<Position2D>()
                    {
                        new Position2D(111, 10)
                    };
            List<Position2D> activeBits4 = new List<Position2D>()
                    {
                        new Position2D(243, 10)
                    };

            KeyValuePair<int, List<Position2D>> kvp1 = new KeyValuePair<int, List<Position2D>>(77, activeBits1);
            KeyValuePair<int, List<Position2D>> kvp2 = new KeyValuePair<int, List<Position2D>>(55, activeBits2);
            KeyValuePair<int, List<Position2D>> kvp3 = new KeyValuePair<int, List<Position2D>>(11, activeBits3);
            KeyValuePair<int, List<Position2D>> kvp4 = new KeyValuePair<int, List<Position2D>>(24, activeBits4);
            

            Sensation_Location sensei1 = new Sensation_Location();

            sensei1.AddNewSensationAtThisLocation(bPos1, kvp1);
            sensei1.AddNewSensationAtThisLocation(bPos2, kvp2);
            sensei1.AddNewSensationAtThisLocation(bPos3, kvp3);
            sensei1.AddNewSensationAtThisLocation(bPos4, kvp4);

            List<Position2D> activeBits = new List<Position2D>();

           activeBits.AddRange(activeBits1);
           activeBits.AddRange(activeBits2);
           activeBits.AddRange(activeBits3);
           activeBits.AddRange(activeBits4);

            SDR_SOM activeSDR = new SDR_SOM(1000, 10, Conver2DtoSOMList(activeBits), iType.SPATIAL);

            Sensation_Location sensei2 = orchestrator.pEncoder.GetSenseiFromSDR_V(activeSDR, point);

            Match match = Sensation_Location.CompareSenseiPercentage(sensei1, sensei2, false, true);

            Assert.AreEqual(4, match.NumberOfLocationIDMisses);
            Assert.AreEqual(0, match.NumberOfBBMIDMises);
            Assert.AreEqual(100, match.GetTotalMatchPercentage());

        }

        [Test, Description("Tests CompareSenseiMatchPercentage for 2 Sensei's based purely on BBM ID for Positive Outcome!")]
        public void TestCompareSenseiMatchPercentagePositiveTestForBBMIDOnly()
        {

            List<Position2D> activeBits1 = new List<Position2D>()
                    {
                        new Position2D(777, 10)
                    };
            List<Position2D> activeBits2 = new List<Position2D>()
                    {
                        new Position2D(555, 10)
                    };
            List<Position2D> activeBits3 = new List<Position2D>()
                    {
                        new Position2D(111, 10)
                    };
            List<Position2D> activeBits4 = new List<Position2D>()
                    {
                        new Position2D(243, 10)
                    };

            KeyValuePair<int, List<Position2D>> kvp1 = new KeyValuePair<int, List<Position2D>>(77, activeBits1);
            KeyValuePair<int, List<Position2D>> kvp2 = new KeyValuePair<int, List<Position2D>>(55, activeBits2);
            KeyValuePair<int, List<Position2D>> kvp3 = new KeyValuePair<int, List<Position2D>>(11, activeBits3);
            KeyValuePair<int, List<Position2D>> kvp4 = new KeyValuePair<int, List<Position2D>>(24, activeBits4);           

            Sensation_Location sensei1 = new Sensation_Location();

            sensei1.AddNewSensationAtThisLocation(gPos1, kvp1);
            sensei1.AddNewSensationAtThisLocation(gPos2, kvp2);
            sensei1.AddNewSensationAtThisLocation(gPos3, kvp3);
            sensei1.AddNewSensationAtThisLocation(gPos4, kvp4);

            List<Position2D> activeBits = new List<Position2D>();

           activeBits.AddRange(activeBits1);
           activeBits.AddRange(activeBits2);
           activeBits.AddRange(activeBits3);
           activeBits.AddRange(activeBits4);

            SDR_SOM activeSDR = new SDR_SOM(1000, 10, Conver2DtoSOMList(activeBits), iType.SPATIAL);

            Sensation_Location sensei2 = orchestrator.pEncoder.GetSenseiFromSDR_V(activeSDR, point);

            Assert.AreEqual(100, Sensation_Location.CompareSenseiPercentage(sensei1, sensei2, true, false).GetTotalMatchPercentage());

        }


        [Test, Description("Tests CompareSenseiMatchPercentage for 2 Sensei's based purely on BBM ID for Negative Outcome!")]
        public void TestCompareSenseiMatchPercentageNegativeTestForBBMIDOnly()
        {

            List<Position2D> activeBits1 = new List<Position2D>()
                    {
                        new Position2D(777, 10)
                    };
            List<Position2D> activeBits2 = new List<Position2D>()
                    {
                        new Position2D(555, 10)
                    };
            List<Position2D> activeBits3 = new List<Position2D>()
                    {
                        new Position2D(111, 10)
                    };
            List<Position2D> activeBits4 = new List<Position2D>()
                    {
                        new Position2D(243, 10)
                    };

            KeyValuePair<int, List<Position2D>> kvp1 = new KeyValuePair<int, List<Position2D>>(1, activeBits1);
            KeyValuePair<int, List<Position2D>> kvp2 = new KeyValuePair<int, List<Position2D>>(2, activeBits2);
            KeyValuePair<int, List<Position2D>> kvp3 = new KeyValuePair<int, List<Position2D>>(3, activeBits3);
            KeyValuePair<int, List<Position2D>> kvp4 = new KeyValuePair<int, List<Position2D>>(4, activeBits4);            

            Sensation_Location sensei1 = new Sensation_Location();

            sensei1.AddNewSensationAtThisLocation(bPos1, kvp1);
            sensei1.AddNewSensationAtThisLocation(bPos2, kvp2);
            sensei1.AddNewSensationAtThisLocation(bPos3, kvp3);
            sensei1.AddNewSensationAtThisLocation(bPos4, kvp4);

            List<Position2D> activeBits = new List<Position2D>();

            activeBits.AddRange(activeBits1);
            activeBits.AddRange(activeBits2);
            activeBits.AddRange(activeBits3);
            activeBits.AddRange(activeBits4);

            SDR_SOM activeSDR = new SDR_SOM(1000, 10, Conver2DtoSOMList(activeBits), iType.SPATIAL);

            Sensation_Location sensei2 = orchestrator.pEncoder.GetSenseiFromSDR_V(activeSDR, point);

            Assert.AreEqual(0, Sensation_Location.CompareSenseiPercentage(sensei1, sensei2, true, false).GetTotalMatchPercentage());

        }

        [Test, Description("Tests CompareSenseiMatchPercentage for 2 Sensei's based on Location && BBM ID for Positive Outcome!")]
        public void TestCompareSenseiMatchPercentagePositiveTestForBBMID_LocationIDOnly()
        {
            List<Position2D> activeBits1 = new List<Position2D>()
                    {
                        new Position2D(777, 10)
                    };
            List<Position2D> activeBits2 = new List<Position2D>()
                    {
                        new Position2D(555, 10)
                    };
            List<Position2D> activeBits3 = new List<Position2D>()
                    {
                        new Position2D(111, 10)
                    };
            List<Position2D> activeBits4 = new List<Position2D>()
                    {
                        new Position2D(243, 10)
                    };

            KeyValuePair<int, List<Position2D>> kvp1 = new KeyValuePair<int, List<Position2D>>(77, activeBits1);
            KeyValuePair<int, List<Position2D>> kvp2 = new KeyValuePair<int, List<Position2D>>(55, activeBits2);
            KeyValuePair<int, List<Position2D>> kvp3 = new KeyValuePair<int, List<Position2D>>(11, activeBits3);
            KeyValuePair<int, List<Position2D>> kvp4 = new KeyValuePair<int, List<Position2D>>(24, activeBits4);

            
            Sensation_Location sensei1 = new Sensation_Location();

            sensei1.AddNewSensationAtThisLocation(gPos1, kvp1);
            sensei1.AddNewSensationAtThisLocation(gPos2, kvp2);
            sensei1.AddNewSensationAtThisLocation(gPos3, kvp3);
            sensei1.AddNewSensationAtThisLocation(gPos4, kvp4);

            List<Position2D> activeBits = new List<Position2D>();

            activeBits.AddRange(activeBits1);
            activeBits.AddRange(activeBits2);
            activeBits.AddRange(activeBits3);
            activeBits.AddRange(activeBits4);

            SDR_SOM activeSDR = new SDR_SOM(1000, 10, Conver2DtoSOMList(activeBits), iType.SPATIAL);

            Sensation_Location sensei2 = orchestrator.pEncoder.GetSenseiFromSDR_V(activeSDR, point);

            Assert.AreEqual(100, Sensation_Location.CompareSenseiPercentage(sensei1, sensei2, true, false).GetTotalMatchPercentage());

        }

        [Test, Description("Tests CompareSenseiMatchPercentage for 2 Sensei's based on Location && BBM ID for Negative Outcome!")]
        public void TestCompareSenseiMatchPercentageNegativeTestForBBMID_LocationIDOnly()
        {

            List<Position2D> activeBits1 = new List<Position2D>()
                    {
                        new Position2D(777, 10)
                    };
            List<Position2D> activeBits2 = new List<Position2D>()
                    {
                        new Position2D(555, 10)
                    };
            List<Position2D> activeBits3 = new List<Position2D>()
                    {
                        new Position2D(111, 10)
                    };
            List<Position2D> activeBits4 = new List<Position2D>()
                    {
                        new Position2D(243, 10)
                    };

            KeyValuePair<int, List<Position2D>> kvp1 = new KeyValuePair<int, List<Position2D>>(1, activeBits1);
            KeyValuePair<int, List<Position2D>> kvp2 = new KeyValuePair<int, List<Position2D>>(2, activeBits2);
            KeyValuePair<int, List<Position2D>> kvp3 = new KeyValuePair<int, List<Position2D>>(3, activeBits3);
            KeyValuePair<int, List<Position2D>> kvp4 = new KeyValuePair<int, List<Position2D>>(4, activeBits4);            

            Sensation_Location sensei1 = new Sensation_Location();

            sensei1.AddNewSensationAtThisLocation(bPos1, kvp1);
            sensei1.AddNewSensationAtThisLocation(bPos2, kvp2);
            sensei1.AddNewSensationAtThisLocation(bPos3, kvp3);
            sensei1.AddNewSensationAtThisLocation(bPos4, kvp4);

            List<Position2D> activeBits = new List<Position2D>();

            activeBits.AddRange(activeBits1);
            activeBits.AddRange(activeBits2);
            activeBits.AddRange(activeBits3);
            activeBits.AddRange(activeBits4);

            SDR_SOM activeSDR = new SDR_SOM(1000, 10,  Conver2DtoSOMList(activeBits), iType.SPATIAL);

            Sensation_Location sensei2 = orchestrator.pEncoder.GetSenseiFromSDR_V(activeSDR, point);

            Match match = Sensation_Location.CompareSenseiPercentage(sensei1, sensei2, true, true);

            Assert.AreEqual(0, match.NumberOfLocationIDMatches);
            Assert.AreEqual(0, match.GetTotalMatchPercentage());

        }
     

        [Test, Description("Tests CompareSenseiMatchPercentage for 2 Sensei's based on Location && BBM ID && Position Lit for Negative Outcome!")]
        public void TestCompareSenseiMatchPercentageNegativeTestForBBMID_LocationID_PositionList()
        {

            List<Position2D> activeBits1 = new List<Position2D>()
                    {
                        new Position2D(777, 10)
                    };
            List<Position2D> activeBits2 = new List<Position2D>()
                    {
                        new Position2D(555, 10)
                    };
            List<Position2D> activeBits3 = new List<Position2D>()
                    {
                        new Position2D(111, 10)
                    };
            List<Position2D> activeBits4 = new List<Position2D>()
                    {
                        new Position2D(243, 10)
                    };

            KeyValuePair<int, List<Position2D>> kvp1 = new KeyValuePair<int, List<Position2D>>(1, activeBits1);
            KeyValuePair<int, List<Position2D>> kvp2 = new KeyValuePair<int, List<Position2D>>(2, activeBits2);
            KeyValuePair<int, List<Position2D>> kvp3 = new KeyValuePair<int, List<Position2D>>(3, activeBits3);
            KeyValuePair<int, List<Position2D>> kvp4 = new KeyValuePair<int, List<Position2D>>(4, activeBits4);
         
            Sensation_Location sensei1 = new Sensation_Location();

            sensei1.AddNewSensationAtThisLocation(bPos1, kvp1);
            sensei1.AddNewSensationAtThisLocation(bPos2, kvp2);
            sensei1.AddNewSensationAtThisLocation(bPos3, kvp3);
            sensei1.AddNewSensationAtThisLocation(bPos4, kvp4);

            List<Position2D> activeBits = new List<Position2D>();

            activeBits.AddRange(activeBits1);
            activeBits.AddRange(activeBits2);
            activeBits.AddRange(activeBits3);
            activeBits.AddRange(activeBits4);

            SDR_SOM activeSDR = new SDR_SOM(1000, 10, Conver2DtoSOMList(activeBits), iType.SPATIAL);

            Sensation_Location sensei2 = orchestrator.pEncoder.GetSenseiFromSDR_V(activeSDR, point);

            Match match = Sensation_Location.CompareSenseiPercentage(sensei1, sensei2, true, true);

            Assert.AreEqual(4, match.NumberOfLocationIDMisses);
            Assert.AreEqual(4, match.NumberOfBBMIDMises);
            Assert.AreEqual(0, match.GetTotalMatchPercentage());

        }


        [Test, Description("To Test if we ever send the same locations from Mapper while predicting as opposed to Training.")]
        public void TestPixelToSensationAtLocationConversion()
        {
            int loc1X = 2333;
            int loc1Y = 1200;

            int loc2X = 1250;
            int loc2Y = 957;

            Bitmap bp1 = new Bitmap(40, 20);

            for (int i = 0; i < 40; i++)
            {
                for (int j = 0; j < 20; j++)
                {
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
            orchestrator.ProcessVisual(bp1);
            orchestrator.AddNewVisualSensationToHC();


            orchestrator.point.X = loc2X;
            orchestrator.point.Y = loc2Y;
            orchestrator.ProcessVisual(bp2);
            orchestrator.AddNewVisualSensationToHC();


            orchestrator.DoneWithTraining();
            orchestrator.ChangeNetworkModeToPrediction();


            orchestrator.point.X = loc1X;
            orchestrator.point.Y = loc1Y;
            orchestrator.ProcessVisual(bp1);
            var pos = orchestrator.Verify_Predict_HC(true);

            Assert.AreEqual(loc2X, pos.X);
            Assert.AreEqual(loc2Y, pos.Y);
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

        private int GetXFromBBM_ID(int bbmId)
        {
            orchestrator.pEncoder.Mappings.TryGetValue(bbmId, out var x);

            return x[0].X;
        }

        private int GetYFromBBM_ID(int bbmId)
        {
            orchestrator.pEncoder.Mappings.TryGetValue(bbmId, out var x);

            return x[0].Y;
        }
    }
}
