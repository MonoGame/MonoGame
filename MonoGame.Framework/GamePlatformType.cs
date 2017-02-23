using System;

namespace Microsoft.Xna.Framework
{
    public partial struct GamePlatformType : IEquatable<GamePlatformType>
    {
        // The public platforms that MonoGame supports
        public static readonly GamePlatformType Android = new GamePlatformType("Android");
        public static readonly GamePlatformType iOS = new GamePlatformType("iOS");
        public static readonly GamePlatformType DesktopGL = new GamePlatformType("DesktopGL");
        public static readonly GamePlatformType Windows = new GamePlatformType("Windows");
        public static readonly GamePlatformType WindowsUniversal = new GamePlatformType("WindowsUniversal");
        public static readonly GamePlatformType Web = new GamePlatformType("Web");

        private readonly int _id;
        private readonly string _name;

        public string Name
        {
            get { return _name; }
        }

        public GamePlatformType(string name)
        {
            _name = name;
            _id = name.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return obj is GamePlatformType && this == (GamePlatformType) obj;
        }

        public override int GetHashCode()
        {
            // The _id field is already the hashcode of the platform name
            return _id;
        }

        public static bool operator ==(GamePlatformType a, GamePlatformType b)
        {
            return a._id == b._id;
        }

        public static bool operator !=(GamePlatformType a, GamePlatformType b)
        {
            return a._id != b._id;
        }

        public bool Equals(GamePlatformType other)
        {
            return _id == other._id;
        }
    }
}
