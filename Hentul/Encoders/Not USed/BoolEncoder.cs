namespace Hentul
{   //Incomplete
    using Common;
    using FirstOrderMemory.BehaviourManagers;
    using FirstOrderMemory.Models;

    public class BoolEncoder : Encoder
    {

        //Todo : need method that can completely generate brand new mappings.
        //Todo : need method to perform both Dense representations and Sparse representations.
        //Todo : Add more UTs

        Dictionary<string, Position_SOM> Mappings;
        List<Position_SOM> _positions;

        // Hard Coded for 20 bool pixel values
        public BoolEncoder(int n, int w) : base(n,w)
        {
            _positions = new List<Position_SOM>();
            Mappings = new Dictionary<string, Position_SOM>()
            {
                { "0-0", new Position_SOM  (1,1) },
                { "0-1", new Position_SOM  (0,3) },
                { "0-2", new Position_SOM  (1,5) },
                { "0-3", new Position_SOM  (0,8) },
                { "0-4", new Position_SOM  (3,1) },
                { "0-5", new Position_SOM  (2,3) },
                { "0-6", new Position_SOM  (3,5) },
                { "0-7", new Position_SOM  (2,8) },
                { "0-8", new Position_SOM  (5,1) },
                { "0-9", new Position_SOM  (4,3) },
                { "1-0", new Position_SOM  (5,5) },
                { "1-1", new Position_SOM  (4,8) },
                { "1-2", new Position_SOM  (7,1) },
                { "1-3", new Position_SOM  (6,3) },
                { "1-4", new Position_SOM  (7,5) },
                { "1-5", new Position_SOM  (6,8) },
                { "1-6", new Position_SOM  (9,1) },
                { "1-7", new Position_SOM  (8,3) },
                { "1-8", new Position_SOM  (9,5) },
                { "1-9", new Position_SOM  (8,8) }
            };
        }

        //One parse can process 20 pixels in one shot
        public void SetEncoderValues(string position)
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

        public bool HasValues() => (_positions.Count != 0); 

        //Takes in location coordinates of the bit that was turned on and Integrates it into a Location SDR for BBM's to process.
        public SDR_SOM Encode(iType iType)
        {
            if(_positions.Count == 0)            
                Console.WriteLine("WARNING! :: BoolEncoder : Creating an empty SDR");

            double sqrt = Math.Sqrt(N);

            if (sqrt % 1 != 0.0)
            {
                Console.WriteLine("WARNING :: Encode :: Encoder size is not perfect Sqaure! It Should always be a perfect Square!!!");
                throw new InvalidOperationException();
            }


            return new SDR_SOM((int)sqrt, (int)sqrt, _positions, iType);
        }


        public void ClearEncoderValues()
        {
            _positions.Clear();
        }
    }
}
