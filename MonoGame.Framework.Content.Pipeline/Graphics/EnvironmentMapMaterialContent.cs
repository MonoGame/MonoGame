// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace Microsoft.Xna.Framework.Content.Pipeline.Graphics
{
    public class EnvironmentMapMaterialContent : MaterialContent
    {
        public const string AlphaKey = "Alpha";
        public const string DiffuseColorKey = "DiffuseColor";
        public const string EmissiveColorKey = "EmissiveColor";
        public const string EnvironmentMapKey = "EnvironmentMap";
        public const string EnvironmentMapAmountKey = "EnvironmentMapAmount";
        public const string EnvironmentMapSpecularKey = " EnvironmentMapSpecular";
        public const string FresnelFactorKey = "FresnelFactor";
        public const string TextureKey = "Texture";

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

        public Vector3? EmissiveColor
        {
            get { return GetValueTypeProperty<Vector3>(EmissiveColorKey); }
            set { SetProperty(EmissiveColorKey, value); }
        }

        public ExternalReference<TextureContent> EnvironmentMap
        {
            get { return GetTexture(EnvironmentMapKey); }
            set { SetTexture(EnvironmentMapKey, value); }
        }

        public float? EnvironmentMapAmount
        {
            get { return GetValueTypeProperty<float>(EnvironmentMapAmountKey); }
            set { SetProperty(EnvironmentMapAmountKey, value); }
        }

        public Vector3? EnvironmentMapSpecular
        {
            get { return GetValueTypeProperty<Vector3>(EnvironmentMapSpecularKey); }
            set { SetProperty(EnvironmentMapSpecularKey, value); }
        }

        public float? FresnelFactor
        {
            get { return GetValueTypeProperty<float>(FresnelFactorKey); }
            set { SetProperty(FresnelFactorKey, value); }
        }

        public ExternalReference<TextureContent> Texture
        {
            get { return GetTexture(TextureKey); }
            set { SetTexture(TextureKey, value); }
        }
    }
}
