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
	[Flags]
	public enum Keys
	{
		None = 0,        // 	Reserved
		Back = 8,        // 	BACKSPACE key
		Tab = 9,        // 	TAB key
		Enter = 13,        // 	ENTER key
		CapsLock = 20,        // 	CAPS LOCK key
		Escape = 27,        // 	ESC key
		Space = 32,        // 	SPACEBAR
		PageUp = 33,        // 	PAGE UP key
		PageDown = 34,        // 	DOWN ARROW key
		End = 35,        // 	END key
		Home = 36,        // 	HOME key
		Left = 37,        // 	LEFT ARROW key
		Up = 38,        // 	UP ARROW key
		Right = 39,        // 	RIGHT ARROW key
		Down = 40,
		Select = 41,        // 	SELECT key
		Print = 42,        // 	PRINT key
		Execute = 43,        // 	EXECUTE key
		PrintScreen = 44,        // 	PRINT SCREEN key
		Insert = 45,        // 	INS key
		Delete = 46,        // 	DEL key
		Help = 47,        // 	HELP key
		D0 = 48,        // 	Used for miscellaneous characters; it can vary by keyboard.
		D1 = 49,        // 	Used for miscellaneous characters; it can vary by keyboard.
		D2 = 50,        // 	Used for miscellaneous characters; it can vary by keyboard.
		D3 = 51,        // 	Used for miscellaneous characters; it can vary by keyboard.
		D4 = 52,        // 	Used for miscellaneous characters; it can vary by keyboard.
		D5 = 53,        // 	Used for miscellaneous characters; it can vary by keyboard.
		D6 = 54,        // 	Used for miscellaneous characters; it can vary by keyboard.
		D7 = 55,        // 	Used for miscellaneous characters; it can vary by keyboard.
		D8 = 56,        // 	Used for miscellaneous characters; it can vary by keyboard.
		D9 = 57,        // 	Used for miscellaneous characters; it can vary by keyboard.
		A = 65,        // 	A key
		B = 66,        // 	B key
		C = 67,        // 	C key
		D = 68,        // 	D key
		E = 69,        // 	E key
		F = 70,        // 	F key
		G = 71,        // 	G key
		H = 72,        // 	H key
		I = 73,        // 	I key
		J = 74,        // 	J key
		K = 75,        // 	K key
		L = 76,        // 	L key
		M = 77,        // 	M key
		N = 78,        // 	N key
		O = 79,        // 	O key
		P = 80,        // 	P key
		Q = 81,        // 	Q key
		R = 82,        // 	R key
		S = 83,        // 	S key
		T = 84,        // 	T key
		U = 85,        // 	U key
		V = 86,        // 	V key
		W = 87,        // 	W key
		X = 88,        // 	X key
		Y = 89,        // 	Y key
		Z = 90,        // 	Z key
		LeftWindows = 91,        // 	Left Windows key
		RightWindows = 92,        // 	Right Windows key
		Apps = 93,        // 	Applications key
		Sleep = 95,        // 	Computer Sleep key
		NumPad0 = 96,        // 	Numeric keypad 0 key
		NumPad1 = 97,        // 	Numeric keypad 1 key
		NumPad2 = 98,        // 	Numeric keypad 2 key
		NumPad3 = 99,        // 	Numeric keypad 3 key
		NumPad4 = 100,        // 	Numeric keypad 4 key
		NumPad5 = 101,        // 	Numeric keypad 5 key
		NumPad6 = 102,        // 	Numeric keypad 6 key
		NumPad7 = 103,        // 	Numeric keypad 7 key
		NumPad8 = 104,        // 	Numeric keypad 8 key
		NumPad9 = 105,        // 	Numeric keypad 9 key
		Multiply = 106,        // 	Multiply key
		Add = 107,        // 	Add key
		Separator = 108,        // 	Separator key
		Subtract = 109,        // 	Subtract key
		Decimal = 110,        // 	Decimal key
		Divide = 111,        // 	Divide key
		F1 = 112,        // 	F1 key
		F2 = 113,        // 	F2 key
		F3 = 114,        // 	F3 key
		F4 = 115,        // 	F4 key
		F5 = 116,        // 	F5 key
		F6 = 117,        // 	F6 key
		F7 = 118,        // 	F7 key
		F8 = 119,        // 	F8 key
		F9 = 120,        // 	F9 key
		F10 = 121,        // 	F10 key
		F11 = 122,        // 	F11 key
		F12 = 123,        // 	F12 key
		F13 = 124,        // 	F13 key
		F14 = 125,        // 	F14 key
		F15 = 126,        // 	F15 key
		F16 = 127,        // 	F16 key
		F17 = 128,        // 	F17 key
		F18 = 129,        // 	F18 key
		F19 = 130,        // 	F19 key
		F20 = 131,        // 	F20 key
		F21 = 132,        // 	F21 key
		F22 = 133,        // 	F22 key
		F23 = 134,        // 	F23 key
		F24 = 135,        // 	F24 key
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
