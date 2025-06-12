using System.Diagnostics.CodeAnalysis;

namespace Common
{
    public class Position_SOM : Position, IEqualityComparer<Position_SOM>, IComparable<Position>
    {
        public char W { get; private set; }

        public Position_SOM(int x, int y, int z = 0, char w = 'N') : base(x, y, z)
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

        public bool Equals(Position_SOM pos, bool checkXNYOnly = false)
        {
            return checkXNYOnly ? ( X == pos.X && Y == pos.Y ) : ( X == pos.X && Y == pos.Y && Z == pos.Z && W == pos.W );
        }

        public new string ToString()
        {
            return X.ToString() + "-" + Y.ToString() + "-" + Z.ToString() + "-" + W.ToString();
        }

        public bool Equals(Position_SOM? x, Position_SOM? y)
        {
            if (x == null || y == null)
                return false;

            return x.W == y.W && x.X == y.X && x.Y == y.Y && x.Z == y.Z;
        }

        public int GetHashCode([DisallowNull] Position_SOM obj)
        {
            return obj.W + obj.X + obj.Y + obj.Z;
        }

        public int CompareTo(Position other)
        {
            if (X > other.X)
                return 1;
            else
            {
                if (Y > other.Y)
                    return 1;
                else
                {
                    if (Z > other.Z)
                        return 1;
                    else
                    {
                        if (X == other.X && Y == other.Y && Z == other.Z)
                            return 0;
                    }
                }
            }

            return -1;
        }
    }
}
