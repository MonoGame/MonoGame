// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Xna.Framework;

namespace MonoGame.InteractiveTests
{
    internal class TestUtils
    {
        public static byte[] ReadBytesFromStream(string titleContainerFilename)
        {
            using (var stream = TitleContainer.OpenStream(titleContainerFilename))
            using (var memoryStream = new MemoryStream())
            {
                stream.CopyTo(memoryStream);
                return memoryStream.ToArray();
            }
        }
    }
}
