namespace FirstOrderMemory.BehaviourManagers
{
    using System.Xml;

    public class Connector
    {
        private int neuronCounter;
        public Connector() { 
        neuronCounter = 0;
        }


        public void ReadFromSchemaFile()
        {
            XmlDocument document = new XmlDocument();
            string documentPath = "C:\\\\Users\\\\depint\\\\source\\\\repos\\\\FirstOrderMemory\\\\Schema Docs\\\\ConnectorSchema.xml";

            if (!File.Exists(documentPath))
            {
                throw new FileNotFoundException(documentPath);
            }


            document.Load(documentPath);

            XmlNodeList columns = document.GetElementsByTagName("Column");

            var numColumns = columns.Count;

            for (int i = 0; i < numColumns; i++)
            {
                neuronCounter = 0;
                var item = columns[i];

                var x = item.Attributes[0]?.Value;
                var y = item.Attributes[1]?.Value;

                //Column
                foreach (XmlNode node in item.ChildNodes)
                {   //Neuron

                    if(neuronCounter >= 10)
                    {
                        int breakpoint = 1;                        
                        break;
                    }


                    if(node?.Attributes == null)
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
                    
                    var neuronNodes = proximalNodes.Item(0).SelectNodes("Neuron");

                    if(neuronNodes.Count != 4)
                    {
                        throw new InvalidOperationException("Invalid Number of Neuronal Connections defined for Neuron" + a.ToString() +  b.ToString() + c.ToString());
                    }
                    foreach(XmlNode neuron in neuronNodes)
                    {               
                        if(neuron?.Attributes.Count != 3)
                        {
                            throw new InvalidOperationException("Number of Attributes in Neuronal Node is not 3");
                        }

                        int e = Convert.ToInt32(neuron.Attributes[0].Value);
                        int f = Convert.ToInt32(neuron.Attributes[1].Value);
                        int g = Convert.ToInt32(neuron.Attributes[2].Value);

                        //Money Shot!!!
                        BlockBehaviourManager.InitConnectionForConnector(a, b, c, e, f, g);
                    }
                    neuronCounter++;
                }

            }






        }
    }
}
