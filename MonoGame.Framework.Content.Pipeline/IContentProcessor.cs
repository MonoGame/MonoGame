using System;

namespace Microsoft.Xna.Framework.Content.Pipeline
{
    public interface IContentProcessor
    {
        public Type InputType { get; }

        public Type OutputType { get; }

        public Object Process(Object input, ContentProcessorContext context);
    }
}
