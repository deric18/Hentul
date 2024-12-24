namespace Hentul.Hippocampal_Entorinal_complex
{
    using Common;
    using FirstOrderMemory.Models;    

    public class HippocampalComplex
    {
        Graph graphHandler;

        static HippocampalComplex _hippocalCampalAccesor;

        private HippocampalComplex() 
        {
            graphHandler = Graph.GetInstance();    
        }

        public static HippocampalComplex GetInstance()
        {
            if(_hippocalCampalAccesor == null)
            {
                _hippocalCampalAccesor = new HippocampalComplex();
            }

            return _hippocalCampalAccesor;
        }


    }

    public enum Direction
    {
        RIGHT,
        LEFT,
        UP,
        DOWN,
        UNDEFINED
    }
}
