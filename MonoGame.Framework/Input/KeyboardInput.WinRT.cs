using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;

namespace Microsoft.Xna.Framework.Input
{
    public static partial class KeyboardInput
    {
        private static readonly CoreDispatcher dispatcher;
        private static TaskCompletionSource<string> tcs;
        private static InputDialog inputDialog;

        static KeyboardInput()
        {
            dispatcher = CoreApplication.MainView.CoreWindow.Dispatcher;
        }

        private static Task<string> PlatformShow(string title, string description, string defaultText, bool usePasswordMode)
        {
            tcs = new TaskCompletionSource<string>();
            dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                async () =>
                {
                    inputDialog = new InputDialog();
                    var result = await inputDialog.ShowAsync(title, description, defaultText, usePasswordMode);

                    if (!tcs.Task.IsCompleted)
                        tcs.SetResult(result);
                });

            return tcs.Task;
        }

        private static void PlatformCancel(string result)
        {
            dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () => await inputDialog.CloseAsync());

            tcs.SetResult(result);
        }
    }

    #region Keyboard Input UI
    [TemplatePart(Name = LayoutRootPanelName, Type = typeof(Panel))]
    [TemplatePart(Name = ContentBorderName, Type = typeof(Border))]
    [TemplatePart(Name = InputTextBoxName, Type = typeof(TextBox))]
    [TemplatePart(Name = TitleTextBlockName, Type = typeof(TextBlock))]
    [TemplatePart(Name = TextTextBlockName, Type = typeof(TextBlock))]
    [TemplatePart(Name = ButtonsPanelName, Type = typeof(Panel))]
    [TemplateVisualState(GroupName = PopupStatesGroupName, Name = OpenPopupStateName)]
    [TemplateVisualState(GroupName = PopupStatesGroupName, Name = ClosedPopupStateName)]
    [StyleTypedProperty(Property = "ButtonStyle", StyleTargetType = typeof(ButtonBase))]
    [StyleTypedProperty(Property = "TitleStyle", StyleTargetType = typeof(TextBlock))]
    [StyleTypedProperty(Property = "TextStyle", StyleTargetType = typeof(TextBlock))]
    [StyleTypedProperty(Property = "InputTextStyle", StyleTargetType = typeof(TextBox))]
    [StyleTypedProperty(Property = "InputPasswordStyle", StyleTargetType = typeof(PasswordBox))]
    sealed class InputDialog : Control
    {
        private const string PopupStatesGroupName = "PopupStates";
        private const string OpenPopupStateName = "OpenPopupState";
        private const string ClosedPopupStateName = "ClosedPopupState";

        private const string LayoutRootPanelName = "LayoutRoot";
        private const string ContentBorderName = "ContentBorder";
        private const string InputTextBoxName = "InputTextBox";
        private const string InputPasswordBoxName = "InputPasswordBox";
        private const string TitleTextBlockName = "TitleTextBlock";
        private const string TextTextBlockName = "TextTextBlock";
        private const string ButtonsPanelName = "ButtonsPanel";

        private const string VirtualKeyboardSlideStoryBoardName = "VirtualKeyboardSlideStoryBoard";
        private const string VirtualKeyboardAnimationName = "VirtualKeyboardAnimation";
        private const string BlackStripeTransformName = "BlackStripeTransform";

        private Panel _layoutRoot;
        private Border _contentBorder;
        private TextBox _inputTextBox;
        private PasswordBox _inputPasswordBox;
        private TextBlock _titleTextBlock;
        private TextBlock _textTextBlock;
        private Panel _buttonsPanel;

        private bool _usePasswordMode;
        private bool _shown;
        private TaskCompletionSource<string> _dismissTaskSource;
        private List<ButtonBase> _buttons;

        private Popup _dialogPopup;
        private Panel _parentPanel;
        private Panel _temporaryParentPanel;
        private ContentControl _parentContentControl;

        private TranslateTransform _blackStripeTransform;

        private Storyboard _virtualKeyboardSlideStoryboard;

        private DoubleAnimation _virtualKeyboardSlideAnimation;

        public static readonly DependencyProperty ButtonStyleProperty =
            DependencyProperty.Register(
                "ButtonStyle",
                typeof(Style),
                typeof(InputDialog),
                new PropertyMetadata(null));

        public Style ButtonStyle
        {
            get { return (Style)GetValue(ButtonStyleProperty); }
            set { SetValue(ButtonStyleProperty, value); }
        }

        public static readonly DependencyProperty InputTextProperty =
            DependencyProperty.Register(
                "InputText",
                typeof(string),
                typeof(InputDialog),
                new PropertyMetadata("", OnInputTextChanged));

        private string InputText
        {
            get { return (string)GetValue(InputTextProperty); }
            set { SetValue(InputTextProperty, value); }
        }

        private static void OnInputTextChanged(
            DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var target = (InputDialog)d;
            var newInputText = target.InputText;
            target.OnInputTextChanged(newInputText);
        }

        private void OnInputTextChanged(string newInputText)
        {
            if (_inputTextBox != null)
            {
                _inputTextBox.Text = newInputText;
            }
            if (_inputPasswordBox != null)
            {
                _inputPasswordBox.Password = newInputText;
            }
        }

        public static readonly DependencyProperty BackgroundScreenBrushProperty =
            DependencyProperty.Register(
                "BackgroundScreenBrush",
                typeof(Brush),
                typeof(InputDialog),
                new PropertyMetadata(null));

        public Brush BackgroundScreenBrush
        {
            get { return (Brush)GetValue(BackgroundScreenBrushProperty); }
            set { SetValue(BackgroundScreenBrushProperty, value); }
        }

        public static readonly DependencyProperty BackgroundStripeBrushProperty =
            DependencyProperty.Register(
                "BackgroundStripeBrush",
                typeof(Brush),
                typeof(InputDialog),
                new PropertyMetadata(null));

        public Brush BackgroundStripeBrush
        {
            get { return (Brush)GetValue(BackgroundStripeBrushProperty); }
            set { SetValue(BackgroundStripeBrushProperty, value); }
        }

        public static readonly DependencyProperty TitleStyleProperty =
            DependencyProperty.Register(
                "TitleStyle",
                typeof(Style),
                typeof(InputDialog),
                new PropertyMetadata(null));

        public Style TitleStyle
        {
            get { return (Style)GetValue(TitleStyleProperty); }
            set { SetValue(TitleStyleProperty, value); }
        }

        public static readonly DependencyProperty TextStyleProperty =
            DependencyProperty.Register(
                "TextStyle",
                typeof(Style),
                typeof(InputDialog),
                new PropertyMetadata(null, OnTextStyleChanged));

        public Style TextStyle
        {
            get { return (Style)GetValue(TextStyleProperty); }
            set { SetValue(TextStyleProperty, value); }
        }

        private static void OnTextStyleChanged(
            DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var target = (InputDialog)d;
            var oldTextStyle = (Style)e.OldValue;
            var newTextStyle = target.TextStyle;
            target.OnTextStyleChanged(oldTextStyle, newTextStyle);
        }

        private void OnTextStyleChanged(
            Style oldTextStyle, Style newTextStyle)
        {
        }

        public static readonly DependencyProperty InputTextStyleProperty =
            DependencyProperty.Register(
                "InputTextStyle",
                typeof(Style),
                typeof(InputDialog),
                new PropertyMetadata(null));

        public Style InputTextStyle
        {
            get { return (Style)GetValue(InputTextStyleProperty); }
            set { SetValue(InputTextStyleProperty, value); }
        }

        public static readonly DependencyProperty InputPasswordStyleProperty =
            DependencyProperty.Register(
                "InputPasswordStyle",
                typeof(Style),
                typeof(InputDialog),
                new PropertyMetadata(null));

        public Style InputPasswordStyle
        {
            get { return (Style)GetValue(InputPasswordStyleProperty); }
            set { SetValue(InputPasswordStyleProperty, value); }
        }

        public static readonly DependencyProperty IsLightDismissEnabledProperty =
            DependencyProperty.Register(
                "IsLightDismissEnabled",
                typeof(bool),
                typeof(InputDialog),
                new PropertyMetadata(false));

        public bool IsLightDismissEnabled
        {
            get { return (bool)GetValue(IsLightDismissEnabledProperty); }
            set { SetValue(IsLightDismissEnabledProperty, value); }
        }

        public static readonly DependencyProperty AwaitsCloseTransitionProperty =
            DependencyProperty.Register(
                "AwaitsCloseTransition",
                typeof(bool),
                typeof(InputDialog),
                new PropertyMetadata(true));

        public bool AwaitsCloseTransition
        {
            get { return (bool)GetValue(AwaitsCloseTransitionProperty); }
            set { SetValue(AwaitsCloseTransitionProperty, value); }
        }

        public static readonly DependencyProperty ButtonsPanelOrientationProperty =
            DependencyProperty.Register(
                "ButtonsPanelOrientation",
                typeof(Orientation),
                typeof(InputDialog),
                new PropertyMetadata(Orientation.Horizontal));

        public Orientation ButtonsPanelOrientation
        {
            get { return (Orientation)GetValue(ButtonsPanelOrientationProperty); }
            set { SetValue(ButtonsPanelOrientationProperty, value); }
        }

        public InputDialog()
        {
            DefaultStyleKey = typeof(InputDialog);

            Visibility = Visibility.Collapsed;
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _layoutRoot = GetTemplateChild(LayoutRootPanelName) as Panel;
            _contentBorder = GetTemplateChild(ContentBorderName) as Border;

            _inputTextBox = GetTemplateChild(InputTextBoxName) as TextBox;
            _inputPasswordBox = GetTemplateChild(InputPasswordBoxName) as PasswordBox;
            _titleTextBlock = GetTemplateChild(TitleTextBlockName) as TextBlock;
            _textTextBlock = GetTemplateChild(TextTextBlockName) as TextBlock;
            _buttonsPanel = GetTemplateChild(ButtonsPanelName) as Panel;

            _virtualKeyboardSlideStoryboard = GetTemplateChild(VirtualKeyboardSlideStoryBoardName) as Storyboard;
            _virtualKeyboardSlideAnimation = GetTemplateChild(VirtualKeyboardAnimationName) as DoubleAnimation;
            _blackStripeTransform = this.GetTemplateChild(BlackStripeTransformName) as TranslateTransform;

            if (_layoutRoot != null)
                _layoutRoot.Tapped += OnLayoutRootTapped;
            if (_inputTextBox != null && _inputPasswordBox != null)
            {
                _inputTextBox.Text = InputText;
                _inputTextBox.IsTextPredictionEnabled = false;
                _inputTextBox.TextChanged += OnInputTextBoxTextChanged;
                _inputTextBox.KeyUp += OnInputTextBoxKeyUp;

                _inputPasswordBox.Password = InputText;
                _inputPasswordBox.PasswordChanged += OnInputPasswordBoxTextChanged;
                _inputPasswordBox.KeyUp += OnInputTextBoxKeyUp;

                if (_usePasswordMode)
                {
                    _inputTextBox.Visibility = Visibility.Collapsed;
                    _inputTextBox = null;
                    _inputPasswordBox.Visibility = Visibility.Visible;
                }
                else
                {
                    _inputTextBox.Visibility = Visibility.Visible;
                    _inputPasswordBox.Visibility = Visibility.Collapsed;
                    _inputPasswordBox = null;
                }
            }
            if (_contentBorder != null)
                _contentBorder.Tapped += OnContentBorderTapped;
        }

        private void OnGlobalKeyUp(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Escape)
            {
                DismissDialog();
                e.Handled = true;
            }
        }

        private void OnLayoutRootTapped(object sender, TappedRoutedEventArgs e)
        {
            _buttons[0].Focus(FocusState.Programmatic); // Hide input panel

            if (e.OriginalSource == sender &&
                IsLightDismissEnabled)
            {
                _dismissTaskSource.TrySetResult(null);
                e.Handled = true;
            }
        }

        private void OnContentBorderTapped(object sender, TappedRoutedEventArgs e)
        {
            if (e.OriginalSource == sender)
            {
                _buttons[0].Focus(FocusState.Programmatic);
                e.Handled = true;
            }
        }

        private void OnInputTextBoxTextChanged(object sender, TextChangedEventArgs e)
        {
            InputText = _inputTextBox.Text;
        }

        private void OnInputPasswordBoxTextChanged(object sender, RoutedEventArgs e)
        {
            InputText = _inputPasswordBox.Password;
        }

        public async Task<string> ShowAsync(string title, string text, string defaultText, bool usePasswordMode)
        {
            _usePasswordMode = usePasswordMode;
            if (_shown)
            {
                throw new InvalidOperationException("The dialog is already shown.");
            }

            Visibility = Visibility.Visible;
            _shown = true;

            Window.Current.Content.KeyUp += OnGlobalKeyUp;
            _dismissTaskSource = new TaskCompletionSource<string>();

            _parentPanel = Parent as Panel;
            _parentContentControl = Parent as ContentControl;

            if (_parentPanel != null)
            {
                _parentPanel.Children.Remove(this);
            }

            if (_parentContentControl != null)
            {
                _parentContentControl.Content = null;
            }

            _dialogPopup = new Popup { Child = this };

            if (_parentPanel != null)
            {
                _parentPanel.Children.Add(_dialogPopup);
                _parentPanel.SizeChanged += OnParentSizeChanged;
            }
            else if (_parentContentControl != null)
            {
                _parentContentControl.Content = _dialogPopup;
                _parentContentControl.SizeChanged += OnParentSizeChanged;
            }
            else
            {
                _temporaryParentPanel = GetDescendants(Window.Current.Content).OfType<Panel>().FirstOrDefault();

                if (_temporaryParentPanel != null)
                {
                    _temporaryParentPanel.Children.Add(_dialogPopup);
                    _temporaryParentPanel.SizeChanged += OnParentSizeChanged;
                }
            }

            _dialogPopup.IsOpen = true;

            await WaitForLayoutUpdateAsync(this);

            _titleTextBlock.Text = title;
            _textTextBlock.Text = text;
            InputText = defaultText;

            _buttons = new List<ButtonBase>();

            // Button OK
            var btnOK = new Button();
            if (ButtonStyle != null)
                btnOK.Style = ButtonStyle;
            btnOK.Content = "OK";
            btnOK.Tag = 0;
            btnOK.Click += OnButtonClick;
            btnOK.KeyUp += OnGlobalKeyUp;
            _buttons.Add(btnOK);
            _buttonsPanel.Children.Add(btnOK);

            // Button Cancel
            var btnCancel = new Button();
            if (ButtonStyle != null)
                btnCancel.Style = ButtonStyle;
            btnCancel.Content = "Cancel";
            btnCancel.Tag = 1;
            btnCancel.Click += OnButtonClick;
            btnCancel.KeyUp += OnGlobalKeyUp;
            _buttons.Add(btnCancel);
            _buttonsPanel.Children.Add(btnCancel);

            InputPane.GetForCurrentView().Showing += InputDialog_Showing;
            InputPane.GetForCurrentView().Hiding += InputDialod_Hiding;

            if (_inputTextBox != null)
                _inputTextBox.Focus(FocusState.Programmatic);
            if (_inputPasswordBox != null)
                _inputPasswordBox.Focus(FocusState.Programmatic);

            ResizeLayoutRoot();

            // Show dialog
            await GoToVisualStateAsync(this, _layoutRoot, PopupStatesGroupName, OpenPopupStateName);

            // Wait for button click
            var result = await _dismissTaskSource.Task;

            // Hide dialog
            if (AwaitsCloseTransition)
            {
                await CloseAsync();
            }
            else
            {
#pragma warning disable 4014
                CloseAsync();
#pragma warning restore 4014
            }

            InputPane.GetForCurrentView().Showing -= InputDialog_Showing;
            InputPane.GetForCurrentView().Hiding -= InputDialod_Hiding;

            Window.Current.Content.KeyUp -= OnGlobalKeyUp;

            return result;
        }

        private void InputDialod_Hiding(InputPane sender, InputPaneVisibilityEventArgs args)
        {
            TranslateStripeTo(0);
        }

        void InputDialog_Showing(Windows.UI.ViewManagement.InputPane sender, Windows.UI.ViewManagement.InputPaneVisibilityEventArgs args)
        {
            TranslateStripeTo(args.OccludedRect.Height / -2);
        }

        private void TranslateStripeTo(double value)
        {
            if (_virtualKeyboardSlideAnimation == null || _virtualKeyboardSlideStoryboard == null)
                return;

            _blackStripeTransform.Y = value;
            //_virtualKeyboardSlideAnimation.To = value;
            //_virtualKeyboardSlideStoryboard.Begin();
        }

        static async Task WaitForLayoutUpdateAsync(FrameworkElement frameworkElement)
        {
            var tcs = new TaskCompletionSource<bool>();
            EventHandler<object> eventHandler = (sender, args) => tcs.SetResult(true);
            frameworkElement.LayoutUpdated += eventHandler;
            await tcs.Task;
            frameworkElement.LayoutUpdated -= eventHandler;
        }

        static IEnumerable<DependencyObject> GetDescendants(DependencyObject start)
        {
            var queue = new Queue<DependencyObject>();

            var popup = start as Popup;

            if (popup != null)
            {
                if (popup.Child != null)
                {
                    queue.Enqueue(popup.Child);
                    yield return popup.Child;
                }
            }
            else
            {
                var count = VisualTreeHelper.GetChildrenCount(start);

                for (var i = 0; i < count; i++)
                {
                    var child = VisualTreeHelper.GetChild(start, i);
                    queue.Enqueue(child);
                    yield return child;
                }
            }

            while (queue.Count > 0)
            {
                var parent = queue.Dequeue();

                popup = parent as Popup;

                if (popup != null)
                {
                    if (popup.Child != null)
                    {
                        queue.Enqueue(popup.Child);
                        yield return popup.Child;
                    }
                }
                else
                {
                    var count = VisualTreeHelper.GetChildrenCount(parent);

                    for (var i = 0; i < count; i++)
                    {
                        var child = VisualTreeHelper.GetChild(parent, i);
                        yield return child;
                        queue.Enqueue(child);
                    }
                }
            }
        }

        private void ResizeLayoutRoot()
        {
            var root =
                _parentPanel ??
                _parentContentControl ??
                _temporaryParentPanel as FrameworkElement;
            _layoutRoot.Width = root.ActualWidth;
            _layoutRoot.Height = root.ActualHeight;
        }

        private void OnParentSizeChanged(object sender, SizeChangedEventArgs sizeChangedEventArgs)
        {
            ResizeLayoutRoot();
        }

        public async Task CloseAsync()
        {
            if (!_shown)
            {
                throw new InvalidOperationException("The dialog isn't shown, so it can't be closed.");
            }

            await GoToVisualStateAsync(this, _layoutRoot, PopupStatesGroupName, ClosedPopupStateName);
            _dialogPopup.IsOpen = false;
            _buttonsPanel.Children.Clear();

            foreach (var button in _buttons)
            {
                button.Click -= OnButtonClick;
                button.KeyUp -= OnGlobalKeyUp;
            }

            _buttons.Clear();

            _dialogPopup.Child = null;

            if (_parentPanel != null)
            {
                _parentPanel.Children.Remove(_dialogPopup);
                _parentPanel.Children.Add(this);
                _parentPanel.SizeChanged -= OnParentSizeChanged;
                _parentPanel = null;
            }

            if (_parentContentControl != null)
            {
                _parentContentControl.Content = this;
                _parentContentControl.SizeChanged -= OnParentSizeChanged;
                _parentContentControl = null;
            }

            if (_temporaryParentPanel != null)
            {
                _temporaryParentPanel.Children.Remove(_dialogPopup);
                _temporaryParentPanel.SizeChanged -= OnParentSizeChanged;
                _temporaryParentPanel = null;
            }

            _dialogPopup = null;
            Visibility = Visibility.Collapsed;
            _shown = false;
        }

        private static async Task GoToVisualStateAsync(Control control,
            FrameworkElement visualStatesHost,
            string stateGroupName,
            string stateName)
        {
            var tcs = new TaskCompletionSource<Storyboard>();

            var storyboard =
                GetStoryboardForVisualState(visualStatesHost, stateGroupName, stateName);

            if (storyboard != null)
            {
                EventHandler<object> eh = null;

                eh = (s, e) =>
                {
                    storyboard.Completed -= eh;
                    tcs.SetResult(storyboard);
                };

                storyboard.Completed += eh;
            }

            VisualStateManager.GoToState(control, stateName, true);

            if (storyboard == null)
            {
                return;
            }

            await tcs.Task;
        }

        private static Storyboard GetStoryboardForVisualState(
            FrameworkElement visualStatesHost,
            string stateGroupName,
            string stateName)
        {
            Storyboard storyboard = null;

            var stateGroups = VisualStateManager.GetVisualStateGroups(visualStatesHost);
            VisualStateGroup stateGroup = null;

            if (!string.IsNullOrEmpty(stateGroupName))
            {
                stateGroup = stateGroups.FirstOrDefault(g => g.Name == stateGroupName);
            }

            VisualState state = null;

            if (stateGroup != null)
            {
                state = stateGroup.States.FirstOrDefault(s => s.Name == stateName);
            }

            if (state == null)
            {
                foreach (var group in stateGroups)
                {
                    state = group.States.FirstOrDefault(s => s.Name == stateName);

                    if (state != null)
                    {
                        break;
                    }
                }
            }

            if (state != null)
            {
                storyboard = state.Storyboard;
            }

            return storyboard;
        }

        private void OnButtonClick(object sender, RoutedEventArgs e)
        {
            var clickedButton = (ButtonBase)sender;
            _dismissTaskSource.TrySetResult((int)clickedButton.Tag == 0 ? (_inputTextBox != null ? _inputTextBox.Text : _inputPasswordBox.Password) : null);

            if (_buttons.Count > 0)
            {
                var button = (Button)_buttons[0];
                button.Focus(FocusState.Programmatic);
            }
        }

        private void OnInputTextBoxKeyUp(object sender, KeyRoutedEventArgs e)
        {
            if (_inputTextBox != null)
                InputText = _inputTextBox.Text;
            if (_inputPasswordBox != null)
                InputText = _inputPasswordBox.Password;

            if (e.Key == VirtualKey.Enter)
            {
                OnButtonClick(_buttons[0], new RoutedEventArgs());
                e.Handled = true;
                return;
            }

            if (e.Key == VirtualKey.Escape)
            {
                OnButtonClick(_buttons[1], new RoutedEventArgs());
                e.Handled = true;
            }
        }

        private void DismissDialog()
        {
            _dismissTaskSource.TrySetResult(null);

            if (_buttons.Count > 0)
            {
                var button = (Button)_buttons[0];
                button.Focus(FocusState.Programmatic);
            }
        }
    }
    #endregion
}
