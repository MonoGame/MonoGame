// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;
using Microsoft.Xna.Framework.Audio;
using NUnit.Framework;

namespace MonoGame.Tests.Framework.Audio
{
    [TestFixture]
    public class XactTests
    {
        private AudioEngine _audioEngine;
        private SoundBank _soundBank;
        private WaveBank _waveBank;

        [TestFixtureSetUp]
        public void Setup()
        {
            _audioEngine = new AudioEngine(@"Assets\Audio\Win\Tests.xgs");
            _waveBank = new WaveBank(_audioEngine, @"Assets\Audio\Win\Tests.xwb");
            _soundBank = new SoundBank(_audioEngine, @"Assets\Audio\Win\Tests.xsb");

            Assert.False(_audioEngine.IsDisposed);
            Assert.False(_waveBank.IsDisposed);
            Assert.False(_soundBank.IsDisposed);
        }

        [TestFixtureTearDown]
        public void TearDown()
        {
            _soundBank.Dispose();
            _waveBank.Dispose();
            _audioEngine.Dispose();

            Assert.True(_audioEngine.IsDisposed);
            Assert.True(_waveBank.IsDisposed);
            Assert.True(_soundBank.IsDisposed);
        }

        [Test]
        public void AudioEngineCtor()
        {
            Assert.Throws<ArgumentNullException>(() => new AudioEngine(null));
            Assert.Throws<ArgumentNullException>(() => new AudioEngine(""));
            //Assert.Throws<DirectoryNotFoundException>(() => new AudioEngine(@"This\Does\Not\Exist.xgs"));
            Assert.Throws<FileNotFoundException>(() => new AudioEngine(@"Assets\Audio\Win\NotTheFile.xgs"));            
        }

        [Test]
        public void WaveBankCtor()
        {
            Assert.Throws<ArgumentNullException>(() => new WaveBank(null, null));
            Assert.Throws<ArgumentNullException>(() => new WaveBank(_audioEngine, null));
            Assert.Throws<ArgumentNullException>(() => new WaveBank(_audioEngine, ""));
            //Assert.Throws<DirectoryNotFoundException>(() => new WaveBank(_audioEngine, @"This\Does\Not\Exist.xwb"));
            Assert.Throws<FileNotFoundException>(() => new WaveBank(_audioEngine, @"Assets\Audio\Win\NotTheFile.xwb"));            
        }

        [Test]
        public void SoundBankCtor()
        {
            Assert.Throws<ArgumentNullException>(() => new SoundBank(null, null));
            Assert.Throws<ArgumentNullException>(() => new SoundBank(_audioEngine, null));
            Assert.Throws<ArgumentNullException>(() => new SoundBank(_audioEngine, ""));
            //Assert.Throws<DirectoryNotFoundException>(() => new SoundBank(_audioEngine, @"This\Does\Not\Exist.xsb"));
            Assert.Throws<FileNotFoundException>(() => new SoundBank(_audioEngine, @"Assets\Audio\Win\NotTheFile.xsb"));
        }

        [Test]
        public void ContentVersion()
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
    }
}