#if IPHONE
using Microsoft.Xna.Framework;
#else
using Microsoft.Xna.Framework;
#endif

namespace ZuneLib
{
	/// <summary>
	/// A platform-neutral touch structure.
	/// </summary>
	public struct ZuneTouch
	{
		/// <summary>
		/// The ID of the touch.
		/// </summary>
		public int ID;

		/// <summary>
		/// The position of the touch.
		/// </summary>
		public Vector2 Position;
		
		/// <summary>
		/// The state of the touch.
		/// </summary>
		public ZuneTouchState State;

		/// <summary>
		/// The pressure of the touch.
		/// </summary>
		public float Pressure;
	}
}