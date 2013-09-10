using System;
using NUnit.Framework;

namespace MonoGame.Tests {
    class AudioTestFixtureBase {
        protected TestGameBase Game { get; private set; }
         
        [SetUp]
        public virtual void SetUp ()
        {
            Paths.SetStandardWorkingDirectory();
            Game = new TestGameBase ();
            Game.ExitCondition = x => x.DrawNumber > 1;
        }

        [TearDown]
        public virtual void TearDown ()
        {
            Game.Dispose ();
            Game = null;
        }


    }
}

