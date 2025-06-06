namespace Hentul.Hippocampal_Entorinal_complex.Interfaces
{
    using System;

    public interface IPrediction
    {
        public RecognisedVisualEntity Predict(Sensation_Location sensation_location,
                                        List<string> predictedLabels,
                                        Sensation_Location? nextPrediction);   
    }
}
