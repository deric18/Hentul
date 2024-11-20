namespace Hentul.Hippocampal_Entorinal_complex
{
    using Common;
    using FirstOrderMemory.Models;

    public class Sensation_Location
    {                

        /// <summary>
        /// Key : Location on the Screen
        /// Value : KeyValuePair<int, ActiveBits> Key : BBMID, Value : ActiveBits
        /// </summary>
        public Dictionary<string, KeyValuePair<int, List<Position_SOM>>> sensLoc { get; private set; }

        public Sensation_Location()
        {
            sensLoc = new Dictionary<string, KeyValuePair<int, List<Position_SOM>>>();
        }

        public Sensation_Location(Dictionary<string, KeyValuePair<int, List<Position_SOM>>> sensLoc)
        {
            this.sensLoc = sensLoc;
        }

        public bool AddNewSensationAtThisLocation(string location, KeyValuePair<int, List<Position_SOM>> sensation)
        {
            if(!sensLoc.TryGetValue(location, out var kvp))
            {
                sensLoc.Add(location, sensation);
                return true;
            }

            return false;
        }

        public static int CompareSenseiPercentage(Sensation_Location sourceSensei, Sensation_Location targetSensei)
        {
            int maxMatchPercentage = sourceSensei.sensLoc.Count > targetSensei.sensLoc.Count ? ( ( targetSensei.sensLoc.Count * 1000) / sourceSensei.sensLoc.Count ) : 1000;

            int matchPercentage = 0;

            foreach (var sourceKvp in sourceSensei.sensLoc)
            {
                if (targetSensei.sensLoc.TryGetValue(sourceKvp.Key, out var targetKvpValue))
                {
                    int cycleMatchPercentage = ComparePositionList(sourceKvp.Value.Value, targetKvpValue.Value);

                    matchPercentage += cycleMatchPercentage;

                }
            }

            return matchPercentage * 100  / maxMatchPercentage;
        }

        public static int CompareObjectSenseiAgainstListPercentage(Sensation_Location sensei, List<Sensation_Location> senseiList)
        {
            int total = sensei.sensLoc.Count;
            int maxMatch = 0;

            foreach (var item in senseiList)
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

            foreach (var item in sourceSensei.sensLoc)
            {
                if (targetensei.sensLoc.TryGetValue(item.Key, out var s))
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

        public static int ComparePositionList(List<Position> first, List<Position> second)
        {
            if(first.Count == 0 || second.Count == 0) return 0;



            int match = 0;

            foreach (var pos in first)
            {
                if (second.Contains(pos))
                {
                    match++;
                }
            }


            return ((100 * match) / first.Count);

        }

    }
}
