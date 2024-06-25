using Common;
using FirstOrderMemory.Models.Encoders;
using System;

namespace FirstOrderMemory.Models
{
    public class LocationScalarEncoder : Encoder
    {
        /// <summary>
        /// There are 2 co-ordinates X & Y , each coordinate has fixed range of 0 - 9999 and each coordinate gets 16 bits each to represent it. So total of 32 bits assigned for both coordiantes with a sepparation of 3 bits between each byte
        /// X gets 4 numbers 0 - 9 and each number has 4 pixels which will be mapped to 16 pixels and so on for all the numbers.
        /// x : 16
        /// y : 16
        /// T : 32
        /// </summary>
        public int NumBukets { get; private set; }            
        
        public Dictionary<int , int[]> Mappings { get; private set; }

        public LocationScalarEncoder(int n, int w) : base(n, w)
        {

            if( n != 100 & w != 32)
            {
                throw new ArgumentException("ScalarEncoder currently does not support any other configuration for n & w other than 100 & 8 correspondingly!");
            }

            NumBukets = n / w;
            Mappings = new Dictionary<int, int[]>();
            

        }

        private void ComputeMappings()
        {
            //Custom Mappings for Locaation Based Co-ordiantes
            Mappings.Add(0, new int[] { 1, 2, 3, 4 });            
            Mappings.Add(1, new int[] { 5, 6, 7, 8 });
            Mappings.Add(2, new int[] { 1, 2, 3, 4 });
            Mappings.Add(3, new int[] { 5, 6, 7, 8 });
            Mappings.Add(4, new int[] { 1, 2, 3, 4 });
            Mappings.Add(5, new int[] { 5, 6, 7, 8 });
            Mappings.Add(6, new int[] { 1, 2, 3, 4 });
            Mappings.Add(7, new int[] { 5, 6, 7, 8 });
            Mappings.Add(8, new int[] { 1, 2, 3, 4 });
            Mappings.Add(9, new int[] { 5, 6, 7, 8 });
        }

        //public SDR_SOM Encode(int bucket)
        //{
        //    if( 0  >  bucket && bucket >= 10)
        //    {
        //        throw new ArgumentOutOfRangeException("Encoder:: Bucket is not within range : " + bucket.ToString());
        //    }

        //    if(!Mappings.TryGetValue(bucket, out var mappings))
        //    {
        //        throw new ArgumentOutOfRangeException("Encoder:: Encoder could not find the correct bucket, Invalid Bucket Allocation!");
        //    }

        //    List<Position_SOM> activePositons = new List<Position_SOM>();


        //    foreach( var mapping in mappings) 
        //    {
        //        if (bucket >= 10 || mapping > 10)
        //        {
        //            throw new InvalidDataException("Encode :: Active Bits should never corss more than column length and breadth");
        //        }
        //        activePositons.Add(new Position_SOM(bucket % 2 == 0 ? bucket + 1 : bucket, mapping)); 
        //    }
                       
        //    return new SDR_SOM(10, 10, activePositons, iType.TEMPORAL);
        //}


        //Get Dense Representation


        public SDR_SOM Encode( int x, int y)
        {
            //Takes in a BBM Coordinate spanned across 10 bits in the pixel data, and creates one unique SDR_SOM pattern to be fed in a temporal pattern.
            // Todo: Need a fancy encyrption function that takes in 2 integers , x and y and spits a unique list of active positions in a bounded range 
            List<Position_SOM> activePositions = new List<Position_SOM>();

            activePositions.AddRange(GetActivePositionFromScalar(x, 1));
            activePositions.AddRange(GetActivePositionFromScalar(y, 2));

            return new SDR_SOM(10, 10, activePositions, iType.TEMPORAL);
        }

        private List<Position_SOM> GetActivePositionFromScalar(int number, int numberPosition)
        {
            List<Position_SOM> toReturn = new List<Position_SOM>();

            toReturn.AddRange(ExtractAndAddPositions(number, numberPosition));


            return toReturn;
        }
        
        private List<Position_SOM> ExtractAndAddPositions(int number, int numberPosition)
        {
            List<Position_SOM> Positions = new List<Position_SOM>();

            int offset = numberPosition == 1 ? 0 : 58;
            int numbercopy = number;
            int digit = 0;
            int iterator = 0;
            int digitOffset = 0;


            while (numbercopy > 0 && iterator < 4)
            {
                digit = numbercopy % 10;

                digitOffset = ComputeDigitOffset(numberPosition, iterator);

                if(digitOffset == int.MaxValue)
                {
                    throw new ArgumentException("Invalid Operation");
                }
                
                Positions.AddRange(GetPositionsFromDigit(digit, offset));

                numbercopy = numbercopy / 10;

                iterator++;
            }

            return Positions;
        }        

        private List<Position_SOM> GetPositionsFromDigit(int digit, int offset)
        {

            if (digit > 9)
            {
                throw new ArgumentOutOfRangeException("Entire universe will be commanded to revolve around you , you have amanged to do the impossible in math , Thios can never happen !! you cannot exceed a modulo operator to above 9");
            }

            List<Position_SOM> Positions = new List<Position_SOM>();

            switch(digit)
            {
                case 0:       //0 0 0 0
                    break;
                case 1:       //0 0 0 1
                    {
                        Tuple<int,int> tuple = ExtractXnY(offset + 3);
                        Positions.Add(new Position_SOM(tuple.Item1, tuple.Item2));
                        break;
                    }
                case 2:       //0 0 1 0
                    {
                        Tuple<int, int> tuple = ExtractXnY(offset + 2);
                        Positions.Add(new Position_SOM(tuple.Item1, tuple.Item2));
                        break;
                    }                    
                case 3:       //0 0 1 1
                    {
                        Tuple<int, int> tuple1 = ExtractXnY(offset + 2);
                        Tuple<int, int> tuple2 = ExtractXnY(offset + 3);
                        Positions.Add(new Position_SOM(tuple1.Item1, tuple1.Item2));
                        Positions.Add(new Position_SOM(tuple2.Item1, tuple2.Item2));
                        break;
                    }
                case 4:       //0 1 0 0
                    {
                        Tuple<int, int> tuple1 = ExtractXnY(offset + 1);                        
                        Positions.Add(new Position_SOM(tuple1.Item1, tuple1.Item2));                        
                        break;
                    }
                case 5:       //0 1 0 1
                    {
                        Tuple<int, int> tuple1 = ExtractXnY(offset + 1);
                        Tuple<int, int> tuple2 = ExtractXnY(offset + 3);
                        Positions.Add(new Position_SOM(tuple1.Item1, tuple1.Item2));
                        Positions.Add(new Position_SOM(tuple2.Item1, tuple2.Item2));
                        break;
                    }
                case 6:       //0 1 1 0
                    {
                        Tuple<int, int> tuple1 = ExtractXnY(offset + 1);
                        Tuple<int, int> tuple2 = ExtractXnY(offset + 2);
                        Positions.Add(new Position_SOM(tuple1.Item1, tuple1.Item2));
                        Positions.Add(new Position_SOM(tuple2.Item1, tuple2.Item2));
                        break;
                    }
                case 7:       //0 1 1 1
                    {
                        Tuple<int, int> tuple1 = ExtractXnY(offset + 1);
                        Tuple<int, int> tuple2 = ExtractXnY(offset + 2);
                        Tuple<int, int> tuple3 = ExtractXnY(offset + 3);
                        Positions.Add(new Position_SOM(tuple1.Item1, tuple1.Item2));
                        Positions.Add(new Position_SOM(tuple2.Item1, tuple2.Item2));
                        Positions.Add(new Position_SOM(tuple3.Item1, tuple3.Item2));
                        break;
                    }
                case 8:       //1 0 0 0
                    {
                        Tuple<int, int> tuple1 = ExtractXnY(offset + 0);                        
                        Positions.Add(new Position_SOM(tuple1.Item1, tuple1.Item2));                        
                        break;
                    }
                case 9:       //1 0 0 1
                    {
                        Tuple<int, int> tuple1 = ExtractXnY(offset + 0);
                        Tuple<int, int> tuple2 = ExtractXnY(offset + 3);
                        Positions.Add(new Position_SOM(tuple1.Item1, tuple1.Item2));
                        Positions.Add(new Position_SOM(tuple2.Item1, tuple2.Item2));
                        break;
                    }
                default: 
                    {
                        throw new InvalidDataException("GetPositionsFromDigit :: Invalid Digit Extracted , Digit should always fall in the range of 0 - 9");
                    }
            }


            if(digit.Equals(1))
            {
                int breakpoint = 1;
            }

            return Positions;
        }

        private int ComputeDigitOffset(int coordinate, int iterator)
        {
            if (coordinate == 1)
            {
                if (iterator == 0)
                {
                    return 0;
                }
                else if (iterator == 1)
                {
                    return 16;
                }
                else if (iterator == 2)
                {
                    return 20;
                }
                else if (iterator == 3)
                {
                    return 36;
                }
            }
            else if (coordinate == 2)
            {
                if (iterator == 0)
                {
                    return 60;
                }
                else if (iterator == 1)
                {
                    return 76;
                }
                else if (iterator == 2)
                {
                    return 80;
                }
                else if (iterator == 3)
                {
                    return 96;
                }
            }
            else
            {
                throw new InvalidOperationException("ComputeDigitOffset ::  Coordinate Number should never be anything other than 1 or 2!");
            }

            return int.MaxValue;
        }


        private Tuple<int,int> ExtractXnY(int number)
        {
            int Y = number % 10;
            number = number / 10;
            int X = number % 10;
            number = number / 10;
            if(number != 0)
            {
                Console.WriteLine("ExtractXnY :: number should be 0");
            }

            return new Tuple<int, int>(X, Y);
            
        }
    }
}
