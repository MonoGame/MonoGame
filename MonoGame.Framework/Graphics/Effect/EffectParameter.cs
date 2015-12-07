using System;
using System.Text;
using System.Diagnostics;

namespace Microsoft.Xna.Framework.Graphics
{
    [DebuggerDisplay("{DebugDisplayString}")]
	public class EffectParameter
	{
        /// <summary>
        /// The next state key used when an effect parameter
        /// is updated by any of the 'set' methods.
        /// </summary>
        internal static ulong NextStateKey { get; private set; }

        internal EffectParameter(   EffectParameterClass class_, 
                                    EffectParameterType type, 
                                    string name, 
                                    int rowCount, 
                                    int columnCount,
                                    string semantic, 
                                    EffectAnnotationCollection annotations,
                                    EffectParameterCollection elements,
                                    EffectParameterCollection structMembers,
                                    object data )
		{
            ParameterClass = class_;
            ParameterType = type;

            Name = name;
            Semantic = semantic;
            Annotations = annotations;

            RowCount = rowCount;
			ColumnCount = columnCount;

            Elements = elements;
            StructureMembers = structMembers;

            Data = data;
            StateKey = unchecked(NextStateKey++);
		}

        internal EffectParameter(EffectParameter cloneSource)
        {
            // Share all the immutable types.
            ParameterClass = cloneSource.ParameterClass;
            ParameterType = cloneSource.ParameterType;
            Name = cloneSource.Name;
            Semantic = cloneSource.Semantic;
            Annotations = cloneSource.Annotations;
            RowCount = cloneSource.RowCount;
            ColumnCount = cloneSource.ColumnCount;

            // Clone the mutable types.
            Elements = cloneSource.Elements.Clone();
            StructureMembers = cloneSource.StructureMembers.Clone();

            // The data is mutable, so we have to clone it.
            var array = cloneSource.Data as Array;
            if (array != null)
                Data = array.Clone();
            StateKey = unchecked(NextStateKey++);
        }

		public string Name { get; private set; }

        public string Semantic { get; private set; }

		public EffectParameterClass ParameterClass { get; private set; }

		public EffectParameterType ParameterType { get; private set; }

		public int RowCount { get; private set; }

        public int ColumnCount { get; private set; }

        public EffectParameterCollection Elements { get; private set; }

        public EffectParameterCollection StructureMembers { get; private set; }

        public EffectAnnotationCollection Annotations { get; private set; }


        // TODO: Using object adds alot of boxing/unboxing overhead
        // and garbage generation.  We should consider a templated
        // type implementation to fix this!

        internal object Data { get; private set; }

        /// <summary>
        /// The current state key which is used to detect
		/// if the parameter value has been changed.
        /// </summary>
        internal ulong StateKey { get; private set; }

        /// <summary>
        /// Property referenced by the DebuggerDisplayAttribute.
        /// </summary>
        private string DebugDisplayString
        {
            get
            {
                var semanticStr = string.Empty;
                if (!string.IsNullOrEmpty(Semantic))                
                    semanticStr = string.Concat(" <", Semantic, ">");

                string valueStr;
                if (Data == null)
                    valueStr = "(null)";
                else
                {
                    switch (ParameterClass)
                    {
                        // Object types are stored directly in the Data property.
                        // Display Data's string value.
                        case EffectParameterClass.Object:
                            valueStr = Data.ToString();
                            break;

                        // Matrix types are stored in a float[16] which we don't really have room for.
                        // Display "...".
                        case EffectParameterClass.Matrix:
                            valueStr = "...";
                            break;

                        // Scalar types are stored as a float[1].
                        // Display the first (and only) element's string value.                    
                        case EffectParameterClass.Scalar:
                            valueStr = (Data as Array).GetValue(0).ToString();
                            break;

                        // Vector types are stored as an Array<Type>.
                        // Display the string value of each array element.
                        case EffectParameterClass.Vector:
                            var array = Data as Array;
                            var arrayStr = new string[array.Length];
                            var idx = 0;
                            foreach (var e in array)
                            {
                                arrayStr[idx] = array.GetValue(idx).ToString();
                                idx++;
                            }

                            valueStr = string.Join(" ", arrayStr);
                            break;

                        // Handle additional cases here...
                        default:
                            valueStr = Data.ToString();
                            break;
                    }
                }
                
                return string.Concat("[", ParameterClass, " ", ParameterType, "]", semanticStr, " ", Name, " : ", valueStr);
            }
        }


        public bool GetValueBoolean ()
		{
            if (ParameterClass != EffectParameterClass.Scalar || ParameterType != EffectParameterType.Bool)
                throw new InvalidCastException();

#if DIRECTX
            return ((int[])Data)[0] != 0;
#else
            // MojoShader encodes even booleans into a float.
            return ((float[])Data)[0] != 0.0f;
#endif
        }
        
        /*
		public bool[] GetValueBooleanArray ()
		{
			throw new NotImplementedException();
		}
        */

		public int GetValueInt32 ()
		{
            if (ParameterClass != EffectParameterClass.Scalar || ParameterType != EffectParameterType.Int32)
                throw new InvalidCastException();

#if DIRECTX
            return ((int[])Data)[0];
#else
            // MojoShader encodes integers into a float.
            return (int)((float[])Data)[0];
#endif
        }
        
        /*
		public int[] GetValueInt32Array ()
		{
			throw new NotImplementedException();
		}
        */

		public Matrix GetValueMatrix ()
		{
            if (ParameterType != EffectParameterType.Single)
                throw new InvalidCastException();

            if (Data == null)
                throw new InvalidCastException();

            var result = default(Matrix);

            var floatData = (float[])Data;

            if (ParameterClass == EffectParameterClass.Scalar)
            {
                result.M11 =
                result.M12 =
                result.M13 =
                result.M14 =
                result.M21 =
                result.M22 =
                result.M23 =
                result.M24 =
                result.M31 =
                result.M32 =
                result.M33 =
                result.M34 =
                result.M41 =
                result.M42 =
                result.M43 =
                result.M44 = floatData[0];
            }
            else if (ParameterClass == EffectParameterClass.Matrix)
            {
                if (RowCount == 4 && ColumnCount == 4)
                {
                    result.M11 = floatData[0];
                    result.M21 = floatData[1];
                    result.M31 = floatData[2];
                    result.M41 = floatData[3];

                    result.M12 = floatData[4];
                    result.M22 = floatData[5];
                    result.M32 = floatData[6];
                    result.M42 = floatData[7];

                    result.M13 = floatData[8];
                    result.M23 = floatData[9];
                    result.M33 = floatData[10];
                    result.M43 = floatData[11];

                    result.M14 = floatData[12];
                    result.M24 = floatData[13];
                    result.M34 = floatData[14];
                    result.M44 = floatData[15];
                }
                else if (RowCount == 4 && ColumnCount == 3)
                {
                    result.M11 = floatData[0];
                    result.M21 = floatData[1];
                    result.M31 = floatData[2];
                    result.M41 = floatData[3];

                    result.M12 = floatData[4];
                    result.M22 = floatData[5];
                    result.M32 = floatData[6];
                    result.M42 = floatData[7];

                    result.M13 = floatData[8];
                    result.M23 = floatData[9];
                    result.M33 = floatData[10];
                    result.M43 = floatData[11];
                }
                else if (RowCount == 3 && ColumnCount == 4)
                {
                    result.M11 = floatData[0];
                    result.M21 = floatData[1];
                    result.M31 = floatData[2];

                    result.M12 = floatData[3];
                    result.M22 = floatData[4];
                    result.M32 = floatData[5];

                    result.M13 = floatData[6];
                    result.M23 = floatData[7];
                    result.M33 = floatData[8];

                    result.M14 = floatData[9];
                    result.M24 = floatData[10];
                    result.M34 = floatData[11];
                }
                else if (RowCount == 3 && ColumnCount == 3)
                {
                    result.M11 = floatData[0];
                    result.M21 = floatData[1];
                    result.M31 = floatData[2];

                    result.M12 = floatData[3];
                    result.M22 = floatData[4];
                    result.M32 = floatData[5];

                    result.M13 = floatData[6];
                    result.M23 = floatData[7];
                    result.M33 = floatData[8];
                }
                else if (RowCount == 3 && ColumnCount == 2)
                {
                    result.M11 = floatData[0];
                    result.M21 = floatData[1];
                    result.M31 = floatData[2];

                    result.M12 = floatData[3];
                    result.M22 = floatData[4];
                    result.M32 = floatData[5];
                }
                else
                {
                    throw new InvalidCastException();
                }
            }
            else
            {
                throw new InvalidCastException();
            }

            return result;
		}
        
		public Matrix[] GetValueMatrixArray (int count)
		{
            if (count <= 0)
                throw new ArgumentOutOfRangeException ("count");

            if (ParameterClass != EffectParameterClass.Matrix || ParameterType != EffectParameterType.Single)
                throw new InvalidCastException();

            if (Elements == null || Elements.Count == 0)
                throw new InvalidCastException();

            var ret = new Matrix[count];

            var size = Math.Min(count, Elements.Count);
            for (var i = 0; i < size; i++)
                ret[i] = Elements[i].GetValueMatrix();

		    return ret;
		}

        /*
		public Matrix GetValueMatrixTranspose ()
		{
			throw new NotImplementedException();
		}
        */

        /*
		public Matrix[] GetValueMatrixTransposeArray (int count)
		{
			throw new NotImplementedException();
		}
        */

		public Quaternion GetValueQuaternion ()
		{
            if (ParameterClass != EffectParameterClass.Vector || ParameterType != EffectParameterType.Single)
                throw new InvalidCastException();

            var vecInfo = (float[])Data;
            return new Quaternion(vecInfo[0], vecInfo[1], vecInfo[2], vecInfo[3]);
        }

        /*
		public Quaternion[] GetValueQuaternionArray ()
		{
			throw new NotImplementedException();
		}
        */

		public Single GetValueSingle ()
		{
            // TODO: Should this fetch int and bool as a float?
            if (ParameterClass != EffectParameterClass.Scalar || ParameterType != EffectParameterType.Single)
                throw new InvalidCastException();

			return ((float[])Data)[0];
		}

		public Single[] GetValueSingleArray (int count)
		{
            if (count <= 0)
                throw new ArgumentOutOfRangeException ("count");

			if (Elements != null && Elements.Count > 0)
            {
                var size = Math.Min(RowCount * ColumnCount * Elements.Count, count);
                var ret = new Single[count];
                var pos = 0;
				for (int i=0; i<Elements.Count && pos<size; i++)
                {
                    var elmArray = Elements[i].GetValueSingleArray(Elements[i].ColumnCount * Elements[i].RowCount);
                    for (var j = 0; j < elmArray.Length && pos < size; j++)
                    {
						ret[RowCount*ColumnCount*i+j] = elmArray[j];
						pos++;
                    }
				}
				return ret;
			}
			
			var otherArray = new Single[count];
			switch(ParameterClass) 
            {
			case EffectParameterClass.Scalar:
				otherArray[0] = GetValueSingle();
				break;
            case EffectParameterClass.Vector:
			case EffectParameterClass.Matrix:
                    if (Data is Matrix)
                    {
                        var source = Matrix.ToFloatArray((Matrix) Data);
                        Array.Copy(source, otherArray, Math.Min(source.Length, count));
                    }
                    else
                    {
                        var fData = (float[]) Data;
                        var size = Math.Min(fData.Length, count);
                        var pos = 0;
                        for (int i = 0; i < RowCount && pos < size; i++)
                        {
                            for (int j = 0; j < ColumnCount && pos < size; j++)
                            {
                                otherArray[pos] = fData[j * RowCount + i];
                                pos++;
                            }
                        }
                    }
                    break;
			default:
				throw new NotImplementedException();
			}
            return otherArray;
		}

		public string GetValueString ()
		{
            if (ParameterClass != EffectParameterClass.Object || ParameterType != EffectParameterType.String)
                throw new InvalidCastException();

		    return ((string[])Data)[0];
		}

		public Texture2D GetValueTexture2D ()
		{
            if (ParameterClass != EffectParameterClass.Object || ParameterType != EffectParameterType.Texture2D)
                throw new InvalidCastException();

			return (Texture2D)Data;
		}

#if !GLES
	    public Texture3D GetValueTexture3D ()
	    {
            if (ParameterClass != EffectParameterClass.Object || ParameterType != EffectParameterType.Texture3D)
                throw new InvalidCastException();

            return (Texture3D)Data;
	    }
#endif

		public TextureCube GetValueTextureCube ()
		{
            if (ParameterClass != EffectParameterClass.Object || ParameterType != EffectParameterType.TextureCube)
                throw new InvalidCastException();

            return (TextureCube)Data;
		}

		public Vector2 GetValueVector2 ()
		{
            if (ParameterType != EffectParameterType.Single)
                throw new InvalidCastException();

            if (Data == null)
                throw new InvalidCastException();

            var result = default(Vector2);
            var vecData = (float[])Data;
            if (ParameterClass == EffectParameterClass.Scalar)
            {
                result.X =
                result.Y = vecData[0];
            }
            else if (ParameterClass == EffectParameterClass.Vector)
            {
                if (ColumnCount != 2 || RowCount != 1)
                    throw new InvalidCastException();
                result.X = vecData[0];
                result.Y = vecData[1];
            }
            else
            {
                throw new InvalidCastException();
            }
            return result;
		}

		public Vector2[] GetValueVector2Array(int count)
		{
            if (count <= 0)
                throw new ArgumentOutOfRangeException("count");

            if (ParameterType != EffectParameterType.Single)
                throw new InvalidCastException();

            Vector2[] result = new Vector2[count];

            const int VectorSize = 2;
            var size = count * VectorSize;
            var array = GetValueSingleArray(size);
            for (int i = 0; i < count; i++)
            {
                var pos = i * VectorSize;
                result[i].X = array[pos];
                result[i].Y = array[pos + 1];
            }

            return result;
		}

		public Vector3 GetValueVector3 ()
		{
            if (ParameterType != EffectParameterType.Single || Data == null)
                throw new InvalidCastException();

            var vecInfo = (float[])Data;
            var result = default(Vector3);
            if (ParameterClass == EffectParameterClass.Scalar)
            {
                result.X =
                result.Y =
                result.Z = vecInfo[0];
            }
            else if (ParameterClass == EffectParameterClass.Vector)
            {
                if (ColumnCount != 3 || RowCount != 1)
                    throw new InvalidCastException();
                result.X = vecInfo[0];
                result.Y = vecInfo[1];
                result.Z = vecInfo[2];
            }
            else
            {
                throw new InvalidCastException();
            }

            return result;
		}

       public Vector3[] GetValueVector3Array(int count)
        {
            if (count <= 0)
                throw new ArgumentOutOfRangeException("count");

            if (ParameterType != EffectParameterType.Single)
                throw new InvalidCastException();

            const int VectorSize = 3;
            var size = count * VectorSize;
            var array = GetValueSingleArray(size);
            Vector3[] result = new Vector3[count];
            for (int i = 0; i < count; i++)
			{
                var pos = i * VectorSize;
                result[i].X = array[pos];
                result[i].Y = array[pos + 1];
                result[i].Z = array[pos + 2];
			}
            return result;
        }


		public Vector4 GetValueVector4 ()
		{
            if (ParameterType != EffectParameterType.Single || Data == null)
                throw new InvalidCastException();

            var vecInfo = (float[])Data;
            var result = default(Vector4);
            if (ParameterClass == EffectParameterClass.Scalar)
            {
                result.X =
                result.Y =
                result.Z =
                result.W = vecInfo[0];
            }
            else if (ParameterClass == EffectParameterClass.Vector)
            {
                if (ColumnCount != 4 || RowCount != 1)
                    throw new InvalidCastException();
                result.X = vecInfo[0];
                result.Y = vecInfo[1];
                result.Z = vecInfo[2];
                result.W = vecInfo[3];
            }
            else
            {
                throw new InvalidCastException();
            }
            return result;
		}
        
          public Vector4[] GetValueVector4Array(int count)
        {
            if (count <= 0)
                throw new ArgumentOutOfRangeException("count");

            if (ParameterType != EffectParameterType.Single)
                throw new InvalidCastException();

            const int VectorSize = 4;
            var size = count * VectorSize;
            var array = GetValueSingleArray(size);
            Vector4[] result = new Vector4[count];
            for (int i = 0; i < count; i++)
			{
                var pos = i * VectorSize;
                result[i].X = array[pos];
                result[i].Y = array[pos + 1];
                result[i].Z = array[pos + 2];
                result[i].W = array[pos + 3];
			}
            return result;
        }

		public void SetValue (bool value)
		{
            if (ParameterClass != EffectParameterClass.Scalar || ParameterType != EffectParameterType.Bool)
                throw new InvalidCastException();

#if DIRECTX
            // We store the bool as an integer as that
            // is what the constant buffers expect.
            ((int[])Data)[0] = value ? 1 : 0;
#else
            // MojoShader encodes even booleans into a float.
            ((float[])Data)[0] = value ? 1 : 0;
#endif
            StateKey = unchecked(NextStateKey++);
		}

        /*
		public void SetValue (bool[] value)
		{
			throw new NotImplementedException();
		}
        */

		public void SetValue (int value)
		{
            if (ParameterClass != EffectParameterClass.Scalar || ParameterType != EffectParameterType.Int32)
                throw new InvalidCastException();

#if DIRECTX
            ((int[])Data)[0] = value;
#else
            // MojoShader encodes integers into a float.
            ((float[])Data)[0] = value;
#endif
            StateKey = unchecked(NextStateKey++);
		}

        /*
		public void SetValue (int[] value)
		{
			throw new NotImplementedException();
		}
        */

        public void SetValue(Matrix value)
        {
            if (ParameterClass != EffectParameterClass.Matrix || ParameterType != EffectParameterType.Single)
                throw new InvalidCastException();

            // HLSL expects matrices to be transposed by default.
            // These unrolled loops do the transpose during assignment.
            if (RowCount == 4 && ColumnCount == 4)
            {
                var fData = (float[])Data;

                fData[0] = value.M11;
                fData[1] = value.M21;
                fData[2] = value.M31;
                fData[3] = value.M41;

                fData[4] = value.M12;
                fData[5] = value.M22;
                fData[6] = value.M32;
                fData[7] = value.M42;

                fData[8] = value.M13;
                fData[9] = value.M23;
                fData[10] = value.M33;
                fData[11] = value.M43;

                fData[12] = value.M14;
                fData[13] = value.M24;
                fData[14] = value.M34;
                fData[15] = value.M44;
            }
            else if (RowCount == 4 && ColumnCount == 3)
            {
                var fData = (float[])Data;

                fData[0] = value.M11;
                fData[1] = value.M21;
                fData[2] = value.M31;
                fData[3] = value.M41;

                fData[4] = value.M12;
                fData[5] = value.M22;
                fData[6] = value.M32;
                fData[7] = value.M42;

                fData[8] = value.M13;
                fData[9] = value.M23;
                fData[10] = value.M33;
                fData[11] = value.M43;
            }
            else if (RowCount == 3 && ColumnCount == 4)
            {
                var fData = (float[])Data;

                fData[0] = value.M11;
                fData[1] = value.M21;
                fData[2] = value.M31;

                fData[3] = value.M12;
                fData[4] = value.M22;
                fData[5] = value.M32;

                fData[6] = value.M13;
                fData[7] = value.M23;
                fData[8] = value.M33;

                fData[9] = value.M14;
                fData[10] = value.M24;
                fData[11] = value.M34;
            }
            else if (RowCount == 3 && ColumnCount == 3)
            {
                var fData = (float[])Data;

                fData[0] = value.M11;
                fData[1] = value.M21;
                fData[2] = value.M31;

                fData[3] = value.M12;
                fData[4] = value.M22;
                fData[5] = value.M32;

                fData[6] = value.M13;
                fData[7] = value.M23;
                fData[8] = value.M33;
            }
            else if (RowCount == 3 && ColumnCount == 2)
            {
                var fData = (float[])Data;

                fData[0] = value.M11;
                fData[1] = value.M21;
                fData[2] = value.M31;

                fData[3] = value.M12;
                fData[4] = value.M22;
                fData[5] = value.M32;
            }

            StateKey = unchecked(NextStateKey++);
        }

		public void SetValueTranspose(Matrix value)
		{
            if (ParameterClass != EffectParameterClass.Matrix || ParameterType != EffectParameterType.Single)
                throw new InvalidCastException();

            // HLSL expects matrices to be transposed by default, so copying them straight
            // from the in-memory version effectively transposes them back to row-major.
            if (RowCount == 4 && ColumnCount == 4)
            {
                var fData = (float[])Data;

                fData[0] = value.M11;
                fData[1] = value.M12;
                fData[2] = value.M13;
                fData[3] = value.M14;

                fData[4] = value.M21;
                fData[5] = value.M22;
                fData[6] = value.M23;
                fData[7] = value.M24;

                fData[8] = value.M31;
                fData[9] = value.M32;
                fData[10] = value.M33;
                fData[11] = value.M34;

                fData[12] = value.M41;
                fData[13] = value.M42;
                fData[14] = value.M43;
                fData[15] = value.M44;
            }
            else if (RowCount == 4 && ColumnCount == 3)
            {
                var fData = (float[])Data;

                fData[0] = value.M11;
                fData[1] = value.M12;
                fData[2] = value.M13;

                fData[3] = value.M21;
                fData[4] = value.M22;
                fData[5] = value.M23;

                fData[6] = value.M31;
                fData[7] = value.M32;
                fData[8] = value.M33;

                fData[9] = value.M41;
                fData[10] = value.M42;
                fData[11] = value.M43;
            }
            else if (RowCount == 3 && ColumnCount == 4)
            {
                var fData = (float[])Data;

                fData[0] = value.M11;
                fData[1] = value.M12;
                fData[2] = value.M13;
                fData[3] = value.M14;

                fData[4] = value.M21;
                fData[5] = value.M22;
                fData[6] = value.M23;
                fData[7] = value.M24;

                fData[8] = value.M31;
                fData[9] = value.M32;
                fData[10] = value.M33;
                fData[11] = value.M34;
            }
            else if (RowCount == 3 && ColumnCount == 3)
            {
                var fData = (float[])Data;

                fData[0] = value.M11;
                fData[1] = value.M12;
                fData[2] = value.M13;

                fData[3] = value.M21;
                fData[4] = value.M22;
                fData[5] = value.M23;

                fData[6] = value.M31;
                fData[7] = value.M32;
                fData[8] = value.M33;
            }
            else if (RowCount == 3 && ColumnCount == 2)
            {
                var fData = (float[])Data;

                fData[0] = value.M11;
                fData[1] = value.M12;
                fData[2] = value.M13;

                fData[3] = value.M21;
                fData[4] = value.M22;
                fData[5] = value.M23;
            }

			StateKey = unchecked(NextStateKey++);
		}
        
		/*
		public void SetValueTranspose (Matrix[] value)
		{
			throw new NotImplementedException();
		}
        */

		public void SetValue (Matrix[] value)
		{
            if (ParameterClass != EffectParameterClass.Matrix || ParameterType != EffectParameterType.Single)
                throw new InvalidCastException();

		    if (RowCount == 4 && ColumnCount == 4)
		    {
		        for (var i = 0; i < value.Length; i++)
		        {
		            var fData = (float[])Elements[i].Data;

		            fData[0] = value[i].M11;
		            fData[1] = value[i].M21;
		            fData[2] = value[i].M31;
		            fData[3] = value[i].M41;

		            fData[4] = value[i].M12;
		            fData[5] = value[i].M22;
		            fData[6] = value[i].M32;
		            fData[7] = value[i].M42;

		            fData[8] = value[i].M13;
		            fData[9] = value[i].M23;
		            fData[10] = value[i].M33;
		            fData[11] = value[i].M43;

		            fData[12] = value[i].M14;
		            fData[13] = value[i].M24;
		            fData[14] = value[i].M34;
		            fData[15] = value[i].M44;
		        }
		    }
		    else if (RowCount == 4 && ColumnCount == 3)
            {
                for (var i = 0; i < value.Length; i++)
                {
                    var fData = (float[])Elements[i].Data;

                    fData[0] = value[i].M11;
                    fData[1] = value[i].M21;
                    fData[2] = value[i].M31;
                    fData[3] = value[i].M41;

                    fData[4] = value[i].M12;
                    fData[5] = value[i].M22;
                    fData[6] = value[i].M32;
                    fData[7] = value[i].M42;

                    fData[8] = value[i].M13;
                    fData[9] = value[i].M23;
                    fData[10] = value[i].M33;
                    fData[11] = value[i].M43;
                }
            }
            else if (RowCount == 3 && ColumnCount == 4)
            {
                for (var i = 0; i < value.Length; i++)
                {
                    var fData = (float[])Elements[i].Data;

                    fData[0] = value[i].M11;
                    fData[1] = value[i].M21;
                    fData[2] = value[i].M31;

                    fData[3] = value[i].M12;
                    fData[4] = value[i].M22;
                    fData[5] = value[i].M32;

                    fData[6] = value[i].M13;
                    fData[7] = value[i].M23;
                    fData[8] = value[i].M33;

                    fData[9] = value[i].M14;
                    fData[10] = value[i].M24;
                    fData[11] = value[i].M34;
                }
            }
            else if (RowCount == 3 && ColumnCount == 3)
            {
                for (var i = 0; i < value.Length; i++)
                {
                    var fData = (float[])Elements[i].Data;

                    fData[0] = value[i].M11;
                    fData[1] = value[i].M21;
                    fData[2] = value[i].M31;

                    fData[3] = value[i].M12;
                    fData[4] = value[i].M22;
                    fData[5] = value[i].M32;

                    fData[6] = value[i].M13;
                    fData[7] = value[i].M23;
                    fData[8] = value[i].M33;
                }
            }
            else if (RowCount == 3 && ColumnCount == 2)
            {
                for (var i = 0; i < value.Length; i++)
                {
                    var fData = (float[])Elements[i].Data;

                    fData[0] = value[i].M11;
                    fData[1] = value[i].M21;
                    fData[2] = value[i].M31;

                    fData[3] = value[i].M12;
                    fData[4] = value[i].M22;
                    fData[5] = value[i].M32;
                }
            }

            StateKey = unchecked(NextStateKey++);
		}

		public void SetValue (Quaternion value)
		{
            if (ParameterClass != EffectParameterClass.Vector || ParameterType != EffectParameterType.Single)
                throw new InvalidCastException();

            var fData = (float[])Data;
            fData[0] = value.X;
            fData[1] = value.Y;
            fData[2] = value.Z;
            fData[3] = value.W;
            StateKey = unchecked(NextStateKey++);
		}

        /*
		public void SetValue (Quaternion[] value)
		{
			throw new NotImplementedException();
		}
        */

		public void SetValue (Single value)
		{
            if (ParameterType != EffectParameterType.Single)
                throw new InvalidCastException();
			((float[])Data)[0] = value;
            StateKey = unchecked(NextStateKey++);
		}

		public void SetValue (Single[] value)
		{
			for (var i=0; i<value.Length; i++)
				Elements[i].SetValue (value[i]);

            StateKey = unchecked(NextStateKey++);
		}
		
        /*
		public void SetValue (string value)
		{
			throw new NotImplementedException();
		}
        */

		public void SetValue (Texture value)
		{
            if (this.ParameterType != EffectParameterType.Texture && 
                this.ParameterType != EffectParameterType.Texture1D &&
                this.ParameterType != EffectParameterType.Texture2D &&
                this.ParameterType != EffectParameterType.Texture3D &&
                this.ParameterType != EffectParameterType.TextureCube) 
            {
                throw new InvalidCastException();
            }

			Data = value;
            StateKey = unchecked(NextStateKey++);
		}

		public void SetValue (Vector2 value)
		{
            if (ParameterClass != EffectParameterClass.Vector || ParameterType != EffectParameterType.Single)
                throw new InvalidCastException();

            var fData = (float[])Data;
            fData[0] = value.X;
            fData[1] = value.Y;
            StateKey = unchecked(NextStateKey++);
		}

		public void SetValue (Vector2[] value)
		{
            for (var i = 0; i < value.Length; i++)
				Elements[i].SetValue (value[i]);
            StateKey = unchecked(NextStateKey++);
		}

		public void SetValue (Vector3 value)
		{
            if (ParameterClass != EffectParameterClass.Vector || ParameterType != EffectParameterType.Single)
                throw new InvalidCastException();

            var fData = (float[])Data;
            fData[0] = value.X;
            fData[1] = value.Y;
            fData[2] = value.Z;
            StateKey = unchecked(NextStateKey++);
		}

		public void SetValue (Vector3[] value)
		{
            for (var i = 0; i < value.Length; i++)
				Elements[i].SetValue (value[i]);
            StateKey = unchecked(NextStateKey++);
		}

		public void SetValue (Vector4 value)
		{
            if (ParameterClass != EffectParameterClass.Vector || ParameterType != EffectParameterType.Single)
                throw new InvalidCastException();

			var fData = (float[])Data;
            fData[0] = value.X;
            fData[1] = value.Y;
            fData[2] = value.Z;
            fData[3] = value.W;
            StateKey = unchecked(NextStateKey++);
		}

		public void SetValue (Vector4[] value)
		{
            for (var i = 0; i < value.Length; i++)
				Elements[i].SetValue (value[i]);
            StateKey = unchecked(NextStateKey++);
		}
	}
}
