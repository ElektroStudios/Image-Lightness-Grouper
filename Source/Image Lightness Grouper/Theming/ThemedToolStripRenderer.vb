Option Strict On
Option Explicit On
Option Infer Off

#Region " Imports "

Imports System.Drawing
Imports System.Windows.Forms

#End Region

Namespace ImageLightnessGrouper.Theming

    ''' <summary>
    ''' A flat <see cref="ToolStripProfessionalRenderer"/> that honors a <see cref="ThemePalette"/>
    ''' and suppresses the white borders/lines drawn by the default renderer.
    ''' </summary>
    Friend NotInheritable Class ThemedToolStripRenderer : Inherits ToolStripProfessionalRenderer

#Region " Read-only Fields "

        ''' <summary>
        ''' The source palette.
        ''' </summary>
        Private ReadOnly palette As ThemePalette

#End Region

#Region " Constructors "

        ''' <summary>
        ''' Initializes a new instance of the <see cref="ThemedToolStripRenderer"/> class.
        ''' </summary>
        ''' <param name="palette">The source palette.</param>
        ''' <exception cref="ArgumentNullException">Thrown when <paramref name="palette"/> is <see langword="Nothing"/>.</exception>
        Friend Sub New(palette As ThemePalette)
            MyBase.New(New ThemedColorTable(palette))
            Me.palette = palette
            Me.RoundedEdges = False
        End Sub

#End Region

#Region " Overridden Methods "

        ''' <inheritdoc/>
        Protected Overrides Sub OnRenderItemText(e As ToolStripItemTextRenderEventArgs)
            e.TextColor = If(e.Item.Enabled, Me.palette.Foreground, Me.palette.DisabledForeground)
            MyBase.OnRenderItemText(e)
        End Sub

        ''' <inheritdoc/>
        Protected Overrides Sub OnRenderArrow(e As ToolStripArrowRenderEventArgs)
            e.ArrowColor = If(e.Item Is Nothing OrElse e.Item.Enabled, Me.palette.Foreground, Me.palette.DisabledForeground)
            MyBase.OnRenderArrow(e)
        End Sub

        ''' <inheritdoc/>
        Protected Overrides Sub OnRenderToolStripBorder(e As ToolStripRenderEventArgs)
            ' Top-level strips (MenuStrip) get no border at all; drop-downs get a 1px palette border.
            If TypeOf e.ToolStrip Is ToolStripDropDown Then
                Using borderPen As New Pen(Me.palette.MenuBorder)
                    Dim bounds As New Rectangle(0, 0, e.ToolStrip.Width - 1, e.ToolStrip.Height - 1)
                    e.Graphics.DrawRectangle(borderPen, bounds)
                End Using
            End If
        End Sub

        ''' <inheritdoc/>
        Protected Overrides Sub OnRenderSeparator(e As ToolStripSeparatorRenderEventArgs)
            Dim bounds As Rectangle = e.Item.ContentRectangle
            Using separatorPen As New Pen(Me.palette.MenuBorder)
                If e.Vertical Then
                    Dim x As Integer = bounds.Left + (bounds.Width \ 2)
                    e.Graphics.DrawLine(separatorPen, x, bounds.Top, x, bounds.Bottom)
                Else
                    Dim y As Integer = bounds.Top + (bounds.Height \ 2)
                    e.Graphics.DrawLine(separatorPen, bounds.Left + 28, y, bounds.Right - 4, y)
                End If
            End Using
        End Sub

#End Region

    End Class

End Namespace
