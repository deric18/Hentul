using SecondOrderMemory.BehaviourManagers;
using Common;

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

        private ulong redundantCounter = 0;
        public Position_SOM NeuronID { get; private set; }
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

        public Neuron(Position_SOM neuronId, NeuronType nType = NeuronType.NORMAL)
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

        public void ChangeCurrentStateTo(NeuronState state)
        {
            CurrentState = state;
        }

        public void Fire()
        {
            TOTALNUMBEROFPARTICIPATEDCYCLES++;

            if (AxonalList == null || AxonalList?.Count == 0)
            {
                Console.WriteLine("No Neurons are Connected to this Neuron : " + NeuronID.ToString());
                return;
            }

            //Console.WriteLine("Neuron Fired!" + NeuronID.ToString());

            ChangeCurrentStateTo(NeuronState.FIRING);
        }

        public void ProcessVoltage(int voltage)
        {
            Voltage += voltage;

            CurrentState = NeuronState.PREDICTED;

            // strengthen the contributed segment if the spike actually resulted in a Fire.
        }


        public string GetMyTemporalPartner()
        {
            string pos = dendriticList.Values.FirstOrDefault(synapse => synapse.cType == ConnectionType.TEMPRORAL)?.AxonalNeuronId;

            if (!string.IsNullOrEmpty(pos))
            {
                return pos;
            }

            throw new InvalidOperationException("Temporal Neuron Does Not Exist for this Neuron !!! Needs Investigation unless this is the temporal Neuron.");
        }       

        public string GetMyApicalPartner()
        {
            string pos = dendriticList.Values?.FirstOrDefault(synapse => synapse.cType == ConnectionType.APICAL)?.AxonalNeuronId;

            if (!string.IsNullOrEmpty(pos))
            {
                return pos;
            }

            throw new InvalidOperationException();
        }
        

        public void InitProximalConnectionForDendriticConnection(int i, int j, int k)
        {
            //Needs work
            //Add it dictionar;
            //Add it to ConnectedNeurons

            string key = Position_SOM.ConvertIKJtoString(i, j, k);
            AddNewProximalDendriticConnection(key);
        }

        public void InitAxonalConnectionForConnector(int i, int j, int k)
        {
            string key = Position_SOM.ConvertIKJtoString(i, j, k);
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

                this.flag++;

                if (key.Equals(NeuronID))
                {
                    throw new InvalidOperationException("Canot connect neuron to itself");
                }

                if(AxonalList == null || AxonalList.Count == 0)
                {
                    AxonalList.Add(key, new Synapse(NeuronID.ToString(), key, 0, AXONAL_CONNECTION, ConnectionType.AXONTONEURON));                    

                    return true;
                }

                if (AxonalList.TryGetValue(key, out var synapse))
                {
                    Console.WriteLine("ERROR :: SOM :: AddNewAxonalConnection : Connection Already Added Counter : " + redundantCounter.ToString() , ++redundantCounter);
                    
                    return false;
                }
                else
                {

                    AxonalList.Add(key, new Synapse(NeuronID.ToString(), key, 0, AXONAL_CONNECTION, ConnectionType.AXONTONEURON));                    

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

                if (key.Equals(NeuronID))
                {
                    throw new InvalidOperationException("Canot connect neuron to itself");
                }

                if (dendriticList.TryGetValue(key, out var synapse))
                {
                    Console.WriteLine("ERROR :: SOM :: AddNewProximalDendriticConnection : Connection Already Added Counter : ", ++redundantCounter);
                    
                    return false;
                }
                else
                {

                    dendriticList.Add(key, new Synapse(key, NeuronID.ToString(), 0, PROXIMAL_CONNECTION_STRENGTH, ConnectionType.PROXIMALDENDRITICNEURON));

                    //var item = BlockBehaviourManager.GetBlockBehaviourManager().ConvertStringPosToNeuron(key);

                    //ConnectedNeurons.Add(item);

                    return true;
                }
            }
            catch (Exception ex)
            {

                int bp = 1;
            }

            return true;
        }

        //Gets Called for Dendritic End of the Neuron
        public bool AddToDistalList(string axonalNeuronId, NeuronType nTypeSource, ConnectionType? cType = null)
        {

            if(cType == ConnectionType.APICAL)
            {
                bool breakpoint = false;
                breakpoint = true;
            }            

            if(cType.Equals(ConnectionType.TEMPRORAL))
            {
                bool breakpoint = false;
                breakpoint = true;
            }

            if (axonalNeuronId.Equals(NeuronID) && this.nType.Equals(nTypeSource))
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
                        Console.WriteLine("ERROR :: SOM :: AddToDistalList : Connection Already Added Counter : ", ++redundantCounter);

                        //synapse1.IncrementStrength();

                        return false;

                    }
                    else
                    {
                        if (cType.Equals(ConnectionType.TEMPRORAL))
                        {
                            dendriticList.Add(axonalNeuronId, new Synapse(axonalNeuronId, NeuronID.ToString(), BlockBehaviourManager.CycleNum, TEMPORAL_CONNECTION_STRENGTH, ConnectionType.TEMPRORAL));
                        }
                        else if (cType.Equals(ConnectionType.APICAL))
                        {
                            dendriticList.Add(axonalNeuronId, new Synapse(axonalNeuronId, NeuronID.ToString(), BlockBehaviourManager.CycleNum, APICAL_CONNECTION_STRENGTH, ConnectionType.APICAL));
                        }                        

                        return true;
                    }
                }
                
            }
                        

            if (dendriticList.TryGetValue(axonalNeuronId, out var synapse))
            {
                Console.WriteLine("ERROR :: SOM :: AddToDistalList : Connection Already Added to Counter : ", ++redundantCounter);

                synapse.IncrementStrength();

                return false;

            }
            else
            {

                dendriticList.Add(axonalNeuronId, new Synapse(NeuronID.ToString(), axonalNeuronId, BlockBehaviourManager.CycleNum, DISTAL_CONNECTION_STRENGTH, ConnectionType.DISTALDENDRITICNEURON));                

                return true;
            }
        }

        //Gets called for the axonal end of the neuron
        public bool AddtoAxonalList(string key, NeuronType ntype, ConnectionType connectionType)
        {            

            if (key.Equals(NeuronID) && this.nType.Equals(ntype))
            {
                throw new InvalidOperationException("Cannot connect neuron to itself");
            }

            if (AxonalList.TryGetValue(key, out var synapse))
            {
                Console.WriteLine("SOM :: AddtoAxonalList : Connection Already Added Counter : ", ++redundantCounter);                

                return false;
            }
            else
            {

                AxonalList.Add(key, new Synapse(NeuronID.ToString(), key, BlockBehaviourManager.CycleNum, AXONAL_CONNECTION, connectionType));                

                return true;
            }
        }
        public int CompareTo(Neuron? other)
        {
            return this.Voltage > other.Voltage ? 10 : this.Voltage == other.Voltage ? 0 : (this.Voltage < other.Voltage) ? -1 : -1;
        }

        public bool Equals(Neuron? other)
        {
            return this.Voltage == other?.Voltage;
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
        PROXIMALDENDRITICNEURON,
        DISTALDENDRITICNEURON,
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