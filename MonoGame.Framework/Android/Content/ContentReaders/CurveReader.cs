using System;
using System.IO;
using System.Linq;

namespace Microsoft.Xna.Framework.Content
{
	internal class CurveReader : ContentTypeReader<Curve>
	{
		public static string Normalize(string FileName)
		{
			int index = FileName.LastIndexOf(Path.DirectorySeparatorChar);
			string path = string.Empty;
			string file = FileName;
			
			if (index >= 0)
			{
				file = FileName.Substring(index + 1, FileName.Length - index - 1);
				path = FileName.Substring(0, index);
			}
			string[] files = Game.contextInstance.Assets.List(path);
			
			if (Contains(file, files))
				return FileName;
			
			// Check the file extension
			if (!string.IsNullOrEmpty(Path.GetExtension(FileName)))
			{
				return null;
			}
			
			return Path.Combine(path, TryFindAnyCased(file, files, ".xnb"));
		}
		
		private static string TryFindAnyCased(string search, string[] arr, params string[] extensions)
		{
			return arr.FirstOrDefault(s => extensions.Any(ext => s.ToLower() == (search.ToLower() + ext)));
		}
		
		private static bool Contains(string search, string[] arr)
		{
			return arr.Any(s => s == search);
		}
		
		protected internal override Curve Read(ContentReader input, Curve existingInstance)
		{
			Curve curve = existingInstance;
			if (curve == null)
			{
				curve = new Curve();
			}         
			
			curve.PreLoop = (CurveLoopType)input.ReadInt32();
			curve.PostLoop = (CurveLoopType)input.ReadInt32();
			int num6 = input.ReadInt32();
			
			for (int i = 0; i < num6; i++)
			{
				float position = input.ReadSingle();
				float num4 = input.ReadSingle();
				float tangentIn = input.ReadSingle();
				float tangentOut = input.ReadSingle();
				CurveContinuity continuity = (CurveContinuity)input.ReadInt32();
				curve.Keys.Add(new CurveKey(position, num4, tangentIn, tangentOut, continuity));
			}		
			return curve;         
		}
	}
}

