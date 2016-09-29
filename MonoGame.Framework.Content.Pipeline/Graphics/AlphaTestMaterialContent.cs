// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Microsoft.Xna.Framework.Graphics;

namespace Microsoft.Xna.Framework.Content.Pipeline.Graphics
{
    public class AlphaTestMaterialContent : MaterialContent
    {
        public const string AlphaKey = "Alpha";
        public const string AlphaFunctionKey = "AlphaFunction";
        public const string DiffuseColorKey = "DiffuseColor";
        public const string ReferenceAlphaKey = "ReferenceAlpha";
        public const string TextureKey = "Texture";
        public const string VertexColorEnabledKey = "VertexColorEnabled";

        public float? Alpha
        {
            get { return GetValueTypeProperty<float>(AlphaKey); }
            set { SetProperty(AlphaKey, value); }
        }
        
        public CompareFunction? AlphaFunction
        {
            get { return GetValueTypeProperty<CompareFunction>(AlphaFunctionKey); }
            set { SetProperty(AlphaFunctionKey, value); }
        }

        public Vector3? DiffuseColor
        {
            get { return GetValueTypeProperty<Vector3>(DiffuseColorKey); }
            set { SetProperty(DiffuseColorKey, value); }
        }

        public int? ReferenceAlpha
        {
            get { return GetValueTypeProperty<int>(ReferenceAlphaKey); }
            set { SetProperty(ReferenceAlphaKey, value); }
        }

        public ExternalReference<TextureContent> Texture
        {
            get { return GetTexture(TextureKey); }
            set { SetTexture(TextureKey, value); }
        }

        public bool? VertexColorEnabled
        {
            get { return GetValueTypeProperty<bool>(VertexColorEnabledKey); }
            set { SetProperty(VertexColorEnabledKey, value); }
        }
    }
}
