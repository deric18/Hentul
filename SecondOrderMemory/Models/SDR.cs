namespace SecondOrderMemory.Models
{
    public class SDR : IEquatable<SDR>
    {
        public int Length { get; private set; }
        public int Breadth { get; private set; }
        public List<Position> ActiveBits { get; private set; }        

        public iType InputPatternType { get; private set; }

        public int Size() => Length * Breadth;

        public SDR(int length, int breadth)
        {
            this.Length = length;
            this.Breadth = breadth;
            this.ActiveBits = new List<Position>();
        }

        public SDR(int length, int breadth, List<Position> activeBits, iType inputPatternType)
        {
            this.Length = length;
            this.Breadth = breadth;
            this.ActiveBits = activeBits;
            InputPatternType = inputPatternType;
        }

        public SDR(string s, int length, int breadth)
        {
            Length = length;
            Breadth = breadth;
            ActiveBits = new List<Position>();
            for (int i = 0; i < length; i++)
                for (int j = 0; j < breadth; j++)
                {

                }
        }

        public bool IsUnionTo(SDR uniounTo)
        {
            if (Length != uniounTo.Length || uniounTo.Breadth != uniounTo.Breadth || ActiveBits.Count > uniounTo.ActiveBits.Count)
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

        public float CompareFloat(SDR firingPattern)
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
            }

            return true;
        }
    }
}
