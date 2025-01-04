namespace SecondOrderMemory.Models
{
    public class Synapse
    {                

        public Guid Id { get; private set; }

        public HashSet<string> SupportedLabels { get; private set; }                // Holdss only unique Object Label , Does not even hold Schema Bassed Connection or APical / Temporal connections.

        public string AxonalNeuronId { get; private set; }

        public string DendronalNeuronalId { get; private set; }

        public Dictionary<string, ulong> lastFiredCycle {  get; private set; }

        public Dictionary<string,ulong> lastPredictedCycle { get; private set; }

        public bool IsActive { get; private set; }

        public Dictionary<string, ConnectionType> cType { get; private set; }       
        
        public Dictionary<string, uint> PredictiveHitCount { get; private set; }

        public Dictionary<string, uint> FiringHitCount { get; private set; }

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
                lastFiredCycle.Add(BlockBehaviourManager.DEFAULT_SYNAPSE, 0);

                this.lastPredictedCycle = new Dictionary<string, ulong>();
                lastPredictedCycle.Add(BlockBehaviourManager.DEFAULT_SYNAPSE, 0);

                this._strength = new Dictionary<string, uint>();
                _strength.Add(BlockBehaviourManager.DEFAULT_SYNAPSE, 0);

                IsActive = true;

                PredictiveHitCount = new Dictionary<string, uint>();
                PredictiveHitCount.Add(BlockBehaviourManager.DEFAULT_SYNAPSE, BlockBehaviourManager.DISTALNEUROPLASTICITY);

                FiringHitCount = new Dictionary<string, uint>();
                FiringHitCount.Add(BlockBehaviourManager.DEFAULT_SYNAPSE, 0);

                this.cType = new Dictionary<string, ConnectionType>();
                this.cType.Add(BlockBehaviourManager.DEFAULT_SYNAPSE, cType);
            }
            else                        // Object Based Connection
            {
                if (!(cType == ConnectionType.DISTALDENDRITICNEURON || cType == ConnectionType.AXONTONEURON))
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

                PredictiveHitCount = new Dictionary<string, uint>();
                PredictiveHitCount.Add(objectLabel, 0);

                FiringHitCount = new Dictionary<string, uint>();
                FiringHitCount.Add(objectLabel, 0);

                this.cType = new Dictionary<string, ConnectionType>();
                this.cType.Add(objectLabel, cType);
            }
        }

        internal bool AddNewDistalSynapse(ulong currentCycle, ConnectionType cType, string objectLabel, uint strength = 0, bool isActive = false)
        {            
            if (cType != ConnectionType.DISTALDENDRITICNEURON || objectLabel == string.Empty)
            {
                throw new InvalidOperationException("Cannot create  a NON-Distal Dendritic Connection as a Distal connection!");
            }

            bool sucess = false;

            if (SupportedLabels.Add(objectLabel) == false)
                return false;
            
            if(lastFiredCycle.TryGetValue(objectLabel, out var _) == false) 
                this.lastFiredCycle.Add(objectLabel, currentCycle);
            else
            {
                return false;
            }

            if (lastPredictedCycle.TryGetValue(objectLabel, out var _) == false)
                this.lastPredictedCycle.Add(objectLabel, currentCycle);
            else
            { return false; }

            if (_strength.TryGetValue(objectLabel, out var _))
                this._strength.Add(objectLabel, strength);
            else
            { return false; }

            if (PredictiveHitCount.TryGetValue(objectLabel, out var _) == false)
                PredictiveHitCount.Add(objectLabel, 0);
            else { return false; }

            if (FiringHitCount.TryGetValue(objectLabel, out var _) == false)
                FiringHitCount.Add(objectLabel, 0);
            else
            { return false; }

            this.cType.Add(objectLabel, cType);

            return true;
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

            if (DendronalNeuronalId.ToString().Equals("55-2-1-N"))
            { 
                bool breakpoint = false; 
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
