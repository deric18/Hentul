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

        [Test]
        public void TestCode()
        {
            int x = 99;

            var result = x % 10;


            int bp = 1;

        }
      
        [Test, Ignore("Needs a lot more Work")]
        public void TestPreditObject2PositiveTest()
        {
            List<RecognisedVisualEntity> entities = TestUtils.GenerateRandomEntities(4);

            hc.LoadMockObject(entities, true);

            // Build an SDR_SOM from an existing Sensation_Location in the mock entity
            Sensation_Location sensei = entities[1].ObjectSnapshot.ElementAt(2);

            var activeBits = new List<Position_SOM>();
            foreach (var key in sensei.sensLoc.Keys)
            {
                var p = Position2D.ConvertStringToPosition(key);
                if (p != null)
                    activeBits.Add(new Position_SOM(p.X, p.Y));
            }

            var sdr = new SDR_SOM(1200, 600, activeBits, iType.SPATIAL);

            List<Position2D> positions = hc.StoreNewObjectLocationInGraph(sdr, 0, 0);

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

        #region StoreNewObjectLocationInGraph Tests

        /// <summary>
        /// Positive test: verifies that when the network is in TRAINING mode and VisionScope is BROAD,
        /// valid sensation locations are added to the graph and returned.
        /// </summary>
        [Test]
        public void StoreNewObjectLocationInGraph_ValidSensationLocations_ReturnsAddedPositions()
        {
            // Arrange: put orchestrator/hc into TRAINING mode and set VisionScope to BROAD
            orchestrator.ChangeNetworkModeToTraining();
            orchestrator.SetVisionScope(VisionScope.BROAD, true);

            // Build an SDR_SOM with Position_SOM active bits that have positive X and Y coordinates
            var activeBits = new List<Position_SOM>
            {
                new Position_SOM(10, 20),
                new Position_SOM(30, 40),
                new Position_SOM(50, 60)
            };

            var sdr = new SDR_SOM(1200, 600, activeBits, iType.SPATIAL);

            // Act
            var addedPositions = hc.StoreNewObjectLocationInGraph(sdr, 0, 0);

            // Assert: we expect 3 positions to be added (one per active bit)
            Assert.IsNotNull(addedPositions);
            Assert.AreEqual(3, addedPositions.Count, "Expected 3 positions to be added to the graph.");

            // Verify the positions match the SDR active bits
            Assert.IsTrue(addedPositions.Any(p => p.X == 10 && p.Y == 20), "Position (10,20) should be in the result.");
            Assert.IsTrue(addedPositions.Any(p => p.X == 30 && p.Y == 40), "Position (30,40) should be in the result.");
            Assert.IsTrue(addedPositions.Any(p => p.X == 50 && p.Y == 60), "Position (50,60) should be in the result.");
        }

        /// <summary>
        /// Negative test: verifies that calling StoreNewObjectLocationInGraph when network is in PREDICTION mode
        /// throws an InvalidOperationException.
        /// </summary>
        [Test]
        public void StoreNewObjectLocationInGraph_InPredictionMode_ThrowsInvalidOperationException()
        {
            // Arrange: ensure network is in PREDICTION mode
            orchestrator.ChangeNetworkModeToPrediction(true);
            orchestrator.SetVisionScope(VisionScope.BROAD, true);

            var activeBits = new List<Position_SOM>
            {
                new Position_SOM(10, 20)
            };

            var sdr = new SDR_SOM(1200, 600, activeBits, iType.SPATIAL);

            // Act & Assert: should throw because network is in PREDICTION mode
            var ex = Assert.Throws<InvalidOperationException>(() => hc.StoreNewObjectLocationInGraph(sdr, 0, 0));
            Assert.IsNotNull(ex);
            StringAssert.Contains("training", ex.Message.ToLower(), "Exception message should mention training mode requirement.");
        }


        /// <summary>
        /// Test: verifies that nodes are actually created in the Graph for each position returned
        /// by StoreNewObjectLocationInGraph, and that Graph.GetNode can retrieve them with correct coordinates.
        /// </summary>
        [Test]
        public void StoreNewObjectLocationInGraph_CreatesNodesInGraph_NodesAreRetrievable()
        {
            // Arrange
            orchestrator.ChangeNetworkModeToTraining();
            orchestrator.SetVisionScope(VisionScope.BROAD, true);

            var activeBits = new List<Position_SOM>
            {
                new Position_SOM(5, 10),
                new Position_SOM(15, 25),
                new Position_SOM(35, 45)
            };

            var sdr = new SDR_SOM(1200, 600, activeBits, iType.SPATIAL);

            // Act
            var addedPositions = hc.StoreNewObjectLocationInGraph(sdr, 0, 0);

            // Assert: verify positions were returned
            Assert.AreEqual(3, addedPositions.Count, "Expected 3 positions to be added.");

            // Verify each position has a corresponding node in the Graph
            foreach (var pos in addedPositions)
            {
                var node = hc.Graph.GetNode(pos);
                Assert.IsNotNull(node, $"Graph should contain a node at position ({pos.X},{pos.Y})");
                Assert.AreEqual(pos.X, node.PixelCordinates.X, $"Node X coordinate should match position X: {pos.X}");
                Assert.AreEqual(pos.Y, node.PixelCordinates.Y, $"Node Y coordinate should match position Y: {pos.Y}");
            }
        }

        /// <summary>
        /// Test: verifies that multiple calls to StoreNewObjectLocationInGraph accumulate positions
        /// in the Graph and that the Graph's TotalCount increases accordingly.
        /// </summary>
        [Test]
        public void StoreNewObjectLocationInGraph_MultipleCalls_AccumulatesNodesInGraph()
        {
            // Arrange
            orchestrator.ChangeNetworkModeToTraining();
            orchestrator.SetVisionScope(VisionScope.BROAD, true);

            var initialCount = hc.Graph.TotalCount;

            // First call
            var activeBits1 = new List<Position_SOM>
            {
                new Position_SOM(20, 30),
                new Position_SOM(40, 50)
            };
            var sdr1 = new SDR_SOM(1200, 600, activeBits1, iType.SPATIAL);

            // Act: First addition
            var addedPositions1 = hc.StoreNewObjectLocationInGraph(sdr1, 0, 0);

            // Assert: verify first batch
            Assert.AreEqual(2, addedPositions1.Count, "Expected 2 positions from first call.");
            var countAfterFirst = hc.Graph.TotalCount;
            Assert.Greater(countAfterFirst, initialCount, "Graph TotalCount should increase after first addition.");

            // Second call with different positions
            var activeBits2 = new List<Position_SOM>
            {
                new Position_SOM(60, 70),
                new Position_SOM(80, 90),
                new Position_SOM(100, 110)
            };
            var sdr2 = new SDR_SOM(1200, 600, activeBits2, iType.SPATIAL);

            // Act: Second addition
            var addedPositions2 = hc.StoreNewObjectLocationInGraph(sdr2, 0, 0);

            // Assert: verify second batch
            Assert.AreEqual(3, addedPositions2.Count, "Expected 3 positions from second call.");
            var countAfterSecond = hc.Graph.TotalCount;
            Assert.Greater(countAfterSecond, countAfterFirst, "Graph TotalCount should increase after second addition.");

            // Verify all nodes from both calls are retrievable
            var allPositions = addedPositions1.Concat(addedPositions2).ToList();
            foreach (var pos in allPositions)
            {
                var node = hc.Graph.GetNode(pos);
                Assert.IsNotNull(node, $"Graph should contain node at ({pos.X},{pos.Y})");
                Assert.AreEqual(pos.X, node.PixelCordinates.X);
                Assert.AreEqual(pos.Y, node.PixelCordinates.Y);
            }
        }

        /// <summary>
        /// Test: validates Graph state after adding nodes - checks Base node exists,
        /// TotalCount is updated, and MaxRight/MaxUp boundaries are tracked.
        /// </summary>
        [Test]
        public void StoreNewObjectLocationInGraph_ValidatesGraphState_BaseAndBoundariesCorrect()
        {
            // Arrange
            orchestrator.ChangeNetworkModeToTraining();
            orchestrator.SetVisionScope(VisionScope.BROAD, true);

            var initialMaxRight = hc.Graph.MaxRight;
            var initialMaxUp = hc.Graph.MaxUp;

            var activeBits = new List<Position_SOM>
            {
                new Position_SOM(5, 5),
                new Position_SOM(25, 15),
                new Position_SOM(45, 35),
                new Position_SOM(65, 55)
            };

            var sdr = new SDR_SOM(1200, 600, activeBits, iType.SPATIAL);

            // Act
            var addedPositions = hc.StoreNewObjectLocationInGraph(sdr, 0, 0);

            // Assert: Basic Graph state validation
            Assert.IsNotNull(hc.Graph, "Graph should be initialized.");
            Assert.IsNotNull(hc.Graph.Base, "Graph.Base node should exist.");
            Assert.AreEqual(0, hc.Graph.Base.PixelCordinates.X, "Graph.Base should be at X=0.");
            Assert.AreEqual(0, hc.Graph.Base.PixelCordinates.Y, "Graph.Base should be at Y=0.");

            // Assert: TotalCount should have increased
            Assert.Greater(hc.Graph.TotalCount, 0u, "Graph.TotalCount should be greater than 0 after adding nodes.");

            // Assert: MaxRight and MaxUp should reflect the added positions
            var maxX = activeBits.Max(p => p.X);
            var maxY = activeBits.Max(p => p.Y);

            Assert.GreaterOrEqual(hc.Graph.MaxRight, (uint)maxX,
                $"Graph.MaxRight should be at least {maxX} after adding positions.");
            Assert.GreaterOrEqual(hc.Graph.MaxUp, (uint)maxY,
                $"Graph.MaxUp should be at least {maxY} after adding positions.");

            // Assert: All added positions are retrievable
            Assert.AreEqual(4, addedPositions.Count, "Expected 4 positions to be added.");
            foreach (var pos in addedPositions)
            {
                var node = hc.Graph.GetNode(pos);
                Assert.IsNotNull(node, $"Node at ({pos.X},{pos.Y}) should be retrievable from Graph.");
            }
        }


        #endregion

        #region Offset Parameter Tests

        /// <summary>
        /// Test: verifies that positive offsetX and offsetY are correctly added to each position's coordinates.
        /// </summary>
        [Test]
        public void StoreNewObjectLocationInGraph_WithPositiveOffsets_AddsOffsetsToCoordinates()
        {
            // Arrange
            orchestrator.ChangeNetworkModeToTraining();
            orchestrator.SetVisionScope(VisionScope.BROAD, true);

            var activeBits = new List<Position_SOM>
            {
                new Position_SOM(10, 20),
                new Position_SOM(30, 40)
            };

            var sdr = new SDR_SOM(1200, 600, activeBits, iType.SPATIAL);
            int offsetX = 100;
            int offsetY = 200;

            // Act
            var addedPositions = hc.StoreNewObjectLocationInGraph(sdr, offsetX, offsetY);

            // Assert: positions should have offsets applied
            Assert.AreEqual(2, addedPositions.Count, "Expected 2 positions to be added.");
            Assert.IsTrue(addedPositions.Any(p => p.X == 110 && p.Y == 220), 
                "Position (10,20) + offset (100,200) should result in (110,220).");
            Assert.IsTrue(addedPositions.Any(p => p.X == 130 && p.Y == 240), 
                "Position (30,40) + offset (100,200) should result in (130,240).");

            // Verify nodes are retrievable at the offset positions
            foreach (var pos in addedPositions)
            {
                var node = hc.Graph.GetNode(pos);
                Assert.IsNotNull(node, $"Node at ({pos.X},{pos.Y}) should exist in Graph.");
            }
        }

        /// <summary>
        /// Test: verifies that zero offsets leave coordinates unchanged.
        /// </summary>
        [Test]
        public void StoreNewObjectLocationInGraph_WithZeroOffsets_CoordinatesUnchanged()
        {
            // Arrange
            orchestrator.ChangeNetworkModeToTraining();
            orchestrator.SetVisionScope(VisionScope.BROAD, true);

            var activeBits = new List<Position_SOM>
            {
                new Position_SOM(15, 25),
                new Position_SOM(35, 45)
            };

            var sdr = new SDR_SOM(1200, 600, activeBits, iType.SPATIAL);

            // Act
            var addedPositions = hc.StoreNewObjectLocationInGraph(sdr, 0, 0);

            // Assert: positions should be unchanged
            Assert.AreEqual(2, addedPositions.Count, "Expected 2 positions to be added.");
            Assert.IsTrue(addedPositions.Any(p => p.X == 15 && p.Y == 25), 
                "Position (15,25) should remain unchanged with zero offset.");
            Assert.IsTrue(addedPositions.Any(p => p.X == 35 && p.Y == 45), 
                "Position (35,45) should remain unchanged with zero offset.");
        }

        /// <summary>
        /// Test: verifies that large offsets correctly shift positions to high coordinate values.
        /// </summary>
        [Test]
        public void StoreNewObjectLocationInGraph_WithLargeOffsets_PositionsShiftedCorrectly()
        {
            // Arrange
            orchestrator.ChangeNetworkModeToTraining();
            orchestrator.SetVisionScope(VisionScope.BROAD, true);

            var activeBits = new List<Position_SOM>
            {
                new Position_SOM(5, 10),
                new Position_SOM(15, 20)
            };

            var sdr = new SDR_SOM(1200, 600, activeBits, iType.SPATIAL);
            int offsetX = 1000;
            int offsetY = 2000;

            // Act
            var addedPositions = hc.StoreNewObjectLocationInGraph(sdr, offsetX, offsetY);

            // Assert
            Assert.AreEqual(2, addedPositions.Count, "Expected 2 positions to be added.");
            Assert.IsTrue(addedPositions.Any(p => p.X == 1005 && p.Y == 2010), 
                "Position (5,10) + offset (1000,2000) should result in (1005,2010).");
            Assert.IsTrue(addedPositions.Any(p => p.X == 1015 && p.Y == 2020), 
                "Position (15,20) + offset (1000,2000) should result in (1015,2020).");
        }

        /// <summary>
        /// Test: verifies that only offsetX is applied when offsetY is zero.
        /// </summary>
        [Test]
        public void StoreNewObjectLocationInGraph_WithOnlyOffsetX_OnlyXCoordinateShifted()
        {
            // Arrange
            orchestrator.ChangeNetworkModeToTraining();
            orchestrator.SetVisionScope(VisionScope.BROAD, true);

            var activeBits = new List<Position_SOM>
            {
                new Position_SOM(10, 20)
            };

            var sdr = new SDR_SOM(1200, 600, activeBits, iType.SPATIAL);
            int offsetX = 500;
            int offsetY = 0;

            // Act
            var addedPositions = hc.StoreNewObjectLocationInGraph(sdr, offsetX, offsetY);

            // Assert
            Assert.AreEqual(1, addedPositions.Count, "Expected 1 position to be added.");
            Assert.AreEqual(510, addedPositions[0].X, "X should be shifted by offsetX.");
            Assert.AreEqual(20, addedPositions[0].Y, "Y should remain unchanged.");
        }

        /// <summary>
        /// Test: verifies that only offsetY is applied when offsetX is zero.
        /// </summary>
        [Test]
        public void StoreNewObjectLocationInGraph_WithOnlyOffsetY_OnlyYCoordinateShifted()
        {
            // Arrange
            orchestrator.ChangeNetworkModeToTraining();
            orchestrator.SetVisionScope(VisionScope.BROAD, true);

            var activeBits = new List<Position_SOM>
            {
                new Position_SOM(10, 20)
            };

            var sdr = new SDR_SOM(1200, 600, activeBits, iType.SPATIAL);
            int offsetX = 0;
            int offsetY = 300;

            // Act
            var addedPositions = hc.StoreNewObjectLocationInGraph(sdr, offsetX, offsetY);

            // Assert
            Assert.AreEqual(1, addedPositions.Count, "Expected 1 position to be added.");
            Assert.AreEqual(10, addedPositions[0].X, "X should remain unchanged.");
            Assert.AreEqual(320, addedPositions[0].Y, "Y should be shifted by offsetY.");
        }

        /// <summary>
        /// Test: verifies that negative offsets result in positions that may not be added 
        /// if they become invalid (e.g., coordinates <= 0).
        /// </summary>
        [Test]
        public void StoreNewObjectLocationInGraph_WithNegativeOffsetsResultingInInvalidPositions_PositionsNotAdded()
        {
            // Arrange
            orchestrator.ChangeNetworkModeToTraining();
            orchestrator.SetVisionScope(VisionScope.BROAD, true);

            // Position (5, 10) with offset (-10, -20) would result in (-5, -10) which is invalid
            var activeBits = new List<Position_SOM>
            {
                new Position_SOM(5, 10)
            };

            var sdr = new SDR_SOM(1200, 600, activeBits, iType.SPATIAL);
            int offsetX = -10;
            int offsetY = -20;

            // Act
            var addedPositions = hc.StoreNewObjectLocationInGraph(sdr, offsetX, offsetY);

            // Assert: position should not be added since resulting coordinates are negative
            Assert.AreEqual(0, addedPositions.Count, 
                "Position resulting in negative coordinates should not be added.");
        }

        /// <summary>
        /// Test: verifies that negative offsets that still result in valid (positive) coordinates work correctly.
        /// </summary>
        [Test]
        public void StoreNewObjectLocationInGraph_WithNegativeOffsetsResultingInValidPositions_PositionsAdded()
        {
            // Arrange
            orchestrator.ChangeNetworkModeToTraining();
            orchestrator.SetVisionScope(VisionScope.BROAD, true);

            // Position (100, 200) with offset (-50, -100) results in (50, 100) which is valid
            var activeBits = new List<Position_SOM>
            {
                new Position_SOM(100, 200)
            };

            var sdr = new SDR_SOM(1200, 600, activeBits, iType.SPATIAL);
            int offsetX = -50;
            int offsetY = -100;

            // Act
            var addedPositions = hc.StoreNewObjectLocationInGraph(sdr, offsetX, offsetY);

            // Assert
            Assert.AreEqual(1, addedPositions.Count, "Expected 1 position to be added.");
            Assert.AreEqual(50, addedPositions[0].X, "X should be 100 - 50 = 50.");
            Assert.AreEqual(100, addedPositions[0].Y, "Y should be 200 - 100 = 100.");
        }

        /// <summary>
        /// Test: verifies that mixed valid and invalid positions (after offset) correctly filters results.
        /// </summary>
        [Test]
        public void StoreNewObjectLocationInGraph_MixedValidAndInvalidAfterOffset_OnlyValidPositionsAdded()
        {
            // Arrange
            orchestrator.ChangeNetworkModeToTraining();
            orchestrator.SetVisionScope(VisionScope.BROAD, true);

            var activeBits = new List<Position_SOM>
            {
                new Position_SOM(5, 5),     // After offset (-10, -10): (-5, -5) -> INVALID
                new Position_SOM(50, 50),   // After offset (-10, -10): (40, 40) -> VALID
                new Position_SOM(10, 10)    // After offset (-10, -10): (0, 0) -> may be invalid depending on Graph
            };

            var sdr = new SDR_SOM(1200, 600, activeBits, iType.SPATIAL);
            int offsetX = -10;
            int offsetY = -10;

            // Act
            var addedPositions = hc.StoreNewObjectLocationInGraph(sdr, offsetX, offsetY);

            // Assert: only valid positions should be added
            Assert.IsTrue(addedPositions.Any(p => p.X == 40 && p.Y == 40), 
                "Position (50,50) - offset (10,10) = (40,40) should be added.");
            
            // Verify invalid position was not added
            Assert.IsFalse(addedPositions.Any(p => p.X < 0 || p.Y < 0), 
                "No position with negative coordinates should be added.");
        }

        /// <summary>
        /// Test: verifies Graph boundaries (MaxRight, MaxUp) are updated correctly based on offset positions.
        /// </summary>
        [Test]
        public void StoreNewObjectLocationInGraph_WithOffsets_GraphBoundariesUpdatedCorrectly()
        {
            // Arrange
            orchestrator.ChangeNetworkModeToTraining();
            orchestrator.SetVisionScope(VisionScope.BROAD, true);

            var activeBits = new List<Position_SOM>
            {
                new Position_SOM(10, 20),
                new Position_SOM(30, 40)
            };

            var sdr = new SDR_SOM(1200, 600, activeBits, iType.SPATIAL);
            int offsetX = 500;
            int offsetY = 600;

            // Act
            var addedPositions = hc.StoreNewObjectLocationInGraph(sdr, offsetX, offsetY);

            // Assert: Graph boundaries should reflect offset positions
            // Expected max positions: (510, 620) and (530, 640)
            Assert.GreaterOrEqual(hc.Graph.MaxRight, 530u, 
                "Graph.MaxRight should be at least 530 (30 + 500).");
            Assert.GreaterOrEqual(hc.Graph.MaxUp, 640u, 
                "Graph.MaxUp should be at least 640 (40 + 600).");
        }

        /// <summary>
        /// Test: verifies multiple chunks with different offsets accumulate correctly in Graph.
        /// Simulates screen exploration where each chunk has different offsets.
        /// </summary>
        [Test]
        public void StoreNewObjectLocationInGraph_MultipleChunksWithDifferentOffsets_AllPositionsAccumulated()
        {
            // Arrange
            orchestrator.ChangeNetworkModeToTraining();
            orchestrator.SetVisionScope(VisionScope.BROAD, true);

            // First chunk at offset (0, 0)
            var chunk1Bits = new List<Position_SOM>
            {
                new Position_SOM(10, 10),
                new Position_SOM(20, 20)
            };
            var sdr1 = new SDR_SOM(1200, 600, chunk1Bits, iType.SPATIAL);

            // Second chunk at offset (600, 0) - simulating next horizontal chunk
            var chunk2Bits = new List<Position_SOM>
            {
                new Position_SOM(10, 10),
                new Position_SOM(20, 20)
            };
            var sdr2 = new SDR_SOM(1200, 600, chunk2Bits, iType.SPATIAL);

            // Third chunk at offset (0, 1200) - simulating next vertical chunk
            var chunk3Bits = new List<Position_SOM>
            {
                new Position_SOM(10, 10)
            };
            var sdr3 = new SDR_SOM(1200, 600, chunk3Bits, iType.SPATIAL);

            // Act
            var positions1 = hc.StoreNewObjectLocationInGraph(sdr1, 0, 0);
            var positions2 = hc.StoreNewObjectLocationInGraph(sdr2, 600, 0);
            var positions3 = hc.StoreNewObjectLocationInGraph(sdr3, 0, 1200);

            // Assert
            Assert.AreEqual(2, positions1.Count, "First chunk should add 2 positions.");
            Assert.AreEqual(2, positions2.Count, "Second chunk should add 2 positions.");
            Assert.AreEqual(1, positions3.Count, "Third chunk should add 1 position.");

            // Verify positions from chunk 1 (offset 0, 0)
            Assert.IsTrue(positions1.Any(p => p.X == 10 && p.Y == 10));
            Assert.IsTrue(positions1.Any(p => p.X == 20 && p.Y == 20));

            // Verify positions from chunk 2 (offset 600, 0)
            Assert.IsTrue(positions2.Any(p => p.X == 610 && p.Y == 10));
            Assert.IsTrue(positions2.Any(p => p.X == 620 && p.Y == 20));

            // Verify positions from chunk 3 (offset 0, 1200)
            Assert.IsTrue(positions3.Any(p => p.X == 10 && p.Y == 1210));
        }

        #endregion

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
