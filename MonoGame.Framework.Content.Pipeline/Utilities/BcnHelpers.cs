// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;
using BCnEncoder.Encoder;
using BCnEncoder.Shared;
using KtxSharp;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;

namespace Microsoft.Xna.Framework.Content.Pipeline.Utilities;

/// <summary>
/// The <see cref="Encode"/> method uses the https://github.com/Nominom/BCnEncoder.NET/tree/master
/// library to encode various block based compression formats
/// </summary>
internal static class BcnUtil
{
    /// <summary>
    /// Compress the given <see cref="sourceBitmap"/> to the desired block format <see cref="destinationFormat"/>.
    /// No files will be written to disk, and no external tools are invoked.
    /// </summary>
    /// <param name="sourceBitmap"></param>
    /// <param name="destinationFormat"></param>
    /// <param name="compressedBytes"></param>
    public static void Encode(
        BitmapContent sourceBitmap,
        CompressionFormat destinationFormat,
        out byte[] compressedBytes)
    {
        // first, copy the bitmap to a local color (rgba32) format
        var colorBitmap = new PixelBitmapContent<Color>(sourceBitmap.Width, sourceBitmap.Height);
        BitmapContent.Copy(sourceBitmap, colorBitmap);
        var data = colorBitmap.GetPixelData();


        var dataSpan = new ReadOnlySpan<byte>(data);
        var ktxStream = new MemoryStream();

        // setup the encoder
        BcEncoder encoder = new BcEncoder
        {
            OutputOptions =
            {
                GenerateMipMaps = false,
                Quality = CompressionQuality.BestQuality,
                Format = destinationFormat,
                FileFormat = OutputFileFormat.Ktx
            }
        };

        // encode to ktx format
        encoder.EncodeToStream(dataSpan, sourceBitmap.Width, sourceBitmap.Height, PixelFormat.Rgba32, ktxStream);

        // decode the ktx format and find the raw bytes
        ktxStream.Seek(0, SeekOrigin.Begin);
        var ktx = KtxLoader.LoadInput(ktxStream);
        compressedBytes = ktx.textureData.textureDataOfMipmapLevel[0];
    }
}
