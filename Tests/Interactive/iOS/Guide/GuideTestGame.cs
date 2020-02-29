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

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;

using MonoGame.InteractiveTests.TestUI;
using System.Drawing;

namespace MonoGame.InteractiveTests {
	[InteractiveTest("Guide", Categories.GamerServices)]
	public class GuideTestGame : Game {
		public GuideTestGame ()
		{
			var graphics = new GraphicsDeviceManager(this);
			graphics.SupportedOrientations =
				DisplayOrientation.Portrait |
				DisplayOrientation.LandscapeLeft |
				DisplayOrientation.LandscapeRight;

			Components.Add (new GamerServicesComponent (this));

			Content.RootDirectory = "Content";
		}

		protected override void Initialize()
		{
			base.Initialize();
			TouchPanel.EnabledGestures = GestureType.DoubleTap | GestureType.Tap;
		}

		private SpriteFont _font;
		protected override void LoadContent()
		{
			base.LoadContent();

			_font = Content.Load<SpriteFont> (@"Fonts\Default");
			InitializeGui();
		}

		private Universe _universe;
		private Label _labelEndShowKeyboardInput;
		private Label _labelShowKeyboardInputCallback;
		private void InitializeGui()
		{
			_universe = new Universe (Content)
			{
				AutoHandleInput = true
			};
			Components.Add (new UniverseComponent (this, _universe));

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

			_labelEndShowKeyboardInput = new Label
			{
				Frame = new RectangleF (20, 60, 320, 20),
				Font = _font,
				TextColor = Color.White
			};

			_labelShowKeyboardInputCallback = new Label
			{
				Frame = new RectangleF (
					_labelEndShowKeyboardInput.Frame.Left,
					_labelEndShowKeyboardInput.Frame.Bottom + 10,
					_labelEndShowKeyboardInput.Frame.Width,
					_labelEndShowKeyboardInput.Frame.Height),
				Font = _font,
				TextColor = Color.White
			};

			var buttonShowKeyboardInput = new Button
			{
				BackgroundColor = Color.Lavender,
				Content = new Label
				{
					Font = _font,
					Text = "Show Keyboard Input",
					TextColor = Color.Black
				},
				Location = new PointF(20, 200)
			};

			buttonShowKeyboardInput.Content.SizeToFit ();
			buttonShowKeyboardInput.SizeToFit ();
			buttonShowKeyboardInput.Tapped += (sender, e) => {
				TestShowKeyboardInput (
					"Some normal title",
					"And a perfectly ordinary description",
					"the default");
			};

			var buttonShowKeyboardInputLong = new Button
			{
				BackgroundColor = Color.Lavender,
				Content = new Label
				{
					Font = _font,
					Text = "Show Keyboard Input (long)",
					TextColor = Color.Black
				},
				Location = new PointF(
					buttonShowKeyboardInput.Frame.Left,
					buttonShowKeyboardInput.Frame.Bottom + 10)
			};

			buttonShowKeyboardInputLong.Content.SizeToFit ();
			buttonShowKeyboardInputLong.SizeToFit ();
			buttonShowKeyboardInputLong.Tapped += (sender, e) => {
				TestShowKeyboardInput (
					"This is the title that never ends, yes it goes on and on my friends.  One " +
					"day some people started writing it, etc",
					"And here is a super-duper description that rambles on a bit about, you " +
					"know, whatever.  And then finally ends over here at about this point.",
					"surprisingly terse default");
			};

			_universe.Add (exitButton);
			_universe.Add (_labelEndShowKeyboardInput);
			_universe.Add (_labelShowKeyboardInputCallback);
			_universe.Add (buttonShowKeyboardInput);
			_universe.Add (buttonShowKeyboardInputLong);
		}

		private void TestShowKeyboardInput (string title, string description, string defaultText)
		{
			var result = Guide.BeginShowKeyboardInput(
				PlayerIndex.One,
				title,
				description,
				defaultText,
				Guide_ShowKeyboardInputCallback, null);

			_labelEndShowKeyboardInput.Text =
				"EndShow: " + (Guide.EndShowKeyboardInput (result) ?? "<null>");
		}

		private void Guide_ShowKeyboardInputCallback(IAsyncResult result)
		{
			_labelShowKeyboardInputCallback.Text =
				"CallBack: " + (Guide.EndShowKeyboardInput (result) ?? "<null>");
		}

		protected override void Update(GameTime gameTime)
		{
			base.Update(gameTime);
		}

		protected override void Draw(GameTime gameTime)
		{
			GraphicsDevice.Clear (Color.Indigo);
			base.Draw(gameTime);
		}
	}
}

