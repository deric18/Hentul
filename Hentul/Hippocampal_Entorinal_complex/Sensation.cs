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

        /// <summary>
        /// Return true  if both sensations are same else false.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public bool CheckForRepetition(Sensation obj)
        {
            if (Positions.Count == obj.Positions.Count)
            {
                int count = 0;

                foreach (var item in obj.Positions)
                {
                    if (Positions.Contains(item) == false)
                        break;

                    count++;
                }

                if (count == obj.Positions.Count)
                {
                    return true;
                }
            }
                        
            return false;
        }
    }
}