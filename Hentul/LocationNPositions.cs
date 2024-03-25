namespace Hentul.UT
{
    using FirstOrderMemory.Models;

    public class LocationNPositions
    {
        public List<Position_SOM>  Positions { get; private set; }
        public int X { get; private set; }
        public int Y { get; private set; }

        public LocationNPositions(List<Position_SOM> posList, int x , int y) 
        {
            Positions = posList;
            X = x;
            Y = y;
        }

        public void AddNewPostion(Position_SOM pos)
        {
            Positions.Add(pos);
        }

        public void SetCoordinates(int x, int y)
        {
            X = x;
            Y = y;
        }
    }
}
