using Common;

namespace SecondOrderMemory.Models
{
    public class SDR_SOM : SDR
    {
        public new List<Position_SOM> ActiveBits;

        public SDR_SOM(int length, int breadth, List<Position_SOM> activeBits, iType type) : base(length,breadth, type)
        {
            ActiveBits = activeBits;
        }
    }
}
