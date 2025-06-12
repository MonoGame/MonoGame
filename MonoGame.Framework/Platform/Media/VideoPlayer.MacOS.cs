// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Foundation;
using AVFoundation;
using RectF = CoreGraphics.CGRect;
using Microsoft.Xna.Framework.Graphics;
using AppKit;
using CoreAnimation;

namespace Microsoft.Xna.Framework.Media
{
    public sealed partial class VideoPlayer : IDisposable
    {
        private Game _game;
        NSSDLWindow nsWindow;
        AVPlayerLayer layer;
        NSView view;

        private void PlatformInitialize()
        {
            _game = Game.Instance;
            Sdl.Window.SDL_SysWMinfo sys = new Sdl.Window.SDL_SysWMinfo();
            Sdl.Window.GetWindowWMInfo(_game.Window.Handle, ref sys);
            nsWindow = new NSSDLWindow(sys.window);
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
            _currentVideo.Player.Pause();
        }

        private void PlatformResume()
        {
            _currentVideo.Volume = _volume;
            _currentVideo.Player.Play();
        }

        private void PlatformPlay()
        {
            layer = AVPlayerLayer.FromPlayer(_currentVideo.Player);
            view = new NSView(nsWindow.ContentView.Frame);
            view.WantsLayer = true;
            view.Layer = layer;
            layer.Frame = nsWindow.ContentView.Bounds;
            nsWindow.ContentView.AddSubview(view);
           
            NSNotificationCenter.DefaultCenter.AddObserver(AVPlayerItem.DidPlayToEndTimeNotification,
                                                               notification =>
            {
                Stop();

                if (IsLooped)
                    PlatformPlay();

            });

            _currentVideo.Volume = _volume;
            _currentVideo.Player.Play();

        }

        private void PlatformStop()
        {
            var movieView = _currentVideo.Player;
            movieView.Pause();
            movieView.Seek(CoreMedia.CMTime.Zero);

            nsWindow.ContentView.WillRemoveSubview(view);
            view.RemoveFromSuperview();
            view.Dispose();
            view = null;
            layer.Dispose();
            layer = null;
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

    internal class NSSDLWindow : AppKit.NSWindow
    {
        public NSSDLWindow(IntPtr handle) : base(handle)
        {
        }
    }
}