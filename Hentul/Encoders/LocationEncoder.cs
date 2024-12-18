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



            N_X = 22;
            W_X = 11;                //11 * 2
            N_Y = 18;
            W_Y = 9;
            Buckets_X = 11;         // My Screen Coordinates usual doesnt ggo above 3000
            Num_Bits_Per_Digit_X = 2;
            Num_Bits_Per_Digit_Y = 2;
            Locations_X = new int[22];
            Locations_Y = new int[18];
            TotalBitsUsed = Num_Bits_Per_Digit_X * Locations_X.Length + Num_Bits_Per_Digit_Y * Locations_Y.Length;
            maxValX = 2639;
            maxValY = 1215;

            Mappings_X = new Dictionary<int, List<Position_SOM>>()
            {
                {0 , new List<Position_SOM>() { new Position_SOM(0, 0) } },
                {1 , new List<Position_SOM>() { new Position_SOM(0, 0) } },
                {2 , new List<Position_SOM>() { new Position_SOM(0, 0) } },
                {3 , new List<Position_SOM>() { new Position_SOM(0, 0) } },
                {4 , new List<Position_SOM>() { new Position_SOM(0, 0) } },
                {5 , new List<Position_SOM>() { new Position_SOM(0, 0) } },
                {6 , new List<Position_SOM>() { new Position_SOM(0, 0) } },
                {7 , new List<Position_SOM>() { new Position_SOM(0, 0) } },
                {8 , new List<Position_SOM>() { new Position_SOM(0, 0) } },
                {9 , new List<Position_SOM>() { new Position_SOM(0, 0) } },
                {10 , new List<Position_SOM>() { new Position_SOM(0, 0) } }                
            };

            Mappings_X = new Dictionary<int, List<Position_SOM>>()
            {
                {1 , new List<Position_SOM>() { new Position_SOM(0, 0) } },
                {2 , new List<Position_SOM>() { new Position_SOM(0, 0) } },
                {3 , new List<Position_SOM>() { new Position_SOM(0, 0) } },
                {4 , new List<Position_SOM>() { new Position_SOM(0, 0) } },
                {5 , new List<Position_SOM>() { new Position_SOM(0, 0) } },
                {6 , new List<Position_SOM>() { new Position_SOM(0, 0) } },
                {7 , new List<Position_SOM>() { new Position_SOM(0, 0) } },
                {8 , new List<Position_SOM>() { new Position_SOM(0, 0) } },
                {9 , new List<Position_SOM>() { new Position_SOM(0, 0) } },
                {9 , new List<Position_SOM>() { new Position_SOM(0, 0) } },
            };

        }

        public SDR_SOM Encode(int numberX, int numberY)
        {
            if(numberX > maxValX || numberY > maxValY)
            {
                throw new InvalidOperationException("Sadly ! I currently only Support corresponding ranges for X : " + maxValX.ToString() +" &Y: " + maxValY.ToString());
            }

            List<Position_SOM> activeBits = new List<Position_SOM>();
            SDR_SOM temporalSDR = new SDR_SOM(10, 4, activeBits, iType.TEMPORAL);

            int copynumber = numberX;
            int index = 0;
            int tens = 10;
            int offset = 3;

            while (copynumber > 0)
            {
                int lastNumber = copynumber % tens;





            }

            return temporalSDR;
        }



    }
}
