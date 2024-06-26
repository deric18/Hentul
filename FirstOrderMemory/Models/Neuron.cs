using FirstOrderMemory.BehaviourManagers;
using Common;

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
        private const uint PRUNE_THRESHOLD = 25;
        private const uint DISTALNEURONPLASTICITY = 5;
        private const uint PRUNESTRENGTH = 1;

        #endregion

        private ulong redundantCounter = 0;

        public Position_SOM BlockID { get; private set; }

        public Position_SOM NeuronID { get; private set; }

        public NeuronType nType { get; private set; }

        public Dictionary<string, char> TAContributors { get; private set; }

        public Dictionary<string, Synapse> AxonalList { get; private set; }

        public Dictionary<string, Synapse> ProximoDistalDendriticList { get; private set; }                

        public List<Segment>? Segments { get; private set; } = null;

        public NeuronState CurrentState { get; private set; }

        public int flag { get; set; }
        
        public int Voltage { get; private set; }

        public Neuron(Position_SOM neuronId, Position_SOM blockid, NeuronType nType = NeuronType.NORMAL)
        {
            NeuronID = neuronId;
            BlockID = new Position_SOM(blockid.X, blockid.Y, blockid.Z);
            this.nType = nType;
            TAContributors = new Dictionary<string, char>();            
            ProximoDistalDendriticList = new Dictionary<string, Synapse>();
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
                Console.WriteLine(" ERROR :: Neuron.Fire() :: No Neurons are Connected to this Neuron : " + NeuronID.ToString());
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
        

        public void InitProximalConnectionForDendriticConnection(int i, int j, int k)
        {                        
            string key = Position_SOM.ConvertIKJtoString(i, j, k);
            AddNewProximalDendriticConnection(key);
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
                    
                    //Do Nothing;

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

                    if (ProximoDistalDendriticList.TryGetValue(axonalNeuronId, out var synapse1))
                    {
                        Console.WriteLine("ERROR :: SOM :: AddToDistalList : Connection Already Added Counter : ", ++redundantCounter);

                        synapse1.IncrementHitCount();

                        return true;

                    }
                    else
                    {
                        if (cType.Equals(ConnectionType.TEMPRORAL))
                        {
                            ProximoDistalDendriticList.Add(axonalNeuronId, new Synapse(axonalNeuronId, NeuronID.ToString(), BlockBehaviourManager.CycleNum, INITIAL_SYNAPTIC_CONNECTION_STRENGTH, ConnectionType.TEMPRORAL));
                        }
                        else if (cType.Equals(ConnectionType.APICAL))
                        {
                            ProximoDistalDendriticList.Add(axonalNeuronId, new Synapse(axonalNeuronId, NeuronID.ToString(), BlockBehaviourManager.CycleNum, INITIAL_SYNAPTIC_CONNECTION_STRENGTH, ConnectionType.APICAL));
                        }                        

                        return true;
                    }
                }
                
            }
                        

            if (ProximoDistalDendriticList.TryGetValue(axonalNeuronId, out var synapse))
            {
                synapse.IncrementHitCount();

                return false;
            }
            else
            {
                ProximoDistalDendriticList.Add(axonalNeuronId, new Synapse(axonalNeuronId, NeuronID.ToString(), BlockBehaviourManager.CycleNum, INITIAL_SYNAPTIC_CONNECTION_STRENGTH, ConnectionType.DISTALDENDRITICNEURON));

                //Console.WriteLine("AddToDistalList :: Adding new dendonal Connection to neuron : " + axonalNeuronId);

                if(ProximoDistalDendriticList.Count > 100)
                {
                    Console.WriteLine(" WARNING :: Neuron : " + NeuronID.ToString() + " has reached more than 100 Distal Dendritic Connections " + BlockID);

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
        public bool AddtoAxonalList(string key, NeuronType ntype, ConnectionType connectionType)
        {            

            if (key.Equals(NeuronID) && this.nType.Equals(ntype))
            {
                throw new InvalidOperationException("Cannot connect neuron to itself");
            }

            if (AxonalList.TryGetValue(key, out var synapse))
            {
                //Console.WriteLine("SOM :: AddtoAxonalList : Connection Already Added Counter : Will Strethen Synapse", ++redundantCounter);

                //synapse.IncrementHitCount();

                return true;
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
            //Console.WriteLine("Flushing Voltage on Neuron !!! " + NeuronID.ToString);
            Voltage = 0;
            CurrentState = NeuronState.RESTING;
        }

        internal void Prune()
        {

            if(NeuronID.ToString().Equals("3-1-1-N"))
            {
                int bp = 1;
            }

            if (ProximoDistalDendriticList == null || ProximoDistalDendriticList.Count == 0)
            { return; }

            List<string> removeList = null;

            var distalDendriticList = ProximoDistalDendriticList.Values.Where(x => x.cType.Equals(ConnectionType.DISTALDENDRITICNEURON) && x.GetStrength() <= PRUNESTRENGTH && x.PredictiveHitCount != 5);

            if (distalDendriticList.Count() != 0)
            {
                foreach (var item in ProximoDistalDendriticList)
                {

                    if (item.Value.cType == ConnectionType.DISTALDENDRITICNEURON && ( (BlockBehaviourManager.CycleNum - Math.Max(item.Value.lastFiredCycle, item.Value.lastPredictedCycle)) > PRUNE_THRESHOLD))
                    {
                        if (removeList == null)
                        {
                            removeList = new List<string>();
                        }

                        removeList.Add(item.Key);
                    }
                }

                if (removeList?.Count > 0)
                {
                    for (int i = 0; i < removeList.Count; i++)
                    {
                        ProximoDistalDendriticList.Remove(removeList[i]);

                        BlockBehaviourManager.totalDendronalConnections--;
                    }
                }
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