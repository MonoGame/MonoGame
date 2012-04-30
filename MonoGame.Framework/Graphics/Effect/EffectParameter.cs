using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Microsoft.Xna.Framework.Graphics
{
    [DebuggerDisplay("{ParameterClass} {ParameterType} {Name} : {Semantic}")]
	public class EffectParameter
	{
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

        internal EffectParameter(DXEffectObject.d3dx_parameter parameter)
		{
			switch (parameter.class_) {
			case DXEffectObject.D3DXPARAMETER_CLASS.SCALAR:
				paramClass = EffectParameterClass.Scalar;
				break;
			case DXEffectObject.D3DXPARAMETER_CLASS.VECTOR:
				paramClass = EffectParameterClass.Vector;
				break;
			case DXEffectObject.D3DXPARAMETER_CLASS.MATRIX_ROWS:
			case DXEffectObject.D3DXPARAMETER_CLASS.MATRIX_COLUMNS:
				paramClass = EffectParameterClass.Matrix;
				break;
			case DXEffectObject.D3DXPARAMETER_CLASS.OBJECT:
				paramClass = EffectParameterClass.Object;
				break;
			case DXEffectObject.D3DXPARAMETER_CLASS.STRUCT:
				paramClass = EffectParameterClass.Struct;
				break;
			default:
				throw new NotImplementedException();
			}
			
			switch (parameter.type) {
			case DXEffectObject.D3DXPARAMETER_TYPE.VOID:
				paramType = EffectParameterType.Void;
				break;
			case DXEffectObject.D3DXPARAMETER_TYPE.BOOL:
				paramType = EffectParameterType.Bool;
				break;
			case DXEffectObject.D3DXPARAMETER_TYPE.INT:
				paramType = EffectParameterType.Int32;
				break;
			case DXEffectObject.D3DXPARAMETER_TYPE.FLOAT:
				paramType = EffectParameterType.Single;
				break;
			case DXEffectObject.D3DXPARAMETER_TYPE.STRING:
				paramType = EffectParameterType.String;
				break;
			case DXEffectObject.D3DXPARAMETER_TYPE.TEXTURE:
				paramType = EffectParameterType.Texture;
				break;
			case DXEffectObject.D3DXPARAMETER_TYPE.TEXTURE1D:
				paramType = EffectParameterType.Texture1D;
				break;
			case DXEffectObject.D3DXPARAMETER_TYPE.TEXTURE2D:
				paramType = EffectParameterType.Texture2D;
				break;
			case DXEffectObject.D3DXPARAMETER_TYPE.TEXTURE3D:
				paramType = EffectParameterType.Texture3D;
				break;
			case DXEffectObject.D3DXPARAMETER_TYPE.TEXTURECUBE:
				paramType = EffectParameterType.TextureCube;
				break;
			default:
				//we need types not normally exposed in XNA for internal use
				rawParameter = true;
				break;
			}
			
			name = parameter.name;
			rowCount = (int)parameter.rows;
			colCount = (int)parameter.columns;
			semantic = parameter.semantic;
			
			annotations = new EffectAnnotationCollection();
			for (int i=0; i<parameter.annotation_count; i++) {
				EffectAnnotation annotation = new EffectAnnotation();
				annotations._annotations.Add (annotation);
			}
			
			elements = new EffectParameterCollection();
			structMembers = new EffectParameterCollection();
			if (parameter.element_count > 0) {
				for (int i=0; i<parameter.element_count; i++) {
					EffectParameter element = new EffectParameter(parameter.member_handles[i]);
					elements._parameters.Add (element);
				}
			} else if (parameter.member_count > 0) {
				for (int i=0; i<parameter.member_count; i++) {
					EffectParameter member = new EffectParameter(parameter.member_handles[i]);
					structMembers._parameters.Add (member);
				}
			} else {
				if (rawParameter) {
					data = parameter.data;
				} else {
					//interpret data
					switch (paramClass) {
					case EffectParameterClass.Scalar:
						switch (paramType) {
						case EffectParameterType.Bool:
							data = BitConverter.ToBoolean((byte[])parameter.data, 0);
							break;
						case EffectParameterType.Int32:
							data = BitConverter.ToInt32 ((byte[])parameter.data, 0);
							break;
						case EffectParameterType.Single:
							data = BitConverter.ToSingle((byte[])parameter.data, 0);
							break;
						case EffectParameterType.Void:
							data = null;
							break;
						default:
							break;
							//throw new NotSupportedException();
						}
						break;
					case EffectParameterClass.Vector:
					case EffectParameterClass.Matrix:
						switch (paramType) {
						case EffectParameterType.Single:
							float[] vals = new float[rowCount*colCount];
							//transpose maybe?
							for (int i=0; i<rowCount*colCount; i++) {
								vals[i] = BitConverter.ToSingle ((byte[])parameter.data, i*4);
							}
							data = vals;
							break;
						default:
							break;
						}
						break;
					default:
						//throw new NotSupportedException();
						break;
					}
				}
			}
		}

        public int ColumnCount {
			get { return colCount; }
		}

            Data = data;
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
            Elements = new EffectParameterCollection(cloneSource.Elements);
            StructureMembers = new EffectParameterCollection(cloneSource.StructureMembers);

            // Data is mutable, but a new copy happens during
            // boxing/unboxing so we can just assign it.
            Data = cloneSource.Data;
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


		public void SetValue (object value)
		{
			throw new NotImplementedException();
		}

		public bool GetValueBoolean ()
		{
			throw new NotImplementedException();
		}

		public bool[] GetValueBooleanArray ()
		{
			throw new NotImplementedException();
		}

		public int GetValueInt32 ()
		{
            return (int)Data;
		}

		public int[] GetValueInt32Array ()
		{
			throw new NotImplementedException();
		}

		public Matrix GetValueMatrix ()
		{
			throw new NotImplementedException();
		}

		public Matrix[] GetValueMatrixArray (int count)
		{
			throw new NotImplementedException();
		}

		public Quaternion GetValueQuaternion ()
		{
			throw new NotImplementedException();
		}

		public Quaternion[] GetValueQuaternionArray ()
		{
			throw new NotImplementedException();
		}

		public Single GetValueSingle ()
		{
			switch(ParameterType) 
            {
			case EffectParameterType.Int32:
				return (Single)(int)Data;
			default:
				return (Single)Data;
			}
		}

		public Single[] GetValueSingleArray ()
		{
			if (Elements != null && Elements.Count > 0)
            {
                var ret = new Single[RowCount * ColumnCount * Elements.Count];
				for (int i=0; i<Elements.Count; i++)
                {
                    var elmArray = Elements[i].GetValueSingleArray();
                    for (var j = 0; j < elmArray.Length; j++)
						ret[RowCount*ColumnCount*i+j] = elmArray[j];
				}
				return ret;
			}
			
			switch(ParameterClass) 
            {
			case EffectParameterClass.Scalar:
				return new Single[] { GetValueSingle () };
            case EffectParameterClass.Vector:
			case EffectParameterClass.Matrix:
                    if (Data is Matrix)
                        return Matrix.ToFloatArray((Matrix)Data);
                    else
                        return (float[])Data;
			default:
				throw new NotImplementedException();
			}
		}

		public string GetValueString ()
		{
			throw new NotImplementedException();
		}

		public Texture2D GetValueTexture2D ()
		{
			return (Texture2D)Data;
		}

        // TODO: Add Texture3D support!
		//		public Texture3D GetValueTexture3D ()
		//		{
		//			return new Texture3D ();
		//		}

		public TextureCube GetValueTextureCube ()
		{
			throw new NotImplementedException();
		}

		public Vector2 GetValueVector2 ()
		{
            var vecInfo = (float[])Data;
			return new Vector2(vecInfo[0],vecInfo[1]);
		}

		public Vector2[] GetValueVector2Array ()
		{
			throw new NotImplementedException();
		}

		public Vector3 GetValueVector3 ()
		{
            var vecInfo = (float[])Data;
			return new Vector3(vecInfo[0],vecInfo[1],vecInfo[2]);

		}

		public Vector3[] GetValueVector3Array ()
		{
			throw new NotImplementedException();
		}

		public Vector4 GetValueVector4 ()
		{
            var vecInfo = (float[])Data;
			return new Vector4(vecInfo[0],vecInfo[1],vecInfo[2],vecInfo[3]);
		}

		public Vector4[] GetValueVector4Array ()
		{
			throw new NotImplementedException();
		}

		public void SetValue (bool value)
		{
			throw new NotImplementedException();
		}

		public void SetValue (bool[] value)
		{
			throw new NotImplementedException();
		}

		public void SetValue (int value)
		{
			Data = value;
		}

		public void SetValue (int[] value)
		{
			throw new NotImplementedException();
		}

		public void SetValue (Matrix value)
		{
			float[] matrixData = Matrix.ToFloatArray(Matrix.Transpose (value));
			for (int y=0; y<RowCount; y++) {
				for (int x=0; x<ColumnCount; x++) {
					((float[])data)[y*ColumnCount+x] = matrixData[y*4+x];
				}
			}
		}

		public void SetValue (Matrix[] value)
		{
            for (var i = 0; i < value.Length; i++)
				Elements[i].SetValue (value[i]);
		}

		public void SetValue (Quaternion value)
		{
			throw new NotImplementedException();
		}

		public void SetValue (Quaternion[] value)
		{
			throw new NotImplementedException();
		}

		public void SetValue (Single value)
		{
			switch (ParameterClass) 
            {
			case EffectParameterClass.Matrix:
			case EffectParameterClass.Vector:
				((float[])Data)[0] = value;
				break;
			case EffectParameterClass.Scalar:
				Data = value;
				break;
			default:
				throw new NotImplementedException();
			}
		}

		public void SetValue (Single[] value)
		{
			for (var i=0; i<value.Length; i++)
				Elements[i].SetValue (value[i]);
		}
		
		public void SetValue (string value)
		{
			throw new NotImplementedException();
		}

		public void SetValue (Texture value)
		{
			Data = value;
		}

		public void SetValue (Vector2 value)
		{
			Data = new float[2] { value.X, value.Y };
		}

		public void SetValue (Vector2[] value)
		{
            for (var i = 0; i < value.Length; i++)
				Elements[i].SetValue (value[i]);
		}

		public void SetValue (Vector3 value)
		{
			Data = new float[3] { value.X, value.Y, value.Z };
		}

		public void SetValue (Vector3[] value)
		{
            for (var i = 0; i < value.Length; i++)
				Elements[i].SetValue (value[i]);
		}

		public void SetValue (Vector4 value)
		{
			Data = new float[4] { value.X, value.Y, value.Z, value.W };
		}

		public void SetValue (Vector4[] value)
		{
            for (var i = 0; i < value.Length; i++)
				Elements[i].SetValue (value[i]);
		}
	}
}