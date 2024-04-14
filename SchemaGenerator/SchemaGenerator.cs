using System.Xml;

namespace SchemaGenerator
{
    internal class SchemaGenerator
    {
        private const string filePath = "C:\\Users\\depint\\Desktop\\Hentul\\SchemaGenerator\\Schemas\\";
        private const string denrticicfileName = "DendriticSchema.xml";
        private const string axonalfileName = "AxonalSchema.xml";


        int numX = 0;
        int numY = 0;
        int numZ = 0;
        int numL = 0;
        Random rand;

        public SchemaGenerator(int x, int y, int z, int numConns)
        {
            this.numX = x;
            this.numY = y;
            this.numZ = z;
            this.numL = numConns;
            rand = new Random();
        }

        private int GetARandomNumberBetweenTwoSets(int min1, int max1, int min2, int max2)
        {
            return rand.Next(min1, max1) + rand.Next(min2, max2) % 2 == 0 ? rand.Next(min1) : rand.Next(min2, max2);
        }

        private int GetRandomNumberExcept(int min, int max, int except)
        {
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

        public void GenerateDendriticSchema()
        {
            XmlDocument xmlDocument = new XmlDocument();

            xmlDocument.LoadXml("<DendriticSchema></DendriticSchema>");

            for (int i = 0; i < numX; i++)              //Column Level
            {
                var columnNode = xmlDocument.CreateNode(XmlNodeType.Element, "Column", string.Empty);

                var sourceNeuronElement = xmlDocument.CreateElement("Neuron", string.Empty);

                for (int j = 0; j < numY; j++)          //Neuron Level
                {                    
                    for (int k = 0; k < numZ; k++)      //Proximal Connection Per Neuron Level
                    {                        

                        sourceNeuronElement.SetAttribute("X", i.ToString());
                        sourceNeuronElement.SetAttribute("Y", j.ToString());
                        sourceNeuronElement.SetAttribute("Z", k.ToString());

                        var markerNode = xmlDocument.CreateElement("ProximalConnections", string.Empty);

                        for (int l = 0; l < numL; l++)   //Connections to other Neurons except Source Neuron
                        {
                            var proximalConnection = xmlDocument.CreateElement("Neuron", string.Empty);

                            int x = GetRandomNumberExcept(0, numX, i);
                            int y = GetRandomNumberExcept(0, numX, i);
                            int z = GetRandomNumberExcept(0, numX, i);

                            proximalConnection.SetAttribute("X", x.ToString());
                            proximalConnection.SetAttribute("Y", y.ToString());
                            proximalConnection.SetAttribute("Z", z.ToString());

                            markerNode.AppendChild(proximalConnection);
                        }
                         
                        sourceNeuronElement.AppendChild(markerNode);
                    }                    
                }

                columnNode.AppendChild(sourceNeuronElement);

                xmlDocument?.DocumentElement?.AppendChild(columnNode);
            }


            //                var distalNode = xmlDocument.CreateNode(XmlNodeType.Element, "DistalDendriticConnection", string.Empty);

            //                var blockIdElement = xmlDocument.CreateElement("BlockID", string.Empty);
            //                blockIdElement.InnerText = BlockID.X.ToString();

            //                var sourceNeuronElement = xmlDocument.CreateElement("SourceNeuronID", string.Empty);

            //                sourceNeuronElement.InnerText = distalSynapse.Value.AxonalNeuronId.ToString();

            //                var targetNeuronElement = xmlDocument.CreateNode(XmlNodeType.Element, "TargetNeuronID", string.Empty);
            //                targetNeuronElement.InnerText = distalSynapse.Value.DendronalNeuronalId.ToString();

            //                distalNode.AppendChild(sourceNeuronElement);
            //                distalNode.AppendChild(targetNeuronElement);

            //                xmlDocument?.DocumentElement?.AppendChild(distalNode);
            //            }

            //        }
            //    }
            //}

            xmlDocument?.Save(filePath + denrticicfileName);        

        }

        public void GenerateAxonalSchema()
        {
            XmlDocument xmlDocument = new XmlDocument();

            xmlDocument.LoadXml("<AxonalSchema></AxonalSchema>");

            for (int i = 0; i < numX; i++)              //Column Level
            {
                var axonalConnectionNode = xmlDocument.CreateElement("AxonalConnection", string.Empty);

                for (int j = 0; j < numY; j++)          //Neuron Level
                {                    

                    for (int k = 0; k < numZ; k++)      //Proximal Connection Per Neuron Level
                    {

                        axonalConnectionNode.SetAttribute("X", i.ToString());
                        axonalConnectionNode.SetAttribute("Y", j.ToString());
                        axonalConnectionNode.SetAttribute("Z", k.ToString());

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
    }
}
