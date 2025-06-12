using Common;

namespace FirstOrderMemory.Models.Encoders
{
    public class LocationEncoder
    {
        protected int N { get; set; }

        protected int W { get; set; }

        private int Buckets { get; set; }

        protected Tuple<int, int, int, int>[,] Mappings { get; set; }

        private uint LastValue { get; set; }

        private Random rand;

        public LocationEncoder(iType type, int locationMax)
        {
            //if (Math.Sqrt(n2) % 1 != 0)
            //{
            //    throw new InvalidDataException("SDR Dimension Cannot be set to " + n);
            //}

            if (type == iType.TEMPORAL)
            {
                N = 40;
                W = 8;     //10 * 4
                Buckets = N / W;
            }
            else if(type == iType.SPATIAL || type == iType.APICAL)
            {
                throw new NotImplementedException();
            }
        }

    }
}
