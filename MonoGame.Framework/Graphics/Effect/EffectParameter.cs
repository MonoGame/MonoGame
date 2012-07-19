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
            Elements = new EffectParameterCollection(cloneSource.Elements);
            StructureMembers = new EffectParameterCollection(cloneSource.StructureMembers);

            // Data is mutable, but a new copy happens during
            // boxing/unboxing so we can just assign it.
            Data = cloneSource.Data;
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
            StateKey = unchecked(NextStateKey++);
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
					((float[])Data)[y*ColumnCount+x] = matrixData[y*4+x];
			}

            StateKey = unchecked(NextStateKey++);
		}

		public void SetValue (Matrix[] value)
		{
            for (var i = 0; i < value.Length; i++)
				Elements[i].SetValue (value[i]);

            StateKey = unchecked(NextStateKey++);
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

            StateKey = unchecked(NextStateKey++);
		}

		public void SetValue (Single[] value)
		{
			for (var i=0; i<value.Length; i++)
				Elements[i].SetValue (value[i]);

            StateKey = unchecked(NextStateKey++);
		}
		
		public void SetValue (string value)
		{
			throw new NotImplementedException();
		}

		public void SetValue (Texture value)
		{
			Data = value;
            StateKey = unchecked(NextStateKey++);
		}

		public void SetValue (Vector2 value)
		{
			Data = new float[2] { value.X, value.Y };
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
			Data = new float[3] { value.X, value.Y, value.Z };
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
			Data = new float[4] { value.X, value.Y, value.Z, value.W };
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