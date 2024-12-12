/// Author : Deric Pinto
namespace Hentul.Hippocampal_Entorinal_complex
{
    public class RecognisedEntity : BaseObject
    {
        public List<Sensation_Location> ObjectSnapshot { get; set; }

        private List<string> _verifiedID;

        public Sensation_Location CurrentComparision;

        public int CurrentComparisionKeyIndex { get; private set; }

        public bool IncrementCurrentComparisionKeyIndex()
        {
            if(CurrentComparision.sensLoc.Count - 1 >= (CurrentComparisionKeyIndex + 1))
            {
                CurrentComparisionKeyIndex++;
                return true;
            }
            else
            {
                return false;
            }
        }

        public string Label { get; private set; }

        public RecognisedEntity(string name)
        {
            Label = name;
            ObjectSnapshot = new List<Sensation_Location>();
        }

        public RecognisedEntity(UnrecognisedEntity unrec)
        {
            Label = unrec.Label;
            ObjectSnapshot = unrec.ObjectSnapshot;
        }

        public void Clean()
        {
            _verifiedID.Clear();
            CurrentComparision = null;
        }

        public Sensation_Location GetNextSenseiToVerify()
        {
            Sensation_Location toReturn = null;

            if (ObjectSnapshot.Count == 0)
            {
                throw new InvalidOperationException("Cannot generate a new Position with empty Object Snapshot");
            }

            if (_verifiedID.Count == 0)
            {
                _verifiedID = new List<string>();
            }

            toReturn = GetRandSenseiToVerify();

            _verifiedID.Add(toReturn.Id);

            return toReturn;
        }


        private Sensation_Location GetRandSenseiToVerify()
        {
            bool flag = true;

            Random rand = new Random();

            int index = rand.Next(0, ObjectSnapshot.Count);

            var sensloc = ObjectSnapshot[index];

            if (_verifiedID.Contains(sensloc.Id))
            {
                while (flag)
                {
                    index = rand.Next(0, ObjectSnapshot.Count);
                    sensloc = ObjectSnapshot[index];

                    if (_verifiedID.Contains(sensloc.Id) == false)
                        flag = false;
                }
            }

            _verifiedID.Add((string) sensloc.Id);

            CurrentComparision = sensloc;

            return sensloc;
        }

        public int CheckPatternMatchPercentage(Sensation_Location sensei)
        {
            int toReturn = 0;

            toReturn = Sensation_Location.CompareObjectSenseiAgainstListPercentage(sensei, ObjectSnapshot, true, false);

            if (toReturn == 0)
            {
                toReturn = Sensation_Location.CompareObjectSenseiAgainstListPercentage(sensei, ObjectSnapshot, true, true);
            }

            return toReturn;
        }
    }

}
