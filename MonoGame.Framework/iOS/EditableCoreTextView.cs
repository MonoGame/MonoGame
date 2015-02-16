using System;
using System.Drawing;
using System.Text;

using System.Threading.Tasks;

using System.Collections.Generic;
using System.Linq;

using CoreGraphics;
using CoreText;
using CoreAnimation;
using Foundation;
using ObjCRuntime;
using OpenGLES;
using UIKit;

using OpenTK.Graphics;
using OpenTK.Graphics.ES20;
using OpenTK.Platform.iPhoneOS;

using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;

using All = OpenTK.Graphics.ES20.All;
using Microsoft.Xna.Framework.Input;


namespace Microsoft.Xna.Framework
{
	[Adopts ("UITextInput")]
	[Adopts ("UIKeyInput")]
	[Adopts ("UITextInputTraits")]
	[Register ("EditableCoreTextView")]

	public class EditableCoreTextView : UIView
	{
		StringBuilder text = new StringBuilder ();
		IUITextInputTokenizer tokenizer;
		SimpleCoreTextView textView;
		NSDictionary markedTextStyle;
		IUITextInputDelegate inputDelegate;
		public UITextField textfield;

		public delegate void ViewWillEditDelegate (EditableCoreTextView editableCoreTextView);
		public event ViewWillEditDelegate ViewWillEdit;

		public EditableCoreTextView (CGRect frame)
			: base (frame)
		{
			// Add tap gesture recognizer to let the user enter editing mode
			UITapGestureRecognizer tap = new UITapGestureRecognizer (Tap) {
				ShouldReceiveTouch = delegate(UIGestureRecognizer recognizer, UITouch touch) {
					// If gesture touch occurs in our view, we want to handle it
					return touch.View == this;
				}
			};
			AddGestureRecognizer (tap);

			// Create our tokenizer and text storage
			tokenizer = new UITextInputStringTokenizer ();

			// Create and set up our SimpleCoreTextView that will do the drawing
			textView = new SimpleCoreTextView (Bounds.Inset (5, 5));
			textView.AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleHeight;
			UserInteractionEnabled = true;
			AutosizesSubviews = true;
			AddSubview (textView);
			textView.Text = string.Empty;
			textView.UserInteractionEnabled = false;

			textfield = new UITextField
			{
				Placeholder = "Enter your username",
				BorderStyle = UITextBorderStyle.RoundedRect,
				Frame = new RectangleF(10, 32, 0, 0)
			};

			textfield.KeyboardAppearance = UIKeyboardAppearance.Dark;
			textfield.KeyboardType = UIKeyboardType.NumberPad;

			AddSubview(textfield);
		}

		protected override void Dispose (bool disposing)
		{
			markedTextStyle = null;
			tokenizer = null;
			text = null;
			textView = null;
			base.Dispose (disposing);
		}

		public delegate void KeyboardTextEntryEventHandler(object sender, KeyboardEntryEventArgs e);
		public event KeyboardTextEntryEventHandler KeyboardTextEntry;
		// Invoke the KeyboardTextEntry event
		public virtual void OnKeyboardTextEntry(KeyboardEntryEventArgs e) 
		{
			if (KeyboardTextEntry != null) 
			{
				KeyboardTextEntry (this, e);
			}
		}

		public delegate void KeyboardTextChangeEventHandler(object sender, KeyboardEntryEventArgs e);
		public event KeyboardTextChangeEventHandler KeyboardTextChange;
		// Invoke the KeyboardTextEntry event
		public virtual void OnKeyboardTextChange(KeyboardEntryEventArgs e) 
		{
			if (KeyboardTextChange != null) 
			{
				KeyboardTextChange (this, e);
			}
		}

		public delegate void SelectionChangedEventHandler(object sender, SelectionChangedEventArgs e);
		public event SelectionChangedEventHandler SelectionChanged;
		// Invoke the KeyboardTextEntry event
		public virtual void OnSelectionChanged(SelectionChangedEventArgs e) 
		{
			if (SelectionChanged != null) 
			{
				SelectionChanged (this, e);
			}
		}

		public delegate void TabKeyPressedEventHandler(object sender, EventArgs e);
		public event TabKeyPressedEventHandler TabKeyPressed;
		// Invoke the KeyboardTextEntry event
		public virtual void OnTabKeyPressed(EventArgs e) 
		{
			if (TabKeyPressed != null) 
			{
				TabKeyPressed (this, e);
			}
		}

		public void Restart()
		{
			if (!IsFirstResponder) {
				// Inform controller that we're about to enter editing mode
				if (ViewWillEdit != null)
				{
					ViewWillEdit (this);
				}

				textfield.BecomeFirstResponder();

				// Flag that underlying SimpleCoreTextView is now in edit mode
				// Become first responder state (which shows software keyboard, if applicable)
				BecomeFirstResponder();

			} 

			textView.MarkedTextRange = new NSRange (NSRange.NotFound, 0);
			textView.SelectedTextRange = new NSRange (0, 0);
			this.text.Clear();
			textView.IsEditing = true;
			textView.Text = string.Empty;
		}

		public void SetText(string text)
		{
			if (text != textView.Text)
			{		
				this.text.Clear();
				this.text.Append(text);
				textView.Text = text;
			}
		}

		public void SetSelection(NSRange range)
		{
			textView.SelectedTextRange = range;
		}
			
		#region Custom user interaction

		// UIResponder protocol override - our view can become first responder to
		// receive user text input
		public override bool CanBecomeFirstResponder {
			get 
			{
				return true;
			}
		}

		// UIResponder protocol override - called when our view is being asked to resign
		// first responder state (in this sample by using the "Done" button)
		public override bool ResignFirstResponder ()
		{
			textView.IsEditing = false;
			return base.ResignFirstResponder ();
		}
						
		// Our tap gesture recognizer selector that enters editing mode, or if already
		// in editing mode, updates the text insertion point
		[Export ("Tap:")]
		public void Tap (UITapGestureRecognizer tap)
		{
			if (!IsFirstResponder) {
				// Inform controller that we're about to enter editing mode
				if (ViewWillEdit != null)
					ViewWillEdit (this);
				// Flag that underlying SimpleCoreTextView is now in edit mode
				textView.IsEditing = true;
				// Become first responder state (which shows software keyboard, if applicable)
				BecomeFirstResponder ();
			} else {
				// Already in editing mode, set insertion point (via selectedTextRange)
				// Find and update insertion point in underlying SimpleCoreTextView
				int index = textView.ClosestIndex (tap.LocationInView (textView));
				textView.MarkedTextRange = new NSRange (NSRange.NotFound, 0);
				textView.SelectedTextRange = new NSRange (index, 0);
			}
		}
		#endregion

		#region UITextInput methods
		[Export ("inputDelegate")]
		public IUITextInputDelegate InputDelegate {
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
			return text.ToString ().Substring ((int)r.Range.Location, (int)r.Range.Length);
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
			this.text.Remove ((int)r.Range.Location, (int)r.Range.Length);
			this.text.Insert ((int)r.Range.Location, text);

			// Update underlying SimpleCoreTextView
			textView.Text = this.text.ToString();
			textView.SelectedTextRange = selectedNSRange;


			if (text == " " || text == "\n") 
			{
				OnKeyboardTextChange (new KeyboardEntryEventArgs (text));
			} else if (text.Trim () != "") {

				OnKeyboardTextChange (new KeyboardEntryEventArgs (text, (int)r.Range.Location, (int)r.Range.Length, (int)(r.Range.Location + r.Range.Length)));
			}

		}
		#endregion

		public UIKeyboardType _keyboardType = UIKeyboardType.EmailAddress;

		[Export ("keyboardAppearance")]
		UIKeyboardAppearance keyboardAppearance {
			get 
			{
				return UIKeyboardAppearance.Dark;
			}
		}

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
			
				OnSelectionChanged(new SelectionChangedEventArgs(value.Range));
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
				text.Remove ((int)markedTextRange.Location, (int)markedTextRange.Length);
				text.Insert ((int)markedTextRange.Location, markedText);
				markedTextRange.Length = markedText.Length;
			} else if (selectedNSRange.Length > 0) {
				// There currently isn't a marked text range, but there is a selected range,
				// so replace text storage at selected range and update markedTextRange.
				text.Remove ((int)selectedNSRange.Location, (int)selectedNSRange.Length);
				text.Insert ((int)selectedNSRange.Location, markedText);
				markedTextRange.Location = selectedNSRange.Location;
				markedTextRange.Length = markedText.Length;
			} else {
				// There currently isn't marked or selected text ranges, so just insert
				// given text into storage and update markedTextRange.
				text.Insert ((int)selectedNSRange.Location, markedText);
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
			// Generate IndexedPosition instances that wrap the to and from ranges
			IndexedPosition @from = (IndexedPosition) fromPosition;
			IndexedPosition @to = (IndexedPosition) toPosition;
			NSRange range = new NSRange (Math.Min (@from.Index, @to.Index), Math.Abs (to.Index - @from.Index));
			return IndexedRange.GetRange (range);
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
		IUITextInputTokenizer Tokenizer {
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
			int pos = (int)r.Range.Location;

			// For this sample, we just return the extent of the given range if the
			// given direction is "forward" in a LTR context (UITextLayoutDirectionRight
			// or UITextLayoutDirectionDown), otherwise we return just the range position
			switch (direction) {
			case UITextLayoutDirection.Up:
			case UITextLayoutDirection.Left:
				pos = (int)r.Range.Location;
				break;
			case UITextLayoutDirection.Right:
			case UITextLayoutDirection.Down:
				pos = (int)(r.Range.Location + r.Range.Length);
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
		CGRect FirstRect (UITextRange range)
		{
			// FIXME: the Objective-C code doesn't get a null range
			// This is the reason why we don't get the autocorrection suggestions
			// (it'll just autocorrect without showing any suggestions).
			// Possibly due to http://bugzilla.xamarin.com/show_bug.cgi?id=265
			IndexedRange r = (IndexedRange) (range ?? IndexedRange.GetRange (new NSRange (0, 1)));
			// Use underlying SimpleCoreTextView to get rect for range
			CGRect rect = textView.FirstRect (r.Range);
			// Convert rect to our view coordinates
			return ConvertRectFromView (rect, textView);
		}

		// UITextInput protocol required method - Return a rectangle used to draw the caret
		// at a given insertion point.
		[Export ("caretRectForPosition:")]
		CGRect CaretRect (UITextPosition position)
		{
			// FIXME: the Objective-C code doesn't get a null position
			// This is the reason why we don't get the autocorrection suggestions
			// (it'll just autocorrect without showing any suggestions).
			// Possibly due to http://bugzilla.xamarin.com/show_bug.cgi?id=265
			IndexedPosition pos = (IndexedPosition) (position ?? IndexedPosition.GetPosition (0));
			// Get caret rect from underlying SimpleCoreTextView
			CGRect rect = textView.CaretRect (pos.Index);
			// Convert rect to our view coordinates
			return ConvertRectFromView (rect, textView);
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
		UITextPosition ClosestPosition (CGPoint point)
		{
			// Not implemented in this sample.  Could utilize underlying
			// SimpleCoreTextView:closestIndexToPoint:point
			return null;
		}

		// UITextInput protocol required method - Return the position in a document that
		// is closest to a specified point in a given range.
		[Export ("closestPositionToPoint:withinRange:")]
		UITextPosition ClosestPosition (CGPoint point, UITextRange range)
		{
			// Not implemented in this sample.  Could utilize underlying
			// SimpleCoreTextView:closestIndexToPoint:point
			return null;
		}

		// UITextInput protocol required method - Return the character or range of
		// characters that is at a given point in a document.
		[Export ("characterRangeAtPoint:")]
		UITextRange CharacterRange (CGPoint point)
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
			return new NSDictionary (CTStringAttributeKey.Font, textView.Font);
		}
		#endregion
		#region UIKeyInput methods
		// UIKeyInput required method - A Boolean value that indicates whether the text-entry
		// objects have any text.
		[Export ("hasText")]
		bool HasText {
			get { return text.Length > 0; }
		}

		// UIKeyInput required method - Insert a character into the displayed text.
		// Called by the text system when the user has entered simple text
		[Export ("insertText:")]
		void InsertText (string text)
		{
			if (text == " " || text == "\n") 
			{
				OnKeyboardTextEntry (new KeyboardEntryEventArgs (text));
			} else if (text.Trim () != "") {
				OnKeyboardTextEntry (new KeyboardEntryEventArgs (text));
			}
			else
			{
				OnTabKeyPressed(EventArgs.Empty);
			}

			NSRange selectedNSRange = textView.SelectedTextRange;
			NSRange markedTextRange = textView.MarkedTextRange;

			// Note: While this sample does not provide a way for the user to
			// create marked or selected text, the following code still checks for
			// these ranges and acts accordingly.
			if (markedTextRange.Location != NSRange.NotFound) {
				// There is marked text -- replace marked text with user-entered text
				this.text.Remove ((int)markedTextRange.Location, (int)markedTextRange.Length);
				this.text.Insert ((int)markedTextRange.Location, text);
				selectedNSRange.Location = markedTextRange.Location + text.Length;
				selectedNSRange.Length = 0;
				markedTextRange = new NSRange (NSRange.NotFound, 0);
			} else if (selectedNSRange.Length > 0) {
				// Replace selected text with user-entered text
				this.text.Remove ((int)selectedNSRange.Location, (int)selectedNSRange.Length);
				this.text.Insert ((int)selectedNSRange.Location, text);
				selectedNSRange.Length = 0;
				selectedNSRange.Location += text.Length;
			} else {
				// Insert user-entered text at current insertion point
				this.text.Insert ((int)selectedNSRange.Location, text);
				selectedNSRange.Location += text.Length;
			}

			// Update underlying SimpleCoreTextView
			textView.Text = this.text.ToString ();
			textView.MarkedTextRange = markedTextRange;
			textView.SelectedTextRange = selectedNSRange;
		}

		// UIKeyInput required method - Delete a character from the displayed text.
		// Called by the text system when the user is invoking a delete (e.g. pressing
		// the delete software keyboard key)
		[Export ("deleteBackward")]
		void DeleteBackward ()
		{
			OnKeyboardTextEntry(new KeyboardEntryEventArgs (Keypresses.backspaceDeleteKey));

			NSRange selectedNSRange = textView.SelectedTextRange;
			NSRange markedTextRange = textView.MarkedTextRange;

			// Note: While this sample does not provide a way for the user to
			// create marked or selected text, the following code still checks for
			// these ranges and acts accordingly.
			if (markedTextRange.Location != NSRange.NotFound) {
				// There is marked text, so delete it
				text.Remove ((int)markedTextRange.Location, (int)markedTextRange.Length);
				selectedNSRange.Location = markedTextRange.Location;
				selectedNSRange.Length = 0;
				markedTextRange = new NSRange (NSRange.NotFound, 0);
			} else if (selectedNSRange.Length > 0) {
				// Delete the selected text
				text.Remove ((int)selectedNSRange.Location, (int)selectedNSRange.Length);
				selectedNSRange.Length = 0;
			} else if (selectedNSRange.Location > 0) {
				// Delete one char of text at the current insertion point
				selectedNSRange.Location--;
				selectedNSRange.Length = 1;
				text.Remove ((int)selectedNSRange.Location, (int)selectedNSRange.Length);
				selectedNSRange.Length = 0;
			}

			// Update underlying SimpleCoreTextView
			textView.Text = text.ToString ();
			textView.MarkedTextRange = markedTextRange;
			textView.SelectedTextRange = selectedNSRange;
		}
	}
	#endregion
	#endregion

	//*********************************************************************


	public class SimpleCoreTextView : UIView
	{
		string text = "";
		string selectionText = "";
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

		public SimpleCoreTextView (CGRect frame)
			: base (frame)
		{
			Layer.GeometryFlipped = true;  // For ease of interaction with the CoreText coordinate system.
			attributes = new CTStringAttributes ();
			Text = string.Empty;
			Font = UIFont.SystemFontOfSize (18);
			BackgroundColor = UIColor.Clear;
			caretView = new SimpleCaretView (CGRect.Empty);
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
				int end = Math.Min ((int)(first.Location + first.Length), (int)(second.Location + second.Length));
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
					nfloat xStart = line.GetOffsetForStringIndex (intersection.Location);
					nfloat xEnd = line.GetOffsetForStringIndex (intersection.Location + intersection.Length);
					var origin = new CGPoint [lines.Length];
					// Get coordinate and bounds information for the intersection text range
					frame.GetLineOrigins (new NSRange (i, 0), origin );
					nfloat ascent, descent, leading;
					line.GetTypographicBounds (out ascent, out descent, out leading);
					// Create a rect for the intersection and draw it with selection color
					CGRect selectionRect = new CGRect (xStart, origin [0].Y - descent, xEnd - xStart, ascent + descent);
					UIGraphics.RectFill (selectionRect);
				}
			}
		}

		// Standard UIView drawRect override that uses Core Text to draw our text contents
		public override void Draw (CGRect rect)
		{
			// First draw selection / marked text, then draw text
			//DrawRangeAsSelection (SelectedTextRange);
			//DrawRangeAsSelection (MarkedTextRange);
			//frame.Draw (UIGraphics.GetCurrentContext ());

			//caretView.RemoveFromSuperview ();
		}

		// Public method to find the text range index for a given CGPoint
		public int ClosestIndex (CGPoint point)
		{
			// Use Core Text to find the text index for a given CGPoint by
			// iterating over the y-origin points for each line, finding the closest
			// line, and finding the closest index within that line.
			var lines = frame.GetLines ();
			var origins = new CGPoint [lines.Length];
			frame.GetLineOrigins (new NSRange (0, lines.Length), origins);

			for (int i = 0; i < lines.Length; i++) {
				if (point.Y > origins [i].Y) {
					// This line origin is closest to the y-coordinate of our point,
					// now look for the closest string index in this line.
					return (int)lines [i].GetStringIndexForPosition (point);
				}
			}

			return text.Length;
		}

		// Public method to determine the CGRect for the insertion point or selection, used
		// when creating or updating our SimpleCaretView instance
		public CGRect CaretRect (int index)
		{
			var lines = frame.GetLines ();

			// Special case, no text
			if (text.Length == 0) {
				CGPoint origin = new CGPoint (Bounds.GetMinX (), Bounds.GetMinY () - font.Leading);
				// Note: using fabs() for typically negative descender from fonts
				return new CGRect (origin.X, origin.Y - (nfloat)Math.Abs (font.Descender), 3, font.Ascender + (nfloat)Math.Abs (font.Descender));
			}

			// Special case, insertion point at final position in text after newline
			if (index == text.Length && text.EndsWith ("\n")) {
				CTLine line = lines [lines.Length - 1];
				NSRange range = line.StringRange;
				nfloat xPos = line.GetOffsetForStringIndex (range.Location);
				var origins = new CGPoint [lines.Length];
				nfloat ascent, descent, leading;
				line.GetTypographicBounds (out ascent, out descent, out leading);
				frame.GetLineOrigins (new NSRange (lines.Length - 1, 0), origins);
				// Place point after last line, including any font leading spacing if applicable
				origins[0].Y -= font.Leading;
				return new CGRect (xPos, origins [0].Y - descent, 3, ascent + descent);
			}

			// Regular case, caret somewhere within our text content range
			for (int i = 0; i < lines.Length; i++) {
				CTLine line = lines [i];
				NSRange range = line.StringRange;
				int localIndex = index - (int)range.Location;
				if (localIndex >= 0 && localIndex <= range.Length) {
					// index is in the range for this line
					nfloat xPos = line.GetOffsetForStringIndex (index);
					var origins = new CGPoint [lines.Length];
					nfloat ascent, descent, leading;
					line.GetTypographicBounds (out ascent, out descent, out leading);
					frame.GetLineOrigins (new NSRange (i, 0), origins);
					// Make a small "caret" rect at the index position
					return new CGRect (xPos, origins [0].Y - descent, 3, ascent + descent);
				}
			}

			return CGRect.Empty;
		}

		// Public method to create a rect for a given range in the text contents
		// Called by our EditableTextRange to implement the required
		// UITextInput:firstRectForRange method
		public CGRect FirstRect (NSRange range)
		{
			int index = (int)range.Location;

			// Iterate over our CTLines, looking for the line that encompasses the given range
			var lines = frame.GetLines ();
			for (int i = 0; i < lines.Length; i++) {
				CTLine line = lines [i];
				NSRange lineRange = line.StringRange;
				int localIndex = index - (int)lineRange.Location;
				if (localIndex >= 0 && localIndex < lineRange.Length) {
					// For this sample, we use just the first line that intersects range
					int finalIndex = (int)Math.Min (lineRange.Location + lineRange.Length, range.Location + range.Length);
					// Create a rect for the given range within this line
					nfloat xStart = line.GetOffsetForStringIndex (index);
					nfloat xEnd = line.GetOffsetForStringIndex (finalIndex);
					var origins = new CGPoint [lines.Length];
					frame.GetLineOrigins (new NSRange (i, 0), origins);
					nfloat ascent, descent, leading;
					line.GetTypographicBounds (out ascent, out descent, out leading);

					return new CGRect (xStart, origins [0].Y - descent, xEnd - xStart, ascent + descent);
				}
			}

			return CGRect.Empty;
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
				caretView.Frame = CaretRect ((int)SelectedTextRange.Location);
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

		public SimpleCaretView (CGRect frame)
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
				blink_timer = NSTimer.CreateRepeatingScheduledTimer (BlinkRate, (timer) => Blink ());
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



	//*********************************************************************




	// A UITextRange object represents a range of characters in a text container; in other words,
	// it identifies a starting index and an ending index in string backing a text-entry object.
	//
	// Classes that adopt the UITextInput protocol must create custom UITextRange objects for 
	// representing ranges within the text managed by the class. The starting and ending indexes 
	// of the range are represented by UITextPosition objects. The text system uses both UITextRange 
	// and UITextPosition objects for communicating text-layout information.
	public class IndexedRange : UITextRange 
	{
		public NSRange Range { get; private set; }

		private IndexedRange ()
		{

		}

		public override UITextPosition Start {
			get {
				return IndexedPosition.GetPosition ((int)Range.Location);
			}
		}

		public override UITextPosition End {
			get {
				return IndexedPosition.GetPosition ((int)Range.Location + (int)Range.Length);
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


	//*********************************************************************



	// A UITextPosition object represents a position in a text container; in other words, it is 
	// an index into the backing string in a text-displaying view.
	// 
	// Classes that adopt the UITextInput protocol must create custom UITextPosition objects 
	// for representing specific locations within the text managed by the class. The text input 
	// system uses both these objects and UITextRange objects for communicating text-layout information.
	//
	// We could use more sophisticated objects, but for demonstration purposes it suffices to wrap integers.
	public class IndexedPosition : UITextPosition
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

	//*********************************************************************

}

