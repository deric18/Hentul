using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Common;
using Hentul;
using OpenCvSharp;
using OpenCvSharp.Extensions;

namespace HentulWinforms
{
    public partial class Form1 : Form
    {
        Orchestrator orchestrator;
        NetworkMode networkMode;
        readonly int numPixels = 10;
        ulong counter = 0;
        int imageIndex = 0;
        int totalImagesToProcess = 1;
        List<string> objectList = new();

        // LT : 784,367   RT: 1414,367  LB : 784, 1034   RB: 1414, 1034
        Orchestrator.POINT LeftTop = new Orchestrator.POINT();
        Orchestrator.POINT RightTop = new Orchestrator.POINT();
        Orchestrator.POINT LeftBottom = new Orchestrator.POINT();
        Orchestrator.POINT RightBottom = new Orchestrator.POINT();

        // --- backup dirs: keep LOCAL dynamic paths; drop master’s hard-coded absolute paths
        string baseDir = AppContext.BaseDirectory;
        string backupDirHC;
        string backupDirFOM;
        string backupDirSOM;

        // --- SOM Visualization Fields ---        
        private const int SOM_X = 1250;   // Matches LearningUnit X
        private const int SOM_Y = 10;     // Matches NumColumns
        private readonly object somDrawLock = new();

        // --- FOM (Layer 4) Visualization Fields ---
        private PictureBox[] fomPictureBoxes;  // pictureBox2 .. pictureBox11

        public Form1()
        {
            InitializeComponent();

            // build portable backup paths relative to the app base dir
            backupDirHC = Path.GetFullPath(Path.Combine(baseDir, @"..\..\..\..\..\")); // repo root
            backupDirFOM = Path.GetFullPath(Path.Combine(baseDir, @"..\..\..\..\..\Hentul\Hentul\BackUp\FOM\"));
            backupDirSOM = Path.GetFullPath(Path.Combine(baseDir, @"..\..\..\..\..\Hentul\Hentul\BackUp\SOM\"));

            networkMode = NetworkMode.TRAINING;
            train_another_object.Visible = false;
        }

        private void UpdateCursorLabels(Orchestrator.POINT value)
        {
            // LOCAL training guardrails + begin/learn flow
            label_done.Text = "Procesing";
            label_done.Refresh();            

            if (counter > 0)
            {
                orchestrator.LearnNewObject(objectBox.Text);
            }
            else
            {
                orchestrator.BeginTraining(objectBox.Text);
            }

            var next = LeftTop;
            next.X = next.X + numPixels;
            next.Y = next.Y + numPixels;
            orchestrator.MoveCursor(next);

            // include master’s useful UI label updates
            labelX.Text = value.X.ToString();
            labelY.Text = value.Y.ToString();
            labelX.Refresh();
            labelY.Refresh();
        }

        private Orchestrator.POINT GetNextCursorPosition(Orchestrator.POINT current)
        {
            if (current.X <= RightTop.X - numPixels)
                return MoveRight(current);
            else if (current.Y <= RightBottom.Y - numPixels)
            {
                var next = MoveDown(current);
                return SetLeft(next);
            }
            return current;
        }

        private void StartButton_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(objectBox.Text))
            {
                label_done.Text = "Enter object label before you train!!"; label_done.Refresh();
                return;
            }

            if (objectList.Contains(objectBox.Text))
            {
                label_done.Text = "Object Already Trained!!"; label_done.Refresh();
                return;
            }

            objectList.Add(objectBox.Text);

            label_done.Text = "Processing";
            label_done.Refresh();

            var value = new Orchestrator.POINT
            {
                X = LeftTop.X + numPixels,
                Y = LeftTop.Y + numPixels
            };

            orchestrator.MoveCursor(value);
            UpdateCursorLabels(value);

            bool finished = false;

            while (!finished)
            {
                // Stop when we reach end of scan region
                if (value.X >= RightTop.X - numPixels && value.Y >= RightBottom.Y - numPixels)
                {
                    finished = true;
                    label_done.Text = "Finished Processing Image";
                    label_done.Refresh();
                    train_another_object.Visible = true;
                    break;
                }

                // Move cursor across the grid
                value = GetNextCursorPosition(value);

                // Update UI labels and move pointer
                UpdateCursorLabels(value);
                orchestrator.MoveCursor(value);

                // Record multi-scale pixels (V1/V2/V3)
                var (v1, v2, v3) = orchestrator.RecordPixels();

                // Only process every Nth frame (perf)
                if (!orchestrator.ShouldProcessThisFrame)
                    continue;

                // Display processed frames (grayscale & whitescale)
                V1Gray.Image = ToGray(v1.Processed);
                V1White.Image = ToWhiteScale(v1.Processed);
                V1Gray.Refresh();

                V2Gray.Image = ToGray(v2.Processed);
                V2White.Image = ToWhiteScale(v2.Processed);
                V2Gray.Refresh();

                V3Gray.Image = ToGray(v3.Processed);
                V3White.Image = ToWhiteScale(v3.Processed);
                V3Gray.Refresh();

                // Display raw-edge previews (for quick inspection)
                V1White.Image = ConverToEdgedBitmap(v1.Raw);
                V1White.Refresh();

                V2White.Image = ConverToEdgedBitmap(v2.Raw);
                V2White.Refresh();

                V3White.Image = ConverToEdgedBitmap(v3.Raw);
                V3White.Refresh();

                // Edge inputs fed to HTM pipeline (normalized 40x20 already done by Orchestrator)
                var v1Edge = ConverToEdgedBitmap(v1.Processed);
                var v2Edge = ConverToEdgedBitmap(v2.Processed);
                var v3Edge = ConverToEdgedBitmap(v3.Processed);

                var didProcess = orchestrator.ProcessVisual(v1Edge, v2Edge, v3Edge);

                if (didProcess && networkMode == NetworkMode.TRAINING)
                {
                    orchestrator.AddNewVisualSensationToHc();
                    counter++;
                    CycleLabel.Text = counter.ToString();
                    CycleLabel.Refresh();
                }
                else if (didProcess && networkMode == NetworkMode.PREDICTION)
                {
                    var motorOutput = orchestrator.Verify_Predict_HC();
                    if (motorOutput != null)
                    {
                        if (motorOutput.X == int.MaxValue && motorOutput.Y == int.MaxValue)
                        {
                            var obj = orchestrator.GetPredictedObject();
                            ObjectLabel.Text = obj.Label;
                            ObjectLabel.Refresh();

                            label_done.Text = "Object Recognised!";
                            label_done.Refresh();
                            train_another_object.Visible = true;
                            wanderingButton.Visible = true;
                            finished = true; // stop scanning
                        }
                        else if (motorOutput.X == int.MinValue && motorOutput.Y == int.MinValue)
                        {
                            label_done.Text = "Object Could Not be Recognised!";
                            label_done.Refresh();
                            finished = true;
                        }
                        else
                        {
                            orchestrator.MoveCursor(new Orchestrator.POINT { X = motorOutput.X, Y = motorOutput.Y });
                        }
                    }
                }

                // Optional visualizations
                DrawSomLayer();
                // DrawFomLayers(); // uncomment if you want to draw many FOM PBs
            }

            // After loop completes
            if (networkMode == NetworkMode.TRAINING)
            {
                imageIndex++;
                if (imageIndex == totalImagesToProcess)
                {
                    StartButton.Text = "Test Classification Algo";
                    StartButton.Refresh();
                    orchestrator.ChangeNetworkToPredictionMode();
                    networkMode = NetworkMode.PREDICTION;
                    BackUp.Visible = true;
                    orchestrator.MoveCursor(LeftTop);
                }
                else
                {
                    orchestrator.DoneWithTraining();
                    startClassificationButton.Visible = true;
                }
            }
        }

        private void WanderingButton_Click(object sender, EventArgs e)
        {
            StartBurstAvoidance();
        }

        // (from master) keep this as a separate handler as well
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

                var (V1, V2, V3) = orchestrator.RecordPixels();

                // show current image and its edges (use any of the three; here V1 raw)
                CurrentImage.Image = V1.Raw; CurrentImage.Refresh();
                EdgedImage.Image = ConverToEdgedBitmap(V1.Raw); EdgedImage.Refresh();

                // feed edged processed frames
                var didProcess = orchestrator.ProcessVisual(
                    ConverToEdgedBitmap(V1.Processed),
                    ConverToEdgedBitmap(V2.Processed),
                    ConverToEdgedBitmap(V3.Processed)
                );

                // Note: this branch was originally wired to somBBM_L3B_V.NetWorkMode in master
                // Keep prediction loop simple here; SOM visual draw below:
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
        
        private void button1_Click(object sender, EventArgs e)
        {
            label_done.Text = "Innitting...";
            label_done.Refresh();
            networkMode = NetworkMode.TRAINING;
            orchestrator = Orchestrator.GetInstance();

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

            var (V1, V2, V3) = orchestrator.RecordPixels();

            // Show one of them (medium/Raw is usually good for display)
            V1Gray.Image = V1.Raw;
            V1Gray.Refresh();

            // Apply edge detection to the same image
            V1White.Image = ConverToEdgedBitmap(V1.Raw);
            V1White.Refresh();

            label_done.Text = "Ready";
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            LeftTop.X = 954; LeftTop.Y = 416;
            RightTop.X = 1596; RightTop.Y = LeftTop.Y;
            LeftBottom.X = LeftTop.X; LeftBottom.Y = 1116;
            RightBottom.X = RightTop.X; RightBottom.Y = LeftBottom.Y - 500;

            label_done.Text = "Ready";
            wanderingButton.Visible = false;
            BackUp.Visible = false;
            startClassificationButton.Visible = false;
            Restore.Visible = false;
            BackUp.Visible = false;
        }

        private Bitmap ConverToEdgedBitmap(Bitmap incoming)
        {
            if (incoming == null)
                throw new ArgumentNullException(nameof(incoming), "ConverToEdgedBitmap: incoming bitmap was null.");

            // In-memory OpenCV Canny (no disk I/O)
            using var src = BitmapConverter.ToMat(incoming);
            using var edges = new Mat();
            Cv2.Canny(src, edges, 50, 200);
            return BitmapConverter.ToBitmap(edges);
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

        private void textBox1_TextChanged(object sender, EventArgs e) { }

        private void label2_Click(object sender, EventArgs e) { }

        private Bitmap ToGray(Bitmap src)
        {
            using var m = BitmapConverter.ToMat(src);
            using var gray = new Mat();
            Cv2.CvtColor(m, gray, ColorConversionCodes.BGR2GRAY);
            return BitmapConverter.ToBitmap(gray);
        }

        // “whitescale” = binary (black/white)
        private Bitmap ToWhiteScale(Bitmap src)
        {
            using var m = BitmapConverter.ToMat(src);
            using var gray = new Mat();
            using var bin = new Mat();
            Cv2.CvtColor(m, gray, ColorConversionCodes.BGR2GRAY);
            // Otsu picks threshold automatically; BinaryInv if you prefer white foreground
            Cv2.Threshold(gray, bin, 0, 255, ThresholdTypes.Binary | ThresholdTypes.Otsu);
            return BitmapConverter.ToBitmap(bin);
        }

        // -------------------- SOM Visualization Logic --------------------        
        private void DrawSomLayer()
        {
            if (orchestrator == null ||
                orchestrator.VisionProcessor == null ||
                orchestrator.VisionProcessor.v1 == null ||
                orchestrator.VisionProcessor.v1.somBBM_L3B_V == null ||
                pictureBox1.IsDisposed)
            {
                return;
            }

            var somMgr = orchestrator.VisionProcessor.v1.somBBM_L3B_V;

            // Attempt to get latest firing neurons (CycleNum + 1 pattern consistent with tests)
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
                var bmp = new Bitmap(pictureBox1.Width, pictureBox1.Height);
                using (var g = Graphics.FromImage(bmp))
                {
                    g.SmoothingMode = SmoothingMode.HighSpeed;
                    g.Clear(Color.Black);

                    double cellW = (double)bmp.Width / SOM_X;
                    double cellH = (double)bmp.Height / SOM_Y;

                    // Draw firing neurons
                    foreach (var pos in firing.ActiveBits)
                    {
                        int x = pos.X;
                        int y = pos.Y;
                        if (x < 0 || x >= SOM_X || y < 0 || y >= SOM_Y) continue;

                        var rect = new RectangleF(
                            (float)(x * cellW),
                            (float)(y * cellH),
                            (float)Math.Ceiling(cellW),
                            (float)Math.Ceiling(cellH));

                        // Lime = normal firing
                        g.FillRectangle(Brushes.Lime, rect);
                    }
                }

                // Swap image
                var old = pictureBox1.Image;
                pictureBox1.Image = bmp;
                pictureBox1.Refresh();
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
