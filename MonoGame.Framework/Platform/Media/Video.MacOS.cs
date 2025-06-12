// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;

using ObjCRuntime;
using Foundation;
using AVFoundation;

namespace Microsoft.Xna.Framework.Media
{
    /// <summary>
    /// Represents a video.
    /// </summary>
    public sealed partial class Video : IDisposable
    {
        AVPlayerItem movie;

        internal AVPlayer Player { get; private set; }

        internal float Volume
        {
            get { return Player.Volume; }
            set
            {
                // TODO When Xamarain fix the set Volume mMovie.Volume = value;
            }
        }

        internal TimeSpan CurrentPosition
        {
            get { return new TimeSpan(movie.CurrentTime.Value); }
        }

        private void PlatformInitialize()
        {
            var err = new NSError();

            movie = AVPlayerItem.FromUrl(NSUrl.FromFilename(FileName));
            Player = new AVPlayer(movie);
        }

        private void PlatformDispose(bool disposing)
        {
            if (Player != null)
            {
                Player.Dispose();
                Player = null;
            }

            if (movie != null)
            {
                movie.Dispose();
                movie = null;
            }
        }
    }
}
