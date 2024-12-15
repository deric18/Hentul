namespace Hentul.UT
{
    using Hentul.Hippocampal_Entorinal_complex;
    using FirstOrderMemory.Models;
    using Common;

    public  class MapperTests
    {

        Orchestrator orchestrator;
        Mapper mapper;

        [SetUp]
        public void Setup()
        {
            orchestrator = Orchestrator.GetInstance(true, false, NetworkMode.PREDICTION);
            mapper = orchestrator.Mapper;
        }


        [Test]
        public void TestGetPositionForActiveBit()
        {
            // This is the fucking culprit , took so much of my fucking time. its not funny , squash this bug like a fucking cockroach.
        }
    }
}
