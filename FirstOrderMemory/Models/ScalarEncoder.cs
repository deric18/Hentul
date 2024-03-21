namespace FirstOrderMemory.Models
{
    internal class ScalarEncoder : Encoder
    {

        public int NumBukets { get; private set; }

        public ScalarEncoder(int n, int w) : base(n, w)
        {
            NumBukets = n / w;


        }

        public SDR_SOM Encode(int number)
        {

        }

    }
}
