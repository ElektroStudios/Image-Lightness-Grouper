Option Strict On
Option Explicit On
Option Infer Off

#Region " Imports "

Imports System.Threading

Imports ImageLightnessGrouper.Infrastructure

#End Region

Namespace ImageLightnessGrouper.Tests.Infrastructure

    ''' <summary>
    ''' Tests for <see cref="BufferedProgress(Of T)"/>.
    ''' </summary>
    Public NotInheritable Class BufferedProgressTests

        ''' <summary>
        ''' Reports are drained in arrival order and the queue is emptied.
        ''' </summary>
        <Fact>
        Public Sub DrainTo_ReturnsReportsInArrivalOrder()
            Dim progress As New BufferedProgress(Of String)()
            Dim sink As IProgress(Of String) = progress
            sink.Report("a")
            sink.Report("b")
            sink.Report("c")

            Dim drained As New List(Of String)()
            Assert.True(progress.HasPendingReports)
            Assert.Equal(3, progress.DrainTo(drained))
            Assert.Equal({"a", "b", "c"}, drained)
            Assert.False(progress.HasPendingReports)
            Assert.Equal(0, progress.DrainTo(drained))
        End Sub

        ''' <summary>
        ''' Concurrent reports from many threads are never lost.
        ''' </summary>
        <Fact>
        Public Sub Report_IsThreadSafe()
            Dim progress As New BufferedProgress(Of String)()
            Dim sink As IProgress(Of String) = progress

            Parallel.For(0, 20000, Sub(index As Integer) sink.Report(index.ToString(Globalization.CultureInfo.InvariantCulture)))

            Dim drained As New List(Of String)()
            Assert.Equal(20000, progress.DrainTo(drained))
            Assert.Equal(20000, drained.Distinct().Count())
        End Sub

        ''' <summary>
        ''' Reporting never blocks nor invokes any callback (it is consumed by polling).
        ''' </summary>
        <Fact>
        Public Sub Report_DoesNotRequireASynchronizationContext()
            Dim progress As New BufferedProgress(Of String)()
            Dim sink As IProgress(Of String) = progress
            Dim reportingThread As New Thread(Sub() sink.Report("from another thread"))

            reportingThread.Start()
            reportingThread.Join()

            Dim drained As New List(Of String)()
            progress.DrainTo(drained)
            Assert.Equal({"from another thread"}, drained)
        End Sub

        ''' <summary>
        ''' Null arguments must fail fast.
        ''' </summary>
        <Fact>
        Public Sub NullArguments_Throw()
            Dim progress As New BufferedProgress(Of String)()
            Dim sink As IProgress(Of String) = progress

            Assert.Throws(Of ArgumentNullException)(Sub() sink.Report(Nothing))
            Assert.Throws(Of ArgumentNullException)(Sub() progress.DrainTo(Nothing))
        End Sub

    End Class

End Namespace
