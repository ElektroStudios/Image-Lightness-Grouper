Option Strict On
Option Explicit On
Option Infer Off

#Region " Imports "

Imports System.Diagnostics
Imports System.Drawing
Imports System.IO
Imports System.Threading.Tasks

Imports ImageLightnessGrouper.CommandLine
Imports ImageLightnessGrouper.Core
Imports ImageLightnessGrouper.Infrastructure
Imports ImageLightnessGrouper.Tests.TestInfrastructure

#End Region

Namespace ImageLightnessGrouper.Tests.CommandLine

    ''' <summary>
    ''' Runs the real command-line executable in its own process, and checks what a user sees in the console:
    ''' the standard output, the standard error output and the exit code.
    ''' </summary>
    Public NotInheritable Class CommandLineProcessTests

#Region " Helpers "

        ''' <summary>
        ''' The result of running the executable.
        ''' </summary>
        Private NotInheritable Class ProcessResult

            ''' <summary>
            ''' Gets or sets the exit code.
            ''' </summary>
            Public Property ExitCode As Integer

            ''' <summary>
            ''' Gets or sets the standard output lines.
            ''' </summary>
            Public Property Output As String()

            ''' <summary>
            ''' Gets or sets the standard error output.
            ''' </summary>
            Public Property [Error] As String

        End Class

        ''' <summary>
        ''' Runs the command-line executable (copied next to the test assembly).
        ''' </summary>
        ''' <param name="workingDirectory">The current directory of the process.</param>
        ''' <param name="args">The arguments.</param>
        ''' <returns>The result.</returns>
        Private Shared Function RunExecutable(workingDirectory As String, ParamArray args As String()) As ProcessResult
            Dim executablePath As String = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ProductInfo.CliExecutableName)
            Assert.True(File.Exists(executablePath), $"The executable was not found: {executablePath}")

            Dim arguments As String = String.Join(" ", args.Select(Function(argument As String) If(argument.Contains(" "), $"""{argument}""", argument)))
            Dim startInfo As ProcessStartInfo =
                If(TestEnvironment.IsMono,
                   New ProcessStartInfo("mono", $"""{executablePath}"" {arguments}"),
                   New ProcessStartInfo(executablePath, arguments))

            startInfo.UseShellExecute = False
            startInfo.CreateNoWindow = True
            startInfo.RedirectStandardOutput = True
            startInfo.RedirectStandardError = True
            startInfo.WorkingDirectory = workingDirectory

            Using process As Process = Process.Start(startInfo)
                Dim errorTask As Task(Of String) = process.StandardError.ReadToEndAsync()
                Dim output As String = process.StandardOutput.ReadToEnd()
                Assert.True(process.WaitForExit(120000), "The executable did not finish in time.")

                Return New ProcessResult() With {
                    .ExitCode = process.ExitCode,
                    .Output = output.Split({Environment.NewLine}, StringSplitOptions.None),
                    .Error = errorTask.GetAwaiter().GetResult()
                }
            End Using
        End Function

        ''' <summary>
        ''' Asserts that the output starts with the title, version and architecture, followed by the repository address.
        ''' </summary>
        ''' <param name="result">The result.</param>
        Private Shared Sub AssertHeader(result As ProcessResult)
            Dim version As Version = GetType(CommandLineApplication).Assembly.GetName().Version
            Assert.Equal($"Image Lightness Grouper CLI v{version} ({UtilApplication.ProcessArchitecture.ToString().ToUpperInvariant()})", result.Output(0))
            Assert.Equal(ProductInfo.RepositoryUrl, result.Output(1))
        End Sub

#End Region

#Region " Tests "

        ''' <summary>
        ''' The executable has the documented file name, and the main window too.
        ''' </summary>
        <Fact>
        Public Sub ExecutableNames_MatchTheDocumentation()
            Assert.Equal(ProductInfo.CliExecutableName, Path.GetFileName(GetType(CommandLineApplication).Assembly.Location))
            Assert.Equal(ProductInfo.GuiExecutableName, Path.GetFileName(GetType(Global.ImageLightnessGrouper.Forms.MainForm).Assembly.Location))
        End Sub

        ''' <summary>
        ''' Without arguments, the header and the help are written to the standard output, with exit code 0.
        ''' </summary>
        <Fact>
        Public Sub NoArguments_WritesTheHeaderAndTheHelp()
            Dim result As ProcessResult = RunExecutable(Path.GetTempPath())

            Assert.Equal(CInt(CommandLineExitCode.Success), result.ExitCode)
            AssertHeader(result)
            Assert.Contains("USAGE:", result.Output)
            Assert.Contains("EXIT CODES:", result.Output)
            Assert.Equal(String.Empty, result.Error)
        End Sub

        ''' <summary>
        ''' Invalid arguments: the header on the standard output, the error on the standard error output, exit code 2.
        ''' </summary>
        <Fact>
        Public Sub InvalidArguments_WritesTheErrorAndExitCode2()
            Dim result As ProcessResult = RunExecutable(Path.GetTempPath(), "/bogus")

            Assert.Equal(CInt(CommandLineExitCode.InvalidArguments), result.ExitCode)
            AssertHeader(result)
            Assert.Contains("[ERROR] Unknown switch: /bogus", result.Error)
        End Sub

        ''' <summary>
        ''' A missing directory: the error on the standard error output, exit code 3.
        ''' </summary>
        <Fact>
        Public Sub MissingDirectory_WritesTheErrorAndExitCode3()
            Using temp As New TemporaryDirectory()
                Dim result As ProcessResult = RunExecutable(temp.FullPath, "missing folder")

                Assert.Equal(CInt(CommandLineExitCode.InvalidDirectory), result.ExitCode)
                AssertHeader(result)
                Assert.Contains("[ERROR] The directory does not exist:", result.Error)
            End Using
        End Sub

        ''' <summary>
        ''' Grouping: every image and the summary are written to the standard output, and the images are moved.
        ''' </summary>
        <Fact>
        Public Sub Group_WritesTheProgressAndTheSummary()
            Using temp As New TemporaryDirectory()
                Dim directoryPath As String = temp.Combine("My Images")
                TestImageFactory.CreateSolidImage(Path.Combine(directoryPath, "white.png"), 4, 4, Color.White)
                TestImageFactory.CreateSolidImage(Path.Combine(directoryPath, "black.png"), 4, 4, Color.Black)

                Dim result As ProcessResult = RunExecutable(temp.FullPath, "My Images", "/parallel:1")

                Assert.Equal(CInt(CommandLineExitCode.Success), result.ExitCode)
                AssertHeader(result)
                Assert.Contains(result.Output, Function(line As String) line.StartsWith("[START] ", StringComparison.Ordinal))
                Assert.Contains(result.Output, Function(line As String) line.Contains("white.png"))
                Assert.Contains(result.Output, Function(line As String) line.Contains("black.png"))
                Assert.Contains("[SUCCESS] Operation completed. Moved 2 image(s).", result.Output)
                Assert.Equal(String.Empty, result.Error)
                Assert.True(File.Exists(CommandLineApplicationTests.GroupedPath(directoryPath, 100.0R, "white.png")))
                Assert.True(File.Exists(CommandLineApplicationTests.GroupedPath(directoryPath, 0.0R, "black.png")))
            End Using
        End Sub

#End Region

    End Class

End Namespace
