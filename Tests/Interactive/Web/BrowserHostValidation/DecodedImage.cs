// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace BrowserHostValidation;

internal sealed class DecodedImage
{
    internal DecodedImage(int width, int height, byte[] rgbaBytes)
    {
        Width = width;
        Height = height;
        RgbaBytes = rgbaBytes;
    }

    internal int Width { get; }

    internal int Height { get; }

    internal byte[] RgbaBytes { get; }
}
