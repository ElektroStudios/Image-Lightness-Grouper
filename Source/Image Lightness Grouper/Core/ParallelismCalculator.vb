Option Strict On
Option Explicit On
Option Infer Off

#Region " Imports "

Imports System.ComponentModel
Imports System.Runtime.InteropServices

Imports ImageLightnessGrouper.Win32

#End Region

Namespace ImageLightnessGrouper.Core

    ''' <summary>
    ''' Decides how many images are decoded in parallel.
    ''' <para></para>
    ''' Decoding a large image can take a lot of memory, so the automatic value is limited both by the free physical
    ''' memory (assuming <see cref="MemoryRequiredPerThreadGB"/> per image) and by the number of logical processors.
    ''' A 32-bit process always uses a single thread: its small, fragmented address space would otherwise run out of
    ''' memory while decoding several big images at once.
    ''' </summary>
    Public NotInheritable Class ParallelismCalculator

#Region " Constants "

        ''' <summary>
        ''' The value that means "calculate the degree of parallelism automatically".
        ''' </summary>
        Public Const Automatic As Integer = 0

        ''' <summary>
        ''' The largest degree of parallelism that can be requested explicitly.
        ''' </summary>
        Public Const MaximumRequestable As Integer = 256

        ''' <summary>
        ''' The estimated maximum amount of memory, in gigabytes, needed to decode a single image.
        ''' </summary>
        Public Const MemoryRequiredPerThreadGB As Double = 1.0R

#End Region

#Region " Constructors "

        ''' <summary>
        ''' Prevents a default instance of the <see cref="ParallelismCalculator"/> class from being created.
        ''' </summary>
        Private Sub New()
        End Sub

#End Region

#Region " Public Methods "

        ''' <summary>
        ''' Calculates the automatic degree of parallelism.
        ''' </summary>
        ''' <param name="availableMemoryGB">The available physical memory, in gigabytes.</param>
        ''' <param name="memoryRequiredPerThreadGB">The memory needed by a single thread, in gigabytes.</param>
        ''' <param name="processorCount">The number of logical processors.</param>
        ''' <param name="isX86Process">Whether the current process is a 32-bit process.</param>
        ''' <returns>
        ''' Always 1 for 32-bit processes. Otherwise, the number of threads that fit in the available memory,
        ''' never lower than 1 and never greater than <paramref name="processorCount"/>.
        ''' </returns>
        ''' <exception cref="ArgumentOutOfRangeException">
        ''' Thrown when <paramref name="availableMemoryGB"/> is negative or not finite, <paramref name="memoryRequiredPerThreadGB"/>
        ''' is not positive or not finite, or <paramref name="processorCount"/> is lower than 1.
        ''' </exception>
        Public Shared Function CalculateAutomatic(availableMemoryGB As Double, memoryRequiredPerThreadGB As Double,
                                                  processorCount As Integer, isX86Process As Boolean) As Integer

            If Double.IsNaN(availableMemoryGB) OrElse Double.IsInfinity(availableMemoryGB) OrElse availableMemoryGB < 0.0R Then
                Throw New ArgumentOutOfRangeException(NameOf(availableMemoryGB), availableMemoryGB, "The available memory must be a finite, non-negative number.")
            End If

            If Double.IsNaN(memoryRequiredPerThreadGB) OrElse Double.IsInfinity(memoryRequiredPerThreadGB) OrElse memoryRequiredPerThreadGB <= 0.0R Then
                Throw New ArgumentOutOfRangeException(NameOf(memoryRequiredPerThreadGB), memoryRequiredPerThreadGB, "The memory required per thread must be a finite, positive number.")
            End If

            If processorCount < 1 Then
                Throw New ArgumentOutOfRangeException(NameOf(processorCount), processorCount, "The processor count must be 1 or greater.")
            End If

            If isX86Process Then
                Return 1
            End If

            ' Clamped in floating point first, so huge ratios cannot overflow the conversion to Integer.
            Dim threadsByMemory As Double = Math.Floor(availableMemoryGB / memoryRequiredPerThreadGB)
            Return Convert.ToInt32(Math.Max(1.0R, Math.Min(threadsByMemory, processorCount)))
        End Function

        ''' <summary>
        ''' Validates a requested degree of parallelism.
        ''' </summary>
        ''' <param name="requested">The requested degree of parallelism.</param>
        ''' <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="requested"/> is outside 0-<see cref="MaximumRequestable"/>.</exception>
        Public Shared Sub ValidateRequested(requested As Integer)
            If requested < Automatic OrElse requested > MaximumRequestable Then
                Throw New ArgumentOutOfRangeException(NameOf(requested), requested,
                                                      $"The degree of parallelism must be between {Automatic} (automatic) and {MaximumRequestable}.")
            End If
        End Sub

        ''' <summary>
        ''' Gets the physical memory available to the decoding threads, in gigabytes.
        ''' </summary>
        ''' <param name="isX86Process">Whether the current process is a 32-bit process.</param>
        ''' <returns>
        ''' A fixed 1 GB budget for 32-bit processes (their address space, not the physical memory, is the limit);
        ''' otherwise, the physical memory currently available.
        ''' </returns>
        ''' <exception cref="Win32Exception">Thrown when the memory status cannot be retrieved.</exception>
        Public Shared Function GetAvailableMemoryGB(isX86Process As Boolean) As Double
            If isX86Process Then
                Return 1.0R
            End If

            Dim status As New MemoryStatusEx() With {
                .Length = CUInt(Marshal.SizeOf(GetType(MemoryStatusEx)))
            }

            If Not NativeMethods.GlobalMemoryStatusEx(status) Then
                Dim errorCode As Integer = Marshal.GetLastWin32Error()
                Throw New Win32Exception(errorCode, $"Cannot retrieve the memory status (GlobalMemoryStatusEx failed with Win32 error {errorCode}).")
            End If

            Return status.AvailablePhysical / 1024.0R / 1024.0R / 1024.0R
        End Function

#End Region

    End Class

End Namespace
