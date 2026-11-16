Option Strict On
Option Explicit On
Option Infer Off

#Region " Imports "

Imports System.Drawing
Imports System.Windows.Forms

#End Region

Namespace ImageLightnessGrouper.Theming

    ''' <summary>
    ''' A <see cref="ProfessionalColorTable"/> driven by a <see cref="ThemePalette"/>, with flat (non-gradient) colors.
    ''' </summary>
    Friend NotInheritable Class ThemedColorTable : Inherits ProfessionalColorTable

#Region " Read-only Fields "

        ''' <summary>
        ''' The source palette.
        ''' </summary>
        Private ReadOnly palette As ThemePalette

#End Region

#Region " Constructors "

        ''' <summary>
        ''' Initializes a new instance of the <see cref="ThemedColorTable"/> class.
        ''' </summary>
        ''' <param name="palette">The source palette.</param>
        ''' <exception cref="ArgumentNullException">Thrown when <paramref name="palette"/> is <see langword="Nothing"/>.</exception>
        Friend Sub New(palette As ThemePalette)
            If palette Is Nothing Then
                Throw New ArgumentNullException(NameOf(palette))
            End If

            Me.palette = palette
            Me.UseSystemColors = False
        End Sub

#End Region

#Region " Overridden Properties "

        ''' <inheritdoc/>
        Public Overrides ReadOnly Property MenuStripGradientBegin As Color
            Get
                Return Me.palette.MenuBackground
            End Get
        End Property

        ''' <inheritdoc/>
        Public Overrides ReadOnly Property MenuStripGradientEnd As Color
            Get
                Return Me.palette.MenuBackground
            End Get
        End Property

        ''' <inheritdoc/>
        Public Overrides ReadOnly Property ToolStripDropDownBackground As Color
            Get
                Return Me.palette.MenuBackground
            End Get
        End Property

        ''' <inheritdoc/>
        Public Overrides ReadOnly Property ImageMarginGradientBegin As Color
            Get
                Return Me.palette.MenuBackground
            End Get
        End Property

        ''' <inheritdoc/>
        Public Overrides ReadOnly Property ImageMarginGradientMiddle As Color
            Get
                Return Me.palette.MenuBackground
            End Get
        End Property

        ''' <inheritdoc/>
        Public Overrides ReadOnly Property ImageMarginGradientEnd As Color
            Get
                Return Me.palette.MenuBackground
            End Get
        End Property

        ''' <inheritdoc/>
        Public Overrides ReadOnly Property MenuBorder As Color
            Get
                Return Me.palette.MenuBorder
            End Get
        End Property

        ''' <inheritdoc/>
        Public Overrides ReadOnly Property MenuItemBorder As Color
            Get
                Return Me.palette.MenuHighlight
            End Get
        End Property

        ''' <inheritdoc/>
        Public Overrides ReadOnly Property MenuItemSelected As Color
            Get
                Return Me.palette.MenuHighlight
            End Get
        End Property

        ''' <inheritdoc/>
        Public Overrides ReadOnly Property MenuItemSelectedGradientBegin As Color
            Get
                Return Me.palette.MenuHighlight
            End Get
        End Property

        ''' <inheritdoc/>
        Public Overrides ReadOnly Property MenuItemSelectedGradientEnd As Color
            Get
                Return Me.palette.MenuHighlight
            End Get
        End Property

        ''' <inheritdoc/>
        Public Overrides ReadOnly Property MenuItemPressedGradientBegin As Color
            Get
                Return Me.palette.MenuHighlight
            End Get
        End Property

        ''' <inheritdoc/>
        Public Overrides ReadOnly Property MenuItemPressedGradientMiddle As Color
            Get
                Return Me.palette.MenuHighlight
            End Get
        End Property

        ''' <inheritdoc/>
        Public Overrides ReadOnly Property MenuItemPressedGradientEnd As Color
            Get
                Return Me.palette.MenuHighlight
            End Get
        End Property

        ''' <inheritdoc/>
        Public Overrides ReadOnly Property SeparatorDark As Color
            Get
                Return Me.palette.MenuBorder
            End Get
        End Property

        ''' <inheritdoc/>
        Public Overrides ReadOnly Property SeparatorLight As Color
            Get
                Return Me.palette.MenuBorder
            End Get
        End Property

        ''' <inheritdoc/>
        Public Overrides ReadOnly Property CheckBackground As Color
            Get
                Return Me.palette.MenuHighlight
            End Get
        End Property

        ''' <inheritdoc/>
        Public Overrides ReadOnly Property CheckSelectedBackground As Color
            Get
                Return Me.palette.MenuHighlight
            End Get
        End Property

        ''' <inheritdoc/>
        Public Overrides ReadOnly Property CheckPressedBackground As Color
            Get
                Return Me.palette.MenuHighlight
            End Get
        End Property

#End Region

    End Class

End Namespace
