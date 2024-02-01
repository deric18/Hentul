namespace Common
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

        public bool Equals(Position pos)
        {
            return X == pos.X && Y == pos.Y && Z == pos.Z;
        }

        public override string ToString()
        {
            return X.ToString() + "-" + Y.ToString() + "-" + Z.ToString();
        }

        public static Position ConvertStringToPosition(string key)
        {
            var parts = key.Split('-');
            int x = Convert.ToInt32(parts[0]);
            int y = Convert.ToInt32(parts[1]);
            int z = Convert.ToInt32(parts[2]);

            return new Position(x, y, z);
        }



        public static string ConvertIKJtoString(int i, int j, int k)
        {
            return i.ToString() + "-" + j.ToString() + "-" + k.ToString();
        }
    }
}
