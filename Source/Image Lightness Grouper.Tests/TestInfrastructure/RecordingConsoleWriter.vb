Option Strict On
Option Explicit On
Option Infer Off

#Region " Imports "

Imports ImageLightnessGrouper.CommandLine
Imports ImageLightnessGrouper.Core

#End Region

Namespace ImageLightnessGrouper.Tests.TestInfrastructure

    ''' <summary>
    ''' A thread-safe <see cref="IConsoleWriter"/> that records every line, for the command-line tests.
    ''' </summary>
    Public NotInheritable Class RecordingConsoleWriter : Implements IConsoleWriter

        ''' <summary>
        ''' The recorded standard output lines.
        ''' </summary>
        Private ReadOnly outputLines As New List(Of LogLine)()

        ''' <summary>
        ''' The recorded standard error lines.
        ''' </summary>
        Private ReadOnly errorLines As New List(Of String)()

        ''' <summary>
        ''' Serializes the concurrent writes.
        ''' </summary>
        Private ReadOnly syncRoot As New Object()

        ''' <summary>
        ''' Gets a snapshot of the standard output lines.
        ''' </summary>
        Public ReadOnly Property Output As List(Of LogLine)
            Get
                SyncLock Me.syncRoot
                    Return New List(Of LogLine)(Me.outputLines)
                End SyncLock
            End Get
        End Property

        ''' <summary>
        ''' Gets a snapshot of the standard error lines.
        ''' </summary>
        Public ReadOnly Property Errors As List(Of String)
            Get
                SyncLock Me.syncRoot
                    Return New List(Of String)(Me.errorLines)
                End SyncLock
            End Get
        End Property

        ''' <summary>
        ''' Gets the whole standard output, as a single text.
        ''' </summary>
        Public ReadOnly Property OutputText As String
            Get
                Return String.Join(Environment.NewLine, Me.Output.Select(Function(line As LogLine) line.Text))
            End Get
        End Property

        ''' <summary>
        ''' Gets the whole standard error output, as a single text.
        ''' </summary>
        Public ReadOnly Property ErrorText As String
            Get
                Return String.Join(Environment.NewLine, Me.Errors)
            End Get
        End Property

        ''' <inheritdoc/>
        Public Sub WriteLine(text As String, severity As LogSeverity) Implements IConsoleWriter.WriteLine
            SyncLock Me.syncRoot
                Me.outputLines.Add(New LogLine(text, severity))
            End SyncLock
        End Sub

        ''' <inheritdoc/>
        Public Sub WriteErrorLine(text As String) Implements IConsoleWriter.WriteErrorLine
            If text Is Nothing Then
                Throw New ArgumentNullException(NameOf(text))
            End If

            SyncLock Me.syncRoot
                Me.errorLines.Add(text)
            End SyncLock
        End Sub

    End Class

End Namespace
