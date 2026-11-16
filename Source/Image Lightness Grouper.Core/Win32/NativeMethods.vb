Option Strict On
Option Explicit On
Option Infer Off

#Region " Imports "

Imports System.Runtime.InteropServices
Imports System.Security

#End Region

Namespace ImageLightnessGrouper.Core.Win32

    ''' <summary>
    ''' Platform Invocation (P/Invoke) declarations used by the core library.
    ''' </summary>
    <SuppressUnmanagedCodeSecurity>
    Friend NotInheritable Class NativeMethods

#Region " Constructors "

        ''' <summary>
        ''' Prevents a default instance of the <see cref="NativeMethods"/> class from being created.
        ''' </summary>
        Private Sub New()
        End Sub

#End Region

#Region " kernel32.dll "

        ''' <summary>
        ''' Retrieves information about the current usage of both physical and virtual memory.
        ''' </summary>
        ''' <param name="buffer">
        ''' A <see cref="MemoryStatusEx"/> structure whose <see cref="MemoryStatusEx.Length"/> member
        ''' must be set to the size of the structure before the call.
        ''' </param>
        ''' <returns><see langword="True"/> on success; otherwise, <see langword="False"/> (see <c>GetLastError</c>).</returns>
        ''' <remarks>
        ''' <see href="https://learn.microsoft.com/en-us/windows/win32/api/sysinfoapi/nf-sysinfoapi-globalmemorystatusex"/>
        ''' </remarks>
        <DllImport("kernel32.dll", SetLastError:=True)>
        Friend Shared Function GlobalMemoryStatusEx(ByRef buffer As MemoryStatusEx) As <MarshalAs(UnmanagedType.Bool)> Boolean
        End Function

#End Region

    End Class

End Namespace
