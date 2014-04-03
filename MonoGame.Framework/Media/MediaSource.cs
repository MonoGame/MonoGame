// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
#if IOS
using MonoTouch.UIKit;
#endif

namespace Microsoft.Xna.Framework.Media
{
	public sealed class MediaSource
    {
		private MediaSourceType _type;
		private string _name;
		internal MediaSource (string name, MediaSourceType type)
		{
			_name = name;
			_type = type;
		}
				
        public Microsoft.Xna.Framework.Media.MediaSourceType MediaSourceType
        {
            get
            {
				return _type;
            }
        }

        public string Name
        {
            get
            {
				return _name;
            }
        }
	
		public static IList<MediaSource> GetAvailableMediaSources()
        {
#if IOS
			MediaSource[] result = { new MediaSource(UIDevice.CurrentDevice.SystemName, MediaSourceType.LocalDevice) };
#else
			MediaSource[] result = { new MediaSource("DummpMediaSource", MediaSourceType.LocalDevice) };
#endif
			return result;
        }
    }
}
