using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Microsoft.Xna.Framework.Graphics
{
	public partial class DXEffectObject
	{
        /// <summary>
        /// Writes the effect for loading later.
        /// </summary>
        public void Write(BinaryWriter writer)
        {
            // Write a very simple header for identification and versioning.
            writer.Write(Header.ToCharArray());
            writer.Write((byte)Version);

            // Write the shared objects... we will write the index to
            // the object later in the file.
            writer.Write((byte)Objects.Length);
            for (var o = 0; o < Objects.Length; o++)
            {
                var param = Objects[o];
                writer.Write(param != null);
                if (param == null)
                    continue;

                WriteParameter(writer, param);

                // Shove a index in here for later.
                param.object_id = o;
            }

            // Write the parameters.
            writer.Write((byte)Parameters.Length);
            foreach (var param in Parameters)
                WriteParameter(writer, param);

            // Write the techniques.
            writer.Write((byte)Techniques.Length);
            foreach (var technique in Techniques)
                WriteTechnique(writer, technique);
        }

        private static void WriteParameter(BinaryWriter writer, d3dx_parameter param)
        {
            writer.Write((byte)param.object_id);
            if (param.object_id != -1)
                return;

            writer.Write((byte)param.class_);
            writer.Write(param.name);
            writer.Write((byte)param.type);
            writer.Write((byte)param.rows);
            writer.Write((byte)param.columns);
            writer.Write(param.semantic);

            WriteAnnotations(writer, param.annotation_handles, param.annotation_count);

            // Note if this is a struct or not.
            var isStruct = param.member_count > 0;
            writer.Write(isStruct);

            // Write the correct number of elements/members.
            var count = isStruct ? param.member_count : param.element_count;
            writer.Write((byte)count);
            for(var i=0; i < count; i++)
                WriteParameter(writer, param.member_handles[i]);
            
            // Write the data payload.
            if (param.data is byte[])
            {
                writer.Write((byte)0);
                writer.Write((short)param.bytes);
                writer.Write((byte[])param.data);
            }
            else if (param.data is DXExpression)
            {
                writer.Write((byte)1);
                WriteExpression(writer, (DXExpression)param.data);
            }
            else if (param.data is DXShader)
            {
                writer.Write((byte)2);
                var shader = (DXShader)param.data;
                shader.Write(writer);
            }
            else if (param.data is d3dx_sampler)
            {
                writer.Write((byte)3);
                var sampler = (d3dx_sampler)param.data;
                WriteStates(writer, sampler.states, sampler.state_count);
            }
            else
            {
                throw new Exception("Unknown data type!");
            }
        }

        private static void WriteAnnotations(BinaryWriter writer, d3dx_parameter[] annotations, uint count)
        {
            writer.Write((byte)count);
            for (var i = 0; i < count; i++)
                WriteParameter(writer, annotations[i]);
        }

        private static void WriteTechnique(BinaryWriter writer, d3dx_technique technique)
        {
            writer.Write(technique.name);
            WriteAnnotations(writer, technique.annotation_handles, technique.annotation_count);

            // Write the passes.
            writer.Write((byte)technique.pass_count);
            for (var p = 0; p < technique.pass_count; p++)
            {
                var pass = technique.pass_handles[p];

                writer.Write(pass.name);
                WriteAnnotations(writer, pass.annotation_handles, pass.annotation_count);
                WriteStates(writer, pass.states, pass.state_count);
            }
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

        private static void WriteExpression(BinaryWriter writer, DXExpression expression)
        {
            writer.Write(expression.IndexName);
            writer.Write((ushort)expression.ExpressionCode.Length);
            writer.Write(expression.ExpressionCode);
        }
	}
}

