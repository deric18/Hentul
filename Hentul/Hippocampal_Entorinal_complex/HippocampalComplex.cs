using HentulWinforms;

namespace Hentul.Hippocampal_Entorinal_complex
{
    public class HippocampalComplex
    {
        public Dictionary<string, RecognisedObject>? Objects { get; private set; }

        public UnrecognisedObject? CurrentObject { get; private set; }

        private NetworkMode networkMode;

        public HippocampalComplex()
        {

            Objects = new Dictionary<string, RecognisedObject>();
            CurrentObject = new UnrecognisedObject();
            networkMode = NetworkMode.TRAINING;
        }        

        public void SetNetworkModeToTraining()
        {
            networkMode = NetworkMode.TRAINING;               
        }

        public void SetNetworkModeToPrediction()
        {
            networkMode = NetworkMode.PREDICTION;
        }



        public void ProcessCurrentPatternForObject(Sensation_Location sensei)
        {
            if (CurrentObject == null)
                CurrentObject = new UnrecognisedObject();

            //Keep storing <Location , ActiveBit> -> KVPs under Training Mode , If Under PredictionMode keep updating CurrentObject based on the best prediction
            
        }




        public void FinishedProcessingImage()
        {
            //Store object into list and move on to next object
        }

        public bool IsObjectIdentified => CurrentObject == null ? false : CurrentObject.IsObjectIdentified;
    }
}
