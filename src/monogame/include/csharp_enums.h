//
// This code is auto generated, don't modify it by hand.
// To regenerate it run: Tools/MonoGame.Generator.CTypes
//

#pragma once

#include "csharp_common.h"

enum CSGraphicsBackend : csint
{
    DirectX = 0,
    OpenGL = 1,
    Vulkan = 2,
    Metal = 3,
};

class ECSGraphicsBackend
{
public:
    static const char* ToString(CSGraphicsBackend enumValue)
    {
        switch (enumValue)
        {
            case DirectX: return "DirectX";
            case OpenGL: return "OpenGL";
            case Vulkan: return "Vulkan";
            case Metal: return "Metal";
        }

        return "Unknown Value";
    }
};

enum CSMonoGamePlatform : csint
{
    Android = 0,
    iOS = 1,
    tvOS = 2,
    DesktopGL = 3,
    Windows = 4,
    WindowsUniversal = 5,
    WebGL = 6,
    XboxOne = 7,
    PlayStation4 = 8,
    PlayStation5 = 9,
    NintendoSwitch = 10,
    Stadia = 11,
};

class ECSMonoGamePlatform
{
public:
    static const char* ToString(CSMonoGamePlatform enumValue)
    {
        switch (enumValue)
        {
            case Android: return "Android";
            case iOS: return "iOS";
            case tvOS: return "tvOS";
            case DesktopGL: return "DesktopGL";
            case Windows: return "Windows";
            case WindowsUniversal: return "WindowsUniversal";
            case WebGL: return "WebGL";
            case XboxOne: return "XboxOne";
            case PlayStation4: return "PlayStation4";
            case PlayStation5: return "PlayStation5";
            case NintendoSwitch: return "NintendoSwitch";
            case Stadia: return "Stadia";
        }

        return "Unknown Value";
    }
};

enum CSContainmentType : csint
{
    Disjoint = 0,
    Contains = 1,
    Intersects = 2,
};

class ECSContainmentType
{
public:
    static const char* ToString(CSContainmentType enumValue)
    {
        switch (enumValue)
        {
            case Disjoint: return "Disjoint";
            case Contains: return "Contains";
            case Intersects: return "Intersects";
        }

        return "Unknown Value";
    }
};

enum CSCurveContinuity : csint
{
    Smooth = 0,
    Step = 1,
};

class ECSCurveContinuity
{
public:
    static const char* ToString(CSCurveContinuity enumValue)
    {
        switch (enumValue)
        {
            case Smooth: return "Smooth";
            case Step: return "Step";
        }

        return "Unknown Value";
    }
};

enum CSCurveLoopType : csint
{
    Constant = 0,
    Cycle = 1,
    CycleOffset = 2,
    Oscillate = 3,
    Linear = 4,
};

class ECSCurveLoopType
{
public:
    static const char* ToString(CSCurveLoopType enumValue)
    {
        switch (enumValue)
        {
            case Constant: return "Constant";
            case Cycle: return "Cycle";
            case CycleOffset: return "CycleOffset";
            case Oscillate: return "Oscillate";
            case Linear: return "Linear";
        }

        return "Unknown Value";
    }
};

enum CSCurveTangent : csint
{
    Flat = 0,
    Linear = 1,
    Smooth = 2,
};

class ECSCurveTangent
{
public:
    static const char* ToString(CSCurveTangent enumValue)
    {
        switch (enumValue)
        {
            case Flat: return "Flat";
            case Linear: return "Linear";
            case Smooth: return "Smooth";
        }

        return "Unknown Value";
    }
};

enum CSDisplayOrientation : csint
{
    Default = 0,
    LandscapeLeft = 1,
    LandscapeRight = 2,
    Portrait = 4,
    PortraitDown = 8,
    Unknown = 16,
};

class ECSDisplayOrientation
{
public:
    static const char* ToString(CSDisplayOrientation enumValue)
    {
        switch (enumValue)
        {
            case Default: return "Default";
            case LandscapeLeft: return "LandscapeLeft";
            case LandscapeRight: return "LandscapeRight";
            case Portrait: return "Portrait";
            case PortraitDown: return "PortraitDown";
            case Unknown: return "Unknown";
        }

        return "Unknown Value";
    }
};

enum CSGameRunBehavior : csint
{
    Asynchronous = 0,
    Synchronous = 1,
};

class ECSGameRunBehavior
{
public:
    static const char* ToString(CSGameRunBehavior enumValue)
    {
        switch (enumValue)
        {
            case Asynchronous: return "Asynchronous";
            case Synchronous: return "Synchronous";
        }

        return "Unknown Value";
    }
};

enum CSPlaneIntersectionType : csint
{
    Front = 0,
    Back = 1,
    Intersecting = 2,
};

class ECSPlaneIntersectionType
{
public:
    static const char* ToString(CSPlaneIntersectionType enumValue)
    {
        switch (enumValue)
        {
            case Front: return "Front";
            case Back: return "Back";
            case Intersecting: return "Intersecting";
        }

        return "Unknown Value";
    }
};

enum CSPlayerIndex : csint
{
    One = 0,
    Two = 1,
    Three = 2,
    Four = 3,
};

class ECSPlayerIndex
{
public:
    static const char* ToString(CSPlayerIndex enumValue)
    {
        switch (enumValue)
        {
            case One: return "One";
            case Two: return "Two";
            case Three: return "Three";
            case Four: return "Four";
        }

        return "Unknown Value";
    }
};

enum CSMediaSourceType : csint
{
    LocalDevice = 0,
    WindowsMediaConnect = 4,
};

class ECSMediaSourceType
{
public:
    static const char* ToString(CSMediaSourceType enumValue)
    {
        switch (enumValue)
        {
            case LocalDevice: return "LocalDevice";
            case WindowsMediaConnect: return "WindowsMediaConnect";
        }

        return "Unknown Value";
    }
};

enum CSMediaState : csint
{
    Stopped = 0,
    Playing = 1,
    Paused = 2,
};

class ECSMediaState
{
public:
    static const char* ToString(CSMediaState enumValue)
    {
        switch (enumValue)
        {
            case Stopped: return "Stopped";
            case Playing: return "Playing";
            case Paused: return "Paused";
        }

        return "Unknown Value";
    }
};

enum CSVideoSoundtrackType : csint
{
    Music = 0,
    Dialog = 1,
    MusicAndDialog = 2,
};

class ECSVideoSoundtrackType
{
public:
    static const char* ToString(CSVideoSoundtrackType enumValue)
    {
        switch (enumValue)
        {
            case Music: return "Music";
            case Dialog: return "Dialog";
            case MusicAndDialog: return "MusicAndDialog";
        }

        return "Unknown Value";
    }
};

enum CSButtons : csint
{
    None = 0,
    DPadUp = 1,
    DPadDown = 2,
    DPadLeft = 4,
    DPadRight = 8,
    Start = 16,
    Back = 32,
    LeftStick = 64,
    RightStick = 128,
    LeftShoulder = 256,
    RightShoulder = 512,
    BigButton = 2048,
    A = 4096,
    B = 8192,
    X = 16384,
    Y = 32768,
    LeftThumbstickLeft = 2097152,
    RightTrigger = 4194304,
    LeftTrigger = 8388608,
    RightThumbstickUp = 16777216,
    RightThumbstickDown = 33554432,
    RightThumbstickRight = 67108864,
    RightThumbstickLeft = 134217728,
    LeftThumbstickUp = 268435456,
    LeftThumbstickDown = 536870912,
    LeftThumbstickRight = 1073741824,
};

class ECSButtons
{
public:
    static const char* ToString(CSButtons enumValue)
    {
        switch (enumValue)
        {
            case None: return "None";
            case DPadUp: return "DPadUp";
            case DPadDown: return "DPadDown";
            case DPadLeft: return "DPadLeft";
            case DPadRight: return "DPadRight";
            case Start: return "Start";
            case Back: return "Back";
            case LeftStick: return "LeftStick";
            case RightStick: return "RightStick";
            case LeftShoulder: return "LeftShoulder";
            case RightShoulder: return "RightShoulder";
            case BigButton: return "BigButton";
            case A: return "A";
            case B: return "B";
            case X: return "X";
            case Y: return "Y";
            case LeftThumbstickLeft: return "LeftThumbstickLeft";
            case RightTrigger: return "RightTrigger";
            case LeftTrigger: return "LeftTrigger";
            case RightThumbstickUp: return "RightThumbstickUp";
            case RightThumbstickDown: return "RightThumbstickDown";
            case RightThumbstickRight: return "RightThumbstickRight";
            case RightThumbstickLeft: return "RightThumbstickLeft";
            case LeftThumbstickUp: return "LeftThumbstickUp";
            case LeftThumbstickDown: return "LeftThumbstickDown";
            case LeftThumbstickRight: return "LeftThumbstickRight";
        }

        return "Unknown Value";
    }
};

enum CSButtonState : csint
{
    Released = 0,
    Pressed = 1,
};

class ECSButtonState
{
public:
    static const char* ToString(CSButtonState enumValue)
    {
        switch (enumValue)
        {
            case Released: return "Released";
            case Pressed: return "Pressed";
        }

        return "Unknown Value";
    }
};

enum CSGamePadDeadZone : csint
{
    None = 0,
    IndependentAxes = 1,
    Circular = 2,
};

class ECSGamePadDeadZone
{
public:
    static const char* ToString(CSGamePadDeadZone enumValue)
    {
        switch (enumValue)
        {
            case None: return "None";
            case IndependentAxes: return "IndependentAxes";
            case Circular: return "Circular";
        }

        return "Unknown Value";
    }
};

enum CSGamePadType : csint
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

class ECSGamePadType
{
public:
    static const char* ToString(CSGamePadType enumValue)
    {
        switch (enumValue)
        {
            case Unknown: return "Unknown";
            case GamePad: return "GamePad";
            case Wheel: return "Wheel";
            case ArcadeStick: return "ArcadeStick";
            case FlightStick: return "FlightStick";
            case DancePad: return "DancePad";
            case Guitar: return "Guitar";
            case AlternateGuitar: return "AlternateGuitar";
            case DrumKit: return "DrumKit";
            case BigButtonPad: return "BigButtonPad";
        }

        return "Unknown Value";
    }
};

enum CSKeys : csint
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

class ECSKeys
{
public:
    static const char* ToString(CSKeys enumValue)
    {
        switch (enumValue)
        {
            case None: return "None";
            case Back: return "Back";
            case Tab: return "Tab";
            case Enter: return "Enter";
            case Pause: return "Pause";
            case CapsLock: return "CapsLock";
            case Kana: return "Kana";
            case Kanji: return "Kanji";
            case Escape: return "Escape";
            case ImeConvert: return "ImeConvert";
            case ImeNoConvert: return "ImeNoConvert";
            case Space: return "Space";
            case PageUp: return "PageUp";
            case PageDown: return "PageDown";
            case End: return "End";
            case Home: return "Home";
            case Left: return "Left";
            case Up: return "Up";
            case Right: return "Right";
            case Down: return "Down";
            case Select: return "Select";
            case Print: return "Print";
            case Execute: return "Execute";
            case PrintScreen: return "PrintScreen";
            case Insert: return "Insert";
            case Delete: return "Delete";
            case Help: return "Help";
            case D0: return "D0";
            case D1: return "D1";
            case D2: return "D2";
            case D3: return "D3";
            case D4: return "D4";
            case D5: return "D5";
            case D6: return "D6";
            case D7: return "D7";
            case D8: return "D8";
            case D9: return "D9";
            case A: return "A";
            case B: return "B";
            case C: return "C";
            case D: return "D";
            case E: return "E";
            case F: return "F";
            case G: return "G";
            case H: return "H";
            case I: return "I";
            case J: return "J";
            case K: return "K";
            case L: return "L";
            case M: return "M";
            case N: return "N";
            case O: return "O";
            case P: return "P";
            case Q: return "Q";
            case R: return "R";
            case S: return "S";
            case T: return "T";
            case U: return "U";
            case V: return "V";
            case W: return "W";
            case X: return "X";
            case Y: return "Y";
            case Z: return "Z";
            case LeftWindows: return "LeftWindows";
            case RightWindows: return "RightWindows";
            case Apps: return "Apps";
            case Sleep: return "Sleep";
            case NumPad0: return "NumPad0";
            case NumPad1: return "NumPad1";
            case NumPad2: return "NumPad2";
            case NumPad3: return "NumPad3";
            case NumPad4: return "NumPad4";
            case NumPad5: return "NumPad5";
            case NumPad6: return "NumPad6";
            case NumPad7: return "NumPad7";
            case NumPad8: return "NumPad8";
            case NumPad9: return "NumPad9";
            case Multiply: return "Multiply";
            case Add: return "Add";
            case Separator: return "Separator";
            case Subtract: return "Subtract";
            case Decimal: return "Decimal";
            case Divide: return "Divide";
            case F1: return "F1";
            case F2: return "F2";
            case F3: return "F3";
            case F4: return "F4";
            case F5: return "F5";
            case F6: return "F6";
            case F7: return "F7";
            case F8: return "F8";
            case F9: return "F9";
            case F10: return "F10";
            case F11: return "F11";
            case F12: return "F12";
            case F13: return "F13";
            case F14: return "F14";
            case F15: return "F15";
            case F16: return "F16";
            case F17: return "F17";
            case F18: return "F18";
            case F19: return "F19";
            case F20: return "F20";
            case F21: return "F21";
            case F22: return "F22";
            case F23: return "F23";
            case F24: return "F24";
            case NumLock: return "NumLock";
            case Scroll: return "Scroll";
            case LeftShift: return "LeftShift";
            case RightShift: return "RightShift";
            case LeftControl: return "LeftControl";
            case RightControl: return "RightControl";
            case LeftAlt: return "LeftAlt";
            case RightAlt: return "RightAlt";
            case BrowserBack: return "BrowserBack";
            case BrowserForward: return "BrowserForward";
            case BrowserRefresh: return "BrowserRefresh";
            case BrowserStop: return "BrowserStop";
            case BrowserSearch: return "BrowserSearch";
            case BrowserFavorites: return "BrowserFavorites";
            case BrowserHome: return "BrowserHome";
            case VolumeMute: return "VolumeMute";
            case VolumeDown: return "VolumeDown";
            case VolumeUp: return "VolumeUp";
            case MediaNextTrack: return "MediaNextTrack";
            case MediaPreviousTrack: return "MediaPreviousTrack";
            case MediaStop: return "MediaStop";
            case MediaPlayPause: return "MediaPlayPause";
            case LaunchMail: return "LaunchMail";
            case SelectMedia: return "SelectMedia";
            case LaunchApplication1: return "LaunchApplication1";
            case LaunchApplication2: return "LaunchApplication2";
            case OemSemicolon: return "OemSemicolon";
            case OemPlus: return "OemPlus";
            case OemComma: return "OemComma";
            case OemMinus: return "OemMinus";
            case OemPeriod: return "OemPeriod";
            case OemQuestion: return "OemQuestion";
            case OemTilde: return "OemTilde";
            case ChatPadGreen: return "ChatPadGreen";
            case ChatPadOrange: return "ChatPadOrange";
            case OemOpenBrackets: return "OemOpenBrackets";
            case OemPipe: return "OemPipe";
            case OemCloseBrackets: return "OemCloseBrackets";
            case OemQuotes: return "OemQuotes";
            case Oem8: return "Oem8";
            case OemBackslash: return "OemBackslash";
            case ProcessKey: return "ProcessKey";
            case OemCopy: return "OemCopy";
            case OemAuto: return "OemAuto";
            case OemEnlW: return "OemEnlW";
            case Attn: return "Attn";
            case Crsel: return "Crsel";
            case Exsel: return "Exsel";
            case EraseEof: return "EraseEof";
            case Play: return "Play";
            case Zoom: return "Zoom";
            case Pa1: return "Pa1";
            case OemClear: return "OemClear";
        }

        return "Unknown Value";
    }
};

enum CSKeyState : csint
{
    Up = 0,
    Down = 1,
};

class ECSKeyState
{
public:
    static const char* ToString(CSKeyState enumValue)
    {
        switch (enumValue)
        {
            case Up: return "Up";
            case Down: return "Down";
        }

        return "Unknown Value";
    }
};

enum CSGestureType : csint
{
    None = 0,
    Tap = 1,
    DragComplete = 2,
    Flick = 4,
    FreeDrag = 8,
    Hold = 16,
    HorizontalDrag = 32,
    Pinch = 64,
    PinchComplete = 128,
    DoubleTap = 256,
    VerticalDrag = 512,
};

class ECSGestureType
{
public:
    static const char* ToString(CSGestureType enumValue)
    {
        switch (enumValue)
        {
            case None: return "None";
            case Tap: return "Tap";
            case DragComplete: return "DragComplete";
            case Flick: return "Flick";
            case FreeDrag: return "FreeDrag";
            case Hold: return "Hold";
            case HorizontalDrag: return "HorizontalDrag";
            case Pinch: return "Pinch";
            case PinchComplete: return "PinchComplete";
            case DoubleTap: return "DoubleTap";
            case VerticalDrag: return "VerticalDrag";
        }

        return "Unknown Value";
    }
};

enum CSTouchLocationState : csint
{
    Invalid = 0,
    Moved = 1,
    Pressed = 2,
    Released = 3,
};

class ECSTouchLocationState
{
public:
    static const char* ToString(CSTouchLocationState enumValue)
    {
        switch (enumValue)
        {
            case Invalid: return "Invalid";
            case Moved: return "Moved";
            case Pressed: return "Pressed";
            case Released: return "Released";
        }

        return "Unknown Value";
    }
};

enum CSClearOptions : csint
{
    Target = 1,
    DepthBuffer = 2,
    Stencil = 4,
};

class ECSClearOptions
{
public:
    static const char* ToString(CSClearOptions enumValue)
    {
        switch (enumValue)
        {
            case Target: return "Target";
            case DepthBuffer: return "DepthBuffer";
            case Stencil: return "Stencil";
        }

        return "Unknown Value";
    }
};

enum CSColorWriteChannels : csint
{
    None = 0,
    Red = 1,
    Green = 2,
    Blue = 4,
    Alpha = 8,
    All = 15,
};

class ECSColorWriteChannels
{
public:
    static const char* ToString(CSColorWriteChannels enumValue)
    {
        switch (enumValue)
        {
            case None: return "None";
            case Red: return "Red";
            case Green: return "Green";
            case Blue: return "Blue";
            case Alpha: return "Alpha";
            case All: return "All";
        }

        return "Unknown Value";
    }
};

enum CSCubeMapFace : csint
{
    PositiveX = 0,
    NegativeX = 1,
    PositiveY = 2,
    NegativeY = 3,
    PositiveZ = 4,
    NegativeZ = 5,
};

class ECSCubeMapFace
{
public:
    static const char* ToString(CSCubeMapFace enumValue)
    {
        switch (enumValue)
        {
            case PositiveX: return "PositiveX";
            case NegativeX: return "NegativeX";
            case PositiveY: return "PositiveY";
            case NegativeY: return "NegativeY";
            case PositiveZ: return "PositiveZ";
            case NegativeZ: return "NegativeZ";
        }

        return "Unknown Value";
    }
};

enum CSEffectDirtyFlags : csint
{
    WorldViewProj = 1,
    World = 2,
    EyePosition = 4,
    MaterialColor = 8,
    Fog = 16,
    FogEnable = 32,
    AlphaTest = 64,
    ShaderIndex = 128,
    All = -1,
};

class ECSEffectDirtyFlags
{
public:
    static const char* ToString(CSEffectDirtyFlags enumValue)
    {
        switch (enumValue)
        {
            case WorldViewProj: return "WorldViewProj";
            case World: return "World";
            case EyePosition: return "EyePosition";
            case MaterialColor: return "MaterialColor";
            case Fog: return "Fog";
            case FogEnable: return "FogEnable";
            case AlphaTest: return "AlphaTest";
            case ShaderIndex: return "ShaderIndex";
            case All: return "All";
        }

        return "Unknown Value";
    }
};

enum CSEffectParameterClass : csint
{
    Scalar = 0,
    Vector = 1,
    Matrix = 2,
    Object = 3,
    Struct = 4,
};

class ECSEffectParameterClass
{
public:
    static const char* ToString(CSEffectParameterClass enumValue)
    {
        switch (enumValue)
        {
            case Scalar: return "Scalar";
            case Vector: return "Vector";
            case Matrix: return "Matrix";
            case Object: return "Object";
            case Struct: return "Struct";
        }

        return "Unknown Value";
    }
};

enum CSEffectParameterType : csint
{
    Void = 0,
    Bool = 1,
    Int32 = 2,
    Single = 3,
    String = 4,
    Texture = 5,
    Texture1D = 6,
    Texture2D = 7,
    Texture3D = 8,
    TextureCube = 9,
};

class ECSEffectParameterType
{
public:
    static const char* ToString(CSEffectParameterType enumValue)
    {
        switch (enumValue)
        {
            case Void: return "Void";
            case Bool: return "Bool";
            case Int32: return "Int32";
            case Single: return "Single";
            case String: return "String";
            case Texture: return "Texture";
            case Texture1D: return "Texture1D";
            case Texture2D: return "Texture2D";
            case Texture3D: return "Texture3D";
            case TextureCube: return "TextureCube";
        }

        return "Unknown Value";
    }
};

enum CSGraphicsDeviceStatus : csint
{
    Normal = 0,
    Lost = 1,
    NotReset = 2,
};

class ECSGraphicsDeviceStatus
{
public:
    static const char* ToString(CSGraphicsDeviceStatus enumValue)
    {
        switch (enumValue)
        {
            case Normal: return "Normal";
            case Lost: return "Lost";
            case NotReset: return "NotReset";
        }

        return "Unknown Value";
    }
};

enum CSGraphicsProfile : csint
{
    Reach = 0,
    HiDef = 1,
};

class ECSGraphicsProfile
{
public:
    static const char* ToString(CSGraphicsProfile enumValue)
    {
        switch (enumValue)
        {
            case Reach: return "Reach";
            case HiDef: return "HiDef";
        }

        return "Unknown Value";
    }
};

enum CSPresentInterval : csint
{
    Default = 0,
    One = 1,
    Two = 2,
    Immediate = 3,
};

class ECSPresentInterval
{
public:
    static const char* ToString(CSPresentInterval enumValue)
    {
        switch (enumValue)
        {
            case Default: return "Default";
            case One: return "One";
            case Two: return "Two";
            case Immediate: return "Immediate";
        }

        return "Unknown Value";
    }
};

enum CSRenderTargetUsage : csint
{
    DiscardContents = 0,
    PreserveContents = 1,
    PlatformContents = 2,
};

class ECSRenderTargetUsage
{
public:
    static const char* ToString(CSRenderTargetUsage enumValue)
    {
        switch (enumValue)
        {
            case DiscardContents: return "DiscardContents";
            case PreserveContents: return "PreserveContents";
            case PlatformContents: return "PlatformContents";
        }

        return "Unknown Value";
    }
};

enum CSSetDataOptions : csint
{
    None = 0,
    Discard = 1,
    NoOverwrite = 2,
};

class ECSSetDataOptions
{
public:
    static const char* ToString(CSSetDataOptions enumValue)
    {
        switch (enumValue)
        {
            case None: return "None";
            case Discard: return "Discard";
            case NoOverwrite: return "NoOverwrite";
        }

        return "Unknown Value";
    }
};

enum CSSamplerType : csint
{
    Sampler2D = 0,
    SamplerCube = 1,
    SamplerVolume = 2,
    Sampler1D = 3,
};

class ECSSamplerType
{
public:
    static const char* ToString(CSSamplerType enumValue)
    {
        switch (enumValue)
        {
            case Sampler2D: return "Sampler2D";
            case SamplerCube: return "SamplerCube";
            case SamplerVolume: return "SamplerVolume";
            case Sampler1D: return "Sampler1D";
        }

        return "Unknown Value";
    }
};

enum CSShaderStage : csint
{
    Vertex = 0,
    Pixel = 1,
};

class ECSShaderStage
{
public:
    static const char* ToString(CSShaderStage enumValue)
    {
        switch (enumValue)
        {
            case Vertex: return "Vertex";
            case Pixel: return "Pixel";
        }

        return "Unknown Value";
    }
};

enum CSSpriteEffects : csint
{
    None = 0,
    FlipHorizontally = 1,
    FlipVertically = 2,
};

class ECSSpriteEffects
{
public:
    static const char* ToString(CSSpriteEffects enumValue)
    {
        switch (enumValue)
        {
            case None: return "None";
            case FlipHorizontally: return "FlipHorizontally";
            case FlipVertically: return "FlipVertically";
        }

        return "Unknown Value";
    }
};

enum CSSpriteSortMode : csint
{
    Deferred = 0,
    Immediate = 1,
    Texture = 2,
    BackToFront = 3,
    FrontToBack = 4,
};

class ECSSpriteSortMode
{
public:
    static const char* ToString(CSSpriteSortMode enumValue)
    {
        switch (enumValue)
        {
            case Deferred: return "Deferred";
            case Immediate: return "Immediate";
            case Texture: return "Texture";
            case BackToFront: return "BackToFront";
            case FrontToBack: return "FrontToBack";
        }

        return "Unknown Value";
    }
};

enum CSBlend : csint
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

class ECSBlend
{
public:
    static const char* ToString(CSBlend enumValue)
    {
        switch (enumValue)
        {
            case One: return "One";
            case Zero: return "Zero";
            case SourceColor: return "SourceColor";
            case InverseSourceColor: return "InverseSourceColor";
            case SourceAlpha: return "SourceAlpha";
            case InverseSourceAlpha: return "InverseSourceAlpha";
            case DestinationColor: return "DestinationColor";
            case InverseDestinationColor: return "InverseDestinationColor";
            case DestinationAlpha: return "DestinationAlpha";
            case InverseDestinationAlpha: return "InverseDestinationAlpha";
            case BlendFactor: return "BlendFactor";
            case InverseBlendFactor: return "InverseBlendFactor";
            case SourceAlphaSaturation: return "SourceAlphaSaturation";
        }

        return "Unknown Value";
    }
};

enum CSBlendFunction : csint
{
    Add = 0,
    Subtract = 1,
    ReverseSubtract = 2,
    Min = 3,
    Max = 4,
};

class ECSBlendFunction
{
public:
    static const char* ToString(CSBlendFunction enumValue)
    {
        switch (enumValue)
        {
            case Add: return "Add";
            case Subtract: return "Subtract";
            case ReverseSubtract: return "ReverseSubtract";
            case Min: return "Min";
            case Max: return "Max";
        }

        return "Unknown Value";
    }
};

enum CSCompareFunction : csint
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

class ECSCompareFunction
{
public:
    static const char* ToString(CSCompareFunction enumValue)
    {
        switch (enumValue)
        {
            case Always: return "Always";
            case Never: return "Never";
            case Less: return "Less";
            case LessEqual: return "LessEqual";
            case Equal: return "Equal";
            case GreaterEqual: return "GreaterEqual";
            case Greater: return "Greater";
            case NotEqual: return "NotEqual";
        }

        return "Unknown Value";
    }
};

enum CSCullMode : csint
{
    None = 0,
    CullClockwiseFace = 1,
    CullCounterClockwiseFace = 2,
};

class ECSCullMode
{
public:
    static const char* ToString(CSCullMode enumValue)
    {
        switch (enumValue)
        {
            case None: return "None";
            case CullClockwiseFace: return "CullClockwiseFace";
            case CullCounterClockwiseFace: return "CullCounterClockwiseFace";
        }

        return "Unknown Value";
    }
};

enum CSDepthFormat : csint
{
    None = 0,
    Depth16 = 1,
    Depth24 = 2,
    Depth24Stencil8 = 3,
};

class ECSDepthFormat
{
public:
    static const char* ToString(CSDepthFormat enumValue)
    {
        switch (enumValue)
        {
            case None: return "None";
            case Depth16: return "Depth16";
            case Depth24: return "Depth24";
            case Depth24Stencil8: return "Depth24Stencil8";
        }

        return "Unknown Value";
    }
};

enum CSFillMode : csint
{
    Solid = 0,
    WireFrame = 1,
};

class ECSFillMode
{
public:
    static const char* ToString(CSFillMode enumValue)
    {
        switch (enumValue)
        {
            case Solid: return "Solid";
            case WireFrame: return "WireFrame";
        }

        return "Unknown Value";
    }
};

enum CSStencilOperation : csint
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

class ECSStencilOperation
{
public:
    static const char* ToString(CSStencilOperation enumValue)
    {
        switch (enumValue)
        {
            case Keep: return "Keep";
            case Zero: return "Zero";
            case Replace: return "Replace";
            case Increment: return "Increment";
            case Decrement: return "Decrement";
            case IncrementSaturation: return "IncrementSaturation";
            case DecrementSaturation: return "DecrementSaturation";
            case Invert: return "Invert";
        }

        return "Unknown Value";
    }
};

enum CSTextureAddressMode : csint
{
    Wrap = 0,
    Clamp = 1,
    Mirror = 2,
    Border = 3,
};

class ECSTextureAddressMode
{
public:
    static const char* ToString(CSTextureAddressMode enumValue)
    {
        switch (enumValue)
        {
            case Wrap: return "Wrap";
            case Clamp: return "Clamp";
            case Mirror: return "Mirror";
            case Border: return "Border";
        }

        return "Unknown Value";
    }
};

enum CSTextureFilter : csint
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

class ECSTextureFilter
{
public:
    static const char* ToString(CSTextureFilter enumValue)
    {
        switch (enumValue)
        {
            case Linear: return "Linear";
            case Point: return "Point";
            case Anisotropic: return "Anisotropic";
            case LinearMipPoint: return "LinearMipPoint";
            case PointMipLinear: return "PointMipLinear";
            case MinLinearMagPointMipLinear: return "MinLinearMagPointMipLinear";
            case MinLinearMagPointMipPoint: return "MinLinearMagPointMipPoint";
            case MinPointMagLinearMipLinear: return "MinPointMagLinearMipLinear";
            case MinPointMagLinearMipPoint: return "MinPointMagLinearMipPoint";
        }

        return "Unknown Value";
    }
};

enum CSTextureFilterMode : csint
{
    Default = 0,
    Comparison = 1,
};

class ECSTextureFilterMode
{
public:
    static const char* ToString(CSTextureFilterMode enumValue)
    {
        switch (enumValue)
        {
            case Default: return "Default";
            case Comparison: return "Comparison";
        }

        return "Unknown Value";
    }
};

enum CSSurfaceFormat : csint
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

class ECSSurfaceFormat
{
public:
    static const char* ToString(CSSurfaceFormat enumValue)
    {
        switch (enumValue)
        {
            case Color: return "Color";
            case Bgr565: return "Bgr565";
            case Bgra5551: return "Bgra5551";
            case Bgra4444: return "Bgra4444";
            case Dxt1: return "Dxt1";
            case Dxt3: return "Dxt3";
            case Dxt5: return "Dxt5";
            case NormalizedByte2: return "NormalizedByte2";
            case NormalizedByte4: return "NormalizedByte4";
            case Rgba1010102: return "Rgba1010102";
            case Rg32: return "Rg32";
            case Rgba64: return "Rgba64";
            case Alpha8: return "Alpha8";
            case Single: return "Single";
            case Vector2: return "Vector2";
            case Vector4: return "Vector4";
            case HalfSingle: return "HalfSingle";
            case HalfVector2: return "HalfVector2";
            case HalfVector4: return "HalfVector4";
            case HdrBlendable: return "HdrBlendable";
            case Bgr32: return "Bgr32";
            case Bgra32: return "Bgra32";
            case ColorSRgb: return "ColorSRgb";
            case Bgr32SRgb: return "Bgr32SRgb";
            case Bgra32SRgb: return "Bgra32SRgb";
            case Dxt1SRgb: return "Dxt1SRgb";
            case Dxt3SRgb: return "Dxt3SRgb";
            case Dxt5SRgb: return "Dxt5SRgb";
            case RgbPvrtc2Bpp: return "RgbPvrtc2Bpp";
            case RgbPvrtc4Bpp: return "RgbPvrtc4Bpp";
            case RgbaPvrtc2Bpp: return "RgbaPvrtc2Bpp";
            case RgbaPvrtc4Bpp: return "RgbaPvrtc4Bpp";
            case RgbEtc1: return "RgbEtc1";
            case Dxt1a: return "Dxt1a";
            case RgbaAtcExplicitAlpha: return "RgbaAtcExplicitAlpha";
            case RgbaAtcInterpolatedAlpha: return "RgbaAtcInterpolatedAlpha";
            case Rgb8Etc2: return "Rgb8Etc2";
            case Srgb8Etc2: return "Srgb8Etc2";
            case Rgb8A1Etc2: return "Rgb8A1Etc2";
            case Srgb8A1Etc2: return "Srgb8A1Etc2";
            case Rgba8Etc2: return "Rgba8Etc2";
            case SRgb8A8Etc2: return "SRgb8A8Etc2";
        }

        return "Unknown Value";
    }
};

enum CSBufferUsage : csint
{
    None = 0,
    WriteOnly = 1,
};

class ECSBufferUsage
{
public:
    static const char* ToString(CSBufferUsage enumValue)
    {
        switch (enumValue)
        {
            case None: return "None";
            case WriteOnly: return "WriteOnly";
        }

        return "Unknown Value";
    }
};

enum CSIndexElementSize : csint
{
    SixteenBits = 0,
    ThirtyTwoBits = 1,
};

class ECSIndexElementSize
{
public:
    static const char* ToString(CSIndexElementSize enumValue)
    {
        switch (enumValue)
        {
            case SixteenBits: return "SixteenBits";
            case ThirtyTwoBits: return "ThirtyTwoBits";
        }

        return "Unknown Value";
    }
};

enum CSPrimitiveType : csint
{
    TriangleList = 0,
    TriangleStrip = 1,
    LineList = 2,
    LineStrip = 3,
    PointList = 4,
};

class ECSPrimitiveType
{
public:
    static const char* ToString(CSPrimitiveType enumValue)
    {
        switch (enumValue)
        {
            case TriangleList: return "TriangleList";
            case TriangleStrip: return "TriangleStrip";
            case LineList: return "LineList";
            case LineStrip: return "LineStrip";
            case PointList: return "PointList";
        }

        return "Unknown Value";
    }
};

enum CSVertexElementFormat : csint
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

class ECSVertexElementFormat
{
public:
    static const char* ToString(CSVertexElementFormat enumValue)
    {
        switch (enumValue)
        {
            case Single: return "Single";
            case Vector2: return "Vector2";
            case Vector3: return "Vector3";
            case Vector4: return "Vector4";
            case Color: return "Color";
            case Byte4: return "Byte4";
            case Short2: return "Short2";
            case Short4: return "Short4";
            case NormalizedShort2: return "NormalizedShort2";
            case NormalizedShort4: return "NormalizedShort4";
            case HalfVector2: return "HalfVector2";
            case HalfVector4: return "HalfVector4";
        }

        return "Unknown Value";
    }
};

enum CSVertexElementUsage : csint
{
    Position = 0,
    Color = 1,
    TextureCoordinate = 2,
    Normal = 3,
    Binormal = 4,
    Tangent = 5,
    BlendIndices = 6,
    BlendWeight = 7,
    Depth = 8,
    Fog = 9,
    PointSize = 10,
    Sample = 11,
    TessellateFactor = 12,
};

class ECSVertexElementUsage
{
public:
    static const char* ToString(CSVertexElementUsage enumValue)
    {
        switch (enumValue)
        {
            case Position: return "Position";
            case Color: return "Color";
            case TextureCoordinate: return "TextureCoordinate";
            case Normal: return "Normal";
            case Binormal: return "Binormal";
            case Tangent: return "Tangent";
            case BlendIndices: return "BlendIndices";
            case BlendWeight: return "BlendWeight";
            case Depth: return "Depth";
            case Fog: return "Fog";
            case PointSize: return "PointSize";
            case Sample: return "Sample";
            case TessellateFactor: return "TessellateFactor";
        }

        return "Unknown Value";
    }
};

enum CSAudioChannels : csint
{
    Mono = 1,
    Stereo = 2,
};

class ECSAudioChannels
{
public:
    static const char* ToString(CSAudioChannels enumValue)
    {
        switch (enumValue)
        {
            case Mono: return "Mono";
            case Stereo: return "Stereo";
        }

        return "Unknown Value";
    }
};

enum CSMicrophoneState : csint
{
    Started = 0,
    Stopped = 1,
};

class ECSMicrophoneState
{
public:
    static const char* ToString(CSMicrophoneState enumValue)
    {
        switch (enumValue)
        {
            case Started: return "Started";
            case Stopped: return "Stopped";
        }

        return "Unknown Value";
    }
};

enum CSSoundState : csint
{
    Playing = 0,
    Paused = 1,
    Stopped = 2,
};

class ECSSoundState
{
public:
    static const char* ToString(CSSoundState enumValue)
    {
        switch (enumValue)
        {
            case Playing: return "Playing";
            case Paused: return "Paused";
            case Stopped: return "Stopped";
        }

        return "Unknown Value";
    }
};

enum CSAudioStopOptions : csint
{
    AsAuthored = 0,
    Immediate = 1,
};

class ECSAudioStopOptions
{
public:
    static const char* ToString(CSAudioStopOptions enumValue)
    {
        switch (enumValue)
        {
            case AsAuthored: return "AsAuthored";
            case Immediate: return "Immediate";
        }

        return "Unknown Value";
    }
};

enum CSCrossfadeType : csint
{
    Linear = 0,
    Logarithmic = 1,
    EqualPower = 2,
};

class ECSCrossfadeType
{
public:
    static const char* ToString(CSCrossfadeType enumValue)
    {
        switch (enumValue)
        {
            case Linear: return "Linear";
            case Logarithmic: return "Logarithmic";
            case EqualPower: return "EqualPower";
        }

        return "Unknown Value";
    }
};

enum CSFilterMode : csint
{
    LowPass = 0,
    BandPass = 1,
    HighPass = 2,
};

class ECSFilterMode
{
public:
    static const char* ToString(CSFilterMode enumValue)
    {
        switch (enumValue)
        {
            case LowPass: return "LowPass";
            case BandPass: return "BandPass";
            case HighPass: return "HighPass";
        }

        return "Unknown Value";
    }
};

enum CSMaxInstanceBehavior : csint
{
    FailToPlay = 0,
    Queue = 1,
    ReplaceOldest = 2,
    ReplaceQuietest = 3,
    ReplaceLowestPriority = 4,
};

class ECSMaxInstanceBehavior
{
public:
    static const char* ToString(CSMaxInstanceBehavior enumValue)
    {
        switch (enumValue)
        {
            case FailToPlay: return "FailToPlay";
            case Queue: return "Queue";
            case ReplaceOldest: return "ReplaceOldest";
            case ReplaceQuietest: return "ReplaceQuietest";
            case ReplaceLowestPriority: return "ReplaceLowestPriority";
        }

        return "Unknown Value";
    }
};

enum CSMiniFormatTag : csint
{
    Pcm = 0,
    Xma = 1,
    Xma = 1,
    Adpcm = 2,
    Wma = 3,
};

class ECSMiniFormatTag
{
public:
    static const char* ToString(CSMiniFormatTag enumValue)
    {
        switch (enumValue)
        {
            case Pcm: return "Pcm";
            case Xma: return "Xma";
            case Xma: return "Xma";
            case Adpcm: return "Adpcm";
            case Wma: return "Wma";
        }

        return "Unknown Value";
    }
};

enum CSVariationType : csint
{
    Ordered = 0,
    OrderedFromRandom = 1,
    Random = 2,
    RandomNoImmediateRepeats = 3,
    Shuffle = 4,
};

class ECSVariationType
{
public:
    static const char* ToString(CSVariationType enumValue)
    {
        switch (enumValue)
        {
            case Ordered: return "Ordered";
            case OrderedFromRandom: return "OrderedFromRandom";
            case Random: return "Random";
            case RandomNoImmediateRepeats: return "RandomNoImmediateRepeats";
            case Shuffle: return "Shuffle";
        }

        return "Unknown Value";
    }
};

enum CSRpcParameter : csint
{
    Volume = 0,
    Pitch = 1,
    ReverbSend = 2,
    FilterFrequency = 3,
    FilterQFactor = 4,
    NumParameters = 5,
};

class ECSRpcParameter
{
public:
    static const char* ToString(CSRpcParameter enumValue)
    {
        switch (enumValue)
        {
            case Volume: return "Volume";
            case Pitch: return "Pitch";
            case ReverbSend: return "ReverbSend";
            case FilterFrequency: return "FilterFrequency";
            case FilterQFactor: return "FilterQFactor";
            case NumParameters: return "NumParameters";
        }

        return "Unknown Value";
    }
};

enum CSRpcPointType : csint
{
    Linear = 0,
    Fast = 1,
    Slow = 2,
    SinCos = 3,
};

class ECSRpcPointType
{
public:
    static const char* ToString(CSRpcPointType enumValue)
    {
        switch (enumValue)
        {
            case Linear: return "Linear";
            case Fast: return "Fast";
            case Slow: return "Slow";
            case SinCos: return "SinCos";
        }

        return "Unknown Value";
    }
};

