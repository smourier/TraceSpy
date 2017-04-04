
Sub WriteOutputDebugString()

    Ods "tést"

End Sub

Sub RegisterETW()

    ' call that only once when the program starts, with a guid specific to your environment
    ' you'll have to declare that same guid as an ETW Provider in TraceSpy
	
	' TODO: change this guid!
    RegisterETWProvider "{31bd04b9-e510-44f8-ab70-cb8275818861}"
    
End Sub

Sub WriteETW()

    ' call that anywhere in your code
    WriteETWEvent "hello world 1"
    WriteETWEvent "hello world 2"
    WriteETWEvent "hello world 3"
    
End Sub

Sub UnregisterETW()

    ' call that only once when the program quits
    UnregisterETWProvider

End Sub

