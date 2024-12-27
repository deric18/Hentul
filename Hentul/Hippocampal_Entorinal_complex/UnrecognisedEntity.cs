

using System.Security.Cryptography;

/// Author : Deric Pinto
namespace Hentul.Hippocampal_Entorinal_complex
{    

    public class UnrecognisedEntity
    {
        public bool IsObjectIdentified { get; private set; }

        public List<Sensation_Location> ObjectSnapshot { get; set; }


        //KeyValuePair<Sensei ID , List< Location ID's >
        public Dictionary<string, List<string>> MatchedSensations { get; set; }

        public string Label { get; internal set; }

        public UnrecognisedEntity()
        {
            Label = string.Empty;
            ObjectSnapshot = new List<Sensation_Location>();
            MatchedSensations = new Dictionary<string, List<string>>();
        }

        public bool AddNewSenei(Sensation_Location sensei)
        {
            if (sensei.CenterPosition == null)
                return false;

            foreach (var item in sensei.sensLoc.Values)
            {
                if (item.Value.Count <= 0)
                    return false;
            }

            bool toReturn = false;
                     
            if(ObjectSnapshot.Count == 0)
            {
                ObjectSnapshot.Add(sensei);
                return true;
            }

            Tuple<int,int> tuple = Sensation_Location.CompareObjectSenseiAgainstListPercentage(sensei, ObjectSnapshot, null, true, true);

            if (tuple.Item1 == 100)
            {
                if(MatchedSensations.TryGetValue(sensei.Id, out var locStrings))
                {
                    locStrings.AddRange(sensei.sensLoc.Keys.ToList<string>());
                    toReturn = true;
                }
                else
                {
                    MatchedSensations.Add(sensei.Id, sensei.sensLoc.Keys.ToList<String>());
                    toReturn = true;
                }
            }
            else if (tuple.Item1 != 100)
            {
                ObjectSnapshot.Add(sensei);
                toReturn = true;
            }

            if(toReturn == false)
            {
                bool bp = true;
            }

            return toReturn;
        }

        public void SetObjectLabel(string name)
        {
            if(Label == string.Empty)
                Label = string.Empty;
        }
    }
}
