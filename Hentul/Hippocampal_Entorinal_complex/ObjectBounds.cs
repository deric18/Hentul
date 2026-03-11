///Author : Deric Pinto
namespace Hentul.Hippocampal_Entorinal_complex
{
    using Common;

    /// <summary>
    /// Axis-aligned bounding box for a recognised object within the environment.
    /// Derived from the sensation positions stored in the Graph.
    /// </summary>
    public class ObjectBounds
    {
        public string Label { get; }

        public int MinX { get; }
        public int MinY { get; }
        public int MaxX { get; }
        public int MaxY { get; }

        public int Width  => MaxX - MinX;
        public int Height => MaxY - MinY;

        public Position2D TopLeft     => new Position2D(MinX, MinY);
        public Position2D BottomRight => new Position2D(MaxX, MaxY);
        public Position2D Center      => new Position2D((MinX + MaxX) / 2, (MinY + MaxY) / 2);

        public ObjectBounds(string label, int minX, int minY, int maxX, int maxY)
        {
            Label = label;
            MinX  = minX;
            MinY  = minY;
            MaxX  = maxX;
            MaxY  = maxY;
        }

        /// <summary>Builds bounds by scanning a RecognisedVisualEntity's sensation positions.</summary>
        public static ObjectBounds FromEntity(RecognisedVisualEntity entity)
        {
            if (entity == null || entity.ObjectSnapshot == null || entity.ObjectSnapshot.Count == 0)
                throw new ArgumentException("Cannot compute bounds for an entity with no sensations.", nameof(entity));

            int minX = int.MaxValue, minY = int.MaxValue;
            int maxX = int.MinValue, maxY = int.MinValue;

            foreach (var sensation in entity.ObjectSnapshot)
            {
                if (sensation.CenterPosition == null) continue;

                int x = sensation.CenterPosition.X;
                int y = sensation.CenterPosition.Y;

                if (x < minX) minX = x;
                if (y < minY) minY = y;
                if (x > maxX) maxX = x;
                if (y > maxY) maxY = y;
            }

            if (minX == int.MaxValue)
                throw new InvalidOperationException($"No valid center positions found for entity '{entity.Label}'.");

            return new ObjectBounds(entity.Label, minX, minY, maxX, maxY);
        }

        public override string ToString() =>
            $"{Label}: TopLeft=({MinX},{MinY}) BottomRight=({MaxX},{MaxY}) Size={Width}x{Height}";
    }
}
