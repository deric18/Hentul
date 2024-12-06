
namespace HentulWinforms
{
    using Common;
    using Hentul;
    using OpenCvSharp;
    using OpenCvSharp.Extensions;
    using System.Drawing.Imaging;

    public partial class Form1 : Form
    {
        Orchestrator orchestrator;
        NetworkMode networkMode;
        readonly int numPixels = 10;
        int counter = 0;
        int imageIndex = 0;

        // LT : 784,367   RT: 1414,367  LB : 784, 1034   RB: 1414, 1034
        Orchestrator.POINT LeftTop = new Orchestrator.POINT();
        Orchestrator.POINT RightTop = new Orchestrator.POINT();
        Orchestrator.POINT LeftBottom = new Orchestrator.POINT();
        Orchestrator.POINT RightBottom = new Orchestrator.POINT();

        List<String> objectlabellist;

        public Form1()
        {
            InitializeComponent();            
            networkMode = NetworkMode.TRAINING;
            objectlabellist = new List<string>
            {
                "Apple",
                "Ananas",
                "Watermelon",
                "JackFruit"
            };
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
                if (networkMode.Equals(NetworkMode.TRAINING))
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
                                    break;
                                }
                            }
                        }
                    }

                    labelX.Text = value.X.ToString(); labelX.Refresh();
                    labelY.Text = value.Y.ToString(); labelY.Refresh();

                    orchestrator.MoveCursor(value);

                    orchestrator.ProcessStep0();        //Grab Image             

                    CurrentImage.Image = orchestrator.bmp;

                    CurrentImage.Refresh();

                    EdgedImage.Image = ConverToEdgedBitmap(orchestrator.bmp);

                    EdgedImage.Refresh();

                    orchestrator.ProcesStep1(ConverToEdgedBitmap(orchestrator.bmp));     // Fire FOMS per image

                    orchestrator.ProcessStep2();    // Fire SOM per FOMS

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

                    var motorOutput = orchestrator.ProcessStep2();    // Fire SOM per FOMS

                    if (motorOutput != null)
                    {
                        Hentul.Orchestrator.POINT p = new Orchestrator.POINT();

                        p.X = motorOutput.X; p.Y = motorOutput.Y;

                        orchestrator.MoveCursor(p);
                    }

                    
                }
            }

            if (label_done.Text == "Finished Processing Image")
            {
                if (imageIndex >= 3)
                {
                    //orchestrator.BackUp();
                    StartButton.Text = "Start Prediction";
                    StartButton.Refresh();
                    orchestrator.DoneWithTraining();                    
                    orchestrator.ChangeNetworkModeToPrediction();
                }
                else
                {
                    orchestrator.DoneWithTraining();
                    imageIndex++;
                    StartButton.Text = "Start Another Image";
                    StartButton.Refresh();                    
                }

            }
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
            orchestrator = new Orchestrator(numPixels);

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


        private Orchestrator.POINT MoveUp(Orchestrator.POINT value)
        {
            value.X = value.Y - numPixels;
            return value;
        }

        private void Form1_Load(object sender, EventArgs e)
        {   //RT : 1575, LB : 1032
            LeftTop.X = 960; LeftTop.Y = 365; RightTop.X = 1575; RightTop.Y = LeftTop.Y; LeftBottom.X = LeftTop.X; LeftBottom.Y = 1032; RightBottom.X = RightTop.X; RightBottom.Y = LeftBottom.Y;
            label_done.Text = "Ready";
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
    }
}
