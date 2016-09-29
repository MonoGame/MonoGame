// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections;
using System.Collections.Generic;

namespace Microsoft.Xna.Framework.Content.Pipeline.Graphics
{
    /// <summary>
    /// Provides methods and properties for maintaining an animation channel. An animation channel is a collection of keyframes describing the movement of a single bone or rigid object.
    /// </summary>
    public sealed class AnimationChannel : ICollection<AnimationKeyframe>, IEnumerable<AnimationKeyframe>, IEnumerable
    {
        List<AnimationKeyframe> keyframes;

        /// <summary>
        /// Gets the number of keyframes in the collection.
        /// </summary>
        public int Count
        {
            get
            {
                return keyframes.Count;
            }
        }

        /// <summary>
        /// Gets the keyframe at the specified index position.
        /// </summary>
        public AnimationKeyframe this[int index]
        {
            get
            {
                return keyframes[index];
            }
        }

        /// <summary>
        /// Returns a value indicating whether the object is read-only.
        /// </summary>
        bool ICollection<AnimationKeyframe>.IsReadOnly
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Initializes a new instance of AnimationChannel.
        /// </summary>
        public AnimationChannel()
        {
            keyframes = new List<AnimationKeyframe>();
        }

        /// <summary>
        /// To satisfy ICollection
        /// </summary>
        /// <param name="item"></param>
        void ICollection<AnimationKeyframe>.Add(AnimationKeyframe item)
        {
            keyframes.Add(item);
        }

        /// <summary>
        /// Adds a new keyframe to the collection, automatically sorting the contents according to keyframe times.
        /// </summary>
        /// <param name="item">Keyframe to be added to the channel.</param>
        /// <returns>Index of the new keyframe.</returns>
        public int Add(AnimationKeyframe item)
        {
            if (item == null)
                throw new ArgumentNullException("item");

            // Find the correct place at which to insert it, so we can know the index to return.
            // The alternative is Add, Sort then return IndexOf, which would be horribly inefficient
            // and the order returned by Sort would change each time for keyframes with the same time.

            // BinarySearch returns the index of the first item found with the same time, or the bitwise
            // complement of the next largest item found.
            int index = keyframes.BinarySearch(item);
            if (index >= 0)
            {
                // If a match is found, we do not know if it is at the start, middle or end of a range of
                // keyframes with the same time value.  So look for the end of the range and insert there
                // so we have deterministic behaviour.
                while (index < keyframes.Count)
                {
                    if (item.CompareTo(keyframes[index]) != 0)
                        break;
                    ++index;
                }
            }
            else
            {
                // If BinarySearch returns a negative value, it is the bitwise complement of the next largest
                // item in the list.  So we just do a bitwise complement and insert at that index.
                index = ~index;
            }
            keyframes.Insert(index, item);

            return index;
        }

        /// <summary>
        /// Removes all keyframes from the collection.
        /// </summary>
        public void Clear()
        {
            keyframes.Clear();
        }

        /// <summary>
        /// Searches the collection for the specified keyframe.
        /// </summary>
        /// <param name="item">Keyframe being searched for.</param>
        /// <returns>true if the keyframe exists; false otherwise.</returns>
        public bool Contains(AnimationKeyframe item)
        {
            return keyframes.Contains(item);
        }

        /// <summary>
        /// To satisfy ICollection
        /// </summary>
        /// <param name="array"></param>
        /// <param name="arrayIndex"></param>
        void ICollection<AnimationKeyframe>.CopyTo(AnimationKeyframe[] array, int arrayIndex)
        {
            keyframes.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Determines the index for the specified keyframe.
        /// </summary>
        /// <param name="item">Identity of a keyframe.</param>
        /// <returns>Index of the specified keyframe.</returns>
        public int IndexOf(AnimationKeyframe item)
        {
            return keyframes.IndexOf(item);
        }

        /// <summary>
        /// Removes the specified keyframe from the collection.
        /// </summary>
        /// <param name="item">Keyframe being removed.</param>
        /// <returns>true if the keyframe was removed; false otherwise.</returns>
        public bool Remove(AnimationKeyframe item)
        {
            return keyframes.Remove(item);
        }

        /// <summary>
        /// Removes the keyframe at the specified index position.
        /// </summary>
        /// <param name="index">Index of the keyframe being removed.</param>
        public void RemoveAt(int index)
        {
            keyframes.RemoveAt(index);
        }

        /// <summary>
        /// Returns an enumerator that iterates through the keyframes.
        /// </summary>
        /// <returns>Enumerator for the keyframe collection.</returns>
        public IEnumerator<AnimationKeyframe> GetEnumerator()
        {
            return keyframes.GetEnumerator();
        }

        /// <summary>
        /// To satisfy ICollection
        /// </summary>
        /// <returns></returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return keyframes.GetEnumerator();
        }
    }
}
