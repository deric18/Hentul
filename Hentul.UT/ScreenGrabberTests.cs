namespace Hentul.UT
{
    using Hentul;
    using System.Diagnostics;

    public class ScreenGrabberTest
    {
        ScreenGrabber sg;

        [SetUp]
        public void Setup()
        {
           
        }

        [Test]
        public void MemoryTest()
        {

            //Stopwatch sw = new Stopwatch();

            //sw.Start();

            sg = new ScreenGrabber(1);           

            int a = 0;

            //sw.Stop();

            //Console.WriteLine(sw.Elapsed.ToString());

            //sg.Grab();

            //sg.ProcessPixelData();




            Assert.Pass();
        }

        [Test]
        public void TestSCreenGrabberConstructor()
        {
            sg = new ScreenGrabber(30);

            Assert.Pass();
        }
    }
}