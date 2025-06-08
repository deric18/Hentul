using Common;

namespace Hentul.Hippocampal_Entorinal_complex
{
    public class Sensation
    {
        public int BbbmId { get; private set; } 

        public List<Position_SOM> Positions { get; private set; }

        public Sensation()
        {
            BbbmId = 0;
            Positions = new List<Position_SOM>();
        }

        public Sensation(int bbmid, List<Position_SOM> positions)
        {
            this.BbbmId = bbmid;
            this.Positions = positions;
        }

        public bool Equals(Sensation? obj)
        {
            if(this.Positions.Except(obj.Positions).Any()) return false;

            return true;
        }

    }
}
