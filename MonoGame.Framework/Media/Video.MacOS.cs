// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;

#if PLATFORM_MACOS_LEGACY
using MonoMac.ObjCRuntime;
using MonoMac.QTKit;
using MonoMac.Foundation;
#else
using ObjCRuntime;
using QTKit;
using Foundation;
#endif

namespace Microsoft.Xna.Framework.Media
{
    /// <summary>
    /// Represents a video.
    /// </summary>
    public sealed partial class Video : IDisposable
    {
        private QTMovie _mMovie;
        internal QTMovieView MovieView { get; private set; }

        internal float Volume
        {
            get { return _mMovie.Volume; }
            set
            {
                // TODO When Xamarain fix the set Volume mMovie.Volume = value;
            }
        }

        internal TimeSpan CurrentPosition
        {
            get { return new TimeSpan(_mMovie.CurrentTime.TimeValue); }
        }

        private void PlatformInitialize()
        {
            var err = new NSError();

            _mMovie = new QTMovie(FileName, out err);
            if (_mMovie != null)
            {
                MovieView = new QTMovieView();
                MovieView.Movie = _mMovie;

                MovieView.IsControllerVisible = false;
            }
            else
                Console.WriteLine(err);
        }

        private void PlatformDispose(bool disposing)
        {
            if (MovieView != null)
            {
                MovieView.Dispose();
                MovieView = null;
            }

            if (_mMovie != null)
            {
                _mMovie.Dispose();
                _mMovie = null;
            }
        }
    }
}
