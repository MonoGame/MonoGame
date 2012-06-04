using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using Microsoft.Xna.Framework.Graphics;
using TwoMGFX;
using System.IO;
using System.Collections;

namespace MonoGameContentProcessors.Processors
{
    [ContentProcessor(DisplayName = "MonoGame Effect")]
    class MGEffectProcessor : EffectProcessor
    {
        public override CompiledEffectContent Process(EffectContent input, ContentProcessorContext context)
        {
            //System.Diagnostics.Debugger.Launch();

            // If this isn't a MonoGame platform then do the default processing.
            var platform = ContentHelper.GetMonoGamePlatform();
            if (platform == MonoGamePlatform.None)
                return base.Process(input, context);

            var options = new Options();
            options.SourceFile = input.Identity.SourceFilename;
            options.DX11Profile = platform == MonoGamePlatform.Windows8 ? true : false;
            options.OutputFile = context.OutputFilename;

            // Parse the MGFX file expanding includes, macros, and returning the techniques.
            ShaderInfo shaderInfo;
            try
            {
                shaderInfo = ShaderInfo.FromFile(options.SourceFile, options);
            }
            catch (Exception ex)
            {
                throw new InvalidContentException("Failed to parse the effect!", ex);
            }

            // Create the effect object.
            DXEffectObject effect;
            try
            {
                effect = DXEffectObject.FromShaderInfo(shaderInfo);
            }
            catch (Exception ex)
            {
                throw new InvalidContentException("Failed to create the effect!", ex);
            }

            // Write out the effect to a runtime format.
            CompiledEffectContent result;
            try
            {
                using (var stream = new MemoryStream())
                {
                    using (var writer = new BinaryWriter(stream))
                        effect.Write(writer, options);

                    result = new CompiledEffectContent(stream.GetBuffer());
                }
            }
            catch (Exception ex)
            {
                throw new InvalidContentException("Failed to serialize the effect!", ex);
            }

            return result;
        }
    }
}
