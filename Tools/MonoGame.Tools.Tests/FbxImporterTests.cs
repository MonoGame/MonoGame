// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using NUnit.Framework;
using System.Collections.Generic;
using System.IO;

namespace MonoGame.Tests.ContentPipeline
{
    class FbxImporterTests
    {

        // XNA only supported up to the FBX 6.1.0 format
        // where as we only support FBX 7.1.0 and newer
        // via the Open Asset Importer.
        //
        //  - This is annoying for users porting old samples.
        //  - Conversion could loose data from the original FBX.
        //  - Do we care to resolve this in the future?
        //
#if XNA
        const string DudeFbx = "Assets/Models/Dude/dude.fbx";
#else
        const string DudeFbx = "Assets/Models/Dude/dude_2011.fbx";
#endif

        [Test]
        public void Arguments()
        {
            var context = new TestImporterContext("TestObj", "TestBin");
            var importer = new FbxImporter();

            Assert.Throws<ArgumentNullException>(() => importer.Import(null, context));
            Assert.Throws<FileNotFoundException>(() => importer.Import("does_not_exist", context));

            // XNA bug/omission:  crashes with a NullReferenceException 
            // where as we correctly throw a ArgumentNullException.
#if XNA
            Assert.Throws<NullReferenceException>(() => importer.Import(DudeFbx, null));
#else
            Assert.Throws<ArgumentNullException>(() => importer.Import("file", null));
#endif
        }

        [Test]
#if DESKTOPGL
        [Ignore("Disabled until latest NVTT is merged on Mac!")]
#endif
        public void Dude()
        {
            var context = new TestImporterContext("TestObj", "TestBin");
            var importer = new FbxImporter();

            var nodeContent = importer.Import(DudeFbx, context);

            Assert.AreEqual("RootNode", nodeContent.Name);
            Assert.AreEqual(null, nodeContent.Parent);

            Assert.AreEqual(0, nodeContent.Animations.Count);

            Assert.AreEqual(Matrix.Identity, nodeContent.Transform);
            Assert.AreEqual(Matrix.Identity, nodeContent.AbsoluteTransform);

            Assert.NotNull(nodeContent.Identity);
            Assert.NotNull(nodeContent.Identity.SourceFilename);
            Assert.IsNull(nodeContent.Identity.FragmentIdentifier);
            Assert.AreEqual("FbxImporter", nodeContent.Identity.SourceTool);

            Assert.AreEqual(2, nodeContent.Children.Count);


            // MeshContent
            Assert.IsInstanceOf<MeshContent>(nodeContent.Children[0]);
            var meshContent = nodeContent.Children[0] as MeshContent;
            Assert.AreEqual("him", meshContent.Name);
            Assert.AreEqual(nodeContent, meshContent.Parent);
            Assert.AreEqual(0, meshContent.Children.Count);
            Assert.AreEqual(0, meshContent.Animations.Count);
            Assert.AreEqual(0, meshContent.OpaqueData.Count);
            Assert.AreEqual(Matrix.Identity, meshContent.AbsoluteTransform);
            Assert.AreEqual(Matrix.Identity, meshContent.Transform);
            Assert.AreEqual(5, meshContent.Geometry.Count);

            // TODO: MG returns more positions than XNA.
            //
            //  - Is this a bug in our FbxImporer?
            //  - A limitation of AssImp?
            //  - Conversion bug from FBX 6.1.0 to FBX 7.1.0?
            //  - Are we missing some welding of verts?
            //
#if XNA
            Assert.AreEqual(11433, meshContent.Positions.Count);
#else
            Assert.AreEqual(13126, meshContent.Positions.Count);
#endif


            // MaterialContent
            var materials = new Dictionary<string, BasicMaterialContent>();
            foreach (var g in meshContent.Geometry)
            {
                Assert.IsNull(g.Name);
                //Assert.IsNull(g.Identity);
                Assert.AreEqual(meshContent, g.Parent);
                Assert.AreEqual(0, g.OpaqueData.Count);
                Assert.Greater(g.Indices.Count, 0);
                Assert.Greater(g.Vertices.VertexCount, 0);
                Assert.Greater(g.Vertices.Positions.Count, 0);
                Assert.Greater(g.Vertices.PositionIndices.Count, 0);

                Assert.NotNull(g.Material);
                Assert.IsInstanceOf<BasicMaterialContent>(g.Material);
                Assert.NotNull(g.Material.Identity);
                Assert.NotNull(g.Material.Identity.SourceFilename);
                Assert.IsNull(g.Material.Identity.FragmentIdentifier);
                Assert.AreEqual("FbxImporter", g.Material.Identity.SourceTool);
                Assert.NotNull(g.Material.Name);
                Assert.IsFalse(materials.ContainsKey(g.Material.Name));
                materials.Add(g.Material.Name, g.Material as BasicMaterialContent);
            }

            Assert.AreEqual(5, materials.Count);

            foreach (var m in materials.Values)
            {
                Assert.AreEqual(1, m.Alpha);
                Assert.AreEqual(new Vector3(0.0f, 0.0f, 0.0f), m.EmissiveColor);
                Assert.AreEqual(new Vector3(0.5f, 0.5f, 0.5f), m.SpecularColor);
                Assert.AreEqual(null, m.VertexColorEnabled);
                Assert.AreEqual(3, m.Textures.Count);
                Assert.AreEqual(m.Texture, m.Textures["Texture"]);
                Assert.IsNull(m.Textures["Texture"].Name);
                Assert.IsNull(m.Textures["Specular"].Name);
                Assert.IsNull(m.Textures["Bump"].Name);
                Assert.AreEqual("TextureCoordinate0", m.Textures["Texture"].OpaqueData["TextureCoordinate"]);
                Assert.AreEqual("TextureCoordinate0", m.Textures["Specular"].OpaqueData["TextureCoordinate"]);
                Assert.AreEqual("TextureCoordinate0", m.Textures["Bump"].OpaqueData["TextureCoordinate"]);
            }

            Assert.AreEqual(new Vector3(1.0f, 1.0f, 1.0f), materials["character_anim:headM"].DiffuseColor);
            Assert.AreEqual(new Vector3(1.0f, 1.0f, 1.0f), materials["character_anim:jacketM"].DiffuseColor);
            Assert.AreEqual(new Vector3(0.8f, 0.8f, 0.8f), materials["character_anim:pantsM"].DiffuseColor);
            Assert.AreEqual(new Vector3(1.0f, 1.0f, 1.0f), materials["character_anim:upBodyM"].DiffuseColor);
            Assert.AreEqual(new Vector3(1.0f, 1.0f, 1.0f), materials["character_anim:eyeBallM"].DiffuseColor);
            Assert.AreEqual(1.24573088f, materials["character_anim:headM"].SpecularPower, 0.00001f);
            Assert.AreEqual(1.24573088f, materials["character_anim:jacketM"].SpecularPower, 0.00001f);
            Assert.AreEqual(1.24573088f, materials["character_anim:pantsM"].SpecularPower, 0.00001f);
            Assert.AreEqual(1.19371974f, materials["character_anim:upBodyM"].SpecularPower, 0.00001f);
            Assert.AreEqual(65.986f, materials["character_anim:eyeBallM"].SpecularPower, 0.00001f);

            Paths.AreEqual(@"Assets/Models/Dude/head.tga", materials["character_anim:headM"].Textures["Texture"].Filename);
            Paths.AreEqual(@"Assets/Models/Dude/headS.tga", materials["character_anim:headM"].Textures["Specular"].Filename);
            Paths.AreEqual(@"Assets/Models/Dude/headN.tga", materials["character_anim:headM"].Textures["Bump"].Filename);
            Paths.AreEqual(@"Assets/Models/Dude/jacket.tga", materials["character_anim:jacketM"].Textures["Texture"].Filename);
            Paths.AreEqual(@"Assets/Models/Dude/jacketS.tga", materials["character_anim:jacketM"].Textures["Specular"].Filename);
            Paths.AreEqual(@"Assets/Models/Dude/jacketN.tga", materials["character_anim:jacketM"].Textures["Bump"].Filename);
            Paths.AreEqual(@"Assets/Models/Dude/pants.tga", materials["character_anim:pantsM"].Textures["Texture"].Filename);
            Paths.AreEqual(@"Assets/Models/Dude/pantsS.tga", materials["character_anim:pantsM"].Textures["Specular"].Filename);
            Paths.AreEqual(@"Assets/Models/Dude/pantsN.tga", materials["character_anim:pantsM"].Textures["Bump"].Filename);
            Paths.AreEqual(@"Assets/Models/Dude/upBodyC.tga", materials["character_anim:upBodyM"].Textures["Texture"].Filename);
            Paths.AreEqual(@"Assets/Models/Dude/upBodyS.tga", materials["character_anim:upBodyM"].Textures["Specular"].Filename);
            Paths.AreEqual(@"Assets/Models/Dude/upbodyN.tga", materials["character_anim:upBodyM"].Textures["Bump"].Filename);
            Paths.AreEqual(@"Assets/Models/Dude/upBodyC.tga", materials["character_anim:eyeBallM"].Textures["Texture"].Filename);
            Paths.AreEqual(@"Assets/Models/Dude/upBodyS.tga", materials["character_anim:eyeBallM"].Textures["Specular"].Filename);
            Paths.AreEqual(@"Assets/Models/Dude/upbodyN.tga", materials["character_anim:eyeBallM"].Textures["Bump"].Filename);

            // BoneContent
            Assert.IsInstanceOf<BoneContent>(nodeContent.Children[1]);
            var bonehContent = nodeContent.Children[1] as BoneContent;
            Assert.AreEqual("Root", bonehContent.Name);
            Assert.AreEqual(1, bonehContent.Children.Count);

            // TODO: MG doesn't return this.  
            //
            //  - Is this a bug in our FbxImporer?
            //  - A limitation of AssImp?
            //  - Conversion bug from FBX 6.1.0 to FBX 7.1.0?
            //  - What is "liw" and why is it false?
            //  - Do we care about this incompatibility?
            //
#if XNA
            Assert.AreEqual(1, bonehContent.OpaqueData.Count);
            Assert.AreEqual(false, bonehContent.OpaqueData["liw"]);
#endif


            // AnimationContent
            Assert.AreEqual(1, bonehContent.Animations.Count);
            Assert.IsTrue(bonehContent.Animations.ContainsKey("Take 001"));
            var animationContent = bonehContent.Animations["Take 001"];
            Assert.AreEqual("Take 001", animationContent.Name);
            Assert.AreEqual(0, animationContent.OpaqueData.Count);

            // TODO: A few channels are missing from XNA:
            //
            //  - Is this a bug in our FbxImporer?
            //  - A limitation of AssImp?
            //  - Conversion bug from FBX 6.1.0 to FBX 7.1.0?
            //  - Do these missing channels matter?
            //
#if XNA
            Assert.AreEqual(58, animationContent.Channels.Count);
#endif
            var channels = new[] {
                "Pelvis", "Spine1", "Spine2", "Spine3", "Neck", "Head", "L_eye_joint1", "R_eye_joint",
                "L_eyeBall_joint2", "R_eyeBall_joint", "L_UpperArm", "L_Forearm", "L_Hand", "L_Thumb1",
                "L_Thumb2", "L_Thumb3", "L_Index1", "L_Index2", "L_Index3", "L_Middle1", "L_Middle2", "L_Middle3",
                "L_Ring1", "L_Ring2", "L_Ring3", "L_Pinky1", "L_Pinky2", "L_Pinky3", "R_UpperArm", "R_Forearm",
                "R_Hand", "R_Thumb1", "R_Thumb2", "R_Thumb3", "R_Index1", "R_Index2", "R_Index3", "R_Middle1", "R_Middle2",
                "R_Middle3", "R_Ring1", "R_Ring2", "R_Ring3", "R_Pinky1", "R_Pinky2", "R_Pinky3", "L_Thigh1", "L_Knee2",
                "L_Ankle1", "L_Ball", "R_Thigh", "R_Knee", "R_Ankle", "R_Ball",

                // TODO: These channels are missing in MG!
#if XNA
                "Root", "Spine", "L_Clavicle", "R_Clavicle",
#endif
            };
            foreach (var name in channels)
                Assert.IsTrue(animationContent.Channels.ContainsKey(name), "Channels.ContainsKey failed: " + name);
            foreach (var c in animationContent.Channels.Values)
                Assert.Greater(c.Count, 0);

            // I think in this case the old XNA FBX importer was bugged and
            // returned a bigger animation duration that is correct.  Looking
            // at the content of the FBX ascii i can see the math is:
            // 
            //  (57732697500 - 1924423250) / 46186158000 = 1.208 seconds
            //
            // Which is the correct result and what our FBX importer returns.
            // I highly suspect that XNA was wrong.
            //
            // https://github.com/assimp/assimp/issues/1720
            //
#if XNA
            Assert.AreEqual(12670000, animationContent.Duration.Ticks);
#else
            Assert.AreEqual(12080000, animationContent.Duration.Ticks);            
#endif

            // TODO: XNA assigns the identity to null on all NodeContent
            // other than the one returned from the importer.
            //
            //  - Is this something we should fix?
            //
#if XNA
            Assert.IsNull(meshContent.Identity);
            Assert.IsNull(bonehContent.Identity);
            Assert.IsNull(animationContent.Identity);
#endif
        }
    }
}