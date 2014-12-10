 /* Copyright (c) 2010 Michael Lidgren

Permission is hereby granted, free of charge, to any person obtaining a copy of this software
and associated documentation files (the "Software"), to deal in the Software without
restriction, including without limitation the rights to use, copy, modify, merge, publish,
distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom
the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or
substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE
USE OR OTHER DEALINGS IN THE SOFTWARE.
*/
using System;
using System.Reflection;

namespace Lidgren.Network
{
	public partial class NetBuffer
	{
		/// <summary>
		/// Reads all public and private declared instance fields of the object in alphabetical order using reflection
		/// </summary>
		public void ReadAllFields(object target)
		{
			ReadAllFields(target, BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
		}

		/// <summary>
		/// Reads all fields with the specified binding of the object in alphabetical order using reflection
		/// </summary>
		public void ReadAllFields(object target, BindingFlags flags)
		{
			if (target == null)
				throw new ArgumentNullException("target");

			Type tp = target.GetType();

			FieldInfo[] fields = tp.GetFields(flags);
			NetUtility.SortMembersList(fields);

			foreach (FieldInfo fi in fields)
			{
				object value;

				// find read method
				MethodInfo readMethod;
				if (s_readMethods.TryGetValue(fi.FieldType, out readMethod))
				{
					// read value
					value = readMethod.Invoke(this, null);

					// set the value
					fi.SetValue(target, value);
				}
			}
		}

		/// <summary>
		/// Reads all public and private declared instance fields of the object in alphabetical order using reflection
		/// </summary>
		public void ReadAllProperties(object target)
		{
			ReadAllProperties(target, BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
		}

		/// <summary>
		/// Reads all fields with the specified binding of the object in alphabetical order using reflection
		/// </summary>
		public void ReadAllProperties(object target, BindingFlags flags)
		{
			if (target == null)
				throw new ArgumentNullException("target");

			Type tp = target.GetType();

			PropertyInfo[] fields = tp.GetProperties(flags);
			NetUtility.SortMembersList(fields);
			foreach (PropertyInfo fi in fields)
			{
				object value;

				// find read method
				MethodInfo readMethod;
				if (s_readMethods.TryGetValue(fi.PropertyType, out readMethod))
				{
					// read value
					value = readMethod.Invoke(this, null);

					// set the value
#if UNITY_WEBPLAYER || UNITY_4_5
					var setMethod = fi.GetSetMethod();
#else
					var setMethod = fi.SetMethod;
#endif
					if (setMethod != null)
						setMethod.Invoke(target, new object[] { value });
				}
			}
		}
	}
}