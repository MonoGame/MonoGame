// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;

using Microsoft.Xna.Framework;

namespace Microsoft.Xna.Framework.Audio
{
    /// <summary>
    /// Class used to create and manipulate code audio objects.
    /// </summary> 
	public class AudioEngine : IDisposable
	{
        ///<summary>
        ///Specifies the current content version.
        ///</summary>
		public const int ContentVersion = 46;
		
		internal Dictionary<string, WaveBank> Wavebanks = new Dictionary<string, WaveBank>();

		AudioCategory[] categories;
		Dictionary<string, int> categoryLookup = new Dictionary<string, int>();

        internal List<Cue> _activeCues = new List<Cue>();

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
        private Stopwatch _stopwatch;
        private TimeSpan _lastUpdateTime;



        /// <param name="settingsFile">Path to a XACT settings file.</param>
		public AudioEngine (string settingsFile)
			: this(settingsFile, TimeSpan.Zero, "")
		{            
		}

        /// <param name="settingsFile">Path to a XACT settings file.</param>
        /// <param name="lookAheadTime">Determines how many milliseconds the engine will look ahead when determing when to transition to another sound.</param>
        /// <param name="rendererId">A string that specifies the audio renderer to use.</param>
        /// <remarks>For the best results, use a lookAheadTime of 250 milliseconds or greater.</remarks>
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

                    reader.ReadUInt16 (); // toolVersion
#if DEBUG
                    uint formatVersion = reader.ReadUInt16 ();
					if (formatVersion != 42) {
						System.Diagnostics.Debug.WriteLine ("Warning: XGS format not supported");
					}
#else
                    reader.ReadUInt16 (); // formatVersion
#endif
                    reader.ReadUInt16 (); // crc

                    reader.ReadUInt32 (); // lastModifiedLow
                    reader.ReadUInt32 (); // lastModifiedHigh

					reader.ReadByte (); //unkn, 0x03. Platform?

					uint numCats = reader.ReadUInt16 ();
					uint numVars = reader.ReadUInt16 ();

					reader.ReadUInt16 (); //unkn, 0x16
					reader.ReadUInt16 (); //unkn, 0x16

					uint numRpc = reader.ReadUInt16 ();
                    reader.ReadUInt16 (); // numDspPresets
                    reader.ReadUInt16 (); // numDspParams

					uint catsOffset = reader.ReadUInt32 ();
					uint varsOffset = reader.ReadUInt32 ();

					reader.ReadUInt32 (); //unknown, leads to a short with value of 1?
                    reader.ReadUInt32 (); // catNameIndexOffset
					reader.ReadUInt32 (); //unknown, two shorts of values 2 and 3?
                    reader.ReadUInt32 (); // varNameIndexOffset

					uint catNamesOffset = reader.ReadUInt32 ();
					uint varNamesOffset = reader.ReadUInt32 ();
					uint rpcOffset = reader.ReadUInt32 ();
                    reader.ReadUInt32 (); // dspPresetsOffset
                    reader.ReadUInt32 (); // dspParamsOffset
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

            _stopwatch = new Stopwatch();
            _stopwatch.Start();
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

        /// <summary>Performs periodic work required by the audio engine.</summary>
        /// <remarks>Must be called at least once per frame.</remarks>
		public void Update ()
        {
            var cur = _stopwatch.Elapsed;
            var elapsed = cur - _lastUpdateTime;
            _lastUpdateTime = cur;
            var dt = (float)elapsed.TotalSeconds;
            
            for (var x = 0; x < _activeCues.Count; )
            {
                var cue = _activeCues[x];

                cue.Update(dt);

                if (cue.IsStopped)
                {
                    _activeCues.Remove(cue);
                    continue;
                }

                x++;
            }
		}
		
        /// <summary>Returns an audio category by name.</summary>
        /// <param name="name">Friendly name of the category to get.</param>
        /// <returns>The AudioCategory with a matching name. Throws an exception if not found.</returns>
		public AudioCategory GetCategory (string name)
		{
			return categories [categoryLookup [name]];
		}

        /// <summary>Gets the value of a global variable.</summary>
        /// <param name="name">Friendly name of the variable.</param>
        /// <returns>float value of the queried variable.</returns>
        /// <remarks>A global variable has global scope. It can be accessed by all code within a project.</remarks>
		public float GetGlobalVariable(string name)
		{
			return variables[variableLookup[name]].value;
		}

        /// <summary>Sets the value of a global variable.</summary>
        /// <param name="name">Friendly name of the variable.</param>
        /// <param name="value">Value of the global variable.</param>
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

