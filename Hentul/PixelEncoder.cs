namespace Hentul
{
    using Common;
    using FirstOrderMemory.Models;
    using Hentul.Hippocampal_Entorinal_complex;
    using System.Collections.Generic;
    using System.Drawing;
    using static FirstOrderMemory.BehaviourManagers.BlockBehaviourManagerFOM;

    /// <summary>
    /// Maps all the Bits to there respective BBMIDs.
    /// </summary>
    public class PixelEncoder
    {
        public int NumBBM { get; private set; }

        public int NumPixels { get; private set; }

        public int NumPixelsPerBBM { get; private set; }

        public int Xoffset { get; private set; }

        public int YOffset { get; private set; }

        public Dictionary<int, Position[]> Mappings { get; private set; }

        /// <summary>
        /// Per BBM of size 10 * 10 , the BBM gets split into 4 parts for 4 pixels , one part for each pixel 
        /// pixel 1 : X : 0 - 5 , Y : 0 - 5
        /// pixel 2 : X : 5 - 10, Y : 0 - 5
        /// pixel 3 : X : 5 - 10, Y : 5 - 10
        /// pixel 4 : X : 0 - 5, Y : 5 - 10
        /// </summary>

        List<Position_SOM> ONbits1FOM;
        List<Position_SOM> ONbits2FOM;
        List<Position_SOM> ONbits3FOM;
        List<Position_SOM> ONbits4FOM;


        public Dictionary<MAPPERCASE, List<int>> FOMBBMIDS { get; private set; }

        public Dictionary<MAPPERCASE, List<int>> SOMBBMIDS { get; private set; }

        public List<Position_SOM> somPositions;

        public bool[,] testBmpCoverage { get; private set; }

        public int LENGTH { get; private set; }

        public int WIDTH { get; private set; }

        public PixelEncoder(int numBBM, int numPixels)
        {
            if (numBBM != 100 || numPixels != 400)
            {
                throw new InvalidOperationException("Currently only supported for 10*10 Image Size with 2 Pixel per BBM W");
            }

            NumBBM = numBBM;
            NumPixels = numPixels;
            NumPixelsPerBBM = numPixels / numBBM;

            PerformMappings();

            ONbits1FOM = new List<Position_SOM>()
            {
                new Position_SOM(0, 1),
                new Position_SOM(2, 2)
            };

            ONbits2FOM = new List<Position_SOM>()
            {
                new Position_SOM(6, 0),
                new Position_SOM(8, 4)
            };

            ONbits3FOM = new List<Position_SOM>()
            {
                new Position_SOM(5, 6),
                new Position_SOM(8, 9)
            };

            ONbits4FOM = new List<Position_SOM>()
            {
                new Position_SOM(7, 1),
                new Position_SOM(9, 2)
            };

            somPositions = new List<Position_SOM>();

            FOMBBMIDS = new Dictionary<MAPPERCASE, List<int>>();
            SOMBBMIDS = new Dictionary<MAPPERCASE, List<int>>();

            Xoffset = -1;
            YOffset = -1;
        }


        /// <summary>
        /// <BBM ID , Locations of Pixels for BBMID >
        /// Image  Size : X : 20 rows Y : 20 Columns
        /// Number Of Pixels : 400 pixels 
        /// BBM : 100
        /// NumPixelPerBBM : 4
        /// Row ID could be more than 20 but the total number of rows processed will be 20 , this is a misnomer that confused me a while back.
        /// </summary>
        private void PerformMappings()
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
                {   10  , new Position[4] { new Position (  4   ,   0   ), new Position( 4   ,   1   ), new Position(    5   ,   0   ), new Position(    5   ,   1   ) } },
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


        public SDR_SOM GetSDR_SOMForMapperCase(MAPPERCASE mappercase, int bbmID)
        {
            var positionstoAdd = new List<Position_SOM>();

            switch (mappercase)
            {
                case MAPPERCASE.ALL:
                    {
                        positionstoAdd.AddRange(ONbits1FOM);
                        positionstoAdd.AddRange(ONbits2FOM);
                        positionstoAdd.AddRange(ONbits3FOM);
                        positionstoAdd.AddRange(ONbits4FOM);

                        somPositions.AddRange(GetSOMEquivalentPositionsofFOM(ONbits1FOM, bbmID));
                        somPositions.AddRange(GetSOMEquivalentPositionsofFOM(ONbits2FOM, bbmID));
                        somPositions.AddRange(GetSOMEquivalentPositionsofFOM(ONbits3FOM, bbmID));
                        somPositions.AddRange(GetSOMEquivalentPositionsofFOM(ONbits4FOM, bbmID));

                    }
                    break;
                case MAPPERCASE.ONETWOTHREEE:
                    {
                        positionstoAdd.AddRange(ONbits1FOM);
                        positionstoAdd.AddRange(ONbits2FOM);
                        positionstoAdd.AddRange(ONbits3FOM);

                        somPositions.AddRange(GetSOMEquivalentPositionsofFOM(ONbits1FOM, bbmID));
                        somPositions.AddRange(GetSOMEquivalentPositionsofFOM(ONbits2FOM, bbmID));
                        somPositions.AddRange(GetSOMEquivalentPositionsofFOM(ONbits3FOM, bbmID));

                    }
                    break;
                case MAPPERCASE.TWOTHREEFOUR:
                    {

                        positionstoAdd.AddRange(ONbits2FOM);
                        positionstoAdd.AddRange(ONbits3FOM);
                        positionstoAdd.AddRange(ONbits4FOM);

                        somPositions.AddRange(GetSOMEquivalentPositionsofFOM(ONbits2FOM, bbmID));
                        somPositions.AddRange(GetSOMEquivalentPositionsofFOM(ONbits3FOM, bbmID));
                        somPositions.AddRange(GetSOMEquivalentPositionsofFOM(ONbits4FOM, bbmID));

                    }
                    break;
                case MAPPERCASE.ONETWOFOUR:
                    {

                        positionstoAdd.AddRange(ONbits1FOM);
                        positionstoAdd.AddRange(ONbits2FOM);
                        positionstoAdd.AddRange(ONbits4FOM);

                        somPositions.AddRange(GetSOMEquivalentPositionsofFOM(ONbits1FOM, bbmID));
                        somPositions.AddRange(GetSOMEquivalentPositionsofFOM(ONbits2FOM, bbmID));
                        somPositions.AddRange(GetSOMEquivalentPositionsofFOM(ONbits4FOM, bbmID));

                    }
                    break;
                case MAPPERCASE.ONETHREEFOUR:
                    {

                        positionstoAdd.AddRange(ONbits1FOM);
                        positionstoAdd.AddRange(ONbits3FOM);
                        positionstoAdd.AddRange(ONbits4FOM);

                        somPositions.AddRange(GetSOMEquivalentPositionsofFOM(ONbits1FOM, bbmID));
                        somPositions.AddRange(GetSOMEquivalentPositionsofFOM(ONbits3FOM, bbmID));
                        somPositions.AddRange(GetSOMEquivalentPositionsofFOM(ONbits4FOM, bbmID));

                    }
                    break;
                case MAPPERCASE.ONETWO:
                    {

                        positionstoAdd.AddRange(ONbits1FOM);
                        positionstoAdd.AddRange(ONbits2FOM);

                        somPositions.AddRange(GetSOMEquivalentPositionsofFOM(ONbits1FOM, bbmID));
                        somPositions.AddRange(GetSOMEquivalentPositionsofFOM(ONbits2FOM, bbmID));

                    }
                    break;
                case MAPPERCASE.ONETHREE:
                    {

                        positionstoAdd.AddRange(ONbits1FOM);
                        positionstoAdd.AddRange(ONbits3FOM);

                        somPositions.AddRange(GetSOMEquivalentPositionsofFOM(ONbits1FOM, bbmID));
                        somPositions.AddRange(GetSOMEquivalentPositionsofFOM(ONbits3FOM, bbmID));

                    }
                    break;
                case MAPPERCASE.ONEFOUR:
                    {

                        positionstoAdd.AddRange(ONbits1FOM);
                        positionstoAdd.AddRange(ONbits4FOM);

                        somPositions.AddRange(GetSOMEquivalentPositionsofFOM(ONbits1FOM, bbmID));
                        somPositions.AddRange(GetSOMEquivalentPositionsofFOM(ONbits4FOM, bbmID));

                    }
                    break;
                case MAPPERCASE.TWOTHREE:
                    {

                        positionstoAdd.AddRange(ONbits2FOM);
                        positionstoAdd.AddRange(ONbits3FOM);

                        somPositions.AddRange(GetSOMEquivalentPositionsofFOM(ONbits2FOM, bbmID));
                        somPositions.AddRange(GetSOMEquivalentPositionsofFOM(ONbits3FOM, bbmID));

                    }
                    break;
                case MAPPERCASE.TWOFOUR:
                    {

                        positionstoAdd.AddRange(ONbits2FOM);
                        positionstoAdd.AddRange(ONbits4FOM);

                        somPositions.AddRange(GetSOMEquivalentPositionsofFOM(ONbits2FOM, bbmID));
                        somPositions.AddRange(GetSOMEquivalentPositionsofFOM(ONbits4FOM, bbmID));

                    }
                    break;
                case MAPPERCASE.THREEFOUR:
                    {

                        positionstoAdd.AddRange(ONbits3FOM);
                        positionstoAdd.AddRange(ONbits4FOM);

                        somPositions.AddRange(GetSOMEquivalentPositionsofFOM(ONbits3FOM, bbmID));
                        somPositions.AddRange(GetSOMEquivalentPositionsofFOM(ONbits4FOM, bbmID));

                    }
                    break;
                case MAPPERCASE.ONE:
                    {

                        positionstoAdd.AddRange(ONbits1FOM);

                        somPositions.AddRange(GetSOMEquivalentPositionsofFOM(ONbits1FOM, bbmID));


                    }
                    break;
                case MAPPERCASE.TWO:
                    {

                        positionstoAdd.AddRange(ONbits2FOM);

                        somPositions.AddRange(GetSOMEquivalentPositionsofFOM(ONbits2FOM, bbmID));

                    }
                    break;
                case MAPPERCASE.THREE:
                    {

                        positionstoAdd.AddRange(ONbits3FOM);

                        somPositions.AddRange(GetSOMEquivalentPositionsofFOM(ONbits3FOM, bbmID));

                    }
                    break;
                case MAPPERCASE.FOUR:
                    {
                        positionstoAdd.AddRange(ONbits4FOM);
                        somPositions.AddRange(GetSOMEquivalentPositionsofFOM(ONbits4FOM, bbmID));
                    }
                    break;
                default:
                    {
                        throw new NotImplementedException("No Valid Mapper Case Found for Layer Type");
                    }
            }

            return new SDR_SOM(10, 10, positionstoAdd, iType.SPATIAL);
        }

        internal static List<Position_SOM> GetSOMEquivalentPositionsofFOM(List<Position_SOM> oNbitsFOM, int bbmID)
        {
            List<Position_SOM> retList = new List<Position_SOM>();
            Position_SOM newPosition;

            foreach (var pos in oNbitsFOM)
            {
                newPosition = new Position_SOM(pos.X + 10 * bbmID, pos.Y);
                retList.Add(newPosition);
            }

            return retList;
        }

        internal static List<Position_SOM> GetFOMEquivalentPositionsofSOM(List<Position_SOM> oNbitsFOM, int bbmID)
        {
            List<Position_SOM> retList = new List<Position_SOM>();
            Position_SOM newPosition;

            foreach (var pos in oNbitsFOM)
            {
                newPosition = new Position_SOM(pos.X + 10 * bbmID, pos.Y);
                retList.Add(newPosition);
            }

            return retList;
        }

        internal static Dictionary<int, List<Position_SOM>> GetFOMEquivalentPositionsofSOM(List<Position_SOM> somBits)
        {
            if (somBits.Count == 0)
                return null;
            
            Dictionary<int, List<Position_SOM>> retDict = new Dictionary<int, List<Position_SOM>>();

            foreach (var pos in somBits)
            {
                int bbmID = pos.X / 10;

                if (bbmID > 120 || bbmID < 0)
                {
                    //throw new InvalidOperationException("BBM ID cannot exceed more than 99 for this system!");

                    continue;
                }

                Position_SOM newPos = null;

                if (pos.X < 10)
                    newPos = new Position_SOM(pos.X, pos.Y, pos.Z);
                else
                {
                    newPos = new Position_SOM(pos.X % 10, pos.Y, pos.Z); 
                }

                if(newPos.X >= 10)
                {
                    int breakpoint = 10;
                }

                if (retDict.TryGetValue(bbmID, out var posList))
                {
                    posList.Add(newPos);
                }
                else
                { 
                    retDict.Add(bbmID, new List<Position_SOM>() { newPos });
                }
            }

            return retDict;
        }

        public void ParseBitmap(Bitmap bitmap)
        {
            if (bitmap.Width != 40 || bitmap.Height != 20)
            {
                throw new InvalidDataException("Invalid Data Dimensions!");
            }

            List<Position> toRet = new List<Position>();

            testBmpCoverage = new bool[bitmap.Width, bitmap.Height];

            //Iterating over these mappings will cover the incoming bmp of dimensions 20 * 20 [400 pixels in total].

            foreach (var kvp in Mappings)
            {
                var bbmID = kvp.Key;
                var posList = kvp.Value;

                var pixel1 = posList[0];
                var pixel2 = posList[1];
                var pixel3 = posList[2];
                var pixel4 = posList[3];

                Color color1 = bitmap.GetPixel(pixel1.X, pixel1.Y);
                testBmpCoverage[pixel1.X, pixel1.Y] = true;

                Color color2 = bitmap.GetPixel(pixel2.X, pixel2.Y);
                testBmpCoverage[pixel2.X, pixel2.Y] = true;

                Color color3 = bitmap.GetPixel(pixel3.X, pixel3.Y);
                testBmpCoverage[pixel3.X, pixel3.Y] = true;

                Color color4 = bitmap.GetPixel(pixel4.X, pixel4.Y);
                testBmpCoverage[pixel4.X, pixel4.Y] = true;

                bool check1 = CheckIfColorIsWhite(color1);
                bool check2 = CheckIfColorIsWhite(color2);
                bool check3 = CheckIfColorIsWhite(color3);
                bool check4 = CheckIfColorIsWhite(color4);

                if (check1 && check2 && check3 && check4)
                {
                    CheckNInsert(FOMBBMIDS, bbmID, MAPPERCASE.ALL);
                    CheckNInsert(SOMBBMIDS, bbmID, MAPPERCASE.ALL);
                }
                else if (check1 && check2 && check3 && check4 == false)
                {
                    if (bbmID == 79)
                    {
                        bool breakpoint = true;
                    }

                    CheckNInsert(FOMBBMIDS, bbmID, MAPPERCASE.ONETWOTHREEE);
                    CheckNInsert(SOMBBMIDS, bbmID, MAPPERCASE.ONETWOTHREEE);
                }
                else if (check1 && check2 && check4 && check3 == false)
                {
                    CheckNInsert(FOMBBMIDS, bbmID, MAPPERCASE.ONETWOFOUR);
                    CheckNInsert(SOMBBMIDS, bbmID, MAPPERCASE.ONETWOFOUR);
                }
                else if (check1 && check3 && check4 && check2 == false)
                {
                    CheckNInsert(FOMBBMIDS, bbmID, MAPPERCASE.ONETHREEFOUR);

                    CheckNInsert(SOMBBMIDS, bbmID, MAPPERCASE.ONETHREEFOUR);
                }
                else if (check2 && check3 && check4 && check1 == false)                     //3's
                {
                    CheckNInsert(FOMBBMIDS, bbmID, MAPPERCASE.TWOTHREEFOUR);

                    CheckNInsert(SOMBBMIDS, bbmID, MAPPERCASE.TWOTHREEFOUR);
                }
                else if (check1 && check2 && check3 == false && check4 == false)
                {
                    CheckNInsert(FOMBBMIDS, bbmID, MAPPERCASE.ONETWO);

                    CheckNInsert(SOMBBMIDS, bbmID, MAPPERCASE.ONETWO);
                }
                else if (check1 && check3 && check2 == false && check4 == false)
                {
                    CheckNInsert(FOMBBMIDS, bbmID, MAPPERCASE.ONETHREE);

                    CheckNInsert(SOMBBMIDS, bbmID, MAPPERCASE.ONETHREE);
                }
                else if (check4 && check3 && check2 == false && check1 == false)
                {
                    CheckNInsert(FOMBBMIDS, bbmID, MAPPERCASE.THREEFOUR);

                    CheckNInsert(SOMBBMIDS, bbmID, MAPPERCASE.THREEFOUR);
                }
                else if (check4 && check1 && check2 == false && check3 == false)
                {
                    CheckNInsert(FOMBBMIDS, bbmID, MAPPERCASE.ONEFOUR);

                    CheckNInsert(SOMBBMIDS, bbmID, MAPPERCASE.ONEFOUR);
                }
                else if (check4 && check2 && check3 == false && check1 == false)
                {
                    CheckNInsert(FOMBBMIDS, bbmID, MAPPERCASE.TWOFOUR);

                    CheckNInsert(SOMBBMIDS, bbmID, MAPPERCASE.TWOFOUR);
                }
                else if (check2 && check3 && check4 == false && check1 == false)            //2's
                {
                    CheckNInsert(FOMBBMIDS, bbmID, MAPPERCASE.TWOTHREE);

                    CheckNInsert(SOMBBMIDS, bbmID, MAPPERCASE.TWOTHREE);
                }
                else if (check1 && check2 == false && check3 == false && check4 == false)
                {
                    CheckNInsert(FOMBBMIDS, bbmID, MAPPERCASE.ONE);

                    CheckNInsert(SOMBBMIDS, bbmID, MAPPERCASE.ONE);
                }
                else if (check2 && check1 == false && check3 == false && check4 == false)
                {
                    CheckNInsert(FOMBBMIDS, bbmID, MAPPERCASE.TWO);

                    CheckNInsert(SOMBBMIDS, bbmID, MAPPERCASE.TWO);
                }
                else if (check3 && check1 == false && check2 == false && check4 == false)
                {
                    CheckNInsert(FOMBBMIDS, bbmID, MAPPERCASE.THREE);

                    CheckNInsert(SOMBBMIDS, bbmID, MAPPERCASE.THREE);
                }
                else if (check4 && check1 == false && check2 == false && check3 == false)    //1's
                {
                    CheckNInsert(FOMBBMIDS, bbmID, MAPPERCASE.FOUR);

                    CheckNInsert(SOMBBMIDS, bbmID, MAPPERCASE.FOUR);
                }
                else
                {
                    //No Fire :: Do Nothing!
                }
            }
        }

        private void CheckNInsert(Dictionary<MAPPERCASE, List<int>> dict, int bbmID, MAPPERCASE mapperCase)
        {
            if (dict == null)
                return;

            List<int> intlist;

            if (dict.TryGetValue(mapperCase, out intlist))
            {
                if (!intlist.Contains(bbmID))
                {
                    intlist.Add(bbmID);
                }
            }
            else
            {
                intlist = new List<int>() { bbmID };
                dict.Add(mapperCase, intlist);
            }
        }

        private bool CheckIfColorIsWhite(Color color)
            => (color.R > 240 && color.G > 240 && color.B > 240);

        private bool CheckIfColorIsBlack(Color color)
            => (color.R < 10 && color.G < 10 && color.B < 10);

        public void clean()
        {
            FOMBBMIDS.Clear();
            SOMBBMIDS.Clear();
            somPositions.Clear();
            Xoffset = -1;
            YOffset = -1;
        }


        /// <summary>
        /// Parses through all the active Bits and constructs a dictionary of <BBMID, List<Position_SOM>>  
        /// </summary>
        /// <param name="sdr_SOM">Incoming SDR for SOM Layer</param>
        /// <returns> A Sensaei that represents the felt Sensation at the location represented by the cursor.</returns>
        public Sensation_Location GetSensationLocationFromSDR(SDR_SOM sdr_SOM, Orchestrator.POINT point)
        {
            if (sdr_SOM.Length < 1000)
            {
                int exception = 1;
                throw new InvalidDataException("SDR SOM is empty for Layer 3B or Invalid SDR Size!!!");
            }

            Sensation_Location sensation_Location = new Sensation_Location(new SortedDictionary<string, KeyValuePair<int, List<Position2D>>>(), new Position2D(point.X, point.Y));

            if (sdr_SOM.ActiveBits.Count == 0)
                return sensation_Location;

            int iterator = 0;

            Position2D position = null;
            KeyValuePair<int, List<Position2D>> keyValuePair = new KeyValuePair<int, List<Position2D>>();


            foreach (var pos in sdr_SOM.ActiveBits)
            {
                int bbmID = pos.X / 10;

                if (bbmID > 99 || bbmID < 0)
                {
                    // BUG : need to figgure out why SOM can have 1000 active bit
                    //throw new InvalidOperationException("BBM ID cannot exceed more than 99 for this system!");                    
                    continue;
                }

                position = GetPositionForActiveBit(point, pos.X);                

                //position = GetPositionForBBMID(bbmID, point);

                if (sensation_Location.sensLoc.TryGetValue(position?.ToString(), out KeyValuePair<int, List<Position2D>> kvp))
                {
                    kvp.Value.Add( new Position2D(pos.X, pos.Y));
                }
                else
                {                    
                    KeyValuePair<int, List<Position2D>> sensation = new KeyValuePair<int, List<Position2D>>(
                        bbmID,
                        new List<Position2D>() { new Position2D(pos.X, pos.Y) });

                    sensation_Location.AddNewSensationAtThisLocation(position.ToString(), sensation);
                }
            }

            return sensation_Location;
        }


        public Position GetPositionForBBMID(int bbmID, Orchestrator.POINT point)
        {
            Position toReturn = null;

            if (Mappings.TryGetValue(bbmID, out var positions))
            {
                int LocationOffset = positions[0].X > 20 ? positions[0].Y * -1 : positions[0].Y;

                toReturn = new Position(point.X + LocationOffset, point.Y + LocationOffset);
                //Generating Unique Location String for Sensation_Location Object. BUG : Different location are getting stored for FOM / SOM and different is being sent to HC_EC Complex while prediciton
            }

            return toReturn;
        }

        public Position2D GetPositionForActiveBit(Orchestrator.POINT point, int posX)
        {
            Position2D toReturn = null;
            Random random = new Random();
            int bbmID = posX / 10;

            if (Mappings.TryGetValue(bbmID, out var positions))
            {
                int LocationOffset = positions[0].X > 20 ? positions[0].Y * -1 : positions[0].Y;

                toReturn = new Position2D(point.X + LocationOffset, point.Y + LocationOffset);
                //Generating Unique Location String for Sensation_Location Object. BUG : Different location are getting stored for FOM / SOM and different is being sent to HC_EC Complex while prediciton
            }

            return toReturn;
        }
    }

    public enum MAPPERCASE
    {
        ALL,
        ONETWOTHREEE,
        TWOTHREEFOUR,
        ONETWOFOUR,
        ONETHREEFOUR,
        ONETWO,
        ONETHREE,
        ONEFOUR,
        TWOTHREE,
        TWOFOUR,
        THREEFOUR,
        ONE,
        TWO,
        THREE,
        FOUR

    }
}
