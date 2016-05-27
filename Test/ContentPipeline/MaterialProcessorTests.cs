using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;

namespace MonoGame.Tests.ContentPipeline
{
    class MaterialProcessorTests
    {
        [Test]
        public void ValidateDefaults()
        {
            var processor = new MaterialProcessor();
            Assert.AreEqual(new Color(255, 0, 255, 255), processor.ColorKeyColor);
            Assert.AreEqual(true, processor.ColorKeyEnabled);
            Assert.AreEqual(MaterialProcessorDefaultEffect.BasicEffect, processor.DefaultEffect);
            Assert.AreEqual(false, processor.GenerateMipmaps);
            Assert.AreEqual(true, processor.PremultiplyTextureAlpha);
            Assert.AreEqual(true, processor.ResizeTexturesToPowerOfTwo);
            Assert.AreEqual(TextureProcessorOutputFormat.Color, processor.TextureFormat);
        }

        [Test]
        public void CreateDefaultMaterialForBasicEffect()
        {
            var defaultMaterial = MaterialProcessor.CreateDefaultMaterial(MaterialProcessorDefaultEffect.BasicEffect);
            Assert.IsInstanceOf(typeof(BasicMaterialContent), defaultMaterial);
            Assert.IsNotNull(defaultMaterial);
        }


        [Test]
        public void CreateDefaultMaterialForSkinnedEffect()
        {
            var defaultMaterial = MaterialProcessor.CreateDefaultMaterial(MaterialProcessorDefaultEffect.SkinnedEffect);
            Assert.IsInstanceOf(typeof(SkinnedMaterialContent), defaultMaterial);
            Assert.IsNotNull(defaultMaterial);
        }

        [Test]
        public void CreateDefaultMaterialForEnvironmentMapEffect()
        {
            var defaultMaterial = MaterialProcessor.CreateDefaultMaterial(MaterialProcessorDefaultEffect.EnvironmentMapEffect);
            Assert.IsInstanceOf(typeof(EnvironmentMapMaterialContent), defaultMaterial);
            Assert.IsNotNull(defaultMaterial);
        }

        [Test]
        public void CreateDefaultMaterialForDualTextureEffect()
        {
            var defaultMaterial = MaterialProcessor.CreateDefaultMaterial(MaterialProcessorDefaultEffect.DualTextureEffect);
            Assert.IsInstanceOf(typeof(DualTextureMaterialContent), defaultMaterial);
            Assert.IsNotNull(defaultMaterial);
        }

        [Test]
        public void CreateDefaultMaterialForAlphaTestEffect()
        {
            var defaultMaterial = MaterialProcessor.CreateDefaultMaterial(MaterialProcessorDefaultEffect.AlphaTestEffect);
            Assert.IsInstanceOf(typeof(AlphaTestMaterialContent), defaultMaterial);
            Assert.IsNotNull(defaultMaterial);
        }

        [Test]
        public void CreateDefaultMaterialForUnkownEffect()
        {
            MaterialProcessorDefaultEffect effect = (MaterialProcessorDefaultEffect)(-1);//Create an enum value that does not exist
            Assert.Throws<ArgumentOutOfRangeException>(() => 
                MaterialProcessor.CreateDefaultMaterial(effect));
        }

        [Test]
        public void CreateDefaultEffectForAlphaTestMaterialContent()
        {
            var defaultEffect = MaterialProcessor.GetDefaultEffect(new AlphaTestMaterialContent());
            Assert.AreEqual(MaterialProcessorDefaultEffect.AlphaTestEffect, defaultEffect);
        }

        [Test]
        public void CreateDefaultEffectForBasicMaterialContent()
        {
            var defaultEffect = MaterialProcessor.GetDefaultEffect(new BasicMaterialContent());
            Assert.AreEqual(MaterialProcessorDefaultEffect.BasicEffect, defaultEffect);
        }

        [Test]
        public void CreateDefaultEffectForDualTextureMaterialContent()
        {
            var defaultEffect = MaterialProcessor.GetDefaultEffect(new DualTextureMaterialContent());
            Assert.AreEqual(MaterialProcessorDefaultEffect.DualTextureEffect, defaultEffect);
        }
        [Test]
        public void CreateDefaultEffectForEnvironmentMapMaterialContent()
        {
            var defaultEffect = MaterialProcessor.GetDefaultEffect(new EnvironmentMapMaterialContent());
            Assert.AreEqual(MaterialProcessorDefaultEffect.EnvironmentMapEffect, defaultEffect);
        }
        [Test]
        public void CreateDefaultEffectForSkinnedMaterialContent()
        {
            var defaultEffect = MaterialProcessor.GetDefaultEffect(new SkinnedMaterialContent());
            Assert.AreEqual(MaterialProcessorDefaultEffect.SkinnedEffect, defaultEffect);
        }

        [Test]
        public void CreateDefaultEffectForUnkownMaterial()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                MaterialProcessor.GetDefaultEffect(new MockMaterialContent()));
        }

        class MockMaterialContent : MaterialContent
        {
        }
    }
}
