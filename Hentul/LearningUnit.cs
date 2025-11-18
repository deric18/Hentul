namespace Hentul
{
    using System.Collections.Generic;
    using Common;
    using Encoders;
    using SecondOrderMemory.Models;
    using FBBM = FirstOrderMemory.BehaviourManagers.BlockBehaviourManagerFOM;
    using SBBM = SecondOrderMemory.Models.BlockBehaviourManagerSOM;

    public class LearningUnit
    {
        public FBBM[] fomBBMV { get; private set; }

        public List<int> firingFOM_V { get; private set; }

        public SBBM somBBM_L3B_V { get; private set; }

        public SBBM somBBM_L3A_V { get; private set; }

        private string logfileName;

        public ulong CycleNum { get; private set;  }

        public LogMode logMode { get; private set; }

        private int NumColumns { get; set; }

        private int X { get; set; }

        private int Z { get; set; }

        public LearningUnitType LType { get; private set; }

        public LearningUnit(int numBBMNeededV, int numColumns, int z, int x, bool shouldInit, string logfileName, LearningUnitType lType, LogMode lMode = LogMode.BurstOnly)
        {
            fomBBMV = new FBBM[numBBMNeededV];
            firingFOM_V = new List<int>();
            NumColumns = numColumns;

            // Scale X parameter based on learning unit type for different SOM sizes
            switch (lType)
            {
                case LearningUnitType.V1:
                    X = x;          // 1250 (original)
                    break;
                case LearningUnitType.V2:
                    X = x * 5;      // 6250 (5x wider for 100x100 region)
                    break;
                case LearningUnitType.V3:
                    X = x * 10;     // 12500 (10x wider for 200x200 region)
                    break;
                default:
                    X = x;
                    break;
            }

            Z = z;

            if (shouldInit)
            {
                for (int i = 0; i < numBBMNeededV; i++)
                {
                    fomBBMV[i] = new FBBM(numColumns, numColumns, z, LayerType.Layer_4, Common.LogMode.None);
                }

                // Initialize SOM layers with scaled X parameter for different region sizes
                BlockBehaviourManagerSOM.Initialize(X, numColumns, z, LayerType.Layer_3A, LogMode.None);
                somBBM_L3A_V = SBBM.Instance;

                BlockBehaviourManagerSOM.Initialize(X, numColumns, z, LayerType.Layer_3B, LogMode.None);
                somBBM_L3B_V = SBBM.Instance;
            }

            this.logfileName = logfileName;
            this.logMode = lMode;
            this.LType = lType;
        }

        public void Init()
        {
            for (int i = 0; i < fomBBMV.Length; i++)
            {
                fomBBMV[i].Init(i);
            }

            somBBM_L3A_V.Init(1);
            somBBM_L3B_V.Init(1);

            CycleNum = 0;
        }


        internal void LearnNewObject(string objectName)
        {
            somBBM_L3B_V.ChangeCurrentObjectLabel(objectName);            
        }


        // Supports both Training and Prediction Mode
        public void Process(PixelEncoder pEncoder, ulong cycleNum)
        {
            CycleNum = cycleNum;

            FireFOMsV(pEncoder, cycleNum);

            if (pEncoder.SomPositions.Count != 0)
            {
                // Dynamic threshold based on learning unit type
                int expectedMaxPositions = LType switch
                {
                    LearningUnitType.V1 => 125,    // Original threshold for 20x20
                    LearningUnitType.V2 => 625,    // 5x more for 100x100 (125 * 5)
                    LearningUnitType.V3 => 1250,   // 10x more for 200x200 (125 * 10)
                    _ => 125
                };

                if (pEncoder.SomPositions.Count > expectedMaxPositions)
                {
                    WriteLogsToFile($"Layer 3B {LType}: SomPosition Write count " + pEncoder.SomPositions.Count);
                    bool breakpoint = true;
                }

                // L3B fire with dynamic SOM dimensions based on learning unit type
                int somWidth = LType switch
                {
                    LearningUnitType.V1 => 1250,   // Original width for V1
                    LearningUnitType.V2 => 6250,   // 5x wider for V2 
                    LearningUnitType.V3 => 12500,  // 10x wider for V3
                    _ => 1250
                };

                somBBM_L3B_V.Fire(new SDR_SOM(somWidth, 10, pEncoder.SomPositions, iType.SPATIAL), cycleNum);

                // L3A fire (commented out in original, but would need same scaling)
                //SDR_SOM fom_SDR = GetSdrSomFromFOMsV();
                //somBBM_L3A_V.Fire(fom_SDR, cycleNum);
            }
            else
            {
                somBBM_L3B_V.FireBlank(cycleNum);
                somBBM_L3A_V.FireBlank(cycleNum);
            }
        }
        // Go through all the FOM BBM and get there currently firing Active Positions and prep them for L3A.
        //private SDR_SOM GetSdrSomFromFOMsV()
        //{

        //    List<Position_SOM> posList = new List<Position_SOM>();

        //    foreach (var fomID in firingFOM_V)
        //    {
        //        posList.AddRange(PixelEncoder.GetSOMEquivalentPositionsofFOM(fomBBMV[fomID].GetAllColumnsBurstingLatestCycle(CycleNum).ActiveBits, fomID));
        //    }

        //    if (logMode == Common.LogMode.BurstOnly)
        //    {
        //        int count = 0;

        //        foreach (var fomID in firingFOM_V)
        //        {
        //            count += fomBBMV[fomID].GetAllNeuronsFiringLatestCycle(CycleNum, false).ActiveBits.Count;
        //        }

        //        if (count == fomBBMV.Count() * Z)
        //        {
        //            WriteLogsToFile(" ALL Columns fired for cycle Num :" + CycleNum.ToString());
        //        }
        //    }

        //    if (posList == null || posList.Count == 0)
        //    {
        //        throw new NullReferenceException(" FOM BBM returned empty position list ");
        //    }

        //    return new SDR_SOM(1250, 10, posList, iType.SPATIAL);
        //}


        // Should be called after pixelEncoder has processed the bitmap, parses all the FOMBBMIDS and fires FOMS and tracks all the firing FOMs into firingFOM


        private void FireFOMsV(PixelEncoder pEncoder, ulong CycleNum)
        {
            foreach (var kvp in pEncoder.FOMBBMIDS)
            {
                switch (kvp.Key)
                {
                    case MAPPERCASE.ALL:
                    {
                        foreach (var bbmID in kvp.Value)
                        {
                            var poses = pEncoder.GetSDR_SOMForMapperCase(MAPPERCASE.ALL, bbmID);
                            fomBBMV[bbmID].Fire(poses, CycleNum);
                            firingFOM_V.Add(bbmID);
                        }
                    }
                        break;
                    case MAPPERCASE.ONETWOTHREEE:
                    {
                        foreach (var bbmID in kvp.Value)
                        {
                            var poses = pEncoder.GetSDR_SOMForMapperCase(MAPPERCASE.ONETWOTHREEE, bbmID);
                            fomBBMV[bbmID].Fire(poses, CycleNum);
                            firingFOM_V.Add(bbmID);
                        }
                    }
                        break;
                    case MAPPERCASE.TWOTHREEFOUR:
                    {

                        foreach (var bbmID in kvp.Value)
                        {
                            var poses = pEncoder.GetSDR_SOMForMapperCase(MAPPERCASE.TWOTHREEFOUR, bbmID);
                            fomBBMV[bbmID].Fire(poses, CycleNum);
                            firingFOM_V.Add(bbmID);
                        }
                    }
                        break;
                    case MAPPERCASE.ONETWOFOUR:
                    {
                        foreach (var bbmID in kvp.Value)
                        {
                            var poses = pEncoder.GetSDR_SOMForMapperCase(MAPPERCASE.ONETWOFOUR, bbmID);
                            fomBBMV[bbmID].Fire(poses, CycleNum);
                            firingFOM_V.Add(bbmID);
                        }
                    }
                        break;
                    case MAPPERCASE.ONETHREEFOUR:
                    {
                        foreach (var bbmID in kvp.Value)
                        {
                            var poses = pEncoder.GetSDR_SOMForMapperCase(MAPPERCASE.ONETHREEFOUR, bbmID);
                            fomBBMV[bbmID].Fire(poses, CycleNum);
                            firingFOM_V.Add(bbmID);
                        }
                    }
                        break;
                    case MAPPERCASE.ONETWO:
                    {
                        foreach (var bbmID in kvp.Value)
                        {
                            var poses = pEncoder.GetSDR_SOMForMapperCase(MAPPERCASE.ONETWO, bbmID);
                            fomBBMV[bbmID].Fire(poses, CycleNum);
                            firingFOM_V.Add(bbmID);
                        }
                            break;
                    }                        
                    case MAPPERCASE.ONETHREE:
                    {
                        foreach (var bbmID in kvp.Value)
                        {
                            var poses = pEncoder.GetSDR_SOMForMapperCase(MAPPERCASE.ONETHREE, bbmID);
                            fomBBMV[bbmID].Fire(poses, CycleNum);
                            firingFOM_V.Add(bbmID);
                        }
                    }
                        break;
                    case MAPPERCASE.ONEFOUR:
                    {
                        foreach (var bbmID in kvp.Value)
                        {
                            var poses = pEncoder.GetSDR_SOMForMapperCase(MAPPERCASE.ONEFOUR, bbmID);
                            fomBBMV[bbmID].Fire(poses, CycleNum);
                            firingFOM_V.Add(bbmID);
                        }
                    }
                        break;
                    case MAPPERCASE.TWOTHREE:
                    {
                        foreach (var bbmID in kvp.Value)
                        {
                            var poses = pEncoder.GetSDR_SOMForMapperCase(MAPPERCASE.TWOTHREE, bbmID);
                            fomBBMV[bbmID].Fire(poses, CycleNum);
                            firingFOM_V.Add(bbmID);
                        }
                    }
                        break;
                    case MAPPERCASE.TWOFOUR:
                    {
                        foreach (var bbmID in kvp.Value)
                        {
                            var poses = pEncoder.GetSDR_SOMForMapperCase(MAPPERCASE.TWOFOUR, bbmID);
                            fomBBMV[bbmID].Fire(poses, CycleNum);
                            firingFOM_V.Add(bbmID);
                        }
                    }
                        break;
                    case MAPPERCASE.THREEFOUR:
                    {
                        foreach (var bbmID in kvp.Value)
                        {
                            var poses = pEncoder.GetSDR_SOMForMapperCase(MAPPERCASE.THREEFOUR, bbmID);
                            fomBBMV[bbmID].Fire(poses, CycleNum);
                            firingFOM_V.Add(bbmID);
                        }
                    }
                        break;
                    case MAPPERCASE.ONE:
                    {
                        foreach (var bbmID in kvp.Value)
                        {
                            var poses = pEncoder.GetSDR_SOMForMapperCase(MAPPERCASE.ONE, bbmID);
                            fomBBMV[bbmID].Fire(poses, CycleNum);
                            firingFOM_V.Add(bbmID);
                        }
                    }
                        break;
                    case MAPPERCASE.TWO:
                    {
                        foreach (var bbmID in kvp.Value)
                        {
                            var poses = pEncoder.GetSDR_SOMForMapperCase(MAPPERCASE.TWO, bbmID);
                            fomBBMV[bbmID].Fire(poses, CycleNum);
                            firingFOM_V.Add(bbmID);
                        }
                    }
                        break;
                    case MAPPERCASE.THREE:
                    {
                        foreach (var bbmID in kvp.Value)
                        {
                            var poses = pEncoder.GetSDR_SOMForMapperCase(MAPPERCASE.THREE, bbmID);
                            fomBBMV[bbmID].Fire(poses, CycleNum);
                            firingFOM_V.Add(bbmID);
                        }
                    }
                        break;
                    case MAPPERCASE.FOUR:
                    {
                        foreach (var bbmID in kvp.Value)
                        {
                            var poses = pEncoder.GetSDR_SOMForMapperCase(MAPPERCASE.FOUR, bbmID);
                            fomBBMV[bbmID].Fire(poses, CycleNum);
                            firingFOM_V.Add(bbmID);
                        }
                    }
                        break;
                    default:
                    {
                        throw new NotImplementedException();
                    }
                }
            }
        }

        private void WriteLogsToFile(string v)
        {
            File.WriteAllText(logfileName, v);
        }

        internal void Clear()
        {
            firingFOM_V.Clear();
        }

        internal void Restore()
        {
            for (int i = 0; i < fomBBMV.Length; i++)
            {
                fomBBMV[i] = FBBM.Restore(i.ToString(), LayerType.Layer_4);
            }

            somBBM_L3B_V = SBBM.Restore("SOML3B", LayerType.Layer_3B);

            somBBM_L3A_V = SBBM.Restore("SOML3A", LayerType.Layer_3A);
        }

        internal void Backup()
        {
            for (int i = 0; i < fomBBMV.Length; i++)
            {
                fomBBMV[i].BackUp(i.ToString() + ".json");
            }

            somBBM_L3B_V.BackUp("SOML3B.json");

            somBBM_L3A_V.BackUp("SOML3A.json");
        }

        internal void ChangeNetworkModeToPrediction()
        {
            somBBM_L3A_V.ChangeNetworkModeToPrediction();
            somBBM_L3B_V.ChangeNetworkModeToPrediction();
        }

        internal void BeginTraining(string objectLabel)
        {
            somBBM_L3B_V.BeginTraining(objectLabel);
        }
    }
}