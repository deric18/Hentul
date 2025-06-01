namespace Hentul
{
    using Common;
    using System.Runtime.InteropServices;
    using System;
    using Hentul.Hippocampal_Entorinal_complex;
    using System.Drawing.Imaging;
    using System.Drawing;
    using FBBM = FirstOrderMemory.BehaviourManagers.BlockBehaviourManagerFOM;
    using SBBM = SecondOrderMemory.Models.BlockBehaviourManagerSOM;

    public class Orchestrator
    {

        public struct POINT
        {
            public int X;
            public int Y;
        }

        #region DLLImport
        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool GetCursorPos(out POINT lpPoint);
        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr GetDesktopWindow();
        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr GetWindowDC(IntPtr window);
        [DllImport("gdi32.dll", SetLastError = true)]
        public static extern uint GetPixel(IntPtr dc, int x, int y);
        [DllImport("user32.dll", SetLastError = true)]
        public static extern int ReleaseDC(IntPtr window, IntPtr dc);
        [DllImport("User32.Dll")]
        public static extern long SetCursorPos(int x, int y);
        [DllImport("User32.Dll")]
        public static extern bool ClientToScreen(IntPtr hWnd, ref POINT point);
        #endregion

        #region CONSTRUCTOR 

        #region Used Variables

        private static Orchestrator _orchestrator;

        public int Range { get; private set; }

        public int numPixelsProcessedPerBBM;

        public int NumBBMNeededV { get; private set; }

        public int NumBBMNeededT { get; private set; }

        private bool LogMode { get; set; }

        public bool IsMock { get; private set; }

        int X, NumColumns, Z;

        public FBBM[] fomBBMV { get; private set; }

        public SBBM somBBM_L3B_V { get; private set; }

        public SBBM somBBM_L3A_V { get; private set; }

        public FBBM[] fomBBMT { get; private set; }

        public SBBM somBBM_L3B_T { get; private set; }

        public SBBM somBBM_L3A_T { get; private set; }


        public HippocampalComplex HCAccessor { get; private set; }

        public LocationEncoder locationEncoder { get; private set; }

        public int[] MockBlockNumFires { get; private set; }

        private bool devbox = false;

        public int BlockSize;

        public int ImageIndex { get; private set; }

        public ulong CycleNum;

        public POINT point;

        public List<string> ImageList { get; private set; }

        public Bitmap bmp;

        public string filename;

        public string logfilename;

        public LogMode logMode;

        private List<string> objectlabellist { get; set; }

        private int imageIndex { get; set; }

        public NetworkMode NMode { get; set; }

        List<int> firingFOM_V;

        List<int> firingFOM_T;

        public PixelEncoder pEncoder { get; private set; }

        public CharEncoder cEncoder { get; private set; }

        #endregion

        private Orchestrator(int range, bool isMock = false, bool ShouldInit = true, NetworkMode nMode = NetworkMode.TRAINING, int mockImageIndex = 7)
        {
            //Todo : Project shape data of the input image to one region and project colour data of the image to another region.                        
            if (range != 10)
            {
                throw new InvalidOperationException("Invalid Operation !");
            }

            Range = range;  //10

            bmp = new Bitmap(range + range, range + range);

            numPixelsProcessedPerBBM = 4;

            LogMode = false;

            BlockSize = (2 * range) * (2 * range); //400

            NumBBMNeededV = (BlockSize / numPixelsProcessedPerBBM);   //100

            NumBBMNeededT = 2; //

            bool b = NumBBMNeededV % 1 != 0;

            if (NumBBMNeededV != 100)
            {
                throw new InvalidDataException("Number Of FOMM BBMs needed should always be 100, it throws off SOM Schema of 1250" + range.ToString());
            }

            fomBBMV = new FBBM[NumBBMNeededV];

            X = 1250;

            NumColumns = 10;

            IsMock = isMock;

            Z = 5;

            CycleNum = 0;

            NMode = nMode;

            if (ShouldInit)
            {

                for (int i = 0; i < NumBBMNeededV; i++)
                {
                    fomBBMV[i] = new FBBM(NumColumns, NumColumns, Z, LayerType.Layer_4, Common.LogMode.None);
                }

                if (isMock)
                    ImageIndex = mockImageIndex;
                else
                    ImageIndex = 0;

                somBBM_L3A_V = new SBBM(X, NumColumns, Z, LayerType.Layer_3A, Common.LogMode.None);

                somBBM_L3B_V = new SBBM(X, NumColumns, Z, LayerType.Layer_3B, Common.LogMode.None);

                somBBM_L3A_T = new SBBM(200, NumColumns, Z, LayerType.Layer_3A, Common.LogMode.None);

                somBBM_L3B_T = new SBBM(200, NumColumns, Z, LayerType.Layer_3B, Common.LogMode.None);

                Init();
            }

            locationEncoder = new LocationEncoder(iType.TEMPORAL);

            HCAccessor = new HippocampalComplex("Apple", isMock, nMode);

            objectlabellist = new List<string>
            {
                "Apple",
                "Ananas",
                "Watermelon",
                "JackFruit",
                "Grapes"
            };

            imageIndex = 1;

            MockBlockNumFires = new int[NumBBMNeededV];

            firingFOM_V = new List<int>();

            pEncoder = new PixelEncoder(NumBBMNeededV, BlockSize);

            cEncoder = new CharEncoder();

            filename = "C:\\Users\\depint\\source\\repos\\Hentul\\Hentul\\Images\\savedImage.png";

            logfilename = "C:\\Users\\depint\\source\\Logs\\Hentul-Orchestrator.log";

            logMode = Common.LogMode.BurstOnly;

        }

        public static Orchestrator GetInstance(bool isMock = false, bool shouldInit = true, NetworkMode nMode = NetworkMode.TRAINING)
        {
            if (_orchestrator == null)
            {
                _orchestrator = new Orchestrator(10, isMock, shouldInit, nMode);
            }

            return _orchestrator;
        }

        private void Init()
        {

            Console.WriteLine("Starting Initialization  of FOM objects : \n");


            for (int i = 0; i < NumBBMNeededV; i++)
            {
                fomBBMV[i].Init(i);
            }

            for (int i = 0; i < NumBBMNeededT; i++)
            {
                fomBBMT[i].Init(i);
            }


            Console.WriteLine("Finished Init for this Instance \n");
            Console.WriteLine("Range : " + Range.ToString() + "\n");
            Console.WriteLine("Total Number of Pixels :" + (Range * Range * 4).ToString() + "\n");
            Console.WriteLine("Total First Order BBMs Created : " + NumBBMNeededV.ToString() + "\n");


            Console.WriteLine("Initing SOM Instance now ... \n");

            somBBM_L3A_V.Init(1);

            somBBM_L3B_V.Init(1);

            somBBM_L3A_T.Init(1);

            somBBM_L3B_T.Init(1);

            Console.WriteLine("Finished Init for SOM Instance , Total Time ELapsed : \n");

            Console.WriteLine("Finished Initting of all Instances, System Ready!" + "\n");
        }

        #endregion

        #region Public API

        /// Grabs Cursors Current Position and records pixels.        
        public void RecordPixels(bool isMock = false)
        {
            CycleNum++;

            Console.WriteLine("Grabbing cursor Position");

            point = this.GetCurrentPointerPosition();

            Console.WriteLine("Grabbing Screen Pixels...");

            int Range2 = Range + Range;

            int x1 = point.X - Range < 0 ? 0 : point.X - Range;
            int y1 = point.Y - Range < 0 ? 0 : point.Y - Range;
            int x2 = Math.Abs(point.X + Range2);
            int y2 = Math.Abs(point.Y + Range);

            //this.GetColorByRange(x1, y1, x2, y2);

            Rectangle rect = new Rectangle(x1, y1, x2, y2);

            bmp = new Bitmap(Range2 + Range2, Range2, PixelFormat.Format32bppArgb);

            Graphics g = Graphics.FromImage(bmp);

            g.CopyFromScreen(x1, y1, 0, 0, bmp.Size, CopyPixelOperation.SourceCopy);

            if (isMock == false)
                bmp.Save(filename, ImageFormat.Jpeg);
        }

        /// Takes in a bmp and preps and fires all FOM & SOM's.
        public void FireAll_V(Bitmap greyScalebmp)
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
                SDR_SOM fom_SDR = GetSdrSomFromFOMs();
                somBBM_L3A_V.Fire(fom_SDR, CycleNum);
            }
            else
            {
                somBBM_L3B_V.FireBlank(CycleNum);
                somBBM_L3A_V.FireBlank(CycleNum);
            }

            pEncoder.clean();
            firingFOM_V.Clear();
        }

        //Trains the new object on to HC
        public void AddNewVisualSensationToHC()
        {

            if (!NMode.Equals(NetworkMode.TRAINING))
            {
                throw new InvalidOperationException("INVALID State Management!");
            }

            SDR_SOM fom_SDR = GetSdrSomFromFOMs();

            var som_SDR = somBBM_L3B_V.GetAllNeuronsFiringLatestCycle(CycleNum);

            if (som_SDR != null)
            {

                //Wrong : location should be the location of the mouse pointer relative to the image and not just BBMID.
                var firingSensei = pEncoder.GetSensationLocationFromSDR(som_SDR, point);

                if (HCAccessor.AddNewSensationToObject(firingSensei) == false)
                {
                    throw new InvalidOperationException("Could Not Add Object to HC ! Either it was NOT in TRAINING MODE or sensation already exist in the current Object");
                }
            }
        }

        public void AddNewCharacterSensationToHC()
        {
            if (!NMode.Equals(NetworkMode.TRAINING))
            {
                throw new InvalidOperationException("AddNewCharacterSensationToHC_T :: Network Should be in Training Mode before Predicting!");
            }

            FireFOMsT();

            if (cEncoder.somPositions.Count != 0)
            {
                if (cEncoder.somPositions.Count > 125)
                {
                    WriteLogsToFile("Layer 3B : SomPosition Write count " + cEncoder.somPositions.Count);
                    bool breakpoint = true;
                }

                // L3B fire
                somBBM_L3B_T.Fire(new SDR_SOM(200, 10, cEncoder.somPositions, iType.SPATIAL), CycleNum);

                // L3A fire
                SDR_SOM fom_SDR = GetSdrSomFromFOMs();
                somBBM_L3A_T.Fire(fom_SDR, CycleNum);
            }
            else
            {
                somBBM_L3B_T.FireBlank(CycleNum);
                somBBM_L3A_T.FireBlank(CycleNum);
            }

            cEncoder.Clean();
            firingFOM_T.Clear();
        }

        public Position2D Verify_Predict_HC(bool isMock = false, uint iterationsToConfirmation = 10, bool legacyPipeline = true)
        {
            Position2D motorOutput = null;
            List<Position2D> positionToConfirm = new List<Position2D>();

            if (!NMode.Equals(NetworkMode.PREDICTION))
            {
                throw new InvalidOperationException("Invalid State Managemnt!");
            }

            // If any output from HC execute the location output if NOT then take the standar default output.                
            var som_SDR = somBBM_L3B_V.GetAllNeuronsFiringLatestCycle(CycleNum);
            var predictedSDR = somBBM_L3B_V.GetPredictedSDRForNextCycle(CycleNum + 1);


            if (som_SDR != null)
            {
                var firingSensei = pEncoder.GetSensationLocationFromSDR(som_SDR, point);
                var predictedSensei = pEncoder.GetSensationLocationFromSDR(predictedSDR, point);

                List<string> predictedLabels = somBBM_L3B_V.GetSupportedLabels();

                if (legacyPipeline)
                {
                    motorOutput = HCAccessor.VerifyObject(firingSensei, null, isMock, iterationsToConfirmation);
                }
                else
                {
                    if (predictedSensei != null)
                    {
                        var positionsToConfirm = HCAccessor.StoreObjectInGraph(firingSensei, predictedSensei, isMock);
                    }
                    else
                    {
                        throw new InvalidOperationException("Should Not Happen in PRediction Mode");
                    }
                }
            }

            return motorOutput;
        }

        public void DoneWithTraining()
        {
            HCAccessor.DoneWithTraining();
        }

        public void ChangeNetworkToPredictionMode()
        {
            NMode = NetworkMode.PREDICTION;
            HCAccessor.DoneWithTraining();
            HCAccessor.SetNetworkModeToPrediction();
            somBBM_L3B_V.ChangeNetworkModeToPrediction();
            somBBM_L3A_V.ChangeNetworkModeToPrediction();
        }

        //Gets the current SDR and next cycle predited SDR from classifier layer
        internal Tuple<Sensation_Location, Sensation_Location> GetSDRFromL3B()
        {

            Sensation_Location sensei = null, predictedSensei = null;

            var som_SDR = somBBM_L3B_V.GetAllNeuronsFiringLatestCycle(CycleNum);
            var predictedSDR = somBBM_L3B_V.GetPredictedSDRForNextCycle(CycleNum + 1);

            if (som_SDR != null)
            {
                sensei = pEncoder.GetSensationLocationFromSDR(som_SDR, point);
            }

            if (predictedSDR != null)
            {
                predictedSensei = pEncoder.GetSensationLocationFromSDR(predictedSDR, point);
            }

            return new Tuple<Sensation_Location, Sensation_Location>(sensei, predictedSensei);
        }

        public List<uint> StartBurstAvoidanceWandering(int totalWanders = 5)
        {
            //var temporalSignalForPosition = new SDR_SOM(NumColumns, Z, GetLocationSDR(nextDesiredPosition), iType.TEMPORAL);
            //somBBM_L3A.Fire(temporalSignalForPosition);  

            // Object recognised! 
            int counter = totalWanders > 0 ? HCAccessor.GetObjectTotalSensationCount() : totalWanders;
            int breakpoint = 1;

            List<uint> burstCache = new List<uint>();

            while (counter != 0)
            {
                if (counter == 2)
                    breakpoint = 3;

                Position2D nextDesiredPosition = HCAccessor.GetNextLocationForWandering();


                var apicalSignalSOM = new SDR_SOM(X, NumColumns, Conver2DtoSOMList(HCAccessor.GetNextSensationForWanderingPosition()), iType.APICAL);

                MoveCursorToSpecificPosition(nextDesiredPosition.X, nextDesiredPosition.Y);

                RecordPixels();

                var edgedbmp = ConverToEdgedBitmap();

                var apicalSignal = apicalSignalSOM.ActiveBits;

                var apicalSignalFOM = new SDR_SOM(X, NumColumns, apicalSignal, iType.APICAL);               //Fire FOMS with APICAL input

                BiasFOM(apicalSignalFOM);

                ParseNFireBitmap(edgedbmp);

                pEncoder.clean();
                firingFOM_V.Clear();

                uint postBiasBurstCount = GetTotalBurstCountInFOMLayerInLastCycle(CycleNum);

                if (postBiasBurstCount > 0)
                {
                    Dictionary<int, List<Position_SOM>> dict = new Dictionary<int, List<Position_SOM>>();

                    foreach (var fom in fomBBMV)
                    {
                        Tuple<int, List<Position_SOM>> tuple = fom.GetBurstingColumnsInLastCycle(CycleNum);

                        if (tuple != null)
                            dict.Add(tuple.Item1, tuple.Item2);
                    }

                    breakpoint = 2;
                }

                burstCache.Add(postBiasBurstCount);

                CycleNum++;
                counter--;
            }

            return burstCache;
        }

        #endregion

        #region Private Methods        

        /// <summary>
        /// Takens in a bmp and preps and fires all FOM & SOM's.
        /// </summary>     
        private void ParseNFireBitmap(Bitmap greyScalebmp)
        {

            pEncoder.ParseBitmap(greyScalebmp);

            foreach (var kvp in pEncoder.FOMBBMIDS)
            {
                switch (kvp.Key)
                {
                    case MAPPERCASE.ALL:
                        {
                            foreach (var bbmID in kvp.Value)
                            {
                                pEncoder.GetSDR_SOMForMapperCase(MAPPERCASE.ALL, bbmID);
                            }
                        }
                        break;
                    case MAPPERCASE.ONETWOTHREEE:
                        {
                            foreach (var bbmID in kvp.Value)
                            {
                                pEncoder.GetSDR_SOMForMapperCase(MAPPERCASE.ONETWOTHREEE, bbmID);
                            }
                        }
                        break;
                    case MAPPERCASE.TWOTHREEFOUR:
                        {

                            foreach (var bbmID in kvp.Value)
                            {
                                pEncoder.GetSDR_SOMForMapperCase(MAPPERCASE.TWOTHREEFOUR, bbmID);
                            }
                        }
                        break;
                    case MAPPERCASE.ONETWOFOUR:
                        {
                            foreach (var bbmID in kvp.Value)
                            {
                                pEncoder.GetSDR_SOMForMapperCase(MAPPERCASE.ONETWOFOUR, bbmID);
                            }
                        }
                        break;
                    case MAPPERCASE.ONETHREEFOUR:
                        {
                            foreach (var bbmID in kvp.Value)
                            {
                                pEncoder.GetSDR_SOMForMapperCase(MAPPERCASE.ONETHREEFOUR, bbmID);
                            }
                        }
                        break;
                    case MAPPERCASE.ONETWO:
                        {
                            foreach (var bbmID in kvp.Value)
                            {
                                pEncoder.GetSDR_SOMForMapperCase(MAPPERCASE.ONETWO, bbmID);
                            }
                        }
                        break;
                    case MAPPERCASE.ONETHREE:
                        {
                            foreach (var bbmID in kvp.Value)
                            {
                                pEncoder.GetSDR_SOMForMapperCase(MAPPERCASE.ONETHREE, bbmID);
                            }
                        }
                        break;
                    case MAPPERCASE.ONEFOUR:
                        {
                            foreach (var bbmID in kvp.Value)
                            {
                                pEncoder.GetSDR_SOMForMapperCase(MAPPERCASE.ONEFOUR, bbmID);
                            }
                        }
                        break;
                    case MAPPERCASE.TWOTHREE:
                        {
                            foreach (var bbmID in kvp.Value)
                            {
                                pEncoder.GetSDR_SOMForMapperCase(MAPPERCASE.TWOTHREE, bbmID);
                            }
                        }
                        break;
                    case MAPPERCASE.TWOFOUR:
                        {
                            foreach (var bbmID in kvp.Value)
                            {
                                pEncoder.GetSDR_SOMForMapperCase(MAPPERCASE.TWOFOUR, bbmID);
                            }
                        }
                        break;
                    case MAPPERCASE.THREEFOUR:
                        {
                            foreach (var bbmID in kvp.Value)
                            {
                                pEncoder.GetSDR_SOMForMapperCase(MAPPERCASE.THREEFOUR, bbmID);
                            }
                        }
                        break;
                    case MAPPERCASE.ONE:
                        {
                            foreach (var bbmID in kvp.Value)
                            {
                                pEncoder.GetSDR_SOMForMapperCase(MAPPERCASE.ONE, bbmID);
                            }
                        }
                        break;
                    case MAPPERCASE.TWO:
                        {
                            foreach (var bbmID in kvp.Value)
                            {
                                pEncoder.GetSDR_SOMForMapperCase(MAPPERCASE.TWO, bbmID);
                            }
                        }
                        break;
                    case MAPPERCASE.THREE:
                        {
                            foreach (var bbmID in kvp.Value)
                            {
                                pEncoder.GetSDR_SOMForMapperCase(MAPPERCASE.THREE, bbmID);
                            }
                        }
                        break;
                    case MAPPERCASE.FOUR:
                        {
                            foreach (var bbmID in kvp.Value)
                            {
                                pEncoder.GetSDR_SOMForMapperCase(MAPPERCASE.FOUR, bbmID);
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

        private List<Position_SOM> Conver2DtoSOMList(List<Position2D> somList)
        {
            List<Position_SOM> toReturn = new List<Position_SOM>();

            foreach (var item in somList)
            {
                toReturn.Add(new Position_SOM(item.X, item.Y));
            }

            return toReturn;
        }


        private uint GetTotalBurstCountInFOMLayerInLastCycle(ulong cycleNum)
        {
            uint totalBurstCount = 0;

            foreach (var fom in fomBBMV)
            {
                totalBurstCount += fom.GetTotalBurstCountInLastCycle(cycleNum);
            }

            return totalBurstCount;
        }


        public bool BiasFOM(SDR_SOM hcSignal)
        {
            var spatialSignal = PixelEncoder.GetFOMEquivalentPositionsofSOM(hcSignal.ActiveBits);

            if (spatialSignal == null)
            {
                WriteLogsToFile("Empty Polarizing Signal for FOM During Waaandering!!!");
                return false;
            }

            bool flag = false;

            foreach (var kvp in spatialSignal)
            {
                if (kvp.Value.Count > 0)
                {
                    SDR_SOM fomSDR = null;

                    if (hcSignal.InputPatternType.Equals(iType.TEMPORAL))
                    {
                        fomSDR = new SDR_SOM(NumColumns, Z, kvp.Value, iType.TEMPORAL);
                    }
                    else if (hcSignal.InputPatternType == iType.APICAL)
                    {
                        fomSDR = new SDR_SOM(NumColumns, NumColumns, kvp.Value, iType.APICAL);
                    }


                    if (fomSDR != null && kvp.Key < 101)
                    {
                        RemoveDuplicateEntries(ref fomSDR);

                        if (kvp.Key == 30)
                        {
                            bool bp = true;
                        }

                        fomBBMV[kvp.Key].Fire(fomSDR, CycleNum);

                        flag = true;
                    }
                }
            }

            return flag;
        }

        #endregion

        #region Helper Methods


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


        private void FireFOMsT()
        {
            int bbmID = 0;

            foreach (var kvp in cEncoder.FOMBBMIDS)
            {
                bbmID = kvp.Key;

                switch (kvp.Value)
                {
                    case MAPPERCASE.ALL:
                        {
                            var poses = pEncoder.GetSDR_SOMForMapperCase(MAPPERCASE.ALL, kvp.Key);
                            fomBBMT[bbmID].Fire(poses, CycleNum);
                            firingFOM_T.Add(bbmID);
                        }
                        break;
                    case MAPPERCASE.ONETWOTHREEE:
                        {
                            var poses = pEncoder.GetSDR_SOMForMapperCase(MAPPERCASE.ONETWOTHREEE, bbmID);
                            fomBBMV[bbmID].Fire(poses, CycleNum);
                            firingFOM_T.Add(bbmID);
                        }
                        break;
                    case MAPPERCASE.TWOTHREEFOUR:
                        {
                            var poses = pEncoder.GetSDR_SOMForMapperCase(MAPPERCASE.TWOTHREEFOUR, bbmID);
                            fomBBMV[bbmID].Fire(poses, CycleNum);
                            firingFOM_T.Add(bbmID);
                        }
                        break;
                    case MAPPERCASE.ONETWOFOUR:
                        {
                            var poses = pEncoder.GetSDR_SOMForMapperCase(MAPPERCASE.ONETWOFOUR, bbmID);
                            fomBBMV[bbmID].Fire(poses, CycleNum);
                            firingFOM_T.Add(bbmID);
                        }
                        break;
                    case MAPPERCASE.ONETHREEFOUR:
                        {
                            var poses = pEncoder.GetSDR_SOMForMapperCase(MAPPERCASE.ONETHREEFOUR, bbmID);
                            fomBBMV[bbmID].Fire(poses, CycleNum);
                            firingFOM_T.Add(bbmID);
                        }
                        break;
                    case MAPPERCASE.ONETWO:
                        {
                            var poses = pEncoder.GetSDR_SOMForMapperCase(MAPPERCASE.ONETWO, bbmID);
                            fomBBMV[bbmID].Fire(poses, CycleNum);
                            firingFOM_T.Add(bbmID);
                        }
                        break;
                    case MAPPERCASE.ONETHREE:
                        {
                            var poses = pEncoder.GetSDR_SOMForMapperCase(MAPPERCASE.ONETHREE, bbmID);
                            fomBBMV[bbmID].Fire(poses, CycleNum);
                            firingFOM_T.Add(bbmID);
                        }
                        break;
                    case MAPPERCASE.ONEFOUR:
                        {

                            var poses = pEncoder.GetSDR_SOMForMapperCase(MAPPERCASE.ONEFOUR, bbmID);
                            fomBBMV[bbmID].Fire(poses, CycleNum);
                            firingFOM_T.Add(bbmID);
                        }
                        break;
                    case MAPPERCASE.TWOTHREE:
                        {
                            var poses = pEncoder.GetSDR_SOMForMapperCase(MAPPERCASE.TWOTHREE, bbmID);
                            fomBBMV[bbmID].Fire(poses, CycleNum);
                            firingFOM_T.Add(bbmID);
                        }
                        break;
                    case MAPPERCASE.TWOFOUR:
                        {
                            var poses = pEncoder.GetSDR_SOMForMapperCase(MAPPERCASE.TWOFOUR, bbmID);
                            fomBBMV[bbmID].Fire(poses, CycleNum);
                            firingFOM_T.Add(bbmID);
                        }
                        break;
                    case MAPPERCASE.THREEFOUR:
                        {
                            var poses = pEncoder.GetSDR_SOMForMapperCase(MAPPERCASE.THREEFOUR, bbmID);
                            fomBBMV[bbmID].Fire(poses, CycleNum);
                            firingFOM_T.Add(bbmID);
                        }
                        break;
                    case MAPPERCASE.ONE:
                        {
                            var poses = pEncoder.GetSDR_SOMForMapperCase(MAPPERCASE.ONE, bbmID);
                            fomBBMV[bbmID].Fire(poses, CycleNum);
                            firingFOM_T.Add(bbmID);
                        }
                        break;
                    case MAPPERCASE.TWO:
                        {
                            var poses = pEncoder.GetSDR_SOMForMapperCase(MAPPERCASE.TWO, bbmID);
                            fomBBMV[bbmID].Fire(poses, CycleNum);
                            firingFOM_T.Add(bbmID);
                        }
                        break;
                    case MAPPERCASE.THREE:
                        {
                            var poses = pEncoder.GetSDR_SOMForMapperCase(MAPPERCASE.THREE, bbmID);
                            fomBBMV[bbmID].Fire(poses, CycleNum);
                            firingFOM_T.Add(bbmID);
                        }
                        break;
                    case MAPPERCASE.FOUR:
                        {
                            var poses = pEncoder.GetSDR_SOMForMapperCase(MAPPERCASE.FOUR, bbmID);
                            fomBBMV[bbmID].Fire(poses, CycleNum);
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

        public void RemoveDuplicateEntries(ref SDR_SOM sdr_SOM)
        {
            int i = 0, j = 0;
            List<int> indexsToRemove = new List<int>();
            List<Position_SOM> newActiveBitsList = new List<Position_SOM>();

            foreach (var pos1 in sdr_SOM.ActiveBits)
            {
                if (newActiveBitsList.Where(item => item.X == pos1.X && item.Y == pos1.Y).Count() == 0)
                {
                    newActiveBitsList.Add(pos1);
                }
                else
                {
                    int breakpoint = 1;
                }
            }

            sdr_SOM.ActiveBits = newActiveBitsList;
        }

        public Bitmap ConverToEdgedBitmap()
        {
            Bitmap newBitmap = new Bitmap(bmp.Width, bmp.Height);

            //get a graphics object from the new image
            using (Graphics g = Graphics.FromImage(newBitmap))
            {

                //create the grayscale ColorMatrix
                ColorMatrix colorMatrix = new ColorMatrix(
                   new float[][]
                   {
                     new float[] {.3f, .3f, .3f, 0, 0},
                     new float[] {.59f, .59f, .59f, 0, 0},
                     new float[] {.11f, .11f, .11f, 0, 0},
                     new float[] {0, 0, 0, 1, 0},
                     new float[] {0, 0, 0, 0, 1}
                   });

                //create some image attributes
                using (ImageAttributes attributes = new ImageAttributes())
                {

                    //set the color matrix attribute
                    attributes.SetColorMatrix(colorMatrix);

                    //draw the original image on the new image
                    //using the grayscale color matrix
                    g.DrawImage(bmp, new Rectangle(0, 0, bmp.Width, bmp.Height),
                                0, 0, bmp.Width, bmp.Height, GraphicsUnit.Pixel, attributes);
                }
            }

            return newBitmap;
        }

        public RecognisedEntity GetPredictedObject() => HCAccessor.GetCurrentPredictedObject();

        public RecognitionState CheckIfObjectIsRecognised() => HCAccessor.ObjectState;

        private SDR_SOM GetSdrSomFromFOMs()
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


            if (logMode == Common.LogMode.BurstOnly)
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

            if (posList != null || posList.Count != 0)
            {
                return new SDR_SOM(1250, 10, posList, iType.SPATIAL);
            }

            throw new NullReferenceException(" FOM BBM returned empty position list ");
        }

        private bool CompareTwoPositionLists(List<Position_SOM> pattern1, List<Position_SOM> pattern2)
        {
            int breakpint = 1;

            pattern1.Sort();

            pattern2.Sort();

            List<int> unmatchedIndex = new List<int>();
            int bp2 = 1;

            int matchCount = 0;
            int index = 0;

            foreach (var item in pattern2)
            {
                if (pattern1.Where(x => x.X == item.X && x.Y == item.Y && x.Z == item.Z).Count() != 0)
                    matchCount++;
                else
                {
                    unmatchedIndex.Add(index);
                }

                index++;
            }

            return matchCount == pattern2.Count;
        }

        #endregion

        #region BIG MAN WORK

        //public string StartCycle()
        //{

        //    //Check how big the entire screen / image is and kee papring through all the objects.

        //    var str = DetectObject();

        //    StoreObject();

        //    return str;



        //}

        /// <summary>
        /// PROBLEMS:
        /// 
        /// ALGORITHM:
        /// 
        /// </summary>
        //public string DetectObject()
        //{
        //    var obj = string.Empty;

        //    if (Objects == null || Objects.Count == 0)
        //    {
        //        obj = LearnFirstObject();

        //    }

        //    //Traverse through Object Maps and what sensory inputs are telling you.

        //    return obj;
        //}



        /// <summary>
        /// 
        /// PROBLEMS:
        /// 1. How to figure out on the current mouse position whether if its on an object and how to traverse through the object of the object itself is bigger than the current visual radius.
        /// 2. How to handle 2 different types of feed processing ? Narrow vs Broad fields of vision.
        /// 
        /// ALGORITHM : 
        /// 1. Get Current Position of the Mouse Pointer and get current Screen Measurements.
        /// 2. Run around the object trying to figure out the object, go to as many unexplored locations on the object as possible.
        /// 3. After you feel you have explored all the points on the object , concurrently keep storing all the locations onto the new unrecognised Object.                
        /// </summary>
        private string LearnFirstObject()
        {
            //No Object Frame , Have to create a sense @ Location object map from scratch for this particular object.            

            // -> L4, -> L3b, L3b -> HP -> output , DoesItMatch Expectation ? y ? Get 5 more confirmations and store model : work on the second prediction if there is 

            string obj = string.Empty;

            bool stillRecognising = true;

            //while (stillRecognising)
            //{
            //    // feed L4
            //    SDR_SOM fomSdr;
            //    for (int i = 0; i < fomBBM.Length; i++)
            //    {
            //        fomSdr = GetFomSdrForIteration(i, mapper);
            //        fomBBM[i].Fire(fomSdr);
            //    }

            //    //feed same pattern SOM BBM L3A
            //    SDR_SOM Sdr_Som3A = new SDR_SOM(10, 10, new List<Position_SOM>() { }, iType.SPATIAL);
            //    somBBM_L3Af.Fire(Sdr_Som3A);

            //    // init L3B to Apple
            //    SDR_SOM Sdr_SomL3B = GetSdrSomFromFOMs();
            //    somBBM_L3BV.Fire(Sdr_SomL3B);

            //    // Push new object representation to 3A
            //    // Push Wiring Burst Avoiding LTP to 4 from 3A.
            //}


            return obj;
        }

        #endregion

        #region Public Helper Methods

        public static void MoveCursorToSpecificPosition(int x, int y)
        {
            POINT p;
            IntPtr desk = GetDesktopWindow();
            IntPtr dc = GetWindowDC(desk);

            p.X = x;
            p.Y = y;

            ClientToScreen(dc, ref p);
            SetCursorPos(p.X, p.Y);

            ReleaseDC(desk, dc);
        }

        public void MoveCursor(POINT p)
        {
            IntPtr desk = GetDesktopWindow();
            IntPtr dc = GetWindowDC(desk);

            ClientToScreen(dc, ref p);
            SetCursorPos(p.X, p.Y);

            ReleaseDC(desk, dc);
        }

        public POINT GetCurrentPointerPosition()
        {
            POINT point;

            point = new POINT();
            point.X = 0;
            point.Y = 0;

            if (GetCursorPos(out point))
            {
                //Console.Clear();
                //Console.WriteLine(point.X.ToString() + " " + point.Y.ToString());
                return point;
            }

            return point;
        }

        public static Position GetCurrentPointerPosition1()
        {
            Position position = null;
            POINT point;

            point = new POINT();
            point.X = 0;
            point.Y = 0;

            if (GetCursorPos(out point))
            {
                //Console.Clear();
                //Console.WriteLine(point.X.ToString() + " " + point.Y.ToString());
                position = new Position(point.X, point.Y);
            }

            return position;
        }

        private SDR_SOM AddSOMOverheadtoFOMSDR(SDR_SOM fomSDR, int blockidX, int blockIdY)
        {
            SDR_SOM toRet;
            int block_offset = 10;
            List<Position_SOM> newPosList = new List<Position_SOM>();

            foreach (var pos in fomSDR.ActiveBits)
            {
                newPosList.Add(new Position_SOM(blockidX * block_offset + pos.X, pos.Y));
            }

            toRet = new SDR_SOM(1250, 10, newPosList, iType.SPATIAL);

            return toRet;
        }

        public bool CheckifPixelisBlack(int x, int y)
        {
            if (x >= bmp.Width || y >= bmp.Height)
                return false;

            var color = bmp.GetPixel(x, y);

            return (color.R < 200 && color.G < 200 && color.B < 200);
        }

        public void ChangeNetworkModeToPrediction()
        {
            NMode = NetworkMode.PREDICTION;
            HCAccessor.SetNetworkModeToPrediction();
            somBBM_L3B_V.ChangeNetworkModeToPrediction();
            somBBM_L3A_V.ChangeNetworkModeToPrediction();
        }

        public void ChangeNetworkModeToTraining()
        {
            NMode = NetworkMode.TRAINING;
            HCAccessor.SetNetworkModeToTraining();
        }

        private void PrintMoreBlockVitals()
        {
            Console.WriteLine("Enter '1' to see a list of all the Block Usage List :");

            int w = Console.Read();

            if (w == 49)
            {
                ulong totalIncludedCycle = 0;

                for (int i = 0; i < fomBBMV.Count(); i++)
                {
                    if (fomBBMV[i].BBMID != 0)
                        Console.WriteLine(i.ToString() + " :: Block ID : " + fomBBMV[i].PrintBlockDetailsSingleLine() + " | " + "Inclusded Cycle: " + fomBBMV[i].CycleNum.ToString());

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

        public void BackUp()
        {
            for (int i = 0; i < fomBBMV.Length; i++)
            {
                fomBBMV[i].BackUp(i.ToString() + ".json");
            }

            somBBM_L3B_V.BackUp("SOML3B.json");

            somBBM_L3A_V.BackUp("SOML3A.json");

            HCAccessor.Backup();

        }


        private void WriteLogsToFile(string logMsg)
        {
            File.WriteAllText(filename, logMsg);
        }

        public void Restore()
        {
            for (int i = 0; i < fomBBMV.Length; i++)
            {
                fomBBMV[i] = FBBM.Restore(i.ToString(), LayerType.Layer_4);
            }

            somBBM_L3B_V = SBBM.Restore("SOML3B", LayerType.Layer_3B);

            somBBM_L3A_V = SBBM.Restore("SOML3A", LayerType.Layer_3A);

            _orchestrator.HCAccessor = HippocampalComplex.Restore();
        }

        #endregion

        #region Future Work

        private void MoveToNextObject()
        {
            throw new NotImplementedException();
        }

        private void StoreObject()
        {
            throw new NotImplementedException();
        }

        #region PRIVATE HELPER METHODS

        private void GetPixelsAsPerScope()
        {

            POINT currentMousePosition = GetCurrentPointerPosition();


            switch (GetScope())
            {
                case VisionScope.NarrowScope:       // High Detail In-Object Scope
                    {
                        break;
                    }
                case VisionScope.ObjectScope:       // Fully View of Object
                    {
                        break;
                    }
                case VisionScope.BroadScope:        // complete visual view at capacity.
                    {
                        break;
                    }
                default:
                    {
                        break;
                    }
            }
        }

        private VisionScope GetScope()
        {
            VisionScope scopeToReturn = VisionScope.NarrowScope;

            bool makesSense = false;

            while (makesSense != false)
            {

                // Get Screen Pixel Values 

                // Analyse

                // Increase or decrease Scope && try again

                makesSense = DoesItMakeSense();
            }

            return scopeToReturn;
        }



        private bool DoesItMakeSense()
        {
            // Check if cursor is currently on top of an object or just screen saver ?
            //If object , check what is the size of the object and return the dimension needed to cover the whole object.
            AdjustScopetoNearestObject();


            //if Scope is not on Object then return broad scope to grab the whole screen view with lower pixel rate.
            GrabWholeScreen();

            return true;

        }

        private Bitmap GrabWholeScreen()
        {

            // Bitmap bmpScreenCapture = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
            throw new NotImplementedException();



        }



        private void AdjustScopetoNearestObject()
        {
            POINT p = GetCurrentPointerPosition();





        }


        #region TRASH


        //public void GrabNProcess(ref bool[,] booleans)          //We process one image at once.
        //{
        //    //Todo : Pixel combination should not be serial , it should be randomly distributed through out the unit

        //    Stopwatch stopWatch = new Stopwatch();

        //    stopWatch.Start();

        //    int TotalReps = 2;

        //    int TotalNumberOfPixelsToProcess_X = GetRoundedTotalNumberOfPixelsToProcess(bmp.Width);
        //    int TotalNumberOfPixelsToProcess_Y = GetRoundedTotalNumberOfPixelsToProcess(bmp.Height);

        //    int TotalPixelsCoveredPerIteration = BlockOffset * BlockOffset; //2500            

        //    int num_blocks_per_bmp_x = (int)(TotalNumberOfPixelsToProcess_X / BlockOffset);
        //    int num_blocks_per_bmp_y = (int)(TotalNumberOfPixelsToProcess_Y / BlockOffset);

        //    int num_unit_per_block_x = 5;
        //    int num_unit_per_block_y = 5;

        //    int num_pixels_per_Unit_x = 10;
        //    int num_pixels_per_Unit_y = 10;

        //    for (int reps = 0; reps < TotalReps; reps++)
        //    {
        //        for (int blockid_y = 0; blockid_y < num_blocks_per_bmp_y; blockid_y++)
        //        {
        //            for (int blockid_x = 0; blockid_x < num_blocks_per_bmp_x; blockid_x++)
        //            {
        //                int bbmId = 0;

        //                for (int unitId_y = 0; unitId_y < num_unit_per_block_y; unitId_y++)
        //                {
        //                    for (int unitId_x = 0; unitId_x < num_unit_per_block_x; unitId_x++)
        //                    {
        //                        BoolEncoder boolEncoder = new BoolEncoder(100, 20);

        //                        for (int j = 0; j < num_pixels_per_Unit_x; j++)
        //                        {
        //                            for (int i = 0; i < num_pixels_per_Unit_y; i++)
        //                            {
        //                                int pixel_x = blockid_x * BlockOffset + unitId_x * UnitOffset + i;
        //                                int pixel_y = blockid_y * BlockOffset + unitId_y * UnitOffset + j;

        //                                //if the pixel is Black then tag the pixel location

        //                                if (blockid_x == 6 && blockid_y == 0 && unitId_x == 4 && unitId_y == 2 && j == 2)
        //                                {
        //                                    int bp = 1;
        //                                }

        //                                if (CheckifPixelisBlack(pixel_x, pixel_y))
        //                                {

        //                                    var dataToEncode = (j % 2).ToString() + "-" + i.ToString();
        //                                    boolEncoder.SetEncoderValues(dataToEncode);

        //                                }
        //                            }

        //                            if (j % 2 == 1)     //Bcoz one BBM covers 2 lines of pixel per unit
        //                            {
        //                                if (fomBBM[bbmId].TemporalLineArray[0, 0] == null)
        //                                {
        //                                    fomBBM[bbmId].Init(blockid_x, blockid_y, unitId_x, unitId_y, bbmId);
        //                                }

        //                                if (boolEncoder.HasValues())
        //                                {
        //                                    CycleNum++;

        //                                    var imageSDR = boolEncoder.Encode(iType.SPATIAL);

        //                                    fomBBM[bbmId++].Fire(imageSDR);

        //                                    SDR_SOM fomSDR = fomBBM[bbmId].GetPredictedSDR();

        //                                    if (fomSDR != null && fomSDR.ActiveBits.Count != 0)
        //                                    {
        //                                        fomSDR = AddSOMOverheadtoFOMSDR(fomSDR, blockid_x, blockid_y);

        //                                        somBBM_L3BV.Fire(fomSDR);
        //                                    }

        //                                }

        //                                boolEncoder.ClearEncoderValues();
        //                            }
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //    }


        //    PrintMoreBlockVitals();

        //    BackUp();

        //    Console.WriteLine("Finished Processing Pixel Values : Total Time Elapsed in seconds : " + (stopWatch.ElapsedMilliseconds / 1000).ToString());

        //    Console.WriteLine("Black Pixel Count :: " + blackPixelCount.ToString());

        //    Console.WriteLine("Done Processing Image");

        //    Console.Read();
        //}

        //public int GetRoundedTotalNumberOfPixelsToProcess(int numberOfPixels_Index)
        //{
        //    if (numberOfPixels_Index % BlockOffset == 0)
        //    {
        //        return numberOfPixels_Index;
        //    }

        //    int nextMinNumberOfPixels = numberOfPixels_Index;

        //    int halfOfnextMinNumberOfPixels = numberOfPixels_Index / 2;

        //    while (nextMinNumberOfPixels % 50 != 0)
        //    {
        //        nextMinNumberOfPixels--;

        //        if (nextMinNumberOfPixels < halfOfnextMinNumberOfPixels)
        //        {
        //            Console.WriteLine(" GetRoundedTotalNumberOfPixelsToProcess() :: Unable to find the proper lower Bound");
        //        }

        //    }

        //    if (nextMinNumberOfPixels % 50 != 0)
        //        throw new InvalidDataException("Grab :: blockLength should always be factor of NumPixelToProcess");

        //    return nextMinNumberOfPixels;
        //}


        // Already grey scalled.
        private void GetColorByRange(int x1, int y1, int x2, int y2)
        {
            IntPtr desk = GetDesktopWindow();

            IntPtr dc = GetWindowDC(desk);


            for (int i = x1, k = 0; i < x2 && k < Range + Range; i++, k++)
            {
                for (int j = y1, l = 0; j < y2 && l < Range + Range; j++, l++)
                {
                    int a = (int)GetPixel(dc, i, j);

                    Color color = System.Drawing.Color.FromArgb(255,
                                                 (a >> 0) & 0xff,
                                                 (a >> 8) & 0xff,
                                                 (a >> 16) & 0xff);

                    bmp.SetPixel(k, l, color);

                }

            }

            ReleaseDC(desk, dc);
        }

        #endregion

        #endregion

        #endregion
    }


    #region Enums

    public enum VisionScope
    {
        BroadScope,
        ObjectScope,
        NarrowScope,
        UNKNOWN
    }

    #endregion
}
