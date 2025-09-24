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


        public List<Position_SOM> SomPositions { get; private set; }

        public bool[,] testBmpCoverage { get; private set; }

        public int LENGTH { get; private set; }

        public int WIDTH { get; private set; }
        public class PixelEncoderV2 : PixelEncoder
        {
            public PixelEncoderV2(int numBBM, int numPixels) : base(numBBM, numPixels)
            {
                // V2 specific mappings for 100x100 processing
                PerformV2Mappings();
            }

            private void PerformV2Mappings()
            {
                // Create mappings for 50x50 BBM grid covering 100x100 image
                // Each BBM covers 2x2 pixel area
            }
        }
        public class PixelEncoderV3 : PixelEncoder
        {
            public PixelEncoderV3(int numBBM, int numPixels) : base(numBBM, numPixels)
            {
                // V3 specific mappings for 200x200 processing
                PerformV3Mappings();
            }

            private void PerformV3Mappings()
            {
                // Create mappings for 100x100 BBM grid covering 200x200 image  
                // Each BBM covers 2x2 pixel area
            }
        }
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
            Mappings = new Dictionary<int, Position[]>(capacity: 100);

            int blocksPerSide = 10;   // 20 / 2
            for (int br = 0; br < blocksPerSide; br++)
            {
                for (int bc = 0; bc < blocksPerSide; bc++)
                {
                    int bbmID = br * blocksPerSide + bc;
                    int x0 = br * 2;
                    int y0 = bc * 2;

                    // (x,y) within [0..19]
                    Mappings[bbmID] = new[]
                    {
                new Position(x0,     y0    ),
                new Position(x0,     y0 + 1),
                new Position(x0 + 1, y0    ),
                new Position(x0 + 1, y0 + 1),
            };
                }
            }
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

        public void ParseBitmap(Bitmap bitmap)
        {
            if (bitmap.Width != 20 || bitmap.Height != 20)
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
                }
                else if (check1 && check2 && check3 && check4 == false)
                {
                    if (bbmID == 79)
                    {
                        bool breakpoint = true;
                    }

                    CheckNInsert(FOMBBMIDS, bbmID, MAPPERCASE.ONETWOTHREEE);
                }
                else if (check1 && check2 && check4 && check3 == false)
                {
                    CheckNInsert(FOMBBMIDS, bbmID, MAPPERCASE.ONETWOFOUR);
                }
                else if (check1 && check3 && check4 && check2 == false)
                {
                    CheckNInsert(FOMBBMIDS, bbmID, MAPPERCASE.ONETHREEFOUR);

                }
                else if (check2 && check3 && check4 && check1 == false)                     //3's
                {
                    CheckNInsert(FOMBBMIDS, bbmID, MAPPERCASE.TWOTHREEFOUR);
                }
                else if (check1 && check2 && check3 == false && check4 == false)
                {
                    CheckNInsert(FOMBBMIDS, bbmID, MAPPERCASE.ONETWO);
                }
                else if (check1 && check3 && check2 == false && check4 == false)
                {
                    CheckNInsert(FOMBBMIDS, bbmID, MAPPERCASE.ONETHREE);
                }
                else if (check4 && check3 && check2 == false && check1 == false)
                {
                    CheckNInsert(FOMBBMIDS, bbmID, MAPPERCASE.THREEFOUR);
                }
                else if (check4 && check1 && check2 == false && check3 == false)
                {
                    CheckNInsert(FOMBBMIDS, bbmID, MAPPERCASE.ONEFOUR);
                }
                else if (check4 && check2 && check3 == false && check1 == false)
                {
                    CheckNInsert(FOMBBMIDS, bbmID, MAPPERCASE.TWOFOUR);
                }
                else if (check2 && check3 && check4 == false && check1 == false)            //2's
                {
                    CheckNInsert(FOMBBMIDS, bbmID, MAPPERCASE.TWOTHREE);
                }
                else if (check1 && check2 == false && check3 == false && check4 == false)
                {
                    CheckNInsert(FOMBBMIDS, bbmID, MAPPERCASE.ONE);
                }
                else if (check2 && check1 == false && check3 == false && check4 == false)
                {
                    CheckNInsert(FOMBBMIDS, bbmID, MAPPERCASE.TWO);
                }
                else if (check3 && check1 == false && check2 == false && check4 == false)
                {
                    CheckNInsert(FOMBBMIDS, bbmID, MAPPERCASE.THREE);
                }
                else if (check4 && check1 == false && check2 == false && check3 == false)    //1's
                {
                    CheckNInsert(FOMBBMIDS, bbmID, MAPPERCASE.FOUR);
                }
                else
                {
                    //No Fire :: Do Nothing!
                }
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
