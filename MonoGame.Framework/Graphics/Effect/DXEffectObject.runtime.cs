using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;

namespace Microsoft.Xna.Framework.Graphics
{
	public partial class DXEffectObject
	{
        public static DXEffectObject FromMGFX(byte[] effectFile)
        {
            using ( var stream = new MemoryStream(effectFile))
            using (var reader = new BinaryReader(stream))
                return new DXEffectObject(reader);
        }

        private DXEffectObject(BinaryReader reader)
        {
            // Check the header to make sure the file and version is correct!
            var header = new string(reader.ReadChars(Header.Length));
            var version = (int)reader.ReadByte();
            if (header != Header || version != Version)
                throw new Exception("Unsupported MGFX format!");

            // Read the shared objects first.
            Objects = new d3dx_parameter[(int)reader.ReadByte()];
            for (var o = 0; o < Objects.Length; o++)
            {
                if (reader.ReadBoolean())
                    Objects[o] = ReadParameter(reader);
            }

            // Read the parameters.
            Parameters = new d3dx_parameter[(int)reader.ReadByte()];
            for (var p = 0; p < Parameters.Length; p++)
                Parameters[p] = ReadParameter(reader);

            // Read the techniques.
            Techniques = new d3dx_technique[(int)reader.ReadByte()];
            for (var t = 0; t < Techniques.Length; t++)
                Techniques[t] = ReadTechnique(reader);
        }

        private d3dx_parameter ReadParameter(BinaryReader reader)
        {
            d3dx_parameter param;

            var object_id = (int)reader.ReadByte();
            if (object_id != 255)
            {
                param = Objects[object_id];
                Debug.Assert(param != null, "Invalid object index!");
                return param;
            }

            param = new d3dx_parameter();
            param.class_ = (D3DXPARAMETER_CLASS)reader.ReadByte();
            param.name = reader.ReadString();
            param.type = (D3DXPARAMETER_TYPE)reader.ReadByte();
            param.rows = (uint)reader.ReadByte();
            param.columns = (uint)reader.ReadByte();
            param.semantic = reader.ReadString();

            ReadAnnotations(reader, ref param.annotation_handles, ref param.annotation_count);

            var isStruct = reader.ReadBoolean();
            var count = (uint)reader.ReadByte();
            if (count > 0)
            {
                if (isStruct)
                    param.member_count = count;
                else
                    param.element_count = count;

                param.member_handles = new d3dx_parameter[count];
                for (var i = 0; i < count; i++)
                    param.member_handles[i] = ReadParameter(reader);
            }

            var dataType = reader.ReadByte();
            switch (dataType)
            {
                case 0:
                    param.bytes = (uint)reader.ReadUInt16();
                    if (param.bytes > 0)
                        param.data = reader.ReadBytes((int)param.bytes);
                    break;

                case 1:                    
                    var indexName = reader.ReadString();
                    var bytes = (int)reader.ReadUInt16();
                    var expressionCode = reader.ReadBytes(bytes);
                    param.data = new DXExpression(indexName, expressionCode);
                    break;

                case 2:
                    param.data = new DXShader(reader);
                    break;

                case 3:
                    var sampler = new d3dx_sampler();
                    ReadStates(reader, ref sampler.states, ref sampler.state_count);
                    param.data = sampler;
                    break;

                default:
                    throw new Exception("Unknown data type!");
            }

            return param;
        }

        private void ReadAnnotations(BinaryReader reader, ref d3dx_parameter[] annotations, ref uint count)
        {
            count = (uint)reader.ReadByte();
            if (count == 0)
                return;

            annotations = new d3dx_parameter[count];
            for (var i = 0; i < count; i++)
                annotations[i] = ReadParameter(reader);
        }

        private void ReadStates(BinaryReader reader, ref d3dx_state [] states, ref uint count)
        {
            count = (uint)reader.ReadByte();
            states = new d3dx_state[count];

            for (var s = 0; s < count; s++)
            {
                var state = new d3dx_state();

                state.index = (uint)reader.ReadUInt16();
                state.operation = (uint)reader.ReadByte();
                state.type = (STATE_TYPE)reader.ReadByte();
                state.parameter = ReadParameter(reader);

                states[s] = state;
            }
        }

        private d3dx_technique ReadTechnique(BinaryReader reader)
        {
            var technique = new d3dx_technique();

            technique.name = reader.ReadString();

            ReadAnnotations(reader, ref technique.annotation_handles, ref technique.annotation_count);

            technique.pass_count = (uint)reader.ReadByte();
            technique.pass_handles = new d3dx_pass[technique.pass_count];
            for (var p = 0; p < technique.pass_count; p++)
            {
                var pass = new d3dx_pass();

                pass.name = reader.ReadString();

                ReadAnnotations(reader, ref pass.annotation_handles, ref pass.annotation_count);

                ReadStates(reader, ref pass.states, ref pass.state_count);

                technique.pass_handles[p] = pass;
            }

            return technique;
        }

	}
}

