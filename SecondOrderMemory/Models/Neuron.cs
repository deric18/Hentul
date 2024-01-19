using SecondOrderMemory.BehaviourManagers;
using System.Linq;

namespace SecondOrderMemory.Models
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
        private const int TEMPORAL_CONNECTION_STRENGTH = 100;
        private const int APICAL_CONNECTION_STRENGTH = 100;
        private const int TEMPORAL_NEURON_FIRE_VALUE = 40;
        private const int APICAL_NEURONAL_FIRE_VALUE = 40;
        private const int NMDA_NEURONAL_FIRE_VALUE = 100;
        private const int DISTAL_CONNECTION_STRENGTH = 10;
        private const int PROXIMAL_VOLTAGE_SPIKE_VALUE = 100;
        private const int PROXIMAL_AXON_TO_NEURON_FIRE_VALUE = 50;
        private const int DISTAL_VOLTAGE_SPIKE_VALUE = 20;
        private const int AXONAL_CONNECTION = 1;
        #endregion

        public Position NeuronID { get; private set; }
        public NeuronType nType { get; private set; }
        public Dictionary<string, char> TAContributors { get; private set; }
        public Dictionary<string, Synapse> AxonalList { get; private set; }
        public Dictionary<string, Synapse> dendriticList { get; private set; }
        public List<Neuron> ConnectedNeurons { get; private set; }
        public List<Segment>? Segments { get; private set; } = null;
        public NeuronState CurrentState { get; private set; }

        public int flag { get; set; }

        private List<string>? PreCycleContributingNeurons { get; set; } = null;
        public int Voltage { get; private set; }

        public Neuron(Position neuronId, NeuronType nType = NeuronType.NORMAL)
        {
            NeuronID = neuronId;
            this.nType = nType;
            TAContributors = new Dictionary<string, char>();
            ConnectedNeurons = new List<Neuron>();
            dendriticList = new Dictionary<string, Synapse>();
            AxonalList = new Dictionary<string, Synapse>();
            CurrentState = NeuronState.RESTING;
            Voltage = 0;
            flag = 0;
        }        

        public void Fire()
        {
            TOTALNUMBEROFPARTICIPATEDCYCLES++;

            if (AxonalList == null || AxonalList?.Count == 0)
            {
                Console.WriteLine("No Neurons are Connected to this Neuron : " + NeuronID.ToString());
                return;
            }

            try
            {
                foreach(Synapse synapse in AxonalList.Values)
                {
                    Position.ConvertStringPosToNeuron(synapse.DendronalNeuronalId).ProcessSpikeFromNeuron(Position.ConvertStringToPosition(synapse.AxonalNeuronId), synapse.cType);
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

        public void ProcessSpikeFromNeuron(Position callingNeuron, ConnectionType cType = ConnectionType.PRXOMALDENDRITETONEURON)
        {
            uint multiplier = 1;

            CurrentState = NeuronState.PREDICTED;

            BlockBehaviourManager.GetBlockBehaviourManager().AddPredictedNeuron(this, callingNeuron.ToString());

            if(cType.Equals(ConnectionType.TEMPRORAL) || cType.Equals(ConnectionType.APICAL))
            {
                if(!TAContributors.TryGetValue(callingNeuron.ToString(), out char w))
                {
                    TAContributors.Add(callingNeuron.ToString(), 'T');
                }                 
                else
                {
                    bool breakpoint = false;
                    breakpoint = true;
                }
            }

            if(dendriticList.TryGetValue(callingNeuron.ToString(), out var synapse))
            {
                multiplier += synapse.GetStrength();

                switch (synapse.cType)
                {
                    case ConnectionType.DISTALDENDRITETONEURON:
                        Voltage += DISTAL_VOLTAGE_SPIKE_VALUE;
                        break;
                    case ConnectionType.PRXOMALDENDRITETONEURON:
                        Voltage += PROXIMAL_VOLTAGE_SPIKE_VALUE;
                        break;
                    case ConnectionType.TEMPRORAL:
                        Voltage += TEMPORAL_NEURON_FIRE_VALUE;
                        break;
                    case ConnectionType.APICAL:
                        Voltage += APICAL_NEURONAL_FIRE_VALUE;
                        break;
                    case ConnectionType.NMDATONEURON:
                        Voltage += NMDA_NEURONAL_FIRE_VALUE;
                        break;
                }
            }
            else if(cType.Equals(ConnectionType.AXONTONEURON))
            {
                Voltage += PROXIMAL_AXON_TO_NEURON_FIRE_VALUE;
            }
            else
            {
                throw new InvalidOperationException("ProcessSpikeFormNeuron : Trying to Process Spike from Neuron which is not connected to this Neuron");
            }
        }        

        public void ProcessSpikeFromSegment(Position callingSegment, FIRETYPE spikeType)
        {
            Voltage += DISTAL_VOLTAGE_SPIKE_VALUE;

            // strengthen the contributed segment if the spike actually resulted in a Fire.
        }


        public Neuron GetMyTemporalPartner()
        {
            string pos = dendriticList.Values.FirstOrDefault(synapse => synapse.cType == ConnectionType.TEMPRORAL)?.AxonalNeuronId;

            if (!string.IsNullOrEmpty(pos))
            {
                return Position.ConvertStringPosToNeuron(pos);
            }

            throw new InvalidOperationException("Temporal Neuron Does Not Exist for this Neuron !!! Needs Investigation unless this is the temporal Neuron.");
        }       

        private Neuron GetMyApicalPartner()
        {
            string pos = dendriticList.Values.FirstOrDefault(synapse => synapse.cType == ConnectionType.APICAL)?.AxonalNeuronId;

            if (!string.IsNullOrEmpty(pos))
            {
                return Position.ConvertStringPosToNeuron(pos);
            }

            throw new InvalidOperationException();
        }

        //Gets called when this neuron fired correctly and needs to boost the strength on the contributing neuron
        public void PramoteCorrectPredictionDendronal(Neuron contributingNeuron)
        {                        
            if (dendriticList.Count == 0)
            {
                throw new Exception("Not Supposed to Happen : Trying to Pramote connection on a neuron , not connected yet!");
            }

            if (dendriticList.TryGetValue(contributingNeuron.NeuronID.ToString(), out Synapse synapse))
            {
                if (synapse == null)
                {
                    Console.WriteLine("PramoteCorrectPredictionDendronal: Trying to increment strength on a synapse object that was null!!!");
                    throw new InvalidOperationException("Not Supposed to happen!");
                }

                synapse.IncrementStrength();
            }
        }

        public void StrengthenTemporalConnection()
        {
            PramoteCorrectPredictionDendronal(GetMyTemporalPartner());
        }

        public void StrengthenApicalConnection()
        {
            PramoteCorrectPredictionDendronal(GetMyApicalPartner());
        }

        public void InitProximalConnectionForDendriticConnection(int i, int j, int k)
        {
            //Needs work
            //Add it dictionar;
            //Add it to ConnectedNeurons

            string key = Position.ConvertIKJtoString(i, j, k);
            AddNewProximalDendriticConnection(key);
        }

        public void InitAxonalConnectionForConnector(int i, int j, int k)
        {
            string key = Position.ConvertIKJtoString(i, j, k);
            AddNewAxonalConnection(key);
        }

        public void CleanUpContributersList()
        {
            TAContributors.Clear();
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
                    AxonalList.Add(key, new Synapse(NeuronID.ToString(), key, 0, AXONAL_CONNECTION, ConnectionType.AXONTONEURON));

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

                    AxonalList.Add(key, new Synapse(NeuronID.ToString(), key, 0, AXONAL_CONNECTION, ConnectionType.AXONTONEURON));

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

        private bool AddNewProximalDendriticConnection(string key)        
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

                    dendriticList.Add(key, new Synapse(NeuronID.ToString(), key, 0, PROXIMAL_CONNECTION_STRENGTH, ConnectionType.PRXOMALDENDRITETONEURON));

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

        //Get Called for Dendritic End of the Neuron
        public bool AddToDistalList(string axonalNeuronId, ConnectionType? cType = null)
        {

            if(axonalNeuronId == "0-4-5-T")
            {
                bool breakpoint = false;
                breakpoint = true;
            }

            var neuronToAdd = Position.ConvertStringPosToNeuron(axonalNeuronId);

            if(cType.Equals(ConnectionType.TEMPRORAL))
            {
                bool breakpoint = false;
                breakpoint = true;
            }

            if (neuronToAdd.NeuronID.Equals(NeuronID) && this.nType.Equals(neuronToAdd.nType))
            {
                throw new InvalidOperationException("Cannot connect neuron to itself");
            }

            //Only for Temporal and Apical Connections
            if (cType != null)
            {
                if(cType.Equals(ConnectionType.TEMPRORAL) || cType.Equals(ConnectionType.APICAL))
                {

                    if (dendriticList.TryGetValue(axonalNeuronId, out var synapse1))
                    {
                        Console.WriteLine("Connection Already Added");

                        synapse1.IncrementStrength();

                        return false;

                    }
                    else
                    {
                        if (cType.Equals(ConnectionType.TEMPRORAL))
                        {
                            dendriticList.Add(axonalNeuronId, new Synapse(axonalNeuronId, NeuronID.ToString(), BlockBehaviourManager.GetBlockBehaviourManager().CycleNum, TEMPORAL_CONNECTION_STRENGTH, ConnectionType.TEMPRORAL));
                        }
                        else if (cType.Equals(ConnectionType.APICAL))
                        {
                            dendriticList.Add(axonalNeuronId, new Synapse(axonalNeuronId, NeuronID.ToString(), BlockBehaviourManager.GetBlockBehaviourManager().CycleNum, APICAL_CONNECTION_STRENGTH, ConnectionType.APICAL));
                        }

                        var item = Position.ConvertStringPosToNeuron(axonalNeuronId);

                        ConnectedNeurons.Add(item);

                        return true;
                    }
                }
                
            }
                        

            if (dendriticList.TryGetValue(axonalNeuronId, out var synapse))
            {
                Console.WriteLine("Connection Already Added");

                synapse.IncrementStrength();

                return false;

            }
            else
            {

                dendriticList.Add(axonalNeuronId, new Synapse(NeuronID.ToString(), axonalNeuronId, BlockBehaviourManager.GetBlockBehaviourManager().CycleNum, DISTAL_CONNECTION_STRENGTH, ConnectionType.DISTALDENDRITETONEURON));

                var item = Position.ConvertStringPosToNeuron(axonalNeuronId);

                ConnectedNeurons.Add(item);

                return true;
            }
        }

        //Gets called for the axonal end of the neuron
        public bool AddtoAxonalList(string key, ConnectionType connectionType)
        {
            var neuronToAdd = Position.ConvertStringPosToNeuron(key);

            if (neuronToAdd.NeuronID.Equals(NeuronID) && this.nType.Equals(neuronToAdd.nType))
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

                AxonalList.Add(key, new Synapse(NeuronID.ToString(), key, BlockBehaviourManager.GetBlockBehaviourManager().CycleNum, AXONAL_CONNECTION, connectionType));

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

        internal bool DidItContribute(Neuron temporalContributor)
        {
            return TAContributors.TryGetValue(temporalContributor.NeuronID.ToString(), out char w);
        }
    }

    public enum ConnectionType
    {
        AXONTONEURON,
        PRXOMALDENDRITETONEURON,
        DISTALDENDRITETONEURON,
        NMDATONEURON,
        TEMPRORAL,
        APICAL
    }

    public enum NeuronType
    {
        APICAL,
        TEMPORAL,
        NORMAL
    }
}