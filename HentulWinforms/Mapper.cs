namespace HentulWinforms
{
    using Common;
    using FirstOrderMemory.Models;

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
                throw new InvalidOperationException("Currently only supported for 10*10 Image Size with 2 Pixel per BBM W");
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
            Mappings = new Dictionary<int, Position[]> {
                                {   0   , new Position[4] { new Position (  0   ,   0   ), new Position( 0   ,   1   ), new Position(    1   ,   0   ), new Position(    1   ,   1   ) } },
                {   1   , new Position[4] { new Position (  0   ,   2   ), new Position( 0   ,   3   ), new Position(    1   ,   2   ), new Position(    1   ,   3   ) } },
                {   2   , new Position[4] { new Position (  0   ,   4   ), new Position( 0   ,   5   ), new Position(    1   ,   4   ), new Position(    1   ,   5   ) } },
                {   3   , new Position[4] { new Position (  0   ,   6   ), new Position( 0   ,   7   ), new Position(    1   ,   6   ), new Position(    1   ,   7   ) } },
                {   4   , new Position[4] { new Position (  0   ,   8   ), new Position( 0   ,   9   ), new Position(    1   ,   8   ), new Position(    1   ,   9   ) } },
                {   5   , new Position[4] { new Position (  2   ,   10  ), new Position( 2   ,   11  ), new Position(    3   ,   10  ), new Position(    3   ,   11  ) } },
                {   6   , new Position[4] { new Position (  2   ,   12  ), new Position( 2   ,   13  ), new Position(    3   ,   12  ), new Position(    3   ,   13  ) } },
                {   7   , new Position[4] { new Position (  2   ,   14  ), new Position( 2   ,   15  ), new Position(    3   ,   14  ), new Position(    3   ,   5   ) } },
                {   8   , new Position[4] { new Position (  2   ,   16  ), new Position( 2   ,   17  ), new Position(    3   ,   16  ), new Position(    3   ,   7   ) } },
                {   9   , new Position[4] { new Position (  2   ,   18  ), new Position( 2   ,   19  ), new Position(    3   ,   18  ), new Position(    3   ,   9   ) } },
                {   10  , new Position[4] { new Position (  4   ,   0   ), new Position( 4   ,   1   ), new Position(    5       0   ), new Position(    5   ,   1   ) } },
                {   11  , new Position[4] { new Position (  4   ,   2   ), new Position( 4   ,   3   ), new Position(    5   ,   2   ), new Position(    5   ,   3   ) } },
                {   12  , new Position[4] { new Position (  4   ,   4   ), new Position( 4   ,   5   ), new Position(    5   ,   4   ), new Position(    5   ,   5   ) } },
                {   13  , new Position[4] { new Position (  4   ,   6   ), new Position( 4   ,   7   ), new Position(    5   ,   6   ), new Position(    5   ,   7   ) } },
                {   14  , new Position[4] { new Position (  4   ,   8   ), new Position( 4   ,   9   ), new Position(    5   ,   8   ), new Position(    5   ,   9   ) } },
                {   15  , new Position[4] { new Position (  6   ,   10  ), new Position( 6   ,   11  ), new Position(    7   ,   10  ), new Position(    7   ,   1   ) } },
                {   16  , new Position[4] { new Position (  6   ,   12  ), new Position( 6   ,   13  ), new Position(    7   ,   12  ), new Position(    7   ,   3   ) } },
                {   17  , new Position[4] { new Position (  6   ,   14  ), new Position( 6   ,   15  ), new Position(    7   ,   14  ), new Position(    7   ,   5   ) } },
                {   18  , new Position[4] { new Position (  6   ,   16  ), new Position( 6   ,   17  ), new Position(    7   ,   16  ), new Position(    7   ,   7   ) } },
                {   19  , new Position[4] { new Position (  6   ,   18  ), new Position( 6   ,   19  ), new Position(    7   ,   18  ), new Position(    7   ,   9   ) } },
                {   20  , new Position[4] { new Position (  8   ,   0   ), new Position( 8   ,   1   ), new Position(    9   ,   0   ), new Position(    9   ,   1   ) } },
                {   21  , new Position[4] { new Position (  8   ,   2   ), new Position( 8   ,   3   ), new Position(    9   ,   2   ), new Position(    9   ,   3   ) } },
                {   22  , new Position[4] { new Position (  8   ,   4   ), new Position( 8   ,   5   ), new Position(    9   ,   4   ), new Position(    9   ,   5   ) } },
                {   23  , new Position[4] { new Position (  8   ,   6   ), new Position( 8   ,   7   ), new Position(    9   ,   6   ), new Position(    9   ,   7   ) } },
                {   24  , new Position[4] { new Position (  8   ,   8   ), new Position( 8   ,   9   ), new Position(    9   ,   8   ), new Position(    9   ,   9   ) } },
                {   25  , new Position[4] { new Position (  10  ,   10  ), new Position( 10  ,   11  ), new Position(    11  ,   10  ), new Position(    11  ,   1   ) } },
                {   26  , new Position[4] { new Position (  10  ,   12  ), new Position( 10  ,   13  ), new Position(    11  ,   12  ), new Position(    11  ,   3   ) } },
                {   27  , new Position[4] { new Position (  10  ,   14  ), new Position( 10  ,   15  ), new Position(    11  ,   14  ), new Position(    11  ,   5   ) } },
                {   28  , new Position[4] { new Position (  10  ,   16  ), new Position( 10  ,   17  ), new Position(    11  ,   16  ), new Position(    11  ,   7   ) } },
                {   29  , new Position[4] { new Position (  10  ,   18  ), new Position( 10  ,   19  ), new Position(    11  ,   18  ), new Position(    11  ,   9   ) } },
                {   30  , new Position[4] { new Position (  12  ,   0   ), new Position( 12  ,   1   ), new Position(    13  ,   0   ), new Position(    13  ,   1   ) } },
                {   31  , new Position[4] { new Position (  12  ,   2   ), new Position( 12  ,   3   ), new Position(    13  ,   2   ), new Position(    13  ,   3   ) } },
                {   32  , new Position[4] { new Position (  12  ,   4   ), new Position( 12  ,   5   ), new Position(    13  ,   4   ), new Position(    13  ,   5   ) } },
                {   33  , new Position[4] { new Position (  12  ,   6   ), new Position( 12  ,   7   ), new Position(    13  ,   6   ), new Position(    13  ,   7   ) } },
                {   34  , new Position[4] { new Position (  12  ,   8   ), new Position( 12  ,   9   ), new Position(    13  ,   8   ), new Position(    13  ,   9   ) } },
                {   35  , new Position[4] { new Position (  14  ,   10  ), new Position( 14  ,   11  ), new Position(    15  ,   10  ), new Position(    15  ,   1   ) } },
                {   36  , new Position[4] { new Position (  14  ,   12  ), new Position( 14  ,   13  ), new Position(    15  ,   12  ), new Position(    15  ,   3   ) } },
                {   37  , new Position[4] { new Position (  14  ,   14  ), new Position( 14  ,   15  ), new Position(    15  ,   14  ), new Position(    15  ,   5   ) } },
                {   38  , new Position[4] { new Position (  14  ,   16  ), new Position( 14  ,   17  ), new Position(    15  ,   16  ), new Position(    15  ,   7   ) } },
                {   39  , new Position[4] { new Position (  14  ,   18  ), new Position( 14  ,   19  ), new Position(    15  ,   18  ), new Position(    15  ,   9   ) } },
                {   40  , new Position[4] { new Position (  16  ,   0   ), new Position( 16  ,   1   ), new Position(    17  ,   0   ), new Position(    17  ,   1   ) } },
                {   41  , new Position[4] { new Position (  16  ,   2   ), new Position( 16  ,   3   ), new Position(    17  ,   2   ), new Position(    17  ,   3   ) } },
                {   42  , new Position[4] { new Position (  16  ,   4   ), new Position( 16  ,   5   ), new Position(    17  ,   4   ), new Position(    17  ,   5   ) } },
                {   43  , new Position[4] { new Position (  16  ,   6   ), new Position( 16  ,   7   ), new Position(    17  ,   6   ), new Position(    17  ,   7   ) } },
                {   44  , new Position[4] { new Position (  16  ,   8   ), new Position( 16  ,   9   ), new Position(    17  ,   8   ), new Position(    17  ,   9   ) } },
                {   45  , new Position[4] { new Position (  18  ,   10  ), new Position( 18  ,   11  ), new Position(    19  ,   10  ), new Position(    19  ,   1   ) } },
                {   46  , new Position[4] { new Position (  18  ,   12  ), new Position( 18  ,   13  ), new Position(    19  ,   12  ), new Position(    19  ,   3   ) } },
                {   47  , new Position[4] { new Position (  18  ,   14  ), new Position( 18  ,   15  ), new Position(    19  ,   14  ), new Position(    19  ,   5   ) } },
                {   48  , new Position[4] { new Position (  18  ,   16  ), new Position( 18  ,   17  ), new Position(    19  ,   16  ), new Position(    19  ,   7   ) } },
                {   49  , new Position[4] { new Position (  18  ,   18  ), new Position( 18  ,   19  ), new Position(    19  ,   18  ), new Position(    19  ,   9   ) } },
                {   50  , new Position[4] { new Position (  20  ,   0   ), new Position( 20  ,   1   ), new Position(    21  ,   0   ), new Position(    21  ,   1   ) } },
                {   51  , new Position[4] { new Position (  20  ,   2   ), new Position( 20  ,   3   ), new Position(    21  ,   2   ), new Position(    21  ,   3   ) } },
                {   52  , new Position[4] { new Position (  20  ,   4   ), new Position( 20  ,   5   ), new Position(    21  ,   4   ), new Position(    21  ,   5   ) } },
                {   53  , new Position[4] { new Position (  20  ,   6   ), new Position( 20  ,   7   ), new Position(    21  ,   6   ), new Position(    21  ,   7   ) } },
                {   54  , new Position[4] { new Position (  20  ,   8   ), new Position( 20  ,   9   ), new Position(    21  ,   8   ), new Position(    21  ,   9   ) } },
                {   55  , new Position[4] { new Position (  22  ,   10  ), new Position( 22  ,   11  ), new Position(    23  ,   10  ), new Position(    23  ,   1   ) } },
                {   56  , new Position[4] { new Position (  22  ,   12  ), new Position( 22  ,   13  ), new Position(    23  ,   12  ), new Position(    23  ,   3   ) } },
                {   57  , new Position[4] { new Position (  22  ,   14  ), new Position( 22  ,   15  ), new Position(    23  ,   14  ), new Position(    23  ,   5   ) } },
                {   58  , new Position[4] { new Position (  22  ,   16  ), new Position( 22  ,   17  ), new Position(    23  ,   16  ), new Position(    23  ,   7   ) } },
                {   59  , new Position[4] { new Position (  22  ,   18  ), new Position( 22  ,   19  ), new Position(    23  ,   18  ), new Position(    23  ,   9   ) } },
                {   60  , new Position[4] { new Position (  24  ,   0   ), new Position( 24  ,   1   ), new Position(    25  ,   0   ), new Position(    25  ,   1   ) } },
                {   61  , new Position[4] { new Position (  24  ,   2   ), new Position( 24  ,   3   ), new Position(    25  ,   2   ), new Position(    25  ,   3   ) } },
                {   62  , new Position[4] { new Position (  24  ,   4   ), new Position( 24  ,   5   ), new Position(    25  ,   4   ), new Position(    25  ,   5   ) } },
                {   63  , new Position[4] { new Position (  24  ,   6   ), new Position( 24  ,   7   ), new Position(    25  ,   6   ), new Position(    25  ,   7   ) } },
                {   64  , new Position[4] { new Position (  24  ,   8   ), new Position( 24  ,   9   ), new Position(    25  ,   8   ), new Position(    25  ,   9   ) } },
                {   65  , new Position[4] { new Position (  26  ,   10  ), new Position( 26  ,   11  ), new Position(    27  ,   10  ), new Position(    27  ,   1   ) } },
                {   66  , new Position[4] { new Position (  26  ,   12  ), new Position( 26  ,   13  ), new Position(    27  ,   12  ), new Position(    27  ,   3   ) } },
                {   67  , new Position[4] { new Position (  26  ,   14  ), new Position( 26  ,   15  ), new Position(    27  ,   14  ), new Position(    27  ,   5   ) } },
                {   68  , new Position[4] { new Position (  26  ,   16  ), new Position( 26  ,   17  ), new Position(    27  ,   16  ), new Position(    27  ,   7   ) } },
                {   69  , new Position[4] { new Position (  26  ,   18  ), new Position( 26  ,   19  ), new Position(    27  ,   18  ), new Position(    27  ,   9   ) } },
                {   70  , new Position[4] { new Position (  28  ,   0   ), new Position( 28  ,   1   ), new Position(    30  ,   0   ), new Position(    30  ,   1   ) } },
                {   71  , new Position[4] { new Position (  28  ,   2   ), new Position( 28  ,   3   ), new Position(    30  ,   2   ), new Position(    30  ,   3   ) } },
                {   72  , new Position[4] { new Position (  28  ,   4   ), new Position( 28  ,   5   ), new Position(    30  ,   4   ), new Position(    30  ,   5   ) } },
                {   73  , new Position[4] { new Position (  28  ,   6   ), new Position( 28  ,   7   ), new Position(    30  ,   6   ), new Position(    30  ,   7   ) } },
                {   74  , new Position[4] { new Position (  28  ,   8   ), new Position( 28  ,   9   ), new Position(    30  ,   8   ), new Position(    30  ,   9   ) } },
                {   75  , new Position[4] { new Position (  30  ,   10  ), new Position( 30  ,   11  ), new Position(    31  ,   10  ), new Position(    31  ,   1   ) } },
                {   76  , new Position[4] { new Position (  30  ,   12  ), new Position( 30  ,   13  ), new Position(    31  ,   12  ), new Position(    31  ,   3   ) } },
                {   77  , new Position[4] { new Position (  30  ,   14  ), new Position( 30  ,   15  ), new Position(    31  ,   14  ), new Position(    31  ,   5   ) } },
                {   78  , new Position[4] { new Position (  30  ,   16  ), new Position( 30  ,   17  ), new Position(    31  ,   16  ), new Position(    31  ,   7   ) } },
                {   79  , new Position[4] { new Position (  30  ,   18  ), new Position( 30  ,   19  ), new Position(    31  ,   18  ), new Position(    31  ,   9   ) } },
                {   80  , new Position[4] { new Position (  32  ,   0   ), new Position( 32  ,   1   ), new Position(    33  ,   0   ), new Position(    33  ,   1   ) } },
                {   81  , new Position[4] { new Position (  32  ,   2   ), new Position( 32  ,   3   ), new Position(    33  ,   2   ), new Position(    33  ,   3   ) } },
                {   82  , new Position[4] { new Position (  32  ,   4   ), new Position( 32  ,   5   ), new Position(    33  ,   4   ), new Position(    33  ,   5   ) } },
                {   83  , new Position[4] { new Position (  32  ,   6   ), new Position( 32  ,   7   ), new Position(    33  ,   6   ), new Position(    33  ,   7   ) } },
                {   84  , new Position[4] { new Position (  32  ,   8   ), new Position( 32  ,   9   ), new Position(    33  ,   8   ), new Position(    33  ,   9   ) } },
                {   85  , new Position[4] { new Position (  34  ,   10  ), new Position( 34  ,   11  ), new Position(    35  ,   10  ), new Position(    35  ,   1   ) } },
                {   86  , new Position[4] { new Position (  34  ,   12  ), new Position( 34  ,   13  ), new Position(    35  ,   12  ), new Position(    35  ,   3   ) } },
                {   87  , new Position[4] { new Position (  34  ,   14  ), new Position( 34  ,   15  ), new Position(    35  ,   14  ), new Position(    35  ,   5   ) } },
                {   88  , new Position[4] { new Position (  34  ,   16  ), new Position( 34  ,   17  ), new Position(    35  ,   16  ), new Position(    35  ,   7   ) } },
                {   89  , new Position[4] { new Position (  34  ,   18  ), new Position( 34  ,   19  ), new Position(    35  ,   18  ), new Position(    35  ,   9   ) } },
                {   90  , new Position[4] { new Position (  36  ,   0   ), new Position( 36  ,   1   ), new Position(    37  ,   0   ), new Position(    37  ,   1   ) } },
                {   91  , new Position[4] { new Position (  36  ,   2   ), new Position( 36  ,   3   ), new Position(    37  ,   2   ), new Position(    37  ,   3   ) } },
                {   92  , new Position[4] { new Position (  36  ,   4   ), new Position( 36  ,   5   ), new Position(    37  ,   4   ), new Position(    37  ,   5   ) } },
                {   93  , new Position[4] { new Position (  36  ,   6   ), new Position( 36  ,   7   ), new Position(    37  ,   6   ), new Position(    37  ,   7   ) } },
                {   94  , new Position[4] { new Position (  36  ,   8   ), new Position( 36  ,   9   ), new Position(    37  ,   8   ), new Position(    37  ,   9   ) } },
                {   95  , new Position[4] { new Position (  38  ,   10  ), new Position( 38  ,   11  ), new Position(    39  ,   10  ), new Position(    39  ,   1   ) } },
                {   96  , new Position[4] { new Position (  38  ,   12  ), new Position( 38  ,   13  ), new Position(    39  ,   12  ), new Position(    39  ,   3   ) } },
                {   97  , new Position[4] { new Position (  38  ,   14  ), new Position( 38  ,   15  ), new Position(    39  ,   14  ), new Position(    39  ,   5   ) } },
                {   98  , new Position[4] { new Position (  38  ,   16  ), new Position( 38  ,   17  ), new Position(    39  ,   16  ), new Position(    39  ,   7   ) } },
                {   99  , new Position[4] { new Position (  38  ,   18  ), new Position( 38  ,   19  ), new Position(    39  ,   18  ), new Position(    39  ,   9   ) } }
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

            foreach (var kvp in Mappings)
            {
                var bbmID = kvp.Key;
                var posList = kvp.Value;

                Color color1 = bitmap.GetPixel(posList[0].X, posList[0].Y);
                Color color2 = bitmap.GetPixel(posList[1].X, posList[1].Y);
                Color color3 = bitmap.GetPixel(posList[2].X, posList[2].Y);
                Color color4 = bitmap.GetPixel(posList[3].X, posList[3].Y);

                bool check1 = CheckIfColorIsBlack(color1);
                bool check2 = CheckIfColorIsBlack(color2);
                bool check3 = CheckIfColorIsBlack(color3);
                bool check4 = CheckIfColorIsBlack(color4);

                if ( check1 && check2 && check3 && check4 )                                 //4's
                {
                    
                }
                else if (check1 && check2 && check3 && check4 == false)
                {
                    
                }
                else if (check1 && check2 && check4 && check3 == false)
                {
                    
                }
                else if (check1 && check3 && check4 && check2 == false)
                {

                }
                else if (check2 && check3 && check4 && check1 == false)                     //3's
                {

                }
                else if (check1 && check2 && check3 == false && check4 == false)
                {

                }
                else if (check1 && check3 && check2 == false && check4 == false)
                {

                }
                else if (check4 && check3 && check2 == false && check1 == false)
                {

                }
                else if (check4 && check1 && check2 == false && check3 == false)
                {

                }
                else if (check4 && check2 && check3 == false && check1 == false)
                {

                }
                else if (check2 && check3 && check4 == false && check1 == false)            //2's
                {

                }
                else if (check1 && check2 == false && check3 == false && check4 == false)    
                {

                }
                else if (check2 && check1 == false && check3 == false && check4 == false)    
                {

                }
                else if (check3 && check1 == false && check2 == false && check4 == false)    
                {

                }
                else if (check4 && check1 == false && check2 == false && check3 == false)    //1's
                {

                }
                else
                {
                    //No Fire :: Do Nothing!
                }
            }
        }
        
        private bool CheckIfColorIsBlack(Color color)
            => (color.R < 200 && color.G < 190 && color.B < 190);
    }
}
