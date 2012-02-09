using System;
using System.Collections.Generic;
using System.IO;
using System.Resources;

namespace Microsoft.Xna.Framework.Content
{
    public class ResourceContentManager : ContentManager
    {
        private ResourceManager resource;

        public ResourceContentManager(IServiceProvider servicesProvider, ResourceManager resource)
            : base(servicesProvider)
        {
            if (resource == null)
            {
                throw new ArgumentNullException("resource");
            }
            this.resource = resource;
        }

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
