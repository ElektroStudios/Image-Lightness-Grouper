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
    ''' A themed, strictly <see cref="Integer"/>-based numeric up-down control.
    ''' <para></para>
    ''' It hosts a border-less <see cref="NumericUpDown"/> (zero decimal places, increment of 1, digits-only input),
    ''' paints its own 1px border and repaints the native spin buttons, which otherwise stay light in dark mode.
    ''' </summary>
    <DefaultEvent(NameOf(ThemedNumericUpDown.ValueChanged))>
    <DefaultProperty(NameOf(ThemedNumericUpDown.Value))>
    <ToolboxItem(True)>
    Public Class ThemedNumericUpDown : Inherits UserControl : Implements IThemeable

#Region " Constants "

        ''' <summary>
        ''' Horizontal padding between the left border and the text.
        ''' </summary>
        Private Const HorizontalPadding As Integer = 6

        ''' <summary>
        ''' Vertical padding between the border and the hosted control.
        ''' </summary>
        Private Const VerticalPadding As Integer = 4

        ''' <summary>
        ''' The internal type name of the WinForms spin buttons child control.
        ''' </summary>
        Private Const UpDownButtonsTypeName As String = "UpDownButtons"

#End Region

#Region " Fields "

        ''' <summary>
        ''' The hosted, border-less numeric up-down.
        ''' </summary>
        Private ReadOnly numericUpDown As NumericUpDown

        ''' <summary>
        ''' The native spin buttons child control of <see cref="numericUpDown"/>.
        ''' </summary>
        Private ReadOnly upDownButtons As Control

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
        ''' Occurs when the <see cref="Value"/> property changes.
        ''' </summary>
        <Category("Action")>
        <Description("Occurs when the Value property changes.")>
        Public Event ValueChanged As EventHandler

#End Region

#Region " Constructors "

        ''' <summary>
        ''' Initializes a new instance of the <see cref="ThemedNumericUpDown"/> class.
        ''' </summary>
        ''' <exception cref="InvalidOperationException">Thrown when the native spin buttons cannot be located.</exception>
        Public Sub New()
            Me.SetStyle(ControlStyles.UserPaint Or
                        ControlStyles.AllPaintingInWmPaint Or
                        ControlStyles.OptimizedDoubleBuffer Or
                        ControlStyles.ResizeRedraw, True)

            Me.numericUpDown = New NumericUpDown() With {
                .BorderStyle = BorderStyle.None,
                .DecimalPlaces = 0,
                .Increment = 1D,
                .Minimum = 0D,
                .Maximum = 100D,
                .Value = 0D,
                .ThousandsSeparator = False,
                .InterceptArrowKeys = True,
                .Margin = Padding.Empty
            }

            Me.upDownButtons = ThemedNumericUpDown.FindUpDownButtons(Me.numericUpDown)

            AddHandler Me.numericUpDown.ValueChanged, AddressOf Me.NumericUpDown_ValueChanged
            AddHandler Me.numericUpDown.KeyPress, AddressOf Me.NumericUpDown_KeyPress
            AddHandler Me.numericUpDown.GotFocus, AddressOf Me.NumericUpDown_FocusChanged
            AddHandler Me.numericUpDown.LostFocus, AddressOf Me.NumericUpDown_FocusChanged
            AddHandler Me.numericUpDown.Leave, AddressOf Me.NumericUpDown_FocusChanged

            AddHandler Me.upDownButtons.Paint, AddressOf Me.UpDownButtons_Paint
            AddHandler Me.upDownButtons.MouseMove, AddressOf Me.UpDownButtons_MouseStateChanged
            AddHandler Me.upDownButtons.MouseLeave, AddressOf Me.UpDownButtons_MouseStateChanged
            AddHandler Me.upDownButtons.MouseDown, AddressOf Me.UpDownButtons_MouseStateChanged
            AddHandler Me.upDownButtons.MouseUp, AddressOf Me.UpDownButtons_MouseStateChanged

            Me.Controls.Add(Me.numericUpDown)

            ' The base constructor sets the initial size from DefaultSize without calling SetBoundsCore,
            ' so the height rule must be enforced explicitly once (later changes go through SetBoundsCore).
            Me.Height = Me.FixedHeight
            Me.ApplyTheme(Me.palette)
        End Sub

#End Region

#Region " Properties "

        ''' <summary>
        ''' Gets or sets the minimum allowed value.
        ''' </summary>
        ''' <exception cref="ArgumentOutOfRangeException">Thrown when the value is greater than <see cref="Maximum"/>.</exception>
        <Category("Data")>
        <Description("The minimum allowed value.")>
        <DefaultValue(0)>
        Public Property Minimum As Integer
            Get
                Return Decimal.ToInt32(Me.numericUpDown.Minimum)
            End Get
            Set(value As Integer)
                If value > Me.Maximum Then
                    Throw New ArgumentOutOfRangeException(NameOf(value), value, $"Minimum cannot be greater than Maximum ({Me.Maximum}).")
                End If

                Me.numericUpDown.Minimum = value
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the maximum allowed value.
        ''' </summary>
        ''' <exception cref="ArgumentOutOfRangeException">Thrown when the value is less than <see cref="Minimum"/>.</exception>
        <Category("Data")>
        <Description("The maximum allowed value.")>
        <DefaultValue(100)>
        Public Property Maximum As Integer
            Get
                Return Decimal.ToInt32(Me.numericUpDown.Maximum)
            End Get
            Set(value As Integer)
                If value < Me.Minimum Then
                    Throw New ArgumentOutOfRangeException(NameOf(value), value, $"Maximum cannot be less than Minimum ({Me.Minimum}).")
                End If

                Me.numericUpDown.Maximum = value
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the current integer value.
        ''' </summary>
        ''' <exception cref="ArgumentOutOfRangeException">Thrown when the value is outside [<see cref="Minimum"/>, <see cref="Maximum"/>].</exception>
        <Category("Data")>
        <Description("The current integer value.")>
        <DefaultValue(0)>
        <Bindable(True)>
        Public Property Value As Integer
            Get
                Return Decimal.ToInt32(Me.numericUpDown.Value)
            End Get
            Set(value As Integer)
                If value < Me.Minimum OrElse value > Me.Maximum Then
                    Throw New ArgumentOutOfRangeException(NameOf(value), value, $"Value must be between {Me.Minimum} and {Me.Maximum}.")
                End If

                Me.numericUpDown.Value = value
            End Set
        End Property

        ''' <summary>
        ''' Gets the fixed height for the current font.
        ''' </summary>
        Private ReadOnly Property FixedHeight As Integer
            Get
                Return ControlMetrics.GetInputHeight(Me.Font)
            End Get
        End Property

        ''' <inheritdoc/>
        Protected Overrides ReadOnly Property DefaultSize As Size
            Get
                Return New Size(80, 26)
            End Get
        End Property

#End Region

#Region " IThemeable "

        ''' <inheritdoc/>
        Public Sub ApplyTheme(palette As ThemePalette) Implements IThemeable.ApplyTheme
            If palette Is Nothing Then
                Throw New ArgumentNullException(NameOf(palette))
            End If

            Me.palette = palette

            Dim surface As Color = If(Me.Enabled, palette.SurfaceBackground, palette.DisabledBackground)
            Me.BackColor = surface
            Me.ForeColor = palette.Foreground
            Me.numericUpDown.BackColor = surface
            Me.numericUpDown.ForeColor = If(Me.Enabled, palette.Foreground, palette.DisabledForeground)

            Me.upDownButtons.Invalidate()
            Me.Invalidate()
        End Sub

#End Region

#Region " Event Invocators "

        ''' <summary>
        ''' Raises the <see cref="ValueChanged"/> event.
        ''' </summary>
        ''' <param name="e">The event data.</param>
        Protected Overridable Sub OnValueChanged(e As EventArgs)
            RaiseEvent ValueChanged(Me, e)
        End Sub

#End Region

#Region " Overridden Methods "

        ''' <inheritdoc/>
        Protected Overrides Sub SetBoundsCore(x As Integer, y As Integer, width As Integer, height As Integer, specified As BoundsSpecified)
            ' Construction-order guard: the base UserControl constructor can invoke this virtual member
            ' before this class's constructor has created the hosted control. This is not error hiding:
            ' at that point there is simply no hosted control to constrain yet.
            If Me.numericUpDown Is Nothing Then
                MyBase.SetBoundsCore(x, y, width, height, specified)
                Return
            End If

            MyBase.SetBoundsCore(x, y, width, Me.FixedHeight, specified)
        End Sub

        ''' <inheritdoc/>
        Protected Overrides Sub OnLayout(e As LayoutEventArgs)
            MyBase.OnLayout(e)

            If Me.numericUpDown Is Nothing Then
                Return ' Construction-order guard (see SetBoundsCore).
            End If

            ' The spin buttons reach the right border (1px), the text keeps a left padding.
            Dim innerWidth As Integer = Math.Max(1, Me.ClientSize.Width - HorizontalPadding - 1)
            Dim innerHeight As Integer = Me.numericUpDown.PreferredHeight
            Dim innerTop As Integer = (Me.ClientSize.Height - innerHeight) \ 2
            Me.numericUpDown.Bounds = New Rectangle(HorizontalPadding, innerTop, innerWidth, innerHeight)
        End Sub

        ''' <inheritdoc/>
        Protected Overrides Sub OnFontChanged(e As EventArgs)
            MyBase.OnFontChanged(e)

            If Me.numericUpDown Is Nothing Then
                Return ' Construction-order guard (see SetBoundsCore). The constructor picks up the current font.
            End If

            Me.numericUpDown.Font = Me.Font

            ' Assigning the current height to itself would not re-evaluate the constraint
            ' (WinForms skips SetBoundsCore when the bounds do not change), so the new height is assigned.
            Me.Height = Me.FixedHeight
            Me.PerformLayout()
        End Sub

        ''' <inheritdoc/>
        Protected Overrides Sub OnEnabledChanged(e As EventArgs)
            MyBase.OnEnabledChanged(e)
            Me.ApplyTheme(Me.palette)
        End Sub

        ''' <inheritdoc/>
        Protected Overrides Sub OnMouseDown(e As MouseEventArgs)
            Me.numericUpDown.Focus()
            MyBase.OnMouseDown(e)
        End Sub

        ''' <inheritdoc/>
        Protected Overrides Sub OnMouseEnter(e As EventArgs)
            Me.isHovered = True
            Me.Invalidate()
            MyBase.OnMouseEnter(e)
        End Sub

        ''' <inheritdoc/>
        Protected Overrides Sub OnMouseLeave(e As EventArgs)
            Me.isHovered = Me.ClientRectangle.Contains(Me.PointToClient(Control.MousePosition))
            Me.Invalidate()
            MyBase.OnMouseLeave(e)
        End Sub

        ''' <inheritdoc/>
        Protected Overrides Sub OnPaint(e As PaintEventArgs)
            Dim g As Graphics = e.Graphics
            g.Clear(Me.BackColor)

            Dim isFocused As Boolean = Me.numericUpDown.ContainsFocus
            Dim borderColor As Color

            If Not Me.Enabled Then
                borderColor = Me.palette.Border
            ElseIf isFocused Then
                borderColor = Me.palette.Accent
            ElseIf Me.isHovered Then
                borderColor = Me.palette.BorderHover
            Else
                borderColor = Me.palette.Border
            End If

            Using borderPen As New Pen(borderColor, 1.0F)
                g.DrawRectangle(borderPen, 0, 0, Me.ClientSize.Width - 1, Me.ClientSize.Height - 1)
            End Using
        End Sub

#End Region

#Region " Private Methods "

        ''' <summary>
        ''' Locates the internal spin buttons child control of a <see cref="NumericUpDown"/>.
        ''' </summary>
        ''' <param name="host">The numeric up-down control.</param>
        ''' <returns>The spin buttons control.</returns>
        ''' <exception cref="InvalidOperationException">Thrown when the child control does not exist (framework implementation changed).</exception>
        Private Shared Function FindUpDownButtons(host As NumericUpDown) As Control
            Return ThemedNumericUpDown.FindChildByTypeName(host, UpDownButtonsTypeName)
        End Function


        ''' <summary>
        ''' Locates a direct child control of a <see cref="NumericUpDown"/> by its (internal) type name.
        ''' </summary>
        ''' <param name="host">The numeric up-down control.</param>
        ''' <param name="typeName">The simple type name of the child control.</param>
        ''' <returns>The child control.</returns>
        ''' <exception cref="InvalidOperationException">Thrown when the child control does not exist (framework implementation changed).</exception>
        Private Shared Function FindChildByTypeName(host As NumericUpDown, typeName As String) As Control
            For Each child As Control In host.Controls
                If String.Equals(child.GetType().Name, typeName, StringComparison.Ordinal) Then
                    Return child
                End If
            Next

            Throw New InvalidOperationException($"Could not locate the '{typeName}' child control of {NameOf(NumericUpDown)}.")
        End Function

        ''' <summary>
        ''' Draws a small, centered, filled arrow (triangle) inside the specified bounds.
        ''' </summary>
        ''' <param name="g">The graphics surface.</param>
        ''' <param name="bounds">The button bounds.</param>
        ''' <param name="pointsUp"><see langword="True"/> for an up arrow; <see langword="False"/> for a down arrow.</param>
        ''' <param name="color">The arrow color.</param>
        Private Shared Sub DrawArrow(g As Graphics, bounds As Rectangle, pointsUp As Boolean, color As Color)
            Const ArrowHalfWidth As Single = 3.5F
            Const ArrowHalfHeight As Single = 2.0F

            Dim centerX As Single = bounds.Left + (bounds.Width / 2.0F)
            Dim centerY As Single = bounds.Top + (bounds.Height / 2.0F)
            Dim points As PointF()

            If pointsUp Then
                points = {New PointF(centerX - ArrowHalfWidth, centerY + ArrowHalfHeight),
                          New PointF(centerX + ArrowHalfWidth, centerY + ArrowHalfHeight),
                          New PointF(centerX, centerY - ArrowHalfHeight)}
            Else
                points = {New PointF(centerX - ArrowHalfWidth, centerY - ArrowHalfHeight),
                          New PointF(centerX + ArrowHalfWidth, centerY - ArrowHalfHeight),
                          New PointF(centerX, centerY + ArrowHalfHeight)}
            End If

            Using arrowBrush As New SolidBrush(color)
                g.FillPolygon(arrowBrush, points)
            End Using
        End Sub

#End Region

#Region " Event Handlers "

        ''' <summary>
        ''' Paints the spin buttons on top of the native rendering.
        ''' </summary>
        Private Sub UpDownButtons_Paint(sender As Object, e As PaintEventArgs)
            Dim g As Graphics = e.Graphics
            Dim bounds As Rectangle = Me.upDownButtons.ClientRectangle
            Dim halfHeight As Integer = bounds.Height \ 2
            Dim upBounds As New Rectangle(0, 0, bounds.Width, halfHeight)
            Dim downBounds As New Rectangle(0, halfHeight, bounds.Width, bounds.Height - halfHeight)

            Dim mouseLocation As Point = Me.upDownButtons.PointToClient(Control.MousePosition)
            Dim isLeftButtonDown As Boolean = (Control.MouseButtons And MouseButtons.Left) = MouseButtons.Left
            Dim surface As Color = If(Me.Enabled, Me.palette.SurfaceBackground, Me.palette.DisabledBackground)

            g.Clear(surface)

            For Each buttonBounds As Rectangle In {upBounds, downBounds}
                If Me.Enabled AndAlso buttonBounds.Contains(mouseLocation) Then
                    Using stateBrush As New SolidBrush(If(isLeftButtonDown, Me.palette.ButtonPressed, Me.palette.ButtonHover))
                        g.FillRectangle(stateBrush, buttonBounds)
                    End Using
                End If
            Next

            g.SmoothingMode = SmoothingMode.AntiAlias
            Dim arrowColor As Color = If(Me.Enabled, Me.palette.Foreground, Me.palette.DisabledForeground)
            ThemedNumericUpDown.DrawArrow(g, upBounds, pointsUp:=True, color:=arrowColor)
            ThemedNumericUpDown.DrawArrow(g, downBounds, pointsUp:=False, color:=arrowColor)
        End Sub

        ''' <summary>
        ''' Repaints the spin buttons when the mouse hover/press state changes.
        ''' </summary>
        Private Sub UpDownButtons_MouseStateChanged(sender As Object, e As EventArgs)
            Me.upDownButtons.Invalidate()
        End Sub

        ''' <summary>
        ''' Forwards the inner <see cref="NumericUpDown.ValueChanged"/> event.
        ''' </summary>
        Private Sub NumericUpDown_ValueChanged(sender As Object, e As EventArgs)
            Me.OnValueChanged(e)
        End Sub


        ''' <summary>
        ''' Rejects any keyboard input that could produce a non-integer value (decimal separators, letters, etc).
        ''' </summary>
        Private Sub NumericUpDown_KeyPress(sender As Object, e As KeyPressEventArgs)
            If Char.IsControl(e.KeyChar) OrElse Char.IsDigit(e.KeyChar) Then
                Return
            End If

            If e.KeyChar = "-"c AndAlso Me.Minimum < 0 Then
                Return
            End If

            e.Handled = True
        End Sub

        ''' <summary>
        ''' Repaints the border when the focus state changes.
        ''' </summary>
        Private Sub NumericUpDown_FocusChanged(sender As Object, e As EventArgs)
            Me.Invalidate()
        End Sub

#End Region

    End Class

End Namespace
