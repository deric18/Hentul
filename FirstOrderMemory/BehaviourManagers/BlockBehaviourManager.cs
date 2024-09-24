namespace FirstOrderMemory.BehaviourManagers
{
    using FirstOrderMemory.Models;
    using Common;
    using System.Xml;

    public class BlockBehaviourManager
    {
        #region VARIABLES
        public LayerType Layer { get; private set; }

        public ulong CycleNum { get; private set; }

        public int X { get; private set; }

        public int Y { get; private set; }

        public int Z { get; private set; }

        public Position BlockId { get; private set; }

        public Position UnitId { get; private set; }

        public int BBMID { get; private set; }

        public Dictionary<string, List<Neuron>> PredictedNeuronsForNextCycle { get; private set; }

        public Dictionary<string, List<Neuron>> PredictedNeuronsforThisCycle { get; private set; }

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

        private SchemaType schemToLoad;

        public LogMode Mode { get; private set; }

        public ulong[] WireCasesTracker { get; private set; }

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
        private const uint PRUNE_THRESHOLD = 25;
        private const uint PRUNE_STRENGTH = 1;

        #endregion

        #region CONSTRUCTORS & INITIALIZATIONS 

        public BlockBehaviourManager(int x, int y = 10, int Z = 4, LayerType layertype = LayerType.UNKNOWN, LogMode mode = LogMode.BurstOnly)
        {

            this.NumberOfColumnsThatFiredThisCycle = 0;

            CycleNum = 0;

            totalDendronalConnections = 0;

            X = x;

            Y = y;

            this.Z = Z;

            PreviousBlockCycle = BlockCycle.INITIALIZATION;

            this.CurrentCycleState = BlockCycle.INITIALIZATION;

            IsCurrentApical = false;

            IsCurrentApical = false;

            PredictedNeuronsforThisCycle = new Dictionary<string, List<Neuron>>();

            PredictedNeuronsForNextCycle = new Dictionary<string, List<Neuron>>();

            NeuronsFiringThisCycle = new List<Neuron>();

            NeuronsFiringLastCycle = new List<Neuron>();

            temporalContributors = new List<Neuron>();

            apicalContributors = new List<Neuron>();

            TemporalLineArray = new Neuron[Y, Z];

            ApicalLineArray = new Neuron[X, Y];

            Columns = new Column[X, Y];

            ColumnsThatBurst = new List<Position_SOM>();

            TemporalCycleCache = new Dictionary<ulong, List<Position_SOM>>();

            ApicalCycleCache = new Dictionary<ulong, List<Position_SOM>>();

            BurstCache = new Dictionary<ulong, List<Position_SOM>>();

            totalProximalConnections = 0;

            totalAxonalConnections = 0;

            axonCounter = 0;

            num_continuous_burst = 0;

            Mode = mode;

            schemToLoad = SchemaType.INVALID;

            WireCasesTracker = new ulong[5];

            this.Layer = layertype;
        }

        public void Init(int blockid_x, int blockId_Y, int UnitId_x, int UnitID_y, int bbmID)
        {
            BlockId = new Position(blockid_x, blockId_Y);

            UnitId = new Position(UnitId_x, UnitID_y);

            BBMID = bbmID;

            for (int i = 0; i < X; i++)
            {
                for (int j = 0; j < Y; j++)
                {
                    try
                    {
                        Columns[i, j] = new Column(i, j, Z, BlockId, UnitId, BBMID);
                    }
                    catch (Exception ex)
                    {
                        int breakpoint = 0;
                    }
                }
            }

            ReadDendriticSchema();

            ReadAxonalSchema();

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

        public bool InitDendriticConnectionForConnector(int x, int y, int z, int i, int j, int k)
        {
            bool toRet = false;

            if (x == i && y == j && z == k || (z > Z))
            {
                int breakpoint = 1;
            }
            try
            {
                Column col = this.Columns[x, y];

                Neuron neuron = col.Neurons[z];

                toRet = neuron.InitProximalConnectionForDendriticConnection(i, j, k);

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
                toRet = false;
                Console.WriteLine(ex.ToString());

                throw;

                int breakpoint = 1;
            }

            return toRet;
        }

        public BlockBehaviourManager CloneBBM(int x)
        {
            BlockBehaviourManager blockBehaviourManager;

            blockBehaviourManager = new BlockBehaviourManager(X, Y, Z);

            blockBehaviourManager.Init(0, 1, 0, 1, 99);

            blockBehaviourManager.Init(blockBehaviourManager.BlockId.X, blockBehaviourManager.BlockId.Y, blockBehaviourManager.UnitId.X, blockBehaviourManager.UnitId.Y, blockBehaviourManager.BBMID);

            return blockBehaviourManager;

            #region Cache Code for Connector

            try
            {
                for (int i = 0; i < blockBehaviourManager.Y; i++)
                {
                    for (int j = 0; j < blockBehaviourManager.Y; j++)
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
                                        postSynapticNeuron = blockBehaviourManager.GetNeuronFromString(synapse.DendronalNeuronalId);

                                        if (!blockBehaviourManager.ConnectTwoNeurons(presynapticNeuron, postSynapticNeuron, ConnectionType.PROXIMALDENDRITICNEURON))
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
                                        presynapticNeuron = blockBehaviourManager.GetNeuronFromString(synapse.AxonalNeuronId);
                                        postSynapticNeuron = blockBehaviourManager.GetNeuronFromString(synapse.DendronalNeuronalId);

                                        if (!blockBehaviourManager.ConnectTwoNeurons(presynapticNeuron, postSynapticNeuron, ConnectionType.AXONTONEURON))
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

            backupDirectory = "C:\\Users\\depint\\source\\repos\\Hentul\\Hentul\\BackUp\\";

            filename = filename + "-DendriticSchema.xml";


            foreach (var column in Columns)
            {
                foreach (var neuron in column.Neurons)
                {
                    var distalList = neuron.ProximoDistalDendriticList.Where(conList => conList.Value.cType.Equals(ConnectionType.DISTALDENDRITICNEURON)).ToList();

                    if (distalList.Count > 2)
                    {
                        foreach (var distalSynapse in distalList)
                        {

                            var distalNode = xmlDocument.CreateNode(XmlNodeType.Element, "DistalDendriticConnection", string.Empty);

                            var blockIdElement = xmlDocument.CreateElement("BlockID", string.Empty);
                            blockIdElement.InnerText = BlockId.X.ToString() + BlockId.Y.ToString();

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

        public void RestoreFromBackUp(string filename)
        {
            //throw new NotImplementedException();
        }

        #endregion

        #region FIRE & WIRE        

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

            ValidateInput(incomingPattern);

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
                                if (Mode == LogMode.BurstOnly || Mode == LogMode.All)
                                    Console.WriteLine("BURST :: " + schemToLoad.ToString() + (schemToLoad.Equals(SchemaType.FOMSCHEMA) ? " Block ID : " + PrintBlockDetailsSingleLine() : " SOM Block ") + " Bursting for incoming pattern X :" + incomingPattern.ActiveBits[i].X + " Y : " + incomingPattern.ActiveBits[i].Y);

                                AddNeuronListToNeuronsFiringThisCycleList(Columns[incomingPattern.ActiveBits[i].X, incomingPattern.ActiveBits[i].Y].Neurons);

                                ColumnsThatBurst.Add(incomingPattern.ActiveBits[i]);

                                IsBurstOnly = true;

                                num_continuous_burst++;

                                TotalBurstFire++;
                            }
                            else if (predictedNeuronPositions.Count == 1)
                            {
                                if (Mode == LogMode.All || Mode == LogMode.Info)
                                    Console.WriteLine("INFO :: Block ID : " + PrintBlockDetailsSingleLine() + " Old  Pattern : Predicting Predicted Neurons Count : " + predictedNeuronPositions.Count.ToString());

                                AddNeuronListToNeuronsFiringThisCycleList(predictedNeuronPositions);

                                TotalPredictionFires++;
                            }
                            else
                            {
                                Console.WriteLine(schemToLoad.ToString() + "There Should only be one winner in the Column");
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
            {
                if (BurstCache.Count == 0)
                    BurstCache.Add(CycleNum, incomingPattern.ActiveBits);
                else
                {
                    Console.WriteLine("ERROR : Fire :: BurstCache was not cleaned up from last cycle ." + PrintBlockDetailsSingleLine());
                    Thread.Sleep(2000);
                }

            }

            Fire();

            if (CurrentCycleState == BlockCycle.FIRING || IsCurrentApical || IsCurrentTemporal)
            {
                Wire();
            }

            PrepNetworkForNextCycle();

            if (ignorePostCycleCleanUp == false)
                PostCycleCleanup();


            ValidateNetwork();
        }

        public void LTP(SDR_SOM feedbackSignal)
        {

            /* FUNCTION : Recognise new connections made while recognising the new objects which did not exist before.
			 * Q's :
			 * What connections should be specifically strengthened ? and to what level should each connection be strengthened ?
			 * Apical Connections will only be strengthend , Should Temporal location signals should also be LTP'd ? No.
			 * 
            ALGO :: 
            1. Maintain a Delta of all the new connections that has happened for the brief period of time which truly lead to the discovery of the new object.
            2. If LTP is called that means this new batch was effective in recognising the object so strengthen these new connections.

            IMPLEMENTATION :
            1. Run through the SDR and wire up the incoming apical connection with the respective neuron.
            2. Both Apical SDR corresponds to the existing neuronal structure , No transformations needed.
            3.  
            4. Need UT, CTs, & SVTs  for verifyign these connections are true and correct

            */

            throw new NotImplementedException();

        }

        private void Fire()
        {

            foreach (var neuron in NeuronsFiringThisCycle)
            {
                //check if the synapse is active only then fire

                //Console.WriteLine(" Firing Neuron : " + neuron.NeuronID.ToString());

                neuron.Fire();

                foreach (Synapse synapse in neuron.AxonalList.Values)
                {
                    ProcessSpikeFromNeuron(GetNeuronFromString(synapse.AxonalNeuronId), GetNeuronFromString(synapse.DendronalNeuronalId), synapse.cType);
                }
            }
        }

        private void PostCycleCleanup()
        {


            //Todo : Need Selective Clean Up Logic , Should never perform Full Clean up.


            foreach (var neuronList in PredictedNeuronsforThisCycle)
            {

                var neuron = GetNeuronFromString(neuronList.Key);

                if (neuron.Voltage >= 250 && NeuronsFiringThisCycle.Contains(neuron) == false)
                {
                    Console.WriteLine(" ERROR :: " + Layer.ToString() + " BLOCK ID : " + BlockId.ToString() + " Neuron ID : " + neuron.ToString() + "  has a Higher Voltage than actual firing Voltage but did not get picked up for firing  ");
                    Thread.Sleep(3000);
                }
            }

            //Case 1 : If temporal or Apical or both lines have deplolarized and spatial fired then clean up temporal or apical or both.
            if (PreviousBlockCycle.Equals(BlockCycle.DEPOLARIZATION) && CurrentCycleState.Equals(BlockCycle.FIRING))
            {
                if (TemporalCycleCache.Count == 1)
                {
                    foreach (var kvp in TemporalCycleCache)
                    {
                        if (CycleNum - kvp.Key > 3)
                        {
                            Console.WriteLine("ERROR :: PostCycleCleanUp :: Temporal Cached Pattern is older than Spatial Pattern!" + PrintBlockDetailsSingleLine());
                            Thread.Sleep(2000);
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
                                    var neuronToCleanUp = GetNeuronFromString(synapse.DendronalNeuronalId);

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
                    Console.WriteLine("ERROR :: PostCycleCleanUp() :: TemporalCycle Cache count is more than 1 , It should always be 1" + PrintBlockDetailsSingleLine());

                    throw new InvalidOperationException("TemporalCycle Cache Size should always be 1");
                }

                if (ApicalCycleCache.Count == 1)
                {
                    foreach (var kvp in ApicalCycleCache)
                    {
                        if (CycleNum - kvp.Key > 3)
                        {
                            Console.WriteLine("ERROR :: PostCycleCleanUp :: Temporal Cached Pattern is older than Spatial Pattern! " + PrintBlockDetailsSingleLine());
                        }

                        foreach (var pos in kvp.Value)
                        {
                            foreach (var synapse in ApicalLineArray[pos.X, pos.Y].AxonalList.Values)
                            {
                                if (synapse.DendronalNeuronalId != null) // && BBMUtils.CheckNeuronListHasThisNeuron(NeuronsFiringThisCycle, synapse.DendronalNeuronalId) == false)
                                {
                                    var neuronToCleanUp = GetNeuronFromString(synapse.DendronalNeuronalId);

                                    if (neuronToCleanUp.Voltage != 0)
                                    {
                                        neuronToCleanUp.FlushVoltage();
                                    }
                                    else
                                    {
                                        Console.WriteLine("WARNING :: PostCycleCleanUp ::: Tried to clean up a neuron which was not depolarized!!! " + PrintBlockDetailsSingleLine());
                                    }
                                }
                            }
                        }
                    }

                    ApicalCycleCache.Clear();
                }
                else if (ApicalCycleCache.Count > 1)
                {
                    Console.WriteLine("ERROR :: PostCycleCleanUp() :: TemporalCycle Cache count is more than 1 , It should always be 1 " + PrintBlockDetailsSingleLine());
                    throw new InvalidOperationException("TemporalCycle Cache Size should always be 1");
                }

                PreviousBlockCycle = BlockCycle.POLOARIZED;
            }


            //Case 2 : If a bursting signal came through , after wire , the Bursted neurons and all its connected cells should be cleaned up. Feature : How many Burst Cycle to wait before performing a full clean ? Answer : 1
            if (IsBurstOnly)
            {
                foreach (var kvp in BurstCache)
                {
                    if (CycleNum - kvp.Key > 3)
                    {
                        Console.WriteLine("ERROR :: PostCycleCleanUp :: Temporal Cached Pattern is older than Spatial Pattern! " + PrintBlockDetailsSingleLine());
                    }

                    foreach (var pos in kvp.Value)
                    {
                        for (int i = 0; i < Z; i++)
                        {
                            var neuronToCleanUp = Columns[pos.X, pos.Y].Neurons[i];

                            if (neuronToCleanUp.Voltage != 0)
                            {
                                neuronToCleanUp.FlushVoltage();
                            }
                            else
                            {
                                //Console.WriteLine("WARNING :: PostCycleCleanUp ::: CASE 3 [Burwst Clean up] :::: Tried to clean up a neuron which was not depolarized!!!");
                            }
                        }
                    }
                }

                BurstCache.Clear();
            }


            //Case 3: If NeuronFiringThicCycle has any Temporal / Apical Firing Neurons they should be cleaned up [Thought : The neurons have already fired and Wired , Keepingg them in the list will only conplicate next cycle process            
            NeuronsFiringThisCycle.Clear();

            ColumnsThatBurst.Clear();

            //Every 50 Cycles Prune unused and under Firing Connections
            if (CycleNum >= 1000 && CycleNum % 500 == 0)
            {
                Prune();

                TotalBurstFire = 0;
                TotalPredictionFires = 0;
            }

            IsBurstOnly = false;

            CycleNum++;
            // Process Next pattern.          
        }

        private void Wire()
        {
            //Todo : Provide an enum for the wiring stratergy picked and simplify the below logic to a switch statement
            
            if (CurrentCycleState.Equals(BlockCycle.FIRING))
            {

                CurrentCycleState = BlockCycle.FIRING;

                List<Neuron> predictedNeuronList = new List<Neuron>();

                foreach (var item in PredictedNeuronsforThisCycle.Keys)
                {
                    var neuronToAdd = GetNeuronFromString(item);

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

                    WireCasesTracker[0]++;

                    if (Mode == LogMode.All || Mode == LogMode.Info)
                        Console.WriteLine(" EVENT :: Wire CASE 3 just Occured Count : " + WireCasesTracker[0].ToString());

                    List<Neuron> contributingList;

                    foreach (var correctlyPredictedNeuron in correctPredictionList)
                    {
                        contributingList = new List<Neuron>();

                        if (PredictedNeuronsforThisCycle.TryGetValue(correctlyPredictedNeuron.NeuronID.ToString(), out contributingList))
                        {
                            if (contributingList?.Count == 0)
                            {
                                throw new Exception("Wire : This should never Happen!! Contributing List should not be empty");
                            }

                            foreach (var contributingNeuron in contributingList)
                            {
                                //Position.ConvertStringPosToNeuron(contributingNeuron).PramoteCorrectPredictionAxonal(correctlyPredictedNeuron);

                                PramoteCorrectlyPredictedDendronal(contributingNeuron, correctlyPredictedNeuron);
                            }
                        }
                    }
                }
                else if (ColumnsThatBurst.Count != 0 && correctPredictionList.Count != 0)
                {
                    //Case 2 :  Few Correctly Fired, Few Bursted  : Strengthen the Correctly Fired Neurons
                    //          For Correctly Predicted : Pramote Correctly Predicted Synapses. 
                    //          For Bursted             : Analyse did anybody contribute to the column and dint burst ? if nobody contributed then do Wire 1 Distal Synapses with all the neurons that fired last cycle                   

                    //Boost the few correctly predicted neurons

                    WireCasesTracker[1]++;

                    if (Mode == LogMode.All || Mode == LogMode.Info)
                        Console.WriteLine(" EVENT :: Wire CASE 2 just Occured Count : " + WireCasesTracker[1].ToString());

                    List<Neuron> contributingList;

                    foreach (var correctlyPredictedNeuron in correctPredictionList)
                    {
                        //if correctlyPredictedNeuron is also present in ColumnsThatBurst then remove it from the list ColumnsThatBurst , leads to double Wiring!!!
                        int index = BBMUtils.CheckIfPositionListHasThisPosition(ColumnsThatBurst, correctlyPredictedNeuron.NeuronID);

                        if (index != -1 && index < ColumnsThatBurst.Count)
                        {
                            ColumnsThatBurst.RemoveAt(index);
                        }

                        contributingList = new List<Neuron>();

                        if (PredictedNeuronsforThisCycle.TryGetValue(correctlyPredictedNeuron.NeuronID.ToString(), out contributingList))
                        {
                            if (contributingList?.Count == 0)
                            {
                                throw new Exception("Wire : This should never Happen!! Contributing List should not be empty");
                            }

                            foreach (var contributingNeuron in contributingList)
                            {
                                //Position.ConvertStringPosToNeuron(contributingNeuron).PramoteCorrectPredictionAxonal(correctlyPredictedNeuron);

                                PramoteCorrectlyPredictedDendronal(contributingNeuron, correctlyPredictedNeuron);
                            }
                        }
                    }

                    //Bug : Boosting should not juice the same neurons!

                    //Todo : Need to revisti this stratergy of connecting all the boosted neurons.

                    //Boost the Bursting neurons
                    foreach (var position in ColumnsThatBurst)
                    {
                        foreach (var dendriticNeuron in Columns[position.X, position.Y].Neurons)
                        {
                            foreach (var axonalNeuron in NeuronsFiringLastCycle)
                            {
                                if (CheckifBothNeuronsAreSameOrintheSameColumn(dendriticNeuron, axonalNeuron) == false && BBMUtils.CheckIfTwoNeuronsAreConnected(axonalNeuron, dendriticNeuron) == false)
                                {
                                    ConnectTwoNeurons(axonalNeuron, dendriticNeuron, ConnectionType.DISTALDENDRITICNEURON);
                                }
                            }
                        }
                    }

                }// ColumnsThatBurst.Count == 0 && correctPredictionList.Count = 5 &&  NumberOfColumnsThatFiredThisCycle = 8  cycleNum = 4 , repNum = 29
                else if (ColumnsThatBurst.Count == 0 && NumberOfColumnsThatFiredThisCycle > correctPredictionList.Count)
                {
                    // Case 3 : None Bursted , Some Fired which were NOT predicted , Some fired which were predicted
                    // Strengthen the ones which fired correctly 
                    List<Neuron> contributingList;

                    WireCasesTracker[2]++;

                    Console.WriteLine(" EVENT :: PARTIAL ERROR CASE : Wire CASE 3 just Occured Count : " + WireCasesTracker[2].ToString());
                    Thread.Sleep(1000);

                    foreach (var correctlyPredictedNeuron in correctPredictionList)
                    {
                        contributingList = new List<Neuron>();

                        if (PredictedNeuronsforThisCycle.TryGetValue(correctlyPredictedNeuron.NeuronID.ToString(), out contributingList))
                        {
                            if (contributingList?.Count == 0)
                            {
                                throw new Exception("Wire : This should never Happen!! Contributing List should not be empty");
                            }

                            foreach (var contributingNeuron in contributingList)
                            {
                                //fPosition.ConvertStringPosToNeuron(contributingNeuron).PramoteCorrectPredictionAxonal(correctlyPredictedNeuron);

                                PramoteCorrectlyPredictedDendronal(contributingNeuron, correctlyPredictedNeuron);
                            }
                        }
                    }

                    // Fired But Did Not Get Predicted
                    foreach (var axonalneuron in NeuronsFiringLastCycle)
                    {
                        foreach (var dendronalNeuron in NeuronsFiringThisCycle)
                        {
                            if (CheckifBothNeuronsAreSameOrintheSameColumn(axonalneuron, dendronalNeuron) == false)
                                ConnectTwoNeurons(axonalneuron, dendronalNeuron, ConnectionType.DISTALDENDRITICNEURON);
                        }
                    }
                }
                else if (ColumnsThatBurst.Count == NumberOfColumnsThatFiredThisCycle && correctPredictionList.Count == 0)
                {
                    //Case 4 : All columns Bursted: highly likely first fire or totally new pattern coming in :
                    //         If firing early cycle, then just wire 1 Distal Syanpse to all the neurons that fired last cycle and 1 random connection.
                    //         Todo : If in the middle of the cycle(atleast 10,000 cycles ) then Need to do something new.


                    //BUG 1: NeuronsFiredLastCycle = 10 when last cycle was a Burst Cycle and if this cycle is a Burst cycle then the NeuronsFiringThisCycle will be 10 as well , that leads to 100 new distal connections , not healthy.
                    //Feature : Synapses will be marked InActive on Creation and eventually marked Active once the PredictiveCount increases.
                    //BUG 2: TotalNumberOfDistalConnections always get limited to 400
                    //
                    WireCasesTracker[3]++;

                    Console.WriteLine(" EVENT :: FULL ERROR CASE :: Wire CASE 4 just Occured Count : " + WireCasesTracker[3].ToString());

                    foreach (var position in ColumnsThatBurst)
                    {
                        foreach (var dendriticNeuron in Columns[position.X, position.Y].Neurons)
                        {
                            foreach (var axonalNeuron in NeuronsFiringLastCycle)
                            {
                                if (CheckifBothNeuronsAreSameOrintheSameColumn(axonalNeuron, dendriticNeuron) == false)
                                    ConnectTwoNeurons(axonalNeuron, dendriticNeuron, ConnectionType.DISTALDENDRITICNEURON);
                            }
                        }
                    }
                }
                else if (ColumnsThatBurst.Count < NumberOfColumnsThatFiredThisCycle && correctPredictionList.Count == 0)
                {

                    //Case 5 : None of the predicted Neurons did Fire and some also did BURST and some of them did Fire that were not added to the prediction list.
                    //          For Bursted             : Analyse did anybody contribute to the column and dint burst ? if nobody contributed then do Wire 1 Distal Synapses with all the neurons that fired last cycle                   
                    //          For Fired               : The Fired Neurons did not burst because some neurons deplolarized it in the last cycle , connect to all the neurons that contributed to its Firing.

                    //Bug : Somehow all the neurons in the column have the same voltage , but none of them are added to the PredictedNeuronsForThisCycle List from last firing Cycle.

                    WireCasesTracker[4]++;

                    Console.WriteLine(" EVENT :: Wire CASE 5 just Occured Count : " + WireCasesTracker[4].ToString());
                    Thread.Sleep(1000);

                    List<Neuron> burstList = new List<Neuron>();

                    foreach (var item in ColumnsThatBurst)
                    {
                        burstList.AddRange(Columns[item.X, item.Y].Neurons);
                    }

                    //Throw if any of the neurons that fired last cycle even though connected did not deploarize
                    if (CheckIfNeuronGetsAnyContributionsLastCycle(burstList))
                    {
                        foreach (var neuron in burstList)
                        {
                            foreach (var lastcycleneuron in NeuronsFiringLastCycle)
                            {
                                if (neuron.Equals(lastcycleneuron) && BBMUtils.CheckIfTwoNeuronsAreConnected(lastcycleneuron, neuron))
                                {
                                    Console.WriteLine("Exception : Wire() :: Neurons That Fire Together are not Wiring Together" + PrintBlockDetailsSingleLine());

                                    PramoteCorrectlyPredictedDendronal(lastcycleneuron, neuron);
                                }
                            }
                        }
                    }

                    //Boost All the Bursting Neurons
                    foreach (var position in ColumnsThatBurst)
                    {
                        foreach (var dendriticNeuron in Columns[position.X, position.Y].Neurons)
                        {
                            foreach (var axonalNeuron in NeuronsFiringLastCycle)
                            {
                                if (CheckifBothNeuronsAreSameOrintheSameColumn(axonalNeuron, dendriticNeuron) == false)
                                    ConnectTwoNeurons(axonalNeuron, dendriticNeuron, ConnectionType.DISTALDENDRITICNEURON);
                            }
                        }
                    }

                    //Boost the Non Bursting Neurons

                    var firingNeurons = AntiUniounWithNeuronsFiringThisCycle(burstList);

                    if (firingNeurons.Count != 0)
                    {
                        foreach (var dendriticNeuron in firingNeurons)
                        {
                            foreach (var axonalNeuron in NeuronsFiringLastCycle)
                            {
                                if (CheckifBothNeuronsAreSameOrintheSameColumn(axonalNeuron, dendriticNeuron) == false)
                                    ConnectTwoNeurons(axonalNeuron, dendriticNeuron, ConnectionType.DISTALDENDRITICNEURON);
                            }
                        }
                    }

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
                                PramoteCorrectlyPredictedDendronal(temporalContributor, neuron);
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
                            PramoteCorrectlyPredictedDendronal(apicalContributor, neuron);
                        }
                    }
                }
            }
        }

        #region INTERNAL METHODS

        public string PrintBlockDetailsSingleLine()
        {
            return " Unit ID X :" + UnitId.X + " Unit ID Y : " + UnitId.Y + "/ BBM ID : " + BBMID.ToString();
        }

        public bool AddPredictedNeuronForNextCycle(Neuron predictedNeuron, Neuron contributingNeuron)
        {
            List<Neuron> contributingList = new List<Neuron>();

            //If bursting then 
            //if (predictedNeuron.NeuronID.X == 5 && predictedNeuron.NeuronID.Y == 5 && predictedNeuron.NeuronID.Z == 3)
            //{
            //    int breakpoint = 1;
            //}

            //Need to check if the synapse is active and only then add it to the list            
            if (PredictedNeuronsForNextCycle.Count > 0 && PredictedNeuronsForNextCycle.TryGetValue(predictedNeuron.NeuronID.ToString(), out contributingList))
            {
                contributingList.Add(contributingNeuron);
            }
            else
            {
                PredictedNeuronsForNextCycle.Add(predictedNeuron.NeuronID.ToString(), new List<Neuron>() { contributingNeuron });
            }

            return true;
        }

        public bool ConnectTwoNeurons(Neuron AxonalNeuron, Neuron DendriticNeuron, ConnectionType cType)
        {
            // Make sure while connecting two neurons we enver connect 2 neurons from the same column to each other , this might result in a fire loop.
            //if (cType == ConnectionType.DISTALDENDRITICNEURON)
            //{
            //    if(DendriticNeuron.NeuronID.X == 2 && DendriticNeuron.NeuronID.Y == 8 && DendriticNeuron.NeuronID.Z == 5 && AxonalNeuron.NeuronID.X == 5 && AxonalNeuron.NeuronID.Y == 1 && AxonalNeuron.NeuronID.Z == 4)
            //    {
            //        bool breakpoint = false;
            //        breakpoint = true;
            //    }
            //}

            if (AxonalNeuron == null || DendriticNeuron == null)
            {
                return false;
            }
            else if (CurrentCycleState != BlockCycle.INITIALIZATION && (AxonalNeuron.nType.Equals(NeuronType.TEMPORAL) || AxonalNeuron.nType.Equals(NeuronType.APICAL)))
            {
                // Post Init Temporal / Apical Neurons should not connect with anybody else.
                throw new InvalidOperationException("ConnectTwoNeuronsOrIncrementStrength :: Temporal Neurons cannot connect to Normal Neurons Post Init!");
            }
            else if (AxonalNeuron.NeuronID.X == DendriticNeuron.NeuronID.X && AxonalNeuron.NeuronID.Y == DendriticNeuron.NeuronID.Y && AxonalNeuron.nType.Equals(DendriticNeuron.nType))
            {
                Console.WriteLine("Error :: ConnectTwoNeurons :: Cannot Connect Neuron to itself! Block Id : " + PrintBlockDetailsSingleLine() + " Neuron ID : " + AxonalNeuron.NeuronID.ToString());     // No Same Column Connections 
                Thread.Sleep(2000);                                                                                                                                                      //throw new InvalidDataException("ConnectTwoNeurons: Cannot connect Neuron to Itself!");
                return false;
            }
            else if (AxonalNeuron.nType.Equals(DendriticNeuron.nType) && AxonalNeuron.NeuronID.Equals(DendriticNeuron.NeuronID))                                                      // No Selfing               
            {
                Console.WriteLine("ConnectTwoNeurons() :::: ERROR :: Cannot connect neurons in the same Column [NO SELFING] " + PrintBlockDetailsSingleLine());
                Thread.Sleep(2000);
                return false;
            }

            if ((DendriticNeuron.ProximoDistalDendriticList.Count >= 400 && schemToLoad == SchemaType.FOMSCHEMA) || (DendriticNeuron.ProximoDistalDendriticList.Count >= 1400 && schemToLoad == SchemaType.SOMSCHEMA))
            {
                Console.WriteLine(" EVENT :: ConnectTwoNeurons :::: Neuron inelgible to be have any more Connections ! Auto Selected for Pruning Process " + PrintBlockDetailsSingleLine());

                PruneSingleNeuron(DendriticNeuron);

                if ((DendriticNeuron.ProximoDistalDendriticList.Count >= 400 && schemToLoad == SchemaType.FOMSCHEMA) || (DendriticNeuron.ProximoDistalDendriticList.Count >= 1400 && schemToLoad == SchemaType.SOMSCHEMA))
                {
                    Console.WriteLine("ERROR :: Neuronal Distal Dendritic Connection is not reducing even after pruning!!!");
                    Thread.Sleep(1000);
                }
            }

            //Add only Axonal Connection first to check if its not already added before adding dendronal Connection.
            bool IsAxonalConnectionSuccesful = AxonalNeuron.AddtoAxonalList(DendriticNeuron.NeuronID.ToString(), AxonalNeuron.nType, CycleNum, cType, schemToLoad);

            if (IsAxonalConnectionSuccesful)
            {
                bool IsDendronalConnectionSuccesful = DendriticNeuron.AddToDistalList(AxonalNeuron.NeuronID.ToString(), DendriticNeuron.nType, CycleNum, schemToLoad, cType);

                if (cType.Equals(ConnectionType.DISTALDENDRITICNEURON))     //New Connection Added
                {
                    TotalDistalDendriticConnections++;
                }

                if (IsDendronalConnectionSuccesful && (Mode == LogMode.All || Mode == LogMode.Info))
                {
                    Console.WriteLine("INFO :: Added new Distal Connection between tow Neurons :: A: " + AxonalNeuron.NeuronID.ToString() + " D : " + DendriticNeuron.NeuronID.ToString());
                }
                else if (IsDendronalConnectionSuccesful == false)//If dendronal connection did not succeed then the structure is compromised : Throw;
                {
                    if (AxonalNeuron.RemoveAxonalConnection(DendriticNeuron) == false)
                    {
                        Console.WriteLine(" ERROR :: Axonal Connection Succeded but Distal Connection Failed! ");
                        Thread.Sleep(5000);
                    }

                    throw new InvalidOperationException(" ERROR :: ConnectoTwoNeurons :: Axonal Connection added but unable to add Dendritic Connection for Neuron " + DendriticNeuron.ToString());
                }

                return true;
            }
            else
            {

            }

            return false;
        }

        public bool CheckifBothNeuronsAreSameOrintheSameColumn(Neuron neuron1, Neuron neuron2) =>
            neuron1.NeuronID.X == neuron2.NeuronID.X && neuron1.NeuronID.Y == neuron2.NeuronID.Y;

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

        public Neuron GetNeuronFromString(string posString)
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

            if (x > X || y > Y || z > Z)
            {
                int breakpoint = 1;

                throw new NullReferenceException("ConvertStringPosToNeuron : Couldnt Find the neuron in the columns  posString :  " + posString);
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

            //ActiveBits.Sort();

            return new SDR_SOM(X, Y, ActiveBits, iType.SPATIAL);
        }

        public SDR_SOM GetAllFiringNeuronsThisCycle()
        {
            List<Position_SOM> activeBits = new List<Position_SOM>();

            NeuronsFiringThisCycle.ForEach(n => { if (n.nType == NeuronType.NORMAL) activeBits.Add(n.NeuronID); });

            return new SDR_SOM(X, Y, activeBits, iType.SPATIAL);
        }

        #endregion

        #endregion

        #region PRIVATE METHODS

        private void LTPApicalNeurons(Neuron ApicalNeuron, Neuron NormalNeuron)
        {
            if (!NormalNeuron.GetMyApicalPartner().Equals(ApicalNeuron.NeuronID.ToString()))
            {
                throw new InvalidOperationException("Cannot Boost connectivity of a non Apical Neuron to Normal Neuron");
            }



        }

        private void ValidateNetwork()
        {

            if (PredictedNeuronsForNextCycle.Count >= (0.1 * X * Y * Z))
            {
                Console.WriteLine(schemToLoad.ToString() + " ERROR :: Too many Predictions for the size of the network ");
                Console.Read();
            }
            else if (NeuronsFiringThisCycle.Count >= (0.1 * X * Y * Z))
            {
                Console.WriteLine(schemToLoad.ToString() + " ERROR :: Too many FIRINGS for the size of the network ");
                Console.Read();
            }

            foreach (var neuroString in PredictedNeuronsforThisCycle.Keys)
            {
                var neuron = GetNeuronFromString(neuroString);

                if (BBMUtils.CheckNeuronListHasThisNeuron(NeuronsFiringThisCycle, neuron) && neuron.Voltage >= Neuron.COMMON_NEURONAL_FIRE_VOLTAGE)
                {

                    Console.WriteLine(" WARNING :: Neuron that was heavily Predicted to a level fo firing did not get Fired!!!");
                    Thread.Sleep(3000);
                }
            }
        }

        private void ValidateInput(SDR_SOM incomingPattern)
        {
            if (incomingPattern.ActiveBits.Count == 0)
            {
                Console.WriteLine("EXCEPTION :: Incoming Pattern cannot be empty");
                throw new InvalidOperationException("Incoming Pattern cannot be empty");
            }

            if (incomingPattern.InputPatternType.Equals(iType.TEMPORAL))
            {
                if (TemporalCycleCache.Count != 0)
                {
                    Console.WriteLine("ERROR :: Fire() :::: Trying to Add Temporal Pattern to a Valid cache Item! " + PrintBlockDetailsSingleLine());
                    Thread.Sleep(2000);
                }

                foreach(var input in incomingPattern.ActiveBits)
                {
                    if(input.X >= X || input.Y >= Z)
                    {
                        throw new InvalidOperationException("EXCEPTION :: Invalid Data for Temporal Pattern exceeding bounds!");
                    }
                }

                TemporalCycleCache.Add(CycleNum, TransformTemporalCoordinatesToSpatialCoordinates1(incomingPattern.ActiveBits));
            }

            if (incomingPattern.InputPatternType.Equals(iType.APICAL))
            {
                if (ApicalCycleCache.Count != 0)
                {
                    Console.WriteLine("ERROR :: Fire() :::: Trying to Add Apical Pattern to a Valid cache Item!" + PrintBlockDetailsSingleLine());
                    Thread.Sleep(2000);
                }

                ApicalCycleCache.Add(CycleNum, incomingPattern.ActiveBits);
            }

            foreach (var pos in incomingPattern.ActiveBits)
            {
                if (pos.X > X || pos.Y > Y || pos.Z > Z)
                {
                    Console.WriteLine("EXCEPTION :: Incoming pattern is not encoded in the correct format");

                    throw new InvalidDataException("Incoming SDR is not encoded correctly");
                }
            }

        }

        private void Prune()
        {

            for (int i = 0; i < X; i++)
            {
                for (int j = 0; j < Y; j++)
                {
                    foreach (var neuron in Columns[i, j].Neurons)
                    {
                        if (neuron.NeuronID.ToString() == "2-8-0-N" || neuron.NeuronID.ToString() == "5-1-7-N")
                        {
                            bool bp = true;
                        }

                        if (neuron.ProximoDistalDendriticList == null || neuron.ProximoDistalDendriticList.Count == 0)
                        { return; }

                        List<string> DremoveList = null;
                        List<string> AremoveList = null;

                        var distalDendriticList = neuron.ProximoDistalDendriticList.Values.Where(x => x.cType.Equals(ConnectionType.DISTALDENDRITICNEURON) && x.GetStrength() <= PRUNE_STRENGTH && x.PredictiveHitCount != 5);

                        if (distalDendriticList.Count() != 0)
                        {
                            foreach (var kvp in neuron.ProximoDistalDendriticList)
                            {

                                if (kvp.Value.cType == ConnectionType.DISTALDENDRITICNEURON && ((CycleNum - Math.Max(kvp.Value.lastFiredCycle, kvp.Value.lastPredictedCycle)) > PRUNE_THRESHOLD))
                                {

                                    //Remove Distal Dendrite from Neuron
                                    if (DremoveList == null)
                                    {
                                        DremoveList = new List<string>();
                                    }

                                    DremoveList.Add(kvp.Key);

                                    //Remove Corresponding Connected Axonal Neuron
                                    var axonalNeuron = GetNeuronFromString(kvp.Value.AxonalNeuronId);
                                    if (axonalNeuron.AxonalList.TryGetValue(neuron.NeuronID.ToString(), out var connection))
                                    {
                                        if (axonalNeuron.RemoveAxonalConnection(neuron) == false)
                                        {
                                            Console.WriteLine(" ERROR : Could not remove connected Axonal Neuron");
                                            throw new InvalidOperationException(" Couldnt find the prunning axonal connection on the deleted dendritic connection while Prunning");
                                        }
                                    }
                                    else
                                    {
                                        Console.WriteLine("WARNING :::: PRUNE():: Axonal Neuron does not contain Same synapse from Dendronal Neuron for Prunning!");
                                    }
                                }
                            }

                            if (DremoveList?.Count > 0)
                            {
                                neuron.IncrementPruneCount();

                                for (int k = 0; k < DremoveList.Count; k++)
                                {
                                    neuron.ProximoDistalDendriticList.Remove(DremoveList[k]);

                                    BlockBehaviourManager.totalDendronalConnections--;
                                }
                            }
                        }
                    }
                }
            }
        }

        private void PruneSingleNeuron(Neuron prunedNeuron)
        {
            if (prunedNeuron != null)
            {
                if (prunedNeuron.NeuronID.ToString() == "2-8-0-N" || prunedNeuron.NeuronID.ToString() == "5-1-7-N")
                {
                    bool bp = true;
                }

                if (prunedNeuron.ProximoDistalDendriticList == null || prunedNeuron.ProximoDistalDendriticList.Count == 0)
                { return; }

                List<string> DremoveList = null;
                List<string> AremoveList = null;

                var distalDendriticList = prunedNeuron.ProximoDistalDendriticList.Values.Where(x => x.cType.Equals(ConnectionType.DISTALDENDRITICNEURON) && x.GetStrength() <= PRUNE_STRENGTH && x.PredictiveHitCount != 5);

                //Remove only connected Distal Dendritic connections
                if (distalDendriticList.Count() != 0)
                {
                    foreach (var kvp in prunedNeuron.ProximoDistalDendriticList)
                    {

                        if (kvp.Value.cType == ConnectionType.DISTALDENDRITICNEURON && ((CycleNum - Math.Max(kvp.Value.lastFiredCycle, kvp.Value.lastPredictedCycle)) > PRUNE_THRESHOLD))
                        {

                            //Remove Distal Dendrite from Neuron
                            if (DremoveList == null)
                                DremoveList = new List<string>();

                            DremoveList.Add(kvp.Key);

                            //Remove Corresponding Connected Axonal Neuron
                            var axonalNeuron = GetNeuronFromString(kvp.Value.AxonalNeuronId);

                            if (axonalNeuron.AxonalList.TryGetValue(prunedNeuron.NeuronID.ToString(), out var connection))
                            {
                                if (axonalNeuron.RemoveAxonalConnection(prunedNeuron) == false)
                                {
                                    Console.WriteLine(" ERROR : Could not remove connected Axonal Neuron");
                                    throw new InvalidOperationException(" Couldnt find the prunning axonal connection on the deleted dendritic connection while Prunning");
                                }
                            }
                            else
                            {
                                Console.WriteLine("WARNING :::: PRUNE():: Axonal Neuron does not contain Same synapse from Dendronal Neuron for Prunning!");
                            }
                        }
                    }

                    if (Mode.Equals(LogMode.All) || Mode.Equals(LogMode.Info))
                        Console.WriteLine("INFO : Succesfully removed " + DremoveList.Count.ToString() + " neurons from neuron " + prunedNeuron.NeuronID.ToString());

                    if (DremoveList?.Count > 0)
                    {
                        for (int k = 0; k < DremoveList.Count; k++)
                        {
                            prunedNeuron.ProximoDistalDendriticList.Remove(DremoveList[k]);

                            BlockBehaviourManager.totalDendronalConnections--;
                        }
                    }
                }


                return;
            }
        }

        private void PreCyclePrep()
        {
            //Prepare all the neurons that are predicted 
            if (PredictedNeuronsForNextCycle.Count != 0 && NeuronsFiringThisCycle.Count != 0)
            {
                Console.WriteLine(schemToLoad.ToString() + "Precycle Cleanup Error : _predictedNeuronsForNextCycle is not empty");
                throw new Exception("PreCycle Cleanup Exception!!!");
            }

            if (CurrentCycleState.Equals(BlockCycle.CLEANUP))
            {
                for (int i = 0; i < X; i++)
                    for (int j = 0; j < Y; j++)
                    {
                        if (Columns[i, j].PreCleanupCheck())
                        {
                            Console.WriteLine(schemToLoad.ToString() + "Precycle Cleanup Error : Column {0} {1} is pre Firing", i, j);
                            throw new Exception("PreCycle Cleanup Exception!!!");
                        }
                    }
            }

            NumberOfColumnsThatFiredThisCycle = 0;

            IsBurstOnly = false;

        }

        private void ProcessSpikeFromNeuron(Neuron sourceNeuron, Neuron targetNeuron, ConnectionType cType = ConnectionType.PROXIMALDENDRITICNEURON)
        {

            if (sourceNeuron.NeuronID.ToString().Equals("0-1-4-N") && targetNeuron.NeuronID.ToString().Equals("3-1-1-N"))
            {
                bool breakpoint = false;
            }

            //Do not added Temporal and Apical Neurons to NeuronsFiringThisCycle, it throws off Wiring.            
            AddPredictedNeuronForNextCycle(targetNeuron, sourceNeuron);

            if (cType.Equals(ConnectionType.TEMPRORAL) || cType.Equals(ConnectionType.APICAL))
            {
                if (!targetNeuron.TAContributors.TryGetValue(sourceNeuron.NeuronID.ToString(), out char w))
                {
                    if (cType.Equals(ConnectionType.TEMPRORAL))
                    {
                        targetNeuron.TAContributors.Add(sourceNeuron.NeuronID.ToString(), 'T');
                        targetNeuron.ProcessVoltage(TEMPORAL_NEURON_FIRE_VALUE, Mode);
                    }
                    else if (cType.Equals(ConnectionType.APICAL))
                    {
                        targetNeuron.TAContributors.Add(sourceNeuron.NeuronID.ToString(), 'A');
                        targetNeuron.ProcessVoltage(APICAL_NEURONAL_FIRE_VALUE, Mode);
                    }
                }
                else
                {
                    if (cType.Equals(ConnectionType.TEMPRORAL))
                        targetNeuron.ProcessVoltage(TEMPORAL_NEURON_FIRE_VALUE, Mode);
                    else if (cType.Equals(ConnectionType.APICAL))
                        targetNeuron.ProcessVoltage(APICAL_NEURONAL_FIRE_VALUE, Mode);

                    #region Removed Code [Potential Bug OR Feature
                    //if (cType.Equals(ConnectionType.TEMPRORAL))
                    //    Console.WriteLine("ERROR :: ProcessSpikeFromNeuron :::: Temporal Line Array should be connected in both ways to there respective Temporal / Apical Neuronal Partners");
                    //else if (cType.Equals(ConnectionType.APICAL))
                    //    Console.WriteLine("ERROR :: ProcessSpikeFromNeuron :::: Apical Line Array should be connected in both ways to there respective Temporal / Apical Neuronal Partners");
                    //else
                    //    throw new InvalidOperationException("Invalid Connections");
                    #endregion
                }
            }
            else if (targetNeuron.ProximoDistalDendriticList.TryGetValue(sourceNeuron.NeuronID.ToString(), out var synapse))
            {
                if (synapse.IsActive)       //Process Voltage only if the synapse is active otherwise Increment HitCount.
                {
                    switch (synapse.cType)
                    {
                        case ConnectionType.DISTALDENDRITICNEURON:
                            targetNeuron.ProcessVoltage(DISTAL_VOLTAGE_SPIKE_VALUE, Mode);
                            break;
                        case ConnectionType.PROXIMALDENDRITICNEURON:
                            targetNeuron.ProcessVoltage(PROXIMAL_VOLTAGE_SPIKE_VALUE, Mode);
                            break;
                        case ConnectionType.NMDATONEURON:
                            targetNeuron.ProcessVoltage(NMDA_NEURONAL_FIRE_VALUE, Mode);
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
                targetNeuron.ProcessVoltage(PROXIMAL_AXON_TO_NEURON_FIRE_VALUE, Mode);
            }
            else
            {
                Console.WriteLine(schemToLoad.ToString() + "ProcessSpikeFromNeuron() :::: ERROR :: One of the Neurons is not connected to the other neuron Source : " + sourceNeuron.NeuronID + " Target Neuron : " + targetNeuron.NeuronID);
                PrintBlockDetails();
                throw new InvalidOperationException("ProcessSpikeFromNeuron : Trying to Process Spike from Neuron which is not connected to this Neuron");
            }
        }

        private void PrintBlockDetails()
        {
            Console.Write("Block ID : X : " + BlockId.X.ToString() + " Y :" + BlockId.Y.ToString());
            Console.WriteLine("Unit ID X: " + UnitId.X.ToString() + " Y :" + UnitId.Y.ToString());
            Console.WriteLine("BBM ID : " + BBMID);
        }

        private void PrepNetworkForNextCycle()
        {
            PerCycleFireSparsityPercentage = (NeuronsFiringThisCycle.Count * 100 / (X * Y * Z));

            if (PerCycleFireSparsityPercentage > 20)
            {
                Console.WriteLine(schemToLoad.ToString() + PrintBlockDetailsSingleLine() + "WARNING :: PrepNetworkForNextCycle :: PerCycleFiringSparsity is exceeding 20 %");
            }

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

            if (PredictedNeuronsForNextCycle.Count >= (0.05 * X * Y * Z))
            {
                Console.WriteLine(schemToLoad.ToString() + PrintBlockDetailsSingleLine() + "WARNING :: Total Number of Predicted Neurons should not exceed more than 10% of Network size");

                //Console.ReadKey();
            }

            PredictedNeuronsForNextCycle.Clear();
        }

        private List<Neuron> AntiUniounWithNeuronsFiringThisCycle(List<Neuron> burstList)
        {
            var firingList = new List<Neuron>();

            foreach (var neuron in NeuronsFiringThisCycle)
            {
                if (!burstList.Any(item => item.NeuronID.X == neuron.NeuronID.X && item.NeuronID.Y == neuron.NeuronID.Y && item.NeuronID.Z == neuron.NeuronID.Z))
                {
                    firingList.Add(neuron);
                }
            }

            return firingList;
        }

        private void StrengthenTemporalConnection(Neuron neuron)
        {
            PramoteCorrectlyPredictedDendronal(GetNeuronFromString(neuron.GetMyTemporalPartner()), neuron);
        }

        private void StrengthenApicalConnection(Neuron neuron)
        {
            PramoteCorrectlyPredictedDendronal(GetNeuronFromString(neuron.GetMyApicalPartner()), neuron);
        }

        public void PramoteCorrectlyPredictedDendronal(Neuron contributingNeuron, Neuron targetNeuron)
        {
            // BUG: Axonal synapses are not being incremented as the dendronal ones are.!

            if (targetNeuron.ProximoDistalDendriticList.Count == 0)
            {
                throw new Exception("Not Supposed to Happen : Trying to Pramote connection on a neuron , not connected yet!");
            }

            if (targetNeuron.ProximoDistalDendriticList.TryGetValue(contributingNeuron.NeuronID.ToString(), out Synapse synapse))
            {
                if (synapse == null)
                {
                    Console.WriteLine("ERROR :: PramoteCorrectPredictionDendronal: Trying to increment strength on a synapse object that was null!!!");
                    throw new InvalidOperationException("Not Supposed to happen!");
                }

                //Console.WriteLine("SOM :: Pramoting Correctly Predicted Dendronal Connections");

                synapse.IncrementHitCount(CycleNum);
            }
        }

        private bool CheckIfNeuronGetsAnyContributionsLastCycle(List<Neuron> neruonList)
        {
            foreach (var neuron in neruonList)
            {
                foreach (var neuronKey in neuron.ProximoDistalDendriticList.Keys)
                {
                    var distallyConnectedNeuron = GetNeuronFromString(neuronKey);

                    if (BBMUtils.CheckNeuronListHasThisNeuron(NeuronsFiringLastCycle, distallyConnectedNeuron))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public void PrintBlockStats()
        {
            // Print sparsity of the block.
            // print warning when all the bits of the neuron fired. with more than 8% sparsity
            if (Mode != LogMode.None && (TotalBurstFire > 0 || TotalPredictionFires > 0) && PerCycleFireSparsityPercentage > 0)
            {
                Console.WriteLine(PrintBlockDetailsSingleLine() + " Wins: " + TotalPredictionFires.ToString() + " Loses: " + TotalBurstFire.ToString() + "  Fire Sparsity : " + PerCycleFireSparsityPercentage.ToString() + "%" + "TOTALCYCLESCONTRIBUTED : " + CycleNum.ToString());
            }
        }

        private void GenerateTemporalLines()
        {
            // T : (x,y, z) => (0,y,x)
            // Dont make any changes without understanding this Co mpletely !! Burned hands 

            for (int i = 0; i < Y; i++)
            {
                for (int j = 0; j < Z; j++)
                {

                    if (this.TemporalLineArray[i, j] == null)
                        this.TemporalLineArray[i, j] = new Neuron(new Position_SOM(0, i, j, 'T'), BlockId, UnitId, BBMID, NeuronType.TEMPORAL);

                    for (int k = 0; k < X; k++)
                    {
                        try
                        {
                            ConnectTwoNeurons(this.TemporalLineArray[i, j], Columns[k, i].Neurons[j], ConnectionType.TEMPRORAL);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.ToString());

                            var item1 = TemporalLineArray[j, i] as Neuron;
                            var item2 = Columns[k, i].Neurons[j] as Neuron;
                        }
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
            for (int i = 0; i < X; i++)
            {
                for (int j = 0; j < Y; j++)
                {
                    this.ApicalLineArray[i, j] = new Neuron(new Position_SOM(i, j, 0, 'A'), BlockId, UnitId, BBMID, NeuronType.APICAL);

                    for (int k = 0; k < Z; k++)
                    {
                        ConnectTwoNeurons(this.ApicalLineArray[i, j], Columns[i, j].Neurons[k], ConnectionType.APICAL);
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
                temporalNeurons.Add(this.TemporalLineArray[position.X, position.Y]);
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
                temporalNeurons.Add(new Position_SOM(position.X, position.Y));
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

        private void IncrementProximalConnectionCount()
        {
            this.totalProximalConnections++;
        }

        private void IncrementAxonalConnectionCount()
        {
            this.totalAxonalConnections++;
        }

        private void ReadDendriticSchema()
        {
            #region REAL Code                       

            if ((X == 10 && Y == 10 && Z == 4))
            {
                schemToLoad = SchemaType.FOMSCHEMA;
            }
            else if (X == 1250 && Y == 10 && Z == 4)
            {
                schemToLoad = SchemaType.SOMSCHEMA;
            }

            XmlDocument document = new XmlDocument();

            string dendriteDocumentPath = null;

            if (schemToLoad == SchemaType.INVALID)
            {
                throw new InvalidOperationException("Schema Type Cannot be Invalid");
            }

            if (schemToLoad == SchemaType.FOMSCHEMA)
            {
                dendriteDocumentPath = "C:\\Users\\depint\\source\\repos\\Hentul\\FirstOrderMemory\\Schema Docs\\DendriticSchemaFOM.xml";
            }
            else if (schemToLoad == SchemaType.SOMSCHEMA)
            {
                dendriteDocumentPath = "C:\\Users\\depint\\source\\repos\\Hentul\\FirstOrderMemory\\Schema Docs\\DendriticSchemaSOM.xml";
            }

            if (File.Exists(dendriteDocumentPath) == false)
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

                        //var neuronNodes = proximalNodes.Item(0)
                        //    .SelectNodes("Neuron");

                        if (!(proximalNodes.Count == 4 || proximalNodes.Count == 2))
                        {
                            throw new InvalidOperationException("Invalid Number of Neuronal Connections defined for Neuron" + a.ToString() + b.ToString() + c.ToString());
                        }

                        //4 -> 2 Proximal Dendronal Connections
                        int numDendriticConnectionCount = 0;

                        foreach (XmlNode neuron in proximalNodes)
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
                            if (InitDendriticConnectionForConnector(a, b, c, e, f, g) == false)
                            {
                                throw new InvalidDataException("InitDendriticConnectionForConnector :: Duplicate Dendritic Coo0rdiantes");
                            }

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

        public void ReadAxonalSchema()
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

            schemToLoad = SchemaType.INVALID;

            if ((X == 10 && Y == 10 && Z == 4))
            {
                schemToLoad = SchemaType.FOMSCHEMA;
            }
            else if (X == 1250 && Y == 10 && Z == 4)
            {
                schemToLoad = SchemaType.SOMSCHEMA;
            }

            XmlDocument document = new XmlDocument();

            string axonalDocumentPath = null;

            if (schemToLoad == SchemaType.INVALID)
            {
                throw new InvalidOperationException("Schema Type Cannot be Invalid");
            }


            if (schemToLoad == SchemaType.FOMSCHEMA)
            {
                axonalDocumentPath = "C:\\Users\\depint\\source\\repos\\Hentul\\FirstOrderMemory\\Schema Docs\\AxonalSchemaFOM.xml";
            }
            else if (schemToLoad == SchemaType.SOMSCHEMA)
            {
                axonalDocumentPath = "C:\\Users\\depint\\source\\repos\\Hentul\\FirstOrderMemory\\Schema Docs\\AxonalSchema-SOM.xml";
            }

            if (!File.Exists(axonalDocumentPath))
            {
                throw new FileNotFoundException(axonalDocumentPath);
            }

            document.Load(axonalDocumentPath);

            XmlNodeList columns = document.GetElementsByTagName("AxonalConnection");

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

        #region ENUMS

        public enum LayerType
        {
            Layer_4,
            Layer_3A,
            Layer_3B,
            UNKNOWN
        }

        public enum SchemaType
        {
            FOMSCHEMA,
            SOMSCHEMA,
            INVALID

        }

        public enum BlockCycle
        {
            INITIALIZATION,
            DEPOLARIZATION,
            POLOARIZED,
            FIRING,
            CLEANUP
        }

        public enum LogMode
        {
            None,
            BurstOnly,
            All,
            Info
        }

        #endregion
    }
}
