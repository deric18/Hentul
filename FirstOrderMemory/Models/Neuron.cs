using FirstOrderMemory.BehaviourManagers;
using System.Linq;

namespace FirstOrderMemory.Models
{   

    /// <summary>
    /// Need to support 2 API's
    /// 1.Fire()
    /// 2.Wire()
    /// </summary>
    public class Neuron : IEquatable<Neuron>, IComparable<Neuron>
    {
        #region FLAGS
        public int TOTALNUMBEROFCORRECTPREDICTIONS = 0;
        public int TOTALNUMBEROFINCORRECTPREDICTIONS = 0;
        public int TOTALNUMBEROFPARTICIPATEDCYCLES = 0;
        private const int PROXIMAL_CONNECTION_STRENGTH = 1000;
        private const int PROXIMAL_VOLTAGE_SPIKE_VALUE = 100;
        private const int DISTAL_VOLTAGE_SPIKE_VALUE = 20;
        #endregion


        public Position NeuronID { get; private set; }
        public Dictionary<string, Synapse> ConnectedNeuronsStrength { get; private set; }

        public List<Neuron> ConnectedNeurons { get; private set; } 
        public List<Segment>? Segments { get; private set; } = null;
        public NeuronState CurrentState { get; private set; }

        private List<string>? PreCycleContributingNeurons { get; set; } = null;
        public int Voltage { get; private set; }

        public Neuron(Position neuronId)
        {
            NeuronID = neuronId;
            ConnectedNeurons = new List<Neuron>();
            ConnectedNeuronsStrength = new Dictionary<string, Synapse>();
            CurrentState = NeuronState.RESTING;
            Voltage = 0;

        }

        public List<Neuron>? GetConnectedNeurons()
        {
            return ConnectedNeurons;
        }

        public void InitProximalConnectionForConnector(int i, int j, int k)
        {
            //Needs work
            //Add it dictionary
            //Add it to ConnectedNeurons

            string key = Position.ConvertIKJtoString(i, j, k);
            if (ConnectedNeuronsStrength.TryGetValue(key, out var neuron))
            {
                Console.WriteLine("Connection Already Added");
            }
            else
            {
                ConnectedNeuronsStrength.Add(key, new Synapse(NeuronID.ToString(), key, 0, PROXIMAL_CONNECTION_STRENGTH));
                ConnectedNeurons.Add(Position.ConvertStringPosToNeuron(key));
            }
        }

        public void Fire()
        {
            if (ConnectedNeurons == null || ConnectedNeurons?.Count == 0)
            {
                Console.WriteLine("No Neurons are Connected to this Neuron.");
                return;
            }
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


        //Gets called when this neuron contributed to the firing neuron making a correct prediction
        public void PramoteCorrectPrediction(Neuron callingNeuron)
        {
            TOTALNUMBEROFCORRECTPREDICTIONS++;
            TOTALNUMBEROFPARTICIPATEDCYCLES++;

            if(ConnectedNeurons.Count == 0)
            {
                throw new Exception("Not SUpposed to HAppen : Trying to Pramote connection on an neuron , not connected yet!");
            }

            if(ConnectedNeuronsStrength.TryGetValue(callingNeuron.NeuronID.ToString(), out Synapse synapse))
            {
                synapse.IncrementStrength();
            }
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

        internal void PruneCycleRefresh()
        {
            if(ConnectedNeuronsStrength == null || ConnectedNeuronsStrength.Count == 0)
            { return; }

            

            foreach( var kvp in ConnectedNeuronsStrength)
            {
               // var neuron = 
            }
        }
    }
}