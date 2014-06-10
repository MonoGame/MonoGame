// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Linq;
using Microsoft.Xna.Framework.Media;

namespace Microsoft.Xna.Framework.Content.Pipeline
{
    /// <summary>
    /// Provides a base class for all video objects.
    /// </summary>
    public class VideoContent : ContentItem, IDisposable
    {
        private readonly string[] _splitComma = new[] { ", " };
        private readonly string[] _splitColon = new[] { ": " };

        bool disposed;
        int bitsPerSecond;
        TimeSpan duration;
        float framesPerSecond;
        int height;
        int width;

        /// <summary>
        /// Gets the bit rate for this video.
        /// </summary>
        public int BitsPerSecond { get { return bitsPerSecond; } }

        /// <summary>
        /// Gets the duration of this video.
        /// </summary>
        public TimeSpan Duration { get { return duration; } }

        /// <summary>
        /// Gets or sets the file name for this video.
        /// </summary>
        [ContentSerializerAttribute]
        public string Filename { get; set; }

        /// <summary>
        /// Gets the frame rate for this video.
        /// </summary>
        public float FramesPerSecond { get { return framesPerSecond; } }

        /// <summary>
        /// Gets the height of this video.
        /// </summary>
        public int Height { get { return height; } }

        /// <summary>
        /// Gets or sets the type of soundtrack accompanying the video.
        /// </summary>
        [ContentSerializerAttribute]
        public VideoSoundtrackType VideoSoundtrackType { get; set; }

        /// <summary>
        /// Gets the width of this video.
        /// </summary>
        public int Width { get { return width; } }

        /// <summary>
        /// Initializes a new copy of the VideoContent class for the specified video file.
        /// </summary>
        /// <param name="filename">The file name of the video to import.</param>
        public VideoContent(
            string filename
            )
        {
            Filename = filename;

            string stdout, stderr;
            var result = ExternalTool.Run("ffprobe.exe", string.Format("-i \"{0}\"", Filename), out stdout, out stderr);
            var lines = stderr.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

            // Parse duration and bitrate
            var durationProps = lines.FirstOrDefault(l => l.Trim().StartsWith("Duration"))
                                        .Trim()
                                        .Split(_splitComma, StringSplitOptions.RemoveEmptyEntries);

            foreach (var prop in durationProps)
            {
                var keyVal = prop.Split(_splitColon, StringSplitOptions.RemoveEmptyEntries);
                if (keyVal[0] == "Duration")
                {
                    duration = TimeSpan.Parse(keyVal[1]);
                }
                else if (keyVal[0] == "bitrate")
                {
                    var valUnit = keyVal[1].Split();
                    bitsPerSecond = int.Parse(valUnit[0]);
                    if (valUnit[1] == "kb/s")
                        bitsPerSecond *= 1024;
                    else
                        throw new Exception("Unknown bitrate unit suffix");
                }
            }

            // Parse resolution and frame rate
            var streamLines = lines.Where(l => l.Trim().StartsWith("Stream #")).Select(l => l.Trim());
            foreach (var streamLine in streamLines)
            {
                var props = streamLine.Split(_splitComma, StringSplitOptions.RemoveEmptyEntries);
                if (props[0].Contains("Video"))
                {
                    var res = props[2].Split()[0].Split('x');
                    width = Int32.Parse(res[0]);
                    height = Int32.Parse(res[1]);

                    var fps = props[4].Split();
                    framesPerSecond = float.Parse(fps[0]);
                }
            }
        }

        ~VideoContent()
        {
            Dispose(false);
        }

        /// <summary>
        /// Immediately releases the unmanaged resources used by this object.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    // TODO: Free managed resources here
                    // ...
                }
                // TODO: Free unmanaged resources here
                // ...
                disposed = true;
            }
        }
    }
}
