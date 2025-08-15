namespace SecondOrderMemory.Models
{
    public class Synapse
    {

        public Guid Id { get; private set; }

        // Key - Predicted Neuronal ID , Value - Predicted Object Label
        public List<Prediction> SupportedLabels { get; private set; }   // Holds only unique Object Label , Does not even hold Schema Bassed Connection or APical / Temporal connections.

        public string AxonalNeuronId { get; private set; }

        public string DendronalNeuronalId { get; private set; }

        public ulong lastFiredCycle { get; private set; }

        public ulong lastPredictedCycle { get; private set; }

        public bool IsActive { get; private set; }

        public ConnectionType cType { get; private set; }

        public uint PredictiveHitCount { get; private set; }

        public uint FiringHitCount { get; private set; }

        private uint _strength { get; set; }


        // Only for Non Schema Based Distal Dendritic Connections!
        public Synapse(string axonalNeuronId, string dendriticNeuronId, string objectLabel, ulong currentCycle, uint strength, ConnectionType cType, bool isActive = false)
        {
            Id = Guid.NewGuid();

            AxonalNeuronId = axonalNeuronId;

            DendronalNeuronalId = dendriticNeuronId;

            this.lastFiredCycle = currentCycle;

            this.lastPredictedCycle = currentCycle;

            this._strength = strength;

            this.IsActive = isActive;

            PredictiveHitCount = isActive ? BlockBehaviourManagerSOM.DISTALNEUROPLASTICITY : 0;

            FiringHitCount = 0;

            this.cType = cType;

            if (objectLabel == null)    //Schema Based Connection   Todo: You can use a counter here as a consistency checker.
            {
                if (cType != ConnectionType.DISTALDENDRITICNEURON)
                {
                    throw new InvalidOperationException("Cannot create  a NON-Distal Dendritic Connection as a Distal connection!");
                }                
            }

            SupportedLabels = new();
        }

        // Only for Schema Based Proximal Connections / Temporal / Apical
        public Synapse(string axonalNeuronId, string dendriticNeuronId, ulong currentCycle, uint strength, ConnectionType cType, bool isActive = false)
        {
            Id = Guid.NewGuid();

            AxonalNeuronId = axonalNeuronId;

            DendronalNeuronalId = dendriticNeuronId;

            this.lastFiredCycle = currentCycle;

            this.lastPredictedCycle = currentCycle;

            this._strength = strength;

            this.IsActive = isActive;

            PredictiveHitCount = isActive ? BlockBehaviourManagerSOM.DISTALNEUROPLASTICITY : 0;

            FiringHitCount = 0;

            this.cType = cType;

            if (cType == ConnectionType.DISTALDENDRITICNEURON)
            {
                throw new InvalidOperationException("Cannot create Distal Dendritic Connection as a Schema Based Connection! Need an Object Label to create a Distal Dendritic Connection!");
            }

            SupportedLabels = new();
        }

        internal bool IsSynapseActive() => IsActive;

        public bool IsMultiObjectSupported => SupportedLabels.Count > 1;

        internal bool CheckForSupportedLabel(string objectLabel) =>
            SupportedLabels.Any(prediction => prediction.ObjectLabel.ToLower() == objectLabel.ToLower());

        public bool AddNewObjectLabel(string label)
        {
            if (string.IsNullOrEmpty(label))
                throw new InvalidOperationException("ERROR : Synapse() : Label cannot beempty");

            if (!CheckForSupportedLabel(label))
            {
                SupportedLabels.Add(new Prediction(label));
                return true;
            }

            return false;
        }

        internal bool PopulatePrediction(string objectLabel, List<string> nextNeuronIds)
        {
            if (SupportedLabels.Count == 0)
            {
                SupportedLabels.Add(new Prediction(objectLabel, nextNeuronIds));
                return true;
            }

            foreach (var label in SupportedLabels)
            {
                if (label.ObjectLabel.ToLower() == objectLabel.ToLower())
                {
                    //Found Matching Object Label!  Adding new batch of Prediction Set for the same Object Label!
                    label.PopulatePrediction(label.ObjectLabel, nextNeuronIds);
                    return true;
                }
            }

            //Couldnt Find Matching Object Label ! Adding new Object Label
            SupportedLabels.Add(new Prediction(objectLabel, nextNeuronIds));

            return true;
        }

        public void IncrementHitCount(ulong currentCycleNum)
        {

            if (PredictiveHitCount >= BlockBehaviourManagerSOM.DISTALNEUROPLASTICITY)
            {
                if (IsActive == false)
                    IsActive = true;

                FiringHitCount++;

                _strength++;

                lastFiredCycle++;
            }
            else
            {
                PredictiveHitCount++;
            }
        }

        public void Print()
        {
            Console.WriteLine(" Axonal Neuron ID : " + AxonalNeuronId);
            Console.WriteLine(" Dendronal Neuron ID : " + DendronalNeuronalId);

            foreach (var label in SupportedLabels)
            {
                Console.WriteLine("Label : " + label);
                Console.WriteLine(" Last Fired Cycle : " + lastFiredCycle);
                Console.WriteLine(" Active : " + (IsActive ? "Yes" : "NO"));
                Console.WriteLine(" Stringeth : " + _strength.ToString());
                Console.WriteLine(" Connection Type : " + cType.ToString());
                Console.WriteLine(" Firing Hit Count : " + FiringHitCount + " \n ");
            }
        }

        public uint GetStrength() => _strength;        
    }
}
