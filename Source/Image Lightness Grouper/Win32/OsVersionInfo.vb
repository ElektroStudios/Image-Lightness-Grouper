Option Strict On
Option Explicit On
Option Infer Off

#Region " Imports "

Imports System.Runtime.InteropServices

#End Region

Namespace ImageLightnessGrouper.Win32

    ''' <summary>
    ''' Managed equivalent of the native <c>RTL_OSVERSIONINFOW</c> structure.
    ''' </summary>
    ''' <remarks>
    ''' <see href="https://learn.microsoft.com/en-us/windows-hardware/drivers/ddi/wdm/ns-wdm-_osversioninfow"/>
    ''' </remarks>
    <StructLayout(LayoutKind.Sequential, CharSet:=CharSet.Unicode)>
    Friend Structure OsVersionInfo

        ''' <summary>
        ''' The size of this structure, in bytes. Must be set before calling <c>RtlGetVersion</c>.
        ''' </summary>
        Public OSVersionInfoSize As Integer

        ''' <summary>
        ''' The major version number of the operating system.
        ''' </summary>
        Public MajorVersion As Integer

        ''' <summary>
        ''' The minor version number of the operating system.
        ''' </summary>
        Public MinorVersion As Integer

        ''' <summary>
        ''' The build number of the operating system.
        ''' </summary>
        Public BuildNumber As Integer

        ''' <summary>
        ''' The operating system platform.
        ''' </summary>
        Public PlatformId As Integer

        ''' <summary>
        ''' The latest service pack installed on the system.
        ''' </summary>
        <MarshalAs(UnmanagedType.ByValTStr, SizeConst:=128)>
        Public CSDVersion As String

    End Structure

End Namespace
