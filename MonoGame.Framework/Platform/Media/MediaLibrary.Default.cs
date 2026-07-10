// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace Microsoft.Xna.Framework.Media
{
    public partial class MediaLibrary
    {
        private void PlatformLoad(Action<int> progressCallback)
        {
            
        }

        private AlbumCollection PlatformGetAlbums()
        {
            return null;
        }

        private SongCollection PlatformGetSongs()
        {
            return null;
        }

        private void PlatformDispose()
        {
            
        }
    }
}