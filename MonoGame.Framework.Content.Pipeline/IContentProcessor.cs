// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace Microsoft.Xna.Framework.Content.Pipeline
{
    /// <summary>
    /// Provides methods and properties for accessing a statically typed ContentProcessor subclass, using dynamically typed object data.
    /// </summary>
    public interface IContentProcessor
    {
        /// <summary>
        /// Gets the expected object type of the input parameter to IContentProcessor.Process.
        /// </summary>
        Type InputType { get; }

        /// <summary>
        /// Gets the object type returned by IContentProcessor.Process.
        /// </summary>
        Type OutputType { get; }

        /// <summary>
        /// Processes the specified input data and returns the result.
        /// </summary>
        /// <param name="input">Existing content object being processed.</param>
        /// <param name="context">Contains any required custom process parameters.</param>
        /// <returns>An object representing the processed input.</returns>
        Object Process(Object input, ContentProcessorContext context);
    }
}
