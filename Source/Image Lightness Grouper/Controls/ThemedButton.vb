Option Strict On
Option Explicit On
Option Infer Off

#Region " Imports "

Imports System.ComponentModel
Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Windows.Forms

Imports ImageLightnessGrouper.Theming

#End Region

Namespace ImageLightnessGrouper.Controls

    ''' <summary>
    ''' A fully owner-drawn <see cref="Button"/> without the white borders of the system-rendered buttons.
    ''' </summary>
    <ToolboxItem(True)>
    Public Class ThemedButton : Inherits Button : Implements IThemeable

#Region " Constants "

        ''' <summary>
        ''' The corner radius of the button.
        ''' </summary>
        Private Const CornerRadius As Single = 4.0F

#End Region

#Region " Fields "

        ''' <summary>
        ''' Backing field for <see cref="ButtonStyle"/>.
        ''' </summary>
        Private buttonStyleValue As ThemedButtonStyle = ThemedButtonStyle.Standard

        ''' <summary>
        ''' Whether the mouse pointer is over the button.
        ''' </summary>
        Private isHovered As Boolean

        ''' <summary>
        ''' Whether the left mouse button is pressed over the button.
        ''' </summary>
        Private isPressed As Boolean

        ''' <summary>
        ''' The palette currently applied.
        ''' </summary>
        Private palette As ThemePalette = ThemeManager.CurrentPalette

#End Region

#Region " Constructors "

        ''' <summary>
        ''' Initializes a new instance of the <see cref="ThemedButton"/> class.
        ''' </summary>
        Public Sub New()
            Me.SetStyle(ControlStyles.UserPaint Or
                        ControlStyles.AllPaintingInWmPaint Or
                        ControlStyles.OptimizedDoubleBuffer Or
                        ControlStyles.ResizeRedraw, True)

            MyBase.FlatStyle = FlatStyle.Flat
            Me.UseVisualStyleBackColor = False
            Me.ApplyTheme(Me.palette)
        End Sub

#End Region

#Region " Properties "

        ''' <summary>
        ''' Gets or sets the visual role of the button.
        ''' </summary>
        <Category("Appearance")>
        <Description("The visual role of the button.")>
        <DefaultValue(GetType(ThemedButtonStyle), "Standard")>
        Public Property ButtonStyle As ThemedButtonStyle
            Get
                Return Me.buttonStyleValue
            End Get
            Set(value As ThemedButtonStyle)
                If Not [Enum].IsDefined(GetType(ThemedButtonStyle), value) Then
                    Throw New InvalidEnumArgumentException(NameOf(value), CInt(value), GetType(ThemedButtonStyle))
                End If

                Me.buttonStyleValue = value
                Me.Invalidate()
            End Set
        End Property

        ''' <summary>
        ''' This property is fixed to <see cref="FlatStyle.Flat"/> because the control is fully owner-drawn.
        ''' </summary>
        <Browsable(False)>
        <EditorBrowsable(EditorBrowsableState.Never)>
        <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
        Public Shadows Property FlatStyle As FlatStyle
            Get
                Return MyBase.FlatStyle
            End Get
            Set(value As FlatStyle)
                If value <> FlatStyle.Flat Then
                    Throw New NotSupportedException($"{NameOf(ThemedButton)} is owner-drawn and only supports {NameOf(FlatStyle.Flat)}.")
                End If

                MyBase.FlatStyle = value
            End Set
        End Property

#End Region

#Region " IThemeable "

        ''' <inheritdoc/>
        Public Sub ApplyTheme(palette As ThemePalette) Implements IThemeable.ApplyTheme
            If palette Is Nothing Then
                Throw New ArgumentNullException(NameOf(palette))
            End If

            Me.palette = palette
            Me.BackColor = palette.WindowBackground
            Me.ForeColor = palette.Foreground
            Me.Invalidate()
        End Sub

#End Region

#Region " Overridden Methods "

        ''' <inheritdoc/>
        Protected Overrides Sub OnPaint(e As PaintEventArgs)
            Dim g As Graphics = e.Graphics
            g.SmoothingMode = SmoothingMode.AntiAlias
            g.Clear(Me.palette.WindowBackground)

            Dim fillColor As Color
            Dim borderColor As Color
            Dim textColor As Color
            Me.ResolveColors(fillColor, borderColor, textColor)

            Dim bounds As New RectangleF(0.5F, 0.5F, Me.ClientSize.Width - 1.0F, Me.ClientSize.Height - 1.0F)
            If bounds.Width <= 0.0F OrElse bounds.Height <= 0.0F Then
                Return ' Too small to draw anything meaningful (e.g. while being resized in the designer).
            End If

            Using path As GraphicsPath = DrawingHelper.CreateRoundedRectangle(bounds, CornerRadius),
                  fillBrush As New SolidBrush(fillColor),
                  borderPen As New Pen(borderColor, 1.0F)

                g.FillPath(fillBrush, path)
                g.DrawPath(borderPen, path)
            End Using

            Dim textFlags As TextFormatFlags = TextFormatFlags.HorizontalCenter Or
                                               TextFormatFlags.VerticalCenter Or
                                               TextFormatFlags.SingleLine Or
                                               TextFormatFlags.EndEllipsis
            If Not Me.ShowKeyboardCues Then
                textFlags = textFlags Or TextFormatFlags.HidePrefix
            End If

            TextRenderer.DrawText(g, Me.Text, Me.Font, Me.ClientRectangle, textColor, textFlags)

            ' Accent/danger buttons are already filled with a strong color: their focus cue is a thin inner ring
            ' (standard buttons show their focus with an accent border, exactly like the input controls).
            If Me.Focused AndAlso Me.Enabled AndAlso Me.buttonStyleValue <> ThemedButtonStyle.Standard Then
                Dim focusBounds As RectangleF = RectangleF.Inflate(bounds, -2.0F, -2.0F)
                If focusBounds.Width > 0.0F AndAlso focusBounds.Height > 0.0F Then
                    Using focusPath As GraphicsPath = DrawingHelper.CreateRoundedRectangle(focusBounds, CornerRadius - 1.0F),
                          focusPen As New Pen(Color.FromArgb(160, textColor), 1.0F)

                        g.DrawPath(focusPen, focusPath)
                    End Using
                End If
            End If
        End Sub

        ''' <inheritdoc/>
        Protected Overrides Sub OnMouseEnter(e As EventArgs)
            Me.isHovered = True
            Me.Invalidate()
            MyBase.OnMouseEnter(e)
        End Sub

        ''' <inheritdoc/>
        Protected Overrides Sub OnMouseLeave(e As EventArgs)
            Me.isHovered = False
            Me.isPressed = False
            Me.Invalidate()
            MyBase.OnMouseLeave(e)
        End Sub

        ''' <inheritdoc/>
        Protected Overrides Sub OnMouseDown(mevent As MouseEventArgs)
            If mevent.Button = MouseButtons.Left Then
                Me.isPressed = True
                Me.Invalidate()
            End If

            MyBase.OnMouseDown(mevent)
        End Sub

        ''' <inheritdoc/>
        Protected Overrides Sub OnMouseUp(mevent As MouseEventArgs)
            Me.isPressed = False
            Me.Invalidate()
            MyBase.OnMouseUp(mevent)
        End Sub

        ''' <inheritdoc/>
        Protected Overrides Sub OnEnabledChanged(e As EventArgs)
            If Not Me.Enabled Then
                Me.isHovered = False
                Me.isPressed = False
            End If

            Me.Invalidate()
            MyBase.OnEnabledChanged(e)
        End Sub

#End Region

#Region " Private Methods "

        ''' <summary>
        ''' Resolves the fill, border and text colors for the current style and interaction state.
        ''' </summary>
        ''' <param name="fillColor">Receives the fill color.</param>
        ''' <param name="borderColor">Receives the border color.</param>
        ''' <param name="textColor">Receives the text color.</param>
        ''' <exception cref="InvalidOperationException">Thrown when the button style is not handled.</exception>
        Private Sub ResolveColors(ByRef fillColor As Color, ByRef borderColor As Color, ByRef textColor As Color)

            If Not Me.Enabled Then
                fillColor = Me.palette.DisabledBackground
                borderColor = Me.palette.Border
                textColor = Me.palette.DisabledForeground
                Return
            End If

            Select Case Me.buttonStyleValue

                Case ThemedButtonStyle.Standard
                    fillColor = If(Me.isPressed, Me.palette.ButtonPressed, If(Me.isHovered, Me.palette.ButtonHover, Me.palette.ButtonBackground))
                    borderColor = If(Me.Focused, Me.palette.Accent, If(Me.isHovered, Me.palette.BorderHover, Me.palette.Border))
                    textColor = Me.palette.Foreground

                Case ThemedButtonStyle.Accent
                    fillColor = If(Me.isPressed, Me.palette.AccentPressed, If(Me.isHovered, Me.palette.AccentHover, Me.palette.Accent))
                    borderColor = fillColor
                    textColor = Me.palette.AccentForeground

                Case ThemedButtonStyle.Danger
                    fillColor = If(Me.isPressed, Me.palette.DangerPressed, If(Me.isHovered, Me.palette.DangerHover, Me.palette.Danger))
                    borderColor = fillColor
                    textColor = Me.palette.AccentForeground

                Case Else
                    Throw New InvalidOperationException($"Unhandled button style: {Me.buttonStyleValue}")

            End Select
        End Sub

#End Region

    End Class

End Namespace
