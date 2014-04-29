// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace Microsoft.Xna.Framework.Audio
{

    /// <summary>
    /// The exception that is thrown when no audio hardware is present, or when audio hardware is installed, but the device drivers for the audio hardware are not present or enabled.
    /// </summary>
    [DataContract]
#if WINRT
    public sealed class NoAudioHardwareException : Exception
#else
    public sealed class NoAudioHardwareException : ExternalException
#endif
    {
        /// <summary>
        /// Initializes a new instance of this class with a specified error message and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="msg">A message that describes the error.</param>
        public NoAudioHardwareException(string msg)
            : base(msg)
        {
        }

        /// <summary>
        /// Initializes a new instance of this class with a specified error message and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="msg">A message that describes the error.</param>
        /// <param name="innerException">The exception that is the cause of the current exception. If the inner parameter is not a null reference, the current exception is raised in a catch block that handles the inner exception.</param>
        public NoAudioHardwareException(string msg, Exception innerException)
            : base(msg, innerException)
        {
        }
    }
}

