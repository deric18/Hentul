using Common;

namespace Hentul.Hippocampal_Entorinal_complex
{
    public class Sensation
    {
        public int BbbmId { get; private set; } 

        public List<Position2D> Positions { get; private set; }

        public Sensation()
        {
            BbbmId = 0;
            Positions = new List<Position2D>();
        }

        public Sensation(int bbmid, List<Position2D> positions)
        {
            this.BbbmId = bbmid;
            this.Positions = positions;
        }

    }
}
