using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Resources;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("MonoGame.Framework")]
#if OUYA
[assembly: AssemblyDescription("MonoGame for OUYA")]
#elif ANDROID
[assembly: AssemblyDescription("MonoGame for Android")]
#elif WINDOWS_STOREAPP
[assembly: AssemblyDescription("MonoGame for Windows Store")]
#elif DESKTOPGL
[assembly: AssemblyDescription("MonoGame for all OpenGL Desktop Platforms")]
#elif WINDOWS
[assembly: AssemblyDescription("MonoGame for Windows Desktop (DirectX)")]
#elif MAC
[assembly: AssemblyDescription("MonoGame for Mac OS X")]
#elif IOS
[assembly: AssemblyDescription("MonoGame for iOS")]
#elif WINDOWS_PHONE
[assembly: AssemblyDescription("MonoGame for Windows Phone 8")]
#endif
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("")]
[assembly: AssemblyProduct("MonoGame.Framework")]
[assembly: AssemblyCopyright("Copyright © 2011-2016")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Mark the assembly as CLS compliant so it can be safely used in other .NET languages
[assembly:CLSCompliant(true)]

// Allow the content pipeline assembly to access 
// some of our internal helper methods that it needs.
[assembly: InternalsVisibleTo("MonoGame.Framework.Content.Pipeline")]
[assembly: InternalsVisibleTo("MonoGame.Framework.Net")]

//Tests projects need access too
[assembly: InternalsVisibleTo("MonoGameTests")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("81119db2-82a6-45fb-a366-63a08437b485")]

// This was needed in WinRT releases to inform the system that we
// don't need to load any language specific resources.
[assembly: NeutralResourcesLanguageAttribute("en-US")]

// Version information for the assembly which is automatically 
// set by our automated build process.
[assembly: AssemblyVersion("0.0.0.0")]
[assembly: AssemblyFileVersion("0.0.0.0")]
