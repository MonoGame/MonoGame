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
            parser.Title = "MGCB - The MonoGame content builder command-line tool.";

            // Make sure we got a valid command line.
            if (!parser.ParseCommandLine(args))
                return 1;

            // Let the content build.
            content.Build();

            return 0;
        }
    }
}
