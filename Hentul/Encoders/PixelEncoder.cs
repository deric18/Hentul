namespace Hentul.Encoders
{
    using Common;
    using FirstOrderMemory.Models;
    using Hentul;
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
        public Dictionary<int, Position[]> Mappings { get; private set; }



        public Dictionary<MAPPERCASE, List<int>> FOMBBMIDS { get; private set; }


        public List<Position_SOM> SomPositions { get; private set; }

        public bool[,] testBmpCoverage { get; private set; }

        public int LENGTH { get; private set; }

        public int WIDTH { get; private set; }

        public PixelEncoder(int numBBM, int numPixels)
        {
            Mappings = new Dictionary<int, Position[]>();
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

            SomPositions = new List<Position_SOM>();

            FOMBBMIDS = new Dictionary<MAPPERCASE, List<int>>();

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

            bool ignorecheckforDuplicates = false;

            // Helper to process each ONbitsXFOM list
            void ProcessFOM(List<Position_SOM> fomList)
            {
                positionstoAdd.AddRange(fomList);
                var items = GetSOMEquivalentPositionsofFOM(fomList, bbmID);

                if(ignorecheckforDuplicates)
                    CheckForDuplicates(items);

                SomPositions.AddRange(items);
            }

            switch (mappercase)
            {
                case MAPPERCASE.ALL:
                    ProcessFOM(ONbits1FOM);
                    ProcessFOM(ONbits2FOM);
                    ProcessFOM(ONbits3FOM);
                    ProcessFOM(ONbits4FOM);
                    break;
                case MAPPERCASE.ONETWOTHREEE:
                    ProcessFOM(ONbits1FOM);
                    ProcessFOM(ONbits2FOM);
                    ProcessFOM(ONbits3FOM);
                    break;
                case MAPPERCASE.TWOTHREEFOUR:
                    ProcessFOM(ONbits2FOM);
                    ProcessFOM(ONbits3FOM);
                    ProcessFOM(ONbits4FOM);
                    break;
                case MAPPERCASE.ONETWOFOUR:
                    ProcessFOM(ONbits1FOM);
                    ProcessFOM(ONbits2FOM);
                    ProcessFOM(ONbits4FOM);
                    break;
                case MAPPERCASE.ONETHREEFOUR:
                    ProcessFOM(ONbits1FOM);
                    ProcessFOM(ONbits3FOM);
                    ProcessFOM(ONbits4FOM);
                    break;
                case MAPPERCASE.ONETWO:
                    ProcessFOM(ONbits1FOM);
                    ProcessFOM(ONbits2FOM);
                    break;
                case MAPPERCASE.ONETHREE:
                    ProcessFOM(ONbits1FOM);
                    ProcessFOM(ONbits3FOM);
                    break;
                case MAPPERCASE.ONEFOUR:
                    ProcessFOM(ONbits1FOM);
                    ProcessFOM(ONbits4FOM);
                    break;
                case MAPPERCASE.TWOTHREE:
                    ProcessFOM(ONbits2FOM);
                    ProcessFOM(ONbits3FOM);
                    break;
                case MAPPERCASE.TWOFOUR:
                    ProcessFOM(ONbits2FOM);
                    ProcessFOM(ONbits4FOM);
                    break;
                case MAPPERCASE.THREEFOUR:
                    ProcessFOM(ONbits3FOM);
                    ProcessFOM(ONbits4FOM);
                    break;
                case MAPPERCASE.ONE:
                    ProcessFOM(ONbits1FOM);
                    break;
                case MAPPERCASE.TWO:
                    ProcessFOM(ONbits2FOM);
                    break;
                case MAPPERCASE.THREE:
                    ProcessFOM(ONbits3FOM);
                    break;
                case MAPPERCASE.FOUR:
                    ProcessFOM(ONbits4FOM);
                    break;
                default:
                    throw new NotImplementedException("No Valid Mapper Case Found for Layer Type");
            }

            return new SDR_SOM(10, 10, positionstoAdd, iType.SPATIAL);
        }

        private void CheckForDuplicates(List<Position_SOM> poses)
        {
            foreach (var pos in poses)
            {
                foreach (var existing in SomPositions)
                {
                    // Use Position_SOM.Equals for comparison
                    if (pos.Equals(existing))
                    {
                        // Duplicate found; handle as needed
                        int breakpoint = 1;
                        // Optionally, you could log, throw, or return here
                    }
                }
            }
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

                if (newPos.X >= 10)
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

        
        public void ParseBitmap(Bitmap bitmap, bool[,] validMask = null)
        {
            if (bitmap == null) throw new ArgumentNullException(nameof(bitmap));
            if (bitmap.Width != 40 || bitmap.Height != 20)
                throw new InvalidDataException($"Encoder expects 40x20; got {bitmap.Width}x{bitmap.Height}");

            int width = bitmap.Width;   // 40
            int height = bitmap.Height; // 20
            testBmpCoverage = new bool[width, height];

            foreach (var kvp in Mappings)
            {
                int bbmID = kvp.Key;
                var posArr = kvp.Value;             // Position[4]

                var p1 = posArr[0];
                var p2 = posArr[1];
                var p3 = posArr[2];
                var p4 = posArr[3];

                // Gate on mask if provided (prevents padding/whitespace from activating)
                bool ok1 = validMask == null || validMask[p1.X, p1.Y];
                bool ok2 = validMask == null || validMask[p2.X, p2.Y];
                bool ok3 = validMask == null || validMask[p3.X, p3.Y];
                bool ok4 = validMask == null || validMask[p4.X, p4.Y];

                // Mark coverage (for debugging)
                if (ok1) testBmpCoverage[p1.X, p1.Y] = true;
                if (ok2) testBmpCoverage[p2.X, p2.Y] = true;
                if (ok3) testBmpCoverage[p3.X, p3.Y] = true;
                if (ok4) testBmpCoverage[p4.X, p4.Y] = true;

                // Read pixels only if valid by mask
                var c1 = ok1 ? bitmap.GetPixel(p1.X, p1.Y) : Color.Black;
                var c2 = ok2 ? bitmap.GetPixel(p2.X, p2.Y) : Color.Black;
                var c3 = ok3 ? bitmap.GetPixel(p3.X, p3.Y) : Color.Black;
                var c4 = ok4 ? bitmap.GetPixel(p4.X, p4.Y) : Color.Black;

                bool w1 = ok1 && CheckIfColorIsWhite(c1);
                bool w2 = ok2 && CheckIfColorIsWhite(c2);
                bool w3 = ok3 && CheckIfColorIsWhite(c3);
                bool w4 = ok4 && CheckIfColorIsWhite(c4);

                if (w1 && w2 && w3 && w4) CheckNInsert(FOMBBMIDS, bbmID, MAPPERCASE.ALL);
                else if (w1 && w2 && w3 && !w4) CheckNInsert(FOMBBMIDS, bbmID, MAPPERCASE.ONETWOTHREEE);
                else if (w1 && w2 && !w3 && w4) CheckNInsert(FOMBBMIDS, bbmID, MAPPERCASE.ONETWOFOUR);
                // ... keep the rest of your cases as before ...
            }
        }

        //Populates the appropriate BBM ID to the Mappere Case as per the pixel data.
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
            => color.R > 240 && color.G > 240 && color.B > 240;

        private bool CheckIfColorIsBlack(Color color)
            => color.R < 10 && color.G < 10 && color.B < 10;

        public void Clean()
        {
            FOMBBMIDS.Clear();
            SomPositions.Clear();
            Xoffset = -1;
            YOffset = -1;
        }


        /// <summary>
        /// Takes in a last firing SDR from L3B & Converts it into a Sensation_Location Object.
        /// </summary>                
        public Sensation_Location GetSenseiFromSDR_V(SDR_SOM sdr_SOM, Orchestrator.POINT point)
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

                if (bbmID > 125 || bbmID < 0)
                {
                    // BUG : need to figure out why SOM can have 1000 active bit
                    throw new InvalidOperationException("BBM ID cannot exceed more than 99 for this system!");
                    //continue;
                }

                position = GetPositionForActiveBit(point, pos.X);

                //position = GetPositionForBBMID(bbmID, point);

                if (sensation_Location.sensLoc.TryGetValue(position?.ToString(), out KeyValuePair<int, List<Position2D>> kvp))
                {
                    kvp.Value.Add(new Position2D(pos.X, pos.Y));
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
