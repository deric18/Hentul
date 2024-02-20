using Common;

namespace SecondOrderMemory.Models
{
    public class SDR_SOM : SDR
    {
        public new List<Position_SOM> ActiveBits;

        public SDR_SOM(int length, int breadth, List<Position_SOM> activeBits, iType type = iType.SPATIAL) : base(length,breadth, type)
        {
            ActiveBits = activeBits;
        }

        public bool IsUnionTo(SDR_SOM uniounTo)
        {
            if (uniounTo.ActiveBits == null)
                throw new NullReferenceException();

            if (Length != uniounTo.Length || uniounTo.Breadth != uniounTo.Breadth || ActiveBits.Count > uniounTo?.ActiveBits.Count)
                return false;

            ActiveBits.OrderByDescending(x => x.X);

            uniounTo.ActiveBits.OrderByDescending(y => y.X);

            int counter = 0;
            foreach (var pos in ActiveBits)
            {
                Position uPos = uniounTo.ActiveBits[counter];
                if ((pos.X == uPos.X && pos.Y == uPos.Y) || uPos.X < pos.X)
                {
                    counter++;
                    continue;
                }
                else if (pos.X < uPos.X)
                {
                    return false;
                }
            }

            return true;
        }

        public override float CompareFloat(SDR firingPattern)
        {
            // first pattern is always the firing pattern and second pattern is the predicted pattern

            float matchFloat = 0;
            float unmatchFloat = 0;
            uint totalBits = (uint)(this.ActiveBits.Count + firingPattern.ActiveBits.Count);
            uint MatchingBits = 0;
            uint UnmatchingBits = 0;
            bool flag = false;

            foreach (var item in this.ActiveBits)
            {

                foreach (var item1 in firingPattern.ActiveBits)
                {

                    if (item.Equals(item1))
                    {
                        MatchingBits++;
                        flag = true;
                        break;
                    }
                }

                if (!flag)
                    UnmatchingBits++;

                flag = false;

            }


            matchFloat = (MatchingBits / totalBits) * 100;
            unmatchFloat = (unmatchFloat / totalBits) * 100;

            return matchFloat;
        }

        public bool Equals(SDR y)
        {
            if (this.Length == y.Length && this.Breadth == y.Breadth && this.ActiveBits.Count == y.ActiveBits.Count)
            {
                for (int i = 0; i < this.ActiveBits.Count; i++)
                {
                    if (this.ActiveBits[i] != y.ActiveBits[i])
                        return false;
                }

                if (InputPatternType.Equals(y.InputPatternType))
                {
                    Console.WriteLine("WARNING :: SDR: Incorrect iType in SDR being compared against each other");
                }
            }

            return true;
        }
    }
}
