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
    using System.IO;

    public class Orchestrator
    {
        private readonly string baseDir = AppContext.BaseDirectory;

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

        #region CONSTRUCTOR / FIELDS

        private static Orchestrator _orchestrator;

        public int ProcessEveryNthFrame { get; set; } = 3; // 1 = every frame, 3 = every 3rd frame
        public bool ShouldProcessThisFrame => ProcessEveryNthFrame <= 1 || (CycleNum % (ulong)ProcessEveryNthFrame) == 0;

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

        public static string fileName;   // used by WriteLogsToFile
        public string logfilename;
        public LogMode logMode;

        private List<string> objectlabellist { get; set; }
        private int imageIndex { get; set; }

        public NetworkMode NMode { get; set; }
        public VisionStreamProcessor VisionProcessor { get; set; }
        public TextStreamProcessor TextProcessor { get; private set; }
        public ulong CycleNum { get; private set; }

        private int NumColumns, X, Z;

        // Reusable buffers (avoid GC churn)
        private Bitmap _rawV1, _rawV2, _rawV3;    // 20x20, 100x100, 200x200 raw grabs
        private Bitmap _procV1, _procV2, _procV3; // normalized 40x20 per-scale inputs

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

            ImageIndex = isMock ? mockImageIndex : 0;

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

            // --- Relative, repo-friendly paths (no user-specific absolute paths) ---
            // Images/savedImage.jpg and Logs/Hentul-Orchestrator.log under the solution tree
            fileName = Path.GetFullPath(Path.Combine(baseDir, @"..\..\..\..\..\Hentul\Images\savedImage.jpg"));
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
            Console.WriteLine("Finished Init for this Instance");
            Console.WriteLine("Range : " + Range);
            Console.WriteLine("Initing SOM Instance now ...");
            Console.WriteLine("Finished Init for SOM Instance , Total Time ELapsed : ");
            Console.WriteLine("Finished Initting of all Instances, System Ready!");
        }

        public record RegionFrames(Bitmap Processed, Bitmap Raw, Rectangle Source, LearningUnitType Type);

        public void BeginTraining(string objectLabel)
        {
            VisionProcessor.BeginTraining(objectLabel);
        }

        #endregion

        #region PUBLIC API – TRI-SCALE CAPTURE

        /// <summary>
        /// Capture three regions centered at the cursor:
        /// V1 = 20×20 (range 10), V2 = 100×100 (range 50), V3 = 200×200 (range 100).
        /// Each returns both Raw (native scale) and Processed (normalized 40×20).
        /// </summary>
        public (RegionFrames V1, RegionFrames V2, RegionFrames V3) RecordPixels(bool isMock = false)
        {
            CycleNum++;
            point = GetCurrentPointerPosition();
            var p = new System.Drawing.Point(point.X, point.Y);

            var v1 = RecordRegion(p, range: 10,  label: "V1", type: LearningUnitType.V1, isMock: isMock);
            var v2 = RecordRegion(p, range: 50,  label: "V2", type: LearningUnitType.V2, isMock: isMock);
            var v3 = RecordRegion(p, range: 100, label: "V3", type: LearningUnitType.V3, isMock: isMock);

            return (v1, v2, v3);
        }

        private void EnsureBuffers()
        {
            // Allocate reusable buffers only once
            _rawV1  ??= new Bitmap(20, 20,   PixelFormat.Format32bppArgb);
            _rawV2  ??= new Bitmap(100, 100, PixelFormat.Format32bppArgb);
            _rawV3  ??= new Bitmap(200, 200, PixelFormat.Format32bppArgb);

            _procV1 ??= new Bitmap(40, 20, PixelFormat.Format32bppArgb);
            _procV2 ??= new Bitmap(40, 20, PixelFormat.Format32bppArgb);
            _procV3 ??= new Bitmap(40, 20, PixelFormat.Format32bppArgb);
        }

        private RegionFrames RecordRegion(Point cursor, int range, string label, LearningUnitType type, bool isMock)
        {
            EnsureBuffers();

            // Select raw/processed buffers based on LU
            Bitmap rawBuf, procBuf;
            switch (type)
            {
                case LearningUnitType.V1: rawBuf = _rawV1; procBuf = _procV1; break; // 20×20 → 40×20
                case LearningUnitType.V2: rawBuf = _rawV2; procBuf = _procV2; break; // 100×100 → 40×20
                case LearningUnitType.V3: rawBuf = _rawV3; procBuf = _procV3; break; // 200×200 → 40×20
                default: throw new ArgumentOutOfRangeException(nameof(type));
            }

            int size = range * 2;

            // Desired square centered on cursor
            var desired = new Rectangle(cursor.X - range, cursor.Y - range, size, size);

            // Clip to actual virtual desktop (multi-monitor safe)
            Rectangle desktop = SystemInformation.VirtualScreen;
            Rectangle inter = Rectangle.Intersect(desired, desktop);

            // Clear raw buffer to black so off-screen areas show as black
            using (var g = Graphics.FromImage(rawBuf))
                g.Clear(Color.Black);

            // Copy intersection into the correct offset inside our raw buffer
            if (!inter.IsEmpty)
            {
                int destX = inter.Left - desired.Left;
                int destY = inter.Top  - desired.Top;

                using (var g = Graphics.FromImage(rawBuf))
                {
                    g.CopyFromScreen(
                        inter.Left, inter.Top,   // source origin (screen)
                        destX, destY,            // destination origin (rawBuf)
                        inter.Size,              // copy size
                        CopyPixelOperation.SourceCopy);
                }
            }

            // Downscale raw → 40×20 for the encoder
            using (var g2 = Graphics.FromImage(procBuf))
            {
                g2.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                g2.InterpolationMode  = System.Drawing.Drawing2D.InterpolationMode.HighQualityBilinear;
                g2.PixelOffsetMode    = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
                g2.DrawImage(rawBuf, new Rectangle(0, 0, 40, 20));
            }

            if (SaveDebugFrames)
            {
                string dir = Path.GetDirectoryName(fileName)!;
                string baseName = Path.GetFileNameWithoutExtension(fileName);
                Directory.CreateDirectory(dir);

                string rawPath  = Path.Combine(dir, $"{baseName}_{label}_raw.png");
                string procPath = Path.Combine(dir, $"{baseName}_{label}.png");
                if (File.Exists(rawPath))  File.Delete(rawPath);
                if (File.Exists(procPath)) File.Delete(procPath);

                rawBuf.Save(rawPath,  ImageFormat.Png);
                procBuf.Save(procPath, ImageFormat.Png);
            }

            return new RegionFrames(procBuf, rawBuf, desired, type);
        }

        #endregion
