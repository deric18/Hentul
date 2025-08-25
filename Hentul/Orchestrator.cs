namespace Hentul
{
    using Common;
    using System.Runtime.InteropServices;
    using System;
    using Hentul.Hippocampal_Entorinal_complex;
    using System.Drawing.Imaging;
    using System.Drawing;
    using Hentul.Encoders;
    using System.Windows.Forms;

    public class Orchestrator
    {
        string baseDir = AppContext.BaseDirectory;

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
        public int ProcessEveryNthFrame { get; set; } = 3;// 1 = process every frame, 3 = every 3rd frame
        public bool ShouldProcessThisFrame =>
    ProcessEveryNthFrame <= 1 || (CycleNum % (ulong)ProcessEveryNthFrame) == 0;

        public bool SaveDebugFrames { get; set; } = false;


        public int Range { get; private set; }

        private bool LogMode { get; set; }

        public bool IsMock { get; private set; }

        public HippocampalComplex HCAccessor { get; private set; }        

        public int[] MockBlockNumFires { get; private set; }

        private bool devbox = false;

        public int ImageIndex { get; private set; }

        public POINT point;

        public List<string> ImageList { get; private set; }

        public Bitmap bmp;

        public static string fileName;

        public string logfilename;

        public LogMode logMode;

        private List<string> objectlabellist { get; set; }

        private int imageIndex { get; set; }

        public NetworkMode NMode { get; set; }        
        
        public VisionStreamProcessor VisionProcessor { get; set; }

        public TextStreamProcessor TextProcessor { get; private set; }

        public ulong CycleNum { get; private set; }

        private int NumColumns, X, Z;
        private Bitmap _rawV1, _rawV2, _rawV3;          // 20x20, 100x100, 200x200
        private Bitmap _procV1, _procV2, _procV3;       // all 40x20 (encoder input)

        #endregion

        private Orchestrator(int visionrange, bool isMock = false, bool ShouldInit = true, NetworkMode nMode = NetworkMode.TRAINING, int mockImageIndex = 7)
        {

            X = 1250;

            NumColumns = 10;

            Z = 4;

            LogMode = false;

            Range = visionrange;

            NMode = nMode;

            logMode = Common.LogMode.BurstOnly;

            VisionProcessor = new VisionStreamProcessor(Range, NumColumns, X, logMode, isMock, ShouldInit);

            TextProcessor = new TextStreamProcessor(10, 5, logMode);

            if (isMock)
                ImageIndex = mockImageIndex;
            else
                ImageIndex = 0;

            Init();            

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

            //MockBlockNumFires = new int[NumBBMNeededV];                       

            //fileName = "C:\\darshaka\\Thabrew\\HentulGit\\Hentul\\Images\\savedImage.png";
            fileName = Path.GetFullPath(Path.Combine(baseDir, @"..\..\..\..\..\Hentul\Images\savedImage.jpg"));

            //logfilename = "C:\\darshaka\\Thabrew\\HentulGit\\Hentul\\Logs\\Hentul-Orchestrator.log";
            logfilename = Path.GetFullPath(Path.Combine(baseDir, @"..\..\..\..\..\Hentul\Logs\Hentul-Orchestrator.log"));
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
            Console.WriteLine("Finished Init for this Instance \n");
            Console.WriteLine("Range : " + Range.ToString() + "\n");            
            Console.WriteLine("Initing SOM Instance now ... \n");
            Console.WriteLine("Finished Init for SOM Instance , Total Time ELapsed : \n");
            Console.WriteLine("Finished Initting of all Instances, System Ready!" + "\n");
        }
        public record RegionFrames(Bitmap Processed, Bitmap Raw, Rectangle Source, LearningUnitType Type);

        #endregion

        #region Public API

        /// Grabs Cursors Current Position and records pixels.        
        public (RegionFrames V1, RegionFrames V2, RegionFrames V3) RecordPixels(bool isMock = false)
        {
            CycleNum++;
            point = GetCurrentPointerPosition();
            var p = new System.Drawing.Point(point.X, point.Y);

            var v1 = RecordRegion(p, range: 10, label: "V1", LearningUnitType.V1, isMock);
            var v2 = RecordRegion(p, range: 50, label: "V2", LearningUnitType.V2, isMock);
            var v3 = RecordRegion(p, range: 100, label: "V3", LearningUnitType.V3, isMock);
            return (v1, v2, v3);
        }
        private void EnsureBuffers()
        {
            _rawV1 ??= new Bitmap(20, 20, PixelFormat.Format32bppArgb);
            _rawV2 ??= new Bitmap(100, 100, PixelFormat.Format32bppArgb);
            _rawV3 ??= new Bitmap(200, 200, PixelFormat.Format32bppArgb);

            _procV1 ??= new Bitmap(40, 20, PixelFormat.Format32bppArgb);
            _procV2 ??= new Bitmap(40, 20, PixelFormat.Format32bppArgb);
            _procV3 ??= new Bitmap(40, 20, PixelFormat.Format32bppArgb);
        }

        private RegionFrames RecordRegion(Point cursor, int range, string label, LearningUnitType type, bool isMock)
        {
            EnsureBuffers();

            // pick buffers by type
            Bitmap rawBuf, procBuf;
            switch (type)
            {
                case LearningUnitType.V1: rawBuf = _rawV1; procBuf = _procV1; break; // 20x20 → 40x20
                case LearningUnitType.V2: rawBuf = _rawV2; procBuf = _procV2; break; // 100x100 → 40x20
                case LearningUnitType.V3: rawBuf = _rawV3; procBuf = _procV3; break; // 200x200 → 40x20
                default: throw new ArgumentOutOfRangeException(nameof(type));
            }

            int size = range * 2;

            // Desired source rectangle (centered at cursor)
            var desired = new Rectangle(cursor.X - range, cursor.Y - range, size, size);

            // Clip to virtual desktop
            Rectangle desktop = DesktopBounds.VirtualScreen();
            Rectangle inter = Rectangle.Intersect(desired, desktop);

            // Prepare destination: clear to black (so out-of-screen areas become black)
            using (var g = Graphics.FromImage(rawBuf))
            {
                g.Clear(Color.Black);
            }

            // Only copy if we have a non-empty intersection
            if (!inter.IsEmpty)
            {
                // Compute where in the raw buffer the intersection should land
                // (offset from desired's top-left)
                int destX = inter.Left - desired.Left;
                int destY = inter.Top - desired.Top;

                using (var g = Graphics.FromImage(rawBuf))
                {
                    // CopyFromScreen needs a size; use only the intersection size
                    g.CopyFromScreen(inter.Left, inter.Top, destX, destY, inter.Size, CopyPixelOperation.SourceCopy);
                }
            }

            // Resize rawBuf ⇒ 40×20 for the encoder
            using (var g2 = Graphics.FromImage(procBuf))
            {
                g2.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBilinear;
                g2.DrawImage(rawBuf, new Rectangle(0, 0, 40, 20));
            }

            if (SaveDebugFrames)
            {
                string dir = Path.GetDirectoryName(fileName)!;
                string baseName = Path.GetFileNameWithoutExtension(fileName);
                Directory.CreateDirectory(dir);

                string rawPath = Path.Combine(dir, $"{baseName}_{label}_raw.png");
                string procPath = Path.Combine(dir, $"{baseName}_{label}.png");
                if (File.Exists(rawPath)) File.Delete(rawPath);
                if (File.Exists(procPath)) File.Delete(procPath);
                rawBuf.Save(rawPath, ImageFormat.Png);
                procBuf.Save(procPath, ImageFormat.Png);
            }

            return new RegionFrames(procBuf, rawBuf, desired, type);
        }


        private Bitmap RecordRegion(Point cursor, int range, string label, bool isMock)
        {
            int size = range * 2;
            int x1 = Math.Max(cursor.X - range, 0);
            int y1 = Math.Max(cursor.Y - range, 0);
            Rectangle rect = new Rectangle(x1, y1, size, size);

            Bitmap processed;

            using (Bitmap fullBmp = new Bitmap(size, size, PixelFormat.Format32bppArgb))
            using (Graphics g = Graphics.FromImage(fullBmp))
            {
                g.CopyFromScreen(x1, y1, 0, 0, fullBmp.Size, CopyPixelOperation.SourceCopy);

                if (label == "V1")
                {
                    // No downsampling
                    processed = new Bitmap(40, 20);
                    using (Graphics g2 = Graphics.FromImage(processed))
                    {
                        g2.DrawImage(fullBmp, 0, 0, 40, 20); // Or copy appropriate section
                    }
                }
                else
                {
                    // Downsample to 40x20 (800 pixels)
                    processed = new Bitmap(40, 20);
                    for (int y = 0; y < 20; y++)
                    {
                        for (int x = 0; x < 40; x++)
                        {
                            int srcX = (int)((float)x / 40 * size);
                            int srcY = (int)((float)y / 20 * size);
                            Color pixel = fullBmp.GetPixel(srcX, srcY);
                            processed.SetPixel(x, y, pixel);
                        }
                    }
                }

                if (label == "V2") bmp = processed;

                if (!isMock)
                {
                   
                    try
                    {
                        string outputPath = fileName.Replace(".jpg", $"_{label}.jpg");
                        using (var bmp = new Bitmap(processed))
                        {
                            bmp.Save(outputPath, ImageFormat.Jpeg);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Failed to save image: {ex.Message}");
                    }
                }
            }
            return processed;
        }
        /// Fires L4 and L3B with the same input and output of L4 -> L3A
        public bool ProcessVisual(Bitmap v1Grey, Bitmap v2Grey, Bitmap v3Grey)
        {
            // Skip work unless this is the Nth frame
            if (ProcessEveryNthFrame > 1 && (CycleNum % (ulong)ProcessEveryNthFrame) != 0)
                return false;

            VisionProcessor.ProcessFor(LearningUnitType.V1, v1Grey);
            VisionProcessor.ProcessFor(LearningUnitType.V2, v2Grey);
            VisionProcessor.ProcessFor(LearningUnitType.V3, v3Grey);
            return true;
        }
        public void ProcessVisual(Bitmap v1Grey)
        {
            VisionProcessor.ProcessFor(LearningUnitType.V1, v1Grey);
        }

        //Stores the new object on to HC
        public void AddNewVisualSensationToHc()
        {
            if (NMode != NetworkMode.TRAINING)
                throw new InvalidOperationException("INVALID State Management!");

            // Get SDRs for all three scales at this CycleNum
            var sdrV1 = VisionProcessor.GetSL3BLatestFiringCells(LearningUnitType.V1, CycleNum);
            var sdrV2 = VisionProcessor.GetSL3BLatestFiringCells(LearningUnitType.V2, CycleNum);
            var sdrV3 = VisionProcessor.GetSL3BLatestFiringCells(LearningUnitType.V3, CycleNum);

            if (sdrV1 == null) // keep V1 mandatory to preserve current semantics
                throw new InvalidOperationException("V1 SDR should not be null!");

            // Convert to Sensation_Location(s) with the current cursor point
            var v1 = VisionProcessor.pEncoder.GetSenseiFromSDR_V(sdrV1, point);
            var v2 = (sdrV2 != null) ? VisionProcessor.pEncoder.GetSenseiFromSDR_V(sdrV2, point) : null;
            var v3 = (sdrV3 != null) ? VisionProcessor.pEncoder.GetSenseiFromSDR_V(sdrV3, point) : null;

            var bundle = new VisualSensationBundle(v1, v2, v3, point);

            if (!HCAccessor.AddNewSensationLocationToObject(bundle))
                throw new InvalidOperationException("Could not add object to HC (duplicate or wrong mode).");
        }


        //Fire L4 & L3B for given character , Fires L3A from L4 input, Stores L3A -> HC.
        public void AddNewCharacterSensationToHC(char ch)
        {
            if (!NMode.Equals(NetworkMode.TRAINING))
            {
                throw new InvalidOperationException("AddNewCharacterSensationToHC_T :: Network Should be in Training Mode before Predicting!");
            }

            TextProcessor.ProcessCharacter(ch, CycleNum);

            var som_SDR = TextProcessor.GetL3BSensation(CycleNum);

            if (som_SDR == null || som_SDR.ActiveBits.Count == 0)
            {
                throw new InvalidOperationException("som_SDR should not be Empty!");
            }

            //Wrong : location should be the location of the mouse pointer relative to the image and not just BBMID.
            Sensation firingSensei = TextProcessor.ConvertSDR_To_Sensation(som_SDR);

            if (HCAccessor.AddNewSensationToObject(firingSensei) == false)
            {
                throw new InvalidOperationException("Could Not Add Object to HC! Sensation already exist in the current Object");
            }
        }


        public Position2D Verify_Predict_HC(bool isMock = false, uint iterationsToConfirmation = 10, bool legacyPipeline = true)
        {
            if (NMode != NetworkMode.PREDICTION)
                throw new InvalidOperationException("Invalid State Managemnt!");

            var sdrV1 = VisionProcessor.GetSL3BLatestFiringCells(LearningUnitType.V1, CycleNum);
            if (sdrV1 == null) return null;

            var v1 = VisionProcessor.pEncoder.GetSenseiFromSDR_V(sdrV1, point);

            // Auxiliary evidence (not sent to HC)
            var sdrV2 = VisionProcessor.GetSL3BLatestFiringCells(LearningUnitType.V2, CycleNum);
            var sdrV3 = VisionProcessor.GetSL3BLatestFiringCells(LearningUnitType.V3, CycleNum);

            var v2 = sdrV2 != null ? VisionProcessor.pEncoder.GetSenseiFromSDR_V(sdrV2, point) : null;
            var v3 = sdrV3 != null ? VisionProcessor.pEncoder.GetSenseiFromSDR_V(sdrV3, point) : null;

            // Call existing HC (V1 only)
            var move = HCAccessor.VerifyObject(v1, null, isMock, iterationsToConfirmation);

            // Optional: veto or dampen strange moves unless V2/V3 roughly agree with the label/region.
            // Example placeholder (depends on your Sensation/label APIs):
            /*
            var ok = MultiScaleAgrees(v1, v2, v3);
            if (!ok) return null; // or return a smaller step / keep scanning
            */
            return move;
       }


        public void DoneWithTraining(string label = "")
        {
            HCAccessor.DoneWithTraining(label);
        }

        public void ChangeNetworkToPredictionMode()
        {
            NMode = NetworkMode.PREDICTION;
            VisionProcessor.SetNetworkModeToPrediction();
            HCAccessor.DoneWithTraining();
            HCAccessor.SetNetworkModeToPrediction();
        }

        //Gets the current SDR and next cycle predited SDR from classifier layer
        internal Tuple<Sensation_Location, Sensation_Location> GetSDRFromL3B()
        {

            Sensation_Location sensei = null, predictedSensei = null;

            var som_SDR = VisionProcessor.GetSL3BLatestFiringCells(LearningUnitType.V1, CycleNum);
            var predictedSDR = VisionProcessor.GetSL3BLatestFiringCells(LearningUnitType.V1, CycleNum + 1);

            if (som_SDR != null)
            {
                sensei = VisionProcessor.pEncoder.GetSenseiFromSDR_V(som_SDR, point);
            }

            if (predictedSDR != null)
            {
                predictedSensei =  VisionProcessor.pEncoder.GetSenseiFromSDR_V(predictedSDR, point);
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

                VisionProcessor.Clean();

                uint postBiasBurstCount = GetTotalBurstCountInFOMLayerInLastCycle(CycleNum);

                if (postBiasBurstCount > 0)
                {
                    Dictionary<int, List<Position_SOM>> bbmToFiringPositions = new Dictionary<int, List<Position_SOM>>();

                    foreach (var fom in VisionProcessor.GetFOMBBMVFromLearningUnit(LearningUnitType.V1))
                    {
                        Tuple<int, List<Position_SOM>> tuple = fom.GetBurstingColumnsInLastCycle(CycleNum);

                        if (tuple != null)
                            bbmToFiringPositions.Add(tuple.Item1, tuple.Item2);
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

            VisionProcessor.pEncoder.ParseBitmap(greyScalebmp);

            foreach (var kvp in VisionProcessor.pEncoder.FOMBBMIDS)
            {
                switch (kvp.Key)
                {
                    case MAPPERCASE.ALL:
                        {
                            foreach (var bbmID in kvp.Value)
                            {
                                VisionProcessor.pEncoder.GetSDR_SOMForMapperCase(MAPPERCASE.ALL, bbmID);
                            }
                        }
                        break;
                    case MAPPERCASE.ONETWOTHREEE:
                        {
                            foreach (var bbmID in kvp.Value)
                            {
                                VisionProcessor.pEncoder.GetSDR_SOMForMapperCase(MAPPERCASE.ONETWOTHREEE, bbmID);
                            }
                        }
                        break;
                    case MAPPERCASE.TWOTHREEFOUR:
                        {

                            foreach (var bbmID in kvp.Value)
                            {
                                VisionProcessor.pEncoder.GetSDR_SOMForMapperCase(MAPPERCASE.TWOTHREEFOUR, bbmID);
                            }
                        }
                        break;
                    case MAPPERCASE.ONETWOFOUR:
                        {
                            foreach (var bbmID in kvp.Value)
                            {
                                VisionProcessor.pEncoder.GetSDR_SOMForMapperCase(MAPPERCASE.ONETWOFOUR, bbmID);
                            }
                        }
                        break;
                    case MAPPERCASE.ONETHREEFOUR:
                        {
                            foreach (var bbmID in kvp.Value)
                            {
                                VisionProcessor.pEncoder.GetSDR_SOMForMapperCase(MAPPERCASE.ONETHREEFOUR, bbmID);
                            }
                        }
                        break;
                    case MAPPERCASE.ONETWO:
                        {
                            foreach (var bbmID in kvp.Value)
                            {
                                VisionProcessor.pEncoder.GetSDR_SOMForMapperCase(MAPPERCASE.ONETWO, bbmID);
                            }
                        }
                        break;
                    case MAPPERCASE.ONETHREE:
                        {
                            foreach (var bbmID in kvp.Value)
                            {
                                VisionProcessor.pEncoder.GetSDR_SOMForMapperCase(MAPPERCASE.ONETHREE, bbmID);
                            }
                        }
                        break;
                    case MAPPERCASE.ONEFOUR:
                        {
                            foreach (var bbmID in kvp.Value)
                            {
                                VisionProcessor.pEncoder.GetSDR_SOMForMapperCase(MAPPERCASE.ONEFOUR, bbmID);
                            }
                        }
                        break;
                    case MAPPERCASE.TWOTHREE:
                        {
                            foreach (var bbmID in kvp.Value)
                            {
                                VisionProcessor.pEncoder.GetSDR_SOMForMapperCase(MAPPERCASE.TWOTHREE, bbmID);
                            }
                        }
                        break;
                    case MAPPERCASE.TWOFOUR:
                        {
                            foreach (var bbmID in kvp.Value)
                            {
                                VisionProcessor.pEncoder.GetSDR_SOMForMapperCase(MAPPERCASE.TWOFOUR, bbmID);
                            }
                        }
                        break;
                    case MAPPERCASE.THREEFOUR:
                        {
                            foreach (var bbmID in kvp.Value)
                            {
                                VisionProcessor.pEncoder.GetSDR_SOMForMapperCase(MAPPERCASE.THREEFOUR, bbmID);
                            }
                        }
                        break;
                    case MAPPERCASE.ONE:
                        {
                            foreach (var bbmID in kvp.Value)
                            {
                                VisionProcessor.pEncoder.GetSDR_SOMForMapperCase(MAPPERCASE.ONE, bbmID);
                            }
                        }
                        break;
                    case MAPPERCASE.TWO:
                        {
                            foreach (var bbmID in kvp.Value)
                            {
                                VisionProcessor.pEncoder.GetSDR_SOMForMapperCase(MAPPERCASE.TWO, bbmID);
                            }
                        }
                        break;
                    case MAPPERCASE.THREE:
                        {
                            foreach (var bbmID in kvp.Value)
                            {
                                VisionProcessor.pEncoder.GetSDR_SOMForMapperCase(MAPPERCASE.THREE, bbmID);
                            }
                        }
                        break;
                    case MAPPERCASE.FOUR:
                        {
                            foreach (var bbmID in kvp.Value)
                            {
                                VisionProcessor.pEncoder.GetSDR_SOMForMapperCase(MAPPERCASE.FOUR, bbmID);
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

            foreach (var fom in VisionProcessor.GetFOMBBMVFromLearningUnit(LearningUnitType.V1))
            {
                totalBurstCount += fom.GetTotalBurstCountInLastCycle(cycleNum);
            }

            return totalBurstCount;
        }


        // Call only after Post Classification : Decision has been made on the object and now we are actively what the object might look like at different locations.
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

                        VisionProcessor.GetFOMBBMVFromLearningUnit(LearningUnitType.V1)[kvp.Key].Fire(fomSDR, CycleNum);

                        flag = true;
                    }
                }
            }

            return flag;
        }

        #endregion

        #region Helper Methods
        
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
        public Bitmap ConverToEdgedBitmapIdentify(Bitmap source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));

            var newBitmap = new Bitmap(source.Width, source.Height);

            using (Graphics g = Graphics.FromImage(newBitmap))
            {
                var colorMatrix = new ColorMatrix(new float[][]
                {
            new float[] {.3f,  .3f,  .3f,  0, 0},
            new float[] {.59f, .59f, .59f, 0, 0},
            new float[] {.11f, .11f, .11f, 0, 0},
            new float[] {0,    0,    0,    1, 0},
            new float[] {0,    0,    0,    0, 1}
                });

                using var attributes = new ImageAttributes();
                attributes.SetColorMatrix(colorMatrix);

                g.DrawImage(
                    source,
                    new Rectangle(0, 0, source.Width, source.Height),
                    0, 0, source.Width, source.Height,
                    GraphicsUnit.Pixel,
                    attributes
                );
            }

            return newBitmap;
        }

        public RecognisedVisualEntity GetPredictedObject() => HCAccessor.GetCurrentPredictedObject();

        public RecognitionState CheckIfObjectIsRecognised() => HCAccessor.ObjectState;
        

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

            VisionProcessor.SetNetworkModeToPrediction();
            
        }

        public void ChangeNetworkModeToTraining()
        {
            NMode = NetworkMode.TRAINING;
            HCAccessor.SetNetworkModeToTraining();
        }

        private void PrintMoreBlockVitals()
        {
            VisionProcessor.PrintBlockVitalVision(LearningUnitType.V1);
        }

        public void BackUp()
        {

            VisionProcessor.Backup();
            HCAccessor.Backup();

        }


        public static void WriteLogsToFile(string logMsg)
        {
            File.WriteAllText(fileName, logMsg);
        }

        public void Restore()
        {
            VisionProcessor.Restore();
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

    public enum LearningUnitType
    {
        V1,
        V2,
        V3
    }

    public enum VisionScope
    {
        BroadScope,
        ObjectScope,
        NarrowScope,
        UNKNOWN
    }

    #endregion
}
