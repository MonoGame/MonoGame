namespace TwoMGFX
{
    public class Options
    {
        [Utilities.CommandLineParser.Required]
        public string SourceFile;

        [Utilities.CommandLineParser.Required]
        public string OutputFile = string.Empty;

        [Utilities.CommandLineParser.Name("DX11")]
        public bool DX11Profile;

        [Utilities.CommandLineParser.Name("DEBUG")]
        public bool Debug;
    }
}
