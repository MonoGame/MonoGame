// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using NUnit.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Threading;

namespace MonoGame.Tests.Audio
{
    class SoundEffectInstanceTest
    {
        [SetUp]
        public void SetUp()
        {
            // Necessary to get audio initialised
            FrameworkDispatcher.Update();
        }

        /// <summary>
        /// Unit test for issue #7372 where the Sound effects instance does not play after Play()
        /// is called after calling Pause(), Stop().
        /// </summary>
        [Test]
        public void SoundEffectPauseStopPlay()
        {

            var se = new SoundEffect(new byte[16000], 8000, AudioChannels.Mono);
            
            using (var instance = se.CreateInstance())
            {
                instance.IsLooped = true; //ensures that the sound effect does not stop unless Stop() is called.

                //Test Initial State
                Assert.AreEqual(SoundState.Stopped, instance.State);

                //Test calling pause multiple times
                instance.Play();
                Assert.AreEqual(SoundState.Playing, instance.State);
                instance.Pause();
                Assert.AreEqual(SoundState.Paused, instance.State);
                instance.Stop();
                SleepWhileDispatching(10);// XNA Requires Dispatcher to be updated
                Assert.AreEqual(SoundState.Stopped, instance.State);
                instance.Play();
                Assert.AreEqual(SoundState.Playing, instance.State);
                instance.Pause();
                Assert.AreEqual(SoundState.Paused, instance.State);
            }
        }


        private static void SleepWhileDispatching(int ms)
        {
            int cycles = ms / 10;
            for (int i = 0; i < cycles; i++)
            {
                FrameworkDispatcher.Update();
                Thread.Sleep(10);
            }
        }

    }
}
