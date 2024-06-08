using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;

namespace Microsoft.Xna.Framework
{
    class ConsoleGamePlatform : GamePlatform
    {
        public ConsoleGamePlatform(Game game) : base(game) { }
        public override GameRunBehavior DefaultRunBehavior { get; }
        public override void Exit() { }
        public override void RunLoop() { }
        public override void StartRunLoop() { }
        public override bool BeforeUpdate(GameTime gameTime) => false;
        public override bool BeforeDraw(GameTime gameTime) => false;
        public override void EnterFullScreen() { }
        public override void ExitFullScreen() { }
        public override void BeginScreenDeviceChange(bool willBeFullScreen) { }
        public override void EndScreenDeviceChange(string screenDeviceName, int clientWidth, int clientHeight) { }
    }
    internal class ConsoleGameWindow : GameWindow
    {
        public override bool AllowUserResizing { get; set; }
        public override Rectangle ClientBounds { get; }
        public override DisplayOrientation CurrentOrientation { get; }
        public override IntPtr Handle { get; }
        public override string ScreenDeviceName { get; }
        public override void BeginScreenDeviceChange(bool willBeFullScreen) { }
        public override void EndScreenDeviceChange(string screenDeviceName, int clientWidth, int clientHeight) { }
        protected internal override void SetSupportedOrientations(DisplayOrientation orientations) { }
        protected override void SetTitle(string title) { }
    }
    partial class GamePlatform
    {
        internal static GamePlatform PlatformCreate(Game game) => null;
    }
    partial class TitleContainer
    {
        static partial void PlatformInit() { }
        private static Stream PlatformOpenStream(string safeName) => null;
    }

    namespace Audio
    {
        public sealed partial class DynamicSoundEffectInstance : SoundEffectInstance
        {
            private void PlatformCreate() { }
            private int PlatformGetPendingBufferCount() => 0;
            private void PlatformPlay() { }
            private void PlatformPause() { }
            private void PlatformResume() { }
            private void PlatformStop() { }
            private void PlatformSubmitBuffer(byte[] buffer, int offset, int count) { }
            private void PlatformDispose(bool disposing) { }
            private void PlatformUpdateQueue() { }
        }
        public sealed partial class Microphone
        {
            internal void PlatformStart() { }
            internal void PlatformStop() { }
            internal void Update() { }
            internal int PlatformGetData(byte[] buffer, int offset, int count) => 0;
        }
        public sealed partial class SoundEffect
        {
            internal const int MAX_PLAYING_INSTANCES = 0;
            private void PlatformLoadAudioStream(Stream s, out TimeSpan duration) => duration = default;
            private void PlatformInitializePcm(byte[] buffer, int offset, int count, int sampleBits, int sampleRate, AudioChannels channels, int loopStart, int loopLength) { }
            private void PlatformInitializeFormat(byte[] header, byte[] buffer, int bufferSize, int loopStart, int loopLength) { }
            private void PlatformInitializeXact(MiniFormatTag codec, byte[] buffer, int channels, int sampleRate, int blockAlignment, int loopStart, int loopLength, out TimeSpan duration) => duration = default;
            private void PlatformSetupInstance(SoundEffectInstance instance) { }
            private void PlatformDispose(bool disposing) { }
            internal static void PlatformSetReverbSettings(ReverbSettings reverbSettings) { }
            private static void PlatformInitialize() { }
            internal static void PlatformShutdown() { }
        }
        public partial class SoundEffectInstance : IDisposable
        {
            private void PlatformInitialize(byte[] buffer, int sampleRate, int channels) { }
            private void PlatformApply3D(AudioListener listener, AudioEmitter emitter) { }
            private void PlatformPause() { }
            private void PlatformPlay() { }
            private void PlatformResume() { }
            private void PlatformStop(bool immediate) { }
            private void PlatformSetIsLooped(bool value) { }
            private bool PlatformGetIsLooped() => false;
            private void PlatformSetPan(float value) { }
            private void PlatformSetPitch(float value) { }
            private SoundState PlatformGetState() => default;
            private void PlatformSetVolume(float value) { }
            internal void PlatformSetReverbMix(float mix) { }
            internal void PlatformSetFilter(FilterMode mode, float filterQ, float frequency) { }
            internal void PlatformClearFilter() { }
            private void PlatformDispose(bool disposing) { }
        }
        partial class WaveBank
        {
            private SoundEffectInstance PlatformCreateStream(StreamInfo stream) => null;
        }
    }

    namespace Graphics
    {
        internal partial class ConstantBuffer
        {
            private void PlatformInitialize() { }
            private void PlatformClear() { }
            internal void PlatformApply(GraphicsDevice device, ShaderStage stage, int slot) { }
        }
        public partial class DepthStencilState
        {
            internal void PlatformApplyState(GraphicsDevice device) { }
        }
        internal partial class EffectResource
        {
            const string AlphaTestEffectName = null, BasicEffectName = null, DualTextureEffectName = null, EnvironmentMapEffectName = null, SkinnedEffectName = null, SpriteEffectName = null;
        }
        partial class GraphicsAdapter
        {
            private static void PlatformInitializeAdapters(out ReadOnlyCollection<GraphicsAdapter> adapters) => adapters = null;
            private bool PlatformIsProfileSupported(GraphicsProfile graphicsProfile) => false;
        }
        internal partial class GraphicsCapabilities
        {
            private void PlatformInitialize(GraphicsDevice device) { }
        }
        public partial class GraphicsDebug
        {
            private bool PlatformTryDequeueMessage(out GraphicsDebugMessage message) { message = null; return false; }
        }
        public partial class GraphicsDevice
        {
            private void PlatformSetup() { }
            private void PlatformInitialize() { }
            private void OnPresentationChanged() { }
            private void PlatformClear(ClearOptions options, Vector4 color, float depth, int stencil) { }
            private void PlatformDispose() { }
            private void PlatformPresent() { }
            private void PlatformSetViewport(ref Viewport value) { }
            private void PlatformApplyDefaultRenderTarget() { }
            private void PlatformResolveRenderTargets() { }
            private IRenderTarget PlatformApplyRenderTargets() => null;
            private void PlatformBeginApplyState() { }
            private void PlatformApplyBlend() { }
            private void PlatformApplyState(bool applyShaders) { }
            private void PlatformDrawIndexedPrimitives(PrimitiveType primitiveType, int baseVertex, int startIndex, int primitiveCount) { }
            private void PlatformDrawUserPrimitives<T>(PrimitiveType primitiveType, T[] vertexData, int vertexOffset, VertexDeclaration vertexDeclaration, int vertexCount) where T : struct { }
            private void PlatformDrawPrimitives(PrimitiveType primitiveType, int vertexStart, int vertexCount) { }
            private void PlatformDrawUserIndexedPrimitives<T>(PrimitiveType primitiveType, T[] vertexData, int vertexOffset, int numVertices, short[] indexData, int indexOffset, int primitiveCount, VertexDeclaration vertexDeclaration) where T : struct { }
            private void PlatformDrawUserIndexedPrimitives<T>(PrimitiveType primitiveType, T[] vertexData, int vertexOffset, int numVertices, int[] indexData, int indexOffset, int primitiveCount, VertexDeclaration vertexDeclaration) where T : struct { }
            private void PlatformDrawInstancedPrimitives(PrimitiveType primitiveType, int baseVertex, int startIndex, int primitiveCount, int baseInstance, int instanceCount) { }
            private void PlatformGetBackBufferData<T>(Rectangle? rect, T[] data, int startIndex, int count) where T : struct { }
            private static Rectangle PlatformGetTitleSafeArea(int x, int y, int width, int height) => default;
        }
        public partial class IndexBuffer
        {
            private void PlatformConstruct(IndexElementSize indexElementSize, int indexCount) { }
            private void PlatformGraphicsDeviceResetting() { }
            private void PlatformGetData<T>(int offsetInBytes, T[] data, int startIndex, int elementCount) where T : struct { }
            private void PlatformSetData<T>(int offsetInBytes, T[] data, int startIndex, int elementCount, SetDataOptions options) where T : struct { }
        }
        partial class OcclusionQuery
        {
            private void PlatformConstruct() { }
            private void PlatformBegin() { }
            private void PlatformEnd() { }
            private bool PlatformGetResult(out int pixelCount) { pixelCount = 0; return false; }
        }
        public partial class RasterizerState
        {
            internal void PlatformApplyState(GraphicsDevice device) { }
        }
        public partial class RenderTarget2D
        {
            private void PlatformConstruct(GraphicsDevice graphicsDevice, int width, int height, bool mipMap, DepthFormat preferredDepthFormat, int preferredMultiSampleCount, RenderTargetUsage usage, bool shared) { }
            private void PlatformGraphicsDeviceResetting() { }
        }
        public partial class RenderTarget3D
        {
            private void PlatformConstruct(GraphicsDevice graphicsDevice, int width, int height, bool mipMap, DepthFormat preferredDepthFormat, int preferredMultiSampleCount, RenderTargetUsage usage) { }
        }
        public partial class RenderTargetCube
        {
            private void PlatformConstruct(GraphicsDevice graphicsDevice, bool mipMap, DepthFormat preferredDepthFormat, int preferredMultiSampleCount, RenderTargetUsage usage) { }
        }
        public sealed partial class SamplerStateCollection
        {
            private void PlatformSetSamplerState(int index) { }
            private void PlatformClear() { }
            private void PlatformDirty() { }
            private void PlatformSetSamplers(GraphicsDevice device) { }
        }
        partial class Shader
        {
            private static int PlatformProfile() => 0;
            private void PlatformConstruct(ShaderStage stage, byte[] shaderBytecode) { }
            private void PlatformGraphicsDeviceResetting() { }
        }
        public abstract partial class Texture
        {
            private void PlatformGraphicsDeviceResetting() { }
        }
        public partial class Texture2D : Texture
        {
            private void PlatformConstruct(int width, int height, bool mipmap, SurfaceFormat format, SurfaceType type, bool shared) { }
            private void PlatformSetData<T>(int level, T[] data, int startIndex, int elementCount) where T : struct { }
            private void PlatformSetData<T>(int level, int arraySlice, Rectangle rect, T[] data, int startIndex, int elementCount) where T : struct { }
            private void PlatformGetData<T>(int level, int arraySlice, Rectangle rect, T[] data, int startIndex, int elementCount) where T : struct { }
            private static Texture2D PlatformFromStream(GraphicsDevice graphicsDevice, Stream stream) => null;
            private static Texture2D PlatformFromStream(GraphicsDevice graphicsDevice, Stream stream, Action<byte[]> colorProcessor) => null;
            private void PlatformSaveAsJpeg(Stream stream, int width, int height) { }
            private void PlatformSaveAsPng(Stream stream, int width, int height) { }
            private void PlatformReload(Stream textureStream) { }
        }
        public partial class Texture3D : Texture
        {
            private void PlatformConstruct(GraphicsDevice graphicsDevice, int width, int height, int depth, bool mipMap, SurfaceFormat format, bool renderTarget) { }
            private void PlatformSetData<T>(int level, int left, int top, int right, int bottom, int front, int back, T[] data, int startIndex, int elementCount, int width, int height, int depth) { }
            private void PlatformGetData<T>(int level, int left, int top, int right, int bottom, int front, int back, T[] data, int startIndex, int elementCount) where T : struct { }
        }
        public sealed partial class TextureCollection
        {
            private void PlatformInit() { }
            private void PlatformClear() { }
            private void PlatformSetTextures(GraphicsDevice device) { }
        }
        public partial class TextureCube
        {
            private void PlatformConstruct(GraphicsDevice graphicsDevice, int size, bool mipMap, SurfaceFormat format, bool renderTarget) { }
            private void PlatformGetData<T>(CubeMapFace cubeMapFace, int level, Rectangle rect, T[] data, int startIndex, int elementCount) where T : struct { }
            private void PlatformSetData<T>(CubeMapFace face, int level, Rectangle rect, T[] data, int startIndex, int elementCount) { }
        }
        public partial class VertexBuffer
        {
            private void PlatformConstruct() { }
            private void PlatformGetData<T>(int offsetInBytes, T[] data, int startIndex, int elementCount, int vertexStride) { }
            private void PlatformSetData<T>(int offsetInBytes, T[] data, int startIndex, int elementCount, int vertexStride, SetDataOptions options, int bufferSize, int elementSizeInBytes) { }
            private void PlatformGraphicsDeviceResetting() { }
        }
    }

    namespace Input
    {
        static partial class GamePad
        {
            private static int PlatformGetMaxNumberOfGamePads() => 0;
            private static GamePadCapabilities PlatformGetCapabilities(int index) => default;
            private static GamePadState PlatformGetState(int index, GamePadDeadZone leftDeadZoneMode, GamePadDeadZone rightDeadZoneMode) => default;
            private static bool PlatformSetVibration(int index, float leftMotor, float rightMotor, float leftTrigger, float rightTrigger) => false;
        }
        static partial class Joystick
        {
            private const bool PlatformIsSupported = false;
            private static JoystickCapabilities PlatformGetCapabilities(int index) => default;
            private static JoystickState PlatformGetState(int index) => default;
            private static int PlatformLastConnectedIndex => 0;
            private static void PlatformGetState(ref JoystickState joystickState, int index) { }
        }
        public static partial class Keyboard
        {
            private static KeyboardState PlatformGetState() => default;
            internal static void SetKeys(List<Keys> keys) { }
        }
        public static partial class KeyboardInput
        {
            private static Task<string> PlatformShow(string title, string description, string defaultText, bool usePasswordMode) => null;
            private static void PlatformCancel(string result) { }
        }
        public static partial class MessageBox
        {
            private static Task<int?> PlatformShow(string title, string description, List<string> buttons) => null;
            private static void PlatformCancel(int? result) { }
        }
        public static partial class Mouse
        {
            private static IntPtr PlatformGetWindowHandle() => IntPtr.Zero;
            private static void PlatformSetWindowHandle(IntPtr windowHandle) { }
            private static MouseState PlatformGetState(GameWindow window) => default;
            private static void PlatformSetPosition(int x, int y) { }
            public static void PlatformSetCursor(MouseCursor cursor) { }
        }
        public partial class MouseCursor
        {
            private static void PlatformInitalize() { }
            private static MouseCursor PlatformFromTexture2D(Texture2D texture, int originx, int originy) => null;
            private void PlatformDispose() { }
        }
    }

    namespace Media
    {
        public partial class MediaLibrary
        {
            private void PlatformLoad(Action<int> progressCallback) { }
            private AlbumCollection PlatformGetAlbums() => null;
            private SongCollection PlatformGetSongs() => null;
            private void PlatformDispose() { }
        }
        public static partial class MediaPlayer
        {
            private static void PlatformInitialize() { }
            private static bool PlatformGetIsMuted() => false;
            private static void PlatformSetIsMuted(bool muted) { }
            private static bool PlatformGetIsRepeating() => false;
            private static void PlatformSetIsRepeating(bool repeating) { }
            private static bool PlatformGetIsShuffled() => false;
            private static void PlatformSetIsShuffled(bool shuffled) { }
            private static TimeSpan PlatformGetPlayPosition() => default;
            private static MediaState PlatformGetState() => default;
            private static float PlatformGetVolume() => 0;
            private static void PlatformSetVolume(float volume) { }
            private static bool PlatformGetGameHasControl() => false;
            private static void PlatformPause() { }
            private static void PlatformPlaySong(Song song, TimeSpan? startPosition) { }
            private static void PlatformResume() { }
            private static void PlatformStop() { }
        }
        public sealed partial class Song : IEquatable<Song>, IDisposable
        {
            internal float Volume => 0;
            internal void Stop() { }
            private void PlatformInitialize(string fileName) { }
            private void PlatformDispose(bool disposing) { }
            private Album PlatformGetAlbum() => null;
            private Artist PlatformGetArtist() => null;
            private Genre PlatformGetGenre() => null;
            private TimeSpan PlatformGetDuration() => default;
            private bool PlatformIsProtected() => false;
            private bool PlatformIsRated() => false;
            private string PlatformGetName() => null;
            private int PlatformGetPlayCount() => 0;
            private int PlatformGetRating() => 0;
            private int PlatformGetTrackNumber() => 0;
        }
        public sealed partial class Video : IDisposable
        {
            private void PlatformInitialize() { }
        }
        public sealed partial class VideoPlayer : IDisposable
        {
            private void PlatformInitialize() { }
            private Texture2D PlatformGetTexture() => null;
            private void PlatformGetState(ref MediaState result) { }
            private void PlatformPause() { }
            private void PlatformResume() { }
            private void PlatformPlay() { }
            private void PlatformStop() { }
            private void PlatformSetIsLooped() { }
            private void PlatformSetIsMuted() { }
            private TimeSpan PlatformGetPlayPosition() => default;
            private void PlatformSetVolume() { }
            private void PlatformDispose(bool disposing) { }
        }
    }
}

namespace MonoGame.Framework.Utilities
{
    public static partial class PlatformInfo
    {
        private static MonoGamePlatform PlatformGetMonoGamePlatform() => MonoGamePlatform.XboxOne;
        private static GraphicsBackend PlatformGetGraphicsBackend() => GraphicsBackend.DirectX;
    }
    internal static partial class ReflectionHelpers
    {
        internal static class SizeOf<T> { static public int Get() => 0; }
        internal static int ManagedSizeOf(Type type) => 0;
    }
}
