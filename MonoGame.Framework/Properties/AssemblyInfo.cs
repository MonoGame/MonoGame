using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Resources;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
#if NET
[assembly: AssemblyDescription("MonoGame.Net")]
[assembly: AssemblyTitle("MonoGame.Framework.Net")]
#elif OUYA
[assembly: AssemblyTitle("MonoGame.Framework.OUYA")]
[assembly: AssemblyDescription("MonoGame for OUYA")]
#elif ANDROID
[assembly: AssemblyTitle("MonoGame.Framework.Android")]
[assembly: AssemblyDescription("MonoGame for Android")]
#elif WINDOWS_STOREAPP
[assembly: AssemblyTitle("MonoGame.Framework.Win8")]
[assembly: AssemblyDescription("MonoGame for Windows Store")]
#elif WINDOWS
#if DIRECTX
[assembly: AssemblyTitle("MonoGame.Framework.WinDX")]
[assembly: AssemblyDescription("MonoGame for Windows Desktop (DirectX)")]
#else
[assembly: AssemblyTitle("MonoGame.Framework.WinGL")]
[assembly: AssemblyDescription("MonoGame for Windows Desktop (OpenGL)")]
#endif
#elif PSM
[assembly: AssemblyTitle("MonoGame.Framework.PSM")]
[assembly: AssemblyDescription("MonoGame for PlayStation Mobile")]
#elif LINUX
[assembly: AssemblyTitle("MonoGame.Framework.Linux")]
[assembly: AssemblyDescription("MonoGame for Linux")]
#elif MAC
[assembly: AssemblyTitle("MonoGame.Framework.OSX")]
[assembly: AssemblyDescription("MonoGame for Mac OS X")]
#elif IOS
[assembly: AssemblyTitle("MonoGame.Framework.iOS")]
[assembly: AssemblyDescription("MonoGame for iOS")]
#elif WINDOWS_PHONE
[assembly: AssemblyTitle("MonoGame.Framework.WP8")]
[assembly: AssemblyDescription("MonoGame for Windows Phone 8")]
#endif
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("")]
#if NET
[assembly: AssemblyProduct("MonoGame.Framework.NET")]
#else
[assembly: AssemblyProduct("MonoGame.Framework")]
#endif
[assembly: AssemblyCopyright("Copyright © 2011-2013")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Mark the assembly as CLS compliant so it can be safely used in other .NET languages
[assembly:CLSCompliant(true)]

// Allow the content pipeline assembly to access 
// some of our internal helper methods that it needs.
[assembly: InternalsVisibleTo("MonoGame.Framework.Content.Pipeline")]
[assembly: InternalsVisibleTo("MonoGame.Framework.Net")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
#if NET
[assembly: Guid("C113F941-193E-4030-BC44-95B2C7EF19F9")]
#else
[assembly: Guid("81119db2-82a6-45fb-a366-63a08437b485")]
#endif

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Build and Revision Numbers 
// by using the '*' as shown below:
// [assembly: AssemblyVersion("1.0.*")]
[assembly: AssemblyVersion("3.2.1.0")]
[assembly: AssemblyFileVersion("3.2.1.0")]
[assembly: NeutralResourcesLanguageAttribute("en-US")]
