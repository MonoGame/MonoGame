using System;

namespace OpenAL
{
    public enum ALFormat
    {
        Mono8,
        Mono16,
        Stereo8,
        Stereo16,
    }

    public enum ALError {
        NoError,
    }

    public enum ALGetBufferi {
        Bits, 
        Channels,
    }

    public enum ALSourceb {
        Looping,
    }

    public enum ALSourcei {
        Buffer,
    }

    public enum ALSourcef {
        Gain,
        Pitch
    }

    public enum ALGetSourcei {
        SourceState,
        SampleOffset,
        BuffersQueued,
        BuffersProcessed,
    }

    public enum ALSourceState {
        Stopped,
        Initial,
        Paused,
        Playing,
    }

    public enum ALListener3f {
        Position,
    }

    public enum ALSource3f {
        Position,
        Velocity,
    }

    public enum ALDistanceModel {
        InverseDistanceClamped
    }

    public partial class AL
    {
        public static ALError GetError (IntPtr device) { return ALError.NoError; }
        public static ALError GetError () { return ALError.NoError; }
        public static void GenBuffers(int count, out int buffer) { buffer = 0; }
        public static int[] GenBuffers(int count) { return null; }
        public static void DeleteBuffers(int count, ref int buffer) {}
        public static void DeleteBuffers(int[] buffers) {}
        public static string GetErrorString(ALError errorCode) { return ""; }
        public static void BufferData (int buffer, ALFormat format, byte[] data, int dataSize, int sampleRate) {}
        public static void BufferData (int buffer, ALFormat format, short[] data, int dataSize, int sampleRate) {}
        public static void GetBuffer (int buffer, ALGetBufferi i, out int b) { b = 0;}
        public static void SourcePause(int source) {}
        public static void SourcePlay (int source) {}
        public static bool IsBuffer(int buffer) { return false; }
        public static void GenSources(int[] sources) {}
        public static void DeleteSource(int source) {}
        public static void SourceStop(int sourceId) {}
        public static void Source(int sourceId, ALSourceb i, bool a) {}
        public static void Source(int sourceId, ALSourcei i, int a) {}
        public static void Source(int sourceId, ALSourcef i, float a) {}
        public static void Source(int sourceId, ALSource3f i, float x, float y, float z) {}
        public static void GetSource(int sourceId, ALGetSourcei i, out int state) { state = 0;}
        public static ALSourceState GetSourceState (int sourceId) { return ALSourceState.Stopped; }
        public static void GetListener (ALListener3f listener, out float x, out float y, out float z) {
            x =y =z= 0;
        }
        public static void DistanceModel (ALDistanceModel model) {}
        public static void SourceQueueBuffer (int sourceId, int bufferId) {}
        public static int[] SourceUnqueueBuffers (int sourceId, int processed) { return null; }
        public static int[] SourceUnqueueBuffers (int sourceId, int processed, int[] salvaged) { return null; }
        public static void SourceQueueBuffers (int sourceId, int filled, int[] buffers) {}

        public static void LoadEntryPoints()
        {
            LoadPlatformEntryPoints();
        }

        static partial void LoadPlatformEntryPoints();
    }

    public enum AlcError
    {
        NoError,
    }

    public partial class Alc
    {
        public static AlcError GetError () { return AlcError.NoError; }
        public static AlcError GetError (IntPtr device) { return AlcError.NoError;}
        public static IntPtr GetCurrentContext () { return IntPtr.Zero; }
        public static void MakeContextCurrent (IntPtr context) {}
        public static void DestroyContext(IntPtr context) {}
        public static void CloseDevice(IntPtr device) {}
        public static IntPtr OpenDevice(string device) { return IntPtr.Zero; }
    }
    public class AudioContext : IDisposable {
        public void Dispose () {}
    }

    public class XRamExtension {

        public enum XRamStorage {
            Hardware,
        }

        public bool IsInitialized { get; set; }
        public void SetBufferMode (int i, ref int id, XRamStorage storage) {}
    }

    public enum EfxFilteri {
        FilterType,
    }

    public enum EfxFilterf {
        LowpassGain,
        LowpassGainHF,
    }

    public enum EfxFilterType {
        Lowpass,
    }

    public class EffectsExtension {
        public bool IsInitialized { get; set; }
        public int GenFilter () { return 0; }
        public void Filter (int sourceId, EfxFilteri filter, int EfxFilterType) {}
        public void Filter (int sourceId, EfxFilterf filter, float EfxFilterType) {}
        public void BindFilterToSource (int sourceId, int filterId) {}
        public void DeleteFilter(int filterId) {}
    }
}

