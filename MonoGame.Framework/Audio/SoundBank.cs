#region License
/*
MIT License
Copyright © 2006 The Mono.Xna Team

All rights reserved.

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/
#endregion License

using System;
using System.IO;
using System.Collections.Generic;

namespace Microsoft.Xna.Framework.Audio
{
    public class SoundBank : IDisposable
    {
        string name;
		string filename;
		AudioEngine audioengine;
		WaveBank[] waveBanks;
		Dictionary<string, Cue> cues = new Dictionary<string, Cue>();
        
		bool loaded = false;
		
        public SoundBank(AudioEngine audioEngine, string fileName)
        {
            // Check for windows-style directory separator character
            filename = fileName.Replace('\\',Path.DirectorySeparatorChar);
			audioengine = audioEngine;
		}
		
		//Defer loading because some programs load soundbanks before wavebanks
		private void Load() 
        {	
			FileStream soundbankstream = new FileStream(filename, FileMode.Open);
            BinaryReader soundbankreader = new BinaryReader(soundbankstream);
            
			//Parse the SoundBank.
			//Thanks to Liandril for "xactxtract" for some of the offsets
			
			uint magic = soundbankreader.ReadUInt32 ();
			if (magic != 0x4B424453) { //"SDBK"
				throw new Exception("Bad soundbank format");
			}
			
			uint toolVersion = soundbankreader.ReadUInt16 ();
			uint formatVersion = soundbankreader.ReadUInt16 ();
			if (formatVersion != 46) {
#if DEBUG
				Console.WriteLine ("Warning: SoundBank format not supported");
#endif
			}
			
			uint crc = soundbankreader.ReadUInt16 ();
			//TODO: Verify crc (FCS16)
			
			soundbankreader.ReadBytes(8); //unkn
			uint platform = soundbankreader.ReadByte(); //???
			
			uint numSimpleCues = soundbankreader.ReadUInt16 ();
			uint numComplexCues = soundbankreader.ReadUInt16 ();
			soundbankreader.ReadUInt16 (); //unkn
			uint numTotalCues = soundbankreader.ReadUInt16 ();
			uint numWaveBanks = soundbankreader.ReadByte ();
			uint numSounds = soundbankreader.ReadUInt16 ();
			uint cueNameTableLen = soundbankreader.ReadUInt16 ();
			soundbankreader.ReadUInt16 (); //unkn
			
			uint simpleCuesOffset = soundbankreader.ReadUInt32 ();
			uint complexCuesOffset = soundbankreader.ReadUInt32 (); //unkn
			uint cueNamesOffset = soundbankreader.ReadUInt32 ();
			soundbankreader.ReadUInt32 (); //unkn
			uint variationTablesOffset = soundbankreader.ReadUInt32 ();
			soundbankreader.ReadUInt32 (); //unkn
			uint waveBankNameTableOffset = soundbankreader.ReadUInt32 ();
			uint cueNameHashTableOffset = soundbankreader.ReadUInt32 ();
			uint cueNameHashValsOffset = soundbankreader.ReadUInt32 ();
			uint soundsOffset = soundbankreader.ReadUInt32 ();
			
			name = System.Text.Encoding.UTF8.GetString(soundbankreader.ReadBytes(64)).Replace("\0","");
			
			
			//parse wave bank name table
			soundbankstream.Seek (waveBankNameTableOffset, SeekOrigin.Begin);
			waveBanks = new WaveBank[numWaveBanks];
			for (int i=0; i<numWaveBanks; i++) {
				string bankname = System.Text.Encoding.UTF8.GetString(soundbankreader.ReadBytes(64)).Replace("\0","");
				waveBanks[i] = audioengine.Wavebanks[bankname];
			}
			
			//parse cue name table
			soundbankstream.Seek (cueNamesOffset, SeekOrigin.Begin);
			string[] cueNames = System.Text.Encoding.UTF8.GetString(soundbankreader.ReadBytes((int)cueNameTableLen)).Split('\0');
			soundbankstream.Seek (simpleCuesOffset, SeekOrigin.Begin);
			for (int i=0; i<numSimpleCues; i++) {
				byte flags = soundbankreader.ReadByte ();
				uint soundOffset = soundbankreader.ReadUInt32 ();
				XactSound sound = new XactSound(this, soundbankreader, soundOffset);
				Cue cue = new Cue(cueNames[i], sound);
				
				cues.Add(cue.Name, cue);
			}
			
			soundbankstream.Seek (complexCuesOffset, SeekOrigin.Begin);
			for (int i=0; i<numComplexCues; i++) {
				Cue cue;
				
				byte flags = soundbankreader.ReadByte ();
				if (((flags >> 2) & 1) != 0) {
					//not sure :/
					uint soundOffset = soundbankreader.ReadUInt32 ();
					soundbankreader.ReadUInt32 (); //unkn
					
					XactSound sound = new XactSound(this, soundbankreader, soundOffset);
					cue = new Cue(cueNames[numSimpleCues+i], sound);
				} else {
					uint variationTableOffset = soundbankreader.ReadUInt32 ();
					uint transitionTableOffset = soundbankreader.ReadUInt32 ();
					
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
							uint trackIndex = soundbankreader.ReadUInt16 ();
							byte waveBankIndex = soundbankreader.ReadByte ();
							byte weightMin = soundbankreader.ReadByte ();
							byte weightMax = soundbankreader.ReadByte ();
	
							cueSounds[j] = new XactSound(this.GetWave(waveBankIndex, trackIndex));
							break;
						}
						case 1:
						{
							uint soundOffset = soundbankreader.ReadUInt32 ();
							byte weightMin = soundbankreader.ReadByte ();
							byte weightMax = soundbankreader.ReadByte ();
							
							cueSounds[j] = new XactSound(this, soundbankreader, soundOffset);
							break;
						}
						case 4: //CompactWave
						{
							uint trackIndex = soundbankreader.ReadUInt16 ();
							byte waveBankIndex = soundbankreader.ReadByte ();
							cueSounds[j] = new XactSound(this.GetWave(waveBankIndex, trackIndex));
							break;
						}
						default:
							throw new NotImplementedException();
						}
					}
					
					soundbankstream.Seek (savepos, SeekOrigin.Begin);
					
					cue = new Cue(cueNames[numSimpleCues+i], cueSounds, probs);
				}
				
				//Instance Limit
				soundbankreader.ReadUInt32 ();
				soundbankreader.ReadByte ();
				soundbankreader.ReadByte ();
				
				cues.Add(cue.Name, cue);
			}
			
			soundbankreader.Close ();
			soundbankstream.Close ();
			
			loaded = true;
        }
		
		internal SoundEffectInstance GetWave(byte waveBankIndex, uint trackIndex) {
			return waveBanks[waveBankIndex].sounds[trackIndex];
		}
		
        public Cue GetCue(string name)
        {
			if (!loaded) Load ();
			
			//Does this have to return /new/ Cue instances?
			return cues[name];
        }
		
		public void PlayCue(string name)
		{
			var musicCue = GetCue(name);
            musicCue.Play();
		}
		
		public void PlayCue (string name, AudioListener listener, AudioEmitter emitter)
		{
			throw new NotImplementedException();
		}

		#region IDisposable implementation
		public void Dispose ()
		{
			throw new NotImplementedException ();
		}
		#endregion
    }
}

