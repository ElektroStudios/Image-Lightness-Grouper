Option Strict On
Option Explicit On
Option Infer Off

#Region " Imports "

Imports System.ComponentModel
Imports System.IO

Imports ImageLightnessGrouper.Core

#End Region

Namespace ImageLightnessGrouper.CommandLine

    ''' <summary>
    ''' A thread-safe <see cref="IConsoleWriter"/> that writes to text writers, optionally coloring the standard output
    ''' lines by severity (only meaningful when the writers are the real console).
    ''' </summary>
    Public NotInheritable Class TextConsoleWriter : Implements IConsoleWriter

#Region " Fields "

        ''' <summary>
        ''' The standard output writer.
        ''' </summary>
        Private ReadOnly output As TextWriter

        ''' <summary>
        ''' The standard error writer.
        ''' </summary>
        Private ReadOnly [error] As TextWriter

        ''' <summary>
        ''' Whether the console text color is changed according to the severity.
        ''' </summary>
        Private ReadOnly useColors As Boolean

        ''' <summary>
        ''' Serializes the writes (and the color changes) of the concurrent worker threads.
        ''' </summary>
        Private ReadOnly syncRoot As New Object()

#End Region

#Region " Constructors "

        ''' <summary>
        ''' Initializes a new instance of the <see cref="TextConsoleWriter"/> class.
        ''' </summary>
        ''' <param name="output">The standard output writer.</param>
        ''' <param name="error">The standard error writer.</param>
        ''' <param name="useColors">
        ''' Whether to color the lines with <see cref="Console.ForegroundColor"/>. Use it only when
        ''' <paramref name="output"/> is the real console (colors are not stored in redirected output).
        ''' </param>
        ''' <exception cref="ArgumentNullException">Thrown when a writer is <see langword="Nothing"/>.</exception>
        Public Sub New(output As TextWriter, [error] As TextWriter, useColors As Boolean)
            If output Is Nothing Then
                Throw New ArgumentNullException(NameOf(output))
            End If

            If [error] Is Nothing Then
                Throw New ArgumentNullException(NameOf([error]))
            End If

            Me.output = output
            Me.error = [error]
            Me.useColors = useColors
        End Sub

#End Region

#Region " IConsoleWriter "

        ''' <inheritdoc/>
        Public Sub WriteLine(text As String, severity As LogSeverity) Implements IConsoleWriter.WriteLine
            If text Is Nothing Then
                Throw New ArgumentNullException(NameOf(text))
            End If

            Dim color As ConsoleColor? = TextConsoleWriter.GetColor(severity)

            SyncLock Me.syncRoot
                If Me.useColors AndAlso color.HasValue Then
                    Console.ForegroundColor = color.Value
                    Try
                        Me.output.WriteLine(text)
                        Me.output.Flush()
                    Finally
                        Console.ResetColor()
                    End Try
                Else
                    Me.output.WriteLine(text)
                    Me.output.Flush()
                End If
            End SyncLock
        End Sub

        ''' <inheritdoc/>
        Public Sub WriteErrorLine(text As String) Implements IConsoleWriter.WriteErrorLine
            If text Is Nothing Then
                Throw New ArgumentNullException(NameOf(text))
            End If

            SyncLock Me.syncRoot
                If Me.useColors Then
                    Console.ForegroundColor = ConsoleColor.Red
                    Try
                        Me.error.WriteLine(text)
                        Me.error.Flush()
                    Finally
                        Console.ResetColor()
                    End Try
                Else
                    Me.error.WriteLine(text)
                    Me.error.Flush()
                End If
            End SyncLock
        End Sub

#End Region

#Region " Friend Methods "

        ''' <summary>
        ''' Gets the console color of a severity.
        ''' </summary>
        ''' <param name="severity">The severity.</param>
        ''' <returns>The color, or <see langword="Nothing"/> to keep the default console color.</returns>
        ''' <exception cref="InvalidEnumArgumentException">Thrown when <paramref name="severity"/> is not a defined value.</exception>
        Friend Shared Function GetColor(severity As LogSeverity) As ConsoleColor?
            Select Case severity
                Case LogSeverity.Information
                    Return Nothing
                Case LogSeverity.Muted
                    Return ConsoleColor.DarkGray
                Case LogSeverity.Moved
                    Return ConsoleColor.Green
                Case LogSeverity.Skipped
                    Return ConsoleColor.DarkCyan
                Case LogSeverity.Success
                    Return ConsoleColor.Green
                Case LogSeverity.Warning
                    Return ConsoleColor.Yellow
                Case LogSeverity.Error
                    Return ConsoleColor.Red
                Case Else
                    Throw New InvalidEnumArgumentException(NameOf(severity), CInt(severity), GetType(LogSeverity))
            End Select
        End Function

#End Region

    End Class

End Namespace
