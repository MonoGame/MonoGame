// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;
using System.Text.RegularExpressions;
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

            switch (context.TargetPlatform)
            {
                case TargetPlatform.Windows:
                case TargetPlatform.WindowsPhone8:
                case TargetPlatform.WindowsStoreApp:
                    options.Profile = ShaderProfile.DirectX_11;
                    break;
                case TargetPlatform.iOS:
                case TargetPlatform.Android:
                case TargetPlatform.DesktopGL:
                case TargetPlatform.MacOSX:
                case TargetPlatform.RaspberryPi:
                    options.Profile = ShaderProfile.OpenGL;
                    break;
                default:
                    throw new InvalidContentException(string.Format("{0} effects are not supported.", context.TargetPlatform), input.Identity);
            }

            options.Debug = DebugMode == EffectProcessorDebugMode.Debug;
            options.Defines = Defines;
            options.OutputFile = context.OutputFilename;

            // Parse the MGFX file expanding includes, macros, and returning the techniques.
            ShaderInfo shaderInfo;
            try
            {
                shaderInfo = ShaderInfo.FromFile(options.SourceFile, options, 
                    new ContentPipelineEffectCompilerOutput(context));

                // Add the include dependencies so that if they change
                // it will trigger a rebuild of this effect.
                foreach (var dep in shaderInfo.Dependencies)
                    context.AddDependency(dep);
            }
            catch (InvalidContentException)
            {
                throw;
            }
            catch (Exception ex)
            {
                // TODO: Extract good line numbers from mgfx parser!
                throw new InvalidContentException(ex.Message, input.Identity, ex);
            }

            // Create the effect object.
            EffectObject effect = null;
            var shaderErrorsAndWarnings = string.Empty;
            try
            {
                effect = EffectObject.CompileEffect(shaderInfo, out shaderErrorsAndWarnings);

                // If there were any additional output files we register
                // them so that the cleanup process can manage them.
                foreach (var outfile in shaderInfo.AdditionalOutputFiles)
                    context.AddOutputFile(outfile);
            }
            catch (ShaderCompilerException ex)
            {
                // This will log any warnings and errors and throw.
                ProcessErrorsAndWarnings(true, shaderErrorsAndWarnings, input, context);
            }

            // Process any warning messages that the shader compiler might have produced.
            ProcessErrorsAndWarnings(false, shaderErrorsAndWarnings, input, context);

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

#if WINDOWS
        private class ContentPipelineEffectCompilerOutput : IEffectCompilerOutput
        {
            private readonly ContentProcessorContext _context;

            public ContentPipelineEffectCompilerOutput(ContentProcessorContext context)
            {
                _context = context;
            }

            public void WriteWarning(string file, int line, int column, string message)
            {
                _context.Logger.LogWarning(null, CreateContentIdentity(file, line, column), message);
            }

            public void WriteError(string file, int line, int column, string message)
            {
                throw new InvalidContentException(message, CreateContentIdentity(file, line, column));
            }

            private static ContentIdentity CreateContentIdentity(string file, int line, int column)
            {
                return new ContentIdentity(file, null, line + "," + column);
            }
        }
#endif

        private static void ProcessErrorsAndWarnings(bool buildFailed, string shaderErrorsAndWarnings, EffectContent input, ContentProcessorContext context)
        {
            // Split the errors and warnings into individual lines.
            var errorsAndWarningArray = shaderErrorsAndWarnings.Split(new[] {"\n", "\r", Environment.NewLine},
                                                                      StringSplitOptions.RemoveEmptyEntries);

            var errorOrWarning = new Regex(@"(.*)\(([0-9,]*)\)\s*:\s*(.*)", RegexOptions.Compiled);
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
                        allErrorsAndWarnings = message + Environment.NewLine;
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
    }
}
