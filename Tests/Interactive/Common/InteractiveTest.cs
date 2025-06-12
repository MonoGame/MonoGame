// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using MonoGame.Framework.Utilities;

namespace MonoGame.InteractiveTests
{
    /// <summary>
    /// Manages creating of interactive test(s) instance(s).
    /// </summary>
    public class InteractiveTest
    {
        public static bool TryCreateFrom(Type type, out InteractiveTest test)
        {
            test = null;
            if (!typeof(TestGame).IsAssignableFrom(type)) { return false; }

            var attrs = type.GetCustomAttributes(typeof(InteractiveTestAttribute), false);
            if (attrs.Length == 0) { return false; }

            var attr = (InteractiveTestAttribute)attrs[0];

            test = new InteractiveTest(
                type, attr.Name ?? type.Name, attr.Category ?? Categories.Default, attr.Platforms);
            return true;
        }

        private InteractiveTest(Type type, string name, string category, MonoGamePlatform[] platforms)
        {
            _type = type;
            _name = name;
            _category = category;
            _platforms = platforms;
        }

        private readonly Type _type;
        public Type Type { get { return _type; } }

        private readonly string _name;
        public string Name { get { return _name; } }

        private readonly string _category;
        public string Category { get { return _category; } }

        private readonly MonoGamePlatform[] _platforms;
        public MonoGamePlatform[] Platforms { get { return _platforms; } }

        public Game Create()
        {
            return (Game)Activator.CreateInstance(_type);
        }

        public bool MatchesPlatform(MonoGamePlatform runtimePlatform)
        {
            // Empty array matches everything.
            if (_platforms.Length == 0) { return true; }

            foreach (var testPlatform in _platforms)
            {
                if (testPlatform == runtimePlatform) { return true; }
            }

            return false;
        }
    }
}
