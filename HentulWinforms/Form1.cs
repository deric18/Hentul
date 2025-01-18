
namespace HentulWinforms
{
    using Common;
    using Hentul;
    using OpenCvSharp;
    using OpenCvSharp.Extensions;
    using static System.Windows.Forms.VisualStyles.VisualStyleElement.ToolTip;

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


        string backupDirHC = "C:\\Users\\depint\\source\\repos\\Hentul\\Hentul\\BackUp\\HC-EC\\";
        string backupDirFOM = "C:\\Users\\depint\\source\\repos\\Hentul\\Hentul\\BackUp\\FOM\\";
        string backupDirSOM = "C:\\Users\\depint\\source\\repos\\Hentul\\Hentul\\BackUp\\SOM\\";

        public Form1()
        {
            InitializeComponent();
            networkMode = NetworkMode.TRAINING;
            train_another_object.Visible = false;
        }

        private void StartButton_Click(object sender, EventArgs e)
        {

            label_done.Text = "Procesing";

            label_done.Refresh();

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
                    labelX.Text = value.X.ToString(); labelX.Refresh();
                    labelY.Text = value.Y.ToString(); labelY.Refresh();

                    orchestrator.MoveCursor(value);

                    orchestrator.ProcessStep0();        //Grab Image             

                    CurrentImage.Image = orchestrator.bmp;

                    CurrentImage.Refresh();

                    EdgedImage.Image = ConverToEdgedBitmap(orchestrator.bmp);

                    EdgedImage.Refresh();

                    orchestrator.ProcesStep1(ConverToEdgedBitmap(orchestrator.bmp));     // Fire FOMS per image

                    orchestrator.ProcessSDRForL3B();    // Fire SOM per FOMS

                    counter++;                      // Repeat!

                    CycleLabel.Text = counter.ToString();

                    CycleLabel.Refresh();
                }
                else if (networkMode.Equals(NetworkMode.PREDICTION))
                {
                    orchestrator.ProcessStep0();        //Grab Image

                    CurrentImage.Image = orchestrator.bmp;

                    CurrentImage.Refresh();

                    EdgedImage.Image = ConverToEdgedBitmap(orchestrator.bmp);

                    EdgedImage.Refresh();

                    orchestrator.ProcesStep1(ConverToEdgedBitmap(orchestrator.bmp));     // Fire FOMS per image

                    var motorOutput = orchestrator.ProcessSDRForL3B();    // Fire SOM per FOMS

                    if (motorOutput != null)
                    {
                        if (motorOutput.X == int.MaxValue && motorOutput.Y == int.MaxValue)
                        {
                            //Object Recognised!
                            var obj = orchestrator.GetPredictedObject();
                            ObjectLabel.Text = obj.Label;
                            ObjectLabel.Refresh();

                            label_done.Text = "Object Recognised!";
                            label_done.Refresh();
                            train_another_object.Visible = true;
                            wanderingButton.Visible = true;
                            break;
                        }
                        else if (motorOutput.X == int.MinValue && motorOutput.Y == int.MinValue)
                        {
                            label_done.Text = "Object Could Not be Recognised!";
                            label_done.Refresh();
                            break;
                        }

                        Orchestrator.POINT p = new Orchestrator.POINT();
                        p.X = motorOutput.X; p.Y = motorOutput.Y;
                        orchestrator.MoveCursor(p);
                    }
                    else
                    {
                        //Just Move the cursor to the next default position
                        orchestrator.MoveCursor(value);
                        //label_done.Text = "Finished Processing Image";
                        //break;
                    }

                }
            }

            if (label_done.Text == "Finished Processing Image")
            {
                if (networkMode == NetworkMode.TRAINING)
                {
                    imageIndex++;

                    if (imageIndex == totalImagesToProcess)
                    {                        
                        StartButton.Text = "Tet Classification Algo";
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
                else if (networkMode == NetworkMode.PREDICTION)
                {
                    bool failedToPredict = true;
                }
            }
        }

        private void wanderingButton_Click(object sender, EventArgs e)
        {
            StartBurstAvoidance();
        }

        private void StartBurstAvoidance()
        {
            orchestrator.StartBurstAvoidanceWandering();
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

            orchestrator.ProcessStep0();

            CurrentImage.Image = orchestrator.bmp;

            CurrentImage.Refresh();

            EdgedImage.Image = ConverToEdgedBitmap(orchestrator.bmp);

            EdgedImage.Refresh();

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
            CurrentImage.Image = orchestrator.bmp;

            string filename = "C:\\Users\\depint\\source\\repos\\Hentul\\Hentul\\Images\\savedImage.png";

            orchestrator.bmp.Save("C:\\Users\\depint\\source\\repos\\Hentul\\Hentul\\Images\\savedImage.png");

            //var edgeDetection = Cv2.im


            var edgeImage = Cv2.ImRead(filename);
            var imgdetect = new Mat();
            Cv2.Canny(edgeImage, imgdetect, 50, 200);

            return BitmapConverter.ToBitmap(imgdetect);
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
    }
}
