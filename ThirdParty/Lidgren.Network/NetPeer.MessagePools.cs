using System;
using System.Collections.Generic;
using System.Text;

namespace Lidgren.Network
{
	public partial class NetPeer
	{
		private List<byte[]> m_storagePool; // sorted smallest to largest
		private NetQueue<NetOutgoingMessage> m_outgoingMessagesPool;
		private NetQueue<NetIncomingMessage> m_incomingMessagesPool;

		internal int m_storagePoolBytes;

		private void InitializePools()
		{
			if (m_configuration.UseMessageRecycling)
			{
				m_storagePool = new List<byte[]>(16);
				m_outgoingMessagesPool = new NetQueue<NetOutgoingMessage>(4);
				m_incomingMessagesPool = new NetQueue<NetIncomingMessage>(4);
			}
			else
			{
				m_storagePool = null;
				m_outgoingMessagesPool = null;
				m_incomingMessagesPool = null;
			}
		}

		internal byte[] GetStorage(int minimumCapacityInBytes)
		{
			if (m_storagePool == null)
				return new byte[minimumCapacityInBytes];

			lock (m_storagePool)
			{
				for (int i = 0; i < m_storagePool.Count; i++)
				{
					byte[] retval = m_storagePool[i];
					if (retval != null && retval.Length >= minimumCapacityInBytes)
					{
						m_storagePool[i] = null;
						m_storagePoolBytes -= retval.Length;
						return retval;
					}
				}
			}
			m_statistics.m_bytesAllocated += minimumCapacityInBytes;
			return new byte[minimumCapacityInBytes];
		}

		internal void Recycle(byte[] storage)
		{
			if (m_storagePool == null || storage == null)
				return;

			lock (m_storagePool)
			{
				m_storagePoolBytes += storage.Length;
				int cnt = m_storagePool.Count;
				for (int i = 0; i < cnt; i++)
				{
					if (m_storagePool[i] == null)
					{
						m_storagePool[i] = storage;
						return;
					}
				}
				m_storagePool.Add(storage);
			}
		}

		/// <summary>
		/// Creates a new message for sending
		/// </summary>
		public NetOutgoingMessage CreateMessage()
		{
			return CreateMessage(m_configuration.m_defaultOutgoingMessageCapacity);
		}

		/// <summary>
		/// Creates a new message for sending and writes the provided string to it
		/// </summary>
		public NetOutgoingMessage CreateMessage(string content)
		{
			var om = CreateMessage(2 + content.Length); // fair guess
			om.Write(content);
			return om;
		}

		/// <summary>
		/// Creates a new message for sending
		/// </summary>
		/// <param name="initialCapacity">initial capacity in bytes</param>
		public NetOutgoingMessage CreateMessage(int initialCapacity)
		{
			NetOutgoingMessage retval;
			if (m_outgoingMessagesPool == null || !m_outgoingMessagesPool.TryDequeue(out retval))
				retval = new NetOutgoingMessage();

			if (initialCapacity > 0)
				retval.m_data = GetStorage(initialCapacity);

			return retval;
		}

		internal NetIncomingMessage CreateIncomingMessage(NetIncomingMessageType tp, byte[] useStorageData)
		{
			NetIncomingMessage retval;
			if (m_incomingMessagesPool == null || !m_incomingMessagesPool.TryDequeue(out retval))
				retval = new NetIncomingMessage(tp);
			else
				retval.m_incomingMessageType = tp;
			retval.m_data = useStorageData;
			return retval;
		}

		internal NetIncomingMessage CreateIncomingMessage(NetIncomingMessageType tp, int minimumByteSize)
		{
			NetIncomingMessage retval;
			if (m_incomingMessagesPool == null || !m_incomingMessagesPool.TryDequeue(out retval))
				retval = new NetIncomingMessage(tp);
			else
				retval.m_incomingMessageType = tp;
			retval.m_data = GetStorage(minimumByteSize);
			return retval;
		}

		/// <summary>
		/// Recycles a NetIncomingMessage instance for reuse; taking pressure off the garbage collector
		/// </summary>
		public void Recycle(NetIncomingMessage msg)
		{
			if (m_incomingMessagesPool == null)
				return;

			NetException.Assert(m_incomingMessagesPool.Contains(msg) == false, "Recyling already recycled message! Thread race?");

			byte[] storage = msg.m_data;
			msg.m_data = null;
			Recycle(storage);
			msg.Reset();
			m_incomingMessagesPool.Enqueue(msg);
		}

		/// <summary>
		/// Recycles a list of NetIncomingMessage instances for reuse; taking pressure off the garbage collector
		/// </summary>
		public void Recycle(IEnumerable<NetIncomingMessage> toRecycle)
		{
			if (m_incomingMessagesPool == null)
				return;

			// first recycle the storage of each message
			if (m_storagePool != null)
			{
				lock (m_storagePool)
				{
					foreach (var msg in toRecycle)
					{
						var storage = msg.m_data;
						msg.m_data = null;
						m_storagePoolBytes += storage.Length;
						int cnt = m_storagePool.Count;
						for (int i = 0; i < cnt; i++)
						{
							if (m_storagePool[i] == null)
							{
								m_storagePool[i] = storage;
								return;
							}
						}
						msg.Reset();
						m_storagePool.Add(storage);
					}
				}
			}

			// then recycle the message objects
			m_incomingMessagesPool.Enqueue(toRecycle);
		}

		internal void Recycle(NetOutgoingMessage msg)
		{
			if (m_outgoingMessagesPool == null)
				return;

			NetException.Assert(m_outgoingMessagesPool.Contains(msg) == false, "Recyling already recycled message! Thread race?");
			
			byte[] storage = msg.m_data;
			msg.m_data = null;
			
			// message fragments cannot be recycled
			// TODO: find a way to recycle large message after all fragments has been acknowledged; or? possibly better just to garbage collect them
			if (msg.m_fragmentGroup == 0)
				Recycle(storage);
	
			msg.Reset();
			m_outgoingMessagesPool.Enqueue(msg);
		}

		/// <summary>
		/// Creates an incoming message with the required capacity for releasing to the application
		/// </summary>
		internal NetIncomingMessage CreateIncomingMessage(NetIncomingMessageType tp, string text)
		{
			NetIncomingMessage retval;
			if (string.IsNullOrEmpty(text))
			{
				retval = CreateIncomingMessage(tp, 1);
				retval.Write(string.Empty);
				return retval;
			}

			int numBytes = System.Text.Encoding.UTF8.GetByteCount(text);
			retval = CreateIncomingMessage(tp, numBytes + (numBytes > 127 ? 2 : 1));
			retval.Write(text);

			return retval;
		}
	}
}
