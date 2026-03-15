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

            RecognisedVisualEntity entity = TestUtils.GenerateRandomEntities(1).ElementAt(0);

            bool status = graph.LoadObject(entity);

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

            RecognisedVisualEntity entity = TestUtils.GenerateRandomEntities(1).ElementAt(0);

            bool status = graph.LoadObject(entity);

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
            List<RecognisedVisualEntity> recgEntities = TestUtils.GenerateRandomEntities(2);

            RecognisedVisualEntity first = recgEntities.ElementAt(0);

            RecognisedVisualEntity second = recgEntities.ElementAt(1);


            bool status1 = graph.LoadObject(first);

            Assert.IsTrue(status1);

            bool status2 = graph.LoadObject(second);

            Assert.IsTrue(status2);

            var posList = graph.CompareTwoObjects(first, second);

            foreach(var pos in posList)
            {
                Assert.AreEqual(graph.GetNode(pos).Flags.Count , 1);
                Assert.IsTrue(graph.GetNode(pos).Flags.Contains(first.Label));
            }

        }
        [Test]
        public void TestDirectLoadObjectBounds_StoresAndRetrieves()
        {
            // Simulates what RecordObjectInGraph does for the visualiser
            string label = "TestVisObject_" + Guid.NewGuid().ToString("N")[..6];
            int minX = 300, minY = 200, maxX = 550, maxY = 430;

            graph.DirectLoadObjectBounds(label, minX, minY, maxX, maxY);

            var all = graph.GetAllObjectsInEnvironment();
            var stored = all.FirstOrDefault(o => o.Label == label);

            Assert.IsNotNull(stored, "DirectLoadObjectBounds should store the bounds so GetAllObjectsInEnvironment finds it.");
            Assert.AreEqual(minX, stored.MinX);
            Assert.AreEqual(minY, stored.MinY);
            Assert.AreEqual(maxX, stored.MaxX);
            Assert.AreEqual(maxY, stored.MaxY);
            Assert.AreEqual(maxX - minX, stored.Width,  "Width should be MaxX - MinX");
            Assert.AreEqual(maxY - minY, stored.Height, "Height should be MaxY - MinY");
        }

        [Test]
        public void TestDirectLoadObjectBounds_MultipleObjects()
        {
            string label1 = "Obj_A_" + Guid.NewGuid().ToString("N")[..6];
            string label2 = "Obj_B_" + Guid.NewGuid().ToString("N")[..6];

            graph.DirectLoadObjectBounds(label1, 100, 150, 300, 400);
            graph.DirectLoadObjectBounds(label2, 800, 500, 1100, 750);

            var all = graph.GetAllObjectsInEnvironment();

            Assert.IsTrue(all.Any(o => o.Label == label1), "Object A should be in environment");
            Assert.IsTrue(all.Any(o => o.Label == label2), "Object B should be in environment");
        }
    }
}
