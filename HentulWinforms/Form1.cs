
namespace HentulWinforms
{

    using OpenCvSharp;
    using OpenCvSharp.Extensions;

    public partial class Form1 : Form
    {
        Orchestrator orchestrator;
        readonly int numPixels = 25;

        public Form1()
        {
            InitializeComponent();
        }

        private void StartButton_Click(object sender, EventArgs e)
        {
            orchestrator = new Orchestrator(numPixels);

            orchestrator.StartCycle();
        }

        private void button1_Click(object sender, EventArgs e)
        {

            readyLabel.Text = "Start Position";

            Thread.Sleep(2000);

            readyLabel.Text = "Ready Now";

            var value = orchestrator.GetCurrentPointerPosition();

            labelX.Text = value.X.ToString();
            labelY.Text = value.Y.ToString();

            orchestrator.Grab();

            CurrentImage.Image = orchestrator.bmp;

            EdgedImage.Image = ConverToEdgedBitmap(orchestrator.bmp);
        }

        private void CurrentImage_Click(object sender, EventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {            

            orchestrator = new Orchestrator(numPixels);

            var value = orchestrator.GetCurrentPointerPosition();

            labelX.Text = value.X.ToString();
            labelY.Text = value.Y.ToString();

            orchestrator.Grab();

            EdgedImage.Image = ConverToEdgedBitmap(orchestrator.bmp);


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
       
    }
}
