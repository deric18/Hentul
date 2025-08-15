namespace SecondOrderMemory.Models
{
    using System.Collections.Generic;
    using Common;

    /// <summary>
    /// Need to support 2 API's
    /// "From where i have come , to where i am , to where i will be"
    /// </summary>
    public class Neuron : IEquatable<Neuron>, IComparable<Neuron>
    {
        #region FLAGS

        public static readonly int TOTALNUMBEROFCORRECTPREDICTIONS = 0;
        public static readonly int TOTALNUMBEROFINCORRECTPREDICTIONS = 0;
        public static int TOTALNUMBEROFPARTICIPATEDCYCLES = 0;
        public static readonly int COMMON_NEURONAL_FIRE_VOLTAGE = 100;
        public static readonly int COMMON_NEURONAL_SPIKE_TRAIN_VOLTAGE = 500;
        public static readonly int UNCOMMMON_NEURONAL_SPIKE_TRAIN_VOLTAGE = 700;
        public static readonly int TEMPORAL_NEURON_FIRE_VALUE = 40;
        public static readonly int APICAL_NEURONAL_FIRE_VALUE = 40;
        public static readonly int NMDA_NEURONAL_FIRE_VALUE = 100;
        public static readonly int PROXIMAL_VOLTAGE_SPIKE_VALUE = 100;
        public static readonly int PROXIMAL_AXON_TO_NEURON_FIRE_VALUE = 50;
        public static readonly int DISTAL_VOLTAGE_SPIKE_VALUE = 20;
        public static readonly uint AXONAL_CONNECTION = 1;
        public static readonly uint DISTALNEURONPLASTICITY = 5;
        public static readonly uint INITIAL_SYNAPTIC_CONNECTION_STRENGTH = 1;

        #endregion


        #region VARIABLES && CONSTRUCTORS


        private ulong redundantCounter = 0;

        public uint PruneCount { get; private set; }

        public int BBMId { get; private set; }

        public Position_SOM NeuronID { get; private set; }

        public NeuronType nType { get; private set; }

        public ulong lastSpikeCycleNum { get; private set; }

        public Dictionary<string, char> TAContributors { get; private set; }

        /// <summary>
        /// Key is always Dendronal Neurons ID && Value is Synapse
        /// </summary>
        public Dictionary<string, Synapse> AxonalList { get; private set; }


        /// <summary>
        /// Key is always Axonal Neuronal ID && Value is Synapse
        /// </summary>
        public Dictionary<string, Synapse> ProximoDistalDendriticList { get; private set; }

        //public List<Segment>? Segments { get; private set; } = null;

        public NeuronState CurrentState { get; private set; }

        public int flag { get; set; }

        public int Voltage { get; private set; }

        public Neuron(Position_SOM neuronId, int BBMId, NeuronType nType = NeuronType.NORMAL)
        {
            NeuronID = neuronId;
            this.BBMId = BBMId;
            this.nType = nType;
            TAContributors = new Dictionary<string, char>();
            ProximoDistalDendriticList = new Dictionary<string, Synapse>();
            AxonalList = new Dictionary<string, Synapse>();
            CurrentState = NeuronState.RESTING;
            Voltage = 0;
            flag = 0;
            PruneCount = 0;
            lastSpikeCycleNum = 0;
        }

        public void IncrementPruneCount() => PruneCount++;

        #endregion

        #region METHODSS

        #region FIRE & INIT

        public void ProcessCurrentState(ulong cycleNum, LogMode logmode = LogMode.BurstOnly, string fileName = null)
        {
            if (Voltage == 0)
            {
                CurrentState = NeuronState.RESTING;
            }
            else if (Voltage > 0 && Voltage < COMMON_NEURONAL_FIRE_VOLTAGE)
            {
                CurrentState = NeuronState.PREDICTED;
            }
            else if (Voltage >= COMMON_NEURONAL_FIRE_VOLTAGE && Voltage < COMMON_NEURONAL_SPIKE_TRAIN_VOLTAGE)
            {
                CurrentState = NeuronState.FIRING;
            }
            else if (Voltage >= COMMON_NEURONAL_SPIKE_TRAIN_VOLTAGE)
            {
                if (logmode == LogMode.BurstOnly && fileName != null)
                {
                    WriteLogsToFile("Neuron " + NeuronID.ToString() + " entering Spiking Mode", fileName);
                    if (Voltage >= UNCOMMMON_NEURONAL_SPIKE_TRAIN_VOLTAGE)
                    {
                        WriteLogsToFile("******************* Neuron : " + NeuronID.ToString() + " ENTERING ULTRA SPIKING MODE****************", fileName);
                    }
                }
                else
                {
                    Console.WriteLine("Neuron " + NeuronID.ToString() + " entering Spiking Mode", fileName);
                }
                CurrentState = NeuronState.SPIKING;
                lastSpikeCycleNum = cycleNum;
            }
            else
            {
                throw new InvalidOperationException("Should Never Happen!");
            }
        }

        public void Fire(ulong cycleNum, LogMode logmode = LogMode.BurstOnly, string logFileName = "")
        {
            TOTALNUMBEROFPARTICIPATEDCYCLES++;

            if (AxonalList == null || AxonalList?.Count == 0)
            {
                Console.WriteLine(" ERROR :: Neuron.Fire() :: No Neurons are Connected to this Neuron : " + NeuronID.ToString());

                #if !DEBUG
                Console.ReadKey();
                #endif

                return;
            }

            Voltage += COMMON_NEURONAL_FIRE_VOLTAGE;

            ProcessCurrentState(cycleNum, logmode, logFileName);
        }

        public void ProcessVoltage(int voltage, ulong cycleNum = 0, LogMode logmode = LogMode.BurstOnly)
        {
            if (NeuronID.ToString().Equals("1035-4-1-N"))
            {
                bool breakpoiunt = true;
            }

            Voltage += voltage;

            if (voltage >= COMMON_NEURONAL_FIRE_VOLTAGE)
            {
                if ((logmode == LogMode.Info || logmode == LogMode.All))
                    Console.WriteLine(" INFO :: Neurons.cs :::: Neuron " + NeuronID.ToString() + " is entering firing Mode");
            }

            ProcessCurrentState(cycleNum, logmode);

            // strengthen the contributed segment if the spike actually resulted in a Fire.
        }

        public string GetMyTemporalPartner1()
        {
            string pos = null;

            foreach (var Synapses in ProximoDistalDendriticList.Values)
            {
                if (Synapses.cType == ConnectionType.TEMPRORAL)
                {
                    pos = Synapses.AxonalNeuronId;
                    break;
                }
            }

            if (!string.IsNullOrEmpty(pos))
            {
                return pos;
            }

            throw new InvalidOperationException("GetMyTemproalPartner :: Temporal Neuron Does Not Exist for this Neuron !!! Needs Investigation unless this is the temporal Neuron.");
        }

        public Position_SOM GetMyTemporalPartner2()
        {
            string pos = null;

            foreach (var Synapses in ProximoDistalDendriticList.Values)
            {
                if (Synapses.cType == ConnectionType.TEMPRORAL)
                {
                    pos = Synapses.AxonalNeuronId;
                    break;
                }
            }

            if (!string.IsNullOrEmpty(pos))
            {
                return Position_SOM.ConvertStringToPosition(pos);
            }

            throw new InvalidOperationException("GetMyTemproalPartner :: Temporal Neuron Does Not Exist for this Neuron !!! Needs Investigation unless this is the temporal Neuron.");
        }

        public Position_SOM GetMyApicalPartner()
        {
            string pos = null;

            foreach (var Synapses in ProximoDistalDendriticList.Values)
            {
                if (Synapses.cType == ConnectionType.APICAL)
                {
                    pos = Synapses.AxonalNeuronId;
                    break;
                }
            }

            if (!string.IsNullOrEmpty(pos))
            {
                return Position_SOM.ConvertStringToPosition(pos);
            }

            throw new InvalidOperationException();
        }

        public string GetMyApicalPartner1()
        {
            string pos = null;

            foreach (var Synapses in ProximoDistalDendriticList.Values)
            {
                if (Synapses.cType == ConnectionType.APICAL)
                {
                    pos = Synapses.AxonalNeuronId;
                    break;
                }
            }

            if (!string.IsNullOrEmpty(pos))
            {
                return pos;
            }

            throw new InvalidOperationException();
        }

        public bool InitProximalConnectionForDendriticConnection(int i, int j, int k)
        {
            string key = Position_SOM.ConvertIKJtoString(i, j, k);
            return AddNewProximalOrDendriticConnection(key);
        }

        public void InitAxonalConnectionForConnector(int i, int j, int k)
        {
            string key = Position_SOM.ConvertIKJtoString(i, j, k);
            AddNewAxonalConnection(key, ConnectionType.AXONTONEURON_SCHEMA);
        }

        internal void CleanUpContributersList()
        {
            TAContributors.Clear();
        }

        private void WriteLogsToFile(string logline, string logfilename)
        {
            File.AppendAllText(logfilename, logline + "\n");
        }

        #endregion


        #region CONNECTOR LOGIC

        #region SCHEMA BASED CONNECTIONS

        internal bool AddNewAxonalConnection(string key, ConnectionType cType)
        {
            //if (key == "0-0-1")
            //{
            //    int bp2 = 1;
            //}

            this.flag++;

            if (key.Equals(NeuronID))
            {
                throw new InvalidOperationException("Canot connect neuron to itself");
            }

            if (AxonalList.TryGetValue(key, out var synapse))
            {
                Console.WriteLine("ERROR :: SOM :: AddNewAxonalConnection : Connection Already Added Counter : " + redundantCounter.ToString(), ++redundantCounter);

                return false;
            }
            else
            {

                AxonalList.Add(key, new Synapse(NeuronID.ToString(), key, 0, INITIAL_SYNAPTIC_CONNECTION_STRENGTH, cType, true));

                return true;
            }
        }

        private bool AddNewProximalOrDendriticConnection(string key)
        {
            
            if (key.Equals(NeuronID))
            {
                throw new InvalidOperationException("Cannot connect neuron to itself");
            }

            if (ProximoDistalDendriticList.TryGetValue(key, out var synapse))
            {
                if (BlockBehaviourManagerSOM.Mode <= LogMode.Trace)
                {
                    Console.WriteLine("INFO :: SOM :: AddNewProximalDendriticConnection : Connection Already Added Counter : " + ++redundantCounter);
                }
                
                return false;
            }
            else
            {

                ProximoDistalDendriticList.Add(key, new Synapse(key, NeuronID.ToString(), 0, INITIAL_SYNAPTIC_CONNECTION_STRENGTH, ConnectionType.PROXIMALDENDRITICNEURON, true));
                return true;
            }

        }

        #endregion

        #region Wiring Connector Logic

        internal bool PerformHigherSequencing(string objectLabel, List<string> nextNeuronIds)
        {
            if (objectLabel == null || nextNeuronIds == null || nextNeuronIds.Count == 0)
            {
                throw new InvalidOperationException(" Error: Neuron.cs : PErformHigherSequencing() : Input Prameters cannnot be Null!");
            }

            // Check if the associated object label under Prediction exist in any of the Synapse List if not add new
            // if does then add this prediction to the AxonalList and increment the hitcount on the synapse for this particular objectLabel under synapse

            foreach (var nextNeuronId in nextNeuronIds)
            {
                if (!string.IsNullOrEmpty(nextNeuronId) && !string.IsNullOrWhiteSpace(nextNeuronId))
                {
                    if (AxonalList.TryGetValue(nextNeuronId, out var synapse))
                    {
                        if(!synapse.PopulatePrediction(objectLabel, nextNeuronIds))
                        {
                            throw new InvalidOperationException("Error : Neurons.cs : PerformHigherSequencing() : Could not add Prediction to the associated Object Label under the Synapse!");
                        }
                    }
                    else
                    {
                        //throw new InvalidOperationException(" Error : Neurons.cs : PerformHigherSequencing() : Post Wire , Every neuron in this firing cycle should have added atleast one connection to Neurons Firing Last cycle! that has not happened! ");
                        if(BlockBehaviourManagerSOM.Mode >= LogMode.BurstOnly)
                        {
                            WriteLogsToFile(" Error : Neurons.cs : PerformHigherSequencing() : Post Wire , Every neuron in this firing cycle should have added atleast one connection to Neurons Firing Last cycle! that has not happened!", BlockBehaviourManagerSOM.LogFileName);
                        }
                    }
                }
            }

            return true;
        }

        //Gets Called for Dendritic End of the Neuron , Increments Hit Count if the connection already exists, Prediction adds new label if it does not exist
        public bool AddToDistalList(string axonalNeuronId, string objectLabel, NeuronType nTypeSource, ulong CycleNum, SchemaType schemaType, string filename, ConnectionType cType, bool IsActive = false)
        {

            if (axonalNeuronId.Equals(NeuronID) && this.nType.Equals(nTypeSource))
            {
                throw new InvalidOperationException("Cannot connect neuron to itself");
            }

            //Only for Temporal and Apical Connections
            if (cType != null)
            {
                if (cType.Equals(ConnectionType.TEMPRORAL) || cType.Equals(ConnectionType.APICAL))
                {

                    if (ProximoDistalDendriticList.TryGetValue(axonalNeuronId, out var synapse))
                    {
                        if(BlockBehaviourManagerSOM.Mode == LogMode.All || BlockBehaviourManagerSOM.Mode == LogMode.Info)
                        {
                            Console.WriteLine("INFO :: SOM :: AddToDistalList : Connection Already Added Counter : " + ++redundantCounter);
                        }                        

                        synapse.IncrementHitCount(CycleNum);                        

                        return true;
                    }
                    else
                    {
                        if (cType.Equals(ConnectionType.TEMPRORAL))
                        {
                            ProximoDistalDendriticList.Add(axonalNeuronId, new Synapse(axonalNeuronId, NeuronID.ToString(), CycleNum, INITIAL_SYNAPTIC_CONNECTION_STRENGTH, ConnectionType.TEMPRORAL, true));
                        }
                        else if (cType.Equals(ConnectionType.APICAL))
                        {
                            ProximoDistalDendriticList.Add(axonalNeuronId, new Synapse(axonalNeuronId, NeuronID.ToString(), CycleNum, INITIAL_SYNAPTIC_CONNECTION_STRENGTH, ConnectionType.APICAL, true));
                        }

                        return true;
                    }
                }
            }

            //Only for Distal Connections.
            if (ProximoDistalDendriticList.TryGetValue(axonalNeuronId, out var synapse1))
            {
                synapse1.IncrementHitCount(CycleNum);

                if (!synapse1.CheckForSupportedLabel(objectLabel))  //Higher Order Sequencing Logic!
                {
                    if (!synapse1.AddNewObjectLabel(objectLabel))
                        throw new InvalidOperationException("A Predicted Dendritic Connection should always have a Prediction!");
                }
                else
                {
                    throw new InvalidOperationException("Every Normal Neuronal Distal Denritic Synapse should have an associated Label with it!");
                }

                return true;
            }
            else
            {
                if (ProximoDistalDendriticList.Count >= 1000)
                {
                    Console.WriteLine(" WARNING :: Overconnecting Neuron NeuronID : " + NeuronID.ToString() + "Total DistalDendritic Count :" + ProximoDistalDendriticList.Count);
                    WriteLogsToFile(" WARNING :: Overconnecting Neuron NeuronID : " + NeuronID.ToString() + "Total DistalDendritic Count :" + ProximoDistalDendriticList.Count, filename);
                }

                ProximoDistalDendriticList.Add(axonalNeuronId, new Synapse(axonalNeuronId, NeuronID.ToString(), objectLabel, CycleNum, INITIAL_SYNAPTIC_CONNECTION_STRENGTH, ConnectionType.DISTALDENDRITICNEURON, IsActive));

                return true;
            }

        }

        //Gets called for the axonal end of the neuron
        public ConnectionRemovalReturnType AddtoAxonalList(string key, string objectLabel, NeuronType ntype, ulong CycleNum, ConnectionType connectionType, SchemaType schemaType, bool IsActive = false)
        {

            if (key.Equals(NeuronID) && this.nType.Equals(ntype))
            {
                throw new InvalidOperationException("Cannot connect neuron to itself");
            }

            if (AxonalList.TryGetValue(key, out var synapse))
            {
                Console.WriteLine(schemaType.ToString() + "INFO :: Axon already connected to Neuron");

                return ConnectionRemovalReturnType.SOFTFALSE;

            }
            else
            {

                if (ntype.Equals(NeuronType.NORMAL) && (AxonalList.Count >= 1000))
                {
                    Console.WriteLine(" WARNING :: Overconnecting Neuron NeuronID : " + NeuronID.ToString());
                    Console.WriteLine("Total DistalDendritic Count :" + ProximoDistalDendriticList.Count);
                    return ConnectionRemovalReturnType.SOFTFALSE;           // Over Connecting Neuron
                    //Thread.Sleep(1000);
                }

                if (connectionType == ConnectionType.APICAL || connectionType == ConnectionType.TEMPRORAL)
                {
                    AxonalList.Add(key, new Synapse(NeuronID.ToString(), key, CycleNum, AXONAL_CONNECTION, connectionType, true));
                }
                else
                {
                    AxonalList.Add(key, new Synapse(NeuronID.ToString(), key, objectLabel, CycleNum, AXONAL_CONNECTION, connectionType, IsActive));
                }

                return ConnectionRemovalReturnType.TRUE;
            }
        }

        internal ConnectionRemovalReturnType RemoveAxonalConnection(Neuron dendronalNeuron)
        {
            if (AxonalList.TryGetValue(dendronalNeuron.NeuronID.ToString(), out var synapse))
            {

                if (synapse.cType.Equals(ConnectionType.AXONTONEURON_SCHEMA))
                {
                    Console.WriteLine(" WARNING :: RemoveAxonalConnection :: Cannot Remove Schema Based Axonal Connections");
                    //Thread.Sleep(5000);
                    return ConnectionRemovalReturnType.SOFTFALSE;
                }

                Console.WriteLine("INFO :: Removing axonal connection to a neuron" + dendronalNeuron.NeuronID);

                AxonalList.Remove(dendronalNeuron.NeuronID.ToString());

                return ConnectionRemovalReturnType.TRUE;
            }

            return ConnectionRemovalReturnType.HARDFALSE;
        }        

        #endregion

        #endregion

        #region HELPER METHODS

        public int CompareTo(Neuron? other)
        {
            return this.Voltage > other.Voltage ? 10 : this.Voltage == other.Voltage ? 0 : (this.Voltage < other.Voltage) ? -1 : -1;
        }

        public bool Equals(Neuron? other)
        {
            return this.Voltage == other?.Voltage;
        }

        internal void CheckSpikingFlush(ulong cycleNum)
        {
            if (CurrentState != NeuronState.SPIKING)
            {
                return;
            }

            if (cycleNum == 0 || lastSpikeCycleNum == 0 || lastSpikeCycleNum > cycleNum)
            {
                throw new InvalidOperationException("Last Spiking Value should never be zero");
            }

            if (cycleNum - lastSpikeCycleNum > 1)
            {
                FlushVoltage(cycleNum);
            }
        }

        internal void FlushVoltage(ulong cycleNum)
        {
            //Console.WriteLine("Flushing Voltage on Neuron !!! " + NeuronID.ToString);
            if (NeuronID.ToString().Equals("607-3-3-N"))
            {
                bool breakpoiunt = true;
            }

            Voltage = 0;
            ProcessCurrentState(cycleNum);
        }

        internal bool DidItContribute(Neuron temporalContributor)
        {
            return TAContributors.TryGetValue(temporalContributor.NeuronID.ToString(), out char w);
        }

        internal List<Synapse> CheckForPrunableConnections(ulong currentCycle)
        {
            List<Synapse> staleConnections = ProximoDistalDendriticList.Values.Where(x => x.cType == ConnectionType.DISTALDENDRITICNEURON && x.IsActive == false).ToList();

            return staleConnections;
        }

        internal List<string> GetCurrentPotentialMatchesForCurrentCycle()
        {
            throw new NotImplementedException();

            // return all the potential matches so far for the firing pattern

            // for the current synapse in question , check who contribbuted to this neuron in the previous cycle and look up the synapse for that neuron and return the list of all the potential matches
        }


        #endregion

        #endregion
    };

    #region ENUMS

    public enum ConnectionType
    {
        AXONTONEURON_SCHEMA,
        PROXIMALDENDRITICNEURON,
        DISTALDENDRITICNEURON,
        NMDATONEURON,
        TEMPRORAL,
        APICAL,
        INVALID
    }

    public enum NeuronType
    {
        APICAL,
        TEMPORAL,
        NORMAL
    }

    public enum ConnectionRemovalReturnType
    {
        TRUE,
        HARDFALSE,
        SOFTFALSE
    }

    #endregion
}