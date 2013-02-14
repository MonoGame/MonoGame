using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Xna.Framework
{
    public sealed class GameTimerEventArgs : EventArgs
    {
        public GameTimerEventArgs()
        {
        }

        public GameTimerEventArgs(TimeSpan totalTime, TimeSpan elapsedTime)
        {
            TotalTime = totalTime;
            ElapsedTime = elapsedTime;
        }

        public TimeSpan ElapsedTime { get; internal set; }

        public TimeSpan TotalTime { get; internal set; }
    }
}
