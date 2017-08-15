// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;
using Microsoft.Xna.Framework.Audio;
using NUnit.Framework;
using Microsoft.Xna.Framework;

namespace MonoGame.Tests.Audio
{
    [TestFixture]
    public class XactTests
    {
        private AudioEngine _audioEngine;
        private SoundBank _soundBank;
        private WaveBank _waveBank;

        [SetUp]
        public void Setup()
        {
            // Necessary to get audio initialised
            FrameworkDispatcher.Update();
            _audioEngine = new AudioEngine(@"Assets\Audio\Win\Tests.xgs");
            _waveBank = new WaveBank(_audioEngine, @"Assets\Audio\Win\Tests.xwb");
            _soundBank = new SoundBank(_audioEngine, @"Assets\Audio\Win\Tests.xsb");
        }

        [TearDown]
        public void TearDown()
        {
            _soundBank.Dispose();
            _waveBank.Dispose();
            _audioEngine.Dispose();
        }

        [Test]
        public static void AudioEngineCtor()
        {
            Assert.Throws<ArgumentNullException>(() => new AudioEngine(null));
            Assert.Throws<ArgumentNullException>(() => new AudioEngine(""));
            //Assert.Throws<DirectoryNotFoundException>(() => new AudioEngine(@"This\Does\Not\Exist.xgs"));
            Assert.Throws<FileNotFoundException>(() => new AudioEngine(@"Assets\Audio\Win\NotTheFile.xgs"));

            var audioEngine = new AudioEngine(@"Assets\Audio\Win\Tests.xgs");
            Assert.False(audioEngine.IsDisposed);
            audioEngine.Dispose();
            Assert.True(audioEngine.IsDisposed);
        }

        [Test]
        public void WaveBankCtor()
        {
            Assert.Throws<ArgumentNullException>(() => new WaveBank(null, null));
            Assert.Throws<ArgumentNullException>(() => new WaveBank(_audioEngine, null));
            Assert.Throws<ArgumentNullException>(() => new WaveBank(_audioEngine, ""));
            //Assert.Throws<DirectoryNotFoundException>(() => new WaveBank(_audioEngine, @"This\Does\Not\Exist.xwb"));
            Assert.Throws<FileNotFoundException>(() => new WaveBank(_audioEngine, @"Assets\Audio\Win\NotTheFile.xwb"));

            var waveBank = new WaveBank(_audioEngine, @"Assets\Audio\Win\Tests.xwb");
            Assert.False(waveBank.IsInUse);
            Assert.False(waveBank.IsDisposed);
            Assert.True(waveBank.IsPrepared);

            waveBank.Dispose();
            Assert.True(waveBank.IsDisposed);
            Assert.False(waveBank.IsInUse);
            Assert.False(waveBank.IsPrepared);
        }

        [Test]
        public void SoundBankCtor()
        {
            Assert.Throws<ArgumentNullException>(() => new SoundBank(null, null));
            Assert.Throws<ArgumentNullException>(() => new SoundBank(_audioEngine, null));
            Assert.Throws<ArgumentNullException>(() => new SoundBank(_audioEngine, ""));
            //Assert.Throws<DirectoryNotFoundException>(() => new SoundBank(_audioEngine, @"This\Does\Not\Exist.xsb"));
            Assert.Throws<FileNotFoundException>(() => new SoundBank(_audioEngine, @"Assets\Audio\Win\NotTheFile.xsb"));

            var soundBank = new SoundBank(_audioEngine, @"Assets\Audio\Win\Tests.xsb");
            Assert.False(soundBank.IsInUse);
            Assert.False(soundBank.IsDisposed);

            var cue = soundBank.GetCue("blast_mono");
            Assert.True(soundBank.IsInUse);
            cue.Dispose();
            //Assert.False(soundBank.IsInUse); // MonoGame Fail

            soundBank.Dispose();
            Assert.True(soundBank.IsDisposed);
            Assert.False(soundBank.IsInUse);
        }

        [Test]
        public static void ContentVersion()
        {
            Assert.AreEqual(39, AudioEngine.ContentVersion);            
        }

        [Test]
        public void GetCategory()
        {
            Assert.Throws<ArgumentNullException>(() => _audioEngine.GetCategory(null));
            Assert.Throws<ArgumentNullException>(() => _audioEngine.GetCategory(""));
            Assert.Throws<InvalidOperationException>(() => _audioEngine.GetCategory("DoesNotExist"));

            // Make sure case matters.
            Assert.Throws<InvalidOperationException>(() => _audioEngine.GetCategory("DEFAULT"));
            Assert.Throws<InvalidOperationException>(() => _audioEngine.GetCategory("default"));

            // Make sure we can get the different categories.
            Assert.AreEqual("Default", _audioEngine.GetCategory("Default").Name);
            Assert.AreEqual("The End", _audioEngine.GetCategory("The End").Name);
            Assert.AreEqual("Subcat", _audioEngine.GetCategory("Subcat").Name);
        }

        [Test]
        public void GetGlobalVariable()
        {
            Assert.Throws<ArgumentNullException>(() => _audioEngine.GetGlobalVariable(null));
            Assert.Throws<ArgumentNullException>(() => _audioEngine.GetGlobalVariable(""));
            Assert.Throws<IndexOutOfRangeException>(() => _audioEngine.GetGlobalVariable("DoesNotExist"));

            // Make sure case matters.
            Assert.Throws<IndexOutOfRangeException>(() => _audioEngine.GetGlobalVariable("SPEEDOFSOUND"));
            Assert.Throws<IndexOutOfRangeException>(() => _audioEngine.GetGlobalVariable("speedofsound"));

            Assert.AreEqual(343.5f, _audioEngine.GetGlobalVariable("SpeedOfSound"));
            Assert.AreEqual(12.34f, _audioEngine.GetGlobalVariable("Global Public"));
            
            // Make sure instance variables can't be accessed.
            Assert.Throws<IndexOutOfRangeException>(() => _audioEngine.GetGlobalVariable("OrientationAngle"));

            // Make sure private variables can't be accessed.
            Assert.Throws<IndexOutOfRangeException>(() => _audioEngine.GetGlobalVariable("Global Private"));
        }

        [Test]
        public void SetGlobalVariable()
        {
            Assert.Throws<ArgumentNullException>(() => _audioEngine.SetGlobalVariable(null, 0));
            Assert.Throws<ArgumentNullException>(() => _audioEngine.SetGlobalVariable("", 0));
            Assert.Throws<IndexOutOfRangeException>(() => _audioEngine.SetGlobalVariable("DoesNotExist", 0));

            // Make sure case matters.
            Assert.Throws<IndexOutOfRangeException>(() => _audioEngine.SetGlobalVariable("SPEEDOFSOUND", 0));
            Assert.Throws<IndexOutOfRangeException>(() => _audioEngine.SetGlobalVariable("speedofsound", 0));

            // Make sure a reserved variable can be set.
            Assert.AreEqual(343.5f, _audioEngine.GetGlobalVariable("SpeedOfSound"));
            _audioEngine.SetGlobalVariable("SpeedOfSound", 1.0f);
            Assert.AreEqual(1.0f, _audioEngine.GetGlobalVariable("SpeedOfSound"));
            _audioEngine.SetGlobalVariable("SpeedOfSound", 343.5f);
            Assert.AreEqual(343.5f, _audioEngine.GetGlobalVariable("SpeedOfSound"));

            // Make sure a user variable can be set.
            Assert.AreEqual(12.34f, _audioEngine.GetGlobalVariable("Global Public"));
            _audioEngine.SetGlobalVariable("Global Public", 1.0f);
            Assert.AreEqual(1.0f, _audioEngine.GetGlobalVariable("Global Public"));

            // Make sure variable limits are working.
            _audioEngine.SetGlobalVariable("Global Public", -100.0f);
            Assert.AreEqual(0.0f, _audioEngine.GetGlobalVariable("Global Public"));
            _audioEngine.SetGlobalVariable("Global Public", 1000.0f);
            Assert.AreEqual(100.0f, _audioEngine.GetGlobalVariable("Global Public"));

            // Make sure instance variables can't be accessed.
            Assert.Throws<IndexOutOfRangeException>(() => _audioEngine.SetGlobalVariable("OrientationAngle", 1.0f));

            // Make sure private variables can't be accessed.
            Assert.Throws<IndexOutOfRangeException>(() => _audioEngine.SetGlobalVariable("Global Private", 1.0f));
        }

        [Test]
        public void SoundBankGetCue()
        {
            Assert.Throws<ArgumentNullException>(() => _soundBank.GetCue(null));
            Assert.Throws<ArgumentNullException>(() => _soundBank.GetCue(""));
            Assert.Throws<ArgumentException>(() => _soundBank.GetCue("DoesNotExist"));
            Assert.Throws<ArgumentException>(() => _soundBank.GetCue("BLAST_MONO"));

            var cue = _soundBank.GetCue("blast_mono");
            Assert.NotNull(cue);
            Assert.AreEqual("blast_mono", cue.Name);
            Assert.False(cue.IsDisposed);

            // Make sure the initial state is correct.
            Assert.False(cue.IsCreated);
            Assert.False(cue.IsPreparing);
            Assert.True(cue.IsPrepared);
            Assert.False(cue.IsPlaying);
            Assert.False(cue.IsPaused);
            Assert.False(cue.IsStopped);
            Assert.False(cue.IsStopping);

            cue.Play ();
            cue.Stop (AudioStopOptions.Immediate);

            cue = _soundBank.GetCue ("blast_mono");

            // Make sure the initial state is reset
            Assert.False(cue.IsCreated);
            Assert.False(cue.IsPreparing);
            Assert.True(cue.IsPrepared);
            Assert.False(cue.IsPlaying);
            Assert.False(cue.IsPaused);
            Assert.False(cue.IsStopped);
            Assert.False(cue.IsStopping);

            cue.Dispose();

            // Make sure the disposed state is correct.
            Assert.True(cue.IsDisposed);
            Assert.False(cue.IsCreated);
            Assert.False(cue.IsPreparing);
            Assert.False(cue.IsPrepared);
            Assert.False(cue.IsPlaying);
            Assert.False(cue.IsPaused);
            Assert.False(cue.IsStopped);
            Assert.False(cue.IsStopping);
        }

        [Test]
        public void SoundBankPlayCue()
        {
            Assert.Throws<ArgumentNullException>(() => _soundBank.PlayCue(null));
            Assert.Throws<ArgumentNullException>(() => _soundBank.PlayCue(""));
            Assert.Throws<InvalidOperationException>(() => _soundBank.PlayCue("DoesNotExist", null, null));
            Assert.Throws<ArgumentNullException>(() => _soundBank.PlayCue("blast_mono", null, null));
            Assert.Throws<ArgumentNullException>(() => _soundBank.PlayCue("blast_mono", new AudioListener(), null));

            // TODO: Add actual playback tests!
            //_soundBank.PlayCue("blast_mono");
        }

        [Test]
        public void CueGetVariable()
        {
            var cue = _soundBank.GetCue("blast_mono");

            Assert.Throws<ArgumentNullException>(() => cue.GetVariable(null));
            Assert.Throws<ArgumentNullException>(() => cue.GetVariable(""));
            Assert.Throws<IndexOutOfRangeException>(() => cue.GetVariable("DoesNotExist"));

            // Make sure case matters.
            Assert.Throws<IndexOutOfRangeException>(() => cue.GetVariable("DISTANCE"));
            Assert.Throws<IndexOutOfRangeException>(() => cue.GetVariable("distance"));

            Assert.AreEqual(0.0f, cue.GetVariable("Distance"));
            Assert.AreEqual(45.67f, cue.GetVariable("Cue Public"));
            
            // Make sure globbal variables can't be accessed.
            Assert.Throws<IndexOutOfRangeException>(() => cue.GetVariable("SpeedOfSound"));

            // Make sure private variables can't be accessed.
            Assert.Throws<IndexOutOfRangeException>(() => cue.GetVariable("Cue Private"));

            cue.Dispose();
        }

        [Test]
        public void CueSetVariable()
        {
            var cue = _soundBank.GetCue("blast_mono");

            Assert.Throws<ArgumentNullException>(() => cue.SetVariable(null, 0));
            Assert.Throws<ArgumentNullException>(() => cue.SetVariable("", 0));
            Assert.Throws<IndexOutOfRangeException>(() => cue.SetVariable("DoesNotExist", 0));

            // Make sure case matters.
            Assert.Throws<IndexOutOfRangeException>(() => cue.SetVariable("DISTANCE", 0));
            Assert.Throws<IndexOutOfRangeException>(() => cue.SetVariable("distance", 0));

            // Make sure a reserved variable can be set.
            Assert.AreEqual(0, cue.GetVariable("Distance"));
            cue.SetVariable("Distance", 1.0f);
            Assert.AreEqual(1.0f, cue.GetVariable("Distance"));
            cue.SetVariable("Distance", 0);
            Assert.AreEqual(0, cue.GetVariable("Distance"));

            // Make sure a user variable can be set.
            Assert.AreEqual(45.67f, cue.GetVariable("Cue Public"));
            cue.SetVariable("Cue Public", 1.0f);
            Assert.AreEqual(1.0f, cue.GetVariable("Cue Public"));

            // Make sure variable limits are working.
            cue.SetVariable("Cue Public", -100.0f);
            Assert.AreEqual(0.0f, cue.GetVariable("Cue Public"));
            cue.SetVariable("Cue Public", 1000.0f);
            Assert.AreEqual(100.0f, cue.GetVariable("Cue Public"));

            // Make sure global variables can't be accessed.
            Assert.Throws<IndexOutOfRangeException>(() => cue.SetVariable("Global Public", 1.0f));

            // Make sure private variables can't be accessed.
            Assert.Throws<IndexOutOfRangeException>(() => cue.SetVariable("Cue Private", 1.0f));

            cue.Dispose();
        }
    }
}