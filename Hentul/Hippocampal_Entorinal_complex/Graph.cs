///Author : Deric Pinto
namespace Hentul.Hippocampal_Entorinal_complex
{
    using Common;

    /// <summary>
    /// A (0,0) Co-oridnate based Graph Quad Tree.
    /// </summary>
    public class Graph
    {

        #region COSNTRUCTORS 

        public Node Base { get; private set; }
        
        public uint TotalCount { get; private set; }

        public uint MaxRight { get; private set; }

        public uint MaxUp { get; private set; }

        public static Graph _graph;

        public List<string> CurrentLabels { get; private set; }

        private Graph()
        {
            Base = new Node(new Position2D(0, 0), null);
            CurrentLabels = new List<string>();            
            TotalCount = 0;
            MaxRight = 0;
            MaxUp = 0;
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

        public List<Position2D> CompareTwoObjects(RecognisedEntity first, RecognisedEntity second)
        {
            List<Position2D> toReturn;

            List<Position2D> firstList, secondList;

            firstList = GetAllPositionsForLabel(first.Label);

            secondList = GetAllPositionsForLabel(second.Label);

            if (firstList.Count == 0 || secondList.Count == 0)
            {
                int bp = 1;
                throw new InvalidOperationException("Incapable of Doing some of the lame Stuff!!");
            }

            toReturn = Position2D.RemoveSecondFromFirst(firstList, secondList);

            return toReturn;
        }

        public List<Position2D> GetFavouritePositionsForObject(RecognisedEntity entity) => entity.FavouritePositions;                    

        public bool LoadObject(RecognisedEntity entity)
        {
            if (entity.ObjectSnapshot?.Count == 0)
                return false;

            List<Position2D> posList = new List<Position2D>();

            foreach (var item in entity.ObjectSnapshot)
            {
                foreach (var kvpKey in item.sensLoc.Keys)
                {
                    Position2D position = Position2D.ConvertStringToPosition(kvpKey);

                    posList.Add(position);
                }
            }

            if(!SetAllPositionsForLabel(posList, entity.Label))
            {
                throw new InvalidOperationException("Unable to Lite Up all the objects!");
            }

            if (CurrentLabels.Contains(entity.Label) == false)
                CurrentLabels.Add(entity.Label);

            return true;
        }

        public bool UnloadObject(RecognisedEntity entity)
        {
            if (entity.ObjectSnapshot?.Count == 0 || entity.Label == string.Empty)
                return false;

            List<Position2D> posList = new List<Position2D>();

            foreach (var item in entity.ObjectSnapshot)
            {
                foreach (var kvpKey in item.sensLoc.Keys)
                {
                    Position2D position = Position2D.ConvertStringToPosition(kvpKey);

                    posList.Add(position);
                }
            }

            UnloadPositionsForLabel(posList, entity.Label);

            if (CurrentLabels.Contains(entity.Label) == false)
                CurrentLabels.Add(entity.Label);

            return true;
        }

        public Node GetNode(Position2D? pos, Node currentNode = null, bool speedUpY = false)
        {
            int offsetX = pos.X;
            int offsetY = pos.Y;

            if (currentNode == null)
                currentNode = _graph.Base;

            int currentX = currentNode.PixelCordinates.X;
            int currentY = currentNode.PixelCordinates.Y;

            Node BaseNode = currentNode;

            Node cacheNode = currentNode.Right;

            for (int i = BaseNode.PixelCordinates.X; i <= pos.X; i++)
            {
                if (i > BaseNode.PixelCordinates.X)
                {
                    currentNode = cacheNode;
                }

                if (currentNode.Right == null)
                {
                    currentNode.Right = new Node(new Position2D(i + 1, currentNode.PixelCordinates.Y));
                    currentNode.Right.Left = currentNode;
                    cacheNode = currentNode.Right;
                    TotalCount++;
                    MaxRight++;
                }
                else
                {
                    currentNode = currentNode.Right;
                    cacheNode = currentNode;
                }

                if (speedUpY == false && currentNode.PixelCordinates.Y != pos.Y)
                {
                    for (int j = BaseNode.PixelCordinates.Y + 1; j <= pos.Y; j++)
                    {
                        if (currentNode.Up == null)
                        {
                            currentNode.Up = new Node(new Position2D(i, j));
                            currentNode.Up.Down = currentNode;
                            currentNode = currentNode.Up;
                            TotalCount++;
                            if (currentNode.PixelCordinates.Y > MaxUp)
                                MaxUp = (uint)currentNode.PixelCordinates.Y;
                        }
                        else
                        {
                            currentNode = currentNode.Up;

                            if (currentNode.PixelCordinates.Equals(pos))
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


        #region PRIVATE METHODS       

        private List<Position2D> GetAllPositionsForLabel(string label)
        {
            if (label == null || label == string.Empty)
                return null;

            if(!CurrentLabels.Contains(label))
                   throw new InvalidOperationException("Supplied Label is not learned!");                                       

            if(_graph.Base == null)
            {
                throw new InvalidDataException("Base cannot be null!");
            }

            Node current = this.Base;

            List<Position2D> retList= new List<Position2D> ();

            for( int i = 0; current.Right != null; i++)
            {
                Node cacheBase = current;

                for(int j = 0; current.Up != null; i++)
                {
                    if(current != null &&  current.Flags.Contains(label))
                    {
                        retList.Add(current.PixelCordinates);
                    }

                    if (current.Up == null)
                        break;
                    else
                         current = current.Up;
                }

                current = cacheBase.Right;
            }

            return retList;
        }

        private bool SetAllPositionsForLabel(List<Position2D> posList, string label)
        {
            if (label == null || label == string.Empty)
                throw new InvalidOperationException("Label cannot be null`");

            if (_graph.Base == null)
            {
                throw new InvalidDataException("Base cannot be null!");
            }

            int count = 0;            
            Node current = this.Base;

            foreach (var pos in posList)
            {
                Node nodetoLiteUp = GetNode(pos);

                if(nodetoLiteUp.LiteUpNode(label))
                {
                    count++;
                }
            }

            return count == posList.Count;
        }

        private bool UnloadPositionsForLabel(List<Position2D> posList, string label)
        {
            if (label == null || label == string.Empty)
                throw new InvalidOperationException("Label cannot be null`");

            if (_graph.Base == null)
            {
                throw new InvalidDataException("Base cannot be null!");
            }

            int count = 0;
            Node current = this.Base;

            foreach (var pos in posList)
            {
                Node nodetoLitUp = GetNode(pos);

                if (nodetoLitUp.UnloadLabel(label))
                {
                    count++;
                }
            }

            return count == posList.Count;
        }

        public bool AddNewNode(Position2D pos, Node betterNode = null, bool speedUpY = false)
        {
            if (pos == null || pos?.X <= 0 || pos?.Y <= 0) return false;

            Node currentNode = GetNode(pos, betterNode, speedUpY);

            if (currentNode == null || currentNode?.PixelCordinates?.X != pos.X || currentNode?.PixelCordinates?.Y != pos.Y)
            {
                throw new InvalidOperationException("Bugs in Parsing Logic!!!");
            }

            return true;
        }

        #endregion

    }
}
