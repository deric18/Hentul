using System.Security.Cryptography;

namespace FirstOrderMemory.Models.Encoders
{
    public class BoolEncoder : Encoder
    {

        //Todo : need method that can completely generate brand new mappings.
        //Todo : need method to perform both Dense representations and Sparse representations.

        List<Position_SOM> _positions;

        public BoolEncoder(int n, int w) : base(n,w)
        {

        }

        public void SetEncoderValues( )
        {

        }
        

        //Takes in location coordinates of the bit that was turned on and Integrates it into a Location SDR for BBM's to process.
        public void Encode(int x, int y)
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


            

        }


        public void ClearEncoderValues()
        {
            
        }
    }
}
