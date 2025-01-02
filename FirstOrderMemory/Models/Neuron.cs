namespace FirstOrderMemory.Models
{
    using Common;
    using FirstOrderMemory.BehaviourManagers;    
    using System.Text.Json.Serialization;
   
    public class Neuron : IEquatable<Neuron>, IComparable<Neuron>
    {
        #region FLAGS

        public static readonly int TOTALNUMBEROFCORRECTPREDICTIONS = 0;
        public static readonly int TOTALNUMBEROFINCORRECTPREDICTIONS = 0;
        public static int TOTALNUMBEROFPARTICIPATEDCYCLES = 0;        
        public static readonly int COMMON_NEURONAL_FIRE_VOLTAGE = 100;
		public static readonly int COMMON_NEURONAL_SPIKE_TRAIN_VOLTAGE = 300;
        public static readonly int UNCOMMMON_NEURONAL_SPIKE_TRAIN_VOLTAGE = 600;
        public static readonly int TEMPORAL_NEURON_FIRE_VALUE = 40;
        public static readonly int APICAL_NEURONAL_FIRE_VALUE = 40;
        public static readonly int MIN_DEPOLARIZE_FIRE_VALUE = 39;
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

        public List<Segment>? Segments { get; private set; } = null;

        public NeuronState CurrentState { get; private set; }

        public int flag { get; set; }

        public int Voltage { get; private set; }       


        public Neuron(Position_SOM neuronId, int BBMId, NeuronType nType = NeuronType.NORMAL)
        {
            NeuronID = neuronId;            
            BBMId = BBMId;
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

        [JsonConstructor]
        public Neuron(uint pruneCount , string NeuronId, string nType , Dictionary<string, Synapse> proximoDistalDendriticList, Dictionary<string, Synapse> axonalList, string currentState, int voltage, int flag, uint lastspikecyclenum)
        {
            this.NeuronID = Position_SOM.ConvertStringToPosition(NeuronId);
            this.nType = nType.Equals(NeuronType.NORMAL.ToString()) ? NeuronType.NORMAL : nType.Equals(NeuronType.TEMPORAL.ToString()) ? NeuronType.TEMPORAL : nType.Equals(NeuronType.APICAL.ToString()) ? NeuronType.APICAL : throw new InvalidCastException("Could Not convert Neuron NType to proper Enum!");
            TAContributors = new Dictionary<string, char>();
            ProximoDistalDendriticList = proximoDistalDendriticList;
            AxonalList = axonalList;
            CurrentState = currentState.Equals(NeuronState.RESTING.ToString()) ? NeuronState.RESTING : (currentState.Equals(NeuronState.FIRING.ToString()) ? NeuronState.FIRING : currentState.Equals(NeuronState.SPIKING.ToString()) ? NeuronState.SPIKING : throw new InvalidCastException("Could Not convert Neuron State!"));
            Voltage = voltage;
            this.flag = flag;
            PruneCount = pruneCount;
            lastSpikeCycleNum = lastspikecyclenum;
        }

        public void IncrementPruneCount() => PruneCount++;

        #endregion


        #region METHODSS

        public void ProcessCurrentState(ulong cycleNum, LogMode logmode = LogMode.BurstOnly, string fileName = null)
        {
            if( Voltage == 0)
            {
                CurrentState = NeuronState.RESTING;
            }
            else if ( Voltage > 0 && Voltage < COMMON_NEURONAL_FIRE_VOLTAGE)
            {
                CurrentState = NeuronState.PREDICTED;
            }
            else if( Voltage >= COMMON_NEURONAL_FIRE_VOLTAGE && Voltage < COMMON_NEURONAL_SPIKE_TRAIN_VOLTAGE)
            {
                CurrentState = NeuronState.FIRING;
            }
            else if(Voltage >= COMMON_NEURONAL_SPIKE_TRAIN_VOLTAGE)
            {
                if (logmode == LogMode.BurstOnly && fileName != null)
                {
                    WriteLogsToFile("Neuron " + NeuronID.ToString() + " entering Spiking Mode", fileName);
                    if(Voltage >= UNCOMMMON_NEURONAL_SPIKE_TRAIN_VOLTAGE)
                    {
                        WriteLogsToFile("*******************Neuron : " + NeuronID.ToString() + " ENTERING ULTRA SPIKING MODE****************", fileName);
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
            if(NeuronID.ToString().Equals("2-4-0-N"))
            {
                bool breakpoiunt = true;
            }

            Voltage += voltage;            

            if(voltage >= COMMON_NEURONAL_FIRE_VOLTAGE)
            {
                if((logmode == LogMode.Info || logmode == LogMode.All))
                    Console.WriteLine(" INFO :: Neurons.cs :::: Neuron " + NeuronID.ToString() + " is entering firing Mode");               
            }

            ProcessCurrentState(cycleNum, logmode);

            // strengthen the contributed segment if the spike actually resulted in a Fire.
        }

        public string GetMyTemporalPartner1()
        {
            string pos = ProximoDistalDendriticList.Values.FirstOrDefault(synapse => synapse.cType == ConnectionType.TEMPRORAL)?.AxonalNeuronId;

            if (!string.IsNullOrEmpty(pos))
            {
                return pos;                
            }

            throw new InvalidOperationException("GetMyTemproalPartner :: Temporal Neuron Does Not Exist for this Neuron !!! Needs Investigation unless this is the temporal Neuron.");
        }

        public Position_SOM GetMyTemporalPartner2()
        {
            string pos = ProximoDistalDendriticList.Values.FirstOrDefault(synapse => synapse.cType == ConnectionType.TEMPRORAL)?.AxonalNeuronId;

            if (!string.IsNullOrEmpty(pos))
            {                
                return Position_SOM.ConvertStringToPosition(pos);
            }

            throw new InvalidOperationException("GetMyTemproalPartner :: Temporal Neuron Does Not Exist for this Neuron !!! Needs Investigation unless this is the temporal Neuron.");
        }

        public Position_SOM GetMyApicalPartner()
        {
            string pos = ProximoDistalDendriticList.Values?.FirstOrDefault(synapse => synapse.cType == ConnectionType.APICAL)?.AxonalNeuronId;

            if (!string.IsNullOrEmpty(pos))
            {
                return Position_SOM.ConvertStringToPosition(pos);
            }

            throw new InvalidOperationException();
        }

        public string GetMyApicalPartner1()
        {
            string pos = ProximoDistalDendriticList.Values?.FirstOrDefault(synapse => synapse.cType == ConnectionType.APICAL)?.AxonalNeuronId;

            if (!string.IsNullOrEmpty(pos))
            {
                return pos;
            }

            throw new InvalidOperationException();
        }
        
        public bool InitProximalDendriticConnection(int i, int j, int k)
        {                        
            string key = Position_SOM.ConvertIKJtoString(i, j, k);
            return AddNewProximalDendriticConnection(key);
        }

        public void InitAxonalConnectionForConnector(int i, int j, int k)
        {
            string key = Position_SOM.ConvertIKJtoString(i, j, k);
            AddNewAxonalConnection(key);
        }

        internal void CleanUpContributersList()
        {
            TAContributors.Clear();
        }

        private void WriteLogsToFile(string logline, string logfilename)
        {
            File.AppendAllText(logfilename, logline + "\n");
        }


        #region Schema Based Connections

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

        #endregion

        #region Wiring

        //Gets Called for Dendritic End of the Neuron
        public bool AddToDistalList(string axonalNeuronId, NeuronType nTypeSource, ulong CycleNum, SchemaType schemaType, string filename, ConnectionType? cType = null, bool IsActive = false)
        {

            //if (axonalNeuronId == "5-1-7-N" && NeuronID.ToString() == "2-8-0-N")
            //{
            //    bool bp = true;
            //}

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
                        
            // Increment Hit Count!
            if (ProximoDistalDendriticList.TryGetValue(axonalNeuronId, out var synapse))
            {
                synapse.IncrementHitCount(CycleNum);

                return true;
            }
            else
            {
                if (nTypeSource.Equals(NeuronType.NORMAL) && (ProximoDistalDendriticList.Count >= 400 && schemaType == SchemaType.FOMSCHEMA) || (ProximoDistalDendriticList.Count >= 1000 && schemaType == SchemaType.SOMSCHEMA))
                {

                    Console.WriteLine(" WARNING :: Overconnecting Neuron NeuronID : " + NeuronID.ToString());
                    WriteLogsToFile(" WARNING :: Overconnecting Neuron NeuronID : " + NeuronID.ToString(), filename);

                    Console.WriteLine("Total DistalDendritic Count :" + ProximoDistalDendriticList.Count);
                    WriteLogsToFile(" Total DistalDendritic Count : " + NeuronID.ToString(), filename);
                    return false;
                    //Thread.Sleep(1000);
                }



                ProximoDistalDendriticList.Add(axonalNeuronId, new Synapse(axonalNeuronId, NeuronID.ToString(), CycleNum, INITIAL_SYNAPTIC_CONNECTION_STRENGTH, ConnectionType.DISTALDENDRITICNEURON, IsActive));                
                
                
                if (cType.Equals(ConnectionType.DISTALDENDRITICNEURON))
                {
                    BlockBehaviourManager.totalDendronalConnections++;                    
                }

                return true;
            }            
        }

        //Gets called for the axonal end of the neuron
        public bool AddtoAxonalList(string key, NeuronType ntype, ulong CycleNum, ConnectionType connectionType, SchemaType schemaType, bool IsActive = false)
        {            

            if (key.Equals(NeuronID) && this.nType.Equals(ntype))
            {
                throw new InvalidOperationException("Cannot connect neuron to itself");
            }

            if (AxonalList.TryGetValue(key, out var synapse))
            {
                //Console.WriteLine("SOM :: AddtoAxonalList : Connection Already Added Counter : Will Strethen Synapse", ++redundantCounter);

                //synapse.IncrementHitCount();

                Console.WriteLine(schemaType.ToString() + "INFO :: Axon already connected to Neuron");

                return true;
            }
            else
            {
                if ((AxonalList.Count >= 400 && schemaType == SchemaType.FOMSCHEMA) || (ProximoDistalDendriticList.Count >= 1000 && schemaType == SchemaType.SOMSCHEMA))
                {
                    Console.WriteLine(" WARNING :: Overconnecting Neuron NeuronID : " + NeuronID.ToString());
                    Console.WriteLine("Total DistalDendritic Count :" + ProximoDistalDendriticList.Count);
                    return false;
                    //Thread.Sleep(1000);
                }

                AxonalList.Add(key, new Synapse(NeuronID.ToString(), key, CycleNum, AXONAL_CONNECTION, connectionType, IsActive));                

                return true;
            }
        }


        #endregion

        internal ConnectionRemovalReturnType RemoveAxonalConnection(Neuron dendronalNeuron)
        {
            if (AxonalList.TryGetValue(dendronalNeuron.NeuronID.ToString(), out var synapse))
            {                

                if(synapse.cType.Equals(ConnectionType.AXONTONEURON))
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
            if(CurrentState != NeuronState.SPIKING)
            {
                return;
            }

            if(cycleNum == 0 || lastSpikeCycleNum == 0 || lastSpikeCycleNum > cycleNum)
            {
                throw new InvalidOperationException("Last Spiking Value should never be zero");
            }

            if(cycleNum - lastSpikeCycleNum > 1)
            {
                FlushVoltage();
            }
        }

        internal void FlushVoltage()
        {
            //Console.WriteLine("Flushing Voltage on Neuron !!! " + NeuronID.ToString);
            if (NeuronID.ToString().Equals("607-3-3-N")) 
            {
                bool breakpoiunt = true;
            }

            Voltage = 0;
            ProcessCurrentState(555);
        }

        internal bool DidItContribute(Neuron temporalContributor)
        {
            return TAContributors.TryGetValue(temporalContributor.NeuronID.ToString(), out char w);
        }

        internal bool CheckForPrunableConnections(ulong currentCycle)
        {

            foreach (var val in ProximoDistalDendriticList.Values)
            {
                if (val.AnyStale(currentCycle))
                {
                    return true;
                }
            }

            return false;
        }


        #endregion
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

    internal enum ConnectionRemovalReturnType
    {
        TRUE,
        HARDFALSE,
        SOFTFALSE
    }
}