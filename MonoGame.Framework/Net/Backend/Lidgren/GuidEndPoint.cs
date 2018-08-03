using System;

namespace Microsoft.Xna.Framework.Net.Backend.Lidgren
{
    internal class GuidEndPoint : BasePeerEndPoint
    {
        public static GuidEndPoint Parse(string input)
        {
            Guid guid;
            try { guid = Guid.Parse(input); }
            catch { guid = Guid.Empty; }
            return new GuidEndPoint(guid);
        }

        private Guid guid;

        public GuidEndPoint()
        {
            guid = Guid.NewGuid();
        }

        private GuidEndPoint(Guid guid)
        {
            this.guid = guid;
        }

        public override bool Equals(object obj)
        {
            var otherLidgren = obj as GuidEndPoint;
            if (otherLidgren == null)
            {
                return false;
            }
            return guid.Equals(otherLidgren.guid);
        }

        public override int GetHashCode()
        {
            return guid.GetHashCode();
        }

        public override bool Equals(BasePeerEndPoint other)
        {
            var otherLidgren = other as GuidEndPoint;
            if (otherLidgren == null)
            {
                return false;
            }
            return guid.Equals(otherLidgren.guid);
        }

        public override string ToString()
        {
            return guid.ToString();
        }
    }
}
