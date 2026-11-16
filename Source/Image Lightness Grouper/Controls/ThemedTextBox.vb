Option Strict On
Option Explicit On
Option Infer Off

#Region " Imports "

Imports System.ComponentModel
Imports System.Drawing
Imports System.Windows.Forms

Imports ImageLightnessGrouper.Theming

#End Region

Namespace ImageLightnessGrouper.Controls

    ''' <summary>
    ''' A themed text box: a border-less <see cref="TextBox"/> hosted inside a <see cref="UserControl"/>
    ''' that paints its own 1px border (accent-colored when focused), avoiding the white 3D borders of
    ''' the native control in dark mode.
    ''' </summary>
    <DefaultEvent("TextChanged")>
    <DefaultProperty(NameOf(ThemedTextBox.Text))>
    <ToolboxItem(True)>
    Public Class ThemedTextBox : Inherits UserControl : Implements IThemeable

#Region " Constants "

        ''' <summary>
        ''' Horizontal padding between the border and the text.
        ''' </summary>
        Private Const HorizontalPadding As Integer = 6

        ''' <summary>
        ''' Vertical padding between the border and the text.
        ''' </summary>
        Private Const VerticalPadding As Integer = 4

#End Region

#Region " Fields "

        ''' <summary>
        ''' The hosted, border-less text box.
        ''' </summary>
        Private ReadOnly innerTextBox As TextBox

        ''' <summary>
        ''' Whether the mouse pointer is over the control.
        ''' </summary>
        Private isHovered As Boolean

        ''' <summary>
        ''' The palette currently applied.
        ''' </summary>
        Private palette As ThemePalette = ThemeManager.CurrentPalette

#End Region

#Region " Constructors "

        ''' <summary>
        ''' Initializes a new instance of the <see cref="ThemedTextBox"/> class.
        ''' </summary>
        Public Sub New()
            Me.SetStyle(ControlStyles.UserPaint Or
                        ControlStyles.AllPaintingInWmPaint Or
                        ControlStyles.OptimizedDoubleBuffer Or
                        ControlStyles.ResizeRedraw, True)

            Me.innerTextBox = New TextBox() With {
                .BorderStyle = BorderStyle.None,
                .Margin = Padding.Empty
            }

            AddHandler Me.innerTextBox.TextChanged, AddressOf Me.InnerTextBox_TextChanged
            AddHandler Me.innerTextBox.GotFocus, AddressOf Me.InnerTextBox_FocusChanged
            AddHandler Me.innerTextBox.LostFocus, AddressOf Me.InnerTextBox_FocusChanged
            AddHandler Me.innerTextBox.MouseEnter, AddressOf Me.InnerTextBox_MouseEnter
            AddHandler Me.innerTextBox.MouseLeave, AddressOf Me.InnerTextBox_MouseLeave

            Me.Controls.Add(Me.innerTextBox)

            ' The base constructor sets the initial size from DefaultSize without calling SetBoundsCore,
            ' so the height rule must be enforced explicitly once (later changes go through SetBoundsCore).
            Me.Height = Me.SingleLineHeight
            Me.ApplyTheme(Me.palette)
        End Sub

#End Region

#Region " Properties "

        ''' <summary>
        ''' Gets or sets the text of the control.
        ''' </summary>
        <Browsable(True)>
        <EditorBrowsable(EditorBrowsableState.Always)>
        <DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)>
        <Bindable(True)>
        <Category("Appearance")>
        <Description("The text of the control.")>
        <DefaultValue("")>
        Public Overrides Property Text As String
            Get
                Return Me.innerTextBox.Text
            End Get
            Set(value As String)
                Me.innerTextBox.Text = value
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets a value indicating whether the text is read-only.
        ''' </summary>
        <Category("Behavior")>
        <Description("Indicates whether the text is read-only.")>
        <DefaultValue(False)>
        Public Property [ReadOnly] As Boolean
            Get
                Return Me.innerTextBox.ReadOnly
            End Get
            Set(value As Boolean)
                Me.innerTextBox.ReadOnly = value
                Me.ApplyTheme(Me.palette)
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets a value indicating whether this is a multi-line text box (with a vertical scroll bar).
        ''' </summary>
        <Category("Behavior")>
        <Description("Indicates whether this is a multi-line text box.")>
        <DefaultValue(False)>
        Public Property Multiline As Boolean
            Get
                Return Me.innerTextBox.Multiline
            End Get
            Set(value As Boolean)
                Me.innerTextBox.Multiline = value
                Me.innerTextBox.ScrollBars = If(value, ScrollBars.Vertical, ScrollBars.None)
                Me.innerTextBox.WordWrap = value
                If Not value Then
                    Me.Height = Me.SingleLineHeight
                End If

                Me.PerformLayout()
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the starting point of the selected text.
        ''' </summary>
        <Browsable(False)>
        <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
        Public Property SelectionStart As Integer
            Get
                Return Me.innerTextBox.SelectionStart
            End Get
            Set(value As Integer)
                Me.innerTextBox.SelectionStart = value
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the number of characters selected.
        ''' </summary>
        <Browsable(False)>
        <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
        Public Property SelectionLength As Integer
            Get
                Return Me.innerTextBox.SelectionLength
            End Get
            Set(value As Integer)
                Me.innerTextBox.SelectionLength = value
            End Set
        End Property

        ''' <summary>
        ''' Gets the height of a single-line instance for the current font.
        ''' </summary>
        Private ReadOnly Property SingleLineHeight As Integer
            Get
                Return ControlMetrics.GetInputHeight(Me.Font)
            End Get
        End Property

        ''' <inheritdoc/>
        Protected Overrides ReadOnly Property DefaultSize As Size
            Get
                Return New Size(200, 26)
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
            Me.innerTextBox.BackColor = surface
            Me.innerTextBox.ForeColor = If(Me.Enabled, palette.Foreground, palette.DisabledForeground)

            Me.Invalidate()
        End Sub

#End Region

#Region " Public Methods "

        ''' <summary>
        ''' Selects all the text in the control.
        ''' </summary>
        Public Sub SelectAll()
            Me.innerTextBox.SelectAll()
        End Sub

        ''' <summary>
        ''' Places the caret at the end of the text, without selection, and scrolls the text so the caret is visible
        ''' (e.g. the end of a long path, which is its most relevant part).
        ''' </summary>
        Public Sub MoveCaretToEnd()
            Me.innerTextBox.Select(Me.innerTextBox.TextLength, 0)
            Me.innerTextBox.ScrollToCaret()
        End Sub

#End Region

#Region " Overridden Methods "

        ''' <inheritdoc/>
        Protected Overrides Sub SetBoundsCore(x As Integer, y As Integer, width As Integer, height As Integer, specified As BoundsSpecified)
            ' Construction-order guard: the base UserControl constructor can invoke this virtual member
            ' before this class's constructor has created the hosted control. This is not error hiding:
            ' at that point there is simply no hosted control to constrain yet.
            If Me.innerTextBox Is Nothing Then
                MyBase.SetBoundsCore(x, y, width, height, specified)
                Return
            End If

            ' Single-line instances have a fixed, font-driven height (just like the native TextBox).
            If Not Me.innerTextBox.Multiline Then
                height = Me.SingleLineHeight
            End If

            MyBase.SetBoundsCore(x, y, width, height, specified)
        End Sub

        ''' <inheritdoc/>
        Protected Overrides Sub OnLayout(e As LayoutEventArgs)
            MyBase.OnLayout(e)

            If Me.innerTextBox Is Nothing Then
                Return ' Construction-order guard (see SetBoundsCore).
            End If

            Dim innerWidth As Integer = Math.Max(1, Me.ClientSize.Width - (HorizontalPadding * 2))

            If Me.innerTextBox.Multiline Then
                Dim innerHeight As Integer = Math.Max(1, Me.ClientSize.Height - (VerticalPadding * 2))
                Me.innerTextBox.Bounds = New Rectangle(HorizontalPadding, VerticalPadding, innerWidth, innerHeight)
            Else
                Dim innerTop As Integer = (Me.ClientSize.Height - Me.innerTextBox.Height) \ 2
                Me.innerTextBox.Bounds = New Rectangle(HorizontalPadding, innerTop, innerWidth, Me.innerTextBox.Height)
            End If
        End Sub

        ''' <inheritdoc/>
        Protected Overrides Sub OnFontChanged(e As EventArgs)
            MyBase.OnFontChanged(e)

            If Me.innerTextBox Is Nothing Then
                Return ' Construction-order guard (see SetBoundsCore). The constructor picks up the current font.
            End If

            Me.innerTextBox.Font = Me.Font
            ' Assigning the current height to itself would not re-evaluate the constraint
            ' (WinForms skips SetBoundsCore when the bounds do not change), so the new height is assigned.
            If Not Me.innerTextBox.Multiline Then
                Me.Height = Me.SingleLineHeight
            End If

            Me.PerformLayout()
        End Sub

        ''' <inheritdoc/>
        Protected Overrides Sub OnEnabledChanged(e As EventArgs)
            MyBase.OnEnabledChanged(e)
            Me.ApplyTheme(Me.palette)
        End Sub

        ''' <inheritdoc/>
        Protected Overrides Sub OnMouseDown(e As MouseEventArgs)
            ' Clicking the padding area focuses the text, like the native control.
            Me.innerTextBox.Focus()
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

            Dim isFocused As Boolean = Me.innerTextBox.Focused
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

#Region " Event Handlers "

        ''' <summary>
        ''' Forwards the inner text box <see cref="Control.TextChanged"/> event.
        ''' </summary>
        Private Sub InnerTextBox_TextChanged(sender As Object, e As EventArgs)
            ' Raises Control.TextChanged on this control, so consumers handle it exactly as with a native TextBox.
            Me.OnTextChanged(e)
        End Sub

        ''' <summary>
        ''' Repaints the border when the inner text box gains or loses focus.
        ''' </summary>
        Private Sub InnerTextBox_FocusChanged(sender As Object, e As EventArgs)
            Me.Invalidate()
        End Sub

        ''' <summary>
        ''' Tracks hover state over the inner text box.
        ''' </summary>
        Private Sub InnerTextBox_MouseEnter(sender As Object, e As EventArgs)
            Me.isHovered = True
            Me.Invalidate()
        End Sub

        ''' <summary>
        ''' Tracks hover state over the inner text box.
        ''' </summary>
        Private Sub InnerTextBox_MouseLeave(sender As Object, e As EventArgs)
            Me.isHovered = Me.ClientRectangle.Contains(Me.PointToClient(Control.MousePosition))
            Me.Invalidate()
        End Sub

#End Region

    End Class

End Namespace
