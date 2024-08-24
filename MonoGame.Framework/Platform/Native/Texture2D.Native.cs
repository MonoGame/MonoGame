// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;

namespace Microsoft.Xna.Framework.Graphics;

public partial class Texture2D : Texture
{
    private void PlatformConstruct(int width, int height, bool mipmap, SurfaceFormat format, SurfaceType type, bool shared)
    {

    }

    private void PlatformSetData<T>(int level, T[] data, int startIndex, int elementCount) where T : struct
    {

    }

    private void PlatformSetData<T>(int level, int arraySlice, Rectangle rect, T[] data, int startIndex, int elementCount) where T : struct
    {

    }

    private void PlatformGetData<T>(int level, int arraySlice, Rectangle rect, T[] data, int startIndex, int elementCount) where T : struct
    {

    }

    private static Texture2D PlatformFromStream(GraphicsDevice graphicsDevice, Stream stream)
    {
        return new Texture2D(graphicsDevice, 1, 1);
    }

    private static Texture2D PlatformFromStream(GraphicsDevice graphicsDevice, Stream stream, Action<byte[]> colorProcessor)
    {
        return new Texture2D(graphicsDevice, 1, 1);
    }

    private void PlatformSaveAsJpeg(Stream stream, int width, int height)
    {

    }

    private void PlatformSaveAsPng(Stream stream, int width, int height)
    {

    }

    private void PlatformReload(Stream textureStream)
    {

    }
}
