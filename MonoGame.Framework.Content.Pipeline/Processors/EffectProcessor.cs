// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Graphics;
#if WINDOWS
using TwoMGFX;
#endif

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
#if WINDOWS
            var options = new Options();
            options.SourceFile = input.Identity.SourceFilename;
            options.DX11Profile =   context.TargetPlatform == TargetPlatform.Windows ||
                                    context.TargetPlatform == TargetPlatform.WindowsPhone8 ||
                                    context.TargetPlatform == TargetPlatform.WindowsStoreApp ||
                                    context.TargetPlatform == TargetPlatform.Xbox360;
            options.Debug = DebugMode == EffectProcessorDebugMode.Debug;
            options.OutputFile = context.OutputFilename;

            // Parse the MGFX file expanding includes, macros, and returning the techniques.
            ShaderInfo shaderInfo;
            try
            {
                shaderInfo = ShaderInfo.FromFile(options.SourceFile, options);
                foreach (var dep in shaderInfo.Dependencies)
                    context.AddDependency(dep);
            }
            catch (Exception ex)
            {
                // TODO: Extract good line numbers from mgfx parser!
                throw new InvalidContentException(ex.Message, input.Identity, ex);
            }

            // Create the effect object.
            DXEffectObject effect = null;
            try
            {
                effect = DXEffectObject.FromShaderInfo(shaderInfo);
            }
            catch (Exception ex)
            {
                throw ProcessErrorsAndWarnings(ex.Message, input, context);
            }

            // Write out the effect to a runtime format.
            CompiledEffectContent result;
            try
            {
                using (var stream = new MemoryStream())
                {
                    using (var writer = new BinaryWriter(stream))
                        effect.Write(writer, options);

                    result = new CompiledEffectContent(stream.GetBuffer());
                }
            }
            catch (Exception ex)
            {
                throw new InvalidContentException("Failed to serialize the effect!", input.Identity, ex);
            }

            return result;
#else
            throw new NotImplementedException();
#endif
        }

        private static Exception ProcessErrorsAndWarnings(string errorsAndWarnings, EffectContent input, ContentProcessorContext context)
        {
            // Split the errors by lines.
            var errors = errorsAndWarnings.Split('\n');

            // Process each error line extracting the location and message information.
            for (var i = 0; i < errors.Length; i++)
            {
                // Skip blank lines.
                if (errors[i].StartsWith(Environment.NewLine))
                    break;

                // find some unique characters in the error string
                var openIndex = errors[i].IndexOf('(');
                var closeIndex = errors[i].IndexOf(')');

                // can't process the message if it has no line counter
                if (openIndex == -1 || closeIndex == -1)
                    continue;

                // find the error number, then move forward into the message
                var errorIndex = errors[i].IndexOf('X', closeIndex);
                if (errorIndex < 0)
                    return new InvalidContentException(errors[i], input.Identity);

                // trim out the data we need to feed the logger
                var fileName = errors[i].Remove(openIndex);
                var lineAndColumn = errors[i].Substring(openIndex + 1, closeIndex - openIndex - 1);
                var description = errors[i].Substring(errorIndex);

                // when the file name is not present, the error can be found in the root file
                if (string.IsNullOrEmpty(fileName))
                    fileName = input.Identity.SourceFilename;

                // ensure that the file data points toward the correct file
                var fileInfo = new FileInfo(fileName);
                if (!fileInfo.Exists)
                {
                    var parentFile = new FileInfo(input.Identity.SourceFilename);
                    fileInfo = new FileInfo(Path.Combine(parentFile.Directory.FullName, fileName));
                }
                fileName = fileInfo.FullName;

                // construct the temporary content identity and file the error or warning
                var identity = new ContentIdentity(fileName, input.Identity.SourceTool, lineAndColumn);
                if (errors[i].Contains("warning"))
                {
                    description = "A warning was generated when compiling the effect.\n" + description;
                    context.Logger.LogWarning(string.Empty, identity, description, string.Empty);
                }
                else if (errors[i].Contains("error"))
                {
                    description = "Unable to compile the effect.\n" + description;
                    return new InvalidContentException(description, identity);
                }
            }

            // if no exceptions were created in the above loop, generate a generic one here
            return new InvalidContentException(errorsAndWarnings, input.Identity);
        }
    }
}
