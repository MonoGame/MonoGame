// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections;
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

    [ContentSerializer(ElementName = "Speed")]
    public float speed;

    [ContentSerializer(ElementName = "Organic")]
    public bool isOrganic;

    [ContentSerializer(ElementName = "Dimensions")]
    private Vector2 dimensions;

    public Vector2 Dimensions
    {
        get { return dimensions; }
    }

    internal void SetDimensions(Vector2 value)
    {
        dimensions = value;
    }
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

    [ContentSerializer(Optional = true)]
    public CullMode? d = null;

    [ContentSerializer(Optional = true)]
    public CullMode? e = CullMode.CullClockwiseFace;

    public CullMode? f = CullMode.CullCounterClockwiseFace;

    public CullMode g = CullMode.CullClockwiseFace;
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

public enum CustomEnum
{
    Val1,
    Val2
}

public class CustomItem
{
    public CustomEnum EnumVal;
}

public class CustomItemBase
{
    public double DoubleVal;
    public Nullable<float> NullableFloatVal;
}

public class CustomItemInherited : CustomItemBase
{
    public char[] CharArrayVal;
}

public class Collections
{
    public string[] StringArray;
    public List<string> StringList;
    public int[] IntArray;
    public Color[] ColorArray;
    public List<CustomItem> CustomItemList;
    public List<CustomItemInherited> CustomItemInheritedList;

    // Indexer - should be ignored by intermediate serializer.
    public Color this[int i]
    {
        get { return ColorArray[i]; }
        set { ColorArray[i] = value; }
    }
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
    public Vector2 Vector2;
    public Vector3 Vector3;
    public Vector4 Vector4;
    public Quaternion Quaternion;
    public Plane Plane;
    public Matrix Matrix;
    public Color Color;
    public Vector2[] Vector2Array = new Vector2[0];
    public List<Vector2> Vector2List = new List<Vector2>();
    public List<Vector2> Vector2ListEmpty = new List<Vector2>();
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
    public ICollection<int> IntCollection;
    public object UntypedDictionary;
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

    public Linked2[] LinkedArray;
}

public class Linked
{
    public int Value;

    [ContentSerializer(SharedResource = true)]
    public Linked Next;
}

public class Linked2
{
    [ContentSerializer(SharedResource = true)]
    public Linked2[] Next;
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

#region GetterOnlyProperties
class GetterOnlyProperties
{
    private readonly List<int> _intList;
    private readonly Dictionary<int, string> _intStringDictionary;
    private readonly AnotherClass _customClass;
    private readonly AnotherClass[] _customClassArray;
    private readonly AnotherStruct _customStruct;

    public int IntValue
    {
        get { return 0; }
    }

    public Vector2 Dimensions
    {
        get { return new Vector2(16, 16); }
    }

    public List<int> IntList
    {
        get { return _intList; }
    }

    public Dictionary<int, string> IntStringDictionaryWithPrivateSetter { get; private set; }

    public Dictionary<int, string> IntStringDictionary
    {
        get { return _intStringDictionary; }
    }

    public class AnotherClass
    {
        public int A;
    }

    public AnotherClass CustomClass
    {
        get { return _customClass; }
    }

    public object UntypedCustomClass
    {
        get { return _customClass; }
    }

    public AnotherClass[] CustomClassArray
    {
        get { return _customClassArray; }
    }

    public object UntypedCustomClassArray
    {
        get { return _customClassArray; }
    }

    public struct AnotherStruct
    {
        public int A;
    }

    public AnotherStruct CustomStruct
    {
        get { return _customStruct; }
    }

    public GetterOnlyProperties()
    {
        _intList = new List<int>();
        IntStringDictionaryWithPrivateSetter = new Dictionary<int, string>();
        _intStringDictionary = new Dictionary<int, string>();
        _customClass = new AnotherClass();
        _customClassArray = new [] { new AnotherClass { A = 42 } };
        _customStruct = new AnotherStruct();
    }
}
#endregion

#region GetterOnlyPolymorphicArrayProperties
class GetterOnlyPolymorphicArrayProperties
{
    private readonly AnotherClass[] _customClassArray;

    public class AnotherClass
    {
        public int A;
    }

    public IList CustomClassArrayAsIList
    {
        get { return _customClassArray; }
    }

    public GetterOnlyPolymorphicArrayProperties()
    {
        _customClassArray = new[] { new AnotherClass { A = 42 } };
    }
}
#endregion

#region GenericTypes
class GenericTypes
{
    public GenericClass<int> A;
    public GenericClass<float> B;
    public GenericClass<GenericArg> C;
}

class GenericClass<T>
{
    public T Value;
}
public class GenericArg
{
    public int Value;
}
#endregion

#region ChildCollections
public class ChildCollections
{
    private readonly ChildrenCollection _children;

    [ContentSerializer]
    public ChildrenCollection Children
    {
        get { return _children; }
    }

    public ChildCollections()
    {
        _children = new ChildrenCollection(this);
    }
}

public class ChildrenCollection : ChildCollection<ChildCollections, ChildCollectionChild>
{
    public ChildrenCollection(ChildCollections parent) : base(parent)
    {
    }

    protected override ChildCollections GetParent(ChildCollectionChild child)
    {
        return child.Parent;
    }

    protected override void SetParent(ChildCollectionChild child, ChildCollections parent)
    {
        child.Parent = parent;
    }
}

public class ChildCollectionChild : ContentItem
{
    [ContentSerializerIgnore]
    public ChildCollections Parent { get; set; }
}
#endregion

#region Colors
public class Colors
{
    public Color White { get; set; }
    public Color Black { get; set; }
    public Color Transparent { get; set; }
    public Color Red { get; set; }
    public Color Green { get; set; }
    public Color Blue { get; set; }
}
#endregion

class StructArrayNoElements
{
    public Vector2[] Vector2ArrayNoElements = new Vector2[] {};
}

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
        public List<string> H;
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
    public class CustomFormatting<T, U>
    {
        public T A;
        public List<Vector2> Vector2ListSpaced = new List<Vector2>();
        public string EmptyString;
        public U Rectangle;
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