using System;
using System.Collections.Generic;
using System.IO;

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
            writer.Write(Header);
            writer.Write((byte)Version);

            // Write the parameters.
            writer.Write(Parameters.Length);
            foreach (var param in Parameters)
                WriteParameter(writer, param);

            // Write the techniques.
            writer.Write(Techniques.Length);
            foreach (var technique in Techniques)
                WriteTechnique(writer, technique);
        }

        private static void WriteParameter(BinaryWriter writer, d3dx_parameter param)
        {
            writer.Write(param.name);
            writer.Write((byte)param.class_);
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

            // Now write the data.
            writer.Write((short)param.bytes);
            if ( param.bytes > 0 )
                writer.Write((byte[])param.data);
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

                writer.Write((byte)pass.state_count);
                for (var s = 0; s < pass.state_count; s++)
                {
                    var state = pass.states[s];

                    writer.Write((byte)state.type);
                    writer.Write((byte)state.operation);

                    WriteParameter(writer, state.parameter);
                }
            }             
        }
	}
}

