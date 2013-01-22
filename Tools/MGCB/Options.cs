// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace MGCB
{
    class Options
    {
        [Utilities.CommandLineParser.Required]
        public string ContentProjectFile;

        [Utilities.CommandLineParser.Required]
        public string OutputName;
    }
}
