/// Author : Deric Pinto

namespace FirstOrderMemory.Models.Hippocampal_Entorinal_complex
{
    using Common;

    public class GridCell
    {       
        Dictionary<string, BaseObject> ObjectPosition { get; set; }

        public GridCell() 
        { 
            ObjectPosition = new Dictionary<string, BaseObject>();
        }

    }
}
