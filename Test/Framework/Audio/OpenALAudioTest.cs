using System;
using NUnit.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework;
using System.IO;
using System.Threading;

namespace MonoGame.Tests {
    [TestFixture()]
    class OpenALAudioTest : AudioTestFixtureBase {
//        [Test()]
//        public void TestCase ()
//        {
//            SoundEffect soundEffect = null;
//            
//            Game.LoadContentWith += (sender, e) => {
//                soundEffect = Game.Content.Load<SoundEffect> (Paths.Audio ("song_sample.wav"));
//            };
//
//            Game.UnloadContentWith += (sender, e) => {
//                soundEffect.Dispose ();
//            };
//            
//            
//            var soundEffectInstance = new DynamicSoundEffectInstance(44100, AudioChannels.Stereo);
//            
//            soundEffectInstance.BufferNeeded += (object sender, EventArgs e) => {
////                soundEffectInstance.SubmitBuffer(
//            };
//        }
//        
        [Test]
        public void microsoftSample ()
        {
            DynamicSoundEffectInstance dynamicSound;
            
            int position = 0;
            int count;
            byte [] byteArray;
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
            
            int dataID = reader.ReadInt32 (); // 64 61 74 61
            while (dataID != 0x61746164) {
                dataID = reader.ReadInt32 ();
            }
            
            int dataSize = reader.ReadInt32 ();
            
            byteArray = reader.ReadBytes (dataSize);
            
            dynamicSound = new DynamicSoundEffectInstance (sampleRate, (AudioChannels) channels);
            
            count = dynamicSound.GetSampleSizeInBytes (TimeSpan.FromMilliseconds (100));
            
            dynamicSound.BufferNeeded += new EventHandler<EventArgs> ((object sender, EventArgs e) => {
                // stop if we go over
                if (position + count > byteArray.Length) {
                    return;
                }

                dynamicSound.SubmitBuffer (byteArray, position, count);

                // from original Microsoft implementation
                //dynamicSound.SubmitBuffer (byteArray, position, count / 2);
                //dynamicSound.SubmitBuffer (byteArray, position + count / 2, count / 2);
            
                position += count;
            });
            
            dynamicSound.Play ();
            
            Thread.Sleep(dynamicSound.GetSampleDuration(dataSize) + TimeSpan.FromMilliseconds(100));
        }

    }
}

