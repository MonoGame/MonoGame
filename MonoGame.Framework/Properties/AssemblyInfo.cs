using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Resources;

// Common information about an assembly is controlled through the following
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyDescription ("MonoGame is an open source implementation of the Microsoft XNA 4.x Framework")]
[assembly: AssemblyCompany("MonoGame Team")]
[assembly: AssemblyProduct("MonoGame.Framework")]
[assembly: AssemblyCopyright("Copyright © 2009-2020 MonoGame Team")]
[assembly: AssemblyTrademark("MonoGame® is a registered trademark of the MonoGame Team")]
[assembly: AssemblyCulture("")]

// Mark the assembly as CLS compliant so it can be safely used in other .NET languages
[assembly: CLSCompliant(true)]

// Allow the content pipeline assembly to access
// some of our internal helper methods that it needs.
// [assembly: InternalsVisibleTo("MonoGame.Framework.Content.Pipeline")]
// [assembly: InternalsVisibleTo("MonoGame.Framework.Net")]

//Tests projects need access too
//[assembly: InternalsVisibleTo("MonoGame.Tests")]

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
[assembly: AssemblyVersion("3.8.0.0")]
[assembly: AssemblyFileVersion("3.8.0.0")]
[assembly: AssemblyInformationalVersion("3.8.0-codefoco-build.1+528.Branch.codefoco-build.Sha.d3ec5d11fcd14df10d5a93d6efba0686fe13fd9e")]
