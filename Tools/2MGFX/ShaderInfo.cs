using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TwoMGFX
{
    public class PassInfo
    {
        public string name;

        public string vsModel;
        public string vsFunction;

        public string psModel;
        public string psFunction;
    }

    public class TechniqueInfo
    {
        public int startPos;
        public int length;

        public string name;
        public List<PassInfo> Passes = new List<PassInfo>();
    }

    public class ShaderInfo
    {
        public string fileName;
        public string fileContent;

        public bool DX11Profile;

        public List<TechniqueInfo> Techniques = new List<TechniqueInfo>();
    }
}
