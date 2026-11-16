Option Strict On
Option Explicit On
Option Infer Off

Namespace ImageLightnessGrouper.Tests.TestInfrastructure

    ''' <summary>
    ''' An independent, deliberately straightforward implementation of the sRGB to CIE L* conversion
    ''' (no lookup table, no shared code with the application), used as the reference in the tests.
    ''' </summary>
    Public NotInheritable Class ReferenceLightness

#Region " Constructors "

        ''' <summary>
        ''' Prevents a default instance of the <see cref="ReferenceLightness"/> class from being created.
        ''' </summary>
        Private Sub New()
        End Sub

#End Region

#Region " Public Methods "

        ''' <summary>
        ''' Converts an 8-bit sRGB channel value into linear light.
        ''' </summary>
        ''' <param name="channel">The channel value (0-255).</param>
        ''' <returns>The linear value (0.0 to 1.0).</returns>
        Public Shared Function ToLinear(channel As Integer) As Double
            Dim value As Double = channel / 255.0R
            If value <= 0.04045R Then
                Return value / 12.92R
            End If

            Return Math.Pow((value + 0.055R) / 1.055R, 2.4R)
        End Function

        ''' <summary>
        ''' Converts a relative luminance into CIE L*.
        ''' </summary>
        ''' <param name="luminance">The relative luminance (0.0 to 1.0).</param>
        ''' <returns>The lightness (0.0 to 100.0).</returns>
        Public Shared Function FromLuminance(luminance As Double) As Double
            ' CIE: L* = 116 * f(Y) - 16, where f is the cube root above (6/29)^3 and a linear slope below it.
            If luminance > 0.008856R Then
                Return (116.0R * Math.Pow(luminance, 1.0R / 3.0R)) - 16.0R
            End If

            Return 903.3R * luminance
        End Function

        ''' <summary>
        ''' Computes the CIE L* of an sRGB color.
        ''' </summary>
        ''' <param name="red">The red channel (0-255).</param>
        ''' <param name="green">The green channel (0-255).</param>
        ''' <param name="blue">The blue channel (0-255).</param>
        ''' <returns>The lightness (0.0 to 100.0).</returns>
        Public Shared Function FromRgb(red As Integer, green As Integer, blue As Integer) As Double
            Dim luminance As Double = (0.2126R * ReferenceLightness.ToLinear(red)) +
                                      (0.7152R * ReferenceLightness.ToLinear(green)) +
                                      (0.0722R * ReferenceLightness.ToLinear(blue))

            Return ReferenceLightness.FromLuminance(luminance)
        End Function

#End Region

    End Class

End Namespace
