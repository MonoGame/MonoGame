// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace Microsoft.Xna.Framework.Content
{
    /// <summary>
    /// The exception that's thrown when an error occurs when loading content.
    /// </summary>
    public class ContentLoadException : Exception
    {
        /// <summary>
        /// Create a new <see cref="ContentLoadException"/> instance.
        /// </summary>
        public ContentLoadException() : base()
        {
        }

        /// <summary>
        /// Create a new <see cref="ContentLoadException"/> instance with the specified message.
        /// </summary>
        /// <param name="message">The message of the exception.</param>
        public ContentLoadException(string message) : base(message)
        {
        }

        /// <summary>
        /// Create a new <see cref="ContentLoadException"/> instance with the specified message and inner exception.
        /// </summary>
        /// <param name="message">The message of the exception.</param>
        /// <param name="innerException">The inner exception.</param>
        public ContentLoadException(string message, Exception innerException) : base(message,innerException)
        {
        }
    }
}

