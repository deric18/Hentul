namespace SecondOrderMemory.BehaviourManagers
{
    public class SOMBlockManager
    {
        public List<BlockBehaviourManager> Blocks { get; private set; }

        public const int NumColumns = 10;

        public const int NumRows = 10;
        public SOMBlockManager(int numBlocks) 
        {
            if(numBlocks <= 0) 
            {
                throw new InvalidDataException("SOMBLCOKMANAGER :: NumBlocks Should be more than Zero");
            }

            Blocks = new List<BlockBehaviourManager>();
            
            BlockBehaviourManager bbm;

            for( int i = 0; i < numBlocks; i++ )
            {
            
                bbm  = new BlockBehaviourManager(NumColumns, NumRows, i);
                bbm.Init();
                Blocks.Add(bbm);
            }





        }
    }
}
