'-----------------------------------------------------------------------
' ETWModule from https://github.com/smourier/TraceSpy
'
' MIT License
'
' Copyright (c) 2017-2018 Simon Mourier
'
' Permission is hereby granted, free of charge, to any person obtaining a copy
' of this software and associated documentation files (the "Software"), to deal
' in the Software without restriction, including without limitation the rights
' to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
' copies of the Software, and to permit persons to whom the Software is
' furnished to do so, subject to the following conditions:
'
' The above copyright notice and this permission notice shall be included in all
' copies or substantial portions of the Software.
'
' THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
' IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
' FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
' AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
' LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
' OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
' SOFTWARE.
'-----------------------------------------------------------------------

Global regHandle As LongLong ' note the handle is not a pointer, it's always 64-bit

Private Type GUID
    Data1 As Long
    Data2 As Integer
    Data3 As Integer
    Data4(7) As Byte
End Type

Private Declare PtrSafe Sub OutputDebugString Lib "kernel32" Alias "OutputDebugStringW" (ByVal text As LongPtr)

Private Declare PtrSafe Function CLSIDFromString Lib "ole32.dll" (ByVal lpsz As LongPtr, pclsid As GUID) As Long

Private Declare PtrSafe Function EventRegister Lib "advapi32" (providerId As GUID, ByVal enableCallback As LongPtr, ByVal callbackContext As LongPtr, ByRef handle As LongLong) As Long

Private Declare PtrSafe Function EventUnregister Lib "advapi32" (ByVal handle As LongLong) As Long

Private Declare PtrSafe Function EventWriteString Lib "advapi32" (ByVal handle As LongLong, ByVal level As Byte, ByVal keyword As Long, ByVal text As LongPtr) As Long

Public Sub Ods(text As String)

  OutputDebugString StrPtr(text)

End Sub

Public Sub RegisterETWProvider(providerId As String)

    Dim id As GUID
    Dim result As Long
    
    CLSIDFromString StrPtr(providerId), id
    result = EventRegister(id, 0, 0, regHandle)
    If result <> 0 Then
        Ods "something went wrong with the EventRegister call..."
        Return
    End If

    Ods "ETW Trace provider " & providerId & " was registered successfully."
End Sub

Public Sub UnregisterETWProvider()

    result = EventUnregister(regHandle)
    If result <> 0 Then
        Ods "something went wrong with the EventUnregister call..."
        Return
    End If

    Ods "ETW Trace provider was unregistered successfully."

End Sub

Public Sub WriteETWEvent(text As String)

    result = EventWriteString(regHandle, 0, 0, StrPtr(text))
    If result <> 0 Then
        Ods "something went wrong with the EventWriteString call..."
        Return
    End If

End Sub


