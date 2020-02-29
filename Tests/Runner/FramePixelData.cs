using System;
using Microsoft.Xna.Framework;

namespace MonoGame.Tests {
	partial class FramePixelData {
		public FramePixelData (int width, int height, Color[] data)
		{
			_width = width;
			_height = height;
			_data = data;
		}

		public FramePixelData (int width, int height)
			: this(width, height, new Color[width * height])
		{
		}

		private Color [] _data;
		public Color [] Data {
			get { return _data; }
		}

		private int _width;
		public int Width { get { return _width; } }

		private int _height;
		public int Height { get { return _height; } }
	}
}

