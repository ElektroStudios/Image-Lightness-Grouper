Option Strict On
Option Explicit On
Option Infer Off

#Region " Imports "

Imports System.ComponentModel
Imports System.Windows.Forms

#End Region

Namespace ImageLightnessGrouper.Theming

    ''' <summary>
    ''' Central, application-wide theme coordinator.
    ''' <para></para>
    ''' Holds the active theme, raises <see cref="ThemeChanged"/> and walks control trees
    ''' (iteratively, never recursively) to apply the active <see cref="ThemePalette"/>.
    ''' </summary>
    ''' <remarks>
    ''' This class is not thread-safe by design: every member must be called from the UI thread.
    ''' </remarks>
    Public NotInheritable Class ThemeManager

#Region " Fields "

        ''' <summary>
        ''' Backing field for <see cref="CurrentTheme"/>.
        ''' </summary>
        Private Shared currentThemeValue As AppTheme = AppTheme.Light

#End Region

#Region " Events "

        ''' <summary>
        ''' Occurs after <see cref="CurrentTheme"/> has changed.
        ''' </summary>
        Public Shared Event ThemeChanged As EventHandler

#End Region

#Region " Constructors "

        ''' <summary>
        ''' Prevents a default instance of the <see cref="ThemeManager"/> class from being created.
        ''' </summary>
        Private Sub New()
        End Sub

#End Region

#Region " Properties "

        ''' <summary>
        ''' Gets the active theme.
        ''' </summary>
        Public Shared ReadOnly Property CurrentTheme As AppTheme
            Get
                Return ThemeManager.currentThemeValue
            End Get
        End Property

        ''' <summary>
        ''' Gets the palette of the active theme.
        ''' </summary>
        Public Shared ReadOnly Property CurrentPalette As ThemePalette
            Get
                Return ThemePalette.FromTheme(ThemeManager.currentThemeValue)
            End Get
        End Property

#End Region

#Region " Public Methods "

        ''' <summary>
        ''' Sets the active theme and raises <see cref="ThemeChanged"/> when it actually changes.
        ''' </summary>
        ''' <param name="theme">The theme to activate.</param>
        ''' <exception cref="InvalidEnumArgumentException">Thrown when <paramref name="theme"/> is not a defined value.</exception>
        Public Shared Sub SetTheme(theme As AppTheme)
            If Not [Enum].IsDefined(GetType(AppTheme), theme) Then
                Throw New InvalidEnumArgumentException(NameOf(theme), CInt(theme), GetType(AppTheme))
            End If

            If ThemeManager.currentThemeValue = theme Then
                Return ' Idempotent: nothing changed, nothing to notify.
            End If

            ThemeManager.currentThemeValue = theme
            RaiseEvent ThemeChanged(Nothing, EventArgs.Empty)
        End Sub

        ''' <summary>
        ''' Applies the active palette to the specified control and all its descendants.
        ''' </summary>
        ''' <param name="root">The root control (typically a <see cref="Form"/>).</param>
        ''' <exception cref="ArgumentNullException">Thrown when <paramref name="root"/> is <see langword="Nothing"/>.</exception>
        Public Shared Sub ApplyTheme(root As Control)
            If root Is Nothing Then
                Throw New ArgumentNullException(NameOf(root))
            End If

            Dim palette As ThemePalette = ThemeManager.CurrentPalette
            Dim pending As New Stack(Of Control)()
            pending.Push(root)

            root.SuspendLayout()
            Try
                While pending.Count > 0
                    Dim control As Control = pending.Pop()
                    ThemeManager.ApplyThemeToControl(control, palette)

                    ' IThemeable controls own their private subtree, and tool strips theme their hosted
                    ' controls through their items (ApplyThemeToToolStrip), with the menu background.
                    If TypeOf control Is IThemeable OrElse TypeOf control Is ToolStrip Then
                        Continue While
                    End If

                    For Each child As Control In control.Controls
                        pending.Push(child)
                    Next
                End While
            Finally
                root.ResumeLayout(performLayout:=True)
            End Try

            root.Invalidate(invalidateChildren:=True)
        End Sub

        ''' <summary>
        ''' Applies the specified palette to a <see cref="ToolStrip"/> and to every nested drop-down.
        ''' </summary>
        ''' <param name="toolStrip">The tool strip (menu strip, context menu, etc).</param>
        ''' <param name="palette">The palette to apply.</param>
        ''' <exception cref="ArgumentNullException">Thrown when any argument is <see langword="Nothing"/>.</exception>
        Public Shared Sub ApplyThemeToToolStrip(toolStrip As ToolStrip, palette As ThemePalette)
            If toolStrip Is Nothing Then
                Throw New ArgumentNullException(NameOf(toolStrip))
            End If

            If palette Is Nothing Then
                Throw New ArgumentNullException(NameOf(palette))
            End If

            Dim renderer As New ThemedToolStripRenderer(palette)
            toolStrip.Renderer = renderer
            toolStrip.BackColor = palette.MenuBackground
            toolStrip.ForeColor = palette.Foreground

            Dim pendingItems As New Stack(Of ToolStripItem)()
            For Each item As ToolStripItem In toolStrip.Items
                pendingItems.Push(item)
            Next

            While pendingItems.Count > 0
                Dim item As ToolStripItem = pendingItems.Pop()
                item.ForeColor = palette.Foreground

                ' Hosted custom items (e.g. a toggle switch in the menu bar) theme themselves.
                Dim themeableItem As IThemeable = TryCast(item, IThemeable)
                If themeableItem IsNot Nothing Then
                    themeableItem.ApplyTheme(palette)
                End If

                Dim dropDownItem As ToolStripDropDownItem = TryCast(item, ToolStripDropDownItem)
                If dropDownItem Is Nothing OrElse Not dropDownItem.HasDropDownItems Then
                    Continue While
                End If

                dropDownItem.DropDown.Renderer = renderer
                dropDownItem.DropDown.BackColor = palette.MenuBackground
                dropDownItem.DropDown.ForeColor = palette.Foreground

                For Each childItem As ToolStripItem In dropDownItem.DropDownItems
                    pendingItems.Push(childItem)
                Next
            End While
        End Sub

#End Region

#Region " Private Methods "

        ''' <summary>
        ''' Applies the palette to a single control (not to its children).
        ''' </summary>
        ''' <param name="control">The control.</param>
        ''' <param name="palette">The palette to apply.</param>
        Private Shared Sub ApplyThemeToControl(control As Control, palette As ThemePalette)

            Dim themeable As IThemeable = TryCast(control, IThemeable)
            If themeable IsNot Nothing Then
                themeable.ApplyTheme(palette)

            ElseIf TypeOf control Is ToolStrip Then
                ThemeManager.ApplyThemeToToolStrip(DirectCast(control, ToolStrip), palette)

            ElseIf TypeOf control Is PictureBox Then
                control.BackColor = palette.WindowBackground

            Else
                ' Forms, panels, layout panels and plain labels.
                control.BackColor = palette.WindowBackground
                control.ForeColor = palette.Foreground
            End If

            If control.ContextMenuStrip IsNot Nothing Then
                ThemeManager.ApplyThemeToToolStrip(control.ContextMenuStrip, palette)
            End If
        End Sub

#End Region

    End Class

End Namespace
