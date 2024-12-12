namespace FirstOrderMemoryUnitTest
{
    using Common;
    using FirstOrderMemory.Models;
    using FirstOrderMemory.BehaviourManagers;
    using FirstOrderMemory.Models.Encoders;
    using static FirstOrderMemory.BehaviourManagers.BlockBehaviourManager;

    internal static class TestUtils
    {

        internal static SDR_SOM AddOffsetToSDR(SDR_SOM sdr, int offset)
        {            
            List<Position_SOM> posList = new List<Position_SOM>();

            foreach (var pos in sdr.ActiveBits)
            {
                if (pos.X + (offset * 100) > 1250)
                {
                    throw new InvalidOperationException("Added offset could not exceed network Layer Length!");
                }
                
                posList.Add(new Position_SOM(pos.X + offset * 100, pos.Y , 0));
            }

            return new SDR_SOM(sdr.Length, sdr.Breadth, posList, sdr.InputPatternType);
        }


        internal static SDR_SOM GetEmptySpatialPattern(List<Position_SOM> lastFirsingCells, int length, int breadth, iType iType, LayerType layerType) =>                    
            new SDR_SOM(length, breadth, GetExclusivePositionList(lastFirsingCells, layerType), iType);        


        public static  List<Neuron> ConvertStringListotNeuronalList(List<string> stringList, BlockBehaviourManager bbManager)
        {
            List<Neuron> neurons = new List<Neuron>();

            foreach (var str in stringList)
            {
                Neuron n = bbManager.GetNeuronFromString(str);

                if(n.nType == NeuronType.NORMAL)
                    neurons.Add(n);
            }

            return neurons;
        }

        public static List<Position_SOM> ConvertStringListoPositionList(List<string> stringList)
        {
            List<Position_SOM> positionList = new List<Position_SOM>();

            foreach (var str in stringList)
            {
                var pos = Position_SOM.ConvertStringToPosition(str);
                if(pos.W == 'N')
                    positionList.Add(pos);
            }

            return positionList;
        }

        private static List<Position_SOM> GetExclusivePositionList(List<Position_SOM> lastFiringCells, LayerType layerType)
        {
            List<Position_SOM> exlcudedPosList = new List<Position_SOM>();
            Random rand = new Random();
            List<int> excludeX = new List<int>();
            List<int> excludeY = new List<int>();
            List<int> excludeZ = new List<int>();
            int maxX = 1000, maxY = 10, maxZ = 4;

            if (layerType != LayerType.Layer_4)
            {
                lastFiringCells.ForEach(pos =>
                {
                    excludeX.Add(pos.X);
                    excludeY.Add(pos.Y);
                    excludeZ.Add(pos.Z);
                });
            }

            int x = 0, y = 0;

            while (exlcudedPosList.Count != 1)
            {
                x = rand.Next(0, maxX); y = rand.Next(0, maxY);

                if(excludeX.Contains(x) == false && excludeY.Contains(y) == false)
                {
                    exlcudedPosList.Add(new Position_SOM(x, y));
                }                
            }

            return exlcudedPosList;
        }

        internal static Position_SOM GetSpatialAndTemporalOverlap(Position_SOM spatial, Position_SOM temporal)
        {
            return new Position_SOM(spatial.X, temporal.X, temporal.Y);
        }        

        internal static Neuron GetSpatialNeuronFromTemporalCoordinate(BlockBehaviourManager bbManager, Position_SOM pos) =>
            bbManager.Columns[pos.Z, pos.X].Neurons[pos.Y];


        internal static SDR_SOM ConvertPositionToSDR(List<Position_SOM> posList, iType inputPatternType) =>
        new SDR_SOM(10, 10, posList, inputPatternType);


        internal static Position_SOM GenerateRandomPosition()
        {
            Random rand = new Random();

            return new Position_SOM(rand.Next(0, 10), rand.Next(0, 10), rand.Next(0, 10));
        }


        internal static SDR_SOM GenerateTemporalSDRforDepolarization()
        {
            return new SDR_SOM(10, 10, GeneratePositionsForPredictiveTest1(), iType.TEMPORAL);
        }


        internal static SDR_SOM GenerateApicalOrSpatialSDRForDepolarization(iType iType = iType.APICAL)
        {
            return new SDR_SOM(10, 10, GeneratePositionsForPredictiveTest1(), iType);
        }


        internal static SDR_SOM GetSpatialAndTemporalOverlapSDR(SDR_SOM apicalSdr, SDR_SOM temporalSdr)
        {
            if (apicalSdr.ActiveBits.Count != temporalSdr.ActiveBits.Count)
                throw new InvalidOperationException("Count do not Match!!!");

            apicalSdr.Sort();
            temporalSdr.Sort();

            List<Position_SOM> activeBits = new List<Position_SOM>();

            for (int i = 0; i < temporalSdr.ActiveBits.Count; i++)
            {
                activeBits.Add(new Position_SOM(apicalSdr.ActiveBits[i].X, temporalSdr.ActiveBits[i].X, temporalSdr.ActiveBits[i].Z));
            }

            return new SDR_SOM(apicalSdr.Length, apicalSdr.Breadth, activeBits);

        }

        internal static List<Position_SOM> GeneratePositionsForPredictiveTest1()
        {
            List<Position_SOM> positions = new List<Position_SOM>();

            positions.Add(new Position_SOM(3, 2, 1));
            positions.Add(new Position_SOM(5, 5, 0));
            positions.Add(new Position_SOM(7, 8, 2));
            positions.Add(new Position_SOM(8, 3, 3));

            return positions;

        }

        internal static SDR_SOM GenerateRandomSDR(iType inputPatternType)
        {
            Random rand = new Random();

            int numPos = rand.Next(1, 10);

            List<Position_SOM> posList = new List<Position_SOM>();

            if (inputPatternType == iType.SPATIAL || inputPatternType == iType.APICAL)
            {
                for (int i = 0; i < numPos; i++)
                {
                    var pos = new Position_SOM(rand.Next(0, 10), rand.Next(0, 10), rand.Next(0, 4));

                    if (posList.Contains(pos) == false)
                    {
                        posList.Add(pos);
                    }
                }
            }
            else if (inputPatternType == iType.TEMPORAL)
            {
                for (int i = 0; i < numPos; i++)
                {
                    var pos = new Position_SOM(rand.Next(0, 10), rand.Next(0, 4), rand.Next(0, 10));

                    if (posList.Contains(pos) == false)
                    {
                        posList.Add(pos);
                    }
                }
            }


            return new SDR_SOM(10, 10, posList, inputPatternType);
        }

        internal static SDR_SOM GenerateSpecificSDRForTemporalWiring(iType inputPatternType, LayerType layerType)
        {

            //For Overlapping Temporal and Spatial Patterns , All that has to be done is to match the y-Coordiante.

            if (layerType == LayerType.Layer_4)
            {
                Random rand = new Random();                

                List<Position_SOM> spatialPosList = new List<Position_SOM>()
                {
                    new Position_SOM(5, 2, 1),
                    new Position_SOM(7, 1, 3),
                    new Position_SOM(8, 2, 3),
                    new Position_SOM(4, 0, 0)
                };

                List<Position_SOM> temporalPosList = new List<Position_SOM>()
                {
                    new Position_SOM(2, 1),
                    new Position_SOM(1, 3),
                    new Position_SOM(2, 3),
                    new Position_SOM(0, 0)
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
            else
            {
                Random rand = new Random();                

                List<Position_SOM> spatialPosList = new List<Position_SOM>()
                {
                    new Position_SOM(55, 2, 1),
                    new Position_SOM(21, 1, 3),
                    new Position_SOM(43, 8, 3),
                    new Position_SOM(51, 9, 0)
                };

                List<Position_SOM> temporalPosList = new List<Position_SOM>()
                {
                    new Position_SOM(2, 1),
                    new Position_SOM(1, 3),
                    new Position_SOM(8, 3),
                    new Position_SOM(9, 0)
                };

                if (inputPatternType == iType.TEMPORAL)
                {
                    return new SDR_SOM(10, 4, temporalPosList, inputPatternType);
                }
                else
                {
                    return new SDR_SOM(1250, 10, spatialPosList, inputPatternType);
                }
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

        internal static SDR_SOM ConvertSpatialToTemporal(List<Position_SOM> possom, LayerType layer)
        {
            List<Position_SOM> posList = new List<Position_SOM>();
            
                foreach(var item in possom) {
                    posList.Add(new Position_SOM(item.Y, item.Z));
                }

            return new SDR_SOM(10, 4, posList, iType.TEMPORAL);
        }

        internal static List<Position_SOM> FindNeuronalPositionThatAreConnectedToTargetNeuron(Neuron targetNeuron, BlockBehaviourManager bbManager)
        {
            List<Position_SOM> posList = new List<Position_SOM>();

            foreach (var column in bbManager.Columns)
            {
                foreach( var neuron in column.Neurons)
                {
                    foreach(var kvp in neuron.AxonalList)
                    {
                        if(kvp.Key == targetNeuron.NeuronID.ToString())
                        {
                            posList.Add(neuron.NeuronID);
                        }
                    }
                }
            }

            return posList;
        }

        internal static List<Neuron> ConvertPosListotNeuronalList(List<Position_SOM> connectedPos, BlockBehaviourManager bbManager)
        {
            List<Neuron> neuronList = new List<Neuron>();

            foreach (var item in connectedPos)
            {
                neuronList.Add(bbManager.GetNeuronFromPosition(item));
            }

            return neuronList;
        }
    }
}
