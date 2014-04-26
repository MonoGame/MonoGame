// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace MonoGame.Tests.ContentPipeline
{
    #region The Basics
    public class TheBasics
    {
        public int PublicField = 1;
        protected int ProtectedField = 2;
        private int PrivateField = 3;
        internal int InternalField = 4;

        public string GetSetProperty
        {
            get { return "Hello World"; }
            set { }
        }

        public string GetOnlyProperty
        {
            get { return "Hello World"; }
        }

        public string SetOnlyProperty
        {
            set { }
        }

        public NestedClass Nested = new NestedClass();
    }

    public class NestedClass
    {
        public string Name = "Shawn";
        public bool IsEnglish = true;
    }
    #endregion

    #region Inheritance
    public class InheritanceBase
    {
        public int elf = 23;
    }

    public class Inheritance : InheritanceBase
    {
        public string hello = "world";
    }
    #endregion

    #region IncludingPrivateMembers
    public class IncludingPrivateMembers
    {
        [ContentSerializer]
        private int elf = 23; // will be serialized
    }
    #endregion

    #region ExcludingPublicMembers
    public class ExcludingPublicMembers
    {
        [ContentSerializerIgnore]
        public int elf = 23; // will not be serialized
    }
    #endregion

    #region RenamingXmlElements
    public class RenamingXmlElements
    {
        [ContentSerializer(ElementName = "ShawnSaysHello")]
        string hello = "world";

        [ContentSerializer(ElementName = "ElvesAreCool")]
        int elf = 23;
    }
    #endregion

    #region NullReferences
    public class NullReferences
    {
        public string hello = null;
    }
    #endregion

    #region OptionalElements
    public class OptionalElements
    {
        [ContentSerializer(Optional = true)]
        public string a = null;

        public string b = null;

        [ContentSerializer(Optional = true)]
        public string c = string.Empty;
    }
    #endregion

    #region AllowNull
    public class AllowNull
    {
        [ContentSerializer(AllowNull = false)]
        string a;
    }
    #endregion

    #region Collections
    public class Collections
    {
        public string[] StringArray = { "Hello", "World" };
        public List<string> StringList = new List<string> { "This", "is", "a", "test" };
        public int[] IntArray = { 1, 2, 3, 23, 42 };
    }
    #endregion

    #region CollectionItemName
    public class CollectionItemName
    {
        [ContentSerializer(ElementName = "w00t", CollectionItemName = "Flibble")]
        string[] StringArray = { "Hello", "World" };
    }
    #endregion

    #region Dictionaries
    public class Dictionaries
    {
        public Dictionary<int, bool> TestDictionary = new Dictionary<int, bool>();

        public Dictionaries()
        {
            TestDictionary.Add(23, true);
            TestDictionary.Add(42, false);
        }
    }
    #endregion

    #region MathTypes
    public class MathTypes
    {
        public Vector3 Vector = new Vector3(1, 2, 3);
        public Rectangle Rectangle = new Rectangle(1, 2, 3, 4);
        public Quaternion Quaternion = new Quaternion(1, 2, 3, 4);
        public Color Color = Color.CornflowerBlue;
        public Vector2[] VectorArray = new Vector2[] { Vector2.Zero, Vector2.One };
    }
    #endregion

    #region PolymorphicTypes
    public class PolymorphicA
    {
        public bool Value = true;
    }

    public class PolymorphicB : PolymorphicA { }
    public class PolymorphicC : PolymorphicA { }

    public class PolymorphicTypes
    {
        public object Hello = "World";
        public object Elf = 23;
        public PolymorphicA[] PolymorphicArray = { new PolymorphicA(), new PolymorphicB(), new PolymorphicC() };
    }
    #endregion

    #region Namespaces
    public class NamespaceHelper
    {
        public bool Value = true;
    }

    public class NamespaceClass
    {
        public object A = new NamespaceHelper();
        public object B = Vector2.Zero;
        public object C = Color.CornflowerBlue;
    }
    #endregion

    #region FlattenContent
    public class FlattenContent
    {
        [ContentSerializer(FlattenContent = true)]
        public NestedClass Nested = new NestedClass();

        [ContentSerializer(FlattenContent = true, CollectionItemName = "Boo")]
        public string[] Collection = { "Hello", "World" };
    }
    #endregion

    #region SharedResources
    public class SharedResources
    {
        [ContentSerializer(SharedResource = true)]
        public Linked Head;

        public SharedResources()
        {
            Head = new Linked();
            Head.Value = 1;

            Head.Next = new Linked();
            Head.Next.Value = 2;

            Head.Next.Next = new Linked();
            Head.Next.Next.Value = 3;

            Head.Next.Next.Next = Head;
        }
    }

    public class Linked
    {
        public int Value;

        [ContentSerializer(SharedResource = true)]
        public Linked Next;
    }
    #endregion

    #region ExternalReferences
    public class ExternalReferences
    {
        public ExternalReference<Texture2D> Texture = new ExternalReference<Texture2D>("grass.tga");
        public ExternalReference<Effect> Shader = new ExternalReference<Effect>("foliage.fx");
    }
    #endregion
}
