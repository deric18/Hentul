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

        public static bool CompareSensei(Sensation_Location sense1, Sensation_Location sense2)
        {            

            foreach (var item in sense1.Snapshot)
            {
                if (sense2.Snapshot.TryGetValue(item.Key, out var s))
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


        public static bool CompareSensei(List<Sensation_Location> sensei1ist, Sensation_Location sensei)
        {            
            foreach (var item in sensei1ist)
            {
                if (CompareSensei(sensei, item))
                    return false;
            }

            return true;
        }

        public static bool CompareSensei(List<Sensation_Location> sensei1ist1, List<Sensation_Location> sensei1ist2)
        {
            foreach (var item1 in sensei1ist1)
            {
                foreach( var item2 in sensei1ist2)
                {
                    if(CompareSensei(item1, item2) == false)
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }
}
