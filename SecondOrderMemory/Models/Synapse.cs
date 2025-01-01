namespace SecondOrderMemory.Models
{
    public class Synapse
    {
        public Guid Id { get; private set; }

        public HashSet<string> SupportedLabels { get; private set; }

        public string AxonalNeuronId { get; private set; }

        public string DendronalNeuronalId { get; private set; }

        public Dictionary<string, ulong> lastFiredCycle {  get; private set; }

        public Dictionary<string,ulong> lastPredictedCycle { get; private set; }

        public Dictionary<string, bool> IsActive { get; private set; }

        public ConnectionType cType { get; private set; }
        
        public Dictionary<string, int> PredictiveHitCount { get; private set; }

        public Dictionary<string, int> FiringHitCount { get; private set; }

        private Dictionary<string, uint> _strength {  get; set; } 

        public Synapse(string axonalNeuronId, string dendriticNeuronId, Dictionary<string, ulong> lastFiredCycle, Dictionary<string, uint> strength, ConnectionType cType, bool isActive = false, string objectLabel = null)
        {
            Id = Guid.NewGuid();
            AxonalNeuronId = axonalNeuronId;
            DendronalNeuronalId = dendriticNeuronId;
            this.lastFiredCycle = lastFiredCycle;
            this.lastPredictedCycle = lastFiredCycle;
            this._strength = strength;
            this.cType = cType;
            if (cType.Equals(ConnectionType.APICAL) || cType.Equals(ConnectionType.TEMPRORAL))
            {
                IsActive = new Dictionary<string, bool>();
                PredictiveHitCount = new Dictionary<string, int>();
                FiringHitCount = new Dictionary<string, int>();
            }
            else
            {
                IsActive = new Dictionary<string, bool>();
                PredictiveHitCount = new Dictionary<string, int>();
                FiringHitCount = new Dictionary<string, int>();
            }

            if (objectLabel != null)
            {
                SupportedLabels = new HashSet<string>() { objectLabel };
            }
        }

        public void Print()
        {
            Console.WriteLine(" Axonal Neuron ID : " + AxonalNeuronId);
            Console.WriteLine(" Dendronal Neuron ID : " + DendronalNeuronalId);

            foreach (var label in SupportedLabels)
            {
                Console.WriteLine("Label : " + label);                
                Console.WriteLine(" Last Fired Cycle : " + lastFiredCycle[label]);
                Console.WriteLine(" Active : " + (IsActive[label] ? "Yes" : "NO"));
                Console.WriteLine(" Stringeth : " + _strength[label].ToString());
                Console.WriteLine(" Connection Type : " + cType.ToString());
                Console.WriteLine(" Firing Hit Count : " + FiringHitCount[label].ToString() + " \n ");
            }
        }

        public uint GetStrength(string label)
        {            
            return _strength[label];
        }

        public void IncrementHitCount(ulong currentCycleNum, string label)
        {

            if (DendronalNeuronalId.ToString().Equals("5-3-0-N") && AxonalNeuronId.ToString().Equals("0-2-0-N"))
            {
                bool breakpoint = false;
                breakpoint = true;
            }

            if (PredictiveHitCount.TryGetValue(label, out var predictivehitcount))
            {
                if (predictivehitcount >= BlockBehaviourManager.DISTALNEUROPLASTICITY)
                {
                    if(IsActive[label] == false)
                        IsActive[label] = true;

                    FiringHitCount[label]++;

                    _strength[label]++;

                    lastFiredCycle[label]++;
                }
                else
                {
                    PredictiveHitCount[label]++;
                }
            }
        }
    }
}
