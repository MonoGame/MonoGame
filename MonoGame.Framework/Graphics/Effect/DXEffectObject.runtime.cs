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
                    var preshader = DXPreshader.CreatePreshader(reader);
                    param.data = new DXExpression(indexName, preshader);
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
        
        internal static BlendState GetBlendState(d3dx_state[] states)
        {
            BlendState result = null;

            foreach (var state in states)
            {
                var operation = DXEffectObject.state_table[state.operation];
                if (operation.class_ != DXEffectObject.STATE_CLASS.RENDERSTATE)
                    continue;

                if (state.type != DXEffectObject.STATE_TYPE.CONSTANT)
                    throw new NotSupportedException("We do not support shader expressions!");

                switch ((DXEffectObject.D3DRENDERSTATETYPE)operation.op)
                {
                    case DXEffectObject.D3DRENDERSTATETYPE.BLENDOP:
                        result = result ?? new BlendState();
                        result.ColorBlendFunction = (BlendFunction)state.parameter.data;
                        break;

                    case DXEffectObject.D3DRENDERSTATETYPE.SRCBLEND:
                        result = result ?? new BlendState();
                        result.ColorSourceBlend = (Blend)state.parameter.data;
                        break;

                    case DXEffectObject.D3DRENDERSTATETYPE.DESTBLEND:
                        result = result ?? new BlendState();
                        result.ColorDestinationBlend = (Blend)state.parameter.data;
                        break;

                    case DXEffectObject.D3DRENDERSTATETYPE.COLORWRITEENABLE:
                        result = result ?? new BlendState();
                        result.ColorWriteChannels = (ColorWriteChannels)state.parameter.data;
                        break;

                    case DXEffectObject.D3DRENDERSTATETYPE.ALPHABLENDENABLE:
                        break; //not sure what to do
                }
            }

            return result;
        }

        internal static RasterizerState GetRasterizerState(d3dx_state[] states)
        {
            RasterizerState result = null;

            foreach (var state in states)
            {
                var operation = DXEffectObject.state_table[state.operation];
                if (operation.class_ != DXEffectObject.STATE_CLASS.RENDERSTATE)
                    continue;

                if (state.type != DXEffectObject.STATE_TYPE.CONSTANT)
                    throw new NotSupportedException("We do not support shader expressions!");

                switch ((DXEffectObject.D3DRENDERSTATETYPE)operation.op)
                {
                    case DXEffectObject.D3DRENDERSTATETYPE.SCISSORTESTENABLE:
                        result = result ?? new RasterizerState();
                        result.ScissorTestEnable = (bool)state.parameter.data;
                        break;

                    case DXEffectObject.D3DRENDERSTATETYPE.CULLMODE:
                        result = result ?? new RasterizerState();
                        result.CullMode = (CullMode)state.parameter.data;
                        break;
                }
            }

            return result;
        }

        internal static DepthStencilState GetDepthStencilState(d3dx_state[] states)
        {
            DepthStencilState result = null;

            foreach (var state in states)
            {
                var operation = DXEffectObject.state_table[state.operation];
                if (operation.class_ != DXEffectObject.STATE_CLASS.RENDERSTATE)
                    continue;

                if (state.type != DXEffectObject.STATE_TYPE.CONSTANT)
                    throw new NotSupportedException("We do not support shader expressions!");

                switch ((DXEffectObject.D3DRENDERSTATETYPE)operation.op)
                {
                    case DXEffectObject.D3DRENDERSTATETYPE.STENCILENABLE:
                        result = result ?? new DepthStencilState();
                        result.StencilEnable = (bool)state.parameter.data;
                        break;

                    case DXEffectObject.D3DRENDERSTATETYPE.STENCILFUNC:
                        result = result ?? new DepthStencilState();
                        result.StencilFunction = (CompareFunction)state.parameter.data;
                        break;

                    case DXEffectObject.D3DRENDERSTATETYPE.STENCILPASS:
                        result = result ?? new DepthStencilState();
                        result.StencilPass = (StencilOperation)state.parameter.data;
                        break;

                    case DXEffectObject.D3DRENDERSTATETYPE.STENCILFAIL:
                        result = result ?? new DepthStencilState();
                        result.StencilFail = (StencilOperation)state.parameter.data;
                        break;

                    case DXEffectObject.D3DRENDERSTATETYPE.STENCILREF:
                        result = result ?? new DepthStencilState();
                        result.ReferenceStencil = (int)state.parameter.data;
                        break;
                }
            }

            return result;
        }


        internal static EffectParameterCollection CreateEffectParameters(DXEffectObject.d3dx_parameter[] parameters)
        {
            var collection = new EffectParameterCollection();
            if (parameters == null)
                return collection;

            for (var i = 0; i < parameters.Length; i++)
            {
                var element = DXEffectObject.CreateEffectParameter(parameters[i]);
                collection.Add(element);
            }

            return collection;
        }

        internal static EffectParameter CreateEffectParameter(DXEffectObject.d3dx_parameter parameter)
        {
            var class_ = DXEffectObject.ToParameterClass(parameter.class_);
            var type = DXEffectObject.ToParameterType(parameter.type);

            var annotations = DXEffectObject.CreateEffectAnnotations(parameter.annotation_handles);

            EffectParameterCollection elements;
            if (parameter.element_count > 0)
                elements = CreateEffectParameters(parameter.member_handles);
            else
                elements = new EffectParameterCollection();

            EffectParameterCollection structMembers;
            if (parameter.member_count > 0)
                structMembers = CreateEffectParameters(parameter.member_handles);
            else
                structMembers = new EffectParameterCollection();

            object data = null;
            if (elements.Count == 0 && structMembers.Count == 0)
            {
                //interpret data
                switch (class_)
                {
                    case EffectParameterClass.Scalar:
                        switch (type)
                        {
                            case EffectParameterType.Bool:
                                data = BitConverter.ToBoolean((byte[])parameter.data, 0);
                                break;
                            case EffectParameterType.Int32:
                                data = BitConverter.ToInt32((byte[])parameter.data, 0);
                                break;
                            case EffectParameterType.Single:
                                data = BitConverter.ToSingle((byte[])parameter.data, 0);
                                break;
                            case EffectParameterType.Void:
                                data = null;
                                break;
                            default:
                                throw new NotSupportedException();
                        }
                        break;

                    case EffectParameterClass.Vector:
                    case EffectParameterClass.Matrix:
                        switch (type)
                        {
                            case EffectParameterType.Single:
                                var vals = new float[parameter.rows * parameter.columns];
                                //transpose maybe?
                                for (var i = 0; i < vals.Length; i++)
                                    vals[i] = BitConverter.ToSingle((byte[])parameter.data, i * 4);
                                data = vals;
                                break;
                            default:
                                break;
                        }
                        break;

                    default:
                        data = parameter.data;
                        break;
                }
            }

            return new EffectParameter(class_, type, parameter.name,
                (int)parameter.rows, (int)parameter.columns, parameter.semantic,
                annotations, elements, structMembers, data);
        }

        internal static EffectTechniqueCollection CreateEffectTechniques(Effect effect, DXEffectObject.d3dx_technique[] techniques)
        {
            var collection = new EffectTechniqueCollection();
            if (techniques == null)
                return collection;

            for (var i = 0; i < techniques.Length; i++)
            {
                var technique = new EffectTechnique(
                    effect,
                    techniques[i].name,
                    DXEffectObject.CreateEffectPasses(effect, techniques[i].pass_handles),
                    DXEffectObject.CreateEffectAnnotations(techniques[i].annotation_handles));

                collection.Add(technique);
            }

            return collection;
        }

        internal static EffectPassCollection CreateEffectPasses(Effect effect, DXEffectObject.d3dx_pass[] passes)
        {
            var collection = new EffectPassCollection();
            if (passes == null)
                return collection;

            for (var i = 0; i < passes.Length; i++)
            {
                var pass = new EffectPass(
                    effect,
                    passes[i].name,
                    DXEffectObject.GetVertexShader(passes[i].states),
                    DXEffectObject.GetPixelShader(passes[i].states),
                    DXEffectObject.GetBlendState(passes[i].states),
                    DXEffectObject.GetDepthStencilState(passes[i].states),
                    DXEffectObject.GetRasterizerState(passes[i].states),
                    DXEffectObject.CreateEffectAnnotations(passes[i].annotation_handles));

                collection.Add(pass);
            }

            return collection;
        }

        internal static EffectAnnotationCollection CreateEffectAnnotations(DXEffectObject.d3dx_parameter[] parameters)
        {
            var collection = new EffectAnnotationCollection();
            if (parameters == null)
                return collection;

            for (var i = 0; i < parameters.Length; i++)
            {
                var annotation = new EffectAnnotation();
                collection.Add(annotation);
            }

            return collection;
        }
	}
}

