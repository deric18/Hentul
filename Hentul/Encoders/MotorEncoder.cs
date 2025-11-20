using Common;
using System.Collections.Generic;
using System.Linq;

namespace Hentul.Encoders
{
    /// <summary>
    /// Mouse moves and their encodings.
    /// </summary>
    public enum MouseMove
    {
        Up = 0,
        Down = 1,
        Left = 2,
        Right = 3,
        UpLeft = 4,
        UpRight = 5,
        DownLeft = 6,
        DownRight = 7
    }

    /// <summary>
    /// Encodes mouse moves into stable SDRs (TEMPORAL / motor-like input) and can decode them back.
    /// </summary>
    public sealed class MotorEncoder
    {
        private readonly Dictionary<MouseMove, int> codeMap = new()
        {
            { MouseMove.Up, 0 },
            { MouseMove.Down, 1 },
            { MouseMove.Left, 2 },
            { MouseMove.Right, 3 },
            { MouseMove.UpLeft, 4 },
            { MouseMove.UpRight, 5 },
            { MouseMove.DownLeft, 6 },
            { MouseMove.DownRight, 7 }
        };

        // Lazy-built reverse map of canonical position sets -> MouseMove
        private static Dictionary<MouseMove, HashSet<(int x, int y)>>? _canonicalPatterns;
        private static readonly object _lock = new();

        public int Encode(MouseMove move) => codeMap[move];

        public SDR_SOM ToSDR(MouseMove move, int numColumns = 10, int z = 4, iType type = iType.SPATIAL)
        {
            var bits = move switch
            {
                MouseMove.Up => new List<Position_SOM>
                {
                    P(1, 1), P(2, 1), P(3, 1), P(4, 1)
                },
                MouseMove.Down => new List<Position_SOM>
                {
                    P(5, 8), P(7, 8), P(3, 8), P(4, 8)
                },
                MouseMove.Left => new List<Position_SOM>
                {
                    P(1, 5), P(9, 6), P(7, 7), P(4, 4)
                },
                MouseMove.Right => new List<Position_SOM>
                {
                    P(2, 5), P(5, 6), P(8, 7), P(6, 4)
                },
                MouseMove.UpLeft => new List<Position_SOM>
                {
                    P(1, 1), P(2, 1),
                    P(1, 5), P(9, 6)
                },
                MouseMove.UpRight => new List<Position_SOM>
                {
                    P(1, 1), P(2, 1),
                    P(2, 5), P(5, 6)
                },
                MouseMove.DownLeft => new List<Position_SOM>
                {
                    P(5, 8), P(7, 8),
                    P(1, 5), P(9, 6)
                },
                MouseMove.DownRight => new List<Position_SOM>
                {
                    P(5, 8), P(7, 8),
                    P(2, 5), P(5, 6)
                },
                _ => new List<Position_SOM>()
            };

            return new SDR_SOM(numColumns, z, bits, type);
        }

        /// <summary>
        /// Reverse of ToSDR. Attempts to decode an SDR_SOM into its MouseMove.
        /// Strategy:
        /// 1. Exact match (set equality) against canonical patterns.
        /// 2. If no exact match and allowApproximate == true, pick the pattern with the highest overlap (ties return null).
        /// Returns null if no suitable match.
        /// </summary>
        public MouseMove? Decode(SDR_SOM sdr, bool allowApproximate = true, double minPrecision = 0.5)
        {
            if (sdr == null || sdr.ActiveBits == null || sdr.ActiveBits.Count == 0)
                return null;

            EnsureCanonicalPatterns();

            var inputSet = new HashSet<(int x, int y)>(sdr.ActiveBits.Select(b => (b.X, b.Y)));

            // 1. Exact match
            foreach (var kvp in _canonicalPatterns!)
            {
                if (kvp.Value.SetEquals(inputSet))
                    return kvp.Key;
            }

            if (!allowApproximate)
                return null;

            // 2. Approximate: compute precision = matched / patternSize
            MouseMove? bestMove = null;
            int bestMatched = -1;
            double bestPrecision = -1;

            foreach (var kvp in _canonicalPatterns)
            {
                int matched = kvp.Value.Count(p => inputSet.Contains(p));
                if (matched == 0) continue;

                double precision = matched / (double)kvp.Value.Count;

                if (precision >= minPrecision)
                {
                    if (precision > bestPrecision ||
                        (precision == bestPrecision && matched > bestMatched))
                    {
                        bestPrecision = precision;
                        bestMatched = matched;
                        bestMove = kvp.Key;
                    }
                    else if (precision == bestPrecision && matched == bestMatched)
                    {
                        // Ambiguous tie -> return null to avoid misclassification
                        bestMove = null;
                    }
                }
            }

            return bestMove;
        }

        /// <summary>
        /// Returns all candidate moves with overlap stats for diagnostics.
        /// </summary>
        public IEnumerable<(MouseMove move, int matched, int patternSize, double precision)> DecodeWithScores(SDR_SOM sdr)
        {
            if (sdr == null || sdr.ActiveBits == null || sdr.ActiveBits.Count == 0)
                return Enumerable.Empty<(MouseMove, int, int, double)>();

            EnsureCanonicalPatterns();

            var inputSet = new HashSet<(int x, int y)>(sdr.ActiveBits.Select(b => (b.X, b.Y)));

            return _canonicalPatterns!
                .Select(kvp =>
                {
                    int matched = kvp.Value.Count(p => inputSet.Contains(p));
                    int size = kvp.Value.Count;
                    double precision = size == 0 ? 0 : matched / (double)size;
                    return (kvp.Key, matched, size, precision);
                })
                .Where(t => t.matched > 0)
                .OrderByDescending(t => t.precision)
                .ThenByDescending(t => t.matched)
                .ToList();
        }

        private static void EnsureCanonicalPatterns()
        {
            if (_canonicalPatterns != null) return;

            lock (_lock)
            {
                if (_canonicalPatterns != null) return;

                var encoder = new MotorEncoder();
                _canonicalPatterns = new Dictionary<MouseMove, HashSet<(int x, int y)>>();

                foreach (MouseMove mv in System.Enum.GetValues(typeof(MouseMove)))
                {
                    var sdr = encoder.ToSDR(mv); // uses default shape
                    _canonicalPatterns[mv] = new HashSet<(int x, int y)>(
                        sdr.ActiveBits.Select(p => (p.X, p.Y)));
                }
            }
        }

        private static Position_SOM P(int x, int y) => new Position_SOM(x, y);
    }
}