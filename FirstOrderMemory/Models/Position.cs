namespace FirstOrderMemory.Models
{
    public class Position
    {
        public int X { get; private set; }
        public int Y { get; private set; }

        public int Z { get; private set; }

        public Position(int x, int y, int z = 0)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public override string ToString()
        {           
                return X.ToString() + " " + Y.ToString() + " " + Z.ToString();
        }


    }
}
