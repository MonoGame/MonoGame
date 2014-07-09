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
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

using NUnit.Framework;
using NUnit.Framework.Constraints;

namespace MonoGame.Tests {
	static partial class GameTest {
		abstract class FixtureBase {
			private MockGame _game;

			protected MockGame Game {
				get { return _game; }
			}

			[SetUp]
			public virtual void SetUp ()
			{
				Paths.SetStandardWorkingDirectory();
				_game = new MockGame ();
			}

			[TearDown]
			public virtual void TearDown ()
			{
				_game.Dispose ();
				_game = null;
			}
		}

		[TestFixture]
		class Disposal : FixtureBase {
			[TestCase ("Components")]
			[TestCase ("Content")]
			[TestCase ("GraphicsDevice")]
			[TestCase ("InactiveSleepTime")]
			[TestCase ("IsActive")]
			[TestCase ("IsFixedTimeStep")]
			[TestCase ("IsMouseVisible")]
			[TestCase ("LaunchParameters")]
			[TestCase ("Services")]
			[TestCase ("TargetElapsedTime")]
			[TestCase ("Window")]
			[RequiresSTA]
			public void Property_does_not_throws_after_Dispose (string propertyName)
			{
				var propertyInfo = Game.GetType ().GetProperty (propertyName);
				if (propertyInfo == null)
					Assert.Fail("Property '{0}' not found", propertyName);

				Game.Dispose ();
				AssertDoesNotThrow<ObjectDisposedException>(() =>
					RunAndUnpackException(() => propertyInfo.GetValue(Game, null)));
			}

			[TestCase ("Dispose")]
			[TestCase ("Exit")]
			[TestCase ("ResetElapsedTime")]
			[TestCase ("Run")]
			[TestCase ("RunOneFrame")]
			[TestCase ("SuppressDraw")]
			[TestCase ("Tick")]
			[RequiresSTA]
			public void Method_does_not_throw_after_Dispose (string methodName)
			{
				var methodInfo = Game.GetType ().GetMethod (methodName, new Type [0]);
				if (methodInfo == null)
					Assert.Fail("Method '{0}' not found", methodName);

				Game.Dispose ();
				AssertDoesNotThrow<ObjectDisposedException>(() =>
					RunAndUnpackException(() => methodInfo.Invoke (Game, null)));
			}

			private void RunAndDispose ()
			{
				Game.MakeGraphical ();
				Game.Run ();
				Game.Dispose ();
			}

			private static void RunAndUnpackException (Action action)
			{
				try {
					action ();
				} catch (TargetInvocationException ex) {
					throw ex.InnerException;
				}
			}

			private void RunDisposeAndTry (Action action, string name, bool shouldThrow)
			{
				RunAndDispose ();
				bool didThrow = false;
				try {
					action ();
				} catch (ObjectDisposedException) {
					if (!shouldThrow)
						Assert.Fail ("{0} threw ObjectDisposed when it shouldn't have.", name);
					didThrow = true;
				} catch (TargetInvocationException ex) {
					if (!(ex.InnerException is ObjectDisposedException))
						throw;

					if (!shouldThrow)
						Assert.Fail ("{0} threw ObjectDisposed when it shouldn't have.", name);
					didThrow = true;
				}
				if (didThrow && !shouldThrow)
					Assert.Fail ("{0} did not throw ObjectDisposedException when it should have.",
						     name);
			}

			private static void AssertDoesNotThrow<T> (TestDelegate code) where T : Exception
			{
				try {
					code ();
				} catch (T ex) {
					Assert.AreEqual (null, ex);
				} catch (Exception ex) {
					Console.WriteLine (
						"AssertDoesNotThrow<{0}> caught and ignored {1}", typeof(T), ex);
				}
			}
		}

		[TestFixture]
		class Behaviors : FixtureBase {
			[Test, RequiresSTA]
			public void Nongraphical_run_succeeds ()
			{
				Game.Run ();

				Assert.That (Game, Has.Property ("UpdateCount").EqualTo (1));
				Assert.That (Game, Has.Property ("DrawCount").EqualTo (0));
			}

			[Test, Ignore, RequiresSTA]
			public void Fixed_time_step_skips_draw_when_update_is_slow ()
			{
				Game.MakeGraphical ();

				var targetElapsedTime = TimeSpan.FromSeconds (1f / 10f);
				var slowUpdateTime = TimeSpan.FromSeconds (targetElapsedTime.TotalSeconds * 2);

				var slowUpdater = new SlowUpdater (Game, slowUpdateTime);
				Game.Components.Add (slowUpdater);

				var logger = new RunLoopLogger (Game);
				Game.Components.Add (logger);

				Game.MaxUpdateCount = int.MaxValue;
				Game.MaxDrawCount = 100;

				Game.IsFixedTimeStep = true;
				Game.TargetElapsedTime = targetElapsedTime;
				Game.Run ();

				//Assert.That(_game, Has.Property("UpdateCount").GreaterThan(11));
				//Assert.That(_game, Has.Property("DrawCount").EqualTo(10));
			}
		}

		private class MockGame : TestGameBase {
			public MockGame ()
			{
				MinUpdateCount = int.MaxValue;
				MinDrawCount = int.MaxValue;
				MaxUpdateCount = 1;
				MaxDrawCount = 1;
			}

			public MockGame MakeGraphical ()
			{
				if (Services.GetService (typeof (IGraphicsDeviceManager)) == null)
					new GraphicsDeviceManager (this);
				return this;
			}

			public int MinUpdateCount {
				get; set;
			}

			public int MaxUpdateCount {
				get; set;
			}

			public int MinDrawCount {
				get; set;
			}

			public int MaxDrawCount {
				get; set;
			}

			public int UpdateCount {
				get; private set;
			}

			public int DrawCount {
				get; private set;
			}

			public ExitReason ExitReason {
				get; private set;
			}

			private void EvaluateExitCriteria ()
			{
				ExitReason reason;
				if (UpdateCount >= MinUpdateCount && DrawCount >= MinDrawCount)
					reason = ExitReason.MinimumsSatisfied;
				else if (UpdateCount >= MaxUpdateCount)
					reason = ExitReason.MaxUpdateSatisfied;
				else if (DrawCount >= MaxDrawCount)
					reason = ExitReason.MaxDrawSatisfied;
				else
					reason = ExitReason.None;

				if (reason != ExitReason.None) {
					ExitReason = reason;
					DoExit();
				}
			}

			protected override void BeginRun ()
			{
				base.BeginRun ();
				UpdateCount = 0;
				DrawCount = 0;
			}

			protected override void Update (GameTime gameTime)
			{
				base.Update (gameTime);
				UpdateCount++;
				EvaluateExitCriteria ();
			}

			protected override void Draw (GameTime gameTime)
			{
				base.Draw (gameTime);
				DrawCount++;
				EvaluateExitCriteria ();
			}
		}

		private enum ExitReason {
			None,
			MinimumsSatisfied,
			MaxUpdateSatisfied,
			MaxDrawSatisfied
		}

		private class SlowUpdater : GameComponent {
			private TimeSpan _updateTime;
			public SlowUpdater (Game game, TimeSpan updateTime) :
				base (game)
			{
				_updateTime = updateTime;
			}

			int _count = 0;
			public override void Update (GameTime gameTime)
			{
				base.Update (gameTime);

				if (_count >= 4)
					return;

				_count++;

				//if (!gameTime.IsRunningSlowly)
				{
					var endTick = Stopwatch.GetTimestamp () +
						      (long) (Stopwatch.Frequency * _updateTime.TotalSeconds);
					//long endTick = (long)(_updateTime.TotalMilliseconds * 10) + DateTime.Now.Ticks;
					while (Stopwatch.GetTimestamp () < endTick) {
						// Be busy!
					}
				}
			}
		}

		private class RunLoopLogger : DrawableGameComponent {
			public RunLoopLogger (Game game) :
				base (game)
			{
			}

			private List<Entry> _entries = new List<Entry> ();
			public IEnumerable<Entry> GetEntries ()
			{
				return _entries.ToArray ();
			}

			public override void Update (GameTime gameTime)
			{
				base.Update (gameTime);
				_entries.Add (Entry.FromUpdate (gameTime));
			}

			public override void Draw (GameTime gameTime)
			{
				base.Draw (gameTime);
				_entries.Add (Entry.FromDraw (gameTime));
			}

			public string GetLogString ()
			{
				return string.Join (" ", _entries);
			}

			public struct Entry {
				public static Entry FromDraw (GameTime gameTime)
				{
					return new Entry {
						       Action = RunLoopAction.Draw,
						       ElapsedGameTime = gameTime.ElapsedGameTime,
						       TotalGameTime = gameTime.TotalGameTime,
						       WasRunningSlowly = gameTime.IsRunningSlowly
					};
				}

				public static Entry FromUpdate (GameTime gameTime)
				{
					return new Entry {
						       Action = RunLoopAction.Update,
						       ElapsedGameTime = gameTime.ElapsedGameTime,
						       TotalGameTime = gameTime.TotalGameTime,
						       WasRunningSlowly = gameTime.IsRunningSlowly
					};
				}

				public RunLoopAction Action {
					get; set;
				}

				public TimeSpan ElapsedGameTime {
					get; set;
				}

				public TimeSpan TotalGameTime {
					get; set;
				}

				public bool WasRunningSlowly {
					get; set;
				}

				public override string ToString ()
				{
					char actionInitial;
					switch (Action) {
					case RunLoopAction.Draw: actionInitial = 'd'; break;
					case RunLoopAction.Update: actionInitial = 'u'; break;
					default: throw new NotSupportedException (Action.ToString ());
					}

					return string.Format (
						       "{0}({1:0}{2})",
						       actionInitial,
						       ElapsedGameTime.TotalMilliseconds,
						       WasRunningSlowly ? "!" : "");
				}
			}
		}

		private enum RunLoopAction {
			Draw,
				Update
		}
	}
}
