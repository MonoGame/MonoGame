using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;

namespace MonoGame.Effect
{
    internal static class ShaderConductor
    {
        [DllImport("ShaderConductorWrapper", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern void Compile([In] ref SourceDesc source, [In] ref OptionsDesc options, [In] ref TargetDesc target, out ResultDesc result);

        [DllImport("ShaderConductorWrapper", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern void Disassemble([In] ref DisassembleDesc source, out ResultDesc result);

        [DllImport("ShaderConductorWrapper", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr CreateShaderConductorBlob(IntPtr data, int size);

        [DllImport("ShaderConductorWrapper", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern void DestroyShaderConductorBlob(IntPtr blob);

        [DllImport("ShaderConductorWrapper", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr GetShaderConductorBlobData(IntPtr blob);

        [DllImport("ShaderConductorWrapper", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern int GetShaderConductorBlobSize(IntPtr blob);

        [DllImport("ShaderConductorWrapper", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern int GetStageInputCount([In] ref ResultDesc result);

        [DllImport("ShaderConductorWrapper", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern void GetStageInput([In] ref ResultDesc result, int stageInputIndex, byte[] name, int maxNameLength, out int location, out int rows, out int columns);

        [DllImport("ShaderConductorWrapper", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern int GetUniformBufferCount([In] ref ResultDesc result);

        [DllImport("ShaderConductorWrapper", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern void GetUniformBuffer([In] ref ResultDesc result, int bufferIndex, byte[] blockName, byte[] instanceName, int maxNameLength, out int byteSize, out int slot, out int parameterCount);

        [DllImport("ShaderConductorWrapper", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern void GetParameter([In] ref ResultDesc result, int bufferIndex, int parameterIndex, byte[] name, int maxNameLength, out int type, out int rows, out int columns, out int byteOffset, out int arrayDimensions);

        [DllImport("ShaderConductorWrapper", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern void GetParameterArraySize([In] ref ResultDesc result, int bufferIndex, int parameterIndex, int dimension, out int arraySize);

        [DllImport("ShaderConductorWrapper", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        private static extern int GetSamplerCount([In] ref ResultDesc result);

        [DllImport("ShaderConductorWrapper", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        private static extern void GetSampler([In] ref ResultDesc result, int samplerIndex, byte[] name, byte[] originalName, byte[] textureName, int maxNameLength, out int type, out int slot, out int textureSlot);

        [DllImport("ShaderConductorWrapper", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        private static extern int GetStorageBufferCount([In] ref ResultDesc result);

        [DllImport("ShaderConductorWrapper", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        private static extern void GetStorageBuffer([In] ref ResultDesc result, int bufferIndex, byte[] name, int maxNameLength, out int byteSize, out int slot, out bool readOnly);

        [DllImport("ShaderConductorWrapper", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        private static extern int GetStorageImageCount([In] ref ResultDesc result);

        [DllImport("ShaderConductorWrapper", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        private static extern void GetStorageImage([In] ref ResultDesc result, int imageIndex, byte[] name, int maxNameLength, out int slot);

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

                    defaultInstance.packMatricesInRowMajor = false;
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

        //==============================================================
        // Reflection API
        //==============================================================
        public struct StageInput
        {
            public string name;
            public int location;
            public int rows;
            public int columns;
            public string usage;
            public int index;
        }

        public struct UniformBuffer
        {
            public string blockName;
            public string instanceName;
            public int byteSize;
            public int slot; // register binding
            public List<Parameter> parameters;
        }

        public struct Parameter
        {
            public string name;
            public int type;
            public int rows;
            public int columns;
            public int offset;
            public int arrayDimensions; // 0 = no array, 1 = 1D array, 2 = 2D array, ...
            public List<int> arraySize; // one entry for each array dimension
        }

        public struct Sampler
        {
            public string name;
            public string originalName;
            public string textureName;
            public int slot;
            public int textureSlot;
            public int type; // 1=1D, 2=2D, 3=3D, 4=Cube
        }

        public struct StorageBuffer
        {
            public string name;
            public int byteSize;
            public int slot; // register binding
            public int slotForCounter; // register binding for append/consume/counter buffer
            public bool readOnly;
        }

        public struct StorageImage
        {
            public string name;
            public int slot; // register binding
        }

        public static List<StageInput> GetStageInputs(ResultDesc result)
        {
            byte[] nameBuffer = new byte[MaxNameLength];

            int stageInputCount = GetStageInputCount(ref result);
            var stageInputs = new List<StageInput>();

            for (int i = 0; i < stageInputCount; i++)
            {
                GetStageInput(ref result, i, nameBuffer, MaxNameLength, out int location, out int rows, out int columns);

                string name = ByteBufferToString(nameBuffer);
                ExtractUsageAndIndexFromName(name, out string usage, out int index);

                stageInputs.Add(new StageInput
                {
                    name = name,
                    location = location,
                    rows = rows,
                    columns = columns,
                    usage = usage,
                    index = index,
                });
            }

            return stageInputs;
        }

        public static List<UniformBuffer> GetUniformBuffers(ResultDesc result)
        {
            byte[] blockNameBuffer = new byte[MaxNameLength];
            byte[] instanceNameBuffer = new byte[MaxNameLength];
            byte[] parameterNameBuffer = new byte[MaxNameLength];

            var buffers = new List<UniformBuffer>();
            int bufferCount = GetUniformBufferCount(ref result);

            for (int bufInd = 0; bufInd < bufferCount; bufInd++)
            {
                GetUniformBuffer(ref result, bufInd, blockNameBuffer, instanceNameBuffer, MaxNameLength, out int byteSize, out int slot, out int parameterCount);

                string blockName = ByteBufferToString(blockNameBuffer);
                string instanceName = ByteBufferToString(instanceNameBuffer);

                var parameters = new List<Parameter>();

                for (int i = 0; i < parameterCount; i++)
                {
                    GetParameter(ref result, bufInd, i, parameterNameBuffer, MaxNameLength, out int type, out int rows, out int columns, out int offset, out int arrayDimensions);
                    var param = new Parameter
                    {
                        name = ByteBufferToString(parameterNameBuffer),
                        type = type,
                        rows = rows,
                        columns = columns,
                        offset = offset,
                        arrayDimensions = arrayDimensions,
                        arraySize = new List<int>(),
                    };

                    for (int dim = 0; dim < arrayDimensions; dim++)
                    {
                        GetParameterArraySize(ref result, bufInd, i, dim, out int arraySize);
                        param.arraySize.Add(arraySize);
                    }

                    parameters.Add(param);
                }

                buffers.Add(new UniformBuffer
                {
                    blockName = blockName,
                    instanceName = instanceName,
                    byteSize = byteSize,
                    slot = slot,
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

            int samplerCount = GetSamplerCount(ref result);

            for (int i = 0; i < samplerCount; i++)
            {
                GetSampler(ref result, i, nameBuffer, originalNameBuffer, textureNameBuffer, MaxNameLength, out int type, out int slot, out int textureSlot);

                var sampler = new Sampler
                {
                    name = ByteBufferToString(nameBuffer),
                    originalName = ByteBufferToString(originalNameBuffer),
                    textureName = ByteBufferToString(textureNameBuffer),
                    slot = slot,
                    textureSlot = textureSlot,
                    type = type,
                };

                samplers.Add(sampler);
            }

            return samplers;
        }

        public static List<StorageBuffer> GetStorageBuffers(ResultDesc result)
        {
            var buffers = new List<StorageBuffer>();
            int bufferCount = GetStorageBufferCount(ref result);

            byte[] nameBuffer = new byte[MaxNameLength];
       
            for (int i = 0; i < bufferCount; i++)
            {
                GetStorageBuffer(ref result, i, nameBuffer, MaxNameLength, out int byteSize, out int slot, out bool readOnly);

                var buffer = new StorageBuffer
                {
                    name = ByteBufferToString(nameBuffer),
                    byteSize = byteSize,
                    slot = slot,
                    readOnly = readOnly,
                };

                buffers.Add(buffer);
            }

            // find counter buffers for append/consume/counter structured buffers,
            // remove them from the list, but store their binding slot with the corresponding buffer
            for (int i = 0; i < buffers.Count; i++)
            {
                var buffer = buffers[i];
                string counterBufferPrefix = "counter.var.";

                if (buffer.name.StartsWith(counterBufferPrefix))
                {
                    string correspondingName = buffer.name.Substring(counterBufferPrefix.Length);
                    int correspondingIndex = buffers.FindIndex(b => b.name == correspondingName);
                    if (correspondingIndex > 0)
                    {
                        StorageBuffer corresponsingBuffer = buffers[correspondingIndex];
                        corresponsingBuffer.slotForCounter = buffer.slot;
                        buffers[correspondingIndex] = corresponsingBuffer;

                        buffers.RemoveAt(i);
                        i--;
                    }
                }
            }

            return buffers;
        }

        public static List<StorageImage> GetStorageImages(ResultDesc result)
        {
            var images = new List<StorageImage>();

            byte[] name = new byte[MaxNameLength];

            int imageCount = GetStorageImageCount(ref result);

            for (int i = 0; i < imageCount; i++)
            {
                GetStorageImage(ref result, i, name, MaxNameLength, out int slot);

                var buffer = new StorageImage
                {
                    name = ByteBufferToString(name),
                    slot = slot,
                };

                images.Add(buffer);
            }

            return images;
        }

        private static void ExtractUsageAndIndexFromName(string stageInputName, out string usage, out int index)
        {
            usage = "";
            index = -1;

            var match = usageRegex.Match(stageInputName);
            if (match.Success)
            {
                usage = match.Groups["usage"].Value;
                string indexStr = match.Groups["index"].Value;
                index = indexStr == "" ? 0 : int.Parse(indexStr);
            }
        }

        private static string ByteBufferToString(byte[] buffer)
        {
            int nameLength = Array.IndexOf(buffer, (byte)0);
            return Encoding.ASCII.GetString(buffer, 0, nameLength);
        }

        const int MaxNameLength = 1024;

        private static Regex usageRegex = new Regex(@"in.var.(?<usage>[A-Za-z]+)(?<index>[0-9]*)", RegexOptions.Compiled);
    }
}
