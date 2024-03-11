namespace FirstOrderMemory.Models
{    
    internal class Encoder
    {
        private int N { get; set; }

        private int W { get; set; }

        private int Buckets { get; set; }        

        private Tuple<int,int,int,int>[,] Mappings { get; set; } 

        private uint LastValue { get; set; }       

        private Random rand;
        
        public Encoder(int n, int w) 
        {
            if(Math.Sqrt(n)  % 1 != 0)
            {
                throw new InvalidDataException("SDR Dimension Cannot be set to " + n);
            }            
            N = n;
            W = w;
            Buckets = N / W;
            Mappings = new Tuple<int, int, int, int>[n, n];
            rand = new Random();
            PerformMappings();
        }               

        private void PerformMappings()
        {            
            for(int i=0;i<N;i++)
            {
                for(int j=0; j< N;j++)
                {
                    Mappings[i, j] = new Tuple<int, int, int, int>(rand.Next(0, 99), rand.Next(0, 99), rand.Next(0, 99), rand.Next(0, 99));
                }
            }
        }
    }
}
