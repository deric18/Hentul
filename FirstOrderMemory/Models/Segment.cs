namespace FirstOrderMemory.Models
{
    public class Segment
    {
        private const uint PRE_SYNAPTIC_STRENGTH = 1;
        private const uint MAX_POST_SYNAPTIC_STRENGTH = 10;
        public Position_SOM NeuronId { get; private set; }
        public Dictionary<string, uint>? ConnectedNeurons { get; private set; } = null;

        public Segment(Position_SOM NeuronID)
        {
            NeuronId = NeuronID;
        }

        public void AddNewConnection(Neuron neuron)
        {
            if (ConnectedNeurons != null)
                ConnectedNeurons.Add(neuron.NeuronID.ToString(), PRE_SYNAPTIC_STRENGTH);
            else
            {
                ConnectedNeurons = new Dictionary<string, uint>();
                ConnectedNeurons.Add(neuron.NeuronID.ToString(), PRE_SYNAPTIC_STRENGTH);
            }
        }

        public void RemoveConnection(Neuron neuron)
        {
            if (ConnectedNeurons != null)
            {
                uint strength;
                ConnectedNeurons.TryGetValue(neuron.NeuronID.ToString(), out strength);
                if(strength > MAX_POST_SYNAPTIC_STRENGTH)
                {
                    Console.WriteLine("WARNING : REMOVING STRONG SYNAPSE FROM NEURON ID : " + neuron.NeuronID.ToString());
                }
                ConnectedNeurons.Remove(neuron.NeuronID.ToString());    
            }
        }

        public void Grow()
        {
            throw new NotImplementedException();    
        }

    }
}
