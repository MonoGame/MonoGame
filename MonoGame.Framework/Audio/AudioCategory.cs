using System;

namespace Microsoft.Xna.Framework.Audio
{
	public struct AudioCategory : IEquatable<AudioCategory>
	{
		string name;
		AudioEngine engine;
		
		internal AudioCategory (AudioEngine audioengine, string name) {
			this.name = name;
			engine = audioengine;
		}
		
		public void SetVolume(float volume) {
			throw new NotImplementedException();
		}
		
		public string Name { get { return name; } }
		
		public bool Equals(AudioCategory other)
		{
			throw new NotImplementedException();
		}
		
	}
}

