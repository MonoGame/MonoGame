using System.Diagnostics;
using System.Threading;
using Microsoft.Xna.Framework.Graphics;
using SharpDX;
using SharpDX.MediaFoundation;
using SharpDX.Win32;
using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Microsoft.Xna.Framework.Media
{
    public sealed partial class VideoPlayer : IDisposable
    {
        // HACK: Need SharpDX to fix this.
        internal static Guid AudioStreamVolumeGuid;

        private static Texture2D _retTexture;

        private void PlatformInitialize()
        {
            // The GUID is specified in a GuidAttribute attached to the class
            AudioStreamVolumeGuid = Guid.Parse(((GuidAttribute)typeof(AudioStreamVolume).GetCustomAttributes(typeof(GuidAttribute), false)[0]).Value);
        }

        private Texture2D PlatformGetTexture()
        {
            if (_currentVideo == null)
            {
                return null;
            }

            var sampleGrabber = _currentVideo.SampleGrabber;

            var texData = sampleGrabber.TextureData;

            if (texData == null)
                return null;

            // TODO: This could likely be optimized if we held on to the SharpDX Surface/Texture data,
            // and set it on an XNA one rather than constructing a new one every time this is called.
            if (_retTexture == null || _retTexture.IsDisposed || _retTexture.Width != _currentVideo.Width ||
                _retTexture.Height != _currentVideo.Height)
            {
                if (_retTexture != null && !_retTexture.IsDisposed)
                {
                    _retTexture.Dispose();
                }

                _retTexture = new Texture2D(Game.Instance.GraphicsDevice, _currentVideo.Width, _currentVideo.Height, false, SurfaceFormat.Bgr32);

            }

            _retTexture.SetData(texData);

            return _retTexture;
        }

        private void PlatformGetState(ref MediaState result)
        {
            if (_currentVideo == null)
            {
                result = MediaState.Stopped;
                return;
            }

            _currentVideo.PlatformGetState(out result);
        }

        private void PlatformPause()
        {
            _currentVideo.PlatformPause();
        }

        private void PlatformPlay()
        {
            _currentVideo.SetVolume(_isMuted, _volume);
            _currentVideo.PlatformPlay();
        }

        private void PlatformResume()
        {
            _currentVideo.PlatformResume();
        }

        private void PlatformStop()
        {
            _currentVideo.PlatformStop();
        }

        private void PlatformSetVolume()
        {
            _currentVideo.SetVolume(_isMuted, _volume);
        }

        private void PlatformSetIsLooped()
        {
            throw new NotImplementedException();
        }

        private void PlatformSetIsMuted()
        {
            _currentVideo.SetVolume(_isMuted, _volume);
        }

        private TimeSpan PlatformGetPlayPosition()
        {
            if (State == MediaState.Stopped)
                return TimeSpan.Zero;

            if (_currentVideo == null)
                return TimeSpan.Zero;
            
            return _currentVideo.PlatformGetPlayPosition();
        }

        private void PlatformDispose(bool disposing)
        {
            
        }
    }
}
