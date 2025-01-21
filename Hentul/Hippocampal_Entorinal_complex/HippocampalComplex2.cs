namespace Hentul.Hippocampal_Entorinal_complex
{

    using Common;
    using System.Xml;

    public class HippocampalComplex2
    {

        #region  VARIABLES & CONSTRUCTORS

        public Graph Graph { get; private set; }

        public Dictionary<string, RecognisedEntity> Objects { get; private set; }

        public UnrecognisedEntity? CurrentObject { get; private set; }

        public Position CurrentPosition { get; private set; }

        public Position[] BoundaryPositions { get; private set; }

        private NetworkMode networkMode;

        private List<RecognisedEntity> matchingObjectList;

        private Position _cachedPosition;

        private RecognisedEntity currentmatchingObject;

        public RecognitionState ObjectState { get; private set; }

        public int currentIterationToConfirmation;

        public int NumberOfITerationsToConfirmation { get; private set; }

        private string backupDir;

        private List<string> objectlabellist;

        private int imageIndex;

        private bool isMock;

        public HippocampalComplex2(string firstLabel, bool Ismock = false, NetworkMode nMode = NetworkMode.TRAINING)
        {
            Graph = Graph.GetInstance();
            Objects = new Dictionary<string, RecognisedEntity>();
            CurrentObject = new UnrecognisedEntity();
            CurrentObject.Label = firstLabel;
            isMock = Ismock;
            networkMode = nMode;
            NumberOfITerationsToConfirmation = 6;
            matchingObjectList = new List<RecognisedEntity>();
            currentmatchingObject = null;
            ObjectState = RecognitionState.None;
            currentIterationToConfirmation = 0;
            backupDir = "C:\\Users\\depint\\source\\repos\\Hentul\\Hentul\\BackUp\\HC-EC\\";
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


        public void AddNewSensationToObject(Sensation_Location sensei, Sensation_Location? prediction = null)
        {
            if (networkMode == NetworkMode.TRAINING)
            {
                // Keep storing <Location , ActiveBit> -> KVPs under CurrentObject.                

                if (CurrentObject.AddNewSenei(sensei) == false)
                {
                    throw new InvalidOperationException(" Could Not Add New Sensei ! please Investigate your Shitty Code!");
                }
            }
        }
        
        public List<Position2D> PredictObject2(Sensation_Location sensei, Sensation_Location? predictionU, List<string> predictedLabels, bool isMock = false)
        {
            // Get predicted Labels 

            string objectLabel = null;
            List<Position2D> toReturn = null;
            Sensation_Location orginalSensei = sensei;

            if (networkMode != NetworkMode.PREDICTION)
            {
                throw new InvalidOperationException("cannot Predict Objects unless in network is in Prediction Mode!");
            }

            if (Objects.Count == 0)
            {
                throw new InvalidOperationException("Object Cannot be null under Prediction Mode");
            }
            if (predictedLabels.Count == 0)
            {
                throw new InvalidOperationException("predictedLabels cannot be empty");
            }

            // if there is a prediction load it to graph and suggest next location for verification. Avoid traditional pipeline.            
            RecognisedEntity entity1 = null;
            RecognisedEntity entity2 = null;
            List<Position2D> posList = null;


            if (predictedLabels.Count == 1)
            {
                entity1 = GetRecognisedEntityFromList(predictedLabels[0]);
                Graph.LightUpObject(entity1);

                toReturn = Graph.GetFavouritePositionsForObject(entity1);
            }
            else if (predictedLabels.Count == 2)
            {
                entity1 = GetRecognisedEntityFromList(predictedLabels[0]);
                entity2 = GetRecognisedEntityFromList(predictedLabels[1]);

                Graph.LightUpObject(entity1);
                Graph.LightUpObject(entity2);

                toReturn = Graph.CompareTwoObjects(entity1, entity2);

                if(toReturn.Count > 0)
                {
                    return toReturn;
                }
                else
                {
                    return null;    
                }
            }
            else
            {
                // cant load more than 2 objects on to graph for now.
                bool breakpoint = true;
            }

            return null;
        }

        public Position2D PredictObject(Sensation_Location sensei, Sensation_Location? prediction = null, bool isMock = false, uint iterationToConfirmation = 10)
        {
            string objectLabel = null;
            Position2D toReturn = null;
            Sensation_Location orginalSensei = sensei;

            if (networkMode != NetworkMode.PREDICTION)
            {
                throw new InvalidOperationException("cannot Predict Objects unless in network is in Prediction Mode!");
            }

            if (Objects.Count == 0)
            {
                throw new InvalidOperationException("Object Cannot be null under Prediction Mode");
            }

            matchingObjectList = ParseAllKnownObjectsForIncomingPattern(sensei, prediction);        // Traditional Pipeline.

            if (matchingObjectList.Count > 0)
            {
                _cachedPosition = Orchestrator.GetCurrentPointerPosition1();

                ObjectState = RecognitionState.IsBeingVerified;

                foreach (var matchingObject in matchingObjectList)
                {
                    if (matchingObject.Label == "JackFruit")
                    {
                        bool breakpoint = true;
                    }

                    if (matchingObject.Verify(null, isMock, iterationToConfirmation) == true)
                    {
                        currentmatchingObject = matchingObject;
                        ObjectState = RecognitionState.Recognised;
                        return new Position2D(int.MaxValue, int.MaxValue);
                    }
                    else
                    {
                        Orchestrator.MoveCursorToSpecificPosition(_cachedPosition.X, _cachedPosition.Y);
                    }
                }
            }

            toReturn = new Position2D(int.MinValue, int.MinValue);

            return toReturn;
        }       //Traditional Pipeline

        private RecognisedEntity GetRecognisedEntityFromList(string label)
        {
            if (Objects.Count == 0)
            {
                throw new InvalidOperationException("Cannot Predict with 0 Trained Objects!");
            }

            RecognisedEntity matchedEntity = null;

            foreach (var kvp in Objects)
            {
                if (kvp.Key.ToLower().Equals(label.ToLower()))
                {
                    matchedEntity = kvp.Value;
                }
            }

            return matchedEntity;
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


        #region UTILITY & MOCK 

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

        internal Position2D GetNextLocationForWandering()
        {
            Position2D position = null;

            if (ObjectState == RecognitionState.Recognised)
            {
                var index = currentmatchingObject.GetRandomSenseIndexFromRecognisedEntity();                //Random Sensei 
                position = currentmatchingObject.ObjectSnapshot[index].CenterPosition;                     //Must be ordered first highest X and lowest Y
                currentmatchingObject.SetSenseiToCurrentComparision(index);
            }

            return position;
        }

        internal List<Position2D> GetNextSensationForWanderingPosition()
        {
            List<Position2D> sensation = new List<Position2D>();

            if (ObjectState == RecognitionState.Recognised)
            {
                sensation.AddRange(currentmatchingObject.CurrentComparision.GetActiveBitsFromSensation());
            }

            return sensation;
        }

        public NetworkMode GetCurrentNetworkMode() => networkMode;

        public bool IsObjectIdentified => CurrentObject == null ? false : CurrentObject.IsObjectIdentified;


        public void LoadMockObject(List<RecognisedEntity> mockrecgs, bool doneTraining)
        {
            if (isMock == true)
            {
                foreach (var obj in mockrecgs)
                {
                    Objects.Add(obj.Label, obj);
                }

                if (doneTraining)
                {
                    foreach (var obj in Objects.Values)
                    {
                        obj.DoneTraining();
                    }
                }
            }
        }

        #endregion

        #endregion


        #region BACKUP & RESTORE

        public void Backup()
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

                            sensationLocationElement.AppendChild(xmlNode);
                        }

                        objectNode.AppendChild(objectNode);
                    }
                }

                xmlDocument.AppendChild(objectNode);
            }

            xmlDocument?.Save(backupDir + filename);

        }

        public static HippocampalComplex2 Restore()
        {
            string filename = "HC-EC.xml";

            HippocampalComplex2 hcComplex = new HippocampalComplex2("Apple");

            string backupDir = "C:\\Users\\depint\\source\\repos\\Hentul\\Hentul\\BackUp\\HC-EC\\";

            var xmlDocument = new XmlDocument();

            xmlDocument.LoadXml(backupDir + filename);

            return hcComplex;
        }

        #endregion


        #region PRIVATE HELPER METHODS

        private Tuple<Sensation_Location, Sensation_Location> ProcessStep1N2FromOrchestrator()
        {
            var instance = Orchestrator.GetInstance();
            instance.Read();
            var bmp = instance.ConverToEdgedBitmap();
            instance.ProcesStep1(bmp);
            return instance.GetSDRFromL3B();
        }

        internal static bool VerifyObjectSensei(Sensation_Location sourceSensei, Sensation_Location objectSensei, string label = null, string filename = null)
        {

            Match matchWithLocation = Sensation_Location.CompareSenseiPercentage(sourceSensei, objectSensei, true, true);

            Match matchinWithoutLocation = Sensation_Location.CompareSenseiPercentage(sourceSensei, objectSensei, true, false);

            int withLocation = matchWithLocation != null ? matchWithLocation.GetTotalMatchPercentage() : 0;

            int withoutLocation = matchinWithoutLocation != null ? matchinWithoutLocation.GetTotalMatchPercentage() : 0;


            #region LOG

            if (label != null && filename != null)
            {
                string logs = string.Empty;

                logs += ("Object : " + label);
                logs += "withLocation : " + withLocation + " withoutLocaiton : " + withoutLocation + "\n";

                if (sourceSensei.Id != Sensation_Location.EMPTYID)
                {
                    sourceSensei.ComputeStringID();
                }

                logs += "source sensei ID : <Position ID[0] : BBM ID[0] / sensloc Count[0] " + sourceSensei.Id + "\n";
                logs += "Object Sensei ID : <Position ID[0] : BBM ID[0] / sensloc Count[0] " + objectSensei.Id + "\n";

                logs += "Result :: " + (withLocation >= 50 && withoutLocation >= 50 ? "SUCCESS" : "FAILURE" + "\n");

                File.WriteAllText(filename, logs);
            }

            #endregion

            if (withLocation >= 50 && withoutLocation >= 50)
            {
                return true;
            }
            else
            {
                if (label == "Ananas" || label == "JackFruit")
                {
                    bool breakpoint = false;
                }
                return false;
            }
        }

        internal static bool VerifyObjectSensationAtLocationssInterSectionPolicy(Sensation_Location sourceS, Sensation_Location targetS)
        {
            Match match = Sensation_Location.CompareSensationsSimple(sourceS, targetS);

            return match.GetTotalMatchPercentage() > 50 ? true : false;
        }



        private List<RecognisedEntity> ParseAllKnownObjectsForIncomingPattern(Sensation_Location sensei, Sensation_Location predictedSensei = null)
        {
            List<RecognisedEntity> setAsideList = null;

            if (sensei.sensLoc.Count == 0)
            {
                return setAsideList;
            }

            setAsideList = new List<RecognisedEntity>();
            Tuple<int, int> tuple;

            foreach (var obj in Objects.Values)
            {
                tuple = obj.CheckPatternMatchPercentage(sensei, predictedSensei);

                if (tuple.Item1 != 0)
                {
                    setAsideList.Add(obj);
                }
                else if (tuple.Item2 != 0)
                {
                    // Adding object purely based on prediction is not a good idea.
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

            if (CurrentObject == null)
            {
                CurrentObject = new UnrecognisedEntity();
                CurrentObject.Label = objectlabellist[imageIndex++];
            }
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
