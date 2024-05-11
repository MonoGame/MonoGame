// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;

namespace MonoGame.Effect.Compiler
{
    public static class Program
    {
        private static bool usingWine;

        private static readonly List<(string, string)> sourceFiles = new();

        private static readonly Regex lineColumnRegex = new(@"\((\d*)(,)?(\d*)?(-)?(\d*)?\)", RegexOptions.Compiled);

        public static int Main(string[] args)
        {
            if (!Environment.Is64BitProcess && Environment.OSVersion.Platform != PlatformID.Unix)
            {
                Console.Error.WriteLine("The MonoGame content tools only work on a 64bit OS.");
                return -1;
            }

            var options = new Options();
            var parser = new CommandLineParser(options);
            parser.Title = "mgfxc - The MonoGame Effect compiler.";

            if (!parser.ParseCommandLine(args))
                return 1;

            // We don't support running MGFXC on Unix platforms
            // however Wine can be used to make it work so lets try that.
            if (Environment.OSVersion.Platform == PlatformID.Unix)
            {
                Environment.SetEnvironmentVariable("MGFXC_USE_WINE", "1");
                return WineHelper.Run(options);
            }

            usingWine = Environment.GetEnvironmentVariable("MGFXC_USE_WINE") == "1";

            var sourceFilepath = Path.GetFullPath(options.SourceFile);

            var nativeSourceFilepath = ConvertToNative(sourceFilepath);

            sourceFiles.Add((sourceFilepath, nativeSourceFilepath));

            var sourceDirectory = Path.GetDirectoryName(sourceFilepath);
            sourceFiles.Add((sourceDirectory, ConvertToNative(sourceDirectory)));

            // Validate the input file exits.
            if (!File.Exists(sourceFilepath))
            {
                Console.Error.WriteLine("The input file '{0}' was not found!", nativeSourceFilepath);
                return 1;
            }

            // TODO: This would be where we would decide the user
            // is trying to convert an FX file to a MGFX glsl file.
            //
            // For now we assume we're going right to a compiled MGFXO file.

            // Parse the MGFX file expanding includes, macros, and returning the techniques.
            ShaderResult shaderResult;
            try
            {
                shaderResult = ShaderResult.FromFile(sourceFilepath, options, new ConsoleEffectCompilerOutput());

                foreach (var dependency in shaderResult.Dependencies)
                {
                    var dependencyNativeFilename = ConvertToNative(dependency);
                    Console.WriteLine("Dependency: " + dependencyNativeFilename);

                    sourceFiles.Add((dependency, dependencyNativeFilename));
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ConvertMessage(ex.Message));
                Console.Error.WriteLine("Failed to parse '{0}'!", nativeSourceFilepath);
                return 1;
            }

            // Create the effect object.
            EffectObject effect;
            var shaderErrorsAndWarnings = string.Empty;
            try
            {
                effect = EffectObject.CompileEffect(shaderResult, out shaderErrorsAndWarnings);

                if (!string.IsNullOrEmpty(shaderErrorsAndWarnings))
                    Console.Error.WriteLine(ConvertMessage(shaderErrorsAndWarnings));
            }
            catch (ShaderCompilerException)
            {
                // Write the compiler errors and warnings and let the user know what happened.
                Console.Error.WriteLine(ConvertMessage(shaderErrorsAndWarnings));
                Console.Error.WriteLine("Failed to compile '{0}'!", nativeSourceFilepath);
                return 1;
            }
            catch (Exception ex)
            {
                // First write all the compiler errors and warnings.
                if (!string.IsNullOrEmpty(shaderErrorsAndWarnings))
                    Console.Error.WriteLine(ConvertMessage(shaderErrorsAndWarnings));

                // If we have an exception message then write that.
                if (!string.IsNullOrEmpty(ex.Message))
                    Console.Error.WriteLine(ConvertMessage(ex.Message));

                // Let the user know what happened.
                Console.Error.WriteLine("Unexpected error compiling '{0}'!", nativeSourceFilepath);
                return 1;
            }

            // Get the output file path.
            if (options.OutputFile == string.Empty)
                options.OutputFile = Path.GetFileNameWithoutExtension(sourceFilepath) + ".mgfxo";

            var outputFilepath = Path.GetFullPath(options.OutputFile);
            var nativeOutputFilepath = ConvertToNative(outputFilepath);

            // Write out the effect to a runtime format.
            try
            {
                using (var stream = new FileStream(outputFilepath, FileMode.Create, FileAccess.Write))
                using (var writer = new BinaryWriter(stream))
                    effect.Write(writer, options);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ConvertMessage(ex.Message));
                Console.Error.WriteLine("Failed to write '{0}'!", nativeOutputFilepath);
                return 1;
            }

            // We finished succesfully.
            Console.WriteLine("Compiled '{0}' to '{1}'.", nativeSourceFilepath, nativeOutputFilepath);
            return 0;
        }

        private static string ConvertToNative(string path)
        {
            if (!usingWine)
                return path.Replace('\\', '/');

            var proc = new Process();
            proc.StartInfo.FileName = "winepath.exe";
            proc.StartInfo.Arguments = "-u \"" + path + "\"";
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.RedirectStandardOutput = true;
            proc.Start();

            return proc.StandardOutput.ReadToEnd().Trim('\n');
        }

        private static string ConvertMessage(string sourceString)
        {
            sourceString = sourceString.Replace("\\\\", "\\");

            foreach (var (originalFilename, newFilename) in sourceFiles)
                sourceString = sourceString.Replace(originalFilename, newFilename);

            sourceString = lineColumnRegex.Replace(sourceString, ReplaceRowAndColumn);

            return sourceString;
        }

        private static string ReplaceRowAndColumn(Match match)
        {
            var groups = match.Groups;
            if (groups.Count != 6)
                return match.Value;

            var result = $"({groups[1]}," + (groups[2].Success && groups[3].Value != "0" ? groups[3] : "1");
            if (groups[4].Success)
                result += $",{groups[1]},{int.Parse(groups[5].ValueSpan) + 1}";
            result += ")";

            return result;
        }

        private class ConsoleEffectCompilerOutput : IEffectCompilerOutput
        {
            public void WriteWarning(string file, int line, int column, string message)
            {
                Console.WriteLine("{0}({1},{2}): warning PREPROCESS01: {3}", ConvertToNative(file), line, column, ConvertMessage(message));
            }

            public void WriteError(string file, int line, int column, string message)
            {
                throw new Exception(string.Format("{0}({1},{2}): error PREPROCESS01: {3}", ConvertToNative(file), line, column, ConvertMessage(message)));
            }
        }
    }
}
