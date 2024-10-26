namespace Hentul.Hippocampal_Entorinal_complex
{

    using Common;

    public class HippocampalComplex
    {
        public Dictionary<string, RecognisedObject>? Objects { get; private set; }

        public UnrecognisedObject? CurrentObject { get; private set; }

        public Position CurrentPosition { get; private set; }

        public Position[] BoundaryPositions { get; private set; } 

        private NetworkMode networkMode;
        
        public int NumberOfITerationsToConfirmation { get; private set; }

        public HippocampalComplex()
        {

            Objects = new Dictionary<string, RecognisedObject>();
            CurrentObject = new UnrecognisedObject();
            networkMode = NetworkMode.TRAINING;
            NumberOfITerationsToConfirmation = 6;
        }

        public NetworkMode GetCurrentNetworkMode() => networkMode;

        public void UpdateCurrenttPosition(Position pos)
        {
            CurrentPosition = pos;
        }

        public void SetNetworkModeToTraining()
        {
            networkMode = NetworkMode.TRAINING;               
        }

        public void SetNetworkModeToPrediction()
        {

            ConvertUnrecognisedObjectToRecognisedObject();
            networkMode = NetworkMode.PREDICTION;
        }

        private void ConvertUnrecognisedObjectToRecognisedObject()
        {
            if(CurrentObject.ObjectSnapshot.Count == 0)
            {
                throw new InvalidOperationException("Cannot Transform empty object!");
            }

            RecognisedObject newObject = new RecognisedObject(CurrentObject);
        }


        // Gets called by Orchestrator.
        /// <summary>
        /// Need to define specfic limits for 
        /// 1. object sensei count
        /// 2. Number of cycles 
        /// </summary>
        /// <param name="sensei"></param>
        /// <param name="prediction"></param>
        /// <exception cref="InvalidOperationException"></exception>
        public void ProcessCurrentPatternForObject(int cycleNum, Sensation_Location sensei, Sensation_Location? prediction = null)
        {

            if (networkMode == NetworkMode.TRAINING)
            {                

                // Keep storing <Location , ActiveBit> -> KVPs under CurrentObject.
                if(CurrentObject.AddNewSenei(sensei) == false)
                {
                    int breakpoint = 1; // pattern already added.
                }

                ParseAllKnownObjectsForIncomingPattern();




            }
            else if (networkMode == NetworkMode.PREDICTION)
            {
                if (Objects.Count == 0)
                {
                    throw new InvalidOperationException("Object Cannot be null under Prediction Mode");
                }

                throw new NotImplementedException();

                /* If Under PredictionMode 
                1. Parse known Objects for this location , 
                    if any recognised , put them in priority queue , 
                    else run through prediction 
                2. If any object is recognised, 
                    enter verification Mode and verify atleast 6 more positions.
                3. Else , continue with more inputs from Orchestrator. Record number of iterations to confirmation
                */
            }
        }

        private void ParseAllKnownObjectsForIncomingPattern()
        {
            throw new NotImplementedException();
        }

        public void FinishedProcessingImage()
        {
            //Store object into list and move on to next object
        }

        public bool IsObjectIdentified => CurrentObject == null ? false : CurrentObject.IsObjectIdentified;
    }
}
