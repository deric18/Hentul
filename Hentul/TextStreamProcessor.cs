namespace Hentul
{    
    using Common;
    using Hentul.Encoders;
    using Hentul.Hippocampal_Entorinal_complex;
    using SecondOrderMemory.Models;
    using FBBM = FirstOrderMemory.BehaviourManagers.BlockBehaviourManagerFOM;
    using SBBM = SecondOrderMemory.Models.BlockBehaviourManagerSOM;


    public class TextStreamProcessor
    {

        public FBBM[] fomBBMT { get; private set; }

        public LogMode logMode { get; private set; }

        public SBBM somBBM_L3B { get; private set; }

        public SBBM somBBM_L3A { get; private set; }

        public int NumBBMNeededT { get; private set; }

        int X, NumColumns, Z;

        public CharEncoder cEncoder { get; private set; }

        List<int> firingFOM_T;

        Logger logger;

        public TextStreamProcessor(int numColumns, int z, LogMode logMode, Logger logger)
        {
            NumBBMNeededT = 2; //Number of BBMS needed for processing one character

            fomBBMT = new FBBM[NumBBMNeededT];

            NumColumns = numColumns;

            Z = z;

            for (int i = 0; i < NumBBMNeededT; i++)
            {
                fomBBMT[i] = new FBBM(NumColumns, NumColumns, Z, LayerType.Layer_4, Common.LogMode.None);
            }

            BlockBehaviourManagerSOM.Initialize(200, NumColumns, Z, LayerType.Layer_3A, Common.LogMode.None);

            somBBM_L3A = SBBM.Instance;

            BlockBehaviourManagerSOM.Initialize(200, NumColumns, Z, LayerType.Layer_3B, Common.LogMode.None);

            somBBM_L3B = SBBM.Instance;

            cEncoder = new CharEncoder();

            this.logMode = logMode;

            firingFOM_T = new();

            this.logger = logger;

            Init();
        }

        private void Init()
        {
            for (int i = 0; i < NumBBMNeededT; i++)
            {
                fomBBMT[i].Init(i);
            }

            somBBM_L3A.Init(0);

            somBBM_L3B.Init(0);
        }

        /// Fires L4 and L3B with the same input and output of L4 -> L3A
        public void ProcessCharacter(char ch, ulong cycleNum)
        {

            if (ch < 65 || ch > 90)
            {
                throw new InvalidOperationException("Character is not a valid Upper Case Letter!");
            }

            cEncoder.Encode(ch);

            FireFOMsT(cycleNum);

            if (cEncoder.somPositions.Count != 0)
            {
                if (cEncoder.somPositions.Count > 2)
                    logger.WriteLogsToFile("Layer 3B : SomPosition Write count " + cEncoder.somPositions.Count);

                // L3B fire
                somBBM_L3B.Fire(new SDR_SOM(200, 10, cEncoder.somPositions, iType.SPATIAL), cycleNum);

                // L3A fire
                SDR_SOM fom_SDR = GetSdrSomFromFOMsT(cycleNum);

                if (fom_SDR.ActiveBits.Count == 0)
                    throw new InvalidOperationException("L4 returned empty SDR for L3A!");

                somBBM_L3A.Fire(fom_SDR, cycleNum);
            }
            else
            {
                somBBM_L3B.FireBlank(cycleNum);
                somBBM_L3A.FireBlank(cycleNum);
            }

            cEncoder.Clean();
            firingFOM_T.Clear();
        }

        public SDR_SOM GetL3BSensation(ulong cycleNum)
        {
            return somBBM_L3B.GetAllNeuronsFiringLatestCycle(cycleNum);
        }

        private SDR_SOM GetSdrSomFromFOMsT(ulong CycleNum)
        {
            if (firingFOM_T.Count == 0)
            {
                int exception = 1;
            }

            // Go through all the FOM BBM and get there currently firing Active Positions and prep them for L3A.
            List<Position_SOM> posList = new List<Position_SOM>();

            foreach (var fomID in firingFOM_T)
            {
                posList.AddRange(
                    CharEncoder.GetSOMEquivalentPositionsofFOM(fomBBMT[fomID].GetAllColumnsBurstingLatestCycle(CycleNum).ActiveBits, fomID));
            }

            if (logMode == Common.LogMode.BurstOnly)
            {
                int count = 0;

                foreach (var fomID in firingFOM_T)
                {
                    count += fomBBMT[fomID].GetAllNeuronsFiringLatestCycle(CycleNum, false).ActiveBits.Count;
                }

                if (count == fomBBMT.Count() * Z)
                {
                    logger.WriteLogsToFile(" ALL Columns fired for cycle Num :" + CycleNum.ToString());
                }
            }

            if (posList.Count != 0)
            {
                return new SDR_SOM(200, 10, posList, iType.SPATIAL);
            }

            throw new InvalidOperationException(" FOM BBM returned empty position list ");
        }


        private void FireFOMsT(ulong CycleNum)
        {
            if (cEncoder.FOMBBMIDS.Count == 0)
                throw new InvalidOperationException("Orchestrator :: FireFOMsT :: Encoding Error :: FOMBBMIDs cannot be empty!");

            int bbmID = 0;

            foreach (var kvp in cEncoder.FOMBBMIDS)
            {
                bbmID = kvp.Key;

                if (bbmID >= NumBBMNeededT)
                    throw new InvalidOperationException("FOM BBM ID cannot exceed more than 1!");

                switch (kvp.Value)
                {
                    case MAPPERCASE.ALL:
                        {
                            var poses = cEncoder.GenerateSDR_SOMForMapperCase(MAPPERCASE.ALL, bbmID);
                            fomBBMT[bbmID].Fire(poses, CycleNum);
                            firingFOM_T.Add(bbmID);
                        }
                        break;
                    case MAPPERCASE.ONETWOTHREEE:
                        {
                            var poses = cEncoder.GenerateSDR_SOMForMapperCase(MAPPERCASE.ONETWOTHREEE, bbmID);
                            fomBBMT[bbmID].Fire(poses, CycleNum);
                            firingFOM_T.Add(bbmID);
                        }
                        break;
                    case MAPPERCASE.TWOTHREEFOUR:
                        {
                            var poses = cEncoder.GenerateSDR_SOMForMapperCase(MAPPERCASE.TWOTHREEFOUR, bbmID);
                            fomBBMT[bbmID].Fire(poses, CycleNum);
                            firingFOM_T.Add(bbmID);
                        }
                        break;
                    case MAPPERCASE.ONETWOFOUR:
                        {
                            var poses = cEncoder.GenerateSDR_SOMForMapperCase(MAPPERCASE.ONETWOFOUR, bbmID);
                            fomBBMT[bbmID].Fire(poses, CycleNum);
                            firingFOM_T.Add(bbmID);
                        }
                        break;
                    case MAPPERCASE.ONETHREEFOUR:
                        {
                            var poses = cEncoder.GenerateSDR_SOMForMapperCase(MAPPERCASE.ONETHREEFOUR, bbmID);
                            fomBBMT[bbmID].Fire(poses, CycleNum);
                            firingFOM_T.Add(bbmID);
                        }
                        break;
                    case MAPPERCASE.ONETWO:
                        {
                            var poses = cEncoder.GenerateSDR_SOMForMapperCase(MAPPERCASE.ONETWO, bbmID);
                            fomBBMT[bbmID].Fire(poses, CycleNum);
                            firingFOM_T.Add(bbmID);
                        }
                        break;
                    case MAPPERCASE.ONETHREE:
                        {
                            var poses = cEncoder.GenerateSDR_SOMForMapperCase(MAPPERCASE.ONETHREE, bbmID);
                            fomBBMT[bbmID].Fire(poses, CycleNum);
                            firingFOM_T.Add(bbmID);
                        }
                        break;
                    case MAPPERCASE.ONEFOUR:
                        {
                            var poses = cEncoder.GenerateSDR_SOMForMapperCase(MAPPERCASE.ONEFOUR, bbmID);
                            fomBBMT[bbmID].Fire(poses, CycleNum);
                            firingFOM_T.Add(bbmID);
                        }
                        break;
                    case MAPPERCASE.TWOTHREE:
                        {
                            var poses = cEncoder.GenerateSDR_SOMForMapperCase(MAPPERCASE.TWOTHREE, bbmID);
                            fomBBMT[bbmID].Fire(poses, CycleNum);
                            firingFOM_T.Add(bbmID);
                        }
                        break;
                    case MAPPERCASE.TWOFOUR:
                        {
                            var poses = cEncoder.GenerateSDR_SOMForMapperCase(MAPPERCASE.TWOFOUR, bbmID);
                            fomBBMT[bbmID].Fire(poses, CycleNum);
                            firingFOM_T.Add(bbmID);
                        }
                        break;
                    case MAPPERCASE.THREEFOUR:
                        {
                            var poses = cEncoder.GenerateSDR_SOMForMapperCase(MAPPERCASE.THREEFOUR, bbmID);
                            fomBBMT[bbmID].Fire(poses, CycleNum);
                            firingFOM_T.Add(bbmID);
                        }
                        break;
                    case MAPPERCASE.ONE:
                        {
                            var poses = cEncoder.GenerateSDR_SOMForMapperCase(MAPPERCASE.ONE, bbmID);
                            fomBBMT[bbmID].Fire(poses, CycleNum);
                            firingFOM_T.Add(bbmID);
                        }
                        break;
                    case MAPPERCASE.TWO:
                        {
                            var poses = cEncoder.GenerateSDR_SOMForMapperCase(MAPPERCASE.TWO, bbmID);
                            fomBBMT[bbmID].Fire(poses, CycleNum);
                            firingFOM_T.Add(bbmID);
                        }
                        break;
                    case MAPPERCASE.THREE:
                        {
                            var poses = cEncoder.GenerateSDR_SOMForMapperCase(MAPPERCASE.THREE, bbmID);
                            fomBBMT[bbmID].Fire(poses, CycleNum);
                            firingFOM_T.Add(bbmID);
                        }
                        break;
                    case MAPPERCASE.FOUR:
                        {
                            var poses = cEncoder.GenerateSDR_SOMForMapperCase(MAPPERCASE.FOUR, bbmID);
                            fomBBMT[bbmID].Fire(poses, CycleNum);
                            firingFOM_T.Add(bbmID);
                        }
                        break;
                    default:
                        {
                            throw new InvalidOperationException("Invalid Mapper Case!!");
                        }
                }
            }
        }

        internal Sensation ConvertSDR_To_Sensation(SDR_SOM som_SDR) => cEncoder.GetSenseiFromSDR_T(som_SDR);

        internal string GetPrediction()
        {            

            var res = somBBM_L3B.GetCurrentPredictions();
            
            if(res.Count > 1)
            {
                logger.WriteLogsToFile("TextStreamProcessor should not return more than one string!");
            }

            return res[0];
        }
    }
}
