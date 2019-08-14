// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;
using System.Linq;

namespace Microsoft.Xna.Framework.Media
{
    /// <summary>
    /// Represents a video.
    /// </summary>
    public sealed partial class Video : IDisposable
    {
        internal Android.Media.MediaPlayer Player;

        partial void PlatformInitialize()
        {
            Player = new Android.Media.MediaPlayer();
            if (Player != null)
            {
                var afd = Game.Activity.Assets.OpenFd(FileName);
                if (afd != null)
                {
                    Player.SetDataSource(afd.FileDescriptor, afd.StartOffset, afd.Length);
                    Player.Prepare();
                }
            }
        }

        partial void PlatformDispose(bool disposing)
        {
            if (Player == null)
                return;

            Player.Dispose();
            Player = null;
        }
    }
}
