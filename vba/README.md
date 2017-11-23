You can use ETW real time traces with VBA. I've provided 3 files:

* ETWModule.bas: a reusable VBA module to add support for ETW real time traces to a VB/VBA project. This module is for VB/VBA version higher than version 7.
* ETWModule32.bas: same as ETWModule but for VB/VBA lower than version 7.
* EtwSample.xlsm: an excel file with a spreasheet and buttons that includes ETWModule.bas and Module1.bas. You can use this to test the system

Don't forget to declare your ETW provider guid in TraceSpy (and enable ETW traces in TraceSpy) if you want to test the whole thing.
