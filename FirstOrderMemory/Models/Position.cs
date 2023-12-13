using FirstOrderMemory.BehaviourManagers;

namespace FirstOrderMemory.Models
{
    public class Position
    {
        public int X { get; private set; }
        public int Y { get; private set; }

        public int Z { get; private set; }

        public Position(int x, int y, int z = 0)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public override string ToString()
        {           
                return X.ToString() + "-" + Y.ToString() + "-" + Z.ToString();
        }

        public Neuron ConvertPosToNeuron(Position pos)
        {
            return BlockBehaviourManager.GetNeuronFromPosition(pos);
        }

        public static Neuron ConvertStringPosToNeuron(string posString)
        {
            var parts = posString.Split('-');

            if(parts.Length != 3 ) 
            {
                return BlockBehaviourManager.GetBlockBehaviourManager().Columns[Convert.ToInt32(parts[0]), Convert.ToInt32(parts[1])].Neurons[Convert.ToInt32(parts[3])];
            }

            return null;
        }

        public static string ConvertIKJtoString(int i, int j, int k)
        {
            return i.ToString() + "-" + j.ToString() + "-" + k.ToString();
        }
    }
}
