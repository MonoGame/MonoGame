#region License
// /*
// Microsoft Public License (Ms-PL)
// MonoGame - Copyright © 2009 The MonoGame Team
// 
// All rights reserved.
// 
// This license governs use of the accompanying software. If you use the software, you accept this license. If you do not
// accept the license, do not use the software.
// 
// 1. Definitions
// The terms "reproduce, " "reproduction,  " "derivative works,  " and "distribution" have the same meaning here as under 
// U.S. copyright law.
// 
// A "contribution" is the original software, or any additions or changes to the software.
// A "contributor" is any person that distributes its contribution under this license.
// "Licensed patents" are a contributor's patent claims that read directly on its contribution.
// 
// 2. Grant of Rights
// (A) Copyright Grant- Subject to the terms of this license, including the license conditions and limitations in section 3,  
// each contributor grants you a non-exclusive, worldwide,  royalty-free copyright license to reproduce its contribution, prepare derivative works of its contribution, and distribute its contribution or any derivative works that you create.
// (B) Patent Grant- Subject to the terms of this license, including the license conditions and limitations in section 3,  
// each contributor grants you a non-exclusive, worldwide, royalty-free license under its licensed patents to make,  have made, use,  sell, offer for sale, import,  and/or otherwise dispose of its contribution in the software or derivative works of the contribution in the software.
// 
// 3. Conditions and Limitations
// (A) No Trademark License- This license does not grant you rights to use any contributors' name, logo, or trademarks.
// (B) If you bring a patent claim against any contributor over patents that you claim are infringed by the software, 
// your patent license from such contributor to the software ends automatically.
// (C) If you distribute any portion of the software, you must retain all copyright, patent, trademark, and attribution 
// notices that are present in the software.
// (D) If you distribute any portion of the software in source code form, you may do so only under this license by including 
// a complete copy of this license with your distribution. If you distribute any portion of the software in compiled or object 
// code form, you may only do so under a license that complies with this license.
// (E) The software is licensed "as-is." You bear the risk of using it. The contributors give no express warranties, guarantees
// or conditions. You may have additional consumer rights under your local laws which this license cannot change. To the extent
// permitted under your local laws, the contributors exclude the implied warranties of merchantability, fitness for a particular
// purpose and non-infringement.
// */
#endregion License

#region Using Statements
using System;
#endregion Using Statements

namespace Microsoft.Xna.Framework.Input
{
    /// <summary>
    /// Identifies a particular key on a keyboard.
    /// </summary>
	[Flags]
	public enum Keys
	{
        /// <summary>
        /// Reserved
        /// </summary>
		None = 0,
        /// <summary>
        /// BACKSPACE key
        /// </summary>
		Back = 8,
        /// <summary>
        /// TAB key
        /// </summary>
		Tab = 9,
        /// <summary>
        /// ENTER key
        /// </summary>
		Enter = 13,
        /// <summary>
        /// CAPS LOCK key
        /// </summary>
		CapsLock = 20,
        /// <summary>
        /// ESC key
        /// </summary>
		Escape = 27,
        /// <summary>
        /// SPACEBAR
        /// </summary>
		Space = 32,
        /// <summary>
        /// PAGE UP key
        /// </summary>
		PageUp = 33,
        /// <summary>
        /// PAGE DOWN key
        /// </summary>
		PageDown = 34,
        /// <summary>
        /// END key
        /// </summary>
		End = 35,
        /// <summary>
        /// HOME key
        /// </summary>
		Home = 36,
        /// <summary>
        /// LEFT ARROW key
        /// </summary>
		Left = 37,
        /// <summary>
        /// UP ARROW key
        /// </summary>
		Up = 38,
        /// <summary>
        /// RIGHT ARROW key
        /// </summary>
		Right = 39,
        /// <summary>
        /// DOWN ARROW key
        /// </summary>
		Down = 40,
        /// <summary>
        /// SELECT key
        /// </summary>
		Select = 41,
        /// <summary>
        /// PRINT key
        /// </summary>
		Print = 42,
        /// <summary>
        /// EXECUTE key
        /// </summary>
		Execute = 43,
        /// <summary>
        /// PRINT SCREEN key
        /// </summary>
		PrintScreen = 44,
        /// <summary>
        /// INS key
        /// </summary>
		Insert = 45,
        /// <summary>
        /// DEL key
        /// </summary>
		Delete = 46,
        /// <summary>
        /// HELP key
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
        /// A key
        /// </summary>
		A = 65,
        /// <summary>
        /// B key
        /// </summary>
		B = 66,
        /// <summary>
        /// C key
        /// </summary>
		C = 67,
        /// <summary>
        /// D key
        /// </summary>
		D = 68,
        /// <summary>
        /// E key
        /// </summary>
		E = 69,
        /// <summary>
        /// F key
        /// </summary>
		F = 70,
        /// <summary>
        /// G key
        /// </summary>
		G = 71,
        /// <summary>
        /// H key
        /// </summary>
		H = 72,
        /// <summary>
        /// I key
        /// </summary>
		I = 73,
        /// <summary>
        /// J key
        /// </summary>
		J = 74,
        /// <summary>
        /// K key
        /// </summary>
		K = 75,
        /// <summary>
        /// L key
        /// </summary>
		L = 76,
        /// <summary>
        /// M key
        /// </summary>
		M = 77,
        /// <summary>
        /// N key
        /// </summary>
		N = 78,
        /// <summary>
        /// O key
        /// </summary>
		O = 79,
        /// <summary>
        /// P key
        /// </summary>
		P = 80,
        /// <summary>
        /// Q key
        /// </summary>
		Q = 81,
        /// <summary>
        /// R key
        /// </summary>
		R = 82,
        /// <summary>
        /// S key
        /// </summary>
		S = 83,
        /// <summary>
        /// T key
        /// </summary>
		T = 84,
        /// <summary>
        /// U key
        /// </summary>
		U = 85,
        /// <summary>
        /// V key
        /// </summary>
		V = 86,
        /// <summary>
        /// W key
        /// </summary>
		W = 87,
        /// <summary>
        /// X key
        /// </summary>
		X = 88,
        /// <summary>
        /// Y key
        /// </summary>
		Y = 89,
        /// <summary>
        /// Z key
        /// </summary>
		Z = 90,
        /// <summary>
        /// Left Windows key
        /// </summary>
		LeftWindows = 91,
        /// <summary>
        /// Right Windows key
        /// </summary>
		RightWindows = 92,
        /// <summary>
        /// Applications key
        /// </summary>
		Apps = 93,
        /// <summary>
        /// Computer Sleep key
        /// </summary>
		Sleep = 95,
        /// <summary>
        /// Numeric keypad 0 key
        /// </summary>
		NumPad0 = 96,
        /// <summary>
        /// Numeric keypad 1 key
        /// </summary>
		NumPad1 = 97,
        /// <summary>
        /// Numeric keypad 2 key
        /// </summary>
		NumPad2 = 98,
        /// <summary>
        /// Numeric keypad 3 key
        /// </summary>
		NumPad3 = 99,
        /// <summary>
        /// Numeric keypad 4 key
        /// </summary>
		NumPad4 = 100,
        /// <summary>
        /// Numeric keypad 5 key
        /// </summary>
		NumPad5 = 101,
        /// <summary>
        /// Numeric keypad 6 key
        /// </summary>
		NumPad6 = 102,
        /// <summary>
        /// Numeric keypad 7 key
        /// </summary>
		NumPad7 = 103,
        /// <summary>
        /// Numeric keypad 8 key
        /// </summary>
		NumPad8 = 104,
        /// <summary>
        /// Numeric keypad 9 key
        /// </summary>
		NumPad9 = 105,
        /// <summary>
        /// Multiply key
        /// </summary>
		Multiply = 106,
        /// <summary>
        /// Add key
        /// </summary>
		Add = 107,
        /// <summary>
        /// Separator key
        /// </summary>
		Separator = 108,
        /// <summary>
        /// Subtract key
        /// </summary>
		Subtract = 109,
        /// <summary>
        /// Decimal key
        /// </summary>
		Decimal = 110,
        /// <summary>
        /// Divide key
        /// </summary>
		Divide = 111,
        /// <summary>
        /// F1 key
        /// </summary>
		F1 = 112,
        /// <summary>
        /// F2 key
        /// </summary>
		F2 = 113,
        /// <summary>
        /// F3 key
        /// </summary>
		F3 = 114,
        /// <summary>
        /// F4 key
        /// </summary>
		F4 = 115,
        /// <summary>
        /// F5 key
        /// </summary>
		F5 = 116,
        /// <summary>
        /// F6 key
        /// </summary>
		F6 = 117,
        /// <summary>
        /// F7 key
        /// </summary>
		F7 = 118,
        /// <summary>
        /// F8 key
        /// </summary>
		F8 = 119,
        /// <summary>
        /// F9 key
        /// </summary>
		F9 = 120,
        /// <summary>
        /// F10 key
        /// </summary>
		F10 = 121,
        /// <summary>
        /// F11 key
        /// </summary>
		F11 = 122,
        /// <summary>
        /// F12 key
        /// </summary>
		F12 = 123,
        /// <summary>
        /// F13 key
        /// </summary>
		F13 = 124,
        /// <summary>
        /// F14 key
        /// </summary>
		F14 = 125,
        /// <summary>
        /// F15 key
        /// </summary>
		F15 = 126,
        /// <summary>
        /// F16 key
        /// </summary>
		F16 = 127,
        /// <summary>
        /// F17 key
        /// </summary>
		F17 = 128,
        /// <summary>
        /// F18 key
        /// </summary>
		F18 = 129,
        /// <summary>
        /// F19 key
        /// </summary>
		F19 = 130,
        /// <summary>
        /// F20 key
        /// </summary>
		F20 = 131,
        /// <summary>
        /// F21 key
        /// </summary>
		F21 = 132,
        /// <summary>
        /// F22 key
        /// </summary>
		F22 = 133,
        /// <summary>
        /// F23 key
        /// </summary>
		F23 = 134,
        /// <summary>
        /// F24 key
        /// </summary>
		F24 = 135,
		NumLock = 144,        // 	NUM LOCK key
		Scroll = 145,        // 	SCROLL LOCK key
		LeftShift = 160,        // 	Left SHIFT key
		RightShift = 161,        // 	Right SHIFT key
		LeftControl = 162,        // 	Left CONTROL key
		RightControl = 163,        // 	Right CONTROL key
		LeftAlt = 164,        // 	Left ALT key
		RightAlt = 165,        // 	Right ALT key
		BrowserBack = 166,        // 	Windows 2000/XP: Browser Back key
		BrowserForward = 167,        // 	Windows 2000/XP: Browser Forward key
		BrowserRefresh = 168,        // 	Windows 2000/XP: Browser Refresh key
		BrowserStop = 169,        // 	Windows 2000/XP: Browser Stop key
		BrowserSearch = 170,        // 	Windows 2000/XP: Browser Search key
		BrowserFavorites = 171,        // 	Windows 2000/XP: Browser Favorites key
		BrowserHome = 172,        // 	Windows 2000/XP: Browser Start and Home key
		VolumeMute = 173,        // 	Windows 2000/XP: Volume Mute key
		VolumeDown = 174,        // 	Windows 2000/XP: Volume Down key
		VolumeUp = 175,        // 	Windows 2000/XP: Volume Up key
		MediaNextTrack = 176,        // 	Windows 2000/XP: Next Track key
		MediaPreviousTrack = 177,        // 	Windows 2000/XP: Previous Track key
		MediaStop = 178,        // 	Windows 2000/XP: Stop Media key
		MediaPlayPause = 179,        // 	Windows 2000/XP: Play/Pause Media key
		LaunchMail = 180,        // 	Windows 2000/XP: Start Mail key
		SelectMedia = 181,        // 	Windows 2000/XP: Select Media key
		LaunchApplication1 = 182,        // 	Windows 2000/XP: Start Application 1 key
		LaunchApplication2 = 183,        // 	Windows 2000/XP: Start Application 2 key
		OemSemicolon = 186,        // 	Windows 2000/XP: The OEM Semicolon key on a US standard keyboard
		OemPlus = 187,        // 	Windows 2000/XP: For any country/region, the '+' key
		OemComma = 188,        // 	Windows 2000/XP: For any country/region, the ',' key
		OemMinus = 189,        // 	Windows 2000/XP: For any country/region, the '-' key
		OemPeriod = 190,        // 	Windows 2000/XP: For any country/region, the '.' key
		OemQuestion = 191,        // 	Windows 2000/XP: The OEM question mark key on a US standard keyboard
		OemTilde = 192,        // 	Windows 2000/XP: The OEM tilde key on a US standard keyboard
		OemOpenBrackets = 219,        // 	Windows 2000/XP: The OEM open bracket key on a US standard keyboard
		OemPipe = 220,        // 	Windows 2000/XP: The OEM pipe key on a US standard keyboard
		OemCloseBrackets = 221,        // 	Windows 2000/XP: The OEM close bracket key on a US standard keyboard
		OemQuotes = 222,        // 	Windows 2000/XP: The OEM singled/double quote key on a US standard keyboard
		Oem8 = 223,        // 	Used for miscellaneous characters; it can vary by keyboard.
		OemBackslash = 226,        // 	Windows 2000/XP: The OEM angle bracket or backslash key on the RT 102 key keyboard
		ProcessKey = 229,        // 	Windows 95/98/Me, Windows NT 4.0, Windows 2000/XP: IME PROCESS key
		Attn = 246,        // 	Attn key
		Crsel = 247,        // 	CrSel key
		Exsel = 248,        // 	ExSel key
		EraseEof = 249,        // 	Erase EOF key
		Play = 250,        // 	Play key
		Zoom = 251,        // 	Zoom key
		Pa1 = 253,        // 	PA1 key
		OemClear = 254,        // 	CLEAR key
		ChatPadGreen = 0xCA,        // 	Green ChatPad key
		ChatPadOrange = 0xCB,        // 	Orange ChatPad key
		Pause = 0x13,        // 	PAUSE key
		ImeConvert = 0x1c,        // 	IME Convert key
		ImeNoConvert = 0x1d,        // 	IME NoConvert key
		Kana = 0x15,        // 	Kana key on Japanese keyboards
		Kanji = 0x19,        // 	Kanji key on Japanese keyboards
		OemAuto = 0xf3,        // 	OEM Auto key
		OemCopy = 0xf2,        // 	OEM Copy key
		OemEnlW = 0xf4        // 	OEM Enlarge Window key
	}
}
