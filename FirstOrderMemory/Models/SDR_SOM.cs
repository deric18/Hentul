using Common;

namespace FirstOrderMemory.Models
{
    public class SDR_SOM : SDR
    {
        public new List<Position_SOM> ActiveBits;

        public SDR_SOM(int length, int breadth, List<Position_SOM> activeBits, iType type = iType.SPATIAL) : base(length,breadth, type)
        {
            ActiveBits = activeBits;
        }


        public void Sort()
        {
            ActiveBits.Sort();
        }

        public bool IsUnionTo(SDR_SOM smallerSDR)
        {
            if (smallerSDR.ActiveBits == null)
                throw new NullReferenceException();

            if (Length != smallerSDR.Length || smallerSDR.Breadth != smallerSDR.Breadth || ActiveBits.Count < smallerSDR?.ActiveBits.Count)
                return false;

            foreach(var pos in smallerSDR.ActiveBits)
            {
                if(ActiveBits.Where( B => B.X == pos.X && B.Y == pos.Y && B.Z == pos.Z).Count() == 0)
                {
                    return false;
                }
            }

            return true;
        }

        public bool IsUnionTo(SDR_SOM smallerSDR, List<Position_SOM> exceptionList)
        {
            if (smallerSDR.ActiveBits == null)
                throw new NullReferenceException();

            if (Length != smallerSDR.Length || smallerSDR.Breadth != smallerSDR.Breadth || ActiveBits.Count < smallerSDR?.ActiveBits.Count)
                return false;

            List<Position_SOM> activeBitsListWithoutExceptions = new List<Position_SOM>();

            foreach(var item in ActiveBits)
            {
                if(!exceptionList.Where(x => x.X == item.X && x.Y == item.Y).Any())
                {
                    activeBitsListWithoutExceptions.Add(item);
                }
            }

            foreach (var pos in smallerSDR.ActiveBits)
            {
                               
                if (activeBitsListWithoutExceptions.Where(B => B.X == pos.X && B.Y == pos.Y).Count() == 0)       // I  CHECK THE ENTIRE COLUMN FOR THIS PATTERN
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
