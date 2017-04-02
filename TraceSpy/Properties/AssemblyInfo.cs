using System.Reflection;
using System.Runtime.InteropServices;

[assembly: AssemblyTitle("TraceSpy")]
[assembly: AssemblyDescription("An alternative to DbgView.")]
#if DEBUG
[assembly: AssemblyConfiguration("Debug")]
#else
[assembly: AssemblyConfiguration("Release")]
#endif
[assembly: AssemblyCompany("SoftFluent S.A.S. http://www.softfluent.com")]
[assembly: AssemblyProduct("Trace Spy")]
[assembly: AssemblyCopyright("Copyright © 2011-2014 Simon Mourier. All rights reserved.")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]
[assembly: ComVisible(false)]
[assembly: Guid("dfc20d26-c4bd-440f-b248-81c68fc8c961")]
[assembly: AssemblyVersion("2.3.0.0")]
[assembly: AssemblyFileVersion("2.3.0.0")]
[assembly: AssemblyInformationalVersion("2.3.0.0")]
