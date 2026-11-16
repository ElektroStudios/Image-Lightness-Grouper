Option Strict On
Option Explicit On
Option Infer Off

#Region " Imports "

Imports System.IO
Imports System.Security

#End Region

Namespace ImageLightnessGrouper.Core

    ''' <summary>
    ''' Validates the directory paths written by the user in a command line.
    ''' </summary>
    Public NotInheritable Class DirectoryPathResolver

#Region " Constructors "

        ''' <summary>
        ''' Prevents a default instance of the <see cref="DirectoryPathResolver"/> class from being created.
        ''' </summary>
        Private Sub New()
        End Sub

#End Region

#Region " Public Methods "

        ''' <summary>
        ''' Validates a directory path and resolves it to a full path.
        ''' <para></para>
        ''' Relative paths are resolved against <paramref name="currentDirectory"/>. A trailing quote is removed: it is the
        ''' result of a quoted path that ends with a backslash (<c>"C:\Images\"</c>), because the Windows command-line
        ''' rules turn <c>\"</c> into a literal quote.
        ''' </summary>
        ''' <param name="directoryPath">The directory path, as written by the user.</param>
        ''' <param name="currentDirectory">The absolute directory used to resolve relative paths.</param>
        ''' <returns>The full path of the existing directory, without a trailing separator (except for a root directory).</returns>
        ''' <exception cref="ArgumentNullException">Thrown when an argument is <see langword="Nothing"/>.</exception>
        ''' <exception cref="InvalidDirectoryException">
        ''' Thrown when the path is empty or not valid, points to a file, or the directory does not exist.
        ''' </exception>
        Public Shared Function Resolve(directoryPath As String, currentDirectory As String) As String
            If directoryPath Is Nothing Then
                Throw New ArgumentNullException(NameOf(directoryPath))
            End If

            If currentDirectory Is Nothing Then
                Throw New ArgumentNullException(NameOf(currentDirectory))
            End If

            Dim cleanedPath As String = directoryPath.TrimEnd(""""c).Trim()
            If cleanedPath.Length = 0 Then
                Throw New InvalidDirectoryException("The directory path is empty.")
            End If

            Dim fullPath As String
            Try
                fullPath = Path.GetFullPath(Path.Combine(currentDirectory, cleanedPath))

            Catch ex As Exception When TypeOf ex Is ArgumentException OrElse
                                       TypeOf ex Is NotSupportedException OrElse
                                       TypeOf ex Is PathTooLongException OrElse
                                       TypeOf ex Is SecurityException
                Throw New InvalidDirectoryException($"The directory path is not valid: {directoryPath} ({ex.Message})", ex)
            End Try

            If File.Exists(fullPath) Then
                Throw New InvalidDirectoryException($"The path is a file, not a directory: {fullPath}")
            End If

            If Not Directory.Exists(fullPath) Then
                Throw New InvalidDirectoryException($"The directory does not exist: {fullPath}")
            End If

            ' Keep the root directory separator ("C:\"), which is required for the path to mean the root.
            Dim root As String = Path.GetPathRoot(fullPath)
            If fullPath.Length > root.Length Then
                fullPath = fullPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
            End If

            Return fullPath
        End Function

#End Region

    End Class

End Namespace
