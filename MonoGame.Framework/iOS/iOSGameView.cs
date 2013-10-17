#region License
/*
Microsoft Public License (Ms-PL)
MonoGame - Copyright © 2009-2012 The MonoGame Team

All rights reserved.

This license governs use of the accompanying software. If you use the software,
you accept this license. If you do not accept the license, do not use the
software.

1. Definitions

The terms "reproduce," "reproduction," "derivative works," and "distribution"
have the same meaning here as under U.S. copyright law.

A "contribution" is the original software, or any additions or changes to the
software.

A "contributor" is any person that distributes its contribution under this
license.

"Licensed patents" are a contributor's patent claims that read directly on its
contribution.

2. Grant of Rights

(A) Copyright Grant- Subject to the terms of this license, including the
license conditions and limitations in section 3, each contributor grants you a
non-exclusive, worldwide, royalty-free copyright license to reproduce its
contribution, prepare derivative works of its contribution, and distribute its
contribution or any derivative works that you create.

(B) Patent Grant- Subject to the terms of this license, including the license
conditions and limitations in section 3, each contributor grants you a
non-exclusive, worldwide, royalty-free license under its licensed patents to
make, have made, use, sell, offer for sale, import, and/or otherwise dispose of
its contribution in the software or derivative works of the contribution in the
software.

3. Conditions and Limitations

(A) No Trademark License- This license does not grant you rights to use any
contributors' name, logo, or trademarks.

(B) If you bring a patent claim against any contributor over patents that you
claim are infringed by the software, your patent license from such contributor
to the software ends automatically.

(C) If you distribute any portion of the software, you must retain all
copyright, patent, trademark, and attribution notices that are present in the
software.

(D) If you distribute any portion of the software in source code form, you may
do so only under this license by including a complete copy of this license with
your distribution. If you distribute any portion of the software in compiled or
object code form, you may only do so under a license that complies with this
license.

(E) The software is licensed "as-is." You bear the risk of using it. The
contributors give no express warranties, guarantees or conditions. You may have
additional consumer rights under your local laws which this license cannot
change. To the extent permitted under your local laws, the contributors exclude
the implied warranties of merchantability, fitness for a particular purpose and
non-infringement.
*/
#endregion License

using System;
using System.Drawing;
using System.Text;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

using MonoTouch.CoreGraphics;
using MonoTouch.CoreText;
using MonoTouch.CoreAnimation;
using MonoTouch.Foundation;
using MonoTouch.ObjCRuntime;
using MonoTouch.OpenGLES;
using MonoTouch.UIKit;

using OpenTK.Graphics;
using OpenTK.Graphics.ES20;
using OpenTK.Platform.iPhoneOS;

using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;

using All = OpenTK.Graphics.ES20.All;

namespace Microsoft.Xna.Framework {

    public class KeyboardEntryEventArgs : EventArgs 
    {
        public object TextEtc;
        public KeyboardEntryEventArgs (string text)
        {
            TextEtc = text;
        }
        public KeyboardEntryEventArgs (object text)
        {
            TextEtc = text;
        }
    }
    public enum Keypresses
    {
        backspaceDeleteKey,
        whatelse
    }

    [Adopts ("UIKeyInput")]
    [Adopts ("UITextInput")]  
    [Register("iOSGameView")]
	public partial class iOSGameView : UIView {
		private readonly iOSGamePlatform _platform;
		private int _colorbuffer;
		private int _depthbuffer;
		private int _framebuffer;

	#region Construction/Destruction
		public iOSGameView (iOSGamePlatform platform, RectangleF frame)
			: base(frame)
		{
			if (platform == null)
				throw new ArgumentNullException ("platform");
			_platform = platform;
			Initialize ();

            //**************
            //Most of the UITextInput is probably irrelevant, but leaving it in for now
            //We HAVE to atleast have stubs for these methods because
            //if you Adopt UITextInput, most of these methods are "Required"
            //HOWEVER the text input from dictation still turns up in "void InsertText(string text)"
            //which comes from a different class ie. Adopts("UIKeyInput") instead of Adopts ("UITextInput")
            //**************

            // Create our tokenizer and text storage
            tokenizer = new UITextInputStringTokenizer (this);

            // Create and set up our SimpleCoreTextView that will do the drawing
            textView = new SimpleCoreTextView (new RectangleF ()); //AB: "using UIKit;" is not working!!! should be: Bounds.Inset (5, 5));
            textView.AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleHeight;

            textView.UserInteractionEnabled = true;
            //UserInteractionEnabled = true;
            //AutosizesSubviews = true;
            //AddSubview (textView);
            //textView.Text = string.Empty;

            //**************
		}

		private void Initialize ()
		{
			MultipleTouchEnabled = true;
			Opaque = true;
		}

		protected override void Dispose (bool disposing)
		{
			if (disposing) {
				if (__renderbuffergraphicsContext != null)
					DestroyContext();
			}

			base.Dispose (disposing);
			_isDisposed = true;
		}

#endregion Construction/Destruction

		#region Properties

		private bool _isDisposed;

		public bool IsDisposed {
			get { return _isDisposed; }
		}

		#endregion Properties

		[Export ("layerClass")]
		public static Class GetLayerClass ()
		{
			return new Class (typeof (CAEAGLLayer));
		}

        public bool EnableKeyboard = false;
		public override bool CanBecomeFirstResponder {
            get { return EnableKeyboard; }
		}

		private new CAEAGLLayer Layer {
			get { return base.Layer as CAEAGLLayer; }
		}

		// FIXME: Someday, hopefully it will be possible to move
		//        GraphicsContext into an iOS-specific GraphicsDevice.
		//        Some level of cooperation with the UIView/Layer will
		//        probably always be necessary, unfortunately.
		private GraphicsContext __renderbuffergraphicsContext;
		private IOpenGLApi _glapi;
		private void CreateContext ()
		{
			AssertNotDisposed ();

            // RetainedBacking controls if the content of the colorbuffer should be preserved after being displayed
            // This is the XNA equivalent to set PreserveContent when initializing the GraphicsDevice
            // (should be false by default for better performance)
			Layer.DrawableProperties = NSDictionary.FromObjectsAndKeys (
				new NSObject [] {
					NSNumber.FromBoolean (false), 
					EAGLColorFormat.RGBA8
				},
				new NSObject [] {
					EAGLDrawableProperty.RetainedBacking,
					EAGLDrawableProperty.ColorFormat
				});

			Layer.ContentsScale = Window.Screen.Scale;

			//var strVersion = OpenTK.Graphics.ES11.GL.GetString (OpenTK.Graphics.ES11.All.Version);
			//strVersion = OpenTK.Graphics.ES20.GL.GetString (OpenTK.Graphics.ES20.All.Version);
			//var version = Version.Parse (strVersion);

			try {
				__renderbuffergraphicsContext = new GraphicsContext (null, null, 2, 0, GraphicsContextFlags.Embedded);
				_glapi = new Gles20Api ();
			} catch {
				__renderbuffergraphicsContext = new GraphicsContext (null, null, 1, 1, GraphicsContextFlags.Embedded);
				_glapi = new Gles11Api ();
			}

			__renderbuffergraphicsContext.MakeCurrent (null);
		}

		private void DestroyContext ()
		{
			AssertNotDisposed ();
			AssertValidContext ();

			__renderbuffergraphicsContext.Dispose ();
			__renderbuffergraphicsContext = null;
			_glapi = null;
		}

        [Export("doTick")]
        void DoTick()
        {
            _platform.Tick();
        }

		private void CreateFramebuffer ()
		{
			AssertNotDisposed ();
			AssertValidContext ();

			__renderbuffergraphicsContext.MakeCurrent (null);
			
			// HACK:  GraphicsDevice itself should be calling
			//        glViewport, so we shouldn't need to do it
			//        here and then force the state into
			//        GraphicsDevice.  However, that change is a
			//        ways off, yet.
            int viewportHeight = (int)Math.Round(Layer.Bounds.Size.Height * Layer.ContentsScale);
            int viewportWidth = (int)Math.Round(Layer.Bounds.Size.Width * Layer.ContentsScale);

			_glapi.GenFramebuffers (1, ref _framebuffer);
			_glapi.BindFramebuffer (All.Framebuffer, _framebuffer);
			
			// Create our Depth buffer. Color buffer must be the last one bound
			GL.GenRenderbuffers(1, ref _depthbuffer);
			GL.BindRenderbuffer(All.Renderbuffer, _depthbuffer);
            GL.RenderbufferStorage (All.Renderbuffer, All.DepthComponent16, viewportWidth, viewportHeight);
			GL.FramebufferRenderbuffer(All.Framebuffer, All.DepthAttachment, All.Renderbuffer, _depthbuffer);

			_glapi.GenRenderbuffers(1, ref _colorbuffer);
			_glapi.BindRenderbuffer(All.Renderbuffer, _colorbuffer);

			var ctx = ((IGraphicsContextInternal) __renderbuffergraphicsContext).Implementation as iPhoneOSGraphicsContext;

			// TODO: EAGLContext.RenderBufferStorage returns false
			//       on all but the first call.  Nevertheless, it
			//       works.  Still, it would be nice to know why it
			//       claims to have failed.
			ctx.EAGLContext.RenderBufferStorage ((uint) All.Renderbuffer, Layer);
			
			_glapi.FramebufferRenderbuffer (All.Framebuffer, All.ColorAttachment0, All.Renderbuffer, _colorbuffer);
			
			var status = GL.CheckFramebufferStatus (All.Framebuffer);
			if (status != All.FramebufferComplete)
				throw new InvalidOperationException (
					"Framebuffer was not created correctly: " + status);

			_glapi.Viewport(0, 0, viewportWidth, viewportHeight);
            _glapi.Scissor(0, 0, viewportWidth, viewportHeight);

			var gds = _platform.Game.Services.GetService(
                typeof (IGraphicsDeviceService)) as IGraphicsDeviceService;

			if (gds != null && gds.GraphicsDevice != null)
			{
                var pp = gds.GraphicsDevice.PresentationParameters;
                int height = viewportHeight;
                int width = viewportWidth;

                if (this.NextResponder is iOSGameViewController)
                {
                    var displayOrientation = _platform.Game.Window.CurrentOrientation;
                    if (displayOrientation == DisplayOrientation.LandscapeLeft || displayOrientation == DisplayOrientation.LandscapeRight)
                    {
                        height = Math.Min(viewportHeight, viewportWidth);
                        width = Math.Max(viewportHeight, viewportWidth);
                    }
                    else
                    {
                        height = Math.Max(viewportHeight, viewportWidth);
                        width = Math.Min(viewportHeight, viewportWidth);
                    }
                }

                pp.BackBufferHeight = height;
                pp.BackBufferWidth = width;

				gds.GraphicsDevice.Viewport = new Viewport(
					0, 0,
					pp.BackBufferWidth,
					pp.BackBufferHeight);
				
				// FIXME: These static methods on GraphicsDevice need
				//        to go away someday.
				gds.GraphicsDevice.glFramebuffer = _framebuffer;
			}

            if (Threading.BackgroundContext == null)
                Threading.BackgroundContext = new MonoTouch.OpenGLES.EAGLContext(ctx.EAGLContext.API, ctx.EAGLContext.ShareGroup);
		}

		private void DestroyFramebuffer ()
		{
			AssertNotDisposed ();
			AssertValidContext ();

			__renderbuffergraphicsContext.MakeCurrent (null);

			_glapi.DeleteFramebuffers (1, ref _framebuffer);
			_framebuffer = 0;

			_glapi.DeleteRenderbuffers (1, ref _colorbuffer);
			_colorbuffer = 0;
			
			_glapi.DeleteRenderbuffers (1, ref _depthbuffer);
			_depthbuffer = 0;
		}

		// FIXME: This logic belongs in GraphicsDevice.Present, not
		//        here.  If it can someday be moved there, then the
		//        normal call to Present in Game.Tick should cover
		//        this.  For now, iOSGamePlatform will call Present
		//        in the Draw/Update loop handler.
		[Obsolete("Remove iOSGameView.Present once GraphicsDevice.Present fully expresses this")]
		public void Present ()
		{
			AssertNotDisposed ();
			AssertValidContext ();

			__renderbuffergraphicsContext.MakeCurrent (null);
            GL.BindRenderbuffer (All.Renderbuffer, this._colorbuffer);
            __renderbuffergraphicsContext.SwapBuffers();
		}

		// FIXME: This functionality belongs iMakeCurrentn GraphicsDevice.
		[Obsolete("Move the functionality of iOSGameView.MakeCurrent into GraphicsDevice")]
		public void MakeCurrent ()
		{
			AssertNotDisposed ();
			AssertValidContext ();

			__renderbuffergraphicsContext.MakeCurrent (null);
		}

		public override void LayoutSubviews ()
		{
			base.LayoutSubviews ();

            var gds = _platform.Game.Services.GetService (
                typeof (IGraphicsDeviceService)) as IGraphicsDeviceService;

            if (gds == null || gds.GraphicsDevice == null)
                return;

			if (_framebuffer + _colorbuffer + _depthbuffer != 0)
				DestroyFramebuffer ();
			if (__renderbuffergraphicsContext == null)
				CreateContext();
			CreateFramebuffer ();
		}

		#region UIWindow Notifications

		[Export ("didMoveToWindow")]
		public virtual void DidMoveToWindow ()
		{

            if (Window != null) {
                
                if (__renderbuffergraphicsContext == null)
                    CreateContext ();
                if (_framebuffer * _colorbuffer * _depthbuffer == 0)
                    CreateFramebuffer ();
            }
		}

		#endregion UIWindow Notifications

		private void AssertNotDisposed ()
		{
			if (_isDisposed)
				throw new ObjectDisposedException (GetType ().Name);
		}

		private void AssertValidContext ()
		{
			if (__renderbuffergraphicsContext == null)
				throw new InvalidOperationException (
					"GraphicsContext must be created for this operation to succeed.");
		}





        //**********************************************
        //**********************************************


        public delegate void KeyboardTextEntryEventHandler(object sender, KeyboardEntryEventArgs e);
        public event KeyboardTextEntryEventHandler KeyboardTextEntry;
        // Invoke the KeyboardTextEntry event
        public virtual void OnKeyboardTextEntry(KeyboardEntryEventArgs e) 
        {
            if (KeyboardTextEntry != null) 
                KeyboardTextEntry (this, e);
        }



        [Export ("hasText")]
        bool HasText {
            get { return true; }
        }


        public string outputText;
        // UIKeyInput required method - Insert a character into the displayed text.
        // Called by the text system when the user has entered simple text
        [Export ("insertText:")]
        void InsertText (string text)
        {
            if (text == " ") {
                OnKeyboardTextEntry (new KeyboardEntryEventArgs (text));
            } else if (text.Trim () != "") {
                OnKeyboardTextEntry (new KeyboardEntryEventArgs (text));

//                // Create our tokenizer and text storage
//                tokenizer = new UITextInputStringTokenizer (this);
//
//                // Create and set up our SimpleCoreTextView that will do the drawing
//                textView = new SimpleCoreTextView (new RectangleF ()); //AB: "using UIKit;" is not working!!! should be: Bounds.Inset (5, 5));
//                textView.AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleHeight;
//
//                textView.UserInteractionEnabled = true;
//                //UserInteractionEnabled = true;
//                //AutosizesSubviews = true;
//                //AddSubview (textView);
//                  textView.Text = string.Empty;
            }
        }

        // UIKeyInput required method - Delete a character from the displayed text.
        // Called by the text system when the user is invoking a delete (e.g. pressing
        // the delete software keyboard key)
        [Export ("deleteBackward")]
        void DeleteBackward ()
        {
            OnKeyboardTextEntry(new KeyboardEntryEventArgs (Keypresses.backspaceDeleteKey));
        }


        [Export ("insertDictationResult")]
        void insertDictationResult(NSArray a)
        {
            NSNotificationCenter.DefaultCenter.PostNotificationName ("DictationRecognitionSucceededNotification", this);
        }


        [Export ("dictationRecordingDidEnd")]
        void dictationRecordingDidEnd()
        {
            System.Diagnostics.Trace.Assert (true);
        }

        [Export ("dictationRecognitionFailed")]
        void dictationRecognitionFailed()
        {
            System.Diagnostics.Trace.Assert (true);
        }


        StringBuilder text = new StringBuilder ();
        UITextInputStringTokenizer tokenizer;
        public SimpleCoreTextView textView;
        NSDictionary markedTextStyle;
        UITextInputDelegate inputDelegate;

        public delegate void ViewWillEditDelegate (iOSGameViewController editableCoreTextView);
        public event ViewWillEditDelegate ViewWillEdit;

        #region UITextInput methods
        [Export ("inputDelegate")]
        public UITextInputDelegate InputDelegate {
            get {
                return inputDelegate;
            }
            set {
                inputDelegate = value;
            }
        }

        [Export ("markedTextStyle")]
        public NSDictionary MarkedTextStyle {
            get {
                return markedTextStyle;
            }
            set {
                markedTextStyle = value;
            }
        }

        #region UITextInput - Replacing and Returning Text
        // UITextInput required method - called by text system to get the string for
        // a given range in the text storage
        [Export ("textInRange:")]
        string TextInRange (UITextRange range)
        {
            IndexedRange r = (IndexedRange) range;
            return text.ToString ().Substring (r.Range.Location, r.Range.Length);
        }

        // UITextInput required method - called by text system to replace the given
        // text storage range with new text
        [Export ("replaceRange:withText:")]
        void ReplaceRange (UITextRange range, string text)
        {
            IndexedRange r = (IndexedRange) range;
            NSRange selectedNSRange = textView.SelectedTextRange;
            // Determine if replaced range intersects current selection range
            // and update selection range if so.
            if (r.Range.Location + r.Range.Length <= selectedNSRange.Location) {
                // This is the easy case. 
                selectedNSRange.Location -= (r.Range.Length - text.Length);
            } else {
                // Need to also deal with overlapping ranges.  Not addressed
                // in this simplified sample.
            }

            // Now replace characters in text storage
            this.text.Remove (r.Range.Location, r.Range.Length);
            this.text.Insert (r.Range.Location, text);

            // Update underlying SimpleCoreTextView
            textView.Text = this.text.ToString ();
            textView.SelectedTextRange = selectedNSRange;
        }
        #endregion

        #region UITextInput - Working with Marked and Selected Text

        // UITextInput selectedTextRange property accessor overrides
        // (access/update underlaying SimpleCoreTextView)
        [Export ("selectedTextRange")]
        IndexedRange SelectedTextRange {
            get {
                return IndexedRange.GetRange (textView.SelectedTextRange);
            }
            set {
                textView.SelectedTextRange = value.Range;
            }
        }

        // UITextInput markedTextRange property accessor overrides
        // (access/update underlaying SimpleCoreTextView)
        [Export ("markedTextRange")]
        IndexedRange MarkedTextRange {
            get {
                return IndexedRange.GetRange (textView.MarkedTextRange);
            }
        }

        // UITextInput required method - Insert the provided text and marks it to indicate 
        // that it is part of an active input session. 
        [Export ("setMarkedText:selectedRange:")]
        void SetMarkedText (string markedText, NSRange selectedRange)
        {
            NSRange selectedNSRange = textView.SelectedTextRange;
            NSRange markedTextRange = textView.MarkedTextRange;

            if (markedText == null)
                markedText = string.Empty;

            if (markedTextRange.Location != NSRange.NotFound) {
                // Replace characters in text storage and update markedText range length
                text.Remove (markedTextRange.Location, markedTextRange.Length);
                text.Insert (markedTextRange.Location, markedText);
                markedTextRange.Length = markedText.Length;
            } else if (selectedNSRange.Length > 0) {
                // There currently isn't a marked text range, but there is a selected range,
                // so replace text storage at selected range and update markedTextRange.
                text.Remove (selectedNSRange.Location, selectedNSRange.Length);
                text.Insert (selectedNSRange.Location, markedText);
                markedTextRange.Location = selectedNSRange.Location;
                markedTextRange.Length = markedText.Length;
            } else {
                // There currently isn't marked or selected text ranges, so just insert
                // given text into storage and update markedTextRange.
                text.Insert (selectedNSRange.Location, markedText);
                markedTextRange.Location = selectedNSRange.Location;
                markedTextRange.Length = markedText.Length;
            }


            // Updated selected text range and underlying SimpleCoreTextView
            selectedNSRange = new NSRange (selectedRange.Location + markedTextRange.Location, selectedRange.Length);
            textView.Text = text.ToString ();
            textView.MarkedTextRange = markedTextRange;
            textView.SelectedTextRange = selectedNSRange;
        }

        // UITextInput required method - Unmark the currently marked text.
        [Export ("unmarkText")]
        void UnmarkText () 
        {
            NSRange markedTextRange = textView.MarkedTextRange;

            if (markedTextRange.Location == NSRange.NotFound)
                return;

            // unmark the underlying SimpleCoreTextView.markedTextRange
            markedTextRange.Location = NSRange.NotFound;
            textView.MarkedTextRange = markedTextRange;    

        }
        #endregion
        #region UITextInput - Computing Text Ranges and Text Positions

        // UITextInput beginningOfDocument property accessor override
        [Export ("beginningOfDocument")]
        IndexedPosition BeginningOfDocument {
            get { 
                // For this sample, the document always starts at index 0 and is the full
                // length of the text storage
                return IndexedPosition.GetPosition (0);
            }
        }

        // UITextInput endOfDocument property accessor override
        [Export ("endOfDocument")]
        IndexedPosition EndOfDocument {
            get { 
                // For this sample, the document always starts at index 0 and is the full
                // length of the text storage
                return IndexedPosition.GetPosition (text.Length); 
            }
        }

        // UITextInput protocol required method - Return the range between two text positions
        // using our implementation of UITextRange
        [Export ("textRangeFromPosition:toPosition:")]
        IndexedRange GetTextRange (UITextPosition fromPosition, UITextPosition toPosition)
        {
            try
            {
            // Generate IndexedPosition instances that wrap the to and from ranges
            IndexedPosition @from = (IndexedPosition) fromPosition;
            IndexedPosition @to = (IndexedPosition) toPosition;
            NSRange range = new NSRange (Math.Min (@from.Index, @to.Index), Math.Abs (to.Index - @from.Index));
            return IndexedRange.GetRange (range);
            }
            catch {
                return null;
            }
        }

        // UITextInput protocol required method - Returns the text position at a given offset 
        // from another text position using our implementation of UITextPosition
        [Export ("positionFromPosition:offset:")]
        IndexedPosition GetPosition (UITextPosition position, int offset)
        {
            // Generate IndexedPosition instance, and increment index by offset
            IndexedPosition pos = (IndexedPosition) position;
            int end = pos.Index + offset;
            // Verify position is valid in document
            if (end > text.Length || end < 0)
                return null;

            return IndexedPosition.GetPosition (end);
        }

        // UITextInput protocol required method - Returns the text position at a given offset 
        // in a specified direction from another text position using our implementation of
        // UITextPosition.
        [Export ("positionFromPosition:inDirection:offset:")]
        IndexedPosition GetPosition (UITextPosition position, UITextLayoutDirection direction, int offset)
        {
            // Note that this sample assumes LTR text direction
            IndexedPosition pos = (IndexedPosition) position;
            int newPos = pos.Index;

            switch (direction) {
                case UITextLayoutDirection.Right:
                newPos += offset;
                break;
                case UITextLayoutDirection.Left:
                newPos -= offset;
                break;
                case UITextLayoutDirection.Up:
                case UITextLayoutDirection.Down:
                // This sample does not support vertical text directions
                break;
            }

            // Verify new position valid in document

            if (newPos < 0)
                newPos = 0;

            if (newPos > text.Length)
                newPos = text.Length;

            return IndexedPosition.GetPosition (newPos);
        }
        #endregion
        #region UITextInput - Evaluating Text Positions

        // UITextInput protocol required method - Return how one text position compares to another 
        // text position.  
        [Export ("comparePosition:toPosition:")]
        NSComparisonResult ComparePosition (UITextPosition position, UITextPosition other)
        {
            IndexedPosition pos = (IndexedPosition) position;
            IndexedPosition o = (IndexedPosition) other;

            // For this sample, we simply compare position index values
            if (pos.Index == o.Index) {
                return NSComparisonResult.Same;
            } else if (pos.Index < o.Index) {
                return NSComparisonResult.Ascending;
            } else {
                return NSComparisonResult.Descending;
            }
        }

        // UITextInput protocol required method - Return the number of visible characters 
        // between one text position and another text position.
        [Export ("offsetFromPosition:toPosition:")]
        int GetOffset (IndexedPosition @from, IndexedPosition toPosition)
        {
            return @from.Index - toPosition.Index;
        }
        #endregion
        #region UITextInput - Text Input Delegate and Text Input Tokenizer

        // UITextInput tokenizer property accessor override
        //
        // An input tokenizer is an object that provides information about the granularity 
        // of text units by implementing the UITextInputTokenizer protocol.  Standard units 
        // of granularity include characters, words, lines, and paragraphs. In most cases, 
        // you may lazily create and assign an instance of a subclass of 
        // UITextInputStringTokenizer for this purpose, as this sample does. If you require 
        // different behavior than this system-provided tokenizer, you can create a custom 
        // tokenizer that adopts the UITextInputTokenizer protocol.
        [Export ("tokenizer")]
        UITextInputTokenizer Tokenizer {
            get { 
                return tokenizer; 
            }
        }
        #endregion
        #region UITextInput - Text Layout, writing direction and position related methods
        // UITextInput protocol method - Return the text position that is at the farthest 
        // extent in a given layout direction within a range of text.
        [Export ("positionWithinRange:farthestInDirection:")]
        IndexedPosition GetPosition (UITextRange range, UITextLayoutDirection direction)
        {
            // Note that this sample assumes LTR text direction
            IndexedRange r = (IndexedRange) range;
            int pos = r.Range.Location;

            // For this sample, we just return the extent of the given range if the
            // given direction is "forward" in a LTR context (UITextLayoutDirectionRight
            // or UITextLayoutDirectionDown), otherwise we return just the range position
            switch (direction) {
                case UITextLayoutDirection.Up:
                case UITextLayoutDirection.Left:
                pos = r.Range.Location;
                break;
                case UITextLayoutDirection.Right:
                case UITextLayoutDirection.Down:
                pos = r.Range.Location + r.Range.Length;
                break;
            }

            // Return text position using our UITextPosition implementation.
            // Note that position is not currently checked against document range.
            return IndexedPosition.GetPosition (pos);
        }

        // UITextInput protocol required method - Return a text range from a given text position 
        // to its farthest extent in a certain direction of layout.
        [Export ("characterRangeByExtendingPosition:inDirection:")]
        IndexedRange GetCharacterRange (UITextPosition position, UITextLayoutDirection direction)
        {
            // Note that this sample assumes LTR text direction
            IndexedPosition pos = (IndexedPosition) position;
            NSRange result = new NSRange (pos.Index, 1);

            switch (direction) {
                case UITextLayoutDirection.Up:
                case UITextLayoutDirection.Left:
                result = new NSRange (pos.Index - 1, 1);
                break;
                case UITextLayoutDirection.Right:
                case UITextLayoutDirection.Down:
                result = new NSRange (pos.Index, 1);
                break;
            }

            // Return range using our UITextRange implementation
            // Note that range is not currently checked against document range.
            return IndexedRange.GetRange (result);
        }

        // UITextInput protocol required method - Return the base writing direction for 
        // a position in the text going in a specified text direction.
        [Export ("baseWritingDirectionForPosition:inDirection:")]
        UITextWritingDirection GetBaseWritingDirection (UITextPosition position, UITextStorageDirection direction)
        {
            return UITextWritingDirection.LeftToRight;
        }

        // UITextInput protocol required method - Set the base writing direction for a 
        // given range of text in a document.
        [Export ("setBaseWritingDirection:forRange:")]
        void SetBaseWritingDirection (UITextWritingDirection writingDirection, UITextRange range)
        {
            // This sample assumes LTR text direction and does not currently support BiDi or RTL.
        }
        #endregion
        #region UITextInput - Geometry methods
        // UITextInput protocol required method - Return the first rectangle that encloses 
        // a range of text in a document.
        [Export ("firstRectForRange:")]
        RectangleF FirstRect (UITextRange range)
        {
            // FIXME: the Objective-C code doesn't get a null range
            // This is the reason why we don't get the autocorrection suggestions
            // (it'll just autocorrect without showing any suggestions).
            // Possibly due to http://bugzilla.xamarin.com/show_bug.cgi?id=265
            IndexedRange r = (IndexedRange) (range ?? IndexedRange.GetRange (new NSRange (0, 1)));
            // Use underlying SimpleCoreTextView to get rect for range
            RectangleF rect = textView.FirstRect (r.Range);
            // Convert rect to our view coordinates
            return rect;   //AB: Don't know why this is not working: ConvertRectFromView (rect, textView);
        }

        // UITextInput protocol required method - Return a rectangle used to draw the caret
        // at a given insertion point.
        [Export ("caretRectForPosition:")]
        RectangleF CaretRect (UITextPosition position)
        {
            // FIXME: the Objective-C code doesn't get a null position
            // This is the reason why we don't get the autocorrection suggestions
            // (it'll just autocorrect without showing any suggestions).
            // Possibly due to http://bugzilla.xamarin.com/show_bug.cgi?id=265
            IndexedPosition pos = (IndexedPosition) (position ?? IndexedPosition.GetPosition (0));
            // Get caret rect from underlying SimpleCoreTextView
            RectangleF rect = textView.CaretRect (pos.Index);
            // Convert rect to our view coordinates
            return rect; //AB: Don't know why this is not working: ConvertRectFromView (rect, textView);
        }
        #endregion
        #region UITextInput - Hit testing
        // Note that for this sample hit testing methods are not implemented, as there
        // is no implemented mechanic for letting user select text via touches.  There
        // is a wide variety of approaches for this (gestures, drag rects, etc) and
        // any approach chosen will depend greatly on the design of the application.

        // UITextInput protocol required method - Return the position in a document that 
        // is closest to a specified point. 
        [Export ("closestPositionToPoint:")]
        UITextPosition ClosestPosition (PointF point)
        {
            // Not implemented in this sample.  Could utilize underlying 
            // SimpleCoreTextView:closestIndexToPoint:point
            return null;
        }

        // UITextInput protocol required method - Return the position in a document that 
        // is closest to a specified point in a given range.
        [Export ("closestPositionToPoint:withinRange:")]
        UITextPosition ClosestPosition (PointF point, UITextRange range)
        {
            // Not implemented in this sample.  Could utilize underlying 
            // SimpleCoreTextView:closestIndexToPoint:point
            return null;
        }

        // UITextInput protocol required method - Return the character or range of 
        // characters that is at a given point in a document.
        [Export ("characterRangeAtPoint:")]
        UITextRange CharacterRange (PointF point)
        {
            // Not implemented in this sample.  Could utilize underlying 
            // SimpleCoreTextView:closestIndexToPoint:point
            return null;
        }
        #endregion
        #region UITextInput - Returning Text Styling Information
        // UITextInput protocol method - Return a dictionary with properties that specify 
        // how text is to be style at a certain location in a document.
        [Export ("textStylingAtPosition:inDirection:")]
        NSDictionary TextStyling (UITextPosition position, UITextStorageDirection direction)
        {
            // This sample assumes all text is single-styled, so this is easy.
            return  NSDictionary.FromObjectAndKey (textView.Font, CTStringAttributeKey.Font);
        }
        #endregion
        #endregion

        //***********************************************
        //***********************************************
       
	}


    //********************************************************
    //********************************************************


    // A UITextRange object represents a range of characters in a text container; in other words,
    // it identifies a starting index and an ending index in string backing a text-entry object.
    //
    // Classes that adopt the UITextInput protocol must create custom UITextRange objects for 
    // representing ranges within the text managed by the class. The starting and ending indexes 
    // of the range are represented by UITextPosition objects. The text system uses both UITextRange 
    // and UITextPosition objects for communicating text-layout information.
    class IndexedRange : UITextRange 
    {
        public NSRange Range { get; private set; }

        private IndexedRange ()
        {
        }

        public override UITextPosition start {
            get {
                return IndexedPosition.GetPosition (Range.Location);
            }
        }

        public override UITextPosition end {
            get {
                return IndexedPosition.GetPosition (Range.Location + Range.Length);
            }
        }

        public override bool IsEmpty {
            get {
                return Range.Length == 0;
            }
        }

        // We need to keep all the IndexedRanges we create accessible from managed code,
        // otherwise the garbage collector will collect them since it won't know that native
        // code has references to them.
        private static Dictionary<NSRange, IndexedRange> ranges = new Dictionary<NSRange, IndexedRange> (new NSRangeEqualityComparer ());
        public static IndexedRange GetRange (NSRange theRange)
        {
            IndexedRange result;

            if (theRange.Location == NSRange.NotFound)
                return null;

            if (!ranges.TryGetValue (theRange, out result)) {
                result = new IndexedRange ();
                result.Range = new NSRange (theRange.Location, theRange.Length);
            }
            return result;
        }

        class NSRangeEqualityComparer : IEqualityComparer<NSRange>
        {
            #region IEqualityComparer[NSRange] implementation
            public bool Equals (NSRange x, NSRange y)
            {
                return x.Length == y.Length && x.Location == y.Location;
            }

            public int GetHashCode (NSRange obj)
            {
                return obj.Location.GetHashCode () ^ obj.Length.GetHashCode ();
            }
            #endregion
        }

    }



    // A UITextPosition object represents a position in a text container; in other words, it is 
    // an index into the backing string in a text-displaying view.
    // 
    // Classes that adopt the UITextInput protocol must create custom UITextPosition objects 
    // for representing specific locations within the text managed by the class. The text input 
    // system uses both these objects and UITextRange objects for communicating text-layout information.
    //
    // We could use more sophisticated objects, but for demonstration purposes it suffices to wrap integers.
    class IndexedPosition : UITextPosition
    {
        public int Index { get; private set; }

        private IndexedPosition (int index)
        {
            this.Index = index;
        }

        // We need to keep all the IndexedPositions we create accessible from managed code,
        // otherwise the garbage collector will collect them since it won't know that native
        // code has references to them.
        static Dictionary<int, IndexedPosition> indexes = new Dictionary<int, IndexedPosition> ();
        public static IndexedPosition GetPosition (int index)
        {
            IndexedPosition result;

            if (!indexes.TryGetValue (index, out result)) {
                result = new IndexedPosition (index);
                indexes [index] = result;
            }
            return result;
        }
    }


    public class SimpleCoreTextView : UIView
    {
        string text;
        UIFont font;
        bool is_editing;
        NSRange markedTextRange;
        NSRange selectedTextRange;

        CTStringAttributes attributes;
        CTFramesetter framesetter;
        CTFrame frame;
        SimpleCaretView caretView;


        // Note that for this sample for simplicity, the selection color and
        // insertion point "caret" color are fixed and cannot be changed.
        public static UIColor SelectionColor = new UIColor (0.25f, 0.5f, 1.0f, 0.5f);
        public static UIColor CaretColor = new UIColor (0.25f, 0.5f, 1.0f, 1.0f);

        public SimpleCoreTextView (RectangleF frame)
            : base (frame)
        {
            Layer.GeometryFlipped = true;  // For ease of interaction with the CoreText coordinate system.
            attributes = new CTStringAttributes ();
            Text = string.Empty;
            Font = UIFont.SystemFontOfSize (18);
            BackgroundColor = UIColor.Clear;
            caretView = new SimpleCaretView (RectangleF.Empty);
        }

        protected override void Dispose (bool disposing)
        {
            Font = null;
            attributes = null;
            caretView = null;
            base.Dispose (disposing);
        }

        void ClearPreviousLayoutInformation ()
        {
            if (framesetter != null) {
                framesetter.Dispose ();
                framesetter = null;
            }

            if (frame != null) {
                frame.Dispose ();
                frame = null;
            }
        }

        public UIFont Font {
            get {
                return font;
            }
            set {
                font = value;
                attributes.Font = new CTFont (font.Name, font.PointSize);
                TextChanged ();
            }
        }

        NSAttributedString attributedString;
        void TextChanged ()
        {
            SetNeedsDisplay ();
            ClearPreviousLayoutInformation ();

            attributedString = new NSAttributedString (Text, attributes);
            framesetter = new CTFramesetter (attributedString);

            UIBezierPath path = UIBezierPath.FromRect (Bounds);
            frame = framesetter.GetFrame (new NSRange (0, 0), path.CGPath, null);
        }

        public string Text {
            get {
                return text;
            }
            set {
                text = value;
                TextChanged ();
            }
        }

        // Helper method for obtaining the intersection of two ranges (for handling
        // selection range across multiple line ranges in drawRangeAsSelection below)
        NSRange RangeIntersection (NSRange first, NSRange second)
        {
            NSRange result = new NSRange (NSRange.NotFound, 0);

            // Ensure first range does not start after second range
            if (first.Location > second.Location) {
                NSRange tmp = first;
                first = second;
                second = tmp;
            }

            // Find the overlap intersection range between first and second
            if (second.Location < first.Location + first.Length) {
                result.Location = second.Location;
                int end = Math.Min (first.Location + first.Length, second.Location + second.Length);
                result.Length = end - result.Location;
            }

            return result;
        }

        // Helper method for drawing the current selection range (as a simple filled rect)
        void DrawRangeAsSelection (NSRange selectionRange)
        {

            // If not in editing mode, we do not draw selection rects
            if (!IsEditing)
                return;

            // If selection range empty, do not draw
            if (selectionRange.Length == 0 || selectionRange.Location == NSRange.NotFound)
                return;

            // set the fill color to the selection color
            SelectionColor.SetFill ();

            // Iterate over the lines in our CTFrame, looking for lines that intersect
            // with the given selection range, and draw a selection rect for each intersection
            var lines = frame.GetLines ();

            for (int i = 0; i < lines.Length; i++) {
                CTLine line = lines [i];
                NSRange lineRange = line.StringRange;
                NSRange range = new NSRange (lineRange.Location, lineRange.Length);
                NSRange intersection = RangeIntersection (range, selectionRange);
                if (intersection.Location != NSRange.NotFound && intersection.Length > 0) {
                    // The text range for this line intersects our selection range
                    float xStart = line.GetOffsetForStringIndex (intersection.Location);
                    float xEnd = line.GetOffsetForStringIndex (intersection.Location + intersection.Length);
                    PointF [] origin = new PointF [lines.Length];
                    // Get coordinate and bounds information for the intersection text range
                    frame.GetLineOrigins (new NSRange (i, 0), origin);
                    float ascent, descent, leading;
                    line.GetTypographicBounds (out ascent, out descent, out leading);
                    // Create a rect for the intersection and draw it with selection color
                    RectangleF selectionRect = new RectangleF (xStart, origin [0].Y - descent, xEnd - xStart, ascent + descent);
                    UIGraphics.RectFill (selectionRect);
                }
            }
        }

        // Standard UIView drawRect override that uses Core Text to draw our text contents
        public override void Draw (RectangleF rect)
        {
            // First draw selection / marked text, then draw text
            DrawRangeAsSelection (SelectedTextRange);
            DrawRangeAsSelection (MarkedTextRange);
            frame.Draw (UIGraphics.GetCurrentContext ());
        }

        // Public method to find the text range index for a given CGPoint
        public int ClosestIndex (PointF point)
        {
            // Use Core Text to find the text index for a given CGPoint by
            // iterating over the y-origin points for each line, finding the closest
            // line, and finding the closest index within that line.
            var lines = frame.GetLines ();
            PointF[] origins = new PointF [lines.Length];
            frame.GetLineOrigins (new NSRange (0, lines.Length), origins);

            for (int i = 0; i < lines.Length; i++) {
                if (point.Y > origins [i].Y) {
                    // This line origin is closest to the y-coordinate of our point,
                    // now look for the closest string index in this line.
                    return lines [i].GetStringIndexForPosition (point);
                }
            }

            return text.Length;
        }

        // Public method to determine the CGRect for the insertion point or selection, used
        // when creating or updating our SimpleCaretView instance
        public RectangleF CaretRect (int index)
        {
            var lines = frame.GetLines ();

            // Special case, no text
            if (text.Length == 0) {
                PointF origin = new PointF (Bounds.GetMinX (), Bounds.GetMinY () - font.Leading);
                // Note: using fabs() for typically negative descender from fonts
                return new RectangleF (origin.X, origin.Y - Math.Abs (font.Descender), 3, font.Ascender + Math.Abs (font.Descender));
            }

            // Special case, insertion point at final position in text after newline
            if (index == text.Length && text.EndsWith ("\n")) {
                CTLine line = lines [lines.Length - 1];
                NSRange range = line.StringRange;
                float xPos = line.GetOffsetForStringIndex (range.Location);
                PointF[] origins = new PointF [lines.Length];
                float ascent, descent, leading;
                line.GetTypographicBounds (out ascent, out descent, out leading);
                frame.GetLineOrigins (new NSRange (lines.Length - 1, 0), origins);
                // Place point after last line, including any font leading spacing if applicable
                origins[0].Y -= font.Leading;
                return new RectangleF (xPos, origins [0].Y - descent, 3, ascent + descent);        
            }

            // Regular case, caret somewhere within our text content range
            for (int i = 0; i < lines.Length; i++) {
                CTLine line = lines [i];
                NSRange range = line.StringRange;
                int localIndex = index - range.Location;
                if (localIndex >= 0 && localIndex <= range.Length) {
                    // index is in the range for this line
                    float xPos = line.GetOffsetForStringIndex (index);
                    PointF[] origins = new PointF [lines.Length];
                    float ascent, descent, leading;
                    line.GetTypographicBounds (out ascent, out descent, out leading);
                    frame.GetLineOrigins (new NSRange (i, 0), origins);
                    // Make a small "caret" rect at the index position
                    return new RectangleF (xPos, origins [0].Y - descent, 3, ascent + descent);
                }
            }

            return RectangleF.Empty;
        }

        // Public method to create a rect for a given range in the text contents
        // Called by our EditableTextRange to implement the required 
        // UITextInput:firstRectForRange method
        public RectangleF FirstRect (NSRange range)
        {    
            int index = range.Location;

            // Iterate over our CTLines, looking for the line that encompasses the given range
            var lines = frame.GetLines ();
            for (int i = 0; i < lines.Length; i++) {
                CTLine line = lines [i];
                NSRange lineRange = line.StringRange;
                int localIndex = index - lineRange.Location;
                if (localIndex >= 0 && localIndex < lineRange.Length) {
                    // For this sample, we use just the first line that intersects range
                    int finalIndex = Math.Min (lineRange.Location + lineRange.Length, range.Location + range.Length);
                    // Create a rect for the given range within this line
                    float xStart = line.GetOffsetForStringIndex (index);
                    float xEnd = line.GetOffsetForStringIndex (finalIndex);
                    PointF[] origins = new PointF [lines.Length];
                    frame.GetLineOrigins (new NSRange (i, 0), origins);
                    float ascent, descent, leading;
                    line.GetTypographicBounds (out ascent, out descent, out leading);

                    return new RectangleF (xStart, origins [0].Y - descent, xEnd - xStart, ascent + descent);
                }
            }

            return RectangleF.Empty;
        }

        // Helper method to update caretView when insertion point/selection changes
        void SelectionChanged ()
        {
            // If not in editing mode, we don't show the caret
            if (!IsEditing) {
                caretView.RemoveFromSuperview ();
                return;
            }

            // If there is no selection range (always true for this sample), find the
            // insert point rect and create a caretView to draw the caret at this position
            if (SelectedTextRange.Length == 0) {
                caretView.Frame = CaretRect (SelectedTextRange.Location);
                if (caretView.Superview == null) {
                    AddSubview (caretView);
                    SetNeedsDisplay ();
                }
                // Set up a timer to "blink" the caret
                caretView.DelayBlink ();
            } else {
                // If there is an actual selection, don't draw the insertion caret too
                caretView.RemoveFromSuperview ();
                SetNeedsDisplay ();
            }    

            if (MarkedTextRange.Location != NSRange.NotFound) {
                SetNeedsDisplay ();
            }
        }

        // markedTextRange property accessor overrides 
        public NSRange MarkedTextRange {
            get {
                return markedTextRange;
            }
            set {
                markedTextRange = value;
                // Call selectionChanged to update view if necessary
                SelectionChanged ();
            }
        }

        // selectedTextRange property accessor overrides
        public NSRange SelectedTextRange {
            get {
                return selectedTextRange;
            }
            set {
                selectedTextRange = value;
                // Call selectionChanged to update view if necessary
                SelectionChanged ();
            }
        }

        // editing property accessor overrides
        public bool IsEditing {
            get {
                return is_editing; 
            }
            set {
                is_editing = value;
                SelectionChanged ();
            }
        }


    }

    class SimpleCaretView : UIView
    {
        NSTimer blink_timer;
        const double InitialBlinkDelay = 0.7;
        const double BlinkRate = 0.5;

        public SimpleCaretView (RectangleF frame)
            : base (frame)
        {
            BackgroundColor = SimpleCoreTextView.CaretColor;
        }

        // Helper method to toggle hidden state of caret view
        public void Blink ()
        {
            Hidden = !Hidden;
        }

        public void DelayBlink ()
        {
            Hidden = false;
            blink_timer.FireDate = NSDate.FromTimeIntervalSinceNow (InitialBlinkDelay);
        }

        // UIView didMoveToSuperview override to set up blink timers after caret view created in superview
        public override void MovedToSuperview ()
        {
            Hidden = false;

            if (Superview != null) {
                blink_timer = NSTimer.CreateRepeatingScheduledTimer (BlinkRate, () => Blink ());
                DelayBlink ();
            } else {
                blink_timer.Invalidate ();
                blink_timer.Dispose ();
                blink_timer = null;
            }
        }

        protected override void Dispose (bool disposing)
        {
            base.Dispose (disposing);
        }
    }


    //********************************************************
    //********************************************************






}
