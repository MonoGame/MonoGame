using System;
using System.Reflection;

namespace MonoGame.Utilities
{
    internal static class AssemblyHelper
    {
        public static string GetDefaultWindowTitle()
        {
            // Set the window title.
            string windowTitle = string.Empty;

            // When running unit tests this can return null.
            var assembly = Assembly.GetEntryAssembly();
            if (assembly != null)
            {
                //Use the Title attribute of the Assembly if possible.
                var assemblyTitleAtt = ((AssemblyTitleAttribute)Attribute.GetCustomAttribute(assembly, typeof(AssemblyTitleAttribute)));
                if (assemblyTitleAtt != null)
                    windowTitle = assemblyTitleAtt.Title;

                // Otherwise, fallback to the Name of the assembly.
                if (string.IsNullOrEmpty(windowTitle))
                    windowTitle = assembly.GetName().Name;
            }

            return windowTitle;
        }
    }
}
