///Author : Deric Pinto
namespace Hentul.Hippocampal_Entorinal_complex
{
    using Common;

    /// <summary>
    /// Stores the physical dimensions of the nearby environment (primary screen).
    /// Acts as the outer spatial envelope for the Graph and all object positions.
    /// </summary>
    public class EnvironmentBounds
    {
        public int ScreenWidth  { get; }
        public int ScreenHeight { get; }

        public Position2D TopLeft     => new Position2D(0, 0);
        public Position2D TopRight    => new Position2D(ScreenWidth, 0);
        public Position2D BottomLeft  => new Position2D(0, ScreenHeight);
        public Position2D BottomRight => new Position2D(ScreenWidth, ScreenHeight);
        public Position2D Center      => new Position2D(ScreenWidth / 2, ScreenHeight / 2);

        public int TotalPixels => ScreenWidth * ScreenHeight;

        public EnvironmentBounds(int screenWidth, int screenHeight)
        {
            if (screenWidth <= 0)  throw new ArgumentOutOfRangeException(nameof(screenWidth));
            if (screenHeight <= 0) throw new ArgumentOutOfRangeException(nameof(screenHeight));

            ScreenWidth  = screenWidth;
            ScreenHeight = screenHeight;
        }

        /// <summary>Returns true if the given position lies within the screen boundary.</summary>
        public bool Contains(Position2D pos) =>
            pos != null && pos.X >= 0 && pos.X <= ScreenWidth && pos.Y >= 0 && pos.Y <= ScreenHeight;

        /// <summary>Returns true if the object's bounding box lies fully within the screen.</summary>
        public bool ContainsObject(ObjectBounds bounds) =>
            bounds != null &&
            bounds.MinX >= 0 && bounds.MaxX <= ScreenWidth &&
            bounds.MinY >= 0 && bounds.MaxY <= ScreenHeight;

        public override string ToString() =>
            $"EnvironmentBounds: {ScreenWidth}x{ScreenHeight} ({TotalPixels:N0} pixels)";
    }
}
