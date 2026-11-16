Option Strict On
Option Explicit On
Option Infer Off

#Region " Imports "

Imports System.Runtime.InteropServices

#End Region

Namespace ImageLightnessGrouper.Infrastructure

    ''' <summary>
    ''' Application-level runtime utilities.
    ''' </summary>
    Public NotInheritable Class UtilApplication

#Region " Constructors "

        ''' <summary>
        ''' Prevents a default instance of the <see cref="UtilApplication"/> class from being created.
        ''' </summary>
        Private Sub New()
        End Sub

#End Region

#Region " Properties "

        ''' <summary>
        ''' Gets the processor architecture of the currently running process (e.g. <c>X64</c>, <c>X86</c>, <c>Arm64</c>).
        ''' </summary>
        Public Shared ReadOnly Property ProcessArchitecture As Architecture
            Get
                Return RuntimeInformation.ProcessArchitecture
            End Get
        End Property

#End Region

    End Class

End Namespace
