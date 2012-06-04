using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Microsoft.Xna.Framework.Graphics
{
    partial class DXEffectObject
    {
        public List<ConstantBuffer> ConstantBuffers { get; private set; }
        
        private DXEffectObject()
        {
        }

        static public DXEffectObject FromShaderInfo(TwoMGFX.ShaderInfo shaderInfo)
        {
            var effect = new DXEffectObject();

            // These are filled out as we process stuff.
            effect.ConstantBuffers = new List<ConstantBuffer>();
            effect.Shaders = new List<DXShader>();

            // Go thru the techniques and that will find all the 
            // shaders and constant buffers.
            effect.Techniques = new d3dx_technique[shaderInfo.Techniques.Count];
            for (var i = 0; i < shaderInfo.Techniques.Count; i++)
            {
                var tinfo = shaderInfo.Techniques[i]; ;

                var technique = new d3dx_technique();
                technique.name = tinfo.name;
                technique.pass_count = (uint)tinfo.Passes.Count;
                technique.pass_handles = new d3dx_pass[tinfo.Passes.Count];

                for (var p = 0; p < tinfo.Passes.Count; p++)
                {
                    var pinfo = tinfo.Passes[p];;

                    var pass = new d3dx_pass();
                    pass.name = pinfo.name ?? string.Empty;
                    pass.states = new d3dx_state[2];
                    pass.state_count = 2;

                    // Create the shaders.
                    if (string.IsNullOrEmpty(pinfo.psFunction) || string.IsNullOrEmpty(pinfo.vsFunction))
                        throw new Exception("Passed must have a vertex and pixel shader assigned!");

                    pass.states[0] = effect.CreateShader(shaderInfo, pinfo.psFunction, pinfo.psModel, false);
                    pass.states[1] = effect.CreateShader(shaderInfo, pinfo.vsFunction, pinfo.vsModel, true);

                    technique.pass_handles[p] = pass;
                }

                effect.Techniques[i] = technique;
            }
                
            // Make the list of parameters by combining all the
            // constant buffers ignoring the buffer offsets.
            var parameters = new List<d3dx_parameter>();
            for (var c = 0; c < effect.ConstantBuffers.Count; c++ )
            {
                var cb = effect.ConstantBuffers[c];

                for (var i = 0; i < cb.Parameters.Count; i++ )
                {
                    var param = cb.Parameters[i];

                    var match = parameters.FindIndex(e => e.name == param.name);
                    if (match == -1)
                    {
                        cb.ParameterIndex.Add(parameters.Count);
                        parameters.Add(param);
                    }
                    else
                    {
                        // TODO: Make sure the type and size of 
                        // the parameter match up!
                        cb.ParameterIndex.Add(match);
                    }
                }
            }

            // Add the texture parameters from the samplers.
            foreach (var shader in effect.Shaders)
            {
                for (var s = 0; s < shader._samplers.Length; s++)
                {
                    var sampler = shader._samplers[s];

                    var match = parameters.FindIndex(e => e.name == sampler.parameterName);
                    if (match == -1)
                    {
                        shader._samplers[s].parameter = parameters.Count;

                        var param = new d3dx_parameter();
                        param.class_ = D3DXPARAMETER_CLASS.OBJECT;
                        param.type = D3DXPARAMETER_TYPE.TEXTURE2D; // TODO: Fix this right!
                        param.name = sampler.parameterName;
                        param.semantic = string.Empty;

                        parameters.Add(param);
                    }
                    else
                    {
                        // TODO: Make sure the type and size of 
                        // the parameter match up!

                        shader._samplers[s].parameter = match;
                    }
                }
            }

            // TODO: Annotations are part of the .FX format and
            // not a part of shaders... we need to implement them
            // in our mgfx parser if we want them back.
            /*
            // Now find the things we could not in the constant
            // buffer reflection interface...  semantics and annotations.
            for (var i = 0; i < parameters.Count; i++)
            {
                var vdesc = dxeffect.GetVariableByName(parameters[i].name);
                if (!vdesc.IsValid)
                    continue;

                parameters[i].semantic = vdesc.Description.Semantic ?? string.Empty;

                // TODO: Annotations!
            }
            */

            effect.Parameters = parameters.ToArray();

            return effect;
        }
        
        private d3dx_state CreateShader(TwoMGFX.ShaderInfo shaderInfo, string shaderFunction, string shaderProfile, bool isVertexShader)
        {
            // Compile the shader.
            SharpDX.D3DCompiler.ShaderBytecode shaderByteCode;
            try
            {
                SharpDX.D3DCompiler.ShaderFlags shaderFlags = 0;
                shaderFlags |= SharpDX.D3DCompiler.ShaderFlags.EnableBackwardsCompatibility;
                shaderFlags |= SharpDX.D3DCompiler.ShaderFlags.OptimizationLevel3;
                //shaderFlags |= SharpDX.D3DCompiler.ShaderFlags.NoPreshader;
                //shaderFlags |= SharpDX.D3DCompiler.ShaderFlags.PackMatrixRowMajor;
                //shaderFlags |= SharpDX.D3DCompiler.ShaderFlags.WarningsAreErrors;
                //shaderFlags |= SharpDX.D3DCompiler.ShaderFlags.EnableBackwardsCompatibility;
                //shaderFlags |= SharpDX.D3DCompiler.ShaderFlags.SkipValidation;
                shaderFlags |= SharpDX.D3DCompiler.ShaderFlags.Debug;

                // First compile the effect into bytecode.                
                using (var includer = new TwoMGFX.CompilerInclude())
                {
                    var result = SharpDX.D3DCompiler.ShaderBytecode.Compile(
                        shaderInfo.fileContent, 
                        shaderFunction, 
                        shaderProfile, 
                        shaderFlags, 
                        0, 
                        null, 
                        includer,
                        shaderInfo.fileName);

                    if (result.HasErrors)
                        throw new Exception(result.Message);

                    shaderByteCode = result.Bytecode;
                }
            }
            catch (Exception ex)
            {
                throw ex;
                //Console.WriteLine("Failed to compile the input file '{0}'!", shaderFileName);
                //Console.WriteLine(ex.Message);
                //return 1;
            }

            // Get the shader bytecode.
            var bytecode = new byte[shaderByteCode.Data.Length];
            shaderByteCode.Data.Read(bytecode, 0, bytecode.Length);

            // First look to see if we already created this same shader.
            DXShader dxShader = null;
            foreach (var shader in Shaders)
            {
                if (bytecode.SequenceEqual(shader.Bytecode))
                {
                    dxShader = shader;
                    break;
                }
            }

            // Create a new shader.
            if ( dxShader == null )
            {
                if (shaderInfo.DX11Profile)
                    dxShader = DXShader.CreateHLSL(bytecode, isVertexShader, ConstantBuffers, Shaders.Count);
                else
                    dxShader = DXShader.CreateGLSL(bytecode, isVertexShader, ConstantBuffers, Shaders.Count);

                Shaders.Add(dxShader);
            }
            
            //var assmbly = desc.Bytecode.Disassemble();
            //var buffer = reflection.GetConstantBuffer(0);

            var state = new d3dx_state();
            state.index = 0;
            state.type = STATE_TYPE.CONSTANT;
            state.operation = isVertexShader ? (uint)146 : (uint)147;

            state.parameter = new d3dx_parameter();
            state.parameter.name = string.Empty;
            state.parameter.semantic = string.Empty;
            state.parameter.class_ = D3DXPARAMETER_CLASS.OBJECT;
            state.parameter.type = isVertexShader ? D3DXPARAMETER_TYPE.VERTEXSHADER : D3DXPARAMETER_TYPE.PIXELSHADER;
            state.parameter.rows = 0;
            state.parameter.columns = 0;
            state.parameter.data = dxShader.SharedIndex;

            return state;
        }
    }
}
