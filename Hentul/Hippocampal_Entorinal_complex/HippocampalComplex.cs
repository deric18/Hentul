namespace Hentul.Hippocampal_Entorinal_complex
{
    using Common;
    using FirstOrderMemory.Models;    

    public class HippocampalComplex
    {
        Graph graphHandler;

        static HippocampalComplex _hippocalCampalAccesor;

        RecognitionState currentObjetState;
        private NetworkMode networkMode;

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

        public Position2D GetNextPositiontoDifferentiateObject(Position2D location, Sensation sensation)
        {
            Position2D nextPos = null;

            return nextPos;
        }

        public RecognitionState IsObjectIdentified() => currentObjetState;

        public bool LearnNewObject(Position2D location, Sensation sensation)
        {
            if(networkMode == NetworkMode.PREDICTION)
            {
                return false;
            }

            bool sensationGotAdded = false;






            return sensationGotAdded;
        }

    }
}
