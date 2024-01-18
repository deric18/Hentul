using SecondOrderMemory.Models;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

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

        public List<Position> ColumnsThatBurst { get; private set; }

        private List<Neuron> temporalContributors { get; set; }

        private List<Neuron> apicalContributors { get; set; }

        public Column[,] Columns { get; private set; }

        public Neuron[,] TemporalLineArray { get; private set; }

        public Neuron[,] ApicalLineArray { get; private set; }

        public uint totalProximalConnections;

        public uint totalAxonalConnections;

        private bool IsApical;

        private bool isTemporal;

        public Connector connector { get; private set; }    

        private static BlockBehaviourManager _blockBehaviourManager;

        public static BlockBehaviourManager GetBlockBehaviourManager(int numColumns = 10)
        {
            if (BlockBehaviourManager._blockBehaviourManager == null)
            {
                BlockBehaviourManager._blockBehaviourManager = new BlockBehaviourManager(numColumns);
            }

            return BlockBehaviourManager._blockBehaviourManager;
        }

        private BlockBehaviourManager(int numColumns = 10)
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

            ColumnsThatBurst = new List<Position>();

            totalProximalConnections = 0;

            totalAxonalConnections = 0;

            isTemporal = false;

            IsApical = false;

            for (int i = 0; i < numColumns; i++)
            {
                for (int j = 0; j < numColumns; j++)
                {
                    Columns[i, j] = new Column(i, j, numColumns);
                }
            }

            GenerateTemporalLines();
            GenerateApicalLines();
           
        }

        public void Init()
        {
            connector = new Connector();

            connector.ReadDendriticSchema();

            connector.ReadAxonalSchema();
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

        public void Fire(SDR incomingPattern, bool ignorePrecyclePrep = false)
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
                        if(IsApical == true || isTemporal == true)
                        {
                            Console.WriteLine("BlockBheaviourManager : FIRE : Invalid Execution Order!!!");
                            throw new InvalidOperationException();
                        }

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

                        break;
                    }
                case iType.TEMPORAL:
                    {
                        isTemporal = true;

                        List<Neuron> neuronsToPolarizeList = TransformTemporalCoordinatesToSpatialCoordinates(incomingPattern.ActiveBits);

                        if(neuronsToPolarizeList.Count != 0)
                        {
                            foreach(var neuronToPolarize in neuronsToPolarizeList)
                            {
                                Neuron temporalNeuron = neuronToPolarize.GetMyTemporalPartner();
                                neuronToPolarize.ProcessSpikeFromNeuron(temporalNeuron.NeuronID);
                                temporalContributors.Add(neuronToPolarize);                                
                            }
                        }

                        break;
                    }
                case iType.APICAL:
                    {
                        IsApical = true;

                        List<Neuron> neuronsToPolarizeList = new List<Neuron>();
                        incomingPattern.ActiveBits.ForEach(pos => {
                            neuronsToPolarizeList.AddRange(Columns[pos.X, pos.Y].Neurons);
                            });

                        if (neuronsToPolarizeList.Count != 0)
                        {
                            foreach (var neuronToPolarize in neuronsToPolarizeList)
                            {
                                Neuron apicalNeuron = neuronToPolarize.GetMyTemporalPartner();
                                neuronToPolarize.ProcessSpikeFromNeuron(apicalNeuron.NeuronID);
                                apicalContributors.Add(neuronToPolarize);
                            }
                        }

                        break;
                    }
            }

            Wire();

            PostCycleCleanup();
        }       

        private void Wire()
        {

            //Get intersection of neuronsFiringThisCycle and predictedNeuronsfromLastCycleCycle

            List<Neuron> predictedNeuronList = new List<Neuron>();

            foreach (var item in PredictedNeuronsfromLastCycle.Keys)
            {
                var neuronToAdd = Position.ConvertStringPosToNeuron(item);

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
                foreach(var dendriticNeuronItem in NeuronsFiringThisCycle)
                {
                    foreach( var axonalNeuronItem in NeuronsFiringLastCycle)
                    {
                        //Connect last cycle firing neuronal axons this cycle firing dendrites

                        ConnectTwoNeurons(axonalNeuronItem, dendriticNeuronItem);
                    }
                }

            }

            //Else PramoteCorrectlyPredictedConnections
            foreach (var correctlyPredictedNeuron in correctPredictionList)
            {
                List<string> contributingList;
                if (PredictedNeuronsfromLastCycle.TryGetValue(correctlyPredictedNeuron.NeuronID.ToString(), out contributingList))
                {
                    if(contributingList.Count == 0)
                    {
                        throw new Exception("Wire : This should never Happen!! Contributing List should not be empty");
                    }
                    foreach (var contributingNeuron in contributingList)
                    {
                        //fPosition.ConvertStringPosToNeuron(contributingNeuron).PramoteCorrectPredictionAxonal(correctlyPredictedNeuron);
                        correctlyPredictedNeuron.PramoteCorrectPredictionDendronal(Position.ConvertStringPosToNeuron(contributingNeuron));
                    }
                }
            }          

            if(isTemporal)
            {
                //Get intersection between temporal input SDR and the firing Neurons if any fired and strengthen it
                if(temporalContributors.Count != 0)
                {
                    foreach(var neuron in temporalContributors)
                    {
                        neuron.StrengthenTemporalConnection();
                    }
                }
                 
                isTemporal = false;
            }

            if(IsApical)
            {
                //Get intersection between temporal input SDR and the firing Neurons if any fired and strengthen it

                if (apicalContributors.Count != 0)
                {
                    foreach (var neuron in apicalContributors)
                    {
                        neuron.StrengthenApicalConnection();
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
            // How to get temporal lines inserted into this mix
            // I have column objects vertical ones these will need to work on a horizontal basis

            //Questions:
            //1.Should voltage be distributed across all the neurons in the temporal line or just few neurons.

            for(int i=0;i<NumColumns;i++)
            {
                for (int j = 0; j < NumColumns; j++)
                {
                    for (int k = 0; k < NumColumns; k++)
                    {
                        ConnectTwoNeurons(TemporalLineArray[i, j], Columns[k, j].Neurons[i], ConnectionType.TEMPRORAL);
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
                    for (int k = 0; k < NumColumns; k++)
                    {
                        ConnectTwoNeurons(ApicalLineArray[i, j], Columns[i, j].Neurons[k], ConnectionType.APICAL);
                    }
                }
            }
        }
        
        private List<Neuron> TransformTemporalCoordinatesToSpatialCoordinates(List<Position> positionLists)
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

        public bool ConnectTwoNeurons(Neuron AxonalNeuron, Neuron DendriticNeuron, ConnectionType? cType = null)
        {
            if (AxonalNeuron == null || DendriticNeuron == null)
                return false;

            if(AxonalNeuron.NeuronID.Equals(DendriticNeuron.NeuronID))
            {
                Console.WriteLine("ConnectTwoNeurons : Cannot Connect Neuron to itself!");
                return false;
            }

            AxonalNeuron.AddtoAxonalList(DendriticNeuron.NeuronID.ToString());
            DendriticNeuron.AddToDistalList(AxonalNeuron.NeuronID.ToString(), cType);

            return true;
        }

        public static Neuron GetNeuronFromPosition(Position pos)
            => _blockBehaviourManager.Columns[pos.X, pos.Y].Neurons[pos.Z];

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
