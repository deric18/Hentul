using System;
using System.Collections.Generic;
using System.Drawing;
using Common;
using Hentul;
using Hentul.Hippocampal_Entorinal_complex;

namespace Hentul.Encoders
{
    /// <summary>
    /// Maps every pixel of a 1000x600 bitmap into a unique Position_SOM within a
    /// large bit-space of size GridX x GridY. Mapping is deterministic and
    /// preserves a global sparsity of 2%: PixelCount / (GridX * GridY) = 0.02.
    /// 
    /// Strategy:
    /// - Linearize pixel coordinates (row-major) into index i in [0 .. PixelCount-1].
    /// - Spread indices across the larger bit-space by multiplying with a fixed step:
    ///     absoluteIndex = i * Step
    ///   where Step = (GridX * GridY) / PixelCount (should be integer for exact 2%).
    /// - Convert absoluteIndex -> (x = absoluteIndex % GridX, y = absoluteIndex / GridX).
    /// 
    /// This produces exactly one unique Position_SOM per input pixel and achieves
    /// the requested sparsity when GridX/GridY and image resolution are chosen accordingly.
    /// </summary>
    public class PixelEncoder
    {
        // Input image size expected by this encoder.
        public const int ImgWidth = 1000;
        public const int ImgHeight = 600;
        public const int PixelCount = ImgWidth * ImgHeight; // 600,000

        // Global bit-space dimensions (x in [0..GridX-1], y in [0..GridY-1])
        // These choices yield totalBits = 3_000_000 * 10 = 30_000_000
        private readonly int GridX;
        private readonly int GridY;
        public readonly long TotalBits; // 30_000_000

        // Step between mapped pixels in the bit-space (TotalBits / PixelCount)
        // For the chosen constants this equals 50 -> enforces 2% sparsity
        public readonly long Step;

        public PixelEncoder(int X, int Y)
        {
            GridX = X;
            GridY = Y;
            TotalBits = (long)GridX * GridY;
            Step = TotalBits / PixelCount;
        }

        /// <summary>
        /// Encode the given 1000x600 bitmap into an SDR_SOM whose ActiveBits are the mapped Position_SOMs.
        /// The SDR_SOM Length/Breadth correspond to the global bit-space (GridX x GridY).
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="InvalidOperationException">if bitmap dimensions mismatch.</exception>
        public SDR_SOM EncodeBitmap(Bitmap bmp)
        {
            if (bmp is null) throw new ArgumentNullException(nameof(bmp));
            if (bmp.Width != ImgWidth || bmp.Height != ImgHeight)
                throw new InvalidOperationException($"Bitmap must be {ImgWidth}x{ImgHeight}.");

            var list = new List<Position_SOM>(PixelCount);

            // Row-major traversal: y in [0..ImgHeight-1], x in [0..ImgWidth-1]
            for (int y = 0; y < ImgHeight; y++)
            {
                for (int x = 0; x < ImgWidth; x++)
                {
                    var pos = GetMappedPosition(x, y);
                    list.Add(pos);
                }
            }

            // Return SDR_SOM with the mapped active bits; default to SPATIAL input type.
            return new SDR_SOM(GridX, GridY, list, iType.SPATIAL);
        }

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
            if (absoluteIndex < 0 || absoluteIndex >= TotalBits)
                throw new InvalidOperationException("Calculated absolute index is out of global bit-space bounds.");

            int mappedX = (int)(absoluteIndex % GridX);
            int mappedY = (int)(absoluteIndex / GridX);               // 0..GridY-1

            return new Position_SOM(mappedX, mappedY);
        }

        /// <summary>
        /// Convenience: encode image and return dictionary mapping input pixel (x,y) -> Position_SOM.
        /// Useful for lookup rather than ordered list.
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
