using System;
using Microsoft.Xna.Framework.Graphics;

namespace Microsoft.Xna.Framework.Graphics
{
    public interface IEffectParameter
    {
        string Name { get; }

        string Semantic { get; }

        EffectParameterClass ParameterClass { get; }

        EffectParameterType ParameterType { get; }

        int RowCount { get; }

        int ColumnCount { get; }

        IEffectParameterCollection Elements { get; }

        IEffectParameterCollection StructureMembers { get; }

        IEffectAnnotationCollection Annotations { get; }

        bool GetValueBoolean();
        int GetValueInt32();
        Matrix GetValueMatrix();
        Matrix[] GetValueMatrixArray(int count);
        Quaternion GetValueQuaternion();
        Single GetValueSingle();
        Single[] GetValueSingleArray();
        string GetValueString();
        Texture2D GetValueTexture2D();

#if !GLES
        Texture3D GetValueTexture3D();
#endif

        TextureCube GetValueTextureCube();
        Vector2 GetValueVector2();
        Vector2[] GetValueVector2Array();
        Vector3 GetValueVector3();
        Vector3[] GetValueVector3Array();
        Vector4 GetValueVector4();
        Vector4[] GetValueVector4Array();
        void SetValue(bool value);
        void SetValue(int value);
        void SetValue(Matrix value);
        void SetValueTranspose(Matrix value);
        void SetValue(Matrix[] value);
        void SetValue(Quaternion value);
        void SetValue(Single value);
        void SetValue(Single[] value);
        void SetValue(Texture value);
        void SetValue(Vector2 value);
        void SetValue(Vector2[] value);
        void SetValue(Vector3 value);
        void SetValue(Vector3[] value); 
        void SetValue(Vector4 value);
        void SetValue(Vector4[] value);
    }
}