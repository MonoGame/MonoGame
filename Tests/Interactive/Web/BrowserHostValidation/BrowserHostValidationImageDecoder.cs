// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;
using System.Runtime.InteropServices;

namespace BrowserHostValidation;

internal static unsafe class BrowserHostValidationImageDecoder
{
    internal static DecodedImage DecodeRgba(Stream stream, Action<byte[]>? colorProcessor)
    {
        ArgumentNullException.ThrowIfNull(stream);

        if (!stream.CanSeek)
            throw new ArgumentException("Stream must support seeking.", nameof(stream));

        if (stream.Length > int.MaxValue)
            throw new InvalidOperationException("Validation image stream is too large to decode.");

        stream.Seek(0, SeekOrigin.Begin);

        int dataLength = (int)stream.Length;
        byte[] encodedBytes = new byte[dataLength];
        stream.ReadExactly(encodedBytes);

        fixed (byte* encodedBytesPointer = encodedBytes)
        {
            byte* rgbaBytesPointer;
            int width;
            int height;

            ReadRgba(
                encodedBytesPointer,
                dataLength,
                0,
                out width,
                out height,
                out rgbaBytesPointer);

            if (rgbaBytesPointer == null)
            {
                throw new InvalidOperationException(
                    "Failed to decode RGBA data from the validation image stream.");
            }

            try
            {
                int rgbaByteCount = checked(width * height * 4);
                byte[] rgbaBytes = new byte[rgbaByteCount];
                Marshal.Copy((nint)rgbaBytesPointer, rgbaBytes, 0, rgbaByteCount);

                if (colorProcessor != null)
                    colorProcessor(rgbaBytes);

                return new DecodedImage(width, height, rgbaBytes);
            }
            finally
            {
                FreeRgba(rgbaBytesPointer);
            }
        }
    }

    [DllImport("mgruntime", EntryPoint = "MGI_ReadRGBA", ExactSpelling = true)]
    private static extern void ReadRgba(
        byte* data,
        int dataBytes,
        uint processors,
        out int width,
        out int height,
        out byte* rgba);

    [DllImport("mgruntime", EntryPoint = "MGI_FreeRGBA", ExactSpelling = true)]
    private static extern void FreeRgba(
        byte* rgba);
}
