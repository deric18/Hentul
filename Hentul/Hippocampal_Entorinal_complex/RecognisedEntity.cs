using Common;

/// Author : Deric Pinto
namespace Hentul.Hippocampal_Entorinal_complex
{
    public class RecognisedEntity : BaseObject
    {
        public List<Sensation_Location> ObjectSnapshot { get; set; }        

        private List<string> locations;
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

        public Position GetNextPositionToVerify()
        {
            Position toReturn = null;

            if(ObjectSnapshot.Count == 0)
            {
                throw new InvalidOperationException("Cannot generate a new Position with empty Object Snapshot");
            }

            if (locations.Count == 0)
            {
                locations = new List<string>();
            }

            string pos = GenerateNextKeyLocationToVerify();

            locations.Add(pos);

            return Position.ConvertStringToPosition(pos);
        }

        public string GenerateNextKeyLocationToVerify()
        {
            bool flag = true;

            Random rand = new Random(); 

            int index = rand.Next(0, locations.Count);   

            while (flag)
            {
                var sensloc = ObjectSnapshot[index];

                //var str = sensloc.sensLoc.TryGetValue


            }

            return "3-2-1";
        }

        public int CheckPatternMatchPercentage(Sensation_Location sensei)
        {
            int toReturn = 0;

            toReturn =  Sensation_Location.CompareObjectSenseiAgainstListPercentage(sensei, ObjectSnapshot, true, false);

            if (toReturn == 0)
            {
                toReturn = Sensation_Location.CompareObjectSenseiAgainstListPercentage(sensei, ObjectSnapshot, true, true);
            }

            return toReturn;
        }        
    }

}
