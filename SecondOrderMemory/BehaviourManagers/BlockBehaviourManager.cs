using Bond;
using SecondOrderMemory.Models;
using Common;
using System.Xml;

namespace SecondOrderMemory.BehaviourManagers
{

    public class BlockBehaviourManager 
    {
        public  static ulong CycleNum { get; private set; }

        public int NumColumns { get; private set; }

        private Position_SOM BlockID;

        public Dictionary<string, List<string>> PredictedNeuronsForNextCycle { get; private set; }

        public Dictionary<string, List<string>> PredictedNeuronsfromLastCycle { get; private set; }

        public List<Neuron> NeuronsFiringThisCycle { get; private set; }

        public List<Neuron> NeuronsFiringLastCycle { get; private set; }

        private List<Segment>? _predictedSegmentForThisCycle;

        public List<Position_SOM> ColumnsThatBurst { get; private set; }

        private List<Neuron> temporalContributors { get; set; }

        private List<Neuron> apicalContributors { get; set; }

        public Column[,] Columns { get; private set; }

        public  Neuron[,] TemporalLineArray { get; private set; }

        public Neuron[,] ApicalLineArray { get; private set; }

        public Dictionary<string, int[]> DendriticCache { get; private set; }

        public Dictionary<string, int[]> AxonalCache { get; private set; }

        private static int neuronCounter;

        public uint totalProximalConnections;

        public uint totalAxonalConnections;

        private bool IsApical;

        private bool isTemporal;

        public bool IsSpatial;

        #region CONSTANTS
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

        public BlockBehaviourManager(int x, int y, int z, int numColumns = 10)
        {
            this.BlockID = new Position_SOM(x, y, z);

            CycleNum = 0;

            this.NumColumns = numColumns;

            PredictedNeuronsfromLastCycle = new Dictionary<string, List<string>>();

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

            DendriticCache = new Dictionary<string, int[]>();

            AxonalCache = new Dictionary<string, int[]>();

            totalProximalConnections = 0;

            totalAxonalConnections = 0;

            isTemporal = false;

            IsApical = false;

            IsSpatial = false;

            neuronCounter = 0;

            for (int i = 0; i < numColumns; i++)
            {
                for (int j = 0; j < numColumns; j++)
                {
                    Columns[i, j] = new Column(i, j, numColumns);
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

        public BlockBehaviourManager CloneBBM(int x, int y, int z)
        {
            BlockBehaviourManager toReturn;

            toReturn = new BlockBehaviourManager(x,y,z,NumColumns);

            toReturn.Init();

            return toReturn;

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

        }


        private void PreCyclePrep()
        {
            //Prepare all the neurons that are predicted 
            if (PredictedNeuronsForNextCycle.Count != 0 && NeuronsFiringThisCycle.Count != 0)
            {
                Console.WriteLine("Precycle Cleanup Error : _predictedNeuronsForNextCycle is not empty");
                throw new Exception("PreCycle Cleanup Exception!!!");
            }

            for (int i = 0; i < NumColumns; i++)
                for (int j = 0; j < NumColumns; j++)
                {
                    if (Columns[i, j].PreCleanupCheck())
                    {
                        Console.WriteLine("Precycle Cleanup Error : Column {0} {1} is pre Firing", i, j);
                        throw new Exception("PreCycle Cleanup Exception!!!");
                    }
                }
            ColumnsThatBurst.Clear();
        }

        public void Fire(SDR_SOM incomingPattern, bool ignorePrecyclePrep = false)
        {            

            if(!ignorePrecyclePrep)
                PreCyclePrep();

            if (incomingPattern.ActiveBits.Count == 0)
                return;

            switch(incomingPattern.InputPatternType)
            {
                case iType.SPATIAL:
                    {                        
                        for (int i = 0; i < incomingPattern.ActiveBits.Count; i++)
                        {
                            var predictedNeuronPositions = Columns[incomingPattern.ActiveBits[i].X, incomingPattern.ActiveBits[i].Y].GetPredictedNeuronsFromColumn();

                            if (predictedNeuronPositions?.Count == Columns[0, 0].Neurons.Count)
                            {
                                Console.WriteLine( "Block ID : " + BlockID.ToString() + " New Pattern Coming in ... Bursting New Neuronal Firings Count : " + predictedNeuronPositions.Count.ToString());
                                NeuronsFiringThisCycle.AddRange(Columns[incomingPattern.ActiveBits[i].X, incomingPattern.ActiveBits[i].Y].Neurons);
                                ColumnsThatBurst.Add(incomingPattern.ActiveBits[i]);
                            }
                            else
                            {
                                Console.WriteLine("Block ID :::: " + BlockID.ToString() + " :: Old  Pattern : Predicting Predicted Neurons Count : " + NeuronsFiringThisCycle.Count.ToString());
                                NeuronsFiringThisCycle.AddRange(predictedNeuronPositions);
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

                        if(temporalLineNeurons.Count != 0)
                        {
                            foreach(var temporalNeuron in temporalLineNeurons)
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

                        foreach(var pos in incomingPattern.ActiveBits)
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
            }

            Fire();

            if (IsSpatial == true)
                Wire();

            if(isTemporal == false && IsApical == false)
            PostCycleCleanup();
        }

        private void Fire()
        {
            foreach (var neuron in NeuronsFiringThisCycle)
            {
                neuron.Fire();

                foreach (Synapse synapse in neuron.AxonalList.Values)
                {
                    ProcessSpikeFromNeuron(ConvertStringPosToNeuron(synapse.AxonalNeuronId), ConvertStringPosToNeuron(synapse.DendronalNeuronalId) , synapse.cType);
                }                
            }
        }

        public void ProcessSpikeFromNeuron(Neuron sourceNeuron, Neuron targetNeuron, ConnectionType cType = ConnectionType.PROXIMALDENDRITICNEURON)
        {
            uint multiplier = 1;

            if (targetNeuron.NeuronID.ToString().Equals("2-4-2-N"))
            {
                bool breakpoint = false;
                breakpoint = true;
            }

            targetNeuron.ChangeCurrentStateTo(NeuronState.PREDICTED);

            AddPredictedNeuron(targetNeuron, sourceNeuron.NeuronID.ToString());

            if (cType.Equals(ConnectionType.TEMPRORAL) || cType.Equals(ConnectionType.APICAL))
            {
                if (! targetNeuron.TAContributors.TryGetValue(sourceNeuron.NeuronID.ToString(), out char w))
                {
                    if (cType.Equals(ConnectionType.TEMPRORAL))
                    {
                        targetNeuron.TAContributors.Add(sourceNeuron.NeuronID.ToString(), 'T');
                    }
                    else if (cType.Equals(ConnectionType.APICAL))
                    {
                        targetNeuron.TAContributors.Add(sourceNeuron.NeuronID.ToString(), 'A');
                    }
                }
                else
                {
                    bool breakpoint = false;
                    breakpoint = true;
                }
            }

            if (targetNeuron.dendriticList.TryGetValue(sourceNeuron.NeuronID.ToString(), out var synapse))
            {
                multiplier += synapse.GetStrength();

                switch (synapse.cType)
                {
                    case ConnectionType.DISTALDENDRITICNEURON:
                        targetNeuron.ProcessVoltage(DISTAL_VOLTAGE_SPIKE_VALUE);
                        break;
                    case ConnectionType.PROXIMALDENDRITICNEURON:
                        targetNeuron.ProcessVoltage(PROXIMAL_VOLTAGE_SPIKE_VALUE);
                        break;
                    case ConnectionType.TEMPRORAL:
                        targetNeuron.ProcessVoltage(TEMPORAL_NEURON_FIRE_VALUE);
                        break;
                    case ConnectionType.APICAL:
                        targetNeuron.ProcessVoltage(APICAL_NEURONAL_FIRE_VALUE);
                        break;
                    case ConnectionType.NMDATONEURON:
                        targetNeuron.ProcessVoltage(NMDA_NEURONAL_FIRE_VALUE);
                        break;
                }
            }
            else if (cType.Equals(ConnectionType.AXONTONEURON))
            {
                targetNeuron.ProcessVoltage(PROXIMAL_AXON_TO_NEURON_FIRE_VALUE);
            }
            else
            {
                throw new InvalidOperationException("ProcessSpikeFormNeuron : Trying to Process Spike from Neuron which is not connected to this Neuron");
            }
        }


        public void StrengthenTemporalConnection(Neuron neuron)
        {
            PramoteCorrectPredictionDendronal(ConvertStringPosToNeuron(neuron.GetMyTemporalPartner()), neuron);
        }

        public void StrengthenApicalConnection(Neuron neuron)
        {
            PramoteCorrectPredictionDendronal(ConvertStringPosToNeuron(neuron.GetMyApicalPartner()), neuron);
        }

        private void PramoteCorrectPredictionDendronal(Neuron contributingNeuron, Neuron targetNeuron)
        {
            if (targetNeuron.dendriticList.Count == 0)
            {
                throw new Exception("Not Supposed to Happen : Trying to Pramote connection on a neuron , not connected yet!");
            }

            if (targetNeuron.dendriticList.TryGetValue(contributingNeuron.NeuronID.ToString(), out Synapse synapse))
            {
                if (synapse == null)
                {
                    Console.WriteLine("PramoteCorrectPredictionDendronal: Trying to increment strength on a synapse object that was null!!!");
                    throw new InvalidOperationException("Not Supposed to happen!");
                }

                Console.WriteLine("SOM :: Pramoting Correctly Predicted Dendronal Connections");

                synapse.IncrementStrength();
            }
        }

        private void Wire()
        {

            //Get intersection of neuronsFiringThisCycle and predictedNeuronsfromLastCycleCycle

            if (IsSpatial)
            {
                List<Neuron> predictedNeuronList = new List<Neuron>();

                foreach (var item in PredictedNeuronsfromLastCycle.Keys)
                {
                    var neuronToAdd = ConvertStringPosToNeuron(item);

                    if (neuronToAdd != null)
                    {
                        predictedNeuronList.Add(neuronToAdd);
                    }
                };

                var correctPredictionList = NeuronsFiringThisCycle.Intersect(predictedNeuronList).ToList<Neuron>();


                //Total New Pattern : None of the predicted neurons Fired                 

                if (correctPredictionList.Count == 0 || ColumnsThatBurst.Count != 0)
                {
                    //Todo:
                    //How to wire Bursting Columns ?
                    //Figure out what neurons fired in the last cycle and try to wire them
                    foreach (var dendriticNeuronItem in NeuronsFiringThisCycle)
                    {
                        foreach (var axonalNeuronItem in NeuronsFiringLastCycle)
                        {
                            //Connect last cycle firing neuronal axons this cycle firing dendrites
                            Console.WriteLine("SOM :: Total New Pattern , Bursting");
                            ConnectTwoNeurons(axonalNeuronItem, dendriticNeuronItem, ConnectionType.AXONTONEURON);
                        }
                    }

                }

                //Else PramoteCorrectlyPredictedConnections
                foreach (var correctlyPredictedNeuron in correctPredictionList)
                {
                    List<string> contributingList;
                    if (PredictedNeuronsfromLastCycle.TryGetValue(correctlyPredictedNeuron.NeuronID.ToString(), out contributingList))
                    {
                        if (contributingList.Count == 0)
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

                IsSpatial = false;
            }

            if(isTemporal)
            {
                //Get intersection between temporal input SDR and the firing Neurons if any fired and strengthen it
                foreach(var neuron in NeuronsFiringThisCycle)
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

            if(IsApical)
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


            //Every 50 Cycles Prune unused and under Firing Connections
            if (BlockBehaviourManager.CycleNum > 3000000 && BlockBehaviourManager.CycleNum % 50 == 0)
            {
                foreach (var col in this.Columns)
                {
                    col.PruneCycleRefresh();
                }
            }

            if(IsSpatial == false && isTemporal == false && IsApical == false)
            {
                PostCycleCleanup();
            }
            
            // Todo: Check if neurons that all fired together are connected to each other or not and connect them!   
        }

       

       

        private void PostCycleCleanup()
        {
            //clean up all the fired columns
            foreach (var column in Columns)
            {
                column.PostCycleCleanup();
            }

            //Prepare the predicted list for next cycle Fire 

            foreach (var kvp in PredictedNeuronsForNextCycle)
            {
                PredictedNeuronsfromLastCycle[kvp.Key] = kvp.Value;
            }

            PredictedNeuronsForNextCycle.Clear();

            NeuronsFiringLastCycle.Clear();

            foreach (var item in NeuronsFiringThisCycle)
            {
                NeuronsFiringLastCycle.Add(item);
            }

            NeuronsFiringThisCycle.Clear();

            CycleNum++;
            // Process Next pattern.          
        }


        private void GenerateTemporalLines()
        {
            // T : (x,y, z) => (0,y,x)
       
            for(int i=0; i<NumColumns; i++)
            {
                for (int j = 0; j < NumColumns; j++)
                {

                    if (this.TemporalLineArray[i, j] == null)
                        this.TemporalLineArray[i, j] = new Neuron(new Position_SOM(0, i, j, 'T'), NeuronType.TEMPORAL);

                    for (int k = 0; k < NumColumns; k++)
                    {                        
                        ConnectTwoNeurons(this.TemporalLineArray[i, j], Columns[k, i].Neurons[j], ConnectionType.TEMPRORAL);
                    }
                }
            }
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
                for (int i = 0; i < this.NumColumns; i++)
                {
                    toReturn.Add(this.Columns[i, position.Y].Neurons[position.X]);
                }
            }

            return toReturn;
        }

        public void AddNeuronToCurrentFiringCycle(Neuron neuron)
        {
            if (this.NeuronsFiringThisCycle.Where(n => n.NeuronID.Equals(neuron.NeuronID)).Count() == 0)
                NeuronsFiringThisCycle.Add(neuron);
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
            if (x == i && y == j && z == k)
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

                int breakpoint = 1;
            }
        }
       
        public void AddPredictedNeuron(Neuron predictedNeuron, string contributingNeuron)
        {
            List<string> contributingList = null;
            if (PredictedNeuronsForNextCycle.Count > 0 && PredictedNeuronsForNextCycle.TryGetValue(predictedNeuron.NeuronID.ToString(), out contributingList))
            {
                if (contributingList != null)
                {
                    contributingList.Add(contributingNeuron);
                }
                else
                {
                    contributingList = new List<string>
                    {
                        contributingNeuron
                    };
                }
            }
            else
            {
                PredictedNeuronsForNextCycle.Add(predictedNeuron.NeuronID.ToString(), new List<string>() { contributingNeuron });
            }
        }

        public void AddtoPredictedNeuronFromLastCycleMock(Neuron neuronToAdd, Neuron contributingNeuron)
        {
           
            PredictedNeuronsfromLastCycle.Add(neuronToAdd.NeuronID.ToString(), new List<string>() { contributingNeuron.NeuronID.ToString()});
        }

        public bool ConnectTwoNeurons(Neuron AxonalNeuron, Neuron DendriticNeuron, ConnectionType cType)
        {
            if(cType == null)
            {
                bool breakpoint = false;
                breakpoint = true;
            }

            if (AxonalNeuron == null || DendriticNeuron == null)
                return false;

            if(AxonalNeuron.NeuronID.Equals(DendriticNeuron.NeuronID) && AxonalNeuron.nType.Equals(DendriticNeuron.nType))
            {
                Console.WriteLine("ConnectTwoNeurons : Cannot Connect Neuron to itself!");
                throw new InvalidDataException("CoonectTwoNeurons: Cannot connect Neuron to Itself!");
                //return false;
            }

            return AxonalNeuron.AddtoAxonalList(DendriticNeuron.NeuronID.ToString(), AxonalNeuron.nType, cType) && DendriticNeuron.AddToDistalList(AxonalNeuron.NeuronID.ToString(), DendriticNeuron.nType, cType);            
        }

        public Neuron GetNeuronFromPosition(char w, int x, int y, int z)
        {
            Neuron toRetun = null;

            if (w == 'N')
            {
                toRetun = Columns[x, y].Neurons[z];
            }
            else if (w == 'T')
            {
                toRetun =  TemporalLineArray[y, z];
            }            
            else if(w == 'A')
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

            try
            {
                if (parts.Length != 3 || x > 9 || y > 9 || z > 9)
                {
                    int breakpoint = 1;
                }

                return GetNeuronFromPosition(nType, x, y, z);

            }
            catch (Exception e)
            {
                int bp = 1;
            }

            throw new NullReferenceException("ConvertStringPosToNeuron : Couldnt Find the neuron in the columns Block ID : " + BlockID.ToString() + " : posString :  " + posString);

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

            //if (DendriticCache.Count != 0)
            //{
            //    foreach (var item in DendriticCache)
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

            //        InitDendriticConnectionForConnector(i, j, k, item.Value[0], item.Value[1], item.Value[2]);
            //        InitDendriticConnectionForConnector(i, j, k, item.Value[0 + offset * 1], item.Value[1 + offset * 1], item.Value[2 + offset * 1]);
            //        InitDendriticConnectionForConnector(i, j, k, item.Value[0 + offset * 2], item.Value[1 + offset * 2], item.Value[2 + offset * 2]);
            //        InitDendriticConnectionForConnector(i, j, k, item.Value[0 + offset * 3], item.Value[1 + offset * 3], item.Value[2 + offset * 3]);

            //        //Console.WriteLine("SOM :: Adding connection from Cache : Column X :" + intX.ToString() + " Column Y : " + intY.ToString() + " Dendritic A :" + i.ToString() + " B: " + j.ToString() + " C :" + k.ToString());
            //    }

            //    return;
            //}

            #region REAL Code

            XmlDocument document = new XmlDocument();
            string dendriteDocumentPath = "C:\\Users\\depint\\source\\repos\\SecondOrderMemory\\Schema Docs\\ConnectorSchema.xml";  //"C:\\Users\\depint\\Desktop\\Hentul\\SecondOrderMemory\\Schema Docs\\ConnectorSchema.xml";  


            if (!File.Exists(dendriteDocumentPath))
            {
                throw new FileNotFoundException(dendriteDocumentPath);
            }

            document.Load(dendriteDocumentPath);

            using (XmlNodeList columns = document.GetElementsByTagName("Column"))
            {

                var numColumns = columns.Count;

                for (int i = 0; i < numColumns; i++)
                {//Column
                    neuronCounter = 0;

                    var item = columns[i];

                    int x = Convert.ToInt32(item.Attributes[0]?.Value);
                    var y = Convert.ToInt32(item.Attributes[1]?.Value);

                    //if(x == 0 && y == 1) 
                    //{
                    //    int breakpoint = 0;
                    //}

                    Columns[x, y].Init++;

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

                        //int[] arr = new int[neuronNodes.Count * 3];
                        //int index = 0;

                        //4 Proximal Dendronal Connections
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

                            //Console.WriteLine("SOM :: Adding Connection from Schema :  Column X :" + intX.ToString() + " Column Y : " + intY.ToString() + " Dendritic A :" + a.ToString() + " B: " + b.ToString() + " C :" + c.ToString());

                            //arr[index++] = e;
                            //arr[index++] = f;
                            //arr[index++] = g;
                        }

                        string key = a.ToString() + "-" + b.ToString() + "-" + c.ToString();

                        //if (!DendriticCache.TryGetValue(key, out var conn))
                        //{
                        //    DendriticCache.Add(key, arr);
                        //}
                        //else
                        //{
                        //    Console.WriteLine("ERROR :: AddDendriticSchema : Should not be Trying to add invalid cache entry for the same neuron");
                        //}

                        neuronCounter++;
                    }
                }
            }

            #endregion
        }

        public void ReadAxonalSchema(int intX, int intY)
        {
            #region Cache : Real Code
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

            string axonalDocumentPath = "C:\\Users\\depint\\source\\repos\\SecondOrderMemory\\Schema Docs\\AxonalSchema.xml"; //"C:\\Users\\depint\\Desktop\\Hentul\\SecondOrderMemory\\Schema Docs\\AxonalSchema.xml";  //

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

                //int[] arr = new int[axonList.Count * 3];
                //int index = 0;

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

                        InitAxonalConnectionForConnector(x, y, z, i, j, k);

                        //arr[index++] = i;
                        //arr[index++] = j;
                        //arr[index++] = k;

                        //Console.WriteLine("New Connection From Schema Doc", x, y, z, i, j, k);
                    }
                    catch (Exception e)
                    {
                        int bp = 1;
                        throw new InvalidDataException("ReadAXonalSchema : Invalid Data , Tryign to add new Connections to cache");
                    }

                    neuronCounter++;
                }

                //string key = x.ToString() + "-" + y.ToString() + "-" + z.ToString();

                //if (!AxonalCache.TryGetValue(key, out var conn))
                //{
                //    AxonalCache.Add(key, arr);
                //}
                //else
                //{
                //    Console.WriteLine("AddAxonalSchema : Should not be Trying to add invalid cache entry for the same neuron");
                //}               
            }
        }

    }
}
