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
    /// How to grow the bin when a rectangel can't be placed.
    /// </summary>
    public enum GrowRule
    {
        /// <summary>
        /// Don't grow the bin, throw an exception when full.
        /// </summary>
        None,
        /// <summary>
        /// Grow the bins width.
        /// </summary>
        Width,
        /// <summary>
        /// Grow the bins height.
        /// </summary>
        Height,
        /// <summary>
        /// Alternate growing the bins width and height. Starts with width.
        /// </summary>
        Both
    }
}
