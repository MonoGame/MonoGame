using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.Extensibility;

namespace MonoGame.Tools.VisualStudio
{
    /// <summary>
    /// Implements a Visual Studio custom tool that builds MonoGame MGFX effect files from XNA FX files.
    /// </summary>
    [Guid("2AD954E2-F7EC-4E5E-A4B5-C6E0E826E31F")]
    [ComVisible(true)]
    [ProgId("MonoGame.MgfxGenerator")]
    public class MgfxGenerator : BaseCodeGeneratorWithSite
    {
#if DEBUG
        /// <summary>
        /// wrapper method for testing
        /// </summary>
        /// <param name="inputFileName"></param>
        /// <param name="inputFileContent"></param>
        /// <returns></returns>
        public byte[] GenerateCodeWrapper(string inputFileName, string inputFileContent)
        {
            if (!File.Exists(inputFileName) && !string.IsNullOrEmpty(inputFileContent))
            {
                var fn = Path.GetTempFileName();
                File.Delete(fn);

                using (var writer = new StreamWriter(fn))
                {
                    writer.Write(inputFileContent);
                }

                inputFileName = fn;
            }

            return GenerateCode(inputFileName, inputFileContent);

        }
#endif

        public override string GetDefaultExtension()
        {
            return ".mgfxo";
        }

        /// <summary>
        /// Generates an MGFX effect file from the input.
        /// </summary>
        /// <param name="inputFileName"></param>
        /// <param name="inputFileContent"></param>
        /// <returns></returns>
        protected override byte[] GenerateCode(string inputFileName, string inputFileContent)
        {
            var tempOutputFile = Path.GetTempFileName();

            try
            {

                if (File.Exists(tempOutputFile))
                    File.Delete(tempOutputFile);

                /// allocate a string builder to receive errors from 2MGFX
                var sb = new StringBuilder();

                using (var err = new StringWriter(sb))
                {
                    Console.SetError(err);

                    /// trying to keep the impact on the 2MGFX source minimal, without having to 
                    /// start a new process. Rather than adding a new method to compile to/from streams
                    /// this resorts to a couple of temp files which are cleaned up at the end.
                    var result = TwoMGFX.Program.Main(new[] { inputFileName, tempOutputFile });

                    if (result == 0)
                    {
                        if (File.Exists(tempOutputFile))
                            using (var reader = File.OpenRead(tempOutputFile))
                            {
                                var bytes = new byte[reader.Length];
                                reader.Read(bytes, 0, bytes.Length);

                                return bytes;
                            }
                        else
                        {
                            GeneratorErrorCallback(false, 0, "Could not load output file", 0, 0);
                            return null;
                        }
                    }
                    else
                    {
                        /// force Visual Studio to open the error list if we ran into problems.
                        /// TODO:
                        /// Get the error list panel to show without having to import a ton of shell interop code.
                        /*
                        ErrorList.BringToFront();
                        ErrorList.ForceShowErrors();
                         */


                        var errors = sb.ToString().Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

                        /// match errors in the format
                        /// filenamePath(line,col): summary: detail
                        const string pattern = @"(?<filename>[^\(]+)\((?<line>[^,]+)\,(?<col>[^\)]*)[^\:]*\:\s+((?<summary>[^\:]*)\:)?(?<detail>[^\n]*)";

                        foreach (var error in errors)
                        {
                            var matches = Regex.Matches(error, pattern);

                            if (matches.Count <= 0)
                            {
                                GeneratorErrorCallback(false, 0, error, 0, 0);
                            }

                            foreach (Match match in matches)
                            {
                                /// could do this in the GeneratorErrorCallback invocation, but this seems more clear.
                                var isWarning = !match.Groups["summary"].Value.ToLower().Contains("error");
                                var detail = string.Format(
                                    "{0} : {1}"
                                    , match.Groups["summary"]
                                    , match.Groups["detail"]
                                    );

                                var line = int.Parse(match.Groups["line"].Value);
                                var col = int.Parse(match.Groups["col"].Value);

                                GeneratorErrorCallback(
                                    isWarning
                                    , 0
                                    , detail
                                    , line
                                    , col
                                    );
                            }
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                base.GeneratorErrorCallback(false, 0, ex.Message, 0, 0);
            }
            finally
            {
                /// clean up the temp file
                if (File.Exists(tempOutputFile))
                    File.Delete(tempOutputFile);
            }

            return null;
        }



    }

}
