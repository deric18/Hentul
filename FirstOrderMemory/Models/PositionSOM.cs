using Common;

namespace FirstOrderMemory.Models
{
    public class Position_SOM : Position, IComparable
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
                return i.ToString() + "-" + j.ToString() + "-" + k.ToString() + '-' + 'N';
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
      
        public int CompareTo(object? obj1)
        {
            if (obj1.GetType() == typeof(Position_SOM))
            {
                Position_SOM obj = (Position_SOM)obj1;

                if (X + Y + Z > obj.X + obj.Y + obj.Z) return -1;
                else if (obj.X + obj.Y + obj.Z < obj.X + obj.Y + obj.Z) return +1;
                else
                    return 0;
            }
            return 0;
        }
    }
}
