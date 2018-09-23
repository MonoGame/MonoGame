// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NUnit.Framework;

namespace MonoGame.Tests {
	static partial class GameTest {
		public abstract class FixtureBase {
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
		public class Disposal : FixtureBase {
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
			[Apartment(ApartmentState.STA)]
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
			[Apartment(ApartmentState.STA)]
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
		public class Behaviors : FixtureBase {
			[Test, Ignore("Fix me!"), Apartment(ApartmentState.STA)]
			public void Nongraphical_run_succeeds ()
			{
				Game.Run ();

				Assert.That (Game, Has.Property ("UpdateCount").EqualTo (1));
				Assert.That (Game, Has.Property ("DrawCount").EqualTo (0));
			}

			[Test, Ignore("Fix me!"), Apartment(ApartmentState.STA)]
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

        [TestFixture]
        public class Misc
        {
            [Test]
            [Ignore("MG crashes when no graphicsDeviceManager is set and Run is called")]
            public void LoadContentNotCalledWithoutGdm()
            {
                var g = new CountCallsGame();
                g.PublicInitialize();

                Assert.AreEqual(0, g.LoadContentCount);

                g.Dispose();
            }

            [Test]
            [Ignore("MG crashes when no GraphicsDevice is set and Run is called")]
            public void LoadContentNotCalledWithoutGd()
            {
                var g = new CountCallsGame();
                var gdm = new GraphicsDeviceManager(g);

                g.PublicInitialize();

                Assert.AreEqual(0, g.LoadContentCount);

                g.Dispose();
            }

            [Test]
#if DESKTOPGL
            [Ignore("This crashes inside SDL on Mac!")]
#endif
            public void ExitHappensAtEndOfTick()
            {
                // Exit called in Run
                var g = new ExitTestGame();

                // TODO this is not necessary for XNA, but MG crashes when no GDM is set and Run is called
                new GraphicsDeviceManager(g);
                g.Run();
                Assert.AreEqual(2, g.UpdateCount);
                Assert.AreEqual(0, g.DrawCount); // Draw should be suppressed
                Assert.AreEqual(1, g.ExitingCount);

                g.Dispose();
            }

            private class ExitTestGame : CountCallsGame
            {
                private int count = 0;

                protected override void Update(GameTime gameTime)
                {
                    if (count > 0)
                        Exit();

                    base.Update(gameTime);
                    Assert.IsNotNull(Window);
                    Assert.AreEqual(0, ExitingCount);

                    count++;
                }
            }

            private class CountCallsGame : Game
            {
                public int BeginRunCount { get; set; }
                public int InitializeCount { get; set; }
                public int LoadContentCount { get; set; }
                public int UnloadContentCount { get; set; }
                public int UpdateCount { get; set; }
                public int BeginDrawCount { get; set; }
                public int DrawCount { get; set; }
                public int EndDrawCount { get; set; }
                public int EndRunCount { get; set; }
                public int DeactivatedCount { get; set; }
                public int ActivatedCount { get; set; }
                public int ExitingCount { get; set; }
                public int DisposeCount { get; set; }

                public void PublicBeginRun() { BeginRun(); }
                protected override void BeginRun() { BeginRunCount++; base.BeginRun(); }
                public void PublicInitialize() { Initialize(); }
                protected override void Initialize() { InitializeCount++; base.Initialize(); }
                public void PublicLoadContent() { LoadContent(); }
                protected override void LoadContent() { LoadContentCount++; base.LoadContent(); }
                public void PublicUnloadContent() { UnloadContent(); }
                protected override void UnloadContent() { UnloadContentCount++; base.UnloadContent(); }
                public void PublicUpdate(GameTime gt = null) { Update(gt ?? new GameTime()); }
                protected override void Update(GameTime gameTime) { UpdateCount++; base.Update(gameTime); }
                public bool PublicBeginDraw() { return BeginDraw(); }
                protected override bool BeginDraw() { BeginDrawCount++; return base.BeginDraw(); }
                public void PublicDraw(GameTime gt) { Draw(gt ?? new GameTime()); }
                protected override void Draw(GameTime gameTime) { DrawCount++; base.Draw(gameTime); }
                public bool PublicEndDraw() { return BeginDraw(); }
                protected override void EndDraw() { EndDrawCount++; base.EndDraw(); }
                public void PublicEndRun() { EndRun(); }
                protected override void EndRun() { EndRunCount++; base.EndRun(); }

                protected override void OnActivated(object sender, EventArgs args) { ActivatedCount++; base.OnActivated(sender, args); }
                protected override void OnDeactivated(object sender, EventArgs args) { DeactivatedCount++; base.OnDeactivated(sender, args); }
                protected override void OnExiting(object sender, EventArgs args) { ExitingCount++; base.OnExiting(sender, args); }
                protected override void Dispose(bool disposing) { DisposeCount++; base.Dispose(disposing); }
            }

        }

		public class MockGame : TestGameBase {
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

            protected override void EndRun()
            {
                base.EndRun();
#if XNA
                AbsorbQuitMessage();
#endif
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

		public enum ExitReason {
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
