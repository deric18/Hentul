namespace FirstOrderMemory.Models
{
    public class Column
    {
        public List<Neuron> Neurons { get; private set; }
        public Position ColumnID { get; private set; }


        public Column(int numberofNeurons, int x, int y) 
        {
            Neurons = new List<Neuron>(numberofNeurons);
            ColumnID = new Position(x, y);
        }

        public void BurstFire()
        {
            Neurons.ForEach(x => x.Fire());
        }

        internal void Fire()
        {
            List<Neuron> predictedNeurons = Neurons.Where(neuron => neuron.CurrentState == NeuronState.PREDICTED).ToList();

            if (predictedNeurons.Count() > 1 )
            {
                //Pick the most strongly predicted neuron and then fire

            }
            else if (predictedNeurons.Count == 0)
            {
                //burst
                Neurons.ForEach(x => x.Fire());
            }
            else if(predictedNeurons.Count == 1)
            {
                predictedNeurons[0].Fire();
            }
            else
            {
                throw new Exception("This shoul Never Happen");
            }
        }
    }
}
