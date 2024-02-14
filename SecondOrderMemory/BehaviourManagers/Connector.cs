namespace SecondOrderMemory.BehaviourManagers
{
    using System;
    using System.Xml;

    public class Connector
    {
        
        private static Connector connector = null;
        

        public static Connector GetConnector()
        {
            if (connector != null)
            {
                return connector;
            }

            connector = new Connector();

            return connector;
        }

      

       

        public void ReadDendriticSchema()
        {

        }


        
    }
}
