﻿// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Microsoft.Xna.Framework.Graphics;

namespace Microsoft.Xna.Framework.Content.Pipeline.Graphics
{
    public class PvrtcRgba2BitmapContent : PvrtcBitmapContent
    {
        /// <summary>
        /// Creates an instance of PvrtcRgba2BitBitmapContent with the specified width and height.
        /// </summary>
        /// <param name="width">The width in pixels of the bitmap.</param>
        /// <param name="height">The height in pixels of the bitmap.</param>
        public PvrtcRgba2BitmapContent(int width, int height)
            : base(width, height)
        {
        }

        /// <summary>
        /// Gets the corresponding GPU texture format for the specified bitmap type.
        /// </summary>
        /// <param name="format">Format being retrieved.</param>
        /// <returns>The GPU texture format of the bitmap type.</returns>
        public override bool TryGetFormat(out SurfaceFormat format)
        {
            format = SurfaceFormat.RgbaPvrtc2Bpp;
            return true;
        }

        /// <summary>
        /// Returns a string description of the bitmap.
        /// </summary>
        /// <returns>Description of the bitmap.</returns>
        public override string ToString()
        {
            return "PVRTC RGBA 2bpp " + Width + "x" + Height;
        }
    }
}
