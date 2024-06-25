using Common;
using FirstOrderMemory.BehaviourManagers;

namespace FirstOrderMemory.Models.Encoders
{    
    public class BoolEncoder : Encoder
    {

        //Todo : need method that can completely generate brand new mappings.
        //Todo : need method to perform both Dense representations and Sparse representations.

        Dictionary<int, Position_SOM> Mappings;
        List<Position_SOM> _positions;

        public BoolEncoder(int n, int w) : base(n,w)
        {
            Mappings = new Dictionary<int, Position_SOM>()
            {
                { 0, new Position_SOM(0,1)   },
                { 1, new Position_SOM(0,1)   },
                {  2, new Position_SOM(0,1)  },
                {  3, new Position_SOM(0,1)  },
                {  4, new Position_SOM(0,1)  },
                {  5, new Position_SOM(0,1)  },
                {  6, new Position_SOM(0,1)  },
                {  7, new Position_SOM(0,1)  },
                {  8, new Position_SOM(0,1)  },
                {  9, new Position_SOM(0,1)  },
                {  10, new Position_SOM(0,1) },
                {  11, new Position_SOM(0,1) },
                {  12, new Position_SOM(0,1) },
                {  13, new Position_SOM(0,1) },
                {  14, new Position_SOM(0,1) },
                {  15, new Position_SOM(0,1) },
                {  16, new Position_SOM(0,1) },
                {  17, new Position_SOM(0,1) },
                {  18, new Position_SOM(0,1) },
                {  19, new Position_SOM(0,1) }
            };
        }

        public void SetEncoderValues(int position)
        {
            if (_positions == null)
            {
                _positions = new List<Position_SOM>();
            }

            if (_positions.Count > W)
            {
                Console.WriteLine("EXCEPTION : List of ON bits cannot exceed than W per BBM");
                Console.WriteLine(" TIP:: Call ClearEncodeValue before calling Encode");

                throw new InvalidOperationException("EXCEPTION : List of ON bits cannot exceed than W per BBM");
            }

            if(!Mappings.TryGetValue(position, out var mapping))
            {
                Console.WriteLine("Encoder() :: Invalid Mapping : Mapping Does not exist for the Encoder ! Error in Screen Grabber Logic Likely ! check for loop logic");
                throw new InvalidOperationException("Invalid Mapping : Invalid Mapping : Mapping Does not exist for the Encoder ! Error in Screen Grabber Logic Likely ! check for loop logic");
            }

            if (BBMUtils.ListContains(_positions, mapping))
            {
                Console.WriteLine("Encoder() :: Key Conflict : Cannot add the same Position Twice");
                throw new InvalidOperationException("Key Conflict: Cannot add the same Position Twice");
            }

            _positions.Add(mapping);
        }
        

        //Takes in location coordinates of the bit that was turned on and Integrates it into a Location SDR for BBM's to process.
        public SDR_SOM Encode(iType iType)
        {
            if(_positions.Count == 0)            
                Console.WriteLine("WARNING! :: BoolEncoder : Creating an empty SDR");

            double sqrt = Math.Sqrt(N);
            if (sqrt % 1 != 0.0)            
                Console.WriteLine("WARNING :: Encode :: Encoder size is not perfect Sqaure! It Should always be a perfect Square!!!");


            return new SDR_SOM((int)sqrt, (int)sqrt, _positions, iType);
        }


        public void ClearEncoderValues()
        {
            
        }
    }
}
