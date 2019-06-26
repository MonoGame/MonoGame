// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace Microsoft.Xna.Framework.Media
{
    public sealed class Playlist : IDisposable
    {
        public TimeSpan Duration
        {
            get;
			internal set;
        }

        public string Name
        {
            get;
			internal set;
        }

		public void Dispose()
        {
        }

		
        /*public SongCollection Songs
        {
            get
            {
            }
        }*/
    }
}

