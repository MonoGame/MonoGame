// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GLSLOptimizerSharp;
using Microsoft.Xna.Framework.Graphics;

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
            // TODO GLES?
        }

        internal override void ValidateShaderModels(PassInfo pass)
        {
            // TODO: implement
        }

        internal override void BeforeCreation(ShaderResult shaderResult)
        {
            var shaderInfo = shaderResult.ShaderInfo;
            var content = shaderResult.FileContent;
            ParseTreeTools.WhitespaceNodes(TokenType.Semantic, shaderInfo.ParseTree.Nodes, ref content);
            // we should have pure GLSL now so we can pass it to the optimizer
            // TODO we should also clear the dict with cached shaders here
        }

        internal override ShaderData CreateShader(ShaderResult shaderResult, string shaderFunction, string shaderProfile, bool isVertexShader,
            EffectObject effect, ref string errorsAndWarnings)
        {
            // TODO check if we already created this shader in the dict
            var shaderInfo = shaderResult.ShaderInfo;
            var shaderType = isVertexShader ? ShaderType.Vertex : ShaderType.Fragment;

            // TODO: depending on platform and version this can be GLES 2.0 or 3.0
            var optimizer = new GLSLOptimizer(Target.OpenGL);

            // first replace the function to optimize with 'main'
            ParseNode funcNode;
            if (!shaderInfo.Functions.TryGetValue(shaderFunction, out funcNode))
                throw new Exception(string.Format("Function specified in technique not defined: {0}", shaderFunction));

            // the first node is 'void', the second is the function  name
            var nameNode = funcNode.Nodes[1];
            var token = nameNode.Token;

            // make sure we don't have any illegal syntax
            var text = string.Copy(shaderInfo.FileContent);
            // TODO this is really, really inefficient...
            try
            {
                WhitespaceFunctions(ref text,
                    shaderInfo.Functions.Where(kvp => kvp.Key != shaderFunction).Select(kvp => kvp.Value));
            }
            catch (ShaderCompilerException e)
            {
                errorsAndWarnings += e.Message + '\n';
            }
            // non vertex shaders can't have any "attribute" input variables
            if (shaderStage != ShaderStage.Vertex)
                WhitespaceNodes(ref text, shaderInfo.VsInputVariables.Where(v => v.Value.AttributeSyntax).Select(v => v.Value.Node));

            // note that this modifies the position of characters so any modification based
            // on the parsenodes after this will be incorrect
            var builder = new StringBuilder(text);
            builder.Remove(token.StartPos, token.Length);
            builder.Insert(token.StartPos, "main");

            
            // TODO prepend the version to the content
            //builder.Insert(0, shaderProfile);

            // then optimize it
            var input = builder.ToString();
            const OptimizationOptions options = OptimizationOptions.SkipPreprocessor;
            var result = optimizer.Optimize(shaderStage.ToShaderType(), input, options);

            optimizer.Dispose();

            // Add the required precision specifiers for GLES after the optimization
            var floatPrecision = isVertexShader ? "precision highp float;\r\n" : "precision mediump float;\r\n";
			var shaderByteCode = "#ifdef GL_ES\r\n" +
                 floatPrecision +
				"precision mediump int;\r\n" +
				"#endif\r\n" +
				result.OutputCode;

            // TODO add result to dictionary hashed on shader name and version
            var bytes = Encoding.ASCII.GetBytes(shaderByteCode);
            var data = new ShaderData(isVertexShader, effect.Shaders.Count, bytes);
            data.ShaderCode = bytes;

            data._attributes = new ShaderData.Attribute[result.Inputs.Count];

            for(var i = 0; i < result.Inputs.Count; i++)
            {
                // var usage = shaderInfo.VsInputVariables[result.Inputs[i].Name].SemanticName;
                data._attributes[i] = new ShaderData.Attribute
                {
                    index = i,
                    location = -1,
                    name = result.Inputs[i].Name,
                    //usage =
                };
            }

            data._samplers = new ShaderData.Sampler[result.Textures.Count];
            for(var i = 0; i < result.Textures.Count; i++)
            {
                var tex = result.Textures[i];
                var sampler = shaderInfo.SamplerStates[tex.Name];
                data._samplers[i] = new ShaderData.Sampler
                {
                    //sampler mapping to parameter is unknown atm
                    parameter = -1,
                    samplerName = tex.Name,
                    parameterName = sampler.Name,

                    textureSlot = i,
                    samplerSlot = i,
                    type = tex.Type.ToSamplerType()
                };
            }
            // TODO somehow optimize parameters into 'constant buffers'
            // older GLSL does not have uniform buffers so we'd have to something else. Maybe the
            // optimization MojoShader does -> gather all floats, bools and ints in large arrays
            data._cbuffers = new int[1];

            effect.Shaders.Add(data);

            return data;
        }

        // replace functions corresponding to the given nodes with whitespace
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

                // only the header is parsed and put inside the nodes, so we need
                // to find the end of the function by matching brackets
                var length = 0;
                var openedBrackets = 1;

                while (openedBrackets > 0)
                {
                    length++;
                    var pos = funcHeaderEnd + length;
                    if (pos >= text.Length)
                        throw new ShaderCompilerException("Unmatched bracket!");

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

        private static void WhitespaceNodes(ref string text, IEnumerable<ParseNode> nodes)
        {
            foreach (var node in nodes)
                ParseTreeTools.WhitespaceRange(ref text, node.Token.StartPos, node.Token.Length);
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

    public static partial class ConversionExtensions
    {
        internal static SamplerType ToSamplerType(this BasicType basicType)
        {
            switch (basicType)
            {
                case BasicType.Tex2D:
                    return SamplerType.Sampler2D;
                case BasicType.Tex3D:
                    return SamplerType.SamplerVolume;
                case BasicType.TexCube:
                    return SamplerType.SamplerCube;
                default:
                    throw new ArgumentOutOfRangeException(nameof(basicType), basicType, null);
            }
        }

        internal static ShaderType ToShaderType(this ShaderStage stage)
        {
            switch (stage)
            {
                case ShaderStage.Vertex:
                    return ShaderType.Vertex;
                case ShaderStage.Pixel:
                    return ShaderType.Fragment;
                default:
                    throw new ArgumentOutOfRangeException(nameof(stage), stage, null);
            }
        }
    }
}
