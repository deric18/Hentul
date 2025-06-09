

using System.Security.Cryptography;

/// Author : Deric Pinto
namespace Hentul.Hippocampal_Entorinal_complex
{

    public class UnrecognisedEntity : Entity
    {
        public bool IsObjectIdentified { get; private set; }

        public List<Sensation_Location> ObjectSnapshot { get; private set; }

        public List<Sensation> Sensations { get; private set; }

        public SenseType sType { get; set; }


        //KeyValuePair<Sensei ID , List< Location ID's >
        public Dictionary<string, List<string>> MatchedSensations { get; private  set; }

        public string Label { get; internal set; }

        public UnrecognisedEntity()
        {
            Label = string.Empty;
            ObjectSnapshot = new List<Sensation_Location>();
            MatchedSensations = new Dictionary<string, List<string>>();
            Sensations = new();
            sType = SenseType.Unknown;
        }

        public bool AddNewSensationToObject(Sensation sensation)
        {
            if (sType != SenseType.SenseOnly)
                throw new InvalidProgramException("Cannot Add Sensation & Location to a Sense Only Type!");

             return AddNewSensation(sensation);
        }

        private bool AddNewSensation(Sensation sensation)
        {            
            foreach(var sense in Sensations)
            {
                if (sense.CheckForRepetition(sensation))
                {
                    return false;
                }
            }

            Sensations.Add(sensation);

            return true;
        }

        public bool AddNewSenei(Sensation_Location sensei)
        {

            if (sType != SenseType.SenseNLocation)
                throw new InvalidOperationException(" Cannot add new Sensation to Sensation_Location Type!");

            if (sensei.CenterPosition == null)
                return false;

            foreach (var item in sensei.sensLoc.Values)
            {
                if (item.Value.Count <= 0)
                    return false;
            }

            bool toReturn = false;

            if (ObjectSnapshot.Count == 0)
            {
                ObjectSnapshot.Add(sensei);
                return true;
            }

            Tuple<int, int> tuple = Sensation_Location.CompareObjectSenseiAgainstListPercentage(sensei, ObjectSnapshot, null, true, true);

            if (tuple.Item1 == 100)
            {
                if (MatchedSensations.TryGetValue(sensei.Id, out var locStrings))
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

            if (toReturn == false)
            {
                bool bp = true;
            }

            return toReturn;
        }

        public void SetObjectLabel(string name)
        {
            if (Label == string.Empty)
                Label = string.Empty;
        }
    }

    public enum SenseType
    {
        SenseNLocation,
        SenseOnly,
        Unknown
    }
}
