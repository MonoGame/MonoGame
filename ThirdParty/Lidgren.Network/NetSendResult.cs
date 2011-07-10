using System;

namespace Lidgren.Network
{
	/// <summary>
	/// Result of a SendMessage call
	/// </summary>
	public enum NetSendResult
	{
		/// <summary>
		/// Message failed to enqueue; for example if there's no connection in place
		/// </summary>
		Failed = 0,

		/// <summary>
		/// Message was immediately sent
		/// </summary>
		Sent = 1,

		/// <summary>
		/// Message was queued for delivery
		/// </summary>
		Queued = 2,

		/// <summary>
		/// Message was dropped immediately since too many message were queued
		/// </summary>
		Dropped = 3
	}
}
