// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Utilities;

namespace Microsoft.Xna.Framework.Audio
{
    /// <summary>Represents a collection of Cues.</summary>
    public class SoundBank : IDisposable
    {
        readonly AudioEngine _audioengine;
        readonly string[] _waveBankNames;
        readonly WaveBank[] _waveBanks;
        readonly Dictionary<string, Cue> _cues = new Dictionary<string, Cue>();
        
        public bool IsDisposed { get; private set; }

        internal AudioEngine AudioEngine { get { return _audioengine; } }
		
        /// <param name="audioEngine">AudioEngine that will be associated with this sound bank.</param>
        /// <param name="fileName">Path to a .xsb SoundBank file.</param>
        public SoundBank(AudioEngine audioEngine, string fileName)
        {
            _audioengine = audioEngine;
            fileName = FileHelpers.NormalizeFilePathSeparators(fileName);

#if !ANDROID
            using (var stream = TitleContainer.OpenStream(fileName))
            {
#else
            using (var fileStream = Game.Activity.Assets.Open(fileName))
            {
                MemoryStream stream = new MemoryStream();
                fileStream.CopyTo(stream);
                stream.Position = 0;
#endif
                using (var reader = new BinaryReader(stream))
				{
                    // Thanks to Liandril for "xactxtract" for some of the offsets.

                    uint magic = reader.ReadUInt32();
                    if (magic != 0x4B424453) //"SDBK"
                        throw new Exception ("Bad soundbank format");

                    reader.ReadUInt16(); // toolVersion

                    uint formatVersion = reader.ReadUInt16();
                    // TODO: This does not match XNA, XNA uses 43.
                    if (formatVersion != 46) 
                        Debug.WriteLine("Warning: SoundBank format {0} not supported.", formatVersion);

                    reader.ReadUInt16(); // crc, TODO: Verify crc (FCS16)

                    reader.ReadUInt32(); // lastModifiedLow
                    reader.ReadUInt32(); // lastModifiedHigh
                    reader.ReadByte(); // platform ???

                    uint numSimpleCues = reader.ReadUInt16();
                    uint numComplexCues = reader.ReadUInt16();
                    reader.ReadUInt16(); //unkn
                    reader.ReadUInt16(); // numTotalCues
                    uint numWaveBanks = reader.ReadByte();
                    reader.ReadUInt16(); // numSounds
                    uint cueNameTableLen = reader.ReadUInt16();
                    reader.ReadUInt16(); //unkn

                    uint simpleCuesOffset = reader.ReadUInt32();
                    uint complexCuesOffset = reader.ReadUInt32(); //unkn
                    uint cueNamesOffset = reader.ReadUInt32();
                    reader.ReadUInt32(); //unkn
                    reader.ReadUInt32(); // variationTablesOffset
                    reader.ReadUInt32(); //unkn
                    uint waveBankNameTableOffset = reader.ReadUInt32();
                    reader.ReadUInt32(); // cueNameHashTableOffset
                    reader.ReadUInt32(); // cueNameHashValsOffset
                    reader.ReadUInt32(); // soundsOffset
					
                    //name = System.Text.Encoding.UTF8.GetString(soundbankreader.ReadBytes(64),0,64).Replace("\0","");

					//parse wave bank name table
                    stream.Seek(waveBankNameTableOffset, SeekOrigin.Begin);
                    _waveBanks = new WaveBank[numWaveBanks];
                    _waveBankNames = new string[numWaveBanks];
					for (int i=0; i<numWaveBanks; i++)
                        _waveBankNames[i] = System.Text.Encoding.UTF8.GetString(reader.ReadBytes(64), 0, 64).Replace("\0", "");
					
					//parse cue name table
					stream.Seek(cueNamesOffset, SeekOrigin.Begin);
                    string[] cueNames = System.Text.Encoding.UTF8.GetString(reader.ReadBytes((int)cueNameTableLen), 0, (int)cueNameTableLen).Split('\0');

                    // Simple cues
                    if (numSimpleCues > 0)
                    {
                        stream.Seek(simpleCuesOffset, SeekOrigin.Begin);
                        for (int i = 0; i < numSimpleCues; i++)
                        {
                            reader.ReadByte(); // flags
                            uint soundOffset = reader.ReadUInt32();

                            var oldPosition = stream.Position;
                            stream.Seek(soundOffset, SeekOrigin.Begin);
                            XactSound sound = new XactSound(this, reader);
                            stream.Seek(oldPosition, SeekOrigin.Begin);

                            Cue cue = new Cue(_audioengine, cueNames[i], sound);
                            _cues.Add(cue.Name, cue);
                        }
                    }
					
                    // Complex cues
                    if (numComplexCues > 0)
                    {
                        stream.Seek(complexCuesOffset, SeekOrigin.Begin);
                        for (int i = 0; i < numComplexCues; i++)
                        {
                            Cue cue;

                            byte flags = reader.ReadByte();
                            if (((flags >> 2) & 1) != 0)
                            {
                                uint soundOffset = reader.ReadUInt32();
                                reader.ReadUInt32(); //unkn

                                var oldPosition = stream.Position;
                                stream.Seek(soundOffset, SeekOrigin.Begin);
                                XactSound sound = new XactSound(this, reader);
                                stream.Seek(oldPosition, SeekOrigin.Begin);

                                cue = new Cue(_audioengine, cueNames[numSimpleCues + i], sound);
                            }
                            else
                            {
                                uint variationTableOffset = reader.ReadUInt32();
                                reader.ReadUInt32(); // transitionTableOffset

                                //parse variation table
                                long savepos = stream.Position;
                                stream.Seek(variationTableOffset, SeekOrigin.Begin);

                                uint numEntries = reader.ReadUInt16();
                                uint variationflags = reader.ReadUInt16();
                                reader.ReadByte();
                                reader.ReadUInt16();
                                reader.ReadByte();

                                XactSound[] cueSounds = new XactSound[numEntries];
                                float[] probs = new float[numEntries];

                                uint tableType = (variationflags >> 3) & 0x7;
                                for (int j = 0; j < numEntries; j++)
                                {
                                    switch (tableType)
                                    {
                                        case 0: //Wave
                                            {
                                                int trackIndex = reader.ReadUInt16();
                                                int waveBankIndex = reader.ReadByte();
                                                reader.ReadByte(); // weightMin
                                                reader.ReadByte(); // weightMax

                                                cueSounds[j] = new XactSound(this, waveBankIndex, trackIndex);
                                                break;
                                            }
                                        case 1:
                                            {
                                                uint soundOffset = reader.ReadUInt32();
                                                reader.ReadByte(); // weightMin
                                                reader.ReadByte(); // weightMax

                                                var oldPosition = stream.Position;
                                                stream.Seek(soundOffset, SeekOrigin.Begin);
                                                cueSounds[j] = new XactSound(this, reader);
                                                stream.Seek(oldPosition, SeekOrigin.Begin);
                                                break;
                                            }
                                        case 3:
                                            {
                                                uint soundOffset = reader.ReadUInt32();
                                                reader.ReadSingle(); // weightMin
                                                reader.ReadSingle(); // weightMax
                                                reader.ReadUInt32(); // flags

                                                var oldPosition = stream.Position;
                                                stream.Seek(soundOffset, SeekOrigin.Begin);
                                                cueSounds[j] = new XactSound(this, reader);
                                                stream.Seek(oldPosition, SeekOrigin.Begin);
                                                break;
                                            }
                                        case 4: //CompactWave
                                            {
                                                int trackIndex = reader.ReadUInt16();
                                                int waveBankIndex = reader.ReadByte();
                                                cueSounds[j] = new XactSound(this, waveBankIndex, trackIndex);
                                                break;
                                            }
                                        default:
                                            throw new NotSupportedException();
                                    }
                                }

                                stream.Seek(savepos, SeekOrigin.Begin);

                                cue = new Cue(_audioengine, cueNames[numSimpleCues + i], cueSounds, probs);
                            }

                            // Instance limiting
                            reader.ReadByte(); //instanceLimit
                            reader.ReadUInt16(); //fadeInSec, divide by 1000.0f
                            reader.ReadUInt16(); //fadeOutSec, divide by 1000.0f
                            reader.ReadByte(); //instanceFlags

                            _cues.Add(cue.Name, cue);
                        }
                    }
				}
			}
        }

        internal SoundEffectInstance GetSoundEffectInstance(int waveBankIndex, int trackIndex)
        {
            var waveBank = _waveBanks[waveBankIndex];

            // If the wave bank has not been resolved then do so now.
            if (waveBank == null)
            {
                var name = _waveBankNames[waveBankIndex];
                if (!_audioengine.Wavebanks.TryGetValue(name, out waveBank))
                    throw new Exception("The wave bank '" + name + "' was not found!");
                _waveBanks[waveBankIndex] = waveBank;                
            }

            var sound = waveBank.GetSoundEffect(trackIndex);
            return sound.GetPooledInstance(true);
        }
		
        /// <summary>
        /// Returns a pooled Cue object.
        /// </summary>
        /// <param name="name">Friendly name of the cue to get.</param>
        /// <returns>a unique Cue object from a pool.</returns>
        /// <remarks>
        /// <para>Cue instances are unique, even when sharing the same name. This allows multiple instances to simultaneously play.</para>
        /// </remarks>
        public Cue GetCue(string name)
        {
            // Note: In XNA this returns a new Cue instance, but that
            // generates garbage which is one reason to not do it.
            return _cues[name];
        }
		
        /// <summary>
        /// Plays a cue.
        /// </summary>
        /// <param name="name">Name of the cue to play.</param>
		public void PlayCue(string name)
		{
			var cue = GetCue(name);
            cue.Play();
		}

        /// <summary>
        /// Plays a cue with static 3D positional information.
        /// </summary>
        /// <remarks>
        /// Commonly used for short lived effects.  To dynamically change the 3D 
        /// positional information on a cue over time use <see cref="GetCue"/> and <see cref="Cue.Apply3D"/>.</remarks>
        /// <param name="name">The name of the cue to play.</param>
        /// <param name="listener">The listener state.</param>
        /// <param name="emitter">The cue emitter state.</param>
        public void PlayCue(string name, AudioListener listener, AudioEmitter emitter)
        {
            var cue = GetCue(name);
            cue.Play();
            cue.Apply3D(listener, emitter);
        }

        #region IDisposable implementation
        /// <summary>
        /// Immediately releases any unmanaged resources used by this object.
        /// </summary>
        public void Dispose()
        {
            if (IsDisposed)
                return;

            foreach (var cue in _cues.Values)
                cue.Dispose();

            IsDisposed = true;
        }
		#endregion
    }
}

