// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace TwoMGFX
{
    public class Options
    {
        [CommandLineParser.Required]
        public string SourceFile;

        [CommandLineParser.Required]
        public string OutputFile = string.Empty;

        [CommandLineParser.ProfileName]
        public ShaderProfile Profile = ShaderProfile.OpenGL;

        [CommandLineParser.Name("Debug", "\t\t - Include extra debug information in the compiled effect.")]
        public bool Debug;

        [CommandLineParser.Name("Defines", "\t - Semicolon-delimited define assignments")]
        public string Defines;
    }
}
