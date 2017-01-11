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
using System.Diagnostics;
using System.Collections.Generic;
using System.Threading;

//
// Comment for Linux Mono users: reports of library thread hangs on EnterReadLock() suggests switching to plain lock() works better
//

namespace Lidgren.Network
{
	/// <summary>
	/// Thread safe (blocking) expanding queue with TryDequeue() and EnqueueFirst()
	/// </summary>
	[DebuggerDisplay("Count={Count} Capacity={Capacity}")]
	public sealed class NetQueue<T>
	{
		// Example:
		// m_capacity = 8
		// m_size = 6
		// m_head = 4
		//
		// [0] item
		// [1] item (tail = ((head + size - 1) % capacity)
		// [2] 
		// [3] 
		// [4] item (head)
		// [5] item
		// [6] item 
		// [7] item
		//
		private T[] m_items;
		private readonly ReaderWriterLockSlim m_lock = new ReaderWriterLockSlim();
		private int m_size;
		private int m_head;

		/// <summary>
		/// Gets the number of items in the queue
		/// </summary>
		public int Count {
			get
			{
				m_lock.EnterReadLock();
				int count = m_size;
				m_lock.ExitReadLock();
				return count;
			}
		}

		/// <summary>
		/// Gets the current capacity for the queue
		/// </summary>
		public int Capacity
		{
			get
			{
				m_lock.EnterReadLock();
				int capacity = m_items.Length;
				m_lock.ExitReadLock();
				return capacity;
			}
		}

		/// <summary>
		/// NetQueue constructor
		/// </summary>
		public NetQueue(int initialCapacity)
		{
			m_items = new T[initialCapacity];
		}

		/// <summary>
		/// Adds an item last/tail of the queue
		/// </summary>
		public void Enqueue(T item)
		{
			m_lock.EnterWriteLock();
			try
			{
				if (m_size == m_items.Length)
					SetCapacity(m_items.Length + 8);

				int slot = (m_head + m_size) % m_items.Length;
				m_items[slot] = item;
				m_size++;
			}
			finally
			{
				m_lock.ExitWriteLock();
			}
		}

		/// <summary>
		/// Adds an item last/tail of the queue
		/// </summary>
		public void Enqueue(IEnumerable<T> items)
		{
			m_lock.EnterWriteLock();
			try
			{
				foreach (var item in items)
				{
					if (m_size == m_items.Length)
						SetCapacity(m_items.Length + 8); // @TODO move this out of loop

					int slot = (m_head + m_size) % m_items.Length;
					m_items[slot] = item;
					m_size++;
				}
			}
			finally
			{
				m_lock.ExitWriteLock();
			}
		}

		/// <summary>
		/// Places an item first, at the head of the queue
		/// </summary>
		public void EnqueueFirst(T item)
		{
			m_lock.EnterWriteLock();
			try
			{
				if (m_size >= m_items.Length)
					SetCapacity(m_items.Length + 8);

				m_head--;
				if (m_head < 0)
					m_head = m_items.Length - 1;
				m_items[m_head] = item;
				m_size++;
			}
			finally
			{
				m_lock.ExitWriteLock();
			}
		}

		// must be called from within a write locked m_lock!
		private void SetCapacity(int newCapacity)
		{
			if (m_size == 0)
			{
				if (m_size == 0)
				{
					m_items = new T[newCapacity];
					m_head = 0;
					return;
				}
			}

			T[] newItems = new T[newCapacity];

			if (m_head + m_size - 1 < m_items.Length)
			{
				Array.Copy(m_items, m_head, newItems, 0, m_size);
			}
			else
			{
				Array.Copy(m_items, m_head, newItems, 0, m_items.Length - m_head);
				Array.Copy(m_items, 0, newItems, m_items.Length - m_head, (m_size - (m_items.Length - m_head)));
			}

			m_items = newItems;
			m_head = 0;

		}

		/// <summary>
		/// Gets an item from the head of the queue, or returns default(T) if empty
		/// </summary>
		public bool TryDequeue(out T item)
		{
			if (m_size == 0)
			{
				item = default(T);
				return false;
			}

			m_lock.EnterWriteLock();
			try
			{
				if (m_size == 0)
				{
					item = default(T);
					return false;
				}

				item = m_items[m_head];
				m_items[m_head] = default(T);

				m_head = (m_head + 1) % m_items.Length;
				m_size--;

				return true;
			}
			catch
			{
#if DEBUG
				throw;
#else
				item = default(T);
				return false;
#endif
			}
			finally
			{
				m_lock.ExitWriteLock();
			}
		}

		/// <summary>
		/// Gets all items from the head of the queue, or returns number of items popped
		/// </summary>
		public int TryDrain(IList<T> addTo)
		{
			if (m_size == 0)
				return 0;

			m_lock.EnterWriteLock();
			try
			{
				int added = m_size;
				while (m_size > 0)
				{
					var item = m_items[m_head];
					addTo.Add(item);

					m_items[m_head] = default(T);
					m_head = (m_head + 1) % m_items.Length;
					m_size--;
				}
				return added;
			}
			finally
			{
				m_lock.ExitWriteLock();
			}
		}

		/// <summary>
		/// Returns default(T) if queue is empty
		/// </summary>
		public T TryPeek(int offset)
		{
			if (m_size == 0)
				return default(T);

			m_lock.EnterReadLock();
			try
			{
				if (m_size == 0)
					return default(T);
				return m_items[(m_head + offset) % m_items.Length];
			}
			finally
			{
				m_lock.ExitReadLock();
			}
		}

		/// <summary>
		/// Determines whether an item is in the queue
		/// </summary>
		public bool Contains(T item)
		{
			m_lock.EnterReadLock();
			try
			{
				int ptr = m_head;
				for (int i = 0; i < m_size; i++)
				{
					if (m_items[ptr] == null)
					{
						if (item == null)
							return true;
					}
					else
					{
						if (m_items[ptr].Equals(item))
							return true;
					}
					ptr = (ptr + 1) % m_items.Length;
				}
				return false;
			}
			finally
			{
				m_lock.ExitReadLock();
			}
		}

		/// <summary>
		/// Copies the queue items to a new array
		/// </summary>
		public T[] ToArray()
		{
			m_lock.EnterReadLock();
			try
			{
				T[] retval = new T[m_size];
				int ptr = m_head;
				for (int i = 0; i < m_size; i++)
				{
					retval[i] = m_items[ptr++];
					if (ptr >= m_items.Length)
						ptr = 0;
				}
				return retval;
			}
			finally
			{
				m_lock.ExitReadLock();
			}
		}

		/// <summary>
		/// Removes all objects from the queue
		/// </summary>
		public void Clear()
		{
			m_lock.EnterWriteLock();
			try
			{
				for (int i = 0; i < m_items.Length; i++)
					m_items[i] = default(T);
				m_head = 0;
				m_size = 0;
			}
			finally
			{
				m_lock.ExitWriteLock();
			}
		}
	}
}
