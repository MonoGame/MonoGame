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


namespace Microsoft.Xna.Framework.Audio
{
	public class Cue : IDisposable
	{
		string name;
		XactSound[] sounds;
		float[] probs;
		XactSound curSound;
		Random variationRand;
		
		bool paused = false;
		float volume = 1.0f;
		
		public bool IsPaused
		{
			get { return paused; }
		}
		
		public bool IsPlaying
		{
			get {
				if (curSound != null) {
					return curSound.Playing;
				}
				return false;
			}
		}
		
		public bool IsStopped
		{
			get {
				if (curSound != null) {
					return !curSound.Playing;
				}
				return true;
			}
		}
		
		public string Name
		{
			get { return name; }
		}
		
		internal Cue(string cuename, XactSound sound)
		{
			name = cuename;
			sounds = new XactSound[1];
			sounds[0] = sound;
			
			probs = new float[1];
			probs[0] = 1.0f;
			
			variationRand = new Random();
		}
		
		internal Cue(string cuename, XactSound[] _sounds, float[] _probs)
		{
			name = cuename;
			sounds = _sounds;
			probs = _probs;
			
			variationRand = new Random();
		}
		
		public void Pause()
		{
			if (curSound != null) {
				curSound.Pause();
			}
			paused = true;
		}
		
		public void Play()
		{
			//TODO: Probabilities
			curSound = sounds[variationRand.Next (sounds.Length)];
			
			curSound.Volume = volume;
			curSound.Play ();
			paused = false;
		}
		
		public void Resume()
		{
			if (curSound != null) {
				curSound.Resume ();
			}
			paused = false;
		}
		
		public void Stop(AudioStopOptions options)
		{
			if (curSound != null) {
				curSound.Stop();
			}
			paused = false;
		}
		
		public void SetVariable(string name, float value)
		{
			if (name == "Volume") {
				volume = value;
				if (curSound != null) {
					curSound.Volume = value;
				}
			} else {
				throw new NotImplementedException();
			}
		}
		
		public float GetVariable(string name, float value)
		{
			if (name == "Volume") {
				return volume;
			} else {
				throw new NotImplementedException();
			}
		}
		
		public void Apply3D(AudioListener listener, AudioEmitter emitter) {
			
		}
		
		public bool IsDisposed { get { return false; } }
		
		
		
		#region IDisposable implementation
		public void Dispose ()
		{
			//_sound.Dispose();
		}
		#endregion
	}
}

