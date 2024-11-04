
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
            if(ObjectSnapshot.Count == 0)
            {
                ObjectSnapshot.Add(sensei);
                return true;
            }
           
            if(Sensation_Location.CompareSenseiBool(ObjectSnapshot, sensei))
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
