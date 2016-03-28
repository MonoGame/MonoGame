using System;
using System.Runtime.InteropServices;
#if __IOS__
using ObjCRuntime;
#endif

namespace OpenGL
{
    public interface IWindowInfo {
    }
    public interface IGraphicsContext {
        int SwapInterval { get; set; }
        void MakeCurrent(IWindowInfo info);
        void Dispose ();
        void SwapBuffers ();
    }

    public enum BufferAccess {
        ReadOnly = 0x88B8,
    }

    public enum BufferUsageHint {
        StreamDraw = 0x88E0,
        StaticDraw = 0x88E4,
    }

    public enum StencilFace {
        Front = 0x0404, 
        Back = 0x0405,
    }
    public enum DrawBuffersEnum {
        UnsignedShort,
        UnsignedInt,
    }

    public enum ShaderType {
        VertexShader = 0x8B31,
        FragmentShader = 0x8B30,
    }

    public enum ShaderParameter {
        CompileStatus = 0x8B4F,
    }

    public enum GetProgramParameterName {
        LinkStatus = 0x8B82,
    }

    public enum DrawElementsType {
        UnsignedShort = 0x1403,
        UnsignedInt = 0x1405,
    }

    public enum QueryTarget {
        SamplesPassed = 0x8914,
    }

    public enum GetQueryObjectParam {
        QueryResultAvailable = 0x8867,
        QueryResult = 0x8866,
    }

    public enum GenerateMipmapTarget {
        Texture1D = 0x0DE0,
        Texture2D = 0x0DE1,
        Texture3D = 0x806F,
        TextureCubeMap = 0x8513,
        Texture1DArray = 0x8C18,
        Texture2DArray = 0x8C1A,
        Texture2DMultisample = 0x9100,
        Texture2DMultisampleArray = 0x9102, 
    }

    public enum BlitFramebufferFilter {
        Nearest = 0x2600,
    }

    public enum ReadBufferMode {
        ColorAttachment0 = 0x8CE0,
    }

    public enum DrawBufferMode {
        ColorAttachment0 = 0x8CE0,
    }

    public enum FramebufferErrorCode {
        FramebufferUndefined = 0x8219,
        FramebufferComplete = 0x8CD5,
        FramebufferCompleteExt = 0x8CD5,
        FramebufferIncompleteAttachment = 0x8CD6,
        FramebufferIncompleteAttachmentExt = 0x8CD6,
        FramebufferIncompleteMissingAttachment = 0x8CD7,
        FramebufferIncompleteMissingAttachmentExt = 0x8CD7,
        FramebufferIncompleteDimensionsExt = 0x8CD9,
        FramebufferIncompleteFormatsExt = 0x8CDA,
        FramebufferIncompleteDrawBuffer = 0x8CDB,
        FramebufferIncompleteDrawBufferExt = 0x8CDB,
        FramebufferIncompleteReadBuffer = 0x8CDC,
        FramebufferIncompleteReadBufferExt = 0x8CDC,
        FramebufferUnsupported = 0x8CDD,
        FramebufferUnsupportedExt = 0x8CDD,
        FramebufferIncompleteMultisample = 0x8D56,
        FramebufferIncompleteLayerTargets = 0x8DA8,
        FramebufferIncompleteLayerCount = 0x8DA9,
    }

    public enum BufferTarget {
        ArrayBuffer = 0x8892,
        ElementArrayBuffer = 0x8893,
    }

    public enum RenderbufferTarget {
        Renderbuffer = 0x8D41,
        RenderbufferExt = 0x8D41,
    }

    public enum FramebufferTarget {
        Framebuffer = 0x8D40,
        FramebufferExt = 0x8D40,
        ReadFramebuffer = 0x8CA8,
    }

    public enum RenderbufferStorage {
        Rgba8 = 0x8058,
        DepthComponent16 = 0x81a5,
        DepthComponent24 = 0x81a6,
        Depth24Stencil8 = 0x88F0,
    }

    public enum EnableCap : int
    {
        PointSmooth = 0x0B10,
        LineSmooth = 0x0B20,
        CullFace = 0x0B44,
        Lighting = 0x0B50,
        ColorMaterial = 0x0B57,
        Fog = 0x0B60,
        DepthTest = 0x0B71,
        StencilTest = 0x0B90,
        Normalize = 0x0BA1,
        AlphaTest = 0x0BC0,
        Dither = 0x0BD0,
        Blend = 0x0BE2,
        ColorLogicOp = 0x0BF2,
        ScissorTest = 0x0C11,
        Texture2D = 0x0DE1,
        PolygonOffsetFill = 0x8037,
        RescaleNormal = 0x803A,
        VertexArray = 0x8074,
        NormalArray = 0x8075,
        ColorArray = 0x8076,
        TextureCoordArray = 0x8078,
        Multisample = 0x809D,
        SampleAlphaToCoverage = 0x809E,
        SampleAlphaToOne = 0x809F,
        SampleCoverage = 0x80A0,
    }

    public enum VertexPointerType {
        Float = 0x1406,
        Short = 0x1402,
    }

    public enum VertexAttribPointerType {
        Float = 0x1406,
        Short = 0x1402,
        UnsignedByte = 0x1401,
        HalfFloat = 0x140B,
    }

    public enum CullFaceMode {
        Back = 0x0405, 
        Front = 0x0404,
    }

    public enum FrontFaceDirection {
        Cw = 0x0900,
        Ccw = 0x0901,
    }

    public enum MaterialFace {
        FrontAndBack = 0x0408,
    }

    public enum PolygonMode {
        Fill = 0x1B02,
        Line = 0x1B01,
    }

    public enum ColorPointerType {
        Float = 0x1406,
        Short = 0x1402,
        UnsignedShort = 0x1403,
        UnsignedByte = 0x1401,
        HalfFloat = 0x140B,
    }

    public enum NormalPointerType {
        Byte = 0x1400,
        Float = 0x1406,
        Short = 0x1402,
        UnsignedShort = 0x1403,
        UnsignedByte = 0x1401,
        HalfFloat = 0x140B,
    }

    public enum TexCoordPointerType {
        Byte = 0x1400,
        Float = 0x1406,
        Short = 0x1402,
        UnsignedShort = 0x1403,
        UnsignedByte = 0x1401,
        HalfFloat = 0x140B,
    }

    public enum BlendEquationMode {
        FuncAdd = 0x8006,
        Max = 0x8008,  // ios MaxExt
        Min = 0x8007,  // ios MinExt
        FuncReverseSubtract = 0x800B,
        FuncSubtract = 0x800A,
    }

    public enum BlendingFactorSrc {
        DstAlpha = 0x0304,
        DstColor = 0x0306,
        OneMinusDstAlpha = 0x0305,
        OneMinusDstColor = 0x0307,
        OneMinusSrcAlpha = 0x88FB,
        OneMinusSrcColor = 0x88FA, //
        One = 1,
        SrcAlpha = 0x0302,
        SrcAlphaSaturate = 0x0308,
        SrcColor = 0x0301,
        Zero = 0,
    }

    public enum BlendingFactorDest {
        DstAlpha = 0x0304,
        OneMinusDstAlpha = 0x0305,
        OneMinusSrcAlpha = 0x88FB,
        OneMinusSrcColor = 0x88FA,
        One = 1,
        SrcAlpha = 0x0302,
        Zero = 0,
        SrcColor = 0x0301,
    }

    public enum DepthFunction {
        Always = 0x0207,
        Equal = 0x0202,
        Greater = 0x0204,
        Gequal = 0x0206,
        Less = 0x0201,
        Lequal = 0x0203,
        Never = 0x0200,
        Notequal = 0x0205,
    }

    public enum GetPName : int {
        MaxTextureImageUnits = 0x8872, 
        MaxVertexAttribs = 0x8869, 
        MaxTextureSize = 0x0D33,
        MaxDrawBuffers = 0x8824,
        TextureBinding2D = 0x8069,
        MaxTextureMaxAnisotropyExt = 0x84FF,
    }

    public enum StringName { 
        Extensions = 0x1F03, 
    }

    public enum FramebufferAttachment {
        ColorAttachment0 = 0x8CE0,
        ColorAttachment0Ext = 0x8CE0,
        DepthAttachment = 0x8D00,
        StencilAttachment = 0x8D20,
    }

    public enum GLPrimitiveType {
        Lines = 0x0001,
        LineStrip = 0x0003,
        Triangles = 0x0004,
        TriangleStrip = 0x0005,
    }

    [Flags]
    public enum ClearBufferMask : int
    {
        DepthBufferBit = 0x00000100,
        StencilBufferBit = 0x00000400,
        ColorBufferBit = 0x00004000,
    }

    public enum ErrorCode {
        NoError = 0,
    }

    public enum TextureUnit {
        Texture0 = 0x84C0,
    }

    public enum TextureTarget {
        Texture2D = 0x0DE1,
        Texture3D = 0x806F,
        TextureCubeMap = 0x8513,
        TextureCubeMapPositiveX = 0x8515,
        TextureCubeMapPositiveY = 0x8517,
        TextureCubeMapPositiveZ = 0x8519,
        TextureCubeMapNegativeX = 0x8516,
        TextureCubeMapNegativeY = 0x8518,
        TextureCubeMapNegativeZ = 0x851A,
    }

    public enum PixelInternalFormat {
        Rgba = 0x1908,
        Rgb = 0x1907,
        Rgba4 = 0x8056,
        Luminance = 0x1909,
        CompressedRgbS3tcDxt1Ext = 0x83F0,
        CompressedSrgbS3tcDxt1Ext = 0x8C4C,
        CompressedRgbaS3tcDxt1Ext = 0x83F1,
        CompressedRgbaS3tcDxt3Ext = 0x83F2,
        CompressedSrgbAlphaS3tcDxt3Ext = 0x8C4E,
        CompressedRgbaS3tcDxt5Ext = 0x83F3,
        CompressedSrgbAlphaS3tcDxt5Ext = 0x8C4F,
        R32f = 0x822E,
        Rg16f = 0x822F,
        Rgba16f = 0x881A,
        R16f = 0x822D,
        Rg32f = 0x8230,
        Rgba32f = 0x8814,
        Rg8i = 0x8237,
        Rgba8i = 0x8D8E,
        Rg16ui = 0x823A,
        Rgba16ui = 0x8D76,
        Rgb10A2ui = 0x906F,
        // PVRTC
        CompressedRgbPvrtc2Bppv1Img = 0x8C01,
        CompressedRgbPvrtc4Bppv1Img = 0x8C00,
        CompressedRgbaPvrtc2Bppv1Img = 0x8C03,
        CompressedRgbaPvrtc4Bppv1Img = 0x8C02,
        // ATITC
        AtcRgbaExplicitAlphaAmd = 0x8C93,
        AtcRgbaInterpolatedAlphaAmd = 0x87EE,
        // DXT

    }

    public enum PixelFormat {
        Rgba = 0x1908,
        Rgb = 0x1907,
        Luminance = 0x1909,
        CompressedTextureFormats = 0x86A3,
        Red = 0x1903,
        Rg = 0x8227,
    }

    public enum PixelType {
        UnsignedByte = 0x1401,
        UnsignedShort565 = 0x8363,
        UnsignedShort4444 = 0x8033,
        UnsignedShort5551 = 0x8034,
        Float = 0x1406,
        HalfFloat = 0x140B,
        Byte = 0x1400,
        UnsignedShort = 0x1403,
        UnsignedInt1010102 = 0x8036,
    }

    public enum PixelStoreParameter {
        UnpackAlignment = 0x0CF5,
    }

    public enum GLStencilFunction {
        Always = 0x0207,
        Equal = 0x0202,
        Greater = 0x0204,
        Gequal = 0x0206,
        Less = 0x0201,
        Lequal = 0x0203,
        Never = 0x0200,
        Notequal = 0x0205,
    }

    public enum StencilOp {
        Keep = 0x1E00,
        DecrWrap = 0x8508,
        Decr = 0x1E03,
        Incr = 0x1E02,
        IncrWrap = 0x8507,
        Invert = 0x150A,
        Replace = 0x1E01,
        Zero = 0,
    }

    public enum TextureParameterName {
        TextureMaxAnisotropyExt = 0x84FE,
        TextureMaxLevel = 0x813D,
        TextureMinFilter = 0x2801,
        TextureMagFilter = 0x2800,
        TextureWrapS = 0x2802,
        TextureWrapT = 0x2803,
        TextureBorderColor = 0x1004,
        TextureLodBias = 0x8501,
        TextureCompareMode = 0x884C,
        TextureCompareFunc = 0x884D,
        GenerateMipmap = 0x8191,
    }

    public enum Bool {
        True = 1,
        False = 0,
    }

    public enum TextureMinFilter {
        LinearMipmapNearest = 0x2701,
        NearestMipmapLinear = 0x2702,
        LinearMipmapLinear = 0x2703,
        Linear = 0x2601,
        NearestMipmapNearest = 0x2700,
        Nearest = 0x2600,
    }

    public enum TextureMagFilter {
        Linear = 0x2601,
        Nearest = 0x2600,
    }

    public enum TextureCompareMode {
        CompareRefToTexture = 0x884E,
        None = 0,
    }

    public enum TextureWrapMode {
        ClampToEdge = 0x812F,
        Repeat = 0x2901,
        MirroredRepeat = 0x8370,
        //GLES
        ClampToBorder = 0x812D,
    }

    public partial class ColorFormat {
        public ColorFormat(int r, int g, int b, int a)
        {

        }
    }

    public partial class GL
    {
        public enum RenderApi
        {
            ES = 12448,
            GL = 12450,
        }

        public static RenderApi BoundApi = RenderApi.GL;

        public partial class Ext
        {
            [System.Security.SuppressUnmanagedCodeSecurity ()]
            [MonoNativeFunctionWrapper]
            [UnmanagedFunctionPointer (CallingConvention.Cdecl)]
            public delegate void GenRenderbuffersDelegate (int count, out int buffer);
            public static GenRenderbuffersDelegate GenRenderbuffers;

            [System.Security.SuppressUnmanagedCodeSecurity ()]
            [MonoNativeFunctionWrapper]
            [UnmanagedFunctionPointer (CallingConvention.Cdecl)]
            public delegate void BindRenderbufferDelegate (RenderbufferTarget target, int buffer);
            public static BindRenderbufferDelegate BindRenderbuffer;

            [System.Security.SuppressUnmanagedCodeSecurity ()]
            [MonoNativeFunctionWrapper]
            [UnmanagedFunctionPointer (CallingConvention.Cdecl)]
            public delegate void DeleteRenderbuffersDelegate (int count, ref int buffer);
            public static DeleteRenderbuffersDelegate DeleteRenderbuffers;

            [System.Security.SuppressUnmanagedCodeSecurity ()]
            [MonoNativeFunctionWrapper]
            [UnmanagedFunctionPointer (CallingConvention.Cdecl)]
            public delegate void RenderbufferStorageMultisampleDelegate (RenderbufferTarget target, int sampleCount,
                RenderbufferStorage storage, int width, int height);
            public static RenderbufferStorageMultisampleDelegate RenderbufferStorageMultisample;

            [System.Security.SuppressUnmanagedCodeSecurity ()]
            [MonoNativeFunctionWrapper]
            [UnmanagedFunctionPointer (CallingConvention.Cdecl)]
            public delegate void GenFramebuffersDelegate (int count, out int buffer);
            public static GenFramebuffersDelegate GenFramebuffers;

            [System.Security.SuppressUnmanagedCodeSecurity ()]
            [MonoNativeFunctionWrapper]
            [UnmanagedFunctionPointer (CallingConvention.Cdecl)]
            public delegate void BindFramebufferDelegate (FramebufferTarget target, int buffer);
            public static BindFramebufferDelegate BindFramebuffer;

            [System.Security.SuppressUnmanagedCodeSecurity ()]
            [MonoNativeFunctionWrapper]
            [UnmanagedFunctionPointer (CallingConvention.Cdecl)]
            public delegate void DeleteFramebuffersDelegate (int count, ref int buffer);
            public static DeleteFramebuffersDelegate DeleteFramebuffers;

            [System.Security.SuppressUnmanagedCodeSecurity ()]
            [MonoNativeFunctionWrapper]
            [UnmanagedFunctionPointer (CallingConvention.Cdecl)]
            public delegate void FramebufferTexture2DDelegate (FramebufferTarget target, FramebufferAttachment attachement,
                TextureTarget textureTarget, int texture, int level);
            public static FramebufferTexture2DDelegate FramebufferTexture2D;

            [System.Security.SuppressUnmanagedCodeSecurity ()]
            [MonoNativeFunctionWrapper]
            [UnmanagedFunctionPointer (CallingConvention.Cdecl)]
            public delegate void FramebufferRenderbufferDelegate (FramebufferTarget target, FramebufferAttachment attachement,
                RenderbufferTarget renderBufferTarget, int buffer);
            public static FramebufferRenderbufferDelegate FramebufferRenderbuffer;

            [System.Security.SuppressUnmanagedCodeSecurity ()]
            [MonoNativeFunctionWrapper]
            [UnmanagedFunctionPointer (CallingConvention.Cdecl)]
            public delegate void GenerateMipmapDelegate (GenerateMipmapTarget target);
            public static GenerateMipmapDelegate GenerateMipmap;

            [System.Security.SuppressUnmanagedCodeSecurity ()]
            [MonoNativeFunctionWrapper]
            [UnmanagedFunctionPointer (CallingConvention.Cdecl)]
            public delegate void ReadBufferDelegate (ReadBufferMode buffer);
            public static ReadBufferDelegate ReadBuffer;

            [System.Security.SuppressUnmanagedCodeSecurity ()]
            [MonoNativeFunctionWrapper]
            [UnmanagedFunctionPointer (CallingConvention.Cdecl)]
            public delegate void DrawBufferDelegate (DrawBufferMode buffer);
            public static DrawBufferDelegate DrawBuffer;

            [System.Security.SuppressUnmanagedCodeSecurity ()]
            [MonoNativeFunctionWrapper]
            [UnmanagedFunctionPointer (CallingConvention.Cdecl)]
            public delegate void BlitFramebufferDelegate (int srcX0,
                int srcY0,
                int srcX1,
                int srcY1,
                int dstX0,
                int dstY0,
                int dstX1,
                int dstY1,
                ClearBufferMask mask,
                BlitFramebufferFilter filter);
            public static BlitFramebufferDelegate BlitFramebuffer;

            [System.Security.SuppressUnmanagedCodeSecurity ()]
            [MonoNativeFunctionWrapper]
            [UnmanagedFunctionPointer (CallingConvention.Cdecl)]
            public delegate FramebufferErrorCode CheckFramebufferStatusDelegate (FramebufferTarget target);
            public static CheckFramebufferStatusDelegate CheckFramebufferStatus;
        }

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        [UnmanagedFunctionPointer (CallingConvention.Cdecl)]
        public delegate void EnableVertexAttribArrayDelegate (int attrib);
        public static EnableVertexAttribArrayDelegate EnableVertexAttribArray;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        [UnmanagedFunctionPointer (CallingConvention.Cdecl)]
        public delegate void DisableVertexAttribArrayDelegte (int attrib);
        public static DisableVertexAttribArrayDelegte DisableVertexAttribArray;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        [UnmanagedFunctionPointer (CallingConvention.Cdecl)]
        public delegate void MakeCurrentDelegate(IntPtr window);
        public static MakeCurrentDelegate MakeCurrent;

        [CLSCompliant (false)]
        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        [UnmanagedFunctionPointer (CallingConvention.Cdecl)]
        public unsafe delegate void GetIntegerDelegate(int param, [Out] int* data);
        [CLSCompliant (false)]
        public static GetIntegerDelegate GetIntegeri_v;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        [UnmanagedFunctionPointer (CallingConvention.Cdecl)]
        internal delegate IntPtr GetStringDelegate(StringName param);
        internal static GetStringDelegate GetStringInternal;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        [UnmanagedFunctionPointer (CallingConvention.Cdecl)]
        public delegate void ClearDepthDelegate (float depth);
        public static ClearDepthDelegate ClearDepth;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        [UnmanagedFunctionPointer (CallingConvention.Cdecl)]
        public delegate void DepthRangeDelegate (float min, float max);
        public static DepthRangeDelegate DepthRange;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        [UnmanagedFunctionPointer (CallingConvention.Cdecl)]
        public delegate void ClearDelegate(ClearBufferMask mask);
        public static ClearDelegate Clear;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        [UnmanagedFunctionPointer (CallingConvention.Cdecl)]
        public delegate void ClearColorDelegate(float red,float green,float blue,float alpha);
        public static ClearColorDelegate ClearColor;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        [UnmanagedFunctionPointer (CallingConvention.Cdecl)]
        public delegate void ClearStencilDelegate(int stencil);
        public static ClearStencilDelegate ClearStencil;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        [UnmanagedFunctionPointer (CallingConvention.Cdecl)]
        public delegate void ViewportDelegate(int x, int y, int w, int h);
        public static ViewportDelegate Viewport;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        [UnmanagedFunctionPointer (CallingConvention.Cdecl)]
        public delegate ErrorCode GetErrorDelegate();
        public static GetErrorDelegate GetError;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        [UnmanagedFunctionPointer (CallingConvention.Cdecl)]
        public delegate void FlushDelegate();
        public static FlushDelegate Flush;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        [UnmanagedFunctionPointer (CallingConvention.Cdecl)]
        public delegate void GenTexturesDelegte (int count, [Out] out int id);
        public static GenTexturesDelegte GenTextures;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        [UnmanagedFunctionPointer (CallingConvention.Cdecl)]
        public delegate void BindTextureDelegate(TextureTarget target, int id);
        public static BindTextureDelegate BindTexture;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        [UnmanagedFunctionPointer (CallingConvention.Cdecl)]
        public delegate int EnableDelegate (EnableCap cap);
        public static EnableDelegate Enable;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        [UnmanagedFunctionPointer (CallingConvention.Cdecl)]
        public delegate int DisableDelegate (EnableCap cap);
        public static DisableDelegate Disable;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        [UnmanagedFunctionPointer (CallingConvention.Cdecl)]
        public delegate void CullFaceDelegate(CullFaceMode mode);
        public static CullFaceDelegate CullFace;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        [UnmanagedFunctionPointer (CallingConvention.Cdecl)]
        public delegate void FrontFaceDelegate(FrontFaceDirection direction);
        public static FrontFaceDelegate FrontFace;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        [UnmanagedFunctionPointer (CallingConvention.Cdecl)]
        public delegate void PolygonModeDelegate (MaterialFace face, PolygonMode mode);
        public static PolygonModeDelegate PolygonMode;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        [UnmanagedFunctionPointer (CallingConvention.Cdecl)]
        public delegate void PolygonOffsetDelegate (float slopeScaleDepthBias, float depthbias);
        public static PolygonOffsetDelegate PolygonOffset;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        [UnmanagedFunctionPointer (CallingConvention.Cdecl)]
        public delegate void DrawBuffersDelegate (int count, DrawBuffersEnum[] buffers);
        public static DrawBuffersDelegate DrawBuffers;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        [UnmanagedFunctionPointer (CallingConvention.Cdecl)]
        public delegate void UseProgramDelegate(int program);
        public static UseProgramDelegate UseProgram;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        [UnmanagedFunctionPointer (CallingConvention.Cdecl)]
        public delegate void Uniform4FloatArrayDelegate (int location, int size, float[] values);
        public static Uniform4FloatArrayDelegate Uniform4FloatArray;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        [UnmanagedFunctionPointer (CallingConvention.Cdecl)]
        public delegate void Uniform4FloatDelegate (int location, int size, float value);
        public static Uniform4FloatDelegate Uniform4Float;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        [UnmanagedFunctionPointer (CallingConvention.Cdecl)]
        public delegate void ScissorDelegate(int x, int y, int width, int height);
        public static ScissorDelegate Scissor;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        [UnmanagedFunctionPointer (CallingConvention.Cdecl)]
        public delegate void BindBufferDelegate(BufferTarget target, uint buffer);
        public static BindBufferDelegate BindBuffer;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        [UnmanagedFunctionPointer (CallingConvention.Cdecl)]
        public delegate void DrawElementsDelegate (GLPrimitiveType primitiveType, int coumt, DrawElementsType elementType, IntPtr offset);
        public static DrawElementsDelegate DrawElements;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        [UnmanagedFunctionPointer (CallingConvention.Cdecl)]
        public delegate void DrawArraysDelegate (GLPrimitiveType primitiveType, int offset, int count);
        public static DrawArraysDelegate DrawArrays;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        [UnmanagedFunctionPointer (CallingConvention.Cdecl)]
        public delegate void GenRenderbuffersDelegate(int count, [Out] out int buffer);
        public static GenRenderbuffersDelegate GenRenderbuffers;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        [UnmanagedFunctionPointer (CallingConvention.Cdecl)]
        public delegate void BindRenderbufferDelegate (RenderbufferTarget target, int buffer);
        public static BindRenderbufferDelegate BindRenderbuffer;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        [UnmanagedFunctionPointer (CallingConvention.Cdecl)]
        public delegate void DeleteRenderbuffersDelegate(int count, [In] [Out] ref int buffer);
        public static DeleteRenderbuffersDelegate DeleteRenderbuffers;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        [UnmanagedFunctionPointer (CallingConvention.Cdecl)]
        public delegate void RenderbufferStorageMultisampleDelegate(RenderbufferTarget target, int sampleCount, 
            RenderbufferStorage storage, int width, int height);
        public static RenderbufferStorageMultisampleDelegate RenderbufferStorageMultisample;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        [UnmanagedFunctionPointer (CallingConvention.Cdecl)]
        public delegate void GenFramebuffersDelegate(int count, out int buffer);
        public static GenFramebuffersDelegate GenFramebuffers;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        [UnmanagedFunctionPointer (CallingConvention.Cdecl)]
        public delegate void BindFramebufferDelegate (FramebufferTarget target, int buffer);
        public static BindFramebufferDelegate BindFramebuffer;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        [UnmanagedFunctionPointer (CallingConvention.Cdecl)]
        public delegate void DeleteFramebuffersDelegate(int count, ref int buffer);
        public static DeleteFramebuffersDelegate DeleteFramebuffers;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        [UnmanagedFunctionPointer (CallingConvention.Cdecl)]
        public delegate void FramebufferTexture2DDelegate(FramebufferTarget target, FramebufferAttachment attachement,
            TextureTarget textureTarget, int texture, int level );
        public static FramebufferTexture2DDelegate FramebufferTexture2D;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        [UnmanagedFunctionPointer (CallingConvention.Cdecl)]
        public delegate void FramebufferRenderbufferDelegate (FramebufferTarget target, FramebufferAttachment attachement,
            RenderbufferTarget renderBufferTarget, int buffer);
        public static FramebufferRenderbufferDelegate FramebufferRenderbuffer;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        [UnmanagedFunctionPointer (CallingConvention.Cdecl)]
        public delegate void GenerateMipmapDelegate (GenerateMipmapTarget target);
        public static GenerateMipmapDelegate GenerateMipmap;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        [UnmanagedFunctionPointer (CallingConvention.Cdecl)]
        public delegate void ReadBufferDelegate (ReadBufferMode buffer);
        public static ReadBufferDelegate ReadBuffer;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        [UnmanagedFunctionPointer (CallingConvention.Cdecl)]
        public delegate void DrawBufferDelegate (DrawBufferMode buffer);
        public static DrawBufferDelegate DrawBuffer;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        [UnmanagedFunctionPointer (CallingConvention.Cdecl)]
        public delegate void BlitFramebufferDelegate (int srcX0,
            int srcY0,
            int srcX1,
            int srcY1,
            int dstX0,
            int dstY0,
            int dstX1,
            int dstY1,
            ClearBufferMask mask,
            BlitFramebufferFilter filter);
        public static BlitFramebufferDelegate BlitFramebuffer;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        [UnmanagedFunctionPointer (CallingConvention.Cdecl)]
        public delegate FramebufferErrorCode CheckFramebufferStatusDelegate (FramebufferTarget target);
        public static CheckFramebufferStatusDelegate CheckFramebufferStatus;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        [UnmanagedFunctionPointer (CallingConvention.Cdecl)]
        public delegate void TexParameterFloatDelegate (TextureTarget target, TextureParameterName name, float value);
        public static TexParameterFloatDelegate TexParameterFloat;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        [UnmanagedFunctionPointer (CallingConvention.Cdecl)]
        public delegate void TexParameterFloatArrayDelegate (TextureTarget target, TextureParameterName name, float[] values);
        public static TexParameterFloatArrayDelegate TexParameterFloatArray;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        [UnmanagedFunctionPointer (CallingConvention.Cdecl)]
        public delegate void TexParameterIntDelegate (TextureTarget target, TextureParameterName name, int value);
        public static TexParameterIntDelegate TexParameterInt;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        [UnmanagedFunctionPointer (CallingConvention.Cdecl)]
        public delegate void GenQueriesDelegate (int count, [Out] out int queryId);
        public static GenQueriesDelegate GenQueries;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        [UnmanagedFunctionPointer (CallingConvention.Cdecl)]
        public delegate void BeginQueryDelegate (QueryTarget target, int queryId);
        public static BeginQueryDelegate BeginQuery;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        [UnmanagedFunctionPointer (CallingConvention.Cdecl)]
        public delegate void EndQueryDelegate (QueryTarget target);
        public static EndQueryDelegate EndQuery;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        [UnmanagedFunctionPointer (CallingConvention.Cdecl)]
        public delegate void GetQueryObjectDelegate(int queryId, GetQueryObjectParam getparam, [Out] out int ready);
        public static GetQueryObjectDelegate GetQueryObject;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        [UnmanagedFunctionPointer (CallingConvention.Cdecl)]
        public delegate void DeleteQueriesDelegate(int count, [In] [Out] ref int queryId);
        public static DeleteQueriesDelegate DeleteQueries;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        [UnmanagedFunctionPointer (CallingConvention.Cdecl)]
        public delegate void ActiveTextureDelegate (TextureUnit textureUnit);
        public static ActiveTextureDelegate ActiveTexture;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        [UnmanagedFunctionPointer (CallingConvention.Cdecl)]
        public delegate int CreateShaderDelegate (ShaderType type);
        public static CreateShaderDelegate CreateShader;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        [UnmanagedFunctionPointer (CallingConvention.Cdecl)]
        public delegate void ShaderSourceDelegate(int shaderId, string code);
        public static ShaderSourceDelegate ShaderSource;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        [UnmanagedFunctionPointer (CallingConvention.Cdecl)]
        public delegate void CompileShaderDelegate (int shaderId);
        public static CompileShaderDelegate CompileShader;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        [UnmanagedFunctionPointer (CallingConvention.Cdecl)]
        public delegate void GetShaderDelegate(int shaderId, ShaderParameter parameter, [Out] out int value);
        public static GetShaderDelegate GetShader;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        [UnmanagedFunctionPointer (CallingConvention.Cdecl)]
        public delegate string GetShaderInfoLogDelegate(int shaderId);
        public static GetShaderInfoLogDelegate GetShaderInfoLog;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        [UnmanagedFunctionPointer (CallingConvention.Cdecl)]
        public delegate bool IsShaderDelegate(int shaderId);
        public static IsShaderDelegate IsShader;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        [UnmanagedFunctionPointer (CallingConvention.Cdecl)]
        public delegate void DeleteShaderDelegate (int shaderId);
        public static DeleteShaderDelegate DeleteShader;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        [UnmanagedFunctionPointer (CallingConvention.Cdecl)]
        public delegate int GetAttribLocationDelegate(int programId, string name);
        public static GetAttribLocationDelegate GetAttribLocation;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        [UnmanagedFunctionPointer (CallingConvention.Cdecl)]
        public delegate int GetUniformLocationDelegate(int programId, string name);
        public static GetUniformLocationDelegate GetUniformLocation;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        [UnmanagedFunctionPointer (CallingConvention.Cdecl)]
        public delegate bool IsProgramDelegate (int programId);
        public static IsProgramDelegate IsProgram;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        [UnmanagedFunctionPointer (CallingConvention.Cdecl)]
        public delegate void DeleteProgramDelegate (int programId);
        public static DeleteProgramDelegate DeleteProgram;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        [UnmanagedFunctionPointer (CallingConvention.Cdecl)]
        public delegate int CreateProgramDelegate();
        public static CreateProgramDelegate CreateProgram;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        [UnmanagedFunctionPointer (CallingConvention.Cdecl)]
        public delegate void AttachShaderDelegate (int programId, int shaderId);
        public static AttachShaderDelegate AttachShader;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        [UnmanagedFunctionPointer (CallingConvention.Cdecl)]
        public delegate void LinkProgramDelegate(int programId);
        public static LinkProgramDelegate LinkProgram;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        [UnmanagedFunctionPointer (CallingConvention.Cdecl)]
        public delegate void GetProgramDelegate(int programId, GetProgramParameterName name, [Out] out int linked);
        public static GetProgramDelegate GetProgram;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        [UnmanagedFunctionPointer (CallingConvention.Cdecl)]
        public delegate string GetProgramInfoLogDelegate(int programId);
        public static GetProgramInfoLogDelegate GetProgramInfoLog;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        [UnmanagedFunctionPointer (CallingConvention.Cdecl)]
        public delegate void DetachShaderDelegate(int programId, int shaderId);
        public static DetachShaderDelegate DetachShader;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        [UnmanagedFunctionPointer (CallingConvention.Cdecl)]
        public delegate void BlendColorDelegate(float r, float g, float b, float a);
        public static BlendColorDelegate BlendColor;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        [UnmanagedFunctionPointer (CallingConvention.Cdecl)]
        public delegate void BlendEquationSeparateDelegate(BlendEquationMode colorMode, BlendEquationMode alphaMode);
        public static BlendEquationSeparateDelegate BlendEquationSeparate;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        [UnmanagedFunctionPointer (CallingConvention.Cdecl)]
        public delegate void BlendFuncSeparateDelegate(BlendingFactorSrc colorSrc, BlendingFactorDest colorDst,
            BlendingFactorSrc alphaSrc, BlendingFactorDest alphaDst);
        public static BlendFuncSeparateDelegate BlendFuncSeparate;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        [UnmanagedFunctionPointer (CallingConvention.Cdecl)]
        public delegate void ColorMaskDelegate(bool r, bool g, bool b, bool a);
        public static ColorMaskDelegate ColorMask;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        [UnmanagedFunctionPointer (CallingConvention.Cdecl)]
        public delegate void DepthFuncDelegate(DepthFunction function);
        public static DepthFuncDelegate DepthFunc;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        [UnmanagedFunctionPointer (CallingConvention.Cdecl)]
        public delegate void DepthMaskDelegate (bool enabled);
        public static DepthMaskDelegate DepthMask;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        [UnmanagedFunctionPointer (CallingConvention.Cdecl)]
        public delegate void StencilFuncSeparateDelegate (StencilFace face, GLStencilFunction function, int referenceStencil, int mask);
        public static StencilFuncSeparateDelegate StencilFuncSeparate;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        [UnmanagedFunctionPointer (CallingConvention.Cdecl)]
        public delegate void StencilOpSeparateDelegate(StencilFace face, StencilOp stencilfail, StencilOp depthFail, StencilOp pass);
        public static StencilOpSeparateDelegate StencilOpSeparate;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        [UnmanagedFunctionPointer (CallingConvention.Cdecl)]
        public delegate void StencilFuncDelegate(GLStencilFunction function, int referenceStencil, int mask);
        public static StencilFuncDelegate StencilFunc;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        [UnmanagedFunctionPointer (CallingConvention.Cdecl)]
        public delegate void StencilOpDelegate (StencilOp stencilfail, StencilOp depthFail, StencilOp pass);
        public static StencilOpDelegate StencilOp;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        [UnmanagedFunctionPointer (CallingConvention.Cdecl)]
        public delegate void StencilMaskDelegate(int mask);
        public static StencilMaskDelegate StencilMask;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        [UnmanagedFunctionPointer (CallingConvention.Cdecl)]
        public delegate void CompressedTexImage2DDelegate(TextureTarget target, int level, PixelInternalFormat internalFormat,
            int width, int height, int border, int size, IntPtr data);
        public static CompressedTexImage2DDelegate CompressedTexImage2D;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        [UnmanagedFunctionPointer (CallingConvention.Cdecl)]
        public delegate void TexImage2DDelegate(TextureTarget target,int level, PixelInternalFormat internalFormat,
            int width, int height, int border,PixelFormat format, PixelType pixelType, IntPtr data);
        public static TexImage2DDelegate TexImage2D;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        [UnmanagedFunctionPointer (CallingConvention.Cdecl)]
        public delegate void CompressedTexSubImage2DDelegate (TextureTarget target, int level,
            int x, int y, int width, int height, PixelFormat format, int size, IntPtr data);
        public static CompressedTexSubImage2DDelegate CompressedTexSubImage2D;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        [UnmanagedFunctionPointer (CallingConvention.Cdecl)]
        public delegate void TexSubImage2DDelegate (TextureTarget target, int level,
            int x, int y, int width, int height, PixelFormat format, PixelType pixelType, IntPtr data);
        public static TexSubImage2DDelegate TexSubImage2D;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        [UnmanagedFunctionPointer (CallingConvention.Cdecl)]
        public delegate void PixelStoreDelegate (PixelStoreParameter parameter, int size);
        public static PixelStoreDelegate PixelStore;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        [UnmanagedFunctionPointer (CallingConvention.Cdecl)]
        public delegate void FinishDelegate();
        public static FinishDelegate Finish;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        [UnmanagedFunctionPointer (CallingConvention.Cdecl)]
        internal delegate void GetTexImageDelegate(TextureTarget target, int level, PixelFormat format, PixelType type, [Out] IntPtr pixels);
        internal static GetTexImageDelegate GetTexImageInternal;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        [UnmanagedFunctionPointer (CallingConvention.Cdecl)]
        public delegate void TexImage3DDelegate(TextureTarget target,int level, PixelInternalFormat internalFormat,
            int width, int height, int depth, int border,PixelFormat format, PixelType pixelType, IntPtr data);
        public static TexImage3DDelegate TexImage3D;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        [UnmanagedFunctionPointer (CallingConvention.Cdecl)]
        public delegate void TexSubImage3DDelegate (TextureTarget target, int level,
            int x, int y, int z, int width, int height, int depth, PixelFormat format, PixelType pixelType, IntPtr data);
        public static TexSubImage3DDelegate TexSubImage3D;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        [UnmanagedFunctionPointer (CallingConvention.Cdecl)]
        public delegate void DeleteTexturesDelegate(int count, ref int id);
        public static DeleteTexturesDelegate DeleteTextures;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        [UnmanagedFunctionPointer (CallingConvention.Cdecl)]
        public delegate void GenBuffersDelegate(int count, out uint buffer);
        public static GenBuffersDelegate GenBuffers;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        [UnmanagedFunctionPointer (CallingConvention.Cdecl)]
        public delegate void BufferDataDelegate(BufferTarget target, IntPtr size, IntPtr n, BufferUsageHint usage);
        public static BufferDataDelegate BufferData;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        [UnmanagedFunctionPointer (CallingConvention.Cdecl)]
        public delegate IntPtr MapBufferDelegate(BufferTarget target, BufferAccess access);
        public static MapBufferDelegate MapBuffer;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        [UnmanagedFunctionPointer (CallingConvention.Cdecl)]
        public delegate void UnmapBufferDelegate(BufferTarget target);
        public static UnmapBufferDelegate UnmapBuffer;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        [UnmanagedFunctionPointer (CallingConvention.Cdecl)]
        public delegate void BufferSubDataDelegate (BufferTarget target, IntPtr offset, IntPtr size, IntPtr data);
        public static BufferSubDataDelegate BufferSubData;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        [UnmanagedFunctionPointer (CallingConvention.Cdecl)]
        public delegate void DeleteBuffersDelegate (int count, [In] [Out] ref uint buffer);
        public static DeleteBuffersDelegate DeleteBuffers;

        [System.Security.SuppressUnmanagedCodeSecurity()]
        [MonoNativeFunctionWrapper]
        [UnmanagedFunctionPointer (CallingConvention.Cdecl)]
        public delegate void VertexAttribPointerDelegate(int location, int elementCount, VertexAttribPointerType type, bool normalize,
            int stride, IntPtr data);
        public static VertexAttribPointerDelegate VertexAttribPointer;

        public static int SwapInterval { get; set; }

        public static void LoadEntryPoints()
        {
            LoadPlatformEntryPoints ();

            if (Viewport == null)
                Viewport = (ViewportDelegate)LoadEntryPoint<ViewportDelegate>("glViewport");
            if (Scissor == null)
                Scissor = (ScissorDelegate)LoadEntryPoint<ScissorDelegate>("glScissor");
            GetError = (GetErrorDelegate)LoadEntryPoint<GetErrorDelegate>("glGetError");

            TexParameterFloat = (TexParameterFloatDelegate)LoadEntryPoint<TexParameterFloatDelegate>("glTexParameterf");
            TexParameterFloatArray = (TexParameterFloatArrayDelegate)LoadEntryPoint<TexParameterFloatArrayDelegate>("glTexParameterfv");
            TexParameterInt = (TexParameterIntDelegate)LoadEntryPoint<TexParameterIntDelegate>("glTexParameteri");

            EnableVertexAttribArray = (EnableVertexAttribArrayDelegate)LoadEntryPoint<EnableVertexAttribArrayDelegate>("glEnableVertexAttribArray");
            DisableVertexAttribArray = (DisableVertexAttribArrayDelegte)LoadEntryPoint<DisableVertexAttribArrayDelegte>("glDisableVertexAttribArray");
            //MakeCurrent = (MakeCurrentDelegate)LoadEntryPoint<MakeCurrentDelegate>("glMakeCurrent");
            GetIntegeri_v = (GetIntegerDelegate)LoadEntryPoint<GetIntegerDelegate>("glGetIntegeri_v");
            GetStringInternal = (GetStringDelegate)LoadEntryPoint<GetStringDelegate>("glGetString");
            ClearDepth = (ClearDepthDelegate)LoadEntryPoint<ClearDepthDelegate>("glClearDepth");
            DepthRange = (DepthRangeDelegate)LoadEntryPoint<DepthRangeDelegate>("glDepthRange");
            Clear = (ClearDelegate)LoadEntryPoint<ClearDelegate>("glClear");
            ClearColor = (ClearColorDelegate)LoadEntryPoint<ClearColorDelegate>("glClearColor");
            ClearStencil = (ClearStencilDelegate)LoadEntryPoint<ClearStencilDelegate>("glClearStencil");
            Flush = (FlushDelegate)LoadEntryPoint<FlushDelegate>("glFlush");
            GenTextures = (GenTexturesDelegte)LoadEntryPoint<GenTexturesDelegte>("glGenTextures");
            BindTexture = (BindTextureDelegate)LoadEntryPoint<BindTextureDelegate>("glBindTexture");

            Enable = (EnableDelegate)LoadEntryPoint<EnableDelegate>("glEnable");
            Disable = (DisableDelegate)LoadEntryPoint<DisableDelegate>("glDisable");
            CullFace = (CullFaceDelegate)LoadEntryPoint<CullFaceDelegate>("glCullFace");
            FrontFace = (FrontFaceDelegate)LoadEntryPoint<FrontFaceDelegate>("glFrontFace");
            PolygonMode = (PolygonModeDelegate)LoadEntryPoint<PolygonModeDelegate>("glPolygonMode");
            PolygonOffset = (PolygonOffsetDelegate)LoadEntryPoint<PolygonOffsetDelegate>("glPolygonOffset");

            BindBuffer = (BindBufferDelegate)LoadEntryPoint<BindBufferDelegate>("glBindBuffer");
            DrawBuffers = (DrawBuffersDelegate)LoadEntryPoint<DrawBuffersDelegate>("glDrawBuffers");
            DrawElements = (DrawElementsDelegate)LoadEntryPoint<DrawElementsDelegate>("glDrawElements");
            DrawArrays = (DrawArraysDelegate)LoadEntryPoint<DrawArraysDelegate>("glDrawArrays");
            Uniform4Float = (Uniform4FloatDelegate)LoadEntryPoint<Uniform4FloatDelegate>("glUniform4f");
            Uniform4FloatArray = (Uniform4FloatArrayDelegate)LoadEntryPoint<Uniform4FloatArrayDelegate>("glUniform4fv");

            GenRenderbuffers = (GenRenderbuffersDelegate)LoadEntryPoint<GenRenderbuffersDelegate>("glGenRenderbuffers");
            BindRenderbuffer = (BindRenderbufferDelegate)LoadEntryPoint<BindRenderbufferDelegate>("glBindRenderbuffer");
            DeleteRenderbuffers = (DeleteRenderbuffersDelegate)LoadEntryPoint<DeleteRenderbuffersDelegate>("glDeleteRenderbuffers");

            GenFramebuffers = (GenFramebuffersDelegate)LoadEntryPoint<GenFramebuffersDelegate>("glGenFramebuffers");
            BindFramebuffer = (BindFramebufferDelegate)LoadEntryPoint<BindFramebufferDelegate>("glBindFramebuffer");
            DeleteFramebuffers = (DeleteFramebuffersDelegate)LoadEntryPoint<DeleteFramebuffersDelegate>("glDeleteFramebuffers");

            FramebufferTexture2D = (FramebufferTexture2DDelegate)LoadEntryPoint<FramebufferTexture2DDelegate>("glFramebufferTexture2D");
            FramebufferRenderbuffer = (FramebufferRenderbufferDelegate)LoadEntryPoint<FramebufferRenderbufferDelegate>("glFramebufferRenderbuffer");

            GenerateMipmap = (GenerateMipmapDelegate)LoadEntryPoint<GenerateMipmapDelegate>("glGenerateMipmap");
            ReadBuffer = (ReadBufferDelegate)LoadEntryPoint<ReadBufferDelegate>("glReadBuffer");
            DrawBuffer = (DrawBufferDelegate)LoadEntryPoint<DrawBufferDelegate>("glDrawBuffer");

            BlitFramebuffer = (BlitFramebufferDelegate)LoadEntryPoint<BlitFramebufferDelegate>("glBlitFramebuffer");
            CheckFramebufferStatus = (CheckFramebufferStatusDelegate)LoadEntryPoint<CheckFramebufferStatusDelegate>("glCheckFramebufferStatus");

            GenQueries = (GenQueriesDelegate)LoadEntryPoint<GenQueriesDelegate>("glGenQueries");
            BeginQuery = (BeginQueryDelegate)LoadEntryPoint<BeginQueryDelegate>("glBeginQuery");
            EndQuery = (EndQueryDelegate)LoadEntryPoint<EndQueryDelegate>("glEndQuery");
            GetQueryObject = (GetQueryObjectDelegate)LoadEntryPoint<GetQueryObjectDelegate>("glGetQueryObjectivARB");
            DeleteQueries = (DeleteQueriesDelegate)LoadEntryPoint<DeleteQueriesDelegate>("glDeleteQueries");

            ActiveTexture = (ActiveTextureDelegate)LoadEntryPoint<ActiveTextureDelegate>("glActiveTexture");
            CreateShader = (CreateShaderDelegate)LoadEntryPoint<CreateShaderDelegate>("glCreateShader");
            ShaderSource = (ShaderSourceDelegate)LoadEntryPoint<ShaderSourceDelegate>("glShaderSource");
            CompileShader = (CompileShaderDelegate)LoadEntryPoint<CompileShaderDelegate>("glCompileShader");
            GetShader = (GetShaderDelegate)LoadEntryPoint<GetShaderDelegate>("glGetShaderiv");
            GetShaderInfoLog = (GetShaderInfoLogDelegate)LoadEntryPoint<GetShaderInfoLogDelegate>("glGetShaderInfoLog");
            IsShader = (IsShaderDelegate)LoadEntryPoint<IsShaderDelegate>("glIsShader");
            DeleteShader = (DeleteShaderDelegate)LoadEntryPoint<DeleteShaderDelegate>("glDeleteShader");
            GetAttribLocation = (GetAttribLocationDelegate)LoadEntryPoint<GetAttribLocationDelegate>("glGetAttribLocation");
            GetUniformLocation = (GetUniformLocationDelegate)LoadEntryPoint<GetUniformLocationDelegate>("glGetUniformLocation");

            IsProgram = (IsProgramDelegate)LoadEntryPoint<IsProgramDelegate>("glIsProgram");
            DeleteProgram = (DeleteProgramDelegate)LoadEntryPoint<DeleteProgramDelegate>("glDeleteProgram");
            CreateProgram = (CreateProgramDelegate)LoadEntryPoint<CreateProgramDelegate>("glCreateProgram");
            AttachShader = (AttachShaderDelegate)LoadEntryPoint<AttachShaderDelegate>("glAttachShader");
            LinkProgram = (LinkProgramDelegate)LoadEntryPoint<LinkProgramDelegate>("glLinkProgram");
            GetProgram = (GetProgramDelegate)LoadEntryPoint<GetProgramDelegate>("glGetProgramivARB");
            GetProgramInfoLog = (GetProgramInfoLogDelegate)LoadEntryPoint<GetProgramInfoLogDelegate>("glGetProgramInfoLog");
            DetachShader = (DetachShaderDelegate)LoadEntryPoint<DetachShaderDelegate>("glDetachShader");

            BlendColor = (BlendColorDelegate)LoadEntryPoint<BlendColorDelegate>("glBlendColor");
            BlendEquationSeparate = (BlendEquationSeparateDelegate)LoadEntryPoint<BlendEquationSeparateDelegate>("glBlendEquationSeparate");
            BlendFuncSeparate = (BlendFuncSeparateDelegate)LoadEntryPoint<BlendFuncSeparateDelegate>("glBlendFuncSeparate");
            ColorMask = (ColorMaskDelegate)LoadEntryPoint<ColorMaskDelegate>("glColorMask");
            DepthFunc = (DepthFuncDelegate)LoadEntryPoint<DepthFuncDelegate>("glDepthFunc");
            DepthMask = (DepthMaskDelegate)LoadEntryPoint<DepthMaskDelegate>("glDepthMask");
            StencilFuncSeparate = (StencilFuncSeparateDelegate)LoadEntryPoint<StencilFuncSeparateDelegate>("glStencilFuncSeparate");
            StencilOpSeparate = (StencilOpSeparateDelegate)LoadEntryPoint<StencilOpSeparateDelegate>("glStencilOpSeparate");
            StencilFunc = (StencilFuncDelegate)LoadEntryPoint<StencilFuncDelegate>("glStencilFunc");
            StencilOp = (StencilOpDelegate)LoadEntryPoint<StencilOpDelegate>("glStencilOp");
            StencilMask = (StencilMaskDelegate)LoadEntryPoint<StencilMaskDelegate>("glStencilMask");

            CompressedTexImage2D = (CompressedTexImage2DDelegate)LoadEntryPoint<CompressedTexImage2DDelegate>("glCompressedTexImage2D");
            TexImage2D = (TexImage2DDelegate)LoadEntryPoint<TexImage2DDelegate>("glTexImage2D");
            CompressedTexSubImage2D = (CompressedTexSubImage2DDelegate)LoadEntryPoint<CompressedTexSubImage2DDelegate>("glCompressedTexSubImage2D");
            TexSubImage2D = (TexSubImage2DDelegate)LoadEntryPoint<TexSubImage2DDelegate>("glTexSubImage2D");
            PixelStore = (PixelStoreDelegate)LoadEntryPoint<PixelStoreDelegate>("glPixelStorei");
            Finish = (FinishDelegate)LoadEntryPoint<FinishDelegate>("glFinish");
            GetTexImageInternal = (GetTexImageDelegate)LoadEntryPoint<GetTexImageDelegate>("glGetTexImage");
            TexImage3D = (TexImage3DDelegate)LoadEntryPoint<TexImage3DDelegate>("glTexImage3D");
            TexSubImage3D = (TexSubImage3DDelegate)LoadEntryPoint<TexSubImage3DDelegate>("glTexSubImage3D");
            DeleteTextures = (DeleteTexturesDelegate)LoadEntryPoint<DeleteTexturesDelegate>("glDeleteTextures");

            GenBuffers = (GenBuffersDelegate)LoadEntryPoint<GenBuffersDelegate>("glGenBuffers");
            BufferData = (BufferDataDelegate)LoadEntryPoint<BufferDataDelegate>("glBufferData");
            MapBuffer = (MapBufferDelegate)LoadEntryPoint<MapBufferDelegate>("glMapBuffer");
            UnmapBuffer = (UnmapBufferDelegate)LoadEntryPoint<UnmapBufferDelegate>("glUnmapBuffer");
            BufferSubData = (BufferSubDataDelegate)LoadEntryPoint<BufferSubDataDelegate>("glBufferSubData");
            DeleteBuffers = (DeleteBuffersDelegate)LoadEntryPoint<DeleteBuffersDelegate>("glDeleteBuffers");

            VertexAttribPointer = (VertexAttribPointerDelegate)LoadEntryPoint<VertexAttribPointerDelegate>("glVertexAttribPointer");
        }

        public static System.Delegate LoadEntryPoint<T>(string proc)
        {
            return Marshal.GetDelegateForFunctionPointer(EntryPointHelper.GetAddress(proc), typeof(T));
        }

        static partial void LoadPlatformEntryPoints();

        static partial void CreateContext (ref IGraphicsContext context);

        /* Helper Functions */

        public static void Uniform1 (int location, int value)
        {
        }

        public static void Uniform4 (int location, int size, float[] values) {
            Uniform4FloatArray(location, size, values);
        }

        public static void Uniform4 (int location, int size, float value) {
            Uniform4Float(location, size, value);
        }

        public static unsafe void Uniform4 (int location, int size, float* value) {

        }

        public unsafe static string GetString (StringName name)
        {
            return Marshal.PtrToStringAnsi (GetStringInternal (name));
        }

        public unsafe static void GetInteger (GetPName name, out int value)
        {
            fixed (int* ptr = &value) {
                GetIntegeri_v ((int)name, ptr);
            }
        }

        public static void TexParameter(TextureTarget target, TextureParameterName name, float value)
        {
            TexParameterFloat(target, name, value);
        }

        public static void TexParameter(TextureTarget target, TextureParameterName name, float[] values)
        {
            TexParameterFloatArray(target, name, values);
        }

        public static void TexParameter(TextureTarget target, TextureParameterName name, int value)
        {
            TexParameterInt(target, name, value);
        }

        public static unsafe void GetTexImage<T>(TextureTarget target, int level, PixelFormat format, PixelType type, [In] [Out] T[] pixels) where T : struct
        {
            GCHandle pixels_ptr = GCHandle.Alloc(pixels, GCHandleType.Pinned);
            try
            {
                GetTexImageInternal(target, (Int32)level, format, type, (IntPtr)pixels_ptr.AddrOfPinnedObject());
            }
            finally
            {
                pixels_ptr.Free();
            }
        }
    }
}

