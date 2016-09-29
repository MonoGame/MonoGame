// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using MonoGame.Framework.Content.Pipeline.Builder;
using System;
using System.IO;

namespace Microsoft.Xna.Framework.Content.Pipeline
{
    /// <summary>
    /// Specifies external references to a data file for the content item.
    /// 
    /// While the object model is instantiated, reference file names are absolute. When the file containing the external reference is serialized to disk, file names are relative to the file. This allows movement of the content tree to a different location without breaking internal links.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class ExternalReference<T> : ContentItem
    {
        /// <summary>
        /// Gets and sets the file name of an ExternalReference.
        /// </summary>
        public string Filename { get; set; }

        /// <summary>
        /// Initializes a new instance of ExternalReference.
        /// </summary>
        public ExternalReference()
        {
            Filename = string.Empty;
        }

        /// <summary>
        /// Initializes a new instance of ExternalReference.
        /// </summary>
        /// <param name="filename">The name of the referenced file.</param>
        public ExternalReference(string filename)
        {
            if (string.IsNullOrEmpty(filename))
                throw new ArgumentNullException("filename");
            Filename = filename;
        }

        /// <summary>
        /// Initializes a new instance of ExternalReference, specifying the file path relative to another content item.
        /// </summary>
        /// <param name="filename">The name of the referenced file.</param>
        /// <param name="relativeToContent">The content that the path specified in filename is relative to.</param>
        public ExternalReference(string filename, ContentIdentity relativeToContent)
        {
            if (string.IsNullOrEmpty(filename))
                throw new ArgumentNullException("filename");
            if (relativeToContent == null)
                throw new ArgumentNullException("relativeToContent");
            if (string.IsNullOrEmpty(relativeToContent.SourceFilename))
                throw new ArgumentNullException("relativeToContent.SourceFilename");

            // The intermediate serializer from XNA has the external reference
            // path walking up to the content project directory and then back
            // down to the asset path. We don't appear to have any way to do
            // that from here, so we'll work with the absolute path and let the
            // higher level process sort out any relative paths they need.
            var basePath = Path.GetDirectoryName(relativeToContent.SourceFilename);
            Filename = PathHelper.Normalize(Path.GetFullPath(Path.Combine(basePath, filename)));
        }
    }
}
