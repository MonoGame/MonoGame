// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// This code is auto generated, don't modify it by hand.
// To regenerate it run: Tools/MonoGame.Generator.CTypes

#pragma once

#include "api_common.h"

enum class MGSoundState : mgint
{
    Playing = 0,
    Paused = 1,
    Stopped = 2,
};

enum class MGFilterMode : mgint
{
    LowPass = 0,
    BandPass = 1,
    HighPass = 2,
};

enum class MGSurfaceFormat : mgint
{
    Color = 0,
    Bgr565 = 1,
    Bgra5551 = 2,
    Bgra4444 = 3,
    Dxt1 = 4,
    Dxt3 = 5,
    Dxt5 = 6,
    NormalizedByte2 = 7,
    NormalizedByte4 = 8,
    Rgba1010102 = 9,
    Rg32 = 10,
    Rgba64 = 11,
    Alpha8 = 12,
    Single = 13,
    Vector2 = 14,
    Vector4 = 15,
    HalfSingle = 16,
    HalfVector2 = 17,
    HalfVector4 = 18,
    HdrBlendable = 19,
    Bgr32 = 20,
    Bgra32 = 21,
    ColorSRgb = 30,
    Bgr32SRgb = 31,
    Bgra32SRgb = 32,
    Dxt1SRgb = 33,
    Dxt3SRgb = 34,
    Dxt5SRgb = 35,
    RgbPvrtc2Bpp = 50,
    RgbPvrtc4Bpp = 51,
    RgbaPvrtc2Bpp = 52,
    RgbaPvrtc4Bpp = 53,
    RgbEtc1 = 60,
    Dxt1a = 70,
    RgbaAtcExplicitAlpha = 80,
    RgbaAtcInterpolatedAlpha = 81,
    Rgb8Etc2 = 90,
    Srgb8Etc2 = 91,
    Rgb8A1Etc2 = 92,
    Srgb8A1Etc2 = 93,
    Rgba8Etc2 = 94,
    SRgb8A8Etc2 = 95,
    Astc4X4Rgba = 96,
};

enum class MGDepthFormat : mgint
{
    None = 0,
    Depth16 = 1,
    Depth24 = 2,
    Depth24Stencil8 = 3,
};

enum class MGClearOptions : mgint
{
    Target = 1,
    DepthBuffer = 2,
    Stencil = 4,
};

enum class MGShaderStage : mgint
{
    Vertex = 0,
    Pixel = 1,
    Count = 2,
};

enum class MGIndexElementSize : mgint
{
    SixteenBits = 0,
    ThirtyTwoBits = 1,
};

enum class MGPrimitiveType : mgint
{
    TriangleList = 0,
    TriangleStrip = 1,
    LineList = 2,
    LineStrip = 3,
    PointList = 4,
};

enum class MGBlend : mgint
{
    One = 0,
    Zero = 1,
    SourceColor = 2,
    InverseSourceColor = 3,
    SourceAlpha = 4,
    InverseSourceAlpha = 5,
    DestinationColor = 6,
    InverseDestinationColor = 7,
    DestinationAlpha = 8,
    InverseDestinationAlpha = 9,
    BlendFactor = 10,
    InverseBlendFactor = 11,
    SourceAlphaSaturation = 12,
};

enum class MGBlendFunction : mgint
{
    Add = 0,
    Subtract = 1,
    ReverseSubtract = 2,
    Min = 3,
    Max = 4,
};

enum class MGColorWriteChannels : mgint
{
    None = 0,
    Red = 1,
    Green = 2,
    Blue = 4,
    Alpha = 8,
    All = 15,
};

enum class MGCompareFunction : mgint
{
    Always = 0,
    Never = 1,
    Less = 2,
    LessEqual = 3,
    Equal = 4,
    GreaterEqual = 5,
    Greater = 6,
    NotEqual = 7,
};

enum class MGStencilOperation : mgint
{
    Keep = 0,
    Zero = 1,
    Replace = 2,
    Increment = 3,
    Decrement = 4,
    IncrementSaturation = 5,
    DecrementSaturation = 6,
    Invert = 7,
};

enum class MGFillMode : mgint
{
    Solid = 0,
    WireFrame = 1,
};

enum class MGCullMode : mgint
{
    None = 0,
    CullClockwiseFace = 1,
    CullCounterClockwiseFace = 2,
};

enum class MGTextureAddressMode : mgint
{
    Wrap = 0,
    Clamp = 1,
    Mirror = 2,
    Border = 3,
};

enum class MGTextureFilter : mgint
{
    Linear = 0,
    Point = 1,
    Anisotropic = 2,
    LinearMipPoint = 3,
    PointMipLinear = 4,
    MinLinearMagPointMipLinear = 5,
    MinLinearMagPointMipPoint = 6,
    MinPointMagLinearMipLinear = 7,
    MinPointMagLinearMipPoint = 8,
};

enum class MGTextureFilterMode : mgint
{
    Default = 0,
    Comparison = 1,
};

enum class MGBufferType : mgint
{
    Index = 0,
    Vertex = 1,
    Constant = 2,
};

enum class MGTextureType : mgint
{
    _2D = 0,
    _3D = 1,
    Cube = 2,
};

enum class MGRenderTargetUsage : mgint
{
    DiscardContents = 0,
    PreserveContents = 1,
    PlatformContents = 2,
};

enum class MGVertexElementFormat : mgint
{
    Single = 0,
    Vector2 = 1,
    Vector3 = 2,
    Vector4 = 3,
    Color = 4,
    Byte4 = 5,
    Short2 = 6,
    Short4 = 7,
    NormalizedShort2 = 8,
    NormalizedShort4 = 9,
    HalfVector2 = 10,
    HalfVector4 = 11,
};

enum class MGGameRunBehavior : mgint
{
    Asynchronous = 0,
    Synchronous = 1,
};

enum class MGEventType : mguint
{
    Quit = 0,
    WindowMoved = 1,
    WindowResized = 2,
    WindowGainedFocus = 3,
    WindowLostFocus = 4,
    WindowClose = 5,
    KeyDown = 6,
    KeyUp = 7,
    TextInput = 8,
    MouseMove = 9,
    MouseButtonDown = 10,
    MouseButtonUp = 11,
    MouseWheel = 12,
    ControllerAdded = 13,
    ControllerRemoved = 14,
    ControllerStateChange = 15,
    DropFile = 16,
    DropComplete = 17,
};

enum class MGKeys : mgint
{
    None = 0,
    Back = 8,
    Tab = 9,
    Enter = 13,
    Pause = 19,
    CapsLock = 20,
    Kana = 21,
    Kanji = 25,
    Escape = 27,
    ImeConvert = 28,
    ImeNoConvert = 29,
    Space = 32,
    PageUp = 33,
    PageDown = 34,
    End = 35,
    Home = 36,
    Left = 37,
    Up = 38,
    Right = 39,
    Down = 40,
    Select = 41,
    Print = 42,
    Execute = 43,
    PrintScreen = 44,
    Insert = 45,
    Delete = 46,
    Help = 47,
    D0 = 48,
    D1 = 49,
    D2 = 50,
    D3 = 51,
    D4 = 52,
    D5 = 53,
    D6 = 54,
    D7 = 55,
    D8 = 56,
    D9 = 57,
    A = 65,
    B = 66,
    C = 67,
    D = 68,
    E = 69,
    F = 70,
    G = 71,
    H = 72,
    I = 73,
    J = 74,
    K = 75,
    L = 76,
    M = 77,
    N = 78,
    O = 79,
    P = 80,
    Q = 81,
    R = 82,
    S = 83,
    T = 84,
    U = 85,
    V = 86,
    W = 87,
    X = 88,
    Y = 89,
    Z = 90,
    LeftWindows = 91,
    RightWindows = 92,
    Apps = 93,
    Sleep = 95,
    NumPad0 = 96,
    NumPad1 = 97,
    NumPad2 = 98,
    NumPad3 = 99,
    NumPad4 = 100,
    NumPad5 = 101,
    NumPad6 = 102,
    NumPad7 = 103,
    NumPad8 = 104,
    NumPad9 = 105,
    Multiply = 106,
    Add = 107,
    Separator = 108,
    Subtract = 109,
    Decimal = 110,
    Divide = 111,
    F1 = 112,
    F2 = 113,
    F3 = 114,
    F4 = 115,
    F5 = 116,
    F6 = 117,
    F7 = 118,
    F8 = 119,
    F9 = 120,
    F10 = 121,
    F11 = 122,
    F12 = 123,
    F13 = 124,
    F14 = 125,
    F15 = 126,
    F16 = 127,
    F17 = 128,
    F18 = 129,
    F19 = 130,
    F20 = 131,
    F21 = 132,
    F22 = 133,
    F23 = 134,
    F24 = 135,
    NumLock = 144,
    Scroll = 145,
    LeftShift = 160,
    RightShift = 161,
    LeftControl = 162,
    RightControl = 163,
    LeftAlt = 164,
    RightAlt = 165,
    BrowserBack = 166,
    BrowserForward = 167,
    BrowserRefresh = 168,
    BrowserStop = 169,
    BrowserSearch = 170,
    BrowserFavorites = 171,
    BrowserHome = 172,
    VolumeMute = 173,
    VolumeDown = 174,
    VolumeUp = 175,
    MediaNextTrack = 176,
    MediaPreviousTrack = 177,
    MediaStop = 178,
    MediaPlayPause = 179,
    LaunchMail = 180,
    SelectMedia = 181,
    LaunchApplication1 = 182,
    LaunchApplication2 = 183,
    OemSemicolon = 186,
    OemPlus = 187,
    OemComma = 188,
    OemMinus = 189,
    OemPeriod = 190,
    OemQuestion = 191,
    OemTilde = 192,
    ChatPadGreen = 202,
    ChatPadOrange = 203,
    OemOpenBrackets = 219,
    OemPipe = 220,
    OemCloseBrackets = 221,
    OemQuotes = 222,
    Oem8 = 223,
    OemBackslash = 226,
    ProcessKey = 229,
    OemCopy = 242,
    OemAuto = 243,
    OemEnlW = 244,
    Attn = 246,
    Crsel = 247,
    Exsel = 248,
    EraseEof = 249,
    Play = 250,
    Zoom = 251,
    Pa1 = 253,
    OemClear = 254,
};

enum class MGMouseButton : mgint
{
    Left = 0,
    Middle = 1,
    Right = 2,
    X1 = 3,
    X2 = 4,
};

enum class MGControllerInput : mgint
{
    A = 0,
    B = 1,
    X = 2,
    Y = 3,
    Back = 4,
    Guide = 5,
    Start = 6,
    LeftStick = 7,
    RightStick = 8,
    LeftShoulder = 9,
    RightShoulder = 10,
    DpadUp = 11,
    DpadDown = 12,
    DpadLeft = 13,
    DpadRight = 14,
    Misc1 = 15,
    Paddle1 = 16,
    Paddle2 = 17,
    Paddle3 = 18,
    Paddle4 = 19,
    Touchpad = 20,
    LAST_BUTTON = 20,
    LeftStickX = 21,
    LeftStickY = 22,
    RightStickX = 23,
    RightStickY = 24,
    LeftTrigger = 25,
    LAST_TRIGGER = 26,
    RightTrigger = 26,
    INVALID = -1,
};

enum class MGMonoGamePlatform : mgint
{
    Android = 0,
    iOS = 1,
    tvOS = 2,
    DesktopGL = 3,
    Windows = 4,
    WebGL = 5,
    XboxOne = 6,
    WindowsGDK = 7,
    XboxSeries = 8,
    PlayStation4 = 9,
    PlayStation5 = 10,
    NintendoSwitch = 11,
    DesktopVK = 12,
};

enum class MGGraphicsBackend : mgint
{
    DirectX = 0,
    OpenGL = 1,
    Vulkan = 2,
    Metal = 3,
    DirectX12 = 4,
};

enum class MGSystemCursor : mgint
{
    Arrow = 0,
    IBeam = 1,
    Wait = 2,
    Crosshair = 3,
    WaitArrow = 4,
    SizeNWSE = 5,
    SizeNESW = 6,
    SizeWE = 7,
    SizeNS = 8,
    SizeAll = 9,
    No = 10,
    Hand = 11,
};

enum class MGGamePadType : mgint
{
    Unknown = 0,
    GamePad = 1,
    Wheel = 2,
    ArcadeStick = 3,
    FlightStick = 4,
    DancePad = 5,
    Guitar = 6,
    AlternateGuitar = 7,
    DrumKit = 8,
    BigButtonPad = 768,
};

