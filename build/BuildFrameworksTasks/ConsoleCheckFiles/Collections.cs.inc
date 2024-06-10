using System.Collections.Generic;

namespace System
{
    public struct Index
    {
        public static Index Start = new Index(0);
        public static Index End = new Index(0, true);
        private readonly int value;
        public int Value { get { return IsFromEnd ? ~value : value; } }
        public readonly bool IsFromEnd;
        public Index(int index, bool fromEnd = false)
        {
            value = index;
            IsFromEnd = fromEnd;
        }
        public int GetOffset(int length)
        {
            if (!IsFromEnd) return Value;
            return length - Value;
        }
    }
    public struct Range
    {
        public static readonly Range All = new Range(Index.Start, Index.End);
        public readonly Index Start;
        public readonly Index End;
        public Range(Index start, Index end)
        {
            Start = start;
            End = end;
        }
        public Range(int start, Index end)
        {
            Start = new Index(start);
            End = end;
        }
        public Range(Index start, int end)
        {
            Start = start;
            End = new Index(end);
        }
        public Range(int start, int end)
        {
            Start = new Index(start);
            End = new Index(end);
        }
        public static Range StartAt(int at) { return new Range(at, Index.End); }
        public static Range EndAt(int at) { return new Range(Index.Start, at); }
    }
    public struct Span<T>
    {
        private readonly IList<T> values;
        private readonly int offset, length;
        public int Length { get { return length; } }
        public Span(IList<T> list)
        {
            values = list;
            offset = 0;
            length = -1;
        }
        public Span(IList<T> list, int start, int length)
        {
            values = list;
            offset = start;
            this.length = length;
        }
        public T this[int index] { get { return values[index + offset]; } set { values[index + offset] = value; } }
    }
    public struct ReadOnlySpan<T>
    {
        private readonly IList<T> values;
        private readonly int offset, length;
        public int Length { get { return length; } }
        public ReadOnlySpan(IList<T> list)
        {
            values = list;
            offset = 0;
            length = -1;
        }
        public ReadOnlySpan(IList<T> list, int start, int length)
        {
            values = list;
            offset = start;
            this.length = length;
        }
        public T this[int index] { get { return values[index + offset]; } }
    }
    public static class CollectionsMarshal
    {
        public static void SetCount<T>(List<T> list, int count)
        {
            list.Capacity = count;
            for (int i = 0; i < count; i++)
                list.Add(default(T));
        }
        public static Span<T> AsSpan<T>(List<T> list)
        {
            return new Span<T>(list);
        }
    }
    public static class ListExtensions
    {
        public static List<T> Slice<T>(this List<T> list, int start, int length)
        {
            var newList = new List<T>(length);
            for (int i = 0; i < length; i++)
                newList.Add(list[start + i]);
            return newList;
        }
    }
}
