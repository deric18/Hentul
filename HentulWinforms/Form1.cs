namespace HentulWinforms
{
    using Common;
    using Hentul;
    using OpenCvSharp;
    using OpenCvSharp.Extensions;
    using System.Drawing.Drawing2D;
    using static System.Windows.Forms.VisualStyles.VisualStyleElement.ToolTip;

    public partial class Form1 : Form
    {
        Orchestrator orchestrator;
        NetworkMode networkMode;
        readonly int numPixels = 10;
        ulong counter = 0;
        int imageIndex = 0;
        int totalImagesToProcess = 1;
        List<string> objectList = new(); 
        private PictureBox V2GrayscaleImage, V2WhitescaleImage;
        private PictureBox V3GrayscaleImage, V3WhitescaleImage;

        // LT : 784,367   RT: 1414,367  LB : 784, 1034   RB: 1414, 1034
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
            backupDirHC = Path.GetFullPath(Path.Combine(baseDir, @"..\..\..\..\..\Hentul\Hentul\BackUp\HC-EC\"));
            backupDirFOM = Path.GetFullPath(Path.Combine(baseDir, @"..\..\..\..\..\Hentul\Hentul\BackUp\FOM\"));
            backupDirSOM = Path.GetFullPath(Path.Combine(baseDir, @"..\..\..\..\..\Hentul\Hentul\BackUp\SOM\"));
            networkMode = NetworkMode.TRAINING;
            train_another_object.Visible = false;
        }

        private void StartButton_Click(object sender, EventArgs e)
        {
            label_done.Text = "Processing";
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

            if (counter > 0)
            {
                orchestrator.LearnNewObject(objectBox.Text);
            }
            else
            {
                orchestrator.BeginTraining(objectBox.Text);
            }

            var value = LeftTop;
            value.X = value.X + numPixels;
            value.Y = value.Y + numPixels;

            orchestrator.MoveCursor(value);

            labelX.Text = value.X.ToString();
            labelY.Text = value.Y.ToString();

            while (true)
            {
                if (value.X >= RightTop.X - numPixels && value.Y >= RightBottom.Y - numPixels)
                {
                    label_done.Text = "Finished Processing Image";
                    break;
                }
                else
                {
                    if (value.X <= RightTop.X - numPixels)
                        value = MoveRight(value);
                    else
                    {
                        if (value.Y <= RightBottom.Y - numPixels)
                        {
                            value = MoveDown(value);
                            value = SetLeft(value);
                        }
                        else
                        {
                            if (value.X >= RightTop.X - numPixels && value.Y >= RightBottom.Y - numPixels)
                            {
                                label_done.Text = "Finished Processing Image";
                                label_done.Refresh();
                                train_another_object.Visible = true;
                                break;
                            }
                        }
                    }
                }

                if (networkMode.Equals(NetworkMode.TRAINING))
                {
                    labelX.Text = value.X.ToString();
                    labelX.Refresh();
                    labelY.Text = value.Y.ToString();
                    labelY.Refresh();

                    orchestrator.MoveCursor(value);

                    // Record pixels for all regions (V1, V2, V3)
                    orchestrator.RecordPixels(); // This now captures all three scales internally

                    // Update V1 displays (existing)
                    CurrentImage.Image = orchestrator.bmp;
                    CurrentImage.Refresh();

                    EdgedImage.Image = ConverToEdgedBitmap(orchestrator.bmp);
                    EdgedImage.Refresh();

                    // Update V2 displays (add these PictureBoxes to designer)
                    if (pictureBoxV2Grayscale != null && orchestrator.VisionProcessor.bmpV2 != null)
                    {
                        pictureBoxV2Grayscale.Image = ConverToEdgedBitmap(orchestrator.VisionProcessor.bmpV2);
                        pictureBoxV2Grayscale.Refresh();
                    }

                    if (pictureBoxV2Whitescale != null && orchestrator.VisionProcessor.bmpV2 != null)
                    {
                        pictureBoxV2Whitescale.Image = ConvertToWhitescale(orchestrator.VisionProcessor.bmpV2);
                        pictureBoxV2Whitescale.Refresh();
                    }

                    // Update V3 displays (add these PictureBoxes to designer)
                    if (pictureBoxV2Grayscale != null && orchestrator.VisionProcessor.bmpV3 != null)
                    {
                        pictureBoxV2Grayscale.Image = ConverToEdgedBitmap(orchestrator.VisionProcessor.bmpV3);
                        pictureBoxV2Grayscale.Refresh();
                    }

                    if (pictureBoxV3Whitescale != null && orchestrator.VisionProcessor.bmpV3 != null)
                    {
                        pictureBoxV3Whitescale.Image = ConvertToWhitescale(orchestrator.VisionProcessor.bmpV3);
                        pictureBoxV3Whitescale.Refresh();
                    }

                    // Process visual data for all regions simultaneously
                    var edgedBitmap = ConverToEdgedBitmap(orchestrator.bmp);
                    orchestrator.ProcessVisual(edgedBitmap, counter++); // This now processes V1, V2, V3

                    CycleLabel.Text = counter.ToString();
                    CycleLabel.Refresh();
                }

                // Draw SOM layers for all regions
                DrawAllSomLayers();

                //DrawFomLayers(); // Dont want to print 100 FOM pictureBoxes.
            }

            if (label_done.Text == "Finished Processing Image")
            {
                startClassificationButton.Visible = true;

                if (networkMode == NetworkMode.TRAINING)
                {
                    StartButton.Text = "Start Another Image";
                    StartButton.Refresh();
                }
            }
        }
        
        private void UpdateImageDisplays()
        {
            // Update V1 displays (existing)
            CurrentImage.Image = orchestrator.bmp;
            EdgedImage.Image = ConverToEdgedBitmap(orchestrator.bmp);

            // Update V2 displays (new)
            V2GrayscaleImage.Image = ConverToEdgedBitmap(orchestrator.bmpV2);
            V2WhitescaleImage.Image = ConvertToWhitescale(orchestrator.bmpV2);

            // Update V3 displays (new)  
            V3GrayscaleImage.Image = ConverToEdgedBitmap(orchestrator.bmpV3);
            V3WhitescaleImage.Image = ConvertToWhitescale(orchestrator.bmpV3);
        }
        // Add these helper methods for image conversion
        private Bitmap ConvertToGrayscale(Bitmap source)
        {
            if (source == null) return null;

            var result = new Bitmap(source.Width, source.Height);

            for (int y = 0; y < source.Height; y++)
            {
                for (int x = 0; x < source.Width; x++)
                {
                    var color = source.GetPixel(x, y);
                    var gray = (int)(color.R * 0.3 + color.G * 0.59 + color.B * 0.11);
                    var grayColor = Color.FromArgb(gray, gray, gray);
                    result.SetPixel(x, y, grayColor);
                }
            }

            return result;
        }

        private Bitmap ConvertToWhitescale(Bitmap source)
        {
            if (source == null) return null;

            var result = new Bitmap(source.Width, source.Height);
            byte threshold = 200; // Whitescale threshold

            for (int y = 0; y < source.Height; y++)
            {
                for (int x = 0; x < source.Width; x++)
                {
                    var color = source.GetPixel(x, y);
                    var gray = (int)(color.R * 0.3 + color.G * 0.59 + color.B * 0.11);

                    bool isWhite = gray >= threshold;
                    var whiteColor = isWhite ? Color.White : Color.Black;
                    result.SetPixel(x, y, whiteColor);
                }
            }

            return result;
        }
        private void startClassificationButton_Click(object sender, EventArgs e)
        {
            label_done.Text = "Procesing";
            label_done.Refresh();

            var value = LeftTop;
            value.X = value.X + numPixels;
            value.Y = value.Y + numPixels;

            orchestrator.MoveCursor(value);

            labelX.Text = value.X.ToString();
            labelY.Text = value.Y.ToString();

            networkMode = NetworkMode.PREDICTION;
            orchestrator.ChangeNetworkModeToPrediction();
            ObjectLabel.Visible = true;

            labelX.Text = value.X.ToString();
            labelY.Text = value.Y.ToString();

            while (true)
            {
                if (value.X >= RightTop.X - numPixels && value.Y >= RightBottom.Y - numPixels)
                {
                    label_done.Text = "Reached End Of Image";
                    UpdatePredictions();

                    break;
                }
                else
                {
                    if (value.X <= RightTop.X - numPixels)
                        value = MoveRight(value);
                    else
                    {
                        if (value.Y <= RightBottom.Y - numPixels)
                        {
                            value = MoveDown(value);
                            value = SetLeft(value);
                        }
                        else
                        {
                            if (value.X >= RightTop.X - numPixels && value.Y >= RightBottom.Y - numPixels)
                            {
                                label_done.Text = "Reached End Of Image"; label_done.Refresh();
                                UpdatePredictions();

                                break;
                            }
                        }
                    }
                }

                orchestrator.MoveCursor(value);

                orchestrator.RecordPixels();        //Grab Image

                CurrentImage.Image = orchestrator.bmp; CurrentImage.Refresh();

                EdgedImage.Image = ConverToEdgedBitmap(orchestrator.bmp); EdgedImage.Refresh();

                orchestrator.ProcessVisual(ConverToEdgedBitmap(orchestrator.bmp), counter++);     // Fire FOMS per image

                networkMode = orchestrator.VisionProcessor.v1.somBBM_L3B_V.NetWorkMode;

                if (networkMode == NetworkMode.TRAINING)
                {
                    throw new InvalidOperationException("Mode should be in Prediction or Done!");
                }
                else if (networkMode == NetworkMode.DONE)
                {
                    label_done.Text = "Classification Done!"; label_done.Refresh();

                    var predictions = orchestrator.GetPredictionsVisual();    // Fire SOM per FOMS
                    string val = string.Empty;
                    foreach (var pred in predictions)
                    {
                        val = pred.ToString();
                    }
                    ObjectLabel.Text = val;
                    ObjectLabel.Refresh();

                    break;
                }

                DrawSomLayer();
            }
        }

        private void UpdatePredictions()
        {
            train_another_object.Visible = true;
            var predictions = orchestrator.GetPredictionsVisual();    // Fire SOM per FOMS
            string val = string.Empty;
            predictions.ForEach(x => val += x.ToString() + " | ");

            ObjectLabel.Text = val; ObjectLabel.Refresh();
        }

        private void WanderingButton_Click(object sender, EventArgs e)
        {
            StartBurstAvoidance();
        }

        private void StartBurstAvoidance()
        {
            orchestrator.StartBurstAvoidanceWandering(100);
        }

        public Orchestrator.POINT MoveRight(Orchestrator.POINT value)
        {
            value.X = value.X + numPixels * 2;
            return value;
        }

        public Orchestrator.POINT MoveDown(Orchestrator.POINT value)
        {
            value.Y = value.Y + numPixels * 2;
            return value;
        }

        public Orchestrator.POINT SetLeft(Orchestrator.POINT value)
        {
            value.X = value.X - Math.Abs(LeftTop.X - RightTop.X) + numPixels;
            return value;
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
                var value = orchestrator.GetCurrentPointerPosition();
                labelX.Text = value.X.ToString();
                labelY.Text = value.Y.ToString();

                var standard = LeftTop;
                standard.X = standard.X + numPixels;
                standard.Y = standard.Y + numPixels;

                orchestrator.MoveCursor(standard);

                value = orchestrator.GetCurrentPointerPosition();
                labelX.Text = value.X.ToString();
                labelY.Text = value.Y.ToString();

                orchestrator.RecordPixels();

                CurrentImage.Image = orchestrator.bmp;
                CurrentImage.Refresh();

                EdgedImage.Image = ConverToEdgedBitmap(orchestrator.bmp);
                EdgedImage.Refresh();

                label_done.Text = "Ready";

                // Optional: Initialize V2/V3 in background for later use
                // Uncomment these lines if you want V2/V3 available:
                /*
                _ = Task.Run(() =>
                {
                    orchestrator.InitializeV2V3IfNeeded();
                    this.Invoke(() => label_done.Text = "All regions ready");
                });
                */
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
            CurrentImage.Image = orchestrator.bmp;
            string filename = Path.GetFullPath(Path.Combine(baseDir, @"..\..\..\..\..\Hentul\Hentul\Images\savedImage.png"));
            orchestrator.bmp.Save(filename);

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
            LearningUnit learningUnit;
            PictureBox targetPictureBox;
            int somWidth;

            switch (regionType)
            {
                case LearningUnitType.V1:
                    learningUnit = orchestrator?.VisionProcessor?.v1;
                    targetPictureBox = pictureBox1;
                    somWidth = SOM_X_V1;
                    break;
                case LearningUnitType.V2:
                    learningUnit = orchestrator?.VisionProcessor?.v2;
                    targetPictureBox = pictureBoxV2Som; // Add this to designer
                    somWidth = SOM_X_V2;
                    break;
                case LearningUnitType.V3:
                    learningUnit = orchestrator?.VisionProcessor?.v3;
                    targetPictureBox = pictureBoxV3Som; // Add this to designer
                    somWidth = SOM_X_V3;
                    break;
                default:
                    return;
            }

            // Null checks
            if (orchestrator == null ||
                orchestrator.VisionProcessor == null ||
                learningUnit == null ||
                learningUnit.somBBM_L3B_V == null ||
                targetPictureBox == null ||
                targetPictureBox.IsDisposed)
            {
                return;
            }

            var somMgr = learningUnit.somBBM_L3B_V;

            // Attempt to get latest firing neurons
            SDR_SOM firing = null;
            try
            {
                var cycle = somMgr.CycleNum;
                firing = somMgr.GetAllNeuronsFiringLatestCycle(cycle);

                if (firing.ActiveBits.Count != 0)
                {
                    bool breakpoint = true;
                }
            }
            catch
            {
                return;
            }

            if (firing == null)
            {
                return;
            }

            lock (somDrawLock)
            {
                // Prepare bitmap
                var bmp = new Bitmap(targetPictureBox.Width, targetPictureBox.Height);
                using (var g = Graphics.FromImage(bmp))
                {
                    g.SmoothingMode = SmoothingMode.HighSpeed;
                    g.Clear(Color.Black);

                    // Use dynamic width based on region type
                    double cellW = (double)bmp.Width / somWidth;
                    double cellH = (double)bmp.Height / SOM_Y;

                    // Draw firing neurons
                    foreach (var pos in firing.ActiveBits)
                    {
                        int x = pos.X;
                        int y = pos.Y;

                        // Bounds check with dynamic width
                        if (x < 0 || x >= somWidth || y < 0 || y >= SOM_Y)
                            continue;

                        var rect = new RectangleF(
                            (float)(x * cellW),
                            (float)(y * cellH),
                            (float)Math.Ceiling(cellW),
                            (float)Math.Ceiling(cellH));

                        // Color scheme based on region type for visual distinction:
                        Brush brush = regionType switch
                        {
                            LearningUnitType.V1 => Brushes.Lime,      // Green for V1
                            LearningUnitType.V2 => Brushes.Cyan,      // Cyan for V2  
                            LearningUnitType.V3 => Brushes.Yellow,    // Yellow for V3
                            _ => Brushes.Lime
                        };

                        g.FillRectangle(brush, rect);
                    }
                }

                // Swap image
                var old = targetPictureBox.Image;
                targetPictureBox.Image = bmp;
                targetPictureBox.Refresh();
                old?.Dispose();
            }
        }

        // -------------------- FOM (Layer 4) Visualization Logic --------------------

        private void DrawFomLayers()
        {
            if (orchestrator?.VisionProcessor?.v1 == null || fomPictureBoxes == null)
                return;

            // Ensure designer created these controls: pictureBox2 .. pictureBox11
            try
            {
                fomPictureBoxes = new[]
                {
                    pictureBox2, pictureBox3, pictureBox4, pictureBox5, pictureBox6,
                    pictureBox7, pictureBox8, pictureBox9, pictureBox10, pictureBox11
                };
            }
            catch
            {
                // Ignore if not all present yet
            }

            var lu = orchestrator.VisionProcessor.v1;
            var cycle = lu.CycleNum;

            // Loop through first N FOM BBMs mapped to picture boxes
            int count = Math.Min(lu.fomBBMV.Length, fomPictureBoxes.Length);

            for (int i = 0; i < count; i++)
            {
                var pb = fomPictureBoxes[i];
                if (pb == null || pb.IsDisposed) continue;

                var fom = lu.fomBBMV[i];
                SDR_SOM firing = null;
                SDR_SOM bursting = null;

                try
                {
                    firing = fom.GetAllNeuronsFiringLatestCycle(cycle, true);
                    bursting = fom.GetAllColumnsBurstingLatestCycle(cycle);
                }
                catch
                {
                    continue;
                }

                var bmp = new Bitmap(pb.Width, pb.Height);
                using (var g = Graphics.FromImage(bmp))
                {
                    g.SmoothingMode = SmoothingMode.HighSpeed;
                    g.Clear(Color.Black);

                    // FOM columns are 10 x 10
                    const int GRID_X = 10;
                    const int GRID_Y = 10;

                    double cellW = (double)bmp.Width / GRID_X;
                    double cellH = (double)bmp.Height / GRID_Y;

                    // Cache burst positions
                    HashSet<(int X, int Y)> burstSet = new();
                    if (bursting != null)
                    {
                        foreach (var b in bursting.ActiveBits)
                        {
                            burstSet.Add((b.X, b.Y));
                        }
                    }

                    // Draw firing neurons
                    if (firing != null)
                    {
                        foreach (var pos in firing.ActiveBits)
                        {
                            if (pos.X < 0 || pos.X >= GRID_X || pos.Y < 0 || pos.Y >= GRID_Y) continue;

                            var rect = new RectangleF(
                                (float)(pos.X * cellW),
                                (float)(pos.Y * cellH),
                                (float)Math.Ceiling(cellW),
                                (float)Math.Ceiling(cellH));

                            // Fill firing
                            g.FillRectangle(Brushes.Lime, rect);

                            // Highlight burst
                            if (burstSet.Contains((pos.X, pos.Y)))
                            {
                                using var pen = new Pen(Color.OrangeRed, Math.Max(1f, (float)(cellW * 0.15)));
                                g.DrawRectangle(pen, rect.X, rect.Y, rect.Width, rect.Height);
                            }
                        }
                    }

                    // Optional: draw grid (light)
                    using (var gridPen = new Pen(Color.FromArgb(30, 255, 255, 255), 1f))
                    {
                        for (int gx = 0; gx <= GRID_X; gx++)
                        {
                            float x = (float)(gx * cellW);
                            g.DrawLine(gridPen, x, 0, x, bmp.Height);
                        }
                        for (int gy = 0; gy <= GRID_Y; gy++)
                        {
                            float y = (float)(gy * cellH);
                            g.DrawLine(gridPen, 0, y, bmp.Width, y);
                        }
                    }
                }

                var old = pb.Image;
                pb.Image = bmp;
                pb.Refresh();
                old?.Dispose();
            }
        }
    }
}
