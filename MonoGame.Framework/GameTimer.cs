using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Xna.Framework
{
    public sealed class GameTimer : IDisposable
    {
        static List<GameTimer> _activeTimers = new List<GameTimer>();

        int DrawOrder { get; set; }

        public int FrameActionOrder { get; set; }

        public TimeSpan UpdateInterval { get; set; }

        public int UpdateOrder { get; set; }

        public event EventHandler<EventArgs> FrameAction;

        public event EventHandler<GameTimerEventArgs> Draw;

        public event EventHandler<GameTimerEventArgs> Update;

        public void Start()
        {
            if ( !_activeTimers.Contains(this) )
                _activeTimers.Add(this);
        }

        public void Stop()
        {
            _activeTimers.Remove(this);
        }

        public static void ResetElapsedTime()
        {
        }

        public static void SuppressFrame()
        {
        }

        public void Dispose()
        {
            _activeTimers.Remove(this);
        }
    }
}
