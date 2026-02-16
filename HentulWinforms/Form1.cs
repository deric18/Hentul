namespace HentulWinforms
{
    using Common;
    using Hentul;
    using OpenCvSharp;
    using OpenCvSharp.Extensions;
    using System.Linq;
    using System.Reflection;

    public partial class Form1 : Form
    {
        Orchestrator orchestrator;
        NetworkMode networkMode;
        readonly int numPixels = 10;
        ulong counter = 0;
        int imageIndex = 0;
        int totalImagesToProcess = 1;
        List<string> objectList = new();


        Orchestrator.POINT LeftTop = new Orchestrator.POINT();
        Orchestrator.POINT RightTop = new Orchestrator.POINT();
        Orchestrator.POINT LeftBottom = new Orchestrator.POINT();
        Orchestrator.POINT RightBottom = new Orchestrator.POINT();
        string baseDir = AppContext.BaseDirectory;
        string backupDirHC;
        string backupDirFOM;
        string backupDirSOM;
        private const int SOM_X_V1 = 1250; // matches LearningUnit V1 X
        private const int SOM_X_V2 = 5000; // V2: 100x100 -> X * 4 in LearningUnit
        private const int SOM_X_V3 = 20000; // V3: 200x200 -> X * 16 in LearningUnit

        // --- SOM Visualization Fields ---        
        private const int SOM_X = 1250;   // Matches LearningUnit X
        private const int SOM_Y = 10;     // Matches NumColumns
        private readonly object somDrawLock = new();

        // --- FOM (Layer 4) Visualization Fields ---
        private PictureBox[] fomPictureBoxes;  // pictureBox2 .. pictureBox11

        public Form1()
        {
            InitializeComponent();

            // Ensure non-nullable fields are initialized to valid values to satisfy the compiler.
            orchestrator = Orchestrator.GetInstance();
            fomPictureBoxes = Array.Empty<PictureBox>();

            // Use these fields once so the compiler won't warn about "assigned but never used".
            _ = numPixels;
            _ = imageIndex;

            backupDirHC = Path.GetFullPath(Path.Combine(baseDir, @"..\..\..\..\..\Hentul\Hentul\BackUp\HC-EC\"));
            backupDirFOM = Path.GetFullPath(Path.Combine(baseDir, @"..\..\..\..\..\Hentul\Hentul\BackUp\FOM\"));
            backupDirSOM = Path.GetFullPath(Path.Combine(baseDir, @"..\..\..\..\..\Hentul\Hentul\BackUp\SOM\"));
            networkMode = NetworkMode.TRAINING;
            train_another_object.Visible = false;

            // Attempt to populate fomPictureBoxes from designer-created controls (if present).
            try
            {
                var pics = new List<PictureBox>();
                for (int i = 2; i <= 11; i++)
                {
                    var field = this.GetType().GetField($"pictureBox{i}", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                    if (field?.GetValue(this) is PictureBox pb)
                        pics.Add(pb);
                }
                if (pics.Count > 0)
                    fomPictureBoxes = pics.ToArray();
            }
            catch
            {
                // ignore reflection failures - leave as empty array
            }
        }

        private void StartButton_Click(object sender, EventArgs e)
        {            
        }


        private void Explore_Click(object sender, EventArgs e)
        {
            // 1. Validate network is in training mode and vision scope is BROAD
            if (networkMode != NetworkMode.TRAINING)
                throw new InvalidOperationException("Network must be in TRAINING mode to explore!");

            if (orchestrator.visionScope != VisionScope.BROAD)
                throw new InvalidOperationException("VisionScope must be set to BROAD before exploring!");

            if (string.IsNullOrEmpty(objectBox.Text))
            {
                label_done.Text = "Enter object label before exploring!";
                return;
            }
                
            label_done.Text = "Exploring...";
            label_done.Refresh();

            // 2. Get primary screen dimensions
            int screenWidth = Screen.PrimaryScreen!.Bounds.Width;
            int screenHeight = Screen.PrimaryScreen!.Bounds.Height;

            const int chunkWidth = 600;
            const int chunkHeight = 1200;

            // Calculate number of chunks needed to cover the entire screen
            int chunksX = (int)Math.Ceiling((double)screenWidth / chunkWidth);
            int chunksY = (int)Math.Ceiling((double)screenHeight / chunkHeight);

            int chunkCount = 0;
            int totalChunks = chunksX * chunksY;

            // 3. Iterate over the screen in 1200x600 chunks
            for (int row = 0; row < chunksY; row++)
            {
                for (int col = 0; col < chunksX; col++)
                {
                    // Compute the center of the current chunk
                    int centerX = Math.Min(col * chunkWidth + chunkWidth / 2, screenWidth - 1);
                    int centerY = Math.Min(row * chunkHeight + chunkHeight / 2, screenHeight - 1);

                    // Move cursor to chunk center
                    Orchestrator.MoveCursorToSpecificPosition(centerX, centerY);

                    // Capture the current cursor position
                    Position2D cursorPos = Orchestrator.GetCurrentPointerPosition1();

                    // Record pixels at the current position (BROAD scope uses orchestrator.bmp)
                    orchestrator.RecordPixels(VisionScope.BROAD);

                    // Convert captured bitmap to edge-detected version
                    var edgedBmp = ConverToEdgedBitmap(orchestrator.bmp);

                    // Feed the chunk into the vision pipeline
                    orchestrator.SetupLabel(edgedBmp, objectBox.Text);

                    // Train on this chunk
                    orchestrator.TrainImage(row, col);

                    chunkCount++;
                    label_done.Text = $"Exploring chunk {chunkCount}/{totalChunks}...";
                    label_done.Refresh();
                    Application.DoEvents();
                }
            }

            counter++;
            CycleLabel.Text = counter.ToString();
            CycleLabel.Refresh();

            label_done.Text = $"Exploration complete! Processed {totalChunks} chunks.";
            label_done.Refresh();
        }

        /// <summary>
        /// Draw active bits pixels on the forms UI
        /// </summary>
        /// <param name="onBits"></param>
        private void UpdateUI(List<Position_SOM> onBits)
        {
            const int imgWidth = 1200;
            const int imgHeight = 600;

            // Create bitmap where each grid cell maps to one pixel (1200x600)
            var bmp = new System.Drawing.Bitmap(imgWidth, imgHeight, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

            using (var g = System.Drawing.Graphics.FromImage(bmp))
            {
                // Clear background to white
                g.Clear(System.Drawing.Color.White);

                // Use a single brush for performance
                using var blueBrush = new System.Drawing.SolidBrush(System.Drawing.Color.Blue);

                if (onBits != null)
                {
                    foreach (var pos in onBits)
                    {
                        if (pos == null) continue;

                        int x = pos.X;
                        int y = pos.Y;

                        // Bounds check: ignore positions outside the 1200x600 grid
                        if (x < 0 || x >= imgWidth || y < 0 || y >= imgHeight)
                            continue;

                        // Paint the single grid cell (1x1 pixel) blue
                        g.FillRectangle(blueBrush, x, y, 1, 1);
                    }
                }
            }

            // Assign bitmap to pictureBox4 on UI thread; dispose previous image to avoid leaks
            void SetImage()
            {
                var old = pictureBox4.Image;
                pictureBox4.Image = bmp;
                pictureBox4.SizeMode = PictureBoxSizeMode.Zoom;
                pictureBox4.Refresh();
                old?.Dispose();
            }

            if (pictureBox4.InvokeRequired)
                pictureBox4.Invoke((Action)SetImage);
            else
                SetImage();
        }

        private void startClassificationButton_Click(object sender, EventArgs e)
        {
            label_done.Text = "Procesing";
            label_done.Refresh();

            if (networkMode == NetworkMode.TRAINING)
            {
                throw new InvalidOperationException("Mode should be in Prediction or Done!");
            }
            else if (networkMode == NetworkMode.DONE)
            {
                label_done.Text = "Classification Done!"; label_done.Refresh();

            }

            DrawSomLayer();
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            label_done.Text = "Initializing...";
            label_done.Refresh();
            networkMode = NetworkMode.TRAINING;

            try
            {
                // Use existing GetInstance method (no parameter changes needed)
                await Task.Run(() =>
                {
                    orchestrator = Orchestrator.GetInstance();
                });

                // Rest of your existing code unchanged
                label_done.Text = "Ready";

                openFileDialog1.ShowDialog();
            }
            catch (Exception ex)
            {
                label_done.Text = $"Error: {ex.Message}";
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            LeftTop.X = 954; LeftTop.Y = 416; RightTop.X = 1596; RightTop.Y = LeftTop.Y; LeftBottom.X = LeftTop.X; LeftBottom.Y = 1116; RightBottom.X = RightTop.X; RightBottom.Y = LeftBottom.Y - 500;
            label_done.Text = "Ready";
            wanderingButton.Visible = false;
            BackUp.Visible = false;
            startClassificationButton.Visible = false;
            Restore.Visible = false;
            BackUp.Visible = false;
        }

        private Bitmap ConverToEdgedBitmap(Bitmap incoming)
        {
            string filename = Path.GetFullPath(Path.Combine(baseDir, @"..\..\..\..\..\Hentul\Hentul\Images\savedImage.png"));

            // Ensure orchestrator.bmp access is safe (orchestrator initialized in ctor)
            incoming.Save(filename);

            var edgeImage = Cv2.ImRead(filename);

            var imgdetect = new Mat();

            Cv2.Canny(edgeImage, imgdetect, 50, 200);

            return BitmapConverter.ToBitmap(imgdetect);
        }

        private void BackUp_Click(object sender, EventArgs e)
        {
            orchestrator.BackUp();
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            orchestrator = Orchestrator.GetInstance(false, false);
            orchestrator.Restore();
        }

        private void train_another_object_Click(object sender, EventArgs e)
        {
            totalImagesToProcess++;
            networkMode = NetworkMode.PREDICTION;
            orchestrator.ChangeNetworkModeToPrediction();
            StartButton_Click(sender, e);
        }

        private void button1_Click_3(object sender, EventArgs e)
        {
            List<string> wordsToTrain = new List<string>()
            {
                "MyComputer",
                "RecycleBin",
                "GitBash",
                "QBSStudio",
                "Hentul"
            };

            foreach (var word in wordsToTrain)
            {
                orchestrator.ChangeNetworkModeToTraining();
                foreach (var ch in word)
                {
                    orchestrator.AddNewCharacterSensationToHC(ch);
                }
                orchestrator.DoneWithTraining();
            }
        }

        // -------------------- SOM Visualization Logic --------------------        
        private void DrawAllSomLayers()
        {
            DrawSomLayer(LearningUnitType.V1);
            DrawSomLayer(LearningUnitType.V2);
            DrawSomLayer(LearningUnitType.V3);
        }
        private void DrawSomLayer(LearningUnitType regionType = LearningUnitType.V1)
        {
            // Get the appropriate learning unit and picture box
            //LearningUnit learningUnit;
            //PictureBox targetPictureBox;
            //int somWidth;


            //// Null checks           
            //var somMgr = learningUnit.somBBM_L3B_V;

            //// Attempt to get latest firing neurons
            //SDR_SOM firing = null;
            //try
            //{
            //    var cycle = somMgr.CycleNum;
            //    firing = somMgr.GetAllNeuronsFiringLatestCycle(cycle);

            //    if (firing.ActiveBits.Count != 0)
            //    {
            //        bool breakpoint = true;
            //    }
            //}
            //catch
            //{
            //    return;
            //}

            //if (firing == null)
            //{
            //    return;
            //}

            //lock (somDrawLock)
            //{
            //    // Prepare bitmap
            //    var bmp = new Bitmap(targetPictureBox.Width, targetPictureBox.Height);
            //    using (var g = Graphics.FromImage(bmp))
            //    {
            //        g.SmoothingMode = SmoothingMode.HighSpeed;
            //        g.Clear(Color.Black);

            //        // Use dynamic width based on region type
            //        double cellW = (double)bmp.Width / somWidth;
            //        double cellH = (double)bmp.Height / SOM_Y;

            //        // Draw firing neurons
            //        foreach (var pos in firing.ActiveBits)
            //        {
            //            int x = pos.X;
            //            int y = pos.Y;

            //            // Bounds check with dynamic width
            //            if (x < 0 || x >= somWidth || y < 0 || y >= SOM_Y)
            //                continue;

            //            var rect = new RectangleF(
            //                (float)(x * cellW),
            //                (float)(y * cellH),
            //                (float)Math.Ceiling(cellW),
            //                (float)Math.Ceiling(cellH));

            //            // Color scheme based on region type for visual distinction:
            //            Brush brush = regionType switch
            //            {
            //                LearningUnitType.V1 => Brushes.Lime,      // Green for V1
            //                LearningUnitType.V2 => Brushes.Cyan,      // Cyan for V2  
            //                LearningUnitType.V3 => Brushes.Yellow,    // Yellow for V3
            //                _ => Brushes.Lime
            //            };

            //            g.FillRectangle(brush, rect);
            //        }
            //    }

            //    // Swap image
            //    var old = targetPictureBox.Image;
            //    targetPictureBox.Image = bmp;
            //    targetPictureBox.Refresh();
            //    old?.Dispose();
            //}
        }

        /*
         * StartButton Legacy logic 
         * label_done.Text = "Processing";
            label_done.Refresh();

            if (string.IsNullOrEmpty(objectBox.Text))
            {
                label_done.Text = "Enter object label before you train!!";
                return;
            }

            if (objectList.Contains(objectBox.Text))
            {
                label_done.Text = "Object Already Trained!!";
                return;
            }

            objectList.Add(objectBox.Text);

            if (networkMode.Equals(NetworkMode.TRAINING))
            {

                // Process visual data for all regions simultaneously
                var bmp_g = ConverToEdgedBitmap((Bitmap)pictureBox2.Image);

                // Fix: Orchestrator.SetUpLabel signature is (Bitmap bmp, string objectLabel)
                orchestrator.SetupLabel(bmp_g, objectBox.Text);

                UpdateUI(orchestrator.VisionProcessor.SomSDR.ActiveBits);

                orchestrator.TrainImage();

                // WORK IN PROGRESS

                CycleLabel.Text = counter.ToString();

                CycleLabel.Refresh();
            }

            startClassificationButton.Visible = true;

            if (networkMode == NetworkMode.TRAINING)
            {
                StartButton.Text = "Start Another Image";
                StartButton.Refresh();
                label_done.Text = "Completed!"; label_done.Refresh();
            }
         */

        private void CurrentImage_Click(object sender, EventArgs e)
        {

        }


        private void openFileDialog1_FileOk(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var dlg = sender as OpenFileDialog ?? openFileDialog1;
            var file = dlg?.FileName;
            if (string.IsNullOrEmpty(file) || !System.IO.File.Exists(file))
                return;

            // Load via stream and clone to avoid locking the source file
            using var fs = new System.IO.FileStream(file, System.IO.FileMode.Open, System.IO.FileAccess.Read);
            using var img = Image.FromStream(fs);
            var bmp = new Bitmap(img);

            // Dispose previous image to avoid leaks
            var old = pictureBox2.Image;
            pictureBox2.Image = bmp;
            old?.Dispose();

            pictureBox2.SizeMode = PictureBoxSizeMode.Zoom;
        }      
    }
}
