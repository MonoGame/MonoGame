// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Runtime.InteropServices;

namespace OpenAL
{
    public enum ALFormat
    {
        Mono8 = 0x1100,
        Mono16 = 0x1101,
        Stereo8 = 0x1102,
        Stereo16 = 0x1103,
    }

    public enum ALError
    {
        NoError = 0,
    }

    public enum ALGetBufferi
    {
        Bits = 0x2002,
        Channels = 0x2003,
    }

    public enum ALSourceb
    {
        Looping = 0x1007,
    }

    public enum ALSourcei
    {
        Buffer = 0x1009,
    }

    public enum ALSourcef
    {
        Pitch = 0x1003,
        Gain = 0x100A,
    }

    public enum ALGetSourcei
    {
        SampleOffset = 0x1025,
        SourceState = 0x1010,
        BuffersQueued = 0x1015,
        BuffersProcessed = 0x1016,
    }

    public enum ALSourceState
    {
        Initial = 0x1011,
        Playing = 0x1012,
        Paused = 0x1013,
        Stopped = 0x1014,
    }

    public enum ALListener3f
    {
        Position = 0x1004,
    }

    public enum ALSource3f
    {
        Position = 0x1004,
        Velocity = 0x1006,
    }

    public enum ALDistanceModel
    {
        InverseDistanceClamped = 0xD002,
    }

    public enum AlcError
    {
        NoError = 0,
    }

    public enum EfxFilteri
    {
        FilterType = 0x8001,
    }

    public enum EfxFilterf
    {
        LowpassGain = 0x0001,
        LowpassGainHF = 0x0002,
    }

    public enum EfxFilterType
    {
        Lowpass = 0x0001,
    }

    public class AL
    {
        public const string NativeLibName = "openal32.dll";

        [CLSCompliant(false)]
        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "alBufferData")]
        public static extern void BufferData(uint bid, ALFormat format, IntPtr data, int size, int freq);

        public static void BufferData(int bid, ALFormat format, byte[] data, int size, int freq)
        {
            var handle = GCHandle.Alloc(data, GCHandleType.Pinned);
            BufferData((uint)bid, format, handle.AddrOfPinnedObject(), size, freq);
            handle.Free();
        }

        public static void BufferData(int bid, ALFormat format, short[] data, int size, int freq)
        {
            var handle = GCHandle.Alloc(data, GCHandleType.Pinned);
            BufferData((uint)bid, format, handle.AddrOfPinnedObject(), size, freq);
            handle.Free();
        }

        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "alDeleteBuffers")]
        public static extern void DeleteBuffers(int n, ref int[] buffers);

        public static void DeleteBuffers(int[] buffers)
        {
            DeleteBuffers(buffers.Length, ref buffers);
        }

        public static void DeleteBuffers(int n, ref int buffer)
        {
            var buffers = new[] { buffer };
            DeleteBuffers(n, ref buffers);
        }

        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "alGetBufferi")]
        public static extern void GetBufferi(int bid, ALGetBufferi param, out int value);

        public static void GetBuffer(int bid, ALGetBufferi param, out int value)
        {
            GetBufferi(bid, param, out value);
        }

        [CLSCompliant(false)]
        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "alGenBuffers")]
        public static unsafe extern void GenBuffers(int count, int* buffers);

        internal unsafe static void GenBuffers (int count,out int[] buffers)
        {
            buffers = new int[count];
            fixed (int* ptr = &buffers[0])
            {
                GenBuffers (count, ptr);
            }
        }

        public static void GenBuffers(int count, out int buffer)
        {
            int[] ret;
            GenBuffers(count, out ret);
            buffer = ret[0];
        }

        public static int[] GenBuffers(int count)
        {
            int[] ret;
            GenBuffers(count, out ret);
            return ret;
        }

        [CLSCompliant(false)]
        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "alGenSources")]
        public static extern void GenSources(int n, out uint sources);


        public static void GenSources(int[] sources)
        {
            uint[] temp = new uint[sources.Length];
            GenSources(temp.Length, out temp[0]);
            for (int i = 0; i < temp.Length; i++)
            {
                sources[i] = (int)temp[i];
            }
        }

        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "alGetError")]
        public static extern ALError GetError();

        [CLSCompliant(false)]
        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "alIsBuffer")]
        public static extern bool IsBuffer(uint buffer);

        public static bool IsBuffer(int buffer)
        {
            return IsBuffer((uint)buffer);
        }

        [CLSCompliant(false)]
        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "alSourcePause")]
        public static extern void SourcePause(uint source);

        public static void SourcePause(int source)
        {
            SourcePause((uint)source);
        }

        [CLSCompliant(false)]
        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "alSourcePlay")]
        public static extern void SourcePlay(uint source);

        public static void SourcePlay(int source)
        {
            SourcePlay((uint)source);
        }

        public static string GetErrorString(ALError errorCode) { return ""; }
        public static void DeleteSource(int source) { }
        public static void SourceStop(int sourceId) { }
        public static void Source(int sourceId, ALSourceb i, bool a) { }
        public static void Source(int sourceId, ALSourcei i, int a) { }
        public static void Source(int sourceId, ALSourcef i, float a) { }
        public static void Source(int sourceId, ALSource3f i, float x, float y, float z) { }
        public static void GetSource(int sourceId, ALGetSourcei i, out int state) { state = 0; }
        public static ALSourceState GetSourceState(int sourceId) { return ALSourceState.Stopped; }
        public static void GetListener(ALListener3f listener, out float x, out float y, out float z)
        {
            x = y = z = 0;
        }
        public static void DistanceModel(ALDistanceModel model) { }
        public static void SourceQueueBuffer(int sourceId, int bufferId) { }
        public static int[] SourceUnqueueBuffers(int sourceId, int processed) { return null; }
        public static int[] SourceUnqueueBuffers(int sourceId, int processed, int[] salvaged) { return null; }
        public static void SourceQueueBuffers(int sourceId, int filled, int[] buffers) { }
    }

    public partial class Alc
    {
        public static IntPtr CreateContext(IntPtr device, int[] attributes) { return IntPtr.Zero; }
        public static AlcError GetError() { return AlcError.NoError; }
        public static AlcError GetError(IntPtr device) { return AlcError.NoError; }
        public static IntPtr GetCurrentContext() { return IntPtr.Zero; }
        public static void MakeContextCurrent(IntPtr context) { }
        public static void DestroyContext(IntPtr context) { }
        public static void CloseDevice(IntPtr device) { }
        public static IntPtr OpenDevice(string device) { return IntPtr.Zero; }
    }

    public class XRamExtension
    {
        public enum XRamStorage
        {
            Hardware = 1,
        }

        public bool IsInitialized { get; set; }
        public void SetBufferMode(int i, ref int id, XRamStorage storage) { }
    }

    public class EffectsExtension
    {
        public bool IsInitialized { get; set; }
        public int GenFilter() { return 0; }
        public void Filter(int sourceId, EfxFilteri filter, int EfxFilterType) { }
        public void Filter(int sourceId, EfxFilterf filter, float EfxFilterType) { }
        public void BindFilterToSource(int sourceId, int filterId) { }
        public void DeleteFilter(int filterId) { }
    }
}

