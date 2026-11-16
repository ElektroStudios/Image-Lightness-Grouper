Option Strict On
Option Explicit On
Option Infer Off

#Region " Imports "

Imports System.ComponentModel
Imports System.Runtime.InteropServices

Imports ImageLightnessGrouper.Win32

#End Region

Namespace ImageLightnessGrouper.Infrastructure

    ''' <summary>
    ''' Provides reliable (manifest-independent) information about the running Windows version.
    ''' </summary>
    Friend NotInheritable Class OperatingSystemInfo

#Region " Constants and Read-only Fields "

        ''' <summary>
        ''' Windows 10 version 1809 (October 2018 Update): first build with immersive dark mode support (attribute 19).
        ''' </summary>
        Friend Const Windows10Build1809 As Integer = 17763

        ''' <summary>
        ''' Windows 10 20H1 Insider build: first build with the documented <c>DWMWA_USE_IMMERSIVE_DARK_MODE</c> (attribute 20).
        ''' </summary>
        Friend Const Windows10Build20H1 As Integer = 18985

        ''' <summary>
        ''' Lazily resolved Windows build number.
        ''' </summary>
        Private Shared ReadOnly LazyBuildNumber As New Lazy(Of Integer)(AddressOf OperatingSystemInfo.QueryBuildNumber)

#End Region

#Region " Constructors "

        ''' <summary>
        ''' Prevents a default instance of the <see cref="OperatingSystemInfo"/> class from being created.
        ''' </summary>
        Private Sub New()
        End Sub

#End Region

#Region " Properties "

        ''' <summary>
        ''' Gets the build number of the running Windows operating system.
        ''' </summary>
        Friend Shared ReadOnly Property BuildNumber As Integer
            Get
                Return OperatingSystemInfo.LazyBuildNumber.Value
            End Get
        End Property

#End Region

#Region " Private Methods "

        ''' <summary>
        ''' Queries the real Windows build number through <c>RtlGetVersion</c>.
        ''' </summary>
        ''' <returns>The Windows build number.</returns>
        ''' <exception cref="Win32Exception">Thrown when <c>RtlGetVersion</c> fails.</exception>
        Private Shared Function QueryBuildNumber() As Integer
            Dim versionInfo As New OsVersionInfo() With {
                .OSVersionInfoSize = Marshal.SizeOf(GetType(OsVersionInfo))
            }

            Dim status As Integer = NativeMethods.RtlGetVersion(versionInfo)
            If status <> 0 Then
                Throw New Win32Exception(status, $"RtlGetVersion failed with NTSTATUS 0x{status:X8}.")
            End If

            Return versionInfo.BuildNumber
        End Function

#End Region

    End Class

End Namespace
