// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;
using MonoGame.Interop;
using Microsoft.Xna.Framework.Graphics;


namespace Microsoft.Xna.Framework.Media;

public sealed partial class Video : IDisposable
{
    private unsafe MGM_Video* _video;
    private Texture2D[] _frameCache;
    private bool _muted = false;
    private float _volume = 1.0f;
    private bool _looped = false;

    private unsafe void PlatformInitialize()
    {
        var mediaFilePath = Path.Combine(TitleContainer.Location, FileName);

        // TODO: We should expose the back buffer count so that
        // video playback and optimize the number of textures created
        // for video frames.
        int cachedFrameNum = 2; // Game.Instance.GraphicsDevice.BackBufferCount;

        // TODO: May be worth letting the video return texture
        // format, so it can avoid CPU conversions when possible.

        _video = MGM.Video_Create(mediaFilePath, cachedFrameNum, out var width, out var height, out var fps, out var duration);

        Width = width;
        Height = height;
        Duration = TimeSpan.FromMilliseconds(duration);
        FramesPerSecond = fps;
        _frameCache = new Texture2D[cachedFrameNum];

        // Unsupported
        VideoSoundtrackType = VideoSoundtrackType.MusicAndDialog;
    }

    private unsafe void PlatformDispose(bool disposing)
    {
        if (_video != null)
        {
            MGM.Video_Destroy(_video);
            _video = null;
        }
    }

    internal unsafe Texture2D GetTexture()
    {
        if (_video == null)
            return null;

        MGM.Video_GetFrame(_video, out uint frame, out MGG_Texture* handle);

        uint index = frame % (uint)_frameCache.Length;
        var texture = _frameCache[index];

        if (texture == null)
        {
            // The video player owns the texture handle here.

            _frameCache[index] = texture = new Texture2D(
                Game.Instance.GraphicsDevice,
                handle,
                Width,
                Height,
                false,
                SurfaceFormat.Color,
                Texture2D.SurfaceType.Texture,
                1);
        }

        return texture;
    }

    internal unsafe MediaState State
    {
        get
        {
            if (_video == null)
                return MediaState.Stopped;

            return MGM.Video_GetState(_video);
        }
    }

    internal unsafe TimeSpan Position
    {
        get
        {
            if (_video == null)
                return TimeSpan.Zero;

            var position = MGM.Video_GetPosition(_video);
            return TimeSpan.FromMilliseconds(position);
        }
    }

    internal unsafe bool IsLooped
    {
        set
        {
            _looped = value;
            if (_video != null)
                MGM.Video_SetLooped(_video, value);
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
                if (_video != null)
                    MGM.Video_SetVolume(_video, 0.0f);
            }
            else
            {
                _muted = false;
                if (_video != null)
                    MGM.Video_SetVolume(_video, _volume);
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
            if (!_muted && _video != null)
                MGM.Video_SetVolume(_video, _volume);
        }

        get
        {
            return _volume;
        }
    }

    internal unsafe void Play()
    {
        if (_video != null)
            MGM.Video_Play(_video);
    }

    internal unsafe void Pause()
    {
        if (_video != null)
            MGM.Video_Pause(_video);
    }

    internal unsafe void Resume()
    {
        if (_video != null)
            MGM.Video_Resume(_video);
    }

    internal unsafe void Stop()
    {
        if (_video != null)
            MGM.Video_Stop(_video);
    }
}
