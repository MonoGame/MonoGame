﻿// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace MGCB
{
    class Program
    {

#if WINDOWS
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern bool SetDllDirectory(string lpPathName);
#endif

        static int Main(string[] args)
        {
#if WINDOWS
            // Set the correct directory for our dependency files.
            var is32Bit = IntPtr.Size == 4;
            var directory = string.Format("Dependencies{0}{1}", Path.DirectorySeparatorChar, is32Bit ? "x32" : "x64");
            SetDllDirectory(directory);
#endif

            var content = new BuildContent();

            // Parse the command line.
            var parser = new CommandLineParser(content)
            {
                Title = "MonoGame Content Builder\n" +
                        "Builds optimized game content for MonoGame projects."
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
            
            // Do we have anything to do?
            if (!content.HasWork)
            {
                parser.ShowUsage();
                return 0;
            }

            // Print a startup message.
            var buildStarted = DateTime.Now;
            Console.WriteLine("Build started {0}\n", buildStarted);

            // Let the content build.
            int successCount, errorCount;
            content.Build(out successCount, out errorCount);

            // Print the finishing info.
            Console.WriteLine("\nBuild {0} succeeded, {1} failed.\n", successCount, errorCount);
            Console.WriteLine("Time elapsed {0:hh\\:mm\\:ss\\.ff}.", DateTime.Now - buildStarted);

            // Return the error count.
            return errorCount;
        }
    }
}
