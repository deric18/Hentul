using FirstOrderMemory.BehaviourManagers;

namespace FirstOrderMemory.Models
{   

    /// <summary>
    /// Need to support 2 API's
    /// 1.Fire()
    /// 2.Wire()
    /// </summary>
    public class Neuron : IEquatable<Neuron>, IComparable<Neuron>
    {
        private const int PROXIMAL_VOLTAGE_SPIKE_VALUE = 100;
        private const int DISTAL_VOLTAGE_SPIKE_VALUE = 20;
        public Position NeuronID { get; private set; }
        public List<Neuron>? ConnectedNeurons { get; private set; } = null;
        public List<Segment>? Segments { get; private set; } = null;
        public NeuronState CurrentState { get; private set; }

        private List<string>? PreCycleContributingNeurons { get; set; } = null;
        public int Voltage { get; private set; }

        public Neuron(Position neuronId)
        {
            NeuronID = neuronId;
        }

        public List<Neuron>? GetConnectedNeurons()
        {
            return ConnectedNeurons;
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
            CurrentState = NeuronState.PREDICTED;
            BlockBehaviourManager.GetBlockBehaviourManager().AddPredictedNeuron(this, callingNeuron.ToString());
            Voltage += PROXIMAL_VOLTAGE_SPIKE_VALUE;
        }

        public void ProcessSpikeFromSegment(Position callingSegment , DistalSegmentSpikeType spikeType)
        {
            Voltage += DISTAL_VOLTAGE_SPIKE_VALUE;

            // strengthen the contributed segment if the spike actually resulted in a Fire.
        }

        public bool Equals(Neuron? other)
        {
            return this.Voltage == other?.Voltage;
        }

        public int CompareTo(Neuron? other)
        {
            return  this.Voltage > other.Voltage ? -10 : this.Voltage == other.Voltage ? 0 : (this.Voltage < other.Voltage) ? 10 : 11;
        }

        internal void FlushVoltage()
        {
            Voltage = 0;
            CurrentState = NeuronState.RESTING;
        }
    }
}