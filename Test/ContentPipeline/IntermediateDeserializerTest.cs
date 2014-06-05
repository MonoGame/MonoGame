﻿using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Intermediate;
using Microsoft.Xna.Framework.Graphics;
using NUnit.Framework;
using System.Xml;

namespace MonoGame.Tests.ContentPipeline
{
    class IntermediateDeserializerTest
    {
        [Test]
        public void TheBasics()
        {
            object result;
            var filePath = Paths.Xml("01_TheBasics.xml");
            using (var reader = XmlReader.Create(filePath))            
                result = IntermediateSerializer.Deserialize<object>(reader, filePath);

            Assert.NotNull(result);
            Assert.IsAssignableFrom<TheBasics>(result);

            var theBasics = (TheBasics)result;
            Assert.AreEqual(1, theBasics.PublicField);
            Assert.AreEqual(0, theBasics.InternalField);
            Assert.AreEqual("Hello World", theBasics.GetSetProperty);
            Assert.NotNull(theBasics.Nested);
            Assert.AreEqual("Shawn", theBasics.Nested.Name);
            Assert.AreEqual(true, theBasics.Nested.IsEnglish);
        }

        [Test]
        public void Inheritance()
        {
            object result;
            var filePath = Paths.Xml("02_Inheritance.xml");
            using (var reader = XmlReader.Create(filePath))
                result = IntermediateSerializer.Deserialize<object>(reader, filePath);

            Assert.NotNull(result);
            Assert.IsAssignableFrom<Inheritance>(result);

            var inheritance = (Inheritance)result;
            Assert.AreEqual(23, inheritance.elf);
            Assert.AreEqual("world", inheritance.hello);
        }

        [Test]
        public void IncludingPrivateMembers()
        {
            object result;
            var filePath = Paths.Xml("03_IncludingPrivateMembers.xml");
            using (var reader = XmlReader.Create(filePath))
                result = IntermediateSerializer.Deserialize<object>(reader, filePath);

            Assert.NotNull(result);
            Assert.IsAssignableFrom<IncludingPrivateMembers>(result);

            var including = (IncludingPrivateMembers)result;
            Assert.AreEqual(23, including.GetElfValue());
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
            object result;
            var filePath = Paths.Xml("05_RenamingXmlElements.xml");
            using (var reader = XmlReader.Create(filePath))
                result = IntermediateSerializer.Deserialize<object>(reader, filePath);

            Assert.NotNull(result);
            Assert.IsAssignableFrom<RenamingXmlElements>(result);

            var renaming = (RenamingXmlElements)result;
            Assert.AreEqual("world", renaming.hello);
            Assert.AreEqual(23, renaming.elf);
        }

        [Test]
        public void NullReferences()
        {
            object result;
            var filePath = Paths.Xml("06_NullReferences.xml");
            using (var reader = XmlReader.Create(filePath))
                result = IntermediateSerializer.Deserialize<object>(reader, filePath);

            Assert.NotNull(result);
            Assert.IsAssignableFrom<NullReferences>(result);

            var nullref = (NullReferences)result;
            Assert.AreEqual(null, nullref.hello);
        }

        [Test]
        public void OptionalElements()
        {
            object result;
            var filePath = Paths.Xml("07_OptionalElements.xml");
            using (var reader = XmlReader.Create(filePath))
                result = IntermediateSerializer.Deserialize<object>(reader, filePath);

            Assert.NotNull(result);
            Assert.IsAssignableFrom<OptionalElements>(result);

            var optional = (OptionalElements)result;
            Assert.AreEqual(null, optional.a);
            Assert.AreEqual(null, optional.b);
            Assert.AreEqual(string.Empty, optional.c);
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
            object result;
            var filePath = Paths.Xml("09_Collections.xml");
            using (var reader = XmlReader.Create(filePath))
                result = IntermediateSerializer.Deserialize<object>(reader, filePath);

            Assert.NotNull(result);
            Assert.IsAssignableFrom<Collections>(result);
            var collections = (Collections)result;

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
        }

        [Test]
        public void CollectionItemName()
        {
            object result;
            var filePath = Paths.Xml("10_CollectionItemName.xml");
            using (var reader = XmlReader.Create(filePath))
                result = IntermediateSerializer.Deserialize<object>(reader, filePath);

            Assert.NotNull(result);
            Assert.IsAssignableFrom<CollectionItemName>(result);
            var collections = (CollectionItemName)result;

            Assert.NotNull(collections.StringArray);
            Assert.AreEqual(2, collections.StringArray.Length);
            Assert.AreEqual("Hello", collections.StringArray[0]);
            Assert.AreEqual("World", collections.StringArray[1]);
        }

        [Test]
        public void Dictionaries()
        {
            object result;
            var filePath = Paths.Xml("11_Dictionaries.xml");
            using (var reader = XmlReader.Create(filePath))
                result = IntermediateSerializer.Deserialize<object>(reader, filePath);

            Assert.NotNull(result);
            Assert.IsAssignableFrom<Dictionaries>(result);
            var dictionaries = (Dictionaries)result;

            Assert.NotNull(dictionaries.TestDictionary);
            Assert.AreEqual(2, dictionaries.TestDictionary.Count);
            Assert.AreEqual(true, dictionaries.TestDictionary[23]);
            Assert.AreEqual(false, dictionaries.TestDictionary[42]);
        }

        [Test]
        public void MathTypes()
        {
            object result;
            var filePath = Paths.Xml("12_MathTypes.xml");
            using (var reader = XmlReader.Create(filePath))
                result = IntermediateSerializer.Deserialize<object>(reader, filePath);

            Assert.NotNull(result);
            Assert.IsAssignableFrom<MathTypes>(result);
            var mathTypes = (MathTypes)result;

            Assert.AreEqual(new Vector3(1, 2, 3), mathTypes.Vector);
            Assert.AreEqual(new Rectangle(1, 2, 3, 4), mathTypes.Rectangle);
            Assert.AreEqual(new Quaternion(1, 2, 3, 4), mathTypes.Quaternion);
            Assert.AreEqual(Color.CornflowerBlue, mathTypes.Color);
            Assert.NotNull(mathTypes.VectorArray);
            Assert.AreEqual(2, mathTypes.VectorArray.Length);
            Assert.AreEqual(Vector2.Zero, mathTypes.VectorArray[0]);
            Assert.AreEqual(Vector2.One, mathTypes.VectorArray[1]);
        }

        [Test]
        public void PolymorphicTypes()
        {
            object result;
            var filePath = Paths.Xml("13_PolymorphicTypes.xml");
            using (var reader = XmlReader.Create(filePath))
                result = IntermediateSerializer.Deserialize<object>(reader, filePath);

            Assert.NotNull(result);
            Assert.IsInstanceOf<PolymorphicTypes>(result);
            var polymorphicTypes = (PolymorphicTypes)result;

            Assert.AreEqual("World", polymorphicTypes.Hello);
            Assert.AreEqual(23, polymorphicTypes.Elf);
            Assert.NotNull(polymorphicTypes.PolymorphicArray);
            Assert.AreEqual(3, polymorphicTypes.PolymorphicArray.Length);
            Assert.IsAssignableFrom<PolymorphicA>(polymorphicTypes.PolymorphicArray[0]);
            Assert.AreEqual(true, polymorphicTypes.PolymorphicArray[0].Value);
            Assert.IsAssignableFrom<PolymorphicB>(polymorphicTypes.PolymorphicArray[1]);
            Assert.AreEqual(true, polymorphicTypes.PolymorphicArray[1].Value);
            Assert.IsAssignableFrom<PolymorphicC>(polymorphicTypes.PolymorphicArray[2]);
            Assert.AreEqual(true, polymorphicTypes.PolymorphicArray[2].Value);
        }

        [Test]
        public void Namespaces()
        {
            object result;
            var filePath = Paths.Xml("14_Namespaces.xml");
            using (var reader = XmlReader.Create(filePath))
                result = IntermediateSerializer.Deserialize<object>(reader, filePath);

            Assert.NotNull(result);
            Assert.IsAssignableFrom<NamespaceClass>(result);
            var namespaceClass = (NamespaceClass)result;

            Assert.IsAssignableFrom<NamespaceHelper>(namespaceClass.A);
            Assert.AreEqual(true, ((NamespaceHelper)namespaceClass.A).Value);
            Assert.IsAssignableFrom<Vector2>(namespaceClass.B);
            Assert.AreEqual(Vector2.Zero, namespaceClass.B);
            Assert.IsAssignableFrom<SpriteSortMode>(namespaceClass.C);
            Assert.AreEqual(SpriteSortMode.Immediate, namespaceClass.C);
        }

        [Test]
        public void FlattenContent()
        {
            object result;
            var filePath = Paths.Xml("15_FlattenContent.xml");
            using (var reader = XmlReader.Create(filePath))
                result = IntermediateSerializer.Deserialize<object>(reader, filePath);

            Assert.NotNull(result);
            Assert.IsAssignableFrom<FlattenContent>(result);
            var flattenContent = (FlattenContent)result;

            Assert.IsAssignableFrom<NestedClass>(flattenContent.Nested);
            Assert.NotNull(flattenContent.Nested);
            Assert.AreEqual("Shawn", flattenContent.Nested.Name);
            Assert.AreEqual(true, flattenContent.Nested.IsEnglish);
            Assert.NotNull(flattenContent.Collection);
            Assert.AreEqual(2, flattenContent.Collection.Length);
            Assert.AreEqual("Hello", flattenContent.Collection[0]);
            Assert.AreEqual("World", flattenContent.Collection[1]);
        }

        [Test]
        public void SharedResources()
        {
            object result;
            var filePath = Paths.Xml("16_SharedResources.xml");
            using (var reader = XmlReader.Create(filePath))
                result = IntermediateSerializer.Deserialize<object>(reader, filePath);

            Assert.NotNull(result);
            Assert.IsAssignableFrom<SharedResources>(result);
            var sharedResources = (SharedResources)result;

            Assert.NotNull(sharedResources.Head);
            Assert.AreEqual(1, sharedResources.Head.Value);
            Assert.NotNull(sharedResources.Head.Next);
            Assert.AreEqual(2, sharedResources.Head.Next.Value);
            Assert.NotNull(sharedResources.Head.Next.Next);
            Assert.AreEqual(3, sharedResources.Head.Next.Next.Value);
            Assert.AreSame(sharedResources.Head, sharedResources.Head.Next.Next.Next);
        }

        [Test]
        public void ExternalReferences()
        {
            object result;
            var filePath = Paths.Xml("17_ExternalReferences.xml");
            using (var reader = XmlReader.Create(filePath))
                result = IntermediateSerializer.Deserialize<object>(reader, filePath);

            Assert.NotNull(result);
            Assert.IsAssignableFrom<ExternalReferences>(result);
            var externalReferences = (ExternalReferences)result;

            Assert.NotNull(externalReferences.Texture);
            Assert.IsTrue(externalReferences.Texture.Filename.EndsWith(@"\Xml\grass.tga"));
            Assert.NotNull(externalReferences.Shader);
            Assert.IsTrue(externalReferences.Shader.Filename.EndsWith(@"\Xml\foliage.fx"));
        }
    }
}
