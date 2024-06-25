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
            _positions = new List<Position_SOM>();
            Mappings = new Dictionary<int, Position_SOM>()
            {
                { 0,  new Position_SOM  (1,1)   },
                { 1,  new Position_SOM  (0,3)   },
                {  2, new Position_SOM  (1,5)  },
                {  3, new Position_SOM  (0,8)  },
                {  4, new Position_SOM  (3,1)  },
                {  5, new Position_SOM  (2,3)  },
                {  6, new Position_SOM  (3,5)  },
                {  7, new Position_SOM  (2,8)  },
                {  8, new Position_SOM  (5,1)  },
                {  9, new Position_SOM  (4,3)  },
                {  10, new Position_SOM (5,5) },
                {  11, new Position_SOM (4,8) },
                {  12, new Position_SOM (7,1) },
                {  13, new Position_SOM (6,3) },
                {  14, new Position_SOM (7,5) },
                {  15, new Position_SOM (6,8) },
                {  16, new Position_SOM (9,1) },
                {  17, new Position_SOM (8,3) },
                {  18, new Position_SOM (9,5) },
                {  19, new Position_SOM (8,8) }
            };
        }

        public void SetEncoderValues(int position)
        {            
            if (_positions.Count > W)
            {
                Console.WriteLine("EXCEPTION : List of ON bits cannot exceed than W per BBM");
                Console.WriteLine(" TIP:: Call ClearEncodeValue before calling Encode");

                throw new InvalidOperationException("EXCEPTION : List of ON bits cannot exceed than W per BBM");
            }

            if(!Mappings.TryGetValue(position, out var mapping))
            {
                Console.WriteLine("Encoder() :: Invalid Mapping : Mapping Does not exist for Number for : " + position + " !");
                Console.WriteLine("Error in Screen Grabber Logic Likely ! check for loop logic ");
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
