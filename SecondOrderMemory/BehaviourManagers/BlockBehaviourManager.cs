using Bond;
using SecondOrderMemory.Models;
using Common;

namespace SecondOrderMemory.BehaviourManagers
{
    public class BlockBehaviourManager 
    {
        public ulong CycleNum { get; private set; }

        private int NumColumns;

        public Dictionary<string, List<string>> PredictedNeuronsForNextCycle { get; private set; }

        public Dictionary<string, List<string>> PredictedNeuronsfromLastCycle { get; private set; }

        public List<Neuron> NeuronsFiringThisCycle { get; private set; }

        public List<Neuron> NeuronsFiringLastCycle { get; private set; }

        private List<Segment>? _predictedSegmentForThisCycle;

        public List<Position_SOM> ColumnsThatBurst { get; private set; }

        private List<Neuron> temporalContributors { get; set; }

        private List<Neuron> apicalContributors { get; set; }

        public Column[,] Columns { get; private set; }

        public Neuron[,] TemporalLineArray { get; private set; }

        public Neuron[,] ApicalLineArray { get; private set; }

        public uint totalProximalConnections;

        public uint totalAxonalConnections;

        private bool IsApical;

        private bool isTemporal;

        public bool IsSpatial;

        private static BlockBehaviourManager _blockBehaviourManager;

        public static BlockBehaviourManager GetBlockBehaviourManager(int numColumns = 10)
        {
            if (BlockBehaviourManager._blockBehaviourManager == null)
            {
                BlockBehaviourManager._blockBehaviourManager = new BlockBehaviourManager(numColumns);
            }

            return BlockBehaviourManager._blockBehaviourManager;
        }               
        

        public BlockBehaviourManager(int numColumns = 10)
        {
            this.CycleNum = 0;

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

            totalProximalConnections = 0;

            totalAxonalConnections = 0;

            isTemporal = false;

            IsApical = false;

            IsSpatial = false;

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

            Connector.GetConnector().ReadDendriticSchema(x, y);

            Connector.GetConnector().ReadAxonalSchema(x, y);

            GenerateTemporalLines();
            
            GenerateApicalLines();

        }

        public BlockBehaviourManager CloneBBM(BlockBehaviourManager bbm)
        {
            BlockBehaviourManager toReturn;

            toReturn = new BlockBehaviourManager(bbm.NumColumns);

            try
            {
                for (int i = 0; i < bbm.NumColumns; i++)
                {
                    for (int j = 0; j < bbm.NumColumns; j++)
                    {
                        for (int k = 0; k < bbm.NumColumns; k++)
                        {
                            //Proximal Dendritic Connections
                            Neuron presynapticNeuron, postSynapticNeuron;

                            for(int l=0; l< bbm.Columns[i, j].Neurons[k].dendriticList.Values.Count; l++)
                            {
                                var synapse = bbm.Columns[i, j].Neurons[k].dendriticList.Values.ElementAt(l);

                                if (synapse != null)
                                {
                                    presynapticNeuron = toReturn.ConvertStringPosToNeuron(synapse.AxonalNeuronId);
                                    postSynapticNeuron = toReturn.ConvertStringPosToNeuron(synapse.DendronalNeuronalId);

                                    toReturn.ConnectTwoNeurons(presynapticNeuron, postSynapticNeuron, ConnectionType.PRXOMALDENDRITETONEURON);
                                }
                                else
                                {
                                    throw new InvalidOperationException("Synapse Came Up Empty in Clone Logic");
                                }
                            }


                            //Axonal Connections
                            for(int l = 0; l < bbm.Columns[i, j].Neurons[k].dendriticList.Values.Count; l++)
                            {
                                var synapse = bbm.Columns[i, j].Neurons[k].AxonalList.Values.ElementAt(l);

                                if (synapse != null)
                                {

                                    presynapticNeuron = toReturn.ConvertStringPosToNeuron(synapse.AxonalNeuronId);
                                    postSynapticNeuron = toReturn.ConvertStringPosToNeuron(synapse.DendronalNeuronalId);

                                    toReturn.ConnectTwoNeurons(presynapticNeuron, postSynapticNeuron, ConnectionType.AXONTONEURON);
                                }
                                else
                                {
                                    throw new InvalidOperationException("Synapse Came Up Empty in Clone Logic");
                                }
                            }
                        }
                    }
                }
            }
            catch(Exception e)
            {
                int bb = 1;
            }

            toReturn.GenerateTemporalLines();

            toReturn.GenerateApicalLines();

            return toReturn;
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
            List<Neuron> neuronsFiringThisCycle = new List<Neuron>();

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
                            var predictedNeuronPositioons = Columns[incomingPattern.ActiveBits[i].X, incomingPattern.ActiveBits[i].Y].GetPredictedNeuronsFromColumn();

                            if (predictedNeuronPositioons?.Count == Columns[0, 0].Neurons.Count)
                            {//burst
                                neuronsFiringThisCycle.AddRange(Columns[incomingPattern.ActiveBits[i].X, incomingPattern.ActiveBits[i].Y].Neurons);
                                ColumnsThatBurst.Add(incomingPattern.ActiveBits[i]);
                            }
                            else
                            {//selected predicted neurons
                                neuronsFiringThisCycle.AddRange(predictedNeuronPositioons);
                            }

                            predictedNeuronPositioons = null;
                        }

                        foreach (var neuron in neuronsFiringThisCycle)
                        {
                            neuron.Fire();
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
                                temporalNeuron.Fire();

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
                            apicalLineNeurons.Add(_blockBehaviourManager.ApicalLineArray[pos.X, pos.Y]);
                        }

                        if (ApicalLineArray != null && apicalLineNeurons.Count != 0)
                        {
                            foreach (var apicalNeuron in apicalLineNeurons)
                            {
                                apicalNeuron.Fire();

                                apicalContributors.Add(apicalNeuron);
                            }
                        }

                        break;
                    }
            }

            if(IsSpatial == true)
                Wire();

            if(isTemporal == false && IsApical == false)
            PostCycleCleanup();
        }       

        private void Wire()
        {

            //Get intersection of neuronsFiringThisCycle and predictedNeuronsfromLastCycleCycle

            if (IsSpatial)
            {
                List<Neuron> predictedNeuronList = new List<Neuron>();

                foreach (var item in PredictedNeuronsfromLastCycle.Keys)
                {
                    var neuronToAdd = BlockBehaviourManager.GetBlockBehaviourManager().ConvertStringPosToNeuron(item);

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
                            correctlyPredictedNeuron.PramoteCorrectPredictionDendronal(BlockBehaviourManager.GetBlockBehaviourManager().ConvertStringPosToNeuron(contributingNeuron));
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
                                neuron.PramoteCorrectPredictionDendronal(temporalContributor);
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
                            neuron.PramoteCorrectPredictionDendronal(apicalContributor);
                        }
                    }
                }

                IsApical = false;
            }


            //Every 50 Cycles Prune unused and under Firing Connections
            if (this.CycleNum > 3000000 && this.CycleNum % 50 == 0)
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

                    if (TemporalLineArray[i, j] == null)
                        TemporalLineArray[i, j] = new Neuron(new Position_SOM(0, i, j, 'T'), NeuronType.TEMPORAL);

                    for (int k = 0; k < NumColumns; k++)
                    {                        
                        ConnectTwoNeurons(TemporalLineArray[i, j], Columns[k, i].Neurons[j], ConnectionType.TEMPRORAL);
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
                    ApicalLineArray[i, j] = new Neuron(new Position_SOM(i, j, 0, 'A'), NeuronType.APICAL);

                    for (int k = 0; k < NumColumns; k++)
                    {
                        ConnectTwoNeurons(ApicalLineArray[i, j], Columns[i, j].Neurons[k], ConnectionType.APICAL);
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
                temporalNeurons.Add(TemporalLineArray[position.Y, position.X]);             
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
            if (NeuronsFiringThisCycle.Where(n => n.NeuronID.Equals(neuron.NeuronID)).Count() == 0)
                NeuronsFiringThisCycle.Add(neuron);
        }

        public static void InitAxonalConnectionForConnector(int x, int y, int z, int i, int j, int k)
        {
            if (x == i && y == j && z == k)
            {
                throw new InvalidDataException();
            }
            try
            {
                Column col = GetBlockBehaviourManager().Columns[x, y];

                Neuron neuron = col.Neurons[z];

                neuron.InitAxonalConnectionForConnector(i, j, k);

                BlockBehaviourManager.IncrementAxonalConnectionCount();
                
            }
            catch (Exception ex)
            {

                Console.WriteLine(ex.ToString());

                int breakpoint = 1;
            }
        }

        public static void InitDendriticConnectionForConnector(int x, int y, int z, int i, int j, int k)
        {
            if (x == i && y == j && z == k)
            {
                int breakpoint = 1;
            }
            try
            {
                Column col = GetBlockBehaviourManager().Columns[x, y];

                Neuron neuron = col.Neurons[z];

                neuron.InitProximalConnectionForDendriticConnection(i, j, k);

                BlockBehaviourManager.IncrementProximalConnectionCount();

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

            return AxonalNeuron.AddtoAxonalList(DendriticNeuron.NeuronID.ToString(), cType) && DendriticNeuron.AddToDistalList(AxonalNeuron.NeuronID.ToString(), cType);            
        }

        public Neuron GetNeuronFromPosition(char w, int x, int y, int z)
        {
            if (w == 'N')
            {
                return _blockBehaviourManager.Columns[x, y].Neurons[z];
            }
            else if (w == 'T')
            {
                return _blockBehaviourManager.TemporalLineArray[y, z];
            }            
            else if(w == 'A')
            {
                return _blockBehaviourManager.ApicalLineArray[x, y];
            }

            return null;
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

            throw new DataMisalignedException("ConvertStringPosToNeuron : Couldnt Find the neuron in the columns");

        }

        private static void IncrementProximalConnectionCount()
        {
            BlockBehaviourManager.GetBlockBehaviourManager().totalProximalConnections++;
        }

        private static void IncrementAxonalConnectionCount()
        {
            BlockBehaviourManager.GetBlockBehaviourManager().totalAxonalConnections++;
        }        
    }
}
