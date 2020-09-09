using System;
using System.Collections.Generic;
using Windows.UI.Core;
using Windows.System;
using Windows.Foundation;
using Windows.Graphics.Display;
using Windows.UI.Text.Core;
using Windows.UI.ViewManagement;

namespace Microsoft.Xna.Framework.Input
{
    /// <summary>
    /// Integrate IME to WindowsDX platform with ImeSharp library.
    /// </summary>
    internal sealed class UapImeHandler : ImmService
    {
        private CoreWindow _coreWindow;
        private CoreTextEditContext _editContext;
        private CoreTextRange _selection;

        // The input pane object indicates the visibility of the on screen keyboard.
        // Apps can also ask the keyboard to show or hide.
        private InputPane _inputPane;

        private bool _compositionStarted;
        private DispatcherQueueTimer _timer;
        private List<char> _inputBuffer = new List<char>();
        private List<char> _lastResultText = new List<char>();

        private Rect _textInputRect;
        private double _scaleFactor;

        public override event EventHandler<TextCompositionEventArgs> TextComposition;
        public override event EventHandler<TextInputEventArgs> TextInput;

        private int _virtualKeyboardHeight;
        public override int VirtualKeyboardHeight { get { return _virtualKeyboardHeight; } }

        /// <summary>
        /// Constructor, must be called when the window create.
        /// </summary>
        internal UapImeHandler(CoreWindow window)
        {
            _coreWindow = window;
            _scaleFactor = DisplayInformation.GetForCurrentView().RawPixelsPerViewPixel;

            CoreTextServicesManager manager = CoreTextServicesManager.GetForCurrentView();
            _editContext = manager.CreateEditContext();

            // Get the Input Pane(screen keyboard) so we can programmatically hide and show it.
            _inputPane = InputPane.GetForCurrentView();
            _inputPane.Showing += (o, e) => _virtualKeyboardHeight = (int)e.OccludedRect.Height;
            _inputPane.Hiding += (o, e) => _virtualKeyboardHeight = 0;

            _editContext.InputPaneDisplayPolicy = CoreTextInputPaneDisplayPolicy.Manual;
            _editContext.InputScope = CoreTextInputScope.Text;

            // The system raises this event to request a specific range of text.
            _editContext.TextRequested += EditContext_TextRequested;

            // The system raises this event to request the current selection.
            _editContext.SelectionRequested += EditContext_SelectionRequested;

            // The system raises this event when it wants the edit control to remove focus.
            _editContext.FocusRemoved += EditContext_FocusRemoved;

            // The system raises this event to update text in the edit control.
            _editContext.TextUpdating += EditContext_TextUpdating;

            // The system raises this event to change the selection in the edit control.
            _editContext.SelectionUpdating += EditContext_SelectionUpdating;

            // The system raises this event to request layout information.
            // This is used to help choose a position for the IME candidate window.
            _editContext.LayoutRequested += EditContext_LayoutRequested;

            // The system raises this event to notify the edit control
            // that the string composition has started.
            _editContext.CompositionStarted += EditContext_CompositionStarted;

            // The system raises this event to notify the edit control
            // that the string composition is finished.
            _editContext.CompositionCompleted += EditContext_CompositionCompleted;

            _timer = _coreWindow.DispatcherQueue.CreateTimer();
            _timer.Interval = new TimeSpan(0, 0, 0, 0, 5);
            _timer.IsRepeating = false;
            _timer.Tick += (o, e) =>
            {
                foreach (var c in _lastResultText)
                    OnTextInput(c);
            };

            _coreWindow.Activated += CoreWindow_Activated;
        }

        public override void StartTextInput()
        {
            if (!IsTextInputActive)
            {
                IsTextInputActive = true;

                // Notify the CoreTextEditContext that the edit context has focus,
                // so it should start processing text input.
                _editContext.NotifyFocusEnter();

                // Ask the software keyboard to show.  The system will ultimately decide if it will show.
                // For example, it will not show if there is a keyboard attached.
                _inputPane.TryShow();
            }
        }

        public override void StopTextInput()
        {
            if (IsTextInputActive)
            {
                //Notify the system that this edit context is no longer in focus
                _editContext.NotifyFocusLeave();

                RemoveInternalFocusWorker();
            }
        }

        public override void SetTextInputRect(Rectangle rect)
        {
            if (!IsTextInputActive)
                return;

            Rect windowBounds = _coreWindow.Bounds;

            _textInputRect.X = (rect.X + windowBounds.X) * _scaleFactor;
            _textInputRect.Y = (rect.Y + windowBounds.Y) * _scaleFactor;
            _textInputRect.Width = rect.Width * _scaleFactor;
            _textInputRect.Height = rect.Height * _scaleFactor;
        }

        private void OnTextInput(char charInput)
        {
            if (!IsTextInputActive) return;

            if (TextInput != null)
                TextInput.Invoke(this, new TextInputEventArgs(charInput, Keys.None));
        }

        internal void OnTextInput(char charInput, Keys key)
        {
            if (!IsTextInputActive) return;

            if (TextInput != null)
                TextInput.Invoke(this, new TextInputEventArgs(charInput, key));
        }

        private void OnTextComposition(List<char> compositionText, int cursorPosition)
        {
            if (TextComposition == null)
                return;

            TextComposition.Invoke(this, new TextCompositionEventArgs(new IMEString(compositionText), cursorPosition));
        }

        private void RemoveInternalFocusWorker()
        {
            IsTextInputActive = false;

            // Ask the software keyboard to dismiss.
            _inputPane.TryHide();
        }

        private void CoreWindow_Activated(CoreWindow sender, WindowActivatedEventArgs args)
        {
            if (IsTextInputActive)
            {
                _editContext.NotifyFocusEnter();
                _inputPane.TryShow();
            }
        }

        private void EditContext_FocusRemoved(CoreTextEditContext sender, object args)
        {
            RemoveInternalFocusWorker();
        }

        // Return the specified range of text. Note that the system may ask for more text
        // than exists in the text buffer.
        private void EditContext_TextRequested(CoreTextEditContext sender, CoreTextTextRequestedEventArgs args)
        {
            CoreTextTextRequest request = args.Request;

            var list = _inputBuffer.GetRange(request.Range.StartCaretPosition,
                Math.Min(request.Range.EndCaretPosition, _inputBuffer.Count) - request.Range.StartCaretPosition);
            request.Text = new string(list.ToArray());
        }

        // Return the current selection.
        private void EditContext_SelectionRequested(CoreTextEditContext sender, CoreTextSelectionRequestedEventArgs args)
        {
            CoreTextSelectionRequest request = args.Request;
            request.Selection = _selection;
        }

        private void EditContext_TextUpdating(CoreTextEditContext sender, CoreTextTextUpdatingEventArgs args)
        {
            if (!_compositionStarted) // Only update text  when composition is started.
            {
                args.Result = CoreTextTextUpdatingResult.Failed;
                return;
            }

            CoreTextRange range = args.Range;
            CoreTextRange newSelection = args.NewSelection;

            _inputBuffer.RemoveRange(range.StartCaretPosition, Math.Min(_inputBuffer.Count, range.EndCaretPosition) - range.StartCaretPosition);
            _inputBuffer.InsertRange(range.StartCaretPosition, args.Text);

            // Modify the current selection.
            newSelection.EndCaretPosition = newSelection.StartCaretPosition;

            // Update the selection of the edit context. There is no need to notify the system
            // because the system itself changed the selection.
            _selection = newSelection;

            OnTextComposition(_inputBuffer, _selection.StartCaretPosition);
        }

        private void EditContext_SelectionUpdating(CoreTextEditContext sender, CoreTextSelectionUpdatingEventArgs args)
        {
            // Update the selection of the edit context. There is no need to notify the system
            // because the system itself changed the selection.
            _selection = args.Selection;

            OnTextComposition(_inputBuffer, _selection.StartCaretPosition);
        }

        private void EditContext_LayoutRequested(CoreTextEditContext sender, CoreTextLayoutRequestedEventArgs args)
        {
            CoreTextLayoutRequest request = args.Request;

            request.LayoutBounds.TextBounds = _textInputRect;
        }

        // This indicates that an IME has started composition.  If there is no handler for this event,
        // then composition will not start.
        private void EditContext_CompositionStarted(CoreTextEditContext sender, CoreTextCompositionStartedEventArgs args)
        {
            _compositionStarted = true;
        }

        private void ResetTextStore()
        {
            CoreTextRange modifiedRange = new CoreTextRange { StartCaretPosition = 0, EndCaretPosition = _inputBuffer.Count };

            // Clear the internal text store.
            _inputBuffer.Clear();

            _selection.StartCaretPosition = modifiedRange.StartCaretPosition;
            _selection.EndCaretPosition = _selection.StartCaretPosition;

            // Let the CoreTextEditContext know what changed.
            _editContext.NotifyTextChanged(modifiedRange, 0, _selection);
        }

        private void EditContext_CompositionCompleted(CoreTextEditContext sender, CoreTextCompositionCompletedEventArgs args)
        {
            _lastResultText.Clear();
            foreach (var c in _inputBuffer)
                _lastResultText.Add(c);

            _coreWindow.DispatcherQueue.TryEnqueue(() => { ResetTextStore(); });

            if (!_timer.IsRunning)
                _timer.Start();

            OnTextComposition(null, 0);

            _compositionStarted = false;
        }
    }
}
