namespace FirstOrderMemory.Models
{
    public class Column
    {
        public List<Neuron> Neurons { get; private set; }
        public Position ColumnID { get; private set; }        
        public int Init { get; set; }


        public Column(int x, int y, int numberOfNeurons) 
        {
            Neurons = new List<Neuron>(numberOfNeurons);
            ColumnID = new Position(x, y, numberOfNeurons);
            for (int i=0; i<numberOfNeurons; i++)
            {
                Neurons.Add(new Neuron(new Position(x, y, i)));
            }            
            Init = 0;
        }



        public void BurstFire()
        {
            Neurons.ForEach(x => x.Fire());
        }

        /// <summary>
        /// Fires the predicted neurons in the column , if there are no predicted neurons then it Bursts.
        /// </summary>
        /// <returns> null if it burst else NeuronID for the firing Neuron </returns>
        /// <exception cref="Exception"></exception>
        internal Position? Fire()
        {
            List<Neuron> predictedNeurons = Neurons.Where(neuron => neuron.CurrentState == NeuronState.PREDICTED).ToList();

            if (predictedNeurons.Count() > 1 )          //Pick a winner
            {
                //Pick the most strongly predicted neuron and then fire
                predictedNeurons.Sort();
                predictedNeurons[0].Fire();
                return predictedNeurons[0].NeuronID;
            }
            else if (predictedNeurons.Count == 0)       //Burst
            {
                //burst
                Neurons.ForEach(x => x.Fire());
                return null;
            }
            else if(predictedNeurons.Count == 1)        //Simple Fire
            {
                predictedNeurons[0].Fire();
                return predictedNeurons[0].NeuronID;
            }
            else
            {
                throw new Exception("This should Never Happen");
            }
        }

        internal bool PreCleanupCheck()
        {
            return Neurons.Where(x => x.CurrentState == NeuronState.FIRING).Count() > 0;
        }

        internal void PostCycleCleanup()
        {
            var firingNeurons = Neurons.Where( n => n.CurrentState == NeuronState.FIRING ).ToList();
            firingNeurons.ForEach((x) => {
                x.FlushVoltage();

            });
        }

        internal void PruneCycleRefresh()
        {
            this.Neurons.ForEach(neuron => neuron.PruneCycleRefresh());
        }
    }
}
