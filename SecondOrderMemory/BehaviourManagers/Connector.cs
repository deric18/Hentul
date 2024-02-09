namespace SecondOrderMemory.BehaviourManagers
{
    using System;
    using System.Xml;

    public class Connector
    {
        private static int neuronCounter;
        private static Connector connector = null;
        public Dictionary<string, int[]> DendriticCache { get; private set; }
        public Dictionary<string, int[]> AxonalCache { get; private set; }

        public static Connector GetConnector()
        {
            if (connector != null)
            {
                return connector;
            }

            connector = new Connector();

            return connector;
        }

        private Connector() 
        {
            DendriticCache = new Dictionary<string, int[]>();
            AxonalCache = new Dictionary<string, int[]>();
        }

        public void ReadDendriticSchema(int intX, int intY)
        {

            if(connector.DendriticCache.Count != 0)
            {
                foreach( var item in connector.DendriticCache)
                {
                    var parts = item.Key.Split('-');

                    if( parts.Length != 3 && parts[0] != null && parts[1] != null && parts[2] != null)
                    {
                        throw new Exception();
                    }
                    int i = Convert.ToInt32(parts[0]);
                    int j = Convert.ToInt32(parts[1]);
                    int k = Convert.ToInt32(parts[2]);

                    int offset = 3;

                    BlockBehaviourManager.InitDendriticConnectionForConnector(i, j, k, item.Value[0], item.Value[1], item.Value[2]);
                    BlockBehaviourManager.InitDendriticConnectionForConnector(i, j, k, item.Value[0 + offset * 1], item.Value[1 + offset * 1], item.Value[2 + offset * 1]);
                    BlockBehaviourManager.InitDendriticConnectionForConnector(i, j, k, item.Value[0 + offset * 2], item.Value[1 + offset * 2], item.Value[2 + offset * 2]);
                    BlockBehaviourManager.InitDendriticConnectionForConnector(i, j, k, item.Value[0 + offset * 3], item.Value[1 + offset * 3], item.Value[2 + offset * 3]);

                    Console.WriteLine("Adding connection from Cache : intX :" + intX.ToString() +" intY : "+ intY.ToString() + " Cache Dendritic A :" + i.ToString() + " B: " + j.ToString() + " C :" + k.ToString() );
                }                

                return;
            }

            XmlDocument document = new XmlDocument();
            string dendriteDocumentPath = "C:\\Users\\depint\\Desktop\\Hentul\\SecondOrderMemory\\Schema Docs\\ConnectorSchema.xml";


            if (!File.Exists(dendriteDocumentPath))
            {
                throw new FileNotFoundException(dendriteDocumentPath);
            }

            document.Load(dendriteDocumentPath);

            using (XmlNodeList columns = document.GetElementsByTagName("Column"))
            {

                var numColumns = columns.Count;

                for (int i = 0; i < numColumns; i++)
                {//Column
                    neuronCounter = 0;

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

                        int a = Convert.ToInt32(node.Attributes[0]?.Value);
                        int b = Convert.ToInt32(node.Attributes[1]?.Value);
                        int c = Convert.ToInt32(node.Attributes[2]?.Value);

                        Console.WriteLine("Dendritic A :" + a.ToString() + " B: " + b.ToString() + " C :" + c.ToString());

                        var proximalNodes = node.ChildNodes;

                        var neuronNodes = proximalNodes.Item(0)
                            .SelectNodes("Neuron");

                        if (neuronNodes.Count != 4)
                        {
                            throw new InvalidOperationException("Invalid Number of Neuronal Connections defined for Neuron" + a.ToString() + b.ToString() + c.ToString());
                        }

                        int[] arr = new int[neuronNodes.Count * 3];
                        int index = 0;

                        foreach (XmlNode neuron in neuronNodes)
                        {
                            //ProximalConnection
                            if (neuron?.Attributes?.Count != 3)
                            {
                                throw new InvalidOperationException("Number of Attributes in Neuronal Node is not 3");
                            }

                            int e = Convert.ToInt32(neuron.Attributes[0].Value);
                            int f = Convert.ToInt32(neuron.Attributes[1].Value);
                            int g = Convert.ToInt32(neuron.Attributes[2].Value);

                            //Money Shot!!!
                            BlockBehaviourManager.InitDendriticConnectionForConnector(a, b, c, e, f, g);

                            Console.WriteLine(" Adding Connection from Schema : Dendritic A :" + a.ToString() + " B: " + b.ToString() + " C :" + c.ToString());

                            arr[index++] = e;
                            arr[index++] = f;
                            arr[index++] = g;
                        }

                        string key = a.ToString() + "-"+ b.ToString() + "-" + c.ToString();

                        if (!connector.DendriticCache.TryGetValue(key, out var conn))
                        {
                            connector.DendriticCache.Add(key, arr);
                        }
                        else
                        {
                            Console.WriteLine("AddDendriticSchema : Should not be Trying to add invalid cache entry for the same neuron");
                        }

                        neuronCounter++;
                    }
                }
            }
        }

        public void ReadDendriticSchema()
        {

        }


        public void ReadAxonalSchema(int intX, int intY)
        {
            if (connector.AxonalCache.Count != 0)
            {
                foreach (var item in connector.AxonalCache)
                {
                    var parts = item.Key.Split('-');

                    if (parts.Length != 3 && parts[0] != null && parts[1] != null && parts[2] != null)
                    {
                        throw new Exception();
                    }
                    int i = Convert.ToInt32(parts[0]);
                    int j = Convert.ToInt32(parts[1]);
                    int k = Convert.ToInt32(parts[2]);

                    int offset = 3;

                    BlockBehaviourManager.InitAxonalConnectionForConnector(i, j, k, item.Value[0], item.Value[1], item.Value[2]);
                    BlockBehaviourManager.InitAxonalConnectionForConnector(i, j, k, item.Value[0 + offset * 1], item.Value[1 + offset * 1], item.Value[2 + offset * 1]);
                    BlockBehaviourManager.InitAxonalConnectionForConnector(i, j, k, item.Value[0 + offset * 2], item.Value[1 + offset * 2], item.Value[2 + offset * 2]);
                    BlockBehaviourManager.InitAxonalConnectionForConnector(i, j, k, item.Value[0 + offset * 3], item.Value[1 + offset * 3], item.Value[2 + offset * 3]);

                    Console.WriteLine("Loading connection From Cache : ", i, j, k);

                }

                return;
            }

            XmlDocument document = new XmlDocument();
            string axonalDocumentPath = "C:\\Users\\depint\\Desktop\\Hentul\\SecondOrderMemory\\Schema Docs\\AxonalSchema.xml";

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

                Console.WriteLine("Axonal X :" + x.ToString() + " Y: " + y.ToString() + " Z :" + z.ToString());

                XmlNodeList axonList = connection.ChildNodes;

                int[] arr = new int[axonList.Count * 3];
                int index = 0;

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

                        arr[index++] = i;
                        arr[index++] = j;
                        arr[index++] = k;

                        Console.WriteLine("New Connection From Schema Doc",x,y,z,i,j,k);
                    }
                    catch (Exception e)
                    {
                        int bp = 1;
                        throw new InvalidDataException("ReadAXonalSchema : Invalid Data , Tryign to add new Connections to cache");
                    }                    

                    neuronCounter++;
                }

                string key = x.ToString() + "-" + y.ToString() + "-" + z.ToString();

                if (!connector.AxonalCache.TryGetValue(key, out var conn))
                {
                    connector.AxonalCache.Add(key, arr);
                }
                else
                {
                    Console.WriteLine("AddAxonalSchema : Should not be Trying to add invalid cache entry for the same neuron");
                }
            }
        }
    }
}
