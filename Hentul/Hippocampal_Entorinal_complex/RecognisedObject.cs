
using Common;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.Intrinsics.X86;

/// Author : Deric Pinto
namespace Hentul.Hippocampal_Entorinal_complex
{
    public class RecognisedObject : BaseObject
    {
        public List<Sensation_Location> ObjectSnapshot { get; set; }
        public string Label { get; private set; }

        public RecognisedObject(string name)
        {
            Label = name;
            ObjectSnapshot = new List<Sensation_Location>();
        }

        public RecognisedObject(UnrecognisedObject unrec) 
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
