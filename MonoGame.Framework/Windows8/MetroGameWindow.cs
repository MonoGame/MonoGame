#region License
/*
Microsoft Public License (Ms-PL)
XnaTouch - Copyright © 2009 The XnaTouch Team

All rights reserved.

This license governs use of the accompanying software. If you use the software, you accept this license. If you do not
accept the license, do not use the software.

1. Definitions
The terms "reproduce," "reproduction," "derivative works," and "distribution" have the same meaning here as under 
U.S. copyright law.

A "contribution" is the original software, or any additions or changes to the software.
A "contributor" is any person that distributes its contribution under this license.
"Licensed patents" are a contributor's patent claims that read directly on its contribution.

2. Grant of Rights
(A) Copyright Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, 
each contributor grants you a non-exclusive, worldwide, royalty-free copyright license to reproduce its contribution, prepare derivative works of its contribution, and distribute its contribution or any derivative works that you create.
(B) Patent Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, 
each contributor grants you a non-exclusive, worldwide, royalty-free license under its licensed patents to make, have made, use, sell, offer for sale, import, and/or otherwise dispose of its contribution in the software or derivative works of the contribution in the software.

3. Conditions and Limitations
(A) No Trademark License- This license does not grant you rights to use any contributors' name, logo, or trademarks.
(B) If you bring a patent claim against any contributor over patents that you claim are infringed by the software, 
your patent license from such contributor to the software ends automatically.
(C) If you distribute any portion of the software, you must retain all copyright, patent, trademark, and attribution 
notices that are present in the software.
(D) If you distribute any portion of the software in source code form, you may do so only under this license by including 
a complete copy of this license with your distribution. If you distribute any portion of the software in compiled or object 
code form, you may only do so under a license that complies with this license.
(E) The software is licensed "as-is." You bear the risk of using it. The contributors give no express warranties, guarantees
or conditions. You may have additional consumer rights under your local laws which this license cannot change. To the extent
permitted under your local laws, the contributors exclude the implied warranties of merchantability, fitness for a particular
purpose and non-infringement.
*/
#endregion License

#region Using Statements
using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Windows.UI.Core;
using System.Runtime.InteropServices;
using Windows.Graphics.Display;
#endregion Using Statements

namespace Microsoft.Xna.Framework
{
    public partial class MetroGameWindow : GameWindow
    {
        private CoreWindow _coreWindow;
        protected Game game;
        private readonly List<Keys> _keys;
        private Rectangle _clientBounds;

        #region Internal Properties

        internal Game Game { get; set; }

        internal bool IsExiting { get; set; }

        #endregion

        #region Public Properties

        public override IntPtr Handle { get { return Marshal.GetIUnknownForObject(_coreWindow); } }

        public override string ScreenDeviceName { get { return String.Empty; } } // window.Title

        public override Rectangle ClientBounds { get { return _clientBounds; } }

        public override bool AllowUserResizing
        {
            get { return false; }
            set 
            {
                // You cannot resize a Metro window!
            }
        }

        public override DisplayOrientation CurrentOrientation
        {
            get { return DisplayOrientation.LandscapeLeft; }
        }

        protected internal override void SetSupportedOrientations(DisplayOrientation orientations)
        {
            // Do nothing.  Desktop platforms don't do orientation.
        }

        #endregion

        static public MetroGameWindow Instance { get; private set; }

        static MetroGameWindow()
        {
            Instance = new MetroGameWindow();
        }

        private MetroGameWindow()
        {
            _keys = new List<Keys>();
        }

        #region Restricted Methods

        #region Delegates

        /*
        private void OpenTkGameWindow_Closing(object sender, CancelEventArgs e)
        {
            Game.Exit();
        }
        */

        private void Keyboard_KeyUp(CoreWindow sender, KeyEventArgs args)
        {
            // VirtualKey maps pretty much to XNA keys.
            var xnaKey = (Keys)args.VirtualKey;

            if (_keys.Contains(xnaKey))
                _keys.Remove(xnaKey);
        }

        private void Keyboard_KeyDown(CoreWindow sender, KeyEventArgs args)
        {
            // VirtualKey maps directly to XNA keys.
            var xnaKey = (Keys)args.VirtualKey;

            if (!_keys.Contains(xnaKey))
                _keys.Add(xnaKey);
        }

        #endregion

        private void HandleInput()
        {
            // mouse doesn't need to be treated here, Mouse class does it alone

            // keyboard
            Keyboard.State = new KeyboardState(_keys.ToArray());
        }

        #endregion

        public void Initialize(CoreWindow coreWindow)
        {
            _coreWindow = coreWindow;

            _coreWindow.SizeChanged += Window_SizeChanged;
            _coreWindow.Closed += Window_Closed;

            //window.Closing += new EventHandler<CancelEventArgs>(OpenTkGameWindow_Closing);
            //window.Resize += OnResize;
            _coreWindow.KeyDown += Keyboard_KeyDown;
            _coreWindow.KeyUp += Keyboard_KeyUp;

            var bounds = _coreWindow.Bounds;
            SetClientBounds(bounds.Width, bounds.Height);

            // Set the window icon.
            //window.Icon = Icon.ExtractAssociatedIcon(Assembly.GetEntryAssembly().Location);

            InitializeTouch();
        }

        private void Window_Closed(CoreWindow sender, CoreWindowEventArgs args)
        {
            throw new NotImplementedException();
        }

        private void SetClientBounds(double width, double height)
        {
            var dpi = DisplayProperties.LogicalDpi;
            var pwidth = width * dpi / 96.0;
            var pheight = height * dpi / 96.0;

            _clientBounds = new Rectangle(0, 0, (int)pwidth, (int)pheight);
        }

        private void Window_SizeChanged(CoreWindow sender, WindowSizeChangedEventArgs args)
        {
            SetClientBounds( args.Size.Width, args.Size.Height );
            OnClientSizeChanged();
        }

        protected override void SetTitle(string title)
        {
            //window.Title = title;
        }

        internal void SetCursor(bool visible)
        {
            if ( _coreWindow == null )
                return;

            if (visible)
                _coreWindow.PointerCursor = new CoreCursor(CoreCursorType.Arrow, 0);
            else
                _coreWindow.PointerCursor = null;
        }

        internal void RunLoop()
        {
            SetCursor(Game.IsMouseVisible);
            _coreWindow.Activate();

            while (true)
            {
                // Process events incoming to the window.
                _coreWindow.Dispatcher.ProcessEvents(CoreProcessEventsOption.ProcessAllIfPresent);

                // Process the game.
                if (Game != null)
                {
                    HandleInput();
                    Game.Tick();
                }

                if (IsExiting)
                    break;
            }
        }

        #region Public Methods

        public void Dispose()
        {
            //window.Dispose();
        }

        public override void BeginScreenDeviceChange(bool willBeFullScreen)
        {
        }

        public override void EndScreenDeviceChange(string screenDeviceName, int clientWidth, int clientHeight)
        {

        }

        #endregion
    }
}

