namespace SecondOrderMemory.BehaviourManagers
{
    using SecondOrderMemory.Models;
    using Common;
    using System.Xml;

    public class SBBManager
    {

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
        private int TOTAL_ALLOWED_BURST_PER_CLEANUP = 1;

        #endregion


        #region VARIABLES

        public static ulong CycleNum { get; private set; }

        public int NumColumnsX { get; private set; }

        public int NumColumnsY { get; private set; }

        public int NumRowsZ { get; private set;  } 

        private Position_SOM BlockID;

        public Dictionary<string, List<string>> PredictedNeuronsForNextCycle { get; private set; }

        public Dictionary<string, List<string>> PredictedNeuronsforThisCycle { get; private set; }

        public List<Neuron> NeuronsFiringThisCycle { get; private set; }

        public List<Neuron> NeuronsFiringLastCycle { get; private set; }

        private List<Segment>? _predictedSegmentForThisCycle;

        public List<Position_SOM> ColumnsThatBurst { get; private set; }

        private int NumberOfColumsnThatFiredThisCycle { get; set; }

        private List<Neuron> temporalContributors { get; set; }

        private List<Neuron> apicalContributors { get; set; }

        public Column[,] Columns { get; private set; }        

        public Neuron[,] TemporalLineArray { get; private set; }

        public Neuron[,] ApicalLineArray { get; private set; }

        public List<Neuron> SpikeTrainList { get; private set; }

        public uint TotalDistalDendriticConnections;

        public uint TotalBurstFire;

        public uint TotalPredictionFires;

        //public Dictionary<string, int[]> DendriticCache { get; private set; }

        //public Dictionary<string, int[]> AxonalCache { get; private set; }

        public static int axonCounter { get; private set; }

        public uint totalProximalConnections;

        public static uint totalDendronalConnections;

        public uint totalAxonalConnections;

        private uint num_continuous_burst;

        private bool IsApical;

        private bool isTemporal;

        public bool IsSpatial;

        private bool IsBurstOnly;

        private const bool devbox = false;

        private const int BlockOffset = 10;

        #endregion      

        #region CONSTRUCTORS & INITIALIZATIONS 

        public SBBManager(int numColumnsX = 100, int  numColumnsY= 10, int numRowsZ = 10, int x=0, int y=0, int z=0)
        {
            this.BlockID = new Position_SOM(x, y, z);

            this.NumberOfColumsnThatFiredThisCycle = 0;

            CycleNum = 0;

            totalDendronalConnections = 0;

            this.NumColumnsX = numColumnsX;

            this.NumColumnsY = numColumnsY;

            this.NumRowsZ = numRowsZ;

            PredictedNeuronsforThisCycle = new Dictionary<string, List<string>>();

            //_predictedSegmentForThisCycle = new List<Segment>();
            PredictedNeuronsForNextCycle = new Dictionary<string, List<string>>();

            NeuronsFiringThisCycle = new List<Neuron>();

            NeuronsFiringLastCycle = new List<Neuron>();

            temporalContributors = new List<Neuron>();

            apicalContributors = new List<Neuron>();

            TemporalLineArray = new Neuron[NumColumnsY, NumRowsZ];

            ApicalLineArray = new Neuron[NumColumnsX, NumColumnsY];

            Columns = new Column[NumColumnsX, NumColumnsY];

            ColumnsThatBurst = new List<Position_SOM>();

            SpikeTrainList = new List<Neuron>();

            //DendriticCache = new Dictionary<string, int[]>();

            //AxonalCache = new Dictionary<string, int[]>();

            totalProximalConnections = 0;

            totalAxonalConnections = 0;

            isTemporal = false;

            IsApical = false;

            IsSpatial = false;

            IsBurstOnly = false;

            axonCounter = 0;

            num_continuous_burst = 0;

            for (int i = 0; i < NumColumnsX; i++)
            {
                for (int j = 0; j < NumColumnsY; j++)
                {

                    Columns[i, j] = new Column(i, j, NumRowsZ);
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

                int breakpoint = 1;
            }
        }

        public void InitDendriticConnectionForConnector(int x, int y, int z, int i, int j, int k)
        {
            if (x == i && y == j && z == k || (z > NumRowsZ))
            {
                int breakpoint = 1;
                throw new InvalidDataException("InitDendriticConnectionForConnector :: Invalid Dendritic Schema ");
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

                int breakpoint = 1;
            }
        }

        public SBBManager CloneBBM(int x, int y, int z)
        {
            SBBManager toReturn;

            toReturn = new SBBManager(NumColumnsX, NumColumnsY, NumRowsZ);

            toReturn.Init();

            return toReturn;

            #region Cache Code for Connector
            //try
            //{
            //    for (int i = 0; i < NumColumns; i++)
            //    {
            //        for (int j = 0; j < NumColumns; j++)
            //        {
            //            for (int k = 0; k < NumColumns; k++)
            //            {
            //                //Proximal Dendritic Connections
            //                Neuron presynapticNeuron, postSynapticNeuron;

            //                for(int l=0; l< Columns[i, j].Neurons[k].dendriticList.Values.Count; l++)
            //                {
            //                    var synapse = Columns[i, j].Neurons[k].dendriticList.Values.ElementAt(l);

            //                    if (synapse != null)
            //                    {
            //                        if (synapse.cType.Equals(ConnectionType.PROXIMALDENDRITICNEURON))
            //                        {
            //                            presynapticNeuron = toReturn.ConvertStringPosToNeuron(synapse.AxonalNeuronId);
            //                            postSynapticNeuron = toReturn.ConvertStringPosToNeuron(synapse.DendronalNeuronalId);

            //                            if (!toReturn.ConnectTwoNeurons(presynapticNeuron, postSynapticNeuron, ConnectionType.PROXIMALDENDRITICNEURON))
            //                            {
            //                                Console.WriteLine("Could Not Clone Distal Connection Properly!!!");
            //                            }
            //                        }
            //                    }
            //                    else
            //                    {
            //                        throw new InvalidOperationException("Synapse Came Up Empty in Clone Logic");
            //                    }
            //                }


            //                //Axonal Connections
            //                for (int l = 0; l < Columns[i, j].Neurons[k].AxonalList.Values.Count; l++)
            //                {
            //                    var synapse = Columns[i, j].Neurons[k].AxonalList.Values.ElementAt(l);

            //                    if (synapse != null)
            //                    {
            //                        if (synapse.cType.Equals(ConnectionType.AXONTONEURON))
            //                        {
            //                            presynapticNeuron = toReturn.ConvertStringPosToNeuron(synapse.AxonalNeuronId);
            //                            postSynapticNeuron = toReturn.ConvertStringPosToNeuron(synapse.DendronalNeuronalId);

            //                            if (!toReturn.ConnectTwoNeurons(presynapticNeuron, postSynapticNeuron, ConnectionType.AXONTONEURON))
            //                            {
            //                                Console.WriteLine("Could Not CLone Axonal Connection Properly!!!");
            //                            }
            //                        }
            //                    }
            //                    else
            //                    {
            //                        throw new InvalidOperationException("Synapse Came Up Empty in Clone Logic");
            //                    }
            //                }
            //            }
            //        }
            //    }

            //toReturn.GenerateTemporalLines();

            //toReturn.GenerateApicalLines();

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

        #region PUBLIC METHODS 

        #region PREP -> FIRE -> WIRE -> CLEANUP -> REPEAT

        private void PreCyclePrep()
        {
            //Prepare all the neurons that are predicted 
            if (PredictedNeuronsForNextCycle.Count != 0 && NeuronsFiringThisCycle.Count != 0)
            {
                Console.WriteLine("Precycle Cleanup Error : _predictedNeuronsForNextCycle is not empty");
                throw new Exception("PreCycle Cleanup Exception!!!");
            }

            for (int i = 0; i < NumColumnsX; i++)
                for (int j = 0; j < NumColumnsY; j++)
                {
                    if (Columns[i, j].PreCleanupCheck())
                    {
                        Console.WriteLine("Precycle Cleanup Error : Column {0} {1} is pre Firing", i, j);
                        throw new Exception("PreCycle Cleanup Exception!!!");
                    }
                }

            NumberOfColumsnThatFiredThisCycle = 0;

            IsBurstOnly = false;
        }
                
        public SDR_SOM Fire(SDR_SOM incomingPattern, bool ignorePrecyclePrep = false, bool ignorePostCycleCleanUp = false)
        {
            // Todo : If there is a burst and there is any neuron in any of the columns the fired in the last cycle that has a connection to the bursting column. Column CheckPointing.

            if (ignorePrecyclePrep == false)
                PreCyclePrep();

            if (incomingPattern.ActiveBits.Count == 0)
                return null;

            NumberOfColumsnThatFiredThisCycle = incomingPattern.ActiveBits.Count;

            switch (incomingPattern.InputPatternType)
            {
                case iType.SPATIAL:
                    {
                        for (int i = 0; i < incomingPattern.ActiveBits.Count; i++)
                        {
                            var predictedNeuronPositions = Columns[incomingPattern.ActiveBits[i].X, incomingPattern.ActiveBits[i].Y].GetPredictedNeuronsFromColumn();

                            if (predictedNeuronPositions?.Count == Columns[0, 0].Neurons.Count)
                            {
                                Console.WriteLine("Block ID : " + BlockID.ToString() + " New Pattern Coming in ... Bursting New Neuronal Firings Count : " + predictedNeuronPositions.Count.ToString());

                                NeuronsFiringThisCycle.AddRange(Columns[incomingPattern.ActiveBits[i].X, incomingPattern.ActiveBits[i].Y].Neurons);

                                ColumnsThatBurst.Add(incomingPattern.ActiveBits[i]);

                                IsBurstOnly = true;
                                
                                num_continuous_burst++;

                                TotalBurstFire++;
                            }
                            else if (predictedNeuronPositions.Count == 1)
                            {
                                Console.WriteLine("Block ID : " + BlockID.ToString() + " Old  Pattern : Predicting Predicted Neurons Count : " + predictedNeuronPositions.Count.ToString());

                                NeuronsFiringThisCycle.AddRange(predictedNeuronPositions);

                                TotalPredictionFires++;
                            }
                            else
                            {

                                Console.WriteLine("There Should only be one winner in the Column");

                            }
                            predictedNeuronPositions = null;
                        }

                        IsSpatial = true;

                        break;
                    }
                case iType.TEMPORAL:
                    {
                        isTemporal = true;

                        List<Neuron> temporalLineNeurons = TransformTemporalCoordinatesToSpatialCoordinates(incomingPattern.ActiveBits as List<Position_SOM>);

                        if (temporalLineNeurons.Count != 0)
                        {
                            foreach (var temporalNeuron in temporalLineNeurons)
                            {
                                NeuronsFiringThisCycle.Add(temporalNeuron);

                                temporalContributors.Add(temporalNeuron);
                            }
                        }

                        break;
                    }
                case iType.APICAL:
                    {
                        IsApical = true;

                        List<Neuron> apicalLineNeurons = new List<Neuron>();

                        foreach (var pos in incomingPattern.ActiveBits)
                        {
                            apicalLineNeurons.Add(this.ApicalLineArray[pos.X, pos.Y]);
                        }

                        if (ApicalLineArray != null && apicalLineNeurons.Count != 0)
                        {
                            foreach (var apicalNeuron in apicalLineNeurons)
                            {
                                NeuronsFiringThisCycle.Add(apicalNeuron);

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


            FireLocal();

            if (IsSpatial == true)
            {
                Wire();
            }

            if ((IsSpatial == false && IsApical == false) || ignorePostCycleCleanUp == false)
                PostCycleCleanup();

            if (SpikeTrainList.Any())
            {
                return GetPredictedSDR();
            }

            return null;
        }       

        private void FireLocal()
        {
            foreach (var neuron in NeuronsFiringThisCycle)
            {
                //check if the synapse is active only then fire
                neuron.Fire();

                foreach (Synapse synapse in neuron.AxonalList.Values)
                {
                    ProcessSpikeFromNeuron(ConvertStringPosToNeuron(synapse.AxonalNeuronId), ConvertStringPosToNeuron(synapse.DendronalNeuronalId), synapse.cType);
                }
            }
        }

        private void Wire()
        {
            if (IsSpatial)
            {

                ///Case 1 : All Predicted Neurons Fire : Strengthen only the correct predictions.
                ///Case 2 : Few Fired , Few Bursted : Strengthen the Correctly Fired Neurons , For Bursted , Analyse did anybody contribut to the column and dint burst ? if nobody contributed then do X
                ///Case 3 : All columns Bursted : highly likely first fire or totally new pattern coming in , If firing early cycle , then just wire minimum strength for the connections and move on , if in the middle of the cycle( atleast 10,000 cycle ) then Need to do somethign new Todo .

                //Get intersection of neuronsFiringThisCycle and predictedNEuronsFromThisCycle as if any neurons that were predicted for this cycle actually fired then we got to strengthen those connections first

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
                if (ColumnsThatBurst.Count == 0 && correctPredictionList.Count != 0 && correctPredictionList.Count == NumberOfColumsnThatFiredThisCycle)
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

                                PramoteCorrectPredictionDendronal(ConvertStringPosToNeuron(contributingNeuron), correctlyPredictedNeuron);
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

                                PramoteCorrectPredictionDendronal(ConvertStringPosToNeuron(contributingNeuron), correctlyPredictedNeuron);
                            }
                        }
                    }


                    //Boost the Bursting neurons
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
                }// ColumnsThatBurst.Count == 0 && correctPredictionList.Count = 5 &&  NumberOfColumnsThatFiredThisCycle = 8  cycleNum = 4 , repNum = 29
                else if (ColumnsThatBurst.Count == 0 && NumberOfColumsnThatFiredThisCycle > correctPredictionList.Count)
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

                                PramoteCorrectPredictionDendronal(ConvertStringPosToNeuron(contributingNeuron), correctlyPredictedNeuron);
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
                                ConnectTwoNeuronsOrIncrementStrength(axonalNeuron, dendriticNeuron, ConnectionType.DISTALDENDRITICNEURON);
                            }
                        }
                    }

                    // Fired But Did Not Get Predicted
                    foreach (var axonalneuron in NeuronsFiringLastCycle)
                    {
                        foreach (var dendronalNeuron in NeuronsFiringThisCycle)
                        {
                            ConnectTwoNeuronsOrIncrementStrength(axonalneuron, dendronalNeuron, ConnectionType.DISTALDENDRITICNEURON);
                        }
                    }
                }
                else if (ColumnsThatBurst.Count == NumberOfColumsnThatFiredThisCycle && correctPredictionList.Count == 0)
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
                else
                {
                    throw new NotImplementedException("This should never happen or the code has bugs! Get on it Biiiiiyaaattttcccchhhhhhhh!!!!!");
                }

                IsSpatial = false;

            }
            else if (isTemporal)
            {
                //Get intersection between temporal input SDR and the firing Neurons if any fired and strengthen it
                foreach (var neuron in NeuronsFiringThisCycle)
                {
                    if (neuron.nType.Equals(NeuronType.NORMAL))
                    {
                        foreach (var temporalContributor in temporalContributors)
                        {
                            if (neuron.DidItContribute(temporalContributor))
                            {
                                PramoteCorrectPredictionDendronal(temporalContributor, neuron);
                            }
                        }
                    }
                }

                isTemporal = false;
            }
            else if (IsApical)
            {
                //Get intersection between temporal input SDR and the firing Neurons if any fired and strengthen it

                foreach (var neuron in NeuronsFiringThisCycle)
                {
                    foreach (var apicalContributor in apicalContributors)
                    {
                        if (neuron.DidItContribute(apicalContributor))
                        {
                            PramoteCorrectPredictionDendronal(apicalContributor, neuron);
                        }
                    }
                }

                IsApical = false;
            }
        }

        public void PrintBlockStats()
        {
            if (TotalDistalDendriticConnections > 0 || TotalBurstFire > 0 || TotalPredictionFires > 0)
                Console.WriteLine("Block ID : " + BlockID.ToString());
            if (TotalDistalDendriticConnections > 0)
                Console.WriteLine("Total DISTAL Dendronal Connections : " + TotalDistalDendriticConnections.ToString());
            if (TotalBurstFire > 0)
                Console.WriteLine("Total BURST FIRE COUNT : " + TotalBurstFire.ToString());
            if (TotalPredictionFires > 0)
                Console.WriteLine("Total CORRECT PREDICTIONS : " + TotalPredictionFires.ToString());

        }

        public Neuron GetNeuronFromPosition(char w, int x, int y, int z)
        {
            Neuron toRetun = null;

            if (z >= this.NumRowsZ)
            {
                int breakpoint = 1;
            }

            if (w == 'N')
            {
                toRetun = Columns[x, y].Neurons[z];
            }
            else if (w == 'T')
            {
                toRetun = TemporalLineArray[y, z];
            }
            else if (w == 'A')
            {
                toRetun = ApicalLineArray[x, y];
            }

            if (toRetun == null)
            {
                int bp = 1;
                throw new InvalidOperationException("Your Column structure is messed up!!!");
            }

            return toRetun;
        }

        public bool ConnectTwoNeuronsOrIncrementStrength(Neuron AxonalNeuron, Neuron DendriticNeuron, ConnectionType cType)
        {
            if (cType == null)
            {
                bool breakpoint = false;
                breakpoint = true;
            }

            if (AxonalNeuron == null || DendriticNeuron == null)
                return false;

            if (((AxonalNeuron.NeuronID.X == DendriticNeuron.NeuronID.X && AxonalNeuron.NeuronID.Y == DendriticNeuron.NeuronID.Y) || AxonalNeuron.NeuronID.Equals(DendriticNeuron.NeuronID)) && AxonalNeuron.nType.Equals(DendriticNeuron.nType))
            {
                Console.WriteLine("ConnectTwoNeurons : Cannot Connect Neuron to itself!");

                //throw new InvalidDataException("CoonectTwoNeurons: Cannot connect Neuron to Itself!");
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

            return new SDR_SOM(NumColumnsX, NumColumnsY, ActiveBits, iType.SPATIAL);
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

        #endregion

        #region SOM METHODS
        
        internal void AnySpikesYet()
        {

            //Has Decision Been Made , Any Particular Train Spiking Neurons detected yet ?
            //Goal of this layer is to be on the lookout for temporal and spatial pattern till we generate a Spike Train indicating thats pretty sure it has detected something.

            foreach(var column in Columns)
            {
                foreach(var neuron in column.Neurons)
                {
                    if(neuron.CurrentState == NeuronState.SPIKING)
                    {
                        //Mark the spiking neuron and return true

                    }
                }
            }
        }

        #endregion

        #endregion

        #region PRIVATE METHODS

        //Todo
        private void ProcessSpikeFromNeuron(Neuron sourceNeuron, Neuron targetNeuron, ConnectionType cType = ConnectionType.PROXIMALDENDRITICNEURON)
        {

            if (targetNeuron.NeuronID.ToString().Equals("5-3-0-N") || targetNeuron.NeuronID.ToString().Equals("5-5-2-N") || targetNeuron.NeuronID.ToString().Equals("5-5-2-N"))
            {
                bool breakpoint = false;
                breakpoint = true;
            }

            targetNeuron.ChangeCurrentStateTo(NeuronState.PREDICTED);

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
                    bool breakpoint = false;
                    breakpoint = true;
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
                //else
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
                throw new InvalidOperationException("ProcessSpikeFormNeuron : Trying to Process Spike from Neuron which is not connected to this Neuron");
            }

            if (targetNeuron.CurrentState.Equals(NeuronState.NUTS_MODE))
            {

            }
        }

        private void StrengthenTemporalConnection(Neuron neuron)
        {
            PramoteCorrectPredictionDendronal(ConvertStringPosToNeuron(neuron.GetMyTemporalPartner()), neuron);
        }

        private void StrengthenApicalConnection(Neuron neuron)
        {
            PramoteCorrectPredictionDendronal(ConvertStringPosToNeuron(neuron.GetMyApicalPartner()), neuron);
        }

        private void PramoteCorrectPredictionDendronal(Neuron contributingNeuron, Neuron targetNeuron)
        {
            if (targetNeuron.ProximoDistalDendriticList.Count == 0)
            {
                throw new Exception("Not Supposed to Happen : Trying to Pramote connection on a neuron , not connected yet!");
            }

            if (targetNeuron.ProximoDistalDendriticList.TryGetValue(contributingNeuron.NeuronID.ToString(), out Synapse synapse))
            {
                if (synapse == null)
                {
                    Console.WriteLine("PramoteCorrectPredictionDendronal: Trying to increment strength on a synapse object that was null!!!");
                    throw new InvalidOperationException("Not Supposed to happen!");
                }

                Console.WriteLine("SOM :: Pramoting Correctly Predicted Dendronal Connections");

                synapse.IncrementHitCount();
            }
        }


        //Todo
        private void PostCycleCleanup()
        {
            //clean up all the fired columns if there is no apical or temporal signal
            if (!IsSpatial && !IsApical && !isTemporal)
            {
                foreach (var column in Columns)
                {
                    column.PostCycleCleanup();
                }
            }

            //Prepare the predicted list for next cycle Fire 

            PredictedNeuronsforThisCycle.Clear();

            foreach (var kvp in PredictedNeuronsForNextCycle)
            {
                PredictedNeuronsforThisCycle[kvp.Key] = kvp.Value;
            }

            PredictedNeuronsForNextCycle.Clear();

            NeuronsFiringLastCycle.Clear();

            foreach (var item in NeuronsFiringThisCycle)
            {
                NeuronsFiringLastCycle.Add(item);
            }

            //Every 50 Cycles Prune unused and under Firing Connections
            if (SBBManager.CycleNum >= 25 && SBBManager.CycleNum % 25 == 0)
            {
                foreach (var col in this.Columns)
                {
                    col.PruneCycleRefresh();
                }
            }

            //Feature : How many Burst Cycle to wait before performing a full clean ? Answer : 1

            if (num_continuous_burst > TOTAL_ALLOWED_BURST_PER_CLEANUP)
            {
                for (int i = 0; i < NumColumnsX; i++)
                {
                    for (int j = 0; j < NumColumnsY; j++)
                    {
                        for (int k = 0; k < NumRowsZ; k++)
                        {
                            if (!NeuronsFiringLastCycle.Where(x => x.NeuronID.X == i && x.NeuronID.Y == j && x.NeuronID.Z == k && x.nType == NeuronType.NORMAL).Any())
                            {
                                Columns[i, j].Neurons[k].FlushVoltage();
                            }
                        }
                    }
                }
            }

            IsBurstOnly = false;

            NeuronsFiringThisCycle.Clear();

            ColumnsThatBurst.Clear();

            CycleNum++;
            // Process Next pattern.          
        }


        private void GenerateTemporalLines()        //Todo
        {
            // T : (x,y, z) => (0,y,x)

            try
            {

                for (int i = 0; i < NumColumnsX; i++)
                {
                    for (int j = 0; j < NumColumnsY; j++)
                    {

                        if (this.TemporalLineArray[i, j] == null)
                            this.TemporalLineArray[i, j] = new Neuron(new Position_SOM(0, i, j, 'T'), NeuronType.TEMPORAL);

                        for (int k = 0; k < NumRowsZ; k++)
                        {
                            ConnectTwoNeuronsOrIncrementStrength(this.TemporalLineArray[i, j], Columns[k, i].Neurons[j], ConnectionType.TEMPRORAL);
                        }
                    }
                }
            }
            catch(Exception e)
            {
                int breakpoint = 1;
            }
        }   

        private void GenerateApicalLines()
        {
            try
            {
                for (int i = 0; i < NumColumnsX; i++)
                {
                    for (int j = 0; j < NumColumnsY; j++)
                    {
                        this.ApicalLineArray[i, j] = new Neuron(new Position_SOM(i, j, 0, 'A'), NeuronType.APICAL);

                        for (int k = 0; k < NumRowsZ; k++)
                        {
                            ConnectTwoNeuronsOrIncrementStrength(this.ApicalLineArray[i, j], Columns[i, j].Neurons[k], ConnectionType.APICAL);
                        }
                    }
                }
            }
            catch(Exception e)
            {
                int breakpoint = 1;
            }
        }


        //Todo
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

        private List<Neuron> TransformTemporalCoordinatesToSpatialCoordinates2(List<Position_SOM> positionLists)
        {
            List<Neuron> toReturn = new List<Neuron>();

            if (positionLists.Count == 0)
                return toReturn;

            toReturn = new List<Neuron>();

            foreach (var position in positionLists)
            {
                for (int i = 0; i < this.NumColumnsX; i++)
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

        //Todo
        private void ReadDendriticSchema(int intX, int intY)
        {

            #region REAL Code            

            XmlDocument document = new XmlDocument();            

            string dendriteDocumentPath;

            if (devbox)
            {
                dendriteDocumentPath = "C:\\Users\\depint\\Desktop\\Hentul\\SecondOrderMemory\\Schema Docs\\DendriticSchema.xml";
            }
            else
            {
                dendriteDocumentPath = "C:\\Users\\depint\\source\\repos\\Hentul\\SecondOrderMemory\\Schema Docs\\DendriticSchema.xml";
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

        //Todo
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
                axonalDocumentPath = "C:\\Users\\depint\\Desktop\\Hentul\\SecondOrderMemory\\Schema Docs\\AxonalSchema.xml";
            }
            else
            {
                axonalDocumentPath = "C:\\Users\\depint\\source\\repos\\Hentul\\SecondOrderMemory\\Schema Docs\\AxonalSchema.xml";
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

    }
}
