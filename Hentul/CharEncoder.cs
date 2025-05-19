namespace Hentul
{
    using Common;

    /// <summary>
    /// An Encoder typically takes in human readable input like a pixel value or character in this case and converts it into a machine readable input format like SDR
    /// In this Encoder it typically reads in the input and intializes the below FOMBBMIDs & SOMBBMIDs objects based on the mappings.
    /// A Character encoder takes in a single character which is 2 bytes.
    /// 1 bit -> 25 cells
    /// 16 bits -> 400 cells , 1 BBM -> 100 cells , So 2 bytes -> 4 FOM BBMs
    /// FOM -> 4 BBMS of 10 * 10 * 4
    /// SOM -> 1 BBM of size 400 * 10 * 4
    /// </summary>
    public class CharEncoder
    {         
        public int NumBBMPerChar { get; private set; }        

        public Dictionary<int, Position[]> Mappings { get; private set; }
        
        public Dictionary<MAPPERCASE, List<int>> FOMBBMIDS { get; private set; }

        public Dictionary<MAPPERCASE, List<int>> SOMBBMIDS { get; private set; }



        public CharEncoder()
        {
            NumBBMPerChar = 4;
            Mappings = new();
            FOMBBMIDS = new();
            SOMBBMIDS = new();
        }

        public SDR_SOM Encode(char ch)
        {
            SDR_SOM sdr = new SDR_SOM(10, 10, new List<Position_SOM>());

            return sdr;
        }
    }
}
