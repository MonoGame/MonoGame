/*
This was adapted from a version I found online. Here's the original header:
 	Based on the Public Domain MaxRectsBinPack.cpp source by Jukka Jylänki
 	https://github.com/juj/RectangleBinPack/
 	Ported to C# by Sven Magnus
 	This version is also public domain - do whatever you want with it.
*/

namespace Microsoft.Xna.Framework.Content.Pipeline.Graphics
{
    /// <summary>
    /// Heuristic method to use to choose the rectangles positions.
    /// </summary>
    public enum MaxRectsHeuristic
    {
        /// <summary>
        /// Best Short Side Fit: Positions the rectangle against the short side of a free rectangle into which it fits the best.
        /// </summary>
        Bssf,

        /// <summary>
        /// Best Long Side Fit: Positions the rectangle against the long side of a free rectangle into which it fits the best.
        /// </summary>
        Blsf,

        /// <summary>
        /// Best Area Fit: Positions the rectangle into the smallest free rect into which it fits.
        /// </summary>
        Baf,

        /// <summary>
        /// Bottom Left (Tetris method): Each rectangle is placed to a position (possibly rotating it) where its top side lies as low as possible.
        /// </summary>
        Bl,

        /// <summary>
        /// Contact Point: Choosest the placement where the rectangle touches other rects as much as possible.
        /// </summary>
        RectContactPointRule
    }
}
