Option Strict On
Option Explicit On
Option Infer Off

#Region " Imports "

Imports System.Collections.ObjectModel
Imports System.Drawing
Imports System.Drawing.Imaging
Imports System.Globalization
Imports System.Runtime.InteropServices

#End Region

Namespace ImageLightnessGrouper.Core

    ''' <summary>
    ''' Computes the perceptual lightness of images (CIE L*, from 0 = black to 100 = white)
    ''' and maps it to the lightness range folders (e.g. <c>"Light 40%-50%"</c>).
    ''' <para></para>
    ''' The pixels are converted from sRGB to linear light (official sRGB transfer function), to relative luminance
    ''' (ITU-R BT.709 coefficients), and finally to CIE L*, which models how bright a color is perceived by the human eye.
    ''' </summary>
    Public NotInheritable Class LightnessCalculator

#Region " Constants and Read-only Fields "

        ''' <summary>
        ''' The smallest allowed lightness range width, in percent points.
        ''' </summary>
        Public Const MinimumStepPercentage As Integer = 1

        ''' <summary>
        ''' The largest allowed lightness range width, in percent points.
        ''' </summary>
        Public Const MaximumStepPercentage As Integer = 10

        ''' <summary>
        ''' The default lightness range width, in percent points (folders "Light 00%-10%", "Light 10%-20%", ...).
        ''' </summary>
        Public Const DefaultStepPercentage As Integer = 10

        ''' <summary>
        ''' The default alpha threshold: pixels whose alpha is below this value are ignored.
        ''' </summary>
        Public Const DefaultAlphaThreshold As Integer = 8

        ''' <summary>
        ''' The composite format of the lightness range folder names: <c>{0}</c> is the lower bound and <c>{1}</c> the
        ''' upper bound, in percent (e.g. <c>"Light 40%-50%"</c>).
        ''' </summary>
        Public Const RangeFolderNameFormat As String = "Light {0:D2}%-{1:D2}%"

        ''' <summary>
        ''' ITU-R BT.709 weight of the red channel in the relative luminance.
        ''' </summary>
        Private Const LumaRed As Double = 0.2126R

        ''' <summary>
        ''' ITU-R BT.709 weight of the green channel in the relative luminance.
        ''' </summary>
        Private Const LumaGreen As Double = 0.7152R

        ''' <summary>
        ''' ITU-R BT.709 weight of the blue channel in the relative luminance.
        ''' </summary>
        Private Const LumaBlue As Double = 0.0722R

        ''' <summary>
        ''' CIE L* scale factor.
        ''' </summary>
        Private Const LStarScale As Double = 116.0R

        ''' <summary>
        ''' CIE L* offset.
        ''' </summary>
        Private Const LStarOffset As Double = 16.0R

        ''' <summary>
        ''' Lookup table that converts an 8-bit sRGB channel value into linear light (0.0 to 1.0).
        ''' </summary>
        Private Shared ReadOnly LinearSrgbLookupTable As ReadOnlyCollection(Of Double) = LightnessCalculator.BuildLinearSrgbLookupTable()

        ''' <summary>
        ''' The culture used to format the folder names.
        ''' </summary>
        Private Shared ReadOnly FormatCulture As CultureInfo = CultureInfo.InvariantCulture

        ''' <summary>
        ''' Every range folder name that <see cref="GetFolderName"/> can produce, for every allowed interval (case-insensitive).
        ''' </summary>
        Private Shared ReadOnly AllRangeFolderNames As New Lazy(Of HashSet(Of String))(AddressOf LightnessCalculator.BuildAllRangeFolderNames)

#End Region

#Region " Constructors "

        ''' <summary>
        ''' Prevents a default instance of the <see cref="LightnessCalculator"/> class from being created.
        ''' </summary>
        Private Sub New()
        End Sub

#End Region

#Region " Public Methods "

        ''' <summary>
        ''' Computes the average perceptual lightness (CIE L*) of a 32-bit ARGB bitmap.
        ''' </summary>
        ''' <param name="bitmap">The bitmap. Its pixel format must be <see cref="PixelFormat.Format32bppArgb"/>.</param>
        ''' <param name="alphaThreshold">Pixels whose alpha is below this value (0-255) are ignored.</param>
        ''' <returns>
        ''' The average lightness, from 0.0 (black) to 100.0 (white).
        ''' Zero when every pixel is ignored (fully transparent image).
        ''' </returns>
        ''' <exception cref="ArgumentNullException">Thrown when <paramref name="bitmap"/> is <see langword="Nothing"/>.</exception>
        ''' <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="alphaThreshold"/> is not between 0 and 255.</exception>
        ''' <exception cref="NotSupportedException">Thrown when the pixel format is not <see cref="PixelFormat.Format32bppArgb"/>.</exception>
        Public Shared Function ComputeAverageLightness(bitmap As Bitmap, alphaThreshold As Integer) As Double
            If bitmap Is Nothing Then
                Throw New ArgumentNullException(NameOf(bitmap))
            End If

            LightnessCalculator.ValidateAlphaThreshold(alphaThreshold)

            If bitmap.PixelFormat <> PixelFormat.Format32bppArgb Then
                Throw New NotSupportedException($"Expected pixel format {NameOf(PixelFormat.Format32bppArgb)}, got {bitmap.PixelFormat}.")
            End If

            Dim width As Integer = bitmap.Width
            Dim height As Integer = bitmap.Height
            Dim bitmapData As BitmapData = bitmap.LockBits(New Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb)

            Try
                Dim stride As Integer = bitmapData.Stride
                If stride <= 0 Then
                    Throw New NotSupportedException("Bottom-up bitmaps (negative or zero stride) are not supported.")
                End If

                ' A single row is copied at a time, so the managed memory never grows with the image size.
                Dim rowLength As Integer = width * 4
                Dim row As Byte() = New Byte(rowLength - 1) {}
                Dim totalLightness As Double = 0.0R
                Dim pixelCount As Long = 0L

                For y As Integer = 0 To height - 1
                    Marshal.Copy(IntPtr.Add(bitmapData.Scan0, y * stride), row, 0, rowLength)

                    For offset As Integer = 0 To rowLength - 4 Step 4
                        ' Format32bppArgb byte layout in memory: B, G, R, A.
                        If row(offset + 3) < alphaThreshold Then
                            Continue For
                        End If

                        Dim relativeLuminance As Double =
                            (LightnessCalculator.LinearSrgbLookupTable(row(offset + 2)) * LumaRed) +
                            (LightnessCalculator.LinearSrgbLookupTable(row(offset + 1)) * LumaGreen) +
                            (LightnessCalculator.LinearSrgbLookupTable(row(offset)) * LumaBlue)

                        totalLightness += (LStarScale * LightnessCalculator.ComputeLabF(relativeLuminance)) - LStarOffset
                        pixelCount += 1L
                    Next
                Next

                Return If(pixelCount = 0L, 0.0R, totalLightness / pixelCount)

            Finally
                bitmap.UnlockBits(bitmapData)
            End Try
        End Function

        ''' <summary>
        ''' Gets the bounds of the lightness range that contains the specified lightness value.
        ''' <para></para>
        ''' The value is rounded to the nearest integer percentage; 100% belongs to the last range.
        ''' When the interval does not divide 100, the last range is narrower (e.g. 98%-100% with a 7% interval).
        ''' </summary>
        ''' <param name="averageLightness">The lightness value (CIE L*). Values outside 0-100 are clamped.</param>
        ''' <param name="stepPercentage">The range width, in percent points.</param>
        ''' <returns>The lower (inclusive) and upper (exclusive, except for 100) bounds, in percent.</returns>
        ''' <exception cref="ArgumentOutOfRangeException">
        ''' Thrown when <paramref name="averageLightness"/> is not a finite number,
        ''' or <paramref name="stepPercentage"/> is outside <see cref="MinimumStepPercentage"/>-<see cref="MaximumStepPercentage"/>.
        ''' </exception>
        Public Shared Function GetRangeBounds(averageLightness As Double, stepPercentage As Integer) As (LowerBound As Integer, UpperBound As Integer)
            If Double.IsNaN(averageLightness) OrElse Double.IsInfinity(averageLightness) Then
                Throw New ArgumentOutOfRangeException(NameOf(averageLightness), averageLightness, "The lightness value must be a finite number.")
            End If

            LightnessCalculator.ValidateStepPercentage(stepPercentage)

            ' Clamp before rounding, so out-of-range values cannot overflow.
            Dim clampedLightness As Double = Math.Max(0.0R, Math.Min(100.0R, averageLightness))
            Dim percentage As Integer = Convert.ToInt32(Math.Round(clampedLightness, MidpointRounding.AwayFromZero))

            ' 100% belongs to the last range (the one that contains 99%).
            Dim lowerBound As Integer = (Math.Min(percentage, 99) \ stepPercentage) * stepPercentage
            Dim upperBound As Integer = Math.Min(lowerBound + stepPercentage, 100)

            Return (lowerBound, upperBound)
        End Function

        ''' <summary>
        ''' Gets the name of the lightness range folder that contains the specified lightness value
        ''' (see <see cref="GetRangeBounds"/> and <see cref="RangeFolderNameFormat"/>).
        ''' </summary>
        ''' <param name="averageLightness">The lightness value (CIE L*). Values outside 0-100 are clamped.</param>
        ''' <param name="stepPercentage">The range width, in percent points.</param>
        ''' <returns>The folder name, for example <c>"Light 40%-50%"</c>.</returns>
        ''' <exception cref="ArgumentOutOfRangeException">
        ''' Thrown when <paramref name="averageLightness"/> is not a finite number,
        ''' or <paramref name="stepPercentage"/> is outside <see cref="MinimumStepPercentage"/>-<see cref="MaximumStepPercentage"/>.
        ''' </exception>
        Public Shared Function GetFolderName(averageLightness As Double, stepPercentage As Integer) As String
            Dim bounds As (LowerBound As Integer, UpperBound As Integer) = LightnessCalculator.GetRangeBounds(averageLightness, stepPercentage)
            Return LightnessCalculator.FormatRangeFolderName(bounds.LowerBound, bounds.UpperBound)
        End Function

        ''' <summary>
        ''' Formats a lightness range folder name (see <see cref="RangeFolderNameFormat"/>).
        ''' </summary>
        ''' <param name="lowerBound">The lower bound, in percent.</param>
        ''' <param name="upperBound">The upper bound, in percent.</param>
        ''' <returns>The folder name.</returns>
        Public Shared Function FormatRangeFolderName(lowerBound As Integer, upperBound As Integer) As String
            Return String.Format(LightnessCalculator.FormatCulture, LightnessCalculator.RangeFolderNameFormat, lowerBound, upperBound)
        End Function

        ''' <summary>
        ''' Determines whether a folder name is a lightness range folder name that <see cref="GetFolderName"/>
        ''' can produce with any allowed interval (case-insensitive), i.e. a folder that holds already grouped images.
        ''' </summary>
        ''' <param name="folderName">The folder name (not a path).</param>
        ''' <returns><see langword="True"/> for a range folder name; otherwise, <see langword="False"/>.</returns>
        ''' <exception cref="ArgumentNullException">Thrown when <paramref name="folderName"/> is <see langword="Nothing"/>.</exception>
        Public Shared Function IsRangeFolderName(folderName As String) As Boolean
            If folderName Is Nothing Then
                Throw New ArgumentNullException(NameOf(folderName))
            End If

            Return LightnessCalculator.AllRangeFolderNames.Value.Contains(folderName)
        End Function

        ''' <summary>
        ''' Validates a lightness range width.
        ''' </summary>
        ''' <param name="stepPercentage">The range width, in percent points.</param>
        ''' <exception cref="ArgumentOutOfRangeException">
        ''' Thrown when <paramref name="stepPercentage"/> is outside <see cref="MinimumStepPercentage"/>-<see cref="MaximumStepPercentage"/>.
        ''' </exception>
        Public Shared Sub ValidateStepPercentage(stepPercentage As Integer)
            If stepPercentage < LightnessCalculator.MinimumStepPercentage OrElse stepPercentage > LightnessCalculator.MaximumStepPercentage Then
                Throw New ArgumentOutOfRangeException(NameOf(stepPercentage), stepPercentage,
                                                      $"The step percentage must be between {LightnessCalculator.MinimumStepPercentage} and {LightnessCalculator.MaximumStepPercentage}.")
            End If
        End Sub

        ''' <summary>
        ''' Validates an alpha threshold.
        ''' </summary>
        ''' <param name="alphaThreshold">The alpha threshold.</param>
        ''' <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="alphaThreshold"/> is not between 0 and 255.</exception>
        Public Shared Sub ValidateAlphaThreshold(alphaThreshold As Integer)
            If alphaThreshold < Byte.MinValue OrElse alphaThreshold > Byte.MaxValue Then
                Throw New ArgumentOutOfRangeException(NameOf(alphaThreshold), alphaThreshold, "The alpha threshold must be between 0 and 255.")
            End If
        End Sub

#End Region

#Region " Friend Methods "

        ''' <summary>
        ''' The CIE L*a*b* "f" function: the cube root above the CIE epsilon, and a linear slope below it,
        ''' which avoids the infinite gradient of the cube root at zero.
        ''' </summary>
        ''' <param name="value">The relative luminance (0.0 to 1.0).</param>
        ''' <returns>The transformed value.</returns>
        Friend Shared Function ComputeLabF(value As Double) As Double
            Const Epsilon As Double = 0.008856R
            Const Kappa As Double = 903.3R

            Return If(value > Epsilon,
                      Math.Pow(value, 1.0R / 3.0R),
                      ((Kappa * value) + LStarOffset) / LStarScale)
        End Function

        ''' <summary>
        ''' Builds the set of every range folder name, for every allowed interval and every integer percentage.
        ''' </summary>
        ''' <returns>A case-insensitive set of folder names.</returns>
        Private Shared Function BuildAllRangeFolderNames() As HashSet(Of String)
            Dim names As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
            For stepPercentage As Integer = LightnessCalculator.MinimumStepPercentage To LightnessCalculator.MaximumStepPercentage
                For percentage As Integer = 0 To 100
                    names.Add(LightnessCalculator.GetFolderName(percentage, stepPercentage))
                Next
            Next

            Return names
        End Function

        ''' <summary>
        ''' Builds the lookup table that converts an 8-bit sRGB channel value into linear light,
        ''' using the official sRGB transfer function (linear segment near black, 2.4 power curve above it).
        ''' </summary>
        ''' <returns>A read-only table of 256 values between 0.0 and 1.0.</returns>
        Friend Shared Function BuildLinearSrgbLookupTable() As ReadOnlyCollection(Of Double)
            Const GammaExponent As Double = 2.4R
            Const GammaThreshold As Double = 0.04045R
            Const LinearSlope As Double = 12.92R
            Const Offset As Double = 0.055R

            Dim table As Double() = New Double(255) {}
            For index As Integer = 0 To 255
                Dim value As Double = index / 255.0R
                table(index) = If(value <= GammaThreshold,
                                  value / LinearSlope,
                                  Math.Pow((value + Offset) / (1.0R + Offset), GammaExponent))
            Next

            Return Array.AsReadOnly(table)
        End Function

#End Region

    End Class

End Namespace
