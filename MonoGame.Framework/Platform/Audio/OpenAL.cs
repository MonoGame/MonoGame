// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework.Audio;
using MonoGame.Framework.Utilities;
using System.IO;

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
        ReferenceDistance = 0x1020,
        StereoAngles = 0x1030
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
#if !IOS
        internal const string LibraryName = "openal";
#else
        internal const string LibraryName = "__Internal";
#endif

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void alEnable(int cap);
        internal static void Enable(int cap) => alEnable(cap);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void alBufferData(uint bid, int format, IntPtr data, int size, int freq);

        internal static void BufferData(int bid, ALFormat format, byte[] data, int size, int freq)
        {
            var handle = GCHandle.Alloc(data, GCHandleType.Pinned);
            try
            {
                alBufferData((uint)bid, (int)format, handle.AddrOfPinnedObject(), size, freq);
            }
            finally
            {
                handle.Free();
            }
        }

        internal static void BufferData(int bid, ALFormat format, short[] data, int size, int freq)
        {
            var handle = GCHandle.Alloc(data, GCHandleType.Pinned);
            try
            {
                alBufferData((uint)bid, (int)format, handle.AddrOfPinnedObject(), size, freq);
            }
            finally
            {
                handle.Free();
            }
        }

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        internal unsafe static extern void alDeleteBuffers(int n, int* buffers);

        internal static void DeleteBuffers(int[] buffers)
        {
            DeleteBuffers(buffers.Length, ref buffers[0]);
        }

        internal unsafe static void DeleteBuffers(int n, ref int buffers)
        {
            fixed (int* pbuffers = &buffers)
            {
                alDeleteBuffers(n, pbuffers);
            }
        }

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void alBufferi(int buffer, ALBufferi param, int value);
        internal static void Bufferi(int buffer, ALBufferi param, int value) => alBufferi(buffer, param, value);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void alGetBufferi(int bid, ALGetBufferi param, out int value);
        internal static void GetBufferi(int bid, ALGetBufferi param, out int value) => alGetBufferi(bid, param, out value);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void alBufferiv(int bid, ALBufferi param, int[] values);
        internal static void Bufferiv(int bid, ALBufferi param, int[] values) => alBufferiv(bid, param, values);

        internal static void GetBuffer(int bid, ALGetBufferi param, out int value)
        {
            GetBufferi(bid, param, out value);
        }

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        internal unsafe static extern void alGenBuffers(int count, int* buffers);

        internal unsafe static void GenBuffers(int count, out int[] buffers)
        {
            buffers = new int[count];
            fixed (int* ptr = &buffers[0])
            {
                alGenBuffers(count, ptr);
            }
        }

        internal unsafe static void GenBuffer(out int buffer)
        {
            fixed (int* ptr = &buffer)
            {
                alGenBuffers(1, ptr);
            }
        }

        internal static int[] GenBuffers(int count)
        {
            int[] ret;
            GenBuffers(count, out ret);
            return ret;
        }

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void alGenSources(int n, uint[] sources);


        internal static void GenSources(int[] sources)
        {
            uint[] temp = new uint[sources.Length];
            alGenSources(temp.Length, temp);
            for (int i = 0; i < temp.Length; i++)
            {
                sources[i] = (int)temp[i];
            }
        }

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern ALError alGetError();
        internal static ALError GetError() => alGetError();

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern bool alIsBuffer(uint buffer);

        internal static bool IsBuffer(int buffer)
        {
            return alIsBuffer((uint)buffer);
        }

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void alSourcePause(uint source);

        internal static void SourcePause(int source)
        {
            alSourcePause((uint)source);
        }

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void alSourcePlay(uint source);

        internal static void SourcePlay(int source)
        {
            alSourcePlay((uint)source);
        }

        internal static string GetErrorString(ALError errorCode)
        {
            return errorCode.ToString();
        }

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern bool alIsSource(int source);
        internal static bool IsSource(int source) => alIsSource(source);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void alDeleteSources(int n, ref int sources);

        internal static void DeleteSource(int source)
        {
            alDeleteSources(1, ref source);
        }

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void alSourceStop(int sourceId);
        internal static void SourceStop(int sourceId) => alSourceStop(sourceId);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void alSourcei(int sourceId, int i, int a);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void alSource3i(int sourceId, ALSourcei i, int a, int b, int c);

        internal static void Source(int sourceId, ALSourcei i, int a)
        {
            alSourcei(sourceId, (int)i, a);
        }

        internal static void Source(int sourceId, ALSourceb i, bool a)
        {
            alSourcei(sourceId, (int)i, a ? 1 : 0);
        }

        internal static void Source(int sourceId, ALSource3f i, float x, float y, float z)
        {
            alSource3f(sourceId, i, x, y, z);
        }

        internal static void Source(int sourceId, ALSourcef i, float dist)
        {
            alSourcef(sourceId, i, dist);
        }

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void alSourcef(int sourceId, ALSourcef i, float a);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void alSource3f(int sourceId, ALSource3f i, float x, float y, float z);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void alSourcefv(int sourceId, ALSourcef i, float[] values);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void alGetSourcei(int sourceId, ALGetSourcei i, out int state);
        internal static void GetSource(int sourceId, ALGetSourcei i, out int state) => alGetSourcei(sourceId, i, out state);

        internal static ALSourceState GetSourceState(int sourceId)
        {
            int state;
            GetSource(sourceId, ALGetSourcei.SourceState, out state);
            return (ALSourceState)state;
        }

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void alGetListener3f(ALListener3f param, out float value1, out float value2, out float value3);
        internal static void GetListener(ALListener3f param, out float value1, out float value2, out float value3) => alGetListener3f(param, out value1, out value2, out value3);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void alDistanceModel(ALDistanceModel model);
        internal static void DistanceModel(ALDistanceModel model) => alDistanceModel(model);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void alDopplerFactor(float value);
        internal static void DopplerFactor(float value) => alDopplerFactor(value);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        internal unsafe static extern void alSourceQueueBuffers(int sourceId, int numEntries, int* buffers);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        internal unsafe static extern void alSourceUnqueueBuffers(int sourceId, int numEntries, int* salvaged);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        internal unsafe static extern void alSourceUnqueueBuffers(int sid, int numEntries, out int[] bids);

        internal static void SourceUnqueueBuffers(int sid, int numEntries, out int[] bids)
        {
            alSourceUnqueueBuffers(sid, numEntries, out bids);
        }

        internal static unsafe void SourceQueueBuffers(int sourceId, int numEntries, int[] buffers)
        {
            fixed (int* ptr = &buffers[0])
            {
                AL.alSourceQueueBuffers(sourceId, numEntries, ptr);
            }
        }

        internal unsafe static void SourceQueueBuffer(int sourceId, int buffer)
        {
            AL.alSourceQueueBuffers(sourceId, 1, &buffer);
        }

        internal static unsafe int[] SourceUnqueueBuffers(int sourceId, int numEntries)
        {
            if (numEntries <= 0)
            {
                throw new ArgumentOutOfRangeException("numEntries", "Must be greater than zero.");
            }
            int[] array = new int[numEntries];
            fixed (int* ptr = &array[0])
            {
                alSourceUnqueueBuffers(sourceId, numEntries, ptr);
            }
            return array;
        }

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int alGetEnumValue(string enumName);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern bool alIsExtensionPresent(string extensionName);
        internal static bool IsExtensionPresent(string extensionName) => alIsExtensionPresent(extensionName);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr alGetProcAddress(string functionName);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
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
        [DllImport(AL.LibraryName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr alcCreateContext(IntPtr device, int[] attributes);
        internal static IntPtr CreateContext(IntPtr device, int[] attributes) => alcCreateContext(device, attributes);

        internal static AlcError GetError()
        {
            return GetErrorForDevice(IntPtr.Zero);
        }

        [DllImport(AL.LibraryName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern AlcError alcGetError(IntPtr device);
        internal static AlcError GetErrorForDevice(IntPtr device) => alcGetError(device);

        [DllImport(AL.LibraryName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void alcGetIntegerv(IntPtr device, int param, int size, int[] values);

        internal static void GetInteger(IntPtr device, AlcGetInteger param, int size, int[] values)
        {
            alcGetIntegerv(device, (int)param, size, values);
        }

        [DllImport(AL.LibraryName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr alcGetCurrentContext();
        internal static IntPtr GetCurrentContext() => alcGetCurrentContext();

        [DllImport(AL.LibraryName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void alcMakeContextCurrent(IntPtr context);
        internal static void MakeContextCurrent(IntPtr context) => alcMakeContextCurrent(context);

        [DllImport(AL.LibraryName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void alcDestroyContext(IntPtr context);
        internal static void DestroyContext(IntPtr context) => alcDestroyContext(context);

        [DllImport(AL.LibraryName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void alcCloseDevice(IntPtr device);
        internal static void CloseDevice(IntPtr device) => alcCloseDevice(device);

        [DllImport(AL.LibraryName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr alcOpenDevice(string device);
        internal static IntPtr OpenDevice(string device) => alcOpenDevice(device);

        [DllImport(AL.LibraryName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr alcCaptureOpenDevice(string device, uint sampleRate, int format, int sampleSize);

        internal static IntPtr CaptureOpenDevice(string device, uint sampleRate, ALFormat format, int sampleSize)
        {
            return alcCaptureOpenDevice(device, sampleRate, (int)format, sampleSize);
        }

        [DllImport(AL.LibraryName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr alcCaptureStart(IntPtr device);
        internal static IntPtr CaptureStart(IntPtr device) => alcCaptureStart(device);

        [DllImport(AL.LibraryName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void alcCaptureSamples(IntPtr device, IntPtr buffer, int samples);
        internal static void CaptureSamples(IntPtr device, IntPtr buffer, int samples) => alcCaptureSamples(device, buffer, samples);

        [DllImport(AL.LibraryName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr alcCaptureStop(IntPtr device);
        internal static IntPtr CaptureStop(IntPtr device) => alcCaptureStop(device);

        [DllImport(AL.LibraryName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr alcCaptureCloseDevice(IntPtr device);
        internal static IntPtr CaptureCloseDevice(IntPtr device) => alcCaptureCloseDevice(device);

        [DllImport(AL.LibraryName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern bool alcIsExtensionPresent(IntPtr device, string extensionName);
        internal static bool IsExtensionPresent(IntPtr device, string extensionName) => alcIsExtensionPresent(device, extensionName);

        [DllImport(AL.LibraryName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr alcGetString(IntPtr device, int p);

        internal static string GetString(IntPtr device, int p)
        {
            return Marshal.PtrToStringAnsi(alcGetString(device, p));
        }

        internal static string GetString(IntPtr device, AlcGetString p)
        {
            return GetString(device, (int)p);
        }

#if IOS
        [DllImport("_Internal", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void alcSuspendContext(IntPtr context);
        internal static void SuspendContext(IntPtr context) => alcSuspendContext(context);

        [DllImport("_Internal", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void alcProcessContext(IntPtr context);
        internal static void ProcessContext(IntPtr context) => alcProcessContext(context);
#endif

#if ANDROID
        [DllImport("openal", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void alcDevicePauseSOFT(IntPtr device);
        internal static void DevicePause(IntPtr device) => alcDevicePauseSOFT(device);

        [DllImport("openal", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void alcDeviceResumeSOFT(IntPtr device);
        internal static void DeviceResume(IntPtr device) => alcDeviceResumeSOFT(device);
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
            RamSize = AL.alGetEnumValue("AL_EAX_RAM_SIZE");
            RamFree = AL.alGetEnumValue("AL_EAX_RAM_FREE");
            StorageAuto = AL.alGetEnumValue("AL_STORAGE_AUTOMATIC");
            StorageHardware = AL.alGetEnumValue("AL_STORAGE_HARDWARE");
            StorageAccessible = AL.alGetEnumValue("AL_STORAGE_ACCESSIBLE");
            if (RamSize == 0 || RamFree == 0 || StorageAuto == 0 || StorageHardware == 0 || StorageAccessible == 0)
            {
                return;
            }
            try
            {
                setBufferMode = Marshal.GetDelegateForFunctionPointer<XRamExtension.SetBufferModeDelegate>(AL.alGetProcAddress("EAXSetBufferMode"));
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

            alGenEffects = Marshal.GetDelegateForFunctionPointer<alGenEffectsDelegate>(AL.alGetProcAddress("alGenEffects"));
            alDeleteEffects = Marshal.GetDelegateForFunctionPointer<alDeleteEffectsDelegate>(AL.alGetProcAddress("alDeleteEffects"));
            alEffectf = Marshal.GetDelegateForFunctionPointer<alEffectfDelegate>(AL.alGetProcAddress("alEffectf"));
            alEffecti = Marshal.GetDelegateForFunctionPointer<alEffectiDelegate>(AL.alGetProcAddress("alEffecti"));
            alGenAuxiliaryEffectSlots = Marshal.GetDelegateForFunctionPointer<alGenAuxiliaryEffectSlotsDelegate>(AL.alGetProcAddress("alGenAuxiliaryEffectSlots"));
            alDeleteAuxiliaryEffectSlots = Marshal.GetDelegateForFunctionPointer<alDeleteAuxiliaryEffectSlotsDelegate>(AL.alGetProcAddress("alDeleteAuxiliaryEffectSlots"));
            alAuxiliaryEffectSloti = Marshal.GetDelegateForFunctionPointer<alAuxiliaryEffectSlotiDelegate>(AL.alGetProcAddress("alAuxiliaryEffectSloti"));
            alAuxiliaryEffectSlotf = Marshal.GetDelegateForFunctionPointer<alAuxiliaryEffectSlotfDelegate>(AL.alGetProcAddress("alAuxiliaryEffectSlotf"));

            alGenFilters = Marshal.GetDelegateForFunctionPointer<alGenFiltersDelegate>(AL.alGetProcAddress("alGenFilters"));
            alFilteri = Marshal.GetDelegateForFunctionPointer<alFilteriDelegate>(AL.alGetProcAddress("alFilteri"));
            alFilterf = Marshal.GetDelegateForFunctionPointer<alFilterfDelegate>(AL.alGetProcAddress("alFilterf"));
            alDeleteFilters = Marshal.GetDelegateForFunctionPointer<alDeleteFiltersDelegate>(AL.alGetProcAddress("alDeleteFilters"));

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
            AL.alSource3i(SourceId, ALSourcei.EfxAuxilarySendFilter, slot, slotnumber, filter);
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
