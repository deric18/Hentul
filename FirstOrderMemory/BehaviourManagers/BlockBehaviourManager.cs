using FirstOrderMemory.Models;

namespace FirstOrderMemory.BehaviourManagers
{
    public class BlockBehaviourManager
    {
        public ulong CycleNum { get; private set; }

        private int NumColumns { get; set; }

        private int FileSize { get; set; }

        private int NumRows { get; set; } 

        public Dictionary<string, List<string>> PredictedNeuronsForNextCycle { get; private set; }

        public Dictionary<string, List<string>> PredictedNeuronsfromLastCycle { get; private set; }

        public List<Neuron> NeuronsFiringThisCycle { get; private set; }

        public List<Neuron> NeuronsFiringLastCycle { get; private set; }

        private List<Segment>? _predictedSegmentForThisCycle;

        public List<Position> ColumnsThatBurst { get; private set; }

        private Dictionary<int, List<string>>? temporalFiringPairs = null;

        public Column[,] Columns { get; private set; }

        public uint totalProximalConnections;

        public uint totalAxonalConnections;

        public Connector connector { get; private set; }    

        private static BlockBehaviourManager _blockBehaviourManager;

        public static BlockBehaviourManager GetBlockBehaviourManager(int numColumns = 100)
        {
            if (BlockBehaviourManager._blockBehaviourManager == null)
            {
                BlockBehaviourManager._blockBehaviourManager = new BlockBehaviourManager(numColumns);
            }

            return BlockBehaviourManager._blockBehaviourManager;
        }



        private BlockBehaviourManager(int numColumns, int numRows = 1)
        {
            this.CycleNum = 0;

            this.NumColumns = numColumns;

            this.NumRows = numRows;

            if(Math.Sqrt(numColumns) %  1 != 0)
            {
                throw new InvalidDataException("Supplied Value for Number Of Columns is Invalid");
            }

            this.FileSize = (int)Math.Sqrt(numColumns);

            PredictedNeuronsfromLastCycle = new Dictionary<string, List<string>>();

            //_predictedSegmentForThisCycle = new List<Segment>();
            PredictedNeuronsForNextCycle = new Dictionary<string, List<string>>();

            NeuronsFiringThisCycle = new List<Neuron>();

            NeuronsFiringLastCycle = new List<Neuron>();

            Columns = new Column[numColumns, numColumns];

            ColumnsThatBurst = new List<Position>();

            totalProximalConnections = 0;

            totalAxonalConnections = 0;

            for (int i = 0; i < FileSize; i++)
            {
                for (int j = 0; j < FileSize; j++)
                {
                    Columns[i, j] = new Column(i, j, numRows);
                }
            }
           
        }

        public void Init()
        {
            connector = new Connector();

            connector.ReadDendriticSchema(FileSize, NumRows);

            connector.ReadAxonalSchema(FileSize, NumRows);
        }

        private void PreCyclePrep()
        {
            //Prepare all the neurons that are predicted 
            if (PredictedNeuronsForNextCycle.Count != 0 && NeuronsFiringThisCycle.Count != 0)
            {
                Console.WriteLine("Precycle Cleanup Error : _predictedNeuronsForNextCycle is not empty");
                throw new Exception("PreCycle Cleanup Exception!!!");
            }

            for (int i = 0; i < FileSize; i++)
                for (int j = 0; j < FileSize; j++)
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

            for (int i = 0; i < incomingPattern.ActiveBits.Count; i++)
            {
                var predictedNeuronPositioons = Columns[incomingPattern.ActiveBits[i].X, incomingPattern.ActiveBits[i].Y].GetPredictedNeuronsFromColumn();

                if (predictedNeuronPositioons?.Count == Columns[0,0].Neurons.Count)
                {  
                    //burst
                    neuronsFiringThisCycle.AddRange(Columns[incomingPattern.ActiveBits[i].X, incomingPattern.ActiveBits[i].Y].Neurons);
                    ColumnsThatBurst.Add(incomingPattern.ActiveBits[i]);
                }
                else
                {   //selected predicted neurons
                    neuronsFiringThisCycle.AddRange(predictedNeuronPositioons);
                }

                predictedNeuronPositioons = null;
            }           

            foreach( var neuron in neuronsFiringThisCycle)
            {
                neuron.Fire();
            }

            Wire();

            PostCycleCleanup();
        }

        private void Wire()
        {
            //Get all the neurons that fired this cycle ,
            //Get all the neurons that were predicted from last cycle.
            //compare them against the ones that were predicted , if matched strengthen

            // else if its a new pattern, make these connections with the neurons that fired previous cycle , also connect neurons that fired this cycle and prepare neurons that and predicted this cycle

            //Technical : Get intersection of neuronsFiringThisCycle and predictedNeuronsfromLastCycleCycle

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

        public bool ConnectTwoNeurons(Neuron AxonalNeuron, Neuron DendriticNeuron)
        {
            if (AxonalNeuron == null || DendriticNeuron == null)
                return false;

            if(AxonalNeuron.NeuronID.Equals(DendriticNeuron.NeuronID))
            {
                Console.WriteLine("ConnectTwoNeurons : Cannot Connect Neuron to itself!");

                return false;
            }

            AxonalNeuron.AddtoAxonalList(DendriticNeuron.NeuronID.ToString());
            DendriticNeuron.AddToDistalList(AxonalNeuron.NeuronID.ToString());

            return true;

        }

        public static Neuron GetNeuronFromPosition(int x, int y, int z)
            => _blockBehaviourManager.Columns[x, y].Neurons[z];

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
