using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NUnit.Framework;
using NUnit.Framework.Internal;

namespace MonoGame.Tests {
	static class Extensions {
		public static void Add (
			this GameComponentCollection self,
			params IGameComponent [] components)
		{
			foreach (var component in components)
				self.Add (component);
		}

		public static Color ToColor (this string self)
		{
			return (Color)typeof (Color).InvokeMember (self, BindingFlags.GetProperty, null, null, null);
		}

	    public static FramePixelData ToPixelData(this Texture2D texture)
	    {
			var data = new Color [texture.Width * texture.Height];
			texture.GetData (data);
	        return new FramePixelData(texture.Width, texture.Height, data);
	    }

		private static readonly char[] ForbiddenFileNameChars = "{}()\"',:".ToCharArray();

		public static string ReplaceInvalidFileNameChars (this string self)
		{
			var pattern = "[" + Regex.Escape (
				new string(ForbiddenFileNameChars) +
				new string (Path.GetInvalidFileNameChars ())) + "]";
			return Regex.Replace (self, pattern, "_");
		}

		public static string ReplaceInvalidPathChars (this string self)
		{
			var pattern = "[" + Regex.Escape (
				new string(ForbiddenFileNameChars) +
				new string (Path.GetInvalidPathChars ())) + "]";
			return Regex.Replace (self, pattern, "_");
		}

		public static StackFrame FindTestEntryFrame (this StackTrace self)
		{
			foreach (var stackFrame in self.GetFrames ()) {
				var method = stackFrame.GetMethod();
				var testAttributes = method.GetCustomAttributes (typeof (TestAttribute), false);
				if (testAttributes.Length > 0)
					return stackFrame;
			}

			return null;
		}

		static ConcurrentDictionary<string, TestContext.TestAdapter> tests = new ConcurrentDictionary<string, TestContext.TestAdapter>();

        public static void RegisterTestWtihContext (TestContext context, TestContext.TestAdapter test)
        {
            tests.TryAdd(context.Test.ID, test);
        }

		public static void UnRegisterTestWtihContext (TestContext context)
        {
			tests.Clear ();
        }

		public static StackFrame GetTestEntryStackFrame (this StackTrace self)
		{
			var stackFrame = self.FindTestEntryFrame ();
			if (stackFrame == null)
				throw new InvalidOperationException (
					"No method marked with TestAttribute was found in this StackTrace");
			return stackFrame;
		}

		public static string GetTestFolderName (this TestContext self)
		{
			// TODO: Add support for a custom attribute to override
			//       the calculated name.
			//var method = self.GetMethod ();
			if (!tests.TryGetValue (self.Test.ID, out TestContext.TestAdapter test)) {
				test = self.Test;
			}

			var fullTypeName = test.FullName.Remove (
				test.FullName.Length - (test.Name.Length + 1));

			var tokens = fullTypeName.Split('.');
			var typeName = tokens [tokens.Length - 1];
			if (typeName.EndsWith ("Test"))
				typeName = typeName.Remove (typeName.Length - "Test".Length);
			else if (typeName.EndsWith ("Tests"))
				typeName = typeName.Remove (typeName.Length - "Tests".Length);

			return typeName.ReplaceInvalidFileNameChars ();
		}

		public static string GetTestFrameFileNameFormat (this TestContext self, int maxFrameNumber)
		{
			// TODO: Add support for a custom attribute to override
			//       the calculated name.
			if (!tests.TryGetValue (self.Test.ID, out TestContext.TestAdapter test)) {
				test = self.Test;
			}

			if (maxFrameNumber == 0)
				return test.Name.ReplaceInvalidFileNameChars() + "-{0}.png";
			if (maxFrameNumber == 1)
				return test.Name.ReplaceInvalidFileNameChars() + ".png";

			int numDigits = 1 + (int)Math.Log10 (maxFrameNumber + 1);

			var builder = new StringBuilder (test.Name.ReplaceInvalidFileNameChars ());
			builder.Append ("-{0:");
			builder.Append ('0', numDigits);
			builder.Append ("}.png");
			return builder.ToString ();
		}

		public static void AddService<T> (this GameServiceContainer serviceProvider, T service)
		{
			serviceProvider.AddService (typeof (T), service);
		}

		public static T RequireService<T> (this IServiceProvider serviceProvider)
		{
			var service = (T) serviceProvider.GetService (typeof (T));
			if (service == null)
				throw new ServiceNotFoundException (typeof (T));
			return service;
		}
	}

	class ServiceNotFoundException : Exception {
		public ServiceNotFoundException (Type serviceType)
			: base(string.Format("Required service of type '{0}' was not found.", serviceType))
		{
			if (serviceType == null)
				throw new ArgumentNullException ("serviceType");
			ServiceType = serviceType;

		}

		public Type ServiceType { get; private set; }
	}
}
