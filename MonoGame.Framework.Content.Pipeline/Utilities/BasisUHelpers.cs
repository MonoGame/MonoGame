// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Graphics;
using KtxSharp;
using StbImageWriteSharp;

namespace Microsoft.Xna.Framework.Content.Pipeline.Utilities;

/// <summary>
/// <para>
/// BasisU has a set of supported formats, documented here,
/// https://github.com/BinomialLLC/basis_universal/blob/master/transcoder/basisu_transcoder.h#L49
/// </para>
///
/// <para>
/// The <see cref="BasisUFormat"/> struct captures the enum value, the human readable name, and flag information
/// for the various basisU formats that apply to Monogame.
/// </para>
/// </summary>
internal struct BasisUFormat
{
    /// <summary>
    /// BasisU uses an integer to represent texture formats on the CLI
    /// </summary>
    public int code;

    /// <summary>
    /// By default, basisU will assume that all input color formats are in sRGB.
    /// If the format requires linear color space, that must be set manually.
    /// </summary>
    public bool isLinearColorSpace;

    /// <summary>
    /// The human readable name of the format
    /// </summary>
    public string name;

    // Instead of constructing one of these yourself, please use one of the many predefined static class members.
    private BasisUFormat(int code, string name, bool isLinearColorSpace=false)
    {
        this.code = code;
        this.name = name;
        this.isLinearColorSpace = isLinearColorSpace;
    }

    /// <summary>
    /// <see cref="SurfaceFormat.RgbEtc1"/>
    /// Opaque only, returns RGB or alpha data if cDecodeFlagsTranscodeAlphaDataToOpaqueFormats flag is specified
    /// </summary>
    public static readonly BasisUFormat RgbEtc1 = new BasisUFormat(
        code: 0,
        name: "cTFETC1_RGB",
        isLinearColorSpace: true
    );

    /// <summary>
    /// <see cref="SurfaceFormat.Rgba8Etc2"/>
    /// Opaque+alpha, ETC2_EAC_A8 block followed by a ETC1 block, alpha channel will be opaque for opaque .basis files
    /// </summary>
    public static readonly BasisUFormat RgbaEtc2 = new BasisUFormat(
        code: 1,
        name: "cTFETC2_RGBA",
        isLinearColorSpace: true
    );


    /// <summary>
    /// <see cref="SurfaceFormat.Rgba8Etc2"/>
    /// Opaque+alpha, ETC2_EAC_A8 block followed by a ETC1 block, alpha channel will be opaque for opaque .basis files
    /// </summary>
    public static readonly BasisUFormat SRgbaEtc2 = new BasisUFormat(
        code: 1,
        name: "cTFETC2_RGBA",
        isLinearColorSpace: false
    );

    /// <summary>
    /// <see cref="SurfaceFormat.RgbaAtcInterpolatedAlpha"/>
    /// Opaque+alpha, alpha channel will be opaque for opaque .basis files. ATI ATC (GL_ATC_RGBA_INTERPOLATED_ALPHA_AMD)
    /// </summary>
    public static readonly BasisUFormat RgbaAtcInterpolatedAlpha = new BasisUFormat(
        code: 12,
        name: "cTFATC_RGBA",
        isLinearColorSpace: true
    );

    /// <summary>
    /// <see cref="SurfaceFormat.Dxt1"/>
    /// Opaque only, no punchthrough alpha support yet, transcodes alpha slice if cDecodeFlagsTranscodeAlphaDataToOpaqueFormats flag is specified
    /// </summary>
    public static readonly BasisUFormat Dxt1 = new BasisUFormat(
        code: 2,
        name: "cTFBC1_RGB",
        isLinearColorSpace: true
    );


    /// <summary>
    /// <see cref="SurfaceFormat.Dxt1"/>
    /// Opaque only, no punchthrough alpha support yet, transcodes alpha slice if cDecodeFlagsTranscodeAlphaDataToOpaqueFormats flag is specified
    /// </summary>
    public static readonly BasisUFormat Dxt1S = new BasisUFormat(
        code: 2,
        name: "cTFBC1_RGB",
        isLinearColorSpace: false
    );

    /// <summary>
    /// <see cref="SurfaceFormat.Dxt5"/>
    /// Opaque+alpha, BC4 followed by a BC1 block, alpha channel will be opaque for opaque .basis files
    /// </summary>
    public static readonly BasisUFormat Dxt5 = new BasisUFormat(
        code: 3,
        name: "cTFBC3_RGBA",
        isLinearColorSpace: true
    );


    /// <summary>
    /// <see cref="SurfaceFormat.Dxt5"/>
    /// Opaque+alpha, BC4 followed by a BC1 block, alpha channel will be opaque for opaque .basis files
    /// </summary>
    public static readonly BasisUFormat Dxt5S = new BasisUFormat(
        code: 3,
        name: "cTFBC3_RGBA",
        isLinearColorSpace: false
    );

    /// <summary>
    /// <see cref="SurfaceFormat.RgbPvrtc4Bpp"/>
    /// Opaque only, RGB or alpha if cDecodeFlagsTranscodeAlphaDataToOpaqueFormats flag is specified, nearly lowest quality of any texture format.
    /// </summary>
    public static readonly BasisUFormat RgbPvrtc4Bpp = new BasisUFormat(
        code: 8,
        name: "cTFPVRTC1_4_RGB",
        isLinearColorSpace: true
    );

    /// <summary>
    /// <see cref="SurfaceFormat.RgbaPvrtc4Bpp"/>
    ///  Opaque+alpha, most useful for simple opacity maps. If .basis file doesn't have alpha cTFPVRTC1_4_RGB will be used instead. Lowest quality of any supported texture format.
    /// </summary>
    public static readonly BasisUFormat RgbaPvrtc4Bpp = new BasisUFormat(
        code: 9,
        name: "cTFPVRTC1_4_RGBA",
        isLinearColorSpace: true
    );


}

/// <summary>
/// Utilities for using the Basis Universal tool.
/// </summary>
internal static class BasisU
{
    /// <summary>
    /// A lightweight wrapper to invoke the Basis Universal tool (called 'basisu').
    /// </summary>
    /// <param name="args">The args to pass to basisu. These args are passed directly to the tool with no processing. </param>
    /// <param name="stdOut">The standard output buffer from the basisu process</param>
    /// <param name="stdErr">The standard error buffer from the basisu process</param>
    /// <param name="stdIn">The standard input buffer for the basisu process. By default, no standard input is sent to the process. </param>
    /// <returns>The exit code for the basisu process. </returns>
    public static int Run(string args, out string stdOut, out string stdErr, string stdIn=null)
    {
        return ExternalTool.Run("basisu", args, out stdOut, out stdErr, stdIn);
    }

    public static bool TryGetBasisUFormat(SurfaceFormat format, out BasisUFormat basisUFormat, out string error)
    {
        basisUFormat = default;
        error = "";
        switch (format)
        {
            // ATC formats
            case SurfaceFormat.RgbaAtcExplicitAlpha:
                // explicit ATC rgba isn't supported. RGB is, but not RGBA explicit.
                //  Here is a doc describing the difference between explicit and interpolated RGBA ATC
                //  https://registry.khronos.org/OpenGL/extensions/AMD/AMD_compressed_ATC_texture.txt
                error = "basisU does not support explicit ATC.";
                return false;
            case SurfaceFormat.RgbaAtcInterpolatedAlpha:
                basisUFormat = BasisUFormat.RgbaAtcInterpolatedAlpha;
                return true;

            // ETC formats
            case SurfaceFormat.RgbEtc1:
                basisUFormat = BasisUFormat.RgbEtc1;
                return true;
            case SurfaceFormat.Rgba8Etc2:
                basisUFormat = BasisUFormat.RgbaEtc2;
                return true;
            case SurfaceFormat.SRgb8A8Etc2:
                basisUFormat = BasisUFormat.SRgbaEtc2;
                return true;

            // these ETC formats are not supported in BasisU
            //  https://github.com/BinomialLLC/basis_universal/issues/216
            case SurfaceFormat.Rgb8A1Etc2:
            case SurfaceFormat.Srgb8A1Etc2:
                error = "basisU does not support RGB8 A1 ETC2";
                return false;
            case SurfaceFormat.Rgb8Etc2:
            case SurfaceFormat.Srgb8Etc2:
                error = "basisU does not support RGB8 ETC2";
                return false;

            // DXT formats (also called BCn)
            case SurfaceFormat.Dxt1:// dxt1 is often called Bc1
                basisUFormat = BasisUFormat.Dxt1;
                return true;
            case SurfaceFormat.Dxt1SRgb:
                basisUFormat = BasisUFormat.Dxt1S;
                return true;
            case SurfaceFormat.Dxt1a:
                error = "Dxt1a is not supported in basisU; the docs state, 'Opaque only, no punchthrough alpha support yet'";
                return false;
            case SurfaceFormat.Dxt3: // dtx3 is often called Bc2
            case SurfaceFormat.Dxt3SRgb:
                // Dxt3 is not supported by basisU
                //  https://github.com/BinomialLLC/basis_universal/issues/63#issuecomment-535778254
                // error = "Dxt3 is not supported in basisU. Dxt3 is rarely used. Use Dxt5 instead.";
                // return false;
                basisUFormat = BasisUFormat.Dxt5; // best effort?
                return true;
            case SurfaceFormat.Dxt5: // dxt5 is often called Bc3
                basisUFormat = BasisUFormat.Dxt5;
                return true;
            case SurfaceFormat.Dxt5SRgb:
                basisUFormat = BasisUFormat.Dxt5S;
                return true;

            // POWERVR Texture Compression
            case SurfaceFormat.RgbPvrtc2Bpp:
            case SurfaceFormat.RgbaPvrtc2Bpp:
                error = "PowerVR 2 bytes per pixel formats are not supported in basisU";
                return false;

            case SurfaceFormat.RgbPvrtc4Bpp:
                basisUFormat = BasisUFormat.RgbPvrtc4Bpp;
                return true;
            case SurfaceFormat.RgbaPvrtc4Bpp:
                basisUFormat = BasisUFormat.RgbaPvrtc4Bpp;
                return true;

            default:
                // it is slightly redundant to return false in the default case when there are
                //  explicitly listed false returns throughout unsupported formats.
                //  But it is valuable to be explicit about the formats that are not supported
                //  as of July 18th, 2024.
                error = "unsupported";
                return false;
        }
    }

    public static bool TryEncodeBytes(
        BitmapContent sourceBitmap,
        int width,
        int height,
        bool hasAlpha,
        bool isLinearColor,
        SurfaceFormat format,
        out byte[] encodedBytes)
    {
        encodedBytes = Array.Empty<byte>();

        if (!TryGetBasisUFormat(format, out var basisUFormat, out var basisUError))
        {
            // TODO: is there a way to log?
            return false;
        }

        if (!TryWritePngFile(sourceBitmap, width, height, hasAlpha, isLinearColor, out var imageFile))
        {
            return false;
        }

        var intermediateFileName = imageFile + ".ktx";

        if (!TryBuildIntermediateFile(imageFile, intermediateFileName, out var stdErr))
        {
            return false;
        }

        if (!TryUnpackKtx(intermediateFileName, basisUFormat, out var ktxFileName))
        {
            return false;
        }

        if (!TryReadKtx(ktxFileName, out encodedBytes))
        {
            return false;
        }

        return true;
    }

    public static bool TryWritePngFile(
        BitmapContent sourceBitmap,
        int width,
        int height,
        bool hasAlpha,
        bool isLinearColor,
        out string pngFileName)
    {
        pngFileName = $"tempImage_{Guid.NewGuid().ToString()}.png"; // TODO: get a project relative path.
        using var fileStream = File.OpenWrite(pngFileName);

        // in order to save a png, we need a colorBitmap.
        var colorBitmap = new PixelBitmapContent<Color>(sourceBitmap.Width, sourceBitmap.Height);
        BitmapContent.Copy(sourceBitmap, colorBitmap);

        var data = colorBitmap.GetPixelData();
        var writer = new ImageWriter(); // TODO: cache this instance.
        writer.WritePng(data, colorBitmap.Width, colorBitmap.Height, ColorComponents.RedGreenBlueAlpha, fileStream);
        fileStream.Close();
        return true;
    }

    public static bool TryReadKtx(string inputKtxFileName, out byte[] compressedBytes)
    {
        var bytes = File.ReadAllBytes(inputKtxFileName);
        using var stream = new MemoryStream(bytes);
        // KtxLoader.CheckIfInputIsValid()
        var ktx = KtxLoader.LoadInput(stream);

        compressedBytes = ktx.textureData.textureDataOfMipmapLevel[0];
        return true;
    }

    public static bool TryUnpackKtx(
        string basisFileName,
        BasisUFormat basisUFormat,
        out string outputKtxFileName
    )
    {
        outputKtxFileName = null;

        // taken from the basisu docs, https://github.com/MonoGame/MonoGame.Tool.BasisUniversal
        //  basisu -unpack foo.ktx2 -ktx_only -linear -format_only 2
        var linearFlag = basisUFormat.isLinearColorSpace ? "-linear" : "";
        // var linearFlag = "";
        // -ktx_only
        var argStr = $"-unpack -file {basisFileName}  -format_only {basisUFormat.code} {linearFlag}";
        var exitCode = Run(
            args: argStr,
            stdOut: out var stdOut,
            stdErr: out var stdErr);
        // TODO: is there a standard logging practice to log stdErr/stdOut if the exitCode is non-zero?
        var isSuccess = exitCode == 0;
        if (!isSuccess)
            return false;


        // standard output should have many lines of output, where close to the end
        //  it should log the output path.
        //  Example:
        //   Transcode of layer 0 level 0 face 0 res 1800x1350 format ATC_RGBA succeeded
        //   Wrote KTX file "inputFile_transcoded_ATC_RGBA_0000.ktx"
        //   Success
        //
        //  this output needs to be captured later.
        var lines = stdOut.Split(
            separator: Environment.NewLine,
            options: StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries
        );

        // move backwards through the lines because the output we are looking for
        //  should be at the end of the output
        const string logPrefix = "Wrote KTX file \"";
        const string logSuffix = "\"";
        for (var i = lines.Length - 1; i >= 0; i--)
        {
            var line = lines[i];
            if (!line.StartsWith(logPrefix)) continue;

            var fileNameLength = line.Length - logPrefix.Length - logSuffix.Length;
            outputKtxFileName = line.Substring(logPrefix.Length, fileNameLength);
            break;
        }

        var parsedKtxFileName = !string.IsNullOrEmpty(outputKtxFileName);

        return parsedKtxFileName;
    }

    public static bool TryBuildIntermediateFile(
        string imageFileName,
        string intermediateFileName,
        out string stdErr)
    {
        var absImageFileName = Path.GetFullPath(imageFileName);
        // taken from the basisu docs, https://github.com/MonoGame/MonoGame.Tool.BasisUniversal
        //  basisu -file foo.png -ktx2
        // var uastcFlag = "-uastc";
        var argStr = $"-file {absImageFileName} -uastc -ktx2 -output_file {intermediateFileName}";
        var exitCode = Run(
            args: argStr,
            stdOut: out var stdOut,
            stdErr: out stdErr);
        // TODO: is there a standard logging practice to log stdErr/stdOut if the exitCode is non-zero?
        var isSuccess = exitCode == 0;
        return isSuccess;
    }
}
