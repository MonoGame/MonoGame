// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace Microsoft.Xna.Framework.Audio
{

    /// <summary>
    /// The exception thrown when no audio hardware is present, or driver issues are detected.
    /// </summary>
    [DataContract]
    public sealed class NoAudioHardwareException : ExternalException
    {
        /// <param name="msg">A message describing the error.</param>
        public NoAudioHardwareException(string msg)
            : base(msg)
        {
        }

        /// <param name="msg">A message describing the error.</param>
        /// <param name="innerException">The exception that is the underlying cause of the current exception. If not null, the current exception is raised in a try/catch block that handled the innerException.</param>
        public NoAudioHardwareException(string msg, Exception innerException)
            : base(msg, innerException)
        {
        }
    }
}

