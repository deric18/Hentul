namespace Common
{
    public class Position2D
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

    }
}
