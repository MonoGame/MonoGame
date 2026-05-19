// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Linq;
using CoreGraphics;
using Foundation;
using UIKit;
using Microsoft.Xna.Framework;

namespace MonoGame.InteractiveTests.iOS
{
    /// <summary>
    /// A <see cref="UIViewController"/> that shows the tests and invokes the <see cref="TestGame"/>
    /// as needed.
    ///
    /// TODO: Add automatic invocation of tests as needed.
    /// </summary>
    public partial class RootViewController : UIViewController
    {
        private readonly InteractiveTests _interactiveTests;
        private readonly Action<RootViewController> _onLoadView;

        private Game _activeGame;

        private UITableView _tableView;

        public RootViewController(Action<RootViewController> newScreenFunc)
        {
            _interactiveTests = new();
            _onLoadView = newScreenFunc;
            base.Title = "Interactive Tests";
        }

        public override void LoadView()
        {
            View = new UIView();

            var rect = new CGRect(0, 0, (int)View.Frame.Size.Width, (int)View.Frame.Size.Height);
            _tableView = new UITableView(rect);
            _tableView.AutoresizingMask =
                UIViewAutoresizing.FlexibleHeight |
                UIViewAutoresizing.FlexibleWidth;

            _tableView.Delegate = new TableViewDelegate(this);
            _tableView.DataSource = new TableViewDataSource(this);
            View.Add(_tableView);

            _onLoadView?.Invoke(this);
        }

        public override bool ShouldAutorotateToInterfaceOrientation(UIInterfaceOrientation toInterfaceOrientation)
        {
            if (_UserInterfaceIdiomIsPhone)
            {
                return (toInterfaceOrientation != UIInterfaceOrientation.PortraitUpsideDown);
            }

            return true;
        }

        /// <summary>
        /// Run tests one at a time starting from the current index.
        /// RootViewController may be cleanly destroyed and recreated to run the subsequent
        /// tests one at a time, with no preserved state.
        /// TODO: Automatic test runs are in-progress, will add next.
        /// </summary>
        private void RunTests(int testIndex)
        {
            if (testIndex < 0 || testIndex >= _interactiveTests.Tests.Count)
            {
                ActiveGame_Exiting(this, vc => { });
                return;
            }

            _activeGame = _interactiveTests.Tests[testIndex].Create();
            _activeGame.Exiting += (sender, args) =>
            {
                ActiveGame_Exiting(this, vc => { vc.RunTests(testIndex + 1); });
            };
        }

        private void RunTest(InteractiveTest test)
        {
            if (View == null || _activeGame != null)
            {
                throw new InvalidOperationException(
                    "Invalid operation: Empty view or an interactive test is already active.");
            }

            _activeGame = test.Create();
            _activeGame.Exiting += ((sender, args) => ActiveGame_Exiting(this, vc => { }));

            _activeGame.Run(GameRunBehavior.Asynchronous);

            View.Window.Hidden = true;
        }

        public delegate void OnExiting(object sender, Action<RootViewController> newScreenFunc);

        public OnExiting Exiting;

        private void ActiveGame_Exiting(object sender, Action<RootViewController> newScreenFunc)
        {
            GameDebug.C("---Game exiting.");
            if (View != null)
            {
                View.Hidden = true;
                View.RemoveFromSuperview();
            }

            _activeGame = null;

            // This ensure the Game loop keyed off the Window no longer runs.
            foreach (var window in UIApplication.SharedApplication.Windows)
            {
                window.RootViewController = null;
                window.Hidden = true;
                window.RemoveFromSuperview();
            }

            Exiting.Invoke(sender, newScreenFunc);
        }

        // Exposed to the UITableView code below.
        private IEnumerable<InteractiveTest> Tests()
        {
            return _interactiveTests.Tests.OrderBy(x => x.Name);
        }

        // Exposed to the UITableView code below.
        private IEnumerable<string> Categories()
        {
            return Tests().Select(x => x.Category).Distinct().OrderBy(x => x);
        }

        /// <summary>
        /// Shows the list of tests with the source being <see cref="InteractiveTests"/>.
        /// </summary>
        private class TableViewDelegate : UITableViewDelegate
        {
            private readonly RootViewController _owner;

            public TableViewDelegate(RootViewController owner)
            {
                _owner = owner ?? throw new ArgumentNullException("No root view controller present.");
            }

            public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
            {
                var tests = _owner.Tests();
                var categories = _owner.Categories().ToArray();
                var category = categories[indexPath.Section];

                var test = tests.Where(x => x.Category == category).ElementAt(indexPath.Row);

                _owner.RunTest(test);
            }
        }

        /// <summary>
        /// Shows the <see cref="InteractiveTest"/>s supported on this platform in a table with sections.
        /// </summary>
        private class TableViewDataSource : UITableViewDataSource
        {
            private readonly RootViewController _owner;

            public TableViewDataSource(RootViewController owner)
            {
                if (owner == null)
                    throw new ArgumentNullException("owner");
                _owner = owner;
            }

            public override nint NumberOfSections(UITableView tableView)
            {
                return (nint)_owner.Categories().Count();
            }

            public override nint RowsInSection(UITableView tableView, nint section)
            {
                var tests = _owner.Tests();
                var categories = _owner.Categories().ToArray();
                var category = categories[(int)section];

                return (nint)tests.Where(x => x.Category == category).Count();
            }

            public override string TitleForHeader(UITableView tableView, nint section)
            {
                var categories = _owner.Categories().ToArray();
                return categories[(int)section];
            }

            public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
            {
                var tests = _owner.Tests();
                var categories = _owner.Categories().ToArray();
                var category = categories[indexPath.Section];

                var test = tests
                    .Where(x => x.Category == category)
                    .ElementAt(indexPath.Row);

                var cell = tableView.DequeueReusableCell("UITableViewCell");
                if (cell == null)
                    cell = new UITableViewCell(UITableViewCellStyle.Default, "UITableViewCell");
                cell.TextLabel.Text = test.Name;
                return cell;
            }
        }

        private static bool _UserInterfaceIdiomIsPhone
        {
            get { return UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Phone; }
        }
    }
}
