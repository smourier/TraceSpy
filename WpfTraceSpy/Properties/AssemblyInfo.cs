using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;

[assembly: AssemblyTitle("WpfTraceSpy")]
[assembly: AssemblyDescription("An alternative to DbgView.")]
#if DEBUG
[assembly: AssemblyConfiguration("Debug")]
#else
[assembly: AssemblyConfiguration("Release")]
#endif
[assembly: AssemblyCompany("Simon Mourier")]
[assembly: AssemblyProduct("Trace Spy")]
[assembly: AssemblyCopyright("Copyright © 2011-2018 Simon Mourier. All rights reserved.")]
[assembly: ComVisible(false)]
[assembly: ThemeInfo(ResourceDictionaryLocation.None, ResourceDictionaryLocation.SourceAssembly)]
[assembly: Guid("d28061de-725f-4149-b67c-31f31f2d0626")]
[assembly: AssemblyVersion("2.5.0.0")]
[assembly: AssemblyFileVersion("2.5.0.0")]
[assembly: AssemblyInformationalVersion("2.5.0.0")]
