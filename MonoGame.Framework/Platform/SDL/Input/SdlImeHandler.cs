using System;

namespace Microsoft.Xna.Framework.Input
{
    /// <summary>
    /// Integrate IME to DesktopGL(SDL2) platform.
    /// </summary>
    internal class SdlImeHandler : ImmService
    {
        private Game _game;

        public SdlImeHandler(Game game)
        {
            _game = game;

            // Text input is enabled by default in SDL2 internally, we have to set this flag manually.
            IsTextInputActive = true;
            VirtualKeyboardHeight = 0;
        }

        public override event EventHandler<TextCompositionEventArgs> TextComposition;
        public override event EventHandler<TextInputEventArgs> TextInput;

        public void OnTextInput(char charInput, Keys key)
        {
            if (TextInput != null)
                TextInput.Invoke(this, new TextInputEventArgs(charInput, key));
        }

        public void OnTextComposition(IMEString compositionText, int cursorPosition)
        {
            if (TextComposition != null)
                TextComposition.Invoke(this, new TextCompositionEventArgs(compositionText, cursorPosition));
        }

        public override void StartTextInput()
        {
            if (IsTextInputActive)
                return;

            Sdl.StartTextInput();
            IsTextInputActive = true;
        }

        public override void StopTextInput()
        {
            if (!IsTextInputActive)
                return;

            Sdl.StopTextInput();
            IsTextInputActive = false;
        }

        public override void SetTextInputRect(Rectangle rect)
        {
            var sdlRect = new Sdl.Rectangle() { X = rect.X, Y = rect.Y, Width = rect.Width, Height = rect.Height };
            Sdl.SetTextInputRect(ref sdlRect);
        }
    }
}
