//Author : Deric Pinto

namespace FirstOrderMemory.Models
{
    internal class Delta
    {
        public ulong LastAnalyediteration {  get; private set; }
        public KeyValuePair<Position_SOM, Position_SOM> Position { get; private set; }

        public Delta() 
        {

        }



    }
}
