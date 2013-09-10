using System;
using NUnit.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework;
using System.IO;
using System.Threading;

namespace MonoGame.Tests {
    [TestFixture()]
    class OpenALAudioTest : AudioTestFixtureBase {
        DynamicSoundEffectInstance readWav (out int dataSize, out byte[] byteArray)
        {
            var waveFileStream = File.OpenRead (Path.Combine (Game.Content.RootDirectory, Paths.Audio ("song_sample.wav")));
            var reader = new BinaryReader (waveFileStream);
            int chunkID = reader.ReadInt32 ();
            int fileSize = reader.ReadInt32 ();
            int riffType = reader.ReadInt32 ();
            int fmtID = reader.ReadInt32 ();
            int fmtSize = reader.ReadInt32 ();
            int fmtCode = reader.ReadInt16 ();
            int channels = reader.ReadInt16 ();
            int sampleRate = reader.ReadInt32 ();
            int fmtAvgBPS = reader.ReadInt32 ();
            int fmtBlockAlign = reader.ReadInt16 ();
            int bitDepth = reader.ReadInt16 ();
            if (fmtSize == 18) {
                // Read any extra values
                int fmtExtraSize = reader.ReadInt16 ();
                reader.ReadBytes (fmtExtraSize);
            }
            int dataID = reader.ReadInt32 ();
            // 64 61 74 61
            while (dataID != 0x61746164) {
                dataID = reader.ReadInt32 ();
            }
            dataSize = reader.ReadInt32 ();
            byteArray = reader.ReadBytes (dataSize);
            return new DynamicSoundEffectInstance (sampleRate, (AudioChannels) channels);
        }

        [Test]
        public void playWholeClip()
        {
            int dataSize;
            byte[] byteArray;
            DynamicSoundEffectInstance dynamicSound = readWav(out dataSize, out byteArray);            
            
            int position = 0;
            int count = dynamicSound.GetSampleSizeInBytes (TimeSpan.FromMilliseconds (100));
            
            dynamicSound.BufferNeeded += new EventHandler<EventArgs> ((object sender, EventArgs e) => {
                // stop if we go over
                if (position + count > byteArray.Length) {
                    return;
                }

                dynamicSound.SubmitBuffer (byteArray, position, count / 2);
                dynamicSound.SubmitBuffer (byteArray, position + count / 2, count / 2);
            
                position += count;
            });
            
            dynamicSound.Play ();
            
            Thread.Sleep(dynamicSound.GetSampleDuration(dataSize) + TimeSpan.FromMilliseconds(100));
            dynamicSound.Stop();
        }

        [Test]
        public void playOneSecondLoopThrice()
        {
            int dataSize;
            byte[] byteArray;
            DynamicSoundEffectInstance dynamicSound = readWav(out dataSize, out byteArray);            
            
            int position = 0;
            int count = dynamicSound.GetSampleSizeInBytes (TimeSpan.FromMilliseconds (100));
            int oneSecondInBytes = dynamicSound.GetSampleSizeInBytes (TimeSpan.FromSeconds(1));
            
            dynamicSound.BufferNeeded += new EventHandler<EventArgs> ((object sender, EventArgs e) => {
                // loop if we go over
                if (position + count >= oneSecondInBytes) {
                    position = 0;
                }

                dynamicSound.SubmitBuffer (byteArray, position, count);

                position += count;
            });
            
            dynamicSound.Play ();
            
            Thread.Sleep(TimeSpan.FromSeconds(3) + TimeSpan.FromMilliseconds(100));
            dynamicSound.Stop();
        }
        
        [Test]
        public void PlayPauseAndResume()
        {
            int dataSize;
            byte[] byteArray;
            DynamicSoundEffectInstance dynamicSound = readWav(out dataSize, out byteArray);            
            
            int position = 0;
            int count = dynamicSound.GetSampleSizeInBytes (TimeSpan.FromMilliseconds (100));
            
            dynamicSound.BufferNeeded += new EventHandler<EventArgs> ((object sender, EventArgs e) => {
                // loop if we go over
                if (position + count >= dataSize) {
                    position = 0;
                }

                dynamicSound.SubmitBuffer (byteArray, position, count);

                // from original Microsoft implementation
                //dynamicSound.SubmitBuffer (byteArray, position, count / 2);
                //dynamicSound.SubmitBuffer (byteArray, position + count / 2, count / 2);
            
                position += count;
            });
            
            dynamicSound.Play ();
            Thread.Sleep(TimeSpan.FromSeconds(2));
            dynamicSound.Pause();
            Thread.Sleep(TimeSpan.FromSeconds(2));
            dynamicSound.Play ();
            Thread.Sleep(TimeSpan.FromSeconds(2));
            dynamicSound.Stop ();
        }

    }
}

