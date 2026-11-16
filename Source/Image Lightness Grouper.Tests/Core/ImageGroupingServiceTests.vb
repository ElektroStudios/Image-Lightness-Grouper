Option Strict On
Option Explicit On
Option Infer Off

#Region " Imports "

Imports System.Drawing
Imports System.IO
Imports System.Threading

Imports ImageLightnessGrouper.Core
Imports ImageLightnessGrouper.Infrastructure
Imports ImageLightnessGrouper.Tests.TestInfrastructure

#End Region

Namespace ImageLightnessGrouper.Tests.Core

    ''' <summary>
    ''' Integration tests for <see cref="ImageGroupingService"/>, using real image files in temporary directories.
    ''' </summary>
    Public NotInheritable Class ImageGroupingServiceTests

#Region " Helpers "

        ''' <summary>
        ''' The default output folder name.
        ''' </summary>
        Private Const Output As String = ImageGroupingOptions.OutputFolderName

        ''' <summary>
        ''' Gets the name of the range folder of a lightness value, with the default 10% interval (never hard-coded,
        ''' so the tests follow any change of <see cref="LightnessCalculator.RangeFolderNameFormat"/>).
        ''' </summary>
        ''' <param name="lightness">The lightness value.</param>
        ''' <param name="stepPercentage">The interval.</param>
        ''' <returns>The folder name.</returns>
        Private Shared Function Range(lightness As Double, Optional stepPercentage As Integer = LightnessCalculator.DefaultStepPercentage) As String
            Return LightnessCalculator.GetFolderName(lightness, stepPercentage)
        End Function

        ''' <summary>
        ''' Creates a service that never queries the real memory status (so it also runs outside Windows).
        ''' </summary>
        ''' <param name="availableMemoryGB">The available memory reported to the service.</param>
        ''' <param name="isX86Process">Whether the service behaves as a 32-bit process.</param>
        ''' <returns>The service.</returns>
        Private Shared Function CreateService(Optional availableMemoryGB As Double = 64.0R, Optional isX86Process As Boolean = False) As ImageGroupingService
            Return New ImageGroupingService(Function(isX86 As Boolean) availableMemoryGB, isX86Process)
        End Function

        ''' <summary>
        ''' Runs the service and returns its summary, collecting every progress report.
        ''' </summary>
        ''' <param name="options">The options.</param>
        ''' <param name="reports">Receives every progress report, in arrival order.</param>
        ''' <param name="service">The service (a default test service when omitted).</param>
        ''' <returns>The summary.</returns>
        Private Shared Async Function RunAsync(options As ImageGroupingOptions, reports As List(Of ImageGroupingProgress),
                                               Optional service As ImageGroupingService = Nothing) As Task(Of ImageGroupingSummary)

            Dim progress As New BufferedProgress(Of ImageGroupingProgress)()
            Dim summary As ImageGroupingSummary =
                Await If(service, CreateService()).RunAsync(options, progress, TestContext.Current.CancellationToken).ConfigureAwait(False)

            progress.DrainTo(reports)
            Return summary
        End Function

        ''' <summary>
        ''' Creates options with the defaults (10% interval, alpha threshold 8, 4 parallel images).
        ''' </summary>
        ''' <param name="directoryPath">The target directory.</param>
        ''' <param name="recursive">Whether to include the subdirectories.</param>
        ''' <param name="stepPercentage">The interval.</param>
        ''' <param name="maxDegreeOfParallelism">The requested degree of parallelism.</param>
        ''' <returns>The options.</returns>
        Private Shared Function CreateOptions(directoryPath As String, Optional recursive As Boolean = False,
                                              Optional stepPercentage As Integer = 10,
                                              Optional maxDegreeOfParallelism As Integer = 4) As ImageGroupingOptions

            Return New ImageGroupingOptions(directoryPath, recursive, stepPercentage, LightnessCalculator.DefaultAlphaThreshold, maxDegreeOfParallelism)
        End Function

        ''' <summary>
        ''' Creates a solid gray image whose lightness is about the specified percentage.
        ''' </summary>
        ''' <param name="filePath">The file path.</param>
        ''' <param name="gray">The gray level (0-255).</param>
        ''' <returns><paramref name="filePath"/>.</returns>
        Private Shared Function CreateGray(filePath As String, gray As Integer) As String
            Return TestImageFactory.CreateSolidImage(filePath, 4, 4, Color.FromArgb(gray, gray, gray))
        End Function

#End Region

#Region " Grouping "

        ''' <summary>
        ''' Every image ends up in the range folder of its lightness, inside the output folder, and the summary adds up.
        ''' </summary>
        <Fact>
        Public Async Function RunAsync_MovesEachImageToItsLightnessFolder() As Task
            Using temp As New TemporaryDirectory()
                CreateGray(temp.Combine("black.png"), 0)          ' L* 0
                CreateGray(temp.Combine("gray.bmp"), 119)         ' L* 50.0
                CreateGray(temp.Combine("white.tif"), 255)        ' L* 100
                CreateGray(temp.Combine("dark.webp"), 40)         ' L* 16.8
                Dim options As ImageGroupingOptions = CreateOptions(temp.FullPath)
                Dim reports As New List(Of ImageGroupingProgress)()

                Dim summary As ImageGroupingSummary = Await RunAsync(options, reports).ConfigureAwait(False)

                Assert.True(File.Exists(temp.Combine(Output, Range(0), "black.png")))
                Assert.True(File.Exists(temp.Combine(Output, Range(50), "gray.bmp")))
                Assert.True(File.Exists(temp.Combine(Output, Range(100), "white.tif")))
                Assert.True(File.Exists(temp.Combine(Output, Range(10), "dark.webp")))
                Assert.Empty(Directory.GetFiles(temp.FullPath))

                Assert.Equal(4, summary.TotalFiles)
                Assert.Equal(4, summary.MovedCount)
                Assert.Equal(0, summary.ConflictCount)
                Assert.Equal(0, summary.FailedCount)
                Assert.Equal(4, summary.DegreeOfParallelism)
                Assert.Contains(reports, Function(r As ImageGroupingProgress) r.Severity = LogSeverity.Moved AndAlso r.Message.Contains("gray.bmp") AndAlso r.Message.Contains("L*=50.0"))
            End Using
        End Function

        ''' <summary>
        ''' The interval drives the range folders.
        ''' </summary>
        <Fact>
        Public Async Function RunAsync_CustomStep_UsesNarrowerRanges() As Task
            Using temp As New TemporaryDirectory()
                CreateGray(temp.Combine("gray.png"), 119)          ' L* 50.0

                Await RunAsync(CreateOptions(temp.FullPath, stepPercentage:=3), New List(Of ImageGroupingProgress)()).ConfigureAwait(False)

                Assert.True(File.Exists(temp.Combine(Output, Range(50, 3), "gray.png")))
            End Using
        End Function

        ''' <summary>
        ''' Progress reports cover every file exactly once, with consistent counters.
        ''' </summary>
        <Fact>
        Public Async Function RunAsync_ReportsEveryFileOnce() As Task
            Using temp As New TemporaryDirectory()
                For index As Integer = 1 To 12
                    CreateGray(temp.Combine($"image{index:D2}.png"), index * 20)
                Next

                Dim reports As New List(Of ImageGroupingProgress)()
                Dim summary As ImageGroupingSummary = Await RunAsync(CreateOptions(temp.FullPath), reports).ConfigureAwait(False)

                Dim fileReports As List(Of ImageGroupingProgress) = reports.Where(Function(r As ImageGroupingProgress) r.ProcessedCount > 0).ToList()
                Assert.Equal(12, fileReports.Count)
                Assert.Equal(Enumerable.Range(1, 12), fileReports.Select(Function(r As ImageGroupingProgress) r.ProcessedCount).OrderBy(Function(n As Integer) n))
                Assert.All(fileReports, Sub(r As ImageGroupingProgress) Assert.Equal(summary.TotalFiles, r.TotalCount))
            End Using
        End Function

#End Region

#Region " Directories "

        ''' <summary>
        ''' Without recursion, images in subdirectories are left untouched.
        ''' </summary>
        <Fact>
        Public Async Function RunAsync_NonRecursive_IgnoresSubdirectories() As Task
            Using temp As New TemporaryDirectory()
                CreateGray(temp.Combine("top.png"), 255)
                Dim nested As String = CreateGray(temp.Combine("sub", "nested.png"), 255)

                Dim summary As ImageGroupingSummary =
                    Await RunAsync(CreateOptions(temp.FullPath, recursive:=False), New List(Of ImageGroupingProgress)()).ConfigureAwait(False)

                Assert.Equal(1, summary.TotalFiles)
                Assert.True(File.Exists(nested))
            End Using
        End Function

        ''' <summary>
        ''' With recursion, each directory gets its own output folder: images of different directories are never mixed,
        ''' so equal file names in different directories do not conflict.
        ''' </summary>
        <Fact>
        Public Async Function RunAsync_Recursive_GroupsInsideEachDirectory() As Task
            Using temp As New TemporaryDirectory()
                CreateGray(temp.Combine("same.png"), 255)
                CreateGray(temp.Combine("a", "same.png"), 255)
                CreateGray(temp.Combine("a", "b", "deep.png"), 0)

                Dim summary As ImageGroupingSummary =
                    Await RunAsync(CreateOptions(temp.FullPath, recursive:=True), New List(Of ImageGroupingProgress)()).ConfigureAwait(False)

                Assert.Equal(3, summary.MovedCount)
                Assert.Equal(0, summary.ConflictCount)
                Assert.True(File.Exists(temp.Combine(Output, Range(100), "same.png")))
                Assert.True(File.Exists(temp.Combine("a", Output, Range(100), "same.png")))
                Assert.True(File.Exists(temp.Combine("a", "b", Output, Range(0), "deep.png")))
            End Using
        End Function

        ''' <summary>
        ''' The output folders of a previous run are never scanned: a second run finds nothing to group
        ''' and never nests output folders.
        ''' </summary>
        <Fact>
        Public Async Function RunAsync_SecondRun_DoesNotRegroupTheOutputFolders() As Task
            Using temp As New TemporaryDirectory()
                CreateGray(temp.Combine("one.png"), 0)
                CreateGray(temp.Combine("sub", "two.png"), 255)
                Dim options As ImageGroupingOptions = CreateOptions(temp.FullPath, recursive:=True)

                Await RunAsync(options, New List(Of ImageGroupingProgress)()).ConfigureAwait(False)
                Dim second As ImageGroupingSummary = Await RunAsync(options, New List(Of ImageGroupingProgress)()).ConfigureAwait(False)

                Assert.Equal(0, second.TotalFiles)
                Assert.True(File.Exists(temp.Combine(Output, Range(0), "one.png")))
                Assert.True(File.Exists(temp.Combine("sub", Output, Range(100), "two.png")))
                Assert.Empty(Directory.GetDirectories(temp.FullPath, Output, SearchOption.AllDirectories).
                             Where(Function(directoryPath As String) directoryPath.Contains($"{Output}{Path.DirectorySeparatorChar}")))
            End Using
        End Function

        ''' <summary>
        ''' Selecting an output folder, a range folder (even inside a renamed output folder) or a folder inside one,
        ''' fails with a clear error, without moving anything.
        ''' </summary>
        <Fact>
        Public Async Function RunAsync_TargetInsideOutputFolder_Throws() As Task
            Using temp As New TemporaryDirectory()
                Dim image As String = CreateGray(temp.Combine(Output, Range(100), "white.png"), 255)
                Dim service As ImageGroupingService = CreateService()

                Dim renamedRange As String = temp.Combine("Renamed", Range(0))
                Directory.CreateDirectory(temp.Combine(renamedRange, "inner"))

                For Each target As String In {temp.Combine(Output), temp.Combine(Output, Range(100)), renamedRange, temp.Combine(renamedRange, "inner")}
                    Await Assert.ThrowsAsync(Of InvalidOperationException)(
                        Function() service.RunAsync(CreateOptions(target), New BufferedProgress(Of ImageGroupingProgress)(), TestContext.Current.CancellationToken)).ConfigureAwait(False)
                Next

                Assert.True(File.Exists(image))
            End Using
        End Function

        ''' <summary>
        ''' Regression: after renaming the output folder, its range folders are still recognized as already grouped
        ''' and never scanned again, so their images are not moved into nested folders.
        ''' </summary>
        <Fact>
        Public Async Function RunAsync_RenamedOutputFolder_IsNotRegrouped() As Task
            Using temp As New TemporaryDirectory()
                CreateGray(temp.Combine("one.png"), 0)
                CreateGray(temp.Combine("two.png"), 255)
                Dim options As ImageGroupingOptions = CreateOptions(temp.FullPath, recursive:=True)
                Await RunAsync(options, New List(Of ImageGroupingProgress)()).ConfigureAwait(False)
                Directory.Move(temp.Combine(Output), temp.Combine("My grouped images"))
                Dim reports As New List(Of ImageGroupingProgress)()

                Dim second As ImageGroupingSummary = Await RunAsync(options, reports).ConfigureAwait(False)

                Assert.Equal(0, second.TotalFiles)
                Assert.True(File.Exists(temp.Combine("My grouped images", Range(0), "one.png")))
                Assert.True(File.Exists(temp.Combine("My grouped images", Range(100), "two.png")))
                Assert.False(Directory.Exists(temp.Combine("My grouped images", Output)))
                Assert.False(Directory.Exists(temp.Combine("My grouped images", Range(0), Output)))
                Assert.Contains(reports, Function(r As ImageGroupingProgress) r.Severity = LogSeverity.Skipped AndAlso r.Message.Contains(Range(0)))
            End Using
        End Function

        ''' <summary>
        ''' Range folders created by any interval are skipped, wherever they are (not only inside the output folder).
        ''' </summary>
        <Fact>
        Public Async Function RunAsync_RangeFoldersOfAnyStep_AreNotScanned() As Task
            Using temp As New TemporaryDirectory()
                Dim grouped As String = CreateGray(temp.Combine("loose", Range(48, 3), "grouped.png"), 119)
                CreateGray(temp.Combine("loose", "new.png"), 119)

                Dim summary As ImageGroupingSummary =
                    Await RunAsync(CreateOptions(temp.FullPath, recursive:=True), New List(Of ImageGroupingProgress)()).ConfigureAwait(False)

                Assert.Equal(1, summary.TotalFiles)
                Assert.True(File.Exists(grouped))
                Assert.True(File.Exists(temp.Combine("loose", Output, Range(50), "new.png")))
            End Using
        End Function

        ''' <summary>
        ''' Directory links (junctions, symbolic links) are reported and never followed, so a link that points
        ''' back to an ancestor cannot cause an infinite loop nor process the same images twice.
        ''' <para></para>
        ''' The link is a junction on Windows (no elevation required) and a symbolic link on Unix.
        ''' </summary>
        <Fact>
        Public Async Function RunAsync_DoesNotFollowDirectoryLinks() As Task
            Using temp As New TemporaryDirectory()
                CreateGray(temp.Combine("real", "image.png"), 255)
                TestEnvironment.CreateDirectoryLink(temp.Combine("real", "loop"), temp.FullPath)
                Dim reports As New List(Of ImageGroupingProgress)()

                Dim summary As ImageGroupingSummary = Await RunAsync(CreateOptions(temp.FullPath, recursive:=True), reports).ConfigureAwait(False)

                Assert.Equal(1, summary.TotalFiles)
                Assert.Equal(1, summary.MovedCount)
                Assert.Contains(reports, Function(r As ImageGroupingProgress) r.Severity = LogSeverity.Skipped AndAlso r.Message.Contains("loop"))
            End Using
        End Function

#End Region

#Region " Files "

        ''' <summary>
        ''' Only the exact supported extensions are processed (no 8.3 wildcard anomalies like ".jpg2"), case-insensitively.
        ''' </summary>
        <Fact>
        Public Async Function RunAsync_ProcessesOnlySupportedExtensions() As Task
            Using temp As New TemporaryDirectory()
                CreateGray(temp.Combine("upper.PNG"), 255)
                Dim notImage As String = TestImageFactory.CreateTextFile(temp.Combine("notes.txt"))
                Dim lookalike As String = TestImageFactory.CreateTextFile(temp.Combine("photo.jpg2"))
                Dim gif As String = TestImageFactory.CreateTextFile(temp.Combine("animation.gif"))

                Dim summary As ImageGroupingSummary =
                    Await RunAsync(CreateOptions(temp.FullPath), New List(Of ImageGroupingProgress)()).ConfigureAwait(False)

                Assert.Equal(1, summary.TotalFiles)
                Assert.True(File.Exists(notImage))
                Assert.True(File.Exists(lookalike))
                Assert.True(File.Exists(gif))
            End Using
        End Function

        ''' <summary>
        ''' Corrupt images are counted and reported as errors, they stay in place, and they do not stop the other images.
        ''' </summary>
        <Fact>
        Public Async Function RunAsync_CorruptImages_AreReportedAndDoNotAbortTheBatch() As Task
            Using temp As New TemporaryDirectory()
                Dim corruptJpeg As String = TestImageFactory.CreateCorruptImage(temp.Combine("corrupt.jpg"))
                Dim corruptWebP As String = TestImageFactory.CreateCorruptImage(temp.Combine("corrupt.webp"))
                CreateGray(temp.Combine("good.png"), 255)
                Dim reports As New List(Of ImageGroupingProgress)()

                Dim summary As ImageGroupingSummary = Await RunAsync(CreateOptions(temp.FullPath), reports).ConfigureAwait(False)

                Assert.Equal(3, summary.TotalFiles)
                Assert.Equal(2, summary.FailedCount)
                Assert.Equal(1, summary.MovedCount)
                Assert.True(File.Exists(corruptJpeg))
                Assert.True(File.Exists(corruptWebP))
                Assert.Single(reports, Function(r As ImageGroupingProgress) r.Severity = LogSeverity.Error AndAlso r.Message.Contains("corrupt.jpg") AndAlso r.Message.Contains("corrupt file or unsupported image format"))
                Assert.Single(reports, Function(r As ImageGroupingProgress) r.Severity = LogSeverity.Error AndAlso r.Message.Contains("corrupt.webp"))
            End Using
        End Function

        ''' <summary>
        ''' An existing file with the same name in the destination folder is never overwritten:
        ''' the image stays in place and is reported as a conflict.
        ''' </summary>
        <Fact>
        Public Async Function RunAsync_NameConflict_IsReportedAndNothingIsOverwritten() As Task
            Using temp As New TemporaryDirectory()
                Dim source As String = CreateGray(temp.Combine("same.png"), 255)
                Dim existing As String = TestImageFactory.CreateTextFile(temp.Combine(Output, Range(100), "same.png"))
                Dim reports As New List(Of ImageGroupingProgress)()

                Dim summary As ImageGroupingSummary = Await RunAsync(CreateOptions(temp.FullPath), reports).ConfigureAwait(False)

                Assert.Equal(1, summary.TotalFiles)
                Assert.Equal(1, summary.ConflictCount)
                Assert.Equal(0, summary.MovedCount)
                Assert.True(File.Exists(source))
                Assert.Equal("text", File.ReadAllText(existing))
                Assert.Single(reports, Function(r As ImageGroupingProgress) r.Severity = LogSeverity.Warning AndAlso r.Message.Contains("[CONFLICT]"))
            End Using
        End Function

        ''' <summary>
        ''' The files are processed in natural order (image2 before image10) when a single image is processed at a time.
        ''' </summary>
        <Fact>
        Public Async Function RunAsync_SingleThread_ProcessesInNaturalOrder() As Task
            Using temp As New TemporaryDirectory()
                For Each name As String In {"image10.png", "image2.png", "image1.png"}
                    CreateGray(temp.Combine(name), 255)
                Next

                Dim reports As New List(Of ImageGroupingProgress)()
                Await RunAsync(CreateOptions(temp.FullPath, maxDegreeOfParallelism:=1), reports).ConfigureAwait(False)

                Dim order As List(Of String) = reports.Where(Function(r As ImageGroupingProgress) r.ProcessedCount > 0).
                                                       OrderBy(Function(r As ImageGroupingProgress) r.ProcessedCount).
                                                       Select(Function(r As ImageGroupingProgress) r.Message.Substring(r.Message.IndexOf("'"c) + 1, r.Message.IndexOf(".png", StringComparison.Ordinal) - r.Message.IndexOf("'"c) + 3)).
                                                       ToList()
                Assert.Equal(New String() {"image1.png", "image2.png", "image10.png"}, order)
            End Using
        End Function

#End Region

#Region " Parallelism "

        ''' <summary>
        ''' The automatic mode uses the available memory (1 GB per image), capped by the processor count.
        ''' </summary>
        <Fact>
        Public Async Function RunAsync_AutomaticParallelism_UsesTheAvailableMemory() As Task
            Using temp As New TemporaryDirectory()
                CreateGray(temp.Combine("image.png"), 255)
                Dim reports As New List(Of ImageGroupingProgress)()

                Dim summary As ImageGroupingSummary =
                    Await RunAsync(CreateOptions(temp.FullPath, maxDegreeOfParallelism:=ParallelismCalculator.Automatic), reports,
                                   CreateService(availableMemoryGB:=1.5R)).ConfigureAwait(False)

                Assert.Equal(1, summary.DegreeOfParallelism)
                Assert.Contains(reports, Function(r As ImageGroupingProgress) r.Message.Contains("Available memory: 1.50 GB") AndAlso r.Message.Contains("(automatic)"))
            End Using
        End Function

        ''' <summary>
        ''' An explicit value is used as is, without querying the memory.
        ''' </summary>
        <Fact>
        Public Async Function RunAsync_ExplicitParallelism_IsUsed() As Task
            Using temp As New TemporaryDirectory()
                CreateGray(temp.Combine("image.png"), 255)
                Dim memoryQueried As Boolean = False
                Dim service As New ImageGroupingService(Function(isX86 As Boolean)
                                                            memoryQueried = True
                                                            Return 64.0R
                                                        End Function, isX86Process:=False)

                Dim summary As ImageGroupingSummary =
                    Await RunAsync(CreateOptions(temp.FullPath, maxDegreeOfParallelism:=7), New List(Of ImageGroupingProgress)(), service).ConfigureAwait(False)

                Assert.Equal(7, summary.DegreeOfParallelism)
                Assert.False(memoryQueried)
            End Using
        End Function

        ''' <summary>
        ''' A 32-bit process always decodes a single image at a time, ignores an explicit value, and warns about it.
        ''' </summary>
        <Fact>
        Public Async Function RunAsync_X86_UsesASingleThreadAndWarns() As Task
            Using temp As New TemporaryDirectory()
                CreateGray(temp.Combine("image.png"), 255)
                Dim reports As New List(Of ImageGroupingProgress)()

                Dim summary As ImageGroupingSummary =
                    Await RunAsync(CreateOptions(temp.FullPath, maxDegreeOfParallelism:=8), reports,
                                   CreateService(availableMemoryGB:=1.0R, isX86Process:=True)).ConfigureAwait(False)

                Assert.Equal(1, summary.DegreeOfParallelism)
                Assert.Equal(1, summary.MovedCount)
                Assert.Contains(reports, Function(r As ImageGroupingProgress) r.Severity = LogSeverity.Warning AndAlso r.Message.Contains("32-bit") AndAlso r.Message.Contains("requested 8"))
            End Using
        End Function

        ''' <summary>
        ''' A large batch is fully processed in parallel, with consistent counters.
        ''' </summary>
        <Fact>
        Public Async Function RunAsync_LargeBatch_IsConsistent() As Task
            Using temp As New TemporaryDirectory()
                For index As Integer = 1 To 90
                    CreateGray(temp.Combine($"batch{index:D3}.png"), If(index Mod 3 = 0, 0, If(index Mod 3 = 1, 119, 255)))
                Next

                Dim summary As ImageGroupingSummary =
                    Await RunAsync(CreateOptions(temp.FullPath, maxDegreeOfParallelism:=8), New List(Of ImageGroupingProgress)()).ConfigureAwait(False)

                Assert.Equal(90, summary.TotalFiles)
                Assert.Equal(90, summary.MovedCount)
                Assert.Equal(30, Directory.GetFiles(temp.Combine(Output, Range(0))).Length)
                Assert.Equal(30, Directory.GetFiles(temp.Combine(Output, Range(50))).Length)
                Assert.Equal(30, Directory.GetFiles(temp.Combine(Output, Range(100))).Length)
                Assert.Empty(Directory.GetFiles(temp.FullPath))
            End Using
        End Function

#End Region

#Region " Failures "

        ''' <summary>
        ''' An empty directory completes with an all-zero summary and a warning.
        ''' </summary>
        <Fact>
        Public Async Function RunAsync_EmptyDirectory_ReturnsEmptySummary() As Task
            Using temp As New TemporaryDirectory()
                Dim reports As New List(Of ImageGroupingProgress)()

                Dim summary As ImageGroupingSummary = Await RunAsync(CreateOptions(temp.FullPath), reports).ConfigureAwait(False)

                Assert.Equal(0, summary.TotalFiles)
                Assert.Equal(0, summary.DegreeOfParallelism)
                Assert.Contains(reports, Function(r As ImageGroupingProgress) r.Severity = LogSeverity.Warning)
            End Using
        End Function

        ''' <summary>
        ''' A missing directory fails the whole operation.
        ''' </summary>
        <Fact>
        Public Async Function RunAsync_MissingDirectory_Throws() As Task
            Dim missing As String = Path.Combine(Path.GetTempPath(), $"ILG-Missing-{Guid.NewGuid():N}")

            Await Assert.ThrowsAsync(Of DirectoryNotFoundException)(
                Function() CreateService().RunAsync(CreateOptions(missing), New BufferedProgress(Of ImageGroupingProgress)(), TestContext.Current.CancellationToken)).ConfigureAwait(False)
        End Function

        ''' <summary>
        ''' A cancelled token cancels the operation without moving anything.
        ''' </summary>
        <Fact>
        Public Async Function RunAsync_CancelledToken_IsCancelled() As Task
            Using temp As New TemporaryDirectory(),
                  cancellationSource As New CancellationTokenSource()

                Dim image As String = CreateGray(temp.Combine("image.png"), 255)
                cancellationSource.Cancel()

                Await Assert.ThrowsAnyAsync(Of OperationCanceledException)(
                    Function() CreateService().RunAsync(CreateOptions(temp.FullPath), New BufferedProgress(Of ImageGroupingProgress)(), cancellationSource.Token)).ConfigureAwait(False)

                Assert.True(File.Exists(image))
            End Using
        End Function

        ''' <summary>
        ''' Null arguments fail fast, synchronously.
        ''' </summary>
        <Fact>
        Public Sub NullArguments_Throw()
            Dim service As ImageGroupingService = CreateService()
            Dim options As ImageGroupingOptions = CreateOptions(Path.GetTempPath())

            Assert.Throws(Of ArgumentNullException)(Sub() service.RunAsync(Nothing, New BufferedProgress(Of ImageGroupingProgress)(), CancellationToken.None))
            Assert.Throws(Of ArgumentNullException)(Sub() service.RunAsync(options, Nothing, CancellationToken.None))
            Assert.Throws(Of ArgumentNullException)(Function() New ImageGroupingService(Nothing, False))
        End Sub

#End Region

    End Class

End Namespace
