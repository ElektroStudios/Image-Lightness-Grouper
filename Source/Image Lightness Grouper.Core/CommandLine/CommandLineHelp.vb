Option Strict On
Option Explicit On
Option Infer Off

#Region " Imports "

Imports System.Collections.ObjectModel
Imports System.Text

Imports ImageLightnessGrouper.Core

#End Region

Namespace ImageLightnessGrouper.CommandLine

    ''' <summary>
    ''' Builds the command-line reference: the header shown before any output, and the help text.
    ''' <para></para>
    ''' Every switch, range, default value, folder name, file format and exit code is taken from the code that
    ''' defines it, so the help can never get out of date. The same text is shown by the command-line application
    ''' (<c>/?</c>) and by the main window (Help &gt; Command-Line Reference).
    ''' </summary>
    Public NotInheritable Class CommandLineHelp

#Region " Constants and Read-only Fields "

        ''' <summary>
        ''' The maximum line length of the help text (it fits in a default 80-column console without wrapping).
        ''' </summary>
        Public Const MaximumLineLength As Integer = 79

        ''' <summary>
        ''' The description of every exit code.
        ''' </summary>
        Public Shared ReadOnly ExitCodeDescriptions As New ReadOnlyDictionary(Of CommandLineExitCode, String)(
            New Dictionary(Of CommandLineExitCode, String) From {
                {CommandLineExitCode.Success, "Success (or the help was shown)."},
                {CommandLineExitCode.CompletedWithProblems, "Completed, but some images were not moved (failures or name conflicts)."},
                {CommandLineExitCode.InvalidArguments, "Invalid command-line arguments."},
                {CommandLineExitCode.InvalidDirectory, "The directory does not exist or its path is not valid."},
                {CommandLineExitCode.OperationFailed, "The operation failed (e.g. inaccessible or already grouped directory)."},
                {CommandLineExitCode.Cancelled, "Cancelled by the user (Ctrl+C)."},
                {CommandLineExitCode.MissingComponent, "A required component is missing (libSkiaSharp.dll)."}
            })

#End Region

#Region " Constructors "

        ''' <summary>
        ''' Prevents a default instance of the <see cref="CommandLineHelp"/> class from being created.
        ''' </summary>
        Private Sub New()
        End Sub

#End Region

#Region " Public Methods "

        ''' <summary>
        ''' Gets the header written before any other output: the application title, version and architecture,
        ''' followed by the address of the source code repository.
        ''' </summary>
        ''' <param name="title">The application title, version and architecture (e.g. <c>"Image Lightness Grouper CLI v1.1.0.0 (X64)"</c>).</param>
        ''' <returns>The header lines.</returns>
        ''' <exception cref="ArgumentException">Thrown when <paramref name="title"/> is empty.</exception>
        Public Shared Function GetHeaderLines(title As String) As ReadOnlyCollection(Of String)
            If String.IsNullOrWhiteSpace(title) Then
                Throw New ArgumentException("The title cannot be empty.", NameOf(title))
            End If

            Return Array.AsReadOnly({title, ProductInfo.RepositoryUrl})
        End Function

        ''' <summary>
        ''' Gets the help text.
        ''' </summary>
        ''' <returns>The help text, with lines of at most <see cref="MaximumLineLength"/> characters.</returns>
        Public Shared Function GetUsageText() As String
            Dim cli As String = $"""{ProductInfo.CliExecutableName}"""
            Dim gui As String = $"""{ProductInfo.GuiExecutableName}"""
            Dim defaultStep As Integer = LightnessCalculator.DefaultStepPercentage
            Dim exampleFolder As String = $"{ImageGroupingOptions.OutputFolderName}\{LightnessCalculator.GetFolderName(0.0R, defaultStep)}"
            Dim formats As String = String.Join(", ", ImageLightnessReader.SupportedExtensions)
            Dim text As New StringBuilder()

            text.AppendLine("DESCRIPTION:")
            AppendParagraph(text, 2, "Groups the image files of a directory into folders by their average " &
                                     "lightness (CIE L*, the lightness perceived by the human eye, from 0% = " &
                                     "black to 100% = white).")
            text.AppendLine()
            AppendParagraph(text, 2, "The lightness of every pixel of each image is computed (pixels below the " &
                                     "alpha threshold are ignored) and averaged. Then the image is MOVED into " &
                                     "the lightness range folder that contains its average, inside the output " &
                                     $"folder of its own directory, e.g. ""{exampleFolder}"".")
            text.AppendLine()
            AppendParagraph(text, 2, "Existing files are never overwritten: an image whose name already exists " &
                                     "in its destination folder stays in place (name conflict). The output " &
                                     "folders and the lightness range folders are never processed again, so " &
                                     "it is safe to run the program again over the same directory.")
            text.AppendLine()
            AppendParagraph(text, 2, $"Supported formats: {formats}.")
            text.AppendLine()
            text.AppendLine("USAGE:")
            text.AppendLine($"  {cli} <directory> [options]")
            text.AppendLine("      Groups the images of the directory. Relative paths are resolved")
            text.AppendLine("      against the current directory. Enclose paths with spaces in quotes.")
            text.AppendLine()
            text.AppendLine($"  {cli} /?")
            text.AppendLine("      Shows this help (also /h or /help). Running the program without")
            text.AppendLine("      arguments also shows it.")
            text.AppendLine()
            text.AppendLine("OPTIONS:")
            text.AppendLine($"  /{CommandLineSyntax.RecursiveSwitch}, /r")
            text.AppendLine("      Also processes the subdirectories. Each directory gets its own")
            text.AppendLine("      output folder, so images of different folders are never mixed.")
            text.AppendLine("      Default: off.")
            text.AppendLine($"  /{CommandLineSyntax.StepSwitch}:<n>, /s:<n>")
            text.AppendLine($"      Width of each lightness range, in percent points, from {LightnessCalculator.MinimumStepPercentage} to {LightnessCalculator.MaximumStepPercentage}.")
            text.AppendLine($"      Default: {defaultStep}.")
            text.AppendLine($"  /{CommandLineSyntax.AlphaSwitch}:<n>, /a:<n>")
            text.AppendLine($"      Pixels whose alpha (opacity, {Byte.MinValue} to {Byte.MaxValue}) is below this value are")
            text.AppendLine($"      ignored. Default: {LightnessCalculator.DefaultAlphaThreshold}.")
            text.AppendLine($"  /{CommandLineSyntax.ParallelSwitch}:<n>, /p:<n>")
            text.AppendLine($"      Images processed in parallel, from {ParallelismCalculator.Automatic} to {ParallelismCalculator.MaximumRequestable}.")
            text.AppendLine($"      {ParallelismCalculator.Automatic} = automatic (limited by the free memory). Default: {ParallelismCalculator.Automatic}.")
            text.AppendLine()
            AppendParagraph(text, 2, "Switches are case-insensitive and can also start with ""-"" or ""--"". " &
                                     $"Values can also be written with ""="" (e.g. /{CommandLineSyntax.StepSwitch}={defaultStep \ 2}). " &
                                     "The settings of the main window are neither used nor changed.")
            text.AppendLine()
            AppendParagraph(text, 2, "Each image is reported while it is processed, followed by a summary. " &
                                     "Press Ctrl+C to cancel: the images already moved stay in their new folders.")
            text.AppendLine()
            text.AppendLine("EXIT CODES:")
            For Each exitCode As CommandLineExitCode In [Enum].GetValues(GetType(CommandLineExitCode))
                text.AppendLine($"  {CInt(exitCode)}  {CommandLineHelp.ExitCodeDescriptions(exitCode)}")
            Next
            text.AppendLine()
            text.AppendLine("MAIN WINDOW:")
            text.AppendLine($"  {gui} [<directory>]")
            text.AppendLine("      Opens the main window, with the directory already selected (if any).")
            text.AppendLine()
            text.AppendLine("EXAMPLES:")
            text.AppendLine($"  {cli} ""C:\Wallpapers""")
            text.AppendLine($"  {cli} ""C:\Wallpapers"" /{CommandLineSyntax.RecursiveSwitch} /{CommandLineSyntax.StepSwitch}:{defaultStep \ 2}")

            Return text.ToString().TrimEnd()
        End Function

#End Region

#Region " Private Methods "

        ''' <summary>
        ''' Appends a paragraph, word-wrapped to <see cref="MaximumLineLength"/> characters.
        ''' </summary>
        ''' <param name="text">The text being built.</param>
        ''' <param name="indent">The number of spaces before each line.</param>
        ''' <param name="paragraph">The paragraph.</param>
        Private Shared Sub AppendParagraph(text As StringBuilder, indent As Integer, paragraph As String)
            Dim prefix As New String(" "c, indent)
            Dim line As New StringBuilder(prefix)

            For Each word As String In paragraph.Split({" "c}, StringSplitOptions.RemoveEmptyEntries)
                If line.Length > indent AndAlso line.Length + 1 + word.Length > MaximumLineLength Then
                    text.AppendLine(line.ToString())
                    line.Clear().Append(prefix)
                End If

                If line.Length > indent Then
                    line.Append(" "c)
                End If

                line.Append(word)
            Next

            text.AppendLine(line.ToString())
        End Sub

#End Region

    End Class

End Namespace
