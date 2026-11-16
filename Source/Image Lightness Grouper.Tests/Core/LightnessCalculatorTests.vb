Option Strict On
Option Explicit On
Option Infer Off

#Region " Imports "

Imports System.Collections.ObjectModel
Imports System.Drawing
Imports System.Drawing.Imaging
Imports System.Globalization
Imports System.Threading

Imports ImageLightnessGrouper.Core
Imports ImageLightnessGrouper.Tests.TestInfrastructure

#End Region

Namespace ImageLightnessGrouper.Tests.Core

    ''' <summary>
    ''' Tests for <see cref="LightnessCalculator"/>: color math, folder names and bitmap averaging.
    ''' </summary>
    Public NotInheritable Class LightnessCalculatorTests

#Region " Color math "

        ''' <summary>
        ''' The CIE "f" function is the cube root above the epsilon.
        ''' </summary>
        <Theory>
        <InlineData(0.125R, 0.5R)>
        <InlineData(0.216R, 0.6R)>
        <InlineData(0.512R, 0.8R)>
        <InlineData(1.0R, 1.0R)>
        Public Sub ComputeLabF_AboveEpsilon_IsTheCubeRoot(value As Double, expected As Double)
            Assert.Equal(expected, LightnessCalculator.ComputeLabF(value), 12)
        End Sub

        ''' <summary>
        ''' The CIE "f" function uses the linear slope at or below the epsilon, and it is continuous there.
        ''' </summary>
        <Fact>
        Public Sub ComputeLabF_BelowEpsilon_IsLinearAndContinuous()
            Assert.Equal(16.0R / 116.0R, LightnessCalculator.ComputeLabF(0.0R), 12)
            Assert.Equal(((903.3R * 0.001R) + 16.0R) / 116.0R, LightnessCalculator.ComputeLabF(0.001R), 12)

            Dim below As Double = LightnessCalculator.ComputeLabF(0.008856R)
            Dim above As Double = LightnessCalculator.ComputeLabF(0.008857R)
            Assert.InRange(above - below, 0.0R, 0.001R)
        End Sub

        ''' <summary>
        ''' The sRGB lookup table has 256 strictly increasing entries from 0 to 1, matching the reference transfer function.
        ''' </summary>
        <Fact>
        Public Sub BuildLinearSrgbLookupTable_MatchesTheReference()
            Dim table As ReadOnlyCollection(Of Double) = LightnessCalculator.BuildLinearSrgbLookupTable()

            Assert.Equal(256, table.Count)
            Assert.Equal(0.0R, table(0), 12)
            Assert.Equal(1.0R, table(255), 12)
            For index As Integer = 0 To 255
                Assert.Equal(ReferenceLightness.ToLinear(index), table(index), 12)
                If index > 0 Then
                    Assert.True(table(index) > table(index - 1), $"The table must be strictly increasing (index {index}).")
                End If
            Next
        End Sub

#End Region

#Region " Folder names "

        ''' <summary>
        ''' The default 10% interval maps values to the expected ranges (rounded to the nearest percentage),
        ''' and the folder name is the range formatted with <see cref="LightnessCalculator.RangeFolderNameFormat"/>.
        ''' </summary>
        <Theory>
        <InlineData(0.0R, 0, 10)>
        <InlineData(9.4R, 0, 10)>
        <InlineData(9.6R, 10, 20)>
        <InlineData(55.0R, 50, 60)>
        <InlineData(89.4R, 80, 90)>
        <InlineData(90.0R, 90, 100)>
        <InlineData(100.0R, 90, 100)>
        Public Sub GetRangeBounds_DefaultStep(lightness As Double, lowerBound As Integer, upperBound As Integer)
            Dim bounds As (LowerBound As Integer, UpperBound As Integer) = LightnessCalculator.GetRangeBounds(lightness, LightnessCalculator.DefaultStepPercentage)

            Assert.Equal(lowerBound, bounds.LowerBound)
            Assert.Equal(upperBound, bounds.UpperBound)
            Assert.Equal(String.Format(CultureInfo.InvariantCulture, LightnessCalculator.RangeFolderNameFormat, lowerBound, upperBound),
                         LightnessCalculator.GetFolderName(lightness, LightnessCalculator.DefaultStepPercentage))
        End Sub

        ''' <summary>
        ''' Other intervals, including those that do not divide 100 (the last range is capped at 100%).
        ''' </summary>
        <Theory>
        <InlineData(4.0R, 5, 0, 5)>
        <InlineData(53.58R, 5, 50, 55)>
        <InlineData(100.0R, 5, 95, 100)>
        <InlineData(42.0R, 1, 42, 43)>
        <InlineData(100.0R, 1, 99, 100)>
        <InlineData(96.0R, 3, 96, 99)>
        <InlineData(99.0R, 3, 99, 100)>
        <InlineData(97.0R, 7, 91, 98)>
        <InlineData(98.0R, 7, 98, 100)>
        Public Sub GetRangeBounds_CustomStep(lightness As Double, stepPercentage As Integer, lowerBound As Integer, upperBound As Integer)
            Assert.Equal((lowerBound, upperBound), LightnessCalculator.GetRangeBounds(lightness, stepPercentage))
            Assert.Equal(LightnessCalculator.FormatRangeFolderName(lowerBound, upperBound), LightnessCalculator.GetFolderName(lightness, stepPercentage))
        End Sub

        ''' <summary>
        ''' Out-of-range values are clamped instead of failing.
        ''' </summary>
        <Theory>
        <InlineData(-0.1R, 0, 10)>
        <InlineData(-1.0E+300R, 0, 10)>
        <InlineData(100.1R, 90, 100)>
        <InlineData(1.0E+300R, 90, 100)>
        Public Sub GetRangeBounds_OutOfRangeValues_AreClamped(lightness As Double, lowerBound As Integer, upperBound As Integer)
            Assert.Equal((lowerBound, upperBound), LightnessCalculator.GetRangeBounds(lightness, 10))
        End Sub

        ''' <summary>
        ''' For every interval, consecutive integer percentages produce contiguous, non-overlapping ranges
        ''' that contain the value and are never wider than the interval.
        ''' </summary>
        <Fact>
        Public Sub GetRangeBounds_AllSteps_RangesAreContiguousAndContainTheValue()
            For stepPercentage As Integer = LightnessCalculator.MinimumStepPercentage To LightnessCalculator.MaximumStepPercentage
                Dim previousUpper As Integer = 0
                Dim previousLower As Integer = -1

                For percentage As Integer = 0 To 99
                    Dim bounds As (LowerBound As Integer, UpperBound As Integer) = LightnessCalculator.GetRangeBounds(percentage, stepPercentage)

                    Assert.InRange(percentage, bounds.LowerBound, bounds.UpperBound - 1)
                    Assert.InRange(bounds.UpperBound - bounds.LowerBound, 1, stepPercentage)
                    If bounds.LowerBound <> previousLower Then
                        Assert.Equal(previousUpper, bounds.LowerBound)
                        previousUpper = bounds.UpperBound
                        previousLower = bounds.LowerBound
                    End If
                Next

                Assert.Equal(100, previousUpper)
            Next
        End Sub

        ''' <summary>
        ''' The folder names do not depend on the current culture.
        ''' </summary>
        <Fact>
        Public Sub GetFolderName_IsCultureInvariant()
            Dim expected As String = String.Format(CultureInfo.InvariantCulture, LightnessCalculator.RangeFolderNameFormat, 50, 60)
            Dim originalCulture As CultureInfo = Thread.CurrentThread.CurrentCulture
            Try
                Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("ar-SA")
                Assert.Equal(expected, LightnessCalculator.GetFolderName(55.0R, 10))
            Finally
                Thread.CurrentThread.CurrentCulture = originalCulture
            End Try
        End Sub

        ''' <summary>
        ''' Invalid intervals and non-finite values are rejected.
        ''' </summary>
        <Theory>
        <InlineData(50.0R, 0)>
        <InlineData(50.0R, 11)>
        <InlineData(50.0R, -1)>
        <InlineData(Double.NaN, 10)>
        <InlineData(Double.PositiveInfinity, 10)>
        <InlineData(Double.NegativeInfinity, 10)>
        Public Sub GetFolderName_InvalidArguments_Throw(lightness As Double, stepPercentage As Integer)
            Assert.Throws(Of ArgumentOutOfRangeException)(Function() LightnessCalculator.GetFolderName(lightness, stepPercentage))
            Assert.Throws(Of ArgumentOutOfRangeException)(Function() LightnessCalculator.GetRangeBounds(lightness, stepPercentage))
        End Sub

        ''' <summary>
        ''' Every folder name that can be produced, with any interval, is recognized as a range folder (case-insensitively).
        ''' </summary>
        <Fact>
        Public Sub IsRangeFolderName_RecognizesEveryProducibleName()
            For stepPercentage As Integer = LightnessCalculator.MinimumStepPercentage To LightnessCalculator.MaximumStepPercentage
                For percentage As Integer = 0 To 100
                    Dim name As String = LightnessCalculator.GetFolderName(percentage, stepPercentage)

                    Assert.True(LightnessCalculator.IsRangeFolderName(name), name)
                    Assert.True(LightnessCalculator.IsRangeFolderName(name.ToUpperInvariant()), name)
                    Assert.True(LightnessCalculator.IsRangeFolderName(name.ToLowerInvariant()), name)
                Next
            Next
        End Sub

        ''' <summary>
        ''' Other names, including ranges that no interval can produce, are not range folders.
        ''' </summary>
        <Fact>
        Public Sub IsRangeFolderName_RejectsOtherNames()
            Assert.False(LightnessCalculator.IsRangeFolderName(LightnessCalculator.FormatRangeFolderName(0, 11)))
            Assert.False(LightnessCalculator.IsRangeFolderName(LightnessCalculator.FormatRangeFolderName(5, 7)))
            Assert.False(LightnessCalculator.IsRangeFolderName($"{LightnessCalculator.FormatRangeFolderName(0, 10)} (copy)"))
            Assert.False(LightnessCalculator.IsRangeFolderName(String.Empty))
            Assert.False(LightnessCalculator.IsRangeFolderName("Photos"))
            Assert.False(LightnessCalculator.IsRangeFolderName(ImageGroupingOptions.OutputFolderName))
            Assert.Throws(Of ArgumentNullException)(Function() LightnessCalculator.IsRangeFolderName(Nothing))
        End Sub

#End Region

#Region " Bitmap averaging "

        ''' <summary>
        ''' Solid colors produce the lightness of the independent reference implementation
        ''' (well-known values: red 53.2, green 87.7, blue 32.3, yellow 97.1).
        ''' </summary>
        <Theory>
        <InlineData(0, 0, 0)>
        <InlineData(255, 255, 255)>
        <InlineData(255, 0, 0)>
        <InlineData(0, 255, 0)>
        <InlineData(0, 0, 255)>
        <InlineData(255, 255, 0)>
        <InlineData(10, 10, 10)>
        <InlineData(128, 128, 128)>
        <InlineData(200, 50, 10)>
        Public Sub ComputeAverageLightness_SolidColor_MatchesTheReference(red As Integer, green As Integer, blue As Integer)
            Using bitmap As Bitmap = TestImageFactory.CreateSolidBitmap(3, 2, Color.FromArgb(red, green, blue))
                Assert.Equal(ReferenceLightness.FromRgb(red, green, blue), LightnessCalculator.ComputeAverageLightness(bitmap, 8), 9)
            End Using
        End Sub

        ''' <summary>
        ''' The red and blue channels are not swapped (Format32bppArgb stores B, G, R, A in memory).
        ''' </summary>
        <Fact>
        Public Sub ComputeAverageLightness_RedAndBlueAreNotSwapped()
            Using red As Bitmap = TestImageFactory.CreateSolidBitmap(1, 1, Color.FromArgb(255, 0, 0)),
                  blue As Bitmap = TestImageFactory.CreateSolidBitmap(1, 1, Color.FromArgb(0, 0, 255))

                Assert.Equal(53.24R, LightnessCalculator.ComputeAverageLightness(red, 8), 1)
                Assert.Equal(32.30R, LightnessCalculator.ComputeAverageLightness(blue, 8), 1)
            End Using
        End Sub

        ''' <summary>
        ''' The result is the mean of the per-pixel lightness (half black, half white = 50), for any dimensions.
        ''' </summary>
        <Theory>
        <InlineData(2, 1)>
        <InlineData(2, 7)>
        <InlineData(258, 3)>
        Public Sub ComputeAverageLightness_IsTheMeanOfEveryPixel(width As Integer, height As Integer)
            Using bitmap As New Bitmap(width, height, PixelFormat.Format32bppArgb)
                For y As Integer = 0 To height - 1
                    For x As Integer = 0 To width - 1
                        bitmap.SetPixel(x, y, If(x < width \ 2, Color.Black, Color.White))
                    Next
                Next

                Assert.Equal(50.0R, LightnessCalculator.ComputeAverageLightness(bitmap, 8), 9)
            End Using
        End Sub

        ''' <summary>
        ''' Only the pixels whose alpha is strictly below the threshold are ignored; semi-transparent pixels use their
        ''' straight (not premultiplied) color; a fully ignored image is 0.
        ''' </summary>
        <Theory>
        <InlineData(8, 8, 100.0R)>
        <InlineData(8, 7, 0.0R)>
        <InlineData(0, 0, 100.0R)>
        <InlineData(255, 254, 0.0R)>
        <InlineData(255, 255, 100.0R)>
        <InlineData(8, 128, 100.0R)>
        Public Sub ComputeAverageLightness_AlphaThreshold(threshold As Integer, alpha As Integer, expected As Double)
            Using bitmap As Bitmap = TestImageFactory.CreateSolidBitmap(2, 2, Color.FromArgb(alpha, 255, 255, 255))
                Assert.Equal(expected, LightnessCalculator.ComputeAverageLightness(bitmap, threshold), 9)
            End Using
        End Sub

        ''' <summary>
        ''' Only 32-bit ARGB bitmaps are accepted, and invalid arguments fail fast.
        ''' </summary>
        <Fact>
        Public Sub ComputeAverageLightness_InvalidArguments_Throw()
            Using rgb As New Bitmap(2, 2, PixelFormat.Format24bppRgb),
                  argb As New Bitmap(2, 2, PixelFormat.Format32bppArgb)

                Assert.Throws(Of NotSupportedException)(Function() LightnessCalculator.ComputeAverageLightness(rgb, 8))
                Assert.Throws(Of ArgumentNullException)(Function() LightnessCalculator.ComputeAverageLightness(Nothing, 8))
                Assert.Throws(Of ArgumentOutOfRangeException)(Function() LightnessCalculator.ComputeAverageLightness(argb, -1))
                Assert.Throws(Of ArgumentOutOfRangeException)(Function() LightnessCalculator.ComputeAverageLightness(argb, 256))
            End Using
        End Sub

        ''' <summary>
        ''' The bitmap is left unlocked and unchanged, so it can be measured again with the same result.
        ''' </summary>
        <Fact>
        Public Sub ComputeAverageLightness_DoesNotLockNorModifyTheBitmap()
            Using bitmap As Bitmap = TestImageFactory.CreateSolidBitmap(4, 4, Color.FromArgb(77, 150, 33))
                Dim first As Double = LightnessCalculator.ComputeAverageLightness(bitmap, 8)
                Dim second As Double = LightnessCalculator.ComputeAverageLightness(bitmap, 8)

                Assert.Equal(first, second)
                Assert.Equal(Color.FromArgb(77, 150, 33).ToArgb(), bitmap.GetPixel(3, 3).ToArgb())
            End Using
        End Sub

#End Region

    End Class

End Namespace
