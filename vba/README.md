You can use ETW real time traces with VBA. I've provided 3 files:

* ETWModule.bas: a reusable VBA module to add support for ETW real time traces to a VB/VBA project.
* Module1.bas: demonstrates how to use ETWModule
* EtwSample.xlsm: an excel file with a spreasheet and buttons that includes ETWModule.bas and Module1.bas. You can use this to test the system

Don't forget to declare your ETW provider guid in TraceSpy (and enable ETW traces in TraceSpy) if you want to test the whole thing.
