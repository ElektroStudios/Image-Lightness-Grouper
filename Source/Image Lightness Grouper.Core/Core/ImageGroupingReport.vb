Option Strict On
Option Explicit On
Option Infer Off

#Region " Imports "

Imports System.Collections.ObjectModel
Imports System.Globalization

#End Region

Namespace ImageLightnessGrouper.Core

    ''' <summary>
    ''' Builds the human-readable report of an <see cref="ImageGroupingSummary"/>, shared by the main window and the
    ''' command line, so both always show exactly the same summary.
    ''' </summary>
    Public NotInheritable Class ImageGroupingReport

#Region " Constants and Read-only Fields "

        ''' <summary>
        ''' The horizontal rule that separates the sections of the log.
        ''' </summary>
        Public Const Rule As String = "====================================================================="

        ''' <summary>
        ''' The culture used for numeric formatting (thousands separators, etc).
        ''' </summary>
        Private Shared ReadOnly FormatCulture As CultureInfo = CultureInfo.GetCultureInfo("en-US")

#End Region

#Region " Constructors "

        ''' <summary>
        ''' Prevents a default instance of the <see cref="ImageGroupingReport"/> class from being created.
        ''' </summary>
        Private Sub New()
        End Sub

#End Region

#Region " Public Methods "

        ''' <summary>
        ''' Formats an elapsed time (e.g. <c>"00:01:23.456"</c>).
        ''' </summary>
        ''' <param name="elapsed">The elapsed time.</param>
        ''' <returns>The formatted time.</returns>
        Public Shared Function FormatElapsed(elapsed As TimeSpan) As String
            Return elapsed.ToString("hh\:mm\:ss\.fff", ImageGroupingReport.FormatCulture)
        End Function

        ''' <summary>
        ''' Formats a count with thousands separators.
        ''' </summary>
        ''' <param name="count">The count.</param>
        ''' <returns>The formatted count.</returns>
        Public Shared Function FormatCount(count As Integer) As String
            Return count.ToString("N0", ImageGroupingReport.FormatCulture)
        End Function

        ''' <summary>
        ''' Gets the lines of the final summary of a run: the counters, and a final verdict line.
        ''' </summary>
        ''' <param name="summary">The summary.</param>
        ''' <returns>The lines, in display order.</returns>
        ''' <exception cref="ArgumentNullException">Thrown when <paramref name="summary"/> is <see langword="Nothing"/>.</exception>
        Public Shared Function GetSummaryLines(summary As ImageGroupingSummary) As ReadOnlyCollection(Of LogLine)
            If summary Is Nothing Then
                Throw New ArgumentNullException(NameOf(summary))
            End If

            Dim hasFailures As Boolean = summary.FailedCount > 0
            Dim hasConflicts As Boolean = summary.ConflictCount > 0
            Dim lines As New List(Of LogLine) From {
                New LogLine(String.Empty, LogSeverity.Information),
                New LogLine(Rule, LogSeverity.Muted),
                New LogLine($"Total images      : {FormatCount(summary.TotalFiles)}", LogSeverity.Information),
                New LogLine($"Moved             : {FormatCount(summary.MovedCount)}", LogSeverity.Moved),
                New LogLine($"Name conflicts    : {FormatCount(summary.ConflictCount)}", If(hasConflicts, LogSeverity.Warning, LogSeverity.Information)),
                New LogLine($"Failed            : {FormatCount(summary.FailedCount)}", If(hasFailures, LogSeverity.Error, LogSeverity.Information)),
                New LogLine($"Parallel images   : {FormatCount(summary.DegreeOfParallelism)}", LogSeverity.Information),
                New LogLine($"Elapsed time      : {FormatElapsed(summary.Elapsed)}", LogSeverity.Information),
                New LogLine(Rule, LogSeverity.Muted)
            }

            If hasFailures Then
                lines.Add(New LogLine($"[WARNING] Operation completed with {FormatCount(summary.FailedCount)} failure(s). Review the [ERROR] lines above.", LogSeverity.Error))
            ElseIf hasConflicts Then
                lines.Add(New LogLine($"[WARNING] Operation completed. {FormatCount(summary.ConflictCount)} image(s) were left in place because of a name conflict. Review the [CONFLICT] lines above.", LogSeverity.Warning))
            Else
                lines.Add(New LogLine($"[SUCCESS] Operation completed. Moved {FormatCount(summary.MovedCount)} image(s).", LogSeverity.Success))
            End If

            Return lines.AsReadOnly()
        End Function

#End Region

    End Class

End Namespace
