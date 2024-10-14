/// Author : Deric Pinto
/// 

namespace Hentul.Hippocampal_Entorinal_complex
{
    public class UnrecognisedObject : BaseObject
    {
        public bool IsObjectIdentified { get; private set; }

        internal UnrecognisedObject()
        {
            IsObjectIdentified = false; 
        }
    }
}
