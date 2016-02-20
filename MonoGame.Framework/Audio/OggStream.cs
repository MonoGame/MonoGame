// This code originated from:
//
//    http://theinstructionlimit.com/ogg-streaming-using-opentk-and-nvorbis
//    https://github.com/renaudbedard/nvorbis/
//
// It was released to the public domain by the author (Renaud Bedard).
// No other license is intended or required. 

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using NVorbis;
using OpenTK.Audio.OpenAL;

namespace Microsoft.Xna.Framework.Audio
{
    internal class OggStream : IDisposable
    {
        const int DefaultBufferCount = 3;

        internal readonly object stopMutex = new object();
        internal readonly object prepareMutex = new object();

        internal readonly int alSourceId;
        internal readonly int[] alBufferIds;

        readonly int alFilterId;
        readonly string oggFileName;

        internal VorbisReader Reader { get; private set; }
        internal bool Ready { get; private set; }
        internal bool Preparing { get; private set; }

        public Action FinishedAction { get; private set; }
        public int BufferCount { get; private set; }

        public OggStream(string filename, Action finishedAction = null, int bufferCount = DefaultBufferCount)
        {
            oggFileName = filename;
            FinishedAction = finishedAction;
            BufferCount = bufferCount;

            alBufferIds = AL.GenBuffers(bufferCount);
            ALHelper.CheckError("Failed to generate buffers.");
            alSourceId = OpenALSoundController.GetInstance.ReserveSource();

            if (OggStreamer.Instance.XRam.IsInitialized)
            {
                OggStreamer.Instance.XRam.SetBufferMode(BufferCount, ref alBufferIds[0], XRamExtension.XRamStorage.Hardware);
                ALHelper.CheckError("Failed to activate Xram.");
            }

            Volume = 1;

            if (OggStreamer.Instance.Efx.IsInitialized)
            {
                alFilterId = OggStreamer.Instance.Efx.GenFilter();
                ALHelper.CheckError("Failed to generate Efx filter.");
                OggStreamer.Instance.Efx.Filter(alFilterId, EfxFilteri.FilterType, (int)EfxFilterType.Lowpass);
                ALHelper.CheckError("Failed to set Efx filter type.");
                OggStreamer.Instance.Efx.Filter(alFilterId, EfxFilterf.LowpassGain, 1);
                ALHelper.CheckError("Failed to set Efx filter value.");
                LowPassHFGain = 1;
            }
        }

        public void Prepare()
        {
            if (Preparing) return;

            var state = AL.GetSourceState(alSourceId);
            ALHelper.CheckError("Failed to get source state.");

            lock (stopMutex)
            {
                switch (state)
                {
                    case ALSourceState.Playing:
                    case ALSourceState.Paused:
                        return;

                    case ALSourceState.Stopped:
                        lock (prepareMutex)
                        {
                            Close();
                            Empty();
                        }
                        break;
                }

                if (!Ready)
                {
                    lock (prepareMutex)
                    {
                        Preparing = true;
                        Open(precache: true);
                    }
                }
            }
        }

        public void Play()
        {
            var state = AL.GetSourceState(alSourceId);
            ALHelper.CheckError("Failed to get source state.");

            switch (state)
            {
                case ALSourceState.Playing: return;
                case ALSourceState.Paused:
                    Resume();
                    return;
            }

            Prepare();

            AL.SourcePlay(alSourceId);
            ALHelper.CheckError("Failed to play source.");

            Preparing = false;

            OggStreamer.Instance.AddStream(this);
        }

        public void Pause()
        {
            var state = AL.GetSourceState(alSourceId);
            ALHelper.CheckError("Failed to get source state.");
            if (state != ALSourceState.Playing)
                return;

            OggStreamer.Instance.RemoveStream(this);
            AL.SourcePause(alSourceId);
            ALHelper.CheckError("Failed to pause source.");
        }

        public void Resume()
        {
            var state = AL.GetSourceState(alSourceId);
            ALHelper.CheckError("Failed to get source state.");
            if (state != ALSourceState.Paused)
                return;

            OggStreamer.Instance.AddStream(this);
            AL.SourcePlay(alSourceId);
            ALHelper.CheckError("Failed to play source.");
        }

        public void Stop()
        {
            var state = AL.GetSourceState(alSourceId);
            ALHelper.CheckError("Failed to get source state.");
            if (state == ALSourceState.Playing || state == ALSourceState.Paused)
                StopPlayback();

            lock (stopMutex)
            {
                OggStreamer.Instance.RemoveStream(this);

                lock (prepareMutex)
                {
                    if (state != ALSourceState.Initial)
                        Empty(); // force the queued buffers to be unqueued to avoid issues on Mac
                }
            }
            AL.Source(alSourceId, ALSourcei.Buffer, 0);
            ALHelper.CheckError("Failed to free source from buffers.");
        }

        public void SeekToPosition(TimeSpan pos)
        {
            Reader.DecodedTime = pos;
            AL.SourceStop(alSourceId);
            ALHelper.CheckError("Failed to stop source.");
        }

        public TimeSpan GetPosition()
        {
            if (Reader == null)
                return TimeSpan.Zero;

            return Reader.DecodedTime;
        }

        public TimeSpan GetLength()
        {
            return Reader.TotalTime;
        }

        float lowPassHfGain;
        public float LowPassHFGain
        {
            get { return lowPassHfGain; }
            set
            {
                if (OggStreamer.Instance.Efx.IsInitialized)
                {
                    OggStreamer.Instance.Efx.Filter(alFilterId, EfxFilterf.LowpassGainHF, lowPassHfGain = value);
                    ALHelper.CheckError("Failed to set Efx filter.");
                    OggStreamer.Instance.Efx.BindFilterToSource(alSourceId, alFilterId);
                    ALHelper.CheckError("Failed to bind Efx filter to source.");
                }
            }
        }

        float volume;
        public float Volume
        {
            get { return volume; }
            set
            {
                AL.Source(alSourceId, ALSourcef.Gain, volume = value);
                ALHelper.CheckError("Failed to set volume.");
            }
        }

        public bool IsLooped { get; set; }

        public void Dispose()
        {
            var state = AL.GetSourceState(alSourceId);
            ALHelper.CheckError("Failed to get the source state.");
            if (state == ALSourceState.Playing || state == ALSourceState.Paused)
                StopPlayback();

            lock (prepareMutex)
            {
                OggStreamer.Instance.RemoveStream(this);

                if (state != ALSourceState.Initial)
                    Empty();

                Close();
            }

            AL.Source(alSourceId, ALSourcei.Buffer, 0);
            ALHelper.CheckError("Failed to free source from buffers.");
            OpenALSoundController.GetInstance.RecycleSource(alSourceId);
            AL.DeleteBuffers(alBufferIds);
            ALHelper.CheckError("Failed to delete buffer.");
            if (OggStreamer.Instance.Efx.IsInitialized)
            {
                OggStreamer.Instance.Efx.DeleteFilter(alFilterId);
                ALHelper.CheckError("Failed to delete EFX filter.");
            }
            
            
        }

        void StopPlayback()
        {
            AL.SourceStop(alSourceId);
            ALHelper.CheckError("Failed to stop source.");
        }

        void Empty()
        {
            int queued;
            AL.GetSource(alSourceId, ALGetSourcei.BuffersQueued, out queued);
            ALHelper.CheckError("Failed to fetch queued buffers.");
            if (queued > 0)
            {
                try
                {
                    AL.SourceUnqueueBuffers(alSourceId, queued);
                    ALHelper.CheckError("Failed to unqueue buffers (first attempt).");
                }
                catch (InvalidOperationException)
                {
                    // This is a bug in the OpenAL implementation
                    // Salvage what we can
                    int processed;
                    AL.GetSource(alSourceId, ALGetSourcei.BuffersProcessed, out processed);
                    ALHelper.CheckError("Failed to fetch processed buffers.");
                    var salvaged = new int[processed];
                    if (processed > 0)
                    {
                        AL.SourceUnqueueBuffers(alSourceId, processed, salvaged);
                        ALHelper.CheckError("Failed to unqueue buffers (second attempt).");
                    }

                    // Try turning it off again?
                    AL.SourceStop(alSourceId);
                    ALHelper.CheckError("Failed to stop source.");

                    Empty();
                }
            }
        }

        internal void Open(bool precache = false)
        {
            Reader = new VorbisReader(oggFileName);

            if (precache)
            {
                // Fill first buffer synchronously
                OggStreamer.Instance.FillBuffer(this, alBufferIds[0]);
                AL.SourceQueueBuffer(alSourceId, alBufferIds[0]);
                ALHelper.CheckError("Failed to queue buffer.");
            }

            Ready = true;
        }

        internal void Close()
        {
            if (Reader != null)
            {
                Reader.Dispose();
                Reader = null;
            }
            Ready = false;
        }
    }

    internal class OggStreamer : IDisposable
    {
        public readonly XRamExtension XRam = new XRamExtension();
        public readonly EffectsExtension Efx = new EffectsExtension();

        const float DefaultUpdateRate = 10;
        const int DefaultBufferSize = 44100;

        static readonly object singletonMutex = new object();

        readonly object iterationMutex = new object();
        readonly object readMutex = new object();

        readonly float[] readSampleBuffer;
        readonly short[] castBuffer;

        readonly HashSet<OggStream> streams = new HashSet<OggStream>();
        readonly List<OggStream> threadLocalStreams = new List<OggStream>(); 

        readonly Thread underlyingThread;
        volatile bool cancelled;

        public float UpdateRate { get; private set; }
        public int BufferSize { get; private set; }

        static OggStreamer instance;
        public static OggStreamer Instance
        {
            get
            {
                lock (singletonMutex)
                {
                    if (instance == null)
                        throw new InvalidOperationException("No instance running");
                    return instance;
                }
            }
            private set { lock (singletonMutex) instance = value; }
        }

        public OggStreamer(int bufferSize = DefaultBufferSize, float updateRate = DefaultUpdateRate)
        {
            UpdateRate = updateRate;
            BufferSize = bufferSize;

            lock (singletonMutex)
            {
                if (instance != null)
                    throw new InvalidOperationException("Already running");

                Instance = this;
                underlyingThread = new Thread(EnsureBuffersFilled) { Priority = ThreadPriority.Lowest };
                underlyingThread.Start();
            }

            readSampleBuffer = new float[bufferSize];
            castBuffer = new short[bufferSize];
        }

        public void Dispose()
        {
            lock (singletonMutex)
            {
                Debug.Assert(Instance == this, "Two instances running, somehow...?");

                cancelled = true;
                lock (iterationMutex)
                    streams.Clear();

                Instance = null;
            }
        }

        internal bool AddStream(OggStream stream)
        {
            lock (iterationMutex)
                return streams.Add(stream);
        }

        internal bool RemoveStream(OggStream stream)
        {
            lock (iterationMutex)
                return streams.Remove(stream);
        }

        public bool FillBuffer(OggStream stream, int bufferId)
        {
            int readSamples;
            long readerPosition = 0;
            lock (readMutex)
            {
                readerPosition = stream.Reader.DecodedPosition;
                readSamples = stream.Reader.ReadSamples(readSampleBuffer, 0, BufferSize);
                CastBuffer(readSampleBuffer, castBuffer, readSamples);
            }
            AL.BufferData(bufferId, stream.Reader.Channels == 1 ? ALFormat.Mono16 : ALFormat.Stereo16, castBuffer,
                readSamples * sizeof(short), stream.Reader.SampleRate);
            ALHelper.CheckError("Failed to fill buffer, readSamples = {0}, SampleRate = {1}, buffer.Length = {2}.", readSamples, stream.Reader.SampleRate, castBuffer.Length);


            return readSamples != BufferSize;
        }
        static void CastBuffer(float[] inBuffer, short[] outBuffer, int length)
        {
            for (int i = 0; i < length; i++)
            {
                var temp = (int)(32767f * inBuffer[i]);
                if (temp > short.MaxValue) temp = short.MaxValue;
                else if (temp < short.MinValue) temp = short.MinValue;
                outBuffer[i] = (short)temp;
            }
        }

        void EnsureBuffersFilled()
        {
            while (!cancelled)
            {
                Thread.Sleep((int) (1000 / ((UpdateRate <= 0) ? 1 : UpdateRate)));
                if (cancelled) break;

                threadLocalStreams.Clear();
                lock (iterationMutex) threadLocalStreams.AddRange(streams);

                foreach (var stream in threadLocalStreams)
                {
                    lock (stream.prepareMutex)
                    {
                        lock (iterationMutex)
                            if (!streams.Contains(stream))
                                continue;

                        bool finished = false;

                        int queued;
                        AL.GetSource(stream.alSourceId, ALGetSourcei.BuffersQueued, out queued);
                        ALHelper.CheckError("Failed to fetch queued buffers.");
                        int processed;
                        AL.GetSource(stream.alSourceId, ALGetSourcei.BuffersProcessed, out processed);
                        ALHelper.CheckError("Failed to fetch processed buffers.");

                        if (processed == 0 && queued == stream.BufferCount) continue;

                        int[] tempBuffers;
                        if (processed > 0)
                        {
                            tempBuffers = AL.SourceUnqueueBuffers(stream.alSourceId, processed);
                            ALHelper.CheckError("Failed to unqueue buffers.");
                        }
                        else
                            tempBuffers = stream.alBufferIds.Skip(queued).ToArray();

                        int bufferFilled = 0;
                        for (int i = 0; i < tempBuffers.Length; i++)
                        {
                            finished |= FillBuffer(stream, tempBuffers[i]);
                            bufferFilled++;

                            if (finished)
                            {
                                if (stream.IsLooped)
                                {
                                    stream.Close();
                                    stream.Open();
                                }
                                else
                                {
                                    lock (iterationMutex)
                                        streams.Remove(stream);
                                    i = tempBuffers.Length;
                                }

                                if (stream.FinishedAction != null)
                                    stream.FinishedAction.Invoke();
                            }
                        }

                        if (!finished && bufferFilled > 0) // queue only successfully filled buffers
                        {
                            AL.SourceQueueBuffers(stream.alSourceId, bufferFilled, tempBuffers);
                            ALHelper.CheckError("Failed to queue buffers.");
                        }
                        else if (!stream.IsLooped)
                            continue;
                    }

                    lock (stream.stopMutex)
                    {
                        if (stream.Preparing) continue;

                        lock (iterationMutex)
                            if (!streams.Contains(stream))
                                continue;

                        var state = AL.GetSourceState(stream.alSourceId);
                        ALHelper.CheckError("Failed to get source state.");
                        if (state == ALSourceState.Stopped)
                        {
                            AL.SourcePlay(stream.alSourceId);
                            ALHelper.CheckError("Failed to play.");
                        }
                    }
                }
            }
        }
    }
}
