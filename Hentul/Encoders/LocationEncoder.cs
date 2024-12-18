namespace Hentul
{

    using System.Collections.Generic;
    using Common;
    using FirstOrderMemory.Models;


    /// <summary>
    /// Will Represent 2 bits x & y in one SDR , Total 40 bits available  , 32 for X and 18 for Y
    /// if there is no input for the lcoations , there will be no active bit set for it , meaing no output for zero.
    /// Becasue of the situation here i need 11 bits to represetn a 
    /// 2^0 = 1
    /// 2^1 = 2
    /// 2^2 = 4
    /// 2^4 = 16
    /// 2^5 = 32
    /// 2^6 = 64
    /// 2^7 = 128
    /// 2^8 = 256
    /// 2^9 = 712
    /// 2^10 = 1424
    /// 
    /// Total = 2639
    /// </summary>
    public class LocationEncoder
    {
        protected int N_X { get; set; }

        protected int N_Y { get; set; }

        protected int W_X { get; set; }

        protected int W_Y { get; set; }

        private int Buckets_X { get; set; }

        private int Buckets_Y { get; set; }

        private int Num_Bits_Per_Digit_X { get; set; }

        private int Num_Bits_Per_Digit_Y { get; set; }

        private int[] Locations_X { get; set; }
        private int[] Locations_Y { get; set; }

        private Dictionary<int , List<Position_SOM>> Mappings_X { get ; set; }

        private Dictionary<int, List<Position_SOM>> Mappings_Y { get; set; }

        private int TotalBitsUsed { get; set; }

        private uint LastValue_X { get; set; }

        private uint LastValue_Y { get; set; }

        private int maxValX { get; set; }

        private int maxValY { get; set; }

        private Random rand;

        public LocationEncoder(iType type)
        {
            if (type == iType.SPATIAL || type == iType.APICAL)
            {
                throw new NotImplementedException();
            }

            N_X = 32;
            W_X = 11;                //11 * 3
            N_Y = 18;
            W_Y = 9;
            Buckets_X = 12;         // My Screen Coordinates usual doesnt ggo above 3000
            Buckets_Y = 9;
            Num_Bits_Per_Digit_X = 3;
            Num_Bits_Per_Digit_Y = 2;
            Locations_X = new int[32];
            Locations_Y = new int[18];
            TotalBitsUsed = Num_Bits_Per_Digit_X * Locations_X.Length + Num_Bits_Per_Digit_Y * Locations_Y.Length;
            maxValX = 2639;
            maxValY = 1215;

            Mappings_X = new Dictionary<int, List<Position_SOM>>()
            {
                {0 , new List<Position_SOM>() { new Position_SOM(8,3) } },
                {1 , new List<Position_SOM>() { new Position_SOM(8,2) } },
                {2 , new List<Position_SOM>() { new Position_SOM(5,2) } },
                {3 , new List<Position_SOM>() { new Position_SOM(2,2) } },
                {4 , new List<Position_SOM>() { new Position_SOM(0,1) } },
                {5 , new List<Position_SOM>() { new Position_SOM(3,1) } },
                {6 , new List<Position_SOM>() { new Position_SOM(6,1) } },
                {7 , new List<Position_SOM>() { new Position_SOM(9,1) } },
                {8 , new List<Position_SOM>() { new Position_SOM(7,0) } },
                {9 , new List<Position_SOM>() { new Position_SOM(4,0) } },
                {10 , new List<Position_SOM>() { new Position_SOM(1,0) } }                
            };

            Mappings_Y = new Dictionary<int, List<Position_SOM>>()
            {
                {0 , new List<Position_SOM>() { new Position_SOM(6,3) } },
                {1 , new List<Position_SOM>() { new Position_SOM(4,3) } },
                {2 , new List<Position_SOM>() { new Position_SOM(2,3) } },
                {3 , new List<Position_SOM>() { new Position_SOM(0,3) } },
                {4 , new List<Position_SOM>() { new Position_SOM(8,4) } },
                {5 , new List<Position_SOM>() { new Position_SOM(6,4) } },
                {6 , new List<Position_SOM>() { new Position_SOM(4,4) } },
                {7 , new List<Position_SOM>() { new Position_SOM(2,4) } },
                {8 , new List<Position_SOM>() { new Position_SOM(0,4) } }                
            };
        }

        public List<Position_SOM> Encode(int numberX, int numberY)
        {
            if(numberX > maxValX || numberY > maxValY)
            {
                throw new InvalidOperationException("Sadly ! I currently only Support corresponding ranges for X : " + maxValX.ToString() +" &Y: " + maxValY.ToString());
            }

            List<Position_SOM> activeBits = new List<Position_SOM>();     

            int copynumber = numberX;            
            int tens = 10;
            int offset = 3;
            string binary = Convert.ToString(numberX, 2);

            for(int index = 0; index < binary.Length; index++)
            {
                if(binary[index] == '1')
                {
                    if(Mappings_X.TryGetValue(index, out var value))
                        activeBits.AddRange(value);
                }
            }

            copynumber = numberY;
            binary = Convert.ToString(numberY, 2);

            for (int index = 0; index < binary.Length; index++)
            {
                if (binary[index] == '1')
                {
                    if (Mappings_Y.TryGetValue(index, out var value))
                        activeBits.AddRange(value);
                }
            }


            return activeBits;
        }
    }
}
