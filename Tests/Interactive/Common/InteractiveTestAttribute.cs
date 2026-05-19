// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using MonoGame.Framework.Utilities;

namespace MonoGame.InteractiveTests
{
    /// <summary>
    /// Attribute specified on test classes that are automatically
    /// discovered and shown/provided via some UI / command-line arg.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class InteractiveTestAttribute : Attribute
    {
        public InteractiveTestAttribute(string name, string category,
            MonoGamePlatform[] platforms = null)
        {
            _name = name;
            _category = category;

            // Empty array matches everything.
            if (platforms == null) { platforms = new MonoGamePlatform[] { }; }

            _platforms = platforms;
        }

        /// <summary>Human-readable name of the test</summary>
        private readonly string _name;

        public string Name { get { return _name; } }

        /// <summary>Category of the test. See <see cref="Categories"/></summary>
        private readonly string _category;

        public string Category { get { return _category; } }

        /// <summary>
        /// Supported platforms that this test can run on (empty array
        /// allows running on any platform.
        /// </summary>
        private readonly MonoGamePlatform[] _platforms;

        public MonoGamePlatform[] Platforms { get { return _platforms; } }
    }
}
