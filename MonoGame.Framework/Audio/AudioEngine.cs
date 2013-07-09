#region License
/*
Microsoft Public License (Ms-PL)
MonoGame - Copyright © 2009 The MonoGame Team

All rights reserved.

This license governs use of the accompanying software. If you use the software, you accept this license. If you do not
accept the license, do not use the software.

1. Definitions
The terms "reproduce," "reproduction," "derivative works," and "distribution" have the same meaning here as under 
U.S. copyright law.

A "contribution" is the original software, or any additions or changes to the software.
A "contributor" is any person that distributes its contribution under this license.
"Licensed patents" are a contributor's patent claims that read directly on its contribution.

2. Grant of Rights
(A) Copyright Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, 
each contributor grants you a non-exclusive, worldwide, royalty-free copyright license to reproduce its contribution, prepare derivative works of its contribution, and distribute its contribution or any derivative works that you create.
(B) Patent Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, 
each contributor grants you a non-exclusive, worldwide, royalty-free license under its licensed patents to make, have made, use, sell, offer for sale, import, and/or otherwise dispose of its contribution in the software or derivative works of the contribution in the software.

3. Conditions and Limitations
(A) No Trademark License- This license does not grant you rights to use any contributors' name, logo, or trademarks.
(B) If you bring a patent claim against any contributor over patents that you claim are infringed by the software, 
your patent license from such contributor to the software ends automatically.
(C) If you distribute any portion of the software, you must retain all copyright, patent, trademark, and attribution 
notices that are present in the software.
(D) If you distribute any portion of the software in source code form, you may do so only under this license by including 
a complete copy of this license with your distribution. If you distribute any portion of the software in compiled or object 
code form, you may only do so under a license that complies with this license.
(E) The software is licensed "as-is." You bear the risk of using it. The contributors give no express warranties, guarantees
or conditions. You may have additional consumer rights under your local laws which this license cannot change. To the extent
permitted under your local laws, the contributors exclude the implied warranties of merchantability, fitness for a particular
purpose and non-infringement.
*/
#endregion License

using System;
using System.IO;
using System.Collections.Generic;

using Microsoft.Xna.Framework;

namespace Microsoft.Xna.Framework.Audio
{
	public class AudioEngine : IDisposable
	{
		public const int ContentVersion = 46;
		
		internal Dictionary<string, WaveBank> Wavebanks = new Dictionary<string, WaveBank>();

		AudioCategory[] categories;
		Dictionary<string, int> categoryLookup = new Dictionary<string, int>();

		internal AudioCategory[] Categories { get { return categories; } }

		struct Variable {
			public string name;
			public float value;

			public bool isGlobal;
			public bool isReadOnly;
			public bool isPublic;
			public bool isReserved;

			public float initValue;
			public float maxValue;
			public float minValue;
		}
		Variable[] variables;
		Dictionary<string, int> variableLookup = new Dictionary<string, int>();


		enum RpcPointType {
			Linear,
			Fast,
			Slow,
			SinCos
		}
		struct RpcPoint {
			public float x, y;
			public RpcPointType type;
		}

		enum RpcParameter {
			Volume,
			Pitch,
			ReverbSend,
			FilterFrequency,
			FilterQFactor
		}
		struct RpcCurve {
			public int variable;
			public RpcParameter parameter;
			public RpcPoint[] points;
		}

		RpcCurve[] rpcCurves;



		public AudioEngine (string settingsFile)
			: this(settingsFile, TimeSpan.Zero, "")
		{
		}
		
		public AudioEngine (string settingsFile, TimeSpan lookAheadTime, string rendererId)
		{
			//Read the xact settings file
			//Credits to alisci01 for initial format documentation
#if !ANDROID
			using (var stream = TitleContainer.OpenStream(settingsFile))
			{
#else
			using (var fileStream = Game.Activity.Assets.Open(settingsFile))
			{
				MemoryStream stream = new MemoryStream();
				fileStream.CopyTo(stream);
				stream.Position = 0;
#endif
				using (var reader = new BinaryReader(stream)) {
					uint magic = reader.ReadUInt32 ();
					if (magic != 0x46534758) { //'XGFS'
						throw new ArgumentException ("XGS format not recognized");
					}

					uint toolVersion = reader.ReadUInt16 ();
					uint formatVersion = reader.ReadUInt16 ();
#if DEBUG
					if (formatVersion != 42) {
						System.Diagnostics.Debug.WriteLine ("Warning: XGS format not supported");
					}
#endif
					uint crc = reader.ReadUInt16 (); //??

					uint lastModifiedLow = reader.ReadUInt32 ();
					uint lastModifiedHigh = reader.ReadUInt32 ();

					reader.ReadByte (); //unkn, 0x03. Platform?

					uint numCats = reader.ReadUInt16 ();
					uint numVars = reader.ReadUInt16 ();

					reader.ReadUInt16 (); //unkn, 0x16
					reader.ReadUInt16 (); //unkn, 0x16

					uint numRpc = reader.ReadUInt16 ();
					uint numDspPresets = reader.ReadUInt16 ();
					uint numDspParams = reader.ReadUInt16 ();

					uint catsOffset = reader.ReadUInt32 ();
					uint varsOffset = reader.ReadUInt32 ();

					reader.ReadUInt32 (); //unknown, leads to a short with value of 1?
					uint catNameIndexOffset = reader.ReadUInt32 ();
					reader.ReadUInt32 (); //unknown, two shorts of values 2 and 3?
					uint varNameIndexOffset = reader.ReadUInt32 ();

					uint catNamesOffset = reader.ReadUInt32 ();
					uint varNamesOffset = reader.ReadUInt32 ();
					uint rpcOffset = reader.ReadUInt32 ();
					uint dspPresetsOffset = reader.ReadUInt32 ();
					uint dspParamsOffset = reader.ReadUInt32 ();
					reader.BaseStream.Seek (catNamesOffset, SeekOrigin.Begin);
					string[] categoryNames = readNullTerminatedStrings (numCats, reader);

					categories = new AudioCategory[numCats];
					reader.BaseStream.Seek (catsOffset, SeekOrigin.Begin);
					for (int i=0; i<numCats; i++) {
						categories [i] = new AudioCategory (this, categoryNames [i], reader);
						categoryLookup.Add (categoryNames [i], i);
					}

					reader.BaseStream.Seek (varNamesOffset, SeekOrigin.Begin);
					string[] varNames = readNullTerminatedStrings (numVars, reader);

					variables = new Variable[numVars];
					reader.BaseStream.Seek (varsOffset, SeekOrigin.Begin);
					for (int i=0; i<numVars; i++) {
						variables [i].name = varNames [i];

						byte flags = reader.ReadByte ();
						variables [i].isPublic = (flags & 0x1) != 0;
						variables [i].isReadOnly = (flags & 0x2) != 0;
						variables [i].isGlobal = (flags & 0x4) == 0;
						variables [i].isReserved = (flags & 0x8) != 0;
						
						variables [i].initValue = reader.ReadSingle ();
						variables [i].minValue = reader.ReadSingle ();
						variables [i].maxValue = reader.ReadSingle ();

						variables [i].value = variables [i].initValue;

						variableLookup.Add (varNames [i], i);
					}

					rpcCurves = new RpcCurve[numRpc];
					reader.BaseStream.Seek (rpcOffset, SeekOrigin.Begin);
					for (int i=0; i<numRpc; i++) {
						rpcCurves [i].variable = reader.ReadUInt16 ();
						int pointCount = (int)reader.ReadByte ();
						rpcCurves [i].parameter = (RpcParameter)reader.ReadUInt16 ();

						rpcCurves [i].points = new RpcPoint[pointCount];
						for (int j=0; j<pointCount; j++) {
							rpcCurves [i].points [j].x = reader.ReadSingle ();
							rpcCurves [i].points [j].y = reader.ReadSingle ();
							rpcCurves [i].points [j].type = (RpcPointType)reader.ReadByte ();
						}
					}

				}
			}
		}

		//wtf C#
		private static string[] readNullTerminatedStrings (uint count, BinaryReader reader)
		{
			string[] ret = new string[count];
			for (int i=0; i<count; i++) {
				List<char> s = new List<char> ();
				while (reader.PeekChar() != 0) {
					s.Add (reader.ReadChar ()); 
				}
				reader.ReadChar ();
				ret [i] = new string (s.ToArray ());
			}
			return ret;
		}
		
		public void Update ()
		{
			// TODO throw new NotImplementedException ();
		}
		
		public AudioCategory GetCategory (string name)
		{
			return categories [categoryLookup [name]];
		}

		public float GetGlobalVariable(string name)
		{
			return variables[variableLookup[name]].value;
		}

		public void SetGlobalVariable (string name, float value)
		{
			variables [variableLookup [name]].value = value;
		}
		
		#region IDisposable implementation
		public void Dispose ()
		{
		}
		#endregion
	}
}

