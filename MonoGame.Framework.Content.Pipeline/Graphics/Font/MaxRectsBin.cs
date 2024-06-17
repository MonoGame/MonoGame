/*
This was adapted from a version I found online. Here's the original header:
 	Based on the Public Domain MaxRectsBinPack.cpp source by Jukka Jylänki
 	https://github.com/juj/RectangleBinPack/
 	Ported to C# by Sven Magnus
 	This version is also public domain - do whatever you want with it.
*/

using System;
using System.Collections.Generic;

namespace Microsoft.Xna.Framework.Content.Pipeline.Graphics
{
    /// <summary>
    /// A bin that can pack rectangles using the MaxRects algorithm with several heuristics.
    /// </summary>
    public class MaxRectsBin
    {
        /// <summary>
        /// Width of the bin.
        /// </summary>
        public int BinWidth { get; private set; }

        /// <summary>
        /// Height of the bin.
        /// </summary>
        public int BinHeight { get; private set; }

        /// <summary>
        /// Indicates if rectangle can be rotated.
        /// </summary>
        public bool AllowRotations { get; }

        /// <summary>
        /// Padding to add to both sides in the horizontal dimension. Default is 0.
        /// </summary>
        public int PaddingWidth { get; set; }

        /// <summary>
        /// Padding to add to both sides in the vertical dimension. Default is 0.
        /// </summary>
        public int PaddingHeight { get; set; }

        /// <summary>
        /// Determines if padding is included in returned rectangles. Default is <c>false</c>.
        /// </summary>
        public bool IncludePadding { get; set; }

        private readonly List<Rectangle> _usedRectangles;
        private readonly List<Rectangle> _freeRectangles;

        /// <summary>
        /// The actually used width of the bin.
        /// </summary>
        public int UsedWidth { get; private set; }

        /// <summary>
        /// The actually used height of the bin.
        /// </summary>
        public int UsedHeight { get; private set; }

        /// <summary>
        /// The number of times to grow the bin for a single rectangle when it doesn't fit. Defaults to 3.
        /// </summary>
        public int GrowLimit
        {
            get => _growLimit;
            set
            {
                if (value <= 0)
                    throw new ArgumentOutOfRangeException(nameof(value), "Grow limit must be larger than 0.");
                _growLimit = value;
            }
        }

        /// <summary>
        /// Rule to choose in which dimension to grow.
        /// </summary>
        public GrowRule Grow { get; set; }

        /// <summary>
        ///   Determines how the bin is grown.
        ///   A positive value means a fixed size increase, a negative value means a factor increase.
        ///   E.g. -2 will grow the bin by a factor of 2 when it needs to grow.
        /// </summary>
        public float GrowIncrement { get; set; }

        private bool _growWidth;
        private int _growLimit = 3;

        /// <summary>
        /// Create a <see cref="MaxRectsBin"/> instance.
        /// </summary>
        /// <param name="width">Initial available width. Defaults to 64.</param>
        /// <param name="height">Initial available height. Defaults to 64.</param>
        /// <param name="grow">Whether to allow growing the available space when a rectangle doesn't fit.</param>
        /// <param name="growIncrement">
        ///   Determines how the bin is grown.
        ///   A positive value means a fixed size increase, a negative value means a factor increase.
        ///   E.g. -2 will grow the bin by a factor of 2 when it needs to grow.
        /// </param>
        /// <param name="allowRotation">Whether to allow rotating the rectangles.</param>
        public MaxRectsBin(int width = 64, int height = 64, GrowRule grow = GrowRule.Both, float growIncrement = -2, bool allowRotation = false)
        {
            BinWidth = width;
            BinHeight = height;
            Grow = grow;
            GrowIncrement = growIncrement;
            AllowRotations = allowRotation;

            _usedRectangles = new List<Rectangle>();
            _freeRectangles = new List<Rectangle>();

            var n = new Rectangle(0, 0, width, height);
            _freeRectangles.Add(n);
        }

        private void DoGrow()
        {
            switch (Grow)
            {
                case GrowRule.None:
                    break;
                case GrowRule.Width:
                    GrowWidth();
                    break;
                case GrowRule.Height:
                    GrowHeight();
                    break;
                case GrowRule.Both:
                    // alternate between growing width and height
                    if (_growWidth)
                        GrowWidth();
                    else
                        GrowHeight();
                    _growWidth = !_growWidth;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void GrowWidth()
        {
            var grow = GrowIncrement < 0 ? BinWidth * -(GrowIncrement + 1) : BinWidth + GrowIncrement;

            var rect = new Rectangle(BinWidth, 0, (int)grow, BinHeight);
            BinWidth += (int)grow;
            _freeRectangles.Add(rect);
        }

        private void GrowHeight()
        {
            var grow = GrowIncrement < 0 ? BinHeight * -(GrowIncrement + 1) : BinHeight + GrowIncrement;

            var rect = new Rectangle(0, BinHeight, BinWidth, (int)grow);
            BinHeight += (int)grow;
            _freeRectangles.Add(rect);
        }

        /// <summary>
        /// Insert a rectangle into the bin.
        /// </summary>
        /// <param name="width">Width of the rectangle.</param>
        /// <param name="height">Height of the rectangle.</param>
        /// <param name="heuristic">Heuristic to use.</param>
        /// <returns>The bounds of the rectangle after placement.</returns>
        /// <exception cref="Exception">If the rectangle did not fit in the bin.</exception>
        public Rectangle Insert(int width, int height, MaxRectsHeuristic heuristic)
        {
            if (width == 0 || height == 0)
                return new Rectangle(0, 0, width, height);

            width += PaddingWidth;
            height += PaddingHeight;

            void FindPositionForNewNode(out Rectangle rect)
            {
                var score1 = 0; // Unused in this function. We don't need to know the score after finding the position.
                var score2 = 0;
                switch (heuristic)
                {
                    case MaxRectsHeuristic.Bssf:
                        rect = FindPositionForNewNodeBestShortSideFit(width, height, ref score1, ref score2);
                        break;
                    case MaxRectsHeuristic.Bl:
                        rect = FindPositionForNewNodeBottomLeft(width, height, ref score1, ref score2);
                        break;
                    case MaxRectsHeuristic.RectContactPointRule:
                        rect = FindPositionForNewNodeContactPoint(width, height, ref score1);
                        break;
                    case MaxRectsHeuristic.Blsf:
                        rect = FindPositionForNewNodeBestLongSideFit(width, height, ref score2, ref score1);
                        break;
                    case MaxRectsHeuristic.Baf:
                        rect = FindPositionForNewNodeBestAreaFit(width, height, ref score1, ref score2);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(heuristic), heuristic, null);
                }
            }

            FindPositionForNewNode(out var newNode);

            if (Grow == GrowRule.None && newNode.Height == 0)
                throw new Exception($"Failed to pack rectangle of size ({width}, {height}). BinWidth: {BinWidth}, BinHeight: {BinHeight}. Growing not allowed.");

            var growTries = 0;
            while (newNode.Height == 0)
            {
                DoGrow();
                growTries++;
                FindPositionForNewNode(out newNode);
                if (growTries >= GrowLimit)
                    throw new Exception($"Failed to pack rectangle of size ({width}, {height}). BinWidth: {BinWidth}, BinHeight: {BinHeight}, Occupancy: {GetOccupancy()}");
            }

            PlaceRect(ref newNode);

            if (newNode.Right > UsedWidth)
                UsedWidth = newNode.Right;
            if (newNode.Bottom > UsedHeight)
                UsedHeight = newNode.Bottom;

            if (!IncludePadding)
                newNode = new Rectangle(
                    newNode.X + PaddingWidth, newNode.Y + PaddingHeight,
                    newNode.Width - PaddingWidth * 2, newNode.Height - PaddingHeight * 2);
            return newNode;
        }

        /// <summary>
        /// Place an array of rectangles in the bin.
        /// </summary>
        /// <param name="sizes">Sizes of the rectangles.</param>
        /// <param name="heuristic">Heuristic to use.</param>
        /// <returns>Bounds of all rectangles after placement.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="sizes"/> is <c>null</c>.</exception>
        public Rectangle[] Insert(Point[] sizes, MaxRectsHeuristic heuristic = MaxRectsHeuristic.Bssf)
        {
            if (sizes == null)
                throw new ArgumentNullException(nameof(sizes));
            var bounds = new Rectangle[sizes.Length];
            Insert(sizes, bounds, 0, heuristic);
            return bounds;
        }

        /// <summary>
        /// Place an array of rectangles in the bin.
        /// </summary>
        /// <param name="sizes">Sizes of the rectangles.</param>
        /// <param name="bounds">Array to fill with rectangle bounds.</param>
        /// <param name="indexOffset">Index into <paramref name="bounds"/> to start filling.</param>
        /// <param name="heuristic">Heuristic to use.</param>
        /// <exception cref="ArgumentNullException">
        ///   If <paramref name="sizes"/> or <paramref name="bounds"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentException">If <paramref name="indexOffset"/> is less than 0.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        ///   If <paramref name="bounds"/> is not large enough to hold all bounds.
        ///   I.e. <c>bounds.Length - indexOffset &lt; sizes.Length</c>
        /// </exception>
        public void Insert(Point[] sizes, Rectangle[] bounds, int indexOffset = 0, MaxRectsHeuristic heuristic = MaxRectsHeuristic.Bssf)
        {
            if (sizes == null)
                throw new ArgumentNullException(nameof(sizes));
            if (bounds == null)
                throw new ArgumentNullException(nameof(bounds));
            if (indexOffset < 0)
                throw new ArgumentOutOfRangeException(nameof(indexOffset), "Index offset must be larger than 0.");
            if (bounds.Length - indexOffset < sizes.Length)
                throw new ArgumentException("Packed array not large enough to hold bounds.", nameof(bounds));

            var indices = new List<int>(sizes.Length);
            for (var i = 0; i < sizes.Length; i++)
                indices.Add(i);

            while (indices.Count > 0)
            {
                Rectangle bestNode;
                var bestRectIndex = GetBestRect(sizes, heuristic, indices, out bestNode);
                if (Grow == GrowRule.None && bestRectIndex == -1)
                    throw new Exception($"Failed to pack rectangles with {indices.Count} rectangles left. BinWidth: {BinWidth}, BinHeight: {BinHeight}. Growing not allowed.");

                var growTries = 0;
                while (bestRectIndex == -1)
                {
                    DoGrow();
                    growTries++;
                    bestRectIndex = GetBestRect(sizes, heuristic, indices, out bestNode);
                    if (growTries >= GrowLimit)
                        throw new Exception($"Failed to pack rectangles with {indices.Count} rectangles left. BinWidth: {BinWidth}, BinHeight: {BinHeight}, Occupancy: {GetOccupancy()}");
                }

                PlaceRect(ref bestNode);
                var ogIndex = indices[bestRectIndex];
                indices.RemoveAt(bestRectIndex);

                if (bestNode.Right > UsedWidth)
                    UsedWidth = bestNode.Right;
                if (bestNode.Bottom > UsedHeight)
                    UsedHeight = bestNode.Bottom;

                if (!IncludePadding)
                    bestNode = new Rectangle(
                        bestNode.X + PaddingWidth, bestNode.Y + PaddingHeight,
                        bestNode.Width - PaddingWidth * 2, bestNode.Height - PaddingHeight * 2);
                bounds[indexOffset + ogIndex] = bestNode;
            }
        }

        private int GetBestRect(Point[] sizes, MaxRectsHeuristic heuristic, List<int> indices, out Rectangle bestNode)
        {
            var bestScore1 = int.MaxValue;
            var bestScore2 = int.MaxValue;
            var bestRectIndex = -1;
            bestNode = new Rectangle();

            for (var indirectIndex = 0; indirectIndex < indices.Count; indirectIndex++)
            {
                var i = indices[indirectIndex];
                var score1 = 0;
                var score2 = 0;
                var newNode = ScoreRect(sizes[i].X + PaddingWidth * 2, sizes[i].Y + PaddingHeight * 2, heuristic,
                    ref score1,
                    ref score2);

                if (score1 < bestScore1 || (score1 == bestScore1 && score2 < bestScore2))
                {
                    bestScore1 = score1;
                    bestScore2 = score2;
                    bestNode = newNode;
                    bestRectIndex = indirectIndex;
                }
            }

            return bestRectIndex;
        }

        private void PlaceRect(ref Rectangle node)
        {
            var numRectanglesToProcess = _freeRectangles.Count;
            for (var i = 0; i < numRectanglesToProcess; ++i)
            {
                if (SplitFreeNode(_freeRectangles[i], ref node))
                {
                    _freeRectangles.RemoveAt(i);
                    --i;
                    --numRectanglesToProcess;
                }
            }

            PruneFreeList();

            _usedRectangles.Add(node);
        }

        private Rectangle ScoreRect(int width, int height, MaxRectsHeuristic method, ref int score1, ref int score2)
        {
            var newNode = new Rectangle();
            score1 = int.MaxValue;
            score2 = int.MaxValue;
            switch (method)
            {
                case MaxRectsHeuristic.Bssf:
                    newNode = FindPositionForNewNodeBestShortSideFit(width, height, ref score1, ref score2);
                    break;
                case MaxRectsHeuristic.Bl:
                    newNode = FindPositionForNewNodeBottomLeft(width, height, ref score1, ref score2);
                    break;
                case MaxRectsHeuristic.RectContactPointRule:
                    newNode = FindPositionForNewNodeContactPoint(width, height, ref score1);
                    score1 = -score1; // Reverse since we are minimizing, but for contact point score bigger is better.
                    break;
                case MaxRectsHeuristic.Blsf:
                    newNode = FindPositionForNewNodeBestLongSideFit(width, height, ref score2, ref score1);
                    break;
                case MaxRectsHeuristic.Baf:
                    newNode = FindPositionForNewNodeBestAreaFit(width, height, ref score1, ref score2);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(method), method, null);
            }

            // Cannot fit the current rectangle.
            if (newNode.Height == 0)
            {
                score1 = int.MaxValue;
                score2 = int.MaxValue;
            }

            return newNode;
        }

        /// <summary>
        /// Computes the ratio of used surface area.
        /// </summary>
        /// <param name="crop">
        ///   If <c>true</c> this uses <see cref="UsedWidth"/> and <see cref="UsedHeight"/> instead of
        ///   <see cref="BinWidth"/> and <see cref="BinHeight"/> for available area.
        /// </param>
        public float GetOccupancy(bool crop = false)
        {
            ulong usedSurfaceArea = 0;
            for (var i = 0; i < _usedRectangles.Count; ++i)
                usedSurfaceArea += (uint)_usedRectangles[i].Width * (uint)_usedRectangles[i].Height;

            var available = crop ? UsedWidth * UsedHeight : BinWidth * BinHeight;
            return (float)usedSurfaceArea / available;
        }

        private Rectangle FindPositionForNewNodeBottomLeft(int width, int height, ref int bestY, ref int bestX)
        {
            var bestNode = new Rectangle();

            bestY = int.MaxValue;

            for (var i = 0; i < _freeRectangles.Count; ++i)
            {
                // Try to place the rectangle in upright (non-flipped) orientation.
                if (_freeRectangles[i].Width >= width && _freeRectangles[i].Height >= height)
                {
                    var topSideY = _freeRectangles[i].Y + height;
                    if (topSideY < bestY || (topSideY == bestY && _freeRectangles[i].X < bestX))
                    {
                        bestNode = new Rectangle(_freeRectangles[i].X, _freeRectangles[i].Y, width, height);
                        bestY = topSideY;
                        bestX = _freeRectangles[i].X;
                    }
                }

                if (AllowRotations && _freeRectangles[i].Width >= height && _freeRectangles[i].Height >= width)
                {
                    var topSideY = _freeRectangles[i].Y + width;
                    if (topSideY < bestY || (topSideY == bestY && _freeRectangles[i].X < bestX))
                    {
                        bestNode = new Rectangle(_freeRectangles[i].X, _freeRectangles[i].Y, height, width);
                        bestY = topSideY;
                        bestX = _freeRectangles[i].X;
                    }
                }
            }

            return bestNode;
        }

        private Rectangle FindPositionForNewNodeBestShortSideFit(int width, int height, ref int bestShortSideFit,
            ref int bestLongSideFit)
        {
            var bestNode = new Rectangle();

            bestShortSideFit = int.MaxValue;

            for (var i = 0; i < _freeRectangles.Count; ++i)
            {
                // Try to place the rectangle in upright (non-flipped) orientation.
                if (_freeRectangles[i].Width >= width && _freeRectangles[i].Height >= height)
                {
                    var leftoverHoriz = Math.Abs(_freeRectangles[i].Width - width);
                    var leftoverVert = Math.Abs(_freeRectangles[i].Height - height);
                    var shortSideFit = Math.Min(leftoverHoriz, leftoverVert);
                    var longSideFit = Math.Max(leftoverHoriz, leftoverVert);

                    if (shortSideFit < bestShortSideFit ||
                        (shortSideFit == bestShortSideFit && longSideFit < bestLongSideFit))
                    {
                        bestNode = new Rectangle(_freeRectangles[i].X, _freeRectangles[i].Y, width, height);
                        bestShortSideFit = shortSideFit;
                        bestLongSideFit = longSideFit;
                    }
                }

                if (AllowRotations && _freeRectangles[i].Width >= height && _freeRectangles[i].Height >= width)
                {
                    var flippedLeftoverHoriz = Math.Abs(_freeRectangles[i].Width - height);
                    var flippedLeftoverVert = Math.Abs(_freeRectangles[i].Height - width);
                    var flippedShortSideFit = Math.Min(flippedLeftoverHoriz, flippedLeftoverVert);
                    var flippedLongSideFit = Math.Max(flippedLeftoverHoriz, flippedLeftoverVert);

                    if (flippedShortSideFit < bestShortSideFit ||
                        (flippedShortSideFit == bestShortSideFit && flippedLongSideFit < bestLongSideFit))
                    {
                        bestNode = new Rectangle(_freeRectangles[i].X, _freeRectangles[i].Y, height, width);
                        bestShortSideFit = flippedShortSideFit;
                        bestLongSideFit = flippedLongSideFit;
                    }
                }
            }

            return bestNode;
        }

        private Rectangle FindPositionForNewNodeBestLongSideFit(int width, int height, ref int bestShortSideFit,
            ref int bestLongSideFit)
        {
            var bestNode = new Rectangle();

            bestLongSideFit = int.MaxValue;

            for (var i = 0; i < _freeRectangles.Count; ++i)
            {
                // Try to place the rectangle in upright (non-flipped) orientation.
                if (_freeRectangles[i].Width >= width && _freeRectangles[i].Height >= height)
                {
                    var leftoverHoriz = Math.Abs(_freeRectangles[i].Width - width);
                    var leftoverVert = Math.Abs(_freeRectangles[i].Height - height);
                    var shortSideFit = Math.Min(leftoverHoriz, leftoverVert);
                    var longSideFit = Math.Max(leftoverHoriz, leftoverVert);

                    if (longSideFit < bestLongSideFit ||
                        (longSideFit == bestLongSideFit && shortSideFit < bestShortSideFit))
                    {
                        bestNode = new Rectangle(_freeRectangles[i].X, _freeRectangles[i].Y, width, height);
                        bestShortSideFit = shortSideFit;
                        bestLongSideFit = longSideFit;
                    }
                }

                if (AllowRotations && _freeRectangles[i].Width >= height && _freeRectangles[i].Height >= width)
                {
                    var leftoverHoriz = Math.Abs(_freeRectangles[i].Width - height);
                    var leftoverVert = Math.Abs(_freeRectangles[i].Height - width);
                    var shortSideFit = Math.Min(leftoverHoriz, leftoverVert);
                    var longSideFit = Math.Max(leftoverHoriz, leftoverVert);

                    if (longSideFit < bestLongSideFit ||
                        (longSideFit == bestLongSideFit && shortSideFit < bestShortSideFit))
                    {
                        bestNode = new Rectangle(_freeRectangles[i].X, _freeRectangles[i].Y, height, width);
                        bestShortSideFit = shortSideFit;
                        bestLongSideFit = longSideFit;
                    }
                }
            }

            return bestNode;
        }

        private Rectangle FindPositionForNewNodeBestAreaFit(int width, int height, ref int bestAreaFit,
            ref int bestShortSideFit)
        {
            var bestNode = new Rectangle();

            bestAreaFit = int.MaxValue;

            for (var i = 0; i < _freeRectangles.Count; ++i)
            {
                var areaFit = _freeRectangles[i].Width * _freeRectangles[i].Height - width * height;

                // Try to place the rectangle in upright (non-flipped) orientation.
                if (_freeRectangles[i].Width >= width && _freeRectangles[i].Height >= height)
                {
                    var leftoverHoriz = Math.Abs(_freeRectangles[i].Width - width);
                    var leftoverVert = Math.Abs(_freeRectangles[i].Height - height);
                    var shortSideFit = Math.Min(leftoverHoriz, leftoverVert);

                    if (areaFit < bestAreaFit || (areaFit == bestAreaFit && shortSideFit < bestShortSideFit))
                    {
                        bestNode = new Rectangle(_freeRectangles[i].X, _freeRectangles[i].Y, width, height);
                        bestShortSideFit = shortSideFit;
                        bestAreaFit = areaFit;
                    }
                }

                if (AllowRotations && _freeRectangles[i].Width >= height && _freeRectangles[i].Height >= width)
                {
                    var leftoverHoriz = Math.Abs(_freeRectangles[i].Width - height);
                    var leftoverVert = Math.Abs(_freeRectangles[i].Height - width);
                    var shortSideFit = Math.Min(leftoverHoriz, leftoverVert);

                    if (areaFit < bestAreaFit || (areaFit == bestAreaFit && shortSideFit < bestShortSideFit))
                    {
                        bestNode = new Rectangle(_freeRectangles[i].X, _freeRectangles[i].Y, height, width);
                        bestShortSideFit = shortSideFit;
                        bestAreaFit = areaFit;
                    }
                }
            }

            return bestNode;
        }

        /// Returns 0 if the two intervals i1 and i2 are disjoint, or the length of their overlap otherwise.
        private int CommonIntervalLength(int i1Start, int i1End, int i2Start, int i2End)
        {
            if (i1End < i2Start || i2End < i1Start)
                return 0;
            return Math.Min(i1End, i2End) - Math.Max(i1Start, i2Start);
        }

        private int ContactPointScoreNode(int x, int y, int width, int height)
        {
            var score = 0;

            if (x == 0 || x + width == BinWidth)
                score += height;
            if (y == 0 || y + height == BinHeight)
                score += width;

            for (var i = 0; i < _usedRectangles.Count; ++i)
            {
                if (_usedRectangles[i].X == x + width || _usedRectangles[i].X + _usedRectangles[i].Width == x)
                    score += CommonIntervalLength(_usedRectangles[i].Y,
                        _usedRectangles[i].Y + _usedRectangles[i].Height, y, y + height);
                if (_usedRectangles[i].Y == y + height || _usedRectangles[i].Y + _usedRectangles[i].Height == y)
                    score += CommonIntervalLength(_usedRectangles[i].X,
                        _usedRectangles[i].X + _usedRectangles[i].Width, x, x + width);
            }

            return score;
        }

        private Rectangle FindPositionForNewNodeContactPoint(int width, int height, ref int bestContactScore)
        {
            var bestNode = new Rectangle();

            bestContactScore = -1;

            for (var i = 0; i < _freeRectangles.Count; ++i)
            {
                // Try to place the rectangle in upright (non-flipped) orientation.
                if (_freeRectangles[i].Width >= width && _freeRectangles[i].Height >= height)
                {
                    var score = ContactPointScoreNode(_freeRectangles[i].X, _freeRectangles[i].Y, width,
                        height);
                    if (score > bestContactScore)
                    {
                        bestNode = new Rectangle(_freeRectangles[i].X, _freeRectangles[i].Y, width, height);
                        bestContactScore = score;
                    }
                }

                if (AllowRotations && _freeRectangles[i].Width >= height && _freeRectangles[i].Height >= width)
                {
                    var score = ContactPointScoreNode(_freeRectangles[i].X, _freeRectangles[i].Y, height,
                        width);
                    if (score > bestContactScore)
                    {
                        bestNode = new Rectangle(_freeRectangles[i].X, _freeRectangles[i].Y, height, width);
                        bestContactScore = score;
                    }
                }
            }

            return bestNode;
        }

        private bool SplitFreeNode(Rectangle freeNode, ref Rectangle usedNode)
        {
            // Test with SAT if the rectangles even intersect.
            if (usedNode.Left >= freeNode.Right || usedNode.Right <= freeNode.Left ||
                usedNode.Top >= freeNode.Bottom || usedNode.Bottom <= freeNode.Top)
                return false;

            if (usedNode.Left < freeNode.Right && usedNode.Right > freeNode.Left)
            {
                // New node at the top side of the used node.
                if (usedNode.Top > freeNode.Top && usedNode.Top < freeNode.Bottom)
                {
                    var newNode = new Rectangle(freeNode.X, freeNode.Y, freeNode.Width, usedNode.Top - freeNode.Top);
                    _freeRectangles.Add(newNode);
                }

                // New node at the bottom side of the used node.
                if (usedNode.Bottom < freeNode.Bottom)
                {
                    var newNode = new Rectangle(freeNode.X, usedNode.Bottom, freeNode.Width, freeNode.Bottom - usedNode.Bottom);
                    _freeRectangles.Add(newNode);
                }
            }

            if (usedNode.Top < freeNode.Bottom && usedNode.Bottom > freeNode.Top)
            {
                // New node at the left side of the used node.
                if (usedNode.Left > freeNode.Left && usedNode.Left < freeNode.Right)
                {
                    var newNode = new Rectangle(freeNode.Left, freeNode.Top, usedNode.Left - freeNode.Left, freeNode.Height);
                    _freeRectangles.Add(newNode);
                }

                // New node at the right side of the used node.
                if (usedNode.X + usedNode.Width < freeNode.X + freeNode.Width)
                {
                    var newNode = new Rectangle(usedNode.Right, freeNode.Top, freeNode.Right - usedNode.Right, freeNode.Height);
                    _freeRectangles.Add(newNode);
                }
            }

            return true;
        }

        private void PruneFreeList()
        {
            for (var i = 0; i < _freeRectangles.Count; ++i)
                for (var j = i + 1; j < _freeRectangles.Count; ++j)
                {
                    if (IsContainedIn(_freeRectangles[i], _freeRectangles[j]))
                    {
                        _freeRectangles.RemoveAt(i);
                        --i;
                        break;
                    }

                    if (IsContainedIn(_freeRectangles[j], _freeRectangles[i]))
                    {
                        _freeRectangles.RemoveAt(j);
                        --j;
                    }
                }
        }

        private bool IsContainedIn(Rectangle a, Rectangle b)
        {
            return a.Left >= b.Left &&
                   a.Top >= b.Top &&
                   a.Right <= b.Right &&
                   a.Bottom <= b.Bottom;
        }
    }
}
