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
        public void PixelEncoder_Constructor_ThrowsWhenTotalBitsLessThanPixelCount()
        {
            // Suggestion: constructor should validate that TotalBits >= PixelCount and throw otherwise.
            // This test documents the expected behavior (will fail until validation is added).
            Assert.Throws<InvalidOperationException>(() => new PixelEncoder(100, 1));
        }

        [Test]
        public void PixelEncoder_EncodeBitmap_ProducesSDRWithExpectedSparsity()
        {
            using var bmp = new Bitmap(PixelEncoder.ImgWidth, PixelEncoder.ImgHeight);
            var sdr = pixelEncoder.EncodeBitmap(bmp);

            // Active bits should equal number of input pixels
            Assert.AreEqual(PixelEncoder.ImgWidth * PixelEncoder.ImgHeight, sdr.ActiveBits.Count);

            // SDR dimensions must match encoder grid
            Assert.AreEqual(3_000_000, sdr.Length);
            Assert.AreEqual(10, sdr.Breadth);

            // Density (sparsity) ~ 2% => tolerance small
            double density = sdr.ActiveBits.Count / (double)(3_000_000L * 10L);
            Assert.AreEqual(0.02, density, 0.0001, "Density should be approximately 2%");
        }

        [Test]
        public void PixelEncoder_UniqueMappings_NoDuplicates()
        {
            using var bmp = new Bitmap(PixelEncoder.ImgWidth, PixelEncoder.ImgHeight);
            var lookup = pixelEncoder.BuildMappingLookup(bmp);

            // All input pixels present
            Assert.AreEqual(PixelEncoder.PixelCount, lookup.Count);

            // All mapped positions must be unique
            var unique = new HashSet<Position_SOM>(lookup.Values);
            Assert.AreEqual(lookup.Count, unique.Count, "Mapped Position_SOM values should be unique (no collisions).");
        }

        [Test]
        public void PixelEncoder_GetMappedPosition_OutOfRangeThrows()
        {
            // Negative x
            Assert.Throws<ArgumentOutOfRangeException>(() => pixelEncoder.GetMappedPosition(-1, 0));
            // x >= ImgWidth
            Assert.Throws<ArgumentOutOfRangeException>(() => pixelEncoder.GetMappedPosition(PixelEncoder.ImgWidth, 0));
            // Negative y
            Assert.Throws<ArgumentOutOfRangeException>(() => pixelEncoder.GetMappedPosition(0, -1));
            // y >= ImgHeight
            Assert.Throws<ArgumentOutOfRangeException>(() => pixelEncoder.GetMappedPosition(0, PixelEncoder.ImgHeight));
        }

        [Test]
        public void PixelEncoder_MappedPositionsWithinBounds_ForSamplePoints()
        {
            var samples = new (int x, int y)[]
            {
                (0,0),
                (PixelEncoder.ImgWidth - 1, PixelEncoder.ImgHeight - 1),
                (500, 300),
                (123, 456)
            };

            foreach (var (x, y) in samples)
            {
                var p = pixelEncoder.GetMappedPosition(x, y);
                Assert.GreaterOrEqual(p.X, 0);
                Assert.Less(p.X, 3_000_000);
                Assert.GreaterOrEqual(p.Y, 0);
                Assert.Less(p.Y, 10);
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