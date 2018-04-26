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
            var shaderStage = isVertexShader ? ShaderStage.Vertex : ShaderStage.Pixel;

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
            var text = string.Copy(shaderResult.FileContent);
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
                var name = result.Inputs[i].Name;
                data._attributes[i] = new ShaderData.Attribute
                {
                    location = -1,
                    name = name,
                };

                // if this is a vertex shader we can set the semantics because we parsed them earlier
                if (isVertexShader)
                {
                    if (!shaderInfo.VsInputVariables.ContainsKey(name))
                        throw new ShaderCompilerException(string.Format("Did not find usage for VS input variable {0}", name));

                    var semantic = shaderInfo.VsInputVariables[name].SemanticName;
                    VertexElementUsage usage;
                    int index;

                    ParseSemantic(semantic, out usage, out index);

                    data._attributes[i].usage = usage;
                    data._attributes[i].index = index;
                }
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

            // EDIT: this is a difficult optimization to make and glsl_optimizer is no longer being
            // updated. For now we'll create a seperate cbuffer for each parameter. We should look
            // into supporting uniform buffers (UBOs) when a modern GLSL version is being used.
            // Users can do this optimization themselves by putting their uniforms in an array.
            // Maybe we should put that in the docs.

            data._cbuffers = new int[result.Uniforms.Count];

            for (var i = 0; i < result.Uniforms.Count; i++)
            {
                var uniform = result.Uniforms[i];
                var cb = new ConstantBufferData(uniform.Name);
                cb.Parameters.Add(uniform.ToParameter());
                cb.Size = uniform.GetSize();
                cb.ParameterOffset.Add(0);

                data._cbuffers[i] = effect.ConstantBuffers.Count;
                effect.ConstantBuffers.Add(cb);
            }

            effect.Shaders.Add(data);

            return data;
        }

        private static void ParseSemantic(string semantic, out VertexElementUsage usage, out int index)
        {
            // semantic name can either be SEMANTIC or SEMANTICn where n is a number between 0 and 9
            if (int.TryParse(semantic[semantic.Length - 1].ToString(), out index))
                semantic = semantic.Substring(0, semantic.Length - 1);

            // most enum names map to their semantics so they can be parsed directly
            if (Enum.TryParse(semantic, true, out usage))
                return;

            // the following do not map to their semantics
            switch (semantic.ToLower())
            {
                case "texcoord":
                    usage = VertexElementUsage.TextureCoordinate;
                    break;
                case "psize":
                    usage = VertexElementUsage.PointSize;
                    break;
                case "tessfactor":
                    usage = VertexElementUsage.TessellateFactor;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("usage", usage,
                        string.Format("Vertex input semantic {0} is not valid.", usage));
            }
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
                    throw new ArgumentOutOfRangeException("basicType", basicType, null);
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
                    throw new ArgumentOutOfRangeException("stage", stage, null);
            }
        }

        internal static EffectObject.d3dx_parameter ToParameter(this VariableInfo uniform)
        {
            var p = new EffectObject.d3dx_parameter();

            p.name = uniform.Name;
            p.columns = (uint) uniform.VectorSize;
            p.rows = (uint) uniform.MatrixSize;
            p.semantic = string.Empty;
            var size = uniform.GetSize();
            p.data = new byte[size];
            p.bytes = (uint) size;
            p.annotation_handles = new EffectObject.d3dx_parameter[0];

            var arraySize = (uint) (uniform.ArraySize == -1 ? 0 : uniform.ArraySize);
            p.element_count = arraySize;
            p.member_handles = new EffectObject.d3dx_parameter[arraySize];

            for (var i = 0; i < arraySize; i++)
            {
                var param = new EffectObject.d3dx_parameter();
                param.name = uniform.Name;
                param.columns = (uint) uniform.VectorSize;
                param.rows = (uint) uniform.MatrixSize;
                param.semantic = string.Empty;
                param.annotation_handles = new EffectObject.d3dx_parameter[0];
                param.element_count = 0;
                param.member_handles = new EffectObject.d3dx_parameter[0];
                param.data = new byte[4 * param.rows * param.columns];

                p.member_handles[i] = param;
            }

            if (p.rows > 1)
                p.class_ = EffectObject.D3DXPARAMETER_CLASS.MATRIX_ROWS;
            else if (p.columns > 1)
                p.class_ = EffectObject.D3DXPARAMETER_CLASS.VECTOR;
            else
                p.class_ = EffectObject.D3DXPARAMETER_CLASS.SCALAR;

            switch (uniform.Type)
            {
                case BasicType.Bool:
                    p.type = EffectObject.D3DXPARAMETER_TYPE.BOOL;
                    break;
                case BasicType.Int:
                    p.type = EffectObject.D3DXPARAMETER_TYPE.INT;
                    break;
                case BasicType.Float:
                    p.type = EffectObject.D3DXPARAMETER_TYPE.FLOAT;
                    break;
            }
            return p;
        }

        internal static int GetSize(this VariableInfo uniform)
        {
            var size = uniform.ArraySize == -1 ? 1 : uniform.ArraySize;
            size *= uniform.MatrixSize;
            size *= uniform.VectorSize;
            return size * 4;
        }
    }
}
