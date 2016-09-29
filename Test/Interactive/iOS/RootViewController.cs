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
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;

using MonoTouch.Foundation;
using MonoTouch.UIKit;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input.Touch;

namespace MonoGame.InteractiveTests.iOS {
	public class RootViewController : UIViewController {
		private static bool UserInterfaceIdiomIsPhone {
			get { return UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Phone; }
		}

		private UITableView _tableView;
		private readonly InteractiveTest[] _interactiveTests;

		private Game _activeGame;
		private InteractiveTest _activeTest;

		public RootViewController ()
			: base ()
		{
			_interactiveTests = DiscoverInteractiveTests ();
			Title = "Interactive Tests";

			

		}

		private static InteractiveTest[] DiscoverInteractiveTests () {
			var assembly = Assembly.GetExecutingAssembly ();
			var tests = new List<InteractiveTest> ();
			foreach (var type in assembly.GetTypes ()) {
				InteractiveTest test;
				if (!InteractiveTest.TryCreateFrom(type, out test))
					continue;

				tests.Add (test);
			}
			return tests.ToArray ();
		}

		public override void LoadView()
		{
			View = new UIView();
			_tableView = new UITableView(new RectangleF(PointF.Empty, View.Frame.Size));
			_tableView.AutoresizingMask =
				UIViewAutoresizing.FlexibleHeight |
				UIViewAutoresizing.FlexibleWidth;

			_tableView.Delegate = new TableViewDelegate (this);
			_tableView.DataSource = new TableViewDataSource (this);

			View.Add (_tableView);
		}
		
		public override bool ShouldAutorotateToInterfaceOrientation (UIInterfaceOrientation toInterfaceOrientation)
		{
			if (UserInterfaceIdiomIsPhone) {
				return (toInterfaceOrientation != UIInterfaceOrientation.PortraitUpsideDown);
			} else {
				return true;
			}
		}

		private void RunTest(InteractiveTest test) {
			if (_activeGame != null || _activeTest != null)
				throw new InvalidOperationException("An interactive test is already active.");

			_activeTest = test;

			_activeGame = test.Create ();
			_activeGame.Exiting += ActiveGame_Exiting;
			_activeGame.Run (GameRunBehavior.Asynchronous);

			View.Window.Hidden = true;
		}

		private void ActiveGame_Exiting (object sender, EventArgs e)
		{
			_activeGame.Dispose ();
			_activeGame = null;

			_activeTest = null;

			// HACK: TouchPanel should probably clear itself at the
			//       end of a Game run.
			TouchPanel.EnabledGestures = GestureType.None;

			View.Window.MakeKeyAndVisible ();
			View.LayoutSubviews ();
		}

		private class InteractiveTest {
			public static bool TryCreateFrom(Type type, out InteractiveTest test) {
				test = null;
				if (!typeof(Game).IsAssignableFrom(type))
					return false;

				var attrs = type.GetCustomAttributes( typeof(InteractiveTestAttribute), false);
				if (attrs.Length == 0)
					return false;

				var attr = (InteractiveTestAttribute)attrs[0];

				test = new InteractiveTest(
					type, attr.Name ?? type.Name, attr.Category ?? Categories.Default);
				return true;
			}

			private InteractiveTest(Type type, string name, string category) {
				_type = type;
				_name = name;
				_category = category;
			}

			private readonly Type _type;
			public Type Type {
				get { return _type; }
			}

			private readonly string _name;
			public string Name {
				get { return _name; }
			}

			private readonly string _category;
			public string Category {
				get { return _category; }
			}

			public Game Create() {
				return (Game)Activator.CreateInstance(_type);
			}
		}

		private class TableViewDelegate : UITableViewDelegate {
			private readonly RootViewController _owner;

			public TableViewDelegate (RootViewController owner) {
				if (owner == null)
					throw new ArgumentNullException ("owner");
				_owner = owner;
			}

			public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
			{
				var tests = (IEnumerable<InteractiveTest>)_owner._interactiveTests;
				var categories = tests.Select (x => x.Category).OrderBy(x => x).ToArray ();
				var category = categories [indexPath.Section];

				var test = tests
					.Where (x => x.Category == category)
					.OrderBy (x => x.Name)
					.ElementAt (indexPath.Row);

				_owner.RunTest (test);
			}
		}

		private class TableViewDataSource : UITableViewDataSource {

			private readonly RootViewController _owner;
			public TableViewDataSource(RootViewController owner) {
				if (owner == null)
					throw new ArgumentNullException("owner");
				_owner = owner;
			}

			public override int NumberOfSections(UITableView tableView)
			{
				var tests = (IEnumerable<InteractiveTest>)_owner._interactiveTests;
				return tests.Select (x => x.Category).Distinct().Count ();
			}

			public override int RowsInSection(UITableView tableView, int section)
			{
				var tests = (IEnumerable<InteractiveTest>)_owner._interactiveTests;
				var categories = tests.Select (x => x.Category).OrderBy(x => x).ToArray ();
				var category = categories[section];

				return tests.Where (x => x.Category == category).Count ();
			}

			public override string TitleForHeader(UITableView tableView, int section)
			{
				var tests = (IEnumerable<InteractiveTest>)_owner._interactiveTests;
				var categories = tests.Select (x => x.Category).OrderBy(x => x).ToArray ();

				return categories[section];
			}

			public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
			{
				var tests = (IEnumerable<InteractiveTest>)_owner._interactiveTests;
				var categories = tests.Select (x => x.Category).OrderBy(x => x).ToArray ();
				var category = categories [indexPath.Section];

				var test = tests
					.Where (x => x.Category == category)
					.OrderBy (x => x.Name)
					.ElementAt (indexPath.Row);

				var cell = tableView.DequeueReusableCell("UITableViewCell");
				if (cell == null)
					cell = new UITableViewCell(UITableViewCellStyle.Default, "UITableViewCell");
				cell.TextLabel.Text = test.Name;
				return cell;
			}
		}
	}
}
