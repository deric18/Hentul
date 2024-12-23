using Common;

///Author : Deric Pinto
namespace Hentul.Hippocampal_Entorinal_complex
{
    public class Graph
    {
        public Node Base { get; private set; }

        public static Graph _graph;

        private Graph() 
        {
            Base = new Node(new Position2D(0,0), null);
        }

        public static Graph GetInstance()
        {
            if (_graph == null)
            {
                _graph = new Graph();
            }

            return _graph;
        }

        public bool AddNewNode(Position2D pos, SortedDictionary<string, KeyValuePair<int, List<Position2D>>> data)
        {
            if(pos == null || pos?.X <= 0 || pos?.Y <= 0) return false;

            if(data == null) return false;            

            Node newNode = null;

            Node currentNode = ParseGraph(pos);

            if(currentNode == null)
            {
                throw new InvalidOperationException("Bugs in Parsing Logic!!!");
            }
                
            if(currentNode.cursorPosition.X - pos.X == 1)
            {
                newNode = new Node(pos, data);
                currentNode.Right = newNode;
                newNode.Left = currentNode;
            }
            
            if(currentNode.cursorPosition.Y - pos.Y == 1)
            {
                if(newNode == null)
                {
                    newNode = new Node(pos, data);
                }

                currentNode.Up = newNode;
                newNode.Down = currentNode;
                return true;
            }


            return false;
        }

        private Node ParseGraph(Position2D? pos)
        {
            int offsetX = pos.X;
            int offsetY = pos.Y;

            Node currentNode = Base;

            while (offsetX > 0 || offsetY > 0)
            {
                if (offsetX > 0)
                {
                    while(offsetX > 0)
                    {
                        if(currentNode.Right != null)
                        {
                            currentNode = currentNode.Right;
                            offsetX--;
                        }
                        else
                        {
                            break;
                        }
                    }

                    while(pos.X - offsetX > 0)
                    {
                        currentNode.Right = new Node(new Position2D(++offsetX, offsetY));
                        currentNode.Right.Left = currentNode;
                    }
                }
                
                if (offsetY > 0)
                {
                    while (offsetY > 0)
                    {
                        if (currentNode.Down != null)
                        {
                            currentNode = currentNode.Down;
                            offsetY--;
                        }
                        else
                        {
                            break;
                        }
                    }

                    while (pos.Y - offsetY > 0)
                    {
                        currentNode.Up = new Node(new Position2D(offsetX, ++offsetY));
                        currentNode.Up.Down = currentNode;
                    }
                }
            }

            return currentNode; 
        }
    }
}
