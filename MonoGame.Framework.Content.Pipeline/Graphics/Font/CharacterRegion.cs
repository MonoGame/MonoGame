using System;
using System.ComponentModel;
using System.Collections.Generic;

namespace Microsoft.Xna.Framework.Content.Pipeline.Graphics
{
	// Describes a range of consecutive characters that should be included in the font.
	[TypeConverter(typeof(CharacterRegionTypeConverter))]
	public struct CharacterRegion
	{
	    public char Start;
	    public char End;

		// Enumerates all characters within the region.        
	    public IEnumerable<Char> Characters()
	    {
	        for (var c = Start; c <= End; c++)
	        {
	            yield return c;
	        }
	    }

	    // Constructor.
        public CharacterRegion(char start, char end)
        {
            if (start > end)
                throw new ArgumentException();

            Start = start;
            End = end;
        }
		
		// Default to just the base ASCII character set.
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