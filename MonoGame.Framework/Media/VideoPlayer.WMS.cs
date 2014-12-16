﻿using System.Diagnostics;
using System.Threading;
using Microsoft.Xna.Framework.Graphics;
using SharpDX;
using SharpDX.MediaFoundation;
using SharpDX.Win32;
using System;

namespace Microsoft.Xna.Framework.Media
{
    public sealed partial class VideoPlayer : IDisposable
    {
        private static MediaSession _session;
        private static SimpleAudioVolume _volumeController;
        private static PresentationClock _clock;

        // HACK: Need SharpDX to fix this.
        private static readonly Guid MRPolicyVolumeService = Guid.Parse("1abaa2ac-9d3b-47c6-ab48-c59506de784d");
        private static readonly Guid SimpleAudioVolumeGuid = Guid.Parse("089EDF13-CF71-4338-8D13-9E569DBDC319");

        private static Callback _callback;

        private class Callback : IAsyncCallback
        {
            public void Dispose()
            {
            }

            public IDisposable Shadow { get; set; }
            public void Invoke(AsyncResult asyncResultRef)
            {
                var ev = _session.EndGetEvent(asyncResultRef);

                // Trigger an "on Video Ended" event here if needed

                _session.BeginGetEvent(this, null);
            }

            public AsyncCallbackFlags Flags { get; private set; }
            public WorkQueueId WorkQueueId { get; private set; }
        }

        private void PlatformInitialize()
        {
            MediaManagerState.CheckStartup();
            MediaFactory.CreateMediaSession(null, out _session);
        }

        private Texture2D PlatformGetTexture()
        {
            var sampleGrabber = _currentVideo.SampleGrabber;

            var texData = sampleGrabber.TextureData;

            if (texData == null)
                return null;

            // TODO: This could likely be optimized if we held on to the SharpDX Surface/Texture data,
            // and set it on an XNA one rather than constructing a new one every time this is called.
            var retTex = new Texture2D(Game.Instance.GraphicsDevice, _currentVideo.Width, _currentVideo.Height, false, SurfaceFormat.Bgr32);
            
            retTex.SetData(texData);
            
            return retTex;
        }

        private void PlatformGetState(ref MediaState result)
        {
            if (_clock != null)
            {
                ClockState state;
                _clock.GetState(0, out state);

                switch (state)
                {
                    case ClockState.Running:
                        result = MediaState.Playing;
                        return;

                    case ClockState.Paused:
                        result = MediaState.Paused;
                        return;
                }
            }

            result = MediaState.Stopped;
        }

        private void PlatformPause()
        {
            _session.Pause();
        }

        private void PlatformPlay()
        {
            // Cleanup the last song first.
            if (State != MediaState.Stopped)
            {
                _session.Stop();
                _volumeController.Dispose();
                _clock.Dispose();
            }

            // Set the new song.
            _session.SetTopology(0, _currentVideo.Topology);

            _volumeController = CppObject.FromPointer<SimpleAudioVolume>(GetVolumeObj(_session));
            _volumeController.Mute = IsMuted;
            _volumeController.MasterVolume = _volume;

            // Get the clock.
            _clock = _session.Clock.QueryInterface<PresentationClock>();

            //create the callback if it hasn't been created yet
            if (_callback == null)
            {
                _callback = new Callback();
                _session.BeginGetEvent(_callback, null);
            }

            // Start playing.
            var varStart = new Variant();
            _session.Start(null, varStart);
        }

        internal static IntPtr GetVolumeObj(MediaSession session)
        {
            // Get the volume interface - shared between MediaPlayer and VideoPlayer
            const int retries = 10;
            const int sleepTimeFactor = 50;

            var volumeObj = (IntPtr)0;

            //See https://github.com/mono/MonoGame/issues/2620
            //MediaFactory.GetService throws a SharpDX exception for unknown reasons. it appears retrying will solve the problem but there
            //is no specific number of times, nor pause that works. So we will retry N times with an increasing Sleep between each one
            //before finally throwing the error we saw in the first place.
            for (int i = 0; i < retries; i++)
            {
                try
                {
                    MediaFactory.GetService(session, MRPolicyVolumeService, SimpleAudioVolumeGuid, out volumeObj);
                    break;
                }
                catch (SharpDXException)
                {
                    if (i == retries - 1)
                    {
                        throw;
                    }
                    Debug.WriteLine("MediaFactory.GetService failed({0}) sleeping for {1} ms", i + 1, i*sleepTimeFactor);
                    Thread.Sleep(i*sleepTimeFactor); //Sleep for longer and longer times
                }
            }
            return volumeObj;
        }

        private void PlatformResume()
        {
            _session.Start(null, null);
        }

        private void PlatformStop()
        {
            _session.Stop();
        }

        private void PlatformSetVolume()
        {
            if (_volumeController == null)
                return;

            _volumeController.MasterVolume = _volume;
        }

        private void PlatformSetIsLooped()
        {
            throw new NotImplementedException();
        }

        private void PlatformSetIsMuted()
        {
            if (_volumeController == null)
                return;

            _volumeController.Mute = _isMuted;
        }

        private TimeSpan PlatformGetPlayPosition()
        {
            return TimeSpan.FromTicks(_clock.Time);
        }

        private void PlatformDispose(bool disposing)
        {
        }
    }
}
