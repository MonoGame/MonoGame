#region License
/*
Microsoft Public License (Ms-PL)
MonoGame - Copyright © 2009-2012 The MonoGame Team

All rights reserved.

This license governs use of the accompanying software. If you use the software,
you accept this license. If you do not accept the license, do not use the
software.

1. Definitions

The terms "reproduce," "reproduction," "derivative works," and "distribution"
have the same meaning here as under U.S. copyright law.

A "contribution" is the original software, or any additions or changes to the
software.

A "contributor" is any person that distributes its contribution under this
license.

"Licensed patents" are a contributor's patent claims that read directly on its
contribution.

2. Grant of Rights

(A) Copyright Grant- Subject to the terms of this license, including the
license conditions and limitations in section 3, each contributor grants you a
non-exclusive, worldwide, royalty-free copyright license to reproduce its
contribution, prepare derivative works of its contribution, and distribute its
contribution or any derivative works that you create.

(B) Patent Grant- Subject to the terms of this license, including the license
conditions and limitations in section 3, each contributor grants you a
non-exclusive, worldwide, royalty-free license under its licensed patents to
make, have made, use, sell, offer for sale, import, and/or otherwise dispose of
its contribution in the software or derivative works of the contribution in the
software.

3. Conditions and Limitations

(A) No Trademark License- This license does not grant you rights to use any
contributors' name, logo, or trademarks.

(B) If you bring a patent claim against any contributor over patents that you
claim are infringed by the software, your patent license from such contributor
to the software ends automatically.

(C) If you distribute any portion of the software, you must retain all
copyright, patent, trademark, and attribution notices that are present in the
software.

(D) If you distribute any portion of the software in source code form, you may
do so only under this license by including a complete copy of this license with
your distribution. If you distribute any portion of the software in compiled or
object code form, you may only do so under a license that complies with this
license.

(E) The software is licensed "as-is." You bear the risk of using it. The
contributors give no express warranties, guarantees or conditions. You may have
additional consumer rights under your local laws which this license cannot
change. To the extent permitted under your local laws, the contributors exclude
the implied warranties of merchantability, fitness for a particular purpose and
non-infringement.
*/
#endregion License

using System;
using System.Drawing;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;

namespace MonoGame.InteractiveTests.TestUI {
	[InteractiveTest("Test UI Test", Categories.Meta)]
	public class TestUITestGame : Game {
		private SpriteFont _font;
		private Universe _universe;

		public TestUITestGame ()
		{
			new GraphicsDeviceManager (this);
			TouchPanel.EnabledGestures = GestureType.DoubleTap | GestureType.Tap;

			Content.RootDirectory = "Content";
		}

		protected override void Initialize()
		{
			base.Initialize();
		}

		protected override void LoadContent()
		{
			base.LoadContent();

			_font = Content.Load<SpriteFont> (@"Fonts\Default");

			_universe = new Universe(Content) {
				AutoHandleInput = true
			};
			Components.Add (new UniverseComponent(this, _universe));

			var exitButton = new Button
			{
				BackgroundColor = Color.Black,
				Content = new Label {
					Font = _font,
					Text = "Exit",
					TextColor = Color.White
				},
				Location = PointF.Empty
			};

			exitButton.Content.SizeToFit ();
			exitButton.SizeToFit ();
			exitButton.Tapped += (sender, e) => {
				Exit ();
			};

			var label = new Label {
				BackgroundColor = Color.Indigo,
				Font = _font,
				Location = new PointF(20, 200),
				Text = "fantastic",
				TextColor = Color.White
			};

			label.SizeToFit ();

			var button1 = new Button
			{
				BackgroundColor = Color.OrangeRed,
				Content = new Label {
					Font = _font,
					Text = "Button 1",
					TextColor = Color.White
				},
				Location = new PointF(label.Frame.Left, 60)
			};

			button1.Content.SizeToFit ();
			button1.SizeToFit ();

			button1.Tapped += (sender, e) => {
				label.Text = ("button 1!");
			};

			var button2 = new Button
			{
				BackgroundColor = Color.Goldenrod,
				Content = new Label {
					Font = _font,
					Text = "Button 2",
					TextColor = Color.White
				},
				Location = new PointF(button1.Frame.Left, button1.Frame.Bottom)
			};

			button2.Content.SizeToFit ();
			button2.SizeToFit ();

			button2.Tapped += (sender, e) => {
				label.Text = ("button 2!");
			};

			_universe.Add (exitButton);
			_universe.Add (button1);
			_universe.Add (button2);
			_universe.Add (label);
		}

		protected override void UnloadContent()
		{
			base.UnloadContent();

			_font = null;
		}

		protected override void Update(GameTime gameTime)
		{
			base.Update(gameTime);
		}

		protected override void Draw(GameTime gameTime)
		{
			GraphicsDevice.Clear (Color.DarkBlue);
			base.Draw(gameTime);
		}
	}
}

