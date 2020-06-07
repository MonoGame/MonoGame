// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;

namespace Microsoft.Xna.Framework
{
    /// <summary>
    /// Storage for Extended multibyte characters
    /// </summary>
    /// <remarks>
    /// Will properly encode entire Unicode 11 set.
    /// </remarks>
    public struct CharEx : IConvertible, IComparable<CharEx>, IComparable<Char>, IEquatable<CharEx>, IEquatable<Char>
    {
        public readonly int Value;

        #region Constructors

        public CharEx(int UTF32)
        {
            Value = UTF32;
        }

        public CharEx(char High, char Low)
        {
            Value = char.ConvertToUtf32(High, Low);
        }

        public CharEx(char UTF16)
        {
            if (char.IsSurrogate(UTF16))
                throw new ArgumentOutOfRangeException();
            Value = (int)UTF16;
        }

        public CharEx(string UTF32)
        {
            // check for xml encoding
            // use signed 64bit values to handle overflow cases and signed values
            if (UTF32.Length > 2)
                if (UTF32.StartsWith("&#x"))
                    Value = (int)long.Parse(UTF32.Substring(4).Split(';')[0], System.Globalization.NumberStyles.AllowHexSpecifier, System.Globalization.CultureInfo.InvariantCulture);
                else
                        if (UTF32.StartsWith("&#"))
                    Value = (int)long.Parse(UTF32.Substring(3).Split(';')[0], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture);
                else
                    throw new ArgumentOutOfRangeException();

            Value = char.ConvertToUtf32(UTF32, 0);
        }
        #endregion Constructors

        #region Instance Methods

        public bool isASCII()
        {
            return Value <= 0x7F;
        }

        /// <summary>
        /// Simple Test for possibility of cast to char
        /// </summary>
        /// <remarks>Does not imply the cast will be a valid char</remarks>
        /// <returns>Boolean</returns>
        public bool isUTF16()
        {
            return Value <= 0xFFFF;
        }

        /// <summary>
        /// Will a cast to char return a surrogate
        /// </summary>
        /// <returns>Boolean</returns>
        public bool isUTF32()
        {
            return Value > 0xFFFF;
        }

        /// <summary>
        /// Returns the Character as UTF8 encoded bytes
        /// </summary>
        /// <returns>UTF8 encoded bytes</returns>
        public byte[] GetUTF8()
        {
            return System.Text.Encoding.UTF8.GetBytes(ToString().ToCharArray());
        }

        /// <summary>
        /// Returns the Character as UTF16 encoded bytes
        /// </summary>
        /// <returns>UTF16 encoded bytes</returns>
        public byte[] GetUTF16()
        {
            return System.Text.Encoding.Unicode.GetBytes(ToString().ToCharArray());
        }

        #endregion Instance Methods

        #region Static Methods

        /// <summary>
        /// Convert a string to CharEx array
        /// </summary>
        /// <param name="s">String to convert</param>
        /// <returns>Array of CharEx</returns>
        public static CharEx[] ToArray(String s)
        {
            // Allocate list to simplify output
            List<CharEx> tmpList = new List<CharEx>(s.Length * 2);
            for (int i = 0; i < s.Length; i++)
            {
                if (!Char.IsSurrogate(s[i]))
                    tmpList.Add(new CharEx(s[i]));
                else
                {
                    if (i == s.Length - 1)
                        throw new ArgumentOutOfRangeException("Unmatched surrogate at end of string");
                    tmpList.Add(new CharEx(s[i++], s[i]));
                }
            }
            return tmpList.ToArray();
        }
        #endregion Static Methods

        // Disable CLS compliance checks. Custom value types are not CLS compliant. 
#pragma warning disable 3002

        #region Operators

        public static implicit operator char(CharEx c)
        {
            if (c.isUTF32())
                throw new InvalidCastException();
            return (char)c.Value;
        }

        public static implicit operator CharEx(char c)
        {
            return new CharEx(c);
        }

        public static implicit operator int(CharEx c)
        {
            return c.Value;
        }

        public static implicit operator uint(CharEx c)
        {
            return (uint)c.Value;
        }

        public static bool operator >(CharEx a, CharEx b)
        {
            return a.Value > b.Value;
        }

        public static bool operator <(CharEx a, CharEx b)
        {
            return a.Value < b.Value;
        }

        public static CharEx operator +(CharEx a, int i)
        {
            return new CharEx(a.Value + i);
        }

        public static CharEx operator ++(CharEx a)
        {
            return new CharEx(a.Value + 1);
        }

        public static CharEx operator -(CharEx a, int i)
        {
            return new CharEx(a.Value - i);
        }

        public static CharEx operator --(CharEx a)
        {
            return new CharEx(a.Value - 1);
        }

        public static bool operator ==(CharEx a, CharEx b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(CharEx a, char b)
        {
            return !a.Equals(b);
        }

        public static bool operator ==(CharEx a, char b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(CharEx a, CharEx b)
        {
            return !a.Equals(b);
        }

        public static String ToString(CharEx[] Array)
        {
            System.Text.StringBuilder o = new System.Text.StringBuilder();
            foreach (var item in Array)
            {
                o.Append(item.ToString());
            }
            return o.ToString();
        }

        #endregion Operators

        #region Conversion Types
        public TypeCode GetTypeCode()
        {
            return Value.GetTypeCode();
        }

        public bool ToBoolean(IFormatProvider provider)
        {
            return ((IConvertible)Value).ToBoolean(provider);
        }

        public byte ToByte(IFormatProvider provider)
        {
            return ((IConvertible)Value).ToByte(provider);
        }

        public char ToChar(IFormatProvider provider)
        {
            if (isUTF32())
                throw new InvalidCastException();
            return ((IConvertible)Value).ToChar(provider);
        }

        public DateTime ToDateTime(IFormatProvider provider)
        {
            return ((IConvertible)Value).ToDateTime(provider);
        }

        public decimal ToDecimal(IFormatProvider provider)
        {
            return ((IConvertible)Value).ToDecimal(provider);
        }

        public double ToDouble(IFormatProvider provider)
        {
            return ((IConvertible)Value).ToDouble(provider);
        }

        public short ToInt16(IFormatProvider provider)
        {
            return ((IConvertible)Value).ToInt16(provider);
        }

        public int ToInt32(IFormatProvider provider)
        {
            return ((IConvertible)Value).ToInt32(provider);
        }

        public long ToInt64(IFormatProvider provider)
        {
            return ((IConvertible)Value).ToInt64(provider);
        }

        public sbyte ToSByte(IFormatProvider provider)
        {
            return ((IConvertible)Value).ToSByte(provider);
        }

        public float ToSingle(IFormatProvider provider)
        {
            return ((IConvertible)Value).ToSingle(provider);
        }

        public override string ToString()
        {
            return char.ConvertFromUtf32(Value);
        }

        public string ToString(IFormatProvider provider)
        {
            return Value.ToString(provider);
        }

        public object ToType(Type conversionType, IFormatProvider provider)
        {
            return ((IConvertible)Value).ToType(conversionType, provider);
        }

        public ushort ToUInt16(IFormatProvider provider)
        {
            return ((IConvertible)Value).ToUInt16(provider);
        }

        public uint ToUInt32(IFormatProvider provider)
        {
            return ((IConvertible)Value).ToUInt32(provider);
        }

        public ulong ToUInt64(IFormatProvider provider)
        {
            return ((IConvertible)Value).ToUInt64(provider);
        }

        #endregion Conversion Types
#pragma warning restore 3002

        public int CompareTo(CharEx obj)
        {
            return Value.CompareTo(obj.Value);
        }

        public int CompareTo(Char obj)
        {
            return Value.CompareTo((int)obj);
        }

        public bool Equals(CharEx other)
        {
            return this.Value == other.Value;
        }

        public bool Equals(char other)
        {
            return this.Value == (int)other;
        }

        public override bool Equals(Object other)
        {
            return this == (CharEx)other;
        }

        public override int GetHashCode()
        {
            if (isUTF16())
                return ((char)Value).GetHashCode();
            // No precedent, return int hash for simplicity
            return Value.GetHashCode();
        }

    }
}
