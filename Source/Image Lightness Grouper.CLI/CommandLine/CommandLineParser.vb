Option Strict On
Option Explicit On
Option Infer Off

#Region " Imports "

Imports System.Globalization

Imports ImageLightnessGrouper.Core

#End Region

Namespace ImageLightnessGrouper.CommandLine

    ''' <summary>
    ''' Parses the command-line arguments (see <see cref="CommandLineSyntax"/> for the switch rules):
    ''' <list type="bullet">
    ''' <item>No arguments, or <c>/?</c>, <c>/h</c> or <c>/help</c> anywhere: show the help.</item>
    ''' <item><c>&lt;directory&gt; [/recursive] [/step:n] [/alpha:n] [/parallel:n]</c>, in any order: group the images.</item>
    ''' </list>
    ''' </summary>
    Friend NotInheritable Class CommandLineParser

#Region " Constructors "

        ''' <summary>
        ''' Prevents a default instance of the <see cref="CommandLineParser"/> class from being created.
        ''' </summary>
        Private Sub New()
        End Sub

#End Region

#Region " Friend Methods "

        ''' <summary>
        ''' Parses the command-line arguments.
        ''' </summary>
        ''' <param name="args">The arguments, without the executable path.</param>
        ''' <returns>The parsed arguments. The directory is not validated (see <see cref="DirectoryPathResolver.Resolve"/>).</returns>
        ''' <exception cref="ArgumentNullException">Thrown when <paramref name="args"/> is <see langword="Nothing"/>.</exception>
        ''' <exception cref="CommandLineException">Thrown when the arguments are not valid.</exception>
        Friend Shared Function Parse(args As IList(Of String)) As CommandLineArguments
            If args Is Nothing Then
                Throw New ArgumentNullException(NameOf(args))
            End If

            ' Without arguments, the help is shown.
            If args.Count = 0 Then
                Return CommandLineArguments.ForHelp()
            End If

            ' The help switch wins over everything else (even over invalid arguments), so "/?" can be appended to any command line.
            For Each argument As String In args
                If CommandLineSyntax.IsHelpSwitch(argument) Then
                    Return CommandLineArguments.ForHelp()
                End If
            Next

            Dim directories As New List(Of String)()
            Dim switchValues As New Dictionary(Of String, String)(StringComparer.Ordinal)

            For Each argument As String In args
                If argument Is Nothing Then
                    Throw New CommandLineException("A command-line argument is missing (null).", CommandLineExitCode.InvalidArguments)
                End If

                If CommandLineSyntax.IsSwitch(argument) Then
                    Dim name As String = Nothing
                    Dim value As String = Nothing
                    CommandLineParser.ParseSwitch(argument, name, value)

                    If switchValues.ContainsKey(name) Then
                        Throw New CommandLineException($"The switch '/{name}' is specified more than once.", CommandLineExitCode.InvalidArguments)
                    End If

                    switchValues.Add(name, value)
                Else
                    directories.Add(argument)
                End If
            Next

            If directories.Count = 0 Then
                Throw New CommandLineException("The directory is missing.", CommandLineExitCode.InvalidArguments)
            End If

            If directories.Count > 1 Then
                Throw New CommandLineException($"Only one directory can be specified, but {directories.Count} were found " &
                                               $"('{String.Join("', '", directories)}'). Enclose a path that contains spaces in quotes.",
                                               CommandLineExitCode.InvalidArguments)
            End If

            Dim stepPercentage As Integer = CommandLineParser.GetIntegerValue(
                switchValues, CommandLineSyntax.StepSwitch, LightnessCalculator.DefaultStepPercentage,
                LightnessCalculator.MinimumStepPercentage, LightnessCalculator.MaximumStepPercentage)

            Dim alphaThreshold As Integer = CommandLineParser.GetIntegerValue(
                switchValues, CommandLineSyntax.AlphaSwitch, LightnessCalculator.DefaultAlphaThreshold, Byte.MinValue, Byte.MaxValue)

            Dim maxDegreeOfParallelism As Integer = CommandLineParser.GetIntegerValue(
                switchValues, CommandLineSyntax.ParallelSwitch, ParallelismCalculator.Automatic,
                ParallelismCalculator.Automatic, ParallelismCalculator.MaximumRequestable)

            Return New CommandLineArguments(directories(0), switchValues.ContainsKey(CommandLineSyntax.RecursiveSwitch),
                                            stepPercentage, alphaThreshold, maxDegreeOfParallelism)
        End Function

#End Region

#Region " Private Methods "

        ''' <summary>
        ''' Splits a switch into its canonical name and its value, and validates whether a value is expected.
        ''' </summary>
        ''' <param name="argument">The switch, as written in the command line.</param>
        ''' <param name="name">Receives the canonical switch name.</param>
        ''' <param name="value">Receives the value, or <see langword="Nothing"/> for a switch without a value.</param>
        ''' <exception cref="CommandLineException">Thrown when the switch is unknown, or its value is missing or unexpected.</exception>
        Private Shared Sub ParseSwitch(argument As String, ByRef name As String, ByRef value As String)
            Dim writtenName As String = CommandLineSyntax.GetWrittenName(argument)
            Dim body As String = CommandLineSyntax.RemovePrefix(argument)
            value = If(body.Length > writtenName.Length, body.Substring(writtenName.Length + 1), Nothing)

            If Not CommandLineSyntax.SwitchAliases.TryGetValue(writtenName, name) Then
                ' A directory glued to a switch ("/rC:\Images") looks like an unknown switch with a path as its value.
                Dim looksLikeGluedPath As Boolean = value IsNot Nothing AndAlso (value.StartsWith("\", StringComparison.Ordinal) OrElse value.StartsWith("/", StringComparison.Ordinal))
                Throw New CommandLineException($"Unknown switch: {argument}" &
                                               If(looksLikeGluedPath, " (separate the switches and the directory with a space).", String.Empty),
                                               CommandLineExitCode.InvalidArguments)
            End If

            Dim expectsValue As Boolean = CommandLineSyntax.ValueSwitches.Contains(name)
            If expectsValue AndAlso String.IsNullOrWhiteSpace(value) Then
                Throw New CommandLineException($"The switch '/{name}' requires a value (e.g. /{name}:5).", CommandLineExitCode.InvalidArguments)
            End If

            If Not expectsValue AndAlso value IsNot Nothing Then
                Throw New CommandLineException($"The switch '/{name}' does not take a value: {argument}", CommandLineExitCode.InvalidArguments)
            End If
        End Sub

        ''' <summary>
        ''' Gets the integer value of a switch, or a default value when the switch is not specified.
        ''' </summary>
        ''' <param name="switchValues">The parsed switches.</param>
        ''' <param name="name">The canonical switch name.</param>
        ''' <param name="defaultValue">The value used when the switch is not specified.</param>
        ''' <param name="minimum">The smallest allowed value.</param>
        ''' <param name="maximum">The largest allowed value.</param>
        ''' <returns>The value.</returns>
        ''' <exception cref="CommandLineException">Thrown when the value is not an integer or is out of range.</exception>
        Private Shared Function GetIntegerValue(switchValues As Dictionary(Of String, String), name As String,
                                                defaultValue As Integer, minimum As Integer, maximum As Integer) As Integer
            Dim text As String = Nothing
            If Not switchValues.TryGetValue(name, text) Then
                Return defaultValue
            End If

            Dim value As Integer
            If Not Integer.TryParse(text.Trim(), NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, value) OrElse
               value < minimum OrElse value > maximum Then

                Throw New CommandLineException($"The value of '/{name}' must be an integer from {minimum} to {maximum}: {text}", CommandLineExitCode.InvalidArguments)
            End If

            Return value
        End Function

#End Region

    End Class

End Namespace
