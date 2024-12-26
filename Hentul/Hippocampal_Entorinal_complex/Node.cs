///Author Deric Pinto.
namespace Hentul.Hippocampal_Entorinal_complex
{
    using Common;

    public class Node
    {
        //<Object Label/ ID , KeyValuePair<BBMID, List<Position2D> ActiveColumns>>
        public SortedDictionary<string, KeyValuePair<int, List<Position2D>>> Data { get; set; }

        public Position2D cursorPosition { get; private set; }

        public Node Left { get; set; }

        public Node Right { get; set; }

        public Node Up { get; set; }

        public Node Down { get; set; }


        public Node() 
        {
            Data = null;
            cursorPosition = null;
            Left = null;
            Right = null;
            Up = null;
            Down = null;
        }

        public Node(Position2D pos)
        {
            Data = null;
            cursorPosition = pos;
            Left = null;
            Right = null;
            Up = null;
            Down = null;
        }

        public Node(Position2D cursorPos, SortedDictionary<string, KeyValuePair<int, List<Position2D>>> data)
        {
            this.Data = data;
            this.cursorPosition = cursorPos;
            Left = null;
            Right = null;
            Up = null;
            Down = null;
        }
    }   
}
