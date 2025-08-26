namespace SecondOrderMemoryUnitTest
{
    using Common;
    using SecondOrderMemory.Models;
    using static SecondOrderMemory.Models.BlockBehaviourManagerSOM;

    internal static class TestUtils
    {

        public static List<SDR_SOM> GenerateThreeRandomSDRs(int x = 1000, int y = 10, int positionsPerSDR = 10, iType type = iType.SPATIAL)
        {
            var rand = new Random();
            var allUsed = new HashSet<(int, int, int)>();
            var sdrList = new List<SDR_SOM>();

            for (int s = 0; s < 3; s++)
            {
                var positions = new List<Position_SOM>();

                while (positions.Count < positionsPerSDR)
                {
                    int px = rand.Next(1, x + 1);
                    int py = rand.Next(1, y + 1);
                    int pz = 0;

                    var tuple = (px, py, pz);

                    // Ensure uniqueness across all SDRs
                    if (allUsed.Add(tuple))
                    {
                        positions.Add(new Position_SOM(px, py, pz));
                    }
                }

                sdrList.Add(new SDR_SOM(x, y, positions, type));
            }

            return sdrList;
        }

        internal static List<SDR_SOM> GenerateSpepcificSDRsforTestCase2()
        {

            var sdrList = new List<SDR_SOM>();
            int length = 1250;
            int breadth = 10;

            // SDR 1: positions
            var positions1 = new List<Position_SOM>
            {
                new Position_SOM(0, 0, 0),
                new Position_SOM(10, 1, 1)                
            };

            // SDR 2: positions
            var positions2 = new List<Position_SOM>
            {
                new Position_SOM(500, 5, 0),
                new Position_SOM(510, 6, 1)                
            };

            sdrList.Add(new SDR_SOM(length, breadth, positions1, iType.SPATIAL));
            sdrList.Add(new SDR_SOM(length, breadth, positions2, iType.SPATIAL));            

            return sdrList;
        }

        internal static List<SDR_SOM> GenerateSpepcificSDRsForTestCase2()
        {

            var sdrList = new List<SDR_SOM>();
            int length = 1250;
            int breadth = 10;

            // SDR 1: positions
            var positions1 = new List<Position_SOM>
            {
                new Position_SOM(0, 0, 0),
                new Position_SOM(10, 1, 1),
                new Position_SOM(20, 2, 2)
            };

            // SDR 2: positions
            var positions2 = new List<Position_SOM>
            {
                new Position_SOM(500, 5, 0),
                new Position_SOM(510, 6, 1),
                new Position_SOM(520, 7, 2)
            };

            // SDR 3: positions
            var positions3 = new List<Position_SOM>
            {
                new Position_SOM(1000, 0, 0),
                new Position_SOM(1010, 1, 1),
                new Position_SOM(1020, 2, 2)
            };

            sdrList.Add(new SDR_SOM(length, breadth, positions1, iType.SPATIAL));
            sdrList.Add(new SDR_SOM(length, breadth, positions2, iType.SPATIAL));
            sdrList.Add(new SDR_SOM(length, breadth, positions3, iType.SPATIAL));

            return sdrList;
        }

        internal static List<SDR_SOM> GenerateSpepcificSDRsforTestCase3()
        {            

            var sdrList = new List<SDR_SOM>();
            int length = 1250;
            int breadth = 10;

            // SDR 1: positions
            var positions11 = new List<Position_SOM>
            {
                new Position_SOM(0, 0, 0),
                new Position_SOM(10, 1, 1),
                new Position_SOM(20, 2, 2)                
            };

                    
            var positions12 = new List<Position_SOM>
            {
                new Position_SOM(530, 3, 0),
                new Position_SOM(560, 4, 1),
                new Position_SOM(570, 9, 4)
            };

            var positions13 = new List<Position_SOM>
            {                
                new Position_SOM(630, 3, 0),
                new Position_SOM(660, 4, 1),
                new Position_SOM(670, 9, 4)
            };

            var positions14 = new List<Position_SOM>
            {
                new Position_SOM(880, 0, 0),
                new Position_SOM(840, 1, 1),
                new Position_SOM(888, 2, 2)                
            };


            // SDR 2: positions
            var positions21 = new List<Position_SOM>
            {
                new Position_SOM(0, 0, 0),
                new Position_SOM(10, 1, 1),
                new Position_SOM(20, 2, 2)
            };

            var positions22 = new List<Position_SOM>
            {
                new Position_SOM(530, 3, 0),
                new Position_SOM(560, 4, 1),
                new Position_SOM(570, 9, 4)
            };

            var positions23 = new List<Position_SOM>
            {
                new Position_SOM(777, 3, 0),
                new Position_SOM(720, 4, 1),
                new Position_SOM(740, 9, 4)
            };

            var positions24 = new List<Position_SOM>
            {
                new Position_SOM(995, 0, 0),
                new Position_SOM(999, 1, 1),
                new Position_SOM(930, 2, 2)
            };



            sdrList.Add(new SDR_SOM(length, breadth, positions11, iType.SPATIAL));
            sdrList.Add(new SDR_SOM(length, breadth, positions12 , iType.SPATIAL));
            sdrList.Add(new SDR_SOM(length, breadth, positions13, iType.SPATIAL));
            sdrList.Add(new SDR_SOM(length, breadth, positions14, iType.SPATIAL));
            sdrList.Add(new SDR_SOM(length, breadth, positions21, iType.SPATIAL));
            sdrList.Add(new SDR_SOM(length, breadth, positions22, iType.SPATIAL));
            sdrList.Add(new SDR_SOM(length, breadth, positions23, iType.SPATIAL));
            sdrList.Add(new SDR_SOM(length, breadth, positions24, iType.SPATIAL));

            return sdrList;
        }

        internal static List<SDR_SOM> GenerateAlternativeSDRs()
        {
            var sdrList = new List<SDR_SOM>();
            int length = 1250;
            int breadth = 10;

            // SDR 1: alternative positions
            var positions1 = new List<Position_SOM>
            {
                new Position_SOM(10, 1, 0),
                new Position_SOM(150, 2, 1),
                new Position_SOM(275, 3, 2),
                new Position_SOM(425, 4, 3),
                new Position_SOM(575, 5, 4)
            };

            // SDR 2: alternative positions
            var positions2 = new List<Position_SOM>
            {
                new Position_SOM(625, 6, 0),
                new Position_SOM(775, 7, 1),
                new Position_SOM(925, 8, 2),
                new Position_SOM(1075, 9, 3),
                new Position_SOM(1150, 0, 4)
            };

            // SDR 3: alternative positions
            var positions3 = new List<Position_SOM>
            {
                new Position_SOM(1230, 1, 0),
                new Position_SOM(1240, 2, 1),
                new Position_SOM(1245, 3, 2),
                new Position_SOM(1248, 4, 3),
                new Position_SOM(1249, 5, 4)
            };

            sdrList.Add(new SDR_SOM(length, breadth, positions1, iType.SPATIAL));
            sdrList.Add(new SDR_SOM(length, breadth, positions2, iType.SPATIAL));
            sdrList.Add(new SDR_SOM(length, breadth, positions3, iType.SPATIAL));

            return sdrList;
        }

        internal static List<SDR_SOM> GetSDRsForTestCase4()
        {
            int length = 1250;
            int breadth = 10;

            // Shared SDRs for all objects (first two SDRs)
            var sharedPositions1 = new List<Position_SOM>
            {
                new Position_SOM(10, 1, 0),
                new Position_SOM(20, 2, 1),
                new Position_SOM(30, 3, 2),
                new Position_SOM(40, 4, 3),
                new Position_SOM(50, 5, 4)
            };
            var sharedPositions2 = new List<Position_SOM>
            {
                new Position_SOM(60, 6, 0),
                new Position_SOM(70, 7, 1),
                new Position_SOM(80, 8, 2),
                new Position_SOM(90, 9, 3),
                new Position_SOM(100, 0, 4)
            };

            // Object 1 unique SDRs for positions 3, 4, 5
            var obj1Positions3 = new List<Position_SOM>
            {
                new Position_SOM(101, 1, 0),
                new Position_SOM(102, 2, 1),
                new Position_SOM(103, 3, 2),
                new Position_SOM(104, 4, 3),
                new Position_SOM(105, 5, 4)
            };
            var obj1Positions4 = new List<Position_SOM>
            {
                new Position_SOM(111, 1, 0),
                new Position_SOM(112, 2, 1),
                new Position_SOM(113, 3, 2),
                new Position_SOM(114, 4, 3),
                new Position_SOM(115, 5, 4)
            };
            var obj1Positions5 = new List<Position_SOM>
            {
                new Position_SOM(121, 1, 0),
                new Position_SOM(122, 2, 1),
                new Position_SOM(123, 3, 2),
                new Position_SOM(124, 4, 3),
                new Position_SOM(125, 5, 4)
            };

            // Objects 2 and 3 share SDRs for positions 3 and 4
            var sharedObj2Obj3Positions3 = new List<Position_SOM>
            {
                new Position_SOM(210, 6, 0),
                new Position_SOM(220, 7, 1),
                new Position_SOM(230, 8, 2),
                new Position_SOM(240, 9, 3),
                new Position_SOM(250, 0, 4)
            };
            var sharedObj2Obj3Positions4 = new List<Position_SOM>
            {
                new Position_SOM(260, 6, 0),
                new Position_SOM(270, 7, 1),
                new Position_SOM(280, 8, 2),
                new Position_SOM(290, 9, 3),
                new Position_SOM(300, 0, 4)
            };

            // Unique SDRs for position 5 for objects 2 and 3
            var obj2Positions5 = new List<Position_SOM>
            {
                new Position_SOM(311, 6, 0),
                new Position_SOM(312, 7, 1),
                new Position_SOM(313, 8, 2),
                new Position_SOM(314, 9, 3),
                new Position_SOM(315, 0, 4)
            };
            var obj3Positions5 = new List<Position_SOM>
            {
                new Position_SOM(321, 6, 0),
                new Position_SOM(322, 7, 1),
                new Position_SOM(323, 8, 2),
                new Position_SOM(324, 9, 3),
                new Position_SOM(325, 0, 4)
            };

            var sdrList = new List<SDR_SOM>();

            // Object 1
            sdrList.Add(new SDR_SOM(length, breadth, sharedPositions1, iType.SPATIAL));
            sdrList.Add(new SDR_SOM(length, breadth, sharedPositions2, iType.SPATIAL));
            sdrList.Add(new SDR_SOM(length, breadth, obj1Positions3, iType.SPATIAL));
            sdrList.Add(new SDR_SOM(length, breadth, obj1Positions4, iType.SPATIAL));
            sdrList.Add(new SDR_SOM(length, breadth, obj1Positions5, iType.SPATIAL));

            // Object 2
            sdrList.Add(new SDR_SOM(length, breadth, sharedPositions1, iType.SPATIAL));
            sdrList.Add(new SDR_SOM(length, breadth, sharedPositions2, iType.SPATIAL));
            sdrList.Add(new SDR_SOM(length, breadth, sharedObj2Obj3Positions3, iType.SPATIAL));
            sdrList.Add(new SDR_SOM(length, breadth, sharedObj2Obj3Positions4, iType.SPATIAL));
            sdrList.Add(new SDR_SOM(length, breadth, obj2Positions5, iType.SPATIAL));

            // Object 3
            sdrList.Add(new SDR_SOM(length, breadth, sharedPositions1, iType.SPATIAL));
            sdrList.Add(new SDR_SOM(length, breadth, sharedPositions2, iType.SPATIAL));
            sdrList.Add(new SDR_SOM(length, breadth, sharedObj2Obj3Positions3, iType.SPATIAL));
            sdrList.Add(new SDR_SOM(length, breadth, sharedObj2Obj3Positions4, iType.SPATIAL));
            sdrList.Add(new SDR_SOM(length, breadth, obj3Positions5, iType.SPATIAL));

            return sdrList;
        }

        public static Neuron GetNeuronFromString(string posString, BlockBehaviourManagerSOM bbManager)
        {
            var parts = posString.Split('-');
            int x = Convert.ToInt32(parts[0]);
            int y = Convert.ToInt32(parts[1]);
            int z = Convert.ToInt32(parts[2]);
            char nType = 'N';

            if (parts.Length == 4)
            {
                nType = Convert.ToChar(parts[3]);
            }
            

            if (x > bbManager.X || y > bbManager.Y || z > bbManager.Z)
            {
                int breakpoint = 1;

                throw new NullReferenceException("ConvertStringPosToNeuron : Couldnt Find the neuron in the columns  posString :  " + posString);
            }

            return GetNeuronFromPosition(nType, x, y, z, bbManager);
        }

        public static Neuron GetNeuronFromPosition(char w, int x, int y, int z, BlockBehaviourManagerSOM bbM)
        {
            Neuron toReturn = null;

            if (w == 'N' || w == 'n')
            {
                toReturn = bbM.Columns[x, y].Neurons[z];
            }
            else if (w == 'T' || w == 't')
            {
                toReturn = bbM.TemporalLineArray[y, z];
            }
            else if (w == 'A' || w == 'a')
            {
                toReturn = bbM.ApicalLineArray[x, y];
            }
            else
            {
                throw new InvalidOperationException(" GetNeuronFromPosition :: Your Column structure is messed up!!!");
            }

            return toReturn;
        }

        internal static SDR_SOM AddOffsetToSDR(SDR_SOM sdr, int offset)
        {
            List<Position_SOM> posList = new List<Position_SOM>();

            foreach (var pos in sdr.ActiveBits)
            {
                if (pos.X + (offset * 100) > 1250)
                {
                    throw new InvalidOperationException("Added offset could not exceed network Layer Length!");
                }

                posList.Add(new Position_SOM(pos.X + offset * 100, pos.Y, 0));
            }

            return new SDR_SOM(sdr.Length, sdr.Breadth, posList, sdr.InputPatternType);
        }


        internal static SDR_SOM GetEmptySpatialPattern(List<Position_SOM> lastFirsingCells, int length, int breadth, iType iType, LayerType layerType) =>
            new SDR_SOM(length, breadth, GetExclusivePositionList(lastFirsingCells, layerType), iType);


        public static List<Neuron> ConvertStringListotNeuronalList(List<string> stringList, BlockBehaviourManagerSOM bbManager)
        {
            List<Neuron> neurons = new List<Neuron>();

            foreach (var str in stringList)
            {
                Neuron n = bbManager.GetNeuronFromString(str);

                if (n.nType == NeuronType.NORMAL)
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
                if (pos.W == 'N')
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

                if (excludeX.Contains(x) == false && excludeY.Contains(y) == false)
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

        internal static Neuron GetSpatialNeuronFromTemporalCoordinate(BlockBehaviourManagerSOM bbManager, Position_SOM pos) =>
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

                    if (posList.Any(p => p.X == pos.X && p.Y == pos.Y) == false)
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

                    if (posList.Any(p => p.X == pos.X && p.Y == pos.Y) == false)
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

            foreach (var item in possom)
            {
                posList.Add(new Position_SOM(item.Y, item.Z));
            }

            return new SDR_SOM(10, 4, posList, iType.TEMPORAL);
        }

        internal static List<Position_SOM> FindNeuronalPositionThatAreConnectedToTargetNeuron(Neuron targetNeuron, BlockBehaviourManagerSOM bbManager)
        {
            List<Position_SOM> posList = new List<Position_SOM>();

            foreach (var column in bbManager.Columns)
            {
                foreach (var neuron in column.Neurons)
                {
                    foreach (var kvp in neuron.AxonalList)
                    {
                        if (kvp.Key == targetNeuron.NeuronID.ToString())
                        {
                            posList.Add(neuron.NeuronID);
                        }
                    }
                }
            }

            return posList;
        }

        internal static List<Neuron> ConvertPosListotNeuronalList(List<Position_SOM> connectedPos, BlockBehaviourManagerSOM bbManager)
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
