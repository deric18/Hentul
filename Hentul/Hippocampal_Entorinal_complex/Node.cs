///Author Deric Pinto.
namespace Hentul.Hippocampal_Entorinal_complex
{
    using Common;

    public class Node
    {
        //<Object Label/ ID , KeyValuePair<BBMID, List<Position2D> ActiveColumns>>
        public HashSet<string> Flags { get; private set; }

        public Position2D cursorPosition { get; private set; }

        public Node Left { get; set; }

        public Node Right { get; set; }

        public Node Up { get; set; }

        public Node Down { get; set; }


        public Node() 
        {
            Flags = null;
            cursorPosition = null;
            Left = null;
            Right = null;
            Up = null;
            Down = null;
        }

        public bool CheckNode(string label) => Flags.Contains(label);

        public bool LiteUpNode(string label) => Flags.Add(label);

        public bool UnloadLabel(string label) => Flags.Remove(label);

        public Node(Position2D pos)
        {
            Flags = new HashSet<string>();
            cursorPosition = pos;
            Left = null;
            Right = null;
            Up = null;
            Down = null;
        }

        public Node(Position2D cursorPos, SortedDictionary<string, KeyValuePair<int, List<Position2D>>> data)
        {
            Flags = new HashSet<string>();
            this.cursorPosition = cursorPos;
            Left = null;
            Right = null;
            Up = null;
            Down = null;
        }
    }   
}
