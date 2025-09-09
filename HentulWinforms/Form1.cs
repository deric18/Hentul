
namespace HentulWinforms
{
    using Common;
    using Hentul;
    using OpenCvSharp;
    using OpenCvSharp.Extensions;
    using static System.Windows.Forms.VisualStyles.VisualStyleElement.ToolTip;
    using System.Windows.Forms;

    public partial class Form1 : Form
    {
        Orchestrator orchestrator;
        NetworkMode networkMode;
        readonly int numPixels = 10;
        int counter = 0;
        int imageIndex = 0;
        int totalImagesToProcess = 1;

        // LT : 784,367   RT: 1414,367  LB : 784, 1034   RB: 1414, 1034
        Orchestrator.POINT LeftTop = new Orchestrator.POINT();
        Orchestrator.POINT RightTop = new Orchestrator.POINT();
        Orchestrator.POINT LeftBottom = new Orchestrator.POINT();
        Orchestrator.POINT RightBottom = new Orchestrator.POINT();

        string baseDir = AppContext.BaseDirectory;
        string backupDirHC;
        string backupDirFOM;
        string backupDirSOM;
        

        public Form1()
        {
            InitializeComponent();
            backupDirHC = Path.GetFullPath(Path.Combine(baseDir, @"..\..\..\..\..\"));
            backupDirFOM = Path.GetFullPath(Path.Combine(baseDir, @"..\..\..\..\..\Hentul\Hentul\BackUp\FOM\"));
            backupDirSOM = Path.GetFullPath(Path.Combine(baseDir, @"..\..\..\..\..\Hentul\Hentul\BackUp\SOM\"));
            networkMode = NetworkMode.TRAINING;
            train_another_object.Visible = false;
        }
        private void UpdateCursorLabels(Orchestrator.POINT value)
        {
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
                // Check if finished
                if (value.X >= RightTop.X - numPixels && value.Y >= RightBottom.Y - numPixels)
                {
                    finished = true;
                    label_done.Text = "Finished Processing Image";
                    label_done.Refresh();
                    train_another_object.Visible = true;
                    break;
                }

                // Move cursor
                value = GetNextCursorPosition(value);

                // Update UI
                UpdateCursorLabels(value);
                orchestrator.MoveCursor(value);

                // Record pixels
                var (v1, v2, v3) = orchestrator.RecordPixels();

                // Update display images
                //V1Gray.Image = v1.Raw; // Show medium version
                //V1Gray.Refresh();

                // >>> Only do edges + HTM on Nth frames
                if (!orchestrator.ShouldProcessThisFrame)
                    continue;

                V1Gray.Image = ToGray(v1.Processed);
                V1White.Image = ToWhiteScale(v1.Processed);
                V1Gray.Refresh();

                V2Gray.Image = ToGray(v2.Processed);
                V2White.Image = ToWhiteScale(v2.Processed);
                V2Gray.Refresh();

                V3Gray.Image = ToGray(v3.Processed);
                V3White.Image = ToWhiteScale(v3.Processed);
                V3Gray.Refresh();

                V1White.Image = ConverToEdgedBitmap(v1.Raw);
                V1White.Refresh();

                V2White.Image = ConverToEdgedBitmap(v2.Raw);
                V2White.Refresh();

                V3White.Image = ConverToEdgedBitmap(v3.Raw);
                V3White.Refresh();


                // Process visuals

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
                            finished = true; // optional: break out here too
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

            }

            // Final processing after loop
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
                    StartButton.Text = "Start Another Image";
                    StartButton.Refresh();
                }
            }
        }


        private void wanderingButton_Click(object sender, EventArgs e)
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
        {   //RT : 1575, LB : 1032
            LeftTop.X = 954; LeftTop.Y = 416; RightTop.X = 1596; RightTop.Y = LeftTop.Y; LeftBottom.X = LeftTop.X; LeftBottom.Y = 1116; RightBottom.X = RightTop.X; RightBottom.Y = LeftBottom.Y;
            label_done.Text = "Ready";
            wanderingButton.Visible = false;
            BackUp.Visible = false;

            if (Directory.GetFiles(backupDirHC).Length == 0 || Directory.GetFiles(backupDirFOM).Length == 0 || Directory.GetFiles(backupDirSOM).Length == 0)
                Restore.Visible = false;
            else
                Restore.Visible = true;
        }

        private Bitmap ConverToEdgedBitmap(Bitmap incoming)
        {
            if (incoming == null)
                throw new ArgumentNullException(nameof(incoming), "ConverToEdgedBitmap: incoming bitmap was null.");

            // Convert Bitmap -> Mat (no disk I/O)
            using var src = OpenCvSharp.Extensions.BitmapConverter.ToMat(incoming);
            using var edges = new OpenCvSharp.Mat();

            // Canny edge detection
            OpenCvSharp.Cv2.Canny(src, edges, 50, 200);

            // Mat -> Bitmap
            return OpenCvSharp.Extensions.BitmapConverter.ToBitmap(edges);
        }
        


        private void label1_Click1(object sender, EventArgs e)
        {

        }

        private void CurrentImage_Click(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void labelY_Click(object sender, EventArgs e)
        {

        }

        private void label1_Click_1(object sender, EventArgs e)
        {

        }

        private void CycleLabel_Click(object sender, EventArgs e)
        {

        }

        private void prediction_button_Click(object sender, EventArgs e)
        {

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

        private void textBox1_TextChanged(object sender, EventArgs e)
        { }

        private void label2_Click(object sender, EventArgs e)
        { }
        private Bitmap ToGray(Bitmap src)
        {
            using var m = BitmapConverter.ToMat(src);
            using var gray = new OpenCvSharp.Mat();
            Cv2.CvtColor(m, gray, ColorConversionCodes.BGR2GRAY);
            return BitmapConverter.ToBitmap(gray);
        }

        private Bitmap ToWhiteScale(Bitmap src) // “whitescale” = binary (black/white)
        {
            using var m = BitmapConverter.ToMat(src);
            using var gray = new OpenCvSharp.Mat();
            using var bin = new OpenCvSharp.Mat();
            Cv2.CvtColor(m, gray, ColorConversionCodes.BGR2GRAY);
            // Otsu picks threshold automatically; BinaryInv if you prefer white foreground
            Cv2.Threshold(gray, bin, 0, 255, ThresholdTypes.Binary | ThresholdTypes.Otsu);
            return BitmapConverter.ToBitmap(bin);
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
    }
}
