using SecondOrderMemory.BehaviourManagers;

namespace SecondOrderMemory.Models
{
    public class Position
    {
        public char W { get; private set; }

        public int X { get; private set; }
        public int Y { get; private set; }

        public int Z { get; private set; }        

        public Position(int x, int y, int z = 0, char w = 'N')
        {
            W = w;
            X = x;
            Y = y;
            Z = z;
            W = w;
        }

        public bool Equals(Position pos)
        {
            return X == pos.X && Y == pos.Y && Z == pos.Z && W == pos.W;
        }

        public override string ToString()
        {           
                return X.ToString() + "-" + Y.ToString() + "-" + Z.ToString() + "-" + W.ToString();
        }

        public Neuron ConvertPosToNeuron(Position pos)
        {
            return BlockBehaviourManager.GetNeuronFromPosition(pos.W, pos.X, pos.Y, pos.Z);
        }

        public static Position ConvertStringToPosition(string key)
        {
            var parts = key.Split('-');
            
            int x = Convert.ToInt32(parts[0]);
            int y = Convert.ToInt32(parts[1]);
            int z = Convert.ToInt32(parts[2]);
            char w = Convert.ToChar(parts[3]);
            return new Position(x, y, z, w);
        }

        public static Neuron ConvertStringPosToNeuron(string posString)
        {         
            var parts = posString.Split('-');                 
            int x = Convert.ToInt32(parts[0]);
            int y = Convert.ToInt32(parts[1]);
            int z = Convert.ToInt32(parts[2]);
            char nType = 'N';
            if (parts.Length == 4)
            {
                nType = Convert.ToChar(parts[3]);
            }

            try
            {
                if (parts.Length != 3 || x > 9 || y > 9 || z > 9)
                {
                    int breakpoint = 1;
                }
               
                return BlockBehaviourManager.GetNeuronFromPosition(nType, x, y, z);
                
            }
            catch (Exception e)
            {
                int bp = 1;
            }
            return null;
        }

        public static string ConvertIKJtoString(int i, int j, int k, char w = 'N')
        {
            if (w == 'N')
                return i.ToString() + "-" + j.ToString() + "-" + k.ToString();
            else if (w == 'T')
                return i.ToString() + "-" + j.ToString() + "-" + k.ToString() + "-" + w;
            else if (w == 'A')
                return i.ToString() + "-" + j.ToString() + "-" + k.ToString() + "-" + w;

            return i.ToString() + "-" + j.ToString() + "-" + k.ToString();
        }
    }
}
