// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Tool;
using MonoGame.Effect.Compiler.Effect.Spirv;

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

                // Adds HLSL specific reflection information to the SPIR-V
                // https://github.com/Microsoft/DirectXShaderCompiler/blob/main/docs/SPIR-V.rst#reflection
                toolArgs += "-fspv-reflect ";

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
                    // Error: '-Fd cannot be used with -spirv' - investigate
                    //toolArgs += "-Fd \"" + dbgFile + "\" ";
                }
                toolArgs += "\"" + hlslFile + "\"";
                toolResult = Dxc.Run(toolArgs, out stdout, out stderr);

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

                string[] reflectionDataArray = File.ReadAllLines(reflectFile);
                SpirvReflectionInfo reflectionInfo = SpirvReflectionInfo.Parse(reflectionDataArray);

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
                var samplers = new List<ShaderData.Sampler>();

                int cbCount = 0;
                var cbufferIndex = new List<int>();

                // we could have multiple descriptor sets in here, but we don't currently handle that
                var withDescriptorSet = reflectionInfo.Variables.Where(v => v.DescriptorSet.HasValue);

                foreach (SpirvVariable variable in withDescriptorSet)
                {
                    if (variable.Pointer.PointerType.Type == SpirvType.Struct)
                    {
                        // TODO: Look into multiple cbuffer support.
                        if (cbCount > 0)
                        {
                            errorsAndWarnings += "Building effects for Vulkan currently doesn't support more than one constant buffer (cbuffer) structures. Please consider refactoring your HLSL code.";
                            throw new ShaderCompilerException();
                        }

                        SpirvTypeStruct constantBuffer = variable.Pointer.PointerType as SpirvTypeStruct;
                        ConstantBufferData cbuffer = ConstantBufferData.BuildFromSpirvStruct(constantBuffer);

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

                        cbCount++;
                    }
                    else if (variable.Pointer.PointerType.Type == SpirvType.Image)
                    {
                        // find all the times this image was sampled by distinct samplers.
                        var sampledImages = reflectionInfo.SampledImages.Where(si => si.LoadedImage.Variable == variable)
                            .DistinctBy(si => si.LoadedSampler.Variable);

                        // and create a sampler parameter for each.
                        foreach (var sampledImage in sampledImages)
                        {
                            // pull out what we need from the sampled image object
                            var samplerVariable = sampledImage.LoadedSampler.Variable;
                            var imageVariable = sampledImage.LoadedImage.Variable;

                            var samplerType = samplerVariable.Pointer.PointerType as SpirvTypeSampler;
                            var imageType = imageVariable.Pointer.PointerType as SpirvTypeImage;

                            var sampler = new ShaderData.Sampler
                            {
                                samplerSlot = (int)samplerVariable.BindingSlot.Value - SlotOffset,
                                samplerName = samplerVariable.Name,
                                textureSlot = (int)imageVariable.BindingSlot.Value - SlotOffset,
                            };

                            // This image is only sampled by one sampler, we can safely use the texture name for the parameter.
                            if (sampledImages.Count() == 1)
                            {
                                sampler.parameterName = imageVariable.Name;
                            }
                            // otherwise make a composite name for this image/sampler combo.
                            else
                            {
                                sampler.parameterName = $"{samplerVariable.Name}+{imageVariable.Name}";
                            }

                            switch (imageType.Dimensionality)
                            {
                                case ImageDimensionality.OneD:
                                    sampler.type = MojoShader.MOJOSHADER_samplerType.MOJOSHADER_SAMPLER_1D;
                                    break;
                                case ImageDimensionality.TwoD:
                                    sampler.type = MojoShader.MOJOSHADER_samplerType.MOJOSHADER_SAMPLER_2D;
                                    break;
                                case ImageDimensionality.ThreeD:
                                    sampler.type = MojoShader.MOJOSHADER_samplerType.MOJOSHADER_SAMPLER_VOLUME;
                                    break;
                                case ImageDimensionality.Cube:
                                    sampler.type = MojoShader.MOJOSHADER_samplerType.MOJOSHADER_SAMPLER_CUBE;
                                    break;
                            }

                            if (!shaderResult.ShaderInfo.SamplerStates.TryGetValue(samplerVariable.Name, out SamplerStateInfo samplerStateInfo))
                            {
                                errorsAndWarnings += $"Could not find sampler state info for sampler '{samplerVariable.Name}'; using defaults\n";
                                samplerStateInfo = new SamplerStateInfo();
                            }

                            sampler.state = samplerStateInfo.State;
                            samplers.Add(sampler);
                        }
                    }
                }

                // Gather the input attributes.
                var attributes = new List<ShaderData.Attribute>();
                if (isVertexShader)
                {
                    // Sort by the location.
                    var sorted = reflectionInfo.Input.OrderBy(i => i.Location);

                    foreach (SpirvVariable input in sorted)
                    {
                        var a = new ShaderData.Attribute();
                        var semanticId = input.HlslSemantic ?? input.Id.Replace("%in_var_", "");

                        var m = Regex.Match(semanticId, @"(\D+)(\d+)?");
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

                        // TODO: These are unused at runtime under the
                        // new native backends, we will remove them soon.               
                        a.location = 0;
                        a.name = string.Empty;

                        attributes.Add(a);
                    }
                }

                shaderData._samplers = samplers.ToArray();
                shaderData._cbuffers = cbufferIndex.ToArray();
                shaderData._attributes = attributes.ToArray();

                // Generate the layout bindings from our cbuffers, samplers, and textures.
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
