Option Strict On
Option Explicit On
Option Infer Off

#Region " Imports "

Imports System.Drawing
Imports System.Drawing.Drawing2D

#End Region

Namespace ImageLightnessGrouper.Controls

    ''' <summary>
    ''' GDI+ drawing helpers shared by the custom-painted controls.
    ''' </summary>
    Friend NotInheritable Class DrawingHelper

#Region " Constructors "

        ''' <summary>
        ''' Prevents a default instance of the <see cref="DrawingHelper"/> class from being created.
        ''' </summary>
        Private Sub New()
        End Sub

#End Region

#Region " Public Methods "

        ''' <summary>
        ''' Creates a rounded rectangle path. The caller owns (and must dispose) the returned path.
        ''' </summary>
        ''' <param name="bounds">The rectangle bounds.</param>
        ''' <param name="radius">The corner radius. It is clamped to half of the shortest side.</param>
        ''' <returns>A closed <see cref="GraphicsPath"/>.</returns>
        ''' <exception cref="ArgumentOutOfRangeException">Thrown when the bounds are empty or the radius is negative.</exception>
        Friend Shared Function CreateRoundedRectangle(bounds As RectangleF, radius As Single) As GraphicsPath
            If bounds.Width <= 0.0F OrElse bounds.Height <= 0.0F Then
                Throw New ArgumentOutOfRangeException(NameOf(bounds), bounds, "Bounds must have a positive width and height.")
            End If

            If radius < 0.0F Then
                Throw New ArgumentOutOfRangeException(NameOf(radius), radius, "Radius cannot be negative.")
            End If

            Dim path As New GraphicsPath()
            Dim effectiveRadius As Single = Math.Min(radius, Math.Min(bounds.Width, bounds.Height) / 2.0F)

            If effectiveRadius <= 0.0F Then
                path.AddRectangle(bounds)
                Return path
            End If

            Dim diameter As Single = effectiveRadius * 2.0F
            path.AddArc(bounds.Left, bounds.Top, diameter, diameter, 180.0F, 90.0F)
            path.AddArc(bounds.Right - diameter, bounds.Top, diameter, diameter, 270.0F, 90.0F)
            path.AddArc(bounds.Right - diameter, bounds.Bottom - diameter, diameter, diameter, 0.0F, 90.0F)
            path.AddArc(bounds.Left, bounds.Bottom - diameter, diameter, diameter, 90.0F, 90.0F)
            path.CloseFigure()

            Return path
        End Function

#End Region

    End Class

End Namespace
