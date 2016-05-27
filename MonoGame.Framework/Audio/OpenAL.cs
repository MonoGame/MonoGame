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
        MonoMSADPCM =0x1302,
        StereoMSADPCM =0x1303,
    }

    public enum ALError
    {
        NoError = 0,
        InvalidName = 0xA001,
        InvalidEnum = 0xA002,
        InvalidValue = 0xA003,
        InvalidOperation = 0xA004,
        OutOfMemory = 0xA005,
    }

    public enum ALGetString
    {
        Extensions = 0xB004,
    }

    public enum ALBufferi
    {
        UnpackBlockAlignmentSoft = 0x200C,
        LoopSoftPointsExt = 0x2015,
    }

    public enum ALGetBufferi
    {
        Bits = 0x2002,
        Channels = 0x2003,
        Size = 0x2004,
    }

    public enum ALSourceb
    {
        Looping = 0x1007,
    }

    public enum ALSourcei
    {
        Buffer = 0x1009,
        EfxDirectFilter = 0x20005,
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

    public enum AlcGetString
    {
        Extensions = 0x1006,
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
        public const string NativeLibName = "soft_oal.dll";

        [CLSCompliant (false)]
        [DllImport (NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "alEnable")]
        public static extern void Enable (int cap);

        [CLSCompliant(false)]
        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "alBufferData")]
        public static extern void BufferData(uint bid, int format, IntPtr data, int size, int freq);

        public static void BufferData(int bid, ALFormat format, byte[] data, int size, int freq)
        {
            var handle = GCHandle.Alloc(data, GCHandleType.Pinned);
            BufferData((uint)bid, (int)format, handle.AddrOfPinnedObject(), size, freq);
            handle.Free();
        }

        public static void BufferData(int bid, ALFormat format, short[] data, int size, int freq)
        {
            var handle = GCHandle.Alloc(data, GCHandleType.Pinned);
            BufferData((uint)bid, (int)format, handle.AddrOfPinnedObject(), size, freq);
            handle.Free();
        }

        [CLSCompliant (false)]
        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "alDeleteBuffers")]
        public static unsafe extern void DeleteBuffers(int n, int* buffers);

        public static void DeleteBuffers(int[] buffers)
        {
            DeleteBuffers (buffers.Length, ref buffers [0]);
        }

        public unsafe static void DeleteBuffers(int n, ref int buffers)
        {
            fixed (int* pbuffers = &buffers)
            {
                DeleteBuffers (n, pbuffers);
            }
        }

        [CLSCompliant (false)]
        [DllImport (NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "alBufferi")]
        public static extern void Bufferi (int buffer, ALBufferi param, int value);

        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "alGetBufferi")]
        public static extern void GetBufferi(int bid, ALGetBufferi param, out int value);

        [DllImport (NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "alBufferiv")]
        public static extern void Bufferiv (int bid, ALBufferi param, int[] values);

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
        public static extern void GenSources(int n, uint[] sources);


        public static void GenSources(int[] sources)
        {
            uint[] temp = new uint[sources.Length];
            GenSources(temp.Length, temp);
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

        public static string GetErrorString(ALError errorCode)
        {
            return errorCode.ToString (); 
        }

        [CLSCompliant (false)]
        [DllImport (NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "alDeleteSources")]
        public static extern void DeleteSources(int n, ref int sources);

        public static void DeleteSource(int source)
        {
            DeleteSources (1, ref source);
        }

        [CLSCompliant (false)]
        [DllImport (NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "alSourceStop")]
        public static extern void SourceStop (int sourceId);

        [CLSCompliant (false)]
        [DllImport (NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "alSourcei")]
        internal static extern void Source (int sourceId, int i, int a);

        public static void Source (int sourceId, ALSourcei i, int a)
        {
            Source (sourceId, (int)i, a);
        }

        public static void Source (int sourceId, ALSourceb i, bool a) {
            Source (sourceId, (int)i, a ? 1 : 0);
        }

        [CLSCompliant (false)]
        [DllImport (NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "alSourcef")]
        public static extern void Source (int sourceId, ALSourcef i, float a);

        [CLSCompliant (false)]
        [DllImport (NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "alSource3f")]
        public static extern void Source (int sourceId, ALSource3f i, float x, float y, float z);

        [CLSCompliant (false)]
        [DllImport (NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "alGetSourcei")]
        public static extern void GetSource (int sourceId, ALGetSourcei i, out int state);

        public static ALSourceState GetSourceState(int sourceId) {
            int state = (int)ALSourceState.Stopped;
            GetSource (sourceId, ALGetSourcei.SourceState, out state);
            return (ALSourceState)state;
        }

        [CLSCompliant (false)]
        [DllImport (NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "alGetListener3f")]
        public static extern void GetListener (ALListener3f param, out float value1, out float value2, out float value3);

        public static void DistanceModel(ALDistanceModel model) { }

        [CLSCompliant (false)]
        [DllImport (NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "alSourceQueueBuffers")]
        public unsafe static extern void SourceQueueBuffers (int sourceId, int numEntries, [In] int* buffers);

        [CLSCompliant (false)]
        [DllImport (NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "alSourceUnqueueBuffers")]
        public unsafe static extern void SourceUnqueueBuffers (int sourceId, int numEntries, [In] int* salvaged);

        [CLSCompliant (false)]
        public unsafe static void SourceQueueBuffers (int sourceId, int numEntries, int [] buffers)
        {
            fixed (int* ptr = &buffers[0]) {
                AL.SourceQueueBuffers (sourceId, numEntries, ptr);
            }
        }

        public unsafe static void SourceQueueBuffer (int sourceId, int buffer)
        {
            AL.SourceQueueBuffers (sourceId, 1, &buffer);
        }

        [DllImport (NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "alSourceUnqueueBuffers")]
        public static extern void SourceUnqueueBuffers (int sid, int numEntries, [Out] int [] bids);

        public static unsafe int [] SourceUnqueueBuffers (int sourceId, int numEntries)
        {
            if (numEntries <= 0) {
                throw new ArgumentOutOfRangeException ("numEntries", "Must be greater than zero.");
            }
            int [] array = new int [numEntries];
            fixed (int* ptr = &array [0])
            {
                AL.SourceUnqueueBuffers (sourceId, numEntries, ptr);
            }
            return array;
        }

        [DllImport (NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "alGetEnumValue")]
        public static extern int GetEnumValue (string enumName);

        [DllImport (NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "alIsExtensionPresent")]
        public static extern bool IsExtensionPresent (string extensionName);

        [DllImport (NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "alGetProcAddress")]
        public static extern IntPtr GetProcAddress (string functionName);

        [DllImport (NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "alGetString")]
        private static extern IntPtr alGetString (int p);

        public static string GetString (int p)
        {
            return Marshal.PtrToStringAnsi (alGetString (p));
        }

        public static string Get (ALGetString p)
        {
            return GetString ((int)p);
        }
    }

    public partial class Alc
    {
        public const string NativeLibName = "soft_oal.dll";

        [CLSCompliant (false)]
        [DllImport (NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "alcCreateContext")]
        public static extern IntPtr CreateContext (IntPtr device, int [] attributes);

        public static AlcError GetError()
        {
            return GetError (IntPtr.Zero);
        }

        [CLSCompliant (false)]
        [DllImport (NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "alcGetError")]
        public static extern AlcError GetError (IntPtr device);

        [CLSCompliant (false)]
        [DllImport (NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "alcGetCurrentContext")]
        public static extern IntPtr GetCurrentContext ();

        [CLSCompliant (false)]
        [DllImport (NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "alcMakeContextCurrent")]
        public static extern void MakeContextCurrent (IntPtr context);

        [CLSCompliant (false)]
        [DllImport (NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "alcDestroyContext")]
        public static extern void DestroyContext (IntPtr context);

        [CLSCompliant (false)]
        [DllImport (NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "alcCloseDevice")]
        public static extern void CloseDevice (IntPtr device);

        [CLSCompliant (false)]
        [DllImport (NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "alcOpenDevice")]
        public static extern IntPtr OpenDevice (string device);

        [DllImport (NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "alcGetString")]
        internal static extern IntPtr alGetString (IntPtr device, int p);

        public static string GetString (IntPtr device, int p)
        {
            return Marshal.PtrToStringAnsi (alGetString (device, p));
        }

        public static string GetString (IntPtr device, AlcGetString p)
        {
            return GetString (device, (int)p);
        }
    }

    public class XRamExtension
    {
        public enum XRamStorage
        {
            Automatic,
            Hardware,
            Accessible
        }

        private int RamSize;
        private int RamFree;
        private int StorageAuto;
        private int StorageHardware;
        private int StorageAccessible;

        private delegate bool SetBufferModeDelegate (int n, ref int buffers, int value);

        private SetBufferModeDelegate setBufferMode;

        public XRamExtension ()
        {
            IsInitialized = false;
            if (!AL.IsExtensionPresent ("EAX-RAM")) {
                return;
            }
            RamSize = AL.GetEnumValue ("AL_EAX_RAM_SIZE");
            RamFree = AL.GetEnumValue ("AL_EAX_RAM_FREE");
            StorageAuto = AL.GetEnumValue ("AL_STORAGE_AUTOMATIC");
            StorageHardware = AL.GetEnumValue ("AL_STORAGE_HARDWARE");
            StorageAccessible = AL.GetEnumValue ("AL_STORAGE_ACCESSIBLE");
            if (RamSize == 0 || RamFree == 0 || StorageAuto == 0 || StorageHardware == 0 || StorageAccessible == 0) {
                return;
            }
            try {
                setBufferMode = (XRamExtension.SetBufferModeDelegate)Marshal.GetDelegateForFunctionPointer (AL.GetProcAddress ("EAXSetBufferMode"), typeof (XRamExtension.SetBufferModeDelegate));
            } catch (Exception) {
                return;
            }
            IsInitialized = true;
        }

        public bool IsInitialized { get; private set; }

        public bool SetBufferMode(int i, ref int id, XRamStorage storage) {
            if (storage == XRamExtension.XRamStorage.Accessible) {
                return setBufferMode (i, ref id, StorageAccessible);
            }
            if (storage != XRamExtension.XRamStorage.Hardware) {
                return setBufferMode (i, ref id, StorageAuto);
            }
            return setBufferMode (i, ref id, StorageHardware);
        }
    }

    public class EffectsExtension
    {
        private unsafe delegate void alGenFiltersDelegate (int n, [Out] uint* filters);
        private delegate void alFilteriDelegate (uint fid, EfxFilteri param, int value);
        private delegate void alFilterfDelegate (uint fid, EfxFilterf param, float value);
        private unsafe delegate void alDeleteFiltersDelegate (int n, [In] uint* filters);

        private alGenFiltersDelegate alGenFilters;
        private alFilteriDelegate alFilteri;
        private alFilterfDelegate alFilterf;
        private alDeleteFiltersDelegate alDeleteFilters;

        public EffectsExtension ()
        {
            IsInitialized = false;
            if (!AL.IsExtensionPresent ("ALC_EXT_EFX")) {
                return;
            }

            alGenFilters = (alGenFiltersDelegate)Marshal.GetDelegateForFunctionPointer (AL.GetProcAddress ("alGenFilters"), typeof (alGenFiltersDelegate));
            alFilteri = (alFilteriDelegate)Marshal.GetDelegateForFunctionPointer (AL.GetProcAddress ("alFilteri"), typeof (alFilteriDelegate));
            alFilterf = (alFilterfDelegate)Marshal.GetDelegateForFunctionPointer (AL.GetProcAddress ("alFilterf"), typeof (alFilterfDelegate));
            alDeleteFilters = (alDeleteFiltersDelegate)Marshal.GetDelegateForFunctionPointer (AL.GetProcAddress ("alDeleteFilters"), typeof (alDeleteFiltersDelegate));

            IsInitialized = true;
        }

        public bool IsInitialized { get; private set; }

        public unsafe int GenFilter() {
            uint filter = 0;
            this.alGenFilters (1, &filter);
            return (int)filter;
        }
        public void Filter(int sourceId, EfxFilteri filter, int EfxFilterType) {
            this.alFilteri ((uint)sourceId, filter, EfxFilterType);
        }
        public void Filter(int sourceId, EfxFilterf filter, float EfxFilterType) {
            this.alFilterf ((uint)sourceId, filter, EfxFilterType);
        }
        public void BindFilterToSource(int sourceId, int filterId) {
            AL.Source (sourceId, ALSourcei.EfxDirectFilter, filterId);
        }
        public unsafe void DeleteFilter (int filterId)
        {
            alDeleteFilters (1, (uint*)&filterId);
        }
    }
}

