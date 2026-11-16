Option Strict On
Option Explicit On
Option Infer Off

#Region " Imports "

Imports System.ComponentModel
Imports System.Drawing
Imports System.Windows.Forms
Imports System.Windows.Forms.Design

Imports ImageLightnessGrouper.Theming

#End Region

Namespace ImageLightnessGrouper.Controls

    ''' <summary>
    ''' A <see cref="ToolStripControlHost"/> that hosts a <see cref="Controls.ToggleSwitch"/>,
    ''' so a toggle can live inside a <see cref="MenuStrip"/> or a <see cref="ToolStrip"/> (e.g. in its right corner).
    ''' <para></para>
    ''' The item <see cref="ToolStripItem.Text"/> is the caption drawn by the hosted toggle switch itself, which keeps it
    ''' optically centered on the track (unlike a separate <see cref="ToolStripLabel"/>), and the item sizes itself to fit.
    ''' </summary>
    <ToolStripItemDesignerAvailability(ToolStripItemDesignerAvailability.MenuStrip Or
                                       ToolStripItemDesignerAvailability.ToolStrip Or
                                       ToolStripItemDesignerAvailability.StatusStrip)>
    <DefaultEvent(NameOf(ToolStripToggleSwitch.CheckedChanged))>
    <DefaultProperty(NameOf(ToolStripToggleSwitch.Checked))>
    Public Class ToolStripToggleSwitch : Inherits ToolStripControlHost : Implements IThemeable

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
        ''' Initializes a new instance of the <see cref="ToolStripToggleSwitch"/> class.
        ''' </summary>
        Public Sub New()
            MyBase.New(New ToggleSwitch())

            Me.AutoSize = True
            AddHandler Me.ToggleSwitch.CheckedChanged, AddressOf Me.ToggleSwitch_CheckedChanged
            AddHandler Me.ToggleSwitch.SizeChanged, AddressOf Me.ToggleSwitch_SizeChanged
        End Sub

#End Region

#Region " Properties "

        ''' <summary>
        ''' Gets the hosted toggle switch.
        ''' </summary>
        <Browsable(False)>
        <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
        Public ReadOnly Property ToggleSwitch As ToggleSwitch
            Get
                Return DirectCast(Me.Control, ToggleSwitch)
            End Get
        End Property

        ''' <summary>
        ''' Gets or sets a value indicating whether the toggle is in the "on" state.
        ''' </summary>
        <Category("Appearance")>
        <Description("Indicates whether the toggle is in the 'on' state.")>
        <DefaultValue(False)>
        Public Property Checked As Boolean
            Get
                Return Me.ToggleSwitch.Checked
            End Get
            Set(value As Boolean)
                Me.ToggleSwitch.Checked = value
            End Set
        End Property

        ''' <inheritdoc/>
        Protected Overrides ReadOnly Property DefaultMargin As Padding
            Get
                Return New Padding(4, 1, 6, 1)
            End Get
        End Property

#End Region

#Region " IThemeable "

        ''' <inheritdoc/>
        Public Sub ApplyTheme(palette As ThemePalette) Implements IThemeable.ApplyTheme
            If palette Is Nothing Then
                Throw New ArgumentNullException(NameOf(palette))
            End If

            Me.ToggleSwitch.ApplyTheme(palette)

            ' Hosted in a menu bar: blend with the menu background instead of the window background.
            Me.ToggleSwitch.BackColor = palette.MenuBackground
            Me.BackColor = palette.MenuBackground
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

#Region " Event Handlers "

        ''' <summary>
        ''' Forwards the hosted toggle switch <see cref="ToggleSwitch.CheckedChanged"/> event.
        ''' </summary>
        Private Sub ToggleSwitch_CheckedChanged(sender As Object, e As EventArgs)
            Me.OnCheckedChanged(e)
        End Sub

        ''' <summary>
        ''' Re-lays out the owner tool strip when the hosted toggle switch resizes itself (e.g. its caption changed),
        ''' so the auto-sized item and its neighbors are repositioned.
        ''' </summary>
        Private Sub ToggleSwitch_SizeChanged(sender As Object, e As EventArgs)
            If Me.Owner IsNot Nothing Then
                Me.Owner.PerformLayout()
            End If
        End Sub

#End Region

    End Class

End Namespace
