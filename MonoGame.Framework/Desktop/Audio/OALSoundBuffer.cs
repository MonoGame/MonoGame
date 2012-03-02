using System;

#if IPHONE || WINDOWS || LINUX
using OpenTK.Audio.OpenAL;
#elif MONOMAC
using MonoMac.OpenAL;
#endif


namespace Microsoft.Xna.Framework.Audio
{
	internal class OALSoundBuffer : IDisposable
	{
		int openALDataBuffer;
		internal byte[] pcmDataBuffer;
		ALFormat openALFormat;
		int dataSize;
		int sampleRate;
		private int _sourceId;

		public OALSoundBuffer ()
		{
			ALError alError;
			
			alError = AL.GetError ();
			AL.GenBuffers (1, out openALDataBuffer);
			alError = AL.GetError ();
			if (alError != ALError.NoError) {
				Console.WriteLine ("Failed to generate OpenAL data buffer: ", AL.GetErrorString (alError));
			}
		}

		public int OpenALDataBuffer {
			get {
				return openALDataBuffer;
			}
		}

		public double Duration {
			get;
			set;
		}

		public void BindDataBuffer (byte[] dataBuffer, ALFormat format, int size, int sampleRate)
		{
			pcmDataBuffer = dataBuffer;
			openALFormat = format;
			dataSize = size;
			this.sampleRate = sampleRate;
			AL.BufferData (openALDataBuffer, openALFormat, pcmDataBuffer, dataSize, this.sampleRate);

			int bits, channels;

			AL.GetBuffer (openALDataBuffer, ALGetBufferi.Bits, out bits);
			AL.GetBuffer (openALDataBuffer, ALGetBufferi.Channels, out channels);

			ALError alError = AL.GetError ();
			if (alError != ALError.NoError) {
				Console.WriteLine ("Failed to get buffer attributes: ", AL.GetErrorString (alError));
				Duration = -1;
			} else {
				Duration = (float)(size / ((bits / 8) * channels)) / (float)sampleRate;
			}

			//Console.WriteLine("Duration: " + Duration + " / size: " + size + " bits: " + bits + " channels: " + channels + " rate: " + sampleRate);

		}

		public void Dispose ()
		{
			CleanUpBuffer ();
		}

		public void CleanUpBuffer ()
		{
			if (AL.IsBuffer (openALDataBuffer)) {
				AL.DeleteBuffers (1, ref openALDataBuffer);
			}

			pcmDataBuffer = null;

		}

		public int SourceId
		{
			get {
				return _sourceId;
			}

			set {
				_sourceId = value;
				if (Reserved != null)
					Reserved(this, EventArgs.Empty);

			}
		}

		public void RecycleSoundBuffer()
		{
			if (Recycled != null)
				Recycled(this, EventArgs.Empty);
		}

		#region Events
		public event EventHandler<EventArgs> Reserved;
		public event EventHandler<EventArgs> Recycled;
		#endregion
	}
}

