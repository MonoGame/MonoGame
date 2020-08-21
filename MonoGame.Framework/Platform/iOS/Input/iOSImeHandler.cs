using CoreGraphics;
using Foundation;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using System;
using UIKit;

namespace Microsoft.Xna.Framework.iOS.Input
{
    internal class UIBackwardsTextField : UITextField
    {
        // A delegate type for hooking up change notifications.
        public delegate void DeleteBackwardEventHandler(object sender, EventArgs e);

        // An event that clients can use to be notified whenever the
        // elements of the list change.
        public event DeleteBackwardEventHandler DeleteBackwardPressed;
        public event EventHandler TextChanged;
        public event EventHandler TextCompositionChanged;

        private bool UIBackwardsTextField_ShouldChangeCharacters(UITextField textField, NSRange range, string replacementString)
        {
            if (textField.IsFirstResponder)
            {
                if (textField.TextInputMode == null)
                    return false;
            }

            return true;
        }

        public UIBackwardsTextField(CGRect rect) : base(rect)
        {
            this.EditingChanged += UIBackwardsTextField_EditingChanged;
            this.ShouldChangeCharacters += UIBackwardsTextField_ShouldChangeCharacters;
        }

        private void UIBackwardsTextField_EditingChanged(object sender, EventArgs e)
        {
            if (MarkedTextRange == null || MarkedTextRange.IsEmpty)
            {
                if (TextChanged != null)
                    TextChanged(null, null);
            }
            else
            {
                if (TextCompositionChanged != null)
                    TextCompositionChanged(null, null);
            }
        }

        public void OnDeleteBackwardPressed()
        {
            if (DeleteBackwardPressed != null)
                DeleteBackwardPressed(null, null);
        }

        public override void DeleteBackward()
        {
            base.DeleteBackward();
            OnDeleteBackwardPressed();
        }
    }

    public class iOSImeHandler : ImmService
    {
        private UIWindow mainWindow;
        private UIViewController gameViewController;

        private UIBackwardsTextField textField;

        private int _virtualKeyboardHeight;

        public iOSImeHandler(Game game)
        {
            mainWindow = game.Services.GetService<UIWindow>();
            gameViewController = game.Services.GetService<UIViewController>();

            textField = new UIBackwardsTextField(new CGRect(0, -400, 200, 40));
            textField.KeyboardType = UIKeyboardType.Default;
            textField.ReturnKeyType = UIReturnKeyType.Done;
            textField.DeleteBackwardPressed += TextField_DeleteBackward;
            textField.TextChanged += TextField_TextChanged;
            textField.TextCompositionChanged += TextField_TextCompositionChanged;
            textField.ShouldReturn += TextField_ShouldReturn;

            gameViewController.Add(textField);

            UIKeyboard.Notifications.ObserveWillShow((s, e) =>
            {
                _virtualKeyboardHeight = (int)(e.FrameEnd.Height * UIScreen.MainScreen.Scale);
            });

            UIKeyboard.Notifications.ObserveWillHide((s, e) =>
            {
                _virtualKeyboardHeight = 0;
            });
        }

        public override event EventHandler<TextCompositionEventArgs> TextComposition;
        public override event EventHandler<TextInputEventArgs> TextInput;

        public override int VirtualKeyboardHeight { get { return _virtualKeyboardHeight; } }

        private bool TextField_ShouldReturn(UITextField textfield)
        {
            StopTextInput();

            // We do not want UITextField to insert line-breaks.
            return false;
        }

        private void TextField_TextChanged(object sender, EventArgs e)
        {
            // Mimic a CompositionEnd event
            if (TextComposition != null)
                TextComposition.Invoke(this, new TextCompositionEventArgs(IMEString.Empty, 0));

            foreach (var c in textField.Text)
                if (TextInput != null)
                    TextInput.Invoke(this, new TextInputEventArgs(c, KeyboardUtil.ToXna(c)));

            textField.Text = string.Empty;
        }

        const char SIX_PER_EM_SPACE = (char)8198;
        private void TextField_TextCompositionChanged(object sender, EventArgs e)
        {
            var textRange = textField.MarkedTextRange;
            var compStr = textField.TextInRange(textRange);
                compStr = compStr.Replace(SIX_PER_EM_SPACE, ' ');
            if (TextComposition != null)
                TextComposition.Invoke(this, new TextCompositionEventArgs(new IMEString(compStr), compStr.Length));
        }

        private void TextField_DeleteBackward(object sender, EventArgs e)
        {
            var key = Keys.Back;
            if (TextInput != null)
                TextInput.Invoke(this, new TextInputEventArgs((char)key, key));
        }

        public override void StartTextInput()
        {
            if (IsTextInputActive)
                return;

            textField.BecomeFirstResponder();
            IsTextInputActive = true;
        }

        public override void StopTextInput()
        {
            if (!IsTextInputActive)
                return;

            textField.Text = string.Empty;
            textField.ResignFirstResponder();
            IsTextInputActive = false;
        }

        const int KeyboardHideOffset = 20;

        internal void Update()
        {
            if (!IsTextInputActive) return;

            TouchCollection touchCollection = TouchPanel.GetState();
            foreach (TouchLocation touchLocation in touchCollection)
            {
                if (TouchLocationState.Pressed == touchLocation.State)
                {
                    if (touchLocation.Position.Y < ((mainWindow.GetFrame().Height * UIScreen.MainScreen.Scale - _virtualKeyboardHeight) - KeyboardHideOffset))
                        StopTextInput();
                }
            }
        }

        public override void SetTextInputRect(Rectangle rect)
        {
        }
    }
}
