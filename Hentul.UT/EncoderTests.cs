namespace Hentul.UT
{
    using NUnit.Framework;

    public class EncoderTest
    {
        LocationEncoder encoder;

        [SetUp]
        public void Setup()
        {
            encoder = new LocationEncoder(Common.iType.TEMPORAL);
        }

        [Test]
        public void TestLocationEncoder1()
        {
            int loc1X = 2333;
            int loc1Y = 1200;

            int loc2X = 1250;
            int loc2Y = 957;

            var posList = encoder.Encode(loc1X, loc1Y);


            for (int i = 0; i < posList.Count; i++)
            {
                int x = posList[i].X;
                int y = posList[i].Y;

                for (int j = 0; j < posList.Count; j++)
                {
                    if (i != j)
                    {
                        if (posList[j].X == x && posList[j].Y == y)
                        {
                            Assert.Fail();
                        }
                    }
                }
            }

            Assert.Pass();
        }
    }
}