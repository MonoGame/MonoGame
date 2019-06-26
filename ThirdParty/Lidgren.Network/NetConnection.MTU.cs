using System;

namespace Lidgren.Network
{
	public partial class NetConnection  
	{
		private enum ExpandMTUStatus
		{
			None,
			InProgress,
			Finished
		}

		private const int c_protocolMaxMTU = (int)((((float)ushort.MaxValue / 8.0f) - 1.0f));

		private ExpandMTUStatus m_expandMTUStatus;

		private int m_largestSuccessfulMTU;
		private int m_smallestFailedMTU;

		private int m_lastSentMTUAttemptSize;
		private double m_lastSentMTUAttemptTime;
		private int m_mtuAttemptFails;

		internal int m_currentMTU;

		/// <summary>
		/// Gets the current MTU in bytes. If PeerConfiguration.AutoExpandMTU is false, this will be PeerConfiguration.MaximumTransmissionUnit.
		/// </summary>
		public int CurrentMTU { get { return m_currentMTU; } }

		internal void InitExpandMTU(double now)
		{
			m_lastSentMTUAttemptTime = now + m_peerConfiguration.m_expandMTUFrequency + 1.5f + m_averageRoundtripTime; // wait a tiny bit before starting to expand mtu
			m_largestSuccessfulMTU = 512;
			m_smallestFailedMTU = -1;
			m_currentMTU = m_peerConfiguration.MaximumTransmissionUnit;
		}

		private void MTUExpansionHeartbeat(double now)
		{
			if (m_expandMTUStatus == ExpandMTUStatus.Finished)
				return;

			if (m_expandMTUStatus == ExpandMTUStatus.None)
			{
				if (m_peerConfiguration.m_autoExpandMTU == false)
				{
					FinalizeMTU(m_currentMTU);
					return;
				}

				// begin expansion
				ExpandMTU(now);
				return;
			}

			if (now > m_lastSentMTUAttemptTime + m_peerConfiguration.ExpandMTUFrequency)
			{
				m_mtuAttemptFails++;
				if (m_mtuAttemptFails == 3)
				{
					FinalizeMTU(m_currentMTU);
					return;
				}

				// timed out; ie. failed
				m_smallestFailedMTU = m_lastSentMTUAttemptSize;
				ExpandMTU(now);
			}
		}

		private void ExpandMTU(double now)
		{
			int tryMTU;

			// we've nevered encountered failure
			if (m_smallestFailedMTU == -1)
			{
				// we've never encountered failure; expand by 25% each time
				tryMTU = (int)((float)m_currentMTU * 1.25f);
				//m_peer.LogDebug("Trying MTU " + tryMTU);
			}
			else
			{
				// we HAVE encountered failure; so try in between
				tryMTU = (int)(((float)m_smallestFailedMTU + (float)m_largestSuccessfulMTU) / 2.0f);
				//m_peer.LogDebug("Trying MTU " + m_smallestFailedMTU + " <-> " + m_largestSuccessfulMTU + " = " + tryMTU);
			}

			if (tryMTU > c_protocolMaxMTU)
				tryMTU = c_protocolMaxMTU;

			if (tryMTU == m_largestSuccessfulMTU)
			{
				//m_peer.LogDebug("Found optimal MTU - exiting");
				FinalizeMTU(m_largestSuccessfulMTU);
				return;
			}

			SendExpandMTU(now, tryMTU);
		}

		private void SendExpandMTU(double now, int size)
		{
			NetOutgoingMessage om = m_peer.CreateMessage(size);
			byte[] tmp = new byte[size];
			om.Write(tmp);
			om.m_messageType = NetMessageType.ExpandMTURequest;
			int len = om.Encode(m_peer.m_sendBuffer, 0, 0);

			bool ok = m_peer.SendMTUPacket(len, m_remoteEndPoint);
			if (ok == false)
			{
				//m_peer.LogDebug("Send MTU failed for size " + size);

				// failure
				if (m_smallestFailedMTU == -1 || size < m_smallestFailedMTU)
				{
					m_smallestFailedMTU = size;
					m_mtuAttemptFails++;
					if (m_mtuAttemptFails >= m_peerConfiguration.ExpandMTUFailAttempts)
					{
						FinalizeMTU(m_largestSuccessfulMTU);
						return;
					}
				}
				ExpandMTU(now);
				return;
			}

			m_lastSentMTUAttemptSize = size;
			m_lastSentMTUAttemptTime = now;

			m_statistics.PacketSent(len, 1);
			m_peer.Recycle(om);
		}

		private void FinalizeMTU(int size)
		{
			if (m_expandMTUStatus == ExpandMTUStatus.Finished)
				return;
			m_expandMTUStatus = ExpandMTUStatus.Finished;
			m_currentMTU = size;
			if (m_currentMTU != m_peerConfiguration.m_maximumTransmissionUnit)
				m_peer.LogDebug("Expanded Maximum Transmission Unit to: " + m_currentMTU + " bytes");
			return;
		}

		private void SendMTUSuccess(int size)
		{
			NetOutgoingMessage om = m_peer.CreateMessage(4);
			om.Write(size);
			om.m_messageType = NetMessageType.ExpandMTUSuccess;
			int len = om.Encode(m_peer.m_sendBuffer, 0, 0);
			bool connectionReset;
			m_peer.SendPacket(len, m_remoteEndPoint, 1, out connectionReset);
			m_peer.Recycle(om);

			//m_peer.LogDebug("Received MTU expand request for " + size + " bytes");

			m_statistics.PacketSent(len, 1);
		}

		private void HandleExpandMTUSuccess(double now, int size)
		{
			if (size > m_largestSuccessfulMTU)
				m_largestSuccessfulMTU = size;

			if (size < m_currentMTU)
			{
				//m_peer.LogDebug("Received low MTU expand success (size " + size + "); current mtu is " + m_currentMTU);
				return;
			}

			//m_peer.LogDebug("Expanding MTU to " + size);
			m_currentMTU = size;

			ExpandMTU(now);
		}
	}
}
