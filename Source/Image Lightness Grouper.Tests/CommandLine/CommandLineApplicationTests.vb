Option Strict On
Option Explicit On
Option Infer Off

#Region " Imports "

Imports System.Drawing
Imports System.IO
Imports System.Threading

Imports ImageLightnessGrouper.CommandLine
Imports ImageLightnessGrouper.Core
Imports ImageLightnessGrouper.Tests.TestInfrastructure

#End Region

Namespace ImageLightnessGrouper.Tests.CommandLine

    ''' <summary>
    ''' End-to-end tests of the command-line application (<see cref="CommandLineApplication"/>), in process, with real images.
    ''' </summary>
    Public NotInheritable Class CommandLineApplicationTests

#Region " Helpers "

        ''' <summary>
        ''' The title written first.
        ''' </summary>
        Private Const Title As String = "Image Lightness Grouper CLI vTest (X64)"

        ''' <summary>
        ''' Runs a command line.
        ''' </summary>
        ''' <param name="writer">The output.</param>
        ''' <param name="currentDirectory">The current directory, used for relative paths.</param>
        ''' <param name="cancellationToken">The cancellation token.</param>
        ''' <param name="args">The command-line arguments.</param>
        ''' <returns>The exit code.</returns>
        Private Shared Function RunCommandLine(writer As RecordingConsoleWriter, currentDirectory As String,
                                               cancellationToken As CancellationToken, ParamArray args As String()) As CommandLineExitCode

            ' The service never queries the real memory status, so the tests also run outside Windows.
            Dim service As New ImageGroupingService(Function(isX86 As Boolean) 64.0R, isX86Process:=False)
            Return CommandLineApplication.Run(args, writer, Title, service, currentDirectory, cancellationToken)
        End Function

        ''' <summary>
        ''' Runs a command line with the test cancellation token.
        ''' </summary>
        ''' <param name="writer">The output.</param>
        ''' <param name="currentDirectory">The current directory.</param>
        ''' <param name="args">The command-line arguments.</param>
        ''' <returns>The exit code.</returns>
        Private Shared Function RunCommandLine(writer As RecordingConsoleWriter, currentDirectory As String, ParamArray args As String()) As CommandLineExitCode
            Return RunCommandLine(writer, currentDirectory, TestContext.Current.CancellationToken, args)
        End Function

        ''' <summary>
        ''' Asserts that the output starts with the header: title, repository address and an empty line.
        ''' </summary>
        ''' <param name="writer">The output.</param>
        Private Shared Sub AssertHeader(writer As RecordingConsoleWriter)
            Assert.True(writer.Output.Count >= 3)
            Assert.Equal(Title, writer.Output(0).Text)
            Assert.Equal(ProductInfo.RepositoryUrl, writer.Output(1).Text)
            Assert.Equal(String.Empty, writer.Output(2).Text)
        End Sub

        ''' <summary>
        ''' Creates a solid gray image.
        ''' </summary>
        ''' <param name="filePath">The file path.</param>
        ''' <param name="gray">The gray level (0-255).</param>
        ''' <returns><paramref name="filePath"/>.</returns>
        Private Shared Function CreateGray(filePath As String, gray As Integer) As String
            Return TestImageFactory.CreateSolidImage(filePath, 4, 4, Color.FromArgb(gray, gray, gray))
        End Function

        ''' <summary>
        ''' Gets the path of a grouped image.
        ''' </summary>
        ''' <param name="directoryPath">The directory of the original image.</param>
        ''' <param name="lightness">The lightness of the image.</param>
        ''' <param name="fileName">The file name.</param>
        ''' <param name="stepPercentage">The interval.</param>
        ''' <returns>The path.</returns>
        Friend Shared Function GroupedPath(directoryPath As String, lightness As Double, fileName As String,
                                           Optional stepPercentage As Integer = LightnessCalculator.DefaultStepPercentage) As String
            Return Path.Combine(directoryPath, ImageGroupingOptions.OutputFolderName, LightnessCalculator.GetFolderName(lightness, stepPercentage), fileName)
        End Function

#End Region

#Region " Header and help "

        ''' <summary>
        ''' No arguments and the help switch show the header and the help, with exit code 0.
        ''' </summary>
        <Fact>
        Public Sub Help_ShowsTheHeaderAndTheHelp()
            For Each args As String() In New String()() {New String() {}, New String() {"/?"}, New String() {"C:\x", "--help"}}
                Dim writer As New RecordingConsoleWriter()

                Assert.Equal(CommandLineExitCode.Success, RunCommandLine(writer, Path.GetTempPath(), args))
                AssertHeader(writer)
                Assert.Equal(CommandLineHelp.GetUsageText(), String.Join(Environment.NewLine, writer.Output.Skip(3).Select(Function(line As LogLine) line.Text)))
                Assert.Empty(writer.Errors)
            Next
        End Sub

        ''' <summary>
        ''' Invalid arguments: the header, then the error and a hint about the help on the error output, with exit code 2.
        ''' </summary>
        <Fact>
        Public Sub InvalidArguments_ShowTheHeaderAndTheError()
            Dim writer As New RecordingConsoleWriter()

            Assert.Equal(CommandLineExitCode.InvalidArguments, RunCommandLine(writer, Path.GetTempPath(), "x", "/step:0"))
            AssertHeader(writer)
            Assert.Equal(3, writer.Output.Count)
            Assert.StartsWith("[ERROR]", writer.Errors(0), StringComparison.Ordinal)
            Assert.Equal(CommandLineApplication.GetHelpHint(), writer.Errors(1))
        End Sub

#End Region

#Region " Success "

        ''' <summary>
        ''' The images are grouped; the output shows the header, every image and the summary; the exit code is 0.
        ''' </summary>
        <Fact>
        Public Sub Group_MovesTheImages_AndReturnsSuccess()
            Using temp As New TemporaryDirectory()
                CreateGray(temp.Combine("black.png"), 0)
                CreateGray(temp.Combine("white.png"), 255)
                Dim writer As New RecordingConsoleWriter()

                Dim exitCode As CommandLineExitCode = RunCommandLine(writer, Path.GetTempPath(), temp.FullPath, "/p:2")

                Assert.Equal(CommandLineExitCode.Success, exitCode)
                Assert.True(File.Exists(GroupedPath(temp.FullPath, 0.0R, "black.png")))
                Assert.True(File.Exists(GroupedPath(temp.FullPath, 100.0R, "white.png")))
                AssertHeader(writer)
                Assert.Contains(writer.Output, Function(line As LogLine) line.Text.StartsWith("[START] ", StringComparison.Ordinal))
                Assert.Contains(writer.Output, Function(line As LogLine) line.Severity = LogSeverity.Moved AndAlso line.Text.Contains("black.png"))
                Assert.Contains(writer.Output, Function(line As LogLine) line.Severity = LogSeverity.Moved AndAlso line.Text.Contains("white.png"))
                Assert.Empty(writer.Errors)

                ' The summary is exactly the shared summary report (the same as in the main window).
                Dim summaryLines As List(Of LogLine) = writer.Output.Skip(writer.Output.Count - 10).ToList()
                Assert.Equal(ImageGroupingReport.Rule, summaryLines(1).Text)
                Assert.Contains(summaryLines, Function(line As LogLine) line.Text.StartsWith("Moved ", StringComparison.Ordinal) AndAlso line.Text.EndsWith(": 2", StringComparison.Ordinal))
                Assert.Equal(LogSeverity.Success, summaryLines.Last().Severity)
            End Using
        End Sub

        ''' <summary>
        ''' Every option reaches the grouping: recursion, interval, alpha threshold and parallelism.
        ''' </summary>
        <Fact>
        Public Sub Group_AppliesEveryOption()
            Using temp As New TemporaryDirectory()
                CreateGray(temp.Combine("sub", "gray.png"), 119)                                                ' L* 50.0
                TestImageFactory.CreateSolidImage(temp.Combine("transparent.png"), 4, 4, Color.FromArgb(100, 255, 255, 255))
                Dim writer As New RecordingConsoleWriter()

                Dim exitCode As CommandLineExitCode =
                    RunCommandLine(writer, Path.GetTempPath(), temp.FullPath, "/r", "/s:3", "/a:101", "/p:1")

                Assert.Equal(CommandLineExitCode.Success, exitCode)
                Assert.True(File.Exists(GroupedPath(temp.Combine("sub"), 50.0R, "gray.png", stepPercentage:=3)))
                ' Every pixel has alpha 100, below the threshold 101: the image counts as fully ignored (lightness 0).
                Assert.True(File.Exists(GroupedPath(temp.FullPath, 0.0R, "transparent.png", stepPercentage:=3)))
                Assert.Contains(writer.Output, Function(line As LogLine) line.Text.Contains("Parallel images: 1 (user defined)"))
            End Using
        End Sub

        ''' <summary>
        ''' A relative directory is resolved against the current directory; a directory without images succeeds.
        ''' </summary>
        <Fact>
        Public Sub Group_RelativeAndEmptyDirectories()
            Using temp As New TemporaryDirectory()
                CreateGray(temp.Combine("images", "white.png"), 255)
                Directory.CreateDirectory(temp.Combine("empty"))

                Assert.Equal(CommandLineExitCode.Success, RunCommandLine(New RecordingConsoleWriter(), temp.FullPath, "images", "/p:1"))
                Assert.True(File.Exists(GroupedPath(temp.Combine("images"), 100.0R, "white.png")))
                Assert.Equal(CommandLineExitCode.Success, RunCommandLine(New RecordingConsoleWriter(), temp.FullPath, "empty", "/p:1"))
            End Using
        End Sub

#End Region

#Region " Problems "

        ''' <summary>
        ''' Failed and conflicting images make the exit code 1, and they stay in place.
        ''' </summary>
        <Fact>
        Public Sub Group_FailuresAndConflicts_ReturnCompletedWithProblems()
            Using temp As New TemporaryDirectory()
                Dim corrupt As String = TestImageFactory.CreateCorruptImage(temp.Combine("corrupt.png"))

                Assert.Equal(CommandLineExitCode.CompletedWithProblems, RunCommandLine(New RecordingConsoleWriter(), Path.GetTempPath(), temp.FullPath, "/p:1"))
                Assert.True(File.Exists(corrupt))
            End Using

            Using temp As New TemporaryDirectory()
                Dim source As String = CreateGray(temp.Combine("same.png"), 255)
                TestImageFactory.CreateTextFile(GroupedPath(temp.FullPath, 100.0R, "same.png"))

                Assert.Equal(CommandLineExitCode.CompletedWithProblems, RunCommandLine(New RecordingConsoleWriter(), Path.GetTempPath(), temp.FullPath, "/p:1"))
                Assert.True(File.Exists(source))
            End Using
        End Sub

        ''' <summary>
        ''' A missing directory, or a file, is reported on the error output with exit code 3, without moving anything.
        ''' </summary>
        <Fact>
        Public Sub Group_InvalidDirectory_ReturnsInvalidDirectory()
            Using temp As New TemporaryDirectory()
                Dim filePath As String = CreateGray(temp.Combine("image.png"), 255)

                For Each target As String In {temp.Combine("missing"), filePath}
                    Dim writer As New RecordingConsoleWriter()

                    Assert.Equal(CommandLineExitCode.InvalidDirectory, RunCommandLine(writer, Path.GetTempPath(), target))
                    AssertHeader(writer)
                    Assert.StartsWith("[ERROR]", writer.Errors.Single(), StringComparison.Ordinal)
                Next

                Assert.True(File.Exists(filePath))
            End Using
        End Sub

        ''' <summary>
        ''' A folder of already grouped images is refused with exit code 4 (the operation itself fails).
        ''' </summary>
        <Fact>
        Public Sub Group_GroupedFolder_ReturnsOperationFailed()
            Using temp As New TemporaryDirectory()
                Dim image As String = CreateGray(GroupedPath(temp.FullPath, 100.0R, "white.png"), 255)
                Dim outputFolder As String = Path.Combine(temp.FullPath, ImageGroupingOptions.OutputFolderName)
                Dim writer As New RecordingConsoleWriter()

                Assert.Equal(CommandLineExitCode.OperationFailed, RunCommandLine(writer, Path.GetTempPath(), outputFolder, "/p:1"))
                Assert.Contains("[ERROR]", writer.ErrorText)
                Assert.True(File.Exists(image))
            End Using
        End Sub

        ''' <summary>
        ''' A cancelled operation (Ctrl+C) returns exit code 5 and moves nothing.
        ''' </summary>
        <Fact>
        Public Sub Group_Cancelled_ReturnsCancelled()
            Using temp As New TemporaryDirectory(),
                  cancellationSource As New CancellationTokenSource()

                Dim image As String = CreateGray(temp.Combine("image.png"), 255)
                cancellationSource.Cancel()
                Dim writer As New RecordingConsoleWriter()

                Assert.Equal(CommandLineExitCode.Cancelled, RunCommandLine(writer, Path.GetTempPath(), cancellationSource.Token, temp.FullPath, "/p:1"))
                Assert.Contains("[CANCELLED]", writer.ErrorText)
                Assert.True(File.Exists(image))
            End Using
        End Sub

        ''' <summary>
        ''' Invalid calls fail fast.
        ''' </summary>
        <Fact>
        Public Sub Run_InvalidCalls_Throw()
            Dim writer As New RecordingConsoleWriter()
            Dim service As New ImageGroupingService(Function(isX86 As Boolean) 64.0R, isX86Process:=False)
            Dim token As CancellationToken = CancellationToken.None

            Assert.Throws(Of ArgumentNullException)(Function() CommandLineApplication.Run(Nothing, writer, Title, service, Path.GetTempPath(), token))
            Assert.Throws(Of ArgumentNullException)(Function() CommandLineApplication.Run({}, Nothing, Title, service, Path.GetTempPath(), token))
            Assert.Throws(Of ArgumentNullException)(Function() CommandLineApplication.Run({}, writer, Title, Nothing, Path.GetTempPath(), token))
            Assert.Throws(Of ArgumentNullException)(Function() CommandLineApplication.Run({}, writer, Title, service, Nothing, token))
            Assert.Throws(Of ArgumentException)(Function() CommandLineApplication.Run({}, writer, " ", service, Path.GetTempPath(), token))
        End Sub

#End Region

    End Class

End Namespace
