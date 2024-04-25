using System.Xml;

namespace SchemaGenerator
{
    internal class SchemaGenerator
    {
        private const string filePath = "C:\\Users\\depint\\source\\repos\\Hentul\\SchemaGenerator\\Schemas\\";
        private const string denrticicfileName = "DendriticSchema.xml";
        private const string axonalfileName = "AxonalSchema.xml";

        private bool debox = true;

        int numX = 0;
        int numY = 0;
        int numZ = 0;
        int numL = 0;
        Random rand;

        public SchemaGenerator(int x = 250, int y = 10, int z = 10, int numConns = 2)
        {
            this.numX = x;
            this.numY = y;
            this.numZ = z;
            this.numL = numConns;
            rand = new Random();
        }
        
        public void GenerateDendriticSchema()
        {
            XmlDocument xmlDocument = new XmlDocument();

            xmlDocument.LoadXml("<DendriticSchema></DendriticSchema>");

            for (int i = 0; i < numX; i++)              //Column Level
            {                                
                for (int j = 0; j < numY; j++)          //Neuron Level
                {
                    var sourceNeuronElement = xmlDocument?.CreateElement("Neuron", string.Empty);

                    sourceNeuronElement?.SetAttribute("X", i.ToString());
                    sourceNeuronElement?.SetAttribute("Y", j.ToString());

                    for (int k = 0; k < numZ; k++)      //Proximal Connection Per Neuron Level
                    {
                        
                        var proximalNode = xmlDocument?.CreateElement("ProximalConnections", string.Empty);
                        int cache = int.MaxValue;

                        proximalNode?.SetAttribute("X", i.ToString());
                        proximalNode?.SetAttribute("Y", j.ToString());
                        proximalNode?.SetAttribute("Z", k.ToString());

                        for (int l = 0; l < numL; l++)   //Connections to other Neurons except Source Neuron
                        {
                            var neuronNode = xmlDocument?.CreateElement("Connection", string.Empty);

                            int x = GetRandomNumberExcept(0, numX, i);

                            while (x == cache || x == i)
                            {
                                x = GetRandomNumberExcept(0, numX, i);
                            }

                            int y = GetRandomNumberExcept(0, numY, j);
                            int z = GetRandomNumberExcept(0, numZ, k);

                            neuronNode?.SetAttribute("X", x.ToString());
                            neuronNode?.SetAttribute("Y", y.ToString());
                            neuronNode?.SetAttribute("Z", z.ToString());

                            proximalNode.AppendChild(neuronNode);

                            cache = l;
                        }
                         
                        sourceNeuronElement?.AppendChild(proximalNode);                        
                    }

                    xmlDocument?.DocumentElement?.AppendChild(sourceNeuronElement);
                }
                
            }

            xmlDocument?.Save(filePath + denrticicfileName);        

        }

        public void GenerateAxonalSchema()
        {
            XmlDocument xmlDocument = new XmlDocument();

            xmlDocument.LoadXml("<AxonalSchema></AxonalSchema>");

            for (int i = 0; i < numX; i++)              //Column Level
            {                
                for (int j = 0; j < numY; j++)          //Neuron Level
                {
                    var axonalConnectionNode = xmlDocument?.CreateElement("AxonalConnection", string.Empty);

                    axonalConnectionNode.SetAttribute("X", i.ToString());
                    axonalConnectionNode.SetAttribute("Y", j.ToString());

                    for (int k = 0; k < numZ; k++)      //Proximal Connection Per Neuron Level
                    {
                        int cache = int.MaxValue;

                        for (int l = 0; l < numL; l++)   //Connections to other Neurons except Source Neuron
                        {
                            var proximalConnection = xmlDocument.CreateElement("Neuron", string.Empty);

                            int x = GetRandomNumberExcept(0, numX, i);

                            while (x == cache || x == i)
                            {
                                x = GetRandomNumberExcept(0, numX, i);
                            }

                            int y = GetRandomNumberExcept(0, numY, j);
                            int z = GetRandomNumberExcept(0, numZ, k);

                            proximalConnection.SetAttribute("X", x.ToString());
                            proximalConnection.SetAttribute("Y", y.ToString());
                            proximalConnection.SetAttribute("Z", z.ToString());

                            axonalConnectionNode.AppendChild(proximalConnection);

                            cache = x;
                        }                        
                    }

                    xmlDocument?.DocumentElement?.AppendChild(axonalConnectionNode);
                }
            }

            xmlDocument?.Save(filePath + axonalfileName);
        }

        private int GetARandomNumberBetweenTwoSets(int min1, int max1, int min2, int max2)
        {
            return (max1 - min1) > (max2 - min2) ? rand.Next(min1, max1) : rand.Next(min2, max2);
        }

        private int GetRandomNumberExcept(int min, int max, int except)
        {
            if( except < min || except > max)
            {
                return rand.Next(min, max);
            }
            else if (except == min)
                return rand.Next(min + 1, max);
            else if (except == max)
                return rand.Next(min, max - 1);
            else
            {               
                return GetARandomNumberBetweenTwoSets(min, except - 1, except + 1, max);
            }
        }
    }
}
