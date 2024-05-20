//
// This code is auto generated, don't modify it by hand.
// To regenerate it run: Tools/MonoGame.Generator.CTypes
//

#pragma once

#include "csharp_common.h"

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
};

static const char* MGSurfaceFormat_ToString(MGSurfaceFormat enumValue)
{
    switch (enumValue)
    {
        case MGSurfaceFormat::Color: return "Color";
        case MGSurfaceFormat::Bgr565: return "Bgr565";
        case MGSurfaceFormat::Bgra5551: return "Bgra5551";
        case MGSurfaceFormat::Bgra4444: return "Bgra4444";
        case MGSurfaceFormat::Dxt1: return "Dxt1";
        case MGSurfaceFormat::Dxt3: return "Dxt3";
        case MGSurfaceFormat::Dxt5: return "Dxt5";
        case MGSurfaceFormat::NormalizedByte2: return "NormalizedByte2";
        case MGSurfaceFormat::NormalizedByte4: return "NormalizedByte4";
        case MGSurfaceFormat::Rgba1010102: return "Rgba1010102";
        case MGSurfaceFormat::Rg32: return "Rg32";
        case MGSurfaceFormat::Rgba64: return "Rgba64";
        case MGSurfaceFormat::Alpha8: return "Alpha8";
        case MGSurfaceFormat::Single: return "Single";
        case MGSurfaceFormat::Vector2: return "Vector2";
        case MGSurfaceFormat::Vector4: return "Vector4";
        case MGSurfaceFormat::HalfSingle: return "HalfSingle";
        case MGSurfaceFormat::HalfVector2: return "HalfVector2";
        case MGSurfaceFormat::HalfVector4: return "HalfVector4";
        case MGSurfaceFormat::HdrBlendable: return "HdrBlendable";
        case MGSurfaceFormat::Bgr32: return "Bgr32";
        case MGSurfaceFormat::Bgra32: return "Bgra32";
        case MGSurfaceFormat::ColorSRgb: return "ColorSRgb";
        case MGSurfaceFormat::Bgr32SRgb: return "Bgr32SRgb";
        case MGSurfaceFormat::Bgra32SRgb: return "Bgra32SRgb";
        case MGSurfaceFormat::Dxt1SRgb: return "Dxt1SRgb";
        case MGSurfaceFormat::Dxt3SRgb: return "Dxt3SRgb";
        case MGSurfaceFormat::Dxt5SRgb: return "Dxt5SRgb";
        case MGSurfaceFormat::RgbPvrtc2Bpp: return "RgbPvrtc2Bpp";
        case MGSurfaceFormat::RgbPvrtc4Bpp: return "RgbPvrtc4Bpp";
        case MGSurfaceFormat::RgbaPvrtc2Bpp: return "RgbaPvrtc2Bpp";
        case MGSurfaceFormat::RgbaPvrtc4Bpp: return "RgbaPvrtc4Bpp";
        case MGSurfaceFormat::RgbEtc1: return "RgbEtc1";
        case MGSurfaceFormat::Dxt1a: return "Dxt1a";
        case MGSurfaceFormat::RgbaAtcExplicitAlpha: return "RgbaAtcExplicitAlpha";
        case MGSurfaceFormat::RgbaAtcInterpolatedAlpha: return "RgbaAtcInterpolatedAlpha";
        case MGSurfaceFormat::Rgb8Etc2: return "Rgb8Etc2";
        case MGSurfaceFormat::Srgb8Etc2: return "Srgb8Etc2";
        case MGSurfaceFormat::Rgb8A1Etc2: return "Rgb8A1Etc2";
        case MGSurfaceFormat::Srgb8A1Etc2: return "Srgb8A1Etc2";
        case MGSurfaceFormat::Rgba8Etc2: return "Rgba8Etc2";
        case MGSurfaceFormat::SRgb8A8Etc2: return "SRgb8A8Etc2";
    }

    return "Unknown Value";
}

enum class MGDepthFormat : mgint
{
    None = 0,
    Depth16 = 1,
    Depth24 = 2,
    Depth24Stencil8 = 3,
};

static const char* MGDepthFormat_ToString(MGDepthFormat enumValue)
{
    switch (enumValue)
    {
        case MGDepthFormat::None: return "None";
        case MGDepthFormat::Depth16: return "Depth16";
        case MGDepthFormat::Depth24: return "Depth24";
        case MGDepthFormat::Depth24Stencil8: return "Depth24Stencil8";
    }

    return "Unknown Value";
}

enum class MGClearOptions : mgint
{
    Target = 1,
    DepthBuffer = 2,
    Stencil = 4,
};

static const char* MGClearOptions_ToString(MGClearOptions enumValue)
{
    switch (enumValue)
    {
        case MGClearOptions::Target: return "Target";
        case MGClearOptions::DepthBuffer: return "DepthBuffer";
        case MGClearOptions::Stencil: return "Stencil";
    }

    return "Unknown Value";
}

enum class MGShaderStage : mgint
{
    Vertex = 0,
    Pixel = 1,
    Count = 2,
};

static const char* MGShaderStage_ToString(MGShaderStage enumValue)
{
    switch (enumValue)
    {
        case MGShaderStage::Vertex: return "Vertex";
        case MGShaderStage::Pixel: return "Pixel";
        case MGShaderStage::Count: return "Count";
    }

    return "Unknown Value";
}

enum class MGIndexElementSize : mgint
{
    SixteenBits = 0,
    ThirtyTwoBits = 1,
};

static const char* MGIndexElementSize_ToString(MGIndexElementSize enumValue)
{
    switch (enumValue)
    {
        case MGIndexElementSize::SixteenBits: return "SixteenBits";
        case MGIndexElementSize::ThirtyTwoBits: return "ThirtyTwoBits";
    }

    return "Unknown Value";
}

enum class MGPrimitiveType : mgint
{
    TriangleList = 0,
    TriangleStrip = 1,
    LineList = 2,
    LineStrip = 3,
    PointList = 4,
};

static const char* MGPrimitiveType_ToString(MGPrimitiveType enumValue)
{
    switch (enumValue)
    {
        case MGPrimitiveType::TriangleList: return "TriangleList";
        case MGPrimitiveType::TriangleStrip: return "TriangleStrip";
        case MGPrimitiveType::LineList: return "LineList";
        case MGPrimitiveType::LineStrip: return "LineStrip";
        case MGPrimitiveType::PointList: return "PointList";
    }

    return "Unknown Value";
}

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

static const char* MGBlend_ToString(MGBlend enumValue)
{
    switch (enumValue)
    {
        case MGBlend::One: return "One";
        case MGBlend::Zero: return "Zero";
        case MGBlend::SourceColor: return "SourceColor";
        case MGBlend::InverseSourceColor: return "InverseSourceColor";
        case MGBlend::SourceAlpha: return "SourceAlpha";
        case MGBlend::InverseSourceAlpha: return "InverseSourceAlpha";
        case MGBlend::DestinationColor: return "DestinationColor";
        case MGBlend::InverseDestinationColor: return "InverseDestinationColor";
        case MGBlend::DestinationAlpha: return "DestinationAlpha";
        case MGBlend::InverseDestinationAlpha: return "InverseDestinationAlpha";
        case MGBlend::BlendFactor: return "BlendFactor";
        case MGBlend::InverseBlendFactor: return "InverseBlendFactor";
        case MGBlend::SourceAlphaSaturation: return "SourceAlphaSaturation";
    }

    return "Unknown Value";
}

enum class MGBlendFunction : mgint
{
    Add = 0,
    Subtract = 1,
    ReverseSubtract = 2,
    Min = 3,
    Max = 4,
};

static const char* MGBlendFunction_ToString(MGBlendFunction enumValue)
{
    switch (enumValue)
    {
        case MGBlendFunction::Add: return "Add";
        case MGBlendFunction::Subtract: return "Subtract";
        case MGBlendFunction::ReverseSubtract: return "ReverseSubtract";
        case MGBlendFunction::Min: return "Min";
        case MGBlendFunction::Max: return "Max";
    }

    return "Unknown Value";
}

enum class MGColorWriteChannels : mgint
{
    None = 0,
    Red = 1,
    Green = 2,
    Blue = 4,
    Alpha = 8,
    All = 15,
};

static const char* MGColorWriteChannels_ToString(MGColorWriteChannels enumValue)
{
    switch (enumValue)
    {
        case MGColorWriteChannels::None: return "None";
        case MGColorWriteChannels::Red: return "Red";
        case MGColorWriteChannels::Green: return "Green";
        case MGColorWriteChannels::Blue: return "Blue";
        case MGColorWriteChannels::Alpha: return "Alpha";
        case MGColorWriteChannels::All: return "All";
    }

    return "Unknown Value";
}

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

static const char* MGCompareFunction_ToString(MGCompareFunction enumValue)
{
    switch (enumValue)
    {
        case MGCompareFunction::Always: return "Always";
        case MGCompareFunction::Never: return "Never";
        case MGCompareFunction::Less: return "Less";
        case MGCompareFunction::LessEqual: return "LessEqual";
        case MGCompareFunction::Equal: return "Equal";
        case MGCompareFunction::GreaterEqual: return "GreaterEqual";
        case MGCompareFunction::Greater: return "Greater";
        case MGCompareFunction::NotEqual: return "NotEqual";
    }

    return "Unknown Value";
}

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

static const char* MGStencilOperation_ToString(MGStencilOperation enumValue)
{
    switch (enumValue)
    {
        case MGStencilOperation::Keep: return "Keep";
        case MGStencilOperation::Zero: return "Zero";
        case MGStencilOperation::Replace: return "Replace";
        case MGStencilOperation::Increment: return "Increment";
        case MGStencilOperation::Decrement: return "Decrement";
        case MGStencilOperation::IncrementSaturation: return "IncrementSaturation";
        case MGStencilOperation::DecrementSaturation: return "DecrementSaturation";
        case MGStencilOperation::Invert: return "Invert";
    }

    return "Unknown Value";
}

enum class MGFillMode : mgint
{
    Solid = 0,
    WireFrame = 1,
};

static const char* MGFillMode_ToString(MGFillMode enumValue)
{
    switch (enumValue)
    {
        case MGFillMode::Solid: return "Solid";
        case MGFillMode::WireFrame: return "WireFrame";
    }

    return "Unknown Value";
}

enum class MGCullMode : mgint
{
    None = 0,
    CullClockwiseFace = 1,
    CullCounterClockwiseFace = 2,
};

static const char* MGCullMode_ToString(MGCullMode enumValue)
{
    switch (enumValue)
    {
        case MGCullMode::None: return "None";
        case MGCullMode::CullClockwiseFace: return "CullClockwiseFace";
        case MGCullMode::CullCounterClockwiseFace: return "CullCounterClockwiseFace";
    }

    return "Unknown Value";
}

enum class MGTextureAddressMode : mgint
{
    Wrap = 0,
    Clamp = 1,
    Mirror = 2,
    Border = 3,
};

static const char* MGTextureAddressMode_ToString(MGTextureAddressMode enumValue)
{
    switch (enumValue)
    {
        case MGTextureAddressMode::Wrap: return "Wrap";
        case MGTextureAddressMode::Clamp: return "Clamp";
        case MGTextureAddressMode::Mirror: return "Mirror";
        case MGTextureAddressMode::Border: return "Border";
    }

    return "Unknown Value";
}

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

static const char* MGTextureFilter_ToString(MGTextureFilter enumValue)
{
    switch (enumValue)
    {
        case MGTextureFilter::Linear: return "Linear";
        case MGTextureFilter::Point: return "Point";
        case MGTextureFilter::Anisotropic: return "Anisotropic";
        case MGTextureFilter::LinearMipPoint: return "LinearMipPoint";
        case MGTextureFilter::PointMipLinear: return "PointMipLinear";
        case MGTextureFilter::MinLinearMagPointMipLinear: return "MinLinearMagPointMipLinear";
        case MGTextureFilter::MinLinearMagPointMipPoint: return "MinLinearMagPointMipPoint";
        case MGTextureFilter::MinPointMagLinearMipLinear: return "MinPointMagLinearMipLinear";
        case MGTextureFilter::MinPointMagLinearMipPoint: return "MinPointMagLinearMipPoint";
    }

    return "Unknown Value";
}

enum class MGTextureFilterMode : mgint
{
    Default = 0,
    Comparison = 1,
};

static const char* MGTextureFilterMode_ToString(MGTextureFilterMode enumValue)
{
    switch (enumValue)
    {
        case MGTextureFilterMode::Default: return "Default";
        case MGTextureFilterMode::Comparison: return "Comparison";
    }

    return "Unknown Value";
}

enum class MGBufferType : mgint
{
    Index = 0,
    Vertex = 1,
    Constant = 2,
};

static const char* MGBufferType_ToString(MGBufferType enumValue)
{
    switch (enumValue)
    {
        case MGBufferType::Index: return "Index";
        case MGBufferType::Vertex: return "Vertex";
        case MGBufferType::Constant: return "Constant";
    }

    return "Unknown Value";
}

enum class MGTextureType : mgint
{
    _2D = 0,
    _3D = 1,
    Cube = 2,
};

static const char* MGTextureType_ToString(MGTextureType enumValue)
{
    switch (enumValue)
    {
        case MGTextureType::_2D: return "_2D";
        case MGTextureType::_3D: return "_3D";
        case MGTextureType::Cube: return "Cube";
    }

    return "Unknown Value";
}

enum class MGRenderTargetUsage : mgint
{
    DiscardContents = 0,
    PreserveContents = 1,
    PlatformContents = 2,
};

static const char* MGRenderTargetUsage_ToString(MGRenderTargetUsage enumValue)
{
    switch (enumValue)
    {
        case MGRenderTargetUsage::DiscardContents: return "DiscardContents";
        case MGRenderTargetUsage::PreserveContents: return "PreserveContents";
        case MGRenderTargetUsage::PlatformContents: return "PlatformContents";
    }

    return "Unknown Value";
}

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

static const char* MGVertexElementFormat_ToString(MGVertexElementFormat enumValue)
{
    switch (enumValue)
    {
        case MGVertexElementFormat::Single: return "Single";
        case MGVertexElementFormat::Vector2: return "Vector2";
        case MGVertexElementFormat::Vector3: return "Vector3";
        case MGVertexElementFormat::Vector4: return "Vector4";
        case MGVertexElementFormat::Color: return "Color";
        case MGVertexElementFormat::Byte4: return "Byte4";
        case MGVertexElementFormat::Short2: return "Short2";
        case MGVertexElementFormat::Short4: return "Short4";
        case MGVertexElementFormat::NormalizedShort2: return "NormalizedShort2";
        case MGVertexElementFormat::NormalizedShort4: return "NormalizedShort4";
        case MGVertexElementFormat::HalfVector2: return "HalfVector2";
        case MGVertexElementFormat::HalfVector4: return "HalfVector4";
    }

    return "Unknown Value";
}

enum class MGGameRunBehavior : mgint
{
    Asynchronous = 0,
    Synchronous = 1,
};

static const char* MGGameRunBehavior_ToString(MGGameRunBehavior enumValue)
{
    switch (enumValue)
    {
        case MGGameRunBehavior::Asynchronous: return "Asynchronous";
        case MGGameRunBehavior::Synchronous: return "Synchronous";
    }

    return "Unknown Value";
}

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
    DropFile = 13,
    DropComplete = 14,
};

static const char* MGEventType_ToString(MGEventType enumValue)
{
    switch (enumValue)
    {
        case MGEventType::Quit: return "Quit";
        case MGEventType::WindowMoved: return "WindowMoved";
        case MGEventType::WindowResized: return "WindowResized";
        case MGEventType::WindowGainedFocus: return "WindowGainedFocus";
        case MGEventType::WindowLostFocus: return "WindowLostFocus";
        case MGEventType::WindowClose: return "WindowClose";
        case MGEventType::KeyDown: return "KeyDown";
        case MGEventType::KeyUp: return "KeyUp";
        case MGEventType::TextInput: return "TextInput";
        case MGEventType::MouseMove: return "MouseMove";
        case MGEventType::MouseButtonDown: return "MouseButtonDown";
        case MGEventType::MouseButtonUp: return "MouseButtonUp";
        case MGEventType::MouseWheel: return "MouseWheel";
        case MGEventType::DropFile: return "DropFile";
        case MGEventType::DropComplete: return "DropComplete";
    }

    return "Unknown Value";
}

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

static const char* MGKeys_ToString(MGKeys enumValue)
{
    switch (enumValue)
    {
        case MGKeys::None: return "None";
        case MGKeys::Back: return "Back";
        case MGKeys::Tab: return "Tab";
        case MGKeys::Enter: return "Enter";
        case MGKeys::Pause: return "Pause";
        case MGKeys::CapsLock: return "CapsLock";
        case MGKeys::Kana: return "Kana";
        case MGKeys::Kanji: return "Kanji";
        case MGKeys::Escape: return "Escape";
        case MGKeys::ImeConvert: return "ImeConvert";
        case MGKeys::ImeNoConvert: return "ImeNoConvert";
        case MGKeys::Space: return "Space";
        case MGKeys::PageUp: return "PageUp";
        case MGKeys::PageDown: return "PageDown";
        case MGKeys::End: return "End";
        case MGKeys::Home: return "Home";
        case MGKeys::Left: return "Left";
        case MGKeys::Up: return "Up";
        case MGKeys::Right: return "Right";
        case MGKeys::Down: return "Down";
        case MGKeys::Select: return "Select";
        case MGKeys::Print: return "Print";
        case MGKeys::Execute: return "Execute";
        case MGKeys::PrintScreen: return "PrintScreen";
        case MGKeys::Insert: return "Insert";
        case MGKeys::Delete: return "Delete";
        case MGKeys::Help: return "Help";
        case MGKeys::D0: return "D0";
        case MGKeys::D1: return "D1";
        case MGKeys::D2: return "D2";
        case MGKeys::D3: return "D3";
        case MGKeys::D4: return "D4";
        case MGKeys::D5: return "D5";
        case MGKeys::D6: return "D6";
        case MGKeys::D7: return "D7";
        case MGKeys::D8: return "D8";
        case MGKeys::D9: return "D9";
        case MGKeys::A: return "A";
        case MGKeys::B: return "B";
        case MGKeys::C: return "C";
        case MGKeys::D: return "D";
        case MGKeys::E: return "E";
        case MGKeys::F: return "F";
        case MGKeys::G: return "G";
        case MGKeys::H: return "H";
        case MGKeys::I: return "I";
        case MGKeys::J: return "J";
        case MGKeys::K: return "K";
        case MGKeys::L: return "L";
        case MGKeys::M: return "M";
        case MGKeys::N: return "N";
        case MGKeys::O: return "O";
        case MGKeys::P: return "P";
        case MGKeys::Q: return "Q";
        case MGKeys::R: return "R";
        case MGKeys::S: return "S";
        case MGKeys::T: return "T";
        case MGKeys::U: return "U";
        case MGKeys::V: return "V";
        case MGKeys::W: return "W";
        case MGKeys::X: return "X";
        case MGKeys::Y: return "Y";
        case MGKeys::Z: return "Z";
        case MGKeys::LeftWindows: return "LeftWindows";
        case MGKeys::RightWindows: return "RightWindows";
        case MGKeys::Apps: return "Apps";
        case MGKeys::Sleep: return "Sleep";
        case MGKeys::NumPad0: return "NumPad0";
        case MGKeys::NumPad1: return "NumPad1";
        case MGKeys::NumPad2: return "NumPad2";
        case MGKeys::NumPad3: return "NumPad3";
        case MGKeys::NumPad4: return "NumPad4";
        case MGKeys::NumPad5: return "NumPad5";
        case MGKeys::NumPad6: return "NumPad6";
        case MGKeys::NumPad7: return "NumPad7";
        case MGKeys::NumPad8: return "NumPad8";
        case MGKeys::NumPad9: return "NumPad9";
        case MGKeys::Multiply: return "Multiply";
        case MGKeys::Add: return "Add";
        case MGKeys::Separator: return "Separator";
        case MGKeys::Subtract: return "Subtract";
        case MGKeys::Decimal: return "Decimal";
        case MGKeys::Divide: return "Divide";
        case MGKeys::F1: return "F1";
        case MGKeys::F2: return "F2";
        case MGKeys::F3: return "F3";
        case MGKeys::F4: return "F4";
        case MGKeys::F5: return "F5";
        case MGKeys::F6: return "F6";
        case MGKeys::F7: return "F7";
        case MGKeys::F8: return "F8";
        case MGKeys::F9: return "F9";
        case MGKeys::F10: return "F10";
        case MGKeys::F11: return "F11";
        case MGKeys::F12: return "F12";
        case MGKeys::F13: return "F13";
        case MGKeys::F14: return "F14";
        case MGKeys::F15: return "F15";
        case MGKeys::F16: return "F16";
        case MGKeys::F17: return "F17";
        case MGKeys::F18: return "F18";
        case MGKeys::F19: return "F19";
        case MGKeys::F20: return "F20";
        case MGKeys::F21: return "F21";
        case MGKeys::F22: return "F22";
        case MGKeys::F23: return "F23";
        case MGKeys::F24: return "F24";
        case MGKeys::NumLock: return "NumLock";
        case MGKeys::Scroll: return "Scroll";
        case MGKeys::LeftShift: return "LeftShift";
        case MGKeys::RightShift: return "RightShift";
        case MGKeys::LeftControl: return "LeftControl";
        case MGKeys::RightControl: return "RightControl";
        case MGKeys::LeftAlt: return "LeftAlt";
        case MGKeys::RightAlt: return "RightAlt";
        case MGKeys::BrowserBack: return "BrowserBack";
        case MGKeys::BrowserForward: return "BrowserForward";
        case MGKeys::BrowserRefresh: return "BrowserRefresh";
        case MGKeys::BrowserStop: return "BrowserStop";
        case MGKeys::BrowserSearch: return "BrowserSearch";
        case MGKeys::BrowserFavorites: return "BrowserFavorites";
        case MGKeys::BrowserHome: return "BrowserHome";
        case MGKeys::VolumeMute: return "VolumeMute";
        case MGKeys::VolumeDown: return "VolumeDown";
        case MGKeys::VolumeUp: return "VolumeUp";
        case MGKeys::MediaNextTrack: return "MediaNextTrack";
        case MGKeys::MediaPreviousTrack: return "MediaPreviousTrack";
        case MGKeys::MediaStop: return "MediaStop";
        case MGKeys::MediaPlayPause: return "MediaPlayPause";
        case MGKeys::LaunchMail: return "LaunchMail";
        case MGKeys::SelectMedia: return "SelectMedia";
        case MGKeys::LaunchApplication1: return "LaunchApplication1";
        case MGKeys::LaunchApplication2: return "LaunchApplication2";
        case MGKeys::OemSemicolon: return "OemSemicolon";
        case MGKeys::OemPlus: return "OemPlus";
        case MGKeys::OemComma: return "OemComma";
        case MGKeys::OemMinus: return "OemMinus";
        case MGKeys::OemPeriod: return "OemPeriod";
        case MGKeys::OemQuestion: return "OemQuestion";
        case MGKeys::OemTilde: return "OemTilde";
        case MGKeys::ChatPadGreen: return "ChatPadGreen";
        case MGKeys::ChatPadOrange: return "ChatPadOrange";
        case MGKeys::OemOpenBrackets: return "OemOpenBrackets";
        case MGKeys::OemPipe: return "OemPipe";
        case MGKeys::OemCloseBrackets: return "OemCloseBrackets";
        case MGKeys::OemQuotes: return "OemQuotes";
        case MGKeys::Oem8: return "Oem8";
        case MGKeys::OemBackslash: return "OemBackslash";
        case MGKeys::ProcessKey: return "ProcessKey";
        case MGKeys::OemCopy: return "OemCopy";
        case MGKeys::OemAuto: return "OemAuto";
        case MGKeys::OemEnlW: return "OemEnlW";
        case MGKeys::Attn: return "Attn";
        case MGKeys::Crsel: return "Crsel";
        case MGKeys::Exsel: return "Exsel";
        case MGKeys::EraseEof: return "EraseEof";
        case MGKeys::Play: return "Play";
        case MGKeys::Zoom: return "Zoom";
        case MGKeys::Pa1: return "Pa1";
        case MGKeys::OemClear: return "OemClear";
    }

    return "Unknown Value";
}

enum class MGMouseButton : mgint
{
    Left = 0,
    Middle = 1,
    Right = 2,
    X1 = 3,
    X2 = 4,
};

static const char* MGMouseButton_ToString(MGMouseButton enumValue)
{
    switch (enumValue)
    {
        case MGMouseButton::Left: return "Left";
        case MGMouseButton::Middle: return "Middle";
        case MGMouseButton::Right: return "Right";
        case MGMouseButton::X1: return "X1";
        case MGMouseButton::X2: return "X2";
    }

    return "Unknown Value";
}

