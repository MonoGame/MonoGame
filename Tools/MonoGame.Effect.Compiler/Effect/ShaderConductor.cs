using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Xna.Framework.Graphics;

namespace MonoGame.Effect
{
    internal class ShaderConductor
    {
        [DllImport("ShaderConductorWrapper.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern void Compile([In] ref SourceDesc source, [In] ref OptionsDesc options, [In] ref TargetDesc target, out ResultDesc result);

        [DllImport("ShaderConductorWrapper.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern void Disassemble([In] ref DisassembleDesc source, out ResultDesc result);

        [DllImport("ShaderConductorWrapper.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr CreateShaderConductorBlob(IntPtr data, int size);

        [DllImport("ShaderConductorWrapper.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern void DestroyShaderConductorBlob(IntPtr blob);

        [DllImport("ShaderConductorWrapper.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr GetShaderConductorBlobData(IntPtr blob);

        [DllImport("ShaderConductorWrapper.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern int GetShaderConductorBlobSize(IntPtr blob);

        [DllImport("ShaderConductorWrapper.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        private static extern int GetStageInputCount([In] ref ResultDesc result);

        [DllImport("ShaderConductorWrapper.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        private static extern int GetUniformBufferCount([In] ref ResultDesc result);

        [DllImport("ShaderConductorWrapper.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        private static extern int GetSamplerCount([In] ref ResultDesc result);

        [DllImport("ShaderConductorWrapper.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        private static extern void GetStageInput([In] ref ResultDesc result, int stageInputIndex, byte[] name, int maxNameLength, out int location);

        [DllImport("ShaderConductorWrapper.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        private static extern void GetUniformBuffer([In] ref ResultDesc result, int bufferIndex, byte[] name, int maxNameLength, out int byteSize, out int parameterCount);

        [DllImport("ShaderConductorWrapper.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        private static extern void GetParameter([In] ref ResultDesc result, int bufferIndex, int parameterIndex, byte[] name, int maxNameLength, out int type, out int rows, out int columns, out int byteOffset);

        [DllImport("ShaderConductorWrapper.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        private static extern void GetSampler([In] ref ResultDesc result, int samplerIndex, byte[] name, byte[] originalName, byte[] textureName, int maxNameLength, out int type, out int slot);

        public static List<UniformBuffer> GetUniformBuffers(ResultDesc result)
        {
            byte[] nameBuffer = new byte[MaxNameLength];

            var buffers = new List<UniformBuffer>();
            int bufferCount = ShaderConductor.GetUniformBufferCount(ref result);

            for (int bufInd = 0; bufInd < bufferCount; bufInd++)
            {
                ShaderConductor.GetUniformBuffer(ref result, bufInd, nameBuffer, MaxNameLength, out int byteSize, out int parameterCount);

                string uniformBufferName = ByteBufferToString(nameBuffer);

                var parameters = new List<Parameter>();

                for (int i = 0; i < parameterCount; i++)
                {
                    ShaderConductor.GetParameter(ref result, bufInd, i, nameBuffer, MaxNameLength, out int type, out int rows, out int columns, out int offset); 

                    parameters.Add(new Parameter
                    {
                        name = ByteBufferToString(nameBuffer),
                        type = type,
                        rows = rows,
                        columns = columns,
                        offset = offset,
                    });
                }

                buffers.Add(new UniformBuffer
                {
                    name = uniformBufferName,
                    byteSize = byteSize,
                    parameters = parameters,
                });
            }

            return buffers;
        }

        public static List<Sampler> GetSamplers(ResultDesc result)
        {
            var samplers = new List<Sampler>();

            byte[] nameBuffer = new byte[MaxNameLength];
            byte[] originalNameBuffer = new byte[MaxNameLength];
            byte[] textureNameBuffer = new byte[MaxNameLength];

            int samplerCount = ShaderConductor.GetSamplerCount(ref result);

            for (int i = 0; i < samplerCount; i++)
            {
                ShaderConductor.GetSampler(ref result, i, nameBuffer, originalNameBuffer, textureNameBuffer, MaxNameLength, out int type, out int slot);

                samplers.Add(new Sampler {
                    name = ByteBufferToString(nameBuffer),
                    parameterName = ByteBufferToString(originalNameBuffer),
                    textureName = ByteBufferToString(textureNameBuffer),
                    slot = slot,
                });

                switch (type)
                {
                    case 0:
                        samplers[i].type = MojoShader.MOJOSHADER_samplerType.MOJOSHADER_SAMPLER_1D;
                        break;
                    case 1:
                        samplers[i].type = MojoShader.MOJOSHADER_samplerType.MOJOSHADER_SAMPLER_2D;
                        break;
                    case 2:
                        samplers[i].type = MojoShader.MOJOSHADER_samplerType.MOJOSHADER_SAMPLER_VOLUME;
                        break;
                    case 3:
                        samplers[i].type = MojoShader.MOJOSHADER_samplerType.MOJOSHADER_SAMPLER_CUBE;
                        break;
                    default:
                        samplers[i].type = MojoShader.MOJOSHADER_samplerType.MOJOSHADER_SAMPLER_UNKNOWN;
                        break;
                }
            }

            return samplers;
        }

        public static ShaderData.Attribute[] GetStageInputs(ResultDesc result, bool isVertexShader)
        {
            byte[] nameBuffer = new byte[MaxNameLength];

            int attributeCount = ShaderConductor.GetStageInputCount(ref result);
            var attributes = new ShaderData.Attribute[attributeCount];

            for (int i = 0; i < attributeCount; i++)
            {
                ShaderConductor.GetStageInput(ref result, i, nameBuffer, MaxNameLength, out int location);

                string attribName = ByteBufferToString(nameBuffer);

                VertexElementUsage usage = VertexElementUsage.Position;
                int index = 0;

                if (isVertexShader)
                    GetAttributeUsageAndIndexFromName(attribName, out usage, out index);

                attributes[i] = new ShaderData.Attribute
                {
                    name = attribName,
                    location = location,
                    usage = usage,
                    index = index,
                };
            }

            return attributes;
        }

        private static void GetAttributeUsageAndIndexFromName(string attributeName, out VertexElementUsage usage, out int index)
        {
            // GLSL doesn't have semantics for attributes like DirectX.
            // Fortunately ShaderConductor encodes usage and index into the attribute's name, so we can extract it from there.
            usage = VertexElementUsage.Position;
            index = 0;

            int underscrore = attributeName.LastIndexOf('_');
            if (underscrore < 0)
                throw new Exception("No VertexElementUsage found for attribute " + attributeName);

            string usagePlusIndex = attributeName.Substring(underscrore + 1);

            int numberStartIndex = usagePlusIndex.Length;
            while (numberStartIndex > 0 && char.IsDigit(usagePlusIndex[numberStartIndex - 1]))
                numberStartIndex--;

            string indexStr = usagePlusIndex.Substring(numberStartIndex);
            string usageStr = usagePlusIndex.Substring(0, numberStartIndex);

            index = indexStr == "" ? 0 : int.Parse(indexStr);

            switch (usageStr)
            {
                case "BINORMAL":
                    usage = VertexElementUsage.Binormal;
                    break;
                case "BLENDINDICES":
                    usage = VertexElementUsage.BlendIndices;
                    break;
                case "BLENDWEIGHT":
                    usage = VertexElementUsage.BlendWeight;
                    break;
                case "COLOR":
                    usage = VertexElementUsage.Color;
                    break;
                case "NORMAL":
                    usage = VertexElementUsage.Normal;
                    break;
                case "POSITION":
                    usage = VertexElementUsage.Position;
                    break;
                case "PSIZE":
                    usage = VertexElementUsage.PointSize;
                    break;
                case "TANGENT":
                    usage = VertexElementUsage.Tangent;
                    break;
                case "TEXCOORD":
                    usage = VertexElementUsage.TextureCoordinate;
                    break;
                case "FOG":
                    usage = VertexElementUsage.Fog;
                    break;
                case "TESSFACTOR":
                    usage = VertexElementUsage.TessellateFactor;
                    break;
                case "DEPTH":
                    usage = VertexElementUsage.Depth;
                    break;
                case "SAMPLE":
                    usage = VertexElementUsage.Sample;
                    break;
                default:
                    throw new Exception("No VertexElementUsage found for attribute " + attributeName);
            }
        }

        const int MaxNameLength = 1024;

        private static string ByteBufferToString(byte[] buffer)
        {
            int nameLength = Array.IndexOf(buffer, (byte)0);
            return Encoding.ASCII.GetString(buffer, 0, nameLength);
        }

        public class UniformBuffer
        {
            public string name;
            public int byteSize;
            public List<Parameter> parameters;
        }

        public class Parameter
        {
            public string name;
            public int type;
            public int rows;
            public int columns;
            public int offset;
        }

        public class Sampler
        {
            public string name;
            public string parameterName;
            public string textureName;
            public int slot;
            public MojoShader.MOJOSHADER_samplerType type;
        }

        public enum ShaderStage
        {
            VertexShader,
            PixelShader,
            GeometryShader,
            HullShader,
            DomainShader,
            ComputeShader,

            NumShaderStages,
        };

        public enum ShadingLanguage
        {
            Dxil = 0,
            SpirV,

            Hlsl,
            Glsl,
            Essl,
            Msl_macOS,
            Msl_iOS,

            NumShadingLanguages,
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MacroDefine
        {
            public string name;
            public string value;
        };

        [StructLayout(LayoutKind.Sequential)]
        public struct SourceDesc
        {
            public string source;
            public string entryPoint;
            public ShaderStage stage;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct ShaderModel
        {
            public int major;
            public int minor;

            public ShaderModel(int major, int minor)
            {
                this.major = major;
                this.minor = minor;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct OptionsDesc
        {
            [MarshalAs(UnmanagedType.I1)]
            public bool packMatricesInRowMajor; // Experimental: Decide how a matrix get packed
            [MarshalAs(UnmanagedType.I1)]
            public bool enable16bitTypes; // Enable 16-bit types, such as half, uint16_t. Requires shader model 6.2+
            [MarshalAs(UnmanagedType.I1)]
            public bool enableDebugInfo; // Embed debug info into the binary
            [MarshalAs(UnmanagedType.I1)]
            public bool disableOptimizations; // Force to turn off optimizations. Ignore optimizationLevel below.
            public int optimizationLevel; // 0 to 3, no optimization to most optimization

            public ShaderModel shaderModel;

            public int shiftAllTexturesBindings;
            public int shiftAllSamplersBindings;
            public int shiftAllCBuffersBindings;
            public int shiftAllUABuffersBindings;

            public static OptionsDesc Default
            {
                get
                {
                    var defaultInstance = new OptionsDesc();

                    defaultInstance.packMatricesInRowMajor = true;
                    defaultInstance.enable16bitTypes = false;
                    defaultInstance.enableDebugInfo = false;
                    defaultInstance.disableOptimizations = false;
                    defaultInstance.optimizationLevel = 3;
                    defaultInstance.shaderModel = new ShaderModel(6, 0);

                    return defaultInstance;
                }
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct TargetDesc
        {
            public ShadingLanguage language;
            public string version;
            public bool asModule;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct ResultDesc
        {
            public IntPtr target;
            public bool isText;
            public IntPtr errorWarningMsg;
            public bool hasError;
            public IntPtr reflection;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct DisassembleDesc
        {
            public ShadingLanguage language;
            public IntPtr binary;
            public int binarySize;
        }
    }
}
