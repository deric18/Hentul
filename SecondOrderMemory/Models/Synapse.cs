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

        public bool IsActive { get; private set; }

        public Dictionary<string, ConnectionType> cType { get; private set; }
        
        public Dictionary<string, int> PredictiveHitCount { get; private set; }

        public Dictionary<string, int> FiringHitCount { get; private set; }

        private Dictionary<string, uint> _strength {  get; set; }

        public Synapse(string axonalNeuronId, string dendriticNeuronId, ulong currentCycle, uint strength, ConnectionType cType, bool isActive = false, string objectLabel = null)
        {
            Id = Guid.NewGuid();
            AxonalNeuronId = axonalNeuronId;
            DendronalNeuronalId = dendriticNeuronId;

            if (objectLabel == null)    //Schema Based Connection
            {
                if (cType == ConnectionType.DISTALDENDRITICNEURON)
                {
                    throw new InvalidOperationException("Cannot create Distal Dendritic Connection as a Schema Based Connection! Need an Object Label to create a Distal Dendritic Connection!");
                }

                SupportedLabels = new HashSet<string>();

                this.lastFiredCycle = new Dictionary<string, ulong>();

                this.lastPredictedCycle = new Dictionary<string, ulong>();

                this._strength = new Dictionary<string, uint>();

                IsActive = true;                

                PredictiveHitCount = new Dictionary<string, int>();

                FiringHitCount = new Dictionary<string, int>();

                this.cType = new Dictionary<string, ConnectionType>();
                this.cType.Add(BlockBehaviourManager.DEFAULT_SYNAPSE, cType);
            }
            else                        // Object Based Connection
            {
                if (cType != ConnectionType.DISTALDENDRITICNEURON)
                {
                    throw new InvalidOperationException("Cannot create  a NON-Distal Dendritic Connection as a Distal connection!");
                }

                SupportedLabels = new HashSet<string>() { objectLabel };

                this.lastFiredCycle = new Dictionary<string, ulong>();
                this.lastFiredCycle.Add(objectLabel, currentCycle);

                this.lastPredictedCycle = new Dictionary<string, ulong>();
                this.lastPredictedCycle.Add(objectLabel, currentCycle);

                this._strength = new Dictionary<string, uint>();
                this._strength.Add(objectLabel, strength);

                IsActive = false;                

                PredictiveHitCount = new Dictionary<string, int>();
                PredictiveHitCount.Add(objectLabel, 0);

                FiringHitCount = new Dictionary<string, int>();
                FiringHitCount.Add(objectLabel, 0);

                this.cType = new Dictionary<string, ConnectionType>();
                this.cType.Add(objectLabel, cType);
            }
        }

        internal bool IsSynapseActive()  => IsActive;

        public bool IsMultiSynapse => SupportedLabels.Count > 1;

        internal bool AnyStale(ulong currentCycle) => IsActive;        

        public void Print()
        {
            Console.WriteLine(" Axonal Neuron ID : " + AxonalNeuronId);
            Console.WriteLine(" Dendronal Neuron ID : " + DendronalNeuronalId);

            foreach (var label in SupportedLabels)
            {
                Console.WriteLine("Label : " + label);                
                Console.WriteLine(" Last Fired Cycle : " + lastFiredCycle[label]);
                Console.WriteLine(" Active : " + ( IsActive ? "Yes" : "NO"));
                Console.WriteLine(" Stringeth : " + _strength[label].ToString());
                Console.WriteLine(" Connection Type : " + cType.ToString());
                Console.WriteLine(" Firing Hit Count : " + FiringHitCount[label].ToString() + " \n ");
            }
        }

        public uint GetStrength(string label) =>        
            _strength.TryGetValue(label, out var strength) ? strength : 0;

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
                    if(IsActive == false)
                        IsActive = true;

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
