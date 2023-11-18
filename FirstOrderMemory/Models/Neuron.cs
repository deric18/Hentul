using FirstOrderMemory.BehaviourManagers;

namespace FirstOrderMemory.Models
{   

    /// <summary>
    /// Need to support 2 API's
    /// 1.Fire()
    /// 2.Wire()
    /// </summary>
    public class Neuron
    {
        public Position NeuronID { get; private set; }
        public List<Neuron>? ConnectedNeurons { get; private set; } = null;
        public List<Segment>? Segments { get; private set; } = null;
        public NeuronState CurrentState { get; private set; }

        public Neuron(Position neuronId)
        {
            NeuronID = neuronId;
        }

        public void Fire()
        {
            if(ConnectedNeurons == null || ConnectedNeurons?.Count == 0) return;


            ConnectedNeurons.ForEach(
                neuron => neuron.ProcessSpikeFromNeuron(NeuronID)
                );

            return;
        }


        public void ProcessSpikeFromNeuron(Position callingNeuron)
        {
            CurrentState = NeuronState.FIRING;
            BlockBehaviourManager.GetBlockBehaviourManager().AddPredictedNeuron(this);
        }

        public void ProcessSpikeFromSegment(Position callingSegment)
        {

        }

    }
}