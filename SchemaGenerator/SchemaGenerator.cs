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

        public SchemaGenerator(int x, int y, int z, int numConns)
        {
            this.numX = 100;
            this.numY = 10;
            this.numZ = 10;
            this.numL = 2;
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

                        proximalNode?.SetAttribute("X", i.ToString());
                        proximalNode?.SetAttribute("Y", j.ToString());
                        proximalNode?.SetAttribute("Z", k.ToString());

                        for (int l = 0; l < numL; l++)   //Connections to other Neurons except Source Neuron
                        {
                            var neuronNode = xmlDocument?.CreateElement("Connection", string.Empty);

                            int x = GetRandomNumberExcept(0, numX, i);
                            int y = GetRandomNumberExcept(0, numY, j);
                            int z = GetRandomNumberExcept(0, numZ, k);

                            neuronNode?.SetAttribute("X", x.ToString());
                            neuronNode?.SetAttribute("Y", y.ToString());
                            neuronNode?.SetAttribute("Z", z.ToString());

                            proximalNode.AppendChild(neuronNode);
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

                        for (int l = 0; l < numL; l++)   //Connections to other Neurons except Source Neuron
                        {
                            var proximalConnection = xmlDocument.CreateElement("Neuron", string.Empty);

                            int x = GetRandomNumberExcept(0, numX, i);
                            int y = GetRandomNumberExcept(0, numX, i);
                            int z = GetRandomNumberExcept(0, numX, i);

                            proximalConnection.SetAttribute("X", x.ToString());
                            proximalConnection.SetAttribute("Y", y.ToString());
                            proximalConnection.SetAttribute("Z", z.ToString());

                            axonalConnectionNode.AppendChild(proximalConnection);
                        }                        
                    }

                    xmlDocument?.DocumentElement?.AppendChild(axonalConnectionNode);
                }
            }

            xmlDocument?.Save(filePath + axonalfileName);
        }

        private int GetARandomNumberBetweenTwoSets(int min1, int max1, int min2, int max2)
        {
            return rand.Next(min1, max1) + rand.Next(min2, max2) % 2 == 0 ? rand.Next(min1) : rand.Next(min2, max2);
        }

        private int GetRandomNumberExcept(int min, int max, int except)
        {
            if( except < min || except > max)
            {
                return rand.Next(min, max);
            }

            if (except == min)
                return rand.Next(min + 1, max);
            else if (except == max)
                return rand.Next(min, max - 1);
            else
            {
                if (except - 1 == min)
                    return max;
                else if (except + 1 == max)
                    return min;
                else
                    return GetARandomNumberBetweenTwoSets(min, except - 1, except + 1, max);
            }
        }
    }
}
