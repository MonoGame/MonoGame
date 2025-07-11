// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Runtime.InteropServices;


namespace MonoGame.Interop;


[MGHandle]
internal readonly struct MGM_AudioDecoder{ }

[MGHandle]
internal readonly struct MGM_VideoDecoder { }

struct MGM_AudioDecoderInfo
{
    /// <summary>
    /// The audio samples per second, per channel.
    /// </summary>
    public int samplerate;

    /// <summary>
    /// The number of audio channels (typically 1 or 2).
    /// </summary>
    public int channels;

    /// <summary>
    /// The estimated duration of the audio in milliseconds.
    /// </summary>
    public ulong duration;
}


struct MGM_VideoDecoderInfo
{
    /// <summary>
    /// The width of the video frames.
    /// </summary>
    public int width;

    /// <summary>
    /// The height of the video frames.
    /// </summary>
    public int height;

    /// <summary>
    /// The number of frames per second.
    /// </summary>
    public float fps;

    /// <summary>
    /// The estimated duration of the video in milliseconds.
    /// </summary>
    public ulong duration;
}


internal static unsafe partial class MGM
{
    #region Audio Decoder

    /// <summary>
    /// This returns an audio decoder that returns a stream of PCM data from an audio file.
    /// </summary>
    /// <param name="filepath">The absolute file path to the audio file.</param>
    /// <param name="info">Returns information about the opened audio file.</param>
    /// <returns>Returns the audio decoder ready to read data or null if the format is unsupported.</returns>
    [DllImport(MGP.MonoGameNativeDLL, EntryPoint = "MGM_AudioDecoder_Create", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
    public static extern MGM_AudioDecoder* AudioDecoder_Create([MarshalAs(UnmanagedType.LPUTF8Str)] string filepath, out MGM_AudioDecoderInfo info);

    /// <summary>
    /// This releases all internal resources, closes the file, and destroys the audio decoder.
    /// </summary>
    [DllImport(MGP.MonoGameNativeDLL, EntryPoint = "MGM_AudioDecoder_Destroy", ExactSpelling = true)]
    public static extern void AudioDecoder_Destroy(MGM_AudioDecoder* decoder);
   
    /// <summary>
    /// Set the position of the audio decoder in milliseconds.
    /// </summary>
    /// <param name="decoder">The decoder.</param>
    /// <param name="timeMS">The time in millseconds from the start of the audio file.</param>
    [DllImport(MGP.MonoGameNativeDLL, EntryPoint = "MGM_AudioDecoder_SetPosition", ExactSpelling = true)]
    public static extern void AudioDecoder_SetPosition(MGM_AudioDecoder* decoder, ulong timeMS);

    /// <summary>
    /// Decode some PCM data from the audio file.
    /// </summary>
    /// <param name="decoder">The decoder.</param>
    /// <param name="buffer">The decoded PCM data good until the next call.</param>
    /// <param name="size">The size in bytes of the buffer decoded.</param>
    /// <returns>Returns true if we've reached the end of the file.</returns>
    [DllImport(MGP.MonoGameNativeDLL, EntryPoint = "MGM_AudioDecoder_Decode", ExactSpelling = true)]
    public static extern byte AudioDecoder_Decode(MGM_AudioDecoder* decoder, out byte* buffer, out uint size);

    #endregion


    #region Video

    [DllImport(MGP.MonoGameNativeDLL, EntryPoint = "MGM_VideoDecoder_Create", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
    public static extern MGM_VideoDecoder* VideoDecoder_Create(MGG_GraphicsDevice* device, [MarshalAs(UnmanagedType.LPUTF8Str)] string filepath, out MGM_VideoDecoderInfo info);

    [DllImport(MGP.MonoGameNativeDLL, EntryPoint = "MGM_VideoDecoder_Destroy", ExactSpelling = true)]
    public static extern void VideoDecoder_Destroy(MGM_VideoDecoder* decoder);

    [DllImport(MGP.MonoGameNativeDLL, EntryPoint = "MGM_VideoDecoder_GetAudioDecoder", ExactSpelling = true)]
    public static extern MGM_AudioDecoder* VideoDecoder_GetAudioDecoder(MGM_VideoDecoder* decoder, out MGM_AudioDecoderInfo info);

    [DllImport(MGP.MonoGameNativeDLL, EntryPoint = "MGM_VideoDecoder_GetPosition", ExactSpelling = true)]
    public static extern ulong VideoDecoder_GetPosition(MGM_VideoDecoder* decoder);

    [DllImport(MGP.MonoGameNativeDLL, EntryPoint = "MGM_VideoDecoder_SetLooped", ExactSpelling = true)]
    public static extern void VideoDecoder_SetLooped(MGM_VideoDecoder* decoder, byte looped);

    [DllImport(MGP.MonoGameNativeDLL, EntryPoint = "MGM_VideoDecoder_Decode", ExactSpelling = true)]
    public static extern MGG_Texture* VideoDecoder_Decode(MGM_VideoDecoder* decoder);

    #endregion
}
