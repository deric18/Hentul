

namespace Hentul.UT
{
    using Common;
    using Hentul.Hippocampal_Entorinal_complex;

    internal static class TestUtils
    {
        static List<String> objectlabellist = new List<string>
            {
                "Apple",
                "Ananas",
                "Watermelon",
                "JackFruit",
                "Grapes"
            };

        static int objectLabelIndex = 0;

        public static List<RecognisedVisualEntity> GenerateRandomEntities(int num)
        {
            List<RecognisedVisualEntity> recEnList = new List<RecognisedVisualEntity>();

            RecognisedVisualEntity entity;

            for (int i = 0; i < num; i++)
            {
                entity = new RecognisedVisualEntity(GenerateRandomObjectLabels());

                for (int j = 0; j < 10; j++)
                    entity.ObjectSnapshot.Add(GenerateRandomSenation());

                recEnList.Add(entity);
            }

            return recEnList;
        }

        private static Sensation_Location GenerateRandomSenation()
        {

            Random rand = new Random();

            Position2D cursorPos = new Position2D(rand.Next(0, 2000), rand.Next(0, 2000));

            KeyValuePair<int, List<Position2D>> kvp = GenerateNewKeyValuePair();

            SortedDictionary<string, KeyValuePair<int, List<Position2D>>> sdict = GenerateRandomDictionary(cursorPos);

            return new Sensation_Location(sdict, cursorPos);
        }

        private static SortedDictionary<string, KeyValuePair<int, List<Position2D>>> GenerateRandomDictionary(Position2D pos2d, int numEntries = 15)
        {
            Random rand = new Random();

            SortedDictionary<string, KeyValuePair<int, List<Position2D>>> sDict = new SortedDictionary<string, KeyValuePair<int, List<Position2D>>>();

            for (int i = 0; i < numEntries; i++)
            {
                sDict.Add(GetNewRandPos2d().ToString(), GenerateNewKeyValuePair());
            }

            return sDict;
        }

        internal static Position2D GetNewRandPos2d()
        {
            Random rand = new Random();
            return new Position2D(rand.Next(0, 2000), rand.Next(0, 2000));
        }

        private static KeyValuePair<int, List<Position2D>> GenerateNewKeyValuePair(int numKvps = 1)
        {            
            Random rand = new Random();
            return new KeyValuePair<int, List<Position2D>>(rand.Next(0, 99), GenerateRandomPositionList());             
        }
        

        private  static string GenerateRandomObjectLabels()
        {
            if (objectLabelIndex > objectlabellist.Count)
            {
                throw new InvalidOperationException();
            }

            return objectlabellist[objectLabelIndex++];
        }


        private static List<Position2D> GenerateRandomPositionList(int numPos = 3)
        {
            List<Position2D> randPosList = new List<Position2D>();
            Random rand = new Random();

            for (int i = 0; i < numPos; i++)
            {
                randPosList.Add(new Position2D(rand.Next(0, 2000), rand.Next(0, 2000)));
            }

            return randPosList;
        }
    }
}
