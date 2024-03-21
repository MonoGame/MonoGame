// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Microsoft.Xna.Framework.Graphics;

namespace Microsoft.Xna.Framework.Input
{
    /// <summary>
    /// Describes a mouse cursor.
    /// </summary>
    public partial class MouseCursor : IDisposable
    {
        /// <summary>
        /// Gets the default arrow cursor.
        /// </summary>
        public static MouseCursor Arrow { get; private set; }

        /// <summary>
        /// Gets the cursor that appears when the mouse is over text editing regions.
        /// </summary>
        public static MouseCursor IBeam { get; private set; }

        /// <summary>
        /// Gets the waiting cursor that appears while the application/system is busy.
        /// </summary>
        public static MouseCursor Wait { get; private set; }

        /// <summary>
        /// Gets the crosshair ("+") cursor.
        /// </summary>
        public static MouseCursor Crosshair { get; private set; }

        /// <summary>
        /// Gets the cross between Arrow and Wait cursors.
        /// </summary>
        public static MouseCursor WaitArrow { get; private set; }

        /// <summary>
        /// Gets the northwest/southeast ("\") cursor.
        /// </summary>
        public static MouseCursor SizeNWSE { get; private set; }

        /// <summary>
        /// Gets the northeast/southwest ("/") cursor.
        /// </summary>
        public static MouseCursor SizeNESW { get; private set; }

        /// <summary>
        /// Gets the horizontal west/east ("-") cursor.
        /// </summary>
        public static MouseCursor SizeWE { get; private set; }

        /// <summary>
        /// Gets the vertical north/south ("|") cursor.
        /// </summary>
        public static MouseCursor SizeNS { get; private set; }

        /// <summary>
        /// Gets the size all cursor which points in all directions.
        /// </summary>
        public static MouseCursor SizeAll { get; private set; }

        /// <summary>
        /// Gets the cursor that points that something is invalid, usually a cross.
        /// </summary>
        public static MouseCursor No { get; private set; }

        /// <summary>
        /// Gets the hand cursor, usually used for web links.
        /// </summary>
        public static MouseCursor Hand { get; private set; }

        /// <summary>
        /// Creates a mouse cursor from the specified texture.
        /// </summary>
        /// <param name="texture">Texture to use as the cursor image.</param>
        /// <param name="originx">X cordinate of the image that will be used for mouse position.</param>
        /// <param name="originy">Y cordinate of the image that will be used for mouse position.</param>
        public static MouseCursor FromTexture2D(Texture2D texture, int originx, int originy)
        {
            if (texture.Format != SurfaceFormat.Color && texture.Format != SurfaceFormat.ColorSRgb)
                throw new ArgumentException("Only Color or ColorSrgb textures are accepted for mouse cursors", "texture");

            return PlatformFromTexture2D(texture, originx, originy);
        }

        /// <summary>
        /// Gets a handle for this cursor.
        /// </summary>
        public IntPtr Handle { get; private set; }

        private bool _disposed;

        static MouseCursor()
        {
            PlatformInitalize();
        }

        private MouseCursor(IntPtr handle)
        {
            Handle = handle;
        }

        /// <inheritdoc cref="IDisposable.Dispose()"/>
        public void Dispose()
        {
            if (_disposed)
                return;

            PlatformDispose();
            _disposed = true;
        }
    }
}
