Option Strict On
Option Explicit On
Option Infer Off

#Region " Imports "

Imports System.ComponentModel
Imports System.Windows.Forms

Imports ImageLightnessGrouper.Theming

#End Region

Namespace ImageLightnessGrouper.Controls

    ''' <summary>
    ''' A <see cref="Label"/> that supports primary and muted (secondary) theme colors.
    ''' </summary>
    <ToolboxItem(True)>
    Public Class ThemedLabel : Inherits Label : Implements IThemeable

#Region " Fields "

        ''' <summary>
        ''' Backing field for <see cref="IsMuted"/>.
        ''' </summary>
        Private isMutedValue As Boolean

        ''' <summary>
        ''' The palette currently applied.
        ''' </summary>
        Private palette As ThemePalette = ThemeManager.CurrentPalette

#End Region

#Region " Properties "

        ''' <summary>
        ''' Gets or sets a value indicating whether the label uses the secondary (muted) text color.
        ''' </summary>
        <Category("Appearance")>
        <Description("Indicates whether the label uses the secondary (muted) text color.")>
        <DefaultValue(False)>
        Public Property IsMuted As Boolean
            Get
                Return Me.isMutedValue
            End Get
            Set(value As Boolean)
                Me.isMutedValue = value
                Me.ApplyTheme(Me.palette)
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
            Me.ForeColor = If(Me.isMutedValue, palette.MutedForeground, palette.Foreground)
        End Sub

#End Region

    End Class

End Namespace
