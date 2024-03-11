namespace Hentul.UT
{
    using Hentul;
    using System.Diagnostics;

    public class ScreenGrabberTest
    {
        ScreenGrabber sg;
        Random rand;

        [SetUp]
        public void Setup()
        {
            rand = new Random();
        }

        [Test]
        public void MultipleInstanceTest()
        {
            int count = 3;

            sg = new ScreenGrabber(count);

            for (int i = 0; i < count; i++)
            {

                Assert.That(sg.somBBM[i].ApicalLineArray.Length, Is.EqualTo(100));

                Assert.AreEqual(4, sg.somBBM[i].Columns[rand.Next(0, 9), rand.Next(0, 9)].Neurons[rand.Next(0, 9)].ProximoDistalDendriticList.Count);

                Assert.AreEqual(2, sg.somBBM[i].Columns[rand.Next(0, 9), rand.Next(0, 9)].Neurons[rand.Next(0, 9)].AxonalList.Count);

                Assert.IsNotNull(sg.somBBM[i].Columns[rand.Next(0, 9), rand.Next(0, 9)].Neurons[rand.Next(0, 9)].ProximoDistalDendriticList.ElementAt(k));

                Assert.IsNotNull(sg.somBBM[i].Columns[rand.Next(0, 9), rand.Next(0, 9)].Neurons[rand.Next(0, 9)].AxonalList.ElementAt(k));

            }
        }
    }
}