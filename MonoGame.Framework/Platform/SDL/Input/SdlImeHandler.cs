using System;

namespace Microsoft.Xna.Framework.Input
{
    /// <summary>
    /// Integrate IME to DesktopGL(SDL2) platform.
    /// </summary>
    internal class SdlImeHandler : IImeService
    {
        private Game _game;

        public SdlImeHandler(Game game)
        {
            _game = game;

            StopTextInput(); // Force to stop text input by default
        }

        public bool IsTextInputActive { get; private set; }
        public event EventHandler<TextCompositionEventArgs> TextComposition;
        public event EventHandler<TextInputEventArgs> TextInput;

        public bool ShowOSImeWindow { get; set; }
        public int VirtualKeyboardHeight { get { return 0; } }

        public void OnTextInput(char charInput, Keys key)
        {
            if (TextInput != null)
                TextInput.Invoke(this, new TextInputEventArgs(charInput, key));
        }

        public void OnTextComposition(string compositionString, int cursorPosition)
        {
            if (TextComposition != null)
                TextComposition.Invoke(this, new TextCompositionEventArgs(compositionString, cursorPosition));
        }

        public void Update(GameTime gameTime)
        {

        }

        public void StartTextInput()
        {
            if (IsTextInputActive)
                return;

            Sdl.StartTextInput();
            IsTextInputActive = true;
        }

        public void StopTextInput()
        {
            if (!IsTextInputActive)
                return;

            Sdl.StopTextInput();
            IsTextInputActive = false;
        }

        public void SetTextInputRect(Rectangle rect)
        {
            var sdlRect = new Sdl.Rectangle() { X = rect.X, Y = rect.Y, Width = rect.Width, Height = rect.Height };
            Sdl.SetTextInputRect(ref sdlRect);
        }
    }
}
