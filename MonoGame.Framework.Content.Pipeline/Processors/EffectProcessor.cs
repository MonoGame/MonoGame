// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Framework.Utilities;

namespace Microsoft.Xna.Framework.Content.Pipeline.Processors
{
    /// <summary>
    /// Processes a string representation to a platform-specific compiled effect.
    /// </summary>
    [ContentProcessor(DisplayName = "Effect - MonoGame")]
    public class EffectProcessor : ContentProcessor<EffectContent, CompiledEffectContent>
    {
        EffectProcessorDebugMode debugMode;
        string defines;

        /// <summary>
        /// The debug mode for compiling effects.
        /// </summary>
        /// <value>The debug mode to use when compiling effects.</value>
        public virtual EffectProcessorDebugMode DebugMode { get { return debugMode; } set { debugMode = value; } }

        /// <summary>
        /// Define assignments for the effect.
        /// </summary>
        /// <value>A list of define assignments delimited by semicolons.</value>
        public virtual string Defines { get { return defines; } set { defines = value; } }

        /// <summary>
        /// Initializes a new instance of EffectProcessor.
        /// </summary>
        public EffectProcessor()
        {
        }

        /// <summary>
        /// Processes the string representation of the specified effect into a platform-specific binary format using the specified context.
        /// </summary>
        /// <param name="input">The effect string to be processed.</param>
        /// <param name="context">Context for the specified processor.</param>
        /// <returns>A platform-specific compiled binary effect.</returns>
        /// <remarks>If you get an error during processing, compilation stops immediately. The effect processor displays an error message. Once you fix the current error, it is possible you may get more errors on subsequent compilation attempts.</remarks>
        public override CompiledEffectContent Process(EffectContent input, ContentProcessorContext context)
        {
            var mgfxc = Path.Combine(Path.GetDirectoryName(typeof(EffectProcessor).Assembly.Location), "mgfxc.dll");
            var sourceFile = input.Identity.SourceFilename;
            var destFile = Path.GetTempFileName();
            var arguments = "\"" + mgfxc + "\" \"" + sourceFile + "\" \"" + destFile + "\" /Profile:" + GetProfileForPlatform(context.TargetPlatform);

            if (debugMode == EffectProcessorDebugMode.Debug)
                arguments += " /Debug";

            if (!string.IsNullOrWhiteSpace(defines))
                arguments += " \"/Defines:" + defines + "\"";

            string stdout, stderr;

            var success = ExternalTool.Run("dotnet", arguments, out stdout, out stderr) == 0;
            var ret = success ? new CompiledEffectContent(File.ReadAllBytes(destFile)) : null;

            File.Delete(destFile);

            var stdOutLines = stdout.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            foreach (var line in stdOutLines)
            {
                if (line.StartsWith("Dependency:") && line.Length > 12)
                {
                    context.AddDependency(line.Substring(12));
                }
            }

            // ShaderConductor outputs errors and warnings different compared to the DX compiler,
            // so let's use separate code paths for error processing.
            var profile = GetProfileForPlatform(context.TargetPlatform);
            bool isOpenGL = profile == "OpenGL" || profile == "OpenGLES";
            bool isMojoShader = isOpenGL && Defines != null && Defines.Contains("MOJO");
            bool isShaderConductor = isOpenGL && !isMojoShader;

            if (isShaderConductor)
                ProcessErrorsAndWarningsSC(!success, stderr, input, context);
            else
                ProcessErrorsAndWarningsDX(!success, stderr, input, context);

            return ret;
        }

        private string GetProfileForPlatform(TargetPlatform platform)
        {
            switch (platform)
            {
                case TargetPlatform.Windows:
                case TargetPlatform.WindowsPhone8:
                case TargetPlatform.WindowsStoreApp:
                    return "DirectX_11";
                case TargetPlatform.DesktopGL:
                case TargetPlatform.MacOSX:
                case TargetPlatform.RaspberryPi:
                case TargetPlatform.Web:
                    return "OpenGL";
                case TargetPlatform.iOS:
                case TargetPlatform.Android:
                    return "OpenGLES";
            }

            return platform.ToString();
        }

        private static void ProcessErrorsAndWarningsDX(bool buildFailed, string shaderErrorsAndWarnings, EffectContent input, ContentProcessorContext context)
        {
            // Split the errors and warnings into individual lines.
            var errorsAndWarningArray = shaderErrorsAndWarnings.Split(new[] { "\n", "\r", Environment.NewLine },
                                                                      StringSplitOptions.RemoveEmptyEntries);

            var errorOrWarning = new Regex(@"(.*)\(([0-9]*(,[0-9]+(-[0-9]+)?)?)\)\s*:\s*(.*)", RegexOptions.Compiled);
            ContentIdentity identity = null;
            var allErrorsAndWarnings = string.Empty;

            // Process all the lines.
            for (var i = 0; i < errorsAndWarningArray.Length; i++)
            {    
                var match = errorOrWarning.Match(errorsAndWarningArray[i]);
                if (!match.Success || match.Groups.Count != 4)
                {
                    // Just log anything we don't recognize as a warning.
                    if (buildFailed)
                        allErrorsAndWarnings += errorsAndWarningArray[i] + Environment.NewLine;
                    else
                        context.Logger.LogWarning(string.Empty, input.Identity, errorsAndWarningArray[i]);

                    continue;
                }
            
                var fileName = match.Groups[1].Value;
                var lineAndColumn = match.Groups[2].Value;
                var message = match.Groups[3].Value;

                // Try to ensure a good file name for the error message.
                if (string.IsNullOrEmpty(fileName))
                    fileName = input.Identity.SourceFilename;
                else if (!File.Exists(fileName))
                {
                    var folder = Path.GetDirectoryName(input.Identity.SourceFilename);
                    fileName = Path.Combine(folder, fileName);
                }

                // If we got an exception then we'll be throwing an exception 
                // below, so just gather the lines to throw later.
                if (buildFailed)
                {
                    if (identity == null)
                    {
                        identity = new ContentIdentity(fileName, input.Identity.SourceTool, lineAndColumn);
                        allErrorsAndWarnings = errorsAndWarningArray[i] + Environment.NewLine;
                    }
                    else
                        allErrorsAndWarnings += errorsAndWarningArray[i] + Environment.NewLine;
                }
                else
                {
                    identity = new ContentIdentity(fileName, input.Identity.SourceTool, lineAndColumn);
                    context.Logger.LogWarning(string.Empty, identity, message, string.Empty);
                }
            }

            if (buildFailed)
                throw new InvalidContentException(allErrorsAndWarnings, identity ?? input.Identity);
        }

        private static void ProcessErrorsAndWarningsSC(bool buildFailed, string shaderErrorsAndWarnings, EffectContent input, ContentProcessorContext context)
        {
            // Split the errors and warnings into individual lines.
            var errorsAndWarningArray = shaderErrorsAndWarnings.Split(new[] { "\n", "\r", Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            var errorOrWarningRegex = new Regex(@"(.*):(([0-9]+):([0-9]+)):\s*(error|warning):\s*(.*)", RegexOptions.Compiled);

            string unhandledMessages = "";
            bool errorOrWarningHappend = false;

            // Process all the lines.
            for (var i = 0; i < errorsAndWarningArray.Length; i++)
            {
                var line = errorsAndWarningArray[i];
                var match = errorOrWarningRegex.Match(line);

                if (match.Success)
                {
                    // This line is an error or warning.
                    // Create an error string that looks like an error message from DX,
                    // so we don't need a separate code path for ShaderConductor errors in the MGCB Editor
                    var fileName = match.Groups[1].Value;
                    var lineAndColumn = match.Groups[2].Value.Replace(':', ','); // Replace ShaderConductor's colon with a comma to make it look like DX
                    var errorOrWarning = match.Groups[5].Value;
                    var message = match.Groups[6].Value;
                    var errorCode = "S0000"; // DX outputs error codes, ShaderConductor doesn't. We'll always use this generic code
                    var msg = fileName + "(" + lineAndColumn + "): " + errorOrWarning + " " + errorCode + ": " + message;

                    context.Logger.LogWarning(string.Empty, input.Identity, msg);
                    errorOrWarningHappend = true;
                }
                else
                {
                    // ShaderConductor outputs additional info in the lines following the actual error/warning message.
                    // If the previous lines contained an error we will simply log all the following lines.
                    if (errorOrWarningHappend)
                    {
                        // ShaderConductor outputs the faulty line, plus a pointer character in the following line, which points at the exact character location of the error.
                        // This only works for monospace fonts. The MGCB editor doesn't use a momospace font currently for displaying errors and warnings,
                        // so there's no point in logging the line containing this pointer.
                        bool isPointer = line.EndsWith("^") && String.IsNullOrWhiteSpace(line.Substring(0, line.Length - 1));
                        if (!isPointer)
                            context.Logger.LogMessage(line);
                    }
                    else
                    {
                        // If the previous lines didn't contain an error, it's probably an exception message. We don't log those messages now,
                        // instead we'll throw an exception in the end with those messages attached.   
                        unhandledMessages += line + Environment.NewLine;
                    }
                }
            }

            if (buildFailed)
                throw new InvalidContentException(unhandledMessages, input.Identity);
        }
    }
}
