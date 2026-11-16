Option Strict On
Option Explicit On
Option Infer Off

#Region " Imports "

Imports System.Drawing
Imports System.Drawing.Imaging
Imports System.IO

Imports SkiaSharp

#End Region

Namespace ImageLightnessGrouper.Tests.TestInfrastructure

    ''' <summary>
    ''' Creates real image files (and fake ones) for the tests.
    ''' </summary>
    Public NotInheritable Class TestImageFactory

#Region " Constructors "

        ''' <summary>
        ''' Prevents a default instance of the <see cref="TestImageFactory"/> class from being created.
        ''' </summary>
        Private Sub New()
        End Sub

#End Region

#Region " Public Methods "

        ''' <summary>
        ''' Creates a solid mid-gray image (CIE L* ≈ 53.6), in the format given by the file extension.
        ''' </summary>
        ''' <param name="filePath">The file path; the parent directory is created if needed.</param>
        ''' <param name="width">The width, in pixels.</param>
        ''' <param name="height">The height, in pixels.</param>
        ''' <returns><paramref name="filePath"/>.</returns>
        Public Shared Function CreateImage(filePath As String, width As Integer, height As Integer) As String
            Return TestImageFactory.CreateSolidImage(filePath, width, height, Color.FromArgb(128, 128, 128))
        End Function

        ''' <summary>
        ''' Creates a solid-color image in the format given by the file extension
        ''' (PNG, BMP, TIFF and JPEG through GDI+; lossless WebP through SkiaSharp; ICO with an embedded PNG).
        ''' </summary>
        ''' <param name="filePath">The file path; the parent directory is created if needed.</param>
        ''' <param name="width">The width, in pixels.</param>
        ''' <param name="height">The height, in pixels.</param>
        ''' <param name="color">The color, including its alpha (alpha is preserved by PNG, WebP and ICO).</param>
        ''' <returns><paramref name="filePath"/>.</returns>
        ''' <exception cref="ArgumentException">Thrown when the extension is not supported by this factory.</exception>
        Public Shared Function CreateSolidImage(filePath As String, width As Integer, height As Integer, color As Color) As String
            Directory.CreateDirectory(Path.GetDirectoryName(filePath))

            Select Case Path.GetExtension(filePath).ToLowerInvariant()
                Case ".webp"
                    File.WriteAllBytes(filePath, TestImageFactory.EncodeLosslessWebP(width, height, color))
                Case ".ico"
                    File.WriteAllBytes(filePath, TestImageFactory.EncodeIcoWithPng(width, height, color))
                Case Else
                    Using bitmap As Bitmap = TestImageFactory.CreateSolidBitmap(width, height, color)
                        bitmap.Save(filePath, TestImageFactory.GetGdiFormat(filePath))
                    End Using
            End Select

            Return filePath
        End Function

        ''' <summary>
        ''' Creates a new 32-bit ARGB bitmap filled with a single color (alpha included).
        ''' </summary>
        ''' <param name="width">The width, in pixels.</param>
        ''' <param name="height">The height, in pixels.</param>
        ''' <param name="color">The color.</param>
        ''' <returns>The bitmap; the caller must dispose it.</returns>
        Public Shared Function CreateSolidBitmap(width As Integer, height As Integer, color As Color) As Bitmap
            Dim bitmap As New Bitmap(width, height, PixelFormat.Format32bppArgb)
            For y As Integer = 0 To height - 1
                For x As Integer = 0 To width - 1
                    bitmap.SetPixel(x, y, color)
                Next
            Next

            Return bitmap
        End Function

        ''' <summary>
        ''' Creates a file that has an image extension but is not an image.
        ''' </summary>
        ''' <param name="filePath">The file path; the parent directory is created if needed.</param>
        ''' <returns><paramref name="filePath"/>.</returns>
        Public Shared Function CreateCorruptImage(filePath As String) As String
            Directory.CreateDirectory(Path.GetDirectoryName(filePath))
            File.WriteAllText(filePath, "This is definitely not an image.")
            Return filePath
        End Function

        ''' <summary>
        ''' Creates a small text file.
        ''' </summary>
        ''' <param name="filePath">The file path; the parent directory is created if needed.</param>
        ''' <returns><paramref name="filePath"/>.</returns>
        Public Shared Function CreateTextFile(filePath As String) As String
            Directory.CreateDirectory(Path.GetDirectoryName(filePath))
            File.WriteAllText(filePath, "text")
            Return filePath
        End Function

#End Region

#Region " Private Methods "

        ''' <summary>
        ''' Encodes a solid-color image as lossless WebP.
        ''' </summary>
        ''' <param name="width">The width, in pixels.</param>
        ''' <param name="height">The height, in pixels.</param>
        ''' <param name="color">The color.</param>
        ''' <returns>The encoded bytes.</returns>
        Private Shared Function EncodeLosslessWebP(width As Integer, height As Integer, color As Color) As Byte()
            Dim info As New SKImageInfo(width, height, SKColorType.Bgra8888, SKAlphaType.Unpremul)
            Using bitmap As New SKBitmap(info)
                bitmap.Erase(New SKColor(color.R, color.G, color.B, color.A))

                Using pixmap As SKPixmap = bitmap.PeekPixels(),
                      data As SKData = pixmap.Encode(New SKWebpEncoderOptions(SKWebpEncoderCompression.Lossless, 100.0F))

                    If data Is Nothing Then
                        Throw New InvalidOperationException("SkiaSharp could not encode the WebP test image.")
                    End If

                    Return data.ToArray()
                End Using
            End Using
        End Function

        ''' <summary>
        ''' Encodes a solid-color image as an ICO file with a single PNG-compressed entry.
        ''' </summary>
        ''' <param name="width">The width, in pixels (1-256).</param>
        ''' <param name="height">The height, in pixels (1-256).</param>
        ''' <param name="color">The color.</param>
        ''' <returns>The encoded bytes.</returns>
        Private Shared Function EncodeIcoWithPng(width As Integer, height As Integer, color As Color) As Byte()
            Dim pngBytes As Byte()
            Using bitmap As Bitmap = TestImageFactory.CreateSolidBitmap(width, height, color),
                  pngStream As New MemoryStream()

                bitmap.Save(pngStream, ImageFormat.Png)
                pngBytes = pngStream.ToArray()
            End Using

            Using stream As New MemoryStream(),
                  writer As New BinaryWriter(stream)

                ' ICONDIR.
                writer.Write(CUShort(0))     ' Reserved.
                writer.Write(CUShort(1))     ' Type: icon.
                writer.Write(CUShort(1))     ' Image count.

                ' ICONDIRENTRY (0 means 256).
                writer.Write(CByte(If(width >= 256, 0, width)))
                writer.Write(CByte(If(height >= 256, 0, height)))
                writer.Write(CByte(0))       ' Palette colors.
                writer.Write(CByte(0))       ' Reserved.
                writer.Write(CUShort(1))     ' Color planes.
                writer.Write(CUShort(32))    ' Bits per pixel.
                writer.Write(CUInt(pngBytes.Length))
                writer.Write(CUInt(6 + 16))  ' Offset of the image data.

                writer.Write(pngBytes)
                writer.Flush()
                Return stream.ToArray()
            End Using
        End Function

        ''' <summary>
        ''' Gets the GDI+ image format for a file extension.
        ''' </summary>
        ''' <param name="filePath">The file path.</param>
        ''' <returns>The image format.</returns>
        ''' <exception cref="ArgumentException">Thrown when the extension is not supported by this factory.</exception>
        Private Shared Function GetGdiFormat(filePath As String) As ImageFormat
            Select Case Path.GetExtension(filePath).ToLowerInvariant()
                Case ".png"
                    Return ImageFormat.Png
                Case ".jpg", ".jpeg"
                    Return ImageFormat.Jpeg
                Case ".bmp"
                    Return ImageFormat.Bmp
                Case ".tif", ".tiff"
                    Return ImageFormat.Tiff
                Case Else
                    Throw New ArgumentException($"Unsupported test image extension: {filePath}", NameOf(filePath))
            End Select
        End Function

#End Region

    End Class

End Namespace
