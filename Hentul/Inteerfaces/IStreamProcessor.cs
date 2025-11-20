using Common;

namespace Hentul.Inteerfaces
{
    public interface IStreamProcessor
    {
        LogMode LogMode { get; }

        /// <summary>
        /// Process one timestep of input (type depends on implementation).
        /// Vision: Bitmap (grayscale or preprocessed).
        /// Text: char or single-character string.
        /// </summary>
        void Process(object input, ulong cycleNum);

        /// <summary>
        /// Return the L3B (classifier) SDR for the requested cycle (if available).
        /// May return null if no activity.
        /// </summary>
        SDR_SOM? GetL3B(ulong cycleNum);

        /// <summary>
        /// Return current prediction labels (empty if none).
        /// </summary>
        IEnumerable<string> GetCurrentPredictions();
    }
}
