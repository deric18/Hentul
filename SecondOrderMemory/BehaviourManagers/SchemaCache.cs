namespace SecondOrderMemory.Models
{
    using System;
    using System.IO;
    using System.Xml;

    /// <summary>
    /// Converts XML schemas to a compact binary format on first use, then loads from binary
    /// on subsequent runs. Eliminates the 1.4 GB XML DOM load that dominated Init() time.
    ///
    /// Binary layout (both files): flat sequence of int[6] tuples — 24 bytes per connection record.
    ///   Dendritic: (neuron.X, neuron.Y, neuron.Z,  proximal.X, proximal.Y, proximal.Z)
    ///   Axonal:    (source.X, source.Y, source.Z,  target.X,   target.Y,   target.Z)
    /// </summary>
    internal static class SchemaCache
    {
        /// <summary>
        /// Ensures a binary cache of DendriticSchemaSOM.xml exists, creating it from the XML if needed.
        /// Reads only the first 2 ProximalConnections per Neuron (matching original loading logic).
        /// </summary>
        internal static void EnsureDendriticBinary(string xmlPath, string binPath)
        {
            if (File.Exists(binPath)) return;

            Console.WriteLine("[SchemaCache] Building dendritic binary cache from XML (one-time, may take a minute)...");

            string tmp = binPath + ".tmp";
            try
            {
                using (var bw = new BinaryWriter(File.Open(tmp, FileMode.Create, FileAccess.Write, FileShare.None)))
                {
                    var settings = new XmlReaderSettings { IgnoreWhitespace = true, IgnoreComments = true };
                    using var reader = XmlReader.Create(xmlPath, settings);

                    int a = 0, b = 0, c = 0;
                    int connCount = 0;

                    while (reader.Read())
                    {
                        if (reader.NodeType != XmlNodeType.Element) continue;

                        if (reader.Name == "Neuron")
                        {
                            a = int.Parse(reader.GetAttribute("X"));
                            b = int.Parse(reader.GetAttribute("Y"));
                            c = int.Parse(reader.GetAttribute("Z"));
                            connCount = 0;
                        }
                        else if (reader.Name == "ProximalConnections" && connCount < 2)
                        {
                            int e = int.Parse(reader.GetAttribute("X"));
                            int f = int.Parse(reader.GetAttribute("Y"));
                            int g = int.Parse(reader.GetAttribute("Z"));

                            bw.Write(a); bw.Write(b); bw.Write(c);
                            bw.Write(e); bw.Write(f); bw.Write(g);

                            connCount++;
                        }
                    }
                }

                File.Move(tmp, binPath);
                Console.WriteLine("[SchemaCache] Dendritic binary cache written successfully.");
            }
            catch
            {
                if (File.Exists(tmp)) File.Delete(tmp);
                throw;
            }
        }

        /// <summary>
        /// Ensures a binary cache of AxonalSchema-SOM.xml exists, creating it from the XML if needed.
        /// Reads only the first 2 Neuron children per AxonalConnection (matching original loading logic).
        /// </summary>
        internal static void EnsureAxonalBinary(string xmlPath, string binPath)
        {
            if (File.Exists(binPath)) return;

            Console.WriteLine("[SchemaCache] Building axonal binary cache from XML (one-time, may take a minute)...");

            string tmp = binPath + ".tmp";
            try
            {
                using (var bw = new BinaryWriter(File.Open(tmp, FileMode.Create, FileAccess.Write, FileShare.None)))
                {
                    var settings = new XmlReaderSettings { IgnoreWhitespace = true, IgnoreComments = true };
                    using var reader = XmlReader.Create(xmlPath, settings);

                    int x = 0, y = 0, z = 0;
                    int connCount = 0;

                    while (reader.Read())
                    {
                        if (reader.NodeType != XmlNodeType.Element) continue;

                        if (reader.Name == "AxonalConnection")
                        {
                            x = int.Parse(reader.GetAttribute("X"));
                            y = int.Parse(reader.GetAttribute("Y"));
                            z = int.Parse(reader.GetAttribute("Z"));
                            connCount = 0;
                        }
                        else if (reader.Name == "Neuron" && connCount < 2)
                        {
                            int i = int.Parse(reader.GetAttribute("X"));
                            int j = int.Parse(reader.GetAttribute("Y"));
                            int k = int.Parse(reader.GetAttribute("Z"));

                            bw.Write(x); bw.Write(y); bw.Write(z);
                            bw.Write(i); bw.Write(j); bw.Write(k);

                            connCount++;
                        }
                    }
                }

                File.Move(tmp, binPath);
                Console.WriteLine("[SchemaCache] Axonal binary cache written successfully.");
            }
            catch
            {
                if (File.Exists(tmp)) File.Delete(tmp);
                throw;
            }
        }

        /// <summary>Streams all (a,b,c, e,f,g) dendritic tuples from the binary cache.</summary>
        internal static void LoadDendriticBinary(string binPath, Action<int, int, int, int, int, int> handler)
        {
            using var br = new BinaryReader(File.Open(binPath, FileMode.Open, FileAccess.Read, FileShare.Read));
            var stream = br.BaseStream;
            while (stream.Position < stream.Length)
            {
                int a = br.ReadInt32(), b = br.ReadInt32(), c = br.ReadInt32();
                int e = br.ReadInt32(), f = br.ReadInt32(), g = br.ReadInt32();
                handler(a, b, c, e, f, g);
            }
        }

        /// <summary>Streams all (x,y,z, i,j,k) axonal tuples from the binary cache.</summary>
        internal static void LoadAxonalBinary(string binPath, Action<int, int, int, int, int, int> handler)
        {
            using var br = new BinaryReader(File.Open(binPath, FileMode.Open, FileAccess.Read, FileShare.Read));
            var stream = br.BaseStream;
            while (stream.Position < stream.Length)
            {
                int x = br.ReadInt32(), y = br.ReadInt32(), z = br.ReadInt32();
                int i = br.ReadInt32(), j = br.ReadInt32(), k = br.ReadInt32();
                handler(x, y, z, i, j, k);
            }
        }
    }
}
