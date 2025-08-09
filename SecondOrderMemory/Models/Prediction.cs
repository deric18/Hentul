using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecondOrderMemory.Models
{
    public class Prediction
    {
        public List<string> NextNeuronId { get; set; }      // List of Neuron IDs that will fire after this firing.

        public string ObjectLabel { get; set; }


        public uint HitCount { get; set; }

        public Prediction(List<string> nextNeuronId, string objectLabel)
        {
            NextNeuronId = nextNeuronId;            
            ObjectLabel = objectLabel;
            HitCount = 0;
        }

        public bool AddNewNextNeuronID(string nextNeuronId)
        {
            if (!NextNeuronId.Contains(nextNeuronId))
            {
                NextNeuronId.Add(nextNeuronId);
                return true;
            }
            return false;
        }
    }
}
