namespace SecondOrderMemory.Models
{    
    using System.Collections.Generic;
    using System.Linq;
    using Common;

    public static class BBMUtils
    {
        public static bool CheckIfTwoNeuronsHaveAnActiveSynapse(Neuron axonalNeuron, Neuron dendronalNeuron)
        {
            bool toRet = false;

            foreach (var item in dendronalNeuron.ProximoDistalDendriticList)
            {
                if (item.Key == axonalNeuron.NeuronID.ToString() && item.Value.IsActive)
                {

                    if (axonalNeuron.AxonalList.Any(q => q.Key == dendronalNeuron.NeuronID.ToString()))
                    {
                        toRet = true;
                        break;
                    }
                }
            }

            return toRet;
        }

        public static bool CheckIfPositionListHasThisNeuron(List<Position_SOM> columnsThatBurst, Neuron neuron)
        {
            var index = false;

            foreach (var x in columnsThatBurst)
            {
                if (x.X == neuron.NeuronID.X && x.Y == neuron.NeuronID.Y && x.Z == neuron.NeuronID.Z && x.W == neuron.NeuronID.W)
                {
                    index = true;
                    break;
                }
            }

            return index;
        }

        public static bool CheckIfTwoNeuronsAreConnected(Neuron axonalNeuron, Neuron dendriticNeuron)
        {
            bool toRet = false;

            if (axonalNeuron.AxonalList.TryGetValue(dendriticNeuron.NeuronID.ToString(), out Synapse synapse))
            {
                toRet = true;
            }           
            
            return toRet;
        }

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
            neuronList.Any(x => x.NeuronID.X == neuron.NeuronID.X && x.NeuronID.Y == neuron.NeuronID.Y && x.NeuronID.Z == neuron.NeuronID.Z);        

        public static bool CheckifNeuronListStringHAsNeuron(List<string> stringlist, Neuron neuron)
        {
            foreach (var item in stringlist)
            {
                if (item.Equals(neuron.NeuronID))
                {
                    return true;
                }
            }
            return false;
        }

        public static bool DoListsHaveAnyOverlap(List<Neuron> list1, List<Neuron> list2)
        {
            foreach (var neuron in list1)
            {
                foreach (var item in list2)
                {
                    if(neuron.NeuronID.Equals(item.NeuronID))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Returns a new list that contains all the neurons in the second list that are not present in first list.
        /// </summary>                
        public static List<Neuron> GetNonOverlappingNeuronsFromSecondList(List<Neuron> predictedList, List<Neuron> firingList)
        {
            List<Neuron> toRet = new List<Neuron>();

            foreach (var neuron in predictedList)
            {
                if(firingList.Contains(neuron) == false && neuron.nType == NeuronType.NORMAL)
                {
                    toRet.Add(neuron);
                }
            }

            return toRet;
        }


        public static List<Neuron> PerformLeftOuterJoinBetweenTwoLists(List<Neuron> predictedList, List<Neuron> firingList)
        {
            List<Neuron> toRet = new List<Neuron>();

            foreach (var neuron in predictedList)
            {
                if (firingList.Contains(neuron) == false && neuron.nType == NeuronType.NORMAL)
                {
                    toRet.Add(neuron);
                }
            }

            return toRet;
        }        

        public static List<Position_SOM> GetNonOverlappingPositionsFromSecondList(List<Position_SOM> predictedList, List<Position_SOM> firingList)
        {
            List<Position_SOM> toRet = new List<Position_SOM>();

            foreach (var neuron in predictedList)
            {
                if (firingList.Contains(neuron) == false && neuron.W == 'N')
                {
                    toRet.Add(neuron);
                }
            }

            return toRet;
        }
    }
}
