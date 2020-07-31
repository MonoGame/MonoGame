// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using System;
using System.ComponentModel;

namespace Microsoft.Xna.Framework
{
    /// <summary>
    /// The system window used by a <see cref="Game"/>.
    /// </summary>
	public abstract class GameWindow
	{
		#region Properties

	    /// <summary>
	    /// Indicates if users can resize this <see cref="GameWindow"/>.
	    /// </summary>
		[DefaultValue(false)]
		public abstract bool AllowUserResizing { get; set; }

	    /// <summary>
	    /// The client rectangle of the <see cref="GameWindow"/>.
	    /// </summary>
		public abstract Rectangle ClientBounds { get; }

	    internal bool _allowAltF4 = true;

        /// <summary>
        /// Gets or sets a bool that enables usage of Alt+F4 for window closing on desktop platforms. Value is true by default.
        /// </summary>
        public virtual bool AllowAltF4 { get { return _allowAltF4; } set { _allowAltF4 = value; } }

#if (WINDOWS && !WINDOWS_UAP) || DESKTOPGL
        /// <summary>
        /// The location of this window on the desktop, eg: global coordinate space
        /// which stretches across all screens.
        /// </summary>
        public abstract Point Position { get; set; }
#endif

	    /// <summary>
	    /// The display orientation on a mobile device.
	    /// </summary>
		public abstract DisplayOrientation CurrentOrientation { get; }

	    /// <summary>
	    /// The handle to the window used by the backend windowing service.
		///
		/// For WindowsDX this is the Win32 window handle (HWND).
		/// For DesktopGL this is the SDL window handle.
		/// For UWP this is a handle to an IUnknown interface for the CoreWindow.
	    /// </summary>
		public abstract IntPtr Handle { get; }

	    /// <summary>
	    /// The name of the screen the window is currently on.
	    /// </summary>
		public abstract string ScreenDeviceName { get; }

		private string _title;
        /// <summary>
        /// Gets or sets the title of the game window.
        /// </summary>
        /// <remarks>
        /// For UWP this has no effect. The title should be
        /// set by using the DisplayName property found in the app manifest file.
        /// </remarks>
        public string Title {
			get { return _title; }
			set {
				if (_title != value) {
					SetTitle(value);
					_title = value;
				}
			}
		}

        /// <summary>
        /// Determines whether the border of the window is visible. Currently only supported on the WindowsDX and DesktopGL platforms.
        /// </summary>
        /// <exception cref="System.NotImplementedException">
        /// Thrown when trying to use this property on a platform other than WinowsDX or DesktopGL.
        /// </exception>
        public virtual bool IsBorderless
        {
            get
            {
                return false;
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        internal MouseState MouseState;
	    internal TouchPanelState TouchPanelState;

	    /// <summary>
	    /// Create a <see cref="GameWindow"/>.
	    /// </summary>
        protected GameWindow()
        {
            TouchPanelState = new TouchPanelState(this);
        }

		#endregion Properties

		#region Events

	    /// <summary>
	    /// Raised when the user resized the window or the window switches from fullscreen mode to
	    /// windowed mode or vice versa.
	    /// </summary>
		public event EventHandler<EventArgs> ClientSizeChanged;

	    /// <summary>
	    /// Raised when <see cref="CurrentOrientation"/> changed.
	    /// </summary>
		public event EventHandler<EventArgs> OrientationChanged;

	    /// <summary>
	    /// Raised when <see cref="ScreenDeviceName"/> changed.
	    /// </summary>
		public event EventHandler<EventArgs> ScreenDeviceNameChanged;

#if WINDOWS || WINDOWS_UAP || DESKTOPGL|| ANGLE

        /// <summary>
		/// Use this event to user text input.
		/// 
		/// This event is not raised by noncharacter keys except control characters such as backspace, tab, carriage return and escape.
		/// This event also supports key repeat.
		/// </summary>
		/// <remarks>
		/// This event is only supported on desktop platforms.
		/// </remarks>
		public event EventHandler<TextInputEventArgs> TextInput;

        internal bool IsTextInputHandled { get { return TextInput != null; } }

        /// <summary>
        /// Buffered keyboard KeyDown event.
        /// </summary>
		public event EventHandler<InputKeyEventArgs> KeyDown;

        /// <summary>
        /// Buffered keyboard KeyUp event.
        /// </summary>
        public event EventHandler<InputKeyEventArgs> KeyUp;

#endif

        #endregion Events

        /// <summary>
        /// Called before a game switches from windowed to full screen mode or vice versa.
        /// </summary>
        /// <param name="willBeFullScreen">Indicates what mode the game will switch to.</param>
        public abstract void BeginScreenDeviceChange (bool willBeFullScreen);

	    /// <summary>
	    /// Called when a transition from windowed to full screen or vice versa ends, or when
	    /// the <see cref="Graphics.GraphicsDevice"/> is reset.
	    /// </summary>
	    /// <param name="screenDeviceName">Name of the screen to move the window to.</param>
	    /// <param name="clientWidth">The new width of the client rectangle.</param>
	    /// <param name="clientHeight">The new height of the client rectangle.</param>
		public abstract void EndScreenDeviceChange (
			string screenDeviceName, int clientWidth, int clientHeight);

	    /// <summary>
	    /// Called when a transition from windowed to full screen or vice versa ends, or when
	    /// the <see cref="Graphics.GraphicsDevice"/> is reset.
	    /// </summary>
	    /// <param name="screenDeviceName">Name of the screen to move the window to.</param>
		public void EndScreenDeviceChange (string screenDeviceName)
		{
			EndScreenDeviceChange(screenDeviceName, ClientBounds.Width, ClientBounds.Height);
		}

	    /// <summary>
	    /// Called when the window gains focus.
	    /// </summary>
		protected void OnActivated ()
		{
		}

		internal void OnClientSizeChanged ()
		{
            EventHelpers.Raise(this, ClientSizeChanged, EventArgs.Empty);
		}

	    /// <summary>
	    /// Called when the window loses focus.
	    /// </summary>
		protected void OnDeactivated ()
		{
		}
         
	    /// <summary>
	    /// Called when <see cref="CurrentOrientation"/> changed. Raises the <see cref="OnOrientationChanged"/> event.
	    /// </summary>
		protected void OnOrientationChanged ()
		{
            EventHelpers.Raise(this, OrientationChanged, EventArgs.Empty);
		}

		protected void OnPaint ()
		{
		}

	    /// <summary>
	    /// Called when <see cref="ScreenDeviceName"/> changed. Raises the <see cref="ScreenDeviceNameChanged"/> event.
	    /// </summary>
		protected void OnScreenDeviceNameChanged ()
		{
            EventHelpers.Raise(this, ScreenDeviceNameChanged, EventArgs.Empty);
		}

#if WINDOWS || WINDOWS_UAP || DESKTOPGL || ANGLE
	    /// <summary>
	    /// Called when the window receives text input. Raises the <see cref="TextInput"/> event.
	    /// </summary>
	    /// <param name="sender">The game window.</param>
	    /// <param name="e">Parameters to the <see cref="TextInput"/> event.</param>
		internal void OnTextInput(TextInputEventArgs e)
		{
            EventHelpers.Raise(this, TextInput, e);
		}
        internal void OnKeyDown(InputKeyEventArgs e)
	    {
            EventHelpers.Raise(this, KeyDown, e);
	    }
        internal void OnKeyUp(InputKeyEventArgs e)
	    {
            EventHelpers.Raise(this, KeyUp, e);
	    }
#endif

		protected internal abstract void SetSupportedOrientations (DisplayOrientation orientations);

	    /// <summary>
	    /// Set the title of this window to the given string.
	    /// </summary>
	    /// <param name="title">The new title of the window.</param>
		protected abstract void SetTitle (string title);

#if DIRECTX && WINDOWS
        public static GameWindow Create(Game game, int width, int height)
        {
            var window = new MonoGame.Framework.WinFormsGameWindow((MonoGame.Framework.WinFormsGamePlatform)game.Platform);
            window.Initialize(width, height);

            return window;
        }
#endif
    }
}
