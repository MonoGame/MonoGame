using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Diagnostics;
using Task = System.Threading.Tasks.Task;

namespace MonoGame.Templates.VSExtension
{
    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The minimum requirement for a class to be considered a valid package for Visual Studio
    /// is to implement the IVsPackage interface and register itself with the shell.
    /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
    /// to do it: it derives from the Package class that provides the implementation of the
    /// IVsPackage interface and uses the registration attributes defined in the framework to
    /// register itself and its components with the shell. These attributes tell the pkgdef creation
    /// utility what data to put into .pkgdef file.
    /// </para>
    /// <para>
    /// To get loaded into VS, the package must be referred by &lt;Asset Type="Microsoft.VisualStudio.VsPackage" ...&gt; in .vsixmanifest file.
    /// </para>
    /// </remarks>
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [Guid(MonoGameTemplatesVSExtensionPackage.PackageGuidString)]
    [ProvideEditorExtension(typeof(EditorFactory), ".mgcb", int.MaxValue, DefaultName = "MGCB Editor")]
    public sealed class MonoGameTemplatesVSExtensionPackage : AsyncPackage
    {
        /// <summary>
        /// MonoGameTemplatesPackage GUID string.
        /// </summary>
        public const string PackageGuidString = "618bc6bc-7a4c-4c3f-8b0f-d4479c8e8690";

    #region Package Members

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token to monitor for initialization cancellation, which can occur when VS is shutting down.</param>
        /// <param name="progress">A provider for progress updates.</param>
        /// <returns>A task representing the async work of package initialization, or an already completed task if there is none. Do not return null from this method.</returns>
        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            RegisterEditorFactory(new EditorFactory());

            // When initialized asynchronously, the current thread may be a background thread at this point.
            // Do any initialization that requires the UI thread after switching to the UI thread.
            await this.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
        }

    #endregion
    }

    [Guid(EditorFactory.GUID)]
    public class EditorFactory : IVsEditorFactory
    {
        public const string GUID = "3ec6cbd1-65bd-4b26-a758-3b207d048872";

        public int CreateEditorInstance(uint grfCreateDoc, string pszMkDocument, string pszPhysicalView, IVsHierarchy pvHier, uint itemid, IntPtr punkDocDataExisting, out IntPtr ppunkDocView, out IntPtr ppunkDocData, out string pbstrEditorCaption, out Guid pguidCmdUI, out int pgrfCDW)
        {
            ppunkDocView = IntPtr.Zero;
            ppunkDocData = IntPtr.Zero;
            pguidCmdUI = new Guid(GUID);
            pgrfCDW = 0;
            pbstrEditorCaption = null;

            // open MGCB editor here
            var process = new Process();
            process.StartInfo.FileName = "dotnet.exe";
            process.StartInfo.Arguments = $"mgcb-editor \"{pszMkDocument}\"";
            process.StartInfo.WorkingDirectory = System.IO.Path.GetDirectoryName(pszMkDocument);
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;

            process.Start();
            
            // this while loop freezes Visual Studio
            /*
            while (!process.StandardOutput.EndOfStream)
            {
                string str = process.StandardOutput.ReadLine();
                System.Diagnostics.Debug.WriteLine(str);
            }
            */
            return VSConstants.S_OK;
        }

        public int SetSite(Microsoft.VisualStudio.OLE.Interop.IServiceProvider psp)
        {
            return VSConstants.S_OK;
        }

        public int Close()
        {
            return VSConstants.S_OK;
        }

        public int MapLogicalView(ref Guid rguidLogicalView, out string pbstrPhysicalView)
        {
            pbstrPhysicalView = null;

            return VSConstants.S_OK;
        }
    }
}
