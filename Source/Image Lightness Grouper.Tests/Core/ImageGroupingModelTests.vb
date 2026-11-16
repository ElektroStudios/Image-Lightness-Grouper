Option Strict On
Option Explicit On
Option Infer Off

#Region " Imports "

Imports System.IO

Imports ImageLightnessGrouper.Core

#End Region

Namespace ImageLightnessGrouper.Tests.Core

    ''' <summary>
    ''' Tests for the immutable model types: <see cref="ImageGroupingOptions"/>, <see cref="ImageGroupingProgress"/>
    ''' and <see cref="ImageGroupingSummary"/>.
    ''' </summary>
    Public NotInheritable Class ImageGroupingModelTests

#Region " Helpers "

        ''' <summary>
        ''' An absolute directory path that is valid on every platform.
        ''' </summary>
        Private Shared ReadOnly RootedDirectory As String = Path.GetTempPath()

#End Region

#Region " ImageGroupingOptions "

        ''' <summary>
        ''' The options keep every value, and the allowed extensions are the supported ones.
        ''' </summary>
        <Fact>
        Public Sub Options_KeepTheirValues()
            Dim options As New ImageGroupingOptions(RootedDirectory, True, 5, 16, 3)

            Assert.Equal(RootedDirectory, options.TargetDirectory)
            Assert.True(options.SearchRecursively)
            Assert.Equal(5, options.StepPercentage)
            Assert.Equal(16, options.AlphaThreshold)
            Assert.Equal(3, options.MaxDegreeOfParallelism)
            Assert.Equal(ImageLightnessReader.SupportedExtensions, options.AllowedExtensions)
            Assert.Throws(Of NotSupportedException)(Sub() DirectCast(options.AllowedExtensions, IList(Of String)).Add(".gif"))
        End Sub

        ''' <summary>
        ''' Every invalid argument fails fast.
        ''' </summary>
        <Fact>
        Public Sub Options_InvalidArguments_Throw()
            Assert.Throws(Of ArgumentNullException)(Function() New ImageGroupingOptions(Nothing, False, 10, 8, 0))
            Assert.Throws(Of ArgumentException)(Function() New ImageGroupingOptions(" ", False, 10, 8, 0))
            Assert.Throws(Of ArgumentException)(Function() New ImageGroupingOptions("relative", False, 10, 8, 0))
            Assert.Throws(Of ArgumentOutOfRangeException)(Function() New ImageGroupingOptions(RootedDirectory, False, 0, 8, 0))
            Assert.Throws(Of ArgumentOutOfRangeException)(Function() New ImageGroupingOptions(RootedDirectory, False, 11, 8, 0))
            Assert.Throws(Of ArgumentOutOfRangeException)(Function() New ImageGroupingOptions(RootedDirectory, False, 10, -1, 0))
            Assert.Throws(Of ArgumentOutOfRangeException)(Function() New ImageGroupingOptions(RootedDirectory, False, 10, 256, 0))
            Assert.Throws(Of ArgumentOutOfRangeException)(Function() New ImageGroupingOptions(RootedDirectory, False, 10, 8, -1))
            Assert.Throws(Of ArgumentOutOfRangeException)(Function() New ImageGroupingOptions(RootedDirectory, False, 10, 8, ParallelismCalculator.MaximumRequestable + 1))
        End Sub

#End Region

#Region " ImageGroupingProgress "

        ''' <summary>
        ''' A progress report keeps its values.
        ''' </summary>
        <Fact>
        Public Sub Progress_KeepsValues()
            Dim progress As New ImageGroupingProgress(3, 10, "message", LogSeverity.Moved)

            Assert.Equal(3, progress.ProcessedCount)
            Assert.Equal(10, progress.TotalCount)
            Assert.Equal("message", progress.Message)
            Assert.Equal(LogSeverity.Moved, progress.Severity)
        End Sub

        ''' <summary>
        ''' Inconsistent progress reports are rejected.
        ''' </summary>
        <Fact>
        Public Sub Progress_InvalidArguments_Throw()
            Assert.Throws(Of ArgumentNullException)(Function() New ImageGroupingProgress(0, 0, Nothing, LogSeverity.Information))
            Assert.Throws(Of ArgumentOutOfRangeException)(Function() New ImageGroupingProgress(0, -1, "x", LogSeverity.Information))
            Assert.Throws(Of ArgumentOutOfRangeException)(Function() New ImageGroupingProgress(11, 10, "x", LogSeverity.Information))
        End Sub

#End Region

#Region " ImageGroupingSummary "

        ''' <summary>
        ''' The summary keeps its values, and its counters must add up to the total.
        ''' </summary>
        <Fact>
        Public Sub Summary_ValidatesCounters()
            Dim summary As New ImageGroupingSummary(10, 7, 2, 1, 4, TimeSpan.FromSeconds(3))

            Assert.Equal(10, summary.TotalFiles)
            Assert.Equal(7, summary.MovedCount)
            Assert.Equal(2, summary.ConflictCount)
            Assert.Equal(1, summary.FailedCount)
            Assert.Equal(4, summary.DegreeOfParallelism)
            Assert.Equal(TimeSpan.FromSeconds(3), summary.Elapsed)

            Assert.Throws(Of ArgumentOutOfRangeException)(Function() New ImageGroupingSummary(10, 7, 2, 0, 4, TimeSpan.Zero))
            Assert.Throws(Of ArgumentOutOfRangeException)(Function() New ImageGroupingSummary(1, -1, 2, 0, 4, TimeSpan.Zero))
        End Sub

#End Region

    End Class

End Namespace
