using Microsoft.Xna.Framework;
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
            class FakeGraphicsService : IGraphicsDeviceManager
            {
                public GraphicsDevice GraphicsDevice { get; private set; }

#pragma warning disable 67
                public event EventHandler<EventArgs> DeviceCreated;
                public event EventHandler<EventArgs> DeviceDisposing;
                public event EventHandler<EventArgs> DeviceReset;
                public event EventHandler<EventArgs> DeviceResetting;
#pragma warning restore 67

                /// <inheritdoc />
                public GraphicsProfile GraphicsProfile { get; set; }

                /// <inheritdoc />
                public bool AllowResize { get; set; }

                /// <inheritdoc />
                public bool IsFullScreen { get; set; }

                /// <inheritdoc />
                public bool IsMouseVisible { get; set; }

                /// <inheritdoc />
                public bool IsFixedTimeStep { get; set; }

                /// <inheritdoc />
                public TimeSpan TargetElapsedTime { get; set; }

                /// <inheritdoc />
                public bool HardwareModeSwitch { get; set; }

                /// <inheritdoc />
                public bool PreferHalfPixelOffset { get; set; }

                /// <inheritdoc />
                public bool PreferMultiSampling { get; set; }

                /// <inheritdoc />
                public SurfaceFormat PreferredBackBufferFormat { get; set; }

                /// <inheritdoc />
                public int PreferredBackBufferHeight { get; set; }

                /// <inheritdoc />
                public int PreferredBackBufferWidth { get; set; }

                /// <inheritdoc />
                public DepthFormat PreferredDepthStencilFormat { get; set; }

                /// <inheritdoc />
                public bool SynchronizeWithVerticalRetrace { get; set; }

                /// <inheritdoc />
                public DisplayOrientation SupportedOrientations { get; set; }

                /// <inheritdoc />
                public void ApplyChanges()
                {
                    throw new NotImplementedException();
                }
            }

            class FakeServiceProvider : IServiceProvider
            {
                public object GetService(Type serviceType)
                {
                    if (serviceType == typeof(IGraphicsDeviceManager))
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
            TargetPlatform.WindowsStoreApp,
            TargetPlatform.NativeClient,

            TargetPlatform.WindowsPhone8,
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
