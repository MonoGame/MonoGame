// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace Microsoft.Xna.Framework.Content.Pipeline
{
    /// <summary>
    /// Provides a base class to use when developing custom processor components. All processors must derive from this class.
    /// </summary>
    public abstract class ContentProcessor<TInput, TOutput> : IContentProcessor
    {
        /// <summary>
        /// Initializes a new instance of the ContentProcessor class.
        /// </summary>
        protected ContentProcessor()
        {

        }

        /// <summary>
        /// Processes the specified input data and returns the result.
        /// </summary>
        /// <param name="input">Existing content object being processed.</param>
        /// <param name="context">Contains any required custom process parameters.</param>
        /// <returns>A typed object representing the processed input.</returns>
        public abstract TOutput Process(TInput input, ContentProcessorContext context);

        /// <summary>
        /// Gets the expected object type of the input parameter to IContentProcessor.Process.
        /// </summary>
        Type IContentProcessor.InputType
        {
            get { return typeof(TInput); }
        }

        /// <summary>
        /// Gets the object type returned by IContentProcessor.Process.
        /// </summary>
        Type IContentProcessor.OutputType
        {
            get { return typeof(TOutput); }
        }

        /// <summary>
        /// Processes the specified input data and returns the result.
        /// </summary>
        /// <param name="input">Existing content object being processed.</param>
        /// <param name="context">Contains any required custom process parameters.</param>
        /// <returns>The processed input.</returns>
        object IContentProcessor.Process(object input, ContentProcessorContext context)
        {
            if (input == null)
                throw new ArgumentNullException("input");
            if (context == null)
                throw new ArgumentNullException("context");
            if (!(input is TInput))
                throw new InvalidOperationException("input is not of the expected type");
            return Process((TInput)input, context);
        }
    }
}
