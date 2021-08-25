Attribute VB_Name = "ETWModule3264"
Option Explicit
' Contributed by Patrick O'Beirne  (t: @ExcelAnalytics)
'              Systems Modelling http://www.sysmod.com
'              http://ie.linkedin.com/in/patrickobeirne

' Adapted from:
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

' VBA7: True if you're using Office 2010, False for older versions
' WIN64: True if your Office installation is 64-bit, false for 32 bit.
' Contrary to what its name implies, WIN64 does not mean specifically that you _
   are running Windows 64-bit. It indicates whether you are in a 64-bit application.
   Private Type GUID
       Data1 As Long
       Data2 As Integer
       Data3 As Integer
       Data4(7) As Byte
   End Type
   

#If VBA7 And Win64 Then   ' 64 bit Excel 2010+
   Public regHandle As LongPtr ' note the handle is not a pointer, it's always 64-bit LongLong

   Private Declare PtrSafe Sub OutputDebugString Lib "kernel32" Alias "OutputDebugStringW" (ByVal text As LongPtr)

   Private Declare PtrSafe Function CLSIDFromString Lib "ole32.dll" (ByVal lpsz As LongPtr, pclsid As GUID) As Long

   Private Declare PtrSafe Function EventRegister Lib "advapi32" _
        (providerId As GUID, ByVal enableCallback As LongPtr, ByVal callbackContext As LongPtr, ByRef handle As LongPtr) As Long

   Private Declare PtrSafe Function EventUnregister Lib "advapi32" (ByVal handle As LongPtr) As Long

   Private Declare PtrSafe Function EventWriteString Lib "advapi32" _
       (ByVal handle As LongPtr, ByVal level As Byte, ByVal keyword As Long, ByVal text As LongPtr) As Long

#Else

   ' NOTE: This is only for old VB/VBA version (lower than VB7) that don't have new LongPtr types, etc.
   
   ' note the handle is not a pointer, it's always 64-bit
   ' there's no int64 on old VB, so we use double (8 bytes) as an opaque value
   Public regHandle As Double
   
   Private Declare Sub OutputDebugString Lib "kernel32" Alias "OutputDebugStringW" (ByVal text As Long)
   
   Private Declare Function CLSIDFromString Lib "ole32.dll" (ByVal lpsz As Long, pclsid As GUID) As Long
   
   Private Declare Function EventRegister Lib "advapi32" _
      (providerId As GUID, ByVal enableCallback As Long, ByVal callbackContext As Long, ByRef handle As Double) As Long
   
   Private Declare Function EventUnregister Lib "advapi32" (ByVal handle As Double) As Long
   
   Private Declare Function EventWriteString Lib "advapi32" _
      (ByVal handle As Double, ByVal level As Byte, ByVal keyword As Double, ByVal text As Long) As Long
#End If

Public Sub Ods(text As String)
  OutputDebugString StrPtr(text)
End Sub

Public Sub RegisterETWProvider()
   Const providerId As String = "{cd0f2553-3166-46ab-a3de-e5f2595e02fb}"
   Dim id As GUID
   Dim result As Long
   CLSIDFromString StrPtr(providerId), id
   result = EventRegister(id, 0, 0, regHandle)
   If result = 0 Then
      Ods "ETW Trace provider " & providerId & " was registered successfully. Handle " & regHandle
   Else
      Ods "something went wrong with the EventRegister call..."
   End If
End Sub

Public Sub UnregisterETWProvider()
If regHandle <> 0 Then
   Dim result As Long
   result = EventUnregister(regHandle)
   If result = 0 Then
      Ods "ETW Trace provider was unregistered successfully."
   Else
      Ods "something went wrong with the EventUnregister call..."
   End If
End If
End Sub

Public Sub WriteETWEvent(text As String)
   Dim result As Long
   result = EventWriteString(regHandle, 0, 0, StrPtr(text))
   If result <> 0 Then
       Ods "something went wrong with the EventWriteString call..."
   End If
End Sub

Sub testETW()

   ' SEQUENCE:
  RegisterETWProvider  ' (provide your own GUID)
  
  WriteETWEvent "whatever" ' curiously, appears after the ODS unregister trace.
  
  UnregisterETWProvider
End Sub

