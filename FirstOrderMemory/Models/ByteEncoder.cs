namespace FirstOrderMemory.Models
{
    public class ByteEncoder
    {
        private int N { get; set; }

        private int W { get; set; }

        private int fileSize { get; set; }

        private int Buckets { get; set; }

        private Tuple<int, int, int, int>[,] Mappings { get; set; }

        private uint LastValue { get; set; }

        private Random rand;

        private List<Position> ActiveBits { get; set; }

        public ByteEncoder(int n, int w)
        {
           
            if (Math.Sqrt(n) % 1 != 0)
            {
                throw new InvalidDataException("SDR Dimension Cannot be set to " + n);
            }
            fileSize = (int)Math.Sqrt(n);
            Buckets = w / n;
            N = n;
            W = w;
            Buckets = N / W;
            Mappings = new Tuple<int, int, int, int>[n, n];
            rand = new Random();
            ActiveBits = new List<Position>();

            PerformMappings();
        }

        private void PerformMappings()
        {
            for (int i = 0; i < N; i++)
            {
                for (int j = 0; j < N; j++)
                {
                    Mappings[i, j] = new Tuple<int, int, int, int>(rand.Next(0, 99), rand.Next(0, 99), rand.Next(0, 99), rand.Next(0, 99));
                }
            }
        }

        public void Encode(byte R, byte G, byte B)
        {
            if(N != 100)
            {
                throw new InvalidDataException("Cannot Initiate SDR with lower than 100 N for the given byte count to encode");
            }

            List<byte> byteList = new List<byte>()
            {
                R, G, B
            };

            int offset = fileSize / 3;
            int iterator = 1;
            int x = 0, y = 0;
            bool bit = false;

            foreach (Byte b in byteList)
            {
                for(int i = 0; i < 8; i++) 
                {
                    bit = (b & (1 << b - 1)) != 0;

                    x = (iterator * offset);

                    y = i;

                    ActiveBits.Add(new Position(x, y));
                }

                iterator++;
            }

        }

        public SDR GetDenseSDR()
        {
            return new SDR(fileSize, fileSize, ActiveBits);
        }
    }
}
