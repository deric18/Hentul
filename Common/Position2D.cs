namespace Common
{
    public class Position2D : IComparable<Position2D>
    {
        public int X { get; private set; }
        public int Y { get; private set; }
        public Position2D(int x, int y, int z = 0)
        {
            X = x;
            Y = y;
        }

        public bool Equals(Position2D pos)
        {
            return X == pos.X && Y == pos.Y;
        }

        public override string ToString()
        {
            return X.ToString() + "-" + Y.ToString();
        }

        public static Position2D ConvertStringToPosition(string posString)
        {
            var parts = posString.Split('-');
            int x = Convert.ToInt32(parts[0]);
            int y = Convert.ToInt32(parts[1]);

            return new Position2D(x, y);
        }


        public static List<Position2D> Unioun(List<Position2D> firstList, List<Position2D> second)
        {
            List<Position2D> retList = new List<Position2D>();

            foreach (var item in firstList)
            {
                if (second.Contains(item) == false)
                {
                    retList.Add(item);
                }
                else
                {
                    if (retList.Contains(item) == false)
                    {
                        retList.Add(item);
                    }
                }
            }

            return retList;
        }


        public static List<Position2D> Intersection(List<Position2D> first, List<Position2D> second)
        {
            List<Position2D> retList = new List<Position2D>();

            foreach (var item in first)
            {
                if (second.Contains(item))
                {
                    retList.Add(item);
                }
            }

            return retList;
        }

        public static List<Position2D> RemoveSecondFromFirst(List<Position2D> first, List<Position2D> second)
        {
            foreach(var item in first)
            {
                if(second.Contains(item))
                {
                    first.Remove(item); 
                }
            }

            return first;
        }

        public int CompareTo(Position2D? other)
        {
            if (X > other.X)
                return 1;
            else
            {
                if (Y > other.Y)
                    return 1;
                else
                {
                    if (X == other.X && Y == other.Y)
                        return 0;
                }
            }

            return -1;
        }
    }
}
