//
//
//  THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY
//  KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
//  IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR
//  PURPOSE. IT CAN BE DISTRIBUTED FREE OF CHARGE AS LONG AS THIS HEADER 
//  REMAINS UNCHANGED.
//
//  Email:  yetiicb@hotmail.com
//
//  Copyright (C) 2002-2003 Idael Cardoso. 
//

using System;
using System.IO;
using WaveLib;

namespace Yeti.MMedia
{
  /// <summary>
	/// Save RAW PCM data to a stream in WAVE format
	/// </summary>
	public class WaveWriter :  AudioWriter
	{
    private const uint WaveHeaderSize = 38;
    private const uint WaveFormatSize = 18;
    private uint m_AudioDataSize = 0;
    private uint m_WrittenBytes = 0;
	  private bool closed = false;

    public WaveWriter(Stream Output, WaveFormat Format, uint AudioDataSize)
      : base(Output, Format)
    {
      m_AudioDataSize = AudioDataSize;
      WriteWaveHeader();
    }

    public WaveWriter(Stream Output, WaveFormat Format)
      :base(Output, Format)
    {
      if ( !OutStream.CanSeek )
      {
        throw new ArgumentException("The stream must supports seeking if AudioDataSize is not supported", "Output");
      }
      OutStream.Seek(WaveHeaderSize+8, SeekOrigin.Current);
    }

    private byte[] Int2ByteArr(uint val)
    {
      byte[] res = new byte[4];
      for(int i=0; i<4; i++)
      {
        res[i] = (byte)(val >> (i*8));
      }
      return res;
    }

    private byte[] Int2ByteArr(short val)
    {
      byte[] res = new byte[2];
      for(int i=0; i<2; i++)
      {
        res[i] = (byte)(val >> (i*8));
      }
      return res;
    }

    protected void WriteWaveHeader()
    {
      Write(new byte[]{(byte)'R', (byte)'I', (byte)'F', (byte)'F'});
      Write(Int2ByteArr(m_AudioDataSize + WaveHeaderSize));
      Write(new byte[]{(byte)'W', (byte)'A', (byte)'V', (byte)'E'});
      Write(new byte[]{(byte)'f', (byte)'m', (byte)'t', (byte)' '});
      Write(Int2ByteArr(WaveFormatSize));
      Write(Int2ByteArr(m_InputDataFormat.wFormatTag));
      Write(Int2ByteArr(m_InputDataFormat.nChannels));
      Write(Int2ByteArr((uint)m_InputDataFormat.nSamplesPerSec));
      Write(Int2ByteArr((uint)m_InputDataFormat.nAvgBytesPerSec));
      Write(Int2ByteArr(m_InputDataFormat.nBlockAlign));
      Write(Int2ByteArr(m_InputDataFormat.wBitsPerSample));
      Write(Int2ByteArr(m_InputDataFormat.cbSize));
      Write(new byte[]{(byte)'d', (byte)'a', (byte)'t', (byte)'a'});
      Write(Int2ByteArr(m_AudioDataSize));
      m_WrittenBytes -= (WaveHeaderSize+8);
    }

    public override void Close()
    {
	    if (!closed)
	    {
        if ( m_AudioDataSize == 0 )
		    {
			    Seek(-(int)m_WrittenBytes - (int)WaveHeaderSize - 8, SeekOrigin.Current);
			    m_AudioDataSize = m_WrittenBytes;
			    WriteWaveHeader();
		    }
	    }
      closed = true;

      base.Close ();
    }

    protected override void Dispose(bool disposing)
    {
        if (this.BaseStream is FileStream)
            base.Dispose(disposing);
        else
            base.Dispose(false);
    }
  
    public override void Write(byte[] buffer, int index, int count)
    {
      base.Write (buffer, index, count);
      m_WrittenBytes += (uint)count;
    }
  
    public override void Write(byte[] buffer)
    {
      base.Write (buffer);
      m_WrittenBytes += (uint)buffer.Length;
    }
  
    protected override int GetOptimalBufferSize()
    {
      return m_InputDataFormat.nAvgBytesPerSec /10; 
    }

    public static IEditAudioWriterConfig GetConfigControl(AudioWriterConfig config)
    {
      IEditAudioWriterConfig cfg = new EditWaveWriter();
      cfg.Config = config;
      return cfg;
    }
  
  }
}
