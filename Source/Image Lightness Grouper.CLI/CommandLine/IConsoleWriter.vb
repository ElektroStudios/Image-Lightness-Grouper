Option Strict On
Option Explicit On
Option Infer Off

#Region " Imports "

Imports ImageLightnessGrouper.Core

#End Region

Namespace ImageLightnessGrouper.CommandLine

    ''' <summary>
    ''' The output of the command-line mode. Implementations must be thread-safe, because the grouping progress is
    ''' reported concurrently from several worker threads.
    ''' </summary>
    Public Interface IConsoleWriter

        ''' <summary>
        ''' Writes a line to the standard output.
        ''' </summary>
        ''' <param name="text">The text.</param>
        ''' <param name="severity">The severity (it may select the text color).</param>
        Sub WriteLine(text As String, severity As LogSeverity)

        ''' <summary>
        ''' Writes a line to the standard error output (usage errors and fatal errors).
        ''' </summary>
        ''' <param name="text">The text.</param>
        Sub WriteErrorLine(text As String)

    End Interface

End Namespace
