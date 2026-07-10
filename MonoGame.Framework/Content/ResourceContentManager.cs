using System;
using System.Collections.Generic;
using System.IO;
using System.Resources;

namespace Microsoft.Xna.Framework.Content
{
    /// <summary>
    /// Subclass of <see cref="ContentManager"/>, which is specialized to read
    /// from <i>.resx</i> resource files rather than directly from individual files on disk.
    /// </summary>
    public class ResourceContentManager : ContentManager
    {
        private ResourceManager resource;

        /// <summary>
        /// Creates a new instance of <see cref="ResourceContentManager"/>.
        /// </summary>
        /// <param name="servicesProvider">
        /// The service provider the <b>ResourceContentManager</b> should use to locate services.
        /// </param>
        /// <param name="resource">The resource manager for the <b>ResourceContentManager</b> to read from.</param>
        /// <exception cref="ArgumentNullException"><paramref name="resource"/> is <see langword="null"/>.</exception>
        public ResourceContentManager(IServiceProvider servicesProvider, ResourceManager resource)
            : base(servicesProvider)
        {
            if (resource == null)
            {
                throw new ArgumentNullException("resource");
            }
            this.resource = resource;
        }

        /// <summary>
        /// Opens a stream for reading the specified resource.
        /// Derived classes can replace this to implement pack files or asset compression.
        /// </summary>
        /// <param name="assetName">The name of the asset being read.</param>
        /// <exception cref="ContentLoadException">
        /// Error loading <paramref name="assetName"/>.
        /// The resource was not a binary resource, or the resource was not found.
        /// </exception>
        protected override System.IO.Stream OpenStream(string assetName)
        {
            object obj = this.resource.GetObject(assetName);
            if (obj == null)
            {
                throw new ContentLoadException("Resource not found");
            }
            if (!(obj is byte[]))
            {
                throw new ContentLoadException("Resource is not in binary format");
            }
            return new MemoryStream(obj as byte[]);
        }
    }
}
