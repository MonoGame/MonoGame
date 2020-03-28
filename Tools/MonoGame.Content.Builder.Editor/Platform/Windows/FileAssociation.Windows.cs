using Microsoft.Win32;
using MonoGame.Tools.Pipeline.Utilities;
using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;

namespace MonoGame.Tools.Pipeline
{
    public static class FileAssociation
    {
        // Used to refresh Explorer windows after the registry is updated.
        [DllImport("Shell32.dll")]
        private static extern int SHChangeNotify(int eventId, int flags, IntPtr item1, IntPtr item2);

        private const int SHCNE_ASSOCCHANGED = 0x8000000;
        private const int SHCNF_FLUSH = 0x1000;

        private const string extension = "mgcb";
        private const string progId = "MonoGame.ContentBuilderFile";
        private const string fileTypeDescription = "A MonoGame content builder project.";
        private const string verb = "open";
        private const string commandText = "Open with MGCB Editor";

        public static void Associate()
        {
            // Resolve a dotnet ProcessStartInfo to get the commands and arguments to register.
            var assembly = Assembly.GetExecutingAssembly();
            var startInfo = new ProcessStartInfo
            {
                FileName = assembly.GetName().Name,
                Arguments = "\"%1\""
            }.ResolveDotnetApp();
            var command = $"\"{startInfo.FileName}\" {startInfo.Arguments}";
            var iconPath = $"{assembly.Location},0";

            SetWindowsAssociation(extension, progId, fileTypeDescription, iconPath, verb, commandText, command);
            SetVSAssociation();
            RefreshEnvironment();
        }

        public static void Unassociate()
        {
            UnsetWindowsAssociation(extension, progId);
            UnsetVSAssociation();
            RefreshEnvironment();
        }

        private static bool SetWindowsAssociation(string extension, string progId, string fileTypeDescription, string icon, string verb, string commandText, string command)
        {
            var madeChanges = false;
            Console.WriteLine($"Associating MGCB Editor with .{extension} extension in Windows...");
            madeChanges |= SetRegistryKey($@"Software\Classes\.{extension}", null, progId);
            madeChanges |= SetRegistryKey($@"Software\Classes\{progId}", null, fileTypeDescription);
            madeChanges |= SetRegistryKey($@"Software\Classes\{progId}\DefaultIcon", null, icon);
            madeChanges |= SetRegistryKey($@"Software\Classes\{progId}\shell", null, verb);
            madeChanges |= SetRegistryKey($@"Software\Classes\{progId}\shell\{verb}", null, commandText);
            madeChanges |= SetRegistryKey($@"Software\Classes\{progId}\shell\{verb}\command", null, command);
            Console.WriteLine(madeChanges ? $"Association complete!" : "Already associated.");
            return madeChanges;
        }

        private static bool SetVSAssociation()
        {
            return false;
        }

        private static bool UnsetWindowsAssociation(string extension, string progId)
        {
            var madeChanges = false;
            Console.WriteLine($"Unassociating MGCB Editor with .{extension} extension in Windows...");
            madeChanges |= DeleteRegistryKeyTree($@"Software\Classes\.{extension}");
            madeChanges |= DeleteRegistryKeyTree($@"Software\Classes\{progId}");
            Console.WriteLine(madeChanges ? $"Unassociation complete!" : "No association found.");
            return madeChanges;
        }

        private static bool UnsetVSAssociation()
        {
            return false;
        }

        private static bool SetRegistryKey<T>(string keyPath, string name, T value, RegistryValueKind valueKind = RegistryValueKind.String)
        {
            using (var key = Registry.CurrentUser.CreateSubKey(keyPath))
            {
                if (!value.Equals(key.GetValue(name)))
                {
                    key.SetValue(name, value, valueKind);
                    return true;
                }
            }

            return false;
        }

        private static bool DeleteRegistryKeyTree(string keyPath)
        {
            using (var key = Registry.CurrentUser.OpenSubKey(keyPath))
            {
                if (key != null)
                {
                    Registry.CurrentUser.DeleteSubKeyTree(keyPath);
                    return true;
                }
            }

            return false;
        }

        private static void RefreshEnvironment()
        {
            // Refresh Windows explorer.
            SHChangeNotify(SHCNE_ASSOCCHANGED, SHCNF_FLUSH, IntPtr.Zero, IntPtr.Zero);
        }
    }
}
