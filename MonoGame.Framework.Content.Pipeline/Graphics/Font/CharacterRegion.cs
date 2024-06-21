using System;
using System.ComponentModel;
using System.Collections.Generic;

namespace Microsoft.Xna.Framework.Content.Pipeline.Graphics
{
	/// <summary>
	/// Describes a range of consecutive characters that should be included in the font.
	/// </summary>
	[TypeConverter(typeof(CharacterRegionTypeConverter))]
	public struct CharacterRegion
	{
        /// <summary>
        /// Represents the first character in the region.
        /// </summary>
	    public char Start;
        /// <summary>
        /// Represents the last character in the region.
        /// </summary>
	    public char End;

		/// <summary>
		///  Enumerates all characters within the region.
		/// </summary>
	    public IEnumerable<Char> Characters()
	    {
	        for (var c = Start; c <= End; c++)
	        {
	            yield return c;
	        }
	    }

	    /// <summary>
	    /// Creates a new CharacterRegion instance.
	    /// </summary>
	    /// <param name="start">First <c>char</c> of the region.</param>
	    /// <param name="end">Last <c>char</c> of the region.</param>
	    /// <exception cref="ArgumentException">Thrown if the first character is after the last character.</exception>
        public CharacterRegion(char start, char end)
        {
            if (start > end)
                throw new ArgumentException();

            Start = start;
            End = end;
        }

		/// <summary>
		/// Default to just the base ASCII character set.
		/// </summary>
		public static CharacterRegion Default = new CharacterRegion(' ', '~');


		/// <summary>
		/// Test if there is an element in this enumeration.
		/// </summary>
		/// <typeparam name="T">Type of the element</typeparam>
		/// <param name="source">The enumerable source.</param>
		/// <returns><c>true</c> if there is an element in this enumeration, <c>false</c> otherwise</returns>
		public static bool Any<T>(IEnumerable<T> source)
		{
			return source.GetEnumerator().MoveNext();
		}


		/// <summary>
		/// Select elements from an enumeration.
		/// </summary>
		/// <typeparam name="TSource">The type of the T source.</typeparam>
		/// <typeparam name="TResult">The type of the T result.</typeparam>
		/// <param name="source">The source.</param>
		/// <param name="selector">The selector.</param>
		/// <returns>A enumeration of selected values</returns>
		public static IEnumerable<TResult> SelectMany<TSource, TResult>(IEnumerable<TSource> source, Func<TSource, IEnumerable<TResult>> selector)
		{
			foreach (TSource sourceItem in source)
			{
				foreach (TResult result in selector(sourceItem))
					yield return result;
			}
		}

		/// <summary>
		/// Selects distinct elements from an enumeration.
		/// </summary>
		/// <typeparam name="TSource">The type of the T source.</typeparam>
		/// <param name="source">The source.</param>
		/// <param name="comparer">The comparer.</param>
		/// <returns>A enumeration of selected values</returns>
		public static IEnumerable<TSource> Distinct<TSource>(IEnumerable<TSource> source, IEqualityComparer<TSource> comparer = null)
		{
			if (comparer == null)
				comparer = EqualityComparer<TSource>.Default;

			// using Dictionary is not really efficient but easy to implement
			var values = new Dictionary<TSource, object>(comparer);
			foreach (TSource sourceItem in source)
			{
				if (!values.ContainsKey(sourceItem))
				{
					values.Add(sourceItem, null);
					yield return sourceItem;
				}
			}
		}
	}
}
