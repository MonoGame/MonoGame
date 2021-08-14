// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using MonoGame.Effect.TPGParser;

namespace MonoGame.Effect
{
    class OpenGLESShaderProfile : OpenGLShaderProfile
    {
        protected override bool IsESSL => true;

        public OpenGLESShaderProfile()
            : base("OpenGLES")
        {
        }

        internal override void AddMacros(Dictionary<string, string> macros, Options options)
        {
            base.AddMacros(macros, options);
            macros.Add("ESSL", "1");
        }
    }
}
