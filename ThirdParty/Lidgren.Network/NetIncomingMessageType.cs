/* Copyright (c) 2010 Michael Lidgren

Permission is hereby granted, free of charge, to any person obtaining a copy of this software
and associated documentation files (the "Software"), to deal in the Software without
restriction, including without limitation the rights to use, copy, modify, merge, publish,
distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom
the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or
substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE
USE OR OTHER DEALINGS IN THE SOFTWARE.

*/
using System;
using System.Diagnostics.CodeAnalysis;

namespace Lidgren.Network
{
	/// <summary>
	/// The type of a NetIncomingMessage
	/// </summary>
	[SuppressMessage("Microsoft.Design", "CA1027:MarkEnumsWithFlags")]
	public enum NetIncomingMessageType
	{
		//
		// library note: values are power-of-two, but they are not flags - it's a convenience for NetPeerConfiguration.DisabledMessageTypes
		//

		/// <summary>
		/// Error; this value should never appear
		/// </summary>
		Error = 0,

		/// <summary>
		/// Status for a connection changed
		/// </summary>
		StatusChanged = 1 << 0,			// Data (string)

		/// <summary>
		/// Data sent using SendUnconnectedMessage
		/// </summary>
		UnconnectedData = 1 << 1,		// Data					Based on data received

		/// <summary>
		/// Connection approval is needed
		/// </summary>
		ConnectionApproval = 1 << 2,	// Data

		/// <summary>
		/// Application data
		/// </summary>
		Data = 1 << 3,					// Data					Based on data received

		/// <summary>
		/// Receipt of delivery
		/// </summary>
		Receipt = 1 << 4,				// Data

		/// <summary>
		/// Discovery request for a response
		/// </summary>
		DiscoveryRequest = 1 << 5,		// (no data)

		/// <summary>
		/// Discovery response to a request
		/// </summary>
		DiscoveryResponse = 1 << 6,		// Data

		/// <summary>
		/// Verbose debug message
		/// </summary>
		VerboseDebugMessage = 1 << 7,	// Data (string)

		/// <summary>
		/// Debug message
		/// </summary>
		DebugMessage = 1 << 8,			// Data (string)

		/// <summary>
		/// Warning message
		/// </summary>
		WarningMessage = 1 << 9,		// Data (string)

		/// <summary>
		/// Error message
		/// </summary>
		ErrorMessage = 1 << 10,			// Data (string)

		/// <summary>
		/// NAT introduction was successful
		/// </summary>
		NatIntroductionSuccess = 1 << 11, // Data (as passed to master server)

		/// <summary>
		/// A roundtrip was measured and NetConnection.AverageRoundtripTime was updated
		/// </summary>
		ConnectionLatencyUpdated = 1 << 12, // Seconds as a Single
	}
}
