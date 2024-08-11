/// Author : Deric Pinto

namespace Hentul.Hippocampal_Entorinal_complex
{
    using Common;

    public class GridCell
    {       
        public Position[] BoundaryCells { get; set; }

        public Position PlaceCell { get; private set; }

        Dictionary<string, BaseObject> Objects { get; set; }

        public GridCell(Position position) 
        { 
            Objects = new Dictionary<string, BaseObject>();
            PlaceCell = position;
            BoundaryCells = new Position[4];
        }

        public void UpdateCurrentPosition(Position position)
        {
            PlaceCell = position;
        }

        public void UpdateGridCellMap()
        {

        }

        internal void AddKnownObject()
        {

        }

        public void AddUnknownObject()
        {

        }
    }
}