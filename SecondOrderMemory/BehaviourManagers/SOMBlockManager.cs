using Common;
using SecondOrderMemory.Models;

namespace SecondOrderMemory.BehaviourManagers
{
    public class SOMBlockManager
    {
        public List<BlockBehaviourManager> Blocks { get; private set; }

        public int NumColumns = 10;

        public int NumRows = 10;

        public int NumBlocks;

        public SOMBlockManager(int numBlocks, int numColumns, int numRows) 
        {
            if(numBlocks <= 0 || NumRows <= 0 || NumColumns <= 0) 
            {
                throw new InvalidDataException("SOMBLCOKMANAGER :: NumBlocks Should be more than Zero");
            }

            NumBlocks = numBlocks;

            NumColumns = numColumns;

            NumRows = numRows;

            Blocks = new List<BlockBehaviourManager>();
            
            BlockBehaviourManager bbm;

            for( int i = 0; i < numBlocks; i++ )
            {            
                bbm  = new BlockBehaviourManager(NumColumns, NumRows, i);
                bbm.Init();
                Blocks.Add(bbm);
            }
        }

        public void Fire(int blockNumber, SDR_SOM som_sdr)
        {
            Blocks[blockNumber].Fire(som_sdr);
        }

        private void EstablishCurrentState()
        {
            //Check if there is consensus amoung all the blocks either by unioun and Intersection and update CurrentState Enum 

            //Question : what if there is dissonance amoung the blocks ?





        }
    }
}
