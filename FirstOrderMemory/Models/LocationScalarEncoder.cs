using Common;

namespace FirstOrderMemory.Models
{
    public class LocationScalarEncoder : Encoder
    {
        /// <summary>
        /// There are 2 co-ordinates X & Y , each coordinate has fixed range of 0 - 999 and each coordinate gets 50 bits each to represent it.
        /// X gets 3 numbers 0 - 9 and each number has 8 pixels which will be mapped to 16 pixels and so on for all the numbers.
        /// </summary>
        public int NumBukets { get; private set; }            
        
        public Dictionary<int , List<int>> Mappings { get; private set; }

        public LocationScalarEncoder(int n, int w) : base(n, w)
        {

            if( n != 100 & w != 24)
            {
                throw new ArgumentException("ScalarEncoder currently does not support any other configuration for n & w other than 100 & 8 correspondingly!");
            }

            NumBukets = n / w;
            Mappings = new Dictionary<int, List<int>>();
            ComputeMappins();

        }

        private void ComputeMappins()
        {
            //Custom Mappings for Locaation Based Co-ordiantes

            Mappings.Add(0, new List<int>() { 0, 1 });
            Mappings.Add(0, new List<int>() { 0, 1 });


        }

        public SDR_SOM Encode(int x, int y)
        {
            if( ( x < 0 || x > 999 ) && ( y < 0 || y > 99 ))
            {
                throw new ArgumentOutOfRangeException("x & y is not within the range : X : " + x.ToString() + " Y : " + y.ToString());
            }

            


            return new SDR_SOM(10, 10, new List<Position_SOM> { }, iType.TEMPORAL);
        }
        
        private List<Position_SOM> ExtractAndAddPositions(int number)
        {
            List<Position_SOM> Positions = new List<Position_SOM>();

            int numbercopy = number;
            int tens = 0;
            int iteration = 0;


            while (numbercopy > 0)
            {
                tens = numbercopy % 10;



                numbercopy = numbercopy / 10;

                iteration++;
            }


            return Positions;
        }

    }
}
