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
        private const int DISTAL_CONNECTION_STRENGTH = 10;
        private const int PROXIMAL_VOLTAGE_SPIKE_VALUE = 100;
        private const int DISTAL_VOLTAGE_SPIKE_VALUE = 20;
        private const int AXONAL_CONNECTION = 1;
        #endregion

        public Position NeuronID { get; private set; }

        public Dictionary<string, Synapse> AxonalList { get; private set; }
        public Dictionary<string, Synapse> dendriticList { get; private set; }

        public List<Neuron> ConnectedNeurons { get; private set; }
        public List<Segment>? Segments { get; private set; } = null;
        public NeuronState CurrentState { get; private set; }

        public int flag { get; set; }

        private List<string>? PreCycleContributingNeurons { get; set; } = null;
        public int Voltage { get; private set; }

        public Neuron(Position neuronId)
        {
            NeuronID = neuronId;
            ConnectedNeurons = new List<Neuron>();
            dendriticList = new Dictionary<string, Synapse>();
            AxonalList = new Dictionary<string, Synapse>();
            CurrentState = NeuronState.RESTING;
            Voltage = 0;
            flag = 0;
        }

        public List<Neuron>? GetConnectedNeurons()
        {
            return ConnectedNeurons;
        }       

        public void Fire()
        {
            if (AxonalList == null || AxonalList?.Count == 0)
            {
                Console.WriteLine("No Neurons are Connected to this Neuron : " + NeuronID.ToString());
                return;
            }

            try
            {
                foreach(Synapse synapse in AxonalList.Values)
                {
                    Position.ConvertStringPosToNeuron(synapse.TargetNeuronId).ProcessSpikeFromNeuron(Position.ConvertStringToPosition(synapse.SourceNeuronId));
                }

                CurrentState = NeuronState.FIRING;

                BlockBehaviourManager.GetBlockBehaviourManager().AddNeuronToCurrentFiringCycle(this);

            }
            catch (Exception e)
            {
                int breakpoint = 1;
            }
            return;
        }

        public void ProcessSpikeFromNeuron(Position callingNeuron)
        {
            if(!dendriticList.TryGetValue(callingNeuron.ToString(), out Synapse synapse)) 
            {
                throw new InvalidOperationException("ProcessSpikeFromNeuron: Only Axon connected to Neuron But Dendrite is not Connected to Axon!");
            }

            CurrentState = NeuronState.PREDICTED;
            BlockBehaviourManager.GetBlockBehaviourManager().AddPredictedNeuron(this, callingNeuron.ToString());
            Voltage += PROXIMAL_VOLTAGE_SPIKE_VALUE;
        }

        public void ProcessSpikeFromSegment(Position callingSegment, DistalSegmentSpikeType spikeType)
        {
            Voltage += DISTAL_VOLTAGE_SPIKE_VALUE;

            // strengthen the contributed segment if the spike actually resulted in a Fire.
        }


        //Gets called when this neuron contributed to the firing neuron making a correct prediction
        public void PramoteCorrectPrediction(Neuron callingNeuron)
        {
            TOTALNUMBEROFCORRECTPREDICTIONS++;
            TOTALNUMBEROFPARTICIPATEDCYCLES++;

            if (ConnectedNeurons.Count == 0)
            {
                throw new Exception("Not SUpposed to HAppen : Trying to Pramote connection on an neuron , not connected yet!");
            }

            if (dendriticList.TryGetValue(callingNeuron.NeuronID.ToString(), out Synapse synapse))
            {
                synapse.IncrementStrength();
            }
        }

        public void InitProximalConnectionForConnector(int i, int j, int k)
        {
            //Needs work
            //Add it dictionar;
            //Add it to ConnectedNeurons

            string key = Position.ConvertIKJtoString(i, j, k);
            AddNewProximalConnection(key);
        }

        public void InitAxonalConnectionForConnector(int i, int j, int k)
        {
            string key = Position.ConvertIKJtoString(i, j, k);
            AddNewAxonalConnection(key);
        }

        private bool AddNewAxonalConnection(string key)
        {
            try
            {

                if (key == "0-0-1")
                {
                    int bp2 = 1;
                }

                var neuronToAdd = Position.ConvertStringPosToNeuron(key);

                this.flag++;

                if (neuronToAdd.NeuronID.Equals(NeuronID))
                {
                    throw new InvalidOperationException("Canot connect neuron to itself");
                }

                if(AxonalList == null || AxonalList.Count == 0)
                {
                    AxonalList.Add(key, new Synapse(NeuronID.ToString(), key, 0, AXONAL_CONNECTION));

                    var item = Position.ConvertStringPosToNeuron(key);

                    return true;
                }
                if (AxonalList.TryGetValue(neuronToAdd.NeuronID.ToString(), out var synapse))
                {
                    Console.WriteLine("Connection Already Added");

                    synapse.IncrementStrength();

                    return false;
                }
                else
                {

                    AxonalList.Add(key, new Synapse(NeuronID.ToString(), key, 0, AXONAL_CONNECTION));

                    var item = Position.ConvertStringPosToNeuron(key);

                    return true;
                }
            }
            catch (Exception ex)
            {

                int bp = 1;
            }

            return true;
        }

        private bool AddNewProximalConnection(string key)
        {
            try
            {

                if (key == "0-0-1")
                {
                    int bp2 = 1;
                }

                var neuronToAdd = Position.ConvertStringPosToNeuron(key);


                if (neuronToAdd.NeuronID.Equals(NeuronID))
                {
                    throw new InvalidOperationException("Canot connect neuron to itself");
                }

                if (dendriticList.TryGetValue(neuronToAdd.NeuronID.ToString(), out var synapse))
                {
                    Console.WriteLine("Connection Already Added");

                    synapse.IncrementStrength();

                    return false;
                }
                else
                {

                    dendriticList.Add(key, new Synapse(NeuronID.ToString(), key, 0, PROXIMAL_CONNECTION_STRENGTH));

                    var item = Position.ConvertStringPosToNeuron(key);

                    ConnectedNeurons.Add(item);

                    return true;
                }
            }
            catch (Exception ex)
            {

                int bp = 1;
            }

            return true;
        }

        public bool AddToDistalList(string key)
        {
            var neuronToAdd = Position.ConvertStringPosToNeuron(key);

            if (neuronToAdd.NeuronID.Equals(NeuronID))
            {
                throw new InvalidOperationException("Cannot connect neuron to itself");
            }

            if (dendriticList.TryGetValue(key, out var synapse))
            {
                Console.WriteLine("Connection Already Added");

                synapse.IncrementStrength();

                return false;

            }
            else
            {

                dendriticList.Add(key, new Synapse(NeuronID.ToString(), key, BlockBehaviourManager.GetBlockBehaviourManager().CycleNum, DISTAL_CONNECTION_STRENGTH));

                var item = Position.ConvertStringPosToNeuron(key);

                ConnectedNeurons.Add(item);

                return true;
            }
        }

        public bool AddtoAxonalList(string key)
        {
            var neuronToAdd = Position.ConvertStringPosToNeuron(key);

            if (neuronToAdd.NeuronID.Equals(NeuronID))
            {
                throw new InvalidOperationException("Cannot connect neuron to itself");
            }

            if (AxonalList.TryGetValue(key, out var synapse))
            {
                Console.WriteLine("Connection Already Added");

                synapse.IncrementStrength();

                return false;
            }
            else
            {

                AxonalList.Add(key, new Synapse(NeuronID.ToString(), key, BlockBehaviourManager.GetBlockBehaviourManager().CycleNum, AXONAL_CONNECTION));

                var item = Position.ConvertStringPosToNeuron(key);

                ConnectedNeurons.Add(item);

                return true;
            }
        }

        public bool Equals(Neuron? other)
        {
            return this.Voltage == other?.Voltage;
        }

        public int CompareTo(Neuron? other)
        {
            return this.Voltage > other.Voltage ? -10 : this.Voltage == other.Voltage ? 0 : (this.Voltage < other.Voltage) ? 10 : 11;
        }

        internal void FlushVoltage()
        {
            Voltage = 0;
            CurrentState = NeuronState.RESTING;
        }

        internal void PruneCycleRefresh()
        {
            if (dendriticList == null || dendriticList.Count == 0)
            { return; }



            foreach (var kvp in dendriticList)
            {
                // var neuron = 
            }
        }
    }
}