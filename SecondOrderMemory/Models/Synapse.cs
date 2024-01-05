using SecondOrderMemory.BehaviourManagers;

namespace SecondOrderMemory.Models
{
    public class Synapse
    {
        public Guid Id { get; private set; }
        public string SourceNeuronId { get; private set; }
        public string TargetNeuronId { get; private set; }
        public ulong lastFiredCycle {  get; private set; }

        public ConnectionType cType { get; private set; }
        
        private uint _strength {  get; set; } 

        public Synapse(string sourceNeuronId, string targetNeuronId, ulong lastFiredCycle, uint strength, ConnectionType cType)
        {
            Id = Guid.NewGuid();
            SourceNeuronId = sourceNeuronId;
            TargetNeuronId = targetNeuronId;
            this.lastFiredCycle = lastFiredCycle;
            this._strength = strength;
            this.cType = cType;
        }

        public uint GetStrength()
        {
            lastFiredCycle = BlockBehaviourManager.GetBlockBehaviourManager().CycleNum;
            return _strength;
        }

        public void IncrementStrength()
        {
            this._strength += 1;
            this.lastFiredCycle = BlockBehaviourManager.GetBlockBehaviourManager().CycleNum;
        }
    }
}
