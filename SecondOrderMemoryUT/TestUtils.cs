namespace SecondOrderMemoryUT
{
    using Common;    
    using SecondOrderMemory.Models;

    internal static class TestUtils
    {
        internal static Position_SOM GetSpatialAndTemporalOverlap(Position_SOM spatial, Position_SOM temporal)
        {
            return new Position_SOM(spatial.X, spatial.Y, temporal.X);
        }

        internal static Neuron GetSpatialNeuronFromTemporalCoordinate(SBBManager bbManager, Position pos)
        {
            return bbManager.Columns[pos.Z, pos.Y].Neurons[pos.X];
        }

        internal static SDR_SOM GenerateRandomSDRFromPosition(List<Position_SOM> posList, iType inputPatternType)
        {
            return new SDR_SOM(10, 10, posList, inputPatternType);
        }

        internal static Position_SOM GenerateRandomPosition(int posCount)
        {
            Random rand = new Random();

            return new Position_SOM(rand.Next(0, 9), rand.Next(0, 9), rand.Next(0, 9));
        }

        internal static SDR_SOM GenerateRandomSDR(iType inputPatternType)
        {
            Random rand = new Random();

            int numPos = rand.Next(1, 10);

            List<Position_SOM> posList = new List<Position_SOM>();

            for (int i = 0; i < numPos; i++)
            {
                posList.Add(new Position_SOM(rand.Next(0, 9), rand.Next(0, 9), rand.Next(0, 9)));
            }

            return new SDR_SOM(10, 10, posList, inputPatternType);
        }

        internal static SDR_SOM GenerateSpecificSDRForTemporalWiring(iType inputPatternType)
        {
            Random rand = new Random();
            int numPos = rand.Next(0, 10);

            List<Position_SOM> spatialPosList = new List<Position_SOM>()
            {
                new Position_SOM(2,4),
                new Position_SOM(8,3),
                new Position_SOM(7,2),
                new Position_SOM(0,0)
            };

            List<Position_SOM> temporalPosList = new List<Position_SOM>()
            {
                new Position_SOM(0,4),
                new Position_SOM(8,3),
                new Position_SOM(7,2),
                new Position_SOM(0,0)
            };


            if (inputPatternType == iType.TEMPORAL)
            {
                return new SDR_SOM(10, 10, temporalPosList, inputPatternType);
            }
            else
            {
                return new SDR_SOM(10, 10, spatialPosList, inputPatternType);
            }
        }

        internal static SDR_SOM GetSDRFromPattern(char ch)
        {
            ByteEncoder encoder = new ByteEncoder(100, 8);

            encoder.Encode((byte)ch);

            return (SDR_SOM)encoder.GetSparseSDR();
        }

        internal static List<SDR_SOM> GenerateFixedRandomSDR_SOMs(int iterations, int minValue, int maxValue)
        {
            List<SDR_SOM> toReturn = new List<SDR_SOM>();

            int subIterations = 4;

            List<int> Xcordinates = GenerateUnqiueRandomNumbers(subIterations, minValue, maxValue);
            List<int> Ycordinates = GenerateUnqiueRandomNumbers(subIterations, minValue, maxValue);
            List<int> Zcordinates = GenerateUnqiueRandomNumbers(subIterations, minValue, maxValue);
            int i = 0, j = 0;

            while (i < iterations)
            {
                while (j < subIterations)
                {
                    toReturn.Add(new SDR_SOM(10, 10, new List<Position_SOM>() { new Position_SOM(Xcordinates[j], Ycordinates[j], Zcordinates[j]) }));
                    i++;
                    j++;
                }
                j = 0;
            }

            return toReturn;

        }

        internal static List<SDR_SOM> GetSpecificPatternAmoungNoise(int iterations, int patternSize, int noiseSize, int minValue, int maxValue)
        {
            List<SDR_SOM> toReturn = new List<SDR_SOM>();

            List<int> Xcordinates = GenerateUnqiueRandomNumbers(noiseSize, minValue, maxValue);
            List<int> Ycordinates = GenerateUnqiueRandomNumbers(noiseSize, minValue, maxValue);
            List<int> Zcordinates = GenerateUnqiueRandomNumbers(noiseSize, minValue, maxValue);

            int NumColumns = 10;


            SDR_SOM pattern1 = new SDR_SOM(NumColumns, NumColumns, new List<Position_SOM>() { new Position_SOM(5, 5, 5) }, iType.SPATIAL);
            SDR_SOM pattern2 = new SDR_SOM(NumColumns, NumColumns, new List<Position_SOM>() { new Position_SOM(7, 7, 7) }, iType.SPATIAL);


            for (int i = 0; i < iterations; i++)
            {
                List<SDR_SOM> toAdd = new List<SDR_SOM>();


                for (int j = 0; j < noiseSize; j++)
                {
                    try
                    {
                        toAdd.Add(new SDR_SOM(NumColumns, NumColumns, new List<Position_SOM>() { new Position_SOM(Xcordinates[j], Ycordinates[j], Zcordinates[j]) }));
                    }
                    catch (Exception e)
                    {
                        int breakpoint = 1;
                    }
                }

                toAdd.Add(pattern1);
                toAdd.Add(pattern2);
            }


            return toReturn;
        }

        private static List<int> GenerateUnqiueRandomNumbers(int num_nums, int minValue, int maxValue)
        {
            List<int> toReturn = new List<int>();
            Random rand = new Random();
            int num = 0;
            bool b = false;

            while (toReturn.Count <= num_nums)
            {
                num = rand.Next(minValue, maxValue);

                b = toReturn.Contains(num);

                if (b == false)
                {
                    toReturn.Add(num);
                }
                else
                {
                    while (b == true)
                    {
                        num = rand.Next(minValue, maxValue);

                        b = toReturn.Contains(num);
                    }

                    toReturn.Add(num);
                }
            }

            return toReturn;
        }
    }
}
