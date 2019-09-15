using System.Collections.Generic;

namespace TwoMGFX.TPGParser
{
    public class TechniqueInfo
    {
        public int startPos;
        public int length;

        public string name;
        public List<PassInfo> Passes = new List<PassInfo>();
    }
}