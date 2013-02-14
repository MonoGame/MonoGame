﻿using System.Reflection;
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
#elif WINDOWS
#if DIRECTX
[assembly: AssemblyDescription("MonoGame for Windows Desktop (DirectX)")]
#else
[assembly: AssemblyDescription("MonoGame for Windows Desktop (OpenGL)")]
#endif
#elif PSM
[assembly: AssemblyDescription("MonoGame for PlayStation Mobile")]
#elif LINUX
[assembly: AssemblyDescription("MonoGame for Linux")]
#elif MAC
[assembly: AssemblyDescription("MonoGame for Mac OS X")]
#elif IOS
[assembly: AssemblyDescription("MonoGame for iOS")]
#elif WINDOWS_STOREAPP
[assembly: AssemblyDescription("MonoGame for Windows Store")]
#elif WINDOWS_PHONE
[assembly: AssemblyDescription("MonoGame for Windows Phone 8")]
#endif
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("")]
[assembly: AssemblyProduct("MonoGame.Framework")]
[assembly: AssemblyCopyright("Copyright © 2011-2013")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("81119db2-82a6-45fb-a366-63a08437b485")]

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
[assembly: AssemblyVersion("3.0.0.0")]
[assembly: AssemblyFileVersion("3.0.0.0")]
[assembly: NeutralResourcesLanguageAttribute("en-US")]
