using FirstOrderMemory.Models;
using System.Collections.Generic;

namespace FirstOrderMemory.BehaviourManagers
{
    public class BlockBehaviourManager
    {
        public List<Segment>? _predictedSegment;
        private List<string>? _predictedNeuron;
        private Column[,] _columns;
        private Dictionary<string, List<string>>? temporalfiringPairs = null;            
        private static BlockBehaviourManager _blockBehaviourManager;
        private List<Position> _postCycleColumnCleanup;

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
            _predictedNeuron = new List<string>();
            _columns = new Column[numColumns, numColumns];
            for(int i = 0; i < numColumns; i++) 
            {
                for(int j = 0; j < numColumns; j++) 
                {
                    _columns[i, j] = new Column(numColumns, i, j);
                }
            }
        }

        public void ProcessPattern(SDR incomingPattern)
        {
            if (incomingPattern.ActiveBits.Count == 0)
                return;

            for(int i = 0;i < incomingPattern.ActiveBits.Count;i++)
            {
                _columns[incomingPattern.ActiveBits[i].X, incomingPattern.ActiveBits[i].Y].Fire();
                _postCycleColumnCleanup.Add(incomingPattern.ActiveBits[i]);
            }
        }

        public void AddPredictedNeuron(Neuron predictedNeuron)
        {
            if (_predictedNeuron.Count > 0 && _predictedNeuron.Contains(predictedNeuron.NeuronID.ToString()))
            {
                _predictedNeuron.Add(predictedNeuron.NeuronID.ToString());
            }
        }



    }
}
