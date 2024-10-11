namespace Hentul.Hippocampal_Entorinal_complex
{
    public class HippocampalComplex
    {
        Dictionary<string, BaseObject>? Objects { get; set; }

        UnrecognisedObject? CurrentObject { get; set; }

        public HippocampalComplex? HCEComplex { get; private set; }

        public HippocampalComplex()
        {

            Objects = new Dictionary<string, BaseObject>();
            CurrentObject = null;
        }

        internal bool DetectObject(Sensation_Location sensei)
        {
            if (Objects.Count == 0)
                return false;

            bool objectDetected = false;

            foreach (var obj in Objects)
            {
                //match sensei with obj
            }

            return objectDetected;
        }

        internal void ProcessPatternForObject(Sensation_Location sensei)
        {
            if (CurrentObject == null)
                CurrentObject = new UnrecognisedObject();

            //Next pattern wrt this current object.
            //Decide whether to store or throw it away.

        }

        public void FinishedProcessingImage()
        {
            //Store object into list and move on to next object
        }


    }
}
