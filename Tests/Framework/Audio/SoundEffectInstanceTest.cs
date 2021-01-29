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
#if !DESKTOPGL
        [Ignore("bug is platform specific to GLDesktop")]
#endif
        public void SoundEffectPauseStopPlay()
        {

            var se = new SoundEffect(new byte[16000], 8000, AudioChannels.Mono);
            
            using (var instance = se.CreateInstance())
            {
                instance.IsLooped = true; //ensures that the sound effect does not stop unless Stop() is called.

                instance.Play();
                Assert.AreEqual(SoundState.Playing, instance.State);

                instance.Stop();
                Assert.AreEqual(SoundState.Stopped, instance.State);
                instance.Play();
                Assert.AreEqual(SoundState.Playing, instance.State);
                instance.Pause();
                Assert.AreEqual(SoundState.Paused, instance.State);
            }
        }

    }
}
