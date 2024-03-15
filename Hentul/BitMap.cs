namespace Hentul
{
    using System;

    internal class BitMap
    {
        internal bool[,] boolArray { get; private set; }
        internal int NumberOfBuckets { get; private set; }

        internal byte[] Data { get; private set; }


        internal BitMap(int numbBuckets, bool[,] arr)
        {
            boolArray = arr;
            NumberOfBuckets = numbBuckets;
            Data = new byte[NumberOfBuckets];

            PopulateDataBytes(arr);
        }

        private void PopulateDataBytes(bool[,] barr)
        {
            string toReturn = String.Empty;

            if (barr.GetUpperBound(2) != 8)
            {
                throw new InvalidOperationException("Bool Array Value should always be equal to byte Size for conversion, Check you SHitty Code! DumbFuck!!!!");
            }

            for (int i = 0; i < barr.GetUpperBound(1); i++)
            {
                for (int j = 0; j < barr.GetUpperBound(2); j++)
                {                    
                    if (barr[i,j])
                    {
                        toReturn.Append('1');
                    }
                    else
                    {
                        toReturn.Append('0');
                    }

                }

                Data[i] = Byte.Parse(toReturn);
                toReturn = string.Empty;
            }
        }

    }
}
