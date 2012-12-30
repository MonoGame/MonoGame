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
            var options = new Options();
            var parser = new Utilities.CommandLineParser(options);
            parser.Title = "MGCB - The MonoGame content builder command-line tool.";

            // Make sure we got a valid command line.
            if (!parser.ParseCommandLine(args))
                return 1;

            // TODO: Eventually we will support our own simple file format
            // in addition to MSBuild .contentproj files.

            // Validate the content project exits.
            if (!File.Exists(options.ContentProjectFile))
            {
                Console.Error.WriteLine("The input file '{0}' was not found!", options.ContentProjectFile);
                return 1;
            }

            var project = new ContentProject(options.ContentProjectFile, options.OutputName);
            project.Build();

            return 0;
        }
    }
}
