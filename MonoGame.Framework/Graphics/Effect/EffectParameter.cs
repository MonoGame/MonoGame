using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.Xna.Framework.Graphics
{
	public class EffectParameter
	{
		EffectParameterType paramType;
		EffectParameterClass paramClass;
		int rowCount;
		string name;
		int colCount;
		EffectParameterCollection elements;
		string semantic;
		EffectParameterCollection structMembers;
		EffectAnnotationCollection annotations;
		Effect _parentEffect;
		
		internal bool rawParameter = false;
		internal DXEffectObject.D3DXPARAMETER_TYPE rawType;
		
		internal object data;
		
		internal EffectParameter( DXEffectObject.d3dx_parameter parameter )
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
			rawType = parameter.type;
			
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

		public EffectParameterCollection Elements {
			get { return elements; }
		}

		public string Name {
			get { return name; }
		}

		public EffectParameterClass ParameterClass { 
			get { return paramClass;} 
		}

		public EffectParameterType ParameterType { 
			get { return paramType;} 
		}

		public int RowCount {
			get { return rowCount; }
		}

		public string Semantic {
			get { return semantic; }
		}

		EffectParameterCollection StructureMembers {
			get { return structMembers; }
		}

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
			throw new NotImplementedException();
		}

		public int[] GetValueInt32Array ()
		{
			throw new NotImplementedException();
		}

		public Matrix GetValueMatrix ()
		{
			throw new NotImplementedException();
		}

		public Matrix[] GetValueMatrixArray ()
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
			switch(ParameterType) {
			case EffectParameterType.Int32:
				return (Single)(int)data;
			default:
				return (Single)data;
			}
		}

		public Single[] GetValueSingleArray ()
		{
			if (Elements.Count > 0) {
				Single[] ret = new Single[rowCount*colCount*Elements.Count];
				for (int i=0; i<Elements.Count; i++) {
					Single[] elmArray = Elements[i].GetValueSingleArray ();
					for (int j=0; j<elmArray.Length; j++) {
						ret[rowCount*colCount*i+j] = elmArray[j];
					}
				}
				return ret;
			}
			
			switch(ParameterClass) {
			case EffectParameterClass.Scalar:
				return new Single[] { GetValueSingle () };
			case EffectParameterClass.Matrix:
			case EffectParameterClass.Vector:
				return (Single[])data;
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
			throw new NotImplementedException();
		}

		//		public Texture3D GetValueTexture3D ()
		//		{
		//			return new Texture3D ();
		//		}

		//		public TextureCube GetValueTextureCube ()
		//		{
		//			return null; //new TextureCube ();
		//		}

		public Vector2 GetValueVector2 ()
		{
			throw new NotImplementedException();
		}

		public Vector2[] GetValueVector2Array ()
		{
			throw new NotImplementedException();
		}

		public Vector3 GetValueVector3 ()
		{
			throw new NotImplementedException();
		}

		public Vector3[] GetValueVector3Array ()
		{
			throw new NotImplementedException();
		}

		public Vector4 GetValueVector4 ()
		{
			throw new NotImplementedException();
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
			data = value;
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
			throw new NotImplementedException();
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
			switch (ParameterClass) {
			case EffectParameterClass.Matrix:
			case EffectParameterClass.Vector:
				((float[])data)[0] = value;
				break;
			case EffectParameterClass.Scalar:
				data = value;
				break;
			default:
				throw new NotImplementedException();
			}
		}

		public void SetValue (Single[] value)
		{
			for (int i=0; i<value.Length; i++) {
				Elements[i].SetValue (value[i]);
			}
		}
		
		public void SetValue (string value)
		{
			throw new NotImplementedException();
		}

		public void SetValue (Texture value)
		{
			data = value;
		}

		public void SetValue (Vector2 value)
		{
			data = new float[2] { value.X, value.Y };
		}

		public void SetValue (Vector2[] value)
		{
			for (int i=0; i<value.Length; i++) {
				Elements[i].SetValue (value[i]);
			}
		}

		public void SetValue (Vector3 value)
		{
			data = new float[3] { value.X, value.Y, value.Z };
		}

		public void SetValue (Vector3[] value)
		{
			for (int i=0; i<value.Length; i++) {
				Elements[i].SetValue (value[i]);
			}
		}

		public void SetValue (Vector4 value)
		{
			data = new float[4] { value.X, value.Y, value.Z, value.W };
		}

		public void SetValue (Vector4[] value)
		{
			for (int i=0; i<value.Length; i++) {
				Elements[i].SetValue (value[i]);
			}
		}
	}
}
