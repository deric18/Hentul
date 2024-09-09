namespace HentulWinforms
{
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
            var value = orchestrator.GetCurrentPointerPosition();

            labelX.Text = value.X.ToString();
            labelY.Text = value.Y.ToString();

            orchestrator.Grab();

            CurrentImage.Image = orchestrator.bmp;
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

            CurrentImage.Image = orchestrator.bmp;

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }
       
    }
}
