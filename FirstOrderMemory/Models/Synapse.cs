using FirstOrderMemory.BehaviourManagers;

namespace FirstOrderMemory.Models
{
    public class Synapse
    {
        public Guid Id { get; private set; }
        public string SourceNeuronId { get; private set; }
        public string TargetNeuronId { get; private set; }
        public ulong lastFiredCycle {  get; private set; }
        
        public uint strength {  get; private set; } 

        public Synapse(string sourceNeuronId, string targetNeuronId, ulong lastFiredCycle, uint strength)
        {
            Id = Guid.NewGuid();
            SourceNeuronId = sourceNeuronId;
            TargetNeuronId = targetNeuronId;
            this.lastFiredCycle = lastFiredCycle;
            this.strength = strength;
        }

        public void IncrementStrength()
        {
            this.strength += 1;
            this.lastFiredCycle = BlockBehaviourManager.GetBlockBehaviourManager().CycleNum;
        }
    }
}
