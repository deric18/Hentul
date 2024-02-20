namespace SecondOrderMemoryUnitTest
{
    using Common;
    using SecondOrderMemory.Models;
    using SecondOrderMemory.BehaviourManagers;

    internal static class TestUtils
    {
        internal static Position_SOM GetSpatialAndTemporalOverlap(Position_SOM spatial, Position_SOM temporal)
        {
            return new Position_SOM(spatial.X, spatial.Y, temporal.X);
        }

        internal static Neuron GetSpatialNeuronFromTemporalCoordinate(BlockBehaviourManager bbManager, Position pos)
        {
            return bbManager.Columns[pos.Z, pos.Y].Neurons[pos.X];
        }

        internal static SDR_SOM GenerateRandomSDRFromPosition(List<Position_SOM> posList, iType inputPatternType)
        {
            return new SDR_SOM(10, 10, posList, inputPatternType);
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
            ByteEncoder encoder = new ByteEncoder(1000, 8);

            encoder.Encode((byte)ch);

            return (SDR_SOM)encoder.GetSparseSDR();
        }        
    }
}
