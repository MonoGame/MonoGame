﻿/* Copyright (c) 2010 Michael Lidgren

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
using System.Net;
using System.Diagnostics;

namespace Lidgren.Network
{
	/// <summary>
	/// Incoming message either sent from a remote peer or generated within the library
	/// </summary>
	[DebuggerDisplay("Type={MessageType} LengthBits={LengthBits}")]
	public partial class NetIncomingMessage
	{
		internal byte[] m_data;
		internal int m_bitLength;
		internal NetIncomingMessageType m_incomingMessageType;
		internal IPEndPoint m_senderEndpoint;
		internal NetConnection m_senderConnection;
		internal int m_sequenceNumber;
		internal NetMessageType m_receivedMessageType;
		internal bool m_isFragment;
		internal double m_receiveTime;

		/// <summary>
		/// Gets the type of this incoming message
		/// </summary>
		public NetIncomingMessageType MessageType { get { return m_incomingMessageType; } }

		/// <summary>
		/// Gets the delivery method this message was sent with (if user data)
		/// </summary>
		public NetDeliveryMethod DeliveryMethod { get { return NetUtility.GetDeliveryMethod(m_receivedMessageType); } }

		/// <summary>
		/// Gets the sequence channel this message was sent with (if user data)
		/// </summary>
		public int SequenceChannel { get { return (int)m_receivedMessageType - (int)NetUtility.GetDeliveryMethod(m_receivedMessageType); } }

		/// <summary>
		/// IPEndPoint of sender, if any
		/// </summary>
		public IPEndPoint SenderEndpoint { get { return m_senderEndpoint; } }

		/// <summary>
		/// NetConnection of sender, if any
		/// </summary>
		public NetConnection SenderConnection { get { return m_senderConnection; } }

		/// <summary>
		/// What local time the message was received from the network
		/// </summary>
		public double ReceiveTime { get { return m_receiveTime; } }

		/// <summary>
		/// Gets the length of the message payload in bytes
		/// </summary>
		public int LengthBytes
		{
			get { return ((m_bitLength + 7) >> 3); }
		}

		/// <summary>
		/// Gets the length of the message payload in bits
		/// </summary>
		public int LengthBits
		{
			get { return m_bitLength; }
			internal set { m_bitLength = value; }
		}

		internal NetIncomingMessage()
		{
		}

		internal NetIncomingMessage(NetIncomingMessageType tp)
		{
			m_incomingMessageType = tp;
		}

		internal void Reset()
		{
			m_incomingMessageType = NetIncomingMessageType.Error;
			m_readPosition = 0;
			m_receivedMessageType = NetMessageType.LibraryError;
			m_senderConnection = null;
			m_bitLength = 0;
			m_isFragment = false;
		}

		/// <summary>
		/// Decrypt a message
		/// </summary>
		/// <param name="encryption">The encryption algorithm used to encrypt the message</param>
		/// <returns>true on success</returns>
		public bool Decrypt(INetEncryption encryption)
		{
			return encryption.Decrypt(this);
		}

		/// <summary>
		/// Returns a string that represents this object
		/// </summary>
		public override string ToString()
		{
			return "[NetIncomingMessage #" + m_sequenceNumber + " " + this.LengthBytes + " bytes]";
		}
	}
}
