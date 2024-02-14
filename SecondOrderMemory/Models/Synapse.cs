using SecondOrderMemory.BehaviourManagers;

namespace SecondOrderMemory.Models
{
    public class Synapse
    {
        public Guid Id { get; private set; }
        public string AxonalNeuronId { get; private set; }
        public string DendronalNeuronalId { get; private set; }
        public ulong lastFiredCycle {  get; private set; }

        public ConnectionType cType { get; private set; }
        
        private uint _strength {  get; set; } 

        public Synapse(string axonalNeuronId, string dendriticNeuronId, ulong lastFiredCycle, uint strength, ConnectionType cType)
        {
            Id = Guid.NewGuid();
            AxonalNeuronId = axonalNeuronId;
            DendronalNeuronalId = dendriticNeuronId;
            this.lastFiredCycle = lastFiredCycle;
            this._strength = strength;
            this.cType = cType;
        }

        public uint GetStrength()
        {
            lastFiredCycle = BlockBehaviourManager.CycleNum;
            return _strength;
        }

        public void IncrementStrength()
        {
            this._strength += 1;
            this.lastFiredCycle = BlockBehaviourManager.CycleNum;
        }
    }
}
