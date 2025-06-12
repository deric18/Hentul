using Common;

namespace Hentul.Hippocampal_Entorinal_complex
{
    public class RFrame
    {        
        public double[,] DisplacementTable {  get; private set; }

        public Position2D LTBoundary { get; private set; }
        public Position2D RTBoundary { get; private set; }
        public Position2D LBBoundary { get; private set; }
        public Position2D RBBoundary { get; private set; }


        /// <summary>
        /// Should be zero when the whole image fits inside 400 pixels, and should be icnremented accordingly based o nthe size with reference to 400 pixels
        /// </summary>
        public int offsetScale { get; private set; }

        public RFrame(List<Sensation_Location> senselocList)
        {
            DisplacementTable = new double[senselocList.Count, senselocList.Count];
            offsetScale = 0;
            Init(senselocList);
            ComputeScale();
            ComputeBoundaries();
        }

        public bool Init(List<Sensation_Location> senselocList)
        {
            if (senselocList == null || senselocList.Count == 0)
            {
                return false;
            }

            bool sucess = false;            

            for(int i = 0; i < senselocList.Count; i++)
            {
                //Iterate though every sensation and compute distance between center Position of every sensation ith every other sensation in the list.
                for(int j = 0; j < senselocList.Count; j++)
                {
                    DisplacementTable[i, j] = Math.Sqrt( Math.Pow( Math.Abs(senselocList[j].CenterPosition.X - senselocList[i].CenterPosition.X), 2) + Math.Pow( Math.Abs(senselocList[j].CenterPosition.Y - senselocList[i].CenterPosition.Y), 2));
                }
            }            

            return sucess;
        }

        private void ComputeScale()
        {

        }

        private void ComputeBoundaries()
        {

        }
    }
}
