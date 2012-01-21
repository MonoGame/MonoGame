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
using System.IO;

using MonoTouch.AVFoundation;
using MonoTouch.Foundation;

namespace Microsoft.Xna.Framework.Media
{
    public sealed class Video
    {
        internal Video(string fileName)
        {
            _fileName = fileName;
            QueryProperties();
        }

        private TimeSpan _duration;
        /// <summary>
        /// Gets the duration of the <see cref="Video" />.
        /// </summary>
        /// <value>
        /// The duration of the <see cref="Video" />.
        /// </value>
        public TimeSpan Duration
        {
            get { return _duration; }
        }

        private float _framesPerSecond;
        /// <summary>
        /// Gets the rate of the <see cref="Video" />.
        /// </summary>
        /// <value>
        /// The rate of the <see cref="Video" /> in frames per second.
        /// </value>
        public float FramesPerSecond
        {
            get { return _framesPerSecond; }
        }

        private int _width;
        /// <summary>
        /// Gets the width of the <see cref="Video" /> in pixels.
        /// </summary>
        /// <value>
        /// The width of the <see cref="Video" /> in pixels.
        /// </value>
        public int Width
        {
            get { return _width; }
        }

        private int _height;
        /// <summary>
        /// Gets the height of the <see cref="Video" /> in pixels.
        /// </summary>
        /// <value>
        /// The height of the <see cref="Video" /> in pixels.
        /// </value>
        public int Height
        {
            get { return _height; }
        }

        private VideoSoundTrackType _videoSoundTrackType;
        /// <summary>
        /// Gets the VideoSoundTrackType of the <see cref="Video" />.
        /// </summary>
        /// <value>
        /// The type of soundtrack in the video.
        /// </value>
        public VideoSoundTrackType VideoSoundTrackType
        {
            get { return _videoSoundTrackType; }
        }

        // FIXME: Internal properties are not ideal.  There may be no other
        //        choice for a sealed class.  Except a partial class.  Still,
        //        the members introduced in the partial class would need
        //        internal visibility.
        private string _fileName;
        internal string FileName
        {
            get { return _fileName; }
        }

        internal static string Normalize(string fileName)
        {
            if (File.Exists(fileName))
                return fileName;

            // Check the file extension
            if (!string.IsNullOrEmpty(Path.GetExtension(fileName)))
                return null;

            // Concat the file name with valid extensions
            if (File.Exists(fileName+".mp4"))
                return fileName+".mp4";
            if (File.Exists(fileName+".mov"))
                return fileName+".mov";
            if (File.Exists(fileName+".avi"))
                return fileName+".avi";
            if (File.Exists(fileName+".m4v"))
                return fileName+".m4v";
            if (File.Exists(fileName+".3gp"))
                return fileName+".3gp";

            return null;
        }

        private void QueryProperties()
        {
            var url = NSUrl.FromFilename(Path.GetFullPath(_fileName));

            using (var asset = new AVUrlAsset(url, null))
            {
                var videoTracks = asset.TracksWithMediaType(AVMediaType.Video);

                if (videoTracks.Length == 0)
                    // TODO: Is InvalidOperationException the right type of exception here?
                    throw new InvalidOperationException("Media file does not contain a video track.");

                var videoTrack = videoTracks[0];

                var duration = videoTrack.TimeRange.Duration;
                _duration = TimeSpan.FromSeconds((double)duration.Value / (double)duration.TimeScale);

                _framesPerSecond = videoTrack.NominalFrameRate;

                _width = (int)videoTrack.NaturalSize.Width;
                _height = (int)videoTrack.NaturalSize.Height;

                // TODO: Figure out how to set VideoSoundTrackType correctly.
                _videoSoundTrackType = VideoSoundTrackType.MusicAndDialog;
            }
        }
    }
}
