Option Strict On
Option Explicit On
Option Infer Off

Namespace ImageLightnessGrouper.Core

    ''' <summary>
    ''' Immutable final statistics of an <see cref="ImageGroupingService"/> run.
    ''' </summary>
    Public NotInheritable Class ImageGroupingSummary

#Region " Properties "

        ''' <summary>
        ''' Gets the total number of image files found.
        ''' </summary>
        Public ReadOnly Property TotalFiles As Integer

        ''' <summary>
        ''' Gets the number of images moved into a lightness range folder.
        ''' </summary>
        Public ReadOnly Property MovedCount As Integer

        ''' <summary>
        ''' Gets the number of images that were not moved because a file with the same name
        ''' already exists in their lightness range folder.
        ''' </summary>
        Public ReadOnly Property ConflictCount As Integer

        ''' <summary>
        ''' Gets the number of images that could not be processed (corrupt, unsupported or locked files).
        ''' </summary>
        Public ReadOnly Property FailedCount As Integer

        ''' <summary>
        ''' Gets the number of images that were processed in parallel.
        ''' </summary>
        Public ReadOnly Property DegreeOfParallelism As Integer

        ''' <summary>
        ''' Gets the total elapsed time.
        ''' </summary>
        Public ReadOnly Property Elapsed As TimeSpan

#End Region

#Region " Constructors "

        ''' <summary>
        ''' Initializes a new instance of the <see cref="ImageGroupingSummary"/> class.
        ''' </summary>
        ''' <param name="totalFiles">The total number of image files found.</param>
        ''' <param name="movedCount">The number of moved images.</param>
        ''' <param name="conflictCount">The number of images not moved because of a name conflict.</param>
        ''' <param name="failedCount">The number of failed images.</param>
        ''' <param name="degreeOfParallelism">The number of images processed in parallel (0 when no image was found).</param>
        ''' <param name="elapsed">The total elapsed time.</param>
        ''' <exception cref="ArgumentOutOfRangeException">
        ''' Thrown when a count is negative, or the counts do not add up to <paramref name="totalFiles"/>.
        ''' </exception>
        Friend Sub New(totalFiles As Integer, movedCount As Integer, conflictCount As Integer, failedCount As Integer,
                       degreeOfParallelism As Integer, elapsed As TimeSpan)

            If totalFiles < 0 OrElse movedCount < 0 OrElse conflictCount < 0 OrElse failedCount < 0 OrElse degreeOfParallelism < 0 Then
                Throw New ArgumentOutOfRangeException(NameOf(totalFiles), totalFiles, "The counts cannot be negative.")
            End If

            If movedCount + conflictCount + failedCount <> totalFiles Then
                Throw New ArgumentOutOfRangeException(NameOf(totalFiles), totalFiles,
                                                      $"The moved ({movedCount}), conflict ({conflictCount}) and failed ({failedCount}) counts must add up to the total ({totalFiles}).")
            End If

            Me.TotalFiles = totalFiles
            Me.MovedCount = movedCount
            Me.ConflictCount = conflictCount
            Me.FailedCount = failedCount
            Me.DegreeOfParallelism = degreeOfParallelism
            Me.Elapsed = elapsed
        End Sub

#End Region

    End Class

End Namespace
