// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Utilities;

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
            var sourceFile = Path.GetTempFileName();
            var destFile = Path.GetTempFileName();
            var target = context.TargetPlatform.ToString().Contains("Windows");
            
            File.WriteAllText(sourceFile, input.EffectCode);

            var proc = new Process();
            proc.StartInfo.FileName = "dotnet";
            proc.StartInfo.Arguments = "\"" + mgfxc + "\" \"" + sourceFile + "\" \"" + destFile + "\" /Profile:" + GetProfileForPlatform(context.TargetPlatform.ToString());

            if (debugMode == EffectProcessorDebugMode.Debug)
                proc.StartInfo.Arguments += " /Debug";

            if (!string.IsNullOrWhiteSpace(defines))
                proc.StartInfo.Arguments += " \"/Defines:" + defines + "\"";

            proc.Start();
            proc.WaitForExit();

            var success = proc.ExitCode == 0;
            var ret = success ? new CompiledEffectContent(File.ReadAllBytes(destFile)) : null;

            File.Delete(sourceFile);
            File.Delete(destFile);

            if (!success)
                throw new Exception("mgfxc exited with non 0 exit code.");

            return ret;
        }

        private string GetProfileForPlatform(string platform)
        {
            if (platform == "Windows" ||
                platform == "WindowsPhone8" ||
                platform == "WindowsStoreApp")
                return "DirectX_11";

            return "OpenGL";
        }
    }
}
