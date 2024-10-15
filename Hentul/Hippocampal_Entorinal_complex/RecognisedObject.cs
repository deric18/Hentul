
using Common;
using System.Reflection.Metadata.Ecma335;

/// Author : Deric Pinto
namespace Hentul.Hippocampal_Entorinal_complex
{
    public class RecognisedObject : BaseObject
    {
        public Dictionary<string, List<Position>> ObjectPattern { get; set; }
        public string Label { get; private set; }

        public RecognisedObject(string name)
        {
            Label = name;
            ObjectPattern = new Dictionary<string, List<Position>>();
        }

        public RecognisedObject(UnrecognisedObject unrec) 
        {
            Label = unrec.Label;
            ObjectPattern = unrec.ObjectPattern;
        }

        public int CheckPatternMatchPercentage(string location, List<Position> acitivePosition)
        {
            int percent = 0;            

            if (ObjectPattern.TryGetValue(location, out List<Position> result))
            {
                int matchCounter = 0;
                foreach (var item in result)
                {

                    if (acitivePosition.Contains(item))
                    {
                        matchCounter++;
                    }
                }

                percent = ( ( matchCounter * 100 ) / result.Count );
            }


            return percent;
        }        
    }

}
