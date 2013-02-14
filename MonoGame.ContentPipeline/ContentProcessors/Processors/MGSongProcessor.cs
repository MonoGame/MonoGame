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

namespace MonoGameContentProcessors.Processors
{
    [ContentProcessor(DisplayName = "MonoGame Song")]
    public class MGSongProcessor : SongProcessor
    {
        public override SongContent Process(AudioContent input, ContentProcessorContext context)
        {
            // Fallback if we aren't buiding for iOS.
            var platform = ContentHelper.GetMonoGamePlatform();
            if (platform != MonoGamePlatform.iOS && platform != MonoGamePlatform.Linux)
                return base.Process(input, context);

            //TODO: If quality isn't best and it's a .wma, don't compress to MP3. Leave it as a .wav instead
            string outputType = (platform == MonoGamePlatform.iOS) ? "mp3" : "wav";
            string outputFilename = Path.ChangeExtension(context.OutputFilename, outputType);
            string directoryName = Path.GetDirectoryName(outputFilename);
            if (!Directory.Exists(directoryName))
                Directory.CreateDirectory(directoryName);

            var inputFilename = Path.GetFullPath(input.FileName);

            // XNA's songprocessor converts the bitrate on the input file based 
            // on it's conversion quality. 
            //http://blogs.msdn.com/b/etayrien/archive/2008/09/22/audio-input-and-output-formats.aspx
            int desiredOutputBitRate = 0;
            switch (this.Quality)
            {
                case ConversionQuality.Low:
                    desiredOutputBitRate = 96000;
                    break;

                case ConversionQuality.Medium:
                    desiredOutputBitRate = 128000;
                    break;

                case ConversionQuality.Best:
                    desiredOutputBitRate = 192000;
                    break;
            }

            AudioFileType target = (platform == MonoGamePlatform.iOS) ? AudioFileType.Mp3 : AudioFileType.Wav;
            // Create a new file if we need to.
            FileStream outputStream = input.FileType != target ? new FileStream(outputFilename, FileMode.Create) : null;
            
            if (input.FileType != target)
                AudioConverter.ConvertFile(inputFilename, outputStream, target, desiredOutputBitRate,
                            input.Format.BitsPerSample, input.Format.ChannelCount);
            else
                File.Copy(inputFilename, outputFilename, true);

            if (outputStream != null)
                outputStream.Close();

            context.AddOutputFile(outputFilename);

            // SoundEffectContent is a sealed class, construct it using reflection
            var type = typeof(SongContent);
            ConstructorInfo c = type.GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance,
                    null, new Type[] { typeof(string), typeof(int) }, null);

            var outputSongContent = (SongContent)c.Invoke(new Object[] { Path.GetFileName(outputFilename), (int)input.Duration.TotalMilliseconds });

            return outputSongContent;
        }

    }
}
