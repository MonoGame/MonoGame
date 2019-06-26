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
//#define USE_RELEASE_STATISTICS

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;

namespace Lidgren.Network
{
	public partial class NetPeer
	{

#if DEBUG
		private readonly List<DelayedPacket> m_delayedPackets = new List<DelayedPacket>();

		private class DelayedPacket
		{
			public byte[] Data;
			public double DelayedUntil;
			public IPEndPoint Target;
		}

		internal void SendPacket(int numBytes, IPEndPoint target, int numMessages, out bool connectionReset)
		{
			connectionReset = false;

			// simulate loss
			float loss = m_configuration.m_loss;
			if (loss > 0.0f)
			{
				if ((float)MWCRandom.Instance.NextDouble() < loss)
				{
					LogVerbose("Sending packet " + numBytes + " bytes - SIMULATED LOST!");
					return; // packet "lost"
				}
			}

			m_statistics.PacketSent(numBytes, numMessages);

			// simulate latency
			float m = m_configuration.m_minimumOneWayLatency;
			float r = m_configuration.m_randomOneWayLatency;
			if (m == 0.0f && r == 0.0f)
			{
				// no latency simulation
				// LogVerbose("Sending packet " + numBytes + " bytes");
				bool wasSent = ActuallySendPacket(m_sendBuffer, numBytes, target, out connectionReset);
				// TODO: handle wasSent == false?

				if (m_configuration.m_duplicates > 0.0f && MWCRandom.Instance.NextDouble() < m_configuration.m_duplicates)
					ActuallySendPacket(m_sendBuffer, numBytes, target, out connectionReset); // send it again!

				return;
			}

			int num = 1;
			if (m_configuration.m_duplicates > 0.0f && MWCRandom.Instance.NextSingle() < m_configuration.m_duplicates)
				num++;

			float delay = 0;
			for (int i = 0; i < num; i++)
			{
				delay = m_configuration.m_minimumOneWayLatency + (MWCRandom.Instance.NextSingle() * m_configuration.m_randomOneWayLatency);

				// Enqueue delayed packet
				DelayedPacket p = new DelayedPacket();
				p.Target = target;
				p.Data = new byte[numBytes];
				Buffer.BlockCopy(m_sendBuffer, 0, p.Data, 0, numBytes);
				p.DelayedUntil = NetTime.Now + delay;

				m_delayedPackets.Add(p);
			}

			// LogVerbose("Sending packet " + numBytes + " bytes - delayed " + NetTime.ToReadable(delay));
		}

		private void SendDelayedPackets()
		{
			if (m_delayedPackets.Count <= 0)
				return;

			double now = NetTime.Now;

			bool connectionReset;

		RestartDelaySending:
			foreach (DelayedPacket p in m_delayedPackets)
			{
				if (now > p.DelayedUntil)
				{
					ActuallySendPacket(p.Data, p.Data.Length, p.Target, out connectionReset);
					m_delayedPackets.Remove(p);
					goto RestartDelaySending;
				}
			}
		}

		private void FlushDelayedPackets()
		{
			try
			{
				bool connectionReset;
				foreach (DelayedPacket p in m_delayedPackets)
					ActuallySendPacket(p.Data, p.Data.Length, p.Target, out connectionReset);
				m_delayedPackets.Clear();
			}
			catch { }
		}

		internal bool ActuallySendPacket(byte[] data, int numBytes, IPEndPoint target, out bool connectionReset)
		{
			connectionReset = false;
			try
			{
				// TODO: refactor this check outta here
				if (target.Address == IPAddress.Broadcast)
				{
					// Some networks do not allow 
					// a global broadcast so we use the BroadcastAddress from the configuration
					// this can be resolved to a local broadcast addresss e.g 192.168.x.255                    
					target.Address = m_configuration.BroadcastAddress;
					m_socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, true);
				}

				int bytesSent = m_socket.SendTo(data, 0, numBytes, SocketFlags.None, target);
				if (numBytes != bytesSent)
					LogWarning("Failed to send the full " + numBytes + "; only " + bytesSent + " bytes sent in packet!");

				// LogDebug("Sent " + numBytes + " bytes");
			}
			catch (SocketException sx)
			{
				if (sx.SocketErrorCode == SocketError.WouldBlock)
				{
					// send buffer full?
					LogWarning("Socket threw exception; would block - send buffer full? Increase in NetPeerConfiguration");
					return false;
				}
				if (sx.SocketErrorCode == SocketError.ConnectionReset)
				{
					// connection reset by peer, aka connection forcibly closed aka "ICMP port unreachable" 
					connectionReset = true;
					return false;
				}
				LogError("Failed to send packet: " + sx);
			}
			catch (Exception ex)
			{
				LogError("Failed to send packet: " + ex);
			}
			finally
			{
				if (target.Address == IPAddress.Broadcast)
					m_socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, false);
			}
			return true;
		}

		internal bool SendMTUPacket(int numBytes, IPEndPoint target)
		{
			try
			{
				m_socket.DontFragment = true;
				int bytesSent = m_socket.SendTo(m_sendBuffer, 0, numBytes, SocketFlags.None, target);
				if (numBytes != bytesSent)
					LogWarning("Failed to send the full " + numBytes + "; only " + bytesSent + " bytes sent in packet!");

				m_statistics.PacketSent(numBytes, 1);
			}
			catch (SocketException sx)
			{
				if (sx.SocketErrorCode == SocketError.MessageSize)
					return false;
				if (sx.SocketErrorCode == SocketError.WouldBlock)
				{
					// send buffer full?
					LogWarning("Socket threw exception; would block - send buffer full? Increase in NetPeerConfiguration");
					return true;
				}
				if (sx.SocketErrorCode == SocketError.ConnectionReset)
					return true;
				LogError("Failed to send packet: (" + sx.SocketErrorCode + ") " + sx);
			}
			catch (Exception ex)
			{
				LogError("Failed to send packet: " + ex);
			}
			finally
			{
				m_socket.DontFragment = false;
			}
			return true;
		}
#else
		internal bool SendMTUPacket(int numBytes, IPEndPoint target)
		{
			try
			{
				m_socket.DontFragment = true;
				int bytesSent = m_socket.SendTo(m_sendBuffer, 0, numBytes, SocketFlags.None, target);
				if (numBytes != bytesSent)
					LogWarning("Failed to send the full " + numBytes + "; only " + bytesSent + " bytes sent in packet!");
			}
			catch (SocketException sx)
			{
				if (sx.SocketErrorCode == SocketError.MessageSize)
					return false;
				if (sx.SocketErrorCode == SocketError.WouldBlock)
				{
					// send buffer full?
					LogWarning("Socket threw exception; would block - send buffer full? Increase in NetPeerConfiguration");
					return true;
				}
				if (sx.SocketErrorCode == SocketError.ConnectionReset)
					return true;
				LogError("Failed to send packet: (" + sx.SocketErrorCode + ") " + sx);
			}
			catch (Exception ex)
			{
				LogError("Failed to send packet: " + ex);
			}
			finally
			{
				m_socket.DontFragment = false;
			}
			return true;
		}

		//
		// Release - just send the packet straight away
		//
		internal void SendPacket(int numBytes, IPEndPoint target, int numMessages, out bool connectionReset)
		{
#if USE_RELEASE_STATISTICS
			m_statistics.PacketSent(numBytes, numMessages);
#endif
			connectionReset = false;
			try
			{
				// TODO: refactor this check outta here
				if (target.Address == IPAddress.Broadcast)
					m_socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, true);

				int bytesSent = m_socket.SendTo(m_sendBuffer, 0, numBytes, SocketFlags.None, target);
				if (numBytes != bytesSent)
					LogWarning("Failed to send the full " + numBytes + "; only " + bytesSent + " bytes sent in packet!");
			}
			catch (SocketException sx)
			{
				if (sx.SocketErrorCode == SocketError.WouldBlock)
				{
					// send buffer full?
					LogWarning("Socket threw exception; would block - send buffer full? Increase in NetPeerConfiguration");
					return;
				}
				if (sx.SocketErrorCode == SocketError.ConnectionReset)
				{
					// connection reset by peer, aka connection forcibly closed aka "ICMP port unreachable" 
					connectionReset = true;
					return;
				}
				LogError("Failed to send packet: " + sx);
			}
			catch (Exception ex)
			{
				LogError("Failed to send packet: " + ex);
			}
			finally
			{
				if (target.Address == IPAddress.Broadcast)
					m_socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, false);
			}
			return;
		}

		private void FlushDelayedPackets()
		{
		}
#endif
	}
}