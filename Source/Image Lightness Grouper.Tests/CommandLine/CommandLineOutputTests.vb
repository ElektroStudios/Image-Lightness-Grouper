Option Strict On
Option Explicit On
Option Infer Off

#Region " Imports "

Imports System.IO
Imports System.Threading.Tasks

Imports ImageLightnessGrouper.CommandLine
Imports ImageLightnessGrouper.Core
Imports ImageLightnessGrouper.Tests.TestInfrastructure

#End Region

Namespace ImageLightnessGrouper.Tests.CommandLine

    ''' <summary>
    ''' Tests for the command-line help, the exit codes, the console output and the shared summary report.
    ''' </summary>
    Public NotInheritable Class CommandLineOutputTests

#Region " Help "

        ''' <summary>
        ''' Gets the help lines.
        ''' </summary>
        ''' <returns>The lines.</returns>
        Private Shared Function GetUsageLines() As String()
            Return CommandLineHelp.GetUsageText().Split({Environment.NewLine}, StringSplitOptions.None)
        End Function

        ''' <summary>
        ''' The help fits in a default 80-column console.
        ''' </summary>
        <Fact>
        Public Sub Help_FitsIn80Columns()
            For Each line As String In GetUsageLines()
                Assert.True(line.Length <= CommandLineHelp.MaximumLineLength, $"Line too long ({line.Length}): {line}")
            Next
        End Sub

        ''' <summary>
        ''' The help explains how the program works, and documents every switch, the defaults and ranges of the code,
        ''' the supported formats, the main window and every exit code with its value.
        ''' </summary>
        <Fact>
        Public Sub Help_DocumentsEverything()
            Dim usage As String = CommandLineHelp.GetUsageText()

            For Each section As String In {"DESCRIPTION:", "USAGE:", "OPTIONS:", "EXIT CODES:", "MAIN WINDOW:", "EXAMPLES:"}
                Assert.Contains(section, GetUsageLines())
            Next

            For Each switchName As String In {CommandLineSyntax.RecursiveSwitch, CommandLineSyntax.StepSwitch,
                                              CommandLineSyntax.AlphaSwitch, CommandLineSyntax.ParallelSwitch}
                Assert.Contains($"/{switchName}", usage)
                Assert.Contains($"/{switchName.Substring(0, 1)}", usage)
            Next

            Assert.Contains("/?", usage)
            Assert.Contains($"from {LightnessCalculator.MinimumStepPercentage} to {LightnessCalculator.MaximumStepPercentage}", usage)
            Assert.Contains($"Default: {LightnessCalculator.DefaultStepPercentage}.", usage)
            Assert.Contains($"Default: {LightnessCalculator.DefaultAlphaThreshold}.", usage)
            Assert.Contains($"from {ParallelismCalculator.Automatic} to {ParallelismCalculator.MaximumRequestable}", usage)
            Assert.Contains(ImageGroupingOptions.OutputFolderName, usage)
            Assert.Contains(LightnessCalculator.GetFolderName(0.0R, LightnessCalculator.DefaultStepPercentage), usage)
            Assert.Contains($"""{ProductInfo.CliExecutableName}""", usage)
            Assert.Contains($"""{ProductInfo.GuiExecutableName}""", usage)
            Assert.All(ImageLightnessReader.SupportedExtensions, Sub(extension As String) Assert.Contains(extension, usage))

            For Each exitCode As CommandLineExitCode In [Enum].GetValues(GetType(CommandLineExitCode))
                Assert.Contains($"  {CInt(exitCode)}  {CommandLineHelp.ExitCodeDescriptions(exitCode)}", usage)
            Next
        End Sub

        ''' <summary>
        ''' Every example of the help is accepted by the parser (the help never lies).
        ''' </summary>
        <Fact>
        Public Sub Help_ExamplesAreAccepted()
            Dim examples As List(Of String) = GetUsageLines().SkipWhile(Function(line As String) line <> "EXAMPLES:").Skip(1).ToList()
            Assert.NotEmpty(examples)

            For Each example As String In examples
                Dim args As List(Of String) = SplitCommandLine(example.Trim())
                Assert.Equal(ProductInfo.CliExecutableName, args(0))

                Dim arguments As CommandLineArguments = CommandLineParser.Parse(args.Skip(1).ToList())
                Assert.False(arguments.ShowHelp)
            Next
        End Sub

        ''' <summary>
        ''' The header is the title followed by the repository address; an empty title is rejected.
        ''' </summary>
        <Fact>
        Public Sub Header_IsTheTitleAndTheRepository()
            Assert.Equal(New List(Of String) From {"Title v1 (X64)", ProductInfo.RepositoryUrl}, CommandLineHelp.GetHeaderLines("Title v1 (X64)").ToList())
            Assert.Throws(Of ArgumentException)(Function() CommandLineHelp.GetHeaderLines(" "))
            Assert.Throws(Of ArgumentException)(Function() CommandLineHelp.GetHeaderLines(Nothing))
        End Sub

        ''' <summary>
        ''' The title of the real executable includes its version and architecture.
        ''' </summary>
        <Fact>
        Public Sub Title_IncludesTheVersionAndArchitecture()
            Dim version As Version = GetType(Program).Assembly.GetName().Version

            Assert.Equal($"Image Lightness Grouper CLI v{version} ({Global.ImageLightnessGrouper.Infrastructure.UtilApplication.ProcessArchitecture.ToString().ToUpperInvariant()})", Program.GetTitle())
        End Sub

        ''' <summary>
        ''' The exit codes are a public contract: their values never change.
        ''' </summary>
        <Fact>
        Public Sub ExitCodes_HaveStableValues()
            Assert.Equal(0, CInt(CommandLineExitCode.Success))
            Assert.Equal(1, CInt(CommandLineExitCode.CompletedWithProblems))
            Assert.Equal(2, CInt(CommandLineExitCode.InvalidArguments))
            Assert.Equal(3, CInt(CommandLineExitCode.InvalidDirectory))
            Assert.Equal(4, CInt(CommandLineExitCode.OperationFailed))
            Assert.Equal(5, CInt(CommandLineExitCode.Cancelled))
            Assert.Equal(6, CInt(CommandLineExitCode.MissingComponent))
            Assert.Equal(7, [Enum].GetValues(GetType(CommandLineExitCode)).Length)
        End Sub

        ''' <summary>
        ''' A command-line exception requires a defined exit code.
        ''' </summary>
        <Fact>
        Public Sub CommandLineException_RequiresADefinedExitCode()
            Assert.Equal(CommandLineExitCode.InvalidArguments, New CommandLineException("x", CommandLineExitCode.InvalidArguments).ExitCode)
            Assert.Throws(Of ComponentModel.InvalidEnumArgumentException)(Function() New CommandLineException("x", CType(42, CommandLineExitCode)))
        End Sub

        ''' <summary>
        ''' Splits a command line with the basic Windows quoting rules (enough for the examples of the help).
        ''' </summary>
        ''' <param name="commandLine">The command line.</param>
        ''' <returns>The arguments, including the executable.</returns>
        Private Shared Function SplitCommandLine(commandLine As String) As List(Of String)
            Dim result As New List(Of String)()
            Dim current As New Text.StringBuilder()
            Dim inQuotes As Boolean = False
            Dim hasToken As Boolean = False

            For Each character As Char In commandLine
                If character = """"c Then
                    inQuotes = Not inQuotes
                    hasToken = True
                ElseIf character = " "c AndAlso Not inQuotes Then
                    If hasToken Then
                        result.Add(current.ToString())
                        current.Clear()
                        hasToken = False
                    End If
                Else
                    current.Append(character)
                    hasToken = True
                End If
            Next

            If hasToken Then
                result.Add(current.ToString())
            End If

            Return result
        End Function

#End Region

#Region " Console output "

        ''' <summary>
        ''' The text writer writes the output and the errors to their own writers, without colors.
        ''' </summary>
        <Fact>
        Public Sub TextConsoleWriter_WritesToTheRightWriters()
            Using output As New StringWriter(),
                  errorOutput As New StringWriter()

                Dim writer As New TextConsoleWriter(output, errorOutput, useColors:=False)
                writer.WriteLine("out", LogSeverity.Moved)
                writer.WriteErrorLine("err")

                Assert.Equal($"out{Environment.NewLine}", output.ToString())
                Assert.Equal($"err{Environment.NewLine}", errorOutput.ToString())
                Assert.Throws(Of ArgumentNullException)(Sub() writer.WriteLine(Nothing, LogSeverity.Information))
                Assert.Throws(Of ArgumentNullException)(Sub() writer.WriteErrorLine(Nothing))
                Assert.Throws(Of ArgumentNullException)(Function() New TextConsoleWriter(Nothing, errorOutput, False))
            End Using
        End Sub

        ''' <summary>
        ''' Concurrent writes (from the parallel worker threads) are never interleaved within a line.
        ''' </summary>
        <Fact>
        Public Sub TextConsoleWriter_IsThreadSafe()
            Using output As New StringWriter(), errorOutput As New StringWriter()
                Dim writer As New TextConsoleWriter(output, errorOutput, useColors:=False)
                Dim line As New String("x"c, 200)

                Parallel.For(0, 500, Sub(index As Integer) writer.WriteLine(line, LogSeverity.Information))

                Dim lines As String() = output.ToString().Split({Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries)
                Assert.Equal(500, lines.Length)
                Assert.All(lines, Sub(writtenLine As String) Assert.Equal(line, writtenLine))
            End Using
        End Sub

        ''' <summary>
        ''' Every severity has a color mapping (or the default color), and errors are red.
        ''' </summary>
        <Fact>
        Public Sub TextConsoleWriter_MapsEverySeverity()
            For Each severity As LogSeverity In [Enum].GetValues(GetType(LogSeverity))
                TextConsoleWriter.GetColor(severity)
            Next

            Assert.Equal(ConsoleColor.Red, TextConsoleWriter.GetColor(LogSeverity.Error))
            Assert.Null(TextConsoleWriter.GetColor(LogSeverity.Information))
            Assert.Throws(Of ComponentModel.InvalidEnumArgumentException)(Function() TextConsoleWriter.GetColor(CType(99, LogSeverity)))
        End Sub

        ''' <summary>
        ''' The console progress writes every report immediately (not posted to a synchronization context).
        ''' </summary>
        <Fact>
        Public Sub ConsoleProgress_WritesImmediately()
            Dim writer As New RecordingConsoleWriter()
            Dim progress As New ConsoleProgress(writer)

            progress.Report(New ImageGroupingProgress(1, 2, "moved", LogSeverity.Moved))

            Assert.Single(writer.Output)
            Assert.Equal("moved", writer.Output(0).Text)
            Assert.Equal(LogSeverity.Moved, writer.Output(0).Severity)
            Assert.Throws(Of ArgumentNullException)(Sub() progress.Report(Nothing))
            Assert.Throws(Of ArgumentNullException)(Function() New ConsoleProgress(Nothing))
        End Sub

#End Region

#Region " Summary report "

        ''' <summary>
        ''' The verdict line depends on the failures and conflicts, and the counters are formatted.
        ''' </summary>
        <Fact>
        Public Sub SummaryReport_VerdictAndCounters()
            Dim success As List(Of LogLine) = ImageGroupingReport.GetSummaryLines(New ImageGroupingSummary(1234, 1234, 0, 0, 4, TimeSpan.FromSeconds(61.5))).ToList()
            Dim conflicts As List(Of LogLine) = ImageGroupingReport.GetSummaryLines(New ImageGroupingSummary(3, 2, 1, 0, 4, TimeSpan.Zero)).ToList()
            Dim failures As List(Of LogLine) = ImageGroupingReport.GetSummaryLines(New ImageGroupingSummary(3, 1, 1, 1, 4, TimeSpan.Zero)).ToList()

            Assert.Equal(LogSeverity.Success, success.Last().Severity)
            Assert.Equal(LogSeverity.Warning, conflicts.Last().Severity)
            Assert.Equal(LogSeverity.Error, failures.Last().Severity)
            Assert.Contains(success, Function(line As LogLine) line.Text.EndsWith(": 1,234", StringComparison.Ordinal))
            Assert.Contains(success, Function(line As LogLine) line.Text.EndsWith(": 00:01:01.500", StringComparison.Ordinal))
            Assert.Throws(Of ArgumentNullException)(Function() ImageGroupingReport.GetSummaryLines(Nothing))
        End Sub

        ''' <summary>
        ''' A log line validates its arguments.
        ''' </summary>
        <Fact>
        Public Sub LogLine_ValidatesArguments()
            Assert.Throws(Of ArgumentNullException)(Function() New LogLine(Nothing, LogSeverity.Information))
            Assert.Throws(Of ComponentModel.InvalidEnumArgumentException)(Function() New LogLine("x", CType(99, LogSeverity)))
        End Sub

#End Region

    End Class

End Namespace
