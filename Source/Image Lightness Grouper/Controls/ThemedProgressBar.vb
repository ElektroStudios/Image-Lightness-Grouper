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
    ''' A flat, custom-painted progress bar that honors the active theme.
    ''' </summary>
    <DefaultProperty(NameOf(ThemedProgressBar.Value))>
    <ToolboxItem(True)>
    Public Class ThemedProgressBar : Inherits Control : Implements IThemeable

#Region " Fields "

        ''' <summary>
        ''' Backing field for <see cref="Maximum"/>.
        ''' </summary>
        Private maximumValue As Integer = 100

        ''' <summary>
        ''' Backing field for <see cref="Value"/>.
        ''' </summary>
        Private valueValue As Integer

        ''' <summary>
        ''' Backing field for <see cref="State"/>.
        ''' </summary>
        Private stateValue As ProgressBarState = ProgressBarState.Normal

        ''' <summary>
        ''' The palette currently applied.
        ''' </summary>
        Private palette As ThemePalette = ThemeManager.CurrentPalette

#End Region

#Region " Constructors "

        ''' <summary>
        ''' Initializes a new instance of the <see cref="ThemedProgressBar"/> class.
        ''' </summary>
        Public Sub New()
            Me.SetStyle(ControlStyles.UserPaint Or
                        ControlStyles.AllPaintingInWmPaint Or
                        ControlStyles.OptimizedDoubleBuffer Or
                        ControlStyles.ResizeRedraw, True)
            Me.SetStyle(ControlStyles.Selectable, False)

            Me.TabStop = False
            Me.AccessibleRole = AccessibleRole.ProgressBar
            Me.ApplyTheme(Me.palette)
        End Sub

#End Region

#Region " Properties "

        ''' <summary>
        ''' Gets or sets the maximum value of the range. The minimum is always zero.
        ''' </summary>
        ''' <exception cref="ArgumentOutOfRangeException">Thrown when the value is negative.</exception>
        <Category("Behavior")>
        <Description("The maximum value of the range. The minimum is always zero.")>
        <DefaultValue(100)>
        Public Property Maximum As Integer
            Get
                Return Me.maximumValue
            End Get
            Set(value As Integer)
                If value < 0 Then
                    Throw New ArgumentOutOfRangeException(NameOf(value), value, "Maximum cannot be negative.")
                End If

                Me.maximumValue = value
                Me.valueValue = Math.Min(Me.valueValue, value)
                Me.Invalidate()
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the current position of the progress bar.
        ''' </summary>
        ''' <exception cref="ArgumentOutOfRangeException">Thrown when the value is outside [0, <see cref="Maximum"/>].</exception>
        <Category("Behavior")>
        <Description("The current position of the progress bar.")>
        <DefaultValue(0)>
        Public Property Value As Integer
            Get
                Return Me.valueValue
            End Get
            Set(value As Integer)
                If value < 0 OrElse value > Me.maximumValue Then
                    Throw New ArgumentOutOfRangeException(NameOf(value), value, $"Value must be between 0 and {Me.maximumValue}.")
                End If

                If Me.valueValue <> value Then
                    Me.valueValue = value
                    Me.AccessibleDescription = $"{Me.valueValue} of {Me.maximumValue}"
                    Me.Invalidate()
                End If
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the visual state (normal, warning or error), which drives the fill color.
        ''' </summary>
        ''' <exception cref="InvalidEnumArgumentException">Thrown when the value is not a defined <see cref="ProgressBarState"/>.</exception>
        <Category("Appearance")>
        <Description("The visual state (normal, warning or error), which drives the fill color.")>
        <DefaultValue(GetType(ProgressBarState), "Normal")>
        Public Property State As ProgressBarState
            Get
                Return Me.stateValue
            End Get
            Set(value As ProgressBarState)
                If Not [Enum].IsDefined(GetType(ProgressBarState), value) Then
                    Throw New InvalidEnumArgumentException(NameOf(value), CInt(value), GetType(ProgressBarState))
                End If

                If Me.stateValue <> value Then
                    Me.stateValue = value
                    Me.Invalidate()
                End If
            End Set
        End Property

        ''' <inheritdoc/>
        Protected Overrides ReadOnly Property DefaultSize As Size
            Get
                Return New Size(200, 8)
            End Get
        End Property

        ''' <summary>
        ''' This property is not relevant for this control.
        ''' </summary>
        <Browsable(False)>
        <EditorBrowsable(EditorBrowsableState.Never)>
        <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
        Public Overrides Property Text As String
            Get
                Return MyBase.Text
            End Get
            Set(value As String)
                MyBase.Text = value
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
            Me.Invalidate()
        End Sub

#End Region

#Region " Overridden Methods "

        ''' <inheritdoc/>
        Protected Overrides Sub OnPaint(e As PaintEventArgs)
            Dim g As Graphics = e.Graphics
            g.SmoothingMode = SmoothingMode.AntiAlias
            g.Clear(Me.BackColor)

            Dim trackBounds As New RectangleF(0.0F, 0.0F, Me.ClientSize.Width, Me.ClientSize.Height)
            If trackBounds.Width <= 0.0F OrElse trackBounds.Height <= 0.0F Then
                Return ' Too small to draw anything meaningful (e.g. while being resized in the designer).
            End If

            Dim radius As Single = trackBounds.Height / 2.0F

            Using trackPath As GraphicsPath = DrawingHelper.CreateRoundedRectangle(trackBounds, radius),
                  trackBrush As New SolidBrush(Me.palette.ProgressTrack)

                g.FillPath(trackBrush, trackPath)
            End Using

            If Me.maximumValue = 0 OrElse Me.valueValue = 0 Then
                Return
            End If

            Dim fillWidth As Single = Math.Max(trackBounds.Height, trackBounds.Width * Me.valueValue / Me.maximumValue)
            Dim fillBounds As New RectangleF(0.0F, 0.0F, fillWidth, trackBounds.Height)

            Using fillPath As GraphicsPath = DrawingHelper.CreateRoundedRectangle(fillBounds, radius),
                  fillBrush As New SolidBrush(Me.ResolveFillColor())

                g.FillPath(fillBrush, fillPath)
            End Using
        End Sub

#End Region

#Region " Private Methods "

        ''' <summary>
        ''' Resolves the fill color for the current state.
        ''' </summary>
        ''' <returns>The fill color.</returns>
        ''' <exception cref="InvalidOperationException">Thrown when the state is not handled.</exception>
        Private Function ResolveFillColor() As Color
            If Not Me.Enabled Then
                Return Me.palette.DisabledForeground
            End If

            Select Case Me.stateValue
                Case ProgressBarState.Normal
                    Return Me.palette.Accent
                Case ProgressBarState.Warning
                    Return Me.palette.ProgressWarning
                Case ProgressBarState.Error
                    Return Me.palette.ProgressError
                Case Else
                    Throw New InvalidOperationException($"Unhandled progress bar state: {Me.stateValue}")
            End Select
        End Function

#End Region

    End Class

End Namespace
