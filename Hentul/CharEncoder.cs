namespace Hentul
{
    using Common;

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

        public Dictionary<int, List<KeyValuePair<int, MAPPERCASE>>> Mappings { get; private set; }
        
        public Dictionary<MAPPERCASE, List<int>> FOMBBMIDS { get; private set; }

        public Dictionary<MAPPERCASE, List<int>> SOMBBMIDS { get; private set; }

        List<Position_SOM> ONbits1FOM;
        List<Position_SOM> ONbits2FOM;
        List<Position_SOM> ONbits3FOM;
        List<Position_SOM> ONbits4FOM;

        public CharEncoder()
        {
            NumBBMPerChar = 4;            
            FOMBBMIDS = new();
            SOMBBMIDS = new();

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
                {   1   , new List<KeyValuePair<int, MAPPERCASE>>() { new KeyValuePair<int, MAPPERCASE>(1, MAPPERCASE.ONE) } },         //A
                {   2   , new List<KeyValuePair<int, MAPPERCASE>>() { new KeyValuePair<int, MAPPERCASE>(1, MAPPERCASE.TWO) } },         //B
                {   3   , new List<KeyValuePair<int, MAPPERCASE>>() { new KeyValuePair<int, MAPPERCASE>(1, MAPPERCASE.ONETWO) } },      //C
                {   4   , new List<KeyValuePair<int, MAPPERCASE>>() { new KeyValuePair<int, MAPPERCASE>(1, MAPPERCASE.THREE) } },       //D
                {   5   , new List<KeyValuePair<int, MAPPERCASE>>() { new KeyValuePair<int, MAPPERCASE>(1, MAPPERCASE.ONETHREE) } },    //E
                {   6   , new List<KeyValuePair<int, MAPPERCASE>>() { new KeyValuePair<int, MAPPERCASE>(1, MAPPERCASE.TWOTHREE) } },    //F
                {   7   , new List<KeyValuePair<int, MAPPERCASE>>() { new KeyValuePair<int, MAPPERCASE>(1, MAPPERCASE.ONETWOTHREEE) } },//G
                {   8   , new List<KeyValuePair<int, MAPPERCASE>>() { new KeyValuePair<int, MAPPERCASE>(1, MAPPERCASE.FOUR) } },        //H
                {   9   , new List<KeyValuePair<int, MAPPERCASE>>() { new KeyValuePair<int, MAPPERCASE>(1, MAPPERCASE.ONEFOUR) } },     //I
                {   10   , new List<KeyValuePair<int, MAPPERCASE>>() { new KeyValuePair<int, MAPPERCASE>(1, MAPPERCASE.TWOFOUR) } },    //J    
                {   11   , new List<KeyValuePair<int, MAPPERCASE>>() { new KeyValuePair<int, MAPPERCASE>(1, MAPPERCASE.ONETWOFOUR) } }, //K
                {   12   , new List<KeyValuePair<int, MAPPERCASE>>() { new KeyValuePair<int, MAPPERCASE>(1, MAPPERCASE.THREEFOUR) } },  //L
                {   13   , new List<KeyValuePair<int, MAPPERCASE>>() { new KeyValuePair<int, MAPPERCASE>(1, MAPPERCASE.ONETHREEFOUR) } },//M
                {   14   , new List<KeyValuePair<int, MAPPERCASE>>() { new KeyValuePair<int, MAPPERCASE>(1, MAPPERCASE.TWOTHREEFOUR)} },//N
                {   15   , new List<KeyValuePair<int, MAPPERCASE>>() { new KeyValuePair<int, MAPPERCASE>(1, MAPPERCASE.ALL) } },        //O
                {   16   , new List<KeyValuePair<int, MAPPERCASE>>() { new KeyValuePair<int, MAPPERCASE>(2, MAPPERCASE.ONE) } },        //P                
                {   17   , new List<KeyValuePair<int, MAPPERCASE>>() { new KeyValuePair<int, MAPPERCASE>(2, MAPPERCASE.ONE),
                                                                       new KeyValuePair<int, MAPPERCASE>(1, MAPPERCASE.TWO) } },        //Q
                {   18   , new List<KeyValuePair<int, MAPPERCASE>>() { new KeyValuePair<int, MAPPERCASE>(2, MAPPERCASE.ONE),
                                                                       new KeyValuePair<int, MAPPERCASE>(1, MAPPERCASE.TWO)} },         //R
                {   19   , new List<KeyValuePair<int, MAPPERCASE>>() { new KeyValuePair<int, MAPPERCASE>(2, MAPPERCASE.ONE),
                                                                       new KeyValuePair<int, MAPPERCASE>(1, MAPPERCASE.ONETWO) }},      //S
                {   20   , new List<KeyValuePair<int, MAPPERCASE>>() { new KeyValuePair<int, MAPPERCASE>(2, MAPPERCASE.ONE),
                                                                       new KeyValuePair<int, MAPPERCASE>(1, MAPPERCASE.THREE)} },       //T
                {   21   , new List<KeyValuePair<int, MAPPERCASE>>() { new KeyValuePair<int, MAPPERCASE>(2, MAPPERCASE.ONE),
                                                                      new KeyValuePair<int, MAPPERCASE>(1, MAPPERCASE.ONETHREE)} },     //U
                {   22   , new List<KeyValuePair<int, MAPPERCASE>>() { new KeyValuePair<int, MAPPERCASE>(2, MAPPERCASE.ONE),
                                                                      new KeyValuePair<int, MAPPERCASE>(1, MAPPERCASE.TWOTHREE)} },     //V
                {   23  , new List<KeyValuePair<int, MAPPERCASE>>() { new KeyValuePair<int, MAPPERCASE>(2, MAPPERCASE.ONE),
                                                                      new KeyValuePair<int, MAPPERCASE>(1, MAPPERCASE.ONETWOTHREEE)} }, //W
                {   24  , new List<KeyValuePair<int, MAPPERCASE>>() { new KeyValuePair<int, MAPPERCASE>(2, MAPPERCASE.ONE),
                                                                      new KeyValuePair<int, MAPPERCASE>(1, MAPPERCASE.FOUR)} },         //X
                {   25  , new List<KeyValuePair<int, MAPPERCASE>>() { new KeyValuePair<int, MAPPERCASE>(2, MAPPERCASE.ONE),
                                                                      new KeyValuePair<int, MAPPERCASE>(1, MAPPERCASE.ONEFOUR)} },      //Y
                {   26  , new List<KeyValuePair<int, MAPPERCASE>>() { new KeyValuePair<int, MAPPERCASE>(2, MAPPERCASE.ONE),
                                                                      new KeyValuePair<int, MAPPERCASE>(1, MAPPERCASE.TWOFOUR) } }      //Z
            };
        }

        public void Encode(char ch)
        {
            SDR_SOM sdr = new SDR_SOM(10, 10, new List<Position_SOM>());
            
        }
    }
}
