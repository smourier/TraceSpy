
' ETWModule32 from https://github.com/smourier/TraceSpy
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

' NOTE: This file is only for old VB/VBA version (lower than VB7) that don't have new LongPtr types, etc.

' note the handle is not a pointer, it's always 64-bit
' there's no int64 on old VB, so we use double (8 bytes) as an opaque value
Global regHandle As Double

Private Type GUID
    Data1 As Long
    Data2 As Integer
    Data3 As Integer
    Data4(7) As Byte
End Type

Private Declare Sub OutputDebugString Lib "kernel32" Alias "OutputDebugStringW" (ByVal text As String)

Private Declare Function CLSIDFromString Lib "ole32.dll" (ByVal lpsz As String, pclsid As GUID) As Long

Private Declare Function EventRegister Lib "advapi32" (providerId As GUID, ByVal enableCallback As Long, ByVal callbackContext As Long, ByRef handle As Double) As Long

Private Declare Function EventUnregister Lib "advapi32" (ByVal handle As Double) As Long

Private Declare Function EventWriteString Lib "advapi32" (ByVal handle As Double, ByVal level As Byte, ByVal keyword As Double, ByVal text As String) As Long

Public Sub Ods(text As String)

  OutputDebugString StrConv(text, vbUnicode)

End Sub

Public Sub RegisterETWProvider(providerId As String)

    Dim id As GUID
    Dim result As Long
    
    CLSIDFromString StrConv(providerId, vbUnicode), id
    result = EventRegister(id, 0, 0, regHandle)
    If result <> 0 Then
        Ods "something went wrong with the EventRegister call..."
        Return
    End If

    Ods "ETW Trace provider " & providerId & " was registered successfully. Handle " & regHandle
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

    result = EventWriteString(regHandle, 0, 0, StrConv(text, vbUnicode))
    If result <> 0 Then
        Ods "something went wrong with the EventWriteString call..."
        Return
    End If

End Sub
