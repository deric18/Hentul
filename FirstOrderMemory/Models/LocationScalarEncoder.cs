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
        
        public Dictionary<int , int[]> Mappings { get; private set; }

        public LocationScalarEncoder(int n, int w) : base(n, w)
        {

            if( n != 100 & w != 24)
            {
                throw new ArgumentException("ScalarEncoder currently does not support any other configuration for n & w other than 100 & 8 correspondingly!");
            }

            NumBukets = n / w;
            Mappings = new Dictionary<int, int[]>();
            ComputeMappins();

        }

        private void ComputeMappins()
        {
            //Custom Mappings for Locaation Based Co-ordiantes
            Mappings.Add(0, new int[] { 1, 2, 3, 4 });            
            Mappings.Add(1, new int[] { 5, 6, 7, 8 });
            Mappings.Add(2, new int[] { 1, 2, 3, 4 });
            Mappings.Add(3, new int[] { 5, 6, 7, 8 });
            Mappings.Add(4, new int[] { 1, 2, 3, 4 });
            Mappings.Add(5, new int[] { 5, 6, 7, 8 });
            Mappings.Add(6, new int[] { 1, 2, 3, 4 });
            Mappings.Add(7, new int[] { 5, 6, 7, 8 });
            Mappings.Add(8, new int[] { 1, 2, 3, 4 });
            Mappings.Add(9, new int[] { 5, 6, 7, 8 });
        }

        public SDR_SOM Encode(int bucket)
        {
            if( 0  >  bucket && bucket >= 10)
            {
                throw new ArgumentOutOfRangeException("Encoder:: Bucket is not within range : " + bucket.ToString());
            }

            if(!Mappings.TryGetValue(bucket, out var mappings))
            {
                throw new ArgumentOutOfRangeException("Encoder:: Encoder could not find the coorect bucket, Invalid Bucket Allocation!");
            }

            List<Position_SOM> activePositons = new List<Position_SOM>();


            foreach( var mapping in mappings) 
            {
                activePositons.Add(new Position_SOM(bucket % 2 == 0 ? bucket + 1 : bucket, mapping)); 
            }
                       
            return new SDR_SOM(10, 10, activePositons, iType.TEMPORAL);
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
