// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

extern alias MicrosoftXnaFramework;
using System.IO;
using MsMediaLibrary = MicrosoftXnaFramework::Microsoft.Xna.Framework.Media.MediaLibrary;

using System;
using Microsoft.Xna.Framework.Media.PhoneExtensions;

namespace Microsoft.Xna.Framework.Media
{
    public partial class MediaLibrary : IDisposable
    {
        private MsMediaLibrary mediaLibrary;

        private void PlatformLoad(Action<int> progressCallback)
        {
            this.mediaLibrary = new MsMediaLibrary();
        }

        private AlbumCollection PlatformGetAlbums()
        {
            if (this.mediaLibrary != null)
                return this.mediaLibrary.Albums;

            return null;
        }

        private SongCollection PlatformGetSongs()
        {
            if (this.mediaLibrary != null)
                return new SongCollection(this.mediaLibrary.Songs);

            return null;
        }

        private void PlatformDispose()
        {
            this.mediaLibrary.Dispose();
        }

        public string SavePicturePath(string name, Stream stream)
        {
            return this.mediaLibrary.SavePicture(name, stream).GetPath();
        }
    }
}
