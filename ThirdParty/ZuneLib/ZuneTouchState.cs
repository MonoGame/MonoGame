namespace ZuneLib
{
	/// <summary>
	/// Defines the state of a ZuneTouch.
	/// </summary>
	public enum ZuneTouchState
	{
		/// <summary>
		/// The touch is invalid.
		/// </summary>
		Invalid,

		/// <summary>
		/// The touch was just released.
		/// </summary>
		Released,

		/// <summary>
		/// The touch was just pressed.
		/// </summary>
		Pressed,

		/// <summary>
		/// The touch is down, but was also down last time Update was called.
		/// </summary>
		Moved
	}
}