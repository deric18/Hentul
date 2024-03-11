namespace FirstOrderMemory.Models
{
    using Common;


    // 4+4+4 = 12
    public class ByteEncoder
    {
        private int N { get; set; }

        private int W { get; set; }

        private int num_blocks_per_partition { get; set; }

        private int num_bits_per_bool { get; set; }

        private bool grouping_by_block { get; set; }

        private Dictionary<int, int[]> Mappings { get; set; }

        private List<Position_SOM> ActiveBits;

        private uint LastValue { get; set; }

        private Random rand;

        public ByteEncoder(int n, int w)
        {
            if (n != 100)
            {
                throw new InvalidDataException("Cannot Initiate SDR with lower than 100 N for the given byte count to encode");
            }
            if (Math.Sqrt(n) % 1 != 0)
            {
                throw new InvalidDataException("SDR Dimension Cannot be set to " + n);
            }
            N = n;
            W = w;
            num_blocks_per_partition = N / W;
            num_bits_per_bool = 4;
            grouping_by_block = false;
            ActiveBits = new List<Position_SOM>();
            Mappings = new Dictionary<int, int[]>();
            rand = new Random();
            ComputeMappings();
        }

        private void ComputeMappings()
        {
            if (N != 100)
            {
                throw new InvalidDataException("Cannot Initiate SDR with lower than 100 N for the given byte count to encode");
            }
            if (grouping_by_block)
            {
                throw new NotImplementedException();
            }
            else
            {
                Mappings.Add(0, new int[] { 4, 5, 6, 9 });
                Mappings.Add(1, new int[] { 6, 7, 8, 9 });
                Mappings.Add(2, new int[] { 4, 5, 6, 7 });
                Mappings.Add(3, new int[] { 6, 7, 8, 9 });

                Mappings.Add(4, new int[] { 4, 5, 6, 7 });
                Mappings.Add(5, new int[] { 6, 7, 8, 9 });
                Mappings.Add(6, new int[] { 4, 5, 6, 7 });
                Mappings.Add(7, new int[] { 6, 7, 8, 9 });
            }
        }

        public void Encode(byte b)
        {
            LastValue = b;
            ActiveBits.Clear();

            if (N != 100)
            {
                throw new InvalidDataException("Cannot Initiate SDR with lower than 100 N for the given byte count to encode");
            }

            bool bit = false;
            //Sparse Encoding
            for (int index = 0; index < 8; index++)
            {
                bit = (b & (1 << index)) == (1 << index);

                if (bit)
                {
                    SetValuesForBit(index);
                }

            }
            //DenseEncoding
        }

        //Gets Called for only ON Bits for the specific indexes.
        private void SetValuesForBit(int partition)
        {
            if (Mappings.TryGetValue(partition, out int[] arr))
            {
                foreach (var item in arr)
                {
                    ActiveBits.Add(new Position_SOM(partition, item));
                }
            }
        }

        public SDR_SOM GetDenseSDR(iType iType = iType.SPATIAL)
        {
            return new SDR_SOM(10, 10, ActiveBits, iType);
        }


        public SDR_SOM GetSparseSDR()
        {
            return new SDR_SOM(10, 10, ActiveBits, iType.SPATIAL);
        }
    }
}