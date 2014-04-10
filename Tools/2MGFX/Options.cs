namespace TwoMGFX
{
    public class Options
    {
        [Utilities.CommandLineParser.Required]
        public string SourceFile;

        [Utilities.CommandLineParser.Required]
        public string OutputFile = string.Empty;

        [Utilities.CommandLineParser.Name("Profile", "\t - Must be either DirectX_11, OpenGL, or PlayStation4")]
        public ShaderProfile Profile = ShaderProfile.OpenGL;

        [Utilities.CommandLineParser.Name("DEBUG")]
        public bool Debug;
    }
}
