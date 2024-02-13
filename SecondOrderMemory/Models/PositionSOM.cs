using Common;

namespace SecondOrderMemory.Models
{
    public class Position_SOM : Position
    {
        public char W { get; private set; }

        public Position_SOM(int x, int y, int z = 0, char w = 'N'): base(x, y, z)
        {
            W = w;           
        }

        public static new Position_SOM ConvertStringToPosition(string key)
        {
            var parts = key.Split('-');

            int x = Convert.ToInt32(parts[0]);
            int y = Convert.ToInt32(parts[1]);
            int z = Convert.ToInt32(parts[2]);
            char w = Convert.ToChar(parts[3]);
            return new Position_SOM(x, y, z, w);
        }

        public static string ConvertIKJtoString(int i, int j, int k, char w = 'N')
        {
            if (w == 'N')
                return i.ToString() + "-" + j.ToString() + "-" + k.ToString();
            else if (w == 'T')
                return i.ToString() + "-" + j.ToString() + "-" + k.ToString() + "-" + w;
            else if (w == 'A')
                return i.ToString() + "-" + j.ToString() + "-" + k.ToString() + "-" + w;

            return i.ToString() + "-" + j.ToString() + "-" + k.ToString();
        }

        public bool Equals(Position_SOM pos)
        {
            return X == pos.X && Y == pos.Y && Z == pos.Z && W == pos.W;
        }

        public new string ToString()
        {
            return X.ToString() + "-" + Y.ToString() + "-" + Z.ToString() + "-" + W.ToString();
        }
    }
}
