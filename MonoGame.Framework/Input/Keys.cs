// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace Microsoft.Xna.Framework.Input
{
    /// <summary>
    /// Defines the keys on a keyboard.
    /// </summary>	
	public enum Keys
	{
        /// <summary>
        /// Reserved.
        /// </summary>
		None = 0,
        /// <summary>
        /// BACKSPACE key.
        /// </summary>
		Back = 8,
        /// <summary>
        /// TAB key.
        /// </summary>
		Tab = 9,
        /// <summary>
        /// ENTER key.
        /// </summary>
		Enter = 13,
        /// <summary>
        /// CAPS LOCK key.
        /// </summary>
		CapsLock = 20,
        /// <summary>
        /// ESC key.
        /// </summary>
		Escape = 27,
        /// <summary>
        /// SPACEBAR key.
        /// </summary>
		Space = 32,
        /// <summary>
        /// PAGE UP key.
        /// </summary>
		PageUp = 33,
        /// <summary>
        /// PAGE DOWN key.
        /// </summary>
		PageDown = 34,
        /// <summary>
        /// END key.
        /// </summary>
		End = 35,
        /// <summary>
        /// HOME key.
        /// </summary>
		Home = 36,
        /// <summary>
        /// LEFT ARROW key.
        /// </summary>
		Left = 37,
        /// <summary>
        /// UP ARROW key.
        /// </summary>
		Up = 38,
        /// <summary>
        /// RIGHT ARROW key.
        /// </summary>
		Right = 39,
        /// <summary>
        /// DOWN ARROW key.
        /// </summary>
		Down = 40,
        /// <summary>
        /// SELECT key.
        /// </summary>
		Select = 41,
        /// <summary>
        /// PRINT key.
        /// </summary>
		Print = 42,
        /// <summary>
        /// EXECUTE key.
        /// </summary>
		Execute = 43,
        /// <summary>
        /// PRINT SCREEN key.
        /// </summary>
		PrintScreen = 44,
        /// <summary>
        /// INS key.
        /// </summary>
		Insert = 45,
        /// <summary>
        /// DEL key.
        /// </summary>
		Delete = 46,
        /// <summary>
        /// HELP key.
        /// </summary>
		Help = 47,
        /// <summary>
        /// Used for miscellaneous characters; it can vary by keyboard.
        /// </summary>
		D0 = 48,
        /// <summary>
        /// Used for miscellaneous characters; it can vary by keyboard.
        /// </summary>
		D1 = 49,
        /// <summary>
        /// Used for miscellaneous characters; it can vary by keyboard.
        /// </summary>
		D2 = 50,
        /// <summary>
        /// Used for miscellaneous characters; it can vary by keyboard.
        /// </summary>
		D3 = 51,
        /// <summary>
        /// Used for miscellaneous characters; it can vary by keyboard.
        /// </summary>
		D4 = 52,
        /// <summary>
        /// Used for miscellaneous characters; it can vary by keyboard.
        /// </summary>
		D5 = 53,
        /// <summary>
        /// Used for miscellaneous characters; it can vary by keyboard.
        /// </summary>
		D6 = 54,
        /// <summary>
        /// Used for miscellaneous characters; it can vary by keyboard.
        /// </summary>
		D7 = 55,
        /// <summary>
        /// Used for miscellaneous characters; it can vary by keyboard.
        /// </summary>
		D8 = 56,
        /// <summary>
        /// Used for miscellaneous characters; it can vary by keyboard.
        /// </summary>
		D9 = 57,
        /// <summary>
        /// A key.
        /// </summary>
		A = 65,
        /// <summary>
        /// B key.
        /// </summary>
		B = 66,
        /// <summary>
        /// C key.
        /// </summary>
		C = 67,
        /// <summary>
        /// D key.
        /// </summary>
		D = 68,
        /// <summary>
        /// E key.
        /// </summary>
		E = 69,
        /// <summary>
        /// F key.
        /// </summary>
		F = 70,
        /// <summary>
        /// G key.
        /// </summary>
		G = 71,
        /// <summary>
        /// H key.
        /// </summary>
		H = 72,
        /// <summary>
        /// I key.
        /// </summary>
		I = 73,
        /// <summary>
        /// J key.
        /// </summary>
		J = 74,
        /// <summary>
        /// K key.
        /// </summary>
		K = 75,
        /// <summary>
        /// L key.
        /// </summary>
		L = 76,
        /// <summary>
        /// M key.
        /// </summary>
		M = 77,
        /// <summary>
        /// N key.
        /// </summary>
		N = 78,
        /// <summary>
        /// O key.
        /// </summary>
		O = 79,
        /// <summary>
        /// P key.
        /// </summary>
		P = 80,
        /// <summary>
        /// Q key.
        /// </summary>
		Q = 81,
        /// <summary>
        /// R key.
        /// </summary>
		R = 82,
        /// <summary>
        /// S key.
        /// </summary>
		S = 83,
        /// <summary>
        /// T key.
        /// </summary>
		T = 84,
        /// <summary>
        /// U key.
        /// </summary>
		U = 85,
        /// <summary>
        /// V key.
        /// </summary>
		V = 86,
        /// <summary>
        /// W key.
        /// </summary>
		W = 87,
        /// <summary>
        /// X key.
        /// </summary>
		X = 88,
        /// <summary>
        /// Y key.
        /// </summary>
		Y = 89,
        /// <summary>
        /// Z key.
        /// </summary>
		Z = 90,
        /// <summary>
        /// Left Windows key.
        /// </summary>
		LeftWindows = 91,
        /// <summary>
        /// Right Windows key.
        /// </summary>
		RightWindows = 92,
        /// <summary>
        /// Applications key.
        /// </summary>
		Apps = 93,
        /// <summary>
        /// Computer Sleep key.
        /// </summary>
		Sleep = 95,
        /// <summary>
        /// Numeric keypad 0 key.
        /// </summary>
		NumPad0 = 96,
        /// <summary>
        /// Numeric keypad 1 key.
        /// </summary>
		NumPad1 = 97,
        /// <summary>
        /// Numeric keypad 2 key.
        /// </summary>
		NumPad2 = 98,
        /// <summary>
        /// Numeric keypad 3 key.
        /// </summary>
		NumPad3 = 99,
        /// <summary>
        /// Numeric keypad 4 key.
        /// </summary>
		NumPad4 = 100,
        /// <summary>
        /// Numeric keypad 5 key.
        /// </summary>
		NumPad5 = 101,
        /// <summary>
        /// Numeric keypad 6 key.
        /// </summary>
		NumPad6 = 102,
        /// <summary>
        /// Numeric keypad 7 key.
        /// </summary>
		NumPad7 = 103,
        /// <summary>
        /// Numeric keypad 8 key.
        /// </summary>
		NumPad8 = 104,
        /// <summary>
        /// Numeric keypad 9 key.
        /// </summary>
		NumPad9 = 105,
        /// <summary>
        /// Multiply key.
        /// </summary>
		Multiply = 106,
        /// <summary>
        /// Add key.
        /// </summary>
		Add = 107,
        /// <summary>
        /// Separator key.
        /// </summary>
		Separator = 108,
        /// <summary>
        /// Subtract key.
        /// </summary>
		Subtract = 109,
        /// <summary>
        /// Decimal key.
        /// </summary>
		Decimal = 110,
        /// <summary>
        /// Divide key.
        /// </summary>
		Divide = 111,
        /// <summary>
        /// F1 key.
        /// </summary>
		F1 = 112,
        /// <summary>
        /// F2 key.
        /// </summary>
		F2 = 113,
        /// <summary>
        /// F3 key.
        /// </summary>
		F3 = 114,
        /// <summary>
        /// F4 key.
        /// </summary>
		F4 = 115,
        /// <summary>
        /// F5 key.
        /// </summary>
		F5 = 116,
        /// <summary>
        /// F6 key.
        /// </summary>
		F6 = 117,
        /// <summary>
        /// F7 key.
        /// </summary>
		F7 = 118,
        /// <summary>
        /// F8 key.
        /// </summary>
		F8 = 119,
        /// <summary>
        /// F9 key.
        /// </summary>
		F9 = 120,
        /// <summary>
        /// F10 key.
        /// </summary>
		F10 = 121,
        /// <summary>
        /// F11 key.
        /// </summary>
		F11 = 122,
        /// <summary>
        /// F12 key.
        /// </summary>
		F12 = 123,
        /// <summary>
        /// F13 key.
        /// </summary>
		F13 = 124,
        /// <summary>
        /// F14 key.
        /// </summary>
		F14 = 125,
        /// <summary>
        /// F15 key.
        /// </summary>
		F15 = 126,
        /// <summary>
        /// F16 key.
        /// </summary>
		F16 = 127,
        /// <summary>
        /// F17 key.
        /// </summary>
		F17 = 128,
        /// <summary>
        /// F18 key.
        /// </summary>
		F18 = 129,
        /// <summary>
        /// F19 key.
        /// </summary>
		F19 = 130,
        /// <summary>
        /// F20 key.
        /// </summary>
		F20 = 131,
        /// <summary>
        /// F21 key.
        /// </summary>
		F21 = 132,
        /// <summary>
        /// F22 key.
        /// </summary>
		F22 = 133,
        /// <summary>
        /// F23 key.
        /// </summary>
		F23 = 134,
        /// <summary>
        /// F24 key.
        /// </summary>
		F24 = 135,
        /// <summary>
        /// NUM LOCK key.
        /// </summary>
		NumLock = 144,
        /// <summary>
        /// SCROLL LOCK key.
        /// </summary>
		Scroll = 145,
        /// <summary>
        /// Left SHIFT key.
        /// </summary>
		LeftShift = 160,
        /// <summary>
        /// Right SHIFT key.
        /// </summary>
		RightShift = 161,
        /// <summary>
        /// Left CONTROL key.
        /// </summary>
		LeftControl = 162,
        /// <summary>
        /// Right CONTROL key.
        /// </summary>
		RightControl = 163,
        /// <summary>
        /// Left ALT key.
        /// </summary>
		LeftAlt = 164,
        /// <summary>
        /// Right ALT key.
        /// </summary>
		RightAlt = 165,
        /// <summary>
        /// Browser Back key.
        /// </summary>
		BrowserBack = 166,
        /// <summary>
        /// Browser Forward key.
        /// </summary>
		BrowserForward = 167,
        /// <summary>
        /// Browser Refresh key.
        /// </summary>
		BrowserRefresh = 168,
        /// <summary>
        /// Browser Stop key.
        /// </summary>
		BrowserStop = 169,
        /// <summary>
        /// Browser Search key.
        /// </summary>
		BrowserSearch = 170,
        /// <summary>
        /// Browser Favorites key.
        /// </summary>
		BrowserFavorites = 171,
        /// <summary>
        /// Browser Start and Home key.
        /// </summary>
		BrowserHome = 172,
		/// <summary>
        /// Volume Mute key.
		/// </summary>
        VolumeMute = 173,
        /// <summary>
        /// Volume Down key.
        /// </summary>
		VolumeDown = 174,
        /// <summary>
        /// Volume Up key.
        /// </summary>
		VolumeUp = 175,
        /// <summary>
        /// Next Track key.
        /// </summary>
		MediaNextTrack = 176,
        /// <summary>
        /// Previous Track key.
        /// </summary>
		MediaPreviousTrack = 177,
        /// <summary>
        /// Stop Media key.
        /// </summary>
		MediaStop = 178,
        /// <summary>
        /// Play/Pause Media key.
        /// </summary>
		MediaPlayPause = 179,
        /// <summary>
        /// Start Mail key.
        /// </summary>
		LaunchMail = 180,
        /// <summary>
        /// Select Media key.
        /// </summary>
		SelectMedia = 181,
        /// <summary>
        /// Start Application 1 key.
        /// </summary>
		LaunchApplication1 = 182,
        /// <summary>
        /// Start Application 2 key.
        /// </summary>
		LaunchApplication2 = 183,
        /// <summary>
        /// The OEM Semicolon key on a US standard keyboard.
        /// </summary>
		OemSemicolon = 186,
        /// <summary>
        /// For any country/region, the '+' key.
        /// </summary>
		OemPlus = 187,
        /// <summary>
        /// For any country/region, the ',' key.
        /// </summary>
		OemComma = 188,
        /// <summary>
        /// For any country/region, the '-' key.
        /// </summary>
		OemMinus = 189,
        /// <summary>
        /// For any country/region, the '.' key.
        /// </summary>
		OemPeriod = 190,
        /// <summary>
        /// The OEM question mark key on a US standard keyboard.
        /// </summary>
		OemQuestion = 191,
        /// <summary>
        /// The OEM tilde key on a US standard keyboard.
        /// </summary>
		OemTilde = 192,
        /// <summary>
        /// The OEM open bracket key on a US standard keyboard.
        /// </summary>
		OemOpenBrackets = 219,
        /// <summary>
        /// The OEM pipe key on a US standard keyboard.
        /// </summary>
		OemPipe = 220,
        /// <summary>
        /// The OEM close bracket key on a US standard keyboard.
        /// </summary>
		OemCloseBrackets = 221,
        /// <summary>
        /// The OEM singled/double quote key on a US standard keyboard.
        /// </summary>
		OemQuotes = 222,
        /// <summary>
        /// Used for miscellaneous characters; it can vary by keyboard.
        /// </summary>
		Oem8 = 223,
        /// <summary>
        /// The OEM angle bracket or backslash key on the RT 102 key keyboard.
        /// </summary>
		OemBackslash = 226,
        /// <summary>
        /// IME PROCESS key.
        /// </summary>
		ProcessKey = 229,
        /// <summary>
        /// Attn key.
        /// </summary>
		Attn = 246,
        /// <summary>
        /// CrSel key.
        /// </summary>
		Crsel = 247,
        /// <summary>
        /// ExSel key.
        /// </summary>
		Exsel = 248,
        /// <summary>
        /// Erase EOF key.
        /// </summary>
		EraseEof = 249,
        /// <summary>
        /// Play key.
        /// </summary>
		Play = 250,
        /// <summary>
        /// Zoom key.
        /// </summary>
		Zoom = 251,
        /// <summary>
        /// PA1 key.
        /// </summary>
		Pa1 = 253,
        /// <summary>
        /// CLEAR key.
        /// </summary>
		OemClear = 254,
        /// <summary>
        /// Green ChatPad key.
        /// </summary>
		ChatPadGreen = 0xCA,
        /// <summary>
        /// Orange ChatPad key.
        /// </summary>
		ChatPadOrange = 0xCB,
        /// <summary>
        /// PAUSE key.
        /// </summary>
		Pause = 0x13,
        /// <summary>
        /// IME Convert key.
        /// </summary>
		ImeConvert = 0x1c,
        /// <summary>
        /// IME NoConvert key.
        /// </summary>
		ImeNoConvert = 0x1d,
        /// <summary>
        /// Kana key on Japanese keyboards.
        /// </summary>
		Kana = 0x15,
        /// <summary>
        /// Kanji key on Japanese keyboards.
        /// </summary>
		Kanji = 0x19,
        /// <summary>
        /// OEM Auto key.
        /// </summary>
		OemAuto = 0xf3,
        /// <summary>
        /// OEM Copy key.
        /// </summary>
		OemCopy = 0xf2,
        /// <summary>
        /// OEM Enlarge Window key.
        /// </summary>
		OemEnlW = 0xf4
	}
}
