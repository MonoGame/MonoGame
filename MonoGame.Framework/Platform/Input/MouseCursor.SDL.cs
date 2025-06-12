// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Framework.Utilities;

namespace Microsoft.Xna.Framework.Input
{
    public partial class MouseCursor
    {
        private MouseCursor(Sdl.Mouse.SystemCursor cursor)
        {
            Handle = Sdl.Mouse.CreateSystemCursor(cursor);
        }

        private static void PlatformInitalize()
        {
            Arrow = new MouseCursor(Sdl.Mouse.SystemCursor.Arrow);
            IBeam = new MouseCursor(Sdl.Mouse.SystemCursor.IBeam);
            Wait = new MouseCursor(Sdl.Mouse.SystemCursor.Wait);
            Crosshair = new MouseCursor(Sdl.Mouse.SystemCursor.Crosshair);
            WaitArrow = new MouseCursor(Sdl.Mouse.SystemCursor.WaitArrow);
            SizeNWSE = new MouseCursor(Sdl.Mouse.SystemCursor.SizeNWSE);
            SizeNESW = new MouseCursor(Sdl.Mouse.SystemCursor.SizeNESW);
            SizeWE = new MouseCursor(Sdl.Mouse.SystemCursor.SizeWE);
            SizeNS = new MouseCursor(Sdl.Mouse.SystemCursor.SizeNS);
            SizeAll = new MouseCursor(Sdl.Mouse.SystemCursor.SizeAll);
            No = new MouseCursor(Sdl.Mouse.SystemCursor.No);
            Hand = new MouseCursor(Sdl.Mouse.SystemCursor.Hand);
        }

        private static MouseCursor PlatformFromTexture2D(Texture2D texture, int originx, int originy)
        {
            IntPtr surface = IntPtr.Zero;
            IntPtr handle = IntPtr.Zero;
            try
            {
                var bytes = new byte[texture.Width * texture.Height * 4];
                texture.GetData(bytes);
                surface = Sdl.CreateRGBSurfaceFrom(bytes, texture.Width, texture.Height, 32, texture.Width * 4, 0x000000ff, 0x0000FF00, 0x00FF0000, 0xFF000000);
                if (surface == IntPtr.Zero)
                    throw new InvalidOperationException("Failed to create surface for mouse cursor: " + Sdl.GetError());

                handle = Sdl.Mouse.CreateColorCursor(surface, originx, originy);
                if (handle == IntPtr.Zero)
                    throw new InvalidOperationException("Failed to set surface for mouse cursor: " + Sdl.GetError());
            }
            finally
            {
                if (surface != IntPtr.Zero)
                    Sdl.FreeSurface(surface);
            }

            return new MouseCursor(handle);
        }

        private void PlatformDispose()
        {
            if (Handle == IntPtr.Zero)
                return;
            
            Sdl.Mouse.FreeCursor(Handle);
            Handle = IntPtr.Zero;
        }
    }
}