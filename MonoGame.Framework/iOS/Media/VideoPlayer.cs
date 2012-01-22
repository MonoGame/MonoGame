#region License
/*
Microsoft Public License (Ms-PL)
MonoGame - Copyright © 2009-2012 The MonoGame Team

All rights reserved.

This license governs use of the accompanying software. If you use the software,
you accept this license. If you do not accept the license, do not use the
software.

1. Definitions

The terms "reproduce," "reproduction," "derivative works," and "distribution"
have the same meaning here as under U.S. copyright law.

A "contribution" is the original software, or any additions or changes to the
software.

A "contributor" is any person that distributes its contribution under this
license.

"Licensed patents" are a contributor's patent claims that read directly on its
contribution.

2. Grant of Rights

(A) Copyright Grant- Subject to the terms of this license, including the
license conditions and limitations in section 3, each contributor grants you a
non-exclusive, worldwide, royalty-free copyright license to reproduce its
contribution, prepare derivative works of its contribution, and distribute its
contribution or any derivative works that you create.

(B) Patent Grant- Subject to the terms of this license, including the license
conditions and limitations in section 3, each contributor grants you a
non-exclusive, worldwide, royalty-free license under its licensed patents to
make, have made, use, sell, offer for sale, import, and/or otherwise dispose of
its contribution in the software or derivative works of the contribution in the
software.

3. Conditions and Limitations

(A) No Trademark License- This license does not grant you rights to use any
contributors' name, logo, or trademarks.

(B) If you bring a patent claim against any contributor over patents that you
claim are infringed by the software, your patent license from such contributor
to the software ends automatically.

(C) If you distribute any portion of the software, you must retain all
copyright, patent, trademark, and attribution notices that are present in the
software.

(D) If you distribute any portion of the software in source code form, you may
do so only under this license by including a complete copy of this license with
your distribution. If you distribute any portion of the software in compiled or
object code form, you may only do so under a license that complies with this
license.

(E) The software is licensed "as-is." You bear the risk of using it. The
contributors give no express warranties, guarantees or conditions. You may have
additional consumer rights under your local laws which this license cannot
change. To the extent permitted under your local laws, the contributors exclude
the implied warranties of merchantability, fitness for a particular purpose and
non-infringement.
*/
#endregion License

using System;
using System.Drawing;
using System.IO;

using Microsoft.Xna.Framework.Graphics;

using MonoTouch.AVFoundation;
using MonoTouch.CoreAnimation;
using MonoTouch.CoreVideo;
using MonoTouch.Foundation;

using ALL11 = OpenTK.Graphics.ES11.All;
using GL11 = OpenTK.Graphics.ES11.GL;

namespace Microsoft.Xna.Framework.Media
{
    public sealed partial class VideoPlayer : IDisposable
    {
        #region Fields

        private PlayHead _playHead;
        private int _lastProcessedFrame;

        private CADisplayLink _displayLink;

        private AVUrlAsset _asset;
        private AVAssetReader _reader;
        private AVAssetReaderTrackOutput _videoTrackOutput;

        private StreamingAudioPlayer _audioPlayer;

        private Texture2D _texture;

        #endregion Fields

        #region Construction/Destruction

        /// <summary>
        /// Initializes a new instance of the <see cref="VideoPlayer"/> class.
        /// </summary>
        public VideoPlayer()
        {
            _playHead = new PlayHead();
        }

        ~VideoPlayer()
        {
            Dispose(false);
        }

        #endregion Construction/Destruction

        #region Public Properties

        private bool _isDisposed;
        /// <summary>
        /// Gets a value indicating whether the video player is disposed.
        /// </summary>
        /// <value>
        /// <c>true</c> if the video player is disposed; otherwise, <c>false</c>.
        /// </value>
        public bool IsDisposed
        {
            get { return _isDisposed; }
        }

        private bool _isLooped;
        /// <summary>
        /// Gets or sets a value indicating whether the video player loops
        /// the video.
        /// </summary>
        /// <value>
        /// <c>true</c> if the video player is playing the video looped;
        /// otherwise, <c>false</c>.
        /// </value>
        public bool IsLooped
        {
            get { return _isLooped; }
            set { _isLooped = value; }
        }

        private bool _isMuted;
        /// <summary>
        /// Gets or sets a value indicating whether the video player is muted.
        /// </summary>
        /// <value>
        /// <c>true</c> if the video player is muted; otherwise, <c>false</c>.
        /// </value>
        public bool IsMuted
        {
            get { return _isMuted; }
            set
            {
                if (_isMuted != value)
                {
                    _isMuted = value;
                    PushVolumeToAudioPlayer();
                }
            }
        }

        /// <summary>
        /// Gets the play position of the currently playing video.
        /// </summary>
        /// <value>
        /// The play position of the currently playing video.
        /// </value>
        public TimeSpan PlayPosition
        {
            get { return _playHead.Position; }
        }

        /// <summary>
        /// Gets the playback state, <see cref="MediaState" />.
        /// </summary>
        /// <value>
        /// The playback state, <see cref="MediaState" />.
        /// </value>
        public MediaState State
        {
            get { return _playHead.State; }
        }

        private Video _video;
        /// <summary>
        /// Gets the currently playing <see cref="Video" />.
        /// </summary>
        /// <value>
        /// The currently playing <see cref="Video" />.
        /// </value>
        public Video Video
        {
            get { return _video; }
        }

        private float _volume = 1f;
        /// <summary>
        /// Gets or sets the video player volume.
        /// </summary>
        /// <value>
        /// The volume from 0.0f to 1.0f.  Volume of 0.0f means that 96dB are
        /// subtracted from the natural volume of the track.  Volume of 1.0f
        /// means that the track is played at full volume.  Values from 0.0f to
        /// 1.0f subtract from 96dB to 0dB, linearly.  (Note that the final
        /// result is logarithmic, since decibels are logarithmic.)
        /// </value>
        public float Volume
        {
            get { return _volume; }
            set
            {
                if (value < 0f)
                    value = 0f;
                else if (value > 1f)
                    value = 1f;

                if (_volume != value)
                {
                    _volume = value;
                    PushVolumeToAudioPlayer();
                }
            }
        }

        #endregion Public Properties

        #region Public Methods

        /// <summary>
        /// Gets a <see cref="Texture2D" /> representing the current frame of
        /// video being played.
        /// </summary>
        /// <returns>
        /// The current frame of video.
        /// </returns>
        public Texture2D GetTexture()
        {
            return _texture;
        }

        /// <summary>
        /// Pauses the video player.
        /// </summary>
        public void Pause()
        {
            _playHead.Pause();
            if (_audioPlayer != null)
                _audioPlayer.Pause();
        }

        /// <summary>
        /// Play the specified video.
        /// </summary>
        /// <param name='video'>
        /// The <see cref="Video" /> to play.
        /// </param>
        /// <exception cref='ArgumentNullException'>
        /// video is <see langword="null" /> .
        /// </exception>
        public void Play(Video video)
        {
            if (video == null)
                throw new ArgumentNullException("video");

            if (State != MediaState.Stopped)
                Stop();

            _video = video;
            Play();
        }

        /// <summary>
        /// Resumes playback after <see cref="Pause" />.
        /// </summary>
        public void Resume()
        {
            _playHead.Resume();
            if (_audioPlayer != null)
                _audioPlayer.Resume();
        }

        /// <summary>
        /// Stop the video player.
        /// </summary>
        public void Stop()
        {
            if (_reader == null)
                return;

            DestroyDisplayLink();
            DestroyAudioPlayer();
            DestroyAVObjects();
            DestroyTexture();

            _playHead.Stop();
        }

        #endregion Public Methods

        private void Play()
        {
            var url = NSUrl.FromFilename(Path.GetFullPath(Video.FileName));
            _asset = new AVUrlAsset(url, null);
            // TODO: Turn IntPtr.Zero into a real NSError, however that works.
            _reader = new AVAssetReader(_asset, IntPtr.Zero);

            InitializeVideoTrackOutput();
            InitializeAudioPlayer();
            InitializeTextures();

            bool result = _reader.StartReading();
            if (!result)
                // TODO: Is InvalidOperationException the right exception here?
                throw new InvalidOperationException("Could not start video playback.");

            if (_audioPlayer != null)
                _audioPlayer.Play();

            _displayLink = CADisplayLink.Create(displayLink_Update);
            _displayLink.AddToRunLoop(NSRunLoop.Main, NSRunLoop.NSDefaultRunLoopMode);

            _playHead.Play(Video.FramesPerSecond);

            _lastProcessedFrame = -1;
            SyncToCurrentFrame();
        }

        private void InitializeVideoTrackOutput()
        {
            var videoTracks = _asset.TracksWithMediaType(AVMediaType.Video);
            if (videoTracks.Length == 0)
                // TODO: Is InvalidOperationException the right exception here?
                throw new InvalidOperationException("Media file does not contain a video track.");

            var videoTrack = videoTracks[0];

            var videoTrackOutputSettings = NSDictionary.FromObjectAndKey(
                NSNumber.FromUInt32((uint)CVPixelFormatType.CV32BGRA),
                CVPixelBuffer.PixelFormatTypeKey);
            _videoTrackOutput = new AVAssetReaderTrackOutput(videoTrack, videoTrackOutputSettings);
            _reader.AddOutput(_videoTrackOutput);
        }

        private void InitializeAudioPlayer()
        {
            var audioTracks = _asset.TracksWithMediaType(AVMediaType.Audio);
            if (audioTracks.Length > 0)
            {
                _audioPlayer = new StreamingAudioPlayer(_reader, audioTracks);
                _audioPlayer.Volume = Volume;
            }
        }

        private void InitializeTextures()
        {
            _texture = new Texture2D(new ESImage(_video.Width, _video.Height));
        }

        private void displayLink_Update()
        {
            if (State != MediaState.Playing)
                return;
            SyncToCurrentFrame();
        }

        private void SyncToCurrentFrame()
        {
            int frameDelta = _playHead.CurrentFrameNumber - _lastProcessedFrame;
            if (frameDelta > 1)
            {
                var framesToDrop = frameDelta - 1;
                Console.WriteLine("dropping {0} frame(s)", framesToDrop);
                SkipFrames(framesToDrop);
            }

            if (frameDelta > 0 && !CheckForEndOfTrack())
                ReadNextFrameToTexture();

            _lastProcessedFrame += frameDelta;
        }

        private void PushVolumeToAudioPlayer()
        {
            if (_audioPlayer == null)
                return;

            _audioPlayer.Volume = _isMuted ? 0 : _volume;
        }

        private void SkipFrames(int count)
        {
            while (count > 0 && !CheckForEndOfTrack())
            {
                using (var sampleBuffer = _videoTrackOutput.CopyNextSampleBuffer())
                {
                    sampleBuffer.Invalidate();
                }
                count--;
            }
        }

        private void ReadNextFrameToTexture()
        {
            using (var sampleBuffer = _videoTrackOutput.CopyNextSampleBuffer())
            {
                var pixelBuffer = (CVPixelBuffer)sampleBuffer.GetImageBuffer();

                // After the last frame, pixelBuffer will be null, so leave the
                // texture alone and catch the stop condition next time around.
                // This results in the last frame being shown for two frame-
                // lengths, which is not ideal.  But it will require more study
                // to ensure the last frame is only shown once.
                if (pixelBuffer == null)
                    return;

                pixelBuffer.Lock(CVOptionFlags.None);
                try
                {
                    // TODO: Is there some way to get the right pixel format
                    //       without having to swizzle manually?  CoreVideo only
                    //       seems to produce BGRA on iOS and OpenGL only seems
                    //       to accept RGBA.
                    SwizzleBGRAToRGBA(pixelBuffer);

                    // Clear whatever error currently exists.
                    var error = GL11.GetError();

                    GL11.BindTexture(ALL11.Texture2D, _texture.ID);
                    error = GL11.GetError();
                    if (error != ALL11.NoError)
                        throw new InvalidOperationException("Binding video texture failed: " + error);

                    GL11.TexSubImage2D(
                        ALL11.Texture2D, 0, 0, 0,
                        _texture.Width, _texture.Height,
                        ALL11.Rgba, ALL11.UnsignedByte, pixelBuffer.BaseAddress);
                    error = GL11.GetError();
                    if (error != ALL11.NoError)
                        throw new InvalidOperationException("Updating video texture failed: " + error);
                }
                finally
                {
                    pixelBuffer.Unlock(CVOptionFlags.None);
                    sampleBuffer.Invalidate();
                }
            }
        }

        private bool CheckForEndOfTrack()
        {
            switch (_reader.Status)
            {
            case AVAssetReaderStatus.Cancelled:
            case AVAssetReaderStatus.Failed:
                Stop();
                return true;
            case AVAssetReaderStatus.Completed:
                Stop();
                if (IsLooped)
                    Play();
                return true;
            case AVAssetReaderStatus.Reading:
                return false;
            default:
                throw new NotImplementedException("Unhandled AVAssetReaderStatus: " + _reader.Status);
            }
        }

        private static unsafe void SwizzleBGRAToRGBA(CVPixelBuffer pixelBuffer)
        {
            byte *pRow = (byte *)pixelBuffer.BaseAddress;
            byte *pStopRow = pRow + (pixelBuffer.Height * pixelBuffer.BytesPerRow);
            while (pRow < pStopRow)
            {
                int *pPixel = (int *)pRow;
                int *pStopPixel = pPixel + pixelBuffer.Width;
                while (pPixel < pStopPixel)
                {
                    int a = 0xff & (*pPixel >> 24);
                    int r = 0xff & (*pPixel >> 16);
                    int g = 0xff & (*pPixel >> 8);
                    int b = 0xff & *pPixel;

                    *pPixel++ = (a << 24) | (b << 16) | (g << 8) | r;
                }
                pRow += pixelBuffer.BytesPerRow;
            }
        }

        #region IDisposable Members

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                DestroyDisplayLink();
                DestroyAudioPlayer();
                DestroyAVObjects();
                DestroyTexture();
            }

            _playHead.Stop();
            _isDisposed = true;
        }

        #endregion IDisposable Members

        private void DestroyAudioPlayer()
        {
            if (_audioPlayer != null)
            {
                _audioPlayer.Dispose();
                _audioPlayer = null;
            }
        }

        private void DestroyAVObjects()
        {
            if (_videoTrackOutput != null)
            {
                _videoTrackOutput.Dispose();
                _videoTrackOutput = null;
            }

            if (_reader != null)
            {
                _reader.Dispose();
                _reader = null;
            }

            if (_asset != null)
            {
                _asset.Dispose();
                _asset = null;
            }
        }

        private void DestroyDisplayLink()
        {
            if (_displayLink != null)
            {
                _displayLink.Dispose();
                _displayLink = null;
            }
        }

        private void DestroyTexture()
        {
            if (_texture != null)
            {
                _texture.Dispose();
                _texture = null;
            }
        }

        private class PlayHead
        {
            private readonly double _hostClockFrequency;
            private MediaState _state;
            private long _referenceHostTime;
            private float _framesPerSecond;

            public PlayHead()
            {
                _hostClockFrequency = CVTime.GetHostClockFrequency();
                _state = MediaState.Stopped;
            }

            public MediaState State
            {
                get { return _state; }
            }

            private TimeSpan _position;
            public TimeSpan Position
            {
                get
                {
                    PerformTimeUpdate();
                    return _position;
                }
            }

            private int _currentFrameNumber;
            public int CurrentFrameNumber
            {
                get
                {
                    PerformTimeUpdate();
                    return _currentFrameNumber;
                }
            }

            public void Play(float framesPerSecond)
            {
                if (framesPerSecond <= 0f)
                    throw new ArgumentOutOfRangeException(
                        "framesPerSecond", "framesPerSecond must be positive");
                _framesPerSecond = framesPerSecond;

                _referenceHostTime = CVTime.GetCurrentHostTime();
                _state = MediaState.Playing;
            }

            public void Stop()
            {
                _state = MediaState.Stopped;
                _currentFrameNumber = 0;
                _position = TimeSpan.Zero;
            }

            public void Pause()
            {
                _state = MediaState.Paused;
            }

            public void Resume()
            {
                _referenceHostTime = CVTime.GetCurrentHostTime();
                _state = MediaState.Playing;
            }

            private void PerformTimeUpdate()
            {
                if (_state != MediaState.Playing)
                    return;

                var currentHostTime = CVTime.GetCurrentHostTime();

                long delta = currentHostTime - _referenceHostTime;

                long wholeTicks = (long)(delta * TimeSpan.TicksPerSecond / _hostClockFrequency);
                long remainder = delta - (long)(wholeTicks * _hostClockFrequency / TimeSpan.TicksPerSecond);

                _position += TimeSpan.FromTicks(wholeTicks);
                _currentFrameNumber = (int)(_position.Ticks * _framesPerSecond / TimeSpan.TicksPerSecond);

                _referenceHostTime = currentHostTime - remainder;
            }
        }
    }
}
