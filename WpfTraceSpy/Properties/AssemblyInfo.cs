﻿using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;

[assembly: AssemblyTitle("WpfTraceSpy")]
[assembly: AssemblyDescription("An alternative to DbgView. Also supports ETW string traces.")]
#if DEBUG
[assembly: AssemblyConfiguration("Debug")]
#else
[assembly: AssemblyConfiguration("Release")]
#endif
[assembly: AssemblyCompany("Simon Mourier")]
[assembly: AssemblyProduct("WPF Trace Spy")]
[assembly: AssemblyCopyright("Copyright © 2011-2025 Simon Mourier. All rights reserved.")]
[assembly: ComVisible(false)]
[assembly: ThemeInfo(ResourceDictionaryLocation.None, ResourceDictionaryLocation.SourceAssembly)]
[assembly: Guid("d28061de-725f-4149-b67c-31f31f2d0626")]
