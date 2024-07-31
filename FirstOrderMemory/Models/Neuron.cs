using FirstOrderMemory.BehaviourManagers;
using Common;
using System.Data;
using Bond;

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
        private const int INITIAL_SYNAPTIC_CONNECTION_STRENGTH = 1;        
        private const int COMMON_NEURONAL_FIRE_VOLTAGE = 100;
        private const int TEMPORAL_NEURON_FIRE_VALUE = 40;
        private const int APICAL_NEURONAL_FIRE_VALUE = 40;
        private const int NMDA_NEURONAL_FIRE_VALUE = 100;        
        private const int PROXIMAL_VOLTAGE_SPIKE_VALUE = 100;
        private const int PROXIMAL_AXON_TO_NEURON_FIRE_VALUE = 50;
        private const int DISTAL_VOLTAGE_SPIKE_VALUE = 20;
        private const int AXONAL_CONNECTION = 1;        
        private const uint DISTALNEURONPLASTICITY = 5;

        #endregion

        private ulong redundantCounter = 0;

        public uint PruneCount { get; private set; }

        public Position BlockID { get; private set; }

        public Position UnitID { get; private set; }

        public Position BBMId { get; private set; }

        public Position_SOM NeuronID { get; private set; }

        public NeuronType nType { get; private set; }

        public Dictionary<string, char> TAContributors { get; private set; }

        /// <summary>
        /// Key is always Dendronal Neurons ID && Value is Synapse
        /// </summary>
        public Dictionary<string, Synapse> AxonalList { get; private set; }


        /// <summary>
        /// Key is always Axonal Neuronal ID && Value is Synapse
        /// </summary>
        public Dictionary<string, Synapse> ProximoDistalDendriticList { get; private set; }                

        public List<Segment>? Segments { get; private set; } = null;

        public NeuronState CurrentState { get; private set; }

        public int flag { get; set; }
        
        public int Voltage { get; private set; }

        public Neuron(Position_SOM neuronId, Position blockid, Position unitId, int BBMId, NeuronType nType = NeuronType.NORMAL)
        {
            NeuronID = neuronId;
            BlockID = blockid;
            UnitID = unitId;
            BBMId = BBMId;
            this.nType = nType;
            TAContributors = new Dictionary<string, char>();            
            ProximoDistalDendriticList = new Dictionary<string, Synapse>();
            AxonalList = new Dictionary<string, Synapse>();
            CurrentState = NeuronState.RESTING;
            Voltage = 0;
            flag = 0;
            PruneCount = 0;
        }

        public void IncrementPruneCount() => PruneCount++;

        public void ChangeCurrentStateTo(NeuronState state)
        {
            CurrentState = state;
        }

        public void Fire()
        {
            TOTALNUMBEROFPARTICIPATEDCYCLES++;

            if (AxonalList == null || AxonalList?.Count == 0)
            {
                Console.WriteLine(" ERROR :: Neuron.Fire() :: No Neurons are Connected to this Neuron : " + NeuronID.ToString());
                Console.ReadKey();
                return;
            }

            Voltage += COMMON_NEURONAL_FIRE_VOLTAGE;

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
            string pos = ProximoDistalDendriticList.Values.FirstOrDefault(synapse => synapse.cType == ConnectionType.TEMPRORAL)?.AxonalNeuronId;

            if (!string.IsNullOrEmpty(pos))
            {
                return pos;
            }

            throw new InvalidOperationException("GetMyTemproalPartner :: Temporal Neuron Does Not Exist for this Neuron !!! Needs Investigation unless this is the temporal Neuron.");
        }       

        public string GetMyApicalPartner()
        {
            string pos = ProximoDistalDendriticList.Values?.FirstOrDefault(synapse => synapse.cType == ConnectionType.APICAL)?.AxonalNeuronId;

            if (!string.IsNullOrEmpty(pos))
            {
                return pos;
            }

            throw new InvalidOperationException();
        }
        
        public bool InitProximalConnectionForDendriticConnection(int i, int j, int k)
        {                        
            string key = Position_SOM.ConvertIKJtoString(i, j, k);
            return AddNewProximalDendriticConnection(key);
        }

        public void InitAxonalConnectionForConnector(int i, int j, int k)
        {
            string key = Position_SOM.ConvertIKJtoString(i, j, k);
            AddNewAxonalConnection(key);
        }

        public void PostCycleCleanup() => FlushVoltage();        

        internal void CleanUpContributersList()
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
                    AxonalList.Add(key, new Synapse(NeuronID.ToString(), key, 0, AXONAL_CONNECTION, ConnectionType.AXONTONEURON, true));                    

                    return true;
                }

                if (AxonalList.TryGetValue(key, out var synapse))
                {
                    Console.WriteLine("ERROR :: SOM :: AddNewAxonalConnection : Connection Already Added Counter : " + redundantCounter.ToString() , ++redundantCounter);
                    
                    return false;
                }
                else
                {

                    AxonalList.Add(key, new Synapse(NeuronID.ToString(), key, 0, AXONAL_CONNECTION, ConnectionType.AXONTONEURON, true));                    

                    return true;
                }
            }
            catch (Exception ex)
            {
                throw;
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

                if (ProximoDistalDendriticList.TryGetValue(key, out var synapse))
                {
                    Console.WriteLine("ERROR :: SOM :: AddNewProximalDendriticConnection : Connection Already Added Counter : ", ++redundantCounter);                                        
                    return false;
                }
                else
                {

                    ProximoDistalDendriticList.Add(key, new Synapse(key, NeuronID.ToString(), 0, INITIAL_SYNAPTIC_CONNECTION_STRENGTH, ConnectionType.PROXIMALDENDRITICNEURON, false));                    
                    return true;
                }
            }
            catch (Exception ex)
            {
                throw;
                int bp = 1;
            }            
        }

        //Gets Called for Dendritic End of the Neuron
        public bool AddToDistalList(string axonalNeuronId, NeuronType nTypeSource, ulong CycleNum, BlockBehaviourManager.SchemaType schemaType, ConnectionType? cType = null)
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

                    if (ProximoDistalDendriticList.TryGetValue(axonalNeuronId, out var synapse1))
                    {
                        Console.WriteLine("ERROR :: SOM :: AddToDistalList : Connection Already Added Counter : ", ++redundantCounter);

                        synapse1.IncrementHitCount(CycleNum);

                        return true;

                    }
                    else
                    {
                        if (cType.Equals(ConnectionType.TEMPRORAL))
                        {
                            ProximoDistalDendriticList.Add(axonalNeuronId, new Synapse(axonalNeuronId, NeuronID.ToString(), CycleNum, INITIAL_SYNAPTIC_CONNECTION_STRENGTH, ConnectionType.TEMPRORAL));
                        }
                        else if (cType.Equals(ConnectionType.APICAL))
                        {
                            ProximoDistalDendriticList.Add(axonalNeuronId, new Synapse(axonalNeuronId, NeuronID.ToString(), CycleNum, INITIAL_SYNAPTIC_CONNECTION_STRENGTH, ConnectionType.APICAL));
                        }                        

                        return true;
                    }
                }
                
            }
                        

            if (ProximoDistalDendriticList.TryGetValue(axonalNeuronId, out var synapse))
            {
                synapse.IncrementHitCount(CycleNum);

                return false;
            }
            else
            {
                ProximoDistalDendriticList.Add(axonalNeuronId, new Synapse(axonalNeuronId, NeuronID.ToString(), CycleNum, INITIAL_SYNAPTIC_CONNECTION_STRENGTH, ConnectionType.DISTALDENDRITICNEURON));

                //Console.WriteLine("AddToDistalList :: Adding new dendonal Connection to neuron : " + axonalNeuronId);

                if( (ProximoDistalDendriticList.Count >= 400 && schemaType == BlockBehaviourManager.SchemaType.FOMSCHEMA) || (ProximoDistalDendriticList.Count >= 1400 && schemaType == BlockBehaviourManager.SchemaType.SOMSCHEMA))
                {
                    Console.WriteLine(" WARNING :: Neuron : " + NeuronID.ToString() + " has reached more than 400 Distal Dendritic Connections " + BlockID);
                    Console.WriteLine("Total DistalDendritic Count :" + ProximoDistalDendriticList.Count);
                    Thread.Sleep(1000);
                }
                
                if (cType.Equals(ConnectionType.DISTALDENDRITICNEURON))
                {
                    BlockBehaviourManager.totalDendronalConnections++;                    
                }

                return true;
            }            
        }

        //Gets called for the axonal end of the neuron
        public bool AddtoAxonalList(string key, NeuronType ntype, ulong CycleNum, ConnectionType connectionType, BlockBehaviourManager.SchemaType schema)
        {            

            if (key.Equals(NeuronID) && this.nType.Equals(ntype))
            {
                throw new InvalidOperationException("Cannot connect neuron to itself");
            }

            if (AxonalList.TryGetValue(key, out var synapse))
            {
                //Console.WriteLine("SOM :: AddtoAxonalList : Connection Already Added Counter : Will Strethen Synapse", ++redundantCounter);

                //synapse.IncrementHitCount();

                Console.WriteLine(schema.ToString() + "INFO :: Axon already connected to Neuron");

                return true;
            }
            else
            {

                AxonalList.Add(key, new Synapse(NeuronID.ToString(), key, CycleNum, AXONAL_CONNECTION, connectionType, false));                

                return true;
            }
        }

        internal bool RemoveDistalAxonalConnection(Neuron dendronalNeuron)
        {
            if (AxonalList.TryGetValue(dendronalNeuron.NeuronID.ToString(), out var synapse))
            {                
                Console.WriteLine("INFO :: Removing axonal connection to a neuron" + dendronalNeuron.NeuronID);

                AxonalList.Remove(dendronalNeuron.NeuronID.ToString());

                return true;
            }

            return false;
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
            //Console.WriteLine("Flushing Voltage on Neuron !!! " + NeuronID.ToString);
            Voltage = 0;
            CurrentState = NeuronState.RESTING;
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