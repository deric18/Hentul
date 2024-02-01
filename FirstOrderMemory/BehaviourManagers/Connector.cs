namespace FirstOrderMemory.BehaviourManagers
{
    using System.Xml;

    public class Connector
    {        
        private const int NUMBEROFPROXIMALDENDRITICCONNECTIONSPERNEURON = 4;
        private const int NUMBEROFPROXIMALAXONALCONNECTIONSPERNEURON = 2;
        public Connector() {         
        }

        public void ReadDendriticSchema(int fileSize, int numberOfRows)
        {
                   
            Random rand = new Random();

            if (numberOfRows != 10)
            {
                for(int i=0; i< fileSize; i++)
                {
                    for (int j = 0; j < fileSize; j++)
                    {                        
                        for (int k = 0; k < numberOfRows; k++)
                        {
                            for (int l = 0; l < NUMBEROFPROXIMALDENDRITICCONNECTIONSPERNEURON; l++)
                            {
                                Tuple<int, int, int> pos = GetUniqueRandomNumbers(i, j, k, fileSize, fileSize, numberOfRows);

                                if (!BlockBehaviourManager.InitDendriticConnectionForConnector(i, j, k, pos.Item1, pos.Item2, pos.Item3))
                                {
                                    pos = GetUniqueRandomNumbers(i, j, k, fileSize, fileSize, numberOfRows, true, pos.Item1, pos.Item2, pos.Item3);
                                    bool flag = true;
                                    do
                                    {
                                        flag = BlockBehaviourManager.InitDendriticConnectionForConnector(i, j, k, pos.Item1, pos.Item2, pos.Item3);
                                    }
                                    while (!flag);
                                }
                            }
                        }
                    }        
                }
            }
            else
            { 
            XmlDocument document = new XmlDocument();
            string dendriteDocumentPath = "C:\\\\Users\\\\depint\\\\source\\\\repos\\\\FirstOrderMemory\\\\Schema Docs\\\\ConnectorSchema.xml";           


            if (!File.Exists(dendriteDocumentPath))
            {
                throw new FileNotFoundException(dendriteDocumentPath);
            }

            document.Load(dendriteDocumentPath);

            XmlNodeList columns = document.GetElementsByTagName("Column");

            var numColumns = columns.Count;

                for (int i = 0; i < numColumns; i++)
                {//Column                    

                    var item = columns[i];

                    int x = Convert.ToInt32(item.Attributes[0]?.Value);
                    var y = Convert.ToInt32(item.Attributes[1]?.Value);


                    //if(x == 0 && y == 1) 
                    //{
                    //    int breakpoint = 0;
                    //}

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

                        int a1 = Convert.ToInt32(node.Attributes[0]?.Value);
                        int b1 = Convert.ToInt32(node.Attributes[1]?.Value);
                        int c1 = Convert.ToInt32(node.Attributes[2]?.Value);

                        var proximalNodes = node.ChildNodes;

                        var neuronNodes = proximalNodes.Item(0)
                            .SelectNodes("Neuron");

                        if (neuronNodes.Count != 4)
                        {
                            throw new InvalidOperationException("Invalid Number of Neuronal Connections defined for Neuron" + a1.ToString() + b1.ToString() + c1.ToString());
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
                            BlockBehaviourManager.InitDendriticConnectionForConnector(a1, b1, c1, e, f, g);
                        }                        
                    }
                }
            }
        }

        public void ReadAxonalSchema(int fileSize, int numRows)
        {            
            int a, b, c;
            Random rand = new Random();

            if (numRows != 10)
            {
                for (int i = 0; i < fileSize; i++)
                {
                    for (int j = 0; j < fileSize; j++)
                    {
                        for (int k = 0; k < numRows; k++)
                        {
                            for (int l = 0; l < NUMBEROFPROXIMALAXONALCONNECTIONSPERNEURON; l++)
                            {
                                Tuple<int, int, int> pos = GetUniqueRandomNumbers(i, j, k, fileSize, fileSize, numRows);

                                if (!BlockBehaviourManager.InitAxonalConnectionForConnector(i, j, k, pos.Item1, pos.Item2, pos.Item3))
                                {
                                    pos = GetUniqueRandomNumbers(i, j, k, fileSize, fileSize, numRows, true, pos.Item1, pos.Item2, pos.Item3);
                                    bool flag = true;
                                    do
                                    {
                                        flag = BlockBehaviourManager.InitAxonalConnectionForConnector(i, j, k, pos.Item1, pos.Item2, pos.Item3);
                                    }
                                    while (!flag);
                                }                              
                            }
                        }
                    }
                }

            }
            else
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
                        catch (Exception e)
                        {
                            int bp = 1;

                        }
                    }
                }
            }
        }

        private Tuple<int, int, int> GetUniqueRandomNumbers(int i, int j, int k, int max1, int max2, int max3, bool checkCache = false, int cache1 =0, int cache2 = 0, int cache3 = 0)
        {
            int a, b, c;
            Random rand =   new Random();

            a = rand.Next(0, (int)max1); b = rand.Next(0, (int)max2); c = rand.Next(0, max3);

            if (i == a && j == b && k == c)
            {
                if (checkCache)
                {
                    do
                    {
                        a = rand.Next(0, (int)max1); b = rand.Next(0, (int)max2); c = rand.Next(0, max3);
                    }
                    while (i == a && j == b && k == c && a != cache1 && b != cache2 && c != cache3);
                }
                else
                {
                    do
                    {
                        a = rand.Next(0, (int)max1); b = rand.Next(0, (int)max2); c = rand.Next(0, max3);
                    }
                    while (i == a && j == b && k == c);
                }
            }

            return new Tuple<int, int, int>(a, b, c);   
        }
    }
}
