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

        private Position _cachedPosition;

        private List<RecognisedEntity> rejectedObjectList;

        private RecognisedEntity currentmatchingObject;

        private RecognitionState ObjectState;

        public int currentIterationToConfirmation;
        public int NumberOfITerationsToConfirmation { get; private set; }

        private string backupDir;

        private List<string> objectlabellist;

        private int imageIndex;

        private bool isMock;

        public HippocampalComplex(string firstLabel, bool Ismock = false, NetworkMode nMode = NetworkMode.TRAINING)
        {

            Objects = new Dictionary<string, RecognisedEntity>();
            CurrentObject = new UnrecognisedEntity();
            CurrentObject.Label = firstLabel;
            isMock = Ismock;
            networkMode = nMode;
            NumberOfITerationsToConfirmation = 6;
            matchingObjectList = new List<RecognisedEntity>();
            rejectedObjectList = new List<RecognisedEntity>();
            currentmatchingObject = null;
            ObjectState = RecognitionState.None;
            currentIterationToConfirmation = 0;

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
        public Position ProcessCurrentPatternForObject(ulong cycleNum, Sensation_Location sensei, Sensation_Location? prediction = null, bool isMock = false)
        {
            string objectLabel = null;
            Position toReturn = null;

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

                if (prediction == null)
                    matchingObjectList = ParseAllKnownObjectsForIncomingPattern(sensei);
                else
                {
                    matchingObjectList = ParseAllKnownObjectsForIncomingPattern(sensei);
                }

                if (matchingObjectList.Count > 0)
                {
                    _cachedPosition = Orchestrator.GetCurrentPointerPosition1();

                    ObjectState = RecognitionState.IsBeingVerified;

                    currentmatchingObject = matchingObjectList.FirstOrDefault();

                    Position p = null;

                    while (currentIterationToConfirmation < NumberOfITerationsToConfirmation)
                    {

                        if (currentmatchingObject.Label.ToLower() == "watermelon")
                        {
                            bool bp1 = true;
                        }

                        if (currentIterationToConfirmation == 0)
                        {
                            currentmatchingObject.PrepNextSenseiToVerify(sensei);
                        }
                        else if (currentIterationToConfirmation != 0 && p != null)
                        {
                            currentmatchingObject.PrepNextSenseiToVerify(sensei, p);
                        }

                        if (VerifyObjectSensei(sensei, currentmatchingObject.CurrentComparision))
                        {
                            var index = currentmatchingObject.GetRandomSenseIndexFromRecognisedEntity();        //Random Sensei 
                            p = currentmatchingObject.ObjectSnapshot[index].cursorPosition;                     //Must be ordered first highest X and lowest Y
                            currentmatchingObject.SetSenseiToCurrentComparision(index);

                            if (isMock)
                                return p;

                            Orchestrator.MoveCursorToSpecificPosition(p.X, p.Y);

                            if (prediction != null)
                            {
                                bool breakpoint = true;
                            }

                            //currentmatchingObject.IncrementCurrentComparisionKeyIndex();

                            currentIterationToConfirmation++;

                            //Perform Step 0 , Step 1
                            Tuple<Sensation_Location, Sensation_Location> tuple = ProcessStep1N2FromOrchestrator();
                            
                            sensei = tuple.Item1;
                            prediction = tuple.Item2;
                        }
                        else
                        {
                            //Dint match , Move onto next object if there is , if not then move back to the last position of the matchingobjectlist

                            matchingObjectList.RemoveAt(GetMatchingObjectIndexFromList(matchingObjectList, currentmatchingObject));

                            currentmatchingObject.Clean();

                            p = null;

                            if (matchingObjectList.Count > 0)
                            {
                                //Repeat the whole loop with same input for next Object
                                currentmatchingObject = matchingObjectList[0];
                                currentIterationToConfirmation = 0;
                            }
                            else
                            {
                                //Move cursor to cache position and return empty , hand back control to form.cs

                                Orchestrator.MoveCursorToSpecificPosition(_cachedPosition.X, _cachedPosition.Y);
                                objectLabel = null;
                                ObjectState = RecognitionState.None;
                                currentIterationToConfirmation = 0;
                                currentmatchingObject.Clean();
                                matchingObjectList.Clear();
                                toReturn = null;
                                break;
                            }
                        }
                    }

                    if (currentIterationToConfirmation == NumberOfITerationsToConfirmation)
                    {
                        ObjectState = RecognitionState.Recognised;
                        toReturn = new Position(int.MaxValue, int.MaxValue);
                        objectLabel = currentmatchingObject.Label;
                        currentIterationToConfirmation = 0;
                    }
                }
            }

            return toReturn;
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

        public RecognisedEntity GetCurrentPredictedObject()
        {
            if (ObjectState == RecognitionState.Recognised)
            {
                return currentmatchingObject;
            }
            else
            {
                return null;
            }
        }

        public NetworkMode GetCurrentNetworkMode() => networkMode;

        public bool IsObjectIdentified => CurrentObject == null ? false : CurrentObject.IsObjectIdentified;


        public void LoadMockObject(List<RecognisedEntity> mockrecgs)
        {
            if (isMock == true)
            {
                foreach (var obj in mockrecgs)
                {
                    Objects.Add(obj.Label, obj);
                }
            }
        }

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

        private Tuple<Sensation_Location, Sensation_Location> ProcessStep1N2FromOrchestrator()
        {
            var instance = Orchestrator.GetInstance();
            instance.ProcessStep0();
            var bmp = instance.ConverToEdgedBitmap();
            instance.ProcesStep1(bmp);
            return instance.ProcessStep2ForHC();
        }

        private bool VerifyObjectSensei(Sensation_Location sourceSensei, Sensation_Location objectSensei)
        {

            if (currentmatchingObject.Label.ToLower() == "watermelon")
            {
                bool bp = true;
            }

            Match matchWithLocation = Sensation_Location.CompareSenseiPercentage(sourceSensei, currentmatchingObject.CurrentComparision, true, true);

            Match matchinWithoutLocation = Sensation_Location.CompareSenseiPercentage(sourceSensei, currentmatchingObject.CurrentComparision, true, false);

            int withLocation = matchinWithoutLocation.GetTotalMatchPercentage();

            int withoutLocation = matchWithLocation.GetTotalMatchPercentage();

            if (withLocation == 0 && withoutLocation == 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        private List<RecognisedEntity> ParseAllKnownObjectsForIncomingPattern(Sensation_Location sensei, Sensation_Location predictedSensei = null)
        {
            List<RecognisedEntity> setAsideList = null;

            if (sensei.sensLoc.Count == 0)
            {
                return setAsideList;
            }

            setAsideList = new List<RecognisedEntity>();
            
                foreach (var obj in Objects.Values)
                {
                    if (obj.CheckPatternMatchPercentage(sensei, predictedSensei) != 0)
                    {
                        setAsideList.Add(obj);
                    }
                }                        

            return setAsideList;
        }

        private int GetMatchingObjectIndexFromList(List<RecognisedEntity> matchingList, RecognisedEntity matchingObject)
        {
            if (matchingList?.Count == 0 || matchingObject == null)
            {
                return -1;
            }

            int index = 0;

            foreach (var item in matchingList)
            {
                if (item.Label == matchingObject.Label)
                {
                    return index;
                }
                index++;
            }

            return index;
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

            newObject.DoneTraining();

            Objects.Add(newObject.Label, newObject);
        }

        private void FinishedProcessingImage()
        {
            //Store object into list and move on to next object
        }

        #endregion        
    }

    public enum RecognitionState
    {
        None,
        IsBeingVerified,
        Recognised
    }
}
