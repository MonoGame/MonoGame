using System;
using System.IO;
using NUnit.Framework;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;

namespace MonoGame.Tests.ContentPipeline
{
    [TestFixture]
    public class CustomModelProcessorTest
    {
        public class CustomModelProcessor : ModelProcessor
        {
            public override ModelContent Process(
            NodeContent input, ContentProcessorContext context)
            {
                GenerateTangents(input, context);
                UseParentBoneNameIfMeshNameIsNotSet(input);
                StoreEffectTechniqueInMeshName(input, context);
                return base.Process(input, context);
            }

            private void GenerateTangents(
            NodeContent input, ContentProcessorContext context)
            {
                MeshContent mesh = input as MeshContent;
                if (mesh != null)
                {
                    // Generate trangents for the mesh. We don't want binormals,
                    // so null is passed in for the last parameter.
                    MeshHelper.CalculateTangentFrames(mesh,
                        VertexChannelNames.TextureCoordinate(0),
                        VertexChannelNames.Tangent(0), null);
                }

                // Go through all childs
                foreach (NodeContent child in input.Children)
                {
                    GenerateTangents(child, context);
                }
            }

            private void UseParentBoneNameIfMeshNameIsNotSet(NodeContent input)
            {
                if (String.IsNullOrEmpty(input.Name) &&
                    input.Parent != null &&
                    String.IsNullOrEmpty(input.Parent.Name) == false)
                    input.Name = input.Parent.Name;

                foreach (NodeContent node in input.Children)
                    UseParentBoneNameIfMeshNameIsNotSet(node);
            }

            private void StoreEffectTechniqueInMeshName(
            NodeContent input, ContentProcessorContext context)
            {
                MeshContent mesh = input as MeshContent;
                if (mesh != null)
                {
                    foreach (GeometryContent geom in mesh.Geometry)
                    {
                        EffectMaterialContent effectMaterial =
                            geom.Material as EffectMaterialContent;
                        if (effectMaterial != null)
                        {
                            if (effectMaterial.OpaqueData.ContainsKey("technique"))
                            {
                                input.Name = input.Name + effectMaterial.OpaqueData["technique"];
                            }
                        }
                    }
                }

                foreach (NodeContent child in input.Children)
                {
                    StoreEffectTechniqueInMeshName(child, context);
                }
            }
        }

        [Test]
        public void BuildModelUsingCustomProcessor ()
        {
            var context = new TestProcessorContext(TargetPlatform.DesktopGL, "Car.xnb");
            var processor = new CustomModelProcessor();
            var importer = new XImporter();
            Directory.CreateDirectory("bin");
            Directory.CreateDirectory("obj");
            var c = new TestImporterContext("obj", "bin");
            var node = importer.Import(Path.Combine("Assets", "Models", "Car.x"), c);
            var model = processor.Process(node, context);
        }
    }
}
