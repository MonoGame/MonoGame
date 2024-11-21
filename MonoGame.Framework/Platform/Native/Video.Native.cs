// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Threading;
using System.Collections.Concurrent;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Interop;


namespace Microsoft.Xna.Framework.Media;

public sealed partial class Video : IDisposable
{
    // TODO: This entire class is untested and speculative.  It
    // is based on the audio decoder design.
    //
    // We need to add support for one video codec (likely Theora)
    // to help prove this API and make adjustments to it.
    //
    // Things i expect we could have issues with:
    //
    //  - Video getting out of sync.
    //  - Too much CPU use.
    //  - For sure we have race conditions.
    //

    private unsafe MGM_VideoDecoder* _decoderV;
    private MGM_VideoDecoderInfo _infoV;

    private unsafe MGM_AudioDecoder* _decoderA;
    private MGM_AudioDecoderInfo _infoA;

    private unsafe MGA_Voice* _voice;

    private ManualResetEvent _paused = new ManualResetEvent(false);
    private int _state;
    private Thread _thread;

    private ConcurrentQueue<Texture2D> _frames;

    private bool _muted = false;
    private float _volume = 1.0f;
    private bool _looped = false;

    private unsafe void DecoderStream()
    {
        bool play_voice = true;

        Console.WriteLine("Video.DecoderStream");

        // Create the voice if we have audio data.
        if (_decoderA != null)
            _voice = MGA.Voice_Create(SoundEffect.System, _infoA.samplerate, _infoA.channels);

        bool videoFinished = false;
        bool audioFinished = false;

        MGG_Texture* lastFrame = null;

        var device = Game.Instance.GraphicsDevice;

        var sleep = (int)(1000.0f / _infoV.fps);

        while (true)
        {
            // Get the current state.
            var state = (MediaState)Interlocked.CompareExchange(ref _state, -1, -1);

            // Do we need to stop playback?
            if (state == MediaState.Stopped)
                break;

            // Are we paused?
            if (state == MediaState.Paused)
            {
                if (_voice != null)
                {
                    MGA.Voice_Pause(_voice);
                    play_voice = true;
                }

                // Block waiting for it to be signaled.
                _paused.Reset();
                _paused.WaitOne();
                continue;
            }

            // Process an audio frame.
            if (_voice != null && !audioFinished)
            {
                var count = MGA.Voice_GetBufferCount(_voice);
                if (count < 3)
                {
                    MGM.AudioDecoder_Decode(_decoderA, out var buffer, out var size);

                    if (size > 0)
                        MGA.Voice_AppendBuffer(_voice, buffer, size);
                    else
                        audioFinished = true;
                }

                if (play_voice)
                {
                    MGA.Voice_Play(_voice, false);
                    play_voice = false;
                }
            }

            // Process video frames.
            if (!videoFinished)
            {                
                var frame = MGM.VideoDecoder_Decode(_decoderV);

                // Did we get a new frame?
                if (frame != lastFrame)
                {
                    // A null frame means we're out of video data.
                    if (frame == null)
                        videoFinished = true;
                    else
                    {
                        // Queue the frame for rendering.

                        lastFrame = frame;

                        var texture = new Texture2D(
                            device,
                            frame,
                            _infoV.width,
                            _infoV.height,
                            false,
                            SurfaceFormat.Color,
                            Texture2D.SurfaceType.Texture,
                            1);

                        _frames.Enqueue(texture);                        
                    }
                }
            }

            // If both audio and video are finished then exit.
            if (videoFinished && audioFinished)
                break;

            // TODO: We could maybe do better?
            Thread.Sleep(sleep);
        }

        // We're done streaming.. cleanup.
        if (_voice != null)
        {
            MGA.Voice_Destroy(_voice);
            _voice = null;
        }

        Interlocked.Exchange(ref _state, (int)MediaState.Stopped);
    }

    private unsafe void PlatformInitialize()
    {
        var absolutePath = MGP.Platform_MakePath(TitleContainer.Location, FileName);

        // TODO: Maybe there is a better place to get this
        // that doesn't assume Game exists.
        var device = Game.Instance.GraphicsDevice;

        _decoderV = MGM.VideoDecoder_Create(device.Handle, absolutePath, out _infoV);
        if (_decoderV == null)
            return;

        Width = _infoV.width;
        Height = _infoV.height;
        FramesPerSecond = _infoV.fps;
        Duration = TimeSpan.FromMilliseconds(_infoV.duration);

        _state = (int)MediaState.Stopped;

        // Get the audio decoder if we have one.
        _decoderA = MGM.VideoDecoder_GetAudioDecoder(_decoderV, out _infoA);

        // Unsupported
        VideoSoundtrackType = VideoSoundtrackType.MusicAndDialog;
    }

    private unsafe void PlatformDispose(bool disposing)
    {
        Stop();

        if (_decoderV != null)
        {
            MGM.VideoDecoder_Destroy(_decoderV);
            _decoderV = null;
            _decoderA = null;
        }
    }

    internal unsafe Texture2D GetTexture()
    {
        if (_decoderV == null)
            return null;

        // If we have multiple queued frames then we're behind real time
        // and need to pop off the texture to display.
        //
        // If we have only one frame then keep it in the queue. This is
        // for cases like pausing, rendering between frames, or the last
        // frame of a video.
        //
        Texture2D texture = null;        
        if (_frames.Count > 1)
            _frames.TryDequeue(out texture);
        else
            _frames.TryPeek(out texture);

        return texture;
    }

    internal MediaState State
    {
        get
        {
            return (MediaState)Interlocked.CompareExchange(ref _state, -1, -1);
        }
    }

    internal unsafe TimeSpan Position
    {
        get
        {
            if (_decoderV == null)
                return TimeSpan.Zero;

            var position = MGM.VideoDecoder_GetPosition(_decoderV);
            return TimeSpan.FromMilliseconds(position);
        }
    }

    internal unsafe bool IsLooped
    {
        set
        {
            _looped = value;
            if (_decoderV != null)
                MGM.VideoDecoder_SetLooped(_decoderV, value);
        }

        get
        {
            return _looped;
        }
    }

    internal unsafe bool IsMuted
    {
        set
        {
            if (value)
            {
                _muted = true;
                if (_voice != null)
                    MGA.Voice_SetVolume(_voice, 0.0f);
            }
            else
            {
                _muted = false;
                if (_voice != null)
                    MGA.Voice_SetVolume(_voice, _volume);
            }
        }

        get
        {
            return _muted;
        }
    }

    internal unsafe float Volume
    {
        set
        {
            _volume = value;
            if (!_muted && _voice != null)
                MGA.Voice_SetVolume(_voice, _volume);
        }

        get
        {
            return _volume;
        }
    }

    internal unsafe void Play()
    {
        if (_decoderV == null)
            return;

        // Stop the current playback which cleans stuff up.
        Stop();

        // The thread does the work.
        _state = (int)MediaState.Playing;
        _thread = new Thread(DecoderStream);
        _thread.Start();
    }

    internal unsafe void Pause()
    {
        if (_decoderV == null)
            return;

        // Nothing to do if we're not playing.
        if (State != MediaState.Playing)
            return;

        Interlocked.CompareExchange(ref _state, (int)MediaState.Paused, (int)MediaState.Playing);
    }

    internal unsafe void Resume()
    {
        if (_decoderV == null)
            return;

        // Nothing to do if we're not paused.
        if (State != MediaState.Paused)
            return;

        // If we're paused then go back to playing state and wake up the thread.
        Interlocked.Exchange(ref _state, (int)MediaState.Playing);
        _paused.Set();
    }

    internal unsafe void Stop()
    {
        // Nothing to do if we're stopped already.
        if (State == MediaState.Stopped)
            return;

        // Signal the thread to stop... signal just in case it is paused.
        Interlocked.Exchange(ref _state, (int)MediaState.Stopped);
        _paused.Set();

        // Wait for the thread.
        _thread.Join();
        _thread = null;
    }
}
