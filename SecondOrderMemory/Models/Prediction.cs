using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecondOrderMemory.Models
{
    public class Prediction
    {
        public string NextNeuronId { get; set; }

        public string ObjectLabel { get; set; }


        public uint HitCount { get; set; }

        public Prediction(string nnId, string objectLabel)
        {
            NextNeuronId = nnId;
            ObjectLabel = objectLabel;
            HitCount = 0;
        }
    }
}
