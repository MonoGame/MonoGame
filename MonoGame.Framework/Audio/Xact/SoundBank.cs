// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;
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
            using (var soundbankstream = TitleContainer.OpenStream(fileName))
			{
#else
            using (var fileStream = Game.Activity.Assets.Open(fileName))
			{
				MemoryStream soundbankstream = new MemoryStream();
				fileStream.CopyTo(soundbankstream);
				soundbankstream.Position = 0;
#endif
                using (var soundbankreader = new BinaryReader(soundbankstream))
				{
	            
					//Parse the SoundBank.
					//Thanks to Liandril for "xactxtract" for some of the offsets
					
					uint magic = soundbankreader.ReadUInt32 ();
					if (magic != 0x4B424453) { //"SDBK"
						throw new Exception ("Bad soundbank format");
					}
					
                    soundbankreader.ReadUInt16 (); // toolVersion
					uint formatVersion = soundbankreader.ReadUInt16 ();
					if (formatVersion != 46) {
#if DEBUG
						System.Diagnostics.Debug.WriteLine("Warning: SoundBank format {0} not supported.", formatVersion);
#endif
					}
					
                    soundbankreader.ReadUInt16 (); // crc, TODO: Verify crc (FCS16)
					
                    soundbankreader.ReadUInt32 (); // lastModifiedLow
                    soundbankreader.ReadUInt32 (); // lastModifiedHigh
                    soundbankreader.ReadByte(); // platform ???
					
					uint numSimpleCues = soundbankreader.ReadUInt16 ();
					uint numComplexCues = soundbankreader.ReadUInt16 ();
					soundbankreader.ReadUInt16 (); //unkn
                    soundbankreader.ReadUInt16 (); // numTotalCues
					uint numWaveBanks = soundbankreader.ReadByte ();
                    soundbankreader.ReadUInt16 (); // numSounds
					uint cueNameTableLen = soundbankreader.ReadUInt16 ();
					soundbankreader.ReadUInt16 (); //unkn
					
					uint simpleCuesOffset = soundbankreader.ReadUInt32 ();
					uint complexCuesOffset = soundbankreader.ReadUInt32 (); //unkn
					uint cueNamesOffset = soundbankreader.ReadUInt32 ();
					soundbankreader.ReadUInt32 (); //unkn
                    soundbankreader.ReadUInt32 (); // variationTablesOffset
					soundbankreader.ReadUInt32 (); //unkn
					uint waveBankNameTableOffset = soundbankreader.ReadUInt32 ();
                    soundbankreader.ReadUInt32 (); // cueNameHashTableOffset
                    soundbankreader.ReadUInt32 (); // cueNameHashValsOffset
                    soundbankreader.ReadUInt32 (); // soundsOffset
					
                    //name = System.Text.Encoding.UTF8.GetString(soundbankreader.ReadBytes(64),0,64).Replace("\0","");

					//parse wave bank name table
					soundbankstream.Seek (waveBankNameTableOffset, SeekOrigin.Begin);
                    _waveBanks = new WaveBank[numWaveBanks];
                    _waveBankNames = new string[numWaveBanks];
					for (int i=0; i<numWaveBanks; i++)
                        _waveBankNames[i] = System.Text.Encoding.UTF8.GetString(soundbankreader.ReadBytes(64), 0, 64).Replace("\0", "");
					
					//parse cue name table
					soundbankstream.Seek (cueNamesOffset, SeekOrigin.Begin);
					string[] cueNames = System.Text.Encoding.UTF8.GetString(soundbankreader.ReadBytes((int)cueNameTableLen), 0, (int)cueNameTableLen).Split('\0');

                    // Simple cues
					soundbankstream.Seek (simpleCuesOffset, SeekOrigin.Begin);
					for (int i=0; i<numSimpleCues; i++) 
                    {
                        soundbankreader.ReadByte (); // flags
						uint soundOffset = soundbankreader.ReadUInt32 ();

                        var oldPosition = soundbankreader.BaseStream.Position;
                        soundbankreader.BaseStream.Seek(soundOffset, SeekOrigin.Begin);

						XactSound sound = new XactSound(this, soundbankreader);
						Cue cue = new Cue(_audioengine, cueNames[i], sound);						
						
						_cues.Add(cue.Name, cue);

                        soundbankreader.BaseStream.Seek(oldPosition, SeekOrigin.Begin);
					}
					
                    // Complex cues
					soundbankstream.Seek (complexCuesOffset, SeekOrigin.Begin);
					for (int i=0; i<numComplexCues; i++) 
                    {
						Cue cue;
						
						byte flags = soundbankreader.ReadByte ();
						if (((flags >> 2) & 1) != 0) {
							//not sure :/
							uint soundOffset = soundbankreader.ReadUInt32 ();
							soundbankreader.ReadUInt32 (); //unkn

                            var oldPosition = soundbankreader.BaseStream.Position;
                            soundbankreader.BaseStream.Seek(soundOffset, SeekOrigin.Begin);

							XactSound sound = new XactSound(this, soundbankreader);
							cue = new Cue(_audioengine, cueNames[numSimpleCues+i], sound);

                            soundbankreader.BaseStream.Seek(oldPosition, SeekOrigin.Begin);
						} else {
							uint variationTableOffset = soundbankreader.ReadUInt32 ();
                            soundbankreader.ReadUInt32 (); // transitionTableOffset
							
							//parse variation table
							long savepos = soundbankstream.Position;
							soundbankstream.Seek (variationTableOffset, SeekOrigin.Begin);
							
							uint numEntries = soundbankreader.ReadUInt16 ();
							uint variationflags = soundbankreader.ReadUInt16 ();
							soundbankreader.ReadByte ();
							soundbankreader.ReadUInt16 ();
							soundbankreader.ReadByte ();
							
							XactSound[] cueSounds = new XactSound[numEntries];
							float[] probs = new float[numEntries];
							
							uint tableType = (variationflags >> 3) & 0x7;
							for (int j=0; j<numEntries; j++) {
								switch (tableType) {
								case 0: //Wave
								{
									int trackIndex = soundbankreader.ReadUInt16 ();
                                    int waveBankIndex = soundbankreader.ReadByte();
                                    soundbankreader.ReadByte (); // weightMin
                                    soundbankreader.ReadByte (); // weightMax
			
									cueSounds[j] = new XactSound(this, waveBankIndex, trackIndex);
									break;
								}
								case 1:
								{
									uint soundOffset = soundbankreader.ReadUInt32 ();
                                    soundbankreader.ReadByte (); // weightMin
                                    soundbankreader.ReadByte (); // weightMax

                                    var oldPosition = soundbankreader.BaseStream.Position;
                                    soundbankreader.BaseStream.Seek(soundOffset, SeekOrigin.Begin);

									cueSounds[j] = new XactSound(this, soundbankreader);

                                    soundbankreader.BaseStream.Seek(oldPosition, SeekOrigin.Begin);
									break;
								}
								case 4: //CompactWave
								{
                                    int trackIndex = soundbankreader.ReadUInt16();
                                    int waveBankIndex = soundbankreader.ReadByte();
                                    cueSounds[j] = new XactSound(this, waveBankIndex, trackIndex);
									break;
								}
								default:
									throw new NotSupportedException();
								}
							}
							
							soundbankstream.Seek (savepos, SeekOrigin.Begin);
							
							cue = new Cue(_audioengine, cueNames[numSimpleCues+i], cueSounds, probs);
						}
						
						//Instance Limit
						soundbankreader.ReadUInt32 ();
						soundbankreader.ReadByte ();
						soundbankreader.ReadByte ();
						
						_cues.Add(cue.Name, cue);
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
		
        /*
		public void PlayCue (string name, AudioListener listener, AudioEmitter emitter)
		{
			throw new NotImplementedException();
		}
        */

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

