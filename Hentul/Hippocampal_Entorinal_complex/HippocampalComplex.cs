namespace Hentul.Hippocampal_Entorinal_complex
{

    using Common;
    using System.Xml;

    public class HippocampalComplex
    {

        #region  VARIABLES & CONSTRUCTORS

        public Dictionary<string, RecognisedEntity> Objects { get; private set; }

        public UnrecognisedEntity? CurrentObject { get; private set; }

        public Position CurrentPosition { get; private set; }

        public Position[] BoundaryPositions { get; private set; }

        private NetworkMode networkMode;

        private List<RecognisedEntity> matchingObjectList;

        private List<RecognisedEntity> rejectedObjectList;

        private RecognisedEntity currentmatchingObject;

        private bool IsBeingVerified;

        public int currentIterationTOConfirmation;
        public int NumberOfITerationsToConfirmation { get; private set; }

        private string backupDir;

        private List<string> objectlabellist;

        private int imageIndex;

        public HippocampalComplex(string firstLabel)
        {

            Objects = new Dictionary<string, RecognisedEntity>();
            CurrentObject = new UnrecognisedEntity();
            CurrentObject.Label = firstLabel;
            networkMode = NetworkMode.TRAINING;
            NumberOfITerationsToConfirmation = 6;
            matchingObjectList = new List<RecognisedEntity>();
            rejectedObjectList = new List<RecognisedEntity>();
            currentmatchingObject = null;
            IsBeingVerified = false;
            currentIterationTOConfirmation = 0;

            backupDir = "C:\\Users\\depint\\source\\repos\\Hentul\\Hentul\\BackUp\\";
            objectlabellist = new List<string>
            {
                "Apple",
                "Ananas",
                "Watermelon",
                "JackFruit",
                "Grapes"
            };
            imageIndex = 1;
        }

        #endregion


        #region PUBLIC API


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
            Position nextMotorOutput = null;

            if (networkMode == NetworkMode.TRAINING)
            {
                // Keep storing <Location , ActiveBit> -> KVPs under CurrentObject.                

                if (CurrentObject.AddNewSenei(sensei) == false)
                {
                    throw new InvalidOperationException();
                }
            }
            else if (networkMode == NetworkMode.PREDICTION)
            {
                if (Objects.Count == 0)
                {
                    throw new InvalidOperationException("Object Cannot be null under Prediction Mode");
                }

               
                if (IsBeingVerified == false)
                {
                    matchingObjectList = ParseAllKnownObjectsForIncomingPattern(sensei);

                    if(matchingObjectList.Count > 0)
                    {
                        IsBeingVerified = true;
                        currentmatchingObject = matchingObjectList.FirstOrDefault();
                    }                    
                }
                else if(IsBeingVerified == true)
                {
                    if (matchingObjectList.Count == 0)
                    {
                        throw new InvalidOperationException("Should Never Happne!");
                    }

                    if (currentIterationTOConfirmation == 0)
                    {
                        currentIterationTOConfirmation++;
                        // Verify();
                    }
                    else if(currentIterationTOConfirmation > 0) {
                    {
                        //Verify() 
                        //prepare expected sensation and location and compare them with location and without location and see how good the matches are.
                    }

                    if (currentIterationTOConfirmation < NumberOfITerationsToConfirmation)
                    {
                        nextMotorOutput = currentmatchingObject.GetNextSenseiToVerify();
                            //cache sensation and Locations.
                    }                    
                }
            }

            return nextMotorOutput;
        }

        public void DoneWithTraining()
        {
            ConvertUnrecognisedObjectToRecognisedObject();

            CurrentObject = new UnrecognisedEntity();

            if (CurrentObject.Label == string.Empty)
            {
                CurrentObject.Label = objectlabellist[imageIndex];
                imageIndex++;
            }
        }

        public NetworkMode GetCurrentNetworkMode() => networkMode;
        public bool IsObjectIdentified => CurrentObject == null ? false : CurrentObject.IsObjectIdentified;

        #endregion


        #region BACKUP & RESTORE

        private void Backup()
        {
            var xmlDocument = new XmlDocument();

            xmlDocument.LoadXml("<HippocampalEntorhinalComplex></HippocampalEntorhinalComplex>");

            string filename = "HC-EC.xml";

            foreach (var recognisedObject in Objects)
            {
                var objectNode = xmlDocument.CreateNode(XmlNodeType.Element, recognisedObject.Value.Label, string.Empty);

                foreach (var sensei in recognisedObject.Value.ObjectSnapshot)
                {
                    foreach (var dictKVP in sensei.sensLoc)
                    {
                        var sensationLocationElement = xmlDocument.CreateElement("SensationLocation");

                        sensationLocationElement.SetAttribute("Position", dictKVP.Key);

                        sensationLocationElement.SetAttribute("BBMID", dictKVP.Value.Key.ToString());

                        foreach (var positionItem in dictKVP.Value.Value)
                        {
                            var xmlNode = xmlDocument.CreateElement("Position");

                            xmlNode.SetAttribute("X", positionItem.X.ToString());
                            xmlNode.SetAttribute("Y", positionItem.Y.ToString());
                            xmlNode.SetAttribute("Z", positionItem.Z.ToString());

                            sensationLocationElement.AppendChild(xmlNode);
                        }

                        objectNode.AppendChild(objectNode);
                    }
                }

                xmlDocument.AppendChild(objectNode);
            }

            xmlDocument?.Save(backupDir + filename);

        }

        private void Restore()
        {
            string filename = "HC-EC.xml";

            var xmlDocument = new XmlDocument();

            xmlDocument.LoadXml(backupDir + filename);
        }

        #endregion


        #region PRIVATE HELPER METHODS

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

        private void UpdateCurrenttPosition(Position pos)
        {
            CurrentPosition = pos;
        }

        public void SetNetworkModeToTraining()
        {
            networkMode = NetworkMode.TRAINING;
        }

        public void SetNetworkModeToPrediction()
        {
            networkMode = NetworkMode.PREDICTION;
        }


        private void ConvertUnrecognisedObjectToRecognisedObject(string label)
        {
            if (CurrentObject.ObjectSnapshot.Count == 0)
            {
                throw new InvalidOperationException("Cannot Transform empty object!");
            }

            RecognisedEntity newObject = new RecognisedEntity(CurrentObject);

            Objects.Add(label, newObject);

            CurrentObject = new UnrecognisedEntity();
        }


        private void ConvertUnrecognisedObjectToRecognisedObject()
        {
            if (CurrentObject.ObjectSnapshot.Count == 0)
            {
                throw new InvalidOperationException("Cannot Transform empty object!");
            }

            if (CurrentObject.Label == string.Empty)
            {
                CurrentObject.Label = objectlabellist[imageIndex];
                imageIndex++;
            }

            RecognisedEntity newObject = new RecognisedEntity(CurrentObject);

            Objects.Add(newObject.Label, newObject);
        }

        private void FinishedProcessingImage()
        {
            //Store object into list and move on to next object
        }

        #endregion        
    }
}
