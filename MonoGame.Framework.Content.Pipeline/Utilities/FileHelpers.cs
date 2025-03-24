// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;
using KtxSharp;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using MonoGame.Framework.Content;
using StbImageWriteSharp;

namespace Microsoft.Xna.Framework.Content.Pipeline.Utilities;

/// <summary>
/// Utility methods for interacting with .png files
/// <para>
/// This is marked as internal because it is not part of the original XNA api.
/// </para>
/// </summary>
internal static class PngFileHelper
{
    /// <summary>
    /// Convert a <see cref="BitmapContent"/> into a <see cref="PixelBitmapContent{Color}"/> and write
    /// it to disk as a png file.
    ///
    /// This method requires that there is an active context in the <see cref="ContextScopeFactory"/>,
    /// otherwise a <see cref="PipelineException"/> will be thrown
    /// </summary>
    /// <param name="sourceBitmap">the bitmap to write to disk</param>
    /// <param name="pngFileName">
    /// The resulting name of the png image that was written.
    /// This method will not delete the image.
    /// </param>
    public static void WritePngToIntermediate(
        BitmapContent sourceBitmap,
        out string pngFileName)
    {
        var context = ContextScopeFactory.ActiveContext;

        // unfortunately, basisU requires an input _file_.
        pngFileName = $"tempImage_{Guid.NewGuid().ToString()}.png"; // TODO: get a project relative path.
        pngFileName = Path.Combine(context.IntermediateDirectory, pngFileName);
        var directory = Path.GetDirectoryName(pngFileName);
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
        using var fileStream = File.OpenWrite(pngFileName);

        // in order to save a png, we need a colorBitmap.
        var colorBitmap = new PixelBitmapContent<Color>(sourceBitmap.Width, sourceBitmap.Height);
        BitmapContent.Copy(sourceBitmap, colorBitmap);

        var data = colorBitmap.GetPixelData();
        var writer = new ImageWriter();
        writer.WritePng(data, colorBitmap.Width, colorBitmap.Height, ColorComponents.RedGreenBlueAlpha, fileStream);
        fileStream.Close();
    }
}

/// <summary>
/// Utility methods for .ktx files
/// <para>
/// This is marked as internal because it is not part of the original XNA api.
/// </para>
/// </summary>
internal static class KtxFileHelper
{
    /// <summary>
    /// Read in a given ktx file and extract the compressed bytes stored within the file.
    ///
    /// The ktx file format can hold data for multiple mip levels, but
    /// As of August 19th 2024, this method only returns the data for the first mip level.
    /// </summary>
    /// <param name="inputKtxFileName">the path to the ktx file</param>
    /// <param name="compressedBytes">the compressed bytes will be stored here.</param>
    /// <returns></returns>
    public static bool TryReadKtx(string inputKtxFileName, out byte[] compressedBytes)
    {
        compressedBytes = Array.Empty<byte>();
        if (!File.Exists(inputKtxFileName))
        {
            return false;
        }

        var bytes = File.ReadAllBytes(inputKtxFileName);
        using var stream = new MemoryStream(bytes);
        var ktx = KtxLoader.LoadInput(stream);
        compressedBytes = ktx.textureData.textureDataOfMipmapLevel[0];
        return true;
    }
}
