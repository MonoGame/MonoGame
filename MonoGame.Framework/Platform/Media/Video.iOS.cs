// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;

using MediaPlayer;
using Foundation;
using Microsoft.Xna.Framework.Graphics;

namespace Microsoft.Xna.Framework.Media
{
    /// <summary>
    /// Represents a video.
    /// </summary>
    public sealed partial class Video : IDisposable
    {
        internal MPMoviePlayerViewController MovieView { get; private set; }

        /*
        // NOTE: https://developer.apple.com/library/ios/documentation/MediaPlayer/Reference/MPMoviePlayerController_Class/Reference/Reference.html
        // It looks like BackgroundColor doesn't even exist anymore
        // in recent versions of iOS... Why still have this?
        public Color BackgroundColor
        {
            get
            {
                var col = MovieView.MoviePlayer.BackgroundColor;
                return new Color(col.X, col.Y, col.Z, col.W);
            }

            set
            {
                var col = value.ToVector4();
                return MovieView.MoviePlayer.BackgroundColor = UIKit.UIColor(col.X, col.Y, col.Z, col.W);
            }
        }
        */

        private void PlatformInitialize()
        {
            var url = NSUrl.FromFilename(Path.GetFullPath(FileName));

            MovieView = new MPMoviePlayerViewController(url);
            MovieView.MoviePlayer.ScalingMode = MPMovieScalingMode.AspectFill;
            MovieView.MoviePlayer.ControlStyle = MPMovieControlStyle.None;
            MovieView.MoviePlayer.PrepareToPlay();
        }

        private void PlatformDispose(bool disposing)
        {
            if (MovieView == null)
                return;
            
            MovieView.Dispose();
            MovieView = null;
        }
    }
}
