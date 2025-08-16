namespace SecondOrderMemory.Models
{
    public class Prediction
    {
        public Lazy<List<List<string>>> NextNeuronIdLists { get; set; }      // List of Neuron IDs that will fire after this firing.

        public string ObjectLabel { get; set; }     // Unique for every Synapse

        public uint HitCount { get; set; }       

        public Prediction(string objectLabel)
        {
            this.ObjectLabel = objectLabel;            
        }

        public Prediction(string objectLabel, List<string> nextNeuronIDs)
        {
            NextNeuronIdLists = new();
            NextNeuronIdLists.Value.Add(nextNeuronIDs);            
            ObjectLabel = objectLabel;
            HitCount = 0;
        }

        internal string CheckNGetMatchingLabel(string neuronId)
        {
            if (NextNeuronIdLists == null || NextNeuronIdLists.Value == null || NextNeuronIdLists.Value.Count == 0)
                return null;
            foreach (var neuronList in NextNeuronIdLists.Value)
            {
                if (neuronList.Contains(neuronId))
                {                    
                    return ObjectLabel;
                }
            }
            return null;
        }
        
        internal bool PopulatePrediction(string objectLabel, List<string> nextNeuronIdList)
        {
            if(objectLabel != ObjectLabel || string.IsNullOrEmpty(objectLabel))
                throw new InvalidOperationException("Prediction : PopulatePrediction : Object Labels Should Match!");
            
            if(nextNeuronIdList == null || nextNeuronIdList.Count == 0)
                throw new ArgumentException("Prediction : PopulatePrediction : Next Neuron ID List cannot be null or empty!");

            if (!Compare(nextNeuronIdList))
            {
                NextNeuronIdLists.Value.Add(nextNeuronIdList);
                return true;
            }

            return false;
        }

        private bool Compare(List<string> nextNeurons)
        {
            if (NextNeuronIdLists == null || NextNeuronIdLists.Value == null)
                return false;

            foreach (var neuronList in NextNeuronIdLists.Value)
            {
                if (neuronList.SequenceEqual(nextNeurons))
                    return true;
            }
            return false;
        }
    }
}
