namespace Hentul.Hippocampal_Entorinal_complex
{
    using System;

    public class Match
    {
        public int TotalBBMID { get; private set; }

        public int NumberOfBBMIDMatches { get; private set; }

        public int NumberOfBBMIDMises { get; private set; }

        public int NumberOfLocationIDMatches { get; private set; }

        public int NumberOfLocationIDMisses { get; private set; }

        public int[] PositionListMatch { get; private set; }

        public bool DidLocationGetChecked { get; private set; }

        public bool DidBBMGetChecked { get; private set; }

        public Match(Sensation_Location sourceSensei)
        {
            NumberOfBBMIDMatches = 0;
            NumberOfLocationIDMatches = 0;
            NumberOfLocationIDMisses = 0;
            PositionListMatch = new int[sourceSensei.sensLoc.Count];
            TotalBBMID = sourceSensei.sensLoc.Count;
            DidLocationGetChecked = false;
            DidBBMGetChecked = false;
        }

        public void IncrementBBMIDMatched()
        {
            if (DidBBMGetChecked == false)
                DidBBMGetChecked = true;
            NumberOfBBMIDMatches++;
        }

        public void IncrementBBMIDMiss()
        {
            if (DidBBMGetChecked == false)
                DidBBMGetChecked = true;
            NumberOfBBMIDMises++;
        }

        public void IncrementLocationIDMatch()
        {
            if (DidLocationGetChecked == false)
                DidLocationGetChecked = true;
            NumberOfLocationIDMatches++;
        }

        public void IncrementLocationIDMisses()
        {
            if (DidLocationGetChecked == false)
                DidLocationGetChecked = true;
            NumberOfLocationIDMisses++;
        }

        public void IncrementPositionIDMatch(int index) => PositionListMatch[index]++;

        public int GetTotalMatchPercentage()
        {
            if (TotalBBMID == 0)
                return 0;

            int locationMAtchPercentage = 0;
            int BBMMatchPercentage = 0;
            
            if (DidLocationGetChecked)
            {
                if (PositionListMatch.Length == NumberOfLocationIDMatches + NumberOfLocationIDMisses)
                    locationMAtchPercentage = (NumberOfBBMIDMatches * 100) / PositionListMatch.Length;
            }

            if (DidBBMGetChecked)
            {
                if (TotalBBMID != NumberOfBBMIDMatches + NumberOfBBMIDMises)
                {
                    //throw new InvalidOperationException("Something isn't right , Misses + Hits != Total!");
                }

                BBMMatchPercentage = (NumberOfBBMIDMatches * 100) / TotalBBMID;
            }

            return BBMMatchPercentage;
        }

        internal bool CheckMatchValidity()
        {
            bool toReturn = false;

            if (DidLocationGetChecked)
            {
                if (PositionListMatch.Length == NumberOfLocationIDMatches + NumberOfLocationIDMisses)
                    toReturn = true;
            }

            if (DidBBMGetChecked)
            {
                if (TotalBBMID == NumberOfBBMIDMatches + NumberOfBBMIDMises)
                    toReturn = true;
            }

            return toReturn;
        }
    }
}
