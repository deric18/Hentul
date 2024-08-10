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


            
        }
    }
}
