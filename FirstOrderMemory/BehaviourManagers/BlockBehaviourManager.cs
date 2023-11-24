using FirstOrderMemory.Models;
using System.Collections.Generic;

namespace FirstOrderMemory.BehaviourManagers
{
    public class BlockBehaviourManager
    {
        
        private int NumColumns;
        private Dictionary<string, List<string>> _predictedNeuronsForNextCycle;        
        private Dictionary<string, List<string>> _predictedNeuronsfromLastCycle;
        private List<Segment>? _predictedSegmentForThisCycle;
        private Dictionary<int, List<string>>? temporalFiringPairs = null;
        private Column[,] _columns;
        private static BlockBehaviourManager _blockBehaviourManager;        

        public static BlockBehaviourManager GetBlockBehaviourManager(int numColumns = 10)
        {
            if(BlockBehaviourManager._blockBehaviourManager == null)
            {
                BlockBehaviourManager._blockBehaviourManager = new BlockBehaviourManager(numColumns);                
            }

            return BlockBehaviourManager._blockBehaviourManager;
        }


        private BlockBehaviourManager(int numColumns)
        {
            this.NumColumns = numColumns;
            _predictedNeuronsfromLastCycle = new Dictionary<string, List<string>>();
            //_predictedSegmentForThisCycle = new List<Segment>();
            _predictedNeuronsForNextCycle = new Dictionary<string, List<string>>();
            _columns = new Column[numColumns, numColumns];
            for(int i = 0; i < numColumns; i++) 
            {
                for(int j = 0; j < numColumns; j++)
                {
                    _columns[i, j] = new Column(numColumns, i, j);
                }
            }
        }

        private void PreCyclePrep()
        {
            //Prepare all the neurons that are predicted 
            if(_predictedNeuronsForNextCycle.Count != 0)
            {
                Console.WriteLine("Precycle Cleanup Error : _predictedNeuronsForNextCycle is not empty");
                throw new Exception("PreCycle Cleanup Exception!!!");
            }

            for(int i=0;i<NumColumns; i++)
                for (int j=0;j<NumColumns;j++)
                {
                    if(_columns[i,j].PreCleanupCheck())
                    {
                        Console.WriteLine("Precycle Cleanup Error : Column {0} {1} is pre Firing",i,j);
                        throw new Exception("PreCycle Cleanup Exception!!!");
                    }
                }            
        }

        private void PostCycleCleanup()
        {
            //clean up all the fired columns
            foreach (var column in _columns)
            {
                column.PostCycleCleanup();
            }

            //Prepare the predicted list for next cycle Fire 

            foreach (var kvp in _predictedNeuronsForNextCycle)
            {
                _predictedNeuronsfromLastCycle[kvp.Key] = kvp.Value;
            }
            _predictedNeuronsForNextCycle.Clear();

            // Process Next pattern.          
        }     

        private void Wire(List<Neuron> neuronsFiringThisCycle)
        {
            //Get all the neurons that fired this cycle ,
            //Get all the neurons that were predicted from last cycle.
            //compare them against the ones that were predicted , if matched strengthen

            // else if its a new pattern, make these connections with the neurons that fired previous cycle , also connect neurons that fired this cycle and prepare neurons that and predicted this cycle

            throw new NotImplementedException();
        }

        public void Fire(SDR incomingPattern)
        {
            List<Neuron> neuronsFiringThisCycle = new List<Neuron>();

            PreCyclePrep();

            if (incomingPattern.ActiveBits.Count == 0)
                return;

            for(int i = 0;i < incomingPattern.ActiveBits.Count; i++)
            {
                var firingNeuronPosition = _columns[incomingPattern.ActiveBits[i].X, incomingPattern.ActiveBits[i].Y].Fire();
                
                if(firingNeuronPosition == null)
                {
                    neuronsFiringThisCycle.AddRange(_columns[incomingPattern.ActiveBits[i].X, incomingPattern.ActiveBits[i].Y].Neurons);
                }
                else
                {
                    neuronsFiringThisCycle.Add(GetNeuronFromPosition(firingNeuronPosition));
                }
            }

            Wire(neuronsFiringThisCycle);

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
            => _blockBehaviourManager._columns[pos.X, pos.Y].Neurons[pos.Z];
        
    }
}
