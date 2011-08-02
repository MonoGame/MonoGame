using System;
using System.IO;
using OpenTK.Audio.OpenAL;

namespace Microsoft.Xna.Framework.Audio
{
	internal class AudioLoader
	{
		private AudioLoader ()
		{
		}
	
		private static ALFormat GetSoundFormat(int channels, int bits)
        {
            switch (channels)
            {
                case 1: return bits == 8 ? OpenTK.Audio.OpenAL.ALFormat.Mono8 : OpenTK.Audio.OpenAL.ALFormat.Mono16;
                case 2: return bits == 8 ? OpenTK.Audio.OpenAL.ALFormat.Stereo8 : OpenTK.Audio.OpenAL.ALFormat.Stereo16;
                default: throw new NotSupportedException("The specified sound format is not supported.");
            }
        }

		
		public static byte[] Load (Stream data, out ALFormat format, out int size, out int frequency)
		{
			byte[] audioData = null;
			format = ALFormat.Mono8;
			size = 0;
			frequency = 0;
			
			using (BinaryReader reader = new BinaryReader(data))
			{
				// decide which data type is this
				
				// for now we'll only support wave files
				audioData = LoadWave(reader, out format, out size, out frequency);
			}
			
			return audioData;
		}
		
		private static byte[] LoadWave (BinaryReader reader, out ALFormat format, out int size, out int frequency)
		{
			// CODE EXTRACTED FROM http://freeminerdotnet.googlecode.com/svn/trunk/FreeMiner.Plugin.Audio.OpenAL/AudioManagerOpenAL.cs
			
			byte[] audioData;
			
			//header
            string signature = new string(reader.ReadChars(4));
            if (signature != "RIFF")
            {
                throw new NotSupportedException("Specified stream is not a wave file.");
            }

            int riff_chunck_size = reader.ReadInt32();

            string wformat = new string(reader.ReadChars(4));
            if (wformat != "WAVE")
            {
                throw new NotSupportedException("Specified stream is not a wave file.");
            }

            // WAVE header
            string format_signature = new string(reader.ReadChars(4));
            if (format_signature != "fmt ")
            {
                throw new NotSupportedException("Specified wave file is not supported.");
            }

            int format_chunk_size = reader.ReadInt32();
            int audio_format = reader.ReadInt16();
            int num_channels = reader.ReadInt16();
            int sample_rate = reader.ReadInt32();
            int byte_rate = reader.ReadInt32();
            int block_align = reader.ReadInt16();
            int bits_per_sample = reader.ReadInt16();

            string data_signature = new string(reader.ReadChars(4));
            if (data_signature != "data")
            {
                throw new NotSupportedException("Specified wave file is not supported.");
            }

            int data_chunk_size = reader.ReadInt32();

            frequency = sample_rate;
			format = GetSoundFormat(num_channels, bits_per_sample);
			audioData = reader.ReadBytes((int)reader.BaseStream.Length);
			size = audioData.Length;
			
            return audioData;
		}
	}
}

