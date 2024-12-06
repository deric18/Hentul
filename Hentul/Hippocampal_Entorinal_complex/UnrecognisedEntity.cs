
/// Author : Deric Pinto
 
namespace Hentul.Hippocampal_Entorinal_complex
{    

    public class UnrecognisedEntity : BaseObject
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
            foreach( var item in sensei.sensLoc.Values)
            {
                if (item.Value.Count <= 1)
                    return false;                                    
            }            

            if(ObjectSnapshot.Count == 0)
            {
                ObjectSnapshot.Add(sensei);
                return true;
            }

            int match = Sensation_Location.CompareObjectSenseiAgainstListPercentage(sensei, ObjectSnapshot, true, true);

            if (match == 100)
            {
                if(MatchedSensations.TryGetValue(sensei.Id, out var locStrings))
                {
                    locStrings.AddRange(sensei.sensLoc.Keys.ToList<string>());
                }
                else
                {
                    MatchedSensations.Add(sensei.Id, sensei.sensLoc.Keys.ToList<String>());
                }
            }
            else if (match != 100)
            {
                ObjectSnapshot.Add(sensei);
                return true;
            }

            return false;
        }

        public void SetObjectLabel(string name)
        {
            if(Label == string.Empty)
                Label = string.Empty;
        }
    }
}
