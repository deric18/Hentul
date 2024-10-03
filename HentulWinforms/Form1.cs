
namespace HentulWinforms
{
    using Common;
    using FirstOrderMemory.Models;
    using OpenCvSharp;
    using OpenCvSharp.Extensions;
    using System.Drawing.Imaging;

    public partial class Form1 : Form
    {
        Orchestrator orchestrator;

        readonly int numPixels = 10;
        int counter = 0;
        int numRotations = 100;

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
            
            label_done.Text = "Procesing";

            label_done.Refresh();

            var value = LeftTop;

            value.X = value.X + numPixels;
            value.Y = value.Y + numPixels;

            orchestrator.MoveCursor(value);

            labelX.Text = value.X.ToString();
            labelY.Text = value.Y.ToString();
                 

            while (counter < numRotations)
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
                                break;
                            }
                        }
                    }
                }

                labelX.Text = value.X.ToString(); labelX.Refresh(); 
                labelY.Text = value.Y.ToString(); labelY.Refresh();

                orchestrator.MoveCursor(value);

                orchestrator.Grab();

                CurrentImage.Image = orchestrator.bmp;

                CurrentImage.Refresh();

                EdgedImage.Image = ConverToEdgedBitmap(orchestrator.bmp);

                EdgedImage.Refresh();

                orchestrator.Process();

                counter++;
            }

            label_done.Text = "Done";

            label_done.Refresh();            
        }


        private Orchestrator.POINT MoveRight(Orchestrator.POINT value)
        {
            value.X = value.X + numPixels * 2;
            return value;
        }
        

        private Orchestrator.POINT MoveDown(Orchestrator.POINT value)
        {
            value.Y = value.Y + numPixels * 2;
            return value;
        }

        private Orchestrator.POINT SetLeft(Orchestrator.POINT value)
        {
            value.X = value.X - Math.Abs(LeftTop.X - RightTop.X) + numPixels;
            return value;
        }


        private void button1_Click(object sender, EventArgs e)
        {            
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

            orchestrator.Grab();

            CurrentImage.Image = orchestrator.bmp;

            CurrentImage.Refresh();

            EdgedImage.Image = ConverToEdgedBitmap(orchestrator.bmp);

            EdgedImage.Refresh();

            label_done.Text = "Ready";

            label_done.Refresh();
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

            LeftTop.X = 960; LeftTop.Y = 365; RightTop.X = 1598; RightTop.Y = 365; LeftBottom.X = 960; LeftBottom.Y = 1032; RightBottom.X = 1598; RightBottom.Y = 1032;

            label_done.Text = "Ready";

            label_done.Refresh();

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
    }
}
