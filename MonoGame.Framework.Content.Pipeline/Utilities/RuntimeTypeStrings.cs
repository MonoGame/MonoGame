// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace MonoGame.Framework.Content.Pipeline
{
    public static class RuntimeTypeStrings
    {
        public static readonly string RuntimeContentNamespace = "Microsoft.Framework.Xna.Content";
        public static readonly string RuntimeAssemblyName = "Microsoft.Framework.Xna";
        public static readonly string RuntimeAssemblyFullName = "MonoGame.Framework, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null";

        public static string GetAssemblyQualifiedName(string typeName)
        {
			return RuntimeTypeStrings.RuntimeContentNamespace + "." + typeName + ", " + RuntimeTypeStrings.RuntimeAssemblyFullName;
        }
    }
}