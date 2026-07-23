// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using MonoGame.Effect.TPGParser;

namespace MonoGame.Effect
{
    public class ShaderResult
    {
        public required ShaderInfo ShaderInfo { get; init; }

        public required string FilePath { get; init; }
        public string? RelativeFilePath { get; set; }

        public required string FileContent { get; init; }

        public string? OutputFilePath { get; private set; }

        public List<string> Dependencies { get; private set; } = [];

        public List<string> AdditionalOutputFiles { get; private set; } = [];

        public required ShaderProfile Profile { get; init; }

        public required bool Debug { get; init; }


        static public ShaderResult FromFile(string path, Options options, IEffectCompilerOutput output)
        {
            var effectSource = File.ReadAllText(path);
            return FromString(effectSource, path, options, output);
        }

        static public ShaderResult FromString(string effectSource, string filePath, Options options, IEffectCompilerOutput output)
        {
            var macros = new Dictionary<string, string>();
            macros.Add("MGFX", "1");

            options.Profile.AddMacros(macros);

            // If we're building shaders for debug set that flag too.
            if (options.Debug)
                macros.Add("DEBUG", "1");

            if (!string.IsNullOrEmpty(options.Defines))
            {
                var defines = options.Defines.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var define in defines)
                {
                    var name = define;
                    var value = "1";
                    if (define.Contains("="))
                    {
                        var parts = define.Split('=');

                        if (parts.Length > 0)
                            name = parts[0].Trim();

                        if (parts.Length > 1)
                            value = parts[1].Trim();
                    }

                    macros.Add(name, value);
                }
            }

            // Use the D3DCompiler to pre-process the file resolving 
            // all #includes and macros.... this even works for GLSL.
            string newFile;
            var fullPath = Path.GetFullPath(filePath);
            var dependencies = new List<string>();
            newFile = Preprocessor.Preprocess(effectSource, fullPath, macros, dependencies, output);

            // Parse the resulting file for techniques and passes.
            var tree = new Parser(new Scanner()).Parse(newFile, fullPath);
            if (tree.Errors.Count > 0)
            {
                var errors = string.Empty;
                foreach (var error in tree.Errors)
                    errors += $"{error.File},({error.Line},{error.Column}) : {error.Message}\r\n";

                throw new Exception(errors);
            }

            // Evaluate the results of the parse tree.
            var shaderInfo = (ShaderInfo)tree.Eval();

            // Remove the samplers and techniques so that the shader compiler
            // gets a clean file without any FX file syntax in it.
            var cleanFile = newFile;
            WhitespaceNodes(TokenType.Technique_Declaration, tree.Nodes, ref cleanFile);
            WhitespaceNodes(TokenType.Sampler_Declaration_States, tree.Nodes, ref cleanFile);

            // Setup the rest of the shader info.
            ShaderResult result = new()
            {
                ShaderInfo = shaderInfo,
                Dependencies = dependencies,
                FilePath = fullPath,
                FileContent = cleanFile,
                Profile = options.Profile,
                Debug = options.Debug
            };
            if (!string.IsNullOrEmpty(options.OutputFile))
                result.OutputFilePath = Path.GetFullPath(options.OutputFile);

            // Remove empty techniques.
            for (var i = 0; i < shaderInfo.Techniques.Count; i++)
            {
                var tech = shaderInfo.Techniques[i];
                if (tech.Passes.Count <= 0)
                {
                    shaderInfo.Techniques.RemoveAt(i);
                    i--;
                }
            }

            // We must have at least one technique.
            if (shaderInfo.Techniques.Count <= 0)
                throw new Exception("The effect must contain at least one technique and pass!");

            return result;
        }
                
        static void WhitespaceNodes(TokenType type, List<ParseNode> nodes, ref string sourceFile)
        {
            for (var i = 0; i < nodes.Count; i++)
            {
                var n = nodes[i];
                if (n.Token.Type != type)
                {
                    WhitespaceNodes(type, n.Nodes, ref sourceFile);
                    continue;
                }

                // Get the full content of this node.
                var start = n.Token.StartPos;
                var end = n.Token.EndPos;
                var length = end - n.Token.StartPos;
                var content = sourceFile.Substring(start, length);

                // Replace the content of this node with whitespace.
                for (var c = 0; c < length; c++)
                {
                    if (!char.IsWhiteSpace(content[c]))
                        content = content.Replace(content[c], ' ');
                }

                // Add the whitespace back to the source file.
                var newfile = sourceFile.Substring(0, start);
                newfile += content;
                newfile += sourceFile.Substring(end);
                sourceFile = newfile;
            }
        }
    }
}
