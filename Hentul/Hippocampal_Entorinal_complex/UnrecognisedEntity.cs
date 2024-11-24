
/// Author : Deric Pinto
 
namespace Hentul.Hippocampal_Entorinal_complex
{    

    public class UnrecognisedEntity : BaseObject
    {
        public bool IsObjectIdentified { get; private set; }

        public List<Sensation_Location> ObjectSnapshot { get; set; }

        public string Label { get; private set; }

        public UnrecognisedEntity()
        {
            Label = string.Empty;
            ObjectSnapshot = new List<Sensation_Location>();
        }

        public bool AddNewSenei(Sensation_Location sensei)
        {
            bool isValid = false;

            foreach( var item in sensei.sensLoc.Values)
            {
                if(item.Value.Count > 1)
                    isValid = true;
            }

            if (isValid == false)
            {
                return false;
            }

            if(ObjectSnapshot.Count == 0)
            {
                ObjectSnapshot.Add(sensei);
                return true;
            }

            int match = Sensation_Location.CompareObjectSenseiAgainstListPercentage(sensei, ObjectSnapshot, true, true);


            if (match != 100)
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
