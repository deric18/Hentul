namespace SecondOrderMemoryUnitTest
{
    using Common;

    [TestClass]
    public class EncoderTest
    {
        ByteEncoder encoder;

        [TestInitialize]
        public void Setup()
        {
            encoder = new ByteEncoder(100, 8);
        }

        [TestMethod]
        public void TestByteEncoder1()
        {
            byte toEncode1 = (byte)1;

            byte toEncode2 = (byte)9;

            encoder.Encode(toEncode1);

            SDR result1 = encoder.GetDenseSDR();

            encoder.Encode(toEncode2);

            SDR result2 = encoder.GetDenseSDR();

            Assert.AreEqual(result1.ActiveBits.Count, result2.ActiveBits.Count);
        }
    }
}
