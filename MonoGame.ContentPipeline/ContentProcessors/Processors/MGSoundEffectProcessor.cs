using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Collections.ObjectModel;

namespace Microsoft.Xna.Framework.Content.Pipeline.Processors
{
    [ContentProcessor(DisplayName = "MGSoundEffectProcessor")]
    public class MGSoundEffectProcessor : SoundEffectProcessor
    {
        public override SoundEffectContent Process(Audio.AudioContent input, ContentProcessorContext context)
        {
            if (!context.BuildConfiguration.ToUpper().Contains("IOS"))
                return base.Process(input, context);

            // SoundEffectContent is a sealed class, construct it using reflection
            var type = typeof(SoundEffectContent);
            ConstructorInfo c = type.GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance,
                    null, new Type[] { typeof(ReadOnlyCollection<byte>), typeof(ReadOnlyCollection<byte>), typeof(int), typeof(int), typeof(int) }, null);

            var outputSoundEffectContent = (SoundEffectContent)c.Invoke(new Object[] { input.Format.NativeWaveFormat, input.Data, input.LoopStart, input.LoopLength, (int)input.Duration.TotalMilliseconds });

            return outputSoundEffectContent;
        
        }

    }
}
