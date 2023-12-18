namespace FirstOrderMemory.BehaviourManagers
{
    using System.Xml;

    public class Connector
    {
        private int neuronCounter;
        public Connector() { 
        neuronCounter = 0;
        }


        public void ReadDendriticSchema()
        {
            XmlDocument document = new XmlDocument();
            string dendriteDocumentPath = "C:\\\\Users\\\\depint\\\\source\\\\repos\\\\FirstOrderMemory\\\\Schema Docs\\\\ConnectorSchema.xml";
            string axonalDocumentPath = "C:\\Users\\depint\\source\\repos\\FirstOrderMemory\\Schema Docs\\AxonalSchema.xml";


            if (!File.Exists(dendriteDocumentPath))
            {
                throw new FileNotFoundException(dendriteDocumentPath);
            }

            document.Load(dendriteDocumentPath);

            XmlNodeList columns = document.GetElementsByTagName("Column");

            var numColumns = columns.Count;

            for (int i = 0; i < numColumns; i++)
            {//Column
                neuronCounter = 0;

                var item = columns[i];

                int x = Convert.ToInt32(item.Attributes[0]?.Value);
                var y = Convert.ToInt32(item.Attributes[1]?.Value);

                BlockBehaviourManager.GetBlockBehaviourManager().Columns[x, y].Init++;

                foreach (XmlNode node in item.ChildNodes)
                {   //Neuron                   

                    if (node?.Attributes == null)
                    {
                        continue;
                    }

                    if (node.Attributes.Count != 3)
                    {
                        throw new InvalidOperationException("Invalid Neuron Id Supplied in Schema");
                    }

                    int a = Convert.ToInt32(node.Attributes[0]?.Value);
                    int b = Convert.ToInt32(node.Attributes[1]?.Value);
                    int c = Convert.ToInt32(node.Attributes[2]?.Value);

                    var proximalNodes = node.ChildNodes;

                    var neuronNodes = proximalNodes.Item(0)
                        .SelectNodes("Neuron");

                    if (neuronNodes.Count != 4)
                    {
                        throw new InvalidOperationException("Invalid Number of Neuronal Connections defined for Neuron" + a.ToString() + b.ToString() + c.ToString());
                    }


                    foreach (XmlNode neuron in neuronNodes)
                    {
                        if (neuron?.Attributes?.Count != 3)
                        {
                            throw new InvalidOperationException("Number of Attributes in Neuronal Node is not 3");
                        }

                        int e = Convert.ToInt32(neuron.Attributes[0].Value);
                        int f = Convert.ToInt32(neuron.Attributes[1].Value);
                        int g = Convert.ToInt32(neuron.Attributes[2].Value);

                        //Money Shot!!!
                        BlockBehaviourManager.InitDendriticConnectionForConnector(a, b, c, e, f, g);
                    }

                    neuronCounter++;
                }

            }
        }

        public void ReadAxonalSchema()
        {
            XmlDocument document = new XmlDocument();            
            string axonalDocumentPath = "C:\\Users\\depint\\source\\repos\\FirstOrderMemory\\Schema Docs\\AxonalSchema.xml";

            if (!File.Exists(axonalDocumentPath))
            {
                throw new FileNotFoundException(axonalDocumentPath);
            }


            document.Load(axonalDocumentPath);

            XmlNodeList columns = document.GetElementsByTagName("axonalConnection");
         
                for (int icount = 0; icount < columns.Count; icount++)
                {//axonalConnection

                    XmlNode connection = columns[icount];

                    if (connection.Attributes.Count != 3)
                    {
                        throw new InvalidDataException();

                    }

                    int x = Convert.ToInt32(connection.Attributes[0].Value);
                    int y = Convert.ToInt32(connection.Attributes[1].Value);
                    int z = Convert.ToInt32(connection.Attributes[2].Value);

                    XmlNodeList axonList = connection.ChildNodes;

                    foreach (XmlNode axon in axonList)
                    {
                        if (axon.Attributes.Count != 3)
                        {
                            throw new InvalidDataException();
                        }

                    try
                    {
                        int i = Convert.ToInt32(axon.Attributes[0].Value);
                        int j = Convert.ToInt32(axon.Attributes[1].Value);
                        int k = Convert.ToInt32(axon.Attributes[2].Value);                        

                        BlockBehaviourManager.InitAxonalConnectionForConnector(x, y, z, i, j, k);

                    }
                    catch(Exception e)
                    {
                        int bp = 1;

                    }
                       

                    }



                }
                  


        }
    }
}
