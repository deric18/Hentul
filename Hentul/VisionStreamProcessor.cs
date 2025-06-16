namespace Hentul
{
    using System.Drawing;
    using Common;
    using Hentul.Encoders;
    using Hentul.Hippocampal_Entorinal_complex;
    using FBBM = FirstOrderMemory.BehaviourManagers.BlockBehaviourManagerFOM;
    using SBBM = SecondOrderMemory.Models.BlockBehaviourManagerSOM;

    public class VisionStreamProcessor
    {

        #region Constructor & Variables

        public int numPixelsProcessedPerBBM;

        public int NumBBMNeededV { get; private set; }

        public int BlockSize;

        public FBBM[] fomBBMV { get; private set; }

        public List<int>  firingFOM_V {  get; private set; }

        public SBBM somBBM_L3B_V { get; private set; }

        public SBBM somBBM_L3A_V { get; private set; }

        public int Range { get; private set; }

        int X, NumColumns, Z;

        bool IsMock;

        public ulong CycleNum;

        public LogMode LogMode { get; private set; }

        public PixelEncoder pEncoder { get; private set; } 

        public Bitmap bmp { get; private set; }


        public VisionStreamProcessor(int range, int numColumns, int x, LogMode logMode, bool isMock, bool shouldInit)
        {
            this.X = x;

            this.NumColumns = numColumns;
            
            IsMock = isMock;

            Z = 5;

            CycleNum = 0;

            BlockSize = (2 * range) * (2 * range); //400            

            //Todo : Project shape data of the input image to one region and project colour data of the image to another region.                        
            if (range != 10)
            {
                throw new InvalidOperationException("Invalid Operation !");
            }

            Range = range;  //10

            NumBBMNeededV = 100;

            pEncoder = new PixelEncoder(NumBBMNeededV, BlockSize);

            bmp = new Bitmap(range + range, range + range);

            firingFOM_V = new List<int>();

            if (NumBBMNeededV != 100)
            {
                throw new InvalidDataException("Number Of FOMM BBMs needed should always be 100, it throws off SOM Schema of 1250" + range.ToString());
            }

            numPixelsProcessedPerBBM = 4;

            NumBBMNeededV = (BlockSize / numPixelsProcessedPerBBM);   //100

            fomBBMV = new FBBM[NumBBMNeededV];

            if (shouldInit)
            {

                for (int i = 0; i < NumBBMNeededV; i++)
                {
                    fomBBMV[i] = new FBBM(NumColumns, NumColumns, Z, LayerType.Layer_4, Common.LogMode.None);
                }

                somBBM_L3A_V = new SBBM(X, NumColumns, Z, LayerType.Layer_3A, Common.LogMode.None);

                somBBM_L3B_V = new SBBM(X, NumColumns, Z, LayerType.Layer_3B, Common.LogMode.None);

            }                        
            
            if(shouldInit) 
                Init();

            LogMode = logMode;
        }

        #endregion

        private void Init()
        {

            Console.WriteLine("Total Number of Pixels :" + (Range * Range * 4).ToString() + "\n");
            Console.WriteLine("Total First Order BBMs Created : " + NumBBMNeededV.ToString() + "\n");

            somBBM_L3A_V.Init(1);

            somBBM_L3B_V.Init(1);

        }

        internal void Process(Bitmap greyScalebmp)
        {
            pEncoder.ParseBitmap(greyScalebmp);

            FireFOMsV();

            if (pEncoder.somPositions.Count != 0)
            {
                if (pEncoder.somPositions.Count > 125)
                {
                    WriteLogsToFile("Layer 3B : SomPosition Write count " + pEncoder.somPositions.Count);
                    bool breakpoint = true;
                }

                // L3B fire
                somBBM_L3B_V.Fire(new SDR_SOM(1250, 10, pEncoder.somPositions, iType.SPATIAL), CycleNum);

                //L3A fire
                SDR_SOM fom_SDR = GetSdrSomFromFOMsV();
                somBBM_L3A_V.Fire(fom_SDR, CycleNum);
            }
            else
            {
                somBBM_L3B_V.FireBlank(CycleNum);
                somBBM_L3A_V.FireBlank(CycleNum);
            }

            pEncoder.Clean();
            firingFOM_V.Clear();
        }

        internal SDR_SOM GetS3BLatestFiringCells(ulong cyclenum) =>        
             somBBM_L3B_V.GetAllNeuronsFiringLatestCycle(cyclenum);

        private SDR_SOM GetSdrSomFromFOMsV()
        {
            if (firingFOM_V.Count == 0)
            {
                int exception = 1;
            }

            // Go through all the FOM BBM and get there currently firing Active Positions and prep them for L3A.
            List<Position_SOM> posList = new List<Position_SOM>();

            foreach (var fomID in firingFOM_V)
            {
                posList.AddRange(PixelEncoder.GetSOMEquivalentPositionsofFOM(fomBBMV[fomID].GetAllColumnsBurstingLatestCycle(CycleNum).ActiveBits, fomID));
            }


            if (LogMode == Common.LogMode.BurstOnly)
            {
                int count = 0;

                foreach (var fomID in firingFOM_V)
                {
                    count += fomBBMV[fomID].GetAllNeuronsFiringLatestCycle(CycleNum, false).ActiveBits.Count;
                }

                if (count == fomBBMV.Count() * Z)
                {
                    WriteLogsToFile(" ALL Columns fired for cycle Num :" + CycleNum.ToString());
                }
            }

            if (posList == null || posList.Count == 0)
            {
                throw new NullReferenceException(" FOM BBM returned empty position list ");
            }

            return new SDR_SOM(1250, 10, posList, iType.SPATIAL);
        }

        private void WriteLogsToFile(string v)
        {
            throw new NotImplementedException();
        }

        private void FireFOMsV()
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
                        }
                        break;
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


        internal void PrintBlockVitalVision()
        {
            Console.WriteLine("Enter '1' to see a list of all the Block Usage List :");

            int w = Console.Read();

            if (w == 49)
            {
                ulong totalIncludedCycle = 0;

                for (int i = 0; i < fomBBMV.Count(); i++)
                {
                    if (fomBBMV[i].BBMID != 0)
                        Console.WriteLine(i.ToString() + " :: Block ID : " + fomBBMV[i].PrintBlockDetailsSingleLine() + " | " + "Included Cycle: " + fomBBMV[i].CycleNum.ToString());

                    totalIncludedCycle += fomBBMV[i].CycleNum;

                }

                Console.WriteLine("Total Participated Cycles : " + totalIncludedCycle);
                Console.WriteLine("Orchestrator CycleNum : " + CycleNum.ToString());

                if (totalIncludedCycle != CycleNum)
                {
                    Console.WriteLine("ERROR : Incorrect Cycle Distribution amoung blocks");
                    Thread.Sleep(5000);
                }
            }
        }


        internal void SetNetworkModeToPrediction()
        {
            somBBM_L3B_V.ChangeNetworkModeToPrediction();
            somBBM_L3A_V.ChangeNetworkModeToPrediction();
        }

        internal List<string> GetSupportedLabels() => somBBM_L3B_V.GetSupportedLabels();


        internal void Clean()
        {
            pEncoder.Clean();
            firingFOM_V.Clear();
        }
    }
}
