Option Strict On
Option Explicit On
Option Infer Off

#Region " Imports "

Imports System.Runtime.InteropServices

#End Region

Namespace ImageLightnessGrouper.Core.Win32

    ''' <summary>
    ''' Contains information about the current state of both physical and virtual memory.
    ''' </summary>
    ''' <remarks>
    ''' <see href="https://learn.microsoft.com/en-us/windows/win32/api/sysinfoapi/ns-sysinfoapi-memorystatusex"/>
    ''' </remarks>
    <StructLayout(LayoutKind.Sequential)>
    Friend Structure MemoryStatusEx

        ''' <summary>
        ''' <c>dwLength</c>: the size of the structure, in bytes. Must be set before calling
        ''' <see cref="NativeMethods.GlobalMemoryStatusEx"/>.
        ''' </summary>
        Public Length As UInteger

        ''' <summary>
        ''' <c>dwMemoryLoad</c>: the approximate percentage of physical memory that is in use (0-100).
        ''' </summary>
        Public MemoryLoad As UInteger

        ''' <summary>
        ''' <c>ullTotalPhys</c>: the amount of actual physical memory, in bytes.
        ''' </summary>
        Public TotalPhysical As ULong

        ''' <summary>
        ''' <c>ullAvailPhys</c>: the amount of physical memory currently available, in bytes.
        ''' </summary>
        Public AvailablePhysical As ULong

        ''' <summary>
        ''' <c>ullTotalPageFile</c>: the current committed memory limit, in bytes.
        ''' </summary>
        Public TotalPageFile As ULong

        ''' <summary>
        ''' <c>ullAvailPageFile</c>: the maximum amount of memory the current process can commit, in bytes.
        ''' </summary>
        Public AvailablePageFile As ULong

        ''' <summary>
        ''' <c>ullTotalVirtual</c>: the size of the user-mode portion of the virtual address space, in bytes.
        ''' </summary>
        Public TotalVirtual As ULong

        ''' <summary>
        ''' <c>ullAvailVirtual</c>: the amount of unreserved and uncommitted user-mode virtual memory, in bytes.
        ''' </summary>
        Public AvailableVirtual As ULong

        ''' <summary>
        ''' <c>ullAvailExtendedVirtual</c>: reserved; always zero.
        ''' </summary>
        Public AvailableExtendedVirtual As ULong

    End Structure

End Namespace
