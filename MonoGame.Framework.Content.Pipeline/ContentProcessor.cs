// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace Microsoft.Xna.Framework.Content.Pipeline
{
    /// <summary>
    /// Provides a base class to use when developing custom processor components. All processors must derive from this class.
    /// </summary>
    public abstract class ContentProcessor<TInput, TOutput> : IContentProcessor where TOutput : notnull
    {
        /// <summary>
        /// Initializes a new instance of the ContentProcessor class.
        /// </summary>
        protected ContentProcessor()
        {
            Version = (GetType().Assembly.GetName().Version ?? new Version()).ToString();
        }

        /// <summary>
        /// Processes the specified input data and returns the result.
        /// </summary>
        /// <param name="input">Existing content object being processed.</param>
        /// <param name="context">Contains any required custom process parameters.</param>
        /// <returns>A typed object representing the processed input.</returns>
        public abstract TOutput Process(TInput input, ContentProcessorContext context);

        /// <summary>
        /// Gets or sets the version of the current content processor that will be used to determien if the content needs to be rebuilt.
        /// </summary>
        public virtual string Version { get; set; }

        /// <summary>
        /// Gets the expected object type of the input parameter to IContentProcessor.Process.
        /// </summary>
        Type IContentProcessor.InputType => typeof(TInput);

        /// <summary>
        /// Gets the object type returned by IContentProcessor.Process.
        /// </summary>
        Type IContentProcessor.OutputType => typeof(TOutput);

        /// <summary>
        /// Processes the specified input data and returns the result.
        /// </summary>
        /// <param name="input">Existing content object being processed.</param>
        /// <param name="context">Contains any required custom process parameters.</param>
        /// <returns>The processed input.</returns>
        object IContentProcessor.Process(object input, ContentProcessorContext context)
        {
            ArgumentNullException.ThrowIfNull(input);
            ArgumentNullException.ThrowIfNull(context);
            if (input is not TInput tinput)
                throw new InvalidOperationException("input is not of the expected type");
            return Process(tinput, context);
        }
    }
}
