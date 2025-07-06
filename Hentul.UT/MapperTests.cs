namespace Hentul.UT
{
    using Hentul.Hippocampal_Entorinal_complex;
    using FirstOrderMemory.Models;
    using Common;
    using Hentul.Encoders;

    public  class MapperTests
    {

        Orchestrator orchestrator;
        PixelEncoder mapper;

        [SetUp]
        public void Setup()
        {
            orchestrator = Orchestrator.GetInstance(true, false, NetworkMode.PREDICTION);
        }


        [Test]
        public void TestGetPositionForActiveBit()
        {

            
        }
    }
}
