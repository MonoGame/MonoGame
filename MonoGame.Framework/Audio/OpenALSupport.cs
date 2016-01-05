using System;
using System.Runtime.InteropServices;

#if IOS || (MONOMAC && !PLATFORM_MACOS_LEGACY)
#if IOS
using UIKit;
#else
using AppKit;
#endif
using Foundation;
using CoreFoundation;
using AudioToolbox;
using AudioUnit;

using OpenTK.Audio.OpenAL;

#elif MONOMAC

using MonoMac.AppKit;
using MonoMac.Foundation;
using MonoMac.CoreFoundation;
using MonoMac.AudioToolbox;
using MonoMac.AudioUnit;

using MonoMac.OpenAL;
#endif

namespace Microsoft.Xna.Framework.Audio
{
	internal static class OpenALSupport
	{
		public static ExtAudioFile GetExtAudioFile (NSUrl url, out AudioStreamBasicDescription audioDescription)
		{
			// Notice the following line that we can not pass a NSUrl to a CFUrl
			//ExtAudioFile ext = ExtAudioFile.OpenUrl(url);

			// Basic Descriptions
			AudioStreamBasicDescription fileFormat;
			AudioStreamBasicDescription outputFormat;

			// So now we create a CFUrl
			CFUrl curl = CFUrl.FromFile (url.Path);

			// Open the file
			ExtAudioFile ext = ExtAudioFile.OpenUrl (curl);

			// Get the audio format
			fileFormat = ext.FileDataFormat;

			// Don't know how to handle sounds with more than 2 channels (i.e. stereo)
			// Remember that OpenAL sound effects must be mono to be spatialized anyway.
			if (fileFormat.ChannelsPerFrame > 2) {
#if DEBUG				
				Console.WriteLine ("Unsupported Format: Channel count [0] is greater than stereo.", fileFormat.ChannelsPerFrame);
#endif
                audioDescription = new AudioStreamBasicDescription();
				return null;
			}

			// The output format must be linear PCM because that's the only type OpenAL knows how to deal with.
			// Set the client format to 16 bit signed integer (native-endian) data because that is the most
			// optimal format on iPhone/iPod Touch hardware.
			// Maintain the channel count and sample rate of the original source format.
			outputFormat = new AudioStreamBasicDescription ();	// Create our output format description to be converted to
			outputFormat.SampleRate = fileFormat.SampleRate;	// Preserve the original sample rate
			outputFormat.ChannelsPerFrame = fileFormat.ChannelsPerFrame;	// Preserve the orignal number of channels
			outputFormat.Format = AudioFormatType.LinearPCM;	// We want Linear PCM

			// IsBigEndian is causing some problems with distorted sounds on MacOSX
//			outputFormat.FormatFlags = AudioFormatFlags.IsBigEndian
//							| AudioFormatFlags.IsPacked
//							| AudioFormatFlags.IsSignedInteger;
			
			outputFormat.FormatFlags = AudioFormatFlags.IsPacked
							| AudioFormatFlags.IsSignedInteger;
			outputFormat.FramesPerPacket = 1;	// We know for linear PCM, the definition is 1 frame per packet
			outputFormat.BitsPerChannel = 16;	// We know we want 16-bit
			outputFormat.BytesPerPacket = 2 * outputFormat.ChannelsPerFrame;	// We know we are using 16-bit, so 2-bytes per channel per frame
			outputFormat.BytesPerFrame = 2 * outputFormat.ChannelsPerFrame;		// For PCM, since 1 frame is 1 packet, it is the same as mBytesPerPacket

			// Set the desired client (output) data format
			ext.ClientDataFormat = outputFormat;

			// Copy the output format to the audio description that was passed in so the
			// info will be returned to the user.
			audioDescription = outputFormat;

			return ext;
		}

		public static bool GetDataFromExtAudioFile (ExtAudioFile ext, AudioStreamBasicDescription outputFormat, int maxBufferSize,
		                                       byte[] dataBuffer, out int dataBufferSize, out ALFormat format, out double sampleRate)
		{
			uint errorStatus = 0;
			uint bufferSizeInFrames = 0;
			dataBufferSize = 0;
			format = ALFormat.Mono16;
			sampleRate = 0;
			/* Compute how many frames will fit into our max buffer size */
			bufferSizeInFrames = (uint)(maxBufferSize / outputFormat.BytesPerFrame);

			if (dataBuffer != null) {
				var audioBufferList = new AudioBuffers(maxBufferSize);

				// This a hack so if there is a problem speak to kjpou1 -Kenneth
				// the cleanest way is to copy the buffer to the pointer already allocated
				// but what we are going to do is replace the pointer with our own and restore it later
				//
				GCHandle meBePinned = GCHandle.Alloc (dataBuffer, GCHandleType.Pinned);
				IntPtr meBePointer = meBePinned.AddrOfPinnedObject ();

				audioBufferList.SetData (0, meBePointer);

				try {
					// Read the data into an AudioBufferList
					// errorStatus here returns back the amount of information read
					ExtAudioFileError extAudioFileError = ExtAudioFileError.OK;
					errorStatus = ext.Read (bufferSizeInFrames, audioBufferList, out extAudioFileError);
					if (errorStatus >= 0) {
						/* Success */
						/* Note: 0 == bufferSizeInFrames is a legitimate value meaning we are EOF. */

						/* ExtAudioFile.Read returns the number of frames actually read.
						 * Need to convert back to bytes.
						 */
						dataBufferSize = (int)bufferSizeInFrames * outputFormat.BytesPerFrame;

						// Now we set our format
						format = outputFormat.ChannelsPerFrame > 1 ? ALFormat.Stereo16 : ALFormat.Mono16;

						sampleRate = outputFormat.SampleRate;
					} else {
#if DEBUG						
						Console.WriteLine ("ExtAudioFile.Read failed, Error = " + errorStatus);
#endif
						return false;
					}
				} catch (Exception exc) {
#if DEBUG
					Console.WriteLine ("ExtAudioFile.Read failed: " + exc.Message);
#endif
					return false;
				} finally {
					// Don't forget to free our dataBuffer memory pointer that was pinned above
					meBePinned.Free ();
					// and restore what was allocated to beginwith
					audioBufferList.SetData (0, IntPtr.Zero);
				}


			}
			return true;
		}

		/**
		 * Returns a byte buffer containing all the pcm data.
		 */
		public static byte[] GetOpenALAudioDataAll (NSUrl file_url, out int dataBufferSize, out ALFormat alFormat, out double sampleRate, out double duration)
		{

			long fileLengthInFrames = 0;
			AudioStreamBasicDescription outputFormat;
			int maxBufferSize;
			byte[] pcmData;
			dataBufferSize = 0;
			alFormat = 0;
			sampleRate = 0;
			duration = 0;

			ExtAudioFile extFile;

			try {
				extFile = GetExtAudioFile (file_url, out outputFormat);
			} catch (Exception extExc) {
#if DEBUG				
				Console.WriteLine ("ExtAudioFile.OpenUrl failed, Error : " + extExc.Message);
#endif
				return null;
			}

			/* Get the total frame count */
			try {
				fileLengthInFrames = extFile.FileLengthFrames;
			} catch (Exception exc) {
#if DEBUG				
				Console.WriteLine ("ExtAudioFile.FileLengthFranes failed, Error : " + exc.Message);
#endif
				return null;
			}

			/* Compute the number of bytes needed to hold all the data in the file. */
			maxBufferSize = (int)(fileLengthInFrames * outputFormat.BytesPerFrame);
			/* Allocate memory to hold all the decoded PCM data. */
			pcmData = new byte[maxBufferSize];

			bool gotData = GetDataFromExtAudioFile (extFile, outputFormat, maxBufferSize, pcmData,
			                        out dataBufferSize, out alFormat, out sampleRate);

			if (!gotData) {
				pcmData = null;
			}

			duration = (dataBufferSize / ((outputFormat.BitsPerChannel / 8) * outputFormat.ChannelsPerFrame)) / outputFormat.SampleRate;

			// we probably should make sure the buffer sizes are in accordance.
			//	assert(maxBufferSize == dataBufferSize);

			// We do not need the ExtAudioFile so we will set it to null
			extFile = null;
			return pcmData;

		}

		public static byte[] LoadFromFile (string filename, out int dataBufferSize, out ALFormat alFormat, out double sampleRate, out double duration)
		{

			return OpenALSupport.GetOpenALAudioDataAll (NSUrl.FromFilename (filename),
			                                    out dataBufferSize, out alFormat, out sampleRate, out duration);
		}


	}
}

