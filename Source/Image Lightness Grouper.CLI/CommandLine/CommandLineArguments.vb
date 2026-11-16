Option Strict On
Option Explicit On
Option Infer Off

#Region " Imports "

Imports ImageLightnessGrouper.Core

#End Region

Namespace ImageLightnessGrouper.CommandLine

    ''' <summary>
    ''' Immutable result of parsing the command-line arguments (see <see cref="CommandLineParser"/>).
    ''' <para></para>
    ''' The grouping options default to the documented defaults of the application, never to the preferences saved by
    ''' the main window, so a command line always does the same thing on any computer.
    ''' </summary>
    Friend NotInheritable Class CommandLineArguments

#Region " Properties "

        ''' <summary>
        ''' Gets a value indicating whether the help must be shown (instead of grouping the images).
        ''' </summary>
        Friend ReadOnly Property ShowHelp As Boolean

        ''' <summary>
        ''' Gets the directory, exactly as written in the command line (not validated, not resolved), or
        ''' <see langword="Nothing"/> when <see cref="ShowHelp"/> is <see langword="True"/>.
        ''' Use <see cref="DirectoryPathResolver.Resolve"/> to validate it.
        ''' </summary>
        Friend ReadOnly Property Directory As String

        ''' <summary>
        ''' Gets a value indicating whether the images inside the subdirectories are processed too.
        ''' </summary>
        Friend ReadOnly Property SearchRecursively As Boolean

        ''' <summary>
        ''' Gets the width of each lightness range, in percent points.
        ''' </summary>
        Friend ReadOnly Property StepPercentage As Integer

        ''' <summary>
        ''' Gets the alpha threshold (0-255).
        ''' </summary>
        Friend ReadOnly Property AlphaThreshold As Integer

        ''' <summary>
        ''' Gets the requested maximum number of images processed in parallel (0 = automatic).
        ''' </summary>
        Friend ReadOnly Property MaxDegreeOfParallelism As Integer

#End Region

#Region " Constructors "

        ''' <summary>
        ''' Initializes a new instance of the <see cref="CommandLineArguments"/> class, to group the images of a directory.
        ''' </summary>
        ''' <param name="directory">The directory, as written in the command line.</param>
        ''' <param name="searchRecursively">Whether the subdirectories are processed too.</param>
        ''' <param name="stepPercentage">The width of each lightness range.</param>
        ''' <param name="alphaThreshold">The alpha threshold.</param>
        ''' <param name="maxDegreeOfParallelism">The requested degree of parallelism (0 = automatic).</param>
        ''' <exception cref="ArgumentNullException">Thrown when <paramref name="directory"/> is <see langword="Nothing"/>.</exception>
        ''' <exception cref="ArgumentOutOfRangeException">Thrown when a numeric argument is out of range.</exception>
        Friend Sub New(directory As String, searchRecursively As Boolean,
                       stepPercentage As Integer, alphaThreshold As Integer, maxDegreeOfParallelism As Integer)

            If directory Is Nothing Then
                Throw New ArgumentNullException(NameOf(directory))
            End If

            LightnessCalculator.ValidateStepPercentage(stepPercentage)
            LightnessCalculator.ValidateAlphaThreshold(alphaThreshold)
            ParallelismCalculator.ValidateRequested(maxDegreeOfParallelism)

            Me.Directory = directory
            Me.SearchRecursively = searchRecursively
            Me.StepPercentage = stepPercentage
            Me.AlphaThreshold = alphaThreshold
            Me.MaxDegreeOfParallelism = maxDegreeOfParallelism
        End Sub

        ''' <summary>
        ''' Initializes a new instance of the <see cref="CommandLineArguments"/> class, to show the help.
        ''' </summary>
        Private Sub New()
            Me.ShowHelp = True
            Me.StepPercentage = LightnessCalculator.DefaultStepPercentage
            Me.AlphaThreshold = LightnessCalculator.DefaultAlphaThreshold
            Me.MaxDegreeOfParallelism = ParallelismCalculator.Automatic
        End Sub

#End Region

#Region " Friend Methods "

        ''' <summary>
        ''' Creates the arguments that show the help.
        ''' </summary>
        ''' <returns>The arguments.</returns>
        Friend Shared Function ForHelp() As CommandLineArguments
            Return New CommandLineArguments()
        End Function

#End Region

    End Class

End Namespace
