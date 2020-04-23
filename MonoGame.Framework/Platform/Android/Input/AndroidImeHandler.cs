using System;
using Android.Views;
using Android.Text;
using Android.Views.InputMethods;
using Microsoft.Xna.Framework.Input.Touch;
using Android.Widget;
using Android.App;

namespace Microsoft.Xna.Framework.Input
{
    public class AndroidImeHandler : IImmService
    {
        private const int IME_FLAG_NO_EXTRACT_UI = 0x10000000;

        private EditText editText;
        private InputMethodManager inputMethodManager;
        private MonoGameAndroidGameView _gameView;

        private Android.Graphics.Point ScreenSize { get { return Game.Activity.ScreenSize; } }
        public bool IsTextInputActive { get; private set; }
        public bool ShowOSImeWindow { get { return true; } set { } }
        public event EventHandler<TextCompositionEventArgs> TextComposition;
        public event EventHandler<TextInputEventArgs> TextInput;
        public int VirtualKeyboardHeight { get { return Game.Activity.KeyboardHeight; } }

        public AndroidImeHandler(MonoGameAndroidGameView gameView)
        {
            inputMethodManager = (InputMethodManager)Game.Activity.GetSystemService(Activity.InputMethodService);
            _gameView = gameView;

            editText = new EditText(Game.Activity);
            editText.SetMaxLines(1);
            editText.InputType = InputTypes.ClassText;
            editText.ImeOptions = (ImeAction)((int)(ImeAction.Done) | IME_FLAG_NO_EXTRACT_UI);
            editText.SetBackgroundColor(Android.Graphics.Color.Transparent);

            var layoutParams = new RelativeLayout.LayoutParams(LinearLayout.LayoutParams.MatchParent, LinearLayout.LayoutParams.WrapContent);
            layoutParams.TopMargin = -200; // Move editText view off from screen.
            Game.Activity.AddContentView(editText, layoutParams);

            _gameView.ViewTreeObserver.AddOnGlobalLayoutListener(Game.Activity);
            editText.TextChanged += EditText_TextChanged;
            editText.KeyPress += EditText_KeyPress;
        }

        private void EditText_KeyPress(object sender, View.KeyEventArgs e)
        {
            if (e.Event.Action == KeyEventActions.Down && e.KeyCode == Keycode.Del)
            {
                var key = Keys.Back;
                if (TextInput != null)
                    TextInput.Invoke(this, new TextInputEventArgs((char)key, key));
            }
        }

        private void EditText_TextChanged(object sender, TextChangedEventArgs e)
        {
            foreach (var c in e.Text)
                if (TextInput != null)
                    TextInput.Invoke(this, new TextInputEventArgs(c, KeyboardUtil.ToXna(c)));

            editText.TextChanged -= EditText_TextChanged;
            editText.Text = string.Empty;
            editText.TextChanged += EditText_TextChanged;
        }

        public void StartTextInput()
        {
            if (IsTextInputActive)
                return;

            editText.RequestFocus();
            inputMethodManager.ShowSoftInput(editText, ShowFlags.Implicit);
            IsTextInputActive = true;
        }

        public void StopTextInput()
        {
            if (!IsTextInputActive)
                return;

            inputMethodManager.HideSoftInputFromWindow(_gameView.WindowToken, HideSoftInputFlags.NotAlways);
            IsTextInputActive = false;
            _gameView.RequestFocus();
        }

        const int KeyboardHideOffset = 80;

        internal void Update()
        {
            if (!IsTextInputActive) return;

            TouchCollection touchCollection = TouchPanel.GetState();
            foreach (TouchLocation touchLocation in touchCollection)
            {
                if (TouchLocationState.Pressed == touchLocation.State)
                {
                    if (touchLocation.Position.Y < ((ScreenSize.Y - VirtualKeyboardHeight) - KeyboardHideOffset))
                        StopTextInput();
                }
            }
        }

        public void SetTextInputRect(Rectangle rect)
        {
        }
    }
}
