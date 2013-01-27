// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;
using System.Linq;

namespace MGCB
{
    class Program
    {
        static int Main(string[] args)
        {
            var content = new BuildContent();

            // Parse the command line.
            var parser = new CommandLineParser(content)
            {
                Title = "MonoGame Content Builder"
            };
            if (!parser.ParseCommandLine(args))
                return -1;

            // Process all resoponse files.
            foreach (var r in content.ResponseFiles)
            {
                // Read in all the lines, trim whitespace, and remove empty or comment lines.
                var commands = File.ReadAllLines(r).
                                Select(x => x.Trim()).
                                Where(x => !string.IsNullOrEmpty(x) && !x.StartsWith("#")).
                                ToArray();

                // Parse the commands like they came from the command line.
                if (!parser.ParseCommandLine(commands))
                    return -1;
            }

            // Create the output directory.
            if (string.IsNullOrEmpty(content.OutputDir))
            {
                parser.ShowError("/outputDir is required.");
                return -1;
            }
            if (string.IsNullOrEmpty(content.IntermediateDir))
            {
                parser.ShowError("/intermediateDir is required.");
                return -1;
            }

            // Print a startup message.
            var buildStarted = DateTime.Now;
            Console.WriteLine("Build started {0}\n", buildStarted);

            // Let the content build.
            int fileCount, errorCount;
            content.Build(out fileCount, out errorCount);

            // Print the finishing info.
            Console.WriteLine("\nBuild {0} succeeded, {1} failed.\n", fileCount, errorCount);
            Console.WriteLine("Time elapsed {0:hh\\:mm\\:ss\\.ff}.", DateTime.Now - buildStarted);

            // Return the error count.
            return errorCount;
        }
    }
}
