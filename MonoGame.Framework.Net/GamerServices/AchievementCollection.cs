// #region License
// /*
// Microsoft Public License (Ms-PL)
// MonoGame - Copyright © 2009 The MonoGame Team
// 
// All rights reserved.
// 
// This license governs use of the accompanying software. If you use the software, you accept this license. If you do not
// accept the license, do not use the software.
// 
// 1. Definitions
// The terms "reproduce," "reproduction," "derivative works," and "distribution" have the same meaning here as under 
// U.S. copyright law.
// 
// A "contribution" is the original software, or any additions or changes to the software.
// A "contributor" is any person that distributes its contribution under this license.
// "Licensed patents" are a contributor's patent claims that read directly on its contribution.
// 
// 2. Grant of Rights
// (A) Copyright Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, 
// each contributor grants you a non-exclusive, worldwide, royalty-free copyright license to reproduce its contribution, prepare derivative works of its contribution, and distribute its contribution or any derivative works that you create.
// (B) Patent Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, 
// each contributor grants you a non-exclusive, worldwide, royalty-free license under its licensed patents to make, have made, use, sell, offer for sale, import, and/or otherwise dispose of its contribution in the software or derivative works of the contribution in the software.
// 
// 3. Conditions and Limitations
// (A) No Trademark License- This license does not grant you rights to use any contributors' name, logo, or trademarks.
// (B) If you bring a patent claim against any contributor over patents that you claim are infringed by the software, 
// your patent license from such contributor to the software ends automatically.
// (C) If you distribute any portion of the software, you must retain all copyright, patent, trademark, and attribution 
// notices that are present in the software.
// (D) If you distribute any portion of the software in source code form, you may do so only under this license by including 
// a complete copy of this license with your distribution. If you distribute any portion of the software in compiled or object 
// code form, you may only do so under a license that complies with this license.
// (E) The software is licensed "as-is." You bear the risk of using it. The contributors give no express warranties, guarantees
// or conditions. You may have additional consumer rights under your local laws which this license cannot change. To the extent
// permitted under your local laws, the contributors exclude the implied warranties of merchantability, fitness for a particular
// purpose and non-infringement.
// */
// #endregion License
// 
using System;
using System.Collections;
using System.Collections.Generic;

namespace Microsoft.Xna.Framework.GamerServices
{
	public class AchievementCollection : IList<Achievement>, ICollection<Achievement>, IEnumerable<Achievement>, IEnumerable, IDisposable
	{
		private List<Achievement> innerlist;
		
		public AchievementCollection ()
		{
			innerlist = new List<Achievement>();
		}
		
        ~AchievementCollection()
        {
            Dispose(false);
        }

		#region Properties
		public int Count
        {
            get { return innerlist.Count; }
        }
		
		public Achievement this[int index]
        {
            get { return innerlist[index]; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException();

                if (index >= innerlist.Count)
                    throw new IndexOutOfRangeException();

                /*if (innerlist[index].Position == value.Position)
                    innerlist[index] = value;
                else
                {
                    innerlist.RemoveAt(index);
                    innerlist.Add(value);
                }*/
            }
        }

		private bool isReadOnly = false;
		public bool IsReadOnly 
		{
            get
			{
				return isReadOnly;
			}
        }
		
        #endregion Properties
		
		#region Public Methods
		public void Add(Achievement item)
        {
            if (item == null)
                throw new ArgumentNullException();

            if (innerlist.Count == 0)
            {
                innerlist.Add(item);
                return;
            }

            for (int i = 0; i < innerlist.Count; i++)
            {
                /*if (item.Position < innerlist[i].Position)
                {
                    this.innerlist.Insert(i, item);
                    return;
                }*/
            }

            this.innerlist.Add(item);
        }

        public void Clear()
        {
            innerlist.Clear();
        }
		
		public bool Contains(Achievement item)
        {
            return innerlist.Contains(item);
        }
        
        public void CopyTo(Achievement[] array, int arrayIndex)
        {
            innerlist.CopyTo(array, arrayIndex);
        }
		
		public void Dispose()
	    {
            Dispose(true);
            GC.SuppressFinalize(this);
		}

        protected virtual void Dispose(bool disposing)
        {
        
        }
		
		public int IndexOf(Achievement item)
        {
            return innerlist.IndexOf(item);
        }
		
		public void Insert(int index, Achievement item)
        {
            innerlist.Insert(index, item);
        }
        
        public bool Remove(Achievement item)
        {
            return innerlist.Remove(item);
        }
        
        public void RemoveAt(int index)
        {
            innerlist.RemoveAt(index);
        }
		
		public IEnumerator<Achievement> GetEnumerator()
        {
            return innerlist.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return innerlist.GetEnumerator();
        }
		
		#endregion Methods
	}
}

