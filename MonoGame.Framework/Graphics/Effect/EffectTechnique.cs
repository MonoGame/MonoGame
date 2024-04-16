using System;
namespace Microsoft.Xna.Framework.Graphics
{
    /// <summary>
    /// Represents a technique used in a shader effect.
    /// </summary>
    /// <remarks>
    /// Creating and assigning a <b>EffectTechnique</b> instance for each technique in your <see cref="Effect"/>
    /// is significantly faster than using the <see cref="Effect.Techniques"/> indexed property on <see cref="Effect"/>.
    /// </remarks>
    /// <example>
    /// 1) Create a <b>EffectTechnique</b> for each technique in your Effect. <para/>
    /// <code>
    /// public EffectTechnique texture;
    /// public EffectTechnique shadows;
    /// public EffectTechnique shadowMap;
    /// </code>
    /// 2) Assign an <see cref="Effect"/> technique to your <b>EffectTechnique</b>. <para/>
    /// <code>
    /// texture = effect.Techniques["TextureRender"];
    /// shadowMap = effect.Techniques["ShadowMapRender"];
    /// shadows = effect.Techniques["ShadowRender"];
    /// </code>
    /// 3) Assign your <b>EffectTechnique</b> to the
    /// <see cref="Effect.CurrentTechnique"/> of your <see cref="Effect"/> before drawing.
    /// <code>
    /// private void DrawScene(EffectTechnique technique)
    /// {
    ///    MyEffect.mWorld.SetValue(terrainWorld);
    ///    MyEffect.MeshTexture.SetValue(terrainTex);
    ///    foreach (ModelMesh mesh in terrain.Meshes)
    ///    {
    ///        foreach (Effect effect in mesh.Effects)
    ///        {
    ///            effect.CurrentTechnique = technique;
    ///            mesh.Draw();
    ///        }
    ///    }
    /// }
    /// </code>
    /// </example>
    public class EffectTechnique
	{
        /// <summary>
        /// Gets the collection of <see cref="EffectPass"/> objects this rendering technique requires.
        /// </summary>
        public EffectPassCollection Passes { get; private set; }

        /// <summary>
        /// Gets the <see cref="EffectAnnotation"/> objects associated with this technique.
        /// </summary>
        public EffectAnnotationCollection Annotations { get; private set; }

        /// <summary>
        /// Gets the name of this technique.
        /// </summary>
        public string Name { get; private set; }

        internal EffectTechnique(Effect effect, EffectTechnique cloneSource)
        {
            // Share all the immutable types.
            Name = cloneSource.Name;
            Annotations = cloneSource.Annotations;

            // Clone the mutable types.
            Passes = cloneSource.Passes.Clone(effect);
        }

        internal EffectTechnique(Effect effect, string name, EffectPassCollection passes, EffectAnnotationCollection annotations)
        {
            Name = name;
            Passes = passes;
            Annotations = annotations;
        }
    }
}