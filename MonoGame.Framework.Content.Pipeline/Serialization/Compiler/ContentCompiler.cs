using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler
{
    public sealed class ContentCompiler
    {
        // Note: Should be called from ContentTypeWriter.Initialize() method.
        public ContentTypeWriter GetTypeWriter(Type type)
        {
            throw new NotImplementedException();
            return null;
        }
    }
}
