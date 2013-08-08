using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Drawing;

#if MACOS
using MonoMac.CoreGraphics;
using MonoMac.AppKit;
using MonoMac.Foundation;
using MonoMac.CoreText;
using MonoMac.ImageIO;
#endif

namespace Microsoft.Xna.Framework.Content.Pipeline.Graphics
{
 
    static class FontHelper
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct ABC
        {
            public int abcA;
            public uint abcB;
            public int abcC;
        }

#if WINDOWS
		[DllImport("gdi32.dll", ExactSpelling = true)]
		public static extern IntPtr SelectObject(IntPtr hDC, IntPtr hObj);

		[DllImport("gdi32.dll", ExactSpelling = true, SetLastError = true)]
		public static extern int DeleteObject(IntPtr hObj);

		[DllImport("gdi32.dll", ExactSpelling = true, CharSet = CharSet.Unicode, SetLastError = true)]
		public static extern bool GetCharABCWidthsW(IntPtr hdc, uint uFirstChar, uint uLastChar, [Out, MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPStruct, SizeConst = 1)] ABC[] lpabc);

		public static ABC GetCharWidthABC(char ch, Font font, System.Drawing.Graphics gr)
		{
			ABC[] _temp = new ABC[1];
			IntPtr hDC = gr.GetHdc();
			Font ft = (Font)font.Clone();
			IntPtr hFt = ft.ToHfont();
			SelectObject(hDC, hFt);
			GetCharABCWidthsW(hDC, ch, ch, _temp);
			DeleteObject(hFt);
			gr.ReleaseHdc();

			return _temp[0];
		}

#elif MACOS

		static CTFont nativeFont;

		public static ABC GetCharWidthABC(char ch, Font font, System.Drawing.Graphics gr)
		{
			ABC[] _temp = new ABC[1];
			var nativFont = CreateFont (font.Name, font.Size, font.Style, font.GdiCharSet, font.GdiVerticalFont);
			var atts = buildAttributedString(ch.ToString(), nativFont);
			// for now just a line not sure if this is going to work
			CTLine line = new CTLine(atts);

			float ascent;
			float descent;
			float leading;
			_temp[0].abcB = (uint)line.GetTypographicBounds(out ascent, out descent, out leading);


			return _temp[0];
		}

		const byte DefaultCharSet = 1;
		static bool underLine = false;
		static bool strikeThrough = false;

		static float dpiScale = 96f / 72f;


		static internal CTFont CreateFont (string familyName, float emSize)
		{
			return CreateFont (familyName, emSize, FontStyle.Regular, DefaultCharSet, false);
		}

		static internal CTFont CreateFont (string familyName, float emSize, FontStyle style)
		{
			return CreateFont (familyName, emSize, style, DefaultCharSet, false);
		}

		static internal CTFont CreateFont (string familyName, float emSize, FontStyle style, byte gdiCharSet)
		{
			return CreateFont (familyName, emSize, style, gdiCharSet, false);
		}

		static internal CTFont CreateFont (string familyName, float emSize, FontStyle style,
		                                   byte gdiCharSet, bool  gdiVerticalFont )
		{
			if (emSize <= 0)
				throw new ArgumentException("emSize is less than or equal to 0, evaluates to infinity, or is not a valid number.","emSize");

			CTFont nativeFont;

			// convert to 96 Dpi to be consistent with Windows
			var dpiSize = emSize * dpiScale;

			try {
				nativeFont = new CTFont(familyName,dpiSize);
			}
			catch
			{
				nativeFont = new CTFont("Helvetica",dpiSize);
			}

			CTFontSymbolicTraits tMask = CTFontSymbolicTraits.None;

			if ((style & FontStyle.Bold) == FontStyle.Bold)
				tMask |= CTFontSymbolicTraits.Bold;
			if ((style & FontStyle.Italic) == FontStyle.Italic)
				tMask |= CTFontSymbolicTraits.Italic;
			strikeThrough = (style & FontStyle.Strikeout) == FontStyle.Strikeout;
			underLine = (style & FontStyle.Underline) == FontStyle.Underline;

			var nativeFont2 = nativeFont.WithSymbolicTraits(dpiSize,tMask,tMask);

			if (nativeFont2 != null)
				nativeFont = nativeFont2;

			return nativeFont;
		}

		static NSString FontAttributedName = (NSString)"NSFont";
		static NSString ForegroundColorAttributedName = (NSString)"NSColor";
		static NSString UnderlineStyleAttributeName = (NSString)"NSUnderline";
		static NSString ParagraphStyleAttributeName = (NSString)"NSParagraphStyle";
		//NSAttributedString.ParagraphStyleAttributeName
		static NSString StrikethroughStyleAttributeName = (NSString)"NSStrikethrough";

		private static NSMutableAttributedString buildAttributedString(string text, CTFont font, 
		                                                               Color? fontColor=null) 
		{


			// Create a new attributed string from text
			NSMutableAttributedString atts = 
				new NSMutableAttributedString(text);

			var attRange = new NSRange(0, atts.Length);
			var attsDic = new NSMutableDictionary();

			// Font attribute
			NSObject fontObject = new NSObject(font.Handle);
			attsDic.Add(FontAttributedName, fontObject);
			// -- end font 

			if (fontColor.HasValue) {

				// Font color
				var fc = fontColor.Value;
				NSColor color = NSColor.FromDeviceRgba(fc.R / 255f, 
				                                       fc.G / 255f,
				                                       fc.B / 255f,
				                                       fc.A / 255f);
				NSObject colorObject = new NSObject(color.Handle);
				attsDic.Add(ForegroundColorAttributedName, colorObject);
				// -- end font Color
			}

			if (underLine) {
				// Underline
				int single = (int)MonoMac.AppKit.NSUnderlineStyle.Single;
				int solid = (int)MonoMac.AppKit.NSUnderlinePattern.Solid;
				var attss = single | solid;
				var underlineObject = NSNumber.FromInt32(attss);
				//var under = NSAttributedString.UnderlineStyleAttributeName.ToString();
				attsDic.Add(UnderlineStyleAttributeName, underlineObject);
				// --- end underline
			}


			if (strikeThrough) {
				// StrikeThrough
				//				NSColor bcolor = NSColor.Blue;
				//				NSObject bcolorObject = new NSObject(bcolor.Handle);
				//				attsDic.Add(NSAttributedString.StrikethroughColorAttributeName, bcolorObject);
				int stsingle = (int)MonoMac.AppKit.NSUnderlineStyle.Single;
				int stsolid = (int)MonoMac.AppKit.NSUnderlinePattern.Solid;
				var stattss = stsingle | stsolid;
				var stunderlineObject = NSNumber.FromInt32(stattss);

				attsDic.Add(StrikethroughStyleAttributeName, stunderlineObject);
				// --- end underline
			}


			// Text alignment
			var alignment = CTTextAlignment.Left;
			var alignmentSettings = new CTParagraphStyleSettings();
			alignmentSettings.Alignment = alignment;
			var paragraphStyle = new CTParagraphStyle(alignmentSettings);
			NSObject psObject = new NSObject(paragraphStyle.Handle);

			// end text alignment

			attsDic.Add(ParagraphStyleAttributeName, psObject);

			atts.SetAttributes(attsDic, attRange);

			return atts;

		}

#elif LINUX
		public static ABC GetCharWidthABC(char ch, Font font, System.Drawing.Graphics gr)
        {
            var sf = StringFormat.GenericTypographic;
            sf.Trimming = StringTrimming.None;
            sf.FormatFlags = StringFormatFlags.MeasureTrailingSpaces;
            return new ABC
            {
                abcA = 0,
                abcB = (uint)gr.MeasureString(ch.ToString(), font, new PointF(0, 0), sf).Width,
                abcC = 0
            };
        }
#endif
	}
}
