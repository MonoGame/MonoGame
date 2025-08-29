// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace Microsoft.Xna.Framework.Graphics
{
    /// <summary>
    /// Internal effect used by SpriteBatch to render distance-field encoded SpriteFont textures.
    /// Supports single-channel SDF glyph rendering with derivative-based anti-aliasing.
    /// </summary>
    internal sealed class DistanceFieldSpriteEffect : Effect
    {
        private static readonly System.Runtime.CompilerServices.ConditionalWeakTable<GraphicsDevice, DistanceFieldSpriteEffect> _instances = new System.Runtime.CompilerServices.ConditionalWeakTable<GraphicsDevice, DistanceFieldSpriteEffect>();
        private EffectParameter _matrixParam;
        private Viewport _lastViewport;
        private Matrix _projection;

        private EffectParameter _spreadParam;
        private EffectParameter _outlineThicknessParam;
        private EffectParameter _outlineColorParam;

        private DistanceFieldSpriteEffect(GraphicsDevice device)
            : base(device, EffectResource.DistanceFieldSpriteEffect.Bytecode)
        {
            _matrixParam          = Parameters["MatrixTransform"];
            _spreadParam          = Parameters["Spread"];
            _outlineThicknessParam = Parameters["OutlineThickness"];
            _outlineColorParam    = Parameters["OutlineColor"];
        }

        public static DistanceFieldSpriteEffect Instance(GraphicsDevice device)
        {
            if (device == null)
                throw new System.ArgumentNullException(nameof(device));

            if (!_instances.TryGetValue(device, out var effect) || effect.IsDisposed)
            {
                _instances.Remove(device);
                effect = new DistanceFieldSpriteEffect(device);
                _instances.Add(device, effect);
            }

            return effect;
        }

        public Matrix? TransformMatrix { get; set; }

        protected internal override void OnApply()
        {
            var vp = GraphicsDevice.Viewport;
            if ((vp.Width != _lastViewport.Width) || (vp.Height != _lastViewport.Height))
            {
                Matrix.CreateOrthographicOffCenter(0, vp.Width, vp.Height, 0, 0, -1, out _projection);
                if (GraphicsDevice.UseHalfPixelOffset)
                {
                    _projection.M41 += -0.5f * _projection.M11;
                    _projection.M42 += -0.5f * _projection.M22;
                }
                _lastViewport = vp;
            }

            if (TransformMatrix.HasValue)
                _matrixParam.SetValue(TransformMatrix.GetValueOrDefault() * _projection);
            else
                _matrixParam.SetValue(_projection);
        }

        /// <summary>
        /// Applies distance-field sampling parameters. Called by SpriteBatcher before each
        /// flush of a distance-field glyph batch.
        /// </summary>
        /// <param name="spread">Half-range in texels used when the atlas was generated. Stored in the XNB asset.</param>
        internal void ApplyDistanceFieldSettings(float spread)
        {
            _spreadParam?.SetValue(spread);
        }

        /// <summary>
        /// Sets optional outline rendering parameters.
        /// </summary>
        internal void SetOutline(float thickness, Vector4 color)
        {
            _outlineThicknessParam?.SetValue(thickness);
            _outlineColorParam?.SetValue(color);
        }

        /// <summary>
        /// Clears outline (disables outline rendering for glyphs that don't use it).
        /// </summary>
        internal void ClearOutline()
        {
            _outlineThicknessParam?.SetValue(0f);
        }
    }
}
