Option Strict On
Option Explicit On
Option Infer Off

#Region " Imports "

Imports ImageLightnessGrouper.CommandLine
Imports ImageLightnessGrouper.Core

#End Region

Namespace ImageLightnessGrouper.Infrastructure

    ''' <summary>
    ''' The command-line argument of the main window: an optional directory, selected when the window loads.
    ''' <para></para>
    ''' The main window never shows message boxes for its command line: an invalid argument is reported in its
    ''' console, and the window opens without a directory selected. The full command-line interface is a separate
    ''' console application (<see cref="ProductInfo.CliExecutableName"/>).
    ''' </summary>
    Friend NotInheritable Class StartupArguments

#Region " Properties "

        ''' <summary>
        ''' Gets the arguments of the current process, read at startup. The default value has no directory and no error.
        ''' </summary>
        Friend Shared Property Current As StartupArguments = New StartupArguments(Nothing, Nothing)

        ''' <summary>
        ''' Gets the full path of the existing directory to select, or <see langword="Nothing"/>.
        ''' </summary>
        Friend ReadOnly Property Directory As String

        ''' <summary>
        ''' Gets the message that describes why the command line is not valid, or <see langword="Nothing"/>.
        ''' </summary>
        Friend ReadOnly Property ErrorMessage As String

#End Region

#Region " Constructors "

        ''' <summary>
        ''' Initializes a new instance of the <see cref="StartupArguments"/> class.
        ''' </summary>
        ''' <param name="directory">The full path of the directory to select, or <see langword="Nothing"/>.</param>
        ''' <param name="errorMessage">The error message, or <see langword="Nothing"/>.</param>
        ''' <exception cref="ArgumentException">Thrown when both a directory and an error are specified.</exception>
        Friend Sub New(directory As String, errorMessage As String)
            If directory IsNot Nothing AndAlso errorMessage IsNot Nothing Then
                Throw New ArgumentException("A directory and an error cannot be specified together.", NameOf(errorMessage))
            End If

            Me.Directory = directory
            Me.ErrorMessage = errorMessage
        End Sub

#End Region

#Region " Friend Methods "

        ''' <summary>
        ''' Parses the command-line arguments of the main window: none, or a single directory.
        ''' </summary>
        ''' <param name="args">The arguments, without the executable path.</param>
        ''' <param name="currentDirectory">The absolute directory used to resolve a relative directory path.</param>
        ''' <returns>The parsed arguments (never throws for invalid user input: see <see cref="ErrorMessage"/>).</returns>
        ''' <exception cref="ArgumentNullException">Thrown when an argument is <see langword="Nothing"/>.</exception>
        Friend Shared Function Parse(args As IList(Of String), currentDirectory As String) As StartupArguments
            If args Is Nothing Then
                Throw New ArgumentNullException(NameOf(args))
            End If

            If currentDirectory Is Nothing Then
                Throw New ArgumentNullException(NameOf(currentDirectory))
            End If

            If args.Count = 0 Then
                Return New StartupArguments(Nothing, Nothing)
            End If

            If args.Count > 1 OrElse args(0) Is Nothing OrElse CommandLineSyntax.IsSwitch(args(0)) Then
                Return New StartupArguments(Nothing,
                    $"Invalid command line: {String.Join(" ", args)}{Environment.NewLine}" &
                    "The main window only accepts a directory (enclose a path that contains spaces in quotes). " &
                    $"To work from the command line, use ""{ProductInfo.CliExecutableName}"" (see Help > Command-Line Reference).")
            End If

            Try
                Return New StartupArguments(DirectoryPathResolver.Resolve(args(0), currentDirectory), Nothing)
            Catch ex As InvalidDirectoryException
                Return New StartupArguments(Nothing, ex.Message)
            End Try
        End Function

#End Region

    End Class

End Namespace
