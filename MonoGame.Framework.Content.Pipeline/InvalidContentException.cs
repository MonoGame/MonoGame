// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Runtime.Serialization;

namespace Microsoft.Xna.Framework.Content.Pipeline
{
    /// <summary>
    /// Thrown when errors are encountered in content during processing.
    /// </summary>
    [SerializableAttribute]
    public class InvalidContentException : Exception
    {
        /// <summary>
        /// Gets or sets the identity of the content item that caused the exception.
        /// </summary>
        public ContentIdentity ContentIdentity { get; set; }

        /// <summary>
        /// Initializes a new instance of the InvalidContentException class
        /// </summary>
        public InvalidContentException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the InvalidContentException class with information on serialization and streaming context for the related content item.
        /// </summary>
        /// <param name="serializationInfo">Information necessary for serialization and deserialization of the content item.</param>
        /// <param name="streamingContext">Information necessary for the source and destination of a given serialized stream. Also provides an additional caller-defined context.</param>
        protected InvalidContentException(
            SerializationInfo serializationInfo,
            StreamingContext streamingContext
            )
        {
        }

        /// <summary>
        /// Initializes a new instance of the InvalidContentException class with the specified error message.
        /// </summary>
        /// <param name="message">A message that describes the error.</param>
        public InvalidContentException(
            string message
            )
            : this(message, null, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the InvalidContentException class with the specified error message and the identity of the content throwing the exception.
        /// </summary>
        /// <param name="message">A message that describes the error.</param>
        /// <param name="contentIdentity">Information about the content item that caused this error, including the file name. In some cases, a location within the file (of the problem) is specified.</param>
        public InvalidContentException(
            string message,
            ContentIdentity contentIdentity
            )
            : this(message, contentIdentity, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the InvalidContentException class with the specified error message, the identity of the content throwing the exception, and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">A message that describes the error.</param>
        /// <param name="contentIdentity">Information about the content item that caused this error, including the file name. In some cases, a location within the file (of the problem) is specified.</param>
        /// <param name="innerException">The exception that is the cause of the current exception. If innerException is not a null reference, the current exception is raised in a catch block that handles the inner exception.</param>
        public InvalidContentException(
            string message,
            ContentIdentity contentIdentity,
            Exception innerException
            )
            : base(message, innerException)
        {
            ContentIdentity = contentIdentity;
        }

        /// <summary>
        /// Initializes a new instance of the InvalidContentException class with the specified error message and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">A message that describes the error.</param>
        /// <param name="innerException">The exception that is the cause of the current exception. If innerException is not a null reference, the current exception is raised in a catch block that handles the inner exception.</param>
        public InvalidContentException(
            string message,
            Exception innerException
            )
            : this(message, null, innerException)
        {
        }
    }
}
