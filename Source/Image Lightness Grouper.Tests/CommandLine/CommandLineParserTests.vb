Option Strict On
Option Explicit On
Option Infer Off

#Region " Imports "

Imports ImageLightnessGrouper.CommandLine
Imports ImageLightnessGrouper.Core

#End Region

Namespace ImageLightnessGrouper.Tests.CommandLine

    ''' <summary>
    ''' Tests for the command-line parser (<see cref="CommandLineParser"/>) and its syntax rules (<see cref="CommandLineSyntax"/>).
    ''' </summary>
    Public NotInheritable Class CommandLineParserTests

#Region " Helpers "

        ''' <summary>
        ''' Parses the arguments.
        ''' </summary>
        ''' <param name="args">The arguments.</param>
        ''' <returns>The parsed arguments.</returns>
        Private Shared Function Parse(ParamArray args As String()) As CommandLineArguments
            Return CommandLineParser.Parse(args)
        End Function

        ''' <summary>
        ''' Asserts that the arguments are rejected as invalid arguments, and returns the error message.
        ''' </summary>
        ''' <param name="args">The arguments.</param>
        ''' <returns>The error message.</returns>
        Private Shared Function AssertRejected(ParamArray args As String()) As String
            Dim exception As CommandLineException = Assert.Throws(Of CommandLineException)(Function() CommandLineParser.Parse(args))
            Assert.Equal(CommandLineExitCode.InvalidArguments, exception.ExitCode)
            Assert.False(String.IsNullOrWhiteSpace(exception.Message))
            Return exception.Message
        End Function

#End Region

#Region " Help "

        ''' <summary>
        ''' No arguments shows the help.
        ''' </summary>
        <Fact>
        Public Sub Parse_NoArguments_ShowsHelp()
            Dim arguments As CommandLineArguments = Parse()

            Assert.True(arguments.ShowHelp)
            Assert.Null(arguments.Directory)
        End Sub

        ''' <summary>
        ''' Every help switch, with any prefix and case, shows the help, even with other (even invalid) arguments.
        ''' </summary>
        <Theory>
        <InlineData("/?")>
        <InlineData("-?")>
        <InlineData("/h")>
        <InlineData("-H")>
        <InlineData("/help")>
        <InlineData("-help")>
        <InlineData("--help")>
        <InlineData("/HELP")>
        Public Sub Parse_HelpSwitch_ShowsHelp(helpSwitch As String)
            Assert.True(Parse(helpSwitch).ShowHelp)
            Assert.True(Parse("C:\Images", "/step:99", helpSwitch).ShowHelp)
            Assert.True(Parse("/unknown", helpSwitch).ShowHelp)
        End Sub

#End Region

#Region " Grouping "

        ''' <summary>
        ''' A directory alone groups with the documented defaults (never the preferences of the main window),
        ''' and the directory is kept exactly as written (it is validated later).
        ''' </summary>
        <Theory>
        <InlineData("C:\Images")>
        <InlineData("Relative Folder")>
        <InlineData("C:\Images\""")>
        <InlineData("/data/images")>
        <InlineData("\\server\share\images")>
        Public Sub Parse_Directory_UsesTheDefaults(directory As String)
            Dim arguments As CommandLineArguments = Parse(directory)

            Assert.False(arguments.ShowHelp)
            Assert.Equal(directory, arguments.Directory)
            Assert.False(arguments.SearchRecursively)
            Assert.Equal(LightnessCalculator.DefaultStepPercentage, arguments.StepPercentage)
            Assert.Equal(LightnessCalculator.DefaultAlphaThreshold, arguments.AlphaThreshold)
            Assert.Equal(ParallelismCalculator.Automatic, arguments.MaxDegreeOfParallelism)
        End Sub

        ''' <summary>
        ''' Every option, in any order, before or after the directory, with any prefix, separator and case.
        ''' </summary>
        <Theory>
        <InlineData("C:\Images", "/recursive", "/step:5", "/alpha:0", "/parallel:3")>
        <InlineData("/step:5", "/parallel:3", "C:\Images", "/alpha:0", "/recursive")>
        <InlineData("C:\Images", "-r", "-s:5", "-a:0", "-p:3")>
        <InlineData("C:\Images", "--recursive", "--step=5", "--alpha=0", "--parallel=3")>
        <InlineData("C:\Images", "/Recursive", "/STEP:5", "/Alpha=0", "/P: 3 ")>
        Public Sub Parse_AllOptions(a1 As String, a2 As String, a3 As String, a4 As String, a5 As String)
            Dim arguments As CommandLineArguments = Parse(a1, a2, a3, a4, a5)

            Assert.False(arguments.ShowHelp)
            Assert.Equal("C:\Images", arguments.Directory)
            Assert.True(arguments.SearchRecursively)
            Assert.Equal(5, arguments.StepPercentage)
            Assert.Equal(0, arguments.AlphaThreshold)
            Assert.Equal(3, arguments.MaxDegreeOfParallelism)
        End Sub

        ''' <summary>
        ''' The limits of every range are accepted.
        ''' </summary>
        <Fact>
        Public Sub Parse_AcceptsTheLimits()
            Dim lowest As CommandLineArguments = Parse("x", $"/step:{LightnessCalculator.MinimumStepPercentage}", "/alpha:0", "/parallel:0")
            Dim highest As CommandLineArguments = Parse("x", $"/step:{LightnessCalculator.MaximumStepPercentage}", "/alpha:255", $"/parallel:{ParallelismCalculator.MaximumRequestable}")

            Assert.Equal(LightnessCalculator.MinimumStepPercentage, lowest.StepPercentage)
            Assert.Equal(LightnessCalculator.MaximumStepPercentage, highest.StepPercentage)
            Assert.Equal(255, highest.AlphaThreshold)
            Assert.Equal(ParallelismCalculator.MaximumRequestable, highest.MaxDegreeOfParallelism)
        End Sub

#End Region

#Region " Invalid arguments "

        ''' <summary>
        ''' Unknown switches are rejected and named in the message.
        ''' </summary>
        '''
        ''' <param name="unknownSwitch">
        ''' The unknown switch.
        ''' </param>
        <Theory>
        <InlineData("/unknown")>
        <InlineData("/stepp:5")>
        <InlineData("/group")>
        <InlineData("--")>
        <InlineData("-x")>
        Public Sub Parse_UnknownSwitch_IsRejected(unknownSwitch As String)
            Dim message As String = AssertRejected("C:\Images", unknownSwitch)
            Assert.Contains(unknownSwitch, message, StringComparison.Ordinal)
        End Sub

        ''' <summary>
        ''' Regression: a directory glued to a switch (<c>/r"C:\Images"</c> arrives as <c>/rC:\Images</c>) is an unknown
        ''' switch with a hint, never a directory (which would fail later with a confusing message).
        ''' </summary>
        <Theory>
        <InlineData("/rC:\Images")>
        <InlineData("/groupC:\Users\Administrador\Images")>
        <InlineData("-pC:/Images")>
        Public Sub Parse_DirectoryGluedToSwitch_IsRejectedWithAHint(argument As String)
            Dim message As String = AssertRejected(argument)
            Assert.Contains(argument, message)
            Assert.Contains("separate", message)
        End Sub

        ''' <summary>
        ''' Missing, non-integer and out-of-range values are rejected.
        ''' </summary>
        <Theory>
        <InlineData("/step")>
        <InlineData("/step:")>
        <InlineData("/step=")>
        <InlineData("/step: ")>
        <InlineData("/step:abc")>
        <InlineData("/step:5.5")>
        <InlineData("/step:0")>
        <InlineData("/step:11")>
        <InlineData("/step:-1")>
        <InlineData("/step:99999999999999999999")>
        <InlineData("/alpha:-1")>
        <InlineData("/alpha:256")>
        <InlineData("/parallel:-1")>
        <InlineData("/parallel:257")>
        <InlineData("/parallel:1e3")>
        <InlineData("/step:0x5")>
        Public Sub Parse_InvalidValue_IsRejected(invalidSwitch As String)
            AssertRejected("C:\Images", invalidSwitch)
        End Sub

        ''' <summary>
        ''' The error message of an out-of-range value shows the allowed range.
        ''' </summary>
        <Fact>
        Public Sub Parse_OutOfRangeValue_MessageShowsTheRange()
            Dim message As String = AssertRejected("C:\Images", "/step:11")
            Assert.Contains($"{LightnessCalculator.MinimumStepPercentage} to {LightnessCalculator.MaximumStepPercentage}", message)
        End Sub

        ''' <summary>
        ''' Switches that do not take a value reject one.
        ''' </summary>
        <Theory>
        <InlineData("/recursive:yes")>
        <InlineData("/r=1")>
        Public Sub Parse_UnexpectedValue_IsRejected(switchWithValue As String)
            AssertRejected("C:\Images", switchWithValue)
        End Sub

        ''' <summary>
        ''' A repeated switch is rejected, even when written with an alias or another case.
        ''' </summary>
        <Theory>
        <InlineData("/step:5", "/step:5")>
        <InlineData("/step:5", "/s:6")>
        <InlineData("/recursive", "-R")>
        Public Sub Parse_RepeatedSwitch_IsRejected(first As String, second As String)
            AssertRejected("C:\Images", first, second)
        End Sub

        ''' <summary>
        ''' Options without a directory are rejected.
        ''' </summary>
        <Fact>
        Public Sub Parse_WithoutDirectory_IsRejected()
            Assert.Contains("missing", AssertRejected("/r", "/step:5"))
        End Sub

        ''' <summary>
        ''' Several directories are rejected, with a hint about quoting paths that contain spaces.
        ''' </summary>
        <Fact>
        Public Sub Parse_SeveralDirectories_AreRejected()
            Assert.Contains("quotes", AssertRejected("C:\My", "Images"))
        End Sub

        ''' <summary>
        ''' A null argument array fails fast; a null argument is rejected.
        ''' </summary>
        <Fact>
        Public Sub Parse_NullArguments()
            Assert.Throws(Of ArgumentNullException)(Function() CommandLineParser.Parse(Nothing))
            AssertRejected("C:\Images", Nothing)
        End Sub

#End Region

#Region " Syntax "

        ''' <summary>
        ''' Switches are told apart from paths.
        ''' </summary>
        <Theory>
        <InlineData("/r", True)>
        <InlineData("-r", True)>
        <InlineData("--recursive", True)>
        <InlineData("/step:5", True)>
        <InlineData("/?", True)>
        <InlineData("/rC:\Images", True)>
        <InlineData("C:\Images", False)>
        <InlineData("/data/images", False)>
        <InlineData("/tmp", True)>
        <InlineData("-", False)>
        <InlineData("/", False)>
        <InlineData("", False)>
        <InlineData(Nothing, False)>
        <InlineData("Images", False)>
        Public Sub IsSwitch(argument As String, expected As Boolean)
            Assert.Equal(expected, CommandLineSyntax.IsSwitch(argument))
        End Sub

        ''' <summary>
        ''' Every canonical switch has a short alias, and every alias maps to a canonical switch.
        ''' </summary>
        <Fact>
        Public Sub SwitchAliases_AreConsistent()
            Dim canonicalNames As String() = {CommandLineSyntax.RecursiveSwitch, CommandLineSyntax.StepSwitch,
                                              CommandLineSyntax.AlphaSwitch, CommandLineSyntax.ParallelSwitch}

            For Each name As String In canonicalNames
                Assert.Equal(name, CommandLineSyntax.SwitchAliases(name))
                Assert.Equal(name, CommandLineSyntax.SwitchAliases(name.Substring(0, 1)))
            Next

            Assert.All(CommandLineSyntax.SwitchAliases.Values, Sub(value As String) Assert.Contains(value, canonicalNames))
            Assert.Throws(Of ArgumentException)(Function() CommandLineSyntax.RemovePrefix("C:\Images"))
        End Sub

#End Region

    End Class

End Namespace
