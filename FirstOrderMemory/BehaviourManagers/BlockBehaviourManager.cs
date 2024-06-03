namespace FirstOrderMemory.BehaviourManagers
{
    using FirstOrderMemory.Models;
    using Common;
    using System.Xml;

    public class BlockBehaviourManager
    {
        #region VARIABLES
        public static ulong CycleNum { get; private set; }

        public const bool devbox = false;

        public int NumColumns { get; private set; }

        public int Z { get; private set; }

        public Position_SOM BlockID;

        public Dictionary<string, List<string>> PredictedNeuronsForNextCycle { get; private set; }

        public Dictionary<string, List<string>> PredictedNeuronsforThisCycle { get; private set; }

        public List<Neuron> NeuronsFiringThisCycle { get; private set; }

        public List<Neuron> NeuronsFiringLastCycle { get; private set; }

        private List<Segment>? _predictedSegmentForThisCycle;

        public List<Position_SOM> ColumnsThatBurst { get; private set; }

        private int NumberOfColumnsThatFiredThisCycle { get; set; }

        private List<Neuron> temporalContributors { get; set; }

        private List<Neuron> apicalContributors { get; set; }

        public Column[,] Columns { get; private set; }

        private SDR_SOM SDR { get; set; }

        private Dictionary<ulong, List<Position_SOM>> TemporalCycleCache { get; set; }

        private Dictionary<ulong, List<Position_SOM>> ApicalCycleCache { get; set; }

        private Dictionary<ulong, List<Position_SOM>> BurstCache { get; set; }

        public Neuron[,] TemporalLineArray { get; private set; }

        public Neuron[,] ApicalLineArray { get; private set; }

        public int axonCounter { get; private set; }

        public uint totalProximalConnections;

        public static uint totalDendronalConnections;

        public uint TotalDistalDendriticConnections;

        public uint TotalBurstFire;

        public uint TotalPredictionFires;

        public int PerCycleFireSparsityPercentage;

        public uint totalAxonalConnections;

        private uint num_continuous_burst;

        private bool IsBurstOnly;

        private string backupDirectory = "C:\\Users\\depint\\Desktop\\Hentul\\Hentul\\BackUp\\";

        public BlockCycle PreviousBlockCycle { get; private set; }

        public BlockCycle CurrentCycleState { get; private set; }
        public bool IsCurrentTemporal { get; private set; }
        public bool IsCurrentApical { get; private set; }

        #endregion

        #region CONSTANTS
        public int TOTALNUMBEROFCORRECTPREDICTIONS = 0;
        public int TOTALNUMBEROFINCORRECTPREDICTIONS = 0;
        public int TOTALNUMBEROFPARTICIPATEDCYCLES = 0;
        public static UInt16 DISTALNEUROPLASTICITY = 5;
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
        private int TOTAL_ALLOWED_BURST_PER_CLEANUP = 1;


        #endregion

        #region CONSTRUCTORS & INITIALIZATIONS 

        public BlockBehaviourManager(int numColumns = 10, int Z = 10, int x = 0, int y = 0, int z = 0)
        {
            if (numColumns != Z)
            {
                throw new InvalidOperationException("CONSTRUCTOR :: numColumns should be equal to Z");
            }

            this.BlockID = new Position_SOM(x, y, z);

            this.NumberOfColumnsThatFiredThisCycle = 0;

            CycleNum = 0;

            totalDendronalConnections = 0;

            this.NumColumns = numColumns;

            this.Z = Z;

            PreviousBlockCycle = BlockCycle.INITIALIZATION;

            this.CurrentCycleState = BlockCycle.INITIALIZATION;

            IsCurrentApical = false;

            IsCurrentApical = false;

            PredictedNeuronsforThisCycle = new Dictionary<string, List<string>>();

            //_predictedSegmentForThisCycle = new List<Segment>();
            PredictedNeuronsForNextCycle = new Dictionary<string, List<string>>();

            NeuronsFiringThisCycle = new List<Neuron>();

            NeuronsFiringLastCycle = new List<Neuron>();

            temporalContributors = new List<Neuron>();

            apicalContributors = new List<Neuron>();

            TemporalLineArray = new Neuron[numColumns, numColumns];

            ApicalLineArray = new Neuron[numColumns, numColumns];

            Columns = new Column[numColumns, numColumns];

            ColumnsThatBurst = new List<Position_SOM>();

            //DendriticCache = new Dictionary<string, int[]>();

            //AxonalCache = new Dictionary<string, int[]>();

            TemporalCycleCache = new Dictionary<ulong, List<Position_SOM>>();

            ApicalCycleCache = new Dictionary<ulong, List<Position_SOM>>();

            BurstCache = new Dictionary<ulong, List<Position_SOM>>();

            totalProximalConnections = 0;

            totalAxonalConnections = 0;

            axonCounter = 0;

            num_continuous_burst = 0;

            for (int i = 0; i < numColumns; i++)
            {
                for (int j = 0; j < numColumns; j++)
                {
                    Columns[i, j] = new Column(i, j, Z);
                }
            }
        }

        public void Init(int x = -1, int y = -1)
        {

            ReadDendriticSchema(x, y);

            ReadAxonalSchema(x, y);

            GenerateTemporalLines();

            GenerateApicalLines();
        }

        public void InitAxonalConnectionForConnector(int x, int y, int z, int i, int j, int k)
        {
            if (x == i && y == j && z == k)
            {
                throw new InvalidDataException();
            }
            try
            {
                Column col = this.Columns[x, y];

                Neuron neuron = col.Neurons[z];

                neuron.InitAxonalConnectionForConnector(i, j, k);

                IncrementAxonalConnectionCount();

            }
            catch (Exception ex)
            {

                Console.WriteLine(ex.ToString());

                throw;
            }
        }

        public void InitDendriticConnectionForConnector(int x, int y, int z, int i, int j, int k)
        {
            if (x == i && y == j && z == k || (z > Z))
            {
                int breakpoint = 1;
            }
            try
            {
                Column col = this.Columns[x, y];

                Neuron neuron = col.Neurons[z];

                neuron.InitProximalConnectionForDendriticConnection(i, j, k);

                IncrementProximalConnectionCount();

                if (neuron.flag >= 4)
                {
                    int breakpoint2 = 1;
                }
                else
                {
                    neuron.flag++;
                }
            }
            catch (Exception ex)
            {

                Console.WriteLine(ex.ToString());

                throw;

                int breakpoint = 1;
            }
        }

        public BlockBehaviourManager CloneBBM(int x, int y, int z)
        {
            BlockBehaviourManager blockBehaviourManager;

            blockBehaviourManager = new BlockBehaviourManager(NumColumns, Z, x, y, z);

            blockBehaviourManager.Init();

            return blockBehaviourManager;

            #region Cache Code for Connector

            try
            {
                for (int i = 0; i < blockBehaviourManager.NumColumns; i++)
                {
                    for (int j = 0; j < blockBehaviourManager.NumColumns; j++)
                    {
                        for (int k = 0; k < blockBehaviourManager.Columns[0, 0].Neurons.Count; k++)
                        {
                            //Proximal Dendritic Connections
                            Neuron presynapticNeuron, postSynapticNeuron;

                            for (int l = 0; l < Columns[i, j].Neurons[k].ProximoDistalDendriticList.Values.Count; l++)
                            {
                                var synapse = Columns[i, j].Neurons[k].ProximoDistalDendriticList.Values.ElementAt(l);

                                if (synapse != null)
                                {
                                    if (synapse.cType.Equals(ConnectionType.PROXIMALDENDRITICNEURON))
                                    {
                                        presynapticNeuron = blockBehaviourManager.ConvertStringPosToNeuron(synapse.AxonalNeuronId);
                                        postSynapticNeuron = blockBehaviourManager.ConvertStringPosToNeuron(synapse.DendronalNeuronalId);

                                        if (!blockBehaviourManager.ConnectTwoNeuronsOrIncrementStrength(presynapticNeuron, postSynapticNeuron, ConnectionType.PROXIMALDENDRITICNEURON))
                                        {
                                            Console.WriteLine("Could Not Clone Distal Connection Properly!!!");
                                        }
                                    }
                                }
                                else
                                {
                                    throw new InvalidOperationException("Synapse Came Up Empty in Clone Logic");
                                }
                            }


                            //Axonal Connections
                            for (int l = 0; l < Columns[i, j].Neurons[k].AxonalList.Values.Count; l++)
                            {
                                var synapse = Columns[i, j].Neurons[k].AxonalList.Values.ElementAt(l);

                                if (synapse != null)
                                {
                                    if (synapse.cType.Equals(ConnectionType.AXONTONEURON))
                                    {
                                        presynapticNeuron = blockBehaviourManager.ConvertStringPosToNeuron(synapse.AxonalNeuronId);
                                        postSynapticNeuron = blockBehaviourManager.ConvertStringPosToNeuron(synapse.DendronalNeuronalId);

                                        if (!blockBehaviourManager.ConnectTwoNeuronsOrIncrementStrength(presynapticNeuron, postSynapticNeuron, ConnectionType.AXONTONEURON))
                                        {
                                            Console.WriteLine("Could Not Clone Axonal Connection Properly!!!");
                                        }
                                    }
                                }
                                else
                                {
                                    throw new InvalidOperationException("Synapse Came Up Empty in Clone Logic");
                                }
                            }
                        }
                    }
                }

                blockBehaviourManager.GenerateTemporalLines();

                blockBehaviourManager.GenerateApicalLines();
            }
            catch (Exception e)
            {
                throw;
            }

            #endregion
        }

        public void IncrementDistalDendronalConnection()
        {
            TotalDistalDendriticConnections++;
        }

        public void DecrementDistalDendronalConnection()
        {
            TotalDistalDendriticConnections--;
        }

        #endregion

        #region BACKUP & RESTORE

        public void BackUp(string filename)
        {
            var xmlDocument = new XmlDocument();

            xmlDocument.LoadXml("<Connections></Connections>");

            if (!devbox)
            {
                backupDirectory = "C:\\Users\\depint\\source\\repos\\Hentul\\Hentul\\BackUp";

                filename = "\\DendriticSchema.xml";
            }

            foreach (var column in Columns)
            {
                foreach (var neuron in column.Neurons)
                {
                    var distalList = neuron.ProximoDistalDendriticList.Where(conList => conList.Value.cType.Equals(ConnectionType.DISTALDENDRITICNEURON)).ToList();

                    if (distalList.Count > 0)
                    {
                        foreach (var distalSynapse in distalList)
                        {

                            var distalNode = xmlDocument.CreateNode(XmlNodeType.Element, "DistalDendriticConnection", string.Empty);

                            var blockIdElement = xmlDocument.CreateElement("BlockID", string.Empty);
                            blockIdElement.InnerText = BlockID.X.ToString();

                            var sourceNeuronElement = xmlDocument.CreateElement("SourceNeuronID", string.Empty);

                            sourceNeuronElement.InnerText = distalSynapse.Value.AxonalNeuronId.ToString();

                            var targetNeuronElement = xmlDocument.CreateNode(XmlNodeType.Element, "TargetNeuronID", string.Empty);
                            targetNeuronElement.InnerText = distalSynapse.Value.DendronalNeuronalId.ToString();

                            distalNode.AppendChild(sourceNeuronElement);
                            distalNode.AppendChild(targetNeuronElement);

                            xmlDocument?.DocumentElement?.AppendChild(distalNode);
                        }

                    }
                }
            }

            xmlDocument?.Save(backupDirectory + filename);
        }

        public void RetoreFromBackUp(string filename)
        {

        }

        #endregion

        #region FIRE & WIRE

        private void PreCyclePrep()
        {
            //Prepare all the neurons that are predicted 
            if (PredictedNeuronsForNextCycle.Count != 0 && NeuronsFiringThisCycle.Count != 0)
            {
                Console.WriteLine("Precycle Cleanup Error : _predictedNeuronsForNextCycle is not empty");
                throw new Exception("PreCycle Cleanup Exception!!!");
            }

            if (CurrentCycleState.Equals(BlockCycle.CLEANUP))
            {
                for (int i = 0; i < NumColumns; i++)
                    for (int j = 0; j < NumColumns; j++)
                    {
                        if (Columns[i, j].PreCleanupCheck())
                        {
                            Console.WriteLine("Precycle Cleanup Error : Column {0} {1} is pre Firing", i, j);
                            throw new Exception("PreCycle Cleanup Exception!!!");
                        }
                    }
            }

            NumberOfColumnsThatFiredThisCycle = 0;

            IsBurstOnly = false;

        }

        /// <summary>
        /// Processes the Incoming Input Pattern
        /// </summary>
        /// <param name="incomingPattern"> Pattern to Process</param>
        /// <param name="ignorePrecyclePrep"> Will not Perfrom CleanUp if False and vice versa</param>
        /// <param name="ignorePostCycleCleanUp">Will not Perfrom CleanUp if False and vice versa</param>
        /// <exception cref="InvalidOperationException"></exception>
        public void Fire(SDR_SOM incomingPattern, bool ignorePrecyclePrep = false, bool ignorePostCycleCleanUp = false)
        {
            // Todo : If there is a burst and there is any neuron in any of the columns the fired in the last cycle that has a connection to the bursting column. Column CheckPointing.

            //BUG: Potential Bug:  if after one complete cycle of firing ( T -> A -> Spatial) performing a cleanup might remove reset probabilities for the next fire cycle
            if (ignorePrecyclePrep == false)
                PreCyclePrep();

            if (incomingPattern.InputPatternType.Equals(iType.TEMPORAL))
            {
                if (TemporalCycleCache.Count != 0)
                {
                    Console.WriteLine("ERROR :: Fire() :::: Trying to Add Temporal Pattern to a Valid cache Item!");
                }

                TemporalCycleCache.Add(CycleNum, TransformTemporalCoordinatesToSpatialCoordinates1(incomingPattern.ActiveBits));
            }

            if (incomingPattern.InputPatternType.Equals(iType.APICAL))
            {
                if (ApicalCycleCache.Count != 0)
                {
                    Console.WriteLine("ERROR :: Fire() :::: Trying to Add Apical Pattern to a Valid cache Item!");
                }

                ApicalCycleCache.Add(CycleNum, incomingPattern.ActiveBits);
            }

            if (incomingPattern.ActiveBits.Count == 0)
                return;

            NumberOfColumnsThatFiredThisCycle = incomingPattern.ActiveBits.Count;

            switch (incomingPattern.InputPatternType)
            {
                case iType.SPATIAL:
                    {

                        for (int i = 0; i < incomingPattern.ActiveBits.Count; i++)
                        {
                            var predictedNeuronPositions = Columns[incomingPattern.ActiveBits[i].X, incomingPattern.ActiveBits[i].Y].GetPredictedNeuronsFromColumn();

                            if (incomingPattern.ActiveBits[i].X == 0 && incomingPattern.ActiveBits[i].Y == 4 && incomingPattern.ActiveBits.Count == 2 && predictedNeuronPositions.Count != 10)
                            {
                                int breakpoint = 0;
                            }

                            if (predictedNeuronPositions?.Count == Columns[0, 0].Neurons.Count)
                            {
                                Console.WriteLine("Block ID : " + BlockID.ToString() + " Bursting for incoming pattern X :" + incomingPattern.ActiveBits[i].X + " Y : " + incomingPattern.ActiveBits[i].Y);

                                AddNeuronListToNeuronsFiringThisCycleList(Columns[incomingPattern.ActiveBits[i].X, incomingPattern.ActiveBits[i].Y].Neurons);                                                                

                                ColumnsThatBurst.Add(incomingPattern.ActiveBits[i]);

                                IsBurstOnly = true;

                                num_continuous_burst++;

                                TotalBurstFire++;
                            }
                            else if (predictedNeuronPositions.Count == 1)
                            {
                                //Console.WriteLine("Block ID : " + BlockID.ToString() + " Old  Pattern : Predicting Predicted Neurons Count : " + predictedNeuronPositions.Count.ToString());

                                AddNeuronListToNeuronsFiringThisCycleList(predictedNeuronPositions);

                                TotalPredictionFires++;
                            }
                            else
                            {

                                Console.WriteLine("There Should only be one winner in the Column");

                                throw new InvalidOperationException("Fire :: This should not happen ! Bug in PickAwinner or Bursting Logic!!!");

                            }
                            predictedNeuronPositions = null;
                        }

                        CurrentCycleState = BlockCycle.FIRING;

                        break;
                    }
                case iType.TEMPORAL:
                    {
                        IsCurrentTemporal = true;

                        PreviousBlockCycle = BlockCycle.DEPOLARIZATION;

                        List<Neuron> temporalLineNeurons = TransformTemporalCoordinatesToSpatialCoordinates(incomingPattern.ActiveBits as List<Position_SOM>);

                        if (temporalLineNeurons.Count != 0)
                        {
                            foreach (var temporalNeuron in temporalLineNeurons)
                            {
                                AddNeuronToNeuronsFiringThisCycleList(temporalNeuron);

                                temporalContributors.Add(temporalNeuron);
                            }
                        }

                        break;
                    }
                case iType.APICAL:
                    {
                        PreviousBlockCycle = BlockCycle.DEPOLARIZATION;

                        IsCurrentApical = true;

                        List<Neuron> apicalLineNeurons = new List<Neuron>();

                        foreach (var pos in incomingPattern.ActiveBits)
                        {
                            apicalLineNeurons.Add(this.ApicalLineArray[pos.X, pos.Y]);
                        }

                        if (ApicalLineArray != null && apicalLineNeurons.Count != 0)
                        {
                            foreach (var apicalNeuron in apicalLineNeurons)
                            {
                                AddNeuronToNeuronsFiringThisCycleList(apicalNeuron);

                                apicalContributors.Add(apicalNeuron);
                            }
                        }

                        break;
                    }
                default:
                    {
                        throw new InvalidOperationException("Invalid Input Pattern Type");
                    }
            }

            if (IsBurstOnly)
                BurstCache.Add(CycleNum, incomingPattern.ActiveBits);
            else
            {
                Console.WriteLine("ERROR : Fire :: BurstCache was not cleaned up from last cycle");
            }

            Fire();

            if (CurrentCycleState == BlockCycle.FIRING || IsCurrentApical || IsCurrentTemporal)
            {
                Wire();
            }

            PrepNetworkForNextCycle();

            if (ignorePostCycleCleanUp == false)
                PostCycleCleanup();

        }

        private void PostCycleCleanup()
        {
            //Todo : Need Selective Clean Up Logic , Should never perform Full Clean up.

            //Case 1 : If temporal or Apical or both lines have deplolarized and spatial fired then clean up temporal or apical or both.
            if (PreviousBlockCycle.Equals(BlockCycle.DEPOLARIZATION) && CurrentCycleState.Equals(BlockCycle.FIRING))
            {
                if (TemporalCycleCache.Count == 1)
                {
                    foreach (var kvp in TemporalCycleCache)
                    {
                        if (CycleNum - kvp.Key > 3)
                        {
                            Console.WriteLine("ERROR :: PostCycleCleanUp :: Temporal Cached Pattern is older than Spatial Pattern!");
                        }

                        foreach (var pos in kvp.Value)
                        {
                            if (pos.X == 4 && pos.Y == 2)
                            {
                                int breakpoint = 0;
                            }
                            foreach (var synapse in TemporalLineArray[pos.X, pos.Y].AxonalList.Values)
                            {
                                if (synapse.DendronalNeuronalId != null)// && BBMUtils.CheckNeuronListHasThisNeuron(NeuronsFiringThisCycle, synapse.DendronalNeuronalId))
                                {
                                    var neuronToCleanUp = ConvertStringPosToNeuron(synapse.DendronalNeuronalId);

                                    if (neuronToCleanUp.Voltage != 0)
                                    {
                                        neuronToCleanUp.FlushVoltage();
                                    }
                                }
                            }
                        }
                    }

                    TemporalCycleCache.Clear();
                }
                else if (TemporalCycleCache.Count > 1)
                {
                    Console.WriteLine("ERROR :: PostCycleCleanUp() :: TemporalCycle Cache count is more than 1 , It should always be 1");
                    throw new InvalidOperationException("TemporalCycle Cache Size should always be 1");
                }

                if (ApicalCycleCache.Count == 1)
                {
                    foreach (var kvp in ApicalCycleCache)
                    {
                        if (CycleNum - kvp.Key > 3)
                        {
                            Console.WriteLine("ERROR :: PostCycleCleanUp :: Temporal Cached Pattern is older than Spatial Pattern!");
                        }

                        foreach (var pos in kvp.Value)
                        {
                            foreach (var synapse in ApicalLineArray[pos.X, pos.Y].AxonalList.Values)
                            {
                                if (synapse.DendronalNeuronalId != null) // && BBMUtils.CheckNeuronListHasThisNeuron(NeuronsFiringThisCycle, synapse.DendronalNeuronalId) == false)
                                {
                                    var neuronToCleanUp = ConvertStringPosToNeuron(synapse.DendronalNeuronalId);

                                    if (neuronToCleanUp.Voltage != 0)
                                    {
                                        neuronToCleanUp.FlushVoltage();
                                    }
                                    else
                                    {
                                        Console.WriteLine("WARNING :: PostCycleCleanUp ::: Tried to clean up a neuron which was not depolarized!!! ");
                                    }
                                }
                            }
                        }
                    }

                    ApicalCycleCache.Clear();
                }
                else if (ApicalCycleCache.Count > 1)
                {
                    Console.WriteLine("ERROR :: PostCycleCleanUp() :: TemporalCycle Cache count is more than 1 , It should always be 1");
                    throw new InvalidOperationException("TemporalCycle Cache Size should always be 1");
                }

                PreviousBlockCycle = BlockCycle.POLOARIZED;
            }


            //Case 2 : If a bursting signal came through , after wire , the Bursted neurons and all its connected cells should be cleaned up. Feature : How many Burst Cycle to wait before performing a full clean ? Answer : 1
            if (ColumnsThatBurst.Count != 0)
            {
                foreach (var kvp in BurstCache)
                {
                    if (CycleNum - kvp.Key > 3)
                    {
                        Console.WriteLine("ERROR :: PostCycleCleanUp :: Temporal Cached Pattern is older than Spatial Pattern!");
                    }

                    foreach (var pos in kvp.Value)
                    {
                        for (int i = 0; i < NumColumns; i++)
                        {
                            var neuronToCleanUp = Columns[pos.X, pos.Y].Neurons[i];

                            if (neuronToCleanUp.Voltage != 0)
                            {
                                neuronToCleanUp.FlushVoltage();
                            }
                            else
                            {
                                Console.WriteLine("WARNING :: PostCycleCleanUp ::: CASE 3 [Burwst Clean up] :::: Tried to clean up a neuron which was not depolarized!!!");
                            }
                        }
                    }
                }

                BurstCache.Clear();
            }


            //Case 3: If NeuronFiringThicCycle has any Temporal / APical Firing Neurons they should be cleaned up [Thougt : The neurons have already fired and Wired , Keepign them in the list will only conplicate next cycle process            
            NeuronsFiringThisCycle.Clear();
            ColumnsThatBurst.Clear();


            //Every 50 Cycles Prune unused and under Firing Connections
            if (BlockBehaviourManager.CycleNum >= 50 && BlockBehaviourManager.CycleNum % 50 == 0)
            {
                foreach (var col in this.Columns)
                {
                    col.PruneCycleRefresh();
                }

                TotalBurstFire = 0;
                TotalPredictionFires = 0;
            }

            IsBurstOnly = false;

            CycleNum++;
            // Process Next pattern.          
        }

        private void PrepNetworkForNextCycle()
        {

            NeuronsFiringLastCycle.Clear();

            foreach (var neuron in NeuronsFiringThisCycle)
            {
                if (neuron.nType.Equals(NeuronType.NORMAL))
                    NeuronsFiringLastCycle.Add(neuron);
            }

            PredictedNeuronsforThisCycle.Clear();

            foreach (var kvp in PredictedNeuronsForNextCycle)
            {
                PredictedNeuronsforThisCycle[kvp.Key] = kvp.Value;
            }

            PredictedNeuronsForNextCycle.Clear();
        }

        private void Fire()
        {
            PerCycleFireSparsityPercentage = (NeuronsFiringThisCycle.Count / (NumColumns * NumColumns * Z)) * 100;

            foreach (var neuron in NeuronsFiringThisCycle)
            {
                //check if the synapse is active only then fire

                //Console.WriteLine(" Firing Neuron : " + neuron.NeuronID.ToString());

                neuron.Fire();

                foreach (Synapse synapse in neuron.AxonalList.Values)
                {
                    ProcessSpikeFromNeuron(ConvertStringPosToNeuron(synapse.AxonalNeuronId), ConvertStringPosToNeuron(synapse.DendronalNeuronalId), synapse.cType);
                }
            }
        }

        private void Wire()
        {
            if (CurrentCycleState.Equals(BlockCycle.FIRING))
            {

                ///Case 1 : All Predicted Neurons Fire : Strengthen only the correct predictions.
                ///Case 2 : Few Fired , Few Bursted : Strengthen the Correctly Fired Neurons , For Bursted , Analyse did anybody contribut to the column and dint burst ? if nobody contributed then do X
                ///Case 3 : All columns Bursted : highly likely first fire or totally new pattern coming in , If firing early cycle , then just wire minimum strength for the connections and move on , if in the middle of the cycle( atleast 10,000 cycle ) then Need to do somethign new Todo .

                //Get intersection of neuronsFiringThisCycle and predictedNeuronsFromThisCycle as if any neurons that were predicted for this cycle actually fired then we got to strengthen those connections first

                CurrentCycleState = BlockCycle.FIRING;

                List<Neuron> predictedNeuronList = new List<Neuron>();

                foreach (var item in PredictedNeuronsforThisCycle.Keys)
                {
                    var neuronToAdd = ConvertStringPosToNeuron(item);

                    if (neuronToAdd != null)
                    {
                        predictedNeuronList.Add(neuronToAdd);
                    }
                };

                var correctPredictionList = NeuronsFiringThisCycle.Intersect(predictedNeuronList).ToList<Neuron>();

                // ColumnsThatBurst.Count == 0 && correctPredictionList.Count = 5 &&  NumberOfColumsnThatFiredThisCycle = 8  cycleNum = 4 , repNum = 29
                if (ColumnsThatBurst.Count == 0 && correctPredictionList.Count != 0 && correctPredictionList.Count == NumberOfColumnsThatFiredThisCycle)
                {
                    //Case 1: All Predicted Neurons Fired without anyone Bursting.

                    List<string> contributingList;
                    foreach (var correctlyPredictedNeuron in correctPredictionList)
                    {
                        contributingList = new List<string>();

                        if (PredictedNeuronsforThisCycle.TryGetValue(correctlyPredictedNeuron.NeuronID.ToString(), out contributingList))
                        {
                            if (contributingList?.Count == 0)
                            {
                                throw new Exception("Wire : This should never Happen!! Contributing List should not be empty");
                            }

                            foreach (var contributingNeuron in contributingList)
                            {
                                //fPosition.ConvertStringPosToNeuron(contributingNeuron).PramoteCorrectPredictionAxonal(correctlyPredictedNeuron);

                                PramoteCorrectlyPredictionDendronal(ConvertStringPosToNeuron(contributingNeuron), correctlyPredictedNeuron);
                            }
                        }
                    }
                }
                else if (ColumnsThatBurst.Count != 0 && correctPredictionList.Count != 0)
                {
                    //Case 2 :  Few Fired, Few Bursted  : Strengthen the Correctly Fired Neurons
                    //          For Correctly Predicted : Pramote Coorectly PRedicted Synapses. 
                    //          For Bursted             : Analyse did anybody contribut to the column and dint burst ? if nobody contributed then do Wire 1 Distal Synapses with all the neurons that fired last cycle                   



                    //Boost the few correctly predicted neurons
                    List<string> contributingList;

                    foreach (var correctlyPredictedNeuron in correctPredictionList)
                    {
                        //if correctlyPredictedNeuron is also present in ColumnsThatBurst then remove it from the list ColumnsThatBurst , leads to double Wiring!!!
                        int index = BBMUtils.CheckIfPositionListHasThisPosition(ColumnsThatBurst, correctlyPredictedNeuron.NeuronID);

                        if (index != -1 && index < ColumnsThatBurst.Count)
                        {
                            ColumnsThatBurst.RemoveAt(index);
                        }

                        contributingList = new List<string>();

                        if (PredictedNeuronsforThisCycle.TryGetValue(correctlyPredictedNeuron.NeuronID.ToString(), out contributingList))
                        {
                            if (contributingList?.Count == 0)
                            {
                                throw new Exception("Wire : This should never Happen!! Contributing List should not be empty");
                            }

                            foreach (var contributingNeuron in contributingList)
                            {
                                //Position.ConvertStringPosToNeuron(contributingNeuron).PramoteCorrectPredictionAxonal(correctlyPredictedNeuron);

                                PramoteCorrectlyPredictionDendronal(ConvertStringPosToNeuron(contributingNeuron), correctlyPredictedNeuron);
                            }
                        }
                    }

                    //Bug : Boosting should not juice the same neurons!

                    //Boost the Bursting neurons
                    foreach (var position in ColumnsThatBurst)
                    {
                        foreach (var dendriticNeuron in Columns[position.X, position.Y].Neurons)
                        {
                            foreach (var axonalNeuron in NeuronsFiringLastCycle)
                            {
                                if (CheckifBothNeuronsAreSame(dendriticNeuron, axonalNeuron) == false)
                                {

                                    ConnectTwoNeuronsOrIncrementStrength(axonalNeuron, dendriticNeuron, ConnectionType.DISTALDENDRITICNEURON);

                                }
                            }
                        }
                    }

                }// ColumnsThatBurst.Count == 0 && correctPredictionList.Count = 5 &&  NumberOfColumnsThatFiredThisCycle = 8  cycleNum = 4 , repNum = 29
                else if (ColumnsThatBurst.Count == 0 && NumberOfColumnsThatFiredThisCycle > correctPredictionList.Count)
                {
                    // Case 3 : None Bursted , Some Fired which were NOT predicted , Some fired which were predicted
                    // Strengthen the ones which fired correctly 
                    List<string> contributingList;

                    foreach (var correctlyPredictedNeuron in correctPredictionList)
                    {
                        contributingList = new List<string>();

                        if (PredictedNeuronsforThisCycle.TryGetValue(correctlyPredictedNeuron.NeuronID.ToString(), out contributingList))
                        {
                            if (contributingList?.Count == 0)
                            {
                                throw new Exception("Wire : This should never Happen!! Contributing List should not be empty");
                            }

                            foreach (var contributingNeuron in contributingList)
                            {
                                //fPosition.ConvertStringPosToNeuron(contributingNeuron).PramoteCorrectPredictionAxonal(correctlyPredictedNeuron);

                                PramoteCorrectlyPredictionDendronal(ConvertStringPosToNeuron(contributingNeuron), correctlyPredictedNeuron);
                            }
                        }
                    }


                    // The ones that fired without prediction , parse through them and strengthen
                    foreach (var position in ColumnsThatBurst)
                    {
                        foreach (var dendriticNeuron in Columns[position.X, position.Y].Neurons)
                        {
                            foreach (var axonalNeuron in NeuronsFiringLastCycle)
                            {
                                if (CheckifBothNeuronsAreSame(axonalNeuron, dendriticNeuron) == false)
                                    ConnectTwoNeuronsOrIncrementStrength(axonalNeuron, dendriticNeuron, ConnectionType.DISTALDENDRITICNEURON);
                            }
                        }
                    }

                    // Fired But Did Not Get Predicted
                    foreach (var axonalneuron in NeuronsFiringLastCycle)
                    {
                        foreach (var dendronalNeuron in NeuronsFiringThisCycle)
                        {
                            if (CheckifBothNeuronsAreSame(axonalneuron, dendronalNeuron) == false)
                                ConnectTwoNeuronsOrIncrementStrength(axonalneuron, dendronalNeuron, ConnectionType.DISTALDENDRITICNEURON);
                        }
                    }
                }
                else if (ColumnsThatBurst.Count == NumberOfColumnsThatFiredThisCycle && correctPredictionList.Count == 0)
                {
                    //Case 3 : All columns Bursted: highly likely first fire or totally new pattern coming in :
                    //         If firing early cycle, then just wire 1 Distal Syanpse to all the neurons that fired last cycle and 1 random connection.
                    //         If in the middle of the cycle(atleast 10,000 cycles ) then Need to do somethign new Todo.




                    //BUG 1: NeuronsFiredLastCycle = 10 when last cycle was a Burst Cycle and if this cycle is a Burst cycle then the NeuronsFiringThisCycle will be 10 as well , that leads to 100 new distal connections , not healthy.
                    //Feature : Synapses will be marked InActive on Creation and eventually marked Active once the PredictiveCount increases.
                    //BUG 2: TotalNumberOfDistalConnections always get limited to 400

                    foreach (var position in ColumnsThatBurst)
                    {
                        foreach (var dendriticNeuron in Columns[position.X, position.Y].Neurons)
                        {
                            foreach (var axonalNeuron in NeuronsFiringLastCycle)
                            {
                                ConnectTwoNeuronsOrIncrementStrength(axonalNeuron, dendriticNeuron, ConnectionType.DISTALDENDRITICNEURON);
                            }
                        }
                    }
                }
                else if (ColumnsThatBurst.Count < NumberOfColumnsThatFiredThisCycle && correctPredictionList.Count == 0)
                {
                    //Bug : Somehow the all the neurons in the column have the same voltage , but none of them are added to the PredictedNeuronsForThisCycle List from last firing Cycle.

                    Console.WriteLine("WARNING !    WARNING !   WARNING !    WARNING !  WARNING !    WARNING !  WARNING !    WARNING !  WARNING !    WARNING !  WARNING !    WARNING !  WARNING !    WARNING !");
                    Console.WriteLine("CASE 4 happened Again");

                    //May be some voltage on this column was not cleaned up from last cycle somehow or may be its because of the Synapse Not Active Logic i put few weeks back because of PredictedNeuronsList Getting overloaded to 400. now its reducded to 60 per cycle.
                }
                else
                {   // BUG: Few Bursted , Few Fired which were not predicted // Needs analysis on how something can fire without bursting which were not predicted.
                    throw new NotImplementedException("This should never happen or the code has bugs! Get on it Biiiiiyaaattttcccchhhhhhhh!!!!!");
                }

            }
            else if (IsCurrentTemporal)
            {
                CurrentCycleState = BlockCycle.DEPOLARIZATION;

                //Get intersection between temporal input SDR and the firing Neurons if any fired and strengthen it
                foreach (var neuron in NeuronsFiringThisCycle)
                {
                    if (neuron.nType.Equals(NeuronType.NORMAL))
                    {
                        foreach (var temporalContributor in temporalContributors)
                        {
                            if (neuron.DidItContribute(temporalContributor))
                            {
                                PramoteCorrectlyPredictionDendronal(temporalContributor, neuron);
                            }
                        }
                    }
                }
            }
            else if (IsCurrentApical)
            {
                //Get intersection between temporal input SDR and the firing Neurons if any fired and strengthen it

                CurrentCycleState = BlockCycle.DEPOLARIZATION;

                foreach (var neuron in NeuronsFiringThisCycle)
                {
                    foreach (var apicalContributor in apicalContributors)
                    {
                        if (neuron.DidItContribute(apicalContributor))
                        {
                            PramoteCorrectlyPredictionDendronal(apicalContributor, neuron);
                        }
                    }
                }
            }
        }

        private void ProcessSpikeFromNeuron(Neuron sourceNeuron, Neuron targetNeuron, ConnectionType cType = ConnectionType.PROXIMALDENDRITICNEURON)
        {

            if (sourceNeuron.NeuronID.ToString().Equals("0-2-0-N"))
            {
                bool breakpoint = false;
            }

            //Do not added Temporal and Apical Neurons to NeuronsFiringThisCycle, it throws off Wiring.            
            AddPredictedNeuronForNextCycle(targetNeuron, sourceNeuron.NeuronID.ToString());

            if (cType.Equals(ConnectionType.TEMPRORAL) || cType.Equals(ConnectionType.APICAL))
            {
                if (!targetNeuron.TAContributors.TryGetValue(sourceNeuron.NeuronID.ToString(), out char w))
                {
                    if (cType.Equals(ConnectionType.TEMPRORAL))
                    {
                        targetNeuron.TAContributors.Add(sourceNeuron.NeuronID.ToString(), 'T');
                        targetNeuron.ProcessVoltage(TEMPORAL_NEURON_FIRE_VALUE);
                    }
                    else if (cType.Equals(ConnectionType.APICAL))
                    {
                        targetNeuron.TAContributors.Add(sourceNeuron.NeuronID.ToString(), 'A');
                        targetNeuron.ProcessVoltage(APICAL_NEURONAL_FIRE_VALUE);
                    }
                }
                else
                {
                    if (cType.Equals(ConnectionType.TEMPRORAL))
                        Console.WriteLine("ERROR :: ProcessSpikeFromNeuron :::: Temporal Line Array should be connected in both ways to there respective Temporal / Apical Neuronal Partners");
                    else if (cType.Equals(ConnectionType.APICAL))
                        Console.WriteLine("ERROR :: ProcessSpikeFromNeuron :::: Apical Line Array should be connected in both ways to there respective Temporal / Apical Neuronal Partners");
                    else
                        throw new InvalidOperationException("Invalid Connections");
                }
            }
            else if (targetNeuron.ProximoDistalDendriticList.TryGetValue(sourceNeuron.NeuronID.ToString(), out var synapse))
            {
                if (synapse.IsActive)       //Process Voltage only if the synapse is active otherwise Increment HitCount.
                {
                    switch (synapse.cType)
                    {
                        case ConnectionType.DISTALDENDRITICNEURON:
                            targetNeuron.ProcessVoltage(DISTAL_VOLTAGE_SPIKE_VALUE);
                            break;
                        case ConnectionType.PROXIMALDENDRITICNEURON:
                            targetNeuron.ProcessVoltage(PROXIMAL_VOLTAGE_SPIKE_VALUE);
                            break;
                        case ConnectionType.NMDATONEURON:
                            targetNeuron.ProcessVoltage(NMDA_NEURONAL_FIRE_VALUE);
                            break;
                    }
                }
                //else      //This should always be commented because if the second neuron did not fire in the next cycle then the hitcount should not be incremented , the increment should only happen in cases when the second neuron actually did fire in the next timestamp.
                //{
                //    synapse.IncrementHitCount();
                //}
            }
            else if (cType.Equals(ConnectionType.AXONTONEURON))
            {
                targetNeuron.ProcessVoltage(PROXIMAL_AXON_TO_NEURON_FIRE_VALUE);
            }
            else
            {
                throw new InvalidOperationException("ProcessSpikeFromNeuron : Trying to Process Spike from Neuron which is not connected to this Neuron");
            }
        }

        public void AddPredictedNeuronForNextCycle(Neuron predictedNeuron, string contributingNeuron)
        {
            List<string> contributingList = new List<string>();

            //If bursting then 
            if (predictedNeuron.NeuronID.X == 5 && predictedNeuron.NeuronID.Y == 5 && predictedNeuron.NeuronID.Z == 3)
            {
                int breakpoint = 1;
            }

            if (PredictedNeuronsForNextCycle.Count > 0 && PredictedNeuronsForNextCycle.TryGetValue(predictedNeuron.NeuronID.ToString(), out contributingList))
            {
                contributingList.Add(contributingNeuron);
            }
            else
            {
                PredictedNeuronsForNextCycle.Add(predictedNeuron.NeuronID.ToString(), new List<string>() { contributingNeuron });
            }
        }

        public bool ConnectTwoNeuronsOrIncrementStrength(Neuron AxonalNeuron, Neuron DendriticNeuron, ConnectionType cType)
        {
            // Make sure while connecting two neurons we enver connect 2 neurons from the same column to each other , this might result in a fire loop.
            if (cType == null)
            {
                bool breakpoint = false;
                breakpoint = true;
            }

            if (AxonalNeuron == null || DendriticNeuron == null)
                return false;

            if (CurrentCycleState != BlockCycle.INITIALIZATION && (AxonalNeuron.nType.Equals(NeuronType.TEMPORAL) || AxonalNeuron.nType.Equals(NeuronType.APICAL)))        // Post Init Temporal / Apical Neurons should not connect with anybody else.
            {
                throw new InvalidOperationException("ConnectTwoNeuronsOrIncrementStrength :: Temproal Neurons cannot connect to Normal Neurons Post Init!");
            }

            if (((AxonalNeuron.NeuronID.X == DendriticNeuron.NeuronID.X && AxonalNeuron.NeuronID.Y == DendriticNeuron.NeuronID.Y) ||            // No Same Column Connections 
                AxonalNeuron.NeuronID.Equals(DendriticNeuron.NeuronID)) &&                                                                      // No Selfing
                AxonalNeuron.nType.Equals(DendriticNeuron.nType))                                                                               // Prevents a lot of False Positives from Throwing Error.
            {
                Console.WriteLine("ConnectTwoNeurons : Cannot Connect Neuron to itself!");

                //throw new InvalidDataException("ConnectTwoNeurons: Cannot connect Neuron to Itself!");
                return false;
            }

            if (AxonalNeuron.AddtoAxonalList(DendriticNeuron.NeuronID.ToString(), AxonalNeuron.nType, cType) && DendriticNeuron.AddToDistalList(AxonalNeuron.NeuronID.ToString(), DendriticNeuron.nType, cType))
            {

                if (cType.Equals(ConnectionType.DISTALDENDRITICNEURON))
                {
                    TotalDistalDendriticConnections++;
                }

                return true;
            }

            return false;
        }

        public bool CheckifBothNeuronsAreSame(Neuron neuron1, Neuron neuron2) =>
            neuron1.NeuronID.X == neuron2.NeuronID.X && neuron1.NeuronID.Y == neuron2.NeuronID.Y && neuron1.NeuronID.Z == neuron2.NeuronID.Z;

        public Neuron GetNeuronFromPosition(Position_SOM pos)
            => Columns[pos.X, pos.Y].Neurons[pos.Z];

        public Neuron GetNeuronFromPosition(char w, int x, int y, int z)
        {
            Neuron toReturn = null;

            if (w == 'N' || w == 'n')
            {
                toReturn = Columns[x, y].Neurons[z];
            }
            else if (w == 'T' || w == 't')
            {
                toReturn = TemporalLineArray[y, z];
            }
            else if (w == 'A' || w == 'a')
            {
                toReturn = ApicalLineArray[x, y];
            }
            else
            {
                throw new InvalidOperationException(" GetNeuronFromPosition :: Your Column structure is messed up!!!");
            }

            return toReturn;
        }

        public Neuron ConvertStringPosToNeuron(string posString)
        {
            var parts = posString.Split('-');
            int x = Convert.ToInt32(parts[0]);
            int y = Convert.ToInt32(parts[1]);
            int z = Convert.ToInt32(parts[2]);
            char nType = 'N';

            if (parts.Length == 4)
            {
                nType = Convert.ToChar(parts[3]);
            }

            if (x > 9 || y > 9 || z > 9)
            {
                int breakpoint = 1;

                throw new NullReferenceException("ConvertStringPosToNeuron : Couldnt Find the neuron in the columns Block ID : " + BlockID.ToString() + " : posString :  " + posString);
            }

            return GetNeuronFromPosition(nType, x, y, z);

        }

        public SDR_SOM GetPredictedSDR()
        {
            List<Position_SOM> ActiveBits = new List<Position_SOM>();

            foreach (var neuronstringID in PredictedNeuronsforThisCycle.Keys)
            {
                var pos = Position_SOM.ConvertStringToPosition(neuronstringID);

                if (!ActiveBits.Any(pos1 => pos1.X == pos.X && pos1.Y == pos.Y && pos1.Z == pos.Z))
                    ActiveBits.Add(pos);
            }

            ActiveBits.Sort();

            return new SDR_SOM(NumColumns, NumColumns, ActiveBits, iType.SPATIAL);
        }

        public SDR_SOM GetAllFiringNeuronsThisCycle()
        {
            List<Position_SOM> activeBits = new List<Position_SOM>();

            NeuronsFiringThisCycle.ForEach(n => { if (n.nType == NeuronType.NORMAL) activeBits.Add(n.NeuronID); });

            return new SDR_SOM(NumColumns, NumColumns, activeBits, iType.SPATIAL);
        }

        #endregion

        #region PRIVATE METHODS

        private void StrengthenTemporalConnection(Neuron neuron)
        {
            PramoteCorrectlyPredictionDendronal(ConvertStringPosToNeuron(neuron.GetMyTemporalPartner()), neuron);
        }

        private void StrengthenApicalConnection(Neuron neuron)
        {
            PramoteCorrectlyPredictionDendronal(ConvertStringPosToNeuron(neuron.GetMyApicalPartner()), neuron);
        }

        private void PramoteCorrectlyPredictionDendronal(Neuron contributingNeuron, Neuron targetNeuron)
        {
            if (targetNeuron.ProximoDistalDendriticList.Count == 0)
            {
                throw new Exception("Not Supposed to Happen : Trying to Pramote connection on a neuron , not connected yet!");
            }

            if (targetNeuron.ProximoDistalDendriticList.TryGetValue(contributingNeuron.NeuronID.ToString(), out Synapse synapse))
            {
                if (synapse == null)
                {
                    Console.WriteLine(" ERROR :: PramoteCorrectPredictionDendronal: Trying to increment strength on a synapse object that was null!!!");
                    throw new InvalidOperationException("Not Supposed to happen!");
                }

                //Console.WriteLine("SOM :: Pramoting Correctly Predicted Dendronal Connections");

                synapse.IncrementHitCount();
            }
        }

        public void PrintBlockStats()
        {
            // Print sparsity of the block.
            // print warning when all the bits of the neuron fired. with more than 8% sparsity
            if (TotalBurstFire > 0 || TotalPredictionFires > 0)
            {
                Console.WriteLine("  " + BlockID.ToString() + "          W:" + TotalBurstFire.ToString() + "             C:" + TotalPredictionFires.ToString() + "     Fire Sparsity : " + PerCycleFireSparsityPercentage.ToString() + "%");
            }
        }

        private void GenerateTemporalLines()
        {
            // T : (x,y, z) => (0,y,x)

            for (int i = 0; i < NumColumns; i++)
            {
                for (int j = 0; j < NumColumns; j++)
                {

                    if (this.TemporalLineArray[i, j] == null)
                        this.TemporalLineArray[i, j] = new Neuron(new Position_SOM(0, i, j, 'T'), NeuronType.TEMPORAL);

                    for (int k = 0; k < NumColumns; k++)
                    {
                        ConnectTwoNeuronsOrIncrementStrength(this.TemporalLineArray[i, j], Columns[k, i].Neurons[j], ConnectionType.TEMPRORAL);
                    }
                }
            }
        }

        public void AddNeuronListToNeuronsFiringThisCycleList(List<Neuron> neuronToAddList)
        {
            foreach (var neuronToAdd in neuronToAddList)
            {
                AddNeuronToNeuronsFiringThisCycleList(neuronToAdd);
            }
        }

        private void AddNeuronToNeuronsFiringThisCycleList(Neuron neuronToAdd)
        {
            foreach (var neuron in NeuronsFiringThisCycle)
            {
                if (neuron.NeuronID.Equals(neuronToAdd.NeuronID))
                {
                    return;
                }
            }

            NeuronsFiringThisCycle.Add(neuronToAdd);
        }

        private void GenerateApicalLines()
        {
            for (int i = 0; i < NumColumns; i++)
            {
                for (int j = 0; j < NumColumns; j++)
                {
                    this.ApicalLineArray[i, j] = new Neuron(new Position_SOM(i, j, 0, 'A'), NeuronType.APICAL);

                    for (int k = 0; k < NumColumns; k++)
                    {
                        ConnectTwoNeuronsOrIncrementStrength(this.ApicalLineArray[i, j], Columns[i, j].Neurons[k], ConnectionType.APICAL);
                    }
                }
            }
        }

        private List<Neuron> TransformTemporalCoordinatesToSpatialCoordinates(List<Position_SOM> activeBits)
        {
            List<Neuron> temporalNeurons = new List<Neuron>();

            if (activeBits.Count == 0)
                return temporalNeurons;

            foreach (var position in activeBits)
            {
                temporalNeurons.Add(this.TemporalLineArray[position.Y, position.X]);
            }

            return temporalNeurons;

        }

        private List<Position_SOM> TransformTemporalCoordinatesToSpatialCoordinates1(List<Position_SOM> activeBits)
        {
            List<Position_SOM> temporalNeurons = new List<Position_SOM>();

            if (activeBits.Count == 0)
                return temporalNeurons;

            foreach (var position in activeBits)
            {
                temporalNeurons.Add(new Position_SOM(position.Y, position.X));
            }

            return temporalNeurons;
        }

        private List<Position_SOM> TransformTemporalCoordinatesToSpatialCoordinates2(List<Position_SOM> activeBits)
        {
            List<Position_SOM> temporalNeuronsPositions = new List<Position_SOM>();

            if (activeBits.Count == 0)
                return temporalNeuronsPositions;

            foreach (var position in activeBits)
            {
                temporalNeuronsPositions.Add(this.TemporalLineArray[position.Y, position.X].NeuronID);
            }

            return temporalNeuronsPositions;
        }

        private List<Neuron> TransformTemporalCoordinatesToSpatialCoordinates3(List<Position_SOM> activeBits)
        {
            List<Neuron> temporalNeurons = new List<Neuron>();

            if (activeBits.Count == 0)
                return temporalNeurons;

            foreach (var position in activeBits)
            {
                temporalNeurons.Add(this.TemporalLineArray[position.Y, position.X]);
            }

            return temporalNeurons;
        }

        private List<Neuron> TransformTemporalCoordinatesToSpatialCoordinates4(List<Position_SOM> positionLists)
        {
            List<Neuron> toReturn = new List<Neuron>();

            if (positionLists.Count == 0)
                return toReturn;

            toReturn = new List<Neuron>();

            foreach (var position in positionLists)
            {
                for (int i = 0; i < this.NumColumns; i++)
                {
                    toReturn.Add(this.Columns[i, position.Y].Neurons[position.X]);
                }
            }

            return toReturn;
        }

        private void IncrementProximalConnectionCount()
        {
            this.totalProximalConnections++;
        }

        private void IncrementAxonalConnectionCount()
        {
            this.totalAxonalConnections++;
        }

        private void ReadDendriticSchema(int intX, int intY)
        {

            #region REAL Code

            // Todo: Extend Support for Columns Length unique from Number of Rows and Columns.

            XmlDocument document = new XmlDocument();

            string dendriteDocumentPath;

            if (devbox)
            {
                dendriteDocumentPath = "C:\\Users\\depint\\Desktop\\Hentul\\FirstOrderMemory\\Schema Docs\\ConnectorSchema.xml";
            }
            else
            {
                dendriteDocumentPath = "C:\\Users\\depint\\source\\repos\\Hentul\\FirstOrderMemory\\Schema Docs\\ConnectorSchema.xml";
            }

            if (!File.Exists(dendriteDocumentPath))
            {
                throw new FileNotFoundException(dendriteDocumentPath);
            }

            document.Load(dendriteDocumentPath);

            using (XmlNodeList columns = document.GetElementsByTagName("Column"))
            {

                var numColumns = columns.Count;

                for (int i = 0; i < numColumns; i++)
                {
                    //Column

                    axonCounter = 0;

                    var item = columns[i];

                    int x = Convert.ToInt32(item.Attributes[0]?.Value);
                    var y = Convert.ToInt32(item.Attributes[1]?.Value);

                    if (x == 0 && y == 1)
                    {
                        int breakpoint = 0;
                    }

                    try
                    {
                        Columns[x, y].Init++;
                    }
                    catch (Exception e)
                    {
                        throw;
                        int breakpoint = 1;
                    }

                    foreach (XmlNode node in item.ChildNodes)
                    {   //Neuron                   

                        if (node?.Attributes == null)
                        {
                            continue;
                        }

                        if (node.Attributes.Count != 3)
                        {
                            throw new InvalidOperationException("Invalid Neuron Id Supplied in Schema");
                        }

                        int a = Convert.ToInt32(node.Attributes[0]?.Value);
                        int b = Convert.ToInt32(node.Attributes[1]?.Value);
                        int c = Convert.ToInt32(node.Attributes[2]?.Value);

                        //Console.WriteLine("Dendritic A :" + a.ToString() + " B: " + b.ToString() + " C :" + c.ToString());

                        var proximalNodes = node.ChildNodes;

                        var neuronNodes = proximalNodes.Item(0)
                            .SelectNodes("Neuron");

                        if (neuronNodes.Count != 4)
                        {
                            throw new InvalidOperationException("Invalid Number of Neuronal Connections defined for Neuron" + a.ToString() + b.ToString() + c.ToString());
                        }

                        //4 -> 2 Proximal Dendronal Connections
                        int numDendriticConnectionCount = 0;

                        foreach (XmlNode neuron in neuronNodes)
                        {
                            //ProximalConnection
                            if (neuron?.Attributes?.Count != 3)
                            {
                                throw new InvalidOperationException("Number of Attributes in Neuronal Node is not 3");
                            }

                            int e = Convert.ToInt32(neuron.Attributes[0].Value);
                            int f = Convert.ToInt32(neuron.Attributes[1].Value);
                            int g = Convert.ToInt32(neuron.Attributes[2].Value);

                            //Money Shot!!!
                            InitDendriticConnectionForConnector(a, b, c, e, f, g);

                            numDendriticConnectionCount++;

                            if (numDendriticConnectionCount == 2)
                                break;

                        }

                        string key = a.ToString() + "-" + b.ToString() + "-" + c.ToString();

                        axonCounter++;
                    }
                }
            }

            #endregion
        }

        public void ReadAxonalSchema(int intX, int intY)
        {
            #region Cache : Cache Code
            //if (AxonalCache.Count != 0)
            //{
            //    foreach (var item in AxonalCache)
            //    {
            //        var parts = item.Key.Split('-');

            //        if (parts.Length != 3 && parts[0] != null && parts[1] != null && parts[2] != null)
            //        {
            //            throw new Exception();
            //        }
            //        int i = Convert.ToInt32(parts[0]);
            //        int j = Convert.ToInt32(parts[1]);
            //        int k = Convert.ToInt32(parts[2]);

            //        int offset = 3;

            //        //4 Axonaal Connections
            //        InitAxonalConnectionForConnector(i, j, k, item.Value[0], item.Value[1], item.Value[2]);
            //        InitAxonalConnectionForConnector(i, j, k, item.Value[0 + offset * 1], item.Value[1 + offset * 1], item.Value[2 + offset * 1]);
            //        InitAxonalConnectionForConnector(i, j, k, item.Value[0 + offset * 2], item.Value[1 + offset * 2], item.Value[2 + offset * 2]);
            //        InitAxonalConnectionForConnector(i, j, k, item.Value[0 + offset * 3], item.Value[1 + offset * 3], item.Value[2 + offset * 3]);

            //        //Console.WriteLine("SOM :: ReadAxonalSchema : Loading connection From Cache : " + i + j + k);

            //    }

            //    return;
            //}

            #endregion

            XmlDocument document = new XmlDocument();
            string axonalDocumentPath;

            if (devbox)
            {
                axonalDocumentPath = "C:\\Users\\depint\\Desktop\\Hentul\\FirstOrderMemory\\Schema Docs\\AxonalSchema.xml";
            }
            else
            {
                axonalDocumentPath = "C:\\Users\\depint\\source\\repos\\Hentul\\FirstOrderMemory\\Schema Docs\\AxonalSchema.xml";
            }

            if (!File.Exists(axonalDocumentPath))
            {
                throw new FileNotFoundException(axonalDocumentPath);
            }


            document.Load(axonalDocumentPath);

            XmlNodeList columns = document.GetElementsByTagName("axonalConnection");

            for (int icount = 0; icount < columns.Count; icount++)
            {//axonalConnection

                XmlNode connection = columns[icount];

                if (connection.Attributes.Count != 3)
                {
                    throw new InvalidDataException();

                }

                int x = Convert.ToInt32(connection.Attributes[0].Value);
                int y = Convert.ToInt32(connection.Attributes[1].Value);
                int z = Convert.ToInt32(connection.Attributes[2].Value);

                //Console.WriteLine("Axonal X :" + x.ToString() + " Y: " + y.ToString() + " Z :" + z.ToString());

                XmlNodeList axonList = connection.ChildNodes;

                //4 -> 2 Proximal Dendronal Connections
                int numAxonalConnectionCount = 0;

                foreach (XmlNode axon in axonList)
                {
                    if (axon.Attributes.Count != 3)
                    {
                        throw new InvalidDataException();
                    }

                    try
                    {
                        int i = Convert.ToInt32(axon.Attributes[0].Value);
                        int j = Convert.ToInt32(axon.Attributes[1].Value);
                        int k = Convert.ToInt32(axon.Attributes[2].Value);

                        if (x == 5 && y == 7 && z == 5)
                        {
                            int breakpoint = 1;
                        }

                        InitAxonalConnectionForConnector(x, y, z, i, j, k);

                        numAxonalConnectionCount++;

                        if (numAxonalConnectionCount == 2)
                            break;

                        //Console.WriteLine("New Connection From Schema Doc", x, y, z, i, j, k);
                    }
                    catch (Exception e)
                    {
                        int bp = 1;
                        throw new InvalidDataException("ReadAXonalSchema : Invalid Data , Tryign to add new Connections to cache");
                    }

                    axonCounter++;
                }
            }
        }

        #endregion


        public enum BlockCycle
        {
            INITIALIZATION,
            DEPOLARIZATION,
            POLOARIZED,
            FIRING,
            CLEANUP
        }
    }
}
