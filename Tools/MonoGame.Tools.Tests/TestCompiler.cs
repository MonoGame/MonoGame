using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;

namespace MonoGame.Tests.ContentPipeline
{
    class TestCompiler
    {
        class TestContentManager : ContentManager
        {
            class FakeGraphicsService : IGraphicsDeviceService
            {
                public GraphicsDevice GraphicsDevice { get; private set; }

#pragma warning disable 67
                public event EventHandler<EventArgs> DeviceCreated;
                public event EventHandler<EventArgs> DeviceDisposing;
                public event EventHandler<EventArgs> DeviceReset;
                public event EventHandler<EventArgs> DeviceResetting;
#pragma warning restore 67
            }

            class FakeServiceProvider : IServiceProvider
            {
                public object GetService(Type serviceType)
                {
                    if (serviceType == typeof(IGraphicsDeviceService))
                        return new FakeGraphicsService();

                    throw new NotImplementedException();
                }
            }

            private readonly MemoryStream _xnbStream;

            public TestContentManager(MemoryStream xnbStream)
                : base(new FakeServiceProvider(), "NONE")
            {
                _xnbStream = xnbStream;
            }

            protected override Stream OpenStream(string assetName)
            {
                return new MemoryStream(_xnbStream.GetBuffer(), false);
            }
        }

        static readonly IReadOnlyCollection<TargetPlatform> Platforms = new[]
        {
            TargetPlatform.Windows,
            TargetPlatform.Xbox360,
            TargetPlatform.iOS,
            TargetPlatform.Android,
            TargetPlatform.DesktopGL,
            TargetPlatform.MacOSX,
            TargetPlatform.NativeClient,

            TargetPlatform.RaspberryPi,
            TargetPlatform.PlayStation4,
            TargetPlatform.PlayStation5,
            TargetPlatform.XboxOne,
            TargetPlatform.Switch,
            TargetPlatform.Web
        };
        static readonly IReadOnlyCollection<GraphicsProfile> GraphicsProfiles = new[]
        {
            GraphicsProfile.HiDef,
            GraphicsProfile.Reach
        };
        static readonly IReadOnlyCollection<bool> CompressContents = new[]
        {
            true,
            false
        };

        public static void CompileAndLoadAssets<T>(T data, Action<T> validation)
        {
            ContentCompiler compiler = new ContentCompiler();

            foreach (var platform in Platforms)
                foreach (var gfxProfile in GraphicsProfiles)
                    foreach (var compress in CompressContents)
                        using (var xnbStream = new MemoryStream())
                        {
                            compiler.Compile(xnbStream, data, platform, gfxProfile, compress, "", "");
                            using (var content = new TestContentManager(xnbStream))
                            {
                                var result = content.Load<T>("foo");
                                validation(result);
                            }
                        }
        }
    }
}
