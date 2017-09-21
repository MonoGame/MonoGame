// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework.Audio;

namespace MonoGame.OpenAL
{
    internal enum ALFormat
    {
        Mono8 = 0x1100,
        Mono16 = 0x1101,
        Stereo8 = 0x1102,
        Stereo16 = 0x1103,
        MonoIma4 = 0x1300,
        StereoIma4 = 0x1301,
        MonoMSAdpcm = 0x1302,
        StereoMSAdpcm = 0x1303,
        MonoFloat32 = 0x10010,
        StereoFloat32 = 0x10011,
    }

    internal enum ALError
    {
        NoError = 0,
        InvalidName = 0xA001,
        InvalidEnum = 0xA002,
        InvalidValue = 0xA003,
        InvalidOperation = 0xA004,
        OutOfMemory = 0xA005,
    }

    internal enum ALGetString
    {
        Extensions = 0xB004,
    }

    internal enum ALBufferi
    {
        UnpackBlockAlignmentSoft = 0x200C,
        LoopSoftPointsExt = 0x2015,
    }

    internal enum ALGetBufferi
    {
        Bits = 0x2002,
        Channels = 0x2003,
        Size = 0x2004,
    }

    internal enum ALSourceb
    {
        Looping = 0x1007,
    }

    internal enum ALSourcei
    {
        SourceRelative = 0x202,
        Buffer = 0x1009,
        EfxDirectFilter = 0x20005,
        EfxAuxilarySendFilter = 0x20006,
    }

    internal enum ALSourcef
    {
        Pitch = 0x1003,
        Gain = 0x100A,
        ReferenceDistance = 0x1020
    }

    internal enum ALGetSourcei
    {
        SampleOffset = 0x1025,
        SourceState = 0x1010,
        BuffersQueued = 0x1015,
        BuffersProcessed = 0x1016,
    }

    internal enum ALSourceState
    {
        Initial = 0x1011,
        Playing = 0x1012,
        Paused = 0x1013,
        Stopped = 0x1014,
    }

    internal enum ALListener3f
    {
        Position = 0x1004,
    }

    internal enum ALSource3f
    {
        Position = 0x1004,
        Velocity = 0x1006,
    }

    internal enum ALDistanceModel
    {
        None = 0,
        InverseDistanceClamped = 0xD002,
    }

    internal enum AlcError
    {
        NoError = 0,
    }

    internal enum AlcGetString
    {
        CaptureDeviceSpecifier = 0x0310,
        CaptureDefaultDeviceSpecifier = 0x0311,
        Extensions = 0x1006,
    }

    internal enum AlcGetInteger
    {
        CaptureSamples = 0x0312,
    }

    internal enum EfxFilteri
    {
        FilterType = 0x8001,
    }

    internal enum EfxFilterf
    {
        LowpassGain = 0x0001,
        LowpassGainHF = 0x0002,
        HighpassGain = 0x0001,
        HighpassGainLF = 0x0002,
        BandpassGain = 0x0001,
        BandpassGainLF = 0x0002,
        BandpassGainHF = 0x0003,
    }

    internal enum EfxFilterType
    {
        None = 0x0000,
        Lowpass = 0x0001,
        Highpass = 0x0002,
        Bandpass = 0x0003,
    }

    internal enum EfxEffecti
    {
        EffectType = 0x8001,
        SlotEffect = 0x0001,
    }

    internal enum EfxEffectSlotf
    {
        EffectSlotGain = 0x0002,
    }

    internal enum EfxEffectf
    {
        EaxReverbDensity = 0x0001,
        EaxReverbDiffusion = 0x0002,
        EaxReverbGain = 0x0003,
        EaxReverbGainHF = 0x0004,
        EaxReverbGainLF = 0x0005,
        DecayTime = 0x0006,
        DecayHighFrequencyRatio = 0x0007,
        DecayLowFrequencyRation = 0x0008,
        EaxReverbReflectionsGain = 0x0009,
        EaxReverbReflectionsDelay = 0x000A,
        ReflectionsPain = 0x000B,
        LateReverbGain = 0x000C,
        LateReverbDelay = 0x000D,
        LateRevertPain = 0x000E,
        EchoTime = 0x000F,
        EchoDepth = 0x0010,
        ModulationTime = 0x0011,
        ModulationDepth = 0x0012,
        AirAbsorbsionHighFrequency = 0x0013,
        EaxReverbHFReference = 0x0014,
        EaxReverbLFReference = 0x0015,
        RoomRolloffFactor = 0x0016,
        DecayHighFrequencyLimit = 0x0017,
    }

    internal enum EfxEffectType
    {
        Reverb = 0x8000,
    }

    internal class AL
    {
#if ANDROID
        const string NativeLibName = "openal32.dll";
#elif IOS
        const string NativeLibName = "/System/Library/Frameworks/OpenAL.framework/OpenAL";
#else
        internal const string NativeLibName = "soft_oal.dll";
#endif

        [CLSCompliant(false)]
        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "alEnable")]
        internal static extern void Enable(int cap);

        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "alBufferData")]
        internal static extern void BufferData(uint bid, int format, IntPtr data, int size, int freq);

        internal static void BufferData(int bid, ALFormat format, byte[] data, int size, int freq)
        {
            var handle = GCHandle.Alloc(data, GCHandleType.Pinned);
            BufferData((uint)bid, (int)format, handle.AddrOfPinnedObject(), size, freq);
            handle.Free();
        }

        internal static void BufferData(int bid, ALFormat format, short[] data, int size, int freq)
        {
            var handle = GCHandle.Alloc(data, GCHandleType.Pinned);
            BufferData((uint)bid, (int)format, handle.AddrOfPinnedObject(), size, freq);
            handle.Free();
        }

        [CLSCompliant(false)]
        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "alDeleteBuffers")]
        internal static unsafe extern void DeleteBuffers(int n, int* buffers);

        internal static void DeleteBuffers(int[] buffers)
        {
            DeleteBuffers(buffers.Length, ref buffers[0]);
        }

        internal unsafe static void DeleteBuffers(int n, ref int buffers)
        {
            fixed (int* pbuffers = &buffers)
            {
                DeleteBuffers(n, pbuffers);
            }
        }

        [CLSCompliant(false)]
        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "alBufferi")]
        internal static extern void Bufferi(int buffer, ALBufferi param, int value);

        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "alGetBufferi")]
        internal static extern void GetBufferi(int bid, ALGetBufferi param, out int value);

        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "alBufferiv")]
        internal static extern void Bufferiv(int bid, ALBufferi param, int[] values);

        internal static void GetBuffer(int bid, ALGetBufferi param, out int value)
        {
            GetBufferi(bid, param, out value);
        }

        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "alGenBuffers")]
        internal static unsafe extern void GenBuffers(int count, int* buffers);

        internal unsafe static void GenBuffers(int count, out int[] buffers)
        {
            buffers = new int[count];
            fixed (int* ptr = &buffers[0])
            {
                GenBuffers(count, ptr);
            }
        }

        internal static void GenBuffers(int count, out int buffer)
        {
            int[] ret;
            GenBuffers(count, out ret);
            buffer = ret[0];
        }

        internal static int[] GenBuffers(int count)
        {
            int[] ret;
            GenBuffers(count, out ret);
            return ret;
        }

        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "alGenSources")]
        internal static extern void GenSources(int n, uint[] sources);


        internal static void GenSources(int[] sources)
        {
            uint[] temp = new uint[sources.Length];
            GenSources(temp.Length, temp);
            for (int i = 0; i < temp.Length; i++)
            {
                sources[i] = (int)temp[i];
            }
        }

        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "alGetError")]
        internal static extern ALError GetError();

        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "alIsBuffer")]
        internal static extern bool IsBuffer(uint buffer);

        internal static bool IsBuffer(int buffer)
        {
            return IsBuffer((uint)buffer);
        }

        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "alSourcePause")]
        internal static extern void SourcePause(uint source);

        internal static void SourcePause(int source)
        {
            SourcePause((uint)source);
        }

        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "alSourcePlay")]
        internal static extern void SourcePlay(uint source);

        internal static void SourcePlay(int source)
        {
            SourcePlay((uint)source);
        }

        internal static string GetErrorString(ALError errorCode)
        {
            return errorCode.ToString();
        }

        [CLSCompliant(false)]
        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "alIsSource")]
        internal static extern bool IsSource(int source);

        [CLSCompliant(false)]
        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "alDeleteSources")]
        internal static extern void DeleteSources(int n, ref int sources);

        internal static void DeleteSource(int source)
        {
            DeleteSources(1, ref source);
        }

        [CLSCompliant(false)]
        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "alSourceStop")]
        internal static extern void SourceStop(int sourceId);

        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "alSourcei")]
        internal static extern void Source(int sourceId, int i, int a);

        [CLSCompliant(false)]
        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "alSource3i")]
        internal static extern void Source(int sourceId, ALSourcei i, int a, int b, int c);

        internal static void Source(int sourceId, ALSourcei i, int a)
        {
            Source(sourceId, (int)i, a);
        }

        internal static void Source(int sourceId, ALSourceb i, bool a)
        {
            Source(sourceId, (int)i, a ? 1 : 0);
        }

        [CLSCompliant(false)]
        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "alSourcef")]
        internal static extern void Source(int sourceId, ALSourcef i, float a);

        [CLSCompliant(false)]
        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "alSource3f")]
        internal static extern void Source(int sourceId, ALSource3f i, float x, float y, float z);

        [CLSCompliant(false)]
        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "alGetSourcei")]
        internal static extern void GetSource(int sourceId, ALGetSourcei i, out int state);

        internal static ALSourceState GetSourceState(int sourceId)
        {
            int state;
            GetSource(sourceId, ALGetSourcei.SourceState, out state);
            return (ALSourceState)state;
        }

        [CLSCompliant(false)]
        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "alGetListener3f")]
        internal static extern void GetListener(ALListener3f param, out float value1, out float value2, out float value3);

        [CLSCompliant(false)]
        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "alDistanceModel")]
        internal static extern void DistanceModel(ALDistanceModel model);

        [CLSCompliant(false)]
        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "alDopplerFactor")]
        internal static extern void DopplerFactor(float value);

        [CLSCompliant(false)]
        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "alSourceQueueBuffers")]
        internal unsafe static extern void SourceQueueBuffers(int sourceId, int numEntries, [In] int* buffers);

        [CLSCompliant(false)]
        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "alSourceUnqueueBuffers")]
        internal unsafe static extern void SourceUnqueueBuffers(int sourceId, int numEntries, [In] int* salvaged);

        [CLSCompliant(false)]
        internal unsafe static void SourceQueueBuffers(int sourceId, int numEntries, int[] buffers)
        {
            fixed (int* ptr = &buffers[0])
            {
                AL.SourceQueueBuffers(sourceId, numEntries, ptr);
            }
        }

        internal unsafe static void SourceQueueBuffer(int sourceId, int buffer)
        {
            AL.SourceQueueBuffers(sourceId, 1, &buffer);
        }

        [CLSCompliant(false)]
        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "alSourceUnqueueBuffers")]
        internal static extern void SourceUnqueueBuffers(int sid, int numEntries, [Out] int[] bids);

        internal static unsafe int[] SourceUnqueueBuffers(int sourceId, int numEntries)
        {
            if (numEntries <= 0)
            {
                throw new ArgumentOutOfRangeException("numEntries", "Must be greater than zero.");
            }
            int[] array = new int[numEntries];
            fixed (int* ptr = &array[0])
            {
                AL.SourceUnqueueBuffers(sourceId, numEntries, ptr);
            }
            return array;
        }

        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "alGetEnumValue")]
        internal static extern int GetEnumValue(string enumName);

        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "alIsExtensionPresent")]
        internal static extern bool IsExtensionPresent(string extensionName);

        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "alGetProcAddress")]
        internal static extern IntPtr GetProcAddress(string functionName);

        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "alGetString")]
        private static extern IntPtr alGetString(int p);

        internal static string GetString(int p)
        {
            return Marshal.PtrToStringAnsi(alGetString(p));
        }

        internal static string Get(ALGetString p)
        {
            return GetString((int)p);
        }
    }

    internal partial class Alc
    {
#if ANDROID
        const string NativeLibName = "openal32.dll";
#elif IOS
        const string NativeLibName = "/System/Library/Frameworks/OpenAL.framework/OpenAL";
#else
        internal const string NativeLibName = "soft_oal.dll";
#endif

        [CLSCompliant(false)]
        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "alcCreateContext")]
        internal static extern IntPtr CreateContext(IntPtr device, int[] attributes);

        internal static AlcError GetError()
        {
            return GetError(IntPtr.Zero);
        }

        [CLSCompliant(false)]
        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "alcGetError")]
        internal static extern AlcError GetError(IntPtr device);

        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "alcGetIntegerv")]
        internal static extern void alcGetIntegerv(IntPtr device, int param, int size, int[] values);

        internal static void GetInteger(IntPtr device, AlcGetInteger param, int size, int[] values)
        {
            alcGetIntegerv(device, (int)param, size, values);
        }

        [CLSCompliant(false)]
        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "alcGetCurrentContext")]
        internal static extern IntPtr GetCurrentContext();

        [CLSCompliant(false)]
        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "alcMakeContextCurrent")]
        internal static extern void MakeContextCurrent(IntPtr context);

        [CLSCompliant(false)]
        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "alcDestroyContext")]
        internal static extern void DestroyContext(IntPtr context);

        [CLSCompliant(false)]
        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "alcCloseDevice")]
        internal static extern void CloseDevice(IntPtr device);

        [CLSCompliant(false)]
        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "alcOpenDevice")]
        internal static extern IntPtr OpenDevice([In] [MarshalAs(UnmanagedType.LPStr)] string device);

        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "alcCaptureOpenDevice")]
        internal static extern IntPtr alcCaptureOpenDevice([In()] [MarshalAs(UnmanagedType.LPStr)] string device, uint sampleRate, int format, int sampleSize);

        internal static IntPtr CaptureOpenDevice(string device, uint sampleRate, ALFormat format, int sampleSize)
        {
            return alcCaptureOpenDevice(device, sampleRate, (int)format, sampleSize);
        }

        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "alcCaptureStart")]
        internal static extern IntPtr CaptureStart(IntPtr device);

        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "alcCaptureSamples")]
        internal static extern void CaptureSamples(IntPtr device, IntPtr buffer, int samples);

        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "alcCaptureStop")]
        internal static extern IntPtr CaptureStop(IntPtr device);

        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "alcCaptureCloseDevice")]
        internal static extern IntPtr CaptureCloseDevice(IntPtr device);

        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "alcIsExtensionPresent")]
        internal static extern bool IsExtensionPresent(IntPtr device, [MarshalAs(UnmanagedType.LPStr)] string extensionName);

        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "alcGetString")]
        internal static extern IntPtr alGetString(IntPtr device, int p);

        internal static string GetString(IntPtr device, int p)
        {
            return Marshal.PtrToStringAnsi(alGetString(device, p));
        }

        internal static string GetString(IntPtr device, AlcGetString p)
        {
            return GetString(device, (int)p);
        }
#if IOS
        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "alcSuspendContext")]
        internal static extern void SuspendContext(IntPtr context);

        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "alcProcessContext")]
        internal static extern void ProcessContext(IntPtr context);
#endif

#if ANDROID
        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "alcDevicePauseSOFT")]
        internal static extern void DevicePause(IntPtr device);

        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "alcDeviceResumeSOFT")]
        internal static extern void DeviceResume(IntPtr device);
#endif
    }

    internal class XRamExtension
    {
        internal enum XRamStorage
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

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate bool SetBufferModeDelegate(int n, ref int buffers, int value);

        private SetBufferModeDelegate setBufferMode;

        internal XRamExtension()
        {
            IsInitialized = false;
            if (!AL.IsExtensionPresent("EAX-RAM"))
            {
                return;
            }
            RamSize = AL.GetEnumValue("AL_EAX_RAM_SIZE");
            RamFree = AL.GetEnumValue("AL_EAX_RAM_FREE");
            StorageAuto = AL.GetEnumValue("AL_STORAGE_AUTOMATIC");
            StorageHardware = AL.GetEnumValue("AL_STORAGE_HARDWARE");
            StorageAccessible = AL.GetEnumValue("AL_STORAGE_ACCESSIBLE");
            if (RamSize == 0 || RamFree == 0 || StorageAuto == 0 || StorageHardware == 0 || StorageAccessible == 0)
            {
                return;
            }
            try
            {
                setBufferMode = (XRamExtension.SetBufferModeDelegate)Marshal.GetDelegateForFunctionPointer(AL.GetProcAddress("EAXSetBufferMode"), typeof(XRamExtension.SetBufferModeDelegate));
            }
            catch (Exception)
            {
                return;
            }
            IsInitialized = true;
        }

        internal bool IsInitialized { get; private set; }

        internal bool SetBufferMode(int i, ref int id, XRamStorage storage)
        {
            if (storage == XRamExtension.XRamStorage.Accessible)
            {
                return setBufferMode(i, ref id, StorageAccessible);
            }
            if (storage != XRamExtension.XRamStorage.Hardware)
            {
                return setBufferMode(i, ref id, StorageAuto);
            }
            return setBufferMode(i, ref id, StorageHardware);
        }
    }

    [CLSCompliant(false)]
    internal class EffectsExtension
    {
        /* Effect API */

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void alGenEffectsDelegate(int n, out uint effect);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void alDeleteEffectsDelegate(int n, ref int effect);
        //[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
        //private delegate bool alIsEffectDelegate (uint effect);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void alEffectfDelegate(uint effect, EfxEffectf param, float value);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void alEffectiDelegate(uint effect, EfxEffecti param, int value);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void alGenAuxiliaryEffectSlotsDelegate(int n, out uint effectslots);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void alDeleteAuxiliaryEffectSlotsDelegate(int n, ref int effectslots);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void alAuxiliaryEffectSlotiDelegate(uint slot, EfxEffecti type, uint effect);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void alAuxiliaryEffectSlotfDelegate(uint slot, EfxEffectSlotf param, float value);

        /* Filter API */
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private unsafe delegate void alGenFiltersDelegate(int n, [Out] uint* filters);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void alFilteriDelegate(uint fid, EfxFilteri param, int value);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void alFilterfDelegate(uint fid, EfxFilterf param, float value);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private unsafe delegate void alDeleteFiltersDelegate(int n, [In] uint* filters);


        private alGenEffectsDelegate alGenEffects;
        private alDeleteEffectsDelegate alDeleteEffects;
        //private alIsEffectDelegate alIsEffect;
        private alEffectfDelegate alEffectf;
        private alEffectiDelegate alEffecti;
        private alGenAuxiliaryEffectSlotsDelegate alGenAuxiliaryEffectSlots;
        private alDeleteAuxiliaryEffectSlotsDelegate alDeleteAuxiliaryEffectSlots;
        private alAuxiliaryEffectSlotiDelegate alAuxiliaryEffectSloti;
        private alAuxiliaryEffectSlotfDelegate alAuxiliaryEffectSlotf;
        private alGenFiltersDelegate alGenFilters;
        private alFilteriDelegate alFilteri;
        private alFilterfDelegate alFilterf;
        private alDeleteFiltersDelegate alDeleteFilters;

        internal static IntPtr device;
        static EffectsExtension _instance;
        internal static EffectsExtension Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new EffectsExtension();
                return _instance;
            }
        }

        internal EffectsExtension()
        {
            IsInitialized = false;
            if (!Alc.IsExtensionPresent(device, "ALC_EXT_EFX"))
            {
                return;
            }

            alGenEffects = (alGenEffectsDelegate)Marshal.GetDelegateForFunctionPointer(AL.GetProcAddress("alGenEffects"), typeof(alGenEffectsDelegate));
            alDeleteEffects = (alDeleteEffectsDelegate)Marshal.GetDelegateForFunctionPointer(AL.GetProcAddress("alDeleteEffects"), typeof(alDeleteEffectsDelegate));
            alEffectf = (alEffectfDelegate)Marshal.GetDelegateForFunctionPointer(AL.GetProcAddress("alEffectf"), typeof(alEffectfDelegate));
            alEffecti = (alEffectiDelegate)Marshal.GetDelegateForFunctionPointer(AL.GetProcAddress("alEffecti"), typeof(alEffectiDelegate));
            alGenAuxiliaryEffectSlots = (alGenAuxiliaryEffectSlotsDelegate)Marshal.GetDelegateForFunctionPointer(AL.GetProcAddress("alGenAuxiliaryEffectSlots"), typeof(alGenAuxiliaryEffectSlotsDelegate));
            alDeleteAuxiliaryEffectSlots = (alDeleteAuxiliaryEffectSlotsDelegate)Marshal.GetDelegateForFunctionPointer(AL.GetProcAddress("alDeleteAuxiliaryEffectSlots"), typeof(alDeleteAuxiliaryEffectSlotsDelegate));
            alAuxiliaryEffectSloti = (alAuxiliaryEffectSlotiDelegate)Marshal.GetDelegateForFunctionPointer(AL.GetProcAddress("alAuxiliaryEffectSloti"), typeof(alAuxiliaryEffectSlotiDelegate));
            alAuxiliaryEffectSlotf = (alAuxiliaryEffectSlotfDelegate)Marshal.GetDelegateForFunctionPointer(AL.GetProcAddress("alAuxiliaryEffectSlotf"), typeof(alAuxiliaryEffectSlotfDelegate));

            alGenFilters = (alGenFiltersDelegate)Marshal.GetDelegateForFunctionPointer(AL.GetProcAddress("alGenFilters"), typeof(alGenFiltersDelegate));
            alFilteri = (alFilteriDelegate)Marshal.GetDelegateForFunctionPointer(AL.GetProcAddress("alFilteri"), typeof(alFilteriDelegate));
            alFilterf = (alFilterfDelegate)Marshal.GetDelegateForFunctionPointer(AL.GetProcAddress("alFilterf"), typeof(alFilterfDelegate));
            alDeleteFilters = (alDeleteFiltersDelegate)Marshal.GetDelegateForFunctionPointer(AL.GetProcAddress("alDeleteFilters"), typeof(alDeleteFiltersDelegate));

            IsInitialized = true;
        }

        internal bool IsInitialized { get; private set; }

        /*
            
alEffecti (effect, EfxEffecti.FilterType, (int)EfxEffectType.Reverb);
            ALHelper.CheckError ("Failed to set Filter Type.");
            
        */

        internal void GenAuxiliaryEffectSlots(int count, out uint slot)
        {
            this.alGenAuxiliaryEffectSlots(count, out slot);
            ALHelper.CheckError("Failed to Genereate Aux slot");
        }

        internal void GenEffect(out uint effect)
        {
            this.alGenEffects(1, out effect);
            ALHelper.CheckError("Failed to Generate Effect.");
        }

        internal void DeleteAuxiliaryEffectSlot(int slot)
        {
            alDeleteAuxiliaryEffectSlots(1, ref slot);
        }

        internal void DeleteEffect(int effect)
        {
            alDeleteEffects(1, ref effect);
        }

        internal void BindEffectToAuxiliarySlot(uint slot, uint effect)
        {
            alAuxiliaryEffectSloti(slot, EfxEffecti.SlotEffect, effect);
            ALHelper.CheckError("Failed to bind Effect");
        }

        internal void AuxiliaryEffectSlot(uint slot, EfxEffectSlotf param, float value)
        {
            alAuxiliaryEffectSlotf(slot, param, value);
            ALHelper.CheckError("Failes to set " + param + " " + value);
        }

        internal void BindSourceToAuxiliarySlot(int SourceId, int slot, int slotnumber, int filter)
        {
            AL.Source(SourceId, ALSourcei.EfxAuxilarySendFilter, slot, slotnumber, filter);
        }

        internal void Effect(uint effect, EfxEffectf param, float value)
        {
            alEffectf(effect, param, value);
            ALHelper.CheckError("Failed to set " + param + " " + value);
        }

        internal void Effect(uint effect, EfxEffecti param, int value)
        {
            alEffecti(effect, param, value);
            ALHelper.CheckError("Failed to set " + param + " " + value);
        }

        internal unsafe int GenFilter()
        {
            uint filter = 0;
            this.alGenFilters(1, &filter);
            return (int)filter;
        }
        internal void Filter(int sourceId, EfxFilteri filter, int EfxFilterType)
        {
            this.alFilteri((uint)sourceId, filter, EfxFilterType);
        }
        internal void Filter(int sourceId, EfxFilterf filter, float EfxFilterType)
        {
            this.alFilterf((uint)sourceId, filter, EfxFilterType);
        }
        internal void BindFilterToSource(int sourceId, int filterId)
        {
            AL.Source(sourceId, ALSourcei.EfxDirectFilter, filterId);
        }
        internal unsafe void DeleteFilter(int filterId)
        {
            alDeleteFilters(1, (uint*)&filterId);
        }
    }
}

