// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

#region The Basics
public class TheBasics
{
    public int PublicField;
    protected int ProtectedField;
    private int PrivateField;
    internal int InternalField;

    public string GetSetProperty
    {
        get;
        set;
    }

    public string GetOnlyProperty
    {
        get { return "Hello World"; }
    }

    public string SetOnlyProperty { set; private get; }

    public NestedClass Nested;
    public NestedClass Nested2;

    public TheBasics()
    {
        Nested = Nested2 = new NestedClass();
    }
}

public class NestedClass
{
    public string Name;
    public bool IsEnglish;
}
#endregion

#region Inheritance
public class InheritanceBase
{
    public int elf;
}

public class Inheritance : InheritanceBase
{
    public string hello;
}
#endregion

#region IncludingPrivateMembers
public class IncludingPrivateMembers
{
    public IncludingPrivateMembers()
    {
    }

    public IncludingPrivateMembers(int elf)
    {
        this.elf = elf;
    }

    [ContentSerializer]
    private int elf; // will be serialized

    public int GetElfValue()
    {
        return elf;
    }
}
#endregion

#region ExcludingPublicMembers
public class ExcludingPublicMembers
{
    [ContentSerializerIgnore]
    public int elf; // will not be serialized
}
#endregion

#region RenamingXmlElements
public class RenamingXmlElements
{
    [ContentSerializer(ElementName = "ShawnSaysHello")]
    public string hello;

    [ContentSerializer(ElementName = "ElvesAreCool")]
    public int elf;
}
#endregion

#region NullReferences
public class NullReferences
{
    public string hello = "world";
}
#endregion

#region OptionalElements
public class OptionalElements
{
    [ContentSerializer(Optional = true)]
    public string a = null;

    public string b = "b";

    [ContentSerializer(Optional = true)]
    public string c = "c";
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
    public string[] StringArray;
    public List<string> StringList;
    public int[] IntArray;
}
#endregion

#region CollectionItemName
public class CollectionItemName
{
    [ContentSerializer(ElementName = "w00t", CollectionItemName = "Flibble")]
    public string[] StringArray;
}
#endregion

#region Dictionaries
public class Dictionaries
{
    public Dictionary<int, bool> TestDictionary = new Dictionary<int, bool>();
}
#endregion

#region Primitive Types
public class PrimitiveTypes
{
    public char Char;
    public char Char2;
    public char Char3;
    public byte Byte;
    public sbyte SByte;
    public short Short;
    public ushort UShort;
    public int Int;
    public uint UInt;
    public long Long;
    public ulong ULong;
    public float Float;
    public double Double;
    public char? NullChar;                        
    public char? NotNullChar;
}
#endregion

#region MathTypes
public class MathTypes
{
    public Point Point;
    public Rectangle Rectangle;
    public Vector3 Vector3;
    public Vector4 Vector4;
    public Quaternion Quaternion;
    public Plane Plane;
    public Matrix Matrix;
    public Color Color;
    public Vector2[] Vector2Array = new Vector2[0];
    public List<Vector2> Vector2List = new List<Vector2>();
}
#endregion

#region PolymorphicTypes
public class PolymorphicA
{
    public bool Value;
}

public class PolymorphicB : PolymorphicA { }
public class PolymorphicC : PolymorphicA { }

public class PolymorphicTypes
{
    public object Hello;
    public object Elf;
    public PolymorphicA[] TypedArray;
    public Array UntypedArray;
}
#endregion

#region FlattenContent
public class FlattenContent
{
    [ContentSerializer(FlattenContent = true)]
    public NestedClass Nested;

    [ContentSerializer(FlattenContent = true, CollectionItemName = "Boo")]
    public string[] Collection;
}
#endregion

#region SharedResources
public class SharedResources
{
    [ContentSerializer(SharedResource = true)]
    public Linked Head;
}

public class Linked
{
    public int Value;

    [ContentSerializer(SharedResource = true)]
    public Linked Next;
}
#endregion

#region CircularReferences
public class CircularReferences
{
    public CircularLinked Head;
}

public class CircularLinked
{
    public int Value;

    public CircularLinked Next;
}
#endregion

#region ExternalReferences
public class ExternalReferences
{
    public ExternalReference<Texture2D> Texture;
    public ExternalReference<Texture2D> Texture2;
    public ExternalReference<Effect> Shader;
}
#endregion

#region FontDescription
class ExtendedFontDescription : FontDescription
{
    public ExtendedFontDescription()
        : base("Arial", 14, 0)
    {
    }

    [ContentSerializer(Optional = true, CollectionItemName = "Item")]
    public List<string> ExtendedListProperty
    {
        get { return _list; }
    }

    private List<string> _list = new List<string>();
}
#endregion

#region SystemTypes
class SystemTypes
{
    public TimeSpan TimeSpan;
}
#endregion

namespace MonoGame.Tests.ContentPipeline
{
    #region Namespaces
    public class NamespaceHelper
    {
        public bool Value;
    }

    public class NamespaceClass
    {
        public object A;
        public object B;
        public object C;
        public object D;
        public object E;
        public object F;
        public object G;
    }

    namespace Nested
    {
        public class ClassInsideNestedNamespace
        {
            public bool Value;
        }

        namespace ContentPipeline
        {
            public class ClassInsideNestedAmbiguousNamespace
            {
                public bool Value;
            }
        }

        namespace ContentPipeline2
        {
            public class ClassInsideNestedUnambiguousNamespace
            {
                public bool Value;
            }
        }
    }
    #endregion

    #region CustomFormatting
    public class CustomFormatting
    {
        public byte A;
        public List<Vector2> Vector2ListSpaced = new List<Vector2>();
    }
    #endregion
}

namespace MonoGame.Tests.SomethingElse.ContentPipeline
{
    public class ClassInsideAmbiguousNamespace
    {
        public bool Value;
    }
}