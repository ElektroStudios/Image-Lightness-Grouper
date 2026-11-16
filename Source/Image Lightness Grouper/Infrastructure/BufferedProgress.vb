Option Strict On
Option Explicit On
Option Infer Off

#Region " Imports "

Imports System.Collections.Concurrent

#End Region

Namespace ImageLightnessGrouper.Infrastructure

    ''' <summary>
    ''' A thread-safe, non-marshaling <see cref="IProgress(Of T)"/> implementation: reports are only queued,
    ''' and the consumer (typically a UI timer) drains them in batches at its own pace.
    ''' </summary>
    ''' <remarks>
    ''' <see cref="Progress(Of T)"/> posts one message to the UI thread per report. When thousands of reports
    ''' per second arrive from parallel workers, those posted messages starve the lower-priority
    ''' <c>WM_PAINT</c>, input and <c>WM_TIMER</c> messages and the window stops responding.
    ''' This class never touches the UI thread, so the UI stays fully responsive whatever the report rate.
    ''' </remarks>
    ''' <typeparam name="T">The type of the progress report.</typeparam>
    Public NotInheritable Class BufferedProgress(Of T As Class) : Implements IProgress(Of T)

#Region " Read-only Fields "

        ''' <summary>
        ''' The pending reports.
        ''' </summary>
        Private ReadOnly pendingReports As New ConcurrentQueue(Of T)()

#End Region

#Region " Properties "

        ''' <summary>
        ''' Gets a value indicating whether there are pending reports.
        ''' </summary>
        Public ReadOnly Property HasPendingReports As Boolean
            Get
                Return Not Me.pendingReports.IsEmpty
            End Get
        End Property

#End Region

#Region " Public Methods "

        ''' <summary>
        ''' Queues a progress report. This method is thread-safe and never blocks.
        ''' </summary>
        ''' <param name="value">The progress report.</param>
        ''' <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="Nothing"/>.</exception>
        Public Sub Report(value As T) Implements IProgress(Of T).Report
            If value Is Nothing Then
                Throw New ArgumentNullException(NameOf(value))
            End If

            Me.pendingReports.Enqueue(value)
        End Sub

        ''' <summary>
        ''' Moves every pending report, in arrival order, into the specified list.
        ''' </summary>
        ''' <param name="target">The list that receives the reports.</param>
        ''' <returns>The number of reports drained.</returns>
        ''' <exception cref="ArgumentNullException">Thrown when <paramref name="target"/> is <see langword="Nothing"/>.</exception>
        Public Function DrainTo(target As List(Of T)) As Integer
            If target Is Nothing Then
                Throw New ArgumentNullException(NameOf(target))
            End If

            Dim count As Integer = 0
            Dim item As T = Nothing
            While Me.pendingReports.TryDequeue(item)
                target.Add(item)
                count += 1
            End While

            Return count
        End Function

#End Region

    End Class

End Namespace
