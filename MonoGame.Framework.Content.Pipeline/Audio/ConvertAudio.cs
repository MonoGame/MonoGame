using System;
using MonoMac.AudioToolbox;
using System.Runtime.InteropServices;
using MonoMac.CoreFoundation;
using MonoMac.Foundation;

namespace Microsoft.Xna.Framework.Content.Pipeline
{
	public class ConvertAudio
	{
		static AudioFileIO afio;

		public static bool Convert(string input, string output, AudioFormatType targetFormat, AudioFileType containerType, Microsoft.Xna.Framework.Content.Pipeline.Audio.ConversionQuality quality) {
			CFUrl source = CFUrl.FromFile (input);
			CFUrl dest = CFUrl.FromFile (output);
			var dstFormat = new AudioStreamBasicDescription ();
			var sourceFile = AudioFile.Open (source, AudioFilePermission.Read);
			AudioFormatType outputFormat = targetFormat;
			// get the source data format
			var srcFormat = (AudioStreamBasicDescription)sourceFile.DataFormat;
			var outputSampleRate = 0;
			switch (quality)
			{
			case Microsoft.Xna.Framework.Content.Pipeline.Audio.ConversionQuality.Low:
				outputSampleRate = (int)Math.Max (8000, srcFormat.SampleRate / 2);
					break;
			default:
				outputSampleRate = (int)Math.Max (8000, srcFormat.SampleRate);
				break;
			}

			dstFormat.SampleRate = (outputSampleRate == 0 ? srcFormat.SampleRate : outputSampleRate); // set sample rate
			if (outputFormat == AudioFormatType.LinearPCM) {
				// if the output format is PC create a 16-bit int PCM file format description as an example
				dstFormat.Format = outputFormat;
				dstFormat.ChannelsPerFrame = srcFormat.ChannelsPerFrame;
				dstFormat.BitsPerChannel = 16;
				dstFormat.BytesPerPacket = dstFormat.BytesPerFrame = 2 * dstFormat.ChannelsPerFrame;
				dstFormat.FramesPerPacket = 1;
				dstFormat.FormatFlags = AudioFormatFlags.LinearPCMIsPacked | AudioFormatFlags.LinearPCMIsSignedInteger;
			} else {
				// compressed format - need to set at least format, sample rate and channel fields for kAudioFormatProperty_FormatInfo
				dstFormat.Format = outputFormat;
				dstFormat.ChannelsPerFrame = (outputFormat == AudioFormatType.iLBC ? 1 : srcFormat.ChannelsPerFrame); // for iLBC num channels must be 1

				// use AudioFormat API to fill out the rest of the description
				var fie = AudioStreamBasicDescription.GetFormatInfo (ref dstFormat);
				if (fie != AudioFormatError.None) {
					return false;
				}
			}

			var converter = AudioConverter.Create (srcFormat, dstFormat);
			converter.InputData += HandleInputData;

			// if the source has a cookie, get it and set it on the Audio Converter
			ReadCookie (sourceFile, converter);

			// get the actual formats back from the Audio Converter
			srcFormat = converter.CurrentInputStreamDescription;
			dstFormat = converter.CurrentOutputStreamDescription;

			// if encoding to AAC set the bitrate to 192k which is a nice value for this demo
			// kAudioConverterEncodeBitRate is a UInt32 value containing the number of bits per second to aim for when encoding data
			if (dstFormat.Format == AudioFormatType.MPEG4AAC) {
				uint outputBitRate = 192000; // 192k

				// ignore errors as setting may be invalid depending on format specifics such as samplerate
				try {
					converter.EncodeBitRate = outputBitRate;
				} catch {
				}

				// get it back and print it out
				outputBitRate = converter.EncodeBitRate;
			}

			// create the destination file 
			var destinationFile = AudioFile.Create (dest, containerType, dstFormat, AudioFileFlags.EraseFlags);

			// set up source buffers and data proc info struct
			afio = new AudioFileIO (32768);
			afio.SourceFile = sourceFile;
			afio.SrcFormat = srcFormat;

			if (srcFormat.BytesPerPacket == 0) {
				// if the source format is VBR, we need to get the maximum packet size
				// use kAudioFilePropertyPacketSizeUpperBound which returns the theoretical maximum packet size
				// in the file (without actually scanning the whole file to find the largest packet,
				// as may happen with kAudioFilePropertyMaximumPacketSize)
				afio.SrcSizePerPacket = sourceFile.PacketSizeUpperBound;

				// how many packets can we read for our buffer size?
				afio.NumPacketsPerRead = afio.SrcBufferSize / afio.SrcSizePerPacket;

				// allocate memory for the PacketDescription structures describing the layout of each packet
				afio.PacketDescriptions = new AudioStreamPacketDescription [afio.NumPacketsPerRead];
			} else {
				// CBR source format
				afio.SrcSizePerPacket = srcFormat.BytesPerPacket;
				afio.NumPacketsPerRead = afio.SrcBufferSize / afio.SrcSizePerPacket;
				// allocate memory for the PacketDescription structures describing the layout of each packet
				afio.PacketDescriptions = new AudioStreamPacketDescription [afio.NumPacketsPerRead];
			}

			// set up output buffers
			int outputSizePerPacket = dstFormat.BytesPerPacket; // this will be non-zero if the format is CBR
			const int theOutputBufSize = 32768;
			var outputBuffer = Marshal.AllocHGlobal (theOutputBufSize);
			AudioStreamPacketDescription[] outputPacketDescriptions = null;

			if (outputSizePerPacket == 0) {
				// if the destination format is VBR, we need to get max size per packet from the converter
				outputSizePerPacket = (int)converter.MaximumOutputPacketSize;

			}
			// allocate memory for the PacketDescription structures describing the layout of each packet
			outputPacketDescriptions = new AudioStreamPacketDescription [theOutputBufSize / outputSizePerPacket];
			int numOutputPackets = theOutputBufSize / outputSizePerPacket;

			// if the destination format has a cookie, get it and set it on the output file
			WriteCookie (converter, destinationFile);

			// write destination channel layout
			if (srcFormat.ChannelsPerFrame > 2) {
				WriteDestinationChannelLayout (converter, sourceFile, destinationFile);
			}

			long totalOutputFrames = 0; // used for debugging
			long outputFilePos = 0;
			AudioBuffers fillBufList = new AudioBuffers (1);
			bool error = false;

			// loop to convert data
			while (true) {
				// set up output buffer list
				fillBufList [0] = new AudioBuffer () {
					NumberChannels = dstFormat.ChannelsPerFrame,
					DataByteSize = theOutputBufSize,
					Data = outputBuffer
				};

				// convert data
				int ioOutputDataPackets = numOutputPackets;
				var fe = converter.FillComplexBuffer (ref ioOutputDataPackets, fillBufList, outputPacketDescriptions);
				// if interrupted in the process of the conversion call, we must handle the error appropriately
				if (fe != AudioConverterError.None) {
					error = true;
					break;
				}

				if (ioOutputDataPackets == 0) {
					// this is the EOF conditon
					break;
				}

				// write to output file
				var inNumBytes = fillBufList [0].DataByteSize;

				var we = destinationFile.WritePackets (false, inNumBytes, outputPacketDescriptions, outputFilePos, ref ioOutputDataPackets, outputBuffer);
				if (we != 0) {
					error = true;
					break;
				}

				// advance output file packet position
				outputFilePos += ioOutputDataPackets;

				if (dstFormat.FramesPerPacket != 0) { 
					// the format has constant frames per packet
					totalOutputFrames += (ioOutputDataPackets * dstFormat.FramesPerPacket);
				} else {
					// variable frames per packet require doing this for each packet (adding up the number of sample frames of data in each packet)
					for (var i = 0; i < ioOutputDataPackets; ++i)
						totalOutputFrames += outputPacketDescriptions [i].VariableFramesInPacket;
				}

			}

			Marshal.FreeHGlobal (outputBuffer);

			if (!error) {
				// write out any of the leading and trailing frames for compressed formats only
				if (dstFormat.BitsPerChannel == 0) {
					// our output frame count should jive with
					WritePacketTableInfo (converter, destinationFile);
				}

				// write the cookie again - sometimes codecs will update cookies at the end of a conversion
				WriteCookie (converter, destinationFile);
			}

			converter.Dispose ();
			destinationFile.Dispose ();
			sourceFile.Dispose ();

			return true;
		}

		static AudioConverterError HandleInputData (ref int numberDataPackets, AudioBuffers data, ref AudioStreamPacketDescription[] dataPacketDescription)
		{
			int maxPackets = afio.SrcBufferSize / afio.SrcSizePerPacket;
			if (numberDataPackets > maxPackets)
				numberDataPackets = maxPackets;

			// read from the file
			int outNumBytes;
			var res = afio.SourceFile.ReadPackets (false, out outNumBytes, afio.PacketDescriptions, afio.SrcFilePos, ref numberDataPackets, afio.SrcBuffer);
			if (res != 0) {
				throw new ApplicationException (res.ToString ());
			}

			// advance input file packet position
			afio.SrcFilePos += numberDataPackets;

			// put the data pointer into the buffer list
			data.SetData (0, afio.SrcBuffer, outNumBytes);

			// don't forget the packet descriptions if required
			if (dataPacketDescription != null) {
				if (afio.PacketDescriptions != null) {
					dataPacketDescription = afio.PacketDescriptions;
				} else {
					dataPacketDescription = null;
				}
			}

			return AudioConverterError.None;

		}

		// Some audio formats have a magic cookie associated with them which is required to decompress audio data
		// When converting audio, a magic cookie may be returned by the Audio Converter so that it may be stored along with
		// the output data -- This is done so that it may then be passed back to the Audio Converter at a later time as required
		static void WriteCookie (AudioConverter converter, AudioFile destinationFile)
		{
			var cookie = converter.CompressionMagicCookie;
			if (cookie != null && cookie.Length != 0) {
				destinationFile.MagicCookie = cookie;
			}
		}

		// Write output channel layout to destination file
		static void WriteDestinationChannelLayout (AudioConverter converter, AudioFile sourceFile, AudioFile destinationFile)
		{
			// if the Audio Converter doesn't have a layout see if the input file does
			var layout = converter.OutputChannelLayout ?? sourceFile.ChannelLayout;

			if (layout != null) {
				// TODO:
				throw new NotImplementedException ();
				//destinationFile.ChannelLayout = layout;
			}
		}

		// Sets the packet table containing information about the number of valid frames in a file and where they begin and end
		// for the file types that support this information.
		// Calling this function makes sure we write out the priming and remainder details to the destination file	
		static void WritePacketTableInfo (AudioConverter converter, AudioFile destinationFile)
		{
			if (!destinationFile.IsPropertyWritable (AudioFileProperty.PacketTableInfo))
				return;

			// retrieve the leadingFrames and trailingFrames information from the converter,
			AudioConverterPrimeInfo primeInfo = converter.PrimeInfo;

			// we have some priming information to write out to the destination file
			// The total number of packets in the file times the frames per packet (or counting each packet's
			// frames individually for a variable frames per packet format) minus mPrimingFrames, minus
			// mRemainderFrames, should equal mNumberValidFrames.

			AudioFilePacketTableInfo? pti_n = destinationFile.PacketTableInfo;
			if (pti_n == null)
				return;

			AudioFilePacketTableInfo pti = pti_n.Value;

			// there's priming to write out to the file
			// get the total number of frames from the output file
			long totalFrames = pti.ValidFrames + pti.PrimingFrames + pti.RemainderFrames;

			pti.PrimingFrames = primeInfo.LeadingFrames;
			pti.RemainderFrames = primeInfo.TrailingFrames;
			pti.ValidFrames = totalFrames - pti.PrimingFrames - pti.RemainderFrames;

			destinationFile.PacketTableInfo = pti;
		}

		static void ReadCookie (AudioFile sourceFile, AudioConverter converter)
		{
			// grab the cookie from the source file and set it on the converter
			var cookie = sourceFile.MagicCookie;

			// if there is an error here, then the format doesn't have a cookie - this is perfectly fine as some formats do not
			if (cookie != null && cookie.Length != 0) {
				converter.DecompressionMagicCookie = cookie;
			}
		}

		class AudioFileIO
		{
			public AudioFileIO (int bufferSize)
			{
				this.SrcBufferSize = bufferSize;
				this.SrcBuffer = Marshal.AllocHGlobal (bufferSize);
			}

			~AudioFileIO ()
			{
				Marshal.FreeHGlobal (SrcBuffer);
			}

			public AudioFile SourceFile { get; set; }

			public int SrcBufferSize { get; private set; }

			public IntPtr SrcBuffer { get; private set; }

			public int SrcFilePos { get; set; }

			public AudioStreamBasicDescription SrcFormat { get; set; }

			public int SrcSizePerPacket { get; set; }

			public int NumPacketsPerRead { get; set; }

			public AudioStreamPacketDescription[] PacketDescriptions { get; set; }
		}

	}
}

