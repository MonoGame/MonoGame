using System.Collections.Generic;

namespace Microsoft.Xna.Framework.Input.Touch
{
    /// <summary>
    /// Stores touches to apply them once a frame for platforms that dispatch touches asynchronously
    /// while user code is running.
    /// </summary>
    internal class TouchQueue
    {
        private readonly List<TouchEvent> _queue = new List<TouchEvent>(); 

        public void Enqueue(int id, TouchLocationState state, Vector2 pos, bool isMouse = false)
        {
            lock (_queue)
            {
                _queue.Add(new TouchEvent(id, state, pos, isMouse));
            }
        }

        public void ProcessQueued()
        {
            lock (_queue)
            {
                for (var i = 0; i < _queue.Count; i++)
                {
                    var ev = _queue[i];
                    TouchPanel.AddEvent(ev.Id, ev.State, ev.Pos, ev.IsMouse);
                }
                _queue.Clear();
            }
        }

        private struct TouchEvent
        {
            public readonly int Id;
            public readonly TouchLocationState State;
            public readonly Vector2 Pos;
            public readonly bool IsMouse;

            public TouchEvent(int id, TouchLocationState state, Vector2 pos, bool isMouse)
            {
                Id = id;
                State = state;
                Pos = pos;
                IsMouse = isMouse;
            }
        }

    }
}
