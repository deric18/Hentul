using Common;

namespace Hentul.Hippocampal_Entorinal_complex
{

    public class Sensation_Location
    {
        public string ID { get; set; }
        public Dictionary<int, List<Position>> Snapshot { get; set; }

        public Sensation_Location(int cycleNumber, Dictionary<int, List<Position>> snapshot)
        {
            this.Snapshot = snapshot;
        }

        public static int CompareSenseiPercentage(Sensation_Location sourceSensei, Sensation_Location targetensei)
        {
            int total = sourceSensei.Snapshot.Count;
            int match = 0;

            foreach (var item in sourceSensei.Snapshot)
            {
                if (targetensei.Snapshot.TryGetValue(item.Key, out var s))
                {
                    foreach (var position in s)
                    {
                        if (item.Value.Contains(position))
                        {
                            match++;
                        }
                    }
                }
            }

            return (int)((match * 100) / sourceSensei.Snapshot.Count);
        }

        public static int CompareObjectSenseiAgainstListPercentage(Sensation_Location sensei, List<Sensation_Location> sensei1ist)
        {
            int total = sensei.Snapshot.Count;
            int maxMatch = 0;

            foreach (var item in sensei1ist)
            {
                int match = CompareSenseiPercentage(sensei, item);
                if (match > maxMatch)
                    maxMatch = match;
            }

            return maxMatch;
        }

        public static int CompareSenseilistAgainstListPercentage(List<Sensation_Location> sensei1ist1, List<Sensation_Location> sensei1ist2)
        {
            int matchPercentage = 0;

            foreach (var item1 in sensei1ist1)
            {
                int match = CompareObjectSenseiAgainstListPercentage(item1, sensei1ist2);

                if ( match > matchPercentage)
                {
                    matchPercentage = match;
                }
            }

            return matchPercentage;
        }


        public static bool CompareSenseiBool(List<Sensation_Location> sensei1ist, Sensation_Location sensei)
        {
            foreach (var item in sensei1ist)
            {
                if (CompareSenseiBool(sensei, item))
                    return false;
            }

            return true;
        }

        public static bool CompareSenseiBool(List<Sensation_Location> sensei1ist1, List<Sensation_Location> sensei1ist2)
        {
            foreach (var item1 in sensei1ist1)
            {
                foreach (var item2 in sensei1ist2)
                {
                    if (CompareSenseiBool(item1, item2) == false)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public static bool CompareSenseiBool(Sensation_Location sourceSensei, Sensation_Location targetensei)
        {

            foreach (var item in sourceSensei.Snapshot)
            {
                if (targetensei.Snapshot.TryGetValue(item.Key, out var s))
                {
                    foreach (var position in s)
                    {
                        if (!item.Value.Contains(position))
                        {
                            return false;
                        }
                    }
                }
            }

            return true;
        }
    }
}
