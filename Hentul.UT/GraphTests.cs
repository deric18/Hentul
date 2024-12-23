//Author : Deric Pinto
namespace Hentul.UT
{
    using Hentul.Hippocampal_Entorinal_complex;    
    using Common;

    public class GraphTests
    {
        Graph graph;

        [SetUp]
        public void Setup()
        {
            graph = Graph.GetInstance();
        }


        [Test]
        public void TestAddGraphNode1()
        {
            Position2D posToAdd = new Position2D(1, 1);
            SortedDictionary<string, KeyValuePair<int, List<Position2D>>> dict = new SortedDictionary<string, KeyValuePair<int, List<Position2D>>>()
            {
                {"1-2", new KeyValuePair<int, List<Position2D>>(1, new List<Position2D>() {}) }
            };

            graph.AddNewNode(posToAdd, dict);

            Assert.AreEqual(1, graph.Base.Right.Down.cursorPosition.X);
            Assert.AreEqual(1, graph.Base.Down.Right.cursorPosition.Y);
        }
    }
}
