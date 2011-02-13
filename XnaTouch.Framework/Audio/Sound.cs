/*
	Sound.cs
	 
	Author:
	      Christian Beaumont chris@foundation42.org (http://www.foundation42.com)
	
	Copyright (c) 2009 Foundation42 LLC
	
	Permission is hereby granted, free of charge, to any person obtaining a copy
	of this software and associated documentation files (the "Software"), to deal
	in the Software without restriction, including without limitation the rights
	to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
	copies of the Software, and to permit persons to whom the Software is
	furnished to do so, subject to the following conditions:
	
	The above copyright notice and this permission notice shall be included in
	all copies or substantial portions of the Software.
	
	THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
	IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
	FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
	AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
	LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
	OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
	THE SOFTWARE.
*/

namespace Microsoft.Xna.Framework.Audio
{
	using System;
	using System.Text;
	using System.Threading;
	using System.Diagnostics;
	using System.Runtime.InteropServices;
	using System.Collections.Generic;
	using MonoTouch;
	using MonoTouch.Foundation;	
	using OSStatus = System.Int32;
	using AudioFileTypeID = System.UInt32;
	using AudioFileID = System.IntPtr;
	using AudioQueue = System.Int32;
	
	internal enum CFRunLoop : uint
	{
	    Finished = 1,
	    Stopped = 2,
	    TimedOut = 3,
	    HandledSource = 4
	};

	internal enum FileType : uint
	{
		AIFF = 0x41494646, // AIFF
		AIFC = 0x41494643, // AIFC
		WAVE = 0x57415645, // WAVE
		SoundDesigner2 = 0x53643266, // Sd2f
		Next = 0x4e655854, // NeXT
		MP3 = 0x4d504733, // MPG3
		MP2 = 0x4d504732, // MPG2
		MP1 = 0x4d504731, // MPG1
		AC3 = 0x61632d33, // ac-3
		AAC_ADTS = 0x61647473, // adts
		MPEG4 = 0x6d703466, // mp4f
		M4A = 0x6d346166, // m4af
		CAF = 0x63616666, // caff
		ThreeGP = 0x33677070, // 3gpp
		ThreeGP2 = 0x33677032, // 3gp2
		AMR = 0x616d7266, // amrf
	}
	
	internal enum ErrorCodes : uint
	{
		Unspecified = 0x7768743f, // wht?
		UnsupportedFileType = 0x7479703f, // typ?
		UnsupportedDataFormat = 0x666d743f, // fmt?
		UnsupportedProperty = 0x7074793f, // pty?
		BadPropertySize = 0x2173697a, // !siz
		Permissions = 0x70726d3f, // prm?
		NotOptimized = 0x6f70746d, // optm
		InvalidChunk = 0x63686b3f, // chk?
		DoesNotAllow64BitDataSize = 0x6f66663f, // off?
		InvalidPacketOffset = 0x70636b3f, // pck?
		InvalidFile = 0x6474613f, // dta?
	};
	
	internal enum AudioFileProperty : uint
	{
		FileFormat = 0x66666d74, // ffmt
		DataFormat = 0x64666d74, // dfmt
		IsOptimized = 0x6f70746d, // optm
		MagicCookieData = 0x6d676963, // mgic
		AudioDataByteCount = 0x62636e74, // bcnt
		AudioDataPacketCount = 0x70636e74, // pcnt
		MaximumPacketSize = 0x70737a65, // psze
		DataOffset = 0x646f6666, // doff
		ChannelLayout = 0x636d6170, // cmap
		DeferSizeUpdates = 0x64737a75, // dszu
		DataFormatName = 0x666e6d65, // fnme
		MarkerList = 0x6d6b6c73, // mkls
		RegionList = 0x72676c73, // rgls
		PacketToFrame = 0x706b6672, // pkfr
		FrameToPacket = 0x6672706b, // frpk
		ChunkIDs = 0x63686964, // chid
		InfoDictionary = 0x696e666f, // info
		PacketTableInfo = 0x706e666f, // pnfo
		FormatList = 0x666c7374, // flst
		PacketSizeUpperBound = 0x706b7562, // pkub
		ReserveDuration = 0x72737276, // rsrv
		EstimatedDuration = 0x65647572, // edur
		BitRate = 0x62726174, // brat
	};
	
	internal enum AudioQueueProperty
	{
		IsRunning = 0x6171726e, // aqrn'
		SampleRate = 0x61717372, // aqsr'
		NumberChannels = 0x61716463, // aqdc'
		CurrentDevice = 0x61716364, // aqcd'
		MagicCookie = 0x61716d63, // aqmc'
		MaximumOutputPacketSize = 0x786f7073, // xops'
		StreamDescription = 0x61716674, // aqft'
		ChannelLayout = 0x6171636c, // aqcl'
		EnableLevelMetering = 0x61716d65, // aqme'
		CurrentLevelMeter = 0x61716d76, // aqmv'
		CurrentLevelMeterDB = 0x61716d64, // aqmd'
		DecodeBufferSizeFrames = 0x64636266, // dcbf
	};

	internal enum MarkerType : uint
	{
		kAudioFileMarkerType_Generic = 0,
	};
	
	internal enum RegionFlag : uint
	{
		LoopEnable = 1,
		PlayForward = 2,
		PlayBackward = 4
	};

	internal enum AudioFilePermissions : sbyte
	{
		Read = 0x01,
		Write = 0x02,
		ReadWrite = 0x03
	};
		
	internal enum AudioQueueParameter : uint
	{
	    Volume = 1
	};

	[ StructLayout(LayoutKind.Sequential) ]
	internal struct AudioFramePacketTranslation
	{
		long mFrame;
		long mPacket;
		uint mFrameOffsetInPacket;
	};
	
	[ StructLayout(LayoutKind.Sequential) ]
	internal struct AudioFilePacketTableInfo
	{
        long mNumberValidFrames;
        int mPrimingFrames;
        int mRemainderFrames;
	};

	[ StructLayout(LayoutKind.Sequential) ]
	internal struct AudioStreamBasicDescription
	{
		public double SampleRate;
		public uint FormatID;
		public uint FormatFlags;
		public uint BytesPerPacket;
		public uint FramesPerPacket;
		public uint BytesPerFrame;
		public uint ChannelsPerFrame;
		public uint BitsPerChannel;
		public uint Reserved;
		
		public override string ToString ()
		{
			StringBuilder b = new StringBuilder();
			
			b.AppendLine("Stream Description");
			b.AppendLine("==================");
			b.AppendLine("  SampleRate = " + this.SampleRate);
			b.AppendLine("  FormatID = " + API.ToHex(this.FormatID));
			b.AppendLine("  FormatFlags = " + API.ToHex(this.FormatFlags));
			b.AppendLine("  BytesPerPacket = " + this.BytesPerPacket);
			b.AppendLine("  FramesPerPacket = " + this.FramesPerPacket);
			b.AppendLine("  BytesPerFrame = " + this.BytesPerFrame);
			b.AppendLine("  ChannelsPerFrame = " + this.ChannelsPerFrame);
			b.AppendLine("  BitsPerChannel = " + this.BitsPerChannel);
			b.AppendLine("");
			
			return b.ToString();
		}

	};
		
	[ StructLayout(LayoutKind.Sequential) ]
	internal struct SMPTETime
	{
		public short Subframes;
		public short SubframeDivisor;
		public uint Counter;
		public uint Type;
		public uint Flags;
		public short Hours;
		public short Minutes;
		public short Seconds;
		public short Frames;
	};

		
	[ StructLayout(LayoutKind.Sequential) ]
	internal struct AudioTimeStamp
	{
		public double SampleTime;
		public ulong  HostTime;
		public double RateScalar;
		public ulong  WordClockTime;
		public SMPTETime SMPTETime;
		public uint Flags;
		public uint Reserved;
	};

		
	[ StructLayout(LayoutKind.Sequential) ]
	internal unsafe struct AudioQueueBuffer
	{
		public int AudioDataBytesCapacity;
		public byte* AudioData;
		public int AudioDataByteSize;
		public void* UserData;
		public int PacketDescriptionCapacity;
		public AudioStreamPacketDescription* PacketDescriptors;
		public int PacketDescriptorCount;
	};
		
	[ StructLayout(LayoutKind.Sequential) ]
	internal struct AudioStreamPacketDescription
	{
		public long StartOffset;
		public uint VariableFramesInPacket;
		public uint DataByteSize;
	};

	internal class API
	{
		public unsafe delegate void AudioQueueOutputCallback(void* pState, AudioQueue* pQueue, AudioQueueBuffer* pBuffer);
		public unsafe delegate void AudioQueuePropertyChangeCallback(void* pUserData, AudioQueue* pQueue, AudioQueueProperty id);
		internal static object StopLock = new object();
		
		public static string ToHex(uint value)
		{
			StringBuilder builder = new StringBuilder("0x");
			builder.Append(Convert.ToString(value, 16).PadLeft(8, '0'));
			return builder.ToString();
		}
		
		public static unsafe void ZeroMemory(void* pBlock, int size)
		{
			byte* p = (byte*) pBlock;
			
			for (int i = 0; i < size; i++, p++)
				*p = 0;
		}
		
		[Conditional("DEBUG")]
		public static void CheckStatus(OSStatus status)
		{
			if (status == 0)
				return;
		
			Console.WriteLine("AudioStream API Failed: Status Code = " + API.ToHex((uint) status));
		}
		

		[ DllImport(Constants.AudioToolboxLibrary) ]
		public static extern unsafe OSStatus AudioFileReadPackets(AudioFileID fileID, int useCache, int* numBytesReadFromFile, AudioStreamPacketDescription* packetDesc, long startPacket, int* numPackets, void* outputBuffer);

		[ DllImport(Constants.AudioToolboxLibrary) ]
		public static extern unsafe OSStatus AudioQueueEnqueueBuffer(AudioQueue* pQueue, AudioQueueBuffer* pBuffer, int numPacketDescs, AudioStreamPacketDescription* pDescriptors);

		[ DllImport(Constants.AudioToolboxLibrary) ]
		public static extern unsafe OSStatus AudioQueueStop(AudioQueue* pQueue, int immediate);

		[ DllImport(Constants.AudioToolboxLibrary) ]
		public static extern unsafe OSStatus AudioFileOpenURL(IntPtr inFileRef, AudioFilePermissions inPermissions, AudioFileTypeID inFileTypeHint, AudioFileID* outAudioFile);
			
		[ DllImport(Constants.AudioToolboxLibrary) ]
		public static extern unsafe OSStatus AudioQueueAddPropertyListener(AudioQueue* pQueue, AudioQueueProperty propertyId, AudioQueuePropertyChangeCallback callback, void* pUserData);
		
		[ DllImport(Constants.AudioToolboxLibrary) ]
		public static extern unsafe OSStatus AudioFileGetProperty(AudioFileID fileID, AudioFileProperty propertyId, int* dataSize, void* pResult);
			
		[ DllImport(Constants.AudioToolboxLibrary) ]
		public static extern unsafe OSStatus AudioQueueGetProperty(AudioQueue* pQueue, AudioQueueProperty propertyId, void* pResult, int* dataSize);
		
		[ DllImport(Constants.AudioToolboxLibrary) ]
		public static extern unsafe OSStatus AudioQueueNewOutput(AudioStreamBasicDescription* pFormat, AudioQueueOutputCallback callback, void* pUserData, void* runLoop, void* pRunLoopMode, uint flags, AudioQueue** pQueue);
		
		[ DllImport(Constants.AudioToolboxLibrary) ]
		public static extern unsafe OSStatus AudioQueueAllocateBuffer(AudioQueue* pQueue, int byteCount, AudioQueueBuffer** ppBuffer);
		
		[ DllImport(Constants.AudioToolboxLibrary) ]
		public static extern unsafe OSStatus AudioQueueAllocateBufferWithPacketDescriptions(AudioQueue* pQueue, int byteCount, int descriptorCount, AudioQueueBuffer** ppBuffer);
		
		[ DllImport(Constants.AudioToolboxLibrary) ]
		public static extern unsafe OSStatus AudioQueueSetParameter(AudioQueue* pQueue, AudioQueueParameter paramId, float paramVal);
		
		[ DllImport(Constants.AudioToolboxLibrary) ]
		public static extern unsafe OSStatus AudioQueueStart(AudioQueue* pQueue, AudioTimeStamp* startTime);
		
		[ DllImport(Constants.AudioToolboxLibrary) ]
		public static extern unsafe OSStatus AudioQueueSetProperty(AudioQueue* pQueue, AudioQueueProperty propertyId, void* pCookie, int cookieSize);

		[ DllImport(Constants.AudioToolboxLibrary) ]
		public static extern unsafe OSStatus AudioFileGetPropertyInfo(AudioFileID fileID, AudioFileProperty propertyId, int* pSize, int* isWritable);
		
		[ DllImport(Constants.AudioToolboxLibrary) ]
		public static extern unsafe OSStatus AudioQueueDispose(AudioQueue* pQueue, int immediate);
		
		[ DllImport(Constants.AudioToolboxLibrary) ]
		public static extern unsafe OSStatus AudioFileClose(AudioFileID fileID);
		
		[ DllImport(Constants.AudioToolboxLibrary) ]
		public static extern unsafe OSStatus AudioQueueFreeBuffer(AudioQueue* pQueue, AudioQueueBuffer* pBuffer);
	}
	
	[ StructLayoutAttribute(LayoutKind.Sequential) ]
	internal unsafe struct AudioStream
	{
		internal AudioFileID AudioFile;
		internal bool IsRunning;
		internal int MaxPacketSize;
			
		internal int BufferByteSize;
		internal AudioQueueBuffer* Buffer0;
		internal AudioQueueBuffer* Buffer1;
		internal AudioQueueBuffer* Buffer2;
			
		internal AudioQueue* Queue;
		internal AudioStreamBasicDescription DataFormat;
		internal long CurrentPacket;
		internal int NumPacketsToRead;
		
		internal bool Looping;
		
		static API.AudioQueueOutputCallback OnReadBuffer = ReadBufferProc;
		static API.AudioQueuePropertyChangeCallback OnPropertyChange = PropertyChangeProc;
		
		[ MonoPInvokeCallback(typeof(API.AudioQueuePropertyChangeCallback)) ]
		static unsafe void PropertyChangeProc(void* pUserData, AudioQueue* pQueue, AudioQueueProperty id)
		{
			lock (API.StopLock)
			{
				try
				{
					PropertyChangeInternal(pUserData, pQueue, id);
				}
				catch (Exception e)
				{
					Console.WriteLine(e.ToString());
				}
			}
		}
			
		static unsafe void PropertyChangeInternal(void* pUserData, AudioQueue* pQueue, AudioQueueProperty id)
		{
			AudioStream* pThis = (AudioStream*) pUserData;
			
			if (pThis == null)
				Console.WriteLine("PropertyChangeProc: pThis is null");
			
			if (pQueue == null)
				Console.WriteLine("PropertyChangeProc: pQueue is null");
			
			int size = sizeof(uint);
			uint iIsRunning = 0;
			OSStatus status = API.AudioQueueGetProperty(pQueue, AudioQueueProperty.IsRunning, &iIsRunning, &size);
			API.CheckStatus(status);

			bool isRunning = iIsRunning != 0;
			
			if (status == 0)
			{
				if (pThis->IsRunning && !isRunning)
					AudioStream.Stop(pThis);
			}
		}
		
		[ MonoPInvokeCallback(typeof(API.AudioQueueOutputCallback)) ]
		static unsafe void ReadBufferProc(void* pUserData, AudioQueue* pQueue, AudioQueueBuffer* pBuffer)
		{
			lock (API.StopLock)
			{
				try
				{
					ReadBufferInternal(pUserData, pQueue, pBuffer);
				}
				catch (Exception e)
				{
					Console.WriteLine(e.ToString());
				}
			}
		}
		
		static unsafe void ReadBufferInternal(void* pUserData, AudioQueue* pQueue, AudioQueueBuffer* pBuffer)
		{
			AudioStream* pThis = (AudioStream*) pUserData;
			
			if (pThis == null)
				Console.WriteLine("ReadBufferProc: pThis is null");
			
			if (!pThis->IsRunning)
				return;
			
			if (pQueue == null)
				Console.WriteLine("ReadBufferProc: pQueue is null");
			
			if (pBuffer == null)
				Console.WriteLine("ReadBufferProc: pBuffer is null");
			
			if (pBuffer->AudioData == null)
				Console.WriteLine("ReadBufferProc: pBuffer->AudioData is null");
			
			if (pBuffer->PacketDescriptors == null)
				Console.WriteLine("ReadBufferProc: pBuffer->PacketDescriptors is null");
			
			if (pThis->AudioFile == null)
				Console.WriteLine("ReadBufferProc: pThis->AudioFile is null");

			int numPacketsReadFromFile = pThis->NumPacketsToRead;
			int numBytesReadFromFile = 0;

			OSStatus status = API.AudioFileReadPackets(pThis->AudioFile, 0, &numBytesReadFromFile, pBuffer->PacketDescriptors, pThis->CurrentPacket, &numPacketsReadFromFile, pBuffer->AudioData);
			API.CheckStatus(status);
			
			if (status == 0 &&
			    numPacketsReadFromFile == 0 &&
			    pThis->Looping)
			{
				// we ran out of packets and they are
				// asking to loop, so try and reset
				pThis->CurrentPacket = 0;
				numPacketsReadFromFile = pThis->NumPacketsToRead;
				numBytesReadFromFile = 0;
				status = API.AudioFileReadPackets(pThis->AudioFile, 0, &numBytesReadFromFile, pBuffer->PacketDescriptors, pThis->CurrentPacket, &numPacketsReadFromFile, pBuffer->AudioData);
				API.CheckStatus(status);
			}
				
			if (numPacketsReadFromFile > 0)	
			{
				pBuffer->AudioDataByteSize = numBytesReadFromFile;
				pBuffer->PacketDescriptorCount = numPacketsReadFromFile;
				
				status = API.AudioQueueEnqueueBuffer(pThis->Queue, pBuffer, (pBuffer->PacketDescriptors != null ? pBuffer->PacketDescriptorCount : 0), pBuffer->PacketDescriptors);
				API.CheckStatus(status);
				
				pThis->CurrentPacket += numPacketsReadFromFile;
			}
			else
			{
				status = API.AudioQueueStop(pThis->Queue, 0);
				API.CheckStatus(status);
			}
		}
			
		static unsafe void DeriveBufferSize(AudioStreamBasicDescription* ASBDesc, int maxPacketSize, double seconds, int* outBufferSize, int* outNumPacketsToRead)
		{
			const int maxBufferSize = 0x10000;
			const int minBufferSize = 0x4000;
			
			if (ASBDesc->FramesPerPacket != 0)
			{
				double numPacketsForTime = ASBDesc->SampleRate / ASBDesc->FramesPerPacket * seconds;
				*outBufferSize = (int) (numPacketsForTime * maxPacketSize);
			}
			else
				*outBufferSize = maxBufferSize > maxPacketSize ? maxBufferSize : maxPacketSize;
			
			if (	*outBufferSize > maxBufferSize &&
				*outBufferSize > maxPacketSize)
			{
				*outBufferSize = maxBufferSize;
			}
			else
			{
				if (*outBufferSize < minBufferSize)
					*outBufferSize = minBufferSize;
			}
				
			*outNumPacketsToRead = *outBufferSize / maxPacketSize;
		}
			
		private static unsafe void OpenFileAndCreateStream(AudioStream* pThis, string url)
		{
			using (NSUrl nsUrl = new NSUrl(url))
			{
				OSStatus status = API.AudioFileOpenURL(nsUrl.Handle, AudioFilePermissions.Read, 0, &pThis->AudioFile);
				API.CheckStatus(status);
			}
		}
		
		private static unsafe void CreateQueue(AudioStream* pThis)
		{
			int dataFormatSize = sizeof(AudioStreamBasicDescription);
			AudioStreamBasicDescription* pDataFormat = &pThis->DataFormat;
				
			OSStatus status = API.AudioFileGetProperty(pThis->AudioFile, AudioFileProperty.DataFormat, &dataFormatSize, pDataFormat);
			API.CheckStatus(status);
			
			status = API.AudioQueueNewOutput(pDataFormat, OnReadBuffer, pThis, null, null, 0, &pThis->Queue);
			API.CheckStatus(status);
				
			dataFormatSize = sizeof(int);
			int* pMaxPacketSize = &pThis->MaxPacketSize;
			status = API.AudioFileGetProperty(pThis->AudioFile, AudioFileProperty.PacketSizeUpperBound, &dataFormatSize, pMaxPacketSize);
			API.CheckStatus(status);
				
			DeriveBufferSize(&pThis->DataFormat, pThis->MaxPacketSize, 0.5, &pThis->BufferByteSize, &pThis->NumPacketsToRead);
		}
		
		private static unsafe void AllocateAndPrimeBuffers(AudioStream* pThis)
		{
			OSStatus status = API.AudioQueueAllocateBufferWithPacketDescriptions(pThis->Queue, pThis->BufferByteSize, pThis->NumPacketsToRead, &pThis->Buffer0);
			API.CheckStatus(status);
				
			status = API.AudioQueueAllocateBufferWithPacketDescriptions(pThis->Queue, pThis->BufferByteSize, pThis->NumPacketsToRead, &pThis->Buffer1);
			API.CheckStatus(status);
				
			status = API.AudioQueueAllocateBufferWithPacketDescriptions(pThis->Queue, pThis->BufferByteSize, pThis->NumPacketsToRead, &pThis->Buffer2);
			API.CheckStatus(status);
			
			pThis->IsRunning = true;
			
			if (pThis->Buffer0 == null)
				Console.WriteLine("AudioQueueAllocateBufferWithPacketDescriptions failed to allocate buffer0 of size " + pThis->BufferByteSize);
			
			if (pThis->Buffer1 == null)
				Console.WriteLine("AudioQueueAllocateBufferWithPacketDescriptions failed to allocate buffer0 of size " + pThis->BufferByteSize);
			
			if (pThis->Buffer2 == null)
				Console.WriteLine("AudioQueueAllocateBufferWithPacketDescriptions failed to allocate buffer0 of size " + pThis->BufferByteSize);
			
			ReadBufferProc(pThis, pThis->Queue, pThis->Buffer0);
			ReadBufferProc(pThis, pThis->Queue, pThis->Buffer1);
			ReadBufferProc(pThis, pThis->Queue, pThis->Buffer2);
		}
		
		private static unsafe void CopyPropertyFromFileToQueue(AudioStream* pThis, AudioFileProperty fileProp, AudioQueueProperty queueProp)
		{
			int size = sizeof(uint);
			OSStatus status = API.AudioFileGetPropertyInfo(pThis->AudioFile, fileProp, &size, null);
			if (status == 0 && size > 0)
			{
				void* pData = (void*) Marshal.AllocHGlobal(size);
				
				API.AudioFileGetProperty(pThis->AudioFile, fileProp, &size, pData);
				API.AudioQueueSetProperty(pThis->Queue, queueProp, pData, size);
				
				Marshal.FreeHGlobal((IntPtr) pData);
			}
		}
		
		internal static void Play(AudioStream* pThis)
		{
			// start the queue
			OSStatus status = API.AudioQueueStart(pThis->Queue, null);
			API.CheckStatus(status);
		}
		
		internal static void Stop(AudioStream* pThis)
		{
			lock (API.StopLock)
			{
				OSStatus status = API.AudioQueueDispose(pThis->Queue, 1);
				API.CheckStatus(status);
				
				status = API.AudioFileClose(pThis->AudioFile);
				API.CheckStatus(status);
				
				Marshal.FreeHGlobal((IntPtr) pThis);
				
				Sound.OnDisposeStream(pThis);
			}
		}
		
		internal static unsafe void SetVolume(AudioStream* pThis, float volume)
		{
			OSStatus status = API.AudioQueueSetParameter(pThis->Queue, AudioQueueParameter.Volume, volume);
			API.CheckStatus(status);
		}
		
		public unsafe static AudioStream* AllocateStream()
		{
			AudioStream* pThis = (AudioStream*) Marshal.AllocHGlobal(sizeof(AudioStream));
			API.ZeroMemory(pThis, sizeof(AudioStream));
			
			return pThis;
		}
		
		public unsafe static void Create(AudioStream* pThis, string url)
		{
			OpenFileAndCreateStream(pThis, url);
			
			CreateQueue(pThis);
				
			CopyPropertyFromFileToQueue(pThis, AudioFileProperty.MagicCookieData, AudioQueueProperty.MagicCookie);
			CopyPropertyFromFileToQueue(pThis, AudioFileProperty.ChannelLayout, AudioQueueProperty.ChannelLayout);
			
			API.AudioQueueAddPropertyListener(pThis->Queue, AudioQueueProperty.IsRunning, OnPropertyChange, pThis);
				
			AllocateAndPrimeBuffers(pThis);
		}
		
	}	
	
	public unsafe class Sound
	{
		private static bool Running = true;
		private static Queue<Action> WorkItems = new Queue<Action>();
		private static Dictionary<IntPtr, Sound> Map = new Dictionary<IntPtr, Sound>();
		private AudioStream* Stream;
			
		private float _Volume;
		private bool _Looping;
		
		private static void Enqueue(Action workItem)
		{
			lock (Sound.WorkItems)
			{
				Sound.WorkItems.Enqueue(workItem);
				Monitor.Pulse(Sound.WorkItems);
			}
		}
		
		private static void Worker(object state)
		{
			while (Sound.Running)
			{
				Action workItem;
				
				lock (Sound.WorkItems)
				{
					if (Sound.WorkItems.Count == 0)
						Monitor.Wait(Sound.WorkItems);
					
					workItem = Sound.WorkItems.Dequeue();
				}
				
				try
				{
					workItem();
				}
				catch
				{
					Console.WriteLine("Sound thread: Work Exception");
				}
			}
		}
		
		static Sound()
		{
			Thread workerThread = new Thread(Worker);
			workerThread.Start();
		}
		
		private Sound(AudioStream* stream)
		{
			this.Stream = stream;
		}
			
		public float Volume
		{
			get { return this._Volume; }
			set
			{
				this._Volume = value;
				
				Sound.Enqueue(() =>
				    {
						lock (Sound.Map)
						{
							if (this.Stream == null)
								return;
		
							AudioStream.SetVolume(this.Stream, value);
						}
					}
				);
			}
		}
			
		public bool Looping
		{
			get { return this._Looping; }
			set
			{
				if (this._Looping != value )
				{
					this._Looping = value;
					
					Sound.Enqueue(() =>
					    {
							lock (Sound.Map)
							{
								if (this.Stream == null)
									return;
								
								this.Stream->Looping = value;
							}
						}
					);
				}
			}
		}
		
		public void Play()
		{
			Sound.Enqueue(() =>
			    {
					lock (Sound.Map)
					{
						if (this.Stream == null)
							return;
						
						AudioStream.Play(this.Stream);
					}
				}
			);
		}
		
		public void Stop()
		{
			lock (Sound.WorkItems)
			{
				AudioStream.Stop(this.Stream);
			}
		}
		
		public static Sound Create(string url, float volume, bool looping)
		{
			Sound sound = null;
			
			AudioStream* pStream;
			
			lock (Sound.Map)
			{
				pStream = AudioStream.AllocateStream();
				
				sound = new Sound(pStream);
				Sound.Map[(IntPtr) pStream] = sound;
			}
			
			sound.Looping = looping;
			
			Sound.Enqueue(() => AudioStream.Create(pStream, url));
			
			sound.Volume = volume;
			
			return sound;
		}
		
		public static Sound CreateAndPlay(string url, float volume, bool looping)
		{
			Sound sound = Sound.Create(url, volume, looping);
			
			sound.Play();
			
			return sound;
		}
		
		internal static void OnDisposeStream(AudioStream* pStream)
		{
			lock (Sound.Map)
			{
				Sound sound = null;
				bool exists = Sound.Map.TryGetValue((IntPtr) pStream, out sound);
				
				if (exists)
				{
					Sound.Map.Remove((IntPtr) pStream);
					
					if (sound != null)
						sound.Stream = null;
				}
			}
		}
	}
}
