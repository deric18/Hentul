using Common;

namespace FirstOrderMemory.Models
{
    public class Column
    {
        public List<Neuron> Neurons { get; private set; }
        public Position_SOM ColumnID { get; private set; }        
        public int Init { get; set; }


        public Column(int x, int y, int numberOfNeurons, int BBMID) 
        {
            Neurons = new List<Neuron>(numberOfNeurons);
            ColumnID = new Position_SOM(x, y, numberOfNeurons);
            for (int i=0; i<numberOfNeurons; i++)
            {
                Neurons.Add(new Neuron(new Position_SOM(x, y, i), BBMID));
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
        internal List<Neuron>? GetPredictedNeuronsFromColumn()
        {
            List<Neuron> toReturn = new List<Neuron>();
            List<Neuron> predictedNeurons = Neurons.Where(neuron => neuron.CurrentState != NeuronState.RESTING).ToList();

            if (predictedNeurons.Count() > 1 )          //Pick a winner
            {
                //Pick the most strongly predicted neuron and then fire

                return PickWinner();
                
            }
            else if (predictedNeurons.Count == 0)       //Burst
            {
                //burst
                return Neurons;
            }
            else if(predictedNeurons.Count == 1)        //Simple Fire
            {
                return predictedNeurons;
            }
            else
            {
                throw new Exception("This should Never Happen");
            }
        }

        private List<Neuron> PickWinner()
        {
            int maxVoltage = 0, maxIndex = 0, continuousCounter = 0;

            List<Neuron> toReturn = new List<Neuron>();

            for(int i = 0; i < Neurons.Count; i++)
            {
                if (Neurons[i].Voltage > maxVoltage)
                {
                    maxVoltage = Neurons[i].Voltage;
                    maxIndex = i;
                    if (i >= 1 && Neurons[i - 1].Voltage == Neurons[i].Voltage)
                        continuousCounter++;
                }
            }

            if (continuousCounter == Neurons.Count - 2)
                return Neurons;
            else
                toReturn.Add(Neurons[maxIndex]);

            return toReturn;
        }

        internal bool PreCleanupCheck()
        {
            if(Neurons.Any(x => x.CurrentState == NeuronState.FIRING))
            {
                int breakpoint = 1;
                return true;
            }

            return false;
        }

        internal void PostCycleCleanup()
        {
            var firingNeurons = Neurons.Where( n => n.CurrentState == NeuronState.FIRING ).ToList();

            foreach (var neuron in firingNeurons)
            {
                neuron.FlushVoltage();
            }

            foreach (var neuron in Neurons)
            {
                if (neuron.TAContributors.Count > 0)
                {
                    neuron.CleanUpContributersList();
                }
            }
        }        
    }
}
