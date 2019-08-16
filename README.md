*** This is the new home of https://tracespy.codeplex.com/ ***

# TraceSpy
TraceSpy is a pure .NET, 100% free and open source, alternative to the very popular SysInternals DebugView tool.

**Update 2019/08/16** : WPFTraceSpy now can log UWP (Universal Windows Application) LoggingChannel messages.

**Update 2018/03/10** : we have just released the first version of a WPF version "WPFTraceSpy" that's *much* faster than the original Winforms one when tracing millions of trace events. More info at the end of this page.

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
TraceSpy also supports simple ETW (Event Tracing for Windows) real time "message" traces. These traces can be easily created from a client point of view like this:

```{{
Guid providerGuid1 = new Guid("01234567-01234-01234-01234-012345678901"); // change this guid, make it yours!
using (EventProvider prov = new EventProvider(providerGuid1))
{
  prov.WriteMessageEvent("hello", 0, 0);
}
```

These traces are very fast to create, and cost almost nothing to the system. In fact you you should get rid of OutputDebugString (this is also the default trace listener on .NET under Windows) usage, as this is a thing of the past, and use ETW, which is *much* better and faster.

The `EventProvider` class - supported with .NET Framework 4 and higher - is located in the `System.Diagnostics.Eventing` namespace. The good news is these traces are super fast, and they can even be left in production code. This is in fact what Microsoft uses for all Windows code.

If you want to use ETW from other platforms than .NET, it's possible (as long as you run on the Windows OS), I've provided some VBA interop code with an Excel sample here: [VBA ETW real time traces sample](vba) 

What's cool is you can now specialize TraceSpy for a given set of traces. Just uncheck the "Capture OutputDebugString events", define some ETW provider to capture, and you will now only get traces that you need!

From the TraceSpy UI, you just need to configure the provider Guid, in the Options menu, like this:

![etw1.png](doc/etw1.png?raw=true)

And add the provider Guid and an optional description which can be added to traces:

![etw2.png](doc/etw2.png?raw=true)

One last note: for some of ETW  traces to be read, TraceSpy *must* be started as Administrator (run under full UAC token).

ETW support also added the following features:

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

# WpfTraceSpy
This is a WPF version of TraceSpy. Wpf TraceSpy is *much* faster than the Winforms version. You can send a million traces to it and it will digest them without any pain (but it will take a while before the million traces will be visible).

Things that are in TraceSpy but not in WpfTraceSpy:
* File Open, File Save, File Save As are not there. I'm not sure it' so useful since we can copy all lines to the clipboard.
* Quick Colorizers and Regex Colorizers are not part of WpfTraceSpy. I may add them in the future.

![wpftracespy.png](doc/wpftracespy.png?raw=true)

# Using WpfTraceSpy for .NET UWP (Universal Windows Application) Applications logging
WpfTraceSpy (not TraceSpy) can also log traces from UWP (Universal Windows Applications) LoggingChannel classes. To emit a trace from your app, it's super simple:

    // somewhere in your initialization code
    private readonly static LoggingChannel _channel = new LoggingChannel("MyApp", new LoggingChannelOptions(), new Guid("01234567-01234-01234-01234-012345678901")); // change this guid, make it yours!

    // everywhere in your code. add simple string traces
    _channel.LogMessage("hello from UWP!");

Note you must configure the ETW provider specifically for that, like this:

![etw2.png](doc/uwplogging.png?raw=true)

And you should receive all UWP traces in WpfTraceSpy.

# Using TraceSpy for .NET Core (ASP or other) on Windows
.NET Core logging is not super easy to configure (this is the least to say...), especially in ASP.NET Core code. So I have provided a support .cs file that enables you to use ETW simple text traces (on Windows platform only) very easily as a logging provider under .NET core. Of course, you will then be able to get those traces in TraceSpy and WpfTraceSpy!

The source is available here: [.NET Core ETW Simple traces](netcore) 

Here is how you can integrate in your startup code:

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            // Just add this line (with a using TraceSpy; on top of the file)
            // It will only works on Windows, but will fail gracefully w/o error on other OSes
            // The guid is an arbitrary value that you will have to configure in (Wpf)TraceSpy's ETW providers
            loggerFactory.AddEventProvider(new Guid("a3f87db5-0cba-4e4e-b712-439980e59870"));

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvc();
        }

