using System;

namespace Microsoft.Xna.Framework.Net
{
    public sealed class QualityOfService
    {
        public TimeSpan AverageRoundtripTime { get { return TimeSpan.Zero; } }
        public int BytesPerSecondDownstream { get { return 0; } }
        public int BytesPerSecondUpstream { get { return 0; } }
        public bool IsAvailable { get { return false; } }
        public TimeSpan MinimumRoundtripTime { get { return TimeSpan.Zero; } }
    }
}
