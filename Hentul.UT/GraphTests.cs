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

            Assert.AreEqual(1, graph.Base.Right.Up.PixelCordinates.X);
            Assert.AreEqual(1, graph.Base.Right.Up.PixelCordinates.Y);
        }

        [Test]
        public void TestAddGraphNode2()
        {
            Position2D posToAdd = new Position2D(10, 10);

            graph.AddNewNode(posToAdd);      //Create graph for (10,10)


            Position2D nextPos = new Position2D(5, 10); //Create graph for (5, 10)
            graph.AddNewNode(nextPos);

            Node currenNode = graph.Base;
            while (currenNode.PixelCordinates.X != nextPos.X)
            {
                currenNode = currenNode.Right;
            }
            Assert.AreEqual(currenNode.PixelCordinates.X, nextPos.X);


            while (currenNode.PixelCordinates.Y != nextPos.Y) { currenNode = currenNode.Up; }
            Assert.AreEqual(currenNode.PixelCordinates.Y, nextPos.Y);
        }

        [Test]
        public void TestAddGraphNode3()
        {
            Position2D posToAdd = new Position2D(9, 9);

            graph.AddNewNode(posToAdd);

            Node currenNode = graph.Base;
            while (currenNode.PixelCordinates.X < posToAdd.X)
            {
                currenNode = currenNode.Right;
            }

            Assert.AreEqual(posToAdd.X, currenNode.PixelCordinates.X);


            while (currenNode.PixelCordinates.Y < posToAdd.Y) { currenNode = currenNode.Up; }
            Assert.AreEqual(posToAdd.Y, currenNode.PixelCordinates.Y);
        }


        [Test]
        public void TestLiteUpObject()
        {
            Position2D posToAdd = new Position2D(9, 9);

            graph.AddNewNode(posToAdd);

            RecognisedEntity entity = TestUtils.GenerateRandomEntities(1).ElementAt(0);

            bool status = graph.LightUpObject(entity);

            Assert.IsTrue(status);
            
            foreach(var sensei in entity.ObjectSnapshot)
            {
                foreach (var senseloc in sensei.sensLoc)
                {
                    Node node = graph.GetNode(Position2D.ConvertStringToPosition(senseloc.Key));

                    Assert.IsTrue(node.Flags.Contains(entity.Label));
                }
            }
        }

        [Test]
        public void TestUnloadObject()
        {
            Position2D posToAdd = new Position2D(9, 9);

            graph.AddNewNode(posToAdd);

            RecognisedEntity entity = TestUtils.GenerateRandomEntities(1).ElementAt(0);

            bool status = graph.LightUpObject(entity);

            Assert.IsTrue(status);

            List<Position2D> posList = new List<Position2D>();

            graph.UnloadObject(entity);

            foreach (var sensei in entity.ObjectSnapshot)
            {
                foreach (var senseloc in sensei.sensLoc)
                {                    
                    Assert.IsTrue(graph.GetNode(Position2D.ConvertStringToPosition(senseloc.Key)).Flags.Contains(entity.Label) == false);
                }
            }                        
        }


        [Test]
        public void TestGetDiferentiablePositionBetweenObjects()
        {
            List<RecognisedEntity> recgEntities = TestUtils.GenerateRandomEntities(2);

            RecognisedEntity first = recgEntities.ElementAt(0);

            RecognisedEntity second = recgEntities.ElementAt(1);


            bool status1 = graph.LightUpObject(first);

            Assert.IsTrue(status1);

            bool status2 = graph.LightUpObject(second);

            Assert.IsTrue(status2);

            var posList = graph.CompareTwoObjects(first, second);

            foreach(var pos in posList)
            {
                Assert.AreEqual(graph.GetNode(pos).Flags.Count , 1);
                Assert.IsTrue(graph.GetNode(pos).Flags.Contains(first.Label));
            }

        }
    }
}
