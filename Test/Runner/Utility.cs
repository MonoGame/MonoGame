#region License
/*
Microsoft Public License (Ms-PL)
MonoGame - Copyright © 2009-2012 The MonoGame Team

All rights reserved.

This license governs use of the accompanying software. If you use the software,
you accept this license. If you do not accept the license, do not use the
software.

1. Definitions

The terms "reproduce," "reproduction," "derivative works," and "distribution"
have the same meaning here as under U.S. copyright law.

A "contribution" is the original software, or any additions or changes to the
software.

A "contributor" is any person that distributes its contribution under this
license.

"Licensed patents" are a contributor's patent claims that read directly on its
contribution.

2. Grant of Rights

(A) Copyright Grant- Subject to the terms of this license, including the
license conditions and limitations in section 3, each contributor grants you a
non-exclusive, worldwide, royalty-free copyright license to reproduce its
contribution, prepare derivative works of its contribution, and distribute its
contribution or any derivative works that you create.

(B) Patent Grant- Subject to the terms of this license, including the license
conditions and limitations in section 3, each contributor grants you a
non-exclusive, worldwide, royalty-free license under its licensed patents to
make, have made, use, sell, offer for sale, import, and/or otherwise dispose of
its contribution in the software or derivative works of the contribution in the
software.

3. Conditions and Limitations

(A) No Trademark License- This license does not grant you rights to use any
contributors' name, logo, or trademarks.

(B) If you bring a patent claim against any contributor over patents that you
claim are infringed by the software, your patent license from such contributor
to the software ends automatically.

(C) If you distribute any portion of the software, you must retain all
copyright, patent, trademark, and attribution notices that are present in the
software.

(D) If you distribute any portion of the software in source code form, you may
do so only under this license by including a complete copy of this license with
your distribution. If you distribute any portion of the software in compiled or
object code form, you may only do so under a license that complies with this
license.

(E) The software is licensed "as-is." You bear the risk of using it. The
contributors give no express warranties, guarantees or conditions. You may have
additional consumer rights under your local laws which this license cannot
change. To the extent permitted under your local laws, the contributors exclude
the implied warranties of merchantability, fitness for a particular purpose and
non-infringement.
*/
#endregion License

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.IO;

using Microsoft.Xna.Framework;

namespace MonoGame.Tests {
	static class MathUtility {
		public static void MinMax (int a, int b, out int min, out int max)
		{
			if (a > b) {
				min = b;
				max = a;
			} else {
				min = a;
				max = b;
			}
		}
	}

	static class Paths {

		private const string AssetFolder = "Assets";
		private static readonly string FontFolder = Path.Combine (AssetFolder, "Fonts");
		private static readonly string ReferenceImageFolder = Path.Combine (AssetFolder, "ReferenceImages");
		private static readonly string TextureFolder = Path.Combine (AssetFolder, "Textures");
		private static readonly string EffectFolder = Path.Combine (AssetFolder, "Effects");
		private static readonly string ModelFolder = Path.Combine (AssetFolder, "Models");
        private static readonly string XmlFolder = Path.Combine(AssetFolder, "Xml");
        private const string CapturedFrameFolder = "CapturedFrames";
		private const string CapturedFrameDiffFolder = "Diffs";

		public static string Asset (params string [] pathParts)
		{
			return Combine (AssetFolder, pathParts);
		}

		public static string Font (params string [] pathParts)
		{
			return Combine (FontFolder, pathParts);
		}

		public static string Texture (params string [] pathParths)
		{
			return Combine (TextureFolder, pathParths);
		}

		public static string Effect (params string [] pathParths)
		{
			return Combine (EffectFolder, pathParths);
		}

		public static string Model (params string [] pathParths)
		{
			return Combine (ModelFolder, pathParths);
		}

        public static string Xml(params string[] pathParths)
        {
            return Combine(XmlFolder, pathParths);
        }

		public static string ReferenceImage (params string [] pathParts)
		{
			return Combine (ReferenceImageFolder, pathParts);
		}

		public static string CapturedFrame (params string [] pathParts)
		{
			return Combine (CapturedFrameFolder, pathParts);
		}

		public static string CapturedFrameDiff (params string [] pathParts)
		{
			return Combine (CapturedFrameDiffFolder, pathParts);
		}


		private static string Combine (string head, string [] tail)
		{
			return Path.Combine (head, Path.Combine (tail));
		}

		public static void SetStandardWorkingDirectory()
		{
            var directory = AppDomain.CurrentDomain.BaseDirectory;
			Directory.SetCurrentDirectory(directory);
		}
	}

}
