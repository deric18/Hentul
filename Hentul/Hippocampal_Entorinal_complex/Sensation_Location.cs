using Common;

namespace Hentul.Hippocampal_Entorinal_complex
{

    public class Sensation_Location
    {
        public string Location { get; set; }

        public List<Position> ActiveBits { get; set; }

        public Sensation_Location(string location, List<Position> activebits) 
        {
            Location = location;
            ActiveBits = activebits;
        }
    }
}
