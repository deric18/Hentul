using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        
        internal void PopulatePrediction(string objectLabel, List<String> nextNeuronIdList)
        {
            if(objectLabel != ObjectLabel)
                throw new InvalidOperationException("Prediction : PopulatePrediction : Object Labels Should Match!");
            
            NextNeuronIdLists.Value.Add(nextNeuronIdList);            
        }
    }
}
