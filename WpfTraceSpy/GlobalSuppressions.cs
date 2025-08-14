using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Sometimes we need to")]
[assembly: SuppressMessage("Performance", "CA1819:Properties should not return arrays", Justification = "Gimme a break")]
[assembly: SuppressMessage("Style", "IDE0017:Simplify object initialization", Justification = "Don't like that")]
[assembly: SuppressMessage("Style", "IDE0016:Use 'throw' expression", Justification = "Nope")]
[assembly: SuppressMessage("Maintainability", "CA1510:Use ArgumentNullException throw helper", Justification = "Need .NET Framework support")]
[assembly: SuppressMessage("Performance", "CA1837:Use 'Environment.ProcessId'", Justification = "Need .NET Framework support")]
[assembly: SuppressMessage("Style", "IDE0057:Use range operator", Justification = "Need .NET Framework support")]
[assembly: SuppressMessage("Usage", "CA2249:Consider using 'string.Contains' instead of 'string.IndexOf'", Justification = "Need .NET Framework support")]
[assembly: SuppressMessage("Style", "IDE0056:Use index operator", Justification = "Need .NET Framework support")]
[assembly: SuppressMessage("Interoperability", "SYSLIB1054:Use 'LibraryImportAttribute' instead of 'DllImportAttribute' to generate P/Invoke marshalling code at compile time", Justification = "Not now")]
[assembly: SuppressMessage("CodeQuality", "IDE0079:Remove unnecessary suppression", Justification = "There's a reason, dude")]
[assembly: SuppressMessage("Design", "CA1069:Enums values should not be duplicated", Justification = "Welcome to interop definitions")]
