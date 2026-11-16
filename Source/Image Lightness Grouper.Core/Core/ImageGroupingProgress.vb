Option Strict On
Option Explicit On
Option Infer Off

Namespace ImageLightnessGrouper.Core

    ''' <summary>
    ''' Immutable progress notification emitted by <see cref="ImageGroupingService"/> through <see cref="IProgress(Of T)"/>.
    ''' </summary>
    Public NotInheritable Class ImageGroupingProgress

#Region " Properties "

        ''' <summary>
        ''' Gets the number of files processed so far (including failed and skipped ones).
        ''' </summary>
        Public ReadOnly Property ProcessedCount As Integer

        ''' <summary>
        ''' Gets the total number of files to process, or zero while the directory is still being scanned.
        ''' </summary>
        Public ReadOnly Property TotalCount As Integer

        ''' <summary>
        ''' Gets the human-readable log message.
        ''' </summary>
        Public ReadOnly Property Message As String

        ''' <summary>
        ''' Gets the severity of <see cref="Message"/>.
        ''' </summary>
        Public ReadOnly Property Severity As LogSeverity

#End Region

#Region " Constructors "

        ''' <summary>
        ''' Initializes a new instance of the <see cref="ImageGroupingProgress"/> class.
        ''' </summary>
        ''' <param name="processedCount">The number of files processed so far.</param>
        ''' <param name="totalCount">The total number of files to process.</param>
        ''' <param name="message">The human-readable log message.</param>
        ''' <param name="severity">The severity of the message.</param>
        ''' <exception cref="ArgumentNullException">Thrown when <paramref name="message"/> is <see langword="Nothing"/>.</exception>
        ''' <exception cref="ArgumentOutOfRangeException">Thrown when the counters are inconsistent.</exception>
        Public Sub New(processedCount As Integer, totalCount As Integer, message As String, severity As LogSeverity)

            If message Is Nothing Then
                Throw New ArgumentNullException(NameOf(message))
            End If

            If totalCount < 0 Then
                Throw New ArgumentOutOfRangeException(NameOf(totalCount), totalCount, "Total count cannot be negative.")
            End If

            If processedCount < 0 OrElse processedCount > totalCount Then
                Throw New ArgumentOutOfRangeException(NameOf(processedCount), processedCount, $"Processed count must be between 0 and {totalCount}.")
            End If

            Me.ProcessedCount = processedCount
            Me.TotalCount = totalCount
            Me.Message = message
            Me.Severity = severity
        End Sub

#End Region

    End Class

End Namespace
