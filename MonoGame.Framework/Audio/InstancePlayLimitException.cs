// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace Microsoft.Xna.Framework.Audio
{
    /// <summary>
    /// The exception thrown when the system attempts to play more SoundEffectInstances than allotted.
    /// </summary>
    /// <remarks>
    /// Most platforms have a hard limit on how many sounds can be played simultaneously. This exception is thrown when that limit is exceeded.
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

