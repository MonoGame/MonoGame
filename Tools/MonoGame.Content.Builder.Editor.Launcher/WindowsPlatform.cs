using System;
using System.CommandLine.Invocation;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using Microsoft.Win32;
using Process = System.Diagnostics.Process;

namespace MonoGame.Content.Builder.Editor.Launcher
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

    public class WindowsPlatform : IPlatform
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

        private readonly static string _executablePath = Path.Combine(Path.GetDirectoryName(typeof(WindowsPlatform).Assembly.Location), "mgcb-editor-windows", "mgcb-editor-windows.exe");
        private readonly static string _startMenuLink = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.StartMenu), "Programs", "MGCB Editor.lnk");

        public void Register(InvocationContext context)
        {
            var command = $"\"{_executablePath}\" \"%1\"";
            var iconPath = $"{_executablePath},0";

            var link = (IShellLink)new ShellLink();
            link.SetPath(_executablePath);
            ((IPersistFile)link).Save(_startMenuLink, false);

            SetWindowsAssociation(extension, progId, fileTypeDescription, iconPath, verb, commandText, command);
            RefreshEnvironment();
        }

        public void Unregister(InvocationContext context)
        {
            if (File.Exists(_startMenuLink))
                File.Delete(_startMenuLink);

            UnsetWindowsAssociation(extension, progId);
            RefreshEnvironment();
        }

        public void Run(InvocationContext context, string project)
        {
            Process.Start(_executablePath, $"\"{project}\"");
        }

        private static void SetWindowsAssociation(string extension, string progId, string fileTypeDescription, string icon, string verb, string commandText, string command)
        {
            SetRegistryKey($@"Software\Classes\.{extension}", null, progId);
            SetRegistryKey($@"Software\Classes\{progId}", null, fileTypeDescription);
            SetRegistryKey($@"Software\Classes\{progId}\DefaultIcon", null, icon);
            SetRegistryKey($@"Software\Classes\{progId}\shell", null, verb);
            SetRegistryKey($@"Software\Classes\{progId}\shell\{verb}", null, commandText);
            SetRegistryKey($@"Software\Classes\{progId}\shell\{verb}\command", null, command);
        }

        private static void UnsetWindowsAssociation(string extension, string progId)
        {
            DeleteRegistryKeyTree($@"Software\Classes\.{extension}");
            DeleteRegistryKeyTree($@"Software\Classes\{progId}");
        }

        private static void SetRegistryKey<T>(string keyPath, string name, T value, RegistryValueKind valueKind = RegistryValueKind.String)
        {
            using (var key = Registry.CurrentUser.CreateSubKey(keyPath))
            {
                if (!value.Equals(key.GetValue(name)))
                {
                    key.SetValue(name, value, valueKind);
                }
            }
        }

        private static void DeleteRegistryKeyTree(string keyPath)
        {
            using (var key = Registry.CurrentUser.OpenSubKey(keyPath))
            {
                if (key != null)
                {
                    Registry.CurrentUser.DeleteSubKeyTree(keyPath);
                }
            }
        }

        private static void RefreshEnvironment()
        {
            // Refresh Windows explorer.
            SHChangeNotify(SHCNE_ASSOCCHANGED, SHCNF_FLUSH, IntPtr.Zero, IntPtr.Zero);
        }
    }
}
