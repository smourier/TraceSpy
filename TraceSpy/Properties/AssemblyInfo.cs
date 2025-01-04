using System.Reflection;
using System.Runtime.InteropServices;

[assembly: AssemblyTitle("TraceSpy")]
[assembly: AssemblyDescription("An alternative to DbgView.")]
#if DEBUG
[assembly: AssemblyConfiguration("Debug")]
#else
[assembly: AssemblyConfiguration("Release")]
#endif
[assembly: AssemblyCompany("Simon Mourier")]
[assembly: AssemblyProduct("Trace Spy")]
[assembly: AssemblyCopyright("Copyright © 2011-2025 Simon Mourier. All rights reserved.")]
[assembly: ComVisible(false)]
[assembly: Guid("dfc20d26-c4bd-440f-b248-81c68fc8c961")]
[assembly: AssemblyVersion("2.4.2.0")]
[assembly: AssemblyFileVersion("2.4.2.0")]
[assembly: AssemblyInformationalVersion("2.4.2.0")]
