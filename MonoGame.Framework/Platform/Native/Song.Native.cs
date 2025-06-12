// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;
using System.Threading;
using Microsoft.Xna.Framework.Audio;
using MonoGame.Interop;


namespace Microsoft.Xna.Framework.Media;

public sealed partial class Song : IEquatable<Song>, IDisposable
{
    private unsafe MGM_AudioDecoder* _decoder;
    private unsafe MGA_Voice* _voice;

    private MGM_AudioDecoderInfo _info;

    private readonly ManualResetEvent _stop = new ManualResetEvent(false);
    private Thread _thread;

    private float _volume = 1.0f;

    private unsafe void DecoderStream()
    {
        bool start_voice = true;
        bool finished = false;

        Console.WriteLine("DecoderStream");

        while (true)
        {
            // Do we need to stop?
            if (_stop.WaitOne(0))
                break;

            var count = MGA.Voice_GetBufferCount(_voice);
            if (count > 2)
            {
                // TODO: This sucks... add OnBufferEnd type of callback
                // into the voice API so we don't have useless sleeps.
                Thread.Sleep(100);
                continue;
            }

            finished = MGM.AudioDecoder_Decode(_decoder, out var buffer, out var size);

            if (size > 0)
            {
                MGA.Voice_AppendBuffer(_voice, buffer, size);

                if (start_voice)
                {
                    MGA.Voice_Play(_voice, false);
                    start_voice = false;
                }
            }

            if (finished)
            {
                // Signal on the main thread.
                Threading.OnUIThread(() => DonePlaying(this, EventArgs.Empty));
                break;
            }
        }

        // We're done streaming.
    }

    #region The playback API used by MediaPlayer

    private unsafe void PlatformInitialize(string fileName)
    {
        var absolutePath = MGP.Platform_MakePath(TitleContainer.Location, fileName);

        _decoder = MGM.AudioDecoder_Create(absolutePath, out _info);
        if (_decoder == null)
            return;

        SoundEffect.Initialize();

        _voice = MGA.Voice_Create(SoundEffect.System, _info.samplerate, _info.channels);

        _duration = TimeSpan.FromMilliseconds(_info.duration);
    }

    private unsafe void PlatformDispose(bool disposing)
    {
        Stop();

        if (_voice != null)
        {
            MGA.Voice_Destroy(_voice);
            _voice = null;
        }

        if (_decoder != null)
        {
            MGM.AudioDecoder_Destroy(_decoder);
            _decoder = null;
        }
    }

    private int PlatformGetPlayCount()
    {
        return _playCount;
    }

    internal unsafe float Volume
    {
        get
        {
            return _volume;
        }

        set
        {
            _volume = value;

            if (_voice != null)
                MGA.Voice_SetVolume(_voice, _volume);
        }
    }

    internal unsafe TimeSpan Position
    {
        get
        {
            if (_voice == null)
                return TimeSpan.Zero;

            var milliseconds = MGA.Voice_GetPosition(_voice);
            return TimeSpan.FromMilliseconds(milliseconds);
        }
    }

    internal unsafe void Play(TimeSpan? startPosition, FinishedPlayingHandler handler)
    {
        if (_decoder == null)
            return;

        ulong milliseconds = 0;
        if (startPosition.HasValue)
            milliseconds = (ulong)startPosition.Value.TotalMilliseconds;

        // Only setup the finished callback once.
        if (DonePlaying == null)
            DonePlaying += handler;

        // Stop the current playback which cleans stuff up.
        Stop();
        
        // Move the decoder to the new position.
        MGM.AudioDecoder_SetPosition(_decoder, milliseconds);

        // The thread does the rest of the work.
        _stop.Reset();
        _thread = new Thread(DecoderStream);
        _thread.Start();

        _playCount++;
    }

    internal unsafe void Pause()
    {
        if (_voice == null)
            return;

        // The thread will stop processing on its own.
        MGA.Voice_Pause(_voice);
    }

    internal unsafe void Resume()
    {
        if (_voice == null)
            return;

        MGA.Voice_Resume(_voice);
    }

    internal unsafe void Stop()
    {
        if (_thread == null)
            return;

        MGA.Voice_Stop(_voice, false);

        // Halt the thread.
        _stop.Set();
        _thread.Join();
        _thread = null;        
    }


    #endregion

    #region Media Library Features Not Supported

    private Album PlatformGetAlbum()
    {
        // Not Supported.
        return null;
    }

    private Artist PlatformGetArtist()
    {
        // Not Supported.
        return null;
    }

    private Genre PlatformGetGenre()
    {
        // Not Supported.
        return null;
    }

    private bool PlatformIsProtected()
    {
        // Not Supported.
        return false;
    }

    private bool PlatformIsRated()
    {
        // Not Supported.
        return false;
    }

    private int PlatformGetRating()
    {
        // Not Supported.
        return 0;
    }

    private int PlatformGetTrackNumber()
    {
        // Not Supported.
        return 0;
    }

    #endregion
}
