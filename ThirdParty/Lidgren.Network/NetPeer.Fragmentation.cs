using System;
using System.Threading;
using System.Collections.Generic;

namespace Lidgren.Network
{
	internal class ReceivedFragmentGroup
	{
		//public float LastReceived;
		public byte[] Data;
		public NetBitVector ReceivedChunks;
	}

	public partial class NetPeer
	{
		private int m_lastUsedFragmentGroup;

		private Dictionary<NetConnection, Dictionary<int, ReceivedFragmentGroup>> m_receivedFragmentGroups;

		// on user thread
		private NetSendResult SendFragmentedMessage(NetOutgoingMessage msg, IList<NetConnection> recipients, NetDeliveryMethod method, int sequenceChannel)
		{
			// Note: this group id is PER SENDING/NetPeer; ie. same id is sent to all recipients;
			// this should be ok however; as long as recipients differentiate between same id but different sender
			int group = Interlocked.Increment(ref m_lastUsedFragmentGroup);
			if (group >= NetConstants.MaxFragmentationGroups)
			{
				// @TODO: not thread safe; but in practice probably not an issue
				m_lastUsedFragmentGroup = 1;
				group = 1;
			}
			msg.m_fragmentGroup = group;

			// do not send msg; but set fragmentgroup in case user tries to recycle it immediately

			// create fragmentation specifics
			int totalBytes = msg.LengthBytes;

			// determine minimum mtu for all recipients
			int mtu = GetMTU(recipients);
			int bytesPerChunk = NetFragmentationHelper.GetBestChunkSize(group, totalBytes, mtu);

			int numChunks = totalBytes / bytesPerChunk;
			if (numChunks * bytesPerChunk < totalBytes)
				numChunks++;

			NetSendResult retval = NetSendResult.Sent;

			int bitsPerChunk = bytesPerChunk * 8;
			int bitsLeft = msg.LengthBits;
			for (int i = 0; i < numChunks; i++)
			{
				NetOutgoingMessage chunk = CreateMessage(0);

				chunk.m_bitLength = (bitsLeft > bitsPerChunk ? bitsPerChunk : bitsLeft);
				chunk.m_data = msg.m_data;
				chunk.m_fragmentGroup = group;
				chunk.m_fragmentGroupTotalBits = totalBytes * 8;
				chunk.m_fragmentChunkByteSize = bytesPerChunk;
				chunk.m_fragmentChunkNumber = i;

				NetException.Assert(chunk.m_bitLength != 0);
				NetException.Assert(chunk.GetEncodedSize() < mtu);

				Interlocked.Add(ref chunk.m_recyclingCount, recipients.Count);

				foreach (NetConnection recipient in recipients)
				{
					var res = recipient.EnqueueMessage(chunk, method, sequenceChannel);
					if (res == NetSendResult.Dropped)
						Interlocked.Decrement(ref chunk.m_recyclingCount);
					if ((int)res > (int)retval)
						retval = res; // return "worst" result
				}

				bitsLeft -= bitsPerChunk;
			}

			return retval;
		}

		private void HandleReleasedFragment(NetIncomingMessage im)
		{
			VerifyNetworkThread();

			//
			// read fragmentation header and combine fragments
			//
			int group;
			int totalBits;
			int chunkByteSize;
			int chunkNumber;
			int ptr = NetFragmentationHelper.ReadHeader(
				im.m_data, 0,
				out group,
				out totalBits,
				out chunkByteSize,
				out chunkNumber
			);

			NetException.Assert(im.LengthBytes > ptr);

			NetException.Assert(group > 0);
			NetException.Assert(totalBits > 0);
			NetException.Assert(chunkByteSize > 0);
			
			int totalBytes = NetUtility.BytesToHoldBits((int)totalBits);
			int totalNumChunks = totalBytes / chunkByteSize;
			if (totalNumChunks * chunkByteSize < totalBytes)
				totalNumChunks++;

			NetException.Assert(chunkNumber < totalNumChunks);

			if (chunkNumber >= totalNumChunks)
			{
				LogWarning("Index out of bounds for chunk " + chunkNumber + " (total chunks " + totalNumChunks + ")");
				return;
			}

			Dictionary<int, ReceivedFragmentGroup> groups;
			if (!m_receivedFragmentGroups.TryGetValue(im.SenderConnection, out groups))
			{
				groups = new Dictionary<int, ReceivedFragmentGroup>();
				m_receivedFragmentGroups[im.SenderConnection] = groups;
			}

			ReceivedFragmentGroup info;
			if (!groups.TryGetValue(group, out info))
			{
				info = new ReceivedFragmentGroup();
				info.Data = new byte[totalBytes];
				info.ReceivedChunks = new NetBitVector(totalNumChunks);
				groups[group] = info;
			}

			info.ReceivedChunks[chunkNumber] = true;
			//info.LastReceived = (float)NetTime.Now;

			// copy to data
			int offset = (chunkNumber * chunkByteSize);
			Buffer.BlockCopy(im.m_data, ptr, info.Data, offset, im.LengthBytes - ptr);

			int cnt = info.ReceivedChunks.Count();
			//LogVerbose("Found fragment #" + chunkNumber + " in group " + group + " offset " + offset + " of total bits " + totalBits + " (total chunks done " + cnt + ")");

			LogVerbose("Received fragment " + chunkNumber + " of " + totalNumChunks + " (" + cnt + " chunks received)");

			if (info.ReceivedChunks.Count() == totalNumChunks)
			{
				// Done! Transform this incoming message
				im.m_data = info.Data;
				im.m_bitLength = (int)totalBits;
				im.m_isFragment = false;

				LogVerbose("Fragment group #" + group + " fully received in " + totalNumChunks + " chunks (" + totalBits + " bits)");
				groups.Remove(group);

				ReleaseMessage(im);
			}
			else
			{
				// data has been copied; recycle this incoming message
				Recycle(im);
			}

			return;
		}
	}
}
