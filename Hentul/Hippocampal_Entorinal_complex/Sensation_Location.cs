namespace Hentul.Hippocampal_Entorinal_complex
{    
    using FirstOrderMemory.Models;
    using Common;

    public class Sensation_Location
    {


        #region MEMBER & CONSTRUCTORS
        public string Id { get; set; }

        public static readonly string EMPTYID = "EMPTY";

        /// <summary>
        /// Key : Location on the Screen
        /// Value : KeyValuePair<int, ActiveBits> Key : BBMID, Value : ActiveBits
        /// Generally 4 or less
        /// </summary>
        public SortedDictionary<string, KeyValuePair<int, List<Position2D>>> sensLoc { get; private set; }

        public Position2D CenterPosition { get; private set; }


        // Used only for Mock Purposes
        public Sensation_Location()
        {
            sensLoc = new SortedDictionary<string, KeyValuePair<int, List<Position2D>>>();
            ComputeStringID();            
        }

        public Sensation_Location(SortedDictionary<string, KeyValuePair<int, List<Position2D>>> sensLoc)
        {
            this.sensLoc = sensLoc;
            this.CenterPosition = null;
            ComputeStringID();
        }


        //Used for Production.
        public Sensation_Location(SortedDictionary<string, KeyValuePair<int, List<Position2D>>> sensLoc, Position2D cursorPos)
        {
            this.sensLoc = sensLoc;
            this.CenterPosition = cursorPos;
            ComputeStringID();
        }

        internal void RefreshID()
        {
            ComputeStringID();
        }

        #endregion

        public List<Position2D> GetActiveBitsFromSensation()
        {
            List<Position2D> activebits = new List<Position2D>();

            foreach (var kvp in sensLoc.Values)
            {
                activebits.AddRange(kvp.Value);         //No need to offset position with bbmId since this is a SOM sensation.
                
            }

            return activebits;
        }


        //Syntax ::  <Key0 / value[0].Key : value[0].Count>  
        public void ComputeStringID()
        {            
            if (sensLoc.Count == 0)
            {
                Id = EMPTYID;
                return;
            }

            string toReturn = string.Empty;
            char delimeter2 = '/';
            int max = sensLoc.Count;

            if (max == 1 || max == 2)
            {
                toReturn = sensLoc.ElementAt(0).Key + delimeter2 + sensLoc.Values.ElementAt(0).Key.ToString() + delimeter2 + sensLoc.Values.ElementAt(0).Value.Count.ToString() + delimeter2 +
                   sensLoc.ElementAt(max - 1).Key + delimeter2 + sensLoc.Values.ElementAt(max - 1).Key.ToString() + delimeter2 + sensLoc.Values.ElementAt(max - 1).Value.Count.ToString();
            }
            else
            {
                int mid = sensLoc.Count / 2;
                
                toReturn = sensLoc.ElementAt(0).Key + delimeter2 + sensLoc.Values.ElementAt(0).Key.ToString() + delimeter2 + sensLoc.Values.ElementAt(0).Value.Count.ToString() + delimeter2 +
                           sensLoc.ElementAt(mid).Key + delimeter2 + sensLoc.Values.ElementAt(mid).Key.ToString() + delimeter2 + sensLoc.Values.ElementAt(mid).Value.Count.ToString() + delimeter2 +
                           sensLoc.ElementAt(max - 1).Key + delimeter2 + sensLoc.Values.ElementAt(max - 1).Key.ToString() + delimeter2 + sensLoc.Values.ElementAt(max - 1).Value.Count.ToString();
            }

            Id = toReturn;
        }



        public bool AddNewSensationAtThisLocation(string location, KeyValuePair<int, List<Position2D>> sensation)
        {
            if (!sensLoc.TryGetValue(location, out var kvp))
            {
                sensLoc.Add(location, sensation);
                return true;
            }

            return false;
        }


        public static Tuple<int,int> CompareObjectSenseiAgainstListPercentage(Sensation_Location sensei1, List<Sensation_Location> senseiList, Sensation_Location sensei2 = null, bool includeBBM = true, bool includeLocation = true)
        {            
            int maxMatch1 = 0, maxMatch2 = 0;

            Match match1, match2;

            foreach (var item in senseiList)
            {
                match1 = CompareSenseiPercentage(sensei1, item, includeBBM, includeLocation);

                match2 = CompareSenseiPercentage(sensei2, item, includeBBM, includeLocation);

                int currentCycleMatch1 = match1.GetTotalMatchPercentage();

                int currentCycleMatch2 = match2 != null ?  match2.GetTotalMatchPercentage() : 0;

                if (currentCycleMatch1 == 100)
                {
                    int breakpoint = 10;
                }

                if (currentCycleMatch1 > maxMatch1)
                    maxMatch1 = currentCycleMatch1;

                if (currentCycleMatch2 > maxMatch2)
                    maxMatch2 = currentCycleMatch2;
            }

            return new Tuple<int,int>(maxMatch1, maxMatch2);
        }

        public static string GetMatchingSenseiLocation(Sensation_Location sensei, List<Sensation_Location> senseiList)
        {
            int maxMatch = 0;

            Match match;
            string location = null;

            foreach (var item in senseiList)
            {
                match = CompareSenseiPercentage(sensei, item, true, true);

                if(match.GetTotalMatchPercentage() == 100)
                {
                    return "WTF";
                }


            }

            return "NO WTF";
        }

        public static int ComparePositionListPercentage(List<Position2D> first, List<Position2D> second)
        {
            if (first.Count == 0 || second.Count == 0) return 0;

            int match = 0;

            foreach (var pos in first)
            {
                if (second.Contains(pos))
                {
                    match++;
                }
            }

            return ((100 * match) / first.Count);
        }



        public static Match CompareSensationsSimple(Sensation_Location sourceS, Sensation_Location targetS)
        {
            if (sourceS == null || targetS == null)
                return null;

            Match match = new Match(sourceS);
            int index = 0;

            foreach (var item in sourceS.sensLoc)
            {

                if (targetS.sensLoc.TryGetValue(item.Key, out var targetKvpValue))
                {
                    match.IncrementLocationIDMatch();

                    if (CompareKVPStrict(item.Value, targetKvpValue))
                    {
                        match.IncrementPositionIDMatch(index);
                    }
                }

                index++;
            }

            return match;
        }


        private static bool CompareKVPStrict(KeyValuePair<int, List<Position2D>> kvp1, KeyValuePair<int, List<Position2D>> kvp2)
        {
            bool success = false;

            if (kvp1.Key == kvp2.Key)
            {
                foreach (var pos in kvp1.Value)
                {
                    if (!kvp2.Value.Contains(pos))
                    {
                        return false;
                    }
                }

                return true;
            }

            return success;
        }

        /// <summary>
        ///  Compares two sensation_location Objects 
        /// </summary>
        /// <param name="sourceSensei"></param>
        /// <param name="targetSensei"></param>
        /// <param name="includeBBM"></param>
        /// <param name="includeLocation"></param>
        /// <returns>a Percentage 100 if all the source sensations @ locations are also present int target , 0 if not and anyuthing in the middle of how much of a good match it was and parameters selected.</returns>        
        public static Match CompareSenseiPercentage(Sensation_Location sourceSensei, Sensation_Location targetSensei, bool includeBBM = true, bool includeLocation = true)
        {
            if (sourceSensei == null)
                return null;

            Match match = new Match(sourceSensei);

            if (sourceSensei?.sensLoc.Count == 0 || targetSensei?.sensLoc.Count == 0)
            {
                return match;
            }
            
            int matchPercentage = 0;
            bool BBMChecked = false;
            int index = 0;

            foreach (var sourceLocationKvp in sourceSensei.sensLoc)
            {
                if (includeLocation)
                {
                    // Location ID Check
                    if (targetSensei.sensLoc.TryGetValue(sourceLocationKvp.Key, out var targetKvpValue))
                    {
                        match.IncrementLocationIDMatch();

                        if (includeBBM == true)
                        {
                            // includeLocation == true && includeBBM == true
                            BBMChecked = true;
                            int bbmID = sourceLocationKvp.Value.Key;

                            List<Position2D> sourceSOMs = sourceLocationKvp.Value.Value;

                            var targetKvp = targetSensei.sensLoc.Values;

                            bool BBMMatchedFlag = false;

                            foreach (var item in targetKvp)
                            {
                                if (item.Key == bbmID)
                                {
                                    match.IncrementBBMIDMatched();

                                    BBMMatchedFlag = true;

                                    if (ComparePositionListBoolean(sourceSOMs, item.Value))
                                    {
                                        match.IncrementPositionIDMatch(index);
                                    }

                                    break;
                                }
                            }

                            if (BBMMatchedFlag == false)
                            {
                                match.IncrementBBMIDMiss();
                            }

                            if (match.GetTotalMatchPercentage() == 100)
                            {
                                int breakpoint = 10;
                            }
                        }
                        else
                        {
                            // includeLocation == true && includeBBM == false
                            // compare only based on Location ID and ignore BBM ID , Parse through every location list in target Sensei for 100 % match

                            int cycleMatchPercentage = ComparePositionListPercentage(sourceLocationKvp.Value.Value, targetKvpValue.Value);

                            matchPercentage += cycleMatchPercentage;
                        }
                    }
                    else
                    {
                        match.IncrementLocationIDMisses();                        
                    }

                }
                else
                {
                    // includeLocation == false , includeBBMID == true

                    if (includeBBM == true)
                    {
                        int bbmID = sourceLocationKvp.Value.Key;

                        List<Position2D> sourceSOMs = sourceLocationKvp.Value.Value;

                        var targetKvp = targetSensei.sensLoc.Values;

                        bool BBMMatchedFlag = false;                        

                        foreach (var item in targetKvp)
                        {
                            if (item.Key == bbmID)
                            {
                                match.IncrementBBMIDMatched();

                                BBMMatchedFlag = true;

                                if (ComparePositionListBoolean(sourceSOMs, item.Value))
                                {
                                    match.IncrementPositionIDMatch(index);
                                }

                                break;
                            }
                        }

                        if (BBMMatchedFlag == false)
                        {
                            match.IncrementBBMIDMiss();
                        }

                    }
                    else
                    {
                        // includeBBM == false && includeLocation == false

                        List<Position2D> sourceSOMs = sourceLocationKvp.Value.Value;

                        var targetKvp = targetSensei.sensLoc.Values;

                        foreach (var item in targetKvp)
                        {
                            if (ComparePositionListBoolean(sourceSOMs, item.Value))
                            {
                                match.IncrementPositionIDMatch(index);
                                break;
                            }
                        }
                        break;
                    }
                }

                index++;
            }             

            if (match.DidBBMGetChecked == false)
            {
                ComputeBBMIDMisses(sourceSensei, targetSensei, match);
            }

            return match;
        }

        public static bool ComparePositionListBoolean(List<Position2D> first, List<Position2D> second)
        {
            if (first.Count == 0 || second.Count == 0) return false;

            int maxMatch = first.Count;
            int match = 0;

            foreach (var pos in first)
            {
                if (second.Any(position => position.Equals(pos)))
                {
                    match++;
                }
            }

            return maxMatch == match;
        }

        private static void ComputeBBMIDMisses(Sensation_Location sourceSensei, Sensation_Location targetSensei, Match match)
        {

            foreach (var kvp in sourceSensei.sensLoc.Values)
            {
                int bbmID = kvp.Key;

                List<Position2D> sourceSOMs = kvp.Value;

                var targetKvp = targetSensei.sensLoc.Values;

                bool BBMMatchedFlag = false;

                foreach (var item in targetKvp)
                {
                    if (item.Key == bbmID)
                    {
                        match.IncrementBBMIDMatched();

                        BBMMatchedFlag = true;                        
                        break;
                    }
                }

                if (BBMMatchedFlag == false)
                {
                    match.IncrementBBMIDMiss();
                }
            }

        }      

        public static bool CompareSenseiBool(List<Sensation_Location> sensei1ist, Sensation_Location sensei)
        {
            foreach (var item in sensei1ist)
            {
                if (CompareSenseiBool(sensei, item))
                    return false;
            }

            return true;
        }

        public static bool CompareSenseiBool(List<Sensation_Location> sensei1ist1, List<Sensation_Location> sensei1ist2)
        {
            foreach (var item1 in sensei1ist1)
            {
                foreach (var item2 in sensei1ist2)
                {
                    if (CompareSenseiBool(item1, item2) == false)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public static bool CompareSenseiBool(Sensation_Location sourceSensei, Sensation_Location targetensei)
        {

            foreach (var item in sourceSensei.sensLoc)
            {
                if (targetensei.sensLoc.TryGetValue(item.Key, out var keyValuePair))
                {
                    foreach (var position in keyValuePair.Value)
                    {
                        if (!item.Value.Value.Contains(position))
                        {
                            return false;
                        }
                    }
                }
            }

            return true;
        }
    }
}
