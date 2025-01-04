using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Windows;

[assembly: AssemblyTitle("WpfTraceSpyCore")]
[assembly: AssemblyDescription("An alternative to DbgView. Also supports ETW string traces.")]
#if DEBUG
[assembly: AssemblyConfiguration("Debug")]
#else
[assembly: AssemblyConfiguration("Release")]
#endif
[assembly: AssemblyCompany("Simon Mourier")]
[assembly: AssemblyProduct("Trace Spy")]
[assembly: AssemblyCopyright("Copyright © 2011-2025 Simon Mourier. All rights reserved.")]
[assembly: ComVisible(false)]
[assembly: ThemeInfo(ResourceDictionaryLocation.None, ResourceDictionaryLocation.SourceAssembly)]
[assembly: Guid("a87fb863-89df-4b40-b664-f73c18096c1c")]
[assembly: SupportedOSPlatform("Windows")]
