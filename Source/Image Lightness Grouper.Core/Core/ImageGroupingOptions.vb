Option Strict On
Option Explicit On
Option Infer Off

#Region " Imports "

Imports System.Collections.ObjectModel
Imports System.IO

#End Region

Namespace ImageLightnessGrouper.Core

    ''' <summary>
    ''' Immutable set of options that drive a single <see cref="ImageGroupingService"/> run.
    ''' </summary>
    Public NotInheritable Class ImageGroupingOptions

#Region " Constants "

        ''' <summary>
        ''' The name of the output folder created inside each processed directory, which holds the lightness range folders.
        ''' </summary>
        Public Const OutputFolderName As String = "@Grouped by lightness"

#End Region

#Region " Properties "

        ''' <summary>
        ''' Gets the absolute path of the directory that contains the images to process.
        ''' </summary>
        Public ReadOnly Property TargetDirectory As String

        ''' <summary>
        ''' Gets a value indicating whether the images inside the subdirectories are processed too.
        ''' Each directory gets its own output folder.
        ''' </summary>
        Public ReadOnly Property SearchRecursively As Boolean

        ''' <summary>
        ''' Gets the width of each lightness range, in percent points.
        ''' </summary>
        Public ReadOnly Property StepPercentage As Integer

        ''' <summary>
        ''' Gets the alpha threshold: pixels whose alpha is below this value (0-255) are ignored.
        ''' </summary>
        Public ReadOnly Property AlphaThreshold As Integer

        ''' <summary>
        ''' Gets the requested maximum number of images processed in parallel
        ''' (<see cref="ParallelismCalculator.Automatic"/> to calculate it from the free memory and the processors).
        ''' </summary>
        Public ReadOnly Property MaxDegreeOfParallelism As Integer

        ''' <summary>
        ''' Gets the read-only collection of allowed (lower-case, dot-prefixed) file extensions.
        ''' </summary>
        Public ReadOnly Property AllowedExtensions As ReadOnlyCollection(Of String)

#End Region

#Region " Constructors "

        ''' <summary>
        ''' Initializes a new instance of the <see cref="ImageGroupingOptions"/> class.
        ''' </summary>
        ''' <param name="targetDirectory">The absolute path of the directory that contains the images to process.</param>
        ''' <param name="searchRecursively">Whether the images inside the subdirectories are processed too.</param>
        ''' <param name="stepPercentage">The width of each lightness range, in percent points.</param>
        ''' <param name="alphaThreshold">The alpha threshold (0-255).</param>
        ''' <param name="maxDegreeOfParallelism">The requested maximum degree of parallelism (0 = automatic).</param>
        ''' <exception cref="ArgumentNullException">Thrown when <paramref name="targetDirectory"/> is <see langword="Nothing"/>.</exception>
        ''' <exception cref="ArgumentException">Thrown when <paramref name="targetDirectory"/> is empty or not rooted.</exception>
        ''' <exception cref="ArgumentOutOfRangeException">Thrown when a numeric argument is out of range.</exception>
        Public Sub New(targetDirectory As String, searchRecursively As Boolean, stepPercentage As Integer,
                       alphaThreshold As Integer, maxDegreeOfParallelism As Integer)

            If targetDirectory Is Nothing Then
                Throw New ArgumentNullException(NameOf(targetDirectory))
            End If

            If String.IsNullOrWhiteSpace(targetDirectory) Then
                Throw New ArgumentException("The target directory cannot be empty.", NameOf(targetDirectory))
            End If

            If Not Path.IsPathRooted(targetDirectory) Then
                Throw New ArgumentException($"The target directory must be an absolute path: {targetDirectory}", NameOf(targetDirectory))
            End If

            LightnessCalculator.ValidateStepPercentage(stepPercentage)
            LightnessCalculator.ValidateAlphaThreshold(alphaThreshold)
            ParallelismCalculator.ValidateRequested(maxDegreeOfParallelism)

            Me.TargetDirectory = targetDirectory
            Me.SearchRecursively = searchRecursively
            Me.StepPercentage = stepPercentage
            Me.AlphaThreshold = alphaThreshold
            Me.MaxDegreeOfParallelism = maxDegreeOfParallelism
            Me.AllowedExtensions = ImageLightnessReader.SupportedExtensions
        End Sub

#End Region

    End Class

End Namespace
