using Common;
using SecondOrderMemory.Models;

namespace SecondOrderMemory.BehaviourManagers
{
    /// <summary>
    /// 1.
    /// 2.Maintain a precidence list of the top Blocks that are performing mappings based on total actiivty.
    /// </summary>
    public class SBBOrchestrator
    {
        public List<SBBManager> Blocks { get; private set; }

        public int NumColumns = 10;

        public int NumRows = 10;

        public int NumBlocks;

        public SBBOrchestrator(int numBlocks, int numColumns, int numRows) 
        {
            if(numBlocks <= 0 || NumRows <= 0 || NumColumns <= 0) 
            {
                throw new InvalidDataException("SOMBLCOKMANAGER :: NumBlocks Should be more than Zero");
            }

            NumBlocks = numBlocks;

            NumColumns = numColumns;

            NumRows = numRows;

            Blocks = new List<SBBManager>();
            
            SBBManager bbm;

            for( int i = 0; i < numBlocks; i++ )
            {            
                bbm  = new SBBManager(NumColumns, NumRows, i);
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
            
            //SOM cannot uniquely identify an object unless it has previously identified a unique pattern which has linked succesfully to an Object (Higher Order Pattern) ,
            //There has to be some level of Supervised learning here , where 
            //The network has to be trained to identify the object as an apple or orange or house or car as such ...
            // Build the SOM Layer first to see how it works the nfrom the data we can build the logic for higher predictive feedback Layers.


            //Algorithm : 

            //Step 1: Keep checking with all the layers for Confident Classification
            //Step 2: If one of the blocks return abck with a Confident Classification -> cross check this classification with all the other layers , if all of them agree Classify the input to corresponding Object.
            //Step 3: If 2 different block respond with 2 different Classifications with high degree of Confidence , This is a misconfigured Network , Needs More Training , One of the is wrong or both of the mare wrong. Needs Investigation Step 6.
            //Step 4: If 1 Block votes for one object and the rest of the blocks vote for another object, Needs Investigation Step 6.
            //Step 5: All the Blocks are still Confused , Keep repeating Step 1.
            //Step 6: Investigate Dissonance in the network!






        }
    }
}
