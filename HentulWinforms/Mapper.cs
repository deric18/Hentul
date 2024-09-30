namespace HentulWinforms
{
    using Common;
    using FirstOrderMemory.Models;
    using System.Reflection.Metadata.Ecma335;

    internal class Mapper
    {
        public int NumBBM { get; private set; }
        public int NumPixels { get; private set; }

        public int NumPixelsPerBBM { get; private set; }

        public const int LENGTH = 10;
        public const int WIDTH = 10;

        public List<int> firstbitfoms;
        public List<int> secondbitfoms;
        public List<int> doublebitfoms;

        public Dictionary<int, Position[]> Mappings { get; private set; }

        public List<Position_SOM> ONbits1;
        public List<Position_SOM> ONbits2;

        public Mapper(int numBBM, int numPixels)
        {
            if (numBBM != 50 || numPixels != 100)
            {
                throw new InvalidOperationException("Currently only upported for 10*10 Image Size with 2 Pixel per BBM W");
            }

            NumBBM = numBBM;
            NumPixels = numPixels;
            NumPixelsPerBBM = numPixels / numBBM;
            PerformMappingsFor();
            firstbitfoms = new List<int>();
            secondbitfoms = new List<int>();
            doublebitfoms = new List<int>();

            ONbits1 = new List<Position_SOM>()
            {
                new Position_SOM(2,2),
                new Position_SOM(2,8),
                new Position_SOM(5,5)
            };

            ONbits2 = new List<Position_SOM>()
            {
                new Position_SOM(5,6),
                new Position_SOM(8,8),
                new Position_SOM(8,2),
            };
        }


        private void PerformMappingsFor()
        {
            Mappings = new Dictionary<int, Position[]>
            {
                { 0, new Position[2] { new Position(0, 0), new Position(0, 1) } },
                { 1, new Position[2] { new Position(0, 2), new Position(0, 3) } },
                { 2, new Position[2] { new Position(0, 4), new Position(0, 5) } },
                { 3, new Position[2] { new Position(0, 6), new Position(0, 7) } },
                { 4, new Position[2] { new Position(0, 8), new Position(0, 9) } },
                { 5, new Position[2] { new Position(1, 0), new Position(1, 1) } },
                { 6, new Position[2] { new Position(1, 2), new Position(1, 3) } },
                { 7, new Position[2] { new Position(1, 4), new Position(1, 5) } },
                { 8, new Position[2] { new Position(1, 6), new Position(1, 7) } },
                { 9, new Position[2] { new Position(1, 8), new Position(1, 9) } },
                { 10, new Position[2] { new Position(2, 0), new Position(2, 1) } },
                { 11, new Position[2] { new Position(2, 2), new Position(2, 3) } },
                { 12, new Position[2] { new Position(2, 4), new Position(2, 5) } },
                { 13, new Position[2] { new Position(2, 6), new Position(2, 7) } },
                { 14, new Position[2] { new Position(2, 8), new Position(2, 9) } },
                { 15, new Position[2] { new Position(3, 0), new Position(3, 1) } },
                { 16, new Position[2] { new Position(3, 2), new Position(3, 3) } },
                { 17, new Position[2] { new Position(3, 4), new Position(3, 5) } },
                { 18, new Position[2] { new Position(3, 6), new Position(3, 7) } },
                { 19, new Position[2] { new Position(3, 8), new Position(3, 9) } },
                { 20, new Position[2] { new Position(4, 0), new Position(4, 1) } },
                { 21, new Position[2] { new Position(4, 2), new Position(4, 3) } },
                { 22, new Position[2] { new Position(4, 4), new Position(4, 5) } },
                { 23, new Position[2] { new Position(4, 6), new Position(4, 7) } },
                { 24, new Position[2] { new Position(4, 8), new Position(4, 9) } },
                { 25, new Position[2] { new Position(5, 0), new Position(5, 1) } },
                { 26, new Position[2] { new Position(5, 2), new Position(5, 3) } },
                { 27, new Position[2] { new Position(5, 4), new Position(5, 5) } },
                { 28, new Position[2] { new Position(5, 6), new Position(5, 7) } },
                { 29, new Position[2] { new Position(5, 8), new Position(5, 9) } },
                { 30, new Position[2] { new Position(6, 0), new Position(6, 1) } },
                { 31, new Position[2] { new Position(6, 2), new Position(6, 3) } },
                { 32, new Position[2] { new Position(6, 4), new Position(6, 5) } },
                { 33, new Position[2] { new Position(6, 6), new Position(6, 7) } },
                { 34, new Position[2] { new Position(6, 8), new Position(6, 9) } },
                { 35, new Position[2] { new Position(7, 0), new Position(7, 1) } },
                { 36, new Position[2] { new Position(7, 2), new Position(7, 3) } },
                { 37, new Position[2] { new Position(7, 4), new Position(7, 5) } },
                { 38, new Position[2] { new Position(7, 6), new Position(7, 7) } },
                { 39, new Position[2] { new Position(7, 8), new Position(7, 9) } },
                { 40, new Position[2] { new Position(8, 0), new Position(8, 1) } },
                { 41, new Position[2] { new Position(8, 2), new Position(8, 3) } },
                { 42, new Position[2] { new Position(8, 4), new Position(8, 5) } },
                { 43, new Position[2] { new Position(8, 6), new Position(8, 7) } },
                { 44, new Position[2] { new Position(8, 8), new Position(8, 9) } },
                { 45, new Position[2] { new Position(9, 0), new Position(9, 1) } },
                { 46, new Position[2] { new Position(9, 2), new Position(9, 3) } },
                { 47, new Position[2] { new Position(9, 4), new Position(9, 5) } },
                { 48, new Position[2] { new Position(9, 6), new Position(9, 7) } },
                { 49, new Position[2] { new Position(9, 8), new Position(9, 9) } }
            };

        }


        public List<Position> ParseBitmap(Bitmap bitmap)
        {
            if (bitmap.Width != LENGTH || bitmap.Height != WIDTH)
            {
                int bp = 1;

                //Todo: Fix this
            }


            List<Position> toRet = new List<Position>();

            int cacheI = 0;
            int cacheJ = 0;

            for (int i = 0; i < bitmap.Width; i++)
            {
                for (int j = 0; j < bitmap.Height; j = j + 2)
                {
                    Color color1 = bitmap.GetPixel(i, j);
                    Color color2 = bitmap.GetPixel(i, j+1);

                    if (color1.R < 200 && color1.G < 190 && color1.B < 190 && color2.R < 200 && color2.G < 190 && color2.B < 190)
                    {
                        doublebitfoms.Add(GetBBMIDFromPoisiton(i, j));
                    }
                    else if(color1.R < 200 && color1.G < 190 && color1.B < 190)
                    {
                        firstbitfoms.Add(GetBBMIDFromPoisiton(i, j));
                    }
                    else if(color2.R < 200 && color2.G < 190 && color2.B < 190)
                    {
                        secondbitfoms.Add(GetBBMIDFromPoisiton(i, j));
                    }
                    else
                    {
                        //No Fire :: Do Nothing!
                    }
                }
            }
            
            return toRet;
        }

        private int GetBBMIDFromPoisiton(int x, int y)
        {
            return ( 5 * x + (int) ( y / 2) );
        }

    }
}
