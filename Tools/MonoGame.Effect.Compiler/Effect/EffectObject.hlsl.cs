using System;
using System.Linq;
using MonoGame.Effect.Compiler;
using SharpDX.D3DCompiler;

namespace MonoGame.Effect
{
    partial class EffectObject
    {
        public static byte[] CompileHLSL(ShaderResult shaderResult, string shaderFunction, string shaderProfile, ref string errorsAndWarnings)
        {
            ShaderBytecode shaderByteCode;
            try
            {
                ShaderFlags shaderFlags = 0;

                // While we never allow preshaders, this flag is invalid for
                // the DX11 shader compiler which doesn't allow preshaders
                // in the first place.
                //shaderFlags |= ShaderFlags.NoPreshader;

                if (shaderResult.Profile == ShaderProfile.DirectX_11)
                    shaderFlags |= ShaderFlags.EnableBackwardsCompatibility;

                if (shaderResult.Debug)
                {
                    shaderFlags |= ShaderFlags.SkipOptimization;
                    shaderFlags |= ShaderFlags.Debug;
                }
                else
                {
                    shaderFlags |= ShaderFlags.OptimizationLevel3;
                }

                // Compile the shader into bytecode.                
                var result = (Environment.OSVersion.Platform != PlatformID.Unix) ?
                    ShaderBytecode.Compile(shaderResult.FileContent, shaderFunction, shaderProfile, shaderFlags, 0, null, null, shaderResult.FilePath) :
                    WineHelper.RunFxc2(shaderResult.FileContent, shaderFunction, shaderProfile, shaderFlags, shaderResult.FilePath);

                // Store all the errors and warnings to log out later.
                errorsAndWarnings += result.Message;

                if (result.Bytecode == null)
                    throw new ShaderCompilerException();
                
                shaderByteCode = result.Bytecode;
                //var source = shaderByteCode.Disassemble();
            }
            catch (Exception ex)
            {
                errorsAndWarnings += ex.Message;
                throw new ShaderCompilerException();
            }

            // Return a copy of the shader bytecode.
            return shaderByteCode.Data.ToArray();
        }
    }
}
