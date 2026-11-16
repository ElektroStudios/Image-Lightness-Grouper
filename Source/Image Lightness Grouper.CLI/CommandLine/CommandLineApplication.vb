Option Strict On
Option Explicit On
Option Infer Off

#Region " Imports "

Imports System.Globalization
Imports System.Threading

Imports ImageLightnessGrouper.Core

#End Region

Namespace ImageLightnessGrouper.CommandLine

    ''' <summary>
    ''' The command-line application, independent of the real console (see <see cref="Program"/>), so it is fully testable:
    ''' writes the header, parses the arguments, and shows the help or groups the images.
    ''' </summary>
    Friend NotInheritable Class CommandLineApplication

#Region " Constructors "

        ''' <summary>
        ''' Prevents a default instance of the <see cref="CommandLineApplication"/> class from being created.
        ''' </summary>
        Private Sub New()
        End Sub

#End Region

#Region " Friend Methods "

        ''' <summary>
        ''' Runs the command line.
        ''' <para></para>
        ''' The header (title, version, architecture and repository address) is always written first, whatever the
        ''' arguments are. Errors are written to the standard error output.
        ''' </summary>
        ''' <param name="args">The command-line arguments, without the executable path.</param>
        ''' <param name="writer">The output.</param>
        ''' <param name="title">The application title, version and architecture.</param>
        ''' <param name="service">The grouping service.</param>
        ''' <param name="currentDirectory">The absolute directory used to resolve a relative directory path.</param>
        ''' <param name="cancellationToken">A token to cancel the operation (Ctrl+C).</param>
        ''' <returns>The process exit code.</returns>
        ''' <exception cref="ArgumentNullException">Thrown when an argument is <see langword="Nothing"/>.</exception>
        Friend Shared Function Run(args As IList(Of String), writer As IConsoleWriter, title As String,
                                   service As ImageGroupingService, currentDirectory As String,
                                   cancellationToken As CancellationToken) As CommandLineExitCode

            If args Is Nothing Then
                Throw New ArgumentNullException(NameOf(args))
            End If

            If writer Is Nothing Then
                Throw New ArgumentNullException(NameOf(writer))
            End If

            If service Is Nothing Then
                Throw New ArgumentNullException(NameOf(service))
            End If

            If currentDirectory Is Nothing Then
                Throw New ArgumentNullException(NameOf(currentDirectory))
            End If

            For Each line As String In CommandLineHelp.GetHeaderLines(title)
                writer.WriteLine(line, LogSeverity.Information)
            Next
            writer.WriteLine(String.Empty, LogSeverity.Information)

            Dim arguments As CommandLineArguments
            Try
                arguments = CommandLineParser.Parse(args)
            Catch ex As CommandLineException
                writer.WriteErrorLine($"[ERROR] {ex.Message}")
                writer.WriteErrorLine(CommandLineApplication.GetHelpHint())
                Return ex.ExitCode
            End Try

            If arguments.ShowHelp Then
                For Each line As String In CommandLineHelp.GetUsageText().Split({Environment.NewLine}, StringSplitOptions.None)
                    writer.WriteLine(line, LogSeverity.Information)
                Next

                Return CommandLineExitCode.Success
            End If

            Return CommandLineApplication.Group(arguments, writer, service, currentDirectory, cancellationToken)
        End Function

        ''' <summary>
        ''' Gets the hint that tells how to show the help.
        ''' </summary>
        ''' <returns>The hint.</returns>
        Friend Shared Function GetHelpHint() As String
            Return $"Run ""{ProductInfo.CliExecutableName}"" /? to show the help."
        End Function

#End Region

#Region " Private Methods "

        ''' <summary>
        ''' Groups the images of the directory specified in the command line.
        ''' </summary>
        ''' <param name="arguments">The parsed arguments.</param>
        ''' <param name="writer">The output.</param>
        ''' <param name="service">The grouping service.</param>
        ''' <param name="currentDirectory">The absolute directory used to resolve a relative directory path.</param>
        ''' <param name="cancellationToken">A token to cancel the operation.</param>
        ''' <returns>The process exit code.</returns>
        Private Shared Function Group(arguments As CommandLineArguments, writer As IConsoleWriter, service As ImageGroupingService,
                                      currentDirectory As String, cancellationToken As CancellationToken) As CommandLineExitCode

            Dim directoryPath As String
            Try
                directoryPath = DirectoryPathResolver.Resolve(arguments.Directory, currentDirectory)
            Catch ex As InvalidDirectoryException
                writer.WriteErrorLine($"[ERROR] {ex.Message}")
                Return CommandLineExitCode.InvalidDirectory
            End Try

            Try
                ImageLightnessReader.EnsureSkiaSharpIsAvailable()
            Catch ex As InvalidOperationException
                writer.WriteErrorLine($"[ERROR] {ex.Message}")
                Return CommandLineExitCode.MissingComponent
            End Try

            writer.WriteLine(ImageGroupingReport.Rule, LogSeverity.Muted)
            writer.WriteLine($"[START] {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)}", LogSeverity.Information)
            writer.WriteLine(ImageGroupingReport.Rule, LogSeverity.Muted)

            Dim options As New ImageGroupingOptions(directoryPath, arguments.SearchRecursively, arguments.StepPercentage,
                                                    arguments.AlphaThreshold, arguments.MaxDegreeOfParallelism)
            Dim summary As ImageGroupingSummary

            Try
                summary = service.RunAsync(options, New ConsoleProgress(writer), cancellationToken).GetAwaiter().GetResult()

            Catch ex As OperationCanceledException
                writer.WriteErrorLine("[CANCELLED] The operation was cancelled. The images already moved stay in their new folders.")
                Return CommandLineExitCode.Cancelled

            Catch ex As Exception
                ' Any other failure aborts the whole operation (per-image failures never get here: they are counted
                ' in the summary). The process must end with an exit code, so it is reported instead of rethrown.
                writer.WriteErrorLine($"[ERROR] The operation failed. {ex.GetType().Name}: {ex.Message}")
                Return CommandLineExitCode.OperationFailed
            End Try

            For Each line As LogLine In ImageGroupingReport.GetSummaryLines(summary)
                writer.WriteLine(line.Text, line.Severity)
            Next

            Return If(summary.FailedCount > 0 OrElse summary.ConflictCount > 0,
                      CommandLineExitCode.CompletedWithProblems,
                      CommandLineExitCode.Success)
        End Function

#End Region

    End Class

End Namespace
