namespace Hentul
{
    public class Encoder
    {
        protected int N { get; set; }

        protected int W { get; set; }

        private int Buckets { get; set; }

        protected Tuple<int, int, int, int>[,] Mappings { get; set; }

        private uint LastValue { get; set; }

        private Random rand;

        public Encoder(int n, int w)
        {
            if (Math.Sqrt(n) % 1 != 0)
            {
                throw new InvalidDataException("SDR Dimension Cannot be set to " + n);
            }

            N = n;
            W = w;
            Buckets = N / W;            
        }        
    }
}
