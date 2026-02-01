using System;
using System.Collections.Generic;
using System.Drawing;
using Common;
using Hentul;
using Hentul.Hippocampal_Entorinal_complex;

namespace Hentul.Encoders
{
    /// <summary>
    /// Maps every pixel of a 1200x600 bitmap into a unique Position_SOM within a
    /// large bit-space of size GridX x GridY. Mapping is deterministic and
    /// aims to preserve a global sparsity of ~2%: PixelCount / (GridX * GridY) ≈ 0.02
    /// 
    /// Strategy:
    /// - Linearize pixel coordinates (row-major) into index i in [0 .. PixelCount-1].
    /// - Spread indices across the larger bit-space by multiplying with a fixed step:
    ///     absoluteIndex = i * Step
    ///   where Step = (GridX * GridY) / PixelCount (computed at runtime).
    /// - Convert absoluteIndex -> (x = absoluteIndex % GridX, y = absoluteIndex / GridX).
    /// 
    /// This produces exactly one unique Position_SOM per input pixel and achieves
    /// the requested sparsity when GridX/GridY and image resolution are chosen accordingly.
    /// </summary>
    public class PixelEncoder
    {
        // Input image size expected by this encoder.
        public const int ImgWidth = 1200;
        public const int ImgHeight = 600;
        public const int PixelCount = ImgWidth * ImgHeight; // 720,000

        // Global bit-space dimensions (x in [0..GridX-1], y in [0..GridY-1])
        // TotalCells is computed from constructor parameters: TotalCells = GridX * GridY
        private readonly int GridX;
        private readonly int GridY;
        public readonly long TotalCells; // GridX * GridY (computed at runtime)

        // Step between mapped pixels in the bit-space (TotalCells / PixelCount)
        // Computed at runtime in the constructor. For an intended 2% sparsity
        // Step should be approximately 50 (i.e. TotalCells ≈ PixelCount * 50), but
        // actual value depends on the GridX/GridY provided.
        public readonly long Step;

        public PixelEncoder(int X, int Y)
        {
            GridX = X;
            GridY = Y;
            TotalCells = (long)GridX * GridY;

            if (TotalCells < (ImgWidth * ImgHeight))
            {
                throw new InvalidOperationException("TotalCells in Grid must be greater than or equal to the total bitmap pixel count (ImgWidth * ImgHeight).");
            }

            Step = TotalCells / PixelCount;
        }

        /// <summary>
        /// Encode the given 1200x600 bitmap into an SDR_SOM whose ActiveBits are the mapped Position_SOMs.
        /// The SDR_SOM Length/Breadth correspond to the global bit-space (GridX x GridY).
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="InvalidOperationException">if bitmap dimensions mismatch.</exception>
        public SDR_SOM EncodeBitmap(Bitmap bmp, VisionScope vScope, Position2D cursorPosition)
        {
            if (bmp is null) throw new ArgumentNullException(nameof(bmp));

            // Validate dimensions based on vision scope
            if (vScope == VisionScope.NARROW)
            {
                if (bmp.Width != ImgWidth || bmp.Height != ImgHeight)
                    throw new InvalidOperationException($"For NARROW vision, bitmap must be {ImgWidth}x{ImgHeight}.");
            }
            else if (vScope == VisionScope.BROAD)
            {
                // "3 times more than that" -> both dimensions are 3x
                var expectedW = ImgWidth * 3;
                var expectedH = ImgHeight * 3;
                if (bmp.Width != expectedW || bmp.Height != expectedH)
                    throw new InvalidOperationException($"For BROAD vision, bitmap must be {expectedW}x{expectedH}.");
            }
            else
            {
                // Fallback: treat OBJECT same as NARROW unless you want different behaviour
                if (bmp.Width != ImgWidth || bmp.Height != ImgHeight)
                    throw new InvalidOperationException($"Bitmap must be {ImgWidth}x{ImgHeight} for the requested vision scope.");
            }

            // Ensure cursorPosition is inside the bitmap
            if (cursorPosition is null)
                throw new ArgumentNullException(nameof(cursorPosition));
            if (cursorPosition.X < 0 || cursorPosition.X >= bmp.Width ||
                cursorPosition.Y < 0 || cursorPosition.Y >= bmp.Height)
            {
                throw new ArgumentOutOfRangeException(nameof(cursorPosition), "Cursor position must be inside the bitmap bounds.");
            }

            var list = new List<Position_SOM>(PixelCount);

            // Decide sampling scale: 1 for narrow (or object), 3 for broad
            int scale = (vScope == VisionScope.BROAD) ? 3 : 1;

            // Start traversal from cursorPosition and wrap around; use nested loops (sequential dual for loop)
            // Outer loop iterates rows starting at cursorPosition.Y, inner loop iterates columns starting at cursorPosition.X.
            int height = bmp.Height;
            int width = bmp.Width;
            int startY = cursorPosition.Y;
            int startX = cursorPosition.X;

            // Linear counter to allow selective sampling for BROAD (one in 3 pixels)
            long sequentialCounter = 0;

            for (int dy = 0; dy < height; dy++)
            {
                int y = (startY + dy) % height;
                for (int dx = 0; dx < width; dx++)
                {
                    int x = (startX + dx) % width;

                    // For BROAD scope sample one in 3 pixels in the traversal order
                    if (vScope == VisionScope.BROAD)
                    {
                        if ((sequentialCounter % 3L) != 0L)
                        {
                            sequentialCounter++;
                            continue;
                        }
                    }

                    sequentialCounter++;

                    // Check white
                    if (CheckIfColorIsWhite(bmp.GetPixel(x, y)))
                    {
                        // When bmp is larger (BROAD), map pixel coordinates down to the base ImgWidth/ImgHeight
                        // so GetMappedPosition remains valid. This collapses each scale x scale block to a single source pixel.
                        int mappedPx = x / scale;
                        int mappedPy = y / scale;

                        // Safety — GetMappedPosition already validates against ImgWidth/ImgHeight
                        var pos = GetMappedPosition(mappedPx, mappedPy);

                        list.Add(pos);
                    }
                }
            }

            // Return SDR_SOM with the mapped active bits; default to SPATIAL input type.
            return new SDR_SOM(GridX, GridY, list, iType.SPATIAL);
        }

        private bool CheckIfColorIsWhite(Color color)
           => color.R > 240 && color.G > 240 && color.B > 240;

        /// <summary>
        /// Deterministic mapping of a single pixel coordinate (px,py) -> Position_SOM in the global bit-space.
        /// </summary>
        /// <remarks>
        /// Ensures unique mapping for each input pixel given the constants above.
        /// Uses long arithmetic to avoid overflow.
        /// </remarks>
        public Position_SOM GetMappedPosition(int px, int py)
        {
            if (px < 0 || px >= ImgWidth) throw new ArgumentOutOfRangeException(nameof(px));
            if (py < 0 || py >= ImgHeight) throw new ArgumentOutOfRangeException(nameof(py));

            long linearIndex = (long)py * ImgWidth + px;               // 0 .. PixelCount-1
            long absoluteIndex = linearIndex * Step;                   // spread into big space
            if (absoluteIndex < 0 || absoluteIndex >= TotalCells)
                throw new InvalidOperationException("Calculated absolute index is out of global bit-space bounds.");

            int mappedX = (int)(absoluteIndex % GridX);
            int mappedY = (int)(absoluteIndex / GridX);               // 0..GridY-1

            return new Position_SOM(mappedX, mappedY);
        }

        /// <summary>
        /// Convenience: encode image and return dictionary mapping input pixel (x,y) -> Position_SOM.
        /// Useful for lookup rather than ordered list.
        /// 
        /// Note: EncodeBitmap only adds mappings for white pixels, so the returned SDR_SOM.ActiveBits
        /// may have fewer than PixelCount entries. This method currently assumes a mapping entry
        /// exists for every (x,y); if non-white pixels are expected, update this method to account
        /// for missing entries or build the mapping without filtering by color.
        /// </summary>
        public Dictionary<(int x, int y), Position_SOM> BuildMappingLookup(Bitmap bmp)
        {
            var sdr = EncodeBitmap(bmp);
            var positions = sdr.ActiveBits; // List<Position_SOM>
            var dict = new Dictionary<(int x, int y), Position_SOM>(PixelCount);
            int idx = 0;
            for (int y = 0; y < ImgHeight; y++)
            {
                for (int x = 0; x < ImgWidth; x++)
                {
                    dict[(x, y)] = positions[idx++];
                }
            }
            return dict;
        }
    }
}
