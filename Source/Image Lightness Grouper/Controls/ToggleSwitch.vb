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
    ''' A Windows 11 style, fully custom-painted on/off toggle switch with an optional caption.
    ''' <para></para>
    ''' Supports mouse, keyboard (Space/Enter), a subtle accent-border focus cue and theming.
    ''' <para></para>
    ''' The caption (<see cref="Text"/>) is drawn by the control itself, on the right of the track, so that it is
    ''' optically centered on the track: the vertical center of its capital letters is aligned with the center of the
    ''' track. A separate label cannot achieve that, because the glyphs of a font are not vertically centered within
    ''' its line box (the ascent includes the internal leading), so centering both boxes leaves the text visibly
    ''' off-center (lower, with Segoe UI).
    ''' <para></para>
    ''' The track size is derived from the font height, so the control scales with the font and the DPI.
    ''' </summary>
    <DefaultEvent(NameOf(ToggleSwitch.CheckedChanged))>
    <DefaultProperty(NameOf(ToggleSwitch.Checked))>
    <ToolboxItem(True)>
    Public Class ToggleSwitch : Inherits Control : Implements IThemeable

#Region " Constants "

        ''' <summary>
        ''' Gap between the track edge and the thumb.
        ''' </summary>
        Private Const ThumbInset As Single = 4.0F

        ''' <summary>
        ''' Extra height added to the font height to obtain the track height.
        ''' </summary>
        Private Const TrackHeightPadding As Integer = 4

        ''' <summary>
        ''' Horizontal gap between the track and the caption.
        ''' </summary>
        Private Const CaptionGap As Integer = 8

        ''' <summary>
        ''' The text formatting used to measure and draw the caption.
        ''' </summary>
        Private Const CaptionFormatFlags As TextFormatFlags = TextFormatFlags.NoPadding Or TextFormatFlags.SingleLine Or
                                                               TextFormatFlags.NoPrefix Or TextFormatFlags.Left Or TextFormatFlags.Top

#End Region

#Region " Fields "

        ''' <summary>
        ''' Backing field for <see cref="Checked"/>.
        ''' </summary>
        Private checkedValue As Boolean

        ''' <summary>
        ''' Whether the mouse pointer is over the control.
        ''' </summary>
        Private isHovered As Boolean

        ''' <summary>
        ''' The palette currently applied.
        ''' </summary>
        Private palette As ThemePalette = ThemeManager.CurrentPalette

#End Region

#Region " Events "

        ''' <summary>
        ''' Occurs when the value of the <see cref="Checked"/> property changes.
        ''' </summary>
        <Category("Property Changed")>
        <Description("Occurs when the value of the Checked property changes.")>
        Public Event CheckedChanged As EventHandler

#End Region

#Region " Constructors "

        ''' <summary>
        ''' Initializes a new instance of the <see cref="ToggleSwitch"/> class.
        ''' </summary>
        Public Sub New()
            Me.SetStyle(ControlStyles.UserPaint Or
                        ControlStyles.AllPaintingInWmPaint Or
                        ControlStyles.OptimizedDoubleBuffer Or
                        ControlStyles.ResizeRedraw Or
                        ControlStyles.Selectable, True)
            Me.SetStyle(ControlStyles.StandardDoubleClick, False)

            Me.Cursor = Cursors.Hand
            Me.TabStop = True
            Me.AccessibleRole = AccessibleRole.CheckButton
            MyBase.AutoSize = True

            ' The base constructor sets the initial size from DefaultSize without calling SetBoundsCore,
            ' so the size rule must be enforced explicitly once (later changes go through SetBoundsCore).
            Me.Size = Me.ComputeLayout().Size
            Me.ApplyTheme(Me.palette)
        End Sub

#End Region

#Region " Properties "

        ''' <summary>
        ''' Gets or sets a value indicating whether the toggle is in the "on" state.
        ''' </summary>
        <Category("Appearance")>
        <Description("Indicates whether the toggle is in the 'on' state.")>
        <DefaultValue(False)>
        <Bindable(True)>
        Public Property Checked As Boolean
            Get
                Return Me.checkedValue
            End Get
            Set(value As Boolean)
                If Me.checkedValue = value Then
                    Return
                End If

                Me.checkedValue = value
                Me.AccessibleDescription = If(value, "On", "Off")
                Me.Invalidate()
                Me.OnCheckedChanged(EventArgs.Empty)
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the caption drawn on the right of the track. An empty string shows the track alone.
        ''' </summary>
        <Category("Appearance")>
        <Description("The caption drawn on the right of the track.")>
        <Browsable(True)>
        <EditorBrowsable(EditorBrowsableState.Always)>
        <DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)>
        <Localizable(True)>
        <DefaultValue("")>
        Public Overrides Property Text As String
            Get
                Return MyBase.Text
            End Get
            Set(value As String)
                MyBase.Text = value
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets a value indicating whether the control sizes itself to fit the track and the caption.
        ''' </summary>
        <Category("Layout")>
        <Description("Indicates whether the control sizes itself to fit the track and the caption.")>
        <Browsable(True)>
        <EditorBrowsable(EditorBrowsableState.Always)>
        <DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)>
        <DefaultValue(True)>
        Public Overrides Property AutoSize As Boolean
            Get
                Return MyBase.AutoSize
            End Get
            Set(value As Boolean)
                MyBase.AutoSize = value
                Me.ResizeToContent()
            End Set
        End Property

#End Region

#Region " Layout "

        ''' <summary>
        ''' The positions of the track and the caption, relative to the top-left corner of the content.
        ''' </summary>
        Friend Structure ToggleSwitchLayout

            ''' <summary>
            ''' The size of the whole content (track, gap and caption).
            ''' </summary>
            Friend ReadOnly Size As Size

            ''' <summary>
            ''' The bounds of the track.
            ''' </summary>
            Friend ReadOnly TrackBounds As Rectangle

            ''' <summary>
            ''' The bounds of the caption line box (<see cref="Rectangle.Empty"/> when there is no caption).
            ''' </summary>
            Friend ReadOnly CaptionBounds As Rectangle

            ''' <summary>
            ''' Initializes a new instance of the <see cref="ToggleSwitchLayout"/> structure.
            ''' </summary>
            ''' <param name="size">The size of the whole content.</param>
            ''' <param name="trackBounds">The bounds of the track.</param>
            ''' <param name="captionBounds">The bounds of the caption line box.</param>
            Friend Sub New(size As Size, trackBounds As Rectangle, captionBounds As Rectangle)
                Me.Size = size
                Me.TrackBounds = trackBounds
                Me.CaptionBounds = captionBounds
            End Sub

        End Structure

        ''' <summary>
        ''' Computes the layout of the track and the caption for the current font and text.
        ''' </summary>
        ''' <returns>The layout, relative to the top-left corner of the content.</returns>
        Friend Function ComputeLayout() As ToggleSwitchLayout
            Dim trackHeight As Integer = Me.Font.Height + TrackHeightPadding
            Dim trackWidth As Integer = trackHeight * 2

            If String.IsNullOrEmpty(Me.Text) Then
                Return New ToggleSwitchLayout(New Size(trackWidth, trackHeight), New Rectangle(0, 0, trackWidth, trackHeight), Rectangle.Empty)
            End If

            Dim captionSize As Size = TextRenderer.MeasureText(Me.Text, Me.Font, New Size(Integer.MaxValue, Integer.MaxValue), CaptionFormatFlags)

            ' Caption top relative to the track top, so that the center of the rendered capital letters lands on the track center.
            ' When an exact match is impossible (track and capitals of different parity), the caption goes half a pixel lower,
            ' which looks better than higher because the lowercase letters (most of the caption) sit in the lower part.
            Dim captionTop As Integer = CInt(Math.Ceiling((trackHeight / 2.0R) - ToggleSwitch.GetCapCenterFromLineTop(Me.Font)))

            ' The content spans both the track and the caption line box (the caption may stick out above or below the track).
            Dim contentTop As Integer = Math.Min(0, captionTop)
            Dim contentBottom As Integer = Math.Max(trackHeight, captionTop + captionSize.Height)
            Dim trackTop As Integer = -contentTop
            Dim captionLeft As Integer = trackWidth + CaptionGap

            Return New ToggleSwitchLayout(New Size(captionLeft + captionSize.Width, contentBottom - contentTop),
                                          New Rectangle(0, trackTop, trackWidth, trackHeight),
                                          New Rectangle(captionLeft, trackTop + captionTop, captionSize.Width, captionSize.Height))
        End Function

        ''' <summary>
        ''' Gets the distance, in pixels, between the top of a text line box and the vertical center of the capital letters
        ''' (the optical center of a mixed-case UI caption), for the specified font.
        ''' <para></para>
        ''' The capital letter is actually rendered with <see cref="TextRenderer"/> (the same GDI path used to draw the
        ''' caption) and its ink rows are scanned, so the result includes the GDI hinting and grid fitting. Values derived
        ''' from the font design metrics are off by 1-2 pixels at UI sizes, which is clearly visible.
        ''' </summary>
        ''' <param name="font">The font.</param>
        ''' <returns>The distance, in pixels (a multiple of 0.5).</returns>
        ''' <exception cref="ArgumentNullException">Thrown when <paramref name="font"/> is <see langword="Nothing"/>.</exception>
        ''' <exception cref="InvalidOperationException">Thrown when the capital letter renders no visible pixels.</exception>
        Friend Shared Function GetCapCenterFromLineTop(font As Font) As Double
            If font Is Nothing Then
                Throw New ArgumentNullException(NameOf(font))
            End If

            Const Sample As String = "H"
            Dim sampleSize As Size = TextRenderer.MeasureText(Sample, font, New Size(Integer.MaxValue, Integer.MaxValue), CaptionFormatFlags)

            Dim screenDpiX As Single
            Dim screenDpiY As Single
            Using screenGraphics As Graphics = Graphics.FromHwnd(IntPtr.Zero)
                screenDpiX = screenGraphics.DpiX
                screenDpiY = screenGraphics.DpiY
            End Using

            Using bitmap As New Bitmap(Math.Max(1, sampleSize.Width), Math.Max(1, sampleSize.Height), Imaging.PixelFormat.Format32bppArgb)
                bitmap.SetResolution(screenDpiX, screenDpiY)

                Using g As Graphics = Graphics.FromImage(bitmap)
                    g.Clear(Color.White)
                    TextRenderer.DrawText(g, Sample, font, Point.Empty, Color.Black, Color.White, CaptionFormatFlags)
                End Using

                Dim firstInkRow As Integer = -1
                Dim lastInkRow As Integer = -1
                For y As Integer = 0 To bitmap.Height - 1
                    For x As Integer = 0 To bitmap.Width - 1
                        Dim pixel As Color = bitmap.GetPixel(x, y)
                        If (CInt(pixel.R) + pixel.G + pixel.B) < (3 * 128) Then
                            If firstInkRow < 0 Then
                                firstInkRow = y
                            End If

                            lastInkRow = y
                            Exit For
                        End If
                    Next
                Next

                If firstInkRow < 0 Then
                    Throw New InvalidOperationException($"The font '{font.Name}' rendered no visible pixels for the sample capital letter.")
                End If

                ' Center of the ink rows, in continuous coordinates (row N spans N..N+1).
                Return (firstInkRow + lastInkRow + 1) / 2.0R
            End Using
        End Function

        ''' <inheritdoc/>
        Public Overrides Function GetPreferredSize(proposedSize As Size) As Size
            Return Me.ComputeLayout().Size
        End Function

        ''' <summary>
        ''' Resizes the control to fit its content when <see cref="AutoSize"/> is enabled.
        ''' </summary>
        Private Sub ResizeToContent()
            If Me.AutoSize Then
                Me.Size = Me.ComputeLayout().Size
            End If

            Me.Invalidate()
        End Sub

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

#Region " Event Invocators "

        ''' <summary>
        ''' Raises the <see cref="CheckedChanged"/> event.
        ''' </summary>
        ''' <param name="e">The event data.</param>
        Protected Overridable Sub OnCheckedChanged(e As EventArgs)
            RaiseEvent CheckedChanged(Me, e)
        End Sub

#End Region

#Region " Overridden Methods "

        ''' <inheritdoc/>
        Protected Overrides Sub OnPaint(e As PaintEventArgs)
            Dim g As Graphics = e.Graphics
            g.SmoothingMode = SmoothingMode.AntiAlias
            g.Clear(Me.BackColor)

            ' The content is vertically centered within the client area (it fills it exactly when AutoSize is enabled).
            Dim layout As ToggleSwitchLayout = Me.ComputeLayout()
            Dim offsetY As Integer = (Me.ClientSize.Height - layout.Size.Height) \ 2
            Dim trackBounds As New RectangleF(layout.TrackBounds.X + 0.5F, layout.TrackBounds.Y + offsetY + 0.5F,
                                              layout.TrackBounds.Width - 1.0F, layout.TrackBounds.Height - 1.0F)

            If trackBounds.Width <= 0.0F OrElse trackBounds.Height <= 0.0F Then
                Return ' Too small to draw anything meaningful (e.g. while being resized in the designer).
            End If

            Dim trackRadius As Single = trackBounds.Height / 2.0F
            Dim trackFill As Color
            Dim trackBorder As Color
            Dim thumbColor As Color

            If Not Me.Enabled Then
                trackFill = If(Me.checkedValue, Me.palette.DisabledForeground, Me.palette.DisabledBackground)
                trackBorder = Me.palette.DisabledForeground
                thumbColor = If(Me.checkedValue, Me.palette.DisabledBackground, Me.palette.DisabledForeground)

            ElseIf Me.checkedValue Then
                trackFill = If(Me.isHovered, Me.palette.AccentHover, Me.palette.Accent)
                trackBorder = trackFill
                thumbColor = Me.palette.ToggleOnThumb

            Else
                trackFill = If(Me.isHovered, Me.palette.ButtonHover, Me.palette.ToggleOffTrack)
                trackBorder = If(Me.Focused AndAlso Me.ShowFocusCues, Me.palette.Accent, Me.palette.ToggleOffThumb) ' Subtle focus cue, consistent with the text boxes.
                thumbColor = Me.palette.ToggleOffThumb
            End If

            Using trackPath As GraphicsPath = DrawingHelper.CreateRoundedRectangle(trackBounds, trackRadius),
                  fillBrush As New SolidBrush(trackFill),
                  borderPen As New Pen(trackBorder, 1.0F)

                g.FillPath(fillBrush, trackPath)
                g.DrawPath(borderPen, trackPath)
            End Using

            Dim thumbDiameter As Single = trackBounds.Height - (ThumbInset * 2.0F)
            If Me.isHovered AndAlso Me.Enabled Then
                thumbDiameter += 1.0F
            End If

            Dim thumbX As Single = If(Me.checkedValue,
                                      trackBounds.Right - ThumbInset - thumbDiameter,
                                      trackBounds.Left + ThumbInset)
            Dim thumbY As Single = trackBounds.Top + ((trackBounds.Height - thumbDiameter) / 2.0F)

            Using thumbBrush As New SolidBrush(thumbColor)
                g.FillEllipse(thumbBrush, thumbX, thumbY, thumbDiameter, thumbDiameter)
            End Using

            If Not layout.CaptionBounds.IsEmpty Then
                g.SmoothingMode = SmoothingMode.None
                Dim captionColor As Color = If(Me.Enabled, Me.palette.Foreground, Me.palette.DisabledForeground)
                Dim captionLocation As New Point(layout.CaptionBounds.X, layout.CaptionBounds.Y + offsetY)
                TextRenderer.DrawText(g, Me.Text, Me.Font, captionLocation, captionColor, Me.BackColor, CaptionFormatFlags)
            End If
        End Sub

        ''' <inheritdoc/>
        Protected Overrides Sub SetBoundsCore(x As Integer, y As Integer, width As Integer, height As Integer, specified As BoundsSpecified)
            If Me.AutoSize Then
                Dim size As Size = Me.ComputeLayout().Size
                width = size.Width
                height = size.Height
            End If

            MyBase.SetBoundsCore(x, y, width, height, specified)
        End Sub

        ''' <inheritdoc/>
        Protected Overrides Sub OnTextChanged(e As EventArgs)
            Me.ResizeToContent()
            MyBase.OnTextChanged(e)
        End Sub

        ''' <inheritdoc/>
        Protected Overrides Sub OnFontChanged(e As EventArgs)
            Me.ResizeToContent()
            MyBase.OnFontChanged(e)
        End Sub

        ''' <inheritdoc/>
        Protected Overrides Sub OnClick(e As EventArgs)
            Me.Focus()
            Me.Checked = Not Me.Checked
            MyBase.OnClick(e)
        End Sub

        ''' <inheritdoc/>
        Protected Overrides Sub OnKeyDown(e As KeyEventArgs)
            If e.KeyCode = Keys.Space OrElse e.KeyCode = Keys.Enter Then
                Me.Checked = Not Me.Checked
                e.Handled = True
            End If

            MyBase.OnKeyDown(e)
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
            Me.Invalidate()
            MyBase.OnMouseLeave(e)
        End Sub

        ''' <inheritdoc/>
        Protected Overrides Sub OnGotFocus(e As EventArgs)
            Me.Invalidate()
            MyBase.OnGotFocus(e)
        End Sub

        ''' <inheritdoc/>
        Protected Overrides Sub OnLostFocus(e As EventArgs)
            Me.Invalidate()
            MyBase.OnLostFocus(e)
        End Sub

        ''' <inheritdoc/>
        Protected Overrides Sub OnEnabledChanged(e As EventArgs)
            Me.Invalidate()
            MyBase.OnEnabledChanged(e)
        End Sub

#End Region

    End Class

End Namespace
