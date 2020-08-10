using Microsoft.Win32;
using MonoGame.Tools.Pipeline.Utilities;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;

namespace MonoGame.Tools.Pipeline
{
    [ComImport]
    [Guid("00021401-0000-0000-C000-000000000046")]
    internal class ShellLink
    {
    }

    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("000214F9-0000-0000-C000-000000000046")]
    internal interface IShellLink
    {
        void GetPath([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszFile, int cchMaxPath, out IntPtr pfd, int fFlags);
        void GetIDList(out IntPtr ppidl);
        void SetIDList(IntPtr pidl);
        void GetDescription([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszName, int cchMaxName);
        void SetDescription([MarshalAs(UnmanagedType.LPWStr)] string pszName);
        void GetWorkingDirectory([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszDir, int cchMaxPath);
        void SetWorkingDirectory([MarshalAs(UnmanagedType.LPWStr)] string pszDir);
        void GetArguments([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszArgs, int cchMaxPath);
        void SetArguments([MarshalAs(UnmanagedType.LPWStr)] string pszArgs);
        void GetHotkey(out short pwHotkey);
        void SetHotkey(short wHotkey);
        void GetShowCmd(out int piShowCmd);
        void SetShowCmd(int iShowCmd);
        void GetIconLocation([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszIconPath, int cchIconPath, out int piIcon);
        void SetIconLocation([MarshalAs(UnmanagedType.LPWStr)] string pszIconPath, int iIcon);
        void SetRelativePath([MarshalAs(UnmanagedType.LPWStr)] string pszPathRel, int dwReserved);
        void Resolve(IntPtr hwnd, int fFlags);
        void SetPath([MarshalAs(UnmanagedType.LPWStr)] string pszFile);
    }

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

        private readonly static string startMenuLink = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.StartMenu), "Programs", "MGCB Editor.lnk");

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
            RefreshEnvironment();

            var link = (IShellLink)new ShellLink();
            link.SetPath(Path.Combine(Path.GetDirectoryName(typeof(FileAssociation).Assembly.Location), "mgcb-editor-wpf.exe"));
            ((IPersistFile)link).Save(startMenuLink, false);
        }

        public static void Unassociate()
        {
            UnsetWindowsAssociation(extension, progId);
            RefreshEnvironment();

            File.Delete(startMenuLink);
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

        private static bool UnsetWindowsAssociation(string extension, string progId)
        {
            var madeChanges = false;
            Console.WriteLine($"Unassociating MGCB Editor with .{extension} extension in Windows...");
            madeChanges |= DeleteRegistryKeyTree($@"Software\Classes\.{extension}");
            madeChanges |= DeleteRegistryKeyTree($@"Software\Classes\{progId}");
            Console.WriteLine(madeChanges ? $"Unassociation complete!" : "No association found.");
            return madeChanges;
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
