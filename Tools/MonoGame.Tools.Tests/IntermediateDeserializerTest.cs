// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Xml;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Intermediate;
using Microsoft.Xna.Framework.Graphics;
using NUnit.Framework;
#if XNA
using System.Reflection;
#endif

namespace MonoGame.Tests.ContentPipeline
{
    // These tests are based on "Everything you ever wanted to know about IntermediateSerializer" by Shawn Hargreaves
    // http://blogs.msdn.com/b/shawnhar/archive/2008/08/12/everything-you-ever-wanted-to-know-about-intermediateserializer.aspx

    class IntermediateDeserializerTest
    {
        class TestContentManager : ContentManager
        {
            class FakeGraphicsService : IGraphicsDeviceService
            {
                public GraphicsDevice GraphicsDevice { get; private set; }

                #pragma warning disable 67
                public event EventHandler<EventArgs> DeviceCreated;
                public event EventHandler<EventArgs> DeviceDisposing;
                public event EventHandler<EventArgs> DeviceReset;
                public event EventHandler<EventArgs> DeviceResetting;
                #pragma warning restore 67
            }

            class FakeServiceProvider : IServiceProvider
            {
                public object GetService(Type serviceType)
                {
                    if (serviceType == typeof(IGraphicsDeviceService))
                        return new FakeGraphicsService();

                    throw new NotImplementedException();
                }
            }

            private readonly MemoryStream _xnbStream;

            public TestContentManager(MemoryStream xnbStream)
                : base(new FakeServiceProvider(), "NONE")
            {
                _xnbStream = xnbStream;
            }

            protected override Stream OpenStream(string assetName)
            {
                return new MemoryStream(_xnbStream.GetBuffer(), false);
            }
        }

        private static T Deserialize<T>(string file, Action<T> doAsserts)
        {
            object result;
            var filePath = Paths.Xml(file);
            using (var reader = XmlReader.Create(filePath))
                result = IntermediateSerializer.Deserialize<object>(reader, filePath);

            Assert.NotNull(result);
            Assert.IsAssignableFrom<T>(result);

            doAsserts((T)result);

            return (T)result;
        }

        private static void DeserializeCompileAndLoad<T>(string file, Action<T> doAsserts)
        {
            var result = Deserialize(file, doAsserts);

            var xnbStream = new MemoryStream();
#if XNA
            // In MS XNA the ContentCompiler is completely internal, so we need
            // to use just a little reflection to get access to what we need.
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
            var ctor = typeof(ContentCompiler).GetConstructors(flags)[0];
            var compiler = (ContentCompiler)ctor.Invoke(null);
            var compileMethod = typeof(ContentCompiler).GetMethod("Compile", flags);
            compileMethod.Invoke(compiler, new object[] { xnbStream, result, TargetPlatform.Windows, GraphicsProfile.Reach,
                                false, Directory.GetCurrentDirectory(), "referenceRelocationPath" });
#else
            var compiler = new ContentCompiler();
            compiler.Compile(xnbStream, result, TargetPlatform.Windows, GraphicsProfile.Reach, 
                                false, "rootDirectory", "referenceRelocationPath");
#endif

            var content = new TestContentManager(xnbStream);
            var loaded = content.Load<T>("Whatever");

            doAsserts(loaded);
        }

        [Test]
        public void TheBasics()
        {
            DeserializeCompileAndLoad<TheBasics>("01_TheBasics.xml", theBasics =>
            {
                Assert.AreEqual(1, theBasics.PublicField);
                Assert.AreEqual(0, theBasics.InternalField);
                Assert.AreEqual("Hello World", theBasics.GetSetProperty);
                Assert.NotNull(theBasics.Nested);
                Assert.AreEqual("Shawn", theBasics.Nested.Name);
                Assert.AreEqual(true, theBasics.Nested.IsEnglish);
                Assert.NotNull(theBasics.Nested2);
                Assert.AreEqual("Shawn", theBasics.Nested2.Name);
                Assert.AreEqual(true, theBasics.Nested2.IsEnglish);
                Assert.AreNotSame(theBasics.Nested, theBasics.Nested2);
            });
        }

        [Test]
        public void Inheritance()
        {
            DeserializeCompileAndLoad<Inheritance>("02_Inheritance.xml", inheritance =>
            {
                Assert.AreEqual(23, inheritance.elf);
                Assert.AreEqual("world", inheritance.hello);
            });
        }

        [Test]
        public void IncludingPrivateMembers()
        {
            DeserializeCompileAndLoad<IncludingPrivateMembers>("03_IncludingPrivateMembers.xml", including =>
            {
                Assert.AreEqual(23, including.GetElfValue());
            });
        }

        [Test]
        public void ExcludingPublicMembers()
        {
            var filePath = Paths.Xml("04_ExcludingPublicMembers.xml");
            using (var reader = XmlReader.Create(filePath))
            {
                // This should throw an InvalidContentException as the
                // xml tries to set the <elf> element which has a 
                // [ContentSerializerIgnore] attribute.
                Assert.Throws<InvalidContentException>(() =>
                    IntermediateSerializer.Deserialize<object>(reader, filePath));
            }
        }

        [Test]
        public void RenamingXmlElements()
        {
            DeserializeCompileAndLoad<RenamingXmlElements>("05_RenamingXmlElements.xml", renaming =>
            {
                Assert.AreEqual("world", renaming.hello);
                Assert.AreEqual(23, renaming.elf);
                Assert.AreEqual(80.2f, renaming.speed);
                Assert.AreEqual(true, renaming.isOrganic);
                Assert.AreEqual(new Vector2(32, 32), renaming.Dimensions);
            });
        }

        [Test]
        public void NullReferences()
        {
            DeserializeCompileAndLoad<NullReferences>("06_NullReferences.xml", nullref =>
            {
                Assert.AreEqual(null, nullref.hello);
            });
        }

        [Test]
        public void OptionalElements()
        {
            DeserializeCompileAndLoad<OptionalElements>("07_OptionalElements.xml", optional =>
            {
                Assert.AreEqual(null, optional.a);
                Assert.AreEqual(null, optional.b);
                Assert.AreEqual(string.Empty, optional.c);
                Assert.AreEqual(null, optional.d);
                Assert.AreEqual(CullMode.CullClockwiseFace, optional.e);
                Assert.AreEqual(CullMode.CullCounterClockwiseFace, optional.f);
                Assert.AreEqual(CullMode.CullClockwiseFace, optional.g);
            });
        }

        [Test]
        public void AllowNull()
        {
            var filePath = Paths.Xml("08_AllowNull.xml");
            using (var reader = XmlReader.Create(filePath))
            {
                // This should throw an InvalidContentException as the
                // xml tries to set the <elf> element which has a 
                // [ContentSerializerIgnore] attribute.
                Assert.Throws<InvalidContentException>(() =>
                    IntermediateSerializer.Deserialize<object>(reader, filePath));
            }
        }

        [Test]
        public void Collections()
        {
            DeserializeCompileAndLoad<Collections>("09_Collections.xml", collections =>
            {
                Assert.NotNull(collections.StringArray);
                Assert.AreEqual(2, collections.StringArray.Length);
                Assert.AreEqual("Hello", collections.StringArray[0]);
                Assert.AreEqual("World", collections.StringArray[1]);

                Assert.NotNull(collections.StringList);
                Assert.AreEqual(4, collections.StringList.Count);
                Assert.AreEqual("This", collections.StringList[0]);
                Assert.AreEqual("is", collections.StringList[1]);
                Assert.AreEqual("a", collections.StringList[2]);
                Assert.AreEqual("test", collections.StringList[3]);

                Assert.NotNull(collections.IntArray);
                Assert.AreEqual(5, collections.IntArray.Length);
                Assert.AreEqual(1, collections.IntArray[0]);
                Assert.AreEqual(2, collections.IntArray[1]);
                Assert.AreEqual(3, collections.IntArray[2]);
                Assert.AreEqual(23, collections.IntArray[3]);
                Assert.AreEqual(42, collections.IntArray[4]);

                Assert.NotNull(collections.ColorArray);
                Assert.AreEqual(4, collections.ColorArray.Length);
                Assert.AreEqual(new Color(0x88, 0x65, 0x42, 0xFF), collections.ColorArray[0]);
                Assert.AreEqual(new Color(0x91, 0x6B, 0x46, 0xFF), collections.ColorArray[1]);
                Assert.AreEqual(new Color(0x91, 0x7B, 0x46, 0xFF), collections.ColorArray[2]);
                Assert.AreEqual(new Color(0x88, 0x65, 0x43, 0xFF), collections.ColorArray[3]);

                Assert.NotNull(collections.CustomItemList);
                Assert.AreEqual(0, collections.CustomItemList.Count);
            });            
        }

        [Test]
        public void CollectionItemName()
        {
            DeserializeCompileAndLoad<CollectionItemName>("10_CollectionItemName.xml", collections =>
            {
                Assert.NotNull(collections.StringArray);
                Assert.AreEqual(2, collections.StringArray.Length);
                Assert.AreEqual("Hello", collections.StringArray[0]);
                Assert.AreEqual("World", collections.StringArray[1]);
            });
        }

        [Test]
        public void Dictionaries()
        {
            DeserializeCompileAndLoad<Dictionaries>("11_Dictionaries.xml", dictionaries =>
            {
                Assert.NotNull(dictionaries.TestDictionary);
                Assert.AreEqual(2, dictionaries.TestDictionary.Count);
                Assert.AreEqual(true, dictionaries.TestDictionary[23]);
                Assert.AreEqual(false, dictionaries.TestDictionary[42]);
            });
        }

        [Test]
        public void MathTypes()
        {
            DeserializeCompileAndLoad<MathTypes>("12_MathTypes.xml", mathTypes =>
            {
                Assert.AreEqual(new Point(1, 2), mathTypes.Point);
                Assert.AreEqual(new Rectangle(1, 2, 3, 4), mathTypes.Rectangle);
                Assert.AreEqual(new Vector2(1, 2), mathTypes.Vector2);
                Assert.AreEqual(new Vector3(1, 2, 3.1f), mathTypes.Vector3);
                Assert.AreEqual(new Vector4(1, 2, 3, 4), mathTypes.Vector4);
                Assert.AreEqual(new Quaternion(1, 2, 3, 4), mathTypes.Quaternion);
                Assert.AreEqual(new Plane(1, 2, 3, 4), mathTypes.Plane);
                Assert.AreEqual(new Matrix(1, 2, 3, 4, 5 , 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16), mathTypes.Matrix);
                Assert.AreEqual(Color.CornflowerBlue, mathTypes.Color);
                Assert.NotNull(mathTypes.Vector2Array);
                Assert.AreEqual(2, mathTypes.Vector2Array.Length);
                Assert.AreEqual(Vector2.Zero, mathTypes.Vector2Array[0]);
                Assert.AreEqual(Vector2.One, mathTypes.Vector2Array[1]);
                Assert.AreEqual(3, mathTypes.Vector2List.Count);
                Assert.AreEqual(new Vector2(1, 7), mathTypes.Vector2List[0]);
                Assert.AreEqual(new Vector2(1, 9), mathTypes.Vector2List[1]);
                Assert.AreEqual(new Vector2(1, 10), mathTypes.Vector2List[2]);
                Assert.AreEqual(0, mathTypes.Vector2ListEmpty.Count);
            });
        }

        [Test]
        public void PolymorphicTypes()
        {
            DeserializeCompileAndLoad<PolymorphicTypes>("13_PolymorphicTypes.xml", polymorphicTypes =>
            {
                Assert.AreEqual("World", polymorphicTypes.Hello);
                Assert.AreEqual(23, polymorphicTypes.Elf);

                Assert.NotNull(polymorphicTypes.TypedArray);
                Assert.AreEqual(3, polymorphicTypes.TypedArray.Length);
                Assert.IsAssignableFrom<PolymorphicA>(polymorphicTypes.TypedArray[0]);
                Assert.AreEqual(true, polymorphicTypes.TypedArray[0].Value);
                Assert.IsAssignableFrom<PolymorphicB>(polymorphicTypes.TypedArray[1]);
                Assert.AreEqual(true, polymorphicTypes.TypedArray[1].Value);
                Assert.IsAssignableFrom<PolymorphicC>(polymorphicTypes.TypedArray[2]);
                Assert.AreEqual(true, polymorphicTypes.TypedArray[2].Value);

                Assert.NotNull(polymorphicTypes.UntypedArray);
                Assert.AreEqual(3, polymorphicTypes.UntypedArray.Length);
                Assert.IsAssignableFrom<PolymorphicA>(polymorphicTypes.UntypedArray.GetValue(0));
                Assert.AreEqual(true, ((PolymorphicA)polymorphicTypes.UntypedArray.GetValue(0)).Value);
                Assert.IsAssignableFrom<PolymorphicB>(polymorphicTypes.UntypedArray.GetValue(1));
                Assert.AreEqual(true, ((PolymorphicB)polymorphicTypes.UntypedArray.GetValue(1)).Value);
                Assert.IsAssignableFrom<PolymorphicC>(polymorphicTypes.UntypedArray.GetValue(2));
                Assert.AreEqual(true, ((PolymorphicC)polymorphicTypes.UntypedArray.GetValue(2)).Value);

                Assert.NotNull(polymorphicTypes.IntCollection);
                Assert.IsInstanceOf<List<int>>(polymorphicTypes.IntCollection);
                Assert.AreEqual(3, polymorphicTypes.IntCollection.Count);
                Assert.AreEqual(1, polymorphicTypes.IntCollection.ElementAt(0));
                Assert.AreEqual(4, polymorphicTypes.IntCollection.ElementAt(1));
                Assert.AreEqual(6, polymorphicTypes.IntCollection.ElementAt(2));

                Assert.NotNull(polymorphicTypes.UntypedDictionary);
                Assert.IsInstanceOf<Dictionary<int, PolymorphicA>>(polymorphicTypes.UntypedDictionary);
                Assert.AreEqual(2, ((Dictionary<int, PolymorphicA>) polymorphicTypes.UntypedDictionary).Count);
                Assert.AreEqual(true, ((Dictionary<int, PolymorphicA>) polymorphicTypes.UntypedDictionary)[1].Value);
                Assert.AreEqual(false, ((Dictionary<int, PolymorphicA>) polymorphicTypes.UntypedDictionary)[5].Value);
            });
        }

        [Test]
        public void Namespaces()
        {
            DeserializeCompileAndLoad<NamespaceClass>("14_Namespaces.xml", namespaceClass =>
            {
                Assert.IsAssignableFrom<NamespaceHelper>(namespaceClass.A);
                Assert.AreEqual(true, ((NamespaceHelper)namespaceClass.A).Value);
                Assert.IsAssignableFrom<Vector2>(namespaceClass.B);
                Assert.AreEqual(Vector2.Zero, namespaceClass.B);
                Assert.IsAssignableFrom<SpriteSortMode>(namespaceClass.C);
                Assert.AreEqual(SpriteSortMode.Immediate, namespaceClass.C);
                Assert.IsAssignableFrom<Nested.ContentPipeline.ClassInsideNestedAmbiguousNamespace>(namespaceClass.D);
                Assert.AreEqual(true, ((Nested.ContentPipeline.ClassInsideNestedAmbiguousNamespace) namespaceClass.D).Value);
                Assert.IsAssignableFrom<Nested.ClassInsideNestedNamespace>(namespaceClass.E);
                Assert.AreEqual(true, ((Nested.ClassInsideNestedNamespace) namespaceClass.E).Value);
                Assert.IsAssignableFrom<Nested.ContentPipeline2.ClassInsideNestedUnambiguousNamespace>(namespaceClass.F);
                Assert.AreEqual(true, ((Nested.ContentPipeline2.ClassInsideNestedUnambiguousNamespace) namespaceClass.F).Value);
                Assert.IsAssignableFrom<SomethingElse.ContentPipeline.ClassInsideAmbiguousNamespace>(namespaceClass.G);
                Assert.AreEqual(true, ((SomethingElse.ContentPipeline.ClassInsideAmbiguousNamespace) namespaceClass.G).Value);
                Assert.IsNull(namespaceClass.H);
            });
        }

        [Test]
        public void FlattenContent()
        {
            DeserializeCompileAndLoad<FlattenContent>("15_FlattenContent.xml", flattenContent =>
            {
                Assert.IsAssignableFrom<NestedClass>(flattenContent.Nested);
                Assert.NotNull(flattenContent.Nested);
                Assert.AreEqual("Shawn", flattenContent.Nested.Name);
                Assert.AreEqual(true, flattenContent.Nested.IsEnglish);
                Assert.NotNull(flattenContent.Collection);
                Assert.AreEqual(2, flattenContent.Collection.Length);
                Assert.AreEqual("Hello", flattenContent.Collection[0]);
                Assert.AreEqual("World", flattenContent.Collection[1]);
            });
        }

        [Test]
        public void SharedResources()
        {
            DeserializeCompileAndLoad<SharedResources>("16_SharedResources.xml", sharedResources =>
            {
                Assert.NotNull(sharedResources.Head);
                Assert.AreEqual(1, sharedResources.Head.Value);
                Assert.NotNull(sharedResources.Head.Next);
                Assert.AreEqual(2, sharedResources.Head.Next.Value);
                Assert.NotNull(sharedResources.Head.Next.Next);
                Assert.AreEqual(3, sharedResources.Head.Next.Next.Value);
                Assert.AreSame(sharedResources.Head, sharedResources.Head.Next.Next.Next);

                Assert.NotNull(sharedResources.LinkedArray);
                Assert.AreEqual(2, sharedResources.LinkedArray.Length);
                Assert.IsNotNull(sharedResources.LinkedArray[0].Next);
                Assert.AreEqual(2, sharedResources.LinkedArray[0].Next.Length);
                Assert.IsNotNull(sharedResources.LinkedArray[1].Next);
                Assert.AreEqual(1, sharedResources.LinkedArray[1].Next.Length);
                Assert.AreSame(sharedResources.LinkedArray[0].Next, sharedResources.LinkedArray[1].Next[0].Next);
            });
        }

        [Test]
        public void ExternalReferences()
        {
            Deserialize<ExternalReferences>("17_ExternalReferences.xml", externalReferences =>
            {
                Assert.NotNull(externalReferences.Texture);
                Assert.IsTrue(externalReferences.Texture.Filename.EndsWith("/Xml/grass.tga".Replace('/', Path.DirectorySeparatorChar)));
                Assert.NotNull(externalReferences.Texture2);
                Assert.IsTrue(externalReferences.Texture2.Filename.EndsWith("/Xml/grass.tga".Replace ('/', Path.DirectorySeparatorChar)));
                Assert.AreNotSame(externalReferences.Texture, externalReferences.Texture2);
                Assert.NotNull(externalReferences.Shader);
                Assert.IsTrue(externalReferences.Shader.Filename.EndsWith("/Xml/foliage.fx".Replace ('/', Path.DirectorySeparatorChar)));
            });
        }

        [Test]
        public void PrimitiveTypes()
        {
            DeserializeCompileAndLoad<PrimitiveTypes>("18_PrimitiveTypes.xml", primitiveTypes =>
            {
                Assert.AreEqual('A', primitiveTypes.Char);
                Assert.AreEqual('°', primitiveTypes.Char2);
                Assert.AreEqual('Δ', primitiveTypes.Char3);
                Assert.AreEqual(127, primitiveTypes.Byte);
                Assert.AreEqual(-127, primitiveTypes.SByte);
                Assert.AreEqual(-1000, primitiveTypes.Short);
                Assert.AreEqual(1000, primitiveTypes.UShort);
                Assert.AreEqual(-100000, primitiveTypes.Int);
                Assert.AreEqual(100000, primitiveTypes.UInt);
                Assert.AreEqual(-10000000, primitiveTypes.Long);
                Assert.AreEqual(10000000, primitiveTypes.ULong);
                Assert.AreEqual(1234567.0f, primitiveTypes.Float);
                Assert.AreEqual(1234567890.0, primitiveTypes.Double);
                Assert.AreEqual(null, primitiveTypes.NullChar);
                Assert.AreEqual('B', primitiveTypes.NotNullChar);
            });
        }

        [Test]
        public void FontDescription()
        {
            DeserializeCompileAndLoad<ExtendedFontDescription>("19_FontDescription.xml", fontDesc =>
            {
                Assert.AreEqual("Foo", fontDesc.FontName);
                Assert.AreEqual(30.0f, fontDesc.Size);
                Assert.AreEqual(0.75f, fontDesc.Spacing);
                Assert.AreEqual(true, fontDesc.UseKerning);
                Assert.AreEqual(FontDescriptionStyle.Bold, fontDesc.Style);
                Assert.AreEqual('*', fontDesc.DefaultCharacter);
                        
                var expectedCharacters = new List<char>();
                for (var c = WebUtility.HtmlDecode("&#32;")[0]; c <= WebUtility.HtmlDecode("&#126;")[0]; c++)
                    expectedCharacters.Add(c);

                expectedCharacters.Add(WebUtility.HtmlDecode("&#916;")[0]);
                expectedCharacters.Add(WebUtility.HtmlDecode("&#176;")[0]);

                var characters = new List<char>(fontDesc.Characters);
                foreach (var c in expectedCharacters)
                {
                    Assert.Contains(c, characters);
                    characters.Remove(c);
                }

                Assert.IsEmpty(characters);

                var expectedStrings = new List<string>()
                    {
                        "item0",
                        "item1",
                        "item2",
                    };
                var strings = new List<string>(fontDesc.ExtendedListProperty);
                foreach (var s in expectedStrings)
                {
                    Assert.Contains(s, strings);
                    strings.Remove(s);
                }

                Assert.IsEmpty(strings);
            });
        }

        [Test]
        public void SystemTypes()
        {
            DeserializeCompileAndLoad<SystemTypes>("20_SystemTypes.xml", sysTypes =>
            {
                Assert.AreEqual(TimeSpan.FromSeconds(42.5f), sysTypes.TimeSpan);
            });
        }

        [Test]
        public void CustomFormatting()
        {
            DeserializeCompileAndLoad<CustomFormatting<byte, Rectangle>>("21_CustomFormatting.xml", customFormatting =>
            {
                Assert.AreEqual(1, customFormatting.A);
                Assert.AreEqual(3, customFormatting.Vector2ListSpaced.Count);
                Assert.AreEqual(new Vector2(0, 4), customFormatting.Vector2ListSpaced[0]);
                Assert.AreEqual(new Vector2(0, 6), customFormatting.Vector2ListSpaced[1]);
                Assert.AreEqual(new Vector2(0, 7), customFormatting.Vector2ListSpaced[2]);
                Assert.AreEqual(string.Empty, customFormatting.EmptyString);
                Assert.AreEqual(new Rectangle(0, 0, 100, 100), customFormatting.Rectangle);
            });
        }

        [Test]
        public void GetterOnlyProperties()
        {
            DeserializeCompileAndLoad<GetterOnlyProperties>("22_GetterOnlyProperties.xml", getterOnlyProps =>
            {
                Assert.AreEqual(3, getterOnlyProps.IntList.Count);
                Assert.AreEqual(1, getterOnlyProps.IntList[0]);
                Assert.AreEqual(2, getterOnlyProps.IntList[1]);
                Assert.AreEqual(3, getterOnlyProps.IntList[2]);
                Assert.AreEqual(0, getterOnlyProps.IntStringDictionaryWithPrivateSetter.Count);
                Assert.AreEqual(2, getterOnlyProps.IntStringDictionary.Count);
                Assert.AreEqual("Foo", getterOnlyProps.IntStringDictionary[1]);
                Assert.AreEqual("Bar", getterOnlyProps.IntStringDictionary[5]);
                Assert.AreEqual(42, getterOnlyProps.CustomClass.A);
            });
        }

        [Test]
        public void GetterOnlyPolymorphicArrayProperties()
        {
            var filePath = Paths.Xml("23_GetterOnlyPolymorphicArrayProperties.xml");
            using (var reader = XmlReader.Create(filePath))
            {
                // This should throw an InvalidContentException as the
                // xml tries to deserialize into an IList property
                // but the property value is actually an Array.
                Assert.Throws<InvalidOperationException>(() =>
                    IntermediateSerializer.Deserialize<GetterOnlyPolymorphicArrayProperties>(reader, filePath));
            }
        }

        [Test]
        public void GenericTypes()
        {
            DeserializeCompileAndLoad<GenericTypes>("24_GenericTypes.xml", genericTypes =>
            {
                Assert.IsNotNull(genericTypes.A);
                Assert.AreEqual(3, genericTypes.A.Value);
                Assert.IsNotNull(genericTypes.B);
                Assert.AreEqual(4.2f, genericTypes.B.Value);
                Assert.IsNotNull(genericTypes.C);
                Assert.IsNotNull(genericTypes.C.Value);
                Assert.AreEqual(5, genericTypes.C.Value.Value);
            });
        }

        [Test]
        public void StructArrayNoElements()
        {
            // Note that this does not contain a matching SerializeAndAssert test as Vector2ArrayNoElements
            // will serialize to an empty Xml element which defeats the purpose of this test.
            DeserializeCompileAndLoad<StructArrayNoElements>("25_StructArrayNoElements.xml", structArrayNoElems =>
            {
                Assert.IsNotNull(structArrayNoElems.Vector2ArrayNoElements);
                Assert.AreEqual(0, structArrayNoElems.Vector2ArrayNoElements.Length);
            });
        }

        [Test]
        public void ChildCollections()
        {
            // ChildCollection is a ContentPipeline-only type, so we don't need to / shouldn't
            // test running it through ContentCompiler.
            Deserialize<ChildCollections>("26_ChildCollections.xml", childCollections =>
            {
                Assert.IsNotNull(childCollections.Children);
                Assert.AreEqual(2, childCollections.Children.Count);
                Assert.AreEqual(childCollections, childCollections.Children[0].Parent);
                Assert.AreEqual("Foo", childCollections.Children[0].Name);
                Assert.AreEqual(childCollections, childCollections.Children[1].Parent);
                Assert.AreEqual("Bar", childCollections.Children[1].Name);
            });
        }

        [Test]
#if DESKTOPGL
        [Ignore("Fails on Mac build server some reason.")]
#endif
        public void Colors()
        {
            DeserializeCompileAndLoad<Colors>("27_Colors.xml", colors =>
            {
                Assert.AreEqual(colors.White, Color.White);
                Assert.AreEqual(colors.Black, Color.Black);
                Assert.AreEqual(colors.Transparent, Color.Transparent);
                Assert.AreEqual(colors.Red, Color.Red);
                Assert.AreEqual(colors.Green, Color.Green);
                Assert.AreEqual(colors.Blue, Color.Blue);
            });
        }

        [Test]
        public void XnaCurve()
        {
            // Curve in 28_XnaCurve.xml is formated the same way as by XNA's serializer
            DeserializeCompileAndLoad<Curve>("28_XnaCurve.xml", curve =>
            {
                Assert.AreEqual(CurveLoopType.Constant, curve.PreLoop);
                Assert.AreEqual(CurveLoopType.Constant, curve.PostLoop);
                Assert.AreEqual(2, curve.Keys.Count);
                var key1 = curve.Keys[0];
                Assert.AreEqual(0, key1.Position);
                Assert.AreEqual(1, key1.Value);
                Assert.AreEqual(0, key1.TangentIn);
                Assert.AreEqual(0, key1.TangentOut);
                Assert.AreEqual(CurveContinuity.Smooth, key1.Continuity);
                var key2 = curve.Keys[1];
                Assert.AreEqual(0.5f, key2.Position);
                Assert.AreEqual(0.5f, key2.Value);
                Assert.AreEqual(0, key2.TangentIn);
                Assert.AreEqual(0, key2.TangentOut);
                Assert.AreEqual(CurveContinuity.Smooth, key2.Continuity);
            });
        }
    }
}
