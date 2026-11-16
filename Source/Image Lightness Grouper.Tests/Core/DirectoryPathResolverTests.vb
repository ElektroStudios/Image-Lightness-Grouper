Option Strict On
Option Explicit On
Option Infer Off

#Region " Imports "

Imports System.IO

Imports ImageLightnessGrouper.Core
Imports ImageLightnessGrouper.Tests.TestInfrastructure

#End Region

Namespace ImageLightnessGrouper.Tests.Core

    ''' <summary>
    ''' Tests for <see cref="DirectoryPathResolver"/>.
    ''' </summary>
    Public NotInheritable Class DirectoryPathResolverTests

        ''' <summary>
        ''' An existing absolute directory is returned as a full path, without a trailing separator.
        ''' </summary>
        <Fact>
        Public Sub Resolve_ExistingAbsoluteDirectory()
            Using temp As New TemporaryDirectory()
                Dim withSeparator As String = temp.FullPath & Path.DirectorySeparatorChar

                Assert.Equal(temp.FullPath, DirectoryPathResolver.Resolve(temp.FullPath, Path.GetTempPath()))
                Assert.Equal(temp.FullPath, DirectoryPathResolver.Resolve(withSeparator, Path.GetTempPath()))
            End Using
        End Sub

        ''' <summary>
        ''' Relative paths ("sub", ".", "..") are resolved against the current directory.
        ''' </summary>
        <Fact>
        Public Sub Resolve_RelativePaths()
            Using temp As New TemporaryDirectory()
                Dim subdirectory As String = temp.Combine("sub")
                Directory.CreateDirectory(subdirectory)

                Assert.Equal(subdirectory, DirectoryPathResolver.Resolve("sub", temp.FullPath))
                Assert.Equal(temp.FullPath, DirectoryPathResolver.Resolve(".", temp.FullPath))
                Assert.Equal(temp.FullPath, DirectoryPathResolver.Resolve("..", subdirectory))
            End Using
        End Sub

        ''' <summary>
        ''' Regression for the Windows command-line quoting rules: <c>"C:\Images\"</c> arrives as <c>C:\Images"</c>
        ''' (the backslash escapes the closing quote). The trailing quote and surrounding spaces are ignored.
        ''' </summary>
        <Fact>
        Public Sub Resolve_TrailingQuoteAndSpaces_AreIgnored()
            Using temp As New TemporaryDirectory()
                Assert.Equal(temp.FullPath, DirectoryPathResolver.Resolve($"{temp.FullPath}""", Path.GetTempPath()))
                Assert.Equal(temp.FullPath, DirectoryPathResolver.Resolve($"  {temp.FullPath}  ", Path.GetTempPath()))
            End Using
        End Sub

        ''' <summary>
        ''' The root directory keeps its separator (without it, "C:" would mean the current directory of drive C).
        ''' </summary>
        <Fact>
        Public Sub Resolve_Root_KeepsItsSeparator()
            Dim root As String = Path.GetPathRoot(Path.GetTempPath())

            Assert.Equal(root, DirectoryPathResolver.Resolve(root, Path.GetTempPath()))
        End Sub

        ''' <summary>
        ''' Missing directories, files, empty and malformed paths are rejected with a message for the user.
        ''' </summary>
        <Fact>
        Public Sub Resolve_InvalidPaths_AreRejected()
            Using temp As New TemporaryDirectory()
                Dim filePath As String = TestImageFactory.CreateTextFile(temp.Combine("file.txt"))
                Dim invalidPaths As String() = {
                    temp.Combine("missing"),
                    filePath,
                    String.Empty,
                    "   ",
                    """",
                    $"bad{ChrW(0)}path"
                }

                For Each invalidPath As String In invalidPaths
                    Dim exception As InvalidDirectoryException =
                        Assert.Throws(Of InvalidDirectoryException)(Function() DirectoryPathResolver.Resolve(invalidPath, temp.FullPath))
                    Assert.False(String.IsNullOrWhiteSpace(exception.Message))
                Next

                Assert.Contains("does not exist", Assert.Throws(Of InvalidDirectoryException)(Function() DirectoryPathResolver.Resolve(temp.Combine("missing"), temp.FullPath)).Message)
                Assert.Contains("is a file", Assert.Throws(Of InvalidDirectoryException)(Function() DirectoryPathResolver.Resolve(filePath, temp.FullPath)).Message)
            End Using
        End Sub

        ''' <summary>
        ''' Null arguments fail fast.
        ''' </summary>
        <Fact>
        Public Sub Resolve_NullArguments_Throw()
            Assert.Throws(Of ArgumentNullException)(Function() DirectoryPathResolver.Resolve(Nothing, Path.GetTempPath()))
            Assert.Throws(Of ArgumentNullException)(Function() DirectoryPathResolver.Resolve("x", Nothing))
        End Sub

    End Class

End Namespace
