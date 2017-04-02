using System.Reflection;
using System.Runtime.InteropServices;

[assembly: AssemblyTitle("TraceSpyService")]
[assembly: AssemblyDescription("Provides remote ETW tracing for TraceSpy clients.")]
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
[assembly: Guid("d969b2f1-2c60-4a2f-8807-738641c271e0")]
[assembly: AssemblyVersion("2.3.0.0")]
[assembly: AssemblyFileVersion("2.3.0.0")]
[assembly: AssemblyInformationalVersion("2.3.0.0")]
