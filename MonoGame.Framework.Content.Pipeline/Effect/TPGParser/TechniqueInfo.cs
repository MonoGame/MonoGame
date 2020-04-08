using System.Collections.Generic;

namespace MonoGame.Effect.TPGParser
{
    internal class TechniqueInfo
    {
        public int startPos;
        public int length;

        public string name;
        public List<PassInfo> Passes = new List<PassInfo>();
    }
}