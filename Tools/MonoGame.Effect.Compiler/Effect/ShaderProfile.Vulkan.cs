// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using MonoGame.Effect.TPGParser;
using Microsoft.Xna.Framework.Graphics;
using System.Diagnostics;
using System.Threading;

namespace MonoGame.Effect
{
    class VulkanShaderProfile : ShaderProfile
    {
        public VulkanShaderProfile()
            : base("Vulkan", 80)
        {
        }

        internal override void AddMacros(Dictionary<string, string> macros)
        {
            macros.Add("SM6", "1");
            macros.Add("VULKAN", "1");
        }

        internal override void ValidateShaderModels(PassInfo pass)
        {
            if (!string.IsNullOrEmpty(pass.vsFunction))
            {
                if (pass.vsModel != "vs_6_0")
                    throw new Exception(String.Format("Invalid Vulkan vertex profile '{0}'! Requires vs_6_0.", pass.vsModel));
            }

            if (!string.IsNullOrEmpty(pass.psFunction))
            {
                if (pass.psModel != "ps_6_0")
                    throw new Exception(String.Format("Invalid Vulkan pixel profile '{0}'! Requires ps_6_0.", pass.psModel));
            }
        }

        class VkStructMember
        {
            public string name;
            public int offset;
            public string type;
        };

        class VkStruct
        {
            public string name;
            public readonly Dictionary<int, VkStructMember> members = new Dictionary<int, VkStructMember>();
        };

        enum VkDescriptorType : uint
        {
            SAMPLER = 0,
            COMBINED_IMAGE_SAMPLER = 1,
            SAMPLED_IMAGE = 2,
            STORAGE_IMAGE = 3,
            UNIFORM_TEXEL_BUFFER = 4,
            STORAGE_TEXEL_BUFFER = 5,
            UNIFORM_BUFFER = 6,
            STORAGE_BUFFER = 7,
            UNIFORM_BUFFER_DYNAMIC = 8,
            STORAGE_BUFFER_DYNAMIC = 9,
            INPUT_ATTACHMENT = 10,
            INLINE_UNIFORM_BLOCK = 1000138000,
            ACCELERATION_STRUCTURE_KHR = 1000150000,
            ACCELERATION_STRUCTURE_NV = 1000165000,
            SAMPLE_WEIGHT_IMAGE_QCOM = 1000440000,
            BLOCK_MATCH_IMAGE_QCOM = 1000440001,
            MUTABLE_EXT = 1000351000,
            INLINE_UNIFORM_BLOCK_EXT = INLINE_UNIFORM_BLOCK,
            MUTABLE_VALVE = MUTABLE_EXT,
            MAX_ENUM = 0x7FFFFFFF
        };

        [Flags]
        enum VkShaderStageFlags : uint
        {
            VERTEX_BIT = 0x00000001,
            TESSELLATION_CONTROL_BIT = 0x00000002,
            TESSELLATION_EVALUATION_BIT = 0x00000004,
            GEOMETRY_BIT = 0x00000008,
            FRAGMENT_BIT = 0x00000010,
            COMPUTE_BIT = 0x00000020,
            ALL_GRAPHICS = 0x0000001F,
            ALL = 0x7FFFFFFF,
            RAYGEN_BIT_KHR = 0x00000100,
            ANY_HIT_BIT_KHR = 0x00000200,
            CLOSEST_HIT_BIT_KHR = 0x00000400,
            MISS_BIT_KHR = 0x00000800,
            INTERSECTION_BIT_KHR = 0x00001000,
            CALLABLE_BIT_KHR = 0x00002000,
            TASK_BIT_EXT = 0x00000040,
            MESH_BIT_EXT = 0x00000080,
            SUBPASS_SHADING_BIT_HUAWEI = 0x00004000,
            CLUSTER_CULLING_BIT_HUAWEI = 0x00080000,
            RAYGEN_BIT_NV = RAYGEN_BIT_KHR,
            ANY_HIT_BIT_NV = ANY_HIT_BIT_KHR,
            CLOSEST_HIT_BIT_NV = CLOSEST_HIT_BIT_KHR,
            MISS_BIT_NV = MISS_BIT_KHR,
            INTERSECTION_BIT_NV = INTERSECTION_BIT_KHR,
            CALLABLE_BIT_NV = CALLABLE_BIT_KHR,
            TASK_BIT_NV = TASK_BIT_EXT,
            MESH_BIT_NV = MESH_BIT_EXT,
            FLAG_BITS_MAX_ENUM = 0x7FFFFFFF
        };

        class VkDescriptor
        {
            public string name;
            public VkDescriptorType type;
            public int set;
            public int binding;
        };

        class VkInput
        {
            public string name;
            public string type;
            public int location;
        };

        struct VkDescriptorSetLayoutBinding
        {
            public uint binding;
            public VkDescriptorType descriptorType;
            public uint descriptorCount;
            public VkShaderStageFlags stageFlags;
            public nint pImmutableSamplers;
        };
        
        internal override ShaderData CreateShader(ShaderResult shaderResult, string shaderFunction, string shaderProfile, bool isVertexShader, EffectObject effect, ref string errorsAndWarnings)
        {
            const int SlotOffset = 32;

            var outputPath = Path.GetDirectoryName(shaderResult.OutputFilePath);
            var sourceFileName = Path.GetFileNameWithoutExtension(shaderResult.FilePath) + "." + shaderFunction;

            // TODO: We have no intermediate folder in 2MGFX for temp stuff
            // that isn't content, but could be useful later.  So just putting
            // them into the output then cleaning it up after.
            var intermediateDir = outputPath;
            var hlslFile = Path.Combine(intermediateDir, sourceFileName + ".hlsl");
            var glslFile = Path.Combine(intermediateDir, sourceFileName + ".glsl");
            var binFile = Path.Combine(intermediateDir, sourceFileName + ".bin");
            var reflectFile = Path.Combine(intermediateDir, sourceFileName + ".reflect");

            // Need to keep this for debugging to work.
            var dbgFile = Path.Combine(outputPath, sourceFileName + ".dbg");

            // Disable this if you want to keep these around for testing!
            var cleanup = new List<string>();
            cleanup.Add(hlslFile);
            cleanup.Add(binFile);
            cleanup.Add(dbgFile);
            cleanup.Add(reflectFile);
            
            try
            {
                if (!Directory.Exists(intermediateDir))
                    Directory.CreateDirectory(intermediateDir);

                // Replace the entrypoint name with "main" for simplicity at runtime.
                var shaderContent = Regex.Replace(shaderResult.FileContent, @"(?<=\s+)" + shaderFunction + @"(?=\s*[(])", "main");

                // Write preprocessed hlsl file.
                File.WriteAllText(hlslFile, shaderContent);

                // Run HlslCrossCompiler.exe to convert temp.fx to a .glsl
                string stdout = string.Empty;
                string stderr = string.Empty;
                string toolArgs;
                int toolResult;

                toolArgs = "";
                toolArgs += "-nologo ";
                toolArgs += "-spirv ";
                toolArgs += "-fvk-use-dx-layout ";
                if (isVertexShader)
                {
                    toolArgs += "-fvk-invert-y ";
                    toolArgs += "-fvk-use-dx-position-w ";
                }
                else
                {
                    // Move pixel shaders into the second descriptor
                    // to avoid overlapping bindings between the vertex
                    // and pixel stages.
                    toolArgs += "-auto-binding-space 1 ";
                }

                // In SPIR-V the uniform and texture bindings cannot
                // overlap.  To solve this we shift them all forward by
                // a fixed amount here and in the shader layout creation.
                if (isVertexShader)
                {
                    toolArgs += $"-fvk-t-shift {SlotOffset} 0 ";
                    toolArgs += $"-fvk-s-shift {SlotOffset} 0 ";
                }
                else
                {
                    toolArgs += $"-fvk-t-shift {SlotOffset} 1 ";
                    toolArgs += $"-fvk-s-shift {SlotOffset} 1 ";
                }
                
                //toolArgs += "-Qstrip_reflect ";
                //toolArgs += "-fspv-reflect ";
                toolArgs += "-T " + (isVertexShader ? "vs_" : "ps_") + "6_0 ";
                toolArgs += "-E main ";
                toolArgs += "-Fc \"" + reflectFile + "\" ";                
                toolArgs += "-Fo \"" + binFile + "\" ";

                if (shaderResult.Debug)
                {
                    toolArgs += "-Zi ";
                    toolArgs += "-Fd \"" + dbgFile + "\" ";
                }
                toolArgs += "\"" + hlslFile + "\"";

                var processInfo = new ProcessStartInfo
                {
                    Arguments = toolArgs,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    ErrorDialog = false,
                    FileName = "dxc",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    RedirectStandardInput = true,
                };

                using (var process = new Process { StartInfo = processInfo })
                {
                    process.Start();

                    var stdoutThread = new Thread(new ThreadStart(() =>
                    {
                        var memory = new MemoryStream();
                        process.StandardOutput.BaseStream.CopyTo(memory);
                        var bytes = new byte[memory.Position];
                        memory.Seek(0, SeekOrigin.Begin);
                        memory.Read(bytes, 0, bytes.Length);
                        stdout = System.Text.Encoding.ASCII.GetString(bytes);
                    }));
                    stdoutThread.Start();

                    var stderrThread = new Thread(new ThreadStart(() =>
                    {
                        var memory = new MemoryStream();
                        process.StandardError.BaseStream.CopyTo(memory);
                        var bytes = new byte[memory.Position];
                        memory.Seek(0, SeekOrigin.Begin);
                        memory.Read(bytes, 0, bytes.Length);
                        stderr = System.Text.Encoding.ASCII.GetString(bytes);
                    }));
                    stderrThread.Start();

                    process.WaitForExit();

                    stdoutThread.Join();
                    stderrThread.Join();

                    toolResult = process.ExitCode;
                }

                errorsAndWarnings += stderr;

                // jcf: this tool doesn't seem to use stderr for output
                //      but if the return code was not success=0 then treat stdout as stderr
                if (toolResult != 0)
                {
                    errorsAndWarnings += string.Format("DXC.exe returned error code '{0}'.\n", toolResult);
                    errorsAndWarnings += stdout;
                    throw new ShaderCompilerException();
                }

                // Load up the compiled shader.
                var bytecode = File.ReadAllBytes(binFile);

                // First look to see if we already created this same shader.
                foreach (var shader in effect.Shaders)
                {
                    if (bytecode.SequenceEqual(shader.Bytecode))
                        return shader;
                }

                string reflectionData = File.ReadAllText(reflectFile);

                // Keep the debug file if we are creating a new shader
                // and debug shaders are enabled.
                if (shaderResult.Debug)
                {
                    if (File.Exists(dbgFile))
                        shaderResult.AdditionalOutputFiles.Add(dbgFile);

                    cleanup.Remove(dbgFile);
                }

                // Create a new shader.
                var shaderData = new ShaderData(isVertexShader, effect.Shaders.Count, bytecode);

                // Gather all the vulkan reflection info.
                var reader = new StringReader(reflectionData);
                var names = new Dictionary<string, string>();
                var structs = new Dictionary<string, VkStruct>();
                var descriptors = new Dictionary<string, VkDescriptor>();
                var inputs = new Dictionary<string, VkInput>();

                while (true)
                {
                    var line = reader.ReadLine();
                    if (line == null)
                        break;

                    var data = line.Trim().Split(' ');

                    if (data[0] == "OpName")
                    {
                        var name = data[2].Trim(new[] { '\"' });
                        var id = data[1];
                        if (id.StartsWith("%in_var_"))
                            inputs.Add(id, new VkInput { name = id.Substring(8) });
                        else
                            names.Add(id, name);
                    }
                    else if (data[0] == "OpMemberName")
                    {
                        var name = data[3].Trim(new[] { '\"' });
                        var id = data[1];
                        var index = int.Parse(data[2]);

                        VkStruct s;
                        if (!structs.TryGetValue(id, out s))
                        {
                            s = new VkStruct();
                            s.name = names[id];
                            structs.Add(id, s);
                        }

                        var member = new VkStructMember();
                        member.name = name;
                        s.members.Add(index, member);
                    }
                    else if (data[0] == "OpDecorate")
                    {
                        if (data[2] == "DescriptorSet")
                        {
                            VkDescriptor d;
                            if (!descriptors.TryGetValue(data[1], out d))
                            {
                                d = new VkDescriptor();
                                d.name = data[1];
                                d.type = VkDescriptorType.UNIFORM_BUFFER;
                                descriptors.Add(d.name, d);
                            }

                            d.set = int.Parse(data[3]);
                        }
                        else if (data[2] == "Binding")
                        {
                            VkDescriptor d;
                            if (!descriptors.TryGetValue(data[1], out d))
                            {
                                d = new VkDescriptor();
                                d.name = data[1];
                                d.type = VkDescriptorType.UNIFORM_BUFFER;
                                descriptors.Add(d.name, d);
                            }

                            d.binding = int.Parse(data[3]);
                        }
                        else if (data[1].StartsWith("%in_var_"))
                        {
                            inputs[data[1]].location = int.Parse(data[3]);
                        }
                    }
                    else if (data[0] == "OpMemberDecorate")
                    {
                        var id = data[1];
                        var index = int.Parse(data[2]);
                        var s = structs[id];

                        if (data[3] == "Offset")
                        {
                            var offset = int.Parse(data[4]);
                            s.members[index].offset = offset;
                        }
                    }
                    else if (data[0].StartsWith("%in_var_"))
                    {
                        inputs[data[0]].type = data[3].Substring(12);
                    }
                    else if (data[0].StartsWith("%"))
                    {
                        if (data[2] == "OpTypeStruct")
                        {
                            var id = data[0];
                            var s = structs[id];

                            for (int i=3; i < data.Length; i++)
                                s.members[i - 3].type = data[i];
                        }
                        else if (data[2] == "OpVariable")
                        {
                            VkDescriptor d;
                            if (descriptors.TryGetValue(data[0], out d))
                            {
                                if (data[3].EndsWith("_type_2d_image"))
                                    d.type = VkDescriptorType.SAMPLED_IMAGE;
                                else if (data[3].EndsWith("_type_sampler"))
                                    d.type = VkDescriptorType.SAMPLER;
                            }
                        }
                    }
                }

                var cbuffer = new ConstantBufferData("global");

                // TODO: Support multiple constant buffers one day!

                // First gather the uniforms.
                VkStruct globals;
                if (structs.TryGetValue("%type__Globals", out globals))
                {
                    foreach (var member in globals.members.Values)
                        cbuffer.AddParameter(member.name, member.type, 0, member.offset);
                }

                // Gather the input attributes.
                var attributes = new List<ShaderData.Attribute>();
                if (isVertexShader)
                {
                    // Sort by the location.
                    var sorted = inputs.Values.OrderBy(f=>f.location);

                    int offset = 0;

                    foreach (var input in sorted)
                    {
                        var a = new ShaderData.Attribute();

                        var m = Regex.Match(input.name, @"(\D+)(\d+)?");
                        if (m.Groups[2].Success)
                            a.index = int.Parse(m.Groups[2].Value);
                        else
                            a.index = 0;

                        if (m.Groups[1].Success)
                        {
                            switch (m.Groups[1].Value.ToUpper())
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
                        }

                        int size;
                        int len;
                        int inputTypeStringStartIndex;
                        if (input.type.StartsWith("v") && char.IsDigit(input.type[1]))
                        {
                            len = (int)char.GetNumericValue(input.type[1]);
                            inputTypeStringStartIndex = 2;
                        }
                        else
                        {
                            len = 1;
                            inputTypeStringStartIndex = 0;
                        }

                        switch (input.type.Substring(inputTypeStringStartIndex))
                        {
                            case "int":
                            case "uint":
                            case "float":
                                size = len * 4;
                                break;
                            default:
                                errorsAndWarnings += string.Format("Unknown vertex shader input type '{0}'.", input.type);
                                throw new ShaderCompilerException();
                        }

                        offset += size;

                        // TODO: These are unused at runtime under the
                        // new native backends, we will remove them soon.               
                        a.location = 0;
                        a.name = string.Empty;

                        attributes.Add(a);
                    }
                }

                // Now gather the samplers.
                var samplers = new List<ShaderData.Sampler>();
                foreach (var d in descriptors.Values)
                {
                    // TODO: This seems like it is maybe backwards
                    // and i should be looking for samplers for textures?

                    if (d.type != VkDescriptorType.SAMPLER)
                        continue;

                    var s = new ShaderData.Sampler();
                    s.samplerSlot = d.binding;
                    s.samplerName = names[d.name];

                    s.type = MojoShader.MOJOSHADER_samplerType.MOJOSHADER_SAMPLER_2D;
                    //if (data[0] == "SAMPLER3D")
                        //s.type = MojoShader.MOJOSHADER_samplerType.MOJOSHADER_SAMPLER_VOLUME;
                    //else if (data[0] == "SAMPLERCUBE")
                        //s.type = MojoShader.MOJOSHADER_samplerType.MOJOSHADER_SAMPLER_CUBE;

                    foreach (var td in descriptors.Values)
                    {
                        if (td.binding == s.samplerSlot)
                        {
                            s.textureSlot = td.binding - SlotOffset;
                            s.parameterName = names[td.name];
                            break;
                        }
                    }

                    s.samplerSlot -= SlotOffset;

                    // Associate sampler state to the sampler.
                    SamplerStateInfo state;
                    if (shaderResult.ShaderInfo.SamplerStates.TryGetValue(s.samplerName, out state))
                    {
                        s.parameterName = s.parameterName ?? state.TextureName;
                        s.state = state.State;
                        samplers.Add(s);
                        continue;
                    }

                    s.parameterName = s.parameterName ?? s.samplerName;
                    s.state = state.State;
                    samplers.Add(s);
                }

                shaderData._samplers = samplers.ToArray();

                // Look for existing matching constant buffers.
                var cbufferIndex = new List<int>();
                if (cbuffer.Size > 0)
                {
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

                shaderData._attributes = attributes.ToArray();

                // Generate the layout bindings from our cbuffers, samplers, and textures.
                {
                    using (var stream = new MemoryStream())
                    using (var writer = new BinaryWriter(stream))
                    {
                        var bindings = new List<VkDescriptorSetLayoutBinding>();

                        VkDescriptorSetLayoutBinding binding;
                        binding.stageFlags = isVertexShader ? VkShaderStageFlags.VERTEX_BIT : VkShaderStageFlags.FRAGMENT_BIT;
                        binding.pImmutableSamplers = 0;
                        binding.descriptorCount = 1;

                        // Write the number of uniform buffers
                        writer.Write(cbufferIndex.Count);

                        uint uniformSlots = 0;
                        uint textureSlots = 0;
                        uint samplerSlots = 0;

                        // We just have one cbuffer at 0 right now.
                        if (cbufferIndex.Count > 0)
                        {
                            uniformSlots |= 1 << 0;
                            binding.binding = 0;
                            binding.descriptorType = VkDescriptorType.UNIFORM_BUFFER_DYNAMIC;
                            bindings.Add(binding);
                        }

                        foreach (var s in samplers)
                        {
                            if (s.textureSlot == s.samplerSlot)
                            {
                                textureSlots |= (uint)(1 << s.textureSlot);
                                samplerSlots |= (uint)(1 << s.textureSlot);
                                binding.binding = (uint)(s.textureSlot + SlotOffset);
                                binding.descriptorType = VkDescriptorType.COMBINED_IMAGE_SAMPLER;
                                bindings.Add(binding);

                                continue;
                            }

                            samplerSlots |= (uint)(1 << s.samplerSlot);
                            binding.binding = (uint)(s.samplerSlot + SlotOffset);
                            binding.descriptorType = VkDescriptorType.SAMPLER;
                            bindings.Add(binding);

                            textureSlots |= (uint)(1 << s.textureSlot);
                            binding.binding = (uint)(s.textureSlot + SlotOffset);
                            binding.descriptorType = VkDescriptorType.SAMPLED_IMAGE;
                            bindings.Add(binding);
                        }

                        // Write the slot bits.
                        writer.Write(uniformSlots);
                        writer.Write(textureSlots);
                        writer.Write(samplerSlots);

                        // Write the bindings.
                        writer.Write((uint)bindings.Count);
                        foreach (var b in bindings)
                        {
                            writer.Write(b.binding);
                            writer.Write((uint)b.descriptorType);
                            writer.Write(b.descriptorCount);
                            writer.Write((uint)b.stageFlags);
                            writer.Write((UInt64)b.pImmutableSamplers);
                        }

                        // Finally write the shader bytecode.
                        writer.Write(shaderData.Bytecode);

                        // Store the combined binding layout info and shader code.
                        shaderData.ShaderCode = stream.ToArray();
                    }
                }

                effect.Shaders.Add(shaderData);
                                
                return shaderData;
            }
            finally
            {
                foreach (var file in cleanup)
                {
                    try
                    {
                        File.Delete(file);
                    }
                    catch { }
                }
            }
        }
    }
}
