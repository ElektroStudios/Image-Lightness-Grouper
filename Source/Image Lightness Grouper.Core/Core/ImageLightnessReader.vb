Option Strict On
Option Explicit On
Option Infer Off

#Region " Imports "

Imports System.Collections.ObjectModel
Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Drawing.Imaging
Imports System.IO
Imports System.Runtime.InteropServices

Imports SkiaSharp

#End Region

Namespace ImageLightnessGrouper.Core

    ''' <summary>
    ''' Decodes image files and computes their average lightness (see <see cref="LightnessCalculator"/>).
    ''' <para></para>
    ''' JPEG, PNG, BMP and TIFF files are decoded by GDI+. WebP, AVIF and ICO files are decoded by SkiaSharp,
    ''' because GDI+ cannot decode WebP and AVIF, and only decodes the first (often smallest) frame of an icon.
    ''' </summary>
    Public NotInheritable Class ImageLightnessReader

#Region " Constants and Read-only Fields "

        ''' <summary>
        ''' All the supported image file extensions.
        ''' </summary>
        Public Shared ReadOnly SupportedExtensions As ReadOnlyCollection(Of String) = Array.AsReadOnly(
            {".jpg", ".jpeg", ".png", ".bmp", ".tif", ".tiff", ".webp", ".avif", ".ico"})

        ''' <summary>
        ''' The image file extensions decoded by SkiaSharp instead of GDI+.
        ''' </summary>
        Private Shared ReadOnly SkiaDecodedExtensions As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase) From {
            ".webp", ".avif", ".ico"
        }

#End Region

#Region " Constructors "

        ''' <summary>
        ''' Prevents a default instance of the <see cref="ImageLightnessReader"/> class from being created.
        ''' </summary>
        Private Sub New()
        End Sub

#End Region

#Region " Friend Methods "

        ''' <summary>
        ''' Decodes an image file and computes its average lightness.
        ''' </summary>
        ''' <param name="filePath">The image file path.</param>
        ''' <param name="alphaThreshold">Pixels whose alpha is below this value (0-255) are ignored.</param>
        ''' <returns>The average lightness (CIE L*), from 0.0 to 100.0.</returns>
        ''' <exception cref="ArgumentException">Thrown when <paramref name="filePath"/> is empty, or the file is not a valid image.</exception>
        ''' <exception cref="InvalidOperationException">Thrown when SkiaSharp cannot decode the file.</exception>
        Friend Shared Function ComputeAverageLightness(filePath As String, alphaThreshold As Integer) As Double
            If String.IsNullOrWhiteSpace(filePath) Then
                Throw New ArgumentException("The file path cannot be empty.", NameOf(filePath))
            End If

            LightnessCalculator.ValidateAlphaThreshold(alphaThreshold)

            Return If(ImageLightnessReader.IsSkiaDecoded(filePath),
                      ImageLightnessReader.ComputeWithSkia(filePath, alphaThreshold),
                      ImageLightnessReader.ComputeWithGdi(filePath, alphaThreshold))
        End Function

        ''' <summary>
        ''' Ensures that the native SkiaSharp library (<c>libSkiaSharp.dll</c>) can be loaded.
        ''' <para></para>
        ''' The native library is only loaded the first time a WebP, AVIF or ICO file is decoded, so a missing or wrong
        ''' library would otherwise go unnoticed until then, and every such file would fail with a cryptic error.
        ''' Calling this at startup turns that into a single, explicit error.
        ''' </summary>
        ''' <exception cref="InvalidOperationException">Thrown when the native library cannot be loaded.</exception>
        Public Shared Sub EnsureSkiaSharpIsAvailable()
            Try
                ' Any SkiaSharp object creation goes through the native library.
                Using New SKBitmap(1, 1)
                End Using

            Catch ex As Exception When TypeOf ex Is DllNotFoundException OrElse
                                       TypeOf ex Is BadImageFormatException OrElse
                                       TypeOf ex Is EntryPointNotFoundException OrElse
                                       TypeOf ex Is TypeInitializationException
                Dim architectureFolder As String = If(Environment.Is64BitProcess, "x64", "x86")
                Throw New InvalidOperationException(
                    $"The native SkiaSharp library cannot be loaded, so WebP, AVIF and ICO files cannot be decoded.{Environment.NewLine}" &
                    $"Make sure that '{architectureFolder}{Path.DirectorySeparatorChar}libSkiaSharp.dll' exists next to the executable.{Environment.NewLine}{Environment.NewLine}" &
                    $"{ex.GetType().Name}: {ex.Message}", ex)
            End Try
        End Sub

        ''' <summary>
        ''' Determines whether the specified file is decoded by SkiaSharp instead of GDI+.
        ''' </summary>
        ''' <param name="filePath">The file path or name.</param>
        ''' <returns><see langword="True"/> for WebP, AVIF and ICO files; otherwise, <see langword="False"/>.</returns>
        ''' <exception cref="ArgumentNullException">Thrown when <paramref name="filePath"/> is <see langword="Nothing"/>.</exception>
        Friend Shared Function IsSkiaDecoded(filePath As String) As Boolean
            If filePath Is Nothing Then
                Throw New ArgumentNullException(NameOf(filePath))
            End If

            Return ImageLightnessReader.SkiaDecodedExtensions.Contains(Path.GetExtension(filePath))
        End Function

        ''' <summary>
        ''' Converts any image into a new 32-bit ARGB bitmap (the format required by <see cref="LightnessCalculator"/>).
        ''' </summary>
        ''' <param name="image">The source image.</param>
        ''' <returns>A new bitmap; the caller must dispose it.</returns>
        ''' <exception cref="ArgumentNullException">Thrown when <paramref name="image"/> is <see langword="Nothing"/>.</exception>
        Friend Shared Function ConvertToArgbBitmap(image As Image) As Bitmap
            If image Is Nothing Then
                Throw New ArgumentNullException(NameOf(image))
            End If

            Dim bitmap As New Bitmap(image.Width, image.Height, PixelFormat.Format32bppArgb)
            Try
                Using g As Graphics = Graphics.FromImage(bitmap)
                    g.CompositingMode = CompositingMode.SourceCopy
                    g.InterpolationMode = InterpolationMode.NearestNeighbor
                    g.PixelOffsetMode = PixelOffsetMode.HighSpeed
                    g.CompositingQuality = CompositingQuality.HighSpeed
                    g.DrawImage(image, 0, 0, image.Width, image.Height)
                End Using

                Return bitmap

            Catch
                bitmap.Dispose()
                Throw
            End Try
        End Function

#End Region

#Region " Private Methods "

        ''' <summary>
        ''' Decodes the image with GDI+ and computes its average lightness.
        ''' </summary>
        ''' <param name="filePath">The image file path.</param>
        ''' <param name="alphaThreshold">The alpha threshold.</param>
        ''' <returns>The average lightness.</returns>
        Private Shared Function ComputeWithGdi(filePath As String, alphaThreshold As Integer) As Double
            Using stream As New FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read)
                ' validateImageData:=True decodes the whole image, so corrupt files fail here with a clear error.
                Using image As Image = Image.FromStream(stream, useEmbeddedColorManagement:=False, validateImageData:=True)
                    Dim bitmap As Bitmap = TryCast(image, Bitmap)
                    If bitmap IsNot Nothing AndAlso bitmap.PixelFormat = PixelFormat.Format32bppArgb Then
                        Return LightnessCalculator.ComputeAverageLightness(bitmap, alphaThreshold)
                    End If

                    Using argbBitmap As Bitmap = ImageLightnessReader.ConvertToArgbBitmap(image)
                        Return LightnessCalculator.ComputeAverageLightness(argbBitmap, alphaThreshold)
                    End Using
                End Using
            End Using
        End Function

        ''' <summary>
        ''' Decodes the image with SkiaSharp and computes its average lightness.
        ''' </summary>
        ''' <param name="filePath">The image file path.</param>
        ''' <param name="alphaThreshold">The alpha threshold.</param>
        ''' <returns>The average lightness.</returns>
        ''' <exception cref="InvalidOperationException">Thrown when SkiaSharp cannot decode the file.</exception>
        Private Shared Function ComputeWithSkia(filePath As String, alphaThreshold As Integer) As Double
            Using stream As New FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read)
                Dim codecResult As SKCodecResult
                Using codec As SKCodec = SKCodec.Create(stream, codecResult)
                    If codec Is Nothing OrElse codecResult <> SKCodecResult.Success Then
                        Throw New InvalidOperationException($"SkiaSharp cannot decode the file (codec result: {codecResult}). The file may be corrupt, or its extension may not match its format.")
                    End If

                    ' BGRA with unpremultiplied alpha has exactly the byte layout of a GDI+ Format32bppArgb bitmap.
                    Dim decodeInfo As New SKImageInfo(codec.Info.Width, codec.Info.Height, SKColorType.Bgra8888, SKAlphaType.Unpremul)
                    Using skiaBitmap As New SKBitmap(decodeInfo)
                        Dim decodeResult As SKCodecResult = codec.GetPixels(decodeInfo, skiaBitmap.GetPixels())
                        If decodeResult <> SKCodecResult.Success Then
                            Throw New InvalidOperationException($"SkiaSharp cannot decode the image pixels (codec result: {decodeResult}).")
                        End If

                        Using gdiBitmap As New Bitmap(decodeInfo.Width, decodeInfo.Height, PixelFormat.Format32bppArgb)
                            ImageLightnessReader.CopyPixels(skiaBitmap, gdiBitmap)
                            Return LightnessCalculator.ComputeAverageLightness(gdiBitmap, alphaThreshold)
                        End Using
                    End Using
                End Using
            End Using
        End Function

        ''' <summary>
        ''' Copies the pixels of a BGRA SkiaSharp bitmap into a GDI+ 32-bit ARGB bitmap of the same size, row by row.
        ''' </summary>
        ''' <param name="source">The source bitmap.</param>
        ''' <param name="destination">The destination bitmap.</param>
        Private Shared Sub CopyPixels(source As SKBitmap, destination As Bitmap)
            Dim bitmapData As BitmapData = destination.LockBits(New Rectangle(0, 0, destination.Width, destination.Height),
                                                                ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb)
            Try
                Dim sourcePointer As IntPtr = source.GetPixels()
                Dim rowLength As Integer = destination.Width * 4
                Dim row As Byte() = New Byte(rowLength - 1) {}

                For y As Integer = 0 To destination.Height - 1
                    Marshal.Copy(IntPtr.Add(sourcePointer, y * source.RowBytes), row, 0, rowLength)
                    Marshal.Copy(row, 0, IntPtr.Add(bitmapData.Scan0, y * bitmapData.Stride), rowLength)
                Next

            Finally
                destination.UnlockBits(bitmapData)
            End Try
        End Sub

#End Region

    End Class

End Namespace
