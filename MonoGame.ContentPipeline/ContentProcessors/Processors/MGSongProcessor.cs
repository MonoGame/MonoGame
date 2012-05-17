using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using Microsoft.Xna.Framework.Content.Pipeline.Audio;
using System.Reflection;
using System.Collections.ObjectModel;
using System.IO;
using Yeti.WMFSdk;
using Yeti.MMedia;
using Yeti.MMedia.Mp3;
using WaveLib;

namespace MonoGameContentProcessors.Processors
{
    [ContentProcessor(DisplayName = "MGSongProcessor")]
    public class MGSongProcessor : SongProcessor
    {
        public override SongContent Process(AudioContent input, ContentProcessorContext context)
        {
            if (!context.BuildConfiguration.ToUpper().Contains("IOS"))
                return base.Process(input, context);

            //TODO: If quality isn't best and it's a .wma, don't compress to MP3. Leave it as a .wav instead

            string path = Path.ChangeExtension(context.OutputFilename, "mp3");
            string directoryName = Path.GetDirectoryName(path);
            if (!Directory.Exists(directoryName))
                Directory.CreateDirectory(directoryName);

            var inputFilename = Path.GetFullPath(input.FileName);

            Stream inputStream = null;
            Mp3WriterConfig mp3Config = null;

            // If the file's not already an mp3, encode it to .wav
            switch (input.FileType)
            {
                // if it's already an MP3, just pass it through.
                case AudioFileType.Mp3:
                    File.Copy(inputFilename, path, true);
                    break;

                // .wav file. Encode it to raw PCM data.
                case AudioFileType.Wma:
                    inputStream = new MemoryStream();
                    mp3Config = new Mp3WriterConfig(decodeWMAtoWAV(inputStream, inputFilename));
                    break;

                case AudioFileType.Wav:
                    inputStream = new WaveStream(inputFilename);
                    mp3Config = new Mp3WriterConfig((inputStream as WaveStream).Format);
                    // Data's already in wav format
                    break;
            }

            // Convert the data from .wav to .mp3
            if (input.FileType != AudioFileType.Mp3)
            {
                var mp3Writer = new Mp3Writer(new FileStream(path, FileMode.Create), mp3Config);
                byte[] buff = new byte[mp3Writer.OptimalBufferSize];
                int read = 0;
                long total = inputStream.Length;
                inputStream.Position = 0;
                while ((read = inputStream.Read(buff, 0, buff.Length)) > 0)
                    mp3Writer.Write(buff, 0, read);

                mp3Writer.Close();
            }

            if (inputStream != null)
                inputStream.Close();

            context.AddOutputFile(path);

            // SoundEffectContent is a sealed class, construct it using reflection
            var type = typeof(SongContent);
            ConstructorInfo c = type.GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance,
                    null, new Type[] { typeof(string), typeof(int) }, null);

            var outputSongContent = (SongContent)c.Invoke(new Object[] { Path.GetFileName(path), (int)input.Duration.TotalMilliseconds });

            return outputSongContent;
        }

        private WaveFormat decodeWMAtoWAV(Stream inputStream, string filePath)
        {
            using (WmaStream str = new WmaStream(filePath))
            {
                byte[] buffer = new byte[str.SampleSize * 2];
                inputStream = new MemoryStream();
                AudioWriter wavWriter = new WaveWriter(inputStream, str.Format);

                int read;
                while ((read = str.Read(buffer, 0, buffer.Length)) > 0)
                        wavWriter.Write(buffer, 0, read);

                return str.Format;
            }
        }

    }
}
