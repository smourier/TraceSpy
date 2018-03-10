# TraceSpy
TraceSpy is a pure .NET, 100% free and open source, alternative to the very popular SysInternals DebugView tool.

**Update 2018/03/10** : we have just released the first version of a WPF version that's *much* faster than the original Winforms one when tracing millions of trace events. More info at the end of this page.

*** This is the new home of https://tracespy.codeplex.com/ ***

Notables points of interest are:

* It's 100% .NET
* It does not need UAC to be disabled nor special rights (unless you want to use ETW traces, see below)
* It can remove empty lines (which is very handy to get rid of these pesky trace lines sent by Visual Studio or addins, for example...)
* The traced application is less blocked by this tracing tool than by DebugView, because it's more async
* The Copy (CTRL-C) operation just copies the traced text, and not the full line (a full line copy feature is there though)
* The process name of the traced application is optionally displayed instead of the process id (if available and not dead at display  time)
* The find dialog has an autocomplete feature
* Lines that contain newline characters (\r, \n) are not displayed as normal lines (DbgView does this) but as one big line
* There is a super duper cool colorizer feature that allows traces colorization using regular expressions.

![TrceSply.png](doc/TrceSpy.PNG?raw=true)

# ETW messages support
**Starting with version 2.0**, TraceSpy supports simple ETW (Event Tracing for Windows) real time "message" traces. These traces can be easily created from a client point of view like this:

```{{
Guid providerGuid1 = new Guid("01234567-01234-01234-01234-012345678901"); // change this guid, make it yours!
using (EventProvider prov = new EventProvider(providerGuid1))
{
  prov.WriteMessageEvent("hello", 0, 0);
}
```

These traces are very fast to create, and cost almost nothing to the system. In fact you you should get rid of OutputDebugString usage, as this is a thing of the past, and use ETW.

EventProvider - supported with .NET Framework 4 and higher - is located in the System.Diagnostics.Eventing namespace. The good news is these traces are supposed to be super fast, and they can even be left in production code.

If you want to use ETW from other platforms than .NET, it's possible (as long as you run on the Windows OS), I've provided some VBA interop code with an Excel sample here: [VBA ETW real time traces sample](vba) 

What's cool is you can now specialize TraceSpy for a given set of traces. Just uncheck the "Capture OutputDebugString events", define some ETW provider to capture, and you will now only get traces that you need!

From the TraceSpy UI, you just need to configure the provider Guid, in the Options menu, like this:

![etw1.png](doc/etw1.png?raw=true)

And add the provider Guid (the description is mandatory but not used today):

![etw2.png](doc/etw2.png?raw=true)

One last note: for these traces to be read, TraceSpy *must* be started as Administrator (run under full UAC token).

**Version 2.1** version added the following features:

* ETW description with process name (optional).
* Quick Colorizers feature.
* RecordView feature (double click on a trace).
* Support for ETW trace levels (the 2nd parameter in the WriteMessageEvent call above)
* Support for TAB '\t ' character in trace texts.

*Things that are missing* I don't plan to add them since I never used them in DbgView in many years :-)

* Highlights
* Kernel capture
* Save & Log to file (it's easy to do with a copy / paste)
* Append comment
* Remote connect

# ** NEW *** WpfTraceSpy
This is a WPF version of TraceSpy. Wpf TraceSpy is *much* faster than the Winforms version. You can send a million traces to it and it will digest them without any pain (but it will take a while before the million traces will be visible).

Things that are in TraceSpy but not in WpfTraceSpy:
* File Open, File Save, File Save As are not there. I'm not sure it' so useful since we can copy all lines to the clipboard.
* Quick Colorizers and Regex Colorizers are not part of WpfTraceSpy. I may add them in the future.

![wpftracespy.png](doc/wpftracespy.png?raw=true)
