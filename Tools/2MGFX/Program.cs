﻿using System;
using System.IO;

namespace TwoMGFX
{
    public static class Program
    {
        public static int Main(string[] args)
        {
            var options = new Options();
            var parser = new Utilities.CommandLineParser(options);
            parser.Title = "2MGFX - Converts Microsoft FX files to a compiled MonoGame Effect.";

            if (!parser.ParseCommandLine(args))
                return 1;

            // Validate the input file exits.
            if (!File.Exists(options.SourceFile))
            {
                Console.Error.WriteLine("The input file '{0}' was not found!", options.SourceFile);
                return 1;
            }
            
            // TODO: This would be where we would decide the user
            // is trying to convert an FX file to a MGFX glsl file.
            //
            // For now we assume we're going right to a compiled MGFXO file.

            // Parse the MGFX file expanding includes, macros, and returning the techniques.
            ShaderInfo shaderInfo;
            try
            {
                shaderInfo = ShaderInfo.FromFile(options.SourceFile, options);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
                Console.Error.WriteLine("Failed to parse '{0}'!", options.SourceFile);
                return 1;
            }

            // Create the effect object.
            EffectObject effect;
            var shaderErrorsAndWarnings = string.Empty;
            try
            {
                effect = EffectObject.CompileEffect(shaderInfo, out shaderErrorsAndWarnings);

                if (!string.IsNullOrEmpty(shaderErrorsAndWarnings))
                    Console.Error.WriteLine(shaderErrorsAndWarnings);
            }
            catch (ShaderCompilerException)
            {
                // Write the compiler errors and warnings and let the user know what happened.
                Console.Error.WriteLine(shaderErrorsAndWarnings);
                Console.Error.WriteLine("Failed to compile '{0}'!", options.SourceFile);
                return 1;
            }
            catch (Exception ex)
            {
                // First write all the compiler errors and warnings.
                if (!string.IsNullOrEmpty(shaderErrorsAndWarnings))
                    Console.Error.WriteLine(shaderErrorsAndWarnings);

                // If we have an exception message then write that.
                if (!string.IsNullOrEmpty(ex.Message))
                    Console.Error.WriteLine(ex.Message);

                // Let the user know what happened.
                Console.Error.WriteLine("Unexpected error compiling '{0}'!", options.SourceFile);
                return 1;
            }
            
            // Get the output file path.
            if (options.OutputFile == string.Empty)
                options.OutputFile = Path.GetFileNameWithoutExtension(options.SourceFile) + ".mgfxo";

            // Write out the effect to a runtime format.
            try
            {
                using (var stream = new FileStream(options.OutputFile, FileMode.Create, FileAccess.Write))
                using (var writer = new BinaryWriter(stream))
                    effect.Write(writer, options);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
                Console.Error.WriteLine("Failed to write '{0}'!", options.OutputFile);
                return 1;
            }

            // We finished succesfully.
            Console.WriteLine("Compiled '{0}' to '{1}'.", options.SourceFile, options.OutputFile);
            return 0;
        }
    }
}
