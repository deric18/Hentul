namespace Hentul.Hippocampal_Entorinal_complex
{
    using Common;    

    public class HippocampalComplex
    {
        Graph globalFrame;

        static HippocampalComplex _hippocalCampalAccesor;


        private List<RecognisedEntity> matchingObjectList;

        public Position[] BoundaryPositions { get; private set; }

        RecognitionState currentObjetState;

        private NetworkMode networkMode;

        private HippocampalComplex() 
        {
            globalFrame = Graph.GetInstance();

            matchingObjectList = new List<RecognisedEntity>();

            networkMode = NetworkMode.TRAINING;
            currentObjetState = RecognitionState.None;
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

            // Once the sensation comes in ds




            return sensationGotAdded;
        }

    }
}
