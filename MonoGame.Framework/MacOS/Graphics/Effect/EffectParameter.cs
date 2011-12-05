using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using MonoMac.OpenGL;

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
		
		bool isShader = false;
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
			case DXEffectObject.D3DXPARAMETER_TYPE.PIXELSHADER:
			case DXEffectObject.D3DXPARAMETER_TYPE.VERTEXSHADER:
				//we handle these specially in this class for convenience
				isShader = true;
				break;
			default:
				break;
				//throw new NotSupportedException();
			}
			name = parameter.name;
			rowCount = (int)parameter.rows;
			colCount = (int)parameter.columns;
			semantic = parameter.semantic;
			
			if (parameter.annotation_count > 0) {
				annotations = new EffectAnnotationCollection();
				for (int i=0; i<parameter.annotation_count; i++) {
					EffectAnnotation annotation = new EffectAnnotation();
					annotations._annotations.Add (annotation);
				}
			}
			
			if (parameter.element_count > 0) {
				elements = new EffectParameterCollection();
				for (int i=0; i<parameter.element_count; i++) {
					EffectParameter element = new EffectParameter(parameter.member_handles[i]);
					elements._parameters.Add (element);
				}
			} else if (parameter.member_count > 0) {
				structMembers = new EffectParameterCollection();
				for (int i=0; i<parameter.member_count; i++) {
					EffectParameter member = new EffectParameter(parameter.member_handles[i]);
					structMembers._parameters.Add (member);
				}
			} else {
				if (isShader) {
					data = new DXShader((byte[])parameter.data);
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
		}

		public bool GetValueBoolean ()
		{
			return true;
		}

		public bool[] GetValueBooleanArray ()
		{
			return new bool[0];
		}

		public int GetValueInt32 ()
		{
			return 0;
		}

		public int[] GetValueInt32Array ()
		{
			return new int[0];
		}

		public Matrix GetValueMatrix ()
		{
			return new Matrix ();
		}

		public Matrix[] GetValueMatrixArray ()
		{
			return new Matrix[0];
		}

		public Quaternion GetValueQuaternion ()
		{
			return new Quaternion ();
		}

		public Quaternion[] GetValueQuaternionArray ()
		{
			return new Quaternion[0];
		}

		public Single GetValueSingle ()
		{
			return new Single ();
		}

		public Single[] GetValueSingleArray ()
		{
			return new Single[0];
		}

		public string GetValueString ()
		{
			return String.Empty;
		}

		public Texture2D GetValueTexture2D ()
		{
			return null; //new Texture2D ();
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
			return Vector2.One;
		}

		public Vector2[] GetValueVector2Array ()
		{
			return new Vector2[0];
		}

		public Vector3 GetValueVector3 ()
		{
			return Vector3.One;
		}

		public Vector3[] GetValueVector3Array ()
		{
			return new Vector3[0];
		}

		public Vector4 GetValueVector4 ()
		{
			return Vector4.One;
		}

		public Vector4[] GetValueVector4Array ()
		{
			return new Vector4[0];
		}

		public void SetValue (bool value)
		{
		}

		public void SetValue (bool[] value)
		{
		}

		public void SetValue (int value)
		{
		}

		public void SetValue (int[] value)
		{
		}

		public void SetValue (Matrix value)
		{ 
			data = Matrix.ToFloatArray(value);
		}

		public void SetValue (Matrix[] value)
		{
		}

		public void SetValue (Quaternion value)
		{
		}

		public void SetValue (Quaternion[] value)
		{
		}

		public void SetValue (Single value)
		{
		}

		public void SetValue (Single[] value)
		{
		}
		
		public void SetValue (string value)
		{
		}

		public void SetValue (Texture value)
		{
		}

		public void SetValue (Vector2 value)
		{
		}

		public void SetValue (Vector2[] value)
		{
		}

		public void SetValue (Vector3 value)
		{
		}

		public void SetValue (Vector3[] value)
		{
		}

		public void SetValue (Vector4 value)
		{
		}

		public void SetValue (Vector4[] value)
		{
		}
	}
}
