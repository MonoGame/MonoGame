// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace Microsoft.Xna.Framework.Content.Pipeline.Graphics
{
    public class BasicMaterialContent : MaterialContent
    {
        public const string AlphaKey = "Alpha";
        public const string DiffuseColorKey = "DiffuseColor";
        public const string EmissiveColorKey = "EmissiveColor";
        public const string SpecularColorKey = "SpecularColor";
        public const string SpecularPowerKey = "SpecularPower";
        public const string TextureKey = "Texture";
        public const string VertexColorEnabledKey = "VertexColorEnabled";

        public float? Alpha { get; set; }
        public Color? DiffuseColor { get; set; }
        public Color? EmissiveColor { get; set; }
        public Color? SpecularColor { get; set; }
        public float? SpecularPower { get; set; }
        public ExternalReference<TextureContent> Texture { get; set; }
        public bool? VertexColorEnabled { get; set; }
    }
}
