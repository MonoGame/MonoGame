// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace Microsoft.Xna.Framework.Audio
{
    /// <summary>Controls how Cue objects should stop when Stop is called.</summary>
	public enum AudioStopOptions
	{
        /// <summary>Indicates the cue should stop normally, playing any release phase or transition specified in the content.</summary>
		AsAuthored,
        /// <summary>Indicates the cue should stop immediately, ignoring any release phase or transition specified in the content.</summary>
		Immediate
	}
}

