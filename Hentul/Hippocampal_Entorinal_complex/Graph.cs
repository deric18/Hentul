///Author : Deric Pinto
namespace Hentul.Hippocampal_Entorinal_complex
{
    using Common;
    public class Graph
    {

        #region COSNTRUCTORS 

        public Node Base { get; private set; }

        public static Graph _graph;

        private Graph()
        {
            Base = new Node(new Position2D(0, 0), null);
        }

        public static Graph GetInstance()
        {
            if (_graph == null)
            {
                _graph = new Graph();
            }

            return _graph;
        }


        #endregion

        #region PUBLIC API

        public bool AddNewNode(Position2D pos, SortedDictionary<string, KeyValuePair<int, List<Position2D>>> data, Node betterNode = null, bool speedUpY = false)
        {
            if (pos == null || pos?.X <= 0 || pos?.Y <= 0) return false;

            if (data == null) return false;            

            Node currentNode = ParseGraph(pos, betterNode, speedUpY);

            if (currentNode == null ||  currentNode?.cursorPosition?.X != pos.X || currentNode?.cursorPosition?.Y != pos.Y)
            {
                throw new InvalidOperationException("Bugs in Parsing Logic!!!");
            }

            currentNode.Data = data;

            return true;
        }


        public void LightUpObjects(List<RecognisedEntity> entites)
        {
            foreach(var entity in entites)
            {
                LightUpObject(entity);
            }
        }

        public void GetFavouritePositionsForObject(RecognisedEntity entity)
        {
            throw new NotImplementedException();
        }

        public List<Position2D> GetDiferentiablePositionBetweenObjects(RecognisedEntity  first, RecognisedEntity second)
        {
            throw new NotImplementedException();
        }

        public List<Position2D> GetUnioun()
        {
            throw new NotImplementedException();
        }

        public List<Position2D> GetIntersection()
        {
            throw new NotImplementedException();
        }

        public List<Position2D> LoadObjectFrame(RecognisedEntity entity)
        {
            List<Position2D> listPositions = new List<Position2D>();

            foreach (var item in entity.ObjectSnapshot)
            {
                foreach (var kvp in item.sensLoc)
                {
                    Position2D position = Position2D.ConvertStringToPosition(kvp.Key);

                    listPositions.Add(position);

                    var data = kvp.Value;
                }
            }

            return listPositions;
        }

        #endregion

        #region PRIVATE METHODS

        private void LightUpObject(RecognisedEntity entity)
        {

        }

        private Node ParseGraph(Position2D? pos, Node currentNode = null, bool speedUpY = false)
        {
            int offsetX = pos.X;
            int offsetY = pos.Y;

            if (currentNode == null)
                currentNode = _graph.Base;

            int currentX = currentNode.cursorPosition.X;
            int currentY = currentNode.cursorPosition.Y;

            Node BaseNode = currentNode;

            Node cacheNode = currentNode.Right;

            for (int i = BaseNode.cursorPosition.X; i <= pos.X; i++)
            {
                if (i > BaseNode.cursorPosition.X)
                {
                    currentNode = cacheNode;
                }

                if (currentNode.Right == null)
                {
                    currentNode.Right = new Node(new Position2D(i + 1, currentNode.cursorPosition.Y));
                    currentNode.Right.Left = currentNode;
                    cacheNode = currentNode.Right;
                }
                else
                {
                    currentNode = currentNode.Right;
                    cacheNode = currentNode;
                }

                if (speedUpY == false && currentNode.cursorPosition.Y != pos.Y)
                {
                    for (int j = BaseNode.cursorPosition.Y + 1; j <= pos.Y; j++)
                    {
                        if (currentNode.Up == null)
                        {
                            currentNode.Up = new Node(new Position2D(i, j));
                            currentNode.Up.Down = currentNode;
                            currentNode = currentNode.Up;
                        }
                        else
                        {
                            currentNode = currentNode.Up;

                            if (currentNode.cursorPosition.Equals(pos))
                            {
                                return currentNode;
                            }
                        }
                    }
                }
            }
            
            return currentNode;
        }
                        

        #endregion

    }
}
