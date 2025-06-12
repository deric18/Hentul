namespace FirstOrderMemoryUnitTest
{    
    using NUnit.Framework;
    using FirstOrderMemory.Models.Encoders;
    using Common;
    
    public class EncoderTest
    {
        ByteEncoder encoder;

        [SetUp]
        public void Setup()
        {
            encoder = new ByteEncoder(100, 8);
        }

        [Test, Ignore("Needs Work!")]        
        public void TestBoolEncoderKeyConflict()
        {
            BoolEncoder be = new BoolEncoder(100, 20);            

            InvalidOperationException invalidOperationException = Assert.Throws<InvalidOperationException>(code: () => be.SetEncoderValues("1-8"));
            
        }

      [Test]
        public void TestByteEncoder1()
        {
            byte toEncode1 = (byte)1;

            byte toEncode2 = (byte)9;

            encoder.Encode(toEncode1);

            SDR_SOM result1 = encoder.GetSparseSDR();

            encoder.Encode(toEncode2);

            SDR_SOM result2 = encoder.GetSparseSDR();

            Assert.AreEqual(result1.ActiveBits.Count, result2.ActiveBits.Count);
        }

      [Test]
        public void TestByteEncoder2()
        {           

            encoder.Encode(255);

            var sdr = encoder.GetSparseSDR();

            Assert.AreEqual( 32 , sdr.ActiveBits.Count);
            Assert.AreEqual("0-4-0-N", sdr.ActiveBits[0].ToString());
            Assert.AreEqual("7-9-0-N", sdr.ActiveBits[31].ToString());

        }

      [Test]
        public void TestEncoderSparsity()
        {

        }
    }
}
