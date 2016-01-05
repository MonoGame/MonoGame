// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
#if PLATFORM_MACOS_LEGACY
using MonoMac.Foundation;
using MonoMac.QTKit;
using RectF = System.Drawing.RectangleF;
#else
using Foundation;
using QTKit;
using RectF = CoreGraphics.CGRect;
#endif
using Microsoft.Xna.Framework.Graphics;

namespace Microsoft.Xna.Framework.Media
{
    public sealed partial class VideoPlayer : IDisposable
    {
        private Game _game;
        private MacGamePlatform _platform;

        // TODO Needed to bind OpenGL to Quicktime private QTVisualContextRef  textureContext;
        // TODO Needed to grab frame as a texture private CVOpenGLTextureRef  currentFrame;

        private void PlatformInitialize()
        {
            _game = Game.Instance;
            _platform = (MacGamePlatform)_game.Services.GetService(typeof(MacGamePlatform));
        }

        private Texture2D PlatformGetTexture()
        {
            throw new NotImplementedException();
        }

        private void PlatformGetState(ref MediaState result)
        {
        }

        private void PlatformPause()
        {
            _currentVideo.MovieView.Pause(new NSObject());
        }

        private void PlatformResume()
        {
            _currentVideo.Volume = _volume;
            _currentVideo.MovieView.Play(new NSObject());
        }

        private void PlatformPlay()
        {
            //_currentVideo.MovieView.SetFrameOrigin(new System.Drawing.PointF(0,0));
            //_currentVideo.MovieView.SetFrameSize(new System.Drawing.SizeF(_game.GraphicsDevice.PresentationParameters.BackBufferWidth,_game.GraphicsDevice.PresentationParameters.BackBufferHeight));
            _currentVideo.MovieView.Frame = new RectF(0, 0, _game.GraphicsDevice.PresentationParameters.BackBufferWidth, _game.GraphicsDevice.PresentationParameters.BackBufferHeight);

            /*  A primitive way of launching the media player
             * MonoMac.AppKit.NSWorkspace workspace = MonoMac.AppKit.NSWorkspace.SharedWorkspace; 
            workspace.OpenUrls(new[]{NSUrl.FromString(_currentVideo.FileName)},
                                 @"com.apple.quicktimeplayer", 
                                 MonoMac.AppKit.NSWorkspaceLaunchOptions.Async,
                                 new NSAppleEventDescriptor(),
                                 new[]{""}); */

            // TODO when Xamarin implement the relevant functions var theError = QTOpenGLTextureContextCreate( null, null, _game.Window.PixelFormat, _game.Window.OpenGLContext, out textureContext);

            _game.Window.AddSubview(_currentVideo.MovieView);

            NSNotificationCenter.DefaultCenter.AddObserver(new NSString("QTMovieDidEndNotification"), (notification) =>
            {
                Stop();

                if (IsLooped)
                    PlatformPlay();

            });

            _currentVideo.Volume = _volume;
            _currentVideo.MovieView.Play(new NSObject());

            // FIXME: I'm not crazy about keeping track of IsPlayingVideo in MacGamePlatform, but where else can
            //        this concept be expressed?
            _platform.IsPlayingVideo = true;
        }

        private void PlatformStop()
        {
            var movieView = _currentVideo.MovieView;
            var o = new NSObject();
            movieView.Pause(o);
            movieView.GotoBeginning(o);
            
            // FIXME: I'm not crazy about keeping track of IsPlayingVideo in MacGamePlatform, but where else can
            //        this concept be expressed?
            _platform.IsPlayingVideo = false;
            movieView.RemoveFromSuperview();
        }

        private void PlatformSetVolume()
        {
            _currentVideo.Volume = _volume;
        }

        private void PlatformSetIsLooped()
        {
            throw new NotImplementedException();
        }

        private void PlatformSetIsMuted()
        {
            throw new NotImplementedException();
        }

        private TimeSpan PlatformGetPlayPosition()
        {
            return _currentVideo.CurrentPosition;
        }

        private void PlatformDispose(bool disposing)
        {
        }
    }
}