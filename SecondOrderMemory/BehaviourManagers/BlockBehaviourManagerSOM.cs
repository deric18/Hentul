namespace SecondOrderMemory.Models
{
    using System;
    using System.ComponentModel;
    using System.Linq;
    using System.Xml;
    using Common;

    public class BlockBehaviourManagerSOM
    {
        #region VARIABLES

        public LayerType Layer { get; private set; }

        public ulong CycleNum { get; private set; }

        public int X { get; private set; }

        public int Y { get; private set; }

        public int Z { get; private set; }

        public int BBMID { get; private set; }

        public List<string> CurrentPredictions { get; private set; }

        private bool performHighOrderSequencing;

        public Dictionary<string, List<Neuron>> PredictedNeuronsfromLastCycle { get; private set; }

        public Dictionary<string, List<Neuron>> PredictedNeuronsForNextCycle { get; private set; }

        public Dictionary<string, List<Neuron>> PredictedNeuronsforThisCycle { get; private set; }

        public Dictionary<string, List<Neuron>> NeuronFiringNextCycle { get; private set; }

        public List<Neuron> NeuronsFiringThisCycle { get; private set; }

        public List<Neuron> NeuronsFiringLastCycle { get; private set; }

        //private List<Segment>? _predictedSegmentForThisCycle;

        public List<Position_SOM> ColumnsThatBurst { get; private set; }

        private int NumberOfColumnsThatFiredThisCycle { get; set; }

        private List<Neuron> temporalContributors { get; set; }

        private List<Neuron> apicalContributors { get; set; }

        public Column[,] Columns { get; private set; }

        private SDR_SOM SDR { get; set; }

        private Dictionary<ulong, List<Position_SOM>> TemporalCycleCache { get; set; }

        private Dictionary<ulong, List<Position_SOM>> BurstCache { get; set; }

        private Dictionary<ulong, List<Position_SOM>> ApicalCycleCache { get; set; }

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

        private bool IsBurstOnly;

        private bool includeBurstLearning145;

        private string backupDirectory = "C:\\Users\\depint\\Desktop\\Hentul\\Hentul\\BackUp\\";

        public iType PreviousiType { get; private set; }

        public iType CurrentiType { get; private set; }

        public bool IsCurrentTemporal { get; private set; }

        public bool IsCurrentApical { get; private set; }

        private SchemaType schemToLoad;

        public static LogMode Mode { get; private set; }

        public ulong[] WireCasesTracker { get; private set; }

        string logfilename = "C:\\Users\\depint\\source\\repos\\Hentul\\Hentul.log";

        public List<Neuron> OverConnectedOffenderList { get; private set; }

        public List<Neuron> OverConnectedInShortInterval { get; private set; }

        private uint _firingBlankStreak;

        private bool IgnorePostCycleCleanUp;

        public NetworkMode NetWorkMode { get; private set; }

        public string CurrentObjectLabel { get; private set; }

        private HashSet<string> SupportedLabels { get; set; }

        private uint neruonMissedinNeuronsFiringThisCycleCount;

        private uint NumberOfColumnsThatBurstLastCycle { get; set; }

        private uint num_continuous_burst;

        #endregion

        #region CONSTANTS

        public int TOTALNUMBEROFCORRECTPREDICTIONS = 0;
        public int TOTALNUMBEROFINCORRECTPREDICTIONS = 0;
        public int TOTALNUMBEROFPARTICIPATEDCYCLES = 0;
        public static uint DISTALNEUROPLASTICITY = 5;
        public static int NUMBER_OF_CLEANUP_CYCLES_TO_PRESERVE_TALE_VOLTAGE = 1;
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
        private const uint PRUNE_THRESHOLD = 10;
        private const uint PRUNE_STRENGTH = 1;
        private readonly int FOM_SCHEMA_PER_CYCLE_NEW_SYNAPSE_LIMIT;
        private readonly int SOM_SCHEMA_PER_CYCLE_NEW_SYNAPSE_LIMIT;
        private readonly int SOMLTOTAL_NEURON_CONNECTIONLIMIT;
        private const int NUMBER_OF_ALLOWED_MAX_BLACNK_FIRES_BEFORE_CLEANUP = 1;


        #endregion

        #region CONSTRUCTORS & INITIALIZATIONS 

        public BlockBehaviourManagerSOM(int x, int y = 10, int Z = 4, LayerType layertype = LayerType.UNKNOWN, LogMode mode = LogMode.None, string objectLabel = null, bool includeBurstLearning = false)
        {
            this.NumberOfColumnsThatBurstLastCycle = 0;

            this.NumberOfColumnsThatFiredThisCycle = 0;

            CycleNum = 0;

            totalDendronalConnections = 0;

            X = x;

            Y = y;

            this.Z = Z;

            CurrentPredictions = new();

            performHighOrderSequencing = true;

            PreviousiType = iType.NONE;

            CurrentiType = iType.NONE;

            IsCurrentApical = false;

            IsCurrentApical = false;

            PredictedNeuronsfromLastCycle = new Dictionary<string, List<Neuron>>();

            PredictedNeuronsforThisCycle = new Dictionary<string, List<Neuron>>();

            PredictedNeuronsForNextCycle = new Dictionary<string, List<Neuron>>();

            NeuronFiringNextCycle = new Dictionary<string, List<Neuron>>();

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

            if (layertype == LayerType.Layer_4)
            {
                throw new InvalidEnumArgumentException("Cannot accept argument Layer 4 for SOM Model");
            }

            this.Layer = layertype;

            FOM_SCHEMA_PER_CYCLE_NEW_SYNAPSE_LIMIT = (int)(0.1 * x * y * Z);
            SOM_SCHEMA_PER_CYCLE_NEW_SYNAPSE_LIMIT = (int)(0.05 * x * y * Z);
            SOMLTOTAL_NEURON_CONNECTIONLIMIT = (int)(0.25 * x * y * Z);

            OverConnectedOffenderList = new List<Neuron>();
            OverConnectedInShortInterval = new List<Neuron>();

            IgnorePostCycleCleanUp = false;

            _firingBlankStreak = 0;

            this.includeBurstLearning145 = includeBurstLearning;

            NetWorkMode = NetworkMode.TRAINING;

            CurrentObjectLabel = objectLabel;

            SupportedLabels = new HashSet<string>();

            neruonMissedinNeuronsFiringThisCycleCount = 0;
        }

        public void BeginTraining(string Label)
        {
            if (Label == null | Label == string.Empty)
            {
                throw new InvalidDataException("Label cannot be empty!");
            }

            NetWorkMode = NetworkMode.TRAINING;
            CurrentObjectLabel = Label;
        }

        public void ChangeNetworkModeToPrediction()
        {
            NetWorkMode = NetworkMode.PREDICTION;
            CurrentObjectLabel = null;
            CurrentPredictions.Clear();
        }

        public void Init(int bbmID)
        {
            BBMID = bbmID;

            for (int i = 0; i < X; i++)
            {
                for (int j = 0; j < Y; j++)
                {
                    try
                    {
                        Columns[i, j] = new Column(i, j, Z, BBMID);
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
                WriteLogsToFile(ex.Message);

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

        public static BlockBehaviourManagerSOM Restore(string filename, LayerType layer)
        {
            string backupDirectory;
            int X, Y, Z;

            if (layer.Equals(LayerType.Layer_4))
            {
                backupDirectory = "C:\\Users\\depint\\source\\repos\\Hentul\\Hentul\\BackUp\\FOM\\";
                X = 10; Y = 10; Z = 4;
            }
            else if (layer.Equals(LayerType.Layer_3A) || layer.Equals(LayerType.Layer_3B))
            {
                backupDirectory = "C:\\Users\\depint\\source\\repos\\Hentul\\Hentul\\BackUp\\SOM\\";
                X = 1250; Y = 10; Z = 4;
            }
            else
            {
                throw new InvalidOperationException("Invalid Data");
            }

            backupDirectory += filename;

            BlockBehaviourManagerSOM bbManager = new BlockBehaviourManagerSOM(X, Y, Z, layer, LogMode.BurstOnly);

            char delimater = '|';

            string[] jsonArray = File.ReadAllText(backupDirectory + filename).Split(delimater);

            List<Neuron> neuronList = new List<Neuron>();

            //NeuronConverter neuronConverter = new NeuronConverter();

            //foreach (var str in jsonArray)
            //{
            //    neuronList.Add(JsonConvert.DeserializeObject<Neuron>(str, new JsonConverter[] { neuronConverter }));
            //}


            //for (int i = 0; i < X; i++)
            //{
            //    for (int j = 0; j < Y; j++)
            //    {
            //        try
            //        {
            //            bbManager.Columns[i, j] = new Column(i, j, Z, i, neuronList.Where(item => item.NeuronID.X == i && item.NeuronID.Y == j && item.nType == NeuronType.NORMAL).ToList());
            //            bbManager.ApicalLineArray[i, j] = neuronList.FirstOrDefault(item => item.NeuronID.X == i && item.NeuronID.Y == j && item.NeuronID.W == 'A' && item.nType == NeuronType.APICAL);
            //        }
            //        catch (Exception ex)
            //        {
            //            int breakpoint = 0;
            //        }
            //    }
            //}

            //for (int i = 0; i < Y; i++)
            //{
            //    for (int j = 0; j < Z; j++)
            //    {
            //        bbManager.TemporalLineArray[i, j] = neuronList.FirstOrDefault(item => item.NeuronID.X == 0 && item.NeuronID.Y == i && item.NeuronID.Z == j && item.NeuronID.W == 'T' && item.nType == NeuronType.TEMPORAL);
            //    }
            //}

            return bbManager;
        }

        #endregion

        #region FIRE & WIRE        

        #region PUBLIC API

        /// <summary>
        /// Processes the Incoming Input Pattern
        /// </summary>
        /// <param name="incomingPattern"> Pattern to Process</param>
        /// <param name="currentCycle"> Pattern to Process</param>
        /// <param name="ignorePrecyclePrep"> Will not Perfrom CleanUp if False and vice versa</param>
        /// <param name="ignorePostCycleCleanUp">Will not Perfrom CleanUp if False and vice versa</param>
        /// <exception cref="InvalidOperationException"></exception>
        public bool Fire(SDR_SOM incomingPattern, ulong currentCycle = 0, bool ignorePrecyclePrep = false, bool ignorePostCycleCleanUp = false)
        {
            // BUG : Potential Bug:  if after one complete cycle of firing ( T -> A -> Spatial) performing a cleanup might remove reset probabilities for the next fire cycle
            this.IgnorePostCycleCleanUp = ignorePostCycleCleanUp;

            if (ignorePrecyclePrep == false)
                PreCyclePrep(currentCycle, incomingPattern.InputPatternType);

            if (!ValidateInput(incomingPattern, currentCycle))
                return false;

            _firingBlankStreak = 0;

            NumberOfColumnsThatFiredThisCycle = incomingPattern.ActiveBits.Count;

            switch (incomingPattern.InputPatternType)
            {
                case iType.SPATIAL:
                    {

                        for (int i = 0; i < incomingPattern.ActiveBits.Count; i++)
                        {
                            var predictedNeuronPositions = Columns[incomingPattern.ActiveBits[i].X, incomingPattern.ActiveBits[i].Y].GetPredictedNeuronsFromColumn();

                            if (predictedNeuronPositions?.Count == Columns[0, 0].Neurons.Count)
                            {
                                if (Mode == LogMode.Info || Mode == LogMode.All)
                                {
                                    Console.WriteLine("INFO :: BURST :: " + PrintBlockDetailsSingleLine() + " Bursting for incoming pattern X :" + incomingPattern.ActiveBits[i].X + " Y : " + incomingPattern.ActiveBits[i].Y);
                                    WriteLogsToFile("INFO :: BURST :: " + PrintBlockDetailsSingleLine() + " Bursting for incoming pattern X :" + incomingPattern.ActiveBits[i].X + " Y : " + incomingPattern.ActiveBits[i].Y);
                                }

                                AddNeuronListToNeuronsFiringThisCycleList(Columns[incomingPattern.ActiveBits[i].X, incomingPattern.ActiveBits[i].Y].Neurons);

                                ColumnsThatBurst.Add(incomingPattern.ActiveBits[i]);

                                IsBurstOnly = true;

                                num_continuous_burst++;

                                TotalBurstFire++;

                                NumberOfColumnsThatBurstLastCycle++;

                            }
                            else if (predictedNeuronPositions.Count == 1)
                            {
                                if (Mode == LogMode.All || Mode == LogMode.Info)
                                {
                                    Console.WriteLine("INFO :: Block ID : " + PrintBlockDetailsSingleLine() + " Old  Pattern : Predicting Predicted Neurons Count : " + predictedNeuronPositions.Count.ToString());
                                    WriteLogsToFile("INFO :: Block ID : " + PrintBlockDetailsSingleLine() + " Old  Pattern : Predicting Predicted Neurons Count : " + predictedNeuronPositions.Count.ToString());
                                }

                                AddNeuronListToNeuronsFiringThisCycleList(predictedNeuronPositions);

                                TotalPredictionFires++;
                            }
                            else
                            {
                                Console.WriteLine("ERROR :: " + schemToLoad.ToString() + "There Should only be one winner in the Column");
                                throw new InvalidOperationException("Fire :: This should not happen ! Bug in PickAwinner or Bursting Logic!!!");
                            }

                            predictedNeuronPositions = null;
                        }

                        CurrentiType = incomingPattern.InputPatternType;

                        break;
                    }
                case iType.TEMPORAL:
                    {
                        IsCurrentTemporal = true;

                        CurrentiType = incomingPattern.InputPatternType;

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
                        CurrentiType = incomingPattern.InputPatternType;

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
                    WriteLogsToFile("ERROR : Fire :: BurstCache was not cleaned up from last cycle ." + PrintBlockDetailsSingleLine());
                    WriteLogsToFile("Thread Being Slept to 2000");
                    //Thread.Sleep(2000);
                }
            }

            Fire();

            if (NetWorkMode.Equals(NetworkMode.TRAINING) && CurrentiType == iType.SPATIAL)
            {
                Wire();

                if(performHighOrderSequencing && CycleNum > 2)
                {
                    PerformHigherOrderSequencing();
                }
            }

            if (NetWorkMode == NetworkMode.PREDICTION)
                UpdateCurentPredictions();


            PrepNetworkForNextCycle(ignorePostCycleCleanUp, incomingPattern.InputPatternType);

            if (ignorePostCycleCleanUp == false)
                PostCycleCleanup(incomingPattern.InputPatternType);

            ValidateNetwork();

            return true;
        }

        private void UpdateCurentPredictions()
        {
            // After every Sptial Fire Collect all the intersection of all the supported labels from every single neuron that fired and remove the ones that are no longer supported.

            List<string> cyclePredictions = CurrentPredictions.ToList();

            foreach (var neuron in NeuronsFiringThisCycle)
            {
                var neuronalPredictions = neuron.GetCurrentPotentialMatchesForCurrentCycle();

                if (neuronalPredictions != null && neuronalPredictions.Count() > 0)
                {
                    if (cyclePredictions.Count == 0)
                    {
                        cyclePredictions.AddRange(neuronalPredictions);
                    }
                    else
                    {
                        // Get an intersection of cyclePredictions and neuronalPredictions
                        var intersect = cyclePredictions.Intersect(neuronalPredictions).ToList();

                        if(intersect.Count == 0)
                        {
                            // Total Mistake now need to restart from scratch , will cross that bridge when we get there.
                        }
                        else
                        {
                            //increase probability of intersected predictions to 100% if its 1 but if more than keep 50-50
                            if(intersect.Count == 0)
                            {
                                // Too little data. Move on with next Sequence.
                            }
                            if (intersect.Count == 1)
                            {
                                //Prediction is good , NEed to Verify if early in cycle.

                                NetWorkMode = NetworkMode.DONE;
                                CurrentObjectLabel = intersect[0];

                            }
                            else 
                            {
                                CurrentPredictions = intersect;
                            }
                        }
                    }
                }
            }
        }


        private void PerformHigherOrderSequencing()
        {
            // For all the neurons that fired in the last cycle should add the neurons that are firing this cycle as there respected ObjectLAbel predictions.

            //Q: What neurons should be used ? -- all

            // Iterate through NeuronsFiringLastCycle and add the neurons that are firing this cycle as there predicted neurons for next cycle.

            if (NeuronsFiringThisCycle?.Count == 0)
                throw new InvalidOperationException("Error : BlockBehaviourManagerSOm.cs : PerformHigherORderSequencing : NeuronsFiringThisCycle cannot be empty!");

            var nextNeuronIds = NeuronsFiringThisCycle.Where(x => x.nType == NeuronType.NORMAL).Select(x => x.NeuronID.ToString()).ToList();

            foreach (var prevneuron in NeuronsFiringLastCycle)
            {
                if (prevneuron.nType == NeuronType.NORMAL)
                {
                    prevneuron.PerformHigherSequencing(CurrentObjectLabel, nextNeuronIds);
                }
            }
        }

        private void Fire()
        {

            // Populate NeuronsFiringThiscycle
            if (CurrentiType == iType.SPATIAL)
            {
                foreach (var neuroString in PredictedNeuronsforThisCycle.Keys)
                {
                    var neuron = GetNeuronFromString(neuroString);

                    if (BBMUtils.CheckNeuronListHasThisNeuron(NeuronsFiringThisCycle, neuron) == false)
                    {
                        if (neuron.nType == NeuronType.NORMAL && (neuron.Voltage > Neuron.COMMON_NEURONAL_FIRE_VOLTAGE || neuron.CurrentState == NeuronState.FIRING))
                        {
                            if (Mode < LogMode.BurstOnly)
                            {
                                WriteLogsToFile("INFO :: Neuron in the Predicted List was in firing state. Adding it back now! Missed Count : " + PrintBlockDetailsSingleLine());

                                neruonMissedinNeuronsFiringThisCycleCount++;
                            }

                            NeuronsFiringThisCycle.Add(neuron);
                        }
                    }
                }

                foreach (var column in Columns)
                {
                    foreach (var neuron in column.Neurons)
                    {
                        if (BBMUtils.CheckNeuronListHasThisNeuron(NeuronsFiringThisCycle, neuron) == false)
                        {
                            if (neuron.nType == NeuronType.NORMAL && (neuron.Voltage > Neuron.COMMON_NEURONAL_FIRE_VOLTAGE || neuron.CurrentState == NeuronState.FIRING))
                            {
                                NeuronsFiringThisCycle.Add(neuron);

                                WriteLogsToFile("ERROR :: Neuron in the Network was in firing state. Adding it back now! Missed Count : " + neruonMissedinNeuronsFiringThisCycleCount.ToString() + PrintBlockDetailsSingleLine());
                            }
                        }
                    }
                }
            }


            // Iterate through NeuronsFiringTHisCycle and fire!
            foreach (var neuron in NeuronsFiringThisCycle)
            {
                neuron.Fire(CycleNum, Mode, logfilename);

                foreach (Synapse synapse in neuron.AxonalList.Values)       //it must have been done here for a reason.
                {
                    ProcessSpikeFromNeuron(synapse);
                }
            }
        }

        private void ProcessSpikeFromNeuron(Synapse synapse)
        {

            Neuron axonalNeuron = GetNeuronFromString(synapse.AxonalNeuronId);
            Neuron dendronalNeuron = GetNeuronFromString(synapse.DendronalNeuronalId);


            if (dendronalNeuron.NeuronID.ToString().Equals("1035-4-1-N"))
            {
                bool breakpoint = false;
            }


            if (synapse.cType == ConnectionType.TEMPRORAL)
            {
                if (!dendronalNeuron.TAContributors.TryGetValue(axonalNeuron.NeuronID.ToString(), out char w))
                {
                    dendronalNeuron.TAContributors.Add(axonalNeuron.NeuronID.ToString(), 'T');
                    dendronalNeuron.ProcessVoltage(TEMPORAL_NEURON_FIRE_VALUE, CycleNum, Mode);
                }
                else
                {
                    dendronalNeuron.ProcessVoltage(TEMPORAL_NEURON_FIRE_VALUE, CycleNum, Mode);
                }
            }
            else if (synapse.cType == ConnectionType.APICAL)
            {
                if (!dendronalNeuron.TAContributors.TryGetValue(axonalNeuron.NeuronID.ToString(), out char w))
                {
                    dendronalNeuron.TAContributors.Add(axonalNeuron.NeuronID.ToString(), 'A');
                    dendronalNeuron.ProcessVoltage(APICAL_NEURONAL_FIRE_VALUE, CycleNum, Mode);
                }
                else
                {
                    dendronalNeuron.ProcessVoltage(APICAL_NEURONAL_FIRE_VALUE, CycleNum, Mode);

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
            else if (synapse.cType == ConnectionType.AXONTONEURON_SCHEMA)
            {
                dendronalNeuron.ProcessVoltage(PROXIMAL_AXON_TO_NEURON_FIRE_VALUE, CycleNum, Mode);
            }
            else if (synapse.cType == ConnectionType.DISTALDENDRITICNEURON)
            {
                if (NetWorkMode == NetworkMode.TRAINING && (CurrentObjectLabel == null || CurrentObjectLabel == string.Empty))
                    throw new InvalidOperationException("Object Label Cannot be empty During Training Mode!");

                if (dendronalNeuron.ProximoDistalDendriticList.TryGetValue(axonalNeuron.NeuronID.ToString(), out var synapse1))
                    ProcessSpikeAsPer(synapse1, dendronalNeuron);

                //if (NetWorkMode.Equals(NetworkMode.PREDICTION)) Not needed since we are doing high order sequencing
                //{
                //    foreach (var label in synapse.SupportedLabels)
                //        SupportedLabels.Add(label);
                //}
            }
            else
            {
                Console.WriteLine(schemToLoad.ToString() + "ProcessSpikeFromNeuron() :::: ERROR :: One of the Neurons is not connected to the other neuron Source : " + axonalNeuron.NeuronID + " Target Neuron : " + dendronalNeuron.NeuronID);
                WriteLogsToFile(schemToLoad.ToString() + "ProcessSpikeFromNeuron() :::: ERROR :: One of the Neurons is not connected to the other neuron Source : " + axonalNeuron.NeuronID + " Target Neuron : " + dendronalNeuron.NeuronID);
                PrintBlockDetails();
                throw new InvalidOperationException("ProcessSpikeFromNeuron : Trying to Process Spike from Neuron which is not connected to this Neuron");
            }

            //Do not added Temporal and Apical Neurons to NeuronsFiringThisCycle, it throws off Wiring.                                
            AddPredictedNeuronForNextCycle(dendronalNeuron, axonalNeuron);
        }

        private void ProcessSpikeAsPer(Synapse synapse, Neuron targetNeuron)
        {

            if (synapse.IsActive)       //Process Voltage only if the synapse is active.
            {
                switch (synapse.cType)
                {
                    case ConnectionType.DISTALDENDRITICNEURON:
                        targetNeuron.ProcessVoltage(DISTAL_VOLTAGE_SPIKE_VALUE, CycleNum, Mode);
                        break;
                    case ConnectionType.NMDATONEURON:
                        targetNeuron.ProcessVoltage(NMDA_NEURONAL_FIRE_VALUE, CycleNum, Mode);
                        break;

                    default: throw new InvalidOperationException("Object Label Should never have this type Conneciton!");
                }
            }
            //else      //This should always be commented because if the second neuron did not fire in the next cycle then the hitcount should not be incremented , the increment should only happen in cases when the second neuron actually did fire in the next timestamp.
            //{
            //    synapse.IncrementHitCount();
            //}            
        }

        public SDR_SOM GetPredictedSDRForNextCycle(ulong currentCycle = 1)
        {
            SDR_SOM toReturn = null;


            if (currentCycle - CycleNum == 1 && NeuronsFiringLastCycle.Count != 0)
            {
                List<Position_SOM> ActiveBits = new List<Position_SOM>();

                //Using PredictedNeuronsforThisCycle as this PredictedNeuronsforNextCycle gets assigned to this.
                foreach (var neuronstringID in PredictedNeuronsforThisCycle.Keys)
                {
                    var pos = Position_SOM.ConvertStringToPosition(neuronstringID);
                    ActiveBits.Add(pos);
                    toReturn = new SDR_SOM(X, Y, ActiveBits, iType.NONE);
                }
            }
            else if (currentCycle - CycleNum >= 3)
            {
                throw new InvalidOperationException("FOM / SOM does not predict that far ahead!");
            }

            return toReturn;
        }

        public SDR_SOM GetAllNeuronsFiringLatestCycle(ulong currentCycle, bool ignoreZ = true)
        {
            List<Position_SOM> activeBits = new List<Position_SOM>();            

            if (_firingBlankStreak >= NUMBER_OF_ALLOWED_MAX_BLACNK_FIRES_BEFORE_CLEANUP && NeuronsFiringLastCycle.Count > 0)
            {
                throw new InvalidOperationException("Neurons Firing Last Cycle Should be empty after Blank Fires");
            }

            // Commenting the code below to support white images.
            //if (NeuronsFiringLastCycle.Count == 0)
            //    throw new InvalidOperationException("SOM Layer did not fire Last cycle! Check your stupid dumb Code");

            if (currentCycle - CycleNum <= 1 && NeuronsFiringLastCycle.Count != 0)
            {
                if (ignoreZ == true)
                {
                    foreach (var neuron in NeuronsFiringLastCycle)
                    {
                        if (CheckForDuplicates(activeBits, neuron.NeuronID))
                        {
                            activeBits.Add(neuron.NeuronID);
                        }
                    }
                }
                else
                {
                    NeuronsFiringLastCycle.ForEach(n => { if (n.nType == NeuronType.NORMAL) activeBits.Add(n.NeuronID); });
                }                
            }

            return new SDR_SOM(X, Y, activeBits, iType.SPATIAL);
        }

        public List<string> GetCurrentPredictions()
        {
            if (Layer.Equals(LayerType.Layer_3A) == false)
            {
                throw new InvalidOperationException("Only Layer 3A is a Pooling Layer");
            }

            if (NetWorkMode != NetworkMode.PREDICTION)
            {
                throw new InvalidOperationException("Should Only be Called during Prediction Mode!");
            }

            return CurrentPredictions.ToList();
        }

        public List<Position_SOM> GetAllPredictedNeurons()
        {

            List<Position_SOM> toRet = new List<Position_SOM>();

            for (int i = 0; i < X; i++)
            {
                for (int j = 0; j < Y; j++)
                {
                    foreach (var neuron in Columns[i, j].Neurons)
                    {
                        if (neuron.Voltage >= 90)
                        {
                            toRet.Add(neuron.NeuronID);
                        }


                    }
                }
            }


            return toRet;
        }

        public List<Position_SOM> GetAllFiringNeurons()
        {
            List<Position_SOM> firingList = new List<Position_SOM>();

            List<Position_SOM> toRet = new List<Position_SOM>();

            for (int i = 0; i < X; i++)
            {
                for (int j = 0; j < Y; j++)
                {
                    foreach (var neuron in Columns[i, j].Neurons)
                    {

                        if (neuron.Voltage >= 100 || neuron.CurrentState == NeuronState.FIRING)
                        {
                            firingList.Add(neuron.NeuronID);
                        }
                    }
                }
            }


            return firingList;
        }

        public void FireBlank(ulong currentCycle)
        {
            if (CycleNum > currentCycle)
            {
                throw new InvalidOperationException("This should never happen");
            }
            else
            {
                CycleNum = currentCycle;

                _firingBlankStreak++;

                if (_firingBlankStreak >= NUMBER_OF_ALLOWED_MAX_BLACNK_FIRES_BEFORE_CLEANUP && NeuronsFiringLastCycle.Count > 0)
                {
                    FlushAllNeuronsInList(NeuronsFiringLastCycle);
                    CompleteCleanUP();
                }
            }
        }

        private void Wire()
        {
            //Todo : Provide an enum for the wiring stratergy picked and simplify the below logic to a switch statement

            bool allowSequenceMemoryWireCase1 = true;
            //includeBurstLearning145 = false;
            bool allowSequenceMemoryWireCase3 = true;

            if (CurrentiType.Equals(iType.SPATIAL))
            {

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

                if (ColumnsThatBurst.Count == 0 && correctPredictionList.Count != 0 && correctPredictionList.Count >= NumberOfColumnsThatFiredThisCycle)
                {
                    //Case 1: All Predicted Neurons Fired without anyone Bursting.
                    if (Mode == LogMode.Trace)
                    {
                        Console.WriteLine("Wire Case 1 Picked Up!!");
                    }

                    WireCasesTracker[0]++;

                    if (Mode == LogMode.All || Mode == LogMode.Info)
                    {
                        Console.WriteLine(" EVENT :: Wire CASE 3 just Occured Count : " + WireCasesTracker[0].ToString());
                        WriteLogsToFile(" EVENT :: Wire CASE 3 just Occured Count : " + WireCasesTracker[0].ToString());
                    }

                    List<Neuron> contributingList;

                    foreach (var correctlyPredictedNeuron in correctPredictionList)
                    {
                        contributingList = new List<Neuron>();

                        if (PredictedNeuronsforThisCycle.TryGetValue(correctlyPredictedNeuron.NeuronID.ToString(), out contributingList))
                        {
                            foreach (var contributingNeuron in contributingList)
                            {
                                PramoteCorrectlyPredictedDendronal(contributingNeuron, correctlyPredictedNeuron);
                            }
                        }
                    }

                    if (allowSequenceMemoryWireCase1)
                        BuildSequenceMemory();
                }
                else if (ColumnsThatBurst.Count != 0 && correctPredictionList.Count != 0)
                {
                    //Case 2 :  Few Correctly Fired, Few Bursted  : Strengthen the Correctly Fired Neurons
                    //          For Correctly Predicted : Pramote Correctly Predicted Synapses. 
                    //          For Bursted             : Analyse did anybody contribute to the column and dint burst ? if nobody contributed then do Wire 1 Distal Synapses with all the neurons that fired last cycle                   

                    //Boost the few correctly predicted neurons

                    WireCasesTracker[1]++;

                    if (Mode == LogMode.Trace)
                    {
                        Console.WriteLine("Wire Case 2 Picked Up!!");
                    }

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
                        else
                        {
                            WriteLogsToFile("WIRE WARNING :: coorectly predicted Neuron does not exist in PredictedNeuronsforThisCycle. CorrectlyPredicted Neuron :" + correctlyPredictedNeuron.NeuronID.ToString());
                        }
                    }

                    //Bug : Boosting should not juice the same neurons!

                    //Todo : Need to revisit this stratergy of connecting all the boosted neurons.

                    //Boost the Bursting neurons
                    if (includeBurstLearning145 == true)
                        ConnectAllBurstingNeuronstoNeuronssFiringLastcycle();

                }// ColumnsThatBurst.Count == 0 && correctPredictionList.Count = 5 &&  NumberOfColumnsThatFiredThisCycle = 8  cycleNum = 4 , repNum = 29
                else if (ColumnsThatBurst.Count == 0 && NumberOfColumnsThatFiredThisCycle > correctPredictionList.Count)
                {
                    // Case 3 : None Bursted , Some Fired which were NOT predicted , Some fired which were predicted

                    // Strengthen the ones which fired correctly 
                    List<Neuron> contributingList;

                    WireCasesTracker[2]++;

                    if (Mode == LogMode.Trace)
                    {
                        Console.WriteLine("Wire Case 3 Picked Up!!");
                    }

                    if (Mode.Equals(LogMode.All) || Mode.Equals(LogMode.Info))
                    {
                        Console.WriteLine(" EVENT :: PARTIAL ERROR CASE : Wire CASE 3 just Occured Count : " + WireCasesTracker[2].ToString());
                        WriteLogsToFile(" EVENT :: PARTIAL ERROR CASE : Wire CASE 3 just Occured Count : " + WireCasesTracker[2].ToString());
                        //Thread.Sleep(1000);
                    }

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

                    //For the ones that were not predicted, perform sequence memory.
                    if (allowSequenceMemoryWireCase3)
                        BuildSequenceMemory();
                }
                else if (ColumnsThatBurst.Count == NumberOfColumnsThatFiredThisCycle && correctPredictionList.Count == 0)
                {
                    //Case 4 : All columns Bursted: highly likely first fire or totally new pattern coming in :
                    //         If firing early cycle, then just wire 1 Distal Syanpse to all the neurons that fired last cycle and 1 random connection.
                    //         Todo : If in the middle of the cycle(atleast 10,000 cycles ) then Need to do something new.


                    //BUG 1: NeuronsFiredLastCycle = 10 when last cycle was a Burst Cycle and if this cycle is a Burst cycle then the NeuronsFiringThisCycle will be 10 as well , that leads to 100 new distal connections , not healthy.
                    //Feature : Synapses will be marked InActive on Creation and eventually marked Active once the PredictiveCount increases.
                    //BUG 2: TotalNumberOfDistalConnections always get limited to 400

                    WireCasesTracker[3]++;

                    if (Mode == LogMode.Trace)
                    {
                        Console.WriteLine("Wire Case 4 Picked Up!!");
                    }

                    if (includeBurstLearning145)
                        ConnectAllBurstingNeuronstoNeuronssFiringLastcycle();
                }
                else if (ColumnsThatBurst.Count < NumberOfColumnsThatFiredThisCycle && correctPredictionList.Count == 0)
                {

                    //Case 5 : None of the predicted Neurons did Fire and some also did BURST and some of them did Fire that were not added to the prediction list.
                    //          For Bursted             : Analyse did anybody contribute to the column and dint burst ? if nobody contributed then do Wire 1 Distal Synapses with all the neurons that fired last cycle                   
                    //          For Fired               : The Fired Neurons did not burst because some neurons deplolarized it in the last cycle , connect to all the neurons that contributed to its Firing.

                    //Bug : Somehow all the neurons in the column have the same voltage , but none of them are added to the PredictedNeuronsForThisCycle List from last firing Cycle.

                    WireCasesTracker[4]++;

                    if (Mode == LogMode.Trace)
                    {
                        Console.WriteLine("Wire Case 5 Picked Up!!");
                    }

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
                                    WriteLogsToFile("Exception : Wire() :: Neurons That Fire Together are not Wiring Together" + PrintBlockDetailsSingleLine());

                                    PramoteCorrectlyPredictedDendronal(lastcycleneuron, neuron);
                                }
                            }
                        }
                    }

                    //Boost All the Bursting Neurons
                    if (includeBurstLearning145 == true)
                        ConnectAllBurstingNeuronstoNeuronssFiringLastcycle();

                    //Boost the Non Bursting Neurons

                    var firingNeurons = AntiUniounWithNeuronsFiringThisCycle(burstList);

                    if (firingNeurons.Count != 0)
                    {
                        foreach (var dendriticNeuron in firingNeurons)
                        {
                            foreach (var axonalNeuron in NeuronsFiringLastCycle)
                            {
                                if (CheckifBothNeuronsAreSameOrintheSameColumn(axonalNeuron, dendriticNeuron) == false)
                                    if (ConnectTwoNeurons(axonalNeuron, dendriticNeuron, ConnectionType.DISTALDENDRITICNEURON) == ConnectionRemovalReturnType.HARDFALSE)
                                        throw new InvalidOperationException("Unable to Connect two neurons!");
                            }
                        }
                    }

                    //May be some voltage on this column was not cleaned up from last cycle somehow or may be its because of the Synapse Not Active Logic i put few weeks back because of PredictedNeuronsList Getting overloaded to 400. now its reducded to 60 per cycle.
                }
                else
                {   // BUG: Few Bursted , Few Fired which were not predicted // Needs analysis on how something can fire without bursting which were not predicted.
                    throw new NotImplementedException("This should never happen or the code has bugs! Get on it Biiiiiyaaattttcccchhhhhhhh!!!!! Need ENumification of Wire Cases!!!");
                }
            }
            else if (IsCurrentTemporal)
            {
                CurrentiType = iType.TEMPORAL;

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

                CurrentiType = iType.APICAL;

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

        #endregion

        #region INIT METHOD

        private void PrepNetworkForNextCycle(bool ignorePostCycleCleanUp, iType type)
        {
            PerCycleFireSparsityPercentage = (NeuronsFiringThisCycle.Count * 100 / (X * Y * Z));

            if (PerCycleFireSparsityPercentage > 20)
            {
                Console.WriteLine(schemToLoad.ToString() + PrintBlockDetailsSingleLine() + "WARNING :: PrepNetworkForNextCycle :: PerCycleFiringSparsity is exceeding 20 %");
                WriteLogsToFile(schemToLoad.ToString() + PrintBlockDetailsSingleLine() + "WARNING :: PrepNetworkForNextCycle :: PerCycleFiringSparsity is exceeding 20 %");
            }

            if (CurrentiType == iType.SPATIAL)      //Cache firing pattern
            {
                NeuronsFiringLastCycle.Clear();

                foreach (var neuron in NeuronsFiringThisCycle)
                {
                    //Prep for Next cycle Prediction
                    if (neuron.nType.Equals(NeuronType.NORMAL))
                        NeuronsFiringLastCycle.Add(neuron);

                }

            }

            /*  Clean Up Policy : 
             *      1. Any Neuron that was predicted to be fired this cycle but did not will get cleaned up.
             *      2. Should not touch any neuron that is being deploarized this cycle.             
             */
            if (ignorePostCycleCleanUp == false && CurrentiType.Equals(iType.SPATIAL))
            {
                // Perform a mutual exclusive list between NeuronsPredictedForThisCycle , NeuronsFiringThisCycle for cleanup

                var predictedList = ConvertDictToList(PredictedNeuronsforThisCycle);
                var neuronsThatWerePredictedButDidNotFire = BBMUtils.PerformLeftOuterJoinBetweenTwoLists(predictedList, NeuronsFiringLastCycle);

                foreach (var cleanUpNeuron in neuronsThatWerePredictedButDidNotFire)
                {
                    if (cleanUpNeuron.NeuronID.ToString() == "2-1-3-N")
                    {
                        bool breakpoint = true;
                    }


                    cleanUpNeuron.FlushVoltage(CycleNum);
                }

            }
            else
            {
                if (!Mode.Equals(LogMode.None))
                {
                    Console.WriteLine("WARNING :: PrepNetworkForNextcycle :: Ignoring Clean Up of Stale voltage Clean Up!!!");
                    WriteLogsToFile("WARNING :: PrepNetworkForNextcycle :: Ignoring Clean Up of Stale voltage Clean Up!!!");
                }
            }

            PredictedNeuronsfromLastCycle.Clear();

            foreach (var kvp in PredictedNeuronsforThisCycle)
                PredictedNeuronsfromLastCycle.Add(kvp.Key, kvp.Value);

            PredictedNeuronsforThisCycle.Clear();

            foreach (var kvp in PredictedNeuronsForNextCycle)
            {
                PredictedNeuronsforThisCycle[kvp.Key] = kvp.Value;
            }

            if (PredictedNeuronsForNextCycle.Count >= (0.2 * X * Y * Z))
            {
                Console.WriteLine("WARNING :: Total Number of Predicted Neurons should not exceed more than 10% of Network size" + PrintBlockDetailsSingleLine());
                WriteLogsToFile("WARNING :: Total Number of Predicted Neurons should not exceed more than 10% of Network size" + PrintBlockDetailsSingleLine());
                //Console.ReadKey();
            }

            PredictedNeuronsForNextCycle.Clear();
        }

        //Selective Clean Up Logic , Should never perform Full Clean up.
        private void PostCycleCleanup(iType type)
        {
            //Case 1 : If temporal or Apical or both lines have deplolarized and spatial fired then clean up temporal or apical or both.
            if ((PreviousiType.Equals(iType.APICAL) || PreviousiType.Equals(iType.TEMPORAL)) && CurrentiType.Equals(iType.SPATIAL))
            {
                if (TemporalCycleCache.Count == 1)
                {
                    foreach (var kvp in TemporalCycleCache)
                    {
                        if (CycleNum - kvp.Key > 3)
                        {
                            Console.WriteLine("ERROR :: PostCycleCleanUp :: Temporal Cached Pattern is older than Spatial Pattern!" + PrintBlockDetailsSingleLine());
                            WriteLogsToFile("ERROR :: PostCycleCleanUp :: Temporal Cached Pattern is older than Spatial Pattern!" + PrintBlockDetailsSingleLine());
                            throw new InvalidOperationException("Temporal Cache is older than Spatial Pattern");
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

                                    if (neuronToCleanUp.Voltage != 0 && neuronToCleanUp.CurrentState != NeuronState.SPIKING)
                                    {
                                        neuronToCleanUp.FlushVoltage(CycleNum);
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
                            WriteLogsToFile("ERROR :: PostCycleCleanUp :: Temporal Cached Pattern is older than Spatial Pattern! " + PrintBlockDetailsSingleLine());
                            throw new InvalidOperationException("Apical Cache is older than Spatial Pattern");
                        }

                        foreach (var pos in kvp.Value)
                        {
                            foreach (var synapse in ApicalLineArray[pos.X, pos.Y].AxonalList.Values)
                            {
                                if (synapse.DendronalNeuronalId != null) // && BBMUtils.CheckNeuronListHasThisNeuron(NeuronsFiringThisCycle, synapse.DendronalNeuronalId) == false)
                                {
                                    var neuronToCleanUp = GetNeuronFromString(synapse.DendronalNeuronalId);

                                    if (neuronToCleanUp.Voltage != 0 && neuronToCleanUp.CurrentState != NeuronState.SPIKING)
                                    {
                                        neuronToCleanUp.FlushVoltage(CycleNum);
                                    }
                                    else if (neuronToCleanUp.CurrentState != NeuronState.SPIKING)
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

            }


            //Case 2 : If a bursting signal came through , after wire , the Bursted neurons and all its connected cells should be cleaned up. Feature : How many Burst Cycle to wait before performing a full clean ? Answer : 1
            if (IsBurstOnly)
            {
                foreach (var kvp in BurstCache)
                {
                    if (CycleNum - kvp.Key > 3)
                    {
                        Console.WriteLine("ERROR :: PostCycleCleanUp :: Temporal Cached Pattern is older than Spatial Pattern! " + PrintBlockDetailsSingleLine());
                        WriteLogsToFile("ERROR :: PostCycleCleanUp :: Temporal Cached Pattern is older than Spatial Pattern! " + PrintBlockDetailsSingleLine());
                    }

                    foreach (var pos in kvp.Value)
                    {
                        for (int i = 0; i < Z; i++)
                        {
                            var neuronToCleanUp = Columns[pos.X, pos.Y].Neurons[i];

                            if (neuronToCleanUp.Voltage != 0)
                            {
                                neuronToCleanUp.FlushVoltage(CycleNum);
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


            //Case 3: If NeuronFiringThicCycle has any Temporal / Apical Firing Neurons they should be cleaned up [Thought : The neurons have already fired and Wired , Kepingg them in the list will only complicate next cycle process , the other thing is if any neuron is spiking , it should not be cleaned up
            // since that will run the temporal dynamics of the system.
            if (CurrentiType == iType.SPATIAL)
            {
                foreach (var neuron in NeuronsFiringThisCycle)
                {
                    //Cleanup voltages of all the Neurons that Fired this cycle unless its Spiking
                    if (neuron.CurrentState != NeuronState.SPIKING)
                        neuron.FlushVoltage(CycleNum);
                }

                // Alternative approach bit more sophisticated but buggy.
                //foreach (var neuronList in PredictedNeuronsforThisCycle)
                //{

                //    var neuron = GetNeuronFromString(neuronList.Key);

                //    if (neuron.Voltage >= Neuron.COMMON_NEURONAL_FIRE_VOLTAGE && NeuronsFiringThisCycle.Contains(neuron) == false)
                //    {
                //        Console.WriteLine(" ERROR :: " + Layer.ToString() + " Neuron ID : " + neuron.NeuronID.ToString() + "  has a Higher Voltage than actual firing Voltage but did not get picked up for firing  ");
                //        WriteLogsToFile(" ERROR:: " + Layer.ToString() + " Neuron ID: " + neuron.NeuronID.ToString() + "  has a Higher Voltage than actual firing Voltage but did not get picked up for firing  ");
                //        //Thread.Sleep(3000);
                //    }
                //}

            }

            // Case 4: Clean Up Stale Spiking Neurons
            // BUG : If the neuron did fire this cycle as well and it is still spiking then it should be allowed to stay spiking 
            foreach (var neuron in GetSpikingNeuronList())
            {
                if (BBMUtils.CheckNeuronListHasThisNeuron(NeuronsFiringThisCycle, neuron) == false)
                {
                    neuron.CheckSpikingFlush(CycleNum);
                }
            }

            if (Mode != LogMode.None)
            {
                // Log Neurons which dint get picked up for firing despite potential higher voltage.
                foreach (var column in Columns)
                {
                    foreach (var neuron in column.Neurons)
                    {
                        if (BBMUtils.CheckNeuronListHasThisNeuron(NeuronsFiringThisCycle, neuron) && neuron.Voltage >= Neuron.COMMON_NEURONAL_FIRE_VOLTAGE)
                        {
                            if (Mode != LogMode.None)
                            {
                                Console.WriteLine(" ERROR :: " + Layer.ToString() + " Neuron ID : " + neuron.NeuronID.ToString() + "  has a Higher Voltage than actual firing Voltage but did not get picked up for firing  ");
                                WriteLogsToFile(" ERROR:: " + Layer.ToString() + " Neuron ID: " + neuron.NeuronID.ToString() + "  has a Higher Voltage than actual firing Voltage but did not get picked up for firing  ");
                            }
                        }
                    }
                }
            }

            NeuronsFiringThisCycle.Clear();

            ColumnsThatBurst.Clear();

            //Every 50 Cycles Prune unused and under Firing Connections
            if (CycleNum >= 100 && CycleNum % 50 == 0)
            {
                if (Mode == LogMode.All || Mode == LogMode.Info)
                {
                    WriteLogsToFile(" INFO :: Performing Prune(). Cycle NUM : " + CycleNum.ToString() + " BBM ID : " + BBMID.ToString() + " Layer Type : " + Layer.ToString());
                }
                Prune();
                TotalBurstFire = 0;
                TotalPredictionFires = 0;
            }

            IsBurstOnly = false;

            PreviousiType = type;
        }



        #endregion

        #region INTERNAL METHODS

        private void FlushAllNeuronsInList(List<Neuron> list)
        {
            foreach (var neuron in list)
            {
                if (neuron.Voltage != 0)
                {
                    neuron.FlushVoltage(CycleNum);
                }
            }
        }

        private void ConnectAllBurstingNeuronstoNeuronssFiringLastcycle()
        {
            foreach (var position in ColumnsThatBurst)
            {
                foreach (var dendriticNeuron in Columns[position.X, position.Y].Neurons)
                {
                    foreach (var axonalNeuron in NeuronsFiringLastCycle)
                    {
                        if (CheckifBothNeuronsAreSameOrintheSameColumn(dendriticNeuron, axonalNeuron) == false && BBMUtils.CheckIfTwoNeuronsAreConnected(axonalNeuron, dendriticNeuron) == false)
                        {
                            if (ConnectTwoNeurons(axonalNeuron, dendriticNeuron, ConnectionType.DISTALDENDRITICNEURON) == ConnectionRemovalReturnType.HARDFALSE)
                            {
                                throw new InvalidOperationException("Unable to connect neurons!");
                            }
                        }
                    }
                }
            }
        }

        public string PrintBlockDetailsSingleLine()
        {
            return "  BBM ID : " + BBMID.ToString() + " Layer Type : " + Layer.ToString();
        }

        public void AddPredictedNeuronForNextCycle(Neuron predictedNeuron, Neuron contributingNeuron)
        {
            List<Neuron> contributingList = new List<Neuron>();

            //Without RESTING State no new connections will be done.

            if (PredictedNeuronsForNextCycle.Count > 0 && PredictedNeuronsForNextCycle.TryGetValue(predictedNeuron.NeuronID.ToString(), out contributingList))
            {
                contributingList.Add(contributingNeuron);
            }
            else
            {
                PredictedNeuronsForNextCycle.Add(predictedNeuron.NeuronID.ToString(), new List<Neuron>() { contributingNeuron });
            }
        }

        public ConnectionRemovalReturnType ConnectTwoNeurons(Neuron AxonalNeuron, Neuron DendriticNeuron, ConnectionType cType, bool IsActive = false)
        {

            #region Verification

            if (NetWorkMode != NetworkMode.TRAINING)
            {
                return ConnectionRemovalReturnType.SOFTFALSE;
            }
            if (AxonalNeuron == null || DendriticNeuron == null)
            {
                return ConnectionRemovalReturnType.SOFTFALSE;
            }
            else if (CurrentiType != iType.NONE && (AxonalNeuron.nType.Equals(NeuronType.TEMPORAL) || AxonalNeuron.nType.Equals(NeuronType.APICAL)))
            {
                // Post Init Temporal / Apical Neurons should not connect with anybody else.
                throw new InvalidOperationException("ConnectTwoNeuronsOrIncrementStrength :: Temporal Neurons cannot connect to Normal Neurons Post Init!");
            }
            else if (AxonalNeuron.NeuronID.X == DendriticNeuron.NeuronID.X && AxonalNeuron.NeuronID.Y == DendriticNeuron.NeuronID.Y && AxonalNeuron.nType.Equals(DendriticNeuron.nType))
            {
                Console.WriteLine("Error :: ConnectTwoNeurons :: Cannot Connect Neuron to itself! Block Id : " + PrintBlockDetailsSingleLine() + " Neuron ID : " + AxonalNeuron.NeuronID.ToString());     // No Same Column Connections 
                WriteLogsToFile("Error :: ConnectTwoNeurons :: Cannot Connect Neuron to itself! Block Id : " + PrintBlockDetailsSingleLine() + " Neuron ID : " + AxonalNeuron.NeuronID.ToString());
                //Thread.Sleep(2000);                                                                                                                                                      //throw new InvalidDataException("ConnectTwoNeurons: Cannot connect Neuron to Itself!");
                return ConnectionRemovalReturnType.SOFTFALSE;
            }
            else if (AxonalNeuron.nType.Equals(DendriticNeuron.nType) && AxonalNeuron.NeuronID.Equals(DendriticNeuron.NeuronID))                                                      // No Selfing               
            {
                Console.WriteLine("ERROR :: ConnectTwoNeurons() :: Cannot connect neurons in the same Column [NO SELFING] " + PrintBlockDetailsSingleLine());
                WriteLogsToFile("ERROR :: ConnectTwoNeurons() :: Cannot connect neurons in the same Column [NO SELFING] " + PrintBlockDetailsSingleLine());
                return ConnectionRemovalReturnType.SOFTFALSE;
            }

            if (cType == ConnectionType.DISTALDENDRITICNEURON && (CurrentObjectLabel == null || CurrentObjectLabel == string.Empty))
            {
                return ConnectionRemovalReturnType.SOFTFALSE;
            }

            #endregion

            ConnectionRemovalReturnType success = ConnectionRemovalReturnType.HARDFALSE;

            if (cType == ConnectionType.TEMPRORAL || cType == ConnectionType.APICAL)
            {
                success = CheckNConnectTwoNeurons(AxonalNeuron, DendriticNeuron, cType, true);
            }
            else if (cType == ConnectionType.DISTALDENDRITICNEURON)
            {
                //Check For OverConnecting Neurons
                if (AxonalNeuron.nType.Equals(NeuronType.NORMAL) && DendriticNeuron.nType.Equals(NeuronType.NORMAL) && DendriticNeuron.ProximoDistalDendriticList.Count >= SOMLTOTAL_NEURON_CONNECTIONLIMIT)
                {
                    Console.WriteLine("WARNING :: ConnectTwoNeurons :::: Neuron inelgible to  have any more Connections! Auto Selected for Pruning Process " + PrintBlockDetailsSingleLine());
                    WriteLogsToFile(" WARNING :: ConnectTwoNeurons :::: Neuron inelgible to  have any more Connections ! Auto Selected for Pruning Process " + PrintBlockDetailsSingleLine());

                    PruneSingleNeuron(DendriticNeuron);

                    if (DendriticNeuron.ProximoDistalDendriticList.Count >= SOMLTOTAL_NEURON_CONNECTIONLIMIT && schemToLoad == SchemaType.SOMSCHEMA_VISION)
                    {
                        Console.WriteLine("ERROR :: Neuronal Distal Dendritic Connection is not reducing even after pruning!!!");
                        WriteLogsToFile("ERROR :: Neuronal Distal Dendritic Connection is not reducing even after pruning!!!");
                        //Thread.Sleep(1000);
                        OverConnectedOffenderList.Add(DendriticNeuron);
                    }
                }

                success = CheckNConnectTwoNeurons(AxonalNeuron, DendriticNeuron, cType, false);
            }

            return success;
        }

        private ConnectionRemovalReturnType CheckNConnectTwoNeurons(Neuron AxonalNeuron, Neuron DendriticNeuron, ConnectionType cType, bool IsActive = false)
        {
            ConnectionRemovalReturnType IsAxonalConnectionSuccesful;
            //Add only Axonal Connection first to check if its not already added before adding dendronal Connection.           
            IsAxonalConnectionSuccesful = AxonalNeuron.AddtoAxonalList(DendriticNeuron.NeuronID.ToString(), CurrentObjectLabel, AxonalNeuron.nType, CycleNum, cType, schemToLoad, IsActive);

            if (IsAxonalConnectionSuccesful == ConnectionRemovalReturnType.TRUE)
            {
                bool IsDendronalConnectionSuccesful = DendriticNeuron.AddToDistalList(AxonalNeuron.NeuronID.ToString(), CurrentObjectLabel, DendriticNeuron.nType, CycleNum, schemToLoad, logfilename, cType, IsActive);

                if (cType.Equals(ConnectionType.DISTALDENDRITICNEURON))     //New Connection Added
                {
                    TotalDistalDendriticConnections++;
                }

                if (IsDendronalConnectionSuccesful)
                {
                    if ((Mode == LogMode.All || Mode == LogMode.Info))
                    {
                        Console.WriteLine("INFO :: Added new Distal Connection between two Neurons :: A: " + AxonalNeuron.NeuronID.ToString() + " D : " + DendriticNeuron.NeuronID.ToString());
                        WriteLogsToFile("INFO :: Added new Distal Connection between two Neurons :: A: " + AxonalNeuron.NeuronID.ToString() + " D : " + DendriticNeuron.NeuronID.ToString());
                    }

                    return ConnectionRemovalReturnType.TRUE;
                }
                else if (IsDendronalConnectionSuccesful == false)//If dendronal connection did not succeed then the structure is compromised 
                {
                    var returnType = AxonalNeuron.RemoveAxonalConnection(DendriticNeuron);

                    if (returnType == ConnectionRemovalReturnType.TRUE)
                    {
                        //Do Nothing just return false below.
                        return ConnectionRemovalReturnType.TRUE;
                    }
                    else if (returnType == ConnectionRemovalReturnType.SOFTFALSE)
                    {
                        WriteLogsToFile(" ERROR :: Attempting to Remove Schema invoked AXON TO NEURON Connection while connection two Neurons");
                        throw new InvalidOperationException("Attempting to Remove Schema invoked AXON TO NEURON Connection while connection two Neurons");
                    }
                    else if (returnType == ConnectionRemovalReturnType.HARDFALSE)
                    {
                        Console.WriteLine(" ERROR :: Axonal Connection Succeded but Distal Connection Failed! ");
                        WriteLogsToFile(" ERROR :: Axonal Connection Succeded but Distal Connection Failed! ");
                        throw new InvalidOperationException("Neuronal Network Structure Is Compromised ! Cannot pursue any further Layer Type :: " + Layer.ToString() + " BBM ID : " + BBMID.ToString());
                    }
                }
            }
            else if (IsAxonalConnectionSuccesful == ConnectionRemovalReturnType.HARDFALSE)
            {
                //Need investigation as why the same connection is tried twice
                WriteLogsToFile(" ERROR :: ConnectTwoNeurons :::: Could not ADD new Dendronal Connection to the Neuron and return a HArd False! Dendronal Neuron ID : " + DendriticNeuron.NeuronID + " Layer Type :" + Layer.ToString());
                throw new InvalidOperationException("HARD FALSE eStablishing new Axonal Connection!");
            }
            else if (IsAxonalConnectionSuccesful == ConnectionRemovalReturnType.SOFTFALSE)
            {
                //Need investigation as why the same connection is tried twice : No need to investigate
                WriteLogsToFile(" ERROR :: ConnectTwoNeurons ::::Another Failed Attempt at adding a redundant Connection Dendronal Neuron ID :  " + DendriticNeuron.NeuronID + " Axonal Neuron ID : " + AxonalNeuron.NeuronID + " Layer Type :" + Layer.ToString());
                return ConnectionRemovalReturnType.TRUE;
            }

            return ConnectionRemovalReturnType.TRUE;
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

        private void CompleteCleanUP()
        {
            //DebugCheck();
            if(Mode <= LogMode.BurstOnly) 
                WriteLogsToFile(" TRACE :: CompleteCleanUp() :: Module was predicted but never Fired! May be Pooling Layers Need to update there pattern!  Performing Complete Clean Up!" + PrintBlockDetailsSingleLine());

            foreach (var col in Columns)
            {
                foreach (var neuron in col.Neurons)
                {
                    if (neuron.Voltage != 0 || neuron.CurrentState != NeuronState.RESTING)
                    {
                        neuron.FlushVoltage(CycleNum);
                    }
                }
            }

            NeuronsFiringLastCycle.Clear();
            PredictedNeuronsfromLastCycle.Clear();
            PredictedNeuronsforThisCycle.Clear();
            PredictedNeuronsForNextCycle.Clear();
            apicalContributors.Clear();
            temporalContributors.Clear();
        }

        private void DebugCheck()
        {
            foreach (var col in Columns)
            {
                foreach (var neuron in col.Neurons)
                {
                    if (neuron.Voltage != 0)
                    {
                        bool bp = true;    // if this hits , there are more clean up bugs in your code.
                    }
                }
            }
        }

        private List<Neuron> GetSpikingNeuronList()
        {
            List<Neuron> spikingNeuronList = new List<Neuron>();

            foreach (var col in Columns)
            {
                foreach (var neuron in col.Neurons)
                {
                    if (neuron.CurrentState == NeuronState.SPIKING)
                    {
                        spikingNeuronList.Add(neuron);
                    }
                }
            }

            return spikingNeuronList;
        }

        public List<Position_SOM> GetAnySpikeTrainNeuronsThisCycle()
        {
            List<Position_SOM> spikingNeurons = new List<Position_SOM>();

            foreach (var col in Columns)
            {
                foreach (var neuron in col.Neurons)
                {
                    if (neuron.CurrentState == NeuronState.SPIKING)
                    {
                        spikingNeurons.Add(neuron.NeuronID);
                    }
                }
            }

            return spikingNeurons;
        }

        #endregion

        #endregion

        #region PRIVATE METHODS

        private List<Neuron> ConvertDictToList(Dictionary<string, List<Neuron>> predictedNeuronsforLastCycle)
        {
            List<Neuron> toRet = new List<Neuron>();

            foreach (var kvp in predictedNeuronsforLastCycle)
            {
                toRet.Add(GetNeuronFromString(kvp.Key));
            }

            return toRet;
        }

        private bool CheckForDuplicates(List<Position_SOM> activeBits, Position_SOM neuronID)
        {
            if (activeBits.Any(pos => pos.X == neuronID.X && pos.Y == neuronID.Y))
            {
                return false;
            }

            return true;
        }

        private void BuildSequenceMemory()
        {
            foreach (var axonalneuron in NeuronsFiringLastCycle)
            {
                foreach (var dendronalNeuron in NeuronsFiringThisCycle)
                {
                    if (CheckifBothNeuronsAreSameOrintheSameColumn(axonalneuron, dendronalNeuron) == false)
                        if (ConnectTwoNeurons(axonalneuron, dendronalNeuron, ConnectionType.DISTALDENDRITICNEURON) == ConnectionRemovalReturnType.HARDFALSE)
                        {
                            throw new InvalidOperationException("Could Not connect two neuron!");
                        }
                }
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

        private bool ValidateInput(SDR_SOM incomingPattern, ulong currentCycle)
        {
            if (SupportedLabels.Count == 0 && NetWorkMode == NetworkMode.PREDICTION)
            {
                throw new InvalidOperationException(
                    " SOM Layer : " +  Layer.ToString() + "  : Have Not Learned enough to Classify Predictions!");
            }

            if (incomingPattern.ActiveBits.Count == 0)
            {
                Console.WriteLine("EXCEPTION :: Incoming Pattern cannot be empty");
                WriteLogsToFile("EXCEPTION :: Incoming Pattern cannot be empty" + PrintBlockDetailsSingleLine());
                throw new InvalidOperationException(" Incoming Pattern cannot be empty!");
            }

            if ((PreviousiType.Equals(iType.APICAL) && incomingPattern.InputPatternType == iType.APICAL) || (PreviousiType.Equals(iType.TEMPORAL) && incomingPattern.InputPatternType == iType.TEMPORAL))
            {
                if (CycleNum < currentCycle)
                {
                    ApicalCycleCache.Clear();
                    TemporalCycleCache.Clear();
                    CompleteCleanUP();
                }
                else
                {
                    WriteLogsToFile("EXCEPTION :: ValidateInput() ::  Cannot Process same depolarizing Inputs Simultaneously! " + PrintBlockDetailsSingleLine());
                    //CompleteCleanUP();
                    throw new InvalidOperationException("Cannot Process same depolarizing Inputs Simultaneously!");
                }
            }

            for (int i = 0; i < incomingPattern.ActiveBits.Count; i++)
            {
                int x = incomingPattern.ActiveBits[i].X;
                int y = incomingPattern.ActiveBits[i].Y;

                for (int j = 0; j < incomingPattern.ActiveBits.Count; j++)
                {
                    if (i != j)
                    {
                        if (incomingPattern.ActiveBits[j].X == x && incomingPattern.ActiveBits[j].Y == y)
                        {
                            throw new InvalidDataException("Cannot house the same Row and Column Number twice in one SDR! Please Check Your Input!");
                        }
                    }
                }
            }

            if (incomingPattern.InputPatternType.Equals(iType.SPATIAL))
            {

                if (currentCycle <= CycleNum && currentCycle != 0)
                {
                    throw new InvalidOperationException("Invalid Cycle Number");
                }

                CycleNum = currentCycle;

                foreach (var pos in incomingPattern.ActiveBits)
                {
                    if (pos.W == 'W')
                    {
                        WriteLogsToFile("EXCEPTION :: ValidateInput :: Incoming input has temporal and apical positions whil input Pattern Type is spatial");
                        throw new InvalidCastException("Incoming input has temporal and apical positions whil input Pattern Type is spatial");
                    }
                }


                if (PreviousiType.Equals(iType.APICAL) || PreviousiType.Equals(iType.TEMPORAL))
                {
                    bool cachceCleanup = false;

                    if (TemporalCycleCache.Count == 1)
                    {
                        foreach (var kvp in TemporalCycleCache)
                        {
                            if (CycleNum - kvp.Key > 3)
                            {
                                Console.WriteLine("ERROR :: ValidateInput  :: Temporal Cached Pattern is older than Spatial Pattern! Temporal Miss , Cleaning Up Deolarized Neurons" + PrintBlockDetailsSingleLine());
                                WriteLogsToFile("ERROR :: ValidateInput :: Temporal Cached Pattern is older than Spatial Pattern! Temporal Miss , Cleaning Up Deolarized Neurons" + PrintBlockDetailsSingleLine());

                                cachceCleanup = true;

                                foreach (var pos in kvp.Value)
                                {
                                    //BUG : if the neuron is depolarized from a previous sequence memory fire from a neiughbhouring neuron , cleaning up this would clean up that voltage as well!
                                    foreach (var synapse in TemporalLineArray[pos.X, pos.Y].AxonalList.Values)
                                    {
                                        if (synapse.DendronalNeuronalId != null)
                                        {
                                            var neuronToCleanUp = GetNeuronFromString(synapse.DendronalNeuronalId);

                                            if (neuronToCleanUp.Voltage != 0 && neuronToCleanUp.CurrentState != NeuronState.SPIKING)
                                            {
                                                neuronToCleanUp.FlushVoltage(CycleNum);
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        if (cachceCleanup)
                            TemporalCycleCache.Clear();
                    }
                    else if (TemporalCycleCache.Count > 1)
                    {
                        Console.WriteLine("ERROR :: ValidateInput() :: TemporalCycle Cache count is more than 1 , It should always be 1" + PrintBlockDetailsSingleLine());

                        throw new InvalidOperationException("TemporalCycle Cache Size should always be 1");
                    }

                    cachceCleanup = false;

                    if (ApicalCycleCache.Count == 1)
                    {
                        foreach (var kvp in ApicalCycleCache)
                        {
                            if (CycleNum - kvp.Key > 3)
                            {
                                Console.WriteLine("ERROR :: ValidateInput :: Apical Cached Pattern is older than Spatial Pattern! APCIAL MISSS :: Cleaning Up Depolarized Neurons " + PrintBlockDetailsSingleLine());
                                WriteLogsToFile("ERROR :: ValidateInput :: Apical Cached Pattern is older than Spatial Pattern! APCIAL MISSS :: Cleaning Up Depolarized  " + PrintBlockDetailsSingleLine());
                                cachceCleanup = true;

                                //BUG : if the neuron is depolarized from a previous sequence memory fire from a neiughbhouring neuron , cleaning up this would clean up that voltage as well!
                                foreach (var pos in kvp.Value)
                                {
                                    foreach (var synapse in ApicalLineArray[pos.X, pos.Y].AxonalList.Values)
                                    {
                                        if (synapse.DendronalNeuronalId != null) // && BBMUtils.CheckNeuronListHasThisNeuron(NeuronsFiringThisCycle, synapse.DendronalNeuronalId) == false)
                                        {
                                            var neuronToCleanUp = GetNeuronFromString(synapse.DendronalNeuronalId);

                                            if (neuronToCleanUp.Voltage != 0 && neuronToCleanUp.CurrentState != NeuronState.SPIKING)
                                            {
                                                neuronToCleanUp.FlushVoltage(currentCycle);
                                            }
                                            else if (neuronToCleanUp.CurrentState != NeuronState.SPIKING)
                                            {
                                                Console.WriteLine("WARNING :: PostCycleCleanUp ::: Tried to clean up a neuron which was not depolarized!!! " + PrintBlockDetailsSingleLine());
                                            }
                                        }
                                    }
                                }

                            }
                        }

                        if (cachceCleanup)
                            ApicalCycleCache.Clear();
                    }
                    else if (ApicalCycleCache.Count > 1)
                    {
                        Console.WriteLine("ERROR :: PostCycleCleanUp() :: Apical Cache count is more than 1 , It should always be 1 " + PrintBlockDetailsSingleLine());
                        throw new InvalidOperationException("TemporalCycle Cache Size should always be 1");
                    }

                }
            }
            if (incomingPattern.InputPatternType.Equals(iType.TEMPORAL))
            {
                if (TemporalCycleCache.Count != 0)
                {
                    Console.WriteLine("ERROR :: Fire() :::: Trying to Add Temporal Pattern to a Valid cache Item! " + PrintBlockDetailsSingleLine());
                    WriteLogsToFile("ERROR :: Fire() :::: Trying to Add Temporal Pattern to a Valid cache Item! " + PrintBlockDetailsSingleLine());
                    //Thread.Sleep(2000);
                }

                var posList = TransformTemporalCoordinatesToSpatialCoordinates(incomingPattern.ActiveBits as List<Position_SOM>);

                foreach (var input in posList)
                {
                    if (input.NeuronID.Y >= Y || input.NeuronID.Z >= Z)   //Verified!
                    {
                        throw new InvalidOperationException("EXCEPTION :: Invalid Data for Temporal Pattern exceding bounds!");
                    }
                }

                TemporalCycleCache.Add(CycleNum, TransformTemporalCoordinatesToSpatialCoordinates1(incomingPattern.ActiveBits));
            }
            else if (incomingPattern.InputPatternType.Equals(iType.APICAL))
            {
                if (ApicalCycleCache.Count != 0)
                {
                    Console.WriteLine("ERROR :: Fire() :::: Trying to Add Apical Pattern to a Valid cache Item!" + PrintBlockDetailsSingleLine());
                    WriteLogsToFile("ERROR :: Fire() :::: Trying to Add Apical Pattern to a Valid cache Item!" + PrintBlockDetailsSingleLine());
                    //Thread.Sleep(2000);
                }

                foreach (var pos in incomingPattern.ActiveBits)
                {
                    if (pos.X > X || pos.Y > Y || pos.Z > Z)
                    {
                        Console.WriteLine("EXCEPTION :: Incoming pattern is not encoded in the correct format");

                        throw new InvalidDataException("Incoming SDR is not encoded correctly");
                    }
                }

                ApicalCycleCache.Add(CycleNum, incomingPattern.ActiveBits);
            }

            return true;
        }

        private void Prune()
        {

            for (int i = 0; i < X; i++)
            {
                for (int j = 0; j < Y; j++)
                {
                    foreach (var neuron in Columns[i, j].Neurons)
                    {
                        //if (neuron.NeuronID.ToString() == "2-8-0-N" || neuron.NeuronID.ToString() == "5-1-7-N")
                        //{
                        //    bool bp = true;
                        //}

                        if (neuron.ProximoDistalDendriticList == null || neuron.ProximoDistalDendriticList.Count == 0)
                        { return; }

                        List<string> DremoveList = null;
                        List<string> AremoveList = null;

                        var staleConnections = neuron.CheckForPrunableConnections(CycleNum);

                        if ((neuron.ProximoDistalDendriticList.Count > SOMLTOTAL_NEURON_CONNECTIONLIMIT) && staleConnections.Count != 0)
                        {
                            WriteLogsToFile(" PRUNE ERROR : Neuron is connecting too much , need to debug and see why these many connection requests are coming in the first place!" + neuron.NeuronID.ToString());
                            OverConnectedInShortInterval.Add(neuron);
                        }

                        if (staleConnections.Count != 0)
                        {
                            foreach (var synapse in staleConnections)
                            {
                                //Remove Distal Dendrite from Neuron
                                if (DremoveList == null)
                                {
                                    DremoveList = new List<string>();
                                }

                                //Remove Corresponding Connected Axonal Neuron
                                var axonalNeuron = GetNeuronFromString(synapse.AxonalNeuronId);

                                if (axonalNeuron.AxonalList.TryGetValue(neuron.NeuronID.ToString(), out var connection))
                                {
                                    var result = axonalNeuron.RemoveAxonalConnection(neuron);

                                    if (result == ConnectionRemovalReturnType.TRUE)
                                    {
                                        DremoveList.Add(synapse.AxonalNeuronId);
                                    }
                                    if (result == ConnectionRemovalReturnType.HARDFALSE)
                                    {
                                        Console.WriteLine(" EXCEPTION : Could not remove connected Axonal Neuron");
                                        throw new InvalidOperationException(" Couldnt find the prunning axonal connection on the deleted dendritic connection while Prunning");
                                    }
                                    else if (result == ConnectionRemovalReturnType.SOFTFALSE)
                                    {
                                        DremoveList.Add(synapse.AxonalNeuronId);           //Done this way as C# does not allow to change entities it is iterating over in foreach loop above. line :1613.

                                        WriteLogsToFile(" ERROR :: Attempting to Remove Schema invoked AXON TO NEURON Connection while connection two Neurons :: Layer :::: " + Layer.ToString());
                                    }
                                }
                                else
                                {
                                    if (Mode.Equals(LogMode.None) == false)
                                    {
                                        //Axonal is not connected dendronal via regular connection ! Potential Scehma Based Neuronal Prune ! Ignore!!.
                                    }
                                }

                            }

                            if (DremoveList?.Count > 0)
                            {
                                neuron.IncrementPruneCount();

                                for (int k = 0; k < DremoveList.Count; k++)
                                {
                                    neuron.ProximoDistalDendriticList.Remove(DremoveList[k]);

                                    BlockBehaviourManagerSOM.totalDendronalConnections--;
                                }
                            }
                        }
                    }
                }
            }
        }

        private void PruneSingleNeuron(Neuron prunableNeuron)
        {
            if (prunableNeuron != null && prunableNeuron.nType.Equals(NeuronType.NORMAL))
            {
                if (prunableNeuron.NeuronID.ToString() == "2-8-0-N" || prunableNeuron.NeuronID.ToString() == "5-1-7-N")
                {
                    bool bp = true;
                }

                if (prunableNeuron.ProximoDistalDendriticList == null || prunableNeuron.ProximoDistalDendriticList.Count == 0)
                { return; }

                List<string> DremoveList = null;
                List<string> AremoveList = null;

                var staleConnections = prunableNeuron.CheckForPrunableConnections(CycleNum);

                if ((prunableNeuron.ProximoDistalDendriticList.Count > SOMLTOTAL_NEURON_CONNECTIONLIMIT) && staleConnections.Count != 0)
                {
                    WriteLogsToFile(" PRUNE ERROR : Neuron is connecting too much , need to debug and see why these many connection requests are coming in the first place!" + prunableNeuron.NeuronID.ToString());
                    OverConnectedInShortInterval.Add(prunableNeuron);
                }

                //Remove only connected Distal Dendritic connections
                if (staleConnections.Count != 0)
                {
                    foreach (var synapse in staleConnections)
                    {
                        //Remove Distal Dendrite from Neuron
                        if (DremoveList == null)
                            DremoveList = new List<string>();

                        //Remove Corresponding Connected Axonal Neuron
                        var axonalNeuron = GetNeuronFromString(synapse.AxonalNeuronId);

                        if (axonalNeuron.AxonalList.TryGetValue(prunableNeuron.NeuronID.ToString(), out var connection))
                        {
                            var returnType = axonalNeuron.RemoveAxonalConnection(prunableNeuron);

                            if (returnType == ConnectionRemovalReturnType.TRUE)
                            {
                                DremoveList.Add(synapse.AxonalNeuronId);
                            }
                            else if (returnType == ConnectionRemovalReturnType.SOFTFALSE)
                            {
                                WriteLogsToFile(" ERROR :: Attempting to Remove Schema invoked AXON TO NEURON Connection while connection two Neurons");
                                throw new InvalidOperationException("Attempting to Remove Schema invoked AXON TO NEURON Connection while connection two Neurons! Should Never Happen!");
                            }
                            else if (returnType == ConnectionRemovalReturnType.HARDFALSE)
                            {
                                Console.WriteLine(" ERROR :: Axonal Connection Does Not Exist But Dendronal Connection Does! ");
                                WriteLogsToFile(" ERROR :: Axonal Connection Does Not Exist But Dendronal Connection Does! ");
                                throw new InvalidOperationException("Neuronal Network Structure Is Compromised ! Cannot pursue any further Layer Type :: " + Layer.ToString() + " BBM ID : " + BBMID.ToString());
                            }
                        }
                        else
                        {
                            Console.WriteLine("WARNING :::: PRUNE():: Axonal Neuron does not contain Same synapse from Dendronal Neuron for Prunning!");
                            WriteLogsToFile("WARNING :::: PRUNE():: Axonal Neuron does not contain Same synapse from Dendronal Neuron for Prunning!");
                        }
                    }

                    if (Mode == LogMode.All || Mode == LogMode.Info)
                    {
                        Console.WriteLine("INFO : Succesfully removed " + DremoveList.Count.ToString() + " neurons from neuron " + prunableNeuron.NeuronID.ToString() + "Layer Type : " + Layer.ToString());
                        WriteLogsToFile("INFO : Succesfully removed " + DremoveList.Count.ToString() + " neurons from neuron " + prunableNeuron.NeuronID.ToString() + "Layer Type : " + Layer.ToString());
                    }

                    if (DremoveList?.Count > 0)
                    {
                        for (int k = 0; k < DremoveList.Count; k++)
                        {
                            prunableNeuron.ProximoDistalDendriticList.Remove(DremoveList[k]);

                            BlockBehaviourManagerSOM.totalDendronalConnections--;
                        }
                    }
                }


                return;
            }
        }

        private void PreCyclePrep(ulong incomingCycle, iType itype)
        {

            if (incomingCycle - CycleNum > 1 && _firingBlankStreak >= NUMBER_OF_ALLOWED_MAX_BLACNK_FIRES_BEFORE_CLEANUP)
            {
                foreach (var neuron in NeuronsFiringLastCycle)
                {
                    if (neuron.Voltage == 0)
                    {
                        throw new InvalidOperationException("If voltage is zero they should not be");
                    }
                }
            }

            //Prepare all the neurons that are predicted 
            if (PredictedNeuronsForNextCycle.Count != 0 && NeuronsFiringThisCycle.Count != 0)
            {
                Console.WriteLine(schemToLoad.ToString() + "Precycle Cleanup Error : _predictedNeuronsForNextCycle is not empty");
                throw new Exception("PreCycle Cleanup Exception!!!");
            }

            if (itype.Equals(iType.SPATIAL) && incomingCycle < CycleNum)
            {
                throw new InvalidOperationException("BBM cannot be ahead of Cycle!");
            }

            if (CurrentiType.Equals(BlockCycle.CLEANUP))
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

            //if (NeuronFiringNextCycle.Count != 0)
            //{
            //    foreach (var kvp in NeuronFiringNextCycle)
            //    {
            //        var neuron = GetNeuronFromString(kvp.Key);

            //        if (neuron.CurrentState == NeuronState.FIRING || neuron.CurrentState == NeuronState.SPIKING)
            //        {
            //            NeuronsFiringThisCycle.Add(neuron);
            //        }
            //        else
            //        {
            //            throw new InvalidOperationException("Neuron is cleaned up after it was added to the postFireList ! Needs investigation");
            //        }
            //    }

            //    NeuronFiringNextCycle.Clear();
            //}

            NumberOfColumnsThatBurstLastCycle = 0;

            NumberOfColumnsThatFiredThisCycle = 0;

            IsBurstOnly = false;

            SupportedLabels.Clear();

        }

        private void PrintBlockDetails()
        {
            Console.WriteLine("BBM ID : " + BBMID);
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
            PramoteCorrectlyPredictedDendronal(GetNeuronFromString(neuron.GetMyTemporalPartner1()), neuron);
        }

        private void StrengthenApicalConnection(Neuron neuron)
        {
            PramoteCorrectlyPredictedDendronal(GetNeuronFromString(neuron.GetMyApicalPartner1()), neuron);
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
                    Console.WriteLine("ERROR :: PramoteCorrectlyPredictionDendronal: Trying to increment strength on a synapse object that was null!!!");
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
                        this.TemporalLineArray[i, j] = new Neuron(new Position_SOM(0, i, j, 'T'), BBMID, NeuronType.TEMPORAL);


                    for (int k = 0; k < X; k++)
                    {

                        if (ConnectTwoNeurons(this.TemporalLineArray[i, j], Columns[k, i].Neurons[j], ConnectionType.TEMPRORAL, true) == ConnectionRemovalReturnType.HARDFALSE)
                        {
                            throw new InvalidOperationException("Unable to connect two neurons!");
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
                    this.ApicalLineArray[i, j] = new Neuron(new Position_SOM(i, j, 0, 'A'), BBMID, NeuronType.APICAL);

                    for (int k = 0; k < Z; k++)
                    {
                        if (ConnectTwoNeurons(this.ApicalLineArray[i, j], Columns[i, j].Neurons[k], ConnectionType.APICAL, true) == ConnectionRemovalReturnType.HARDFALSE)
                        {
                            throw new InvalidOperationException("Unable to connect two neurons!");
                        }
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

            if (X == 1250 && Y == 10 && Z == 5)
            {
                schemToLoad = SchemaType.SOMSCHEMA_VISION;
            }
            else if(X == 200 && Y == 10 && Z == 5)
            {
                schemToLoad = SchemaType.SOMSCHEMA_TEXT;
            }

            XmlDocument document = new XmlDocument();

            string dendriteDocumentPath = null;

            if (schemToLoad == SchemaType.INVALID)
            {
                throw new InvalidOperationException("Schema Type Cannot be Invalid");
            }

            if (schemToLoad == SchemaType.SOMSCHEMA_VISION)
            {
                dendriteDocumentPath = "C:\\Users\\depint\\source\\repos\\Hentul\\SecondOrderMemory\\Schema Docs\\1K Club\\DendriticSchemaSOM.xml";
            }
            else if(schemToLoad == SchemaType.SOMSCHEMA_TEXT)
            {
                dendriteDocumentPath = "C:\\\\Users\\\\depint\\\\source\\\\repos\\\\Hentul\\\\SecondOrderMemory\\\\Schema Docs\\\\1K Club\\\\Text\\\\DendriticSchemaSOM.xml";
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


            if (X == 1250 && Y == 10 && Z == 5)
            {
                schemToLoad = SchemaType.SOMSCHEMA_VISION;
            }
            else if(X == 200 &&  Y == 10 && Z == 5)
            {
                schemToLoad = SchemaType.SOMSCHEMA_TEXT;
            }

            XmlDocument document = new XmlDocument();

            string axonalDocumentPath = null;

            if (schemToLoad == SchemaType.INVALID)
            {
                throw new InvalidOperationException("Schema Type Cannot be Invalid");
            }


            if (schemToLoad == SchemaType.SOMSCHEMA_VISION)
            {
                axonalDocumentPath = "C:\\Users\\depint\\source\\repos\\Hentul\\SecondOrderMemory\\Schema Docs\\1K Club\\AxonalSchema-SOM.xml";
            }
            else if (schemToLoad == SchemaType.SOMSCHEMA_TEXT)
            {
                axonalDocumentPath = "C:\\\\Users\\\\depint\\\\source\\\\repos\\\\Hentul\\\\SecondOrderMemory\\\\Schema Docs\\\\1K Club\\\\Text\\\\AxonalSchemaSOM.xml";
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

        private void WriteLogsToFile(string logline)
        {
            File.AppendAllText(logfilename, logline + "\n");

        }

        public void LearnNewObject(string objectName)
        {
            CurrentObjectLabel = objectName;
        }

        #endregion

        #region ENUMS    

        public enum BlockCycle
        {
            INITIALIZATION,
            DEPOLARIZATION,
            POLOARIZED,
            FIRING,
            CLEANUP
        }

        #endregion
    }
}


#region Experimental Code

//public void LTP(SDR_SOM feedbackSignal)
//{

//    /* FUNCTION : Recognise new connections made while recognising the new objects which did not exist before.
//     * Q's :
//     * What connections should be specifically strengthened ? and to what level should each connection be strengthened ?
//     * Apical Connections will only be strengthend , Should Temporal location signals should also be LTP'd ? No.
//     * 
//    ALGO :: 
//    1. Maintain a Delta of all the new connections that has happened for the brief period of time which truly lead to the discovery of the new object.
//    2. If LTP is called that means this new batch was effective in recognising the object so strengthen these new connections.

//    IMPLEMENTATION :
//    1. Run through the SDR and wire up the incoming apical connection with the respective neuron.
//    2. Both Apical SDR corresponds to the existing neuronal structure , No transformations needed.
//    3.  
//    4. Need UT, CTs, & SVTs  for verifyign these connections are true and correct

//    */

//    throw new NotImplementedException();

//}

#endregion