
/// Author : Deric Pinto
 
namespace Hentul.Hippocampal_Entorinal_complex
{

    using Common;

    public class UnrecognisedObject : BaseObject
    {
        public bool IsObjectIdentified { get; private set; }

        public Dictionary<string, List<Position>> ObjectPattern { get; set; }

        public string Label { get; private set; }

        public UnrecognisedObject()
        {
            Label = string.Empty;
            ObjectPattern = new Dictionary<string, List<Position>>();
        }

        public bool AddNewSenei(string location, List<Position> positions)
        {
            if (!ObjectPattern.ContainsKey(location))
            {
                ObjectPattern.Add(location, positions);

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
