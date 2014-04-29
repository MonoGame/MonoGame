// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace Microsoft.Xna.Framework.Audio
{
    /// <summary>
    /// The exception that is thrown when there is an attempt to play more than the platform specific maximum SoundEffectInstance sounds concurrently.
    /// </summary>
    /// <remarks>
    /// Each platform has limits on the number of sounds playing simultaneously. InstancePlayLimitException is thrown if this limit is exceeded. Paused or stopped SoundEffectInstance objects do not count against this limit.
    /// </remarks>
    [DataContract]
#if WINRT
    public sealed class InstancePlayLimitException : Exception
#else
    public sealed class InstancePlayLimitException : ExternalException
#endif
	{
	}
}

