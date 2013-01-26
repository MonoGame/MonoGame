// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;

namespace MGCB
{
    class Program
    {
        static int Main(string[] args)
        {
            var content = new BuildContent();
            var parser = new CommandLineParser(content);
            parser.Title = "MonoGame Content Builder";

            // Make sure we got a valid command line.
            if (!parser.ParseCommandLine(args))
                return -1;

            // Print a startup message.
            var buildStarted = DateTime.Now;
            Console.WriteLine("Build started {0}\n", buildStarted);

            // Let the content build.
            int fileCount, errorCount;
            content.Build(out fileCount, out errorCount);

            // Print the finishing info.
            Console.WriteLine("\nBuild {0} succeeded, {1} failed.", fileCount, errorCount);
            Console.WriteLine("Time elapsed {0:hh\\:mm\\:ss\\.ff}.", DateTime.Now - buildStarted);

            // Return the error count.
            return errorCount;
        }
    }
}
