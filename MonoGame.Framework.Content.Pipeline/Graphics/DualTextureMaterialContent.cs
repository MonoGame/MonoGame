// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace Microsoft.Xna.Framework.Content.Pipeline.Graphics
{
    public class DualTextureMaterialContent : MaterialContent
    {
        public const string AlphaKey = "Alpha";
        public const string DiffuseColorKey = "DiffuseColor";
        public const string TextureKey = "Texture";
        public const string Texture2Key = "Texture2";
        public const string VertexColorEnabledKey = "VertexColorEnabled";

        public float? Alpha
        {
            get { return GetValueTypeProperty<float>(AlphaKey); }
            set { SetProperty(AlphaKey, value); }
        }

        public Vector3? DiffuseColor
        {
            get { return GetValueTypeProperty<Vector3>(DiffuseColorKey); }
            set { SetProperty(DiffuseColorKey, value); }
        }

        public ExternalReference<TextureContent> Texture
        {
            get { return GetTexture(TextureKey); }
            set { SetTexture(TextureKey, value); }
        }

        public ExternalReference<TextureContent> Texture2
        {
            get { return GetTexture(Texture2Key); }
            set { SetTexture(Texture2Key, value); }
        }

        public bool? VertexColorEnabled
        {
            get { return GetValueTypeProperty<bool>(VertexColorEnabledKey); }
            set { SetProperty(VertexColorEnabledKey, value); }
        }
    }
}
