using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Microsoft.Xna.Framework.Graphics
{
    [DebuggerDisplay("{paramClass} {paramType} {name} : {semantic}")]
	public class EffectParameter
	{
		EffectParameterClass _class;
        EffectParameterType _type;
        string _name;
        int _rowCount;
		int _colCount;
		EffectParameterCollection _elements;
		string _semantic;
		EffectParameterCollection _structMembers;
		EffectAnnotationCollection _annotations;
		
        // TODO: Using object adds alot of boxing/unboxing overhead
        // and garbage generation.  We should consider a templated
        // type implementation to fix this!

        internal object data;

        internal EffectParameter(DXEffectObject.d3dx_parameter parameter)
		{
            _class = DXEffectObject.ToParameterClass(parameter.class_);
            _type = DXEffectObject.ToParameterType(parameter.type);
			
			_name = parameter.name;
			_rowCount = (int)parameter.rows;
			_colCount = (int)parameter.columns;
			_semantic = parameter.semantic;

			_annotations = new EffectAnnotationCollection();
            for (var i = 0; i < parameter.annotation_count; i++) 
            {
				var annotation = new EffectAnnotation();
				_annotations._annotations.Add (annotation);
			}
			
			_elements = new EffectParameterCollection();
			_structMembers = new EffectParameterCollection();
			if (parameter.element_count > 0) 
            {
                for (var i = 0; i < parameter.element_count; i++) 
                {
                    var element = new EffectParameter(parameter.member_handles[i]);
					_elements.Add(element);
				}
			} 
            else if (parameter.member_count > 0) 
            {
                for (var i = 0; i < parameter.member_count; i++) 
                {
					var member = new EffectParameter(parameter.member_handles[i]);
					_structMembers.Add(member);
				}
			} 
            else 
            {
				//interpret data
				switch (_class) 
                {
				case EffectParameterClass.Scalar:
					switch (_type) 
                    {
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
						throw new NotSupportedException();
                    }
					break;
				case EffectParameterClass.Vector:
				case EffectParameterClass.Matrix:
					switch (_type) 
                    {
					case EffectParameterType.Single:
						var vals = new float[_rowCount*_colCount];
						//transpose maybe?
                        for (var i = 0; i < _rowCount * _colCount; i++)
							vals[i] = BitConverter.ToSingle ((byte[])parameter.data, i*4);
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
		}

        public int ColumnCount {
			get { return _colCount; }
		}

		public EffectParameterCollection Elements {
			get { return _elements; }
		}

		public string Name {
			get { return _name; }
		}

		public EffectParameterClass ParameterClass { 
			get { return _class;} 
		}

		public EffectParameterType ParameterType { 
			get { return _type;} 
		}

		public int RowCount {
			get { return _rowCount; }
		}

		public string Semantic {
			get { return _semantic; }
		}

		EffectParameterCollection StructureMembers {
			get { return _structMembers; }
		}

		EffectAnnotationCollection Annotations {
            get { return _annotations; }
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
            return (int)data;
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
				return (Single)(int)data;
			default:
				return (Single)data;
			}
		}

		public Single[] GetValueSingleArray ()
		{
			if (Elements != null && Elements.Count > 0)
            {
                var ret = new Single[_rowCount * _colCount * Elements.Count];
				for (int i=0; i<Elements.Count; i++)
                {
                    var elmArray = Elements[i].GetValueSingleArray();
                    for (var j = 0; j < elmArray.Length; j++)
						ret[_rowCount*_colCount*i+j] = elmArray[j];
				}
				return ret;
			}
			
			switch(ParameterClass) 
            {
			case EffectParameterClass.Scalar:
				return new Single[] { GetValueSingle () };
            case EffectParameterClass.Vector:
			case EffectParameterClass.Matrix:
                    if (data is Matrix)
                        return Matrix.ToFloatArray((Matrix)data);
                    else
                        return (float[])data;
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
			return (Texture2D)data;
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
            var vecInfo = (float[])data;
			return new Vector2(vecInfo[0],vecInfo[1]);
		}

		public Vector2[] GetValueVector2Array ()
		{
			throw new NotImplementedException();
		}

		public Vector3 GetValueVector3 ()
		{
            var vecInfo = (float[])data;
			return new Vector3(vecInfo[0],vecInfo[1],vecInfo[2]);

		}

		public Vector3[] GetValueVector3Array ()
		{
			throw new NotImplementedException();
		}

		public Vector4 GetValueVector4 ()
		{
            var vecInfo = (float[])data;
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
			data = value;
		}

		public void SetValue (int[] value)
		{
			throw new NotImplementedException();
		}

		public void SetValue (Matrix value)
		{
			var matrixData = Matrix.ToFloatArray(Matrix.Transpose (value));
			for (var y=0; y<RowCount; y++) 
            {
				for (var x=0; x<ColumnCount; x++)
					((float[])data)[y*ColumnCount+x] = matrixData[y*4+x];
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
			for (var i=0; i<value.Length; i++)
				Elements[i].SetValue (value[i]);
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
            for (var i = 0; i < value.Length; i++)
				Elements[i].SetValue (value[i]);
		}

		public void SetValue (Vector3 value)
		{
			data = new float[3] { value.X, value.Y, value.Z };
		}

		public void SetValue (Vector3[] value)
		{
            for (var i = 0; i < value.Length; i++)
				Elements[i].SetValue (value[i]);
		}

		public void SetValue (Vector4 value)
		{
			data = new float[4] { value.X, value.Y, value.Z, value.W };
		}

		public void SetValue (Vector4[] value)
		{
            for (var i = 0; i < value.Length; i++)
				Elements[i].SetValue (value[i]);
		}
	}
}