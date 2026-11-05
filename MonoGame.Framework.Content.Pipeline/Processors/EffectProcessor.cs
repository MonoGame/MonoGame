// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Text.RegularExpressions;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using MonoGame.Effect;

namespace Microsoft.Xna.Framework.Content.Pipeline.Processors;

/// <summary>
/// Processes a string representation to a platform-specific compiled effect.
/// </summary>
[ContentProcessor(DisplayName = "Effect - MonoGame")]
public class EffectProcessor : ContentProcessor<EffectContent, CompiledEffectContent>
{
    private static readonly Regex errorOrWarning = new(@"(.*)\((\d*,\d*(?>,\d*,\d*)?)\):\s*(.*)", RegexOptions.Compiled);

    /// <summary>
    /// The debug mode for compiling effects.
    /// </summary>
    /// <value>The debug mode to use when compiling effects.</value>
    public virtual EffectProcessorDebugMode DebugMode { get; set; } = EffectProcessorDebugMode.Auto;

    /// <summary>
    /// Define assignments for the effect.
    /// </summary>
    /// <value>A list of define assignments delimited by semicolons.</value>
    public virtual string Defines { get; set; } = "";

    /// <summary>
    /// Processes the string representation of the specified effect into a platform-specific binary format using the specified context.
    /// </summary>
    /// <param name="input">The effect string to be processed.</param>
    /// <param name="context">Context for the specified processor.</param>
    /// <returns>A platform-specific compiled binary effect.</returns>
    /// <remarks>If you get an error during processing, compilation stops immediately. The effect processor displays an error message. Once you fix the current error, it is possible you may get more errors on subsequent compilation attempts.</remarks>
    public override CompiledEffectContent Process(EffectContent input, ContentProcessorContext context)
    {
        if (input.Identity == null)
        {
            throw new InvalidContentException("Passed EffectContent input is invalid!");
        }

        var options = new Options
        {
            SourceFile = input.Identity.SourceFilename,
            Profile = ShaderProfile.GetProfileForPlatform(context.TargetPlatform),
            Debug = DebugMode == EffectProcessorDebugMode.Debug,
            Defines = Defines,
            OutputFile = context.OutputFilename
        };

        // Parse the MGFX file expanding includes, macros, and returning the techniques.
        ShaderResult shaderResult;
        try
        {
            shaderResult = ShaderResult.FromFile(options.SourceFile, options,
                new ContentPipelineEffectCompilerOutput(context));

            // Add the include dependencies so that if they change
            // it will trigger a rebuild of this effect.
            foreach (var dep in shaderResult.Dependencies)
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
        EffectObject? effect;
        var shaderErrorsAndWarnings = string.Empty;
        try
        {
            effect = EffectObject.CompileEffect(shaderResult, out shaderErrorsAndWarnings);

            // If there were any additional output files we register
            // them so that the cleanup process can manage them.
            foreach (var outfile in shaderResult.AdditionalOutputFiles)
                context.AddOutputFile(outfile);
        }
        catch (ShaderCompilerException)
        {
            // This will log any warnings and errors and throw.
            ProcessErrorsAndWarnings(true, shaderErrorsAndWarnings, input.Identity, context);
            throw;
        }

        // Process any warning messages that the shader compiler might have produced.
        ProcessErrorsAndWarnings(false, shaderErrorsAndWarnings, input.Identity, context);

        // Write out the effect to a runtime format.
        CompiledEffectContent result;
        try
        {
            using var stream = new MemoryStream();
            using var writer = new BinaryWriter(stream);
            effect.Write(writer, options);

            result = new CompiledEffectContent(stream.GetBuffer());
        }
        catch (Exception ex)
        {
            throw new InvalidContentException("Failed to serialize the effect!", input.Identity, ex);
        }

        return result;
    }

    private static void ProcessErrorsAndWarnings(bool buildFailed, string shaderErrorsAndWarnings, ContentIdentity inputIdentity, ContentProcessorContext context)
    {
        // Split the errors and warnings into individual lines.
        var errorsAndWarningArray = shaderErrorsAndWarnings.Split(["\n", "\r", Environment.NewLine], StringSplitOptions.RemoveEmptyEntries);
        ContentIdentity? identity = null;
        var allErrorsAndWarnings = new System.Text.StringBuilder();

        // Process all the lines.
        foreach (var errorOrWarningLine in errorsAndWarningArray)
        {
            var match = errorOrWarning.Match(errorOrWarningLine);
            if (!match.Success || match.Groups.Count != 4)
            {
                // Just log anything we don't recognize as a warning.
                if (buildFailed)
                    allErrorsAndWarnings.AppendLine(errorOrWarningLine);
                else
                    context.Logger.Log(LogLevel.Warning, $"{errorOrWarningLine}: {context.Logger.GetCurrentFilename(inputIdentity)}");

                continue;
            }

            var fileName = match.Groups[1].Value;
            var lineAndColumn = match.Groups[2].Value;
            var message = match.Groups[3].Value;

            // Try to ensure a good file name for the error message.
            if (string.IsNullOrEmpty(fileName))
            {
                fileName = inputIdentity.SourceFilename;
            }
            else if (!File.Exists(fileName))
            {
                var folder = Path.GetDirectoryName(inputIdentity.SourceFilename) ?? "";
                fileName = Path.Combine(folder, fileName);
            }

            var newIdentity = new ContentIdentity(fileName, inputIdentity.SourceTool, lineAndColumn);

            // If we got an exception then we'll be throwing an exception
            // below, so just gather the lines to throw later.
            if (buildFailed)
            {
                if (identity == null)
                {
                    identity = newIdentity;
                    allErrorsAndWarnings.AppendLine(message);
                }
                else
                    allErrorsAndWarnings.AppendLine(errorOrWarningLine);
            }
            else
                context.Logger.Log(LogLevel.Warning, $"{message}: {context.Logger.GetCurrentFilename(newIdentity)}");
        }

        if (buildFailed)
        {
            throw new InvalidContentException(allErrorsAndWarnings.ToString(), identity ?? inputIdentity);
        }
    }

    private class ContentPipelineEffectCompilerOutput(ContentProcessorContext context) : IEffectCompilerOutput
    {
        public void WriteWarning(string file, int line, int column, string message)
            => context.Logger.Log(LogLevel.Warning, $"{message}: {context.Logger.GetCurrentFilename(CreateContentIdentity(file, line, column))}");

        public void WriteError(string file, int line, int column, string message)
            => throw new InvalidContentException(message, CreateContentIdentity(file, line, column));

        private static ContentIdentity CreateContentIdentity(string file, int line, int column) => new(file, null, line + "," + column);
    }
}
