using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Yeti.WMFSdk;
using System.IO;
//using Yeti.MMedia;
//using WaveLib;
//using Yeti.MMedia.Mp3;

using NAudio;
using NAudio.Wave;
using NAudio.WindowsMediaFormat;
using Microsoft.Xna.Framework.Content.Pipeline.Audio;

namespace MonoGameContentProcessors
{
    public static class AudioConverter
    {
        /// <summary>
        /// Converts a WMA file to a WAV stream
        /// </summary>
        /// <param name="outputStream">Stream to store the converted wav.</param>
        /// <param name="filePath">Path to a .wma file to convert</param>
        /// <returns>The WaveFormat object of the converted wav</returns>
        private static WaveFormat wmaToWav(string pathToWma, Stream outputStream, int sampleRate, int bitDepth, int numChannels)
        {
            if (!Path.GetExtension(pathToWma).ToLower().Contains("wma"))
                throw new ArgumentException("Must be a .wma file!");

            using (var reader = new WMAFileReader(pathToWma))
            {
                var targetFormat = new NAudio.Wave.WaveFormat(sampleRate, bitDepth, numChannels);
                var pcmStream = new WaveFormatConversionStream(targetFormat, reader);
                var buffer = new byte[pcmStream.Length];
                pcmStream.Read(buffer, 0, (int)pcmStream.Length);
                outputStream.Write(buffer, 0, buffer.Length);
                outputStream.Position = 0;

                pcmStream.Close();

                return targetFormat;
            }
        }

        /// <summary>
        /// Converts one audio format into another.
        /// </summary>
        /// <param name="pathToFile">Path to the file to be converted</param>
        /// <param name="outputStream">Stream where the converted sound will be.</param>
        /// /// <param name="bitDepth">The target bit depth.</param>
        /// <param name="sampleRate">The target sample rate.</param>
        /// <param name="bitDepth">The target bit depth.</param>
        /// <param name="numChannels">the number of channels.</param>
        public static WaveFormat ConvertFile(string pathToFile, Stream outputStream, AudioFileType targetFileType, int sampleRate, int bitDepth, int numChannels)
        {
            if (targetFileType == AudioFileType.Wma)
                throw new ArgumentException("WMA is not a vaid output type.");

            string sourceFileType = pathToFile.Substring(pathToFile.Length - 3).ToLower();
            switch (sourceFileType)
            {
                case "mp3":
                    if (targetFileType != AudioFileType.Wav)
                        throw new NotSupportedException("mp3's should only ever be converted to .wav.");

                     return mp3ToWav(pathToFile, outputStream, sampleRate, bitDepth, numChannels);

                case "wma":
                     if (targetFileType == AudioFileType.Mp3)
                     {
                         wmaToMp3(pathToFile, outputStream, sampleRate, bitDepth, numChannels);
                         return null;
                     }
                     else if (targetFileType == AudioFileType.Wav)
                     {
                         return wmaToWav(pathToFile, outputStream, sampleRate, bitDepth, numChannels);
                     }

                     break;

                case "wav":
                     if (targetFileType == AudioFileType.Mp3)
                     {
                         wavToMp3(pathToFile, outputStream, sampleRate, bitDepth, numChannels);
                         return null;
                     }
                    else if (targetFileType == AudioFileType.Wav )
                        return reencodeWav(pathToFile, outputStream, sampleRate, bitDepth, numChannels);

                    break;

            }

            return null;
        }

        private static void wavToMp3(string pathToFile, Stream outputStream, int sampleRate, int bitDepth, int numChannels)
        {
            // wavStream gets closed in wavStreamToMP3();
            wavToMp3(new WaveLib.WaveStream(pathToFile), outputStream, (uint)(sampleRate / 1000));
        }

        private static void wavToMp3(Stream streamToWav, Stream outputStream, uint desiredOutputBitRate)
        {
            var mp3Config = new Yeti.MMedia.Mp3.Mp3WriterConfig((streamToWav as WaveLib.WaveStream).Format, desiredOutputBitRate);

            var mp3Writer = new Yeti.MMedia.Mp3.Mp3Writer(outputStream, mp3Config);
            byte[] buff = new byte[mp3Writer.OptimalBufferSize];
            int read = 0;
            long total = streamToWav.Length;
            streamToWav.Position = 0;
            while ((read = streamToWav.Read(buff, 0, buff.Length)) > 0)
                mp3Writer.Write(buff, 0, read);

            mp3Writer.Close();
            streamToWav.Close();
        }

        private static void wmaToMp3(string pathToFile, Stream outputStream, int sampleRate, int bitDepth, int numChannels)
        {
            var streamToWav = new WaveLib.WaveStream();
            wmaToWav(pathToFile, streamToWav.BaseStream, sampleRate, bitDepth, numChannels);

            streamToWav.ReadHeader();

            // wavStream gets closed in wavStreamToMP3();
            wavToMp3(streamToWav, outputStream, (uint)(sampleRate / 1000));
        }

        private static WaveFormat mp3ToWav(string pathToMp3, Stream outputStream, int sampleRate, int bitDepth, int numChannels)
        {
            using (var reader = new Mp3FileReader(pathToMp3))
            {
                var targetFormat = new NAudio.Wave.WaveFormat(sampleRate, bitDepth, numChannels);
                var pcmStream = new WaveFormatConversionStream(targetFormat, reader);
                var buffer = new byte[pcmStream.Length];
                pcmStream.Read(buffer, 0, (int)pcmStream.Length);
                outputStream.Write(buffer, 0, buffer.Length);
                outputStream.Position = 0;

                pcmStream.Close();

                return targetFormat;
            }
        }

        private static WaveFormat reencodeWav(string pathToWav, Stream outputStream, int sampleRate, int bitDepth, int numChannels)
        {
            using (var reader = new WaveFileReader(pathToWav))
            {
                var targetFormat = new NAudio.Wave.WaveFormat(sampleRate, bitDepth, numChannels);
                var pcmStream = new WaveFormatConversionStream(targetFormat, reader);
                var buffer = new byte[pcmStream.Length];
                pcmStream.Read(buffer, 0, (int)pcmStream.Length);

                outputStream.Write(buffer, 0, buffer.Length);
                outputStream.Position = 0;

                pcmStream.Close();

                return targetFormat;
            }
        }
    }
}
