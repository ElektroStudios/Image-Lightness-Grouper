Option Strict On
Option Explicit On
Option Infer Off

#Region " Imports "

Imports System.IO

Imports ImageLightnessGrouper.Core
Imports ImageLightnessGrouper.Infrastructure
Imports ImageLightnessGrouper.Tests.TestInfrastructure

#End Region

Namespace ImageLightnessGrouper.Tests.Infrastructure

    ''' <summary>
    ''' Tests for <see cref="StartupArguments"/> (the command-line argument of the main window).
    ''' </summary>
    Public NotInheritable Class StartupArgumentsTests

        ''' <summary>
        ''' No arguments: no directory and no error.
        ''' </summary>
        <Fact>
        Public Sub Parse_NoArguments()
            Dim arguments As StartupArguments = StartupArguments.Parse({}, Path.GetTempPath())

            Assert.Null(arguments.Directory)
            Assert.Null(arguments.ErrorMessage)
        End Sub

        ''' <summary>
        ''' An existing directory (absolute, relative, or quoted with a trailing backslash) is resolved to its full path.
        ''' </summary>
        <Fact>
        Public Sub Parse_ExistingDirectory()
            Using temp As New TemporaryDirectory()
                Directory.CreateDirectory(temp.Combine("My Images"))

                For Each argument As String In {temp.Combine("My Images"), "My Images", $"My Images{Path.DirectorySeparatorChar}"""}
                    Dim arguments As StartupArguments = StartupArguments.Parse({argument}, temp.FullPath)

                    Assert.Equal(temp.Combine("My Images"), arguments.Directory)
                    Assert.Null(arguments.ErrorMessage)
                Next
            End Using
        End Sub

        ''' <summary>
        ''' A missing directory, a file or a malformed path gives an error message and no directory (never an exception).
        ''' </summary>
        <Fact>
        Public Sub Parse_InvalidDirectory_GivesAnError()
            Using temp As New TemporaryDirectory()
                Dim filePath As String = TestImageFactory.CreateTextFile(temp.Combine("file.txt"))

                For Each argument As String In {temp.Combine("missing"), filePath, $"bad{ChrW(0)}path", " "}
                    Dim arguments As StartupArguments = StartupArguments.Parse({argument}, temp.FullPath)

                    Assert.Null(arguments.Directory)
                    Assert.False(String.IsNullOrWhiteSpace(arguments.ErrorMessage))
                Next
            End Using
        End Sub

        ''' <summary>
        ''' Switches and several arguments are not accepted by the main window: the error points to the command-line
        ''' application. Regression: a switch glued to a path (<c>/group"C:\Images"</c>) was taken as a directory.
        ''' </summary>
        <Theory>
        <InlineData("/?")>
        <InlineData("/group")>
        <InlineData("/groupC:\Users\Administrador\Images")>
        <InlineData("-r")>
        Public Sub Parse_Switch_PointsToTheCommandLineApplication(argument As String)
            For Each args As String() In New String()() {New String() {argument}, New String() {Path.GetTempPath(), argument}}
                Dim arguments As StartupArguments = StartupArguments.Parse(args, Path.GetTempPath())

                Assert.Null(arguments.Directory)
                Assert.Contains(ProductInfo.CliExecutableName, arguments.ErrorMessage)
                Assert.Contains(argument, arguments.ErrorMessage)
            Next
        End Sub

        ''' <summary>
        ''' Several directories are not accepted (a path with spaces must be quoted).
        ''' </summary>
        <Fact>
        Public Sub Parse_SeveralDirectories_GivesAnError()
            Dim arguments As StartupArguments = StartupArguments.Parse({"C:\My", "Images"}, Path.GetTempPath())

            Assert.Null(arguments.Directory)
            Assert.Contains("quotes", arguments.ErrorMessage)
        End Sub

        ''' <summary>
        ''' Invalid calls fail fast.
        ''' </summary>
        <Fact>
        Public Sub InvalidCalls_Throw()
            Assert.Throws(Of ArgumentNullException)(Function() StartupArguments.Parse(Nothing, Path.GetTempPath()))
            Assert.Throws(Of ArgumentNullException)(Function() StartupArguments.Parse({}, Nothing))
            Assert.Throws(Of ArgumentException)(Function() New StartupArguments("C:\", "error"))
        End Sub

    End Class

End Namespace
