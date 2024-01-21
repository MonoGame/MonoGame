// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

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
