using FirstOrderMemory.Models;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace FirstOrderMemory.BehaviourManagers
{
    public class BlockBehaviourManager
    {
        public ulong CycleNum { get; private set; }
        private int NumColumns;
        private Dictionary<string, List<string>> _predictedNeuronsForNextCycle;
        private Dictionary<string, List<string>> _predictedNeuronsfromLastCycle;
        private List<Neuron> neuronsFiringThisCycle;
        private List<Neuron> neuronsFiringLastCycle;
        private List<Segment>? _predictedSegmentForThisCycle;
        private List<Position> ColumnsThatBurst;
        private Dictionary<int, List<string>>? temporalFiringPairs = null;
        public Column[,] Columns { get; private set; }
        public uint totalProximalConnections;
        private static BlockBehaviourManager _blockBehaviourManager;        

        public static BlockBehaviourManager GetBlockBehaviourManager(int numColumns = 10)
        {
            if(BlockBehaviourManager._blockBehaviourManager == null)
            {
                BlockBehaviourManager._blockBehaviourManager = new BlockBehaviourManager(numColumns);                
            }

            return BlockBehaviourManager._blockBehaviourManager;
        }

        public static void InitConnectionForConnector(int x, int y, int z, int i, int j, int k)
        {
            if(x==i && y == j && z == k)
            {
                int breakpoint = 1;
            }
            try
            {
                Column col = GetBlockBehaviourManager().Columns[x, y];
                if(col.Init <= 40)
                {
                    Neuron neuron = col.Neurons[z];
                    neuron.InitProximalConnectionForConnector(i, j, k);
                    int breakpoint = 1;
                    BlockBehaviourManager.IncrementProximalConnectionCount();
                    col.Init++;
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());   
                int breakpoint = 1;
            }
        }

        private BlockBehaviourManager(int numColumns = 10)
        {
            this.CycleNum = 0;
            this.NumColumns = numColumns;
            _predictedNeuronsfromLastCycle = new Dictionary<string, List<string>>();
            //_predictedSegmentForThisCycle = new List<Segment>();
            _predictedNeuronsForNextCycle = new Dictionary<string, List<string>>();
            neuronsFiringThisCycle = new List<Neuron>();
            neuronsFiringLastCycle = new List<Neuron>();
            Columns = new Column[numColumns, numColumns];
            ColumnsThatBurst = new List<Position>();
            totalProximalConnections = 0;
            for (int i = 0; i < numColumns; i++) 
            {
                for(int j = 0; j < numColumns; j++)
                {
                    Columns[i, j] = new Column(i, j, numColumns);
                }
            }
        }

        private static void IncrementProximalConnectionCount()
        {
            GetBlockBehaviourManager().totalProximalConnections++;
        }

        private void PreCyclePrep()
        {
            //Prepare all the neurons that are predicted 
            if(_predictedNeuronsForNextCycle.Count != 0 && neuronsFiringThisCycle.Count != 0)
            {
                Console.WriteLine("Precycle Cleanup Error : _predictedNeuronsForNextCycle is not empty");
                throw new Exception("PreCycle Cleanup Exception!!!");
            }

            for(int i=0;i<NumColumns; i++)
                for (int j=0;j<NumColumns;j++)
                {
                    if(Columns[i,j].PreCleanupCheck())
                    {
                        Console.WriteLine("Precycle Cleanup Error : Column {0} {1} is pre Firing",i,j);
                        throw new Exception("PreCycle Cleanup Exception!!!");
                    }
                }
            ColumnsThatBurst.Clear();
        }

        private void PostCycleCleanup()
        {            
            //clean up all the fired columns
            foreach (var column in Columns)
            {
                column.PostCycleCleanup();
            }

            //Prepare the predicted list for next cycle Fire 

            foreach (var kvp in _predictedNeuronsForNextCycle)
            {
                _predictedNeuronsfromLastCycle[kvp.Key] = kvp.Value;
            }
            _predictedNeuronsForNextCycle.Clear();

            neuronsFiringLastCycle.Clear();
            foreach(var item in neuronsFiringThisCycle)
            {
                neuronsFiringLastCycle.Add(item);
            }
            neuronsFiringThisCycle.Clear();
            CycleNum++;
            // Process Next pattern.          
        }

        private void Wire()
        {
            //Get all the neurons that fired this cycle ,
            //Get all the neurons that were predicted from last cycle.
            //compare them against the ones that were predicted , if matched strengthen

            // else if its a new pattern, make these connections with the neurons that fired previous cycle , also connect neurons that fired this cycle and prepare neurons that and predicted this cycle

            //Technical : Get intersection of neuronsFiringThisCycle and predictedNeuronsfromLastCycleCycle

            List<Neuron> predictedNeuronList = new List<Neuron>();

            foreach (var item in _predictedNeuronsfromLastCycle.Keys)
            {
                var neuronToAdd = Position.ConvertStringPosToNeuron(item);

                if(neuronToAdd != null) 
                {
                    predictedNeuronList.Add(neuronToAdd);
                }
            };

            var correctPredictionList = neuronsFiringThisCycle.Intersect(predictedNeuronList).ToList<Neuron>();           
            

            //Total New Pattern : None of the predicted neurons Fired 
            if (correctPredictionList.Count == 0 || ColumnsThatBurst.Count != 0)
            {
                //Todo:
                //How to wire Bursting Columns ?
                //Everytime a neuron fires it should be given a chance to connect to another neuron of its wish( suggested wish)

            }

            //Else PramoteCorrectlyPredictedConnections
            foreach(var neuron in correctPredictionList)
            {
                List<string> contributingList;
                if(_predictedNeuronsfromLastCycle.TryGetValue(neuron.NeuronID.ToString(), out contributingList))
                {
                    foreach(var neuronString in contributingList)
                    {
                        Position.ConvertStringPosToNeuron(neuronString).PramoteCorrectPrediction(neuron);
                    }
                }
            }

            //Every 50 Cycles Prune unused and under Firing Connections
            if(this.CycleNum % 50 == 0)
            {
                foreach (var col in this.Columns)
                {
                    col.PruneCycleRefresh();
                }
            }
            // Todo: Check if neurons that all fired together are connected to each other or not and connect them!   
        }

        public void Fire(SDR incomingPattern)
        {
            List<Neuron> neuronsFiringThisCycle = new List<Neuron>();

            PreCyclePrep();

            if (incomingPattern.ActiveBits.Count == 0)
                return;

            for(int i = 0;i < incomingPattern.ActiveBits.Count; i++)
            {
                var firingNeuronPosition = Columns[incomingPattern.ActiveBits[i].X, incomingPattern.ActiveBits[i].Y].Fire();
                
                if(firingNeuronPosition == null)
                {
                    neuronsFiringThisCycle.AddRange(Columns[incomingPattern.ActiveBits[i].X, incomingPattern.ActiveBits[i].Y].Neurons);
                    ColumnsThatBurst.Add(incomingPattern.ActiveBits[i]);
                }
                else
                {
                    neuronsFiringThisCycle.Add(GetNeuronFromPosition(firingNeuronPosition));
                }
            }

            Wire();

            PostCycleCleanup();
        }


        public void AddPredictedNeuron(Neuron predictedNeuron, string contributingNeuron)
        {
            List<string> contributingList = null;
            if (_predictedNeuronsForNextCycle.Count > 0 && _predictedNeuronsForNextCycle.TryGetValue(predictedNeuron.NeuronID.ToString(), out contributingList))
            {
                if(contributingList != null)
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
                _predictedNeuronsForNextCycle.Add(predictedNeuron.NeuronID.ToString(), new List<string>() { contributingNeuron});
            }
        }

        public static Neuron GetNeuronFromPosition(Position pos)
            => _blockBehaviourManager.Columns[pos.X, pos.Y].Neurons[pos.Z];
                
    }
}
