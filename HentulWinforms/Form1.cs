
namespace HentulWinforms
{
    using Common;
    using FirstOrderMemory.Models;
    using OpenCvSharp;
    using OpenCvSharp.Extensions;

    public partial class Form1 : Form
    {
        Orchestrator orchestrator;

        readonly int numPixels = 100;

        // LT : 784,367   RT: 1414,367  LB : 784, 1034   RB: 1414, 1034
        Orchestrator.POINT LeftTop = new Orchestrator.POINT();
        Orchestrator.POINT RightTop = new Orchestrator.POINT();
        Orchestrator.POINT LeftBottom = new Orchestrator.POINT();
        Orchestrator.POINT RightBottom = new Orchestrator.POINT();



        public Form1()
        {
            InitializeComponent();
        }

        private void StartButton_Click(object sender, EventArgs e)
        {
            ObjectLabel.Text = orchestrator.StartCycle();
        }

        private void button1_Click(object sender, EventArgs e)
        {

            readyLabel.Text = "Start Position";

            readyLabel.Text = "Ready Now";

            var value = orchestrator.GetCurrentPointerPosition();

            labelX.Text = value.X.ToString();
            labelY.Text = value.Y.ToString();


            while (true)
            {                

                orchestrator.MoveCursorToSpecificPosition(LeftTop.X, LeftTop.Y);                                

                SDR_SOM fomSdr;

               


                //Move Cursor               

                if (value.X >= RightTop.X - numPixels && value.Y >= RightBottom.Y - numPixels)
                {
                    label_done.Text = "Finished Processing Image";
                    break;
                }
                else
                {
                    if (value.X <= RightTop.X - numPixels)
                    {
                        orchestrator.MoveCursor(MoveRight(value));
                                                
                    }
                    else
                    {
                        if (value.Y <= RightBottom.Y - numPixels)
                        {
                            orchestrator.MoveCursor(MoveDown(value));                           
                        }
                        else
                        {
                            if (value.X >= RightTop.X - numPixels && value.Y >= RightBottom.Y - numPixels)
                            {
                                label_done.Text = "finsihed Processing Image";
                                break;
                            }
                        }
                    }
                }                

                orchestrator.MoveCursor(value);

                orchestrator.Grab();

                CurrentImage.Image = orchestrator.bmp;

                EdgedImage.Image = ConverToEdgedBitmap(orchestrator.bmp);

                //orchestrator.Process();
            }

        }


        private Orchestrator.POINT MoveLeft(Orchestrator.POINT value)
        {
            value.X = value.X - numPixels;
            return value;
        }

        private Orchestrator.POINT MoveRight(Orchestrator.POINT value)
        {            
            value.X = value.X + numPixels;
            return value;
        }

        private Orchestrator.POINT MoveDown(Orchestrator.POINT value)
        {
            value.X = value.Y + numPixels;
            return value;
        }

        private Orchestrator.POINT MoveUp(Orchestrator.POINT value)
        {
            value.X = value.Y - numPixels;
            return value;
        }

        private void Form1_Load(object sender, EventArgs e)
        {

            orchestrator = new Orchestrator(numPixels);

            var value = orchestrator.GetCurrentPointerPosition();

            labelX.Text = value.X.ToString();
            labelY.Text = value.Y.ToString();

            LeftTop.X = 784; LeftTop.Y = 367; RightTop.X = 1414; RightTop.Y = 367; LeftBottom.X = 784; LeftBottom.Y = 1034; RightBottom.X = 1414; RightBottom.Y = 1034;

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
    }
}
