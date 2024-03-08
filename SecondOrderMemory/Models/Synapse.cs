using SecondOrderMemory.BehaviourManagers;

namespace SecondOrderMemory.Models
{
    public class Synapse
    {
        public Guid Id { get; private set; }
        public string AxonalNeuronId { get; private set; }
        public string DendronalNeuronalId { get; private set; }
        public ulong lastFiredCycle {  get; private set; }

        public ulong lastPredictedCycle { get; private set; }
        public bool IsActive { get; private set; }

        public ConnectionType cType { get; private set; }
        
        public UInt16 PredictiveHitCount { get; private set; }

        public UInt16 FiringHitCount { get; private set; }

        private uint _strength {  get; set; } 

        public Synapse(string axonalNeuronId, string dendriticNeuronId, ulong lastFiredCycle, uint strength, ConnectionType cType, bool isActive = false)
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
                IsActive = true;
                PredictiveHitCount = BlockBehaviourManager.DISTALNEUROPLASTICITY;
                FiringHitCount = 0;
            }
            else
            {
                IsActive = isActive;
                PredictiveHitCount = 1;
                FiringHitCount = 0;
            }
        }

        public uint GetStrength()
        {            
            return _strength;
        }

        public void IncrementHitCount()
        {
            if(PredictiveHitCount >= BlockBehaviourManager.DISTALNEUROPLASTICITY) 
            {
                if(IsActive == false) 
                    IsActive = true;

                FiringHitCount++;

                this._strength += 1;
                this.lastFiredCycle = BlockBehaviourManager.CycleNum;
            }
            else
            {
                PredictiveHitCount++;

                this.lastPredictedCycle = BlockBehaviourManager.CycleNum;
            }
        }
    }
}
