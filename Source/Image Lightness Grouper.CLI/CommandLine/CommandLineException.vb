Option Strict On
Option Explicit On
Option Infer Off

#Region " Imports "

Imports System.ComponentModel

#End Region

Namespace ImageLightnessGrouper.CommandLine

    ''' <summary>
    ''' The exception thrown when the command-line arguments (or the directory they specify) are not valid.
    ''' <para></para>
    ''' Its message is meant to be shown to the user as is.
    ''' </summary>
    Public NotInheritable Class CommandLineException : Inherits Exception

#Region " Properties "

        ''' <summary>
        ''' Gets the process exit code that corresponds to this error.
        ''' </summary>
        Public ReadOnly Property ExitCode As CommandLineExitCode

#End Region

#Region " Constructors "

        ''' <summary>
        ''' Initializes a new instance of the <see cref="CommandLineException"/> class.
        ''' </summary>
        ''' <param name="message">The message shown to the user.</param>
        ''' <param name="exitCode">The process exit code that corresponds to this error.</param>
        ''' <exception cref="InvalidEnumArgumentException">Thrown when <paramref name="exitCode"/> is not a defined value.</exception>
        Public Sub New(message As String, exitCode As CommandLineExitCode)
            Me.New(message, exitCode, Nothing)
        End Sub

        ''' <summary>
        ''' Initializes a new instance of the <see cref="CommandLineException"/> class.
        ''' </summary>
        ''' <param name="message">The message shown to the user.</param>
        ''' <param name="exitCode">The process exit code that corresponds to this error.</param>
        ''' <param name="innerException">The exception that caused this error.</param>
        ''' <exception cref="InvalidEnumArgumentException">Thrown when <paramref name="exitCode"/> is not a defined value.</exception>
        Public Sub New(message As String, exitCode As CommandLineExitCode, innerException As Exception)
            MyBase.New(message, innerException)

            If Not [Enum].IsDefined(GetType(CommandLineExitCode), exitCode) Then
                Throw New InvalidEnumArgumentException(NameOf(exitCode), CInt(exitCode), GetType(CommandLineExitCode))
            End If

            Me.ExitCode = exitCode
        End Sub

#End Region

    End Class

End Namespace
