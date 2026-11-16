Option Strict On
Option Explicit On
Option Infer Off

#Region " Imports "

Imports System.Drawing
Imports System.Drawing.Imaging
Imports System.IO

Imports ImageLightnessGrouper.Core
Imports ImageLightnessGrouper.Tests.TestInfrastructure

#End Region

Namespace ImageLightnessGrouper.Tests.Core

    ''' <summary>
    ''' Tests for <see cref="ImageLightnessReader"/>, using real image files of every supported decoder path.
    ''' </summary>
    Public NotInheritable Class ImageLightnessReaderTests

#Region " Decoding "

        ''' <summary>
        ''' Lossless formats decoded by GDI+ produce exactly the reference lightness.
        ''' </summary>
        <Theory>
        <InlineData("png", 128, 128, 128)>
        <InlineData("png", 200, 50, 10)>
        <InlineData("bmp", 10, 50, 200)>
        <InlineData("tif", 77, 150, 33)>
        <InlineData("PNG", 255, 255, 255)>
        Public Sub ComputeAverageLightness_LosslessGdiFormats_MatchTheReference(extension As String, red As Integer, green As Integer, blue As Integer)
            Using temp As New TemporaryDirectory()
                Dim filePath As String = TestImageFactory.CreateSolidImage(temp.Combine($"image.{extension}"), 8, 6, Color.FromArgb(red, green, blue))

                Assert.Equal(ReferenceLightness.FromRgb(red, green, blue), ImageLightnessReader.ComputeAverageLightness(filePath, 8), 6)
            End Using
        End Sub

        ''' <summary>
        ''' JPEG is lossy: the result is close to the reference.
        ''' </summary>
        <Theory>
        <InlineData("jpg")>
        <InlineData("jpeg")>
        Public Sub ComputeAverageLightness_Jpeg_IsCloseToTheReference(extension As String)
            Using temp As New TemporaryDirectory()
                Dim filePath As String = TestImageFactory.CreateSolidImage(temp.Combine($"image.{extension}"), 16, 16, Color.FromArgb(128, 128, 128))

                Assert.InRange(ImageLightnessReader.ComputeAverageLightness(filePath, 8), ReferenceLightness.FromRgb(128, 128, 128) - 1.5R, ReferenceLightness.FromRgb(128, 128, 128) + 1.5R)
            End Using
        End Sub

        ''' <summary>
        ''' Lossless WebP and ICO files are decoded by SkiaSharp and produce the reference lightness.
        ''' </summary>
        <Theory>
        <InlineData("webp", 128, 128, 128)>
        <InlineData("webp", 200, 50, 10)>
        <InlineData("WEBP", 0, 0, 0)>
        <InlineData("ico", 10, 50, 200)>
        <InlineData("ico", 255, 255, 255)>
        Public Sub ComputeAverageLightness_SkiaFormats_MatchTheReference(extension As String, red As Integer, green As Integer, blue As Integer)
            Using temp As New TemporaryDirectory()
                Dim filePath As String = TestImageFactory.CreateSolidImage(temp.Combine($"image.{extension}"), 8, 6, Color.FromArgb(red, green, blue))

                Assert.True(ImageLightnessReader.IsSkiaDecoded(filePath))
                Assert.Equal(ReferenceLightness.FromRgb(red, green, blue), ImageLightnessReader.ComputeAverageLightness(filePath, 8), 6)
            End Using
        End Sub

        ''' <summary>
        ''' Semi-transparent pixels decoded by SkiaSharp are not darkened by premultiplied alpha,
        ''' and the alpha threshold is applied.
        ''' </summary>
        <Fact>
        Public Sub ComputeAverageLightness_SemiTransparentWebP_UsesStraightColors()
            Using temp As New TemporaryDirectory()
                Dim filePath As String = TestImageFactory.CreateSolidImage(temp.Combine("alpha.webp"), 4, 4, Color.FromArgb(128, 255, 255, 255))

                Assert.Equal(100.0R, ImageLightnessReader.ComputeAverageLightness(filePath, 8), 6)
                Assert.Equal(0.0R, ImageLightnessReader.ComputeAverageLightness(filePath, 129), 6)
            End Using
        End Sub

        ''' <summary>
        ''' A transparent PNG is decoded with its alpha channel: fully transparent pixels are ignored.
        ''' </summary>
        <Fact>
        Public Sub ComputeAverageLightness_TransparentPng_IsIgnoredByTheThreshold()
            Using temp As New TemporaryDirectory()
                Dim filePath As String = TestImageFactory.CreateSolidImage(temp.Combine("transparent.png"), 4, 4, Color.FromArgb(0, 255, 255, 255))

                Assert.Equal(0.0R, ImageLightnessReader.ComputeAverageLightness(filePath, 8), 6)
                Assert.Equal(100.0R, ImageLightnessReader.ComputeAverageLightness(filePath, 0), 6)
            End Using
        End Sub

#End Region

#Region " Failures "

        ''' <summary>
        ''' A corrupt file decoded by GDI+ fails with the "invalid argument" HRESULT handled by the service.
        ''' </summary>
        <Theory>
        <InlineData("broken.png")>
        <InlineData("broken.jpg")>
        Public Sub ComputeAverageLightness_CorruptGdiFile_ThrowsInvalidArgument(fileName As String)
            Using temp As New TemporaryDirectory()
                Dim filePath As String = TestImageFactory.CreateCorruptImage(temp.Combine(fileName))

                Dim exception As ArgumentException = Assert.Throws(Of ArgumentException)(Function() ImageLightnessReader.ComputeAverageLightness(filePath, 8))
                Assert.Equal(&H80070057, exception.HResult)
            End Using
        End Sub

        ''' <summary>
        ''' A corrupt file decoded by SkiaSharp fails with an explicit error.
        ''' </summary>
        <Theory>
        <InlineData("broken.webp")>
        <InlineData("broken.ico")>
        <InlineData("broken.avif")>
        Public Sub ComputeAverageLightness_CorruptSkiaFile_ThrowsInvalidOperation(fileName As String)
            Using temp As New TemporaryDirectory()
                Dim filePath As String = TestImageFactory.CreateCorruptImage(temp.Combine(fileName))

                Assert.Throws(Of InvalidOperationException)(Function() ImageLightnessReader.ComputeAverageLightness(filePath, 8))
            End Using
        End Sub

        ''' <summary>
        ''' The file is released after both a success and a failure, so it can be moved right away.
        ''' </summary>
        <Theory>
        <InlineData("image.png", False)>
        <InlineData("image.webp", False)>
        <InlineData("broken.png", True)>
        <InlineData("broken.webp", True)>
        Public Sub ComputeAverageLightness_ReleasesTheFile(fileName As String, isCorrupt As Boolean)
            Using temp As New TemporaryDirectory()
                Dim filePath As String = If(isCorrupt,
                                            TestImageFactory.CreateCorruptImage(temp.Combine(fileName)),
                                            TestImageFactory.CreateImage(temp.Combine(fileName), 4, 4))

                Try
                    ImageLightnessReader.ComputeAverageLightness(filePath, 8)
                Catch ex As Exception When isCorrupt AndAlso (TypeOf ex Is ArgumentException OrElse TypeOf ex Is InvalidOperationException)
                    ' Expected for the corrupt files: the point of this test is the file handle.
                End Try

                Dim movedPath As String = temp.Combine($"moved-{fileName}")
                File.Move(filePath, movedPath)
                Assert.True(File.Exists(movedPath))
            End Using
        End Sub

        ''' <summary>
        ''' Invalid arguments fail fast.
        ''' </summary>
        <Fact>
        Public Sub ComputeAverageLightness_InvalidArguments_Throw()
            Assert.Throws(Of ArgumentException)(Function() ImageLightnessReader.ComputeAverageLightness(" ", 8))
            Assert.Throws(Of ArgumentException)(Function() ImageLightnessReader.ComputeAverageLightness(Nothing, 8))
            Assert.Throws(Of ArgumentOutOfRangeException)(Function() ImageLightnessReader.ComputeAverageLightness("image.png", 300))
            Assert.Throws(Of ArgumentNullException)(Function() ImageLightnessReader.IsSkiaDecoded(Nothing))
        End Sub

#End Region

#Region " Native library "

        ''' <summary>
        ''' The native SkiaSharp library of the current architecture is available.
        ''' </summary>
        <Fact>
        Public Sub EnsureSkiaSharpIsAvailable_DoesNotThrow()
            ImageLightnessReader.EnsureSkiaSharpIsAvailable()
        End Sub

#End Region

#Region " Helpers "

        ''' <summary>
        ''' Only WebP, AVIF and ICO are decoded by SkiaSharp, case-insensitively.
        ''' </summary>
        <Theory>
        <InlineData("a.webp", True)>
        <InlineData("a.AVIF", True)>
        <InlineData("a.Ico", True)>
        <InlineData("a.png", False)>
        <InlineData("a.jpg", False)>
        <InlineData("a.tiff", False)>
        <InlineData("a", False)>
        Public Sub IsSkiaDecoded(fileName As String, expected As Boolean)
            Assert.Equal(expected, ImageLightnessReader.IsSkiaDecoded(fileName))
        End Sub

        ''' <summary>
        ''' Any pixel format is converted to 32-bit ARGB without changing the colors.
        ''' </summary>
        <Theory>
        <InlineData(PixelFormat.Format24bppRgb)>
        <InlineData(PixelFormat.Format32bppRgb)>
        <InlineData(PixelFormat.Format32bppPArgb)>
        Public Sub ConvertToArgbBitmap_KeepsTheColors(pixelFormat As PixelFormat)
            Using source As New Bitmap(3, 3, pixelFormat)
                Using g As Graphics = Graphics.FromImage(source)
                    g.Clear(Color.FromArgb(200, 50, 10))
                End Using

                Using converted As Bitmap = ImageLightnessReader.ConvertToArgbBitmap(source)
                    Assert.Equal(PixelFormat.Format32bppArgb, converted.PixelFormat)
                    Assert.Equal(ReferenceLightness.FromRgb(200, 50, 10), LightnessCalculator.ComputeAverageLightness(converted, 8), 6)
                End Using
            End Using
        End Sub

        ''' <summary>
        ''' The supported extensions are exactly the ones of the console version.
        ''' </summary>
        <Fact>
        Public Sub SupportedExtensions_AreTheExpectedOnes()
            Assert.Equal(New String() {".avif", ".bmp", ".ico", ".jpeg", ".jpg", ".png", ".tif", ".tiff", ".webp"},
                         ImageLightnessReader.SupportedExtensions.OrderBy(Function(extension As String) extension, StringComparer.Ordinal))
        End Sub

#End Region

    End Class

End Namespace
