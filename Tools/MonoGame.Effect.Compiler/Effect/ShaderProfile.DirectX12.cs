// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Win32;
using Microsoft.Xna.Framework.Graphics;

namespace MonoGame.Effect
{
    class DirectX12ShaderProfile : ShaderProfile
    {
        public DirectX12ShaderProfile()
            : base("DirectX_12", 2)
        {
        }

        internal override void AddMacros(Dictionary<string, string> macros)
        {
            macros.Add("HLSL", "1");
            macros.Add("SM6", "1");
        }

        internal override void ValidateShaderModels(MonoGame.Effect.TPGParser.PassInfo pass)
        {
            if (!string.IsNullOrEmpty(pass.vsFunction))
            {
                if (pass.vsModel != "vs_6_0")
                    throw new Exception(String.Format("Invalid DirectX 12 vertex profile '{0}'! Requires vs_6_0.", pass.vsModel));
            }

            if (!string.IsNullOrEmpty(pass.psFunction))
            {
                if (pass.psModel != "ps_6_0")
                    throw new Exception(String.Format("Invalid DirectX 12 pixel profile '{0}'! Requires ps_6_0.", pass.psModel));
            }
        }

        private static readonly Regex CBufferParam = new Regex(@";[\W]+(?:column_major)?(?:row_major)?[\W]+(?<ParamType>[^\r\n\t\f\v} ]+)[\W]+(?<ParamName>[\S]+);[\W]+; Offset:[\W]+(?<ParamOffset>[\d]*)", RegexOptions.Compiled);
        private static readonly Regex CBufferStruct = new Regex(@";[\W]+(?<CBufferName>[^\r\n\t\f\v} ]+);[\W]+; Offset:[\W]+(?<ParamOffset>[\d]*) Size:[\W]+(?<Size>[\d]*)", RegexOptions.Compiled);
        private static readonly Regex ResourceSampler = new Regex(@";[\W]+(?<ResName>[\S]+)[\W]+sampler[\W]+(?<ResFormat>[\S]+)[\W]+(?<ResDim>[\S]+)[\W]+[\S]+[\W]+s(?<ResBind>[\d]*)[\W]+(?<ResCount>[\d]*)", RegexOptions.Compiled);
        private static readonly Regex ResourceTexture = new Regex(@";[\W]+(?<ResName>[\S]+)[\W]+texture[\W]+(?<ResFormat>[\S]+)[\W]+(?<ResDim>[\S]+)[\W]+[\S]+[\W]+t(?<ResBind>[\d]*)[\W]+(?<ResCount>[\d]*)", RegexOptions.Compiled);
        private static readonly Regex InputAttribute = new Regex(@"; (\w+)\s+(\d+)\s+([xyzw]+)\s+(\d+)\s+(\w+)\s+(\w+)\s+([xyzw]+)", RegexOptions.Multiline | RegexOptions.Compiled);

        internal override ShaderData CreateShader(ShaderResult shaderResult, string shaderFunction, string shaderProfile, bool isVertexShader, EffectObject effect, ref string errorsAndWarnings)
        {
            var inputFile = Path.GetTempFileName();
            var outputFile = Path.GetTempFileName();

            // The common tool arguments.
            var toolArgs = "/nologo ";
            toolArgs += "/T " + shaderProfile + " ";
            toolArgs += "/E " + shaderFunction + " ";
            toolArgs += shaderResult.Debug ? "/Od " : "/O3 ";
            toolArgs += "/Zpc "; // Pack matrices in column-major order
            toolArgs += "/force-rootsig-ver rootsig_1_0 ";
            toolArgs += "/rootsig-define _MG_ROOT_SIGNATURE";
            toolArgs += "/D _MG_ROOT_SIGNATURE=\"RootFlags(ALLOW_INPUT_ASSEMBLER_INPUT_LAYOUT | " +
                "DENY_DOMAIN_SHADER_ROOT_ACCESS | " +
                "DENY_GEOMETRY_SHADER_ROOT_ACCESS | " +
                "DENY_HULL_SHADER_ROOT_ACCESS)," +
                "CBV(b0, visibility = SHADER_VISIBILITY_VERTEX)," +
                "CBV(b0, visibility = SHADER_VISIBILITY_PIXEL)," +
                "DescriptorTable(SRV(t0, numDescriptors = unbounded), visibility = SHADER_VISIBILITY_VERTEX)," +
                "DescriptorTable(SRV(t0, numDescriptors = unbounded), visibility = SHADER_VISIBILITY_PIXEL)," +
                "DescriptorTable(Sampler(s0, numDescriptors = unbounded), visibility = SHADER_VISIBILITY_VERTEX)," +
                "DescriptorTable(Sampler(s0, numDescriptors = unbounded), visibility = SHADER_VISIBILITY_PIXEL)\" ";

            string reflectionData;
            byte[] bytecode;
            try
            {
                // Write the input file for the tool to read.
                File.WriteAllText(inputFile, shaderResult.FileContent);

                // Compile the shader once just to get reflection info.
                string stdout, stderr;
                var result = RunTool("dxc", toolArgs + "\"" + inputFile + "\"", out reflectionData, out stderr);
                errorsAndWarnings += stderr;
                if (result > 0)
                    throw new ShaderCompilerException();

                // Now really compile the shader.
                if (shaderResult.Debug)
                {
                    toolArgs += "/Zi ";
                    string outPdb = Path.GetFileNameWithoutExtension(shaderResult.FilePath);
                    outPdb += "_" + shaderFunction;
                    outPdb += isVertexShader ? "_vs" : "_ps";
                    outPdb += ".pdb";
                    Directory.CreateDirectory("ShaderSymbols");
                    toolArgs += "/Fd ShaderSymbols\\" + outPdb + " ";
                }
                else
                {
                    toolArgs += "/Qstrip_reflect ";
                    toolArgs += "/Qstrip_debug ";
                }

                toolArgs += "/Fo " + "\"" + outputFile + "\"" + " ";
                result = RunTool("dxc", toolArgs + "\"" + inputFile + "\"", out stdout, out stderr);
                errorsAndWarnings += stderr;
                if (result > 0)
                    throw new ShaderCompilerException();

                // Load up the compiled shader.
                bytecode = File.ReadAllBytes(outputFile);
            }
            finally
            {
                // Cleanup.
                DeleteFile(inputFile);
                DeleteFile(outputFile);
            }

            // First look to see if we already created this same shader.
            foreach (var shader in effect.Shaders)
            {
                if (bytecode.SequenceEqual(shader.Bytecode))
                    return shader;
            }

            // Create a new shader.
            var shaderData = new ShaderData(isVertexShader, effect.Shaders.Count, bytecode);
            shaderData.ShaderCode = shaderData.Bytecode;

            // Gather the input attributes.
            var attributes = new List<ShaderData.Attribute>();
            if (isVertexShader)
            {
                // First extract the input signature.
                int start = reflectionData.IndexOf("; Input signature:");
                int end = reflectionData.IndexOf("; Output signature:");
                var signature = reflectionData.Substring(start, end - start);

                // Now gather the inputs.
                var matches = InputAttribute.Matches(signature);
                foreach (Match match in matches)
                {
                    var a = new ShaderData.Attribute();

                    //var mask = match.Groups[3].Value;
                    //var register = int.Parse(match.Groups[4].Value);
                    //var format = match.Groups[6].Value;

                    // Get the element index.
                    a.index = int.Parse(match.Groups[2].Value);

                    // Get the element type.
                    var name = match.Groups[1].Value;
                    switch (name.ToUpper())
                    {
                        default:
                            a.usage = VertexElementUsage.TextureCoordinate;
                            break;
                        case "POSITION":
                            a.usage = VertexElementUsage.Position;
                            break;
                        case "NORMAL":
                            a.usage = VertexElementUsage.Normal;
                            break;
                        case "TANGENT":
                            a.usage = VertexElementUsage.Tangent;
                            break;
                        case "BINORMAL":
                            a.usage = VertexElementUsage.Binormal;
                            break;
                        case "COLOR":
                            a.usage = VertexElementUsage.Color;
                            break;
                        case "BLENDINDICES":
                            a.usage = VertexElementUsage.BlendIndices;
                            break;
                        case "BLENDWEIGHT":
                            a.usage = VertexElementUsage.BlendWeight;
                            break;
                        case "DEPTH":
                            a.usage = VertexElementUsage.Depth;
                            break;
                        case "FOG":
                            a.usage = VertexElementUsage.Fog;
                            break;
                        case "POINTSIZE":
                            a.usage = VertexElementUsage.PointSize;
                            break;
                        case "TESSELLATEFACTOR":
                            a.usage = VertexElementUsage.TessellateFactor;
                            break;
                    }

                    // TODO: These are unused at runtime under the
                    // new native backends, we will remove them soon.               
                    a.location = 0;
                    a.name = string.Empty;

                    attributes.Add(a);
                }
            }
            shaderData._attributes = attributes.ToArray();

            // Parse out all the cbuffers we can find.
            var cbuffers = new List<ConstantBufferData>();
            {
                var reader = new StringReader(reflectionData);
                ConstantBufferData current = null;
                for(;;)
                {
                    var line = reader.ReadLine();
                    if (line == null)
                        break;

                    // The first non-comment line means we're out of
                    // the header where the metadata is defined.
                    if (!line.StartsWith(";"))
                        break;

                    if (line.StartsWith("; cbuffer "))
                    {
                        var name = line.Substring(10);
                        current = new ConstantBufferData(name);
                        continue;                        
                    }

                    // Nothing to do if we're not in a cbuffer block.
                    if (current == null)
                        continue;

                    var cbufmatch = CBufferStruct.Match(line);
                    if (cbufmatch.Success)
                    {
                        var cBufferSize = cbufmatch.Groups[3].Value;
                        current.SetSize(int.Parse(cBufferSize));
                        cbuffers.Add(current);
                        current = null;
                        continue;
                    }

                    var match = CBufferParam.Match(line);
                    if (match.Success)
                    {
                        var paramType = match.Groups[1].Value;
                        var paramName = match.Groups[2].Value;
                        var paramOffset = match.Groups[3].Value;
                        current.AddParameter(paramName, paramType, int.Parse(paramOffset));
                        continue;
                    }
                }

                reader.Dispose();
            }

            // Look for existing matching constant buffers.
            var cbufferIndex = new List<int>();
            foreach (var cbuffer in cbuffers)
            {
                if (cbuffer.Size == 0)
                    continue;

                var match = effect.ConstantBuffers.FindIndex(e => e.SameAs(cbuffer));
                if (match == -1)
                {
                    cbufferIndex.Add(effect.ConstantBuffers.Count);
                    effect.ConstantBuffers.Add(cbuffer);
                }
                else
                    cbufferIndex.Add(match);
            }
            shaderData._cbuffers = cbufferIndex.ToArray();

            // Go thru the sampler resources gathering their name and slot index.
            var samplerDescriptions = new List<ShaderData.Sampler>();
            {
                var reader = new StringReader(reflectionData);
                for (;;)
                {
                    var line = reader.ReadLine();
                    if (line == null)
                        break;

                    // The first non-comment line means we're out of
                    // the header where the metadata is defined.
                    if (!line.StartsWith(";"))
                        break;

                    var match = ResourceSampler.Match(line);
                    if (match.Success)
                    {
                        var samplerName = match.Groups[1].Value;
                        var samplerSlot = match.Groups[4].Value;

                        var samplerDesc = new ShaderData.Sampler()
                        {
                            samplerName = samplerName,
                            samplerSlot = int.Parse(samplerSlot),
                            textureSlot = -1,
                            parameterName = String.Empty
                        };

                        samplerDescriptions.Add(samplerDesc);
                    }
                }
            }

            // Go thru the texture resources creating final samplers.
            var samplers = new List<ShaderData.Sampler>();
            {
                var reader = new StringReader(reflectionData);
                for (;;)
                {
                    var line = reader.ReadLine();
                    if (line == null)
                        break;

                    // The first non-comment line means we're out of
                    // the header where the metadata is defined.
                    if (!line.StartsWith(";"))
                        break;

                    var match = ResourceTexture.Match(line);
                    if (match.Success)
                    {
                        var textureName = match.Groups[1].Value;
                        var textureSlot = int.Parse(match.Groups[4].Value);
                        var textureDim = match.Groups[3].Value;

                        var sampler = new ShaderData.Sampler();

                        // Find the sampler state for the texture... we try multiple 
                        // ways here because things are sort of broken.
                        //
                        // TODO: We need to refactor MGFX to seperate 
                        // samplers and textures... they do not belong toether!
                        //

                        // First see if we can match the sampler via texture name.
                        bool found = false;
                        foreach (var s in shaderResult.ShaderInfo.SamplerStates.Values)
                        {
                            if (s.TextureName == null)
                                continue;

                            if (s.TextureName != textureName)
                                continue;

                            sampler = samplerDescriptions.First(sd => sd.samplerName == s.Name);
                            found = true;
                        }
 
                        if (!found)
                        {
                            // Try to match assuming samplers and textures have the same register index.
                            // This can be wrong in some cases, but best we can do right now.
                            sampler = samplerDescriptions.First(sd => sd.samplerSlot == textureSlot);
                        }

                        sampler.state = shaderResult.ShaderInfo.SamplerStates[sampler.samplerName].State;
                        sampler.textureSlot = textureSlot;
                        sampler.parameterName = textureName;

                        switch (textureDim)
                        {
                            case "2d":
                                sampler.type = MojoShader.MOJOSHADER_samplerType.MOJOSHADER_SAMPLER_2D;
                                break;

                            case "cube":
                                sampler.type = MojoShader.MOJOSHADER_samplerType.MOJOSHADER_SAMPLER_CUBE;
                                break;

                            default:
                                throw new Exception("Unexpected sampler class type: " + textureDim);
                        }

                        samplers.Add(sampler);
                    }
                }
            }
            shaderData._samplers = samplers.ToArray();

            effect.Shaders.Add(shaderData);
            return shaderData;
        }
    }
}
