// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using MonoGame.Framework.Content;

namespace Microsoft.Xna.Framework.Content.Pipeline.Utilities
{

    /// <summary>
    /// <para>
    /// The <see cref="CrunchFormat"/> holds all information required to invoke the
    /// Crunch tool for a given configuration.
    /// </para>
    ///
    /// <para>
    /// As of August 19 2024, the only important field is the <see cref="formatString"/>,
    /// but Crunch supports many compression-option flags that may become important.
    /// </para>
    ///
    /// <para>
    /// This is marked as internal because it is not part of the original XNA api.
    /// </para>
    /// </summary>
    internal struct CrunchFormat
    {
        /// <summary>
        /// Represents the format for the compression. The following is taken from the --help invocation of Crunch as of August 19 2024.
        /// <para>
        ///
        /// All supported texture formats (Note: .CRN only supports DXTn pixel formats):
        /// -DXT1
        /// -DXT2
        /// -DXT3
        /// -DXT4
        /// -DXT5
        /// -3DC
        /// -DXN
        /// -DXT5A
        /// -DXT5_CCxY
        /// -DXT5_xGxR
        /// -DXT5_xGBR
        /// -DXT5_AGBR
        /// -DXT1A
        /// -ETC1
        /// -ETC2
        /// -ETC2A
        /// -ETC1S
        /// -ETC2AS
        /// -R8G8B8
        /// -L8
        /// -A8
        /// -A8L8
        /// -A8R8G8B8
        ///
        /// </para>
        /// </summary>
        public string formatString;

        // private constructor because all known formats should be declared as static-readonlys
        private CrunchFormat(string formatString)
        {
            this.formatString = formatString;
        }

        public override string ToString()
        {
            return $"crunch(name={formatString})";
        }

        /// <summary>
        /// Dxt3 (or Bc2)
        /// </summary>
        public static readonly CrunchFormat Dxt3 = new CrunchFormat
        {
            formatString = "DXT3"
        };

        /// <summary>
        /// Dxt1 (or Bc1)
        /// </summary>
        public static readonly CrunchFormat Dxt1 = new CrunchFormat
        {
            formatString = "DXT1A"
        };

        /// <summary>
        /// Dxt1a is a variant of Dxt1 that supports a 1-bit alpha
        /// </summary>
        public static readonly CrunchFormat Dxt1A = new CrunchFormat
        {
            formatString = "DXT1A"
        };

        /// <summary>
        /// Dxt5 (or Bc3)
        /// </summary>
        public static readonly CrunchFormat Dxt5 = new CrunchFormat
        {
            formatString = "DXT5"
        };

        /// <summary>
        /// Etc1
        /// </summary>
        public static readonly CrunchFormat Etc1 = new CrunchFormat
        {
            formatString = "ETC1"
        };

        /// <summary>
        /// Etc2A supports alpha
        /// </summary>
        public static readonly CrunchFormat Etc2A = new CrunchFormat
        {
            formatString = "ETC2A"
        };
    }

    /// <summary>
    /// Utilities for using the Crunch tool.
    /// <para>
    /// This is marked as internal because it is not part of the original XNA api.
    /// </para>
    /// </summary>
    internal static class Crunch
    {
        /// <summary>
        /// A lightweight wrapper to invoke the Crunch tool (called 'crunch').
        /// https://github.com/MonoGame/MonoGame.Tool.Crunch
        /// </summary>
        /// <param name="args">The args to pass to crunch. These args are passed directly to the tool with no processing. </param>
        /// <param name="stdOut">The standard output buffer from the crunch process</param>
        /// <param name="stdErr">The standard error buffer from the crunch process</param>
        /// <returns>The exit code for the basisu process. </returns>
        private static int Run(string args, out string stdOut, out string stdErr)
        {
            return ExternalTool.RunDotnetTool("mgcb-crunch", args, out stdOut, out stdErr);
        }

        /// <summary>
        /// This method will use Crunch to compress the <see cref="sourceBitmap"/> into the
        /// desired <see cref="CrunchFormat"/>.
        ///
        /// <para>
        /// This method can only be called if there is an active context in the <see cref="ContextScopeFactory"/>
        /// </para>
        /// </summary>
        /// <param name="sourceBitmap">
        /// The <see cref="BitmapContent"/> to be compressed.
        /// </param>
        /// <param name="crunchFormat">
        /// The desired <see cref="CrunchFormat"/> to use.
        /// </param>
        /// <param name="encodedBytes">
        /// The resulting compressed bytes.
        /// </param>
        /// <exception cref="PipelineException"></exception>
        public static void EncodeBytes(
            BitmapContent sourceBitmap,
            CrunchFormat crunchFormat,
            out byte[] encodedBytes
        )
        {
            if (!TryEncodeBytes(
                    sourceBitmap: sourceBitmap,
                    crunchFormat: crunchFormat,
                    encodedBytes: out encodedBytes,
                    failureMessage: out var error))
            {
                throw new PipelineException($"Failed to crunch. {error}");
            }
        }

        private static bool TryEncodeBytes(
            BitmapContent sourceBitmap,
            CrunchFormat crunchFormat,
            out byte[] encodedBytes,
            out string failureMessage)
        {
            failureMessage = null;
            encodedBytes = Array.Empty<byte>();

            // these files will likely be created during this method, and should be
            //  deleted before exiting the function.
            string pngFileName = null;
            string ktxFileName = null;

            try
            {
                PngFileHelper.WritePngToIntermediate(sourceBitmap, out pngFileName);

                ktxFileName = pngFileName + ".ktx";

                if (!TryCrunch(crunchFormat, pngFileName, ktxFileName, out var error))
                {
                    failureMessage = error;
                    return false;
                }

                if (!KtxFileHelper.TryReadKtx(ktxFileName, out encodedBytes))
                {
                    failureMessage = "unable to read unpacked ktx file";
                    return false;
                }

                return true;
            }
            finally
            {
                if (!string.IsNullOrEmpty(pngFileName))
                {
                    ExternalTool.DeleteFile(pngFileName);
                }

                if (!string.IsNullOrEmpty(ktxFileName))
                {
                    ExternalTool.DeleteFile(ktxFileName);
                }
            }
        }

        /// <summary>
        /// Run the crunch tool for a given format and generate a ktx file as output
        /// </summary>
        private static bool TryCrunch(
            CrunchFormat format,
            string pngFileName,
            string intermediateFileName,
            out string errorMessage
        )
        {
            errorMessage = null;
            var argStr = $"-file {pngFileName} -{format.formatString} -out {intermediateFileName} -fileformat ktx -forceprimaryencoding -noNormalDetection";
            var exitCode = Run(
                args: argStr,
                stdOut: out var stdOut,
                stdErr: out var stdErr
            );

            var wasSuccess = exitCode == 0;
            if (!wasSuccess)
            {
                errorMessage = $"failed to use crunch tool. args=[{argStr}] stdOut=[{stdOut}] stdErr=[{stdErr}] exitCode=[{exitCode}]";
            }

            return wasSuccess;
        }

    }
}

