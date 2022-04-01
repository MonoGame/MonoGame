using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace MonoGame.Effect
{
    internal partial class ConstantBufferData
    {
        public ConstantBufferData (ShaderConductor.UniformBuffer ub, List<(string type, string name, string value)> shaderParamInitializations, ref string errorsAndWarnings)
        {
            Name = ub.blockName;
            InstanceName = ub.instanceName;
            Size = ub.byteSize;
            BindingSlot = ub.slot;

            ParameterIndex = new List<int>();
            var parameters = new List<EffectObject.d3dx_parameter>();

            // Gather all the parameters.
            foreach (ShaderConductor.Parameter p in ub.parameters)
            {
                var dxParam = CreateDXParamForConductorParam(p);

                // default values are only supported for parameters defined at global scope (not part of a constant buffer declaration)
                if (Name == "type_Globals")
                    dxParam.data = FindDefaultValueForParam(dxParam, shaderParamInitializations, ref errorsAndWarnings);

                parameters.Add(dxParam);
            }

            // Sort them by the offset for some consistent results.
            Parameters = parameters.OrderBy(e => e.bufferOffset).ToList();

            // Store the parameter offsets.
            ParameterOffset = new List<int>();
            foreach (var param in Parameters)
                ParameterOffset.Add(param.bufferOffset);
        }

        private static EffectObject.d3dx_parameter CreateDXParamForConductorParam(ShaderConductor.Parameter p)
        {
            var param = new EffectObject.d3dx_parameter();

            param.rows = (uint)p.columns; // OpenGL to DirectX requires transpose
            param.columns = (uint)p.rows; // OpenGL to DirectX requires transpose
            param.name = p.name;
            param.semantic = string.Empty;
            param.bufferOffset = p.offset;
            param.semantic = string.Empty;
            param.member_count = 0;

            switch (p.type)
            {
                case 1: param.type = EffectObject.D3DXPARAMETER_TYPE.BOOL; break;
                case 2: param.type = EffectObject.D3DXPARAMETER_TYPE.INT; break;
                case 3: param.type = EffectObject.D3DXPARAMETER_TYPE.FLOAT; break;
                default:
                    throw new Exception("Unsupported parameter class!");
            }

            if (p.columns == 1)
            {
                param.class_ = p.rows == 1 ?
                    EffectObject.D3DXPARAMETER_CLASS.SCALAR :
                    EffectObject.D3DXPARAMETER_CLASS.VECTOR;
            }
            else
                param.class_ = EffectObject.D3DXPARAMETER_CLASS.MATRIX_COLUMNS;

            // Flatten the array, MonoGame uses 1D arrays  
            int elementCount = 1;
            for (int dim = 0; dim < p.arrayDimensions; dim++)
                elementCount *= p.arraySize[dim];

            param.element_count = elementCount > 1 ? (uint)elementCount : 0;

            if (param.member_count > 0)
            {
                param.member_handles = new EffectObject.d3dx_parameter[0];
                /*
                param.member_handles = new EffectObject.d3dx_parameter[param.member_count];
                for (var i = 0; i < param.member_count; i++)
                {
                    var mparam = GetParameterFromType(type.GetMemberType(i));
                    mparam.name = type.GetMemberTypeName(i) ?? string.Empty;
                    param.member_handles[i] = mparam;
                }*/
            }
            else
            {
                param.member_handles = CreateArrayFromParameter(param);
            }

            return param;
        }

        public byte[] FindDefaultValueForParam(EffectObject.d3dx_parameter param, List<(string type, string name, string value)> shaderParamInitializations, ref string errorsAndWarnings)
        {
            var data = new byte[param.rows * param.columns * 4];

            if (param.type == EffectObject.D3DXPARAMETER_TYPE.BOOL ||
                param.type == EffectObject.D3DXPARAMETER_TYPE.INT  ||
                param.type == EffectObject.D3DXPARAMETER_TYPE.FLOAT)
            {
                var paramInit = shaderParamInitializations.Find(p => p.name == param.name);
                if (paramInit != default((string, string, string)))
                {
                    // remove everythig from the initializer except values and commas
                    string allValues = paramInit.value;

                    int startInd = allValues.IndexOfAny(new char[]{'(', '{'});
                    if (startInd >= 0)
                        allValues = allValues.Substring(startInd + 1);

                    int endInd = allValues.LastIndexOfAny(new char[] { ')', '}' });
                    if (endInd >= 0)
                        allValues = allValues.Substring(0, endInd);

                    string[] values = allValues.Split(',');
                    bool error = values.Length != param.rows * param.columns;

                    if (!error)
                    {
                        for (uint i = 0; i < values.Length; i++)
                        {
                            var style = NumberStyles.Any;
                            var cult = CultureInfo.InvariantCulture;

                            string val = values[i].Trim();

                            // remove hex specifier
                            if (val.StartsWith("0x", true, cult))
                            {
                                val = val.Substring(2);
                                style = NumberStyles.HexNumber;
                            }

                            // remove floating point f from the end
                            if (val.EndsWith("f", true, cult) && style != NumberStyles.HexNumber)
                                val = val.Substring(0, val.Length - 1);

                            byte[] valData = null;

                            switch (paramInit.type)
                            {
                                case "int":
                                    if (int.TryParse(val, style, cult, out int valInt))
                                        valData = BitConverter.GetBytes(valInt);
                                    break;
                                case "uint":
                                    if (uint.TryParse(val, style, cult, out uint valUInt))
                                        valData = BitConverter.GetBytes(valUInt);
                                    break;
                                case "float":
                                    if (float.TryParse(val, style, cult, out float valFloat))
                                        valData = BitConverter.GetBytes(valFloat);
                                    break;
                                case "bool":
                                    if (bool.TryParse(val, out bool valBool))
                                        valData = new byte[3].Concat(BitConverter.GetBytes(valBool)).ToArray(); // BitConverter returns one byte for bools, we need 4
                                    else if (uint.TryParse(val, style, cult, out uint valBoolAsUInt)) // a bool can also be initialized with a number
                                        valData = BitConverter.GetBytes(valBoolAsUInt > 0 ? 1 : 0); 
                                    break;
                            }

                            if (valData == null || valData.Length != 4)
                            {
                                error = true;
                                Array.Clear(data, 0, data.Length);
                                break;
                            }

                            // transpose matrices
                            uint destInd = i;
                            if (param.rows > 1 && param.columns > 1)
                            {
                                uint c = i % param.rows;
                                uint r = i / param.rows;

                                destInd = c * param.columns + r;
                            }

                            Array.Copy(valData, 0, data, destInd * 4, 4);
                        }
                    }

                    if (error)
                    {
                        errorsAndWarnings += "warning: failed parsing default value for shader parameter: '"
                            + paramInit.name + " = " + paramInit.value
                            + "'\nNo expressions or identifiers allowed, simple numbers only.\n";
                    }
                }
            }

            return data;
        }
    }
}
