/// Author : Deric Pinto
namespace Hentul.Hippocampal_Entorinal_complex
{
    public class RecognisedEntity : BaseObject
    {
        public List<Sensation_Location> ObjectSnapshot { get; set; }
        public string Label { get; private set; }

        public RecognisedEntity(string name)
        {
            Label = name;
            ObjectSnapshot = new List<Sensation_Location>();
        }

        public RecognisedEntity(UnrecognisedEntity unrec) 
        {
            Label = unrec.Label;
            ObjectSnapshot = unrec.ObjectSnapshot;
        }

        public int CheckPatternMatchPercentage(Sensation_Location sensei)
        {            
            return Sensation_Location.CompareObjectSenseiAgainstListPercentage(sensei, ObjectSnapshot);
        }        
    }

}
