namespace FirstOrderMemory.BehaviourManagers
{
    using FirstOrderMemory.Models;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;


    public static class BBMUtils
    {
        public static int CheckIfPositionListHasThisPosition(List<Position_SOM> columnsThatBurst, Position_SOM neuronID)
        {
            var index = -1;

            int i = 0;

            foreach (var x in columnsThatBurst)
            {
                if (x.X == neuronID.X && x.Y == neuronID.Y && x.Z == neuronID.Z && x.W == neuronID.W)
                {
                    index = i;
                    break;
                }
                i++;
            }

            return index;
        }

        public static bool ListContains(List<Position_SOM> x, Position_SOM? y)
        {
            if (x == null || y == null)
                return false;

            foreach (var item in x)
            {
                if (item.W == y.W && item.X == y.X && item.Y == y.Y && item.Z == y.Z)
                {
                    return true;
                }
            }

            return false;
        }

        public static bool CheckNeuronListHasThisNeuron(List<Neuron> neuronList, Neuron neuron) =>
            neuronList.Any(x => x.NeuronID.X == neuron.NeuronID.X && x.NeuronID.Y == neuron.NeuronID.Y && x.NeuronID.Z == neuron.NeuronID.Y && x.NeuronID.Z == neuron.NeuronID.Z);

    }
}
