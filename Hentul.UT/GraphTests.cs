//Author : Deric Pinto
namespace Hentul.UT
{
    using Hentul.Hippocampal_Entorinal_complex;    
    using Common;

    public class GraphTests
    {
        Graph graph = null;

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

            Assert.IsTrue(graph.AddNewNode(posToAdd));

            Assert.AreEqual(1, graph.Base.Right.Up.cursorPosition.X);
            Assert.AreEqual(1, graph.Base.Right.Up.cursorPosition.Y);
        }

        [Test]
        public void TestAddGraphNode2()
        {
            Position2D posToAdd = new Position2D(10, 10);

            graph.AddNewNode(posToAdd);      //Create graph for (10,10)


            Position2D nextPos = new Position2D(5, 10); //Create graph for (5, 10)
            graph.AddNewNode(nextPos);

            Node currenNode = graph.Base;
            while (currenNode.cursorPosition.X != nextPos.X)
            {
                currenNode = currenNode.Right;
            }
            Assert.AreEqual(currenNode.cursorPosition.X, nextPos.X);


            while (currenNode.cursorPosition.Y != nextPos.Y) { currenNode = currenNode.Up; }
            Assert.AreEqual(currenNode.cursorPosition.Y, nextPos.Y);
        }

        [Test]
        public void TestAddGraphNode3()
        {
            Position2D posToAdd = new Position2D(9, 9);
            SortedDictionary<string, KeyValuePair<int, List<Position2D>>> dict = new SortedDictionary<string, KeyValuePair<int, List<Position2D>>>()
            {
                {"1-2", new KeyValuePair<int, List<Position2D>>(1, new List<Position2D>() {}) }
            };

            graph.AddNewNode(posToAdd);

            Node currenNode = graph.Base;
            while (currenNode.cursorPosition.X < posToAdd.X)
            {
                currenNode = currenNode.Right;
            }

            Assert.AreEqual(posToAdd.X, currenNode.cursorPosition.X);


            while (currenNode.cursorPosition.Y < posToAdd.Y) { currenNode = currenNode.Up; }
            Assert.AreEqual(posToAdd.Y, currenNode.cursorPosition.Y);
        }


        [Test]
        public void TestLiteUpObject()
        {

        }


        [Test]
        public void TestUnloadObject()
        {

        }


        [Test]
        public void TestGetDiferentiablePositionBetweenObjects()
        {

        }
    }
}
