///Author : Deric Pinto
namespace Hentul.Hippocampal_Entorinal_complex
{
    using Common;
    public class Graph
    {

        #region COSNTRUCTORS 

        public Node Base { get; private set; }

        public uint RightCount { get; private set; }

        public uint UpCount { get; private set; }


        public uint TotalCount { get; private set; }

        public static Graph _graph;

        public List<string> CurrentLabels { get; private set; }

        private Graph()
        {
            Base = new Node(new Position2D(0, 0), null);
            CurrentLabels = new List<string>();
            RightCount = 0;
            UpCount = 0;
            TotalCount = 0;
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

        public List<Position2D> GetOnlyFirstDifferential(RecognisedEntity first, RecognisedEntity second)
        {
            List<Position2D> toReturn;

            List<Position2D> firstList, secondList;

            firstList = GetAllPositionsForLabel(first.Label);

            secondList = GetAllPositionsForLabel(second.Label);

            toReturn = Position2D.RemoveSecondFromFirst(firstList, secondList);

            return toReturn;
        }

        public void GetFavouritePositionsForObject(RecognisedEntity entity)
        {
            throw new NotImplementedException();
        }

        public bool LightUpObject(RecognisedEntity entity)
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

            SetAllPositionsForLabel(posList, entity.Label);

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


        #region PRIVATE METHODS       


        private List<Position2D> GetAllPositionsForLabel(string label)
        {
            if (label == null || label == string.Empty)
                return null;

            if(_graph.Base == null)
            {
                throw new InvalidDataException("Base cannot be null!");
            }

            Node current = this.Base;

            List<Position2D> retList= new List<Position2D> ();

            for( int i = 0; i < RightCount; i++)
            {
                for(int j = 0; j < UpCount; i++)
                {
                    if(current.Flags.Contains(label))
                    {
                        retList.Add(new Position2D(i, j));
                    }

                    current = current.Up;
                }

                current = current.Right;
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
                Node nodetoLitUp = GetNode(pos);

                if(nodetoLitUp.LiteUpNode(label))
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

            if (currentNode == null || currentNode?.cursorPosition?.X != pos.X || currentNode?.cursorPosition?.Y != pos.Y)
            {
                throw new InvalidOperationException("Bugs in Parsing Logic!!!");
            }

            return true;
        }


        #endregion

    }
}
