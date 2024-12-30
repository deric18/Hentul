/// Author : Deric Pinto
namespace Hentul.Hippocampal_Entorinal_complex
{

    using Common;    

    public class RecognisedEntity
    {
        public List<Sensation_Location> ObjectSnapshot { get; set; }

        //Holds the indexes of all the objects in objectsnapshot which are needed for classfication of the object.
        public RFrame frame { get; private set; }

        public Position2D CenterPosition { get; private set; }

        private List<int> _visitedIndexes;

        public Sensation_Location CurrentComparision;

        public int CurrentComparisionKeyIndex { get; private set; }

        public string Label { get; private set; }

        private string failureReportfileName;

        public RecognisedEntity(string name)
        {
            Label = name;
            ObjectSnapshot = new List<Sensation_Location>();
            _visitedIndexes = new List<int>();
            frame = null;
        }

        public RecognisedEntity(UnrecognisedEntity unrec)
        {
            Label = unrec.Label;
            ObjectSnapshot = unrec.ObjectSnapshot;
            _visitedIndexes = new List<int>();
            frame = null;
            failureReportfileName = "C:\\Users\\depint\\source\\repos\\Hentul\\failureReport.txt";
        }

        public void Clean()
        {
            if (_visitedIndexes != null)
                _visitedIndexes?.Clear();

            CurrentComparision = null;
            CurrentComparisionKeyIndex = 0; 
        }

        public void DoneTraining()
        {
            foreach (var sensei in ObjectSnapshot)
            {
                sensei.RefreshID();
            }
            frame = new RFrame(ObjectSnapshot);
        }

        public bool Verify(Sensation_Location sensei = null, bool isMock = false, uint iterationToConfirmation = 10)
        {
            if(ObjectSnapshot?.Count == 0)
            {
                throw new InvalidOperationException("Snapshot cannot be empty");
            }            
            
            Orchestrator instance = Orchestrator.GetInstance();

            if(instance == null)
            {
                throw new InvalidOperationException("Orchestrator Instance cannot be null!");
            }

            
            if ( frame?.DisplacementTable?.GetLength(0) != ObjectSnapshot.Count || frame?.DisplacementTable?.GetLength(1) != ObjectSnapshot.Count)
            {
                throw new InvalidOperationException("RFrame cannot be Empty!");
            }

            int confirmations = 0;

            for (int i = 0; i < frame?.DisplacementTable?.GetLength(0); i++)
            {
                for(int j=0; j < frame?.DisplacementTable?.GetLength(1); j++)
                {
                    if( i != j)
                    {
                        var newSensei = ObjectSnapshot[j];
                        var newPos = newSensei.CenterPosition;


                        if (isMock == false)
                        {
                            if (Label == "JackFruit")
                            {
                                bool breakpoint = true;
                            }

                            Orchestrator.MoveCursorToSpecificPosition(newPos.X, newPos.Y);
                            instance.ProcessStep0();
                            var bmp = instance.ConverToEdgedBitmap();
                            instance.ProcesStep1(bmp);
                        }                  
                        
                        var tuple = instance.GetSDRFromL3B();

                        if (tuple.Item1 == null)
                            return false;

                        
                        if(HippocampalComplex2.VerifyObjectSensei(tuple.Item1, newSensei, Label, failureReportfileName) == false)
                        {
                            return false;
                        }

                        confirmations++;
                        if(confirmations == iterationToConfirmation)
                        {
                            return true;
                        }
                    }
                }

                return true;
            }

            return true;
        }

        public bool IncrementCurrentComparisionKeyIndex()
        {
            if (CurrentComparision.sensLoc.Count - 1 >= (CurrentComparisionKeyIndex + 1))
            {
                CurrentComparisionKeyIndex++;
                return true;
            }
            else
            {
                return false;
            }
        }

        public int GetRandomSenseIndexFromRecognisedEntity()
        {
            Random rand = new Random();

            int index = rand.Next(0, ObjectSnapshot.Count);

            bool flag = true;

            if (_visitedIndexes.Contains(index))
            {
                while (flag)
                {
                    index = rand.Next(0, ObjectSnapshot.Count);

                    if (_visitedIndexes.Contains(index) == false)
                        flag = false;
                }
            }

            _visitedIndexes.Add(index);

            SetSenseiToCurrentComparision(index);

            return index;
        }

        public void SetSenseiToCurrentComparision(int index)
        {
            if (index < 0 || index >= ObjectSnapshot.Count)
                throw new InvalidOperationException("index cannot exceed objectSnapshot!");

            CurrentComparision = ObjectSnapshot[index];
        }

        public Sensation_Location PrepNextSenseiToVerify(Sensation_Location? source = null, Position2D posToVerify = null)
        {
            Sensation_Location toReturn = null;

            if (ObjectSnapshot.Count == 0)
            {
                throw new InvalidOperationException("Cannot generate a new Position with empty Object Snapshot");
            }

            if (posToVerify != null && source != null)
            {
                //Extract this sensei from object snapshot for comparision exactly at this location.

                int index = 0;

                foreach (var sensei in ObjectSnapshot)
                {
                    if (source.CenterPosition == posToVerify)
                    {
                        //Pos found copy sensei to currentComparision

                        CurrentComparision = sensei;
                        CurrentComparisionKeyIndex = index;
                        _visitedIndexes.Add(index);

                        return CurrentComparision;
                    }

                    index++;
                }               
            }

            if (source != null)
            {
                var matchingSensei = PickMostMatchingSensei(source);

                if (matchingSensei != null)
                {
                    CurrentComparision = matchingSensei;
                }
                else
                {
                    int index = 0;

                    foreach (var sensei in ObjectSnapshot)
                    {
                        if (sensei.Id == source.Id)
                        {
                            //Pos found copy sensei to currentComparision

                            CurrentComparision = sensei;
                            CurrentComparisionKeyIndex = index;
                            _visitedIndexes.Add(index);

                            return CurrentComparision;
                        }

                        index++;
                    }
                }
            }            

            toReturn = GetRandSenseiToVerify();

            return toReturn;
        }

        private Sensation_Location PickMostMatchingSensei(Sensation_Location source)
        {
            Sensation_Location toRet = null;

            source.ComputeStringID();

            int index = 0;

            foreach (var sensei in ObjectSnapshot)
            {
                if (source.Id == sensei.Id)
                {
                    toRet = sensei;
                    break;
                }

                index++;
            }

            if (index != ObjectSnapshot.Count)
            {
                _visitedIndexes.Add(index);
            }

            return toRet;
        }

        private Sensation_Location GetRandSenseiToVerify()
        {
            bool flag = true;

            Random rand = new Random();

            int index = rand.Next(0, ObjectSnapshot.Count);

            var sensloc = ObjectSnapshot[index];

            if(_visitedIndexes.Count == ObjectSnapshot.Count)
            {
                throw new InvalidOperationException("Cannot generate Random Sensation with these filters!");
            }

            if (_visitedIndexes.Contains(index))
            {
                while (flag)
                {
                    index = rand.Next(0, ObjectSnapshot.Count);

                    if (_visitedIndexes.Contains(index) == false)
                        flag = false;
                }
            }

            _visitedIndexes.Add(index);
            CurrentComparisionKeyIndex = index;

            CurrentComparision = sensloc;

            return sensloc;
        }

        public Tuple<int, int> CheckPatternMatchPercentage(Sensation_Location sensei, Sensation_Location predictedSensei = null)
        {
            Tuple<int, int> tuple;

            tuple = Sensation_Location.CompareObjectSenseiAgainstListPercentage(sensei, ObjectSnapshot, predictedSensei, true, false);

            if (tuple.Item1 == 0)
            {
                tuple = Sensation_Location.CompareObjectSenseiAgainstListPercentage(sensei, ObjectSnapshot, predictedSensei, true, true);
            }

            //if(tuple.Item1 > 0 && tuple.Item2 > 0)
            //{
            //    //PRedicted Sensei matche as well
            //}
            //else if(tuple.Item1 == 0 && tuple.Item2 == 0)
            //{

            //}
            //else if (tuple.Item1 == 0 && tuple.Item2 > 0)
            //{

            //}
            //else if( tuple.Item1 > 0 && tuple.Item2 == 0)
            //{

            //}
            //else if(tuple.Item1 == 100 && tuple.Item2 == 100)
            //{

            //}

            return tuple;
        }
    }

}
