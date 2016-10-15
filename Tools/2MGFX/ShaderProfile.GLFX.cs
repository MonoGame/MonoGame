// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GLSLOptimizerSharp;

namespace TwoMGFX
{
    public class GlfxShaderProfile : ShaderProfile
    {
        public GlfxShaderProfile() : base("GLFX", 2)
        {
        }

        internal override void AddMacros(Dictionary<string, string> macros)
        {
            macros.Add("GLSL", "1");
            macros.Add("OPENGL", "1");                
            macros.Add("GLFX", "1");                
        }

        internal override void ValidateShaderModels(PassInfo pass)
        {
            // TODO: implement
        }

        internal override ShaderData CreateShader(ShaderResult shaderResult, string shaderFunction, string shaderProfile, bool isVertexShader,
            EffectObject effect, ref string errorsAndWarnings)
        {
            var shaderInfo = shaderResult.ShaderInfo;
            var content = shaderResult.FileContent;
            ParseTreeTools.WhitespaceNodes(TokenType.Semantic, shaderInfo.ParseTree.Nodes, ref content);

            // we should have pure GLSL now so we can pass it to the optimizer
            // TODO: depending on platform and version this can be GLES 2.0 or 3.0
            var optimizer = new GLSLOptimizer(Target.OpenGL);

            var vsFuncs = new Dictionary<string, OptimizationResult>();
            var psFuncs = new Dictionary<string, OptimizationResult>();

            // we need to pass all functions used in techniques to the optimizer seperately
            foreach (var technique in shaderInfo.Techniques)
            {
                foreach (var pass in technique.Passes)
                {
                    ValidateShaderModels(pass);

                    var vsName = pass.vsFunction;
                    var vsModel = pass.vsModel;
                    if (!string.IsNullOrEmpty(vsName) && !vsFuncs.ContainsKey(vsName))
                    {
                        var result = OptimizeFunction(optimizer, ShaderType.Vertex, vsName, vsModel, shaderInfo);
                        vsFuncs.Add(vsName, result);
                    }

                    var psName = pass.psFunction;
                    var psModel = pass.psModel;
                    if (psFuncs.ContainsKey(psName)) continue;
                    {
                        var result = OptimizeFunction(optimizer, ShaderType.Fragment, psName, psModel, shaderInfo);
                        psFuncs.Add(psName, result);
                    }
                }
            }

            optimizer.Dispose();

            errorsAndWarnings = string.Empty;
            return null;
        }

        private static OptimizationResult OptimizeFunction(GLSLOptimizer optimizer, ShaderType type, 
            string functionName, string version, ShaderInfo shaderInfo)
        {
            // first replace the function to optimize with 'main'
            ParseNode funcNode;
            if (!shaderInfo.Functions.TryGetValue(functionName, out funcNode))
                throw new Exception(string.Format("Function specified in technique not defined: {0}", functionName));

            // the first node is 'void', the second is the function  name
            var nameNode = funcNode.Nodes[1];
            var token = nameNode.Token;

            // make sure we don't have any illegal syntax
            var text = string.Copy(shaderInfo.FileContent);
            // TODO this is really, really inefficient...
            WhitespaceFunctions(ref text,
                shaderInfo.Functions.Where(kvp => kvp.Key != functionName).Select(kvp => kvp.Value));

            // note that this modifies the position of characters so any modification based
            // on the parsenodes after this will be incorrect
            var builder = new StringBuilder(text);
            builder.Remove(token.StartPos, token.Length);
            builder.Insert(token.StartPos, "main");
            
            // TODO prepend the version to the content and add precision qualifiers

            // then optimize it
            var input = builder.ToString();
            const OptimizationOptions options = OptimizationOptions.SkipPreprocessor;
            return optimizer.Optimize(type, input, options);
        }

        private static void WhitespaceFunctions(ref string text, IEnumerable<ParseNode> nodes)
        {
            foreach (var node in nodes)
            {
                // end position of a function node is the opening bracket, so we find that first
                int funcHeaderEnd = node.Token.EndPos;
                while (text[funcHeaderEnd] != '{')
                {
                    if (funcHeaderEnd >= text.Length)
                        throw new Exception("Function header without body!");
                    funcHeaderEnd++;
                } 

                var length = 0;
                var openedBrackets = 1;

                while (openedBrackets > 0)
                {
                    length++;
                    var pos = funcHeaderEnd + length;
                    if (pos >= text.Length)
                        throw new Exception("Unmatched bracket!");

                    var c = text[pos];
                    if (c == '{')
                        openedBrackets++;
                    else if (c == '}')
                        openedBrackets--;
                }
                length++;
                var funcHeaderStart = node.Token.StartPos;
                var funcEnd = node.Token.EndPos + length;
                ParseTreeTools.WhitespaceRange(ref text, funcHeaderStart, funcEnd - funcHeaderStart + 1);
            }
        }

        internal override bool Supports(string platform)
        {
            if (platform == "iOS" ||
                platform == "Android" ||
                platform == "DesktopGL" ||
                platform == "MacOSX" ||
                platform == "RaspberryPi")
                return true;

            return false;
        }
    }
}
