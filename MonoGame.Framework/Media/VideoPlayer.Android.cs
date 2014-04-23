// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Android.Widget;
using Microsoft.Xna.Framework.Graphics;

namespace Microsoft.Xna.Framework.Media
{
    public sealed partial class VideoPlayer : IDisposable
    {
        private Game _game;

        private void PlatformInitialize()
        {
            _game = Game.Instance;
        }

        private Texture2D PlatformGetTexture()
        {
            throw new NotImplementedException();
        }

        private void PlatformPause()
        {
            _currentVideo.Player.Pause();
        }

        private void PlatformResume()
        {
            _currentVideo.Player.Start();
        }

        private void PlatformPlay()
        {
            _currentVideo.Player.SetDisplay(((AndroidGameWindow)_game.Window).GameView.Holder);
            _currentVideo.Player.Start();
            
            AndroidGamePlatform.IsPlayingVdeo = true;
        }

        private void PlatformStop()
        {
            _currentVideo.Player.Stop();

            AndroidGamePlatform.IsPlayingVdeo = false;
            _currentVideo.Player.SetDisplay(null);
        }

        private TimeSpan PlatformGetPlayPosition()
        {
            throw new NotImplementedException();
        }

        private TimeSpan PlatformSetVolume()
        {
            throw new NotImplementedException();
        }
    }
}