namespace Hentul.UT
{

    using NUnit.Framework;
    using System;
    using System.Collections.Generic;
    using Common;
    using Hentul;
    using Hentul.Encoders;
    using System.Drawing;


    public class EncoderTest
    {
        private LocationEncoder locationEncoder;
        private MotorEncoder motorEncoder;
        private MotorStreamProcessor motorStream;
        private PixelEncoder pixelEncoder;

        [SetUp]
        public void Setup()
        {
            locationEncoder = new LocationEncoder(iType.TEMPORAL);
            motorEncoder = new MotorEncoder();
            motorStream = new MotorStreamProcessor(numColumns: 10, z: 5, logMode: LogMode.None);

            // Use the intended large grid that keeps ~2% sparsity for 1000x600 input
            pixelEncoder = new PixelEncoder(3_000_000, 10);
        }

        #region MotorStreamProcessor Tests (2 Positive / 2 Negative)

        [Test]
        public void MotorStreamProcessor_Positive_ProcessTemporalSDR()
        {
            // Arrange
            var upSDR = motorEncoder.ToSDR(MouseMove.Up, 10, 4, iType.SPATIAL);
            ulong cycle = 10;

            // Act
            motorStream.ProcessSDR(upSDR, cycle);
            var l3b = motorStream.GetL3B(cycle);

            // Assert
            Assert.AreEqual(cycle, motorStream.LastCycle);
            Assert.AreSame(upSDR, motorStream.LastInput);
            Assert.IsNotNull(l3b, "Expected bursting columns SDR for latest cycle.");
            Assert.Greater(l3b!.ActiveBits.Count, 0, "Bursting SDR should have active bits.");
        }

        [Test]
        public void MotorStreamProcessor_Positive_L3BOnlyForLatestCycle()
        {
            // Arrange
            var sdr1 = motorEncoder.ToSDR(MouseMove.Left);
            var sdr2 = motorEncoder.ToSDR(MouseMove.Right);

            // Act
            motorStream.ProcessSDR(sdr1, 1);
            var l3bCycle1 = motorStream.GetL3B(1);
            motorStream.ProcessSDR(sdr2, 2);
            var l3bCycle2 = motorStream.GetL3B(2);
            var stale = motorStream.GetL3B(1); // old cycle

            // Assert
            Assert.IsNotNull(l3bCycle1);
            Assert.IsNotNull(l3bCycle2);
            Assert.IsNull(stale, "Old cycle should not return L3B after a newer cycle is processed.");
        }

        [Test]
        public void MotorStreamProcessor_Negative_NonSpatialSDRThrows()
        {
            // Arrange: build a SPATIAL SDR using same active bits as a canonical move
            var spatial = new SDR_SOM(10, 4, motorEncoder.ToSDR(MouseMove.Up).ActiveBits, iType.TEMPORAL);

            // Act / Assert
            var ex = Assert.Throws<InvalidOperationException>(() => motorStream.ProcessSDR(spatial, 5));
            StringAssert.Contains("SPATIAL", ex!.Message);
        }

        [Test]
        public void MotorStreamProcessor_Negative_NullInputThrows()
        {
            Assert.Throws<ArgumentNullException>(() => motorStream.ProcessSDR(null!, 3));
        }

        #endregion

        #region MotorEncoder Tests (2 Positive / 2 Negative)

        [Test]
        public void MotorEncoder_Positive_EncodeDecodeRoundTrip_AllMoves()
        {
            foreach (MouseMove mv in Enum.GetValues(typeof(MouseMove)))
            {
                var sdr = motorEncoder.ToSDR(mv);
                Assert.AreEqual(4, sdr.ActiveBits.Count, $"Expected 4 active bits for {mv}");
                var decoded = motorEncoder.Decode(sdr, allowApproximate: false);
                Assert.IsNotNull(decoded, $"Exact decode failed for {mv}");
                Assert.AreEqual(mv, decoded);
            }
        }

        [Test]
        public void MotorEncoder_Positive_ApproximateDecodeWithBitMissing()
        {
            var rightSDR = motorEncoder.ToSDR(MouseMove.Right);
            var reduced = new List<Position_SOM>(rightSDR.ActiveBits);
            reduced.RemoveAt(0); // remove one bit -> 3/4 = 0.75 precision

            var noisy = new SDR_SOM(10, 4, reduced, iType.TEMPORAL);
            var decoded = motorEncoder.Decode(noisy, allowApproximate: true, minPrecision: 0.6);

            Assert.AreEqual(MouseMove.Right, decoded, "Approximate decode should recover original move.");
        }

        [Test]
        public void MotorEncoder_Negative_AmbiguousPatternReturnsNull()
        {
            // Bits shared by Up and UpRight -> ambiguous partial
            var ambiguousBits = new List<Position_SOM>
            {
                new Position_SOM(1,1),
                new Position_SOM(2,1)
            };
            var ambiguous = new SDR_SOM(10, 4, ambiguousBits, iType.TEMPORAL);

            var decoded = motorEncoder.Decode(ambiguous, allowApproximate: true, minPrecision: 0.5);

            Assert.IsNull(decoded, "Ambiguous overlap should return null.");
        }

        [Test]
        public void MotorEncoder_Negative_EmptySDRReturnsNull()
        {
            var empty = new SDR_SOM(10, 4, new List<Position_SOM>(), iType.TEMPORAL);
            var decoded = motorEncoder.Decode(empty);
            Assert.IsNull(decoded);
        }

        #endregion

        #region PixelEncoder Tests (constructor validation, sparsity, uniqueness, bounds)

        [Test]
        public void Constructor_Throws_WhenTotalCellsLessThanPixelCount()
        {
            Assert.Throws<InvalidOperationException>(() => new PixelEncoder(100, 1));
        }

        [Test]
        public void EncodeBitmap_Null_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => pixelEncoder.EncodeBitmap(null!));
        }

        [Test]
        public void EncodeBitmap_InvalidDimensions_ThrowsInvalidOperationException()
        {
            using var bmp = new Bitmap(10, 10);
            Assert.Throws<InvalidOperationException>(() => pixelEncoder.EncodeBitmap(bmp));
        }

        [Test]
        public void AllWhiteBitmap_EncodesAllPixels()
        {
            using var bmp = new Bitmap(PixelEncoder.ImgWidth, PixelEncoder.ImgHeight);
            using (var g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.White);
            }

            var sdr = pixelEncoder.EncodeBitmap(bmp);

            // All pixels are white -> every pixel should be encoded
            Assert.AreEqual(PixelEncoder.PixelCount, sdr.ActiveBits.Count);

            // Density expected = PixelCount / TotalCells
            double expectedDensity = PixelEncoder.PixelCount / (double)pixelEncoder.TotalCells;
            double actualDensity = sdr.ActiveBits.Count / (double)pixelEncoder.TotalCells;
            Assert.AreEqual(expectedDensity, actualDensity, 1e-12);

            // Mapped positions must be unique (compare X/Y)
            var unique = sdr.ActiveBits.Select(p => (p.X, p.Y)).Distinct().Count();
            Assert.AreEqual(sdr.ActiveBits.Count, unique);
        }

        [Test]
        public void AllBlackBitmap_EncodesNoPixels()
        {
            using var bmp = new Bitmap(PixelEncoder.ImgWidth, PixelEncoder.ImgHeight);
            using (var g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.Black);
            }

            var sdr = pixelEncoder.EncodeBitmap(bmp);

            Assert.AreEqual(0, sdr.ActiveBits.Count);
        }

        [Test]
        public void MixedBitmap_OnlyWhitePixelsIncluded_AndThresholdBehavesAsExpected()
        {
            using var bmp = new Bitmap(PixelEncoder.ImgWidth, PixelEncoder.ImgHeight);
            using (var g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.Black);
            }

            // explicit white pixels (should be included)
            var whites = new List<(int x, int y)>
            {
                (0, 0),
                (PixelEncoder.ImgWidth - 1, PixelEncoder.ImgHeight - 1),
                (500, 300),
                (123, 456)
            };
            foreach (var (x, y) in whites) bmp.SetPixel(x, y, Color.FromArgb(255, 255, 255));

            // near-white at threshold -> CheckIfColorIsWhite uses > 240 so:
            // (241,241,241) should be included, (240,240,240) should NOT.
            var includedNearWhite = (241, 241);
            var excludedNearWhite = (200, 200);
            bmp.SetPixel(includedNearWhite.Item1, includedNearWhite.Item2, Color.FromArgb(241, 241, 241));
            bmp.SetPixel(excludedNearWhite.Item1, excludedNearWhite.Item2, Color.FromArgb(240, 240, 240));

            var sdr = pixelEncoder.EncodeBitmap(bmp);

            // expected included count = whites.Count + 1 (for 241) 
            Assert.AreEqual(whites.Count + 1, sdr.ActiveBits.Count);

            // verify each explicit white pixel is present (compare by coordinates)
            foreach (var (x, y) in whites)
            {
                var mapped = pixelEncoder.GetMappedPosition(x, y);
                Assert.IsTrue(sdr.ActiveBits.Any(p => p.X == mapped.X && p.Y == mapped.Y),
                    $"White pixel ({x},{y}) mapped position missing.");
            }

            // includedNearWhite must be present
            var includedMapped = pixelEncoder.GetMappedPosition(includedNearWhite.Item1, includedNearWhite.Item2);
            Assert.IsTrue(sdr.ActiveBits.Any(p => p.X == includedMapped.X && p.Y == includedMapped.Y));

            // excludedNearWhite must NOT be present
            var excludedMapped = pixelEncoder.GetMappedPosition(excludedNearWhite.Item1, excludedNearWhite.Item2);
            Assert.IsFalse(sdr.ActiveBits.Any(p => p.X == excludedMapped.X && p.Y == excludedMapped.Y));
        }

        [Test]
        public void BuildMappingLookup_OnAllWhiteBitmap_ReturnsFullLookup_WithUniquePositions()
        {
            using var bmp = new Bitmap(PixelEncoder.ImgWidth, PixelEncoder.ImgHeight);
            using (var g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.White);
            }

            var lookup = pixelEncoder.BuildMappingLookup(bmp);

            Assert.AreEqual(PixelEncoder.PixelCount, lookup.Count);

            // verify no collisions in mapped Position_SOMs (compare X/Y)
            var unique = lookup.Values.Select(p => (p.X, p.Y)).Distinct().Count();
            Assert.AreEqual(lookup.Count, unique);
        }

        [Test]
        public void GetMappedPosition_IsDeterministic_AndPositionsWithinBounds()
        {
            var samplePoints = new (int x, int y)[]
            {
                (0, 0),
                (PixelEncoder.ImgWidth - 1, PixelEncoder.ImgHeight - 1),
                (500, 300),
                (123, 456),
                (241, 241) // used in threshold test as well
            };

            foreach (var (x, y) in samplePoints)
            {
                var p1 = pixelEncoder.GetMappedPosition(x, y);
                var p2 = pixelEncoder.GetMappedPosition(x, y);
                Assert.AreEqual(p1.X, p2.X);
                Assert.AreEqual(p1.Y, p2.Y);

                Assert.GreaterOrEqual(p1.X, 0);
                Assert.Less(p1.X, 3_000_000);
                Assert.GreaterOrEqual(p1.Y, 0);
                Assert.Less(p1.Y, 10);
            }
        }

        #endregion

        #region Existing LocationEncoder Test

        [Test]
        public void TestLocationEncoder_NoDuplicatePositions()
        {
            int loc1X = 2333;
            int loc1Y = 1200;

            var posList = locationEncoder.Encode(loc1X, loc1Y);

            for (int i = 0; i < posList.Count; i++)
            {
                int x = posList[i].X;
                int y = posList[i].Y;

                for (int j = 0; j < posList.Count; j++)
                {
                    if (i != j && posList[j].X == x && posList[j].Y == y)
                        Assert.Fail("Duplicate position found in LocationEncoder output.");
                }
            }

            Assert.Pass();
        }

        #endregion
    }
}