namespace Hentul.Hippocampal_Entorinal_complex
{

    using Common;    
    using System.Xml;

    public class HippocampalComplex
    {

        #region  VARIABLES & CONSTRUCTORS

        public Dictionary<string, RecognisedEntity>? Objects { get; private set; }

        public UnrecognisedEntity? CurrentObject { get; private set; }

        public Position CurrentPosition { get; private set; }

        public Position[] BoundaryPositions { get; private set; }

        private NetworkMode networkMode;

        public int NumberOfITerationsToConfirmation { get; private set; }

        private string backupDir;

        public HippocampalComplex()
        {

            Objects = new Dictionary<string, RecognisedEntity>();
            CurrentObject = new UnrecognisedEntity();
            networkMode = NetworkMode.TRAINING;
            NumberOfITerationsToConfirmation = 6;
            backupDir = "C:\\Users\\depint\\source\\repos\\Hentul\\Hentul\\BackUp\\";
        }

        #endregion


        // Gets called by Orchestrator.
        /// <summary>
        /// Need to define specfic limits for 
        /// 1. object sensei count
        /// 2. Number of cycles 
        /// </summary>
        /// <param name="sensei"></param>
        /// <param name="prediction"></param>
        /// <exception cref="InvalidOperationException"></exception>
        public Position ProcessCurrentPatternForObject(ulong cycleNum, Sensation_Location sensei, Sensation_Location? prediction = null)
        {
            Position toReturn = null;

            if (networkMode == NetworkMode.TRAINING)
            {
                // Keep storing <Location , ActiveBit> -> KVPs under CurrentObject.
                if (CurrentObject.AddNewSenei(sensei) == false)
                {
                    int breakpoint1 = 1; // pattern already added.
                }

                var matchingObjectList = ParseAllKnownObjectsForIncomingPattern(sensei);

                if (matchingObjectList.Count != 0)
                {
                    //Object identified even in training phase

                    int breakpint = 1;

                }
                else
                {
                    int breapoint2 = 1;
                    CurrentObject.AddNewSenei(sensei);
                }
            }
            else if (networkMode == NetworkMode.PREDICTION)
            {
                if (Objects.Count == 0)
                {
                    throw new InvalidOperationException("Object Cannot be null under Prediction Mode");
                }

                /* If Under PredictionMode 
                1. Parse known Objects for this location , 
                    if any recognised , put them in priority queue , 
                    else run through prediction 
                2. If any object is recognised, 
                    enter verification Mode and verify atleast 6 more positions.
                3. Else , continue with more inputs from Orchestrator. Record number of iterations to confirmation
                */

                var matchingObjectList = ParseAllKnownObjectsForIncomingPattern(sensei);


                if (matchingObjectList.Count != 0)
                {
                    foreach( var matchingObject in matchingObjectList)
                    {

                    }
                }
                else
                {
                    throw new InvalidOperationException("Object did not match none of Recognised Objects ");
                }




            }

            return toReturn;
        }

        public void DoneWithTraining()
        {
            ConvertUnrecognisedObjectToRecognisedObject();
        }

        private List<RecognisedEntity> ParseAllKnownObjectsForIncomingPattern(Sensation_Location sensei)
        {
            List<RecognisedEntity> setAsideList = new List<RecognisedEntity>();

            foreach (var obj in Objects.Values)
            {
                if (obj.CheckPatternMatchPercentage(sensei) != 0)
                {
                    setAsideList.Add(obj);
                }
            }

            return setAsideList;
        }

        private void Backup()
        {
            var xmlDocument = new XmlDocument();

            xmlDocument.LoadXml("<HippocampalEntorhinalComplex></HippocampalEntorhinalComplex>");

            string filename = "HC-EC.xml";

            foreach (var kvp in Objects)
            {
                foreach (var sensei in kvp.Value.ObjectSnapshot)
                {
                   foreach(var item in sensei.Sensation)
                    {
                        foreach( var pos in item.Value)
                        {

                        }
                    }
                }
            }

            //var distalNode = xmlDocument.CreateNode(XmlNodeType.Element, kvp.Key, string.Empty);

            //var blockIdElement = xmlDocument.CreateElement("BlockID", string.Empty);

            //var sourceNeuronElement = xmlDocument.CreateElement("SourceNeuronID", string.Empty);

            //sourceNeuronElement.InnerText = distalSynapse.Value.AxonalNeuronId.ToString();

            //var targetNeuronElement = xmlDocument.CreateNode(XmlNodeType.Element, "TargetNeuronID", string.Empty);

            //targetNeuronElement.InnerText = distalSynapse.Value.DendronalNeuronalId.ToString();

            //distalNode.AppendChild(sourceNeuronElement);
            //distalNode.AppendChild(targetNeuronElement);

            //xmlDocument?.DocumentElement?.AppendChild(distalNode);
        
            xmlDocument?.Save(backupDir + filename);            

        }

        private void Restore()
        {

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
            if (CurrentObject.ObjectSnapshot.Count == 0)
            {
                throw new InvalidOperationException("Cannot Transform empty object!");
            }

            RecognisedEntity newObject = new RecognisedEntity(CurrentObject);
        }



        public void FinishedProcessingImage()
        {
            //Store object into list and move on to next object
        }

        public bool IsObjectIdentified => CurrentObject == null ? false : CurrentObject.IsObjectIdentified;
    }
}
