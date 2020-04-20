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

    public class iOSImeHandler : IImmService
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
            textField.TextChanged += TextField_TextChanged; ;
            textField.ShouldReturn += TextField_ShouldReturn;

            gameViewController.Add(textField);

            UIKeyboard.Notifications.ObserveWillShow((s, e) =>
            {
                _virtualKeyboardHeight = (int)e.FrameEnd.Height;
            });

            UIKeyboard.Notifications.ObserveWillHide((s, e) =>
            {
                _virtualKeyboardHeight = 0;
            });
        }

        public bool IsTextInputActive { get; private set; }
        public event EventHandler<TextCompositionEventArgs> TextComposition;
        public event EventHandler<TextInputEventArgs> TextInput;

        public bool ShowOSImeWindow { get { return true; } set { } }
        public int VirtualKeyboardHeight { get { return _virtualKeyboardHeight; } }

        private bool TextField_ShouldReturn(UITextField textfield)
        {
            // Not found, so remove keyboard.
            textfield.ResignFirstResponder();

            // We do not want UITextField to insert line-breaks.
            return false;
        }

        private void TextField_TextChanged(object sender, EventArgs e)
        {
            foreach (var c in textField.Text)
                if (TextInput != null)
                    TextInput.Invoke(this, new TextInputEventArgs(c, KeyboardUtil.ToXna(c)));

            textField.Text = string.Empty;
        }

        private void TextField_DeleteBackward(object sender, EventArgs e)
        {
            var key = Keys.Back;
            if (TextInput != null)
                TextInput.Invoke(this, new TextInputEventArgs((char)key, key));
        }

        public void StartTextInput()
        {
            if (IsTextInputActive)
                return;

            textField.BecomeFirstResponder();
            IsTextInputActive = true;
        }

        public void StopTextInput()
        {
            if (!IsTextInputActive)
                return;

            textField.Text = string.Empty;
            textField.ResignFirstResponder();
            IsTextInputActive = false;
        }

        const int KeyboardHideOffset = 80;

        public void Update(GameTime gameTime)
        {
            TouchCollection touchCollection = TouchPanel.GetState();
            foreach (TouchLocation touchLocation in touchCollection)
            {
                if (TouchLocationState.Pressed == touchLocation.State)
                {
                    if (touchLocation.Position.Y < ((mainWindow.GetFrame().Y - _virtualKeyboardHeight) - KeyboardHideOffset))
                        StopTextInput();
                }
            }
        }

        public void SetTextInputRect(Rectangle rect)
        {
        }
    }
}
