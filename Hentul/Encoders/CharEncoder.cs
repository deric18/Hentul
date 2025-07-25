﻿namespace Hentul.Encoders
{
    using Common;
    using Hentul.Hippocampal_Entorinal_complex;

    /// <summary>
    /// An Encoder typically takes in human readable input like a pixel value or character in this case and converts it into a machine readable input format like SDR
    /// In this Encoder it typically reads in the input and intializes the below FOMBBMIDs & SOMBBMIDs objects based on the mappings.
    /// A Character encoder takes in a single character which is 2 bytes.
    /// 1 bit -> 25 cells
    /// 16 bits -> 400 cells , 1 BBM -> 100 cells , So 2 bytes -> 4 FOM BBMs
    /// FOM -> 4 BBMS of 10 * 10 * 4
    /// SOM -> 1 BBM of size 400 * 10 * 4
    /// </summary>
    public class CharEncoder
    {

        public int NumBBMPerChar { get; private set; }

        private Dictionary<int, List<KeyValuePair<int, MAPPERCASE>>> Mappings { get; set; }

        public List<KeyValuePair<int, MAPPERCASE>> FOMBBMIDS { get; private set; }

        public List<Position_SOM> somPositions;

        List<Position_SOM> ONbits1FOM;
        List<Position_SOM> ONbits2FOM;
        List<Position_SOM> ONbits3FOM;
        List<Position_SOM> ONbits4FOM;


        public CharEncoder()
        {
            NumBBMPerChar = 4;
            FOMBBMIDS = new();
            somPositions = new();

            PerformMappings();

            ONbits1FOM = new List<Position_SOM>()
            {
                new Position_SOM(1, 1),
                new Position_SOM(3, 4)
            };

            ONbits2FOM = new List<Position_SOM>()
            {
                new Position_SOM(6, 3),
                new Position_SOM(8, 1)
            };

            ONbits3FOM = new List<Position_SOM>()
            {
                new Position_SOM(6, 6),
                new Position_SOM(8, 8)
            };

            ONbits4FOM = new List<Position_SOM>()
            {
                new Position_SOM(1, 8),
                new Position_SOM(3, 6)
            };
        }

        private void PerformMappings()
        {
            Mappings = new Dictionary<int, List<KeyValuePair<int, MAPPERCASE>>>
            {
                {   1   , new List<KeyValuePair<int, MAPPERCASE>>() { new KeyValuePair<int, MAPPERCASE>(0, MAPPERCASE.ONE) } },         //A
                {   2   , new List<KeyValuePair<int, MAPPERCASE>>() { new KeyValuePair<int, MAPPERCASE>(0, MAPPERCASE.TWO) } },         //B
                {   3   , new List<KeyValuePair<int, MAPPERCASE>>() { new KeyValuePair<int, MAPPERCASE>(0, MAPPERCASE.ONETWO) } },      //C
                {   4   , new List<KeyValuePair<int, MAPPERCASE>>() { new KeyValuePair<int, MAPPERCASE>(0, MAPPERCASE.THREE) } },       //D
                {   5   , new List<KeyValuePair<int, MAPPERCASE>>() { new KeyValuePair<int, MAPPERCASE>(0, MAPPERCASE.ONETHREE) } },    //E
                {   6   , new List<KeyValuePair<int, MAPPERCASE>>() { new KeyValuePair<int, MAPPERCASE>(0, MAPPERCASE.TWOTHREE) } },    //F
                {   7   , new List<KeyValuePair<int, MAPPERCASE>>() { new KeyValuePair<int, MAPPERCASE>(0, MAPPERCASE.ONETWOTHREEE) } },//G
                {   8   , new List<KeyValuePair<int, MAPPERCASE>>() { new KeyValuePair<int, MAPPERCASE>(0, MAPPERCASE.FOUR) } },        //H
                {   9   , new List<KeyValuePair<int, MAPPERCASE>>() { new KeyValuePair<int, MAPPERCASE>(0, MAPPERCASE.ONEFOUR) } },     //I
                {   10   , new List<KeyValuePair<int, MAPPERCASE>>() { new KeyValuePair<int, MAPPERCASE>(0, MAPPERCASE.TWOFOUR) } },    //J    
                {   11   , new List<KeyValuePair<int, MAPPERCASE>>() { new KeyValuePair<int, MAPPERCASE>(0, MAPPERCASE.ONETWOFOUR) } }, //K
                {   12   , new List<KeyValuePair<int, MAPPERCASE>>() { new KeyValuePair<int, MAPPERCASE>(0, MAPPERCASE.THREEFOUR) } },  //L
                {   13   , new List<KeyValuePair<int, MAPPERCASE>>() { new KeyValuePair<int, MAPPERCASE>(0, MAPPERCASE.ONETHREEFOUR) } },//M
                {   14   , new List<KeyValuePair<int, MAPPERCASE>>() { new KeyValuePair<int, MAPPERCASE>(0, MAPPERCASE.TWOTHREEFOUR)} },//N
                {   15   , new List<KeyValuePair<int, MAPPERCASE>>() { new KeyValuePair<int, MAPPERCASE>(0, MAPPERCASE.ALL) } },        //O
                {   16   , new List<KeyValuePair<int, MAPPERCASE>>() { new KeyValuePair<int, MAPPERCASE>(1, MAPPERCASE.ONE) } },        //P                
                {   17   , new List<KeyValuePair<int, MAPPERCASE>>() { new KeyValuePair<int, MAPPERCASE>(1, MAPPERCASE.ONE),
                                                                       new KeyValuePair<int, MAPPERCASE>(0, MAPPERCASE.TWO) } },        //Q
                {   18   , new List<KeyValuePair<int, MAPPERCASE>>() { new KeyValuePair<int, MAPPERCASE>(1, MAPPERCASE.ONE),
                                                                       new KeyValuePair<int, MAPPERCASE>(0, MAPPERCASE.TWO)} },         //R
                {   19   , new List<KeyValuePair<int, MAPPERCASE>>() { new KeyValuePair<int, MAPPERCASE>(1, MAPPERCASE.ONE),
                                                                       new KeyValuePair<int, MAPPERCASE>(0, MAPPERCASE.ONETWO) }},      //S
                {   20   , new List<KeyValuePair<int, MAPPERCASE>>() { new KeyValuePair<int, MAPPERCASE>(1, MAPPERCASE.ONE),
                                                                       new KeyValuePair<int, MAPPERCASE>(0, MAPPERCASE.THREE)} },       //T
                {   21   , new List<KeyValuePair<int, MAPPERCASE>>() { new KeyValuePair<int, MAPPERCASE>(1, MAPPERCASE.ONE),
                                                                      new KeyValuePair<int, MAPPERCASE>(0, MAPPERCASE.ONETHREE)} },     //U
                {   22   , new List<KeyValuePair<int, MAPPERCASE>>() { new KeyValuePair<int, MAPPERCASE>(1, MAPPERCASE.ONE),
                                                                      new KeyValuePair<int, MAPPERCASE>(0, MAPPERCASE.TWOTHREE)} },     //V
                {   23  , new List<KeyValuePair<int, MAPPERCASE>>() { new KeyValuePair<int, MAPPERCASE>(1, MAPPERCASE.ONE),
                                                                      new KeyValuePair<int, MAPPERCASE>(0, MAPPERCASE.ONETWOTHREEE)} }, //W
                {   24  , new List<KeyValuePair<int, MAPPERCASE>>() { new KeyValuePair<int, MAPPERCASE>(1, MAPPERCASE.ONE),
                                                                      new KeyValuePair<int, MAPPERCASE>(0, MAPPERCASE.FOUR)} },         //X
                {   25  , new List<KeyValuePair<int, MAPPERCASE>>() { new KeyValuePair<int, MAPPERCASE>(1, MAPPERCASE.ONE),
                                                                      new KeyValuePair<int, MAPPERCASE>(0, MAPPERCASE.ONEFOUR)} },      //Y
                {   26  , new List<KeyValuePair<int, MAPPERCASE>>() { new KeyValuePair<int, MAPPERCASE>(1, MAPPERCASE.ONE),
                                                                      new KeyValuePair<int, MAPPERCASE>(0, MAPPERCASE.TWOFOUR) } }      //Z
            };
        }

        public void Encode(char ch)
        {
            if (Mappings.TryGetValue(ch - 64, out var listkvp))
            {
                FOMBBMIDS = listkvp;
                return;
            }

            throw new InvalidOperationException("Invalid Character passed! Mappings does not exists for : " + ch);
        }

        public Sensation GetSenseiFromSDR_T(SDR_SOM sdr_SOM)
        {
            if (sdr_SOM.Length < 200)
            {
                throw new InvalidDataException("SDR SOM is empty for Layer 3B or Invalid SDR Size!!!");
            }

            if (sdr_SOM.ActiveBits.Count == 0)
            {
                throw new InvalidOperationException(" sdr_SOM has not Active bits!");
            }

            Sensation sensation = new();

            foreach (var pos in sdr_SOM.ActiveBits)
            {
                int bbmID = pos.X / 100;

                if (bbmID > 1 || bbmID < 0)
                {
                    // BUG : need to figure out why SOM can have 1000 active bit
                    throw new InvalidOperationException("BBM ID cannot exceed more than 99 for this system!");
                }

                sensation.Positions.Add(pos);
            }

            return sensation;
        }

        public SDR_SOM GenerateSDR_SOMForMapperCase(MAPPERCASE mappercase, int bbmID)
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

        public void Clean()
        {
            somPositions.Clear();
        }
    }
}
