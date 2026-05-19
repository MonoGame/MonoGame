using System;
using System.IO;
using Microsoft.Xna.Framework.Content.Pipeline;
using MonoGame.Framework.Content;
using MonoGame.Tests.ContentPipeline;

namespace MonoGame.Tools.Tests
{

    internal class TestBitmapProcessorContext : ContextScopeFactory.ContextScope
    {
        public override string IntermediateDirectory { get; }
        public override ContentBuildLogger Logger { get; } = new TestContentBuildLogger();
        public override ContentIdentity SourceIdentity { get; }

        public TestBitmapProcessorContext()
        {
            var id = Guid.NewGuid().ToString();
            SourceIdentity = new ContentIdentity(id);
            IntermediateDirectory = Path.Combine("test", id);
            Directory.CreateDirectory(IntermediateDirectory);
        }

        public override void Dispose()
        {
            base.Dispose();
            // clean up directory
            Directory.Delete(IntermediateDirectory, recursive: true);
        }
    }
}
