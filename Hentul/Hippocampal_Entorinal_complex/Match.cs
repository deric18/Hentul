namespace Hentul.Hippocampal_Entorinal_complex
{
    using System;

    public class Match
    {
        public int TotalBBMID {  get; private set; }

        public int NumberOfBBMIDMatches { get; private set; }

        public int NumberOfBBMIDMises { get; private set; }

        public int NumberOfLocationIDMatches { get; private set; }

        public int NumberOfLocationIDMisses {  get; private set; }

        public int[] PositionListMatch { get; private set; }

        public Match( Sensation_Location sourceSensei) 
        {
            NumberOfBBMIDMatches = 0;
            NumberOfLocationIDMatches = 0;
            PositionListMatch = new int[sourceSensei.sensLoc.Count];
            TotalBBMID = sourceSensei.sensLoc.Count;
        }

        public void IncrementBBMIDMatched() => NumberOfBBMIDMatches++;

        public void IncrementBBMIDMiss() => NumberOfBBMIDMises++;

        public void IncrementLocationIDMatch() => NumberOfLocationIDMatches++;

        public void IncrementLocationIDMisses() => NumberOfLocationIDMisses++;

        public void IncrementPositionIDMatch(int index) => PositionListMatch[index]++;

        public int GetTotalMatchPercentage()
        {
            if (TotalBBMID == 0)
                return 0;

            if ( TotalBBMID != NumberOfBBMIDMatches + NumberOfBBMIDMises )
            {
                throw new InvalidOperationException("Something isn't right , Misses + Hits != Total!");
            }            

            return ( NumberOfBBMIDMatches * 100 ) / TotalBBMID;
        }

        internal bool CheckMatchValidity()
        {
            return TotalBBMID == NumberOfBBMIDMatches + NumberOfBBMIDMises;
        }
    }
}
