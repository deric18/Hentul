namespace Hentul.Hippocampal_Entorinal_complex
{    
    using FirstOrderMemory.Models;

    public class Sensation_Location
    {

        public string Id { get; set; }

        /// <summary>
        /// Key : Location on the Screen
        /// Value : KeyValuePair<int, ActiveBits> Key : BBMID, Value : ActiveBits
        /// Generally 4 or less
        /// </summary>
        public Dictionary<string, KeyValuePair<int, List<Position_SOM>>> sensLoc { get; private set; }


        // Used only for Mock Purposes
        public Sensation_Location()
        {
            sensLoc = new Dictionary<string, KeyValuePair<int, List<Position_SOM>>>();
            Id = ComputeStringID();            
        }      

        //Used for Production.
        public Sensation_Location(Dictionary<string, KeyValuePair<int, List<Position_SOM>>> sensLoc)
        {
            this.sensLoc = sensLoc;
            Id = ComputeStringID();
        }

        internal void RefreshID()
        {
            Id = ComputeStringID();
        }

        //Syntax ::  <Key / value[0].Key : value[0].Count>  
        internal string ComputeStringID()
        {
            string toReturn = string.Empty;

            if(sensLoc.Count == 0 )            
                return "EMPTY";

            char delimeter1 = ':';
            char delimeter2 = '/';
            int max = sensLoc.Count;

            if (max == 1 || max == 2)
            {
                toReturn = sensLoc.ElementAt(0).Key + delimeter2 + sensLoc.Values.ElementAt(0).Key.ToString() + sensLoc.Values.ElementAt(0).Value.Count.ToString() +
                   sensLoc.ElementAt(max - 1).Key + delimeter2 + sensLoc.Values.ElementAt(max - 1).Key.ToString() + sensLoc.Values.ElementAt(max - 1).Value.Count.ToString();
            }
            else
            {
                int mid = sensLoc.Count / 2;
                
                toReturn = sensLoc.ElementAt(0).Key + delimeter2 + sensLoc.Values.ElementAt(0).Key.ToString() + sensLoc.Values.ElementAt(0).Value.Count.ToString() +
                           sensLoc.ElementAt(mid).Key + delimeter2 + sensLoc.Values.ElementAt(mid).Key.ToString() + sensLoc.Values.ElementAt(mid).Value.Count.ToString() +
                           sensLoc.ElementAt(max - 1).Key + delimeter2 + sensLoc.Values.ElementAt(max - 1).Key.ToString() + sensLoc.Values.ElementAt(max - 1).Value.Count.ToString();
            }


            return toReturn;
        }


        public bool AddNewSensationAtThisLocation(string location, KeyValuePair<int, List<Position_SOM>> sensation)
        {
            if (!sensLoc.TryGetValue(location, out var kvp))
            {
                sensLoc.Add(location, sensation);
                return true;
            }

            return false;
        }


        public static int CompareObjectSenseiAgainstListPercentage(Sensation_Location sensei, List<Sensation_Location> senseiList, bool includeBBM = true, bool includeLocation = true)
        {            
            int maxMatch = 0;

            Match match;

            foreach (var item in senseiList)
            {
                match = CompareSenseiPercentage(sensei, item, includeBBM, includeLocation);

                int currentCycleMatch = match.GetTotalMatchPercentage();

                if(currentCycleMatch == 100)
                {
                    int breakpoint = 10;
                }

                if (currentCycleMatch > maxMatch)
                    maxMatch = currentCycleMatch;
            }

            return maxMatch;
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

        public static int ComparePositionListPercentage(List<Position_SOM> first, List<Position_SOM> second)
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
            Match match = new Match(sourceSensei);

            if (sourceSensei?.sensLoc.Count == 0 || targetSensei?.sensLoc.Count == 0 || sourceSensei.sensLoc.Count != targetSensei.sensLoc.Count )
            {
                return match;
            }

            if ( sourceSensei.sensLoc.Keys.ElementAt(0) == "1188-503-0" && targetSensei.sensLoc.Keys.ElementAt(0) == "1168-503-0")
            {
                bool brekapoint = true;
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

                            List<Position_SOM> sourceSOMs = sourceLocationKvp.Value.Value;

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

                        List<Position_SOM> sourceSOMs = sourceLocationKvp.Value.Value;

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

                        List<Position_SOM> sourceSOMs = sourceLocationKvp.Value.Value;

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

            if (match.CheckMatchValidity() == false)
            {
                ComputeBBMIDMisses(sourceSensei, targetSensei, match);
            }

            return match;
        }

        public static bool ComparePositionListBoolean(List<Position_SOM> first, List<Position_SOM> second)
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

                List<Position_SOM> sourceSOMs = kvp.Value;

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

        public static int CompareSenseilistAgainstListPercentage(List<Sensation_Location> sensei1ist1, List<Sensation_Location> sensei1ist2)
        {
            int matchPercentage = 0;

            foreach (var item1 in sensei1ist1)
            {
                int match = CompareObjectSenseiAgainstListPercentage(item1, sensei1ist2);

                if (match > matchPercentage)
                {
                    matchPercentage = match;
                }
            }

            return matchPercentage;
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
