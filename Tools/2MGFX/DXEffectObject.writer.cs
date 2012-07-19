using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TwoMGFX;

namespace Microsoft.Xna.Framework.Graphics
{
	public partial class DXEffectObject
	{
        /// <summary>
        /// Writes the effect for loading later.
        /// </summary>
        public void Write(BinaryWriter writer, Options options)
        {
            // Write a very simple header for identification and versioning.
            writer.Write(Header.ToCharArray());
            writer.Write((byte)Version);

            // Write an simple identifier for DX11 vs GLSL
            // so we can easily detect the correct shader type.
            var profile = (byte)( options.DX11Profile ? 1 : 0 );
            writer.Write(profile);

            // Write all the constant buffers.
            writer.Write((byte)ConstantBuffers.Count);
            foreach (var cbuffer in ConstantBuffers)
                cbuffer.Write(writer, options);

            // Write all the shaders.
            writer.Write((byte)Shaders.Count);
            foreach (var shader in Shaders)
                shader.Write(writer, options);

            // Write the parameters.
            WriteParameters(writer, Parameters, Parameters.Length);

            // Write the techniques.
            writer.Write((byte)Techniques.Length);
            foreach (var technique in Techniques)
            {
                writer.Write(technique.name);
                WriteAnnotations(writer, technique.annotation_handles);

                // Write the passes.
                writer.Write((byte)technique.pass_count);
                for (var p = 0; p < technique.pass_count; p++)
                {
                    var pass = technique.pass_handles[p];

                    writer.Write(pass.name);
                    WriteAnnotations(writer, pass.annotation_handles);

                    // Write the index for the vertex and pixel shaders.
                    var vertexShader = GetShaderIndex(STATE_CLASS.VERTEXSHADER, pass.states);
                    var pixelShader = GetShaderIndex(STATE_CLASS.PIXELSHADER, pass.states);
                    writer.Write((byte)vertexShader);
                    writer.Write((byte)pixelShader);

                    // Write the state objects too!
                }
            }
        }

        private static void WriteParameters(BinaryWriter writer, d3dx_parameter[] parameters, int count)
        {
            writer.Write((byte)count);
            for (var i = 0; i < count; i++)
                WriteParameter(writer, parameters[i]);
        }

        private static void WriteParameter(BinaryWriter writer, d3dx_parameter param)
        {
            var class_ = ToParameterClass(param.class_);
            var type = ToParameterType(param.type);
            writer.Write((byte)class_);
            writer.Write((byte)type);

            writer.Write(param.name);
            writer.Write(param.semantic);
            WriteAnnotations(writer, param.annotation_handles);

            writer.Write((byte)param.rows);
            writer.Write((byte)param.columns);

            // Write the elements or struct members.
            WriteParameters(writer, param.member_handles, (int)param.element_count);
            WriteParameters(writer, param.member_handles, (int)param.member_count);

            if (param.element_count == 0 && param.member_count == 0)
            {
                switch (type)
                {
                    case EffectParameterType.Bool:
                    case EffectParameterType.Int32:
                    case EffectParameterType.Single:
                        writer.Write((byte[])param.data);
                        break;
                }
            }
        }

        private static void WriteAnnotations(BinaryWriter writer, d3dx_parameter[] annotations)
        {
            var count = annotations == null ? 0 : annotations.Length;
            writer.Write((byte)count);
            for (var i = 0; i < count; i++)
                WriteParameter(writer, annotations[i]);
        }

        private static void WriteStates(BinaryWriter writer, d3dx_state [] states, uint count)
        {
            writer.Write((byte)count);
            for (var s = 0; s < count; s++)
            {
                var state = states[s];

                writer.Write((ushort)state.index);
                writer.Write((byte)state.operation);
                writer.Write((byte)state.type);
                WriteParameter(writer, state.parameter);
            }
        }
	}
}

